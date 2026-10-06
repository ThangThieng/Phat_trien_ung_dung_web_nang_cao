"""Đọc README.md và dựng danh sách đầu việc (mỗi thành viên × mỗi buổi).

Nguồn duy nhất là README: bảng "Thành viên", bảng "Chia việc tổng thể" (Phần 2)
và các mục "### Dev n – Họ tên · ..." trong từng "## BUỔI n" (Phần 3).
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field
from pathlib import Path

SESSION_RE = re.compile(r"^## BUỔI (\d+) – (.+)$")
TASK_RE = re.compile(r"^### (.+)$")
DEV_RE = re.compile(r"^Dev (\d) – (.+?) · (.+)$")
LINK_RE = re.compile(r"\]\((?!https?://|#|mailto:)([^)\s]+)\)")
DONE_MARKERS = ("(đã hoàn thành", "— ✅ đã xong")


@dataclass
class Member:
    dev: int
    name: str
    mssv: str
    area: str
    branch: str
    web_scope: str = ""
    frs: str = ""


@dataclass
class Task:
    key: str
    session: int
    session_title: str
    member: Member
    title: str
    body: str
    done_in_readme: bool


@dataclass
class Plan:
    members: list[Member]
    tasks: list[Task] = field(default_factory=list)
    schedule: dict[int, dict[int, str]] = field(default_factory=dict)  # dev -> buổi -> việc


def _cells(line: str) -> list[str]:
    return [c.strip() for c in line.strip().strip("|").split("|")]


def _strip_md(text: str) -> str:
    return text.replace("**", "").strip()


def _table_after(lines: list[str], heading_prefix: str) -> list[list[str]]:
    """Trả các hàng dữ liệu của bảng markdown đầu tiên sau dòng bắt đầu bằng heading_prefix."""
    start = next(i for i, l in enumerate(lines) if l.startswith(heading_prefix))
    rows: list[list[str]] = []
    i = start + 1
    while i < len(lines) and not lines[i].startswith("|"):
        i += 1
    while i < len(lines) and lines[i].startswith("|"):
        rows.append(_cells(lines[i]))
        i += 1
    return rows[2:]  # bỏ tiêu đề + dòng |---|


def _absolutize_links(text: str, blob_base: str) -> str:
    return LINK_RE.sub(lambda m: f"]({blob_base}/{m.group(1)})", text)


def _clean_title(text: str) -> str:
    for marker in ("(đã hoàn thành)", "— ✅ đã xong"):
        text = text.replace(marker, "")
    text = re.sub(r"\(đã hoàn thành[^)]*\)", "", text)
    return text.strip()


def parse(readme: Path, blob_base: str) -> Plan:
    lines = readme.read_text(encoding="utf-8").splitlines()

    members: list[Member] = []
    for row in _table_after(lines, "## Thành viên"):
        role, name, mssv, area, branch = row[:5]
        members.append(Member(int(role.split()[1]), _strip_md(name).replace(" (trưởng nhóm)", ""),
                              mssv, area, branch.strip("`")))
    by_dev = {m.dev: m for m in members}

    def member_by_name(text: str) -> Member:
        hits = [m for m in members if m.name in text]
        if len(hits) != 1:
            raise ValueError(f"Không xác định được thành viên trong: {text!r}")
        return hits[0]

    for row in _table_after(lines, "# Phần 2 – Chia việc tổng thể"):
        m = member_by_name(_strip_md(row[0]))
        m.web_scope, m.frs = row[1], row[2]

    plan = Plan(members)
    header = _cells(next(l for l in lines if l.startswith("| Buổi | Dev 1")))
    sched_start = lines.index(next(l for l in lines if l.startswith("| Buổi | Dev 1")))
    i = sched_start + 2
    while lines[i].startswith("|"):
        row = _cells(lines[i])
        session = int(re.match(r"\d+", row[0]).group())
        for col, cell in zip(header[1:], row[1:]):
            dev = int(col.split()[1])
            plan.schedule.setdefault(dev, {})[session] = cell
        i += 1

    # ---- Phần 3: tách từng buổi ----
    sessions: list[tuple[int, str, int, int]] = []
    for idx, line in enumerate(lines):
        if m := SESSION_RE.match(line):
            sessions.append((int(m.group(1)), m.group(2), idx, -1))
    end_of_part3 = next(i for i, l in enumerate(lines) if l.startswith("# Phụ lục"))
    sessions = [(n, t, s, sessions[k + 1][2] if k + 1 < len(sessions) else end_of_part3)
                for k, (n, t, s, _) in enumerate(sessions)]

    for number, stitle, start, end in sessions:
        block = lines[start + 1:end]
        counters: dict[int, int] = {}

        def add(member: Member, title: str, body: str, done: bool) -> None:
            counters[member.dev] = counters.get(member.dev, 0) + 1
            plan.tasks.append(Task(
                key=f"b{number}-dev{member.dev}-{counters[member.dev]}",
                session=number, session_title=_clean_title(stitle), member=member,
                title=f"[Buổi {number}] {title}",
                body=_absolutize_links(body.strip(), blob_base), done_in_readme=done))

        task_starts = [k for k, l in enumerate(block) if TASK_RE.match(l)]
        if not task_starts:
            # Buổi 1: không có mục "### Dev", việc từng người nằm trong bảng + các gạch đầu dòng chung
            table = [_cells(l) for l in block if l.startswith("| **Dev")]
            shared = "\n".join(l for l in block if l.startswith("- **"))
            goal = next((l for l in block if l.startswith("**Mục tiêu:**")), "")
            for row in table:
                member = member_by_name(_strip_md(row[0]))
                body = (f"{goal}\n\n- **Phần SRS đọc kỹ:** {row[1]}\n"
                        f"- **Cần trả lời được sau buổi:** {row[2]}\n\n**Việc chung của cả nhóm:**\n{shared}")
                add(member, f"Đọc đặc tả: {row[1]}", body, "(đã hoàn thành" in stitle)
            continue

        for n, k in enumerate(task_starts):
            stop = task_starts[n + 1] if n + 1 < len(task_starts) else len(block)
            section = block[k + 1:stop]
            # phần "Kiểm chứng cuối Buổi" và "---" là của cả buổi, không thuộc mục cuối
            cut = next((j for j, l in enumerate(section)
                        if l.startswith("**Kiểm chứng cuối") or l.strip() == "---"), len(section))
            heading = TASK_RE.match(block[k]).group(1)
            done = any(mark in heading for mark in DONE_MARKERS) or "(đã hoàn thành" in stitle
            if dm := DEV_RE.match(heading):
                member = by_dev[int(dm.group(1))]
                title = _clean_title(dm.group(3))
            else:  # vd. "Commit nền – Nguyễn Thăng Thiêng dẫn · D-11 (422 → 400)"
                member = member_by_name(heading)
                left, _, right = heading.partition(" · ")
                title = _clean_title(f"{left.split(' – ')[0]} · {right}")
            add(member, title, "\n".join(section[:cut]), done)

    return plan
