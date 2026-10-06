"""Đồng bộ phần chia việc trong README.md lên GitHub Issues + Projects (v2).

Mỗi thành viên có 1 issue cha "Họ tên - MSSV"; mỗi phần việc của người đó trong
từng buổi là 1 sub-issue. Tất cả được đưa vào Project với cột Status
Todo / In Progress / Done và hai view: "Issue View" (bảng) và "Tab View" (board).

Chạy lại bao nhiêu lần cũng được: issue được nhận diện bằng khóa ẩn trong body
nên chỉ cập nhật, không tạo trùng; trạng thái đang có trên Project (vd. do auto_complete.py
tự chấm đặt Done) không bao giờ bị hạ xuống.

    python tools/github_board/sync_board.py --dry-run     # chỉ xuất bản xem trước
    python tools/github_board/sync_board.py               # đẩy lên GitHub

Cần GitHub CLI đã đăng nhập với scope `project`:
    gh auth login && gh auth refresh -s project
"""
from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
from pathlib import Path

from project_api import STATUS_RANK, Project
from readme_parser import Member, Plan, Task, parse

ROOT = Path(__file__).resolve().parents[2]
CONFIG = Path(__file__).with_name("config.json")
MARKER = "<!-- culinary-board:key={} -->"
STATUS_DONE, STATUS_DOING, STATUS_TODO = "Done", "In Progress", "Todo"
AREA_LABELS = {1: ("area:auth", "1d76db"), 2: ("area:recipe", "d93f0b"),
               3: ("area:category-search", "0e8a16"), 4: ("area:infra", "5319e7")}


# ---------------------------------------------------------------- GitHub CLI
def _gh_path() -> str:
    found = shutil.which("gh") or r"C:\Program Files\GitHub CLI\gh.exe"
    if not Path(found).exists():
        sys.exit("Không tìm thấy GitHub CLI (gh). Cài: winget install GitHub.cli")
    return found


GH = None


def gh(*args: str, payload: dict | None = None, ok_codes: tuple[int, ...] = ()) -> dict | list | None:
    global GH
    GH = GH or _gh_path()
    cmd = [GH, "api", *args]
    if payload is not None:
        cmd += ["--input", "-"]
    proc = subprocess.run(cmd, input=json.dumps(payload).encode() if payload is not None else None,
                          capture_output=True)
    out = proc.stdout.decode("utf-8", "replace")
    if proc.returncode != 0:
        for code in ok_codes:
            if f"HTTP {code}" in proc.stderr.decode("utf-8", "replace"):
                return None
        sys.exit(f"gh api {' '.join(args[:3])} lỗi:\n{proc.stderr.decode('utf-8', 'replace')}\n{out}")
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


# ---------------------------------------------------------------- nội dung
def task_status(task: Task, cfg: dict) -> str:
    if task.done_in_readme or task.session in cfg["done_sessions"]:
        return STATUS_DONE
    if task.session in cfg["in_progress_sessions"]:
        return STATUS_DOING
    return STATUS_TODO


def task_body(task: Task, cfg: dict, status: str) -> str:
    m = task.member
    anchor = cfg["readme_url"] + "#phần-3--chia-việc-chi-tiết-từng-buổi"
    return (f"{MARKER.format(task.key)}\n"
            f"| Buổi | Người làm | Mảng | Nhánh Git | Trạng thái |\n|---|---|---|---|---|\n"
            f"| {task.session} – {task.session_title} | Dev {m.dev} – {m.name} ({m.mssv}) | {m.area} "
            f"| `{m.branch.replace('{n}', str(task.session))}` | {status} |\n\n"
            f"{task.body}\n\n---\n"
            f"_Sinh tự động từ [README – Phần 3]({anchor}) bằng `tools/github_board/sync_board.py`. "
            f"Sửa README rồi chạy lại tool, đừng sửa tay issue này._")


def parent_title(m: Member) -> str:
    return f"{m.name} - {m.mssv}"


def parent_body(m: Member, plan: Plan, cfg: dict) -> str:
    rows = "\n".join(f"| {s} | {v} |" for s, v in sorted(plan.schedule[m.dev].items()))
    return (f"{MARKER.format(f'member-dev{m.dev}')}\n"
            f"**Dev {m.dev} – {m.name}** · MSSV {m.mssv}\n\n"
            f"- **Phụ trách:** {m.area}\n- **Làm gì trên trang web:** {m.web_scope}\n"
            f"- **FR:** {m.frs}\n- **Nhánh mỗi buổi:** `{m.branch}`\n\n"
            f"### Lịch 8 buổi\n| Buổi | Việc |\n|---|---|\n{rows}\n\n"
            f"Chi tiết từng buổi nằm ở các sub-issue bên dưới.\n\n---\n"
            f"_Sinh tự động từ [README – Phần 2]({cfg['readme_url']}#phần-2--chia-việc-tổng-thể) "
            f"bằng `tools/github_board/sync_board.py`._")


