"""Tự chấm tiến độ: nhánh của ai làm xong đầu việc nào thì đưa đầu việc đó sang Done trên Project.

Với mỗi nhánh dạng `{MSSV}_{HoTen}_buoiso{n}` trên remote, script đọc cây mã của nhánh (không checkout)
và kiểm các tiêu chí trong completion_rules.json cho những đầu việc thuộc CHÍNH chủ nhánh:
  - đạt đủ tiêu chí  → Status = Done, đóng issue (completed), ghi nhánh/commit/ngày làm minh chứng
                       và comment checklist lên issue;
  - đạt một phần ở nhánh đúng buổi → Status = In Progress, trường "Tiêu chí đạt" = x/y.
Không bao giờ hạ trạng thái (Done giữ nguyên Done).

    python tools/github_board/auto_complete.py --all --dry-run        # chỉ in kết quả chấm
    python tools/github_board/auto_complete.py --all                  # chấm mọi nhánh và cập nhật Project
    python tools/github_board/auto_complete.py --branch 2312755_NguyenThangThieng_buoiso4

Trong GitHub Actions (.github/workflows/project-board.yml) script chạy mỗi lần có push lên bất kỳ nhánh
nào và định kỳ, với GH_TOKEN là PAT có scope `repo` + `project`.
"""
from __future__ import annotations

import argparse
import fnmatch
import json
import os
import re
import sys
import subprocess
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path

from project_api import (FIELD_BRANCH, FIELD_COMMIT, FIELD_DONE_AT, FIELD_SCORE, FIELD_SESSION, FIELD_STUDENT,
                         STATUS_DOING, STATUS_DONE, STATUS_RANK, STATUS_TODO, Project, gh, paginate,
                         session_option, student_option)
from readme_parser import Member, Task, parse

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
MARKER = "culinary-board:key="
AUTO_MARK = "<!-- culinary-board:auto-done -->"
BRANCH_RE = re.compile(r"^(?P<mssv>\d{7})[_-].*?buoi(?:so)?[_-]?(?P<session>\d+)", re.IGNORECASE)


# ---------------------------------------------------------------- git
def git(*args: str, check: bool = True) -> subprocess.CompletedProcess:
    proc = subprocess.run(["git", *args], cwd=ROOT, capture_output=True)
    if check and proc.returncode not in (0, 1):
        raise RuntimeError(f"git {' '.join(args)}: {proc.stderr.decode('utf-8', 'replace')}")
    return proc


def remote_branches(remote: str) -> list[str]:
    out = git("for-each-ref", f"refs/remotes/{remote}", "--format=%(refname:short)").stdout.decode()
    prefix = f"{remote}/"
    return sorted(b[len(prefix):] for b in out.split() if b.startswith(prefix) and b != f"{remote}/HEAD")


@dataclass
class Check:
    label: str
    ok: bool


@dataclass
class Evaluation:
    branch: str
    sha: str
    date: str
    checks: list[Check] = field(default_factory=list)

    @property
    def passed(self) -> int:
        return sum(c.ok for c in self.checks)

    @property
    def complete(self) -> bool:
        return bool(self.checks) and self.passed == len(self.checks)


class TreeChecker:
    """Kiểm tiêu chí trên một ref mà không checkout (git grep / git ls-tree trên object)."""

    def __init__(self, ref: str):
        self.ref = ref
        self._files: list[str] | None = None

    def files(self) -> list[str]:
        if self._files is None:
            self._files = git("ls-tree", "-r", "--name-only", self.ref).stdout.decode("utf-8").splitlines()
        return self._files

    def _grep(self, pattern: str, paths: list[str]) -> bool:
        return git("grep", "-q", "-P", "-e", pattern, self.ref, "--", *paths).returncode == 0

    def run(self, rule: dict) -> bool:
        paths = rule.get("in", [])
        paths = [paths] if isinstance(paths, str) else paths
        if "grep" in rule:
            patterns = rule["grep"] if isinstance(rule["grep"], list) else [rule["grep"]]
            return all(self._grep(p, paths) for p in patterns)
        if "absent" in rule:
            return not self._grep(rule["absent"], paths)
        if "file" in rule:
            return sum(fnmatch.fnmatchcase(f, rule["file"]) for f in self.files()) >= rule.get("min", 1)
        raise ValueError(f"Tiêu chí không hợp lệ: {rule}")


def evaluate(remote: str, branch: str, rules: list[dict]) -> Evaluation:
    ref = f"{remote}/{branch}"
    sha, date = git("log", "-1", "--format=%H %cs", ref).stdout.decode().split()
    checker = TreeChecker(ref)
    return Evaluation(branch, sha, date, [Check(r["label"], checker.run(r)) for r in rules])


