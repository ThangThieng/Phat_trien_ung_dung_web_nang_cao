"""Các thao tác GitHub Projects (v2) dùng chung cho sync_board.py và auto_complete.py.

Mọi lời gọi đi qua GitHub CLI (`gh api`), nên chạy được cả trên máy (gh đã đăng nhập) lẫn
trong GitHub Actions (biến môi trường GH_TOKEN là PAT có scope `project` + `repo`).
"""
from __future__ import annotations

import json
import shutil
import subprocess
import sys
from pathlib import Path

STATUS_DONE, STATUS_DOING, STATUS_TODO = "Done", "In Progress", "Todo"
STATUS_RANK = {STATUS_TODO: 0, STATUS_DOING: 1, STATUS_DONE: 2}

# Trường thêm cho giáo viên chấm bài: nhìn một hàng là biết buổi nào, ai, xong chưa, bằng chứng ở đâu.
FIELD_SESSION = "Buổi"
FIELD_STUDENT = "Sinh viên"
FIELD_SCORE = "Tiêu chí đạt"
FIELD_BRANCH = "Nhánh minh chứng"
FIELD_COMMIT = "Commit minh chứng"
FIELD_DONE_AT = "Ngày hoàn thành"
SESSION_COLORS = ["GRAY", "BLUE", "GREEN", "YELLOW", "ORANGE", "RED", "PINK", "PURPLE"]
STUDENT_COLORS = {1: "BLUE", 2: "ORANGE", 3: "GREEN", 4: "PURPLE"}


def _gh_path() -> str:
    found = shutil.which("gh") or r"C:\Program Files\GitHub CLI\gh.exe"
    if not Path(found).exists():
        sys.exit("Không tìm thấy GitHub CLI (gh). Cài: winget install GitHub.cli")
    return found


_GH: str | None = None


def gh(*args: str, payload: dict | None = None, ok_codes: tuple[int, ...] = ()) -> dict | list | None:
    global _GH
    _GH = _GH or _gh_path()
    cmd = [_GH, "api", *args]
    if payload is not None:
        cmd += ["--input", "-"]
    proc = subprocess.run(cmd, input=json.dumps(payload).encode() if payload is not None else None,
                          capture_output=True)
    out = proc.stdout.decode("utf-8", "replace")
    if proc.returncode != 0:
        err = proc.stderr.decode("utf-8", "replace")
        for code in ok_codes:
            if f"HTTP {code}" in err:
                return None
        sys.exit(f"gh api {' '.join(args[:3])} lỗi:\n{err}\n{out}")
    return json.loads(out) if out.strip() else None


def graphql(query: str, **variables) -> dict:
    res = gh("graphql", payload={"query": query, "variables": variables})
    if res.get("errors"):
        sys.exit(f"GraphQL lỗi: {res['errors']}")
    return res["data"]


def paginate(path: str) -> list:
    items, page = [], 1
    while True:
        batch = gh(f"{path}{'&' if '?' in path else '?'}per_page=100&page={page}")
        items += batch
        if len(batch) < 100:
            return items
        page += 1


def session_option(n: int) -> str:
    return f"Buổi {n}"


def student_option(member) -> str:
    return f"Dev {member.dev} – {member.name} ({member.mssv})"