def write_preview(plan: Plan, cfg: dict, path: Path) -> None:
    out = [f"# Xem trước – {cfg['project_title']}\n"]
    for m in plan.members:
        out.append(f"\n## {parent_title(m)}  (assignee: {cfg['logins'].get(str(m.dev)) or '—'})\n")
        for t in (t for t in plan.tasks if t.member is m):
            out.append(f"- **{task_status(t, cfg)}** · {t.title}  `{t.key}`")
    out.append("\n\n---\n# Nội dung đầy đủ từng issue\n")
    for m in plan.members:
        out.append(f"\n\n## ISSUE CHA: {parent_title(m)}\n\n{parent_body(m, plan, cfg)}")
        for t in (t for t in plan.tasks if t.member is m):
            out.append(f"\n\n## {t.title}\n\n{task_body(t, cfg, task_status(t, cfg))}")
    path.write_text("\n".join(out), encoding="utf-8")


# ---------------------------------------------------------------- đồng bộ
class Board:
    def __init__(self, cfg: dict):
        self.cfg, self.repo = cfg, cfg["repo"]
        self.owner = self.repo.split("/")[0]
        self.existing = {}
        for issue in paginate(f"repos/{self.repo}/issues?state=all"):
            body = issue.get("body") or ""
            if "pull_request" not in issue and "culinary-board:key=" in body:
                key = body.split("culinary-board:key=", 1)[1].split(" -->", 1)[0]
                self.existing[key] = issue

    def ensure_labels(self, sessions: set[int]) -> None:
        have = {l["name"] for l in paginate(f"repos/{self.repo}/labels")}
        wanted = [(f"buoi-{s}", "fbca04", f"Buổi {s}") for s in sorted(sessions)]
        wanted += [(n, c, "Mảng phụ trách") for n, c in AREA_LABELS.values()]
        wanted.append(("thanh-vien", "bfdadc", "Issue cha của một thành viên"))
        for name, color, desc in wanted:
            if name not in have:
                gh(f"repos/{self.repo}/labels", "-X", "POST",
                   payload={"name": name, "color": color, "description": desc})

    def upsert(self, key: str, title: str, body: str, labels: list[str],
               assignee: str | None, closed: bool) -> dict:
        data = {"title": title, "body": body, "labels": labels,
                "assignees": [assignee] if assignee else [],
                "state": "closed" if closed else "open"}
        if closed:
            data["state_reason"] = "completed"
        if key in self.existing:
            num = self.existing[key]["number"]
            issue = gh(f"repos/{self.repo}/issues/{num}", "-X", "PATCH", payload=data)
            print(f"  cập nhật #{num} {title}")
        else:
            state = data.pop("state"), data.pop("state_reason", None)
            issue = gh(f"repos/{self.repo}/issues", "-X", "POST", payload=data)
            if closed:
                issue = gh(f"repos/{self.repo}/issues/{issue['number']}", "-X", "PATCH",
                           payload={"state": state[0], "state_reason": state[1]})
            print(f"  tạo    #{issue['number']} {title}")
        return issue

    def link_sub_issues(self, parent: dict, children: list[dict]) -> None:
        have = {s["id"] for s in paginate(f"repos/{self.repo}/issues/{parent['number']}/sub_issues")}
        for child in children:
            if child["id"] not in have:
                gh(f"repos/{self.repo}/issues/{parent['number']}/sub_issues", "-X", "POST",
                   payload={"sub_issue_id": child["id"], "replace_parent": True})

    # ---- Project v2
    def project(self) -> dict:
        q = """query($login:String!){ user(login:$login){ id
                 projectsV2(first:50){ nodes{ id number title url } } } }"""
        user = graphql(q, login=self.owner)["user"]
        proj = next((p for p in user["projectsV2"]["nodes"] if p["title"] == self.cfg["project_title"]), None)
        if not proj:
            proj = graphql("""mutation($o:ID!,$t:String!){ createProjectV2(input:{ownerId:$o,title:$t})
                              { projectV2{ id number title url } } }""",
                           o=user["id"], t=self.cfg["project_title"])["createProjectV2"]["projectV2"]
            print(f"Đã tạo Project: {proj['url']}")
        repo_id = gh(f"repos/{self.repo}")["node_id"]
        graphql("""mutation($p:ID!,$r:ID!){ linkProjectV2ToRepository(input:{projectId:$p,repositoryId:$r})
                   { clientMutationId } }""", p=proj["id"], r=repo_id)
        graphql("""mutation($p:ID!,$pub:Boolean!,$d:String!){ updateProjectV2(input:{projectId:$p,
                   public:$pub, shortDescription:$d}){ projectV2{ id } } }""",
                p=proj["id"], pub=self.cfg["project_public"], d=self.cfg["project_description"])
        return proj

    def status_field(self, project_id: str) -> tuple[str, dict[str, str]]:
        q = """query($p:ID!){ node(id:$p){ ... on ProjectV2 { fields(first:50){ nodes{
                 ... on ProjectV2SingleSelectField{ id name options{ id name } } } } } } }"""
        for f in graphql(q, p=project_id)["node"]["fields"]["nodes"]:
            if f and f.get("name") == "Status":
                return f["id"], {o["name"]: o["id"] for o in f["options"]}
        sys.exit("Project không có trường Status.")

    def add_item(self, project_id: str, issue: dict, field_id: str, option_id: str) -> None:
        item = graphql("""mutation($p:ID!,$c:ID!){ addProjectV2ItemById(input:{projectId:$p,contentId:$c})
                          { item{ id } } }""", p=project_id, c=issue["node_id"])["addProjectV2ItemById"]["item"]
        graphql("""mutation($p:ID!,$i:ID!,$f:ID!,$o:String!){ updateProjectV2ItemFieldValue(input:{
                   projectId:$p,itemId:$i,fieldId:$f,value:{singleSelectOptionId:$o}}){ projectV2Item{ id } } }""",
                p=project_id, i=item["id"], f=field_id, o=option_id)

    def ensure_views(self, project: dict) -> list[str]:
        """Tạo 2 view qua REST. Trả về các view chưa tạo được (để hướng dẫn làm tay)."""
        base = f"users/{self.owner}/projectsV2/{project['number']}"
        names = {v["name"] for v in graphql(
            "query($p:ID!){ node(id:$p){ ... on ProjectV2 { views(first:50){ nodes{ name } } } } }",
            p=project["id"])["node"]["views"]["nodes"]}
        fields = {f["name"]: f["id"] for f in gh(f"{base}/fields")}
        table_cols = ["Title", "Assignees", "Status", "Linked pull requests", "Sub-issues progress"]
        wanted = (
            # Bảng chỉ hiện issue cha; bấm ">" để mở sub-issue như hình mẫu
            ("Issue View", {"layout": "table", "filter": "no:parent-issue",
                            "visible_fields": [fields[c] for c in table_cols if c in fields]}),
            ("Tab View", {"layout": "board"}),
        )
        missing = []
        for name, spec in wanted:
            if name not in names and gh(f"{base}/views", "-X", "POST", payload={"name": name, **spec},
                                        ok_codes=(404, 422)) is None:
                missing.append(f"{name} ({spec['layout']})")
        return missing


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dry-run", action="store_true", help="chỉ xuất file xem trước, không gọi GitHub")
    ap.add_argument("--preview", type=Path, default=Path(__file__).with_name("preview.md"))
    args = ap.parse_args()

    cfg = json.loads(CONFIG.read_text(encoding="utf-8"))
    cfg["readme_url"] = f"https://github.com/{cfg['repo']}/blob/{cfg['branch']}/README.md"
    plan = parse(ROOT / "README.md", f"https://github.com/{cfg['repo']}/blob/{cfg['branch']}")
    write_preview(plan, cfg, args.preview)
    counts = {s: sum(task_status(t, cfg) == s for t in plan.tasks) for s in (STATUS_TODO, STATUS_DOING, STATUS_DONE)}
    print(f"{len(plan.members)} thành viên, {len(plan.tasks)} phần việc {counts}. Xem trước: {args.preview}")
    if args.dry_run:
        return

    board = Board(cfg)
    board.ensure_labels({t.session for t in plan.tasks})
    project = board.project()
    field_id, options = board.status_field(project["id"])
    # Trạng thái hiện có trên Project (có thể do auto_complete.py đặt) — không bao giờ hạ xuống
    on_board = {n: it["values"].get("Status") for n, it in Project(board.owner, cfg["project_title"]).items().items()}

    def keep_highest(key: str, status: str) -> str:
        issue = board.existing.get(key)
        current = on_board.get(issue["number"]) if issue else None
        return current if current and STATUS_RANK[current] > STATUS_RANK[status] else status

    for m in plan.members:
        login = cfg["logins"].get(str(m.dev)) or None
        area = AREA_LABELS[m.dev][0]
        print(f"{parent_title(m)}")
        children = []
        for t in (t for t in plan.tasks if t.member is m):
            status = keep_highest(t.key, task_status(t, cfg))
            issue = board.upsert(t.key, t.title, task_body(t, cfg, status),
                                 [f"buoi-{t.session}", area], login, status == STATUS_DONE)
            board.add_item(project["id"], issue, field_id, options[status])
            children.append((issue, status))
        statuses = {s for _, s in children}
        p_status = (STATUS_DONE if statuses == {STATUS_DONE}
                    else STATUS_DOING if statuses & {STATUS_DOING, STATUS_DONE} else STATUS_TODO)
        p_status = keep_highest(f"member-dev{m.dev}", p_status)
        parent = board.upsert(f"member-dev{m.dev}", parent_title(m), parent_body(m, plan, cfg),
                              ["thanh-vien", area], login, p_status == STATUS_DONE)
        board.link_sub_issues(parent, [c for c, _ in children])
        board.add_item(project["id"], parent, field_id, options[p_status])

    missing = board.ensure_views(project)
    print(f"\nXong. Project: {project['url']}")
    if missing:
        print("Không tạo được view qua API; tạo tay trong Project: " + ", ".join(missing)
              + " — view bảng đặt filter `no:parent-issue` và bật cột 'Sub-issues progress'.")


if __name__ == "__main__":
    main()