# ---------------------------------------------------------------- chấm
@dataclass
class Verdict:
    task: Task
    total: int
    done: Evaluation | None = None          # nhánh chứng minh hoàn thành
    best_partial: Evaluation | None = None  # nhánh đúng buổi đạt nhiều tiêu chí nhất


def grade(tasks: list[Task], members: list[Member], rules: dict, remote: str,
          branches: list[str]) -> list[Verdict]:
    by_mssv = {m.mssv: m for m in members}
    verdicts = {t.key: Verdict(t, len(rules.get(t.key, []))) for t in tasks if rules.get(t.key)}
    for branch in branches:
        m = BRANCH_RE.match(branch)
        if not m or m["mssv"] not in by_mssv:
            continue
        owner, session = by_mssv[m["mssv"]], int(m["session"])
        for v in verdicts.values():
            if v.task.member is not owner:
                continue
            ev = evaluate(remote, branch, rules[v.task.key])
            if ev.complete:
                # ưu tiên nhánh đúng buổi của đầu việc, rồi tới commit sớm nhất
                rank = (session != v.task.session, ev.date)
                if v.done is None or rank < (_session_of(v.done.branch) != v.task.session, v.done.date):
                    v.done = ev
            elif session == v.task.session and ev.passed and (v.best_partial is None
                                                              or ev.passed > v.best_partial.passed):
                v.best_partial = ev
    return list(verdicts.values())


def _session_of(branch: str) -> int | None:
    m = BRANCH_RE.match(branch)
    return int(m["session"]) if m else None


# ---------------------------------------------------------------- GitHub
def issues_by_key(repo: str) -> dict[str, dict]:
    out = {}
    for issue in paginate(f"repos/{repo}/issues?state=all"):
        body = issue.get("body") or ""
        if "pull_request" not in issue and MARKER in body:
            out[body.split(MARKER, 1)[1].split(" -->", 1)[0]] = issue
    return out


STATUS_CELL_RE = re.compile(r"\| (Todo|In Progress|Done) \|$", re.MULTILINE)


def sync_body_status(repo: str, issue: dict, status: str) -> None:
    """Ô "Trạng thái" trong bảng tóm tắt ở đầu issue (do sync_board.py sinh) phải khớp cột trên Project."""
    body = issue.get("body") or ""
    new = STATUS_CELL_RE.sub(f"| {status} |", body, count=1)
    if new != body:
        gh(f"repos/{repo}/issues/{issue['number']}", "-X", "PATCH", payload={"body": new})
        issue["body"] = new


def evidence_comment(v: Verdict, repo: str) -> str:
    ev, t = v.done, v.task
    url = f"https://github.com/{repo}"
    lines = [AUTO_MARK, "### Tự động xác nhận hoàn thành", "",
             "| Buổi | Sinh viên | Nhánh | Commit | Ngày commit |", "|---|---|---|---|---|",
             f"| {t.session} | {t.member.name} ({t.member.mssv}) | [`{ev.branch}`]({url}/tree/{ev.branch}) "
             f"| [`{ev.sha[:7]}`]({url}/commit/{ev.sha}) | {ev.date} |", "",
             f"**Tiêu chí đạt {ev.passed}/{len(ev.checks)}:**"]
    lines += [f"- [x] {c.label}" for c in ev.checks]
    lines += ["", f"_Chấm tự động trên cây mã của nhánh theo `tools/github_board/completion_rules.json` "
                  f"(lúc {datetime.now(timezone.utc):%Y-%m-%d %H:%M} UTC)._"]
    return "\n".join(lines)


