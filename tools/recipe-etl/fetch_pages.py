"""Tải trang công thức từ Wikibooks vào raw/pages.json.

    python fetch_pages.py            # toàn bộ Category:Recipes, bản mới nhất (5–8 phút)
    python fetch_pages.py --pinned   # đúng các revision trong pins.json, để tái lập kết quả đã duyệt
"""
from __future__ import annotations

import argparse
import json

from mediawiki import PINS, RAW, batches, query

REVISION_PROPS = {"prop": "revisions", "rvprop": "ids|timestamp|content", "rvslots": "main"}


def fetch_all() -> list[dict]:
    members: list[dict] = []
    cursor: dict = {}
    while True:
        data = query({"action": "query", "list": "categorymembers", "cmtitle": "Category:Recipes", "cmlimit": 500, "cmnamespace": 102, "cmprop": "ids|title", **cursor})
        members += data["query"]["categorymembers"]
        print(f"đã liệt kê {len(members)} trang", flush=True)
        if "continue" not in data:
            break
        cursor = data["continue"]
    pages: list[dict] = []
    for batch in batches(members, 50):
        data = query({"action": "query", "pageids": "|".join(str(m["pageid"]) for m in batch), **REVISION_PROPS})
        pages += data["query"]["pages"]
        print(f"đã tải {len(pages)}/{len(members)}", flush=True)
    return pages


def fetch_pinned() -> list[dict]:
    pins = json.loads(PINS.read_text(encoding="utf-8"))
    pages: list[dict] = []
    for batch in batches(sorted(pins.values()), 50):
        data = query({"action": "query", "revids": "|".join(str(r) for r in batch), **REVISION_PROPS})
        pages += data["query"]["pages"]
    missing = set(pins.values()) - {p["revisions"][0]["revid"] for p in pages if p.get("revisions")}
    if missing:
        raise SystemExit(f"Không tải được các revision: {sorted(missing)}")
    return pages


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--pinned", action="store_true")
    args = parser.parse_args()
    pages = fetch_pinned() if args.pinned else fetch_all()
    RAW.mkdir(exist_ok=True)
    pages.sort(key=lambda p: p["pageid"])
    (RAW / "pages.json").write_text(json.dumps(pages, ensure_ascii=False), encoding="utf-8")
    print(f"XONG: {len(pages)} trang → raw/pages.json")


if __name__ == "__main__":
    main()