class Project:
    """Một Project v2 của user, nhận diện bằng tiêu đề trong config.json."""

    def __init__(self, owner: str, title: str):
        self.owner = owner
        q = """query($login:String!){ user(login:$login){ id projectsV2(first:50){ nodes{ id number title url } } } }"""
        user = graphql(q, login=owner)["user"]
        self.owner_id = user["id"]
        node = next((p for p in user["projectsV2"]["nodes"] if p["title"] == title), None)
        self.node = node
        if node:
            self.id, self.number, self.url = node["id"], node["number"], node["url"]

    # ------------------------------------------------------------ trường
    def fields(self) -> dict[str, dict]:
        q = """query($p:ID!){ node(id:$p){ ... on ProjectV2 { fields(first:50){ nodes{
                 ... on ProjectV2FieldCommon{ id name dataType }
                 ... on ProjectV2SingleSelectField{ options{ id name } } } } } } }"""
        return {f["name"]: f for f in graphql(q, p=self.id)["node"]["fields"]["nodes"] if f}

    def ensure_single_select(self, name: str, options: list[tuple[str, str, str]]) -> dict:
        """options: (tên, màu, mô tả). Thêm option còn thiếu, giữ nguyên thứ tự mong muốn."""
        have = self.fields().get(name)
        wanted = [{"name": n, "color": c, "description": d} for n, c, d in options]
        if have is None:
            graphql("""mutation($p:ID!,$n:String!,$o:[ProjectV2SingleSelectFieldOptionInput!]!){
                       createProjectV2Field(input:{projectId:$p,dataType:SINGLE_SELECT,name:$n,singleSelectOptions:$o})
                       { projectV2Field{ ... on ProjectV2FieldCommon{ id } } } }""", p=self.id, n=name, o=wanted)
        elif {o["name"] for o in have["options"]} != {o["name"] for o in wanted}:
            graphql("""mutation($f:ID!,$o:[ProjectV2SingleSelectFieldOptionInput!]!){
                       updateProjectV2Field(input:{fieldId:$f,singleSelectOptions:$o})
                       { projectV2Field{ ... on ProjectV2FieldCommon{ id } } } }""", f=have["id"], o=wanted)
        return self.fields()[name]

    def ensure_field(self, name: str, data_type: str) -> dict:
        have = self.fields().get(name)
        if have is None:
            graphql("""mutation($p:ID!,$n:String!,$t:ProjectV2CustomFieldType!){
                       createProjectV2Field(input:{projectId:$p,dataType:$t,name:$n})
                       { projectV2Field{ ... on ProjectV2FieldCommon{ id } } } }""", p=self.id, n=name, t=data_type)
            have = self.fields()[name]
        return have

    def ensure_grading_fields(self, members) -> dict[str, dict]:
        self.ensure_single_select(FIELD_SESSION, [(session_option(n), SESSION_COLORS[n - 1], "")
                                                  for n in range(1, 9)])
        self.ensure_single_select(FIELD_STUDENT, [(student_option(m), STUDENT_COLORS[m.dev], m.area)
                                                  for m in members])
        for name in (FIELD_SCORE, FIELD_BRANCH, FIELD_COMMIT):
            self.ensure_field(name, "TEXT")
        self.ensure_field(FIELD_DONE_AT, "DATE")
        return self.fields()

    # ------------------------------------------------------------ item
    def items(self) -> dict[int, dict]:
        """issue number → {id, state, values{tên trường: giá trị}}"""
        q = """query($p:ID!,$after:String){ node(id:$p){ ... on ProjectV2 { items(first:100, after:$after){
                 pageInfo{ hasNextPage endCursor }
                 nodes{ id content{ ... on Issue{ number state } }
                   fieldValues(first:30){ nodes{
                     ... on ProjectV2ItemFieldSingleSelectValue{ name field{ ... on ProjectV2FieldCommon{ name } } }
                     ... on ProjectV2ItemFieldTextValue{ text field{ ... on ProjectV2FieldCommon{ name } } }
                     ... on ProjectV2ItemFieldDateValue{ date field{ ... on ProjectV2FieldCommon{ name } } } } } } } } } }"""
        out, after = {}, None
        while True:
            page = graphql(q, p=self.id, after=after)["node"]["items"]
            for it in page["nodes"]:
                if not it["content"] or "number" not in it["content"]:
                    continue
                values = {}
                for v in it["fieldValues"]["nodes"]:
                    if v and v.get("field"):
                        values[v["field"]["name"]] = v.get("name") or v.get("text") or v.get("date")
                out[it["content"]["number"]] = {"id": it["id"], "state": it["content"]["state"], "values": values}
            if not page["pageInfo"]["hasNextPage"]:
                return out
            after = page["pageInfo"]["endCursor"]

    def add_issue(self, issue_node_id: str) -> str:
        return graphql("""mutation($p:ID!,$c:ID!){ addProjectV2ItemById(input:{projectId:$p,contentId:$c})
                          { item{ id } } }""", p=self.id, c=issue_node_id)["addProjectV2ItemById"]["item"]["id"]

    def set_value(self, item_id: str, field: dict, value: str | None) -> None:
        if value is None:
            graphql("""mutation($p:ID!,$i:ID!,$f:ID!){ clearProjectV2ItemFieldValue(input:{projectId:$p,itemId:$i,fieldId:$f})
                       { projectV2Item{ id } } }""", p=self.id, i=item_id, f=field["id"])
            return
        if field["dataType"] == "SINGLE_SELECT":
            opt = next(o["id"] for o in field["options"] if o["name"] == value)
            val = {"singleSelectOptionId": opt}
        elif field["dataType"] == "DATE":
            val = {"date": value}
        else:
            val = {"text": value}
        graphql("""mutation($p:ID!,$i:ID!,$f:ID!,$v:ProjectV2FieldValue!){ updateProjectV2ItemFieldValue(input:{
                   projectId:$p,itemId:$i,fieldId:$f,value:$v}){ projectV2Item{ id } } }""",
                p=self.id, i=item_id, f=field["id"], v=val)

    # ------------------------------------------------------------ view
    def view_names(self) -> set[str]:
        q = "query($p:ID!){ node(id:$p){ ... on ProjectV2 { views(first:50){ nodes{ name } } } } }"
        return {v["name"] for v in graphql(q, p=self.id)["node"]["views"]["nodes"]}

    def create_view(self, name: str, spec: dict) -> bool:
        base = f"users/{self.owner}/projectsV2/{self.number}/views"
        return gh(base, "-X", "POST", payload={"name": name, **spec}, ok_codes=(404, 422)) is not None

    def set_readme(self, readme: str, description: str) -> None:
        graphql("""mutation($p:ID!,$r:String!,$d:String!){ updateProjectV2(input:{projectId:$p,readme:$r,
                   shortDescription:$d}){ projectV2{ id } } }""", p=self.id, r=readme, d=description)