def apply(verdicts: list[Verdict], members: list[Member], cfg: dict, dry_run: bool) -> None:
    repo = cfg["repo"]
    project = Project(repo.split("/")[0], cfg["project_title"])
    fields = project.ensure_grading_fields(members)
    items = project.items()
    issues = issues_by_key(repo)

    def item_of(key: str) -> tuple[dict, dict] | None:
        issue = issues.get(key)
        if not issue:
            return None
        item = items.get(issue["number"])
        if item is None:
            item = {"id": project.add_issue(issue["node_id"]), "state": issue["state"].upper(), "values": {}}
            items[issue["number"]] = item
        return issue, item

    def put(item: dict, name: str, value: str | None) -> None:
        if item["values"].get(name) != value and not dry_run:
            project.set_value(item["id"], fields[name], value)
        item["values"][name] = value

    # 1. cột phân loại cho mọi đầu việc (kể cả đầu việc chưa có tiêu chí) — giáo viên nhóm theo buổi / sinh viên
    plan_tasks = {v.task.key: v.task for v in verdicts}
    plan_tasks.update(cfg.get("_all_tasks", {}))
    for key, task in plan_tasks.items():
        if got := item_of(key):
            put(got[1], FIELD_SESSION, session_option(task.session))
            put(got[1], FIELD_STUDENT, student_option(task.member))
    for m in members:
        if got := item_of(f"member-dev{m.dev}"):
            put(got[1], FIELD_STUDENT, student_option(m))

    # 2. kết quả chấm
    for v in sorted(verdicts, key=lambda v: (v.task.session, v.task.member.dev)):
        got = item_of(v.task.key)
        if not got:
            print(f"  ! chưa có issue cho {v.task.key} — chạy sync_board.py trước")
            continue
        issue, item = got
        current = item["values"].get("Status") or STATUS_TODO
        if v.done:
            ev = v.done
            put(item, FIELD_SCORE, f"{ev.passed}/{v.total}")
            put(item, FIELD_BRANCH, ev.branch)
            put(item, FIELD_COMMIT, ev.sha[:7])
            put(item, FIELD_DONE_AT, ev.date)
            if current != STATUS_DONE:
                print(f"  ✔ #{issue['number']} {v.task.title} → Done ({ev.branch} @ {ev.sha[:7]})")
                put(item, "Status", STATUS_DONE)
            if not dry_run:
                sync_body_status(repo, issue, STATUS_DONE)
                if issue["state"] != "closed":
                    gh(f"repos/{repo}/issues/{issue['number']}", "-X", "PATCH",
                       payload={"state": "closed", "state_reason": "completed"})
                comments = paginate(f"repos/{repo}/issues/{issue['number']}/comments")
                if not any(AUTO_MARK in (c.get("body") or "") for c in comments):
                    gh(f"repos/{repo}/issues/{issue['number']}/comments", "-X", "POST",
                       payload={"body": evidence_comment(v, repo)})
        elif v.best_partial and current != STATUS_DONE:
            ev = v.best_partial
            put(item, FIELD_SCORE, f"{ev.passed}/{v.total}")
            put(item, FIELD_BRANCH, ev.branch)
            if STATUS_RANK[current] < STATUS_RANK[STATUS_DOING]:
                print(f"  … #{issue['number']} {v.task.title} → In Progress ({ev.passed}/{v.total}, {ev.branch})")
                put(item, "Status", STATUS_DOING)
            if not dry_run:
                sync_body_status(repo, issue, item["values"].get("Status") or STATUS_TODO)

    # 3. issue cha của mỗi sinh viên theo các sub-issue
    for m in members:
        got = item_of(f"member-dev{m.dev}")
        if not got:
            continue
        issue, item = got
        child = [items[i["number"]]["values"].get("Status") or STATUS_TODO
                 for k, i in issues.items() if k.startswith("b") and f"-dev{m.dev}-" in k and i["number"] in items]
        status = (STATUS_DONE if child and all(s == STATUS_DONE for s in child)
                  else STATUS_DOING if any(s != STATUS_TODO for s in child) else STATUS_TODO)
        if STATUS_RANK[status] > STATUS_RANK[item["values"].get("Status") or STATUS_TODO]:
            put(item, "Status", status)
        done = sum(s == STATUS_DONE for s in child)
        put(item, FIELD_SCORE, f"{done}/{len(child)} đầu việc")


# ---------------------------------------------------------------- view & README cho giáo viên
TEACHER_README = """## Cách đọc bảng (dành cho giảng viên)

- **Tab "Chấm bài – theo buổi"**: mỗi hàng ngang là một **buổi**, mỗi cột là **trạng thái** (Todo / In Progress / Done); mỗi thẻ là phần việc của **một sinh viên** trong buổi đó.
- **Tab "Chấm bài – bảng"**: cùng dữ liệu dạng bảng, nhóm theo buổi, có cột *Tiêu chí đạt*, *Nhánh minh chứng*, *Commit minh chứng*, *Ngày hoàn thành*.
- **Tab "Theo sinh viên"**: nhóm theo từng sinh viên, sắp theo buổi.

## Đầu việc chuyển sang Done tự động như thế nào

1. Mỗi lần có người **push bất kỳ nhánh nào** (và định kỳ mỗi 30 phút), workflow `.github/workflows/project-board.yml` chạy `tools/github_board/auto_complete.py`.
2. Script nhận ra chủ nhánh qua **MSSV ở đầu tên nhánh** (`{MSSV}_{HoTen}_buoiso{n}`) và chỉ chấm **đầu việc của chính người đó**.
3. Mỗi đầu việc có danh sách tiêu chí kiểm được trên mã nguồn (class, route API, migration, kiểm thử, trang giao diện…) trong `tools/github_board/completion_rules.json`, lấy từ mục *Hướng đi / Xong khi* của README.
4. Đạt **đủ** tiêu chí → thẻ sang **Done**, issue được đóng, kèm comment liệt kê tiêu chí cùng nhánh, commit và ngày làm minh chứng. Đạt **một phần** ở nhánh đúng buổi → **In Progress** và hiện *x/y*.
5. Trạng thái **không bao giờ bị hạ** khi chạy lại.

Tiêu chí tự động chỉ xác nhận *sản phẩm có mặt và có kiểm thử*, không thay cho việc đọc code; mọi thẻ Done đều có commit để mở ra xem.
"""


def ensure_views(cfg: dict) -> list[str]:
    repo = cfg["repo"]
    owner = repo.split("/")[0]
    project = Project(owner, cfg["project_title"])
    project.set_readme(TEACHER_README, cfg["project_description"])
    rest = {f["name"]: f["id"] for f in gh(f"users/{owner}/projectsV2/{project.number}/fields")}
    have = project.view_names()

    def ids(*names: str) -> list[int]:
        return [rest[n] for n in names if n in rest]

    tasks_only = "-label:thanh-vien"
    wanted = [
        ("Chấm bài – theo buổi", {
            "layout": "board", "filter": tasks_only,
            "vertical_group_by": ids("Status"), "group_by": ids(FIELD_SESSION),
            "sort_by": [[rest[FIELD_STUDENT], "asc"]],
            "visible_fields": ids("Title", "Assignees", FIELD_STUDENT, FIELD_SCORE, FIELD_COMMIT, FIELD_DONE_AT)}),
        ("Chấm bài – bảng", {
            "layout": "table", "filter": tasks_only, "group_by": ids(FIELD_SESSION),
            "sort_by": [[rest[FIELD_STUDENT], "asc"]],
            "visible_fields": ids("Title", FIELD_STUDENT, "Status", FIELD_SCORE, FIELD_BRANCH, FIELD_COMMIT,
                                  FIELD_DONE_AT)}),
        ("Theo sinh viên", {
            "layout": "table", "filter": tasks_only, "group_by": ids(FIELD_STUDENT),
            "sort_by": [[rest[FIELD_SESSION], "asc"]],
            "visible_fields": ids("Title", FIELD_SESSION, "Status", FIELD_SCORE, FIELD_COMMIT, FIELD_DONE_AT)}),
    ]
    missing = []
    for name, spec in wanted:
        if name not in have and not project.create_view(name, spec):
            missing.append(name)
    return missing


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--branch", action="append", default=[], help="nhánh cần chấm (lặp được)")
    ap.add_argument("--all", action="store_true", help="chấm mọi nhánh trên remote")
    ap.add_argument("--remote", default="origin")
    ap.add_argument("--dry-run", action="store_true", help="chỉ in kết quả, không ghi lên GitHub")
    ap.add_argument("--fetch", action="store_true", help="git fetch remote trước khi chấm")
    args = ap.parse_args()

    cfg = json.loads((HERE / "config.json").read_text(encoding="utf-8"))
    rules = {k: v for k, v in json.loads((HERE / "completion_rules.json").read_text(encoding="utf-8")).items()
             if not k.startswith("_")}
    plan = parse(ROOT / "README.md", f"https://github.com/{cfg['repo']}/blob/{cfg['branch']}")
    if args.fetch:
        git("fetch", "--prune", args.remote)

    branches = remote_branches(args.remote) if args.all or not args.branch else args.branch
    if not args.all and args.branch and not any(BRANCH_RE.match(b) for b in branches):
        print(f"Nhánh {', '.join(branches)} không mang MSSV của sinh viên nào — không có gì để chấm.")
        return
    if not args.dry_run and "GH_TOKEN" in os.environ and not os.environ["GH_TOKEN"]:
        sys.exit("Thiếu secret BOARD_TOKEN (PAT scope repo + project) — xem tools/github_board/README.md.")
    verdicts = grade(plan.tasks, plan.members, rules, args.remote, branches)
    print(f"Chấm {len(branches)} nhánh, {len(verdicts)} đầu việc có tiêu chí:")
    for v in sorted(verdicts, key=lambda v: (v.task.session, v.task.member.dev)):
        if v.done:
            state = f"ĐẠT {v.done.passed}/{v.total} — {v.done.branch} @ {v.done.sha[:7]} ({v.done.date})"
        elif v.best_partial:
            missing = [c.label for c in v.best_partial.checks if not c.ok]
            state = f"{v.best_partial.passed}/{v.total} — {v.best_partial.branch}; thiếu: " + "; ".join(missing)
        else:
            state = "chưa có nhánh"
        print(f"  [{v.task.key}] {v.task.member.name}: {state}")

    if args.dry_run:
        return
    cfg["_all_tasks"] = {t.key: t for t in plan.tasks}
    apply(verdicts, plan.members, cfg, dry_run=False)
    missing = ensure_views(cfg)
    if missing:
        print("Không tạo được view qua API: " + ", ".join(missing))
    print("Xong.")


if __name__ == "__main__":
    main()
