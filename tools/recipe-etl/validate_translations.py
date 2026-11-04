"""Kiểm tra tự động bản dịch. Không thay được người duyệt: công cụ này không phát hiện dịch sai nghĩa.

    python validate_translations.py          # in mọi lỗi; thoát với mã 1 nếu có lỗi hoặc còn công thức chưa dịch
    python validate_translations.py --partial  # bỏ qua công thức chưa dịch (khi đang dịch dở)
"""
from __future__ import annotations

import argparse
import json
import re
import sys

from ingredients import FRACTIONS
from mediawiki import OUT, ROOT
from transform import SEED_DATA, legacy_slugs, slugify

TRANSLATIONS = ROOT / "translations"
LIMITS = {"title": (5, 200), "description": (20, 2000), "name": (1, 200), "notes": (0, 500), "text": (0, 50), "unit": (0, 50), "stepTitle": (1, 200), "stepDescription": (1, 2000)}
_VIETNAMESE = re.compile(r"[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]", re.I)
_ENGLISH = re.compile(r"\b(the|and|with|until|into|then|from|over|your|this)\b", re.I)
_PARENS = re.compile(r"\([^)]*\)")
_FRACTION_NAMES = {char: f" {int(round(value * d))}/{d}" for char, value in FRACTIONS.items() for d in (2, 3, 4, 5, 6, 8) if abs(value * d - round(value * d)) < 1e-9 and round(value * d) >= 1 and all(abs(value * x - round(value * x)) > 1e-9 for x in range(1, d))}


def numbers(text: str) -> set[str]:
    """Các con số trong văn bản, đã chuẩn hóa phân số Unicode và dấu thập phân."""
    for char, name in _FRACTION_NAMES.items():
        text = text.replace(char, name)
    found = re.findall(r"\d+(?:[.,]\d+)?(?:/\d+)?", text)
    return {f.replace(",", ".") for f in found}


def load_translations() -> dict[int, dict]:
    result: dict[int, dict] = {}
    for path in sorted(TRANSLATIONS.glob("batch-*.json")):
        for entry in json.loads(path.read_text(encoding="utf-8")):
            if entry["pageId"] in result:
                raise SystemExit(f"{path.name}: pageId {entry['pageId']} bị dịch hai lần")
            result[entry["pageId"]] = entry
    return result


def _length(errors: list[str], where: str, value: str | None, limit: str) -> None:
    low, high = LIMITS[limit]
    size = len(value or "")
    if not low <= size <= high:
        errors.append(f"{where}: dài {size} ký tự, giới hạn {low}–{high}")


def _lost_numbers(errors: list[str], where: str, source: str, target: str) -> None:
    lost = numbers(source) - numbers(target)
    if lost:
        errors.append(f"{where}: mất số {sorted(lost)}")


def _english_left(errors: list[str], where: str, text: str) -> None:
    match = _ENGLISH.search(_PARENS.sub("", text))
    if match:
        errors.append(f"{where}: còn từ tiếng Anh '{match[0]}'")


def validate_recipe(en: dict, vi: dict) -> list[str]:
    errors: list[str] = []
    head = f"{en['pageId']} {en['title']}"
    if vi.get("sourceHash") != en["sourceHash"]:
        errors.append(f"{head}: sourceHash lệch, bản gốc đã đổi sau khi dịch")
    _length(errors, f"{head} / tiêu đề", vi.get("title"), "title")
    _length(errors, f"{head} / mô tả", vi.get("description"), "description")
    if not _VIETNAMESE.search(vi.get("title", "")):
        errors.append(f"{head}: tiêu đề không có chữ tiếng Việt")
    if not _VIETNAMESE.search(vi.get("description", "")):
        errors.append(f"{head}: mô tả không có chữ tiếng Việt")
    _lost_numbers(errors, f"{head} / mô tả", en["description"], vi.get("description", ""))
    _english_left(errors, f"{head} / mô tả", vi.get("description", ""))
    if len(vi.get("ingredients", [])) != len(en["ingredients"]):
        errors.append(f"{head}: {len(vi.get('ingredients', []))} nguyên liệu, bản gốc có {len(en['ingredients'])}")
    if len(vi.get("steps", [])) != len(en["steps"]):
        errors.append(f"{head}: {len(vi.get('steps', []))} bước, bản gốc có {len(en['steps'])}")
    names: set[str] = set()
    for index, (source, item) in enumerate(zip(en["ingredients"], vi.get("ingredients", [])), 1):
        where = f"{head} / nguyên liệu {index}"
        _length(errors, where, item.get("n"), "name")
        _length(errors, where, item.get("o"), "notes")
        _length(errors, where, item.get("t"), "text")
        _length(errors, where, item.get("u"), "unit")
        lowered = (item.get("n") or "").lower()
        if lowered in names:
            errors.append(f"{where}: trùng tên '{item.get('n')}'")
        names.add(lowered)
        _english_left(errors, where, f"{item.get('n', '')} {item.get('o') or ''}")
        if source["kind"] == "taste" and source["text"] not in ("to taste", "as needed", "optional") and "t" not in item:
            errors.append(f"{where}: thiếu bản dịch cho '{source['text']}'")
    for index, (source, item) in enumerate(zip(en["steps"], vi.get("steps", [])), 1):
        where = f"{head} / bước {index}"
        _length(errors, where, item.get("t"), "stepTitle")
        _length(errors, where, item.get("d"), "stepDescription")
        _lost_numbers(errors, where, source, item.get("d", ""))
        _english_left(errors, where, item.get("d", ""))
        if not _VIETNAMESE.search(item.get("d", "")):
            errors.append(f"{where}: không có chữ tiếng Việt")
    return errors


def validate_all(partial: bool = False) -> tuple[list[str], int]:
    recipes = json.loads((OUT / "recipes_en.json").read_text(encoding="utf-8"))
    translations = load_translations()
    existing = legacy_slugs(SEED_DATA)
    errors: list[str] = []
    seen_slugs: dict[str, int] = {}
    translated = 0
    known = {r["pageId"] for r in recipes}
    errors += [f"pageId {page_id} có bản dịch nhưng không còn trong danh sách" for page_id in translations if page_id not in known]
    for recipe in recipes:
        entry = translations.get(recipe["pageId"])
        if entry is None:
            if not partial:
                errors.append(f"{recipe['pageId']} {recipe['title']}: chưa dịch")
            continue
        translated += 1
        errors += validate_recipe(recipe, entry)
        slug = slugify(entry.get("title", ""))
        if slug in existing:
            errors.append(f"{recipe['pageId']}: slug '{slug}' trùng công thức cũ")
        if slug in seen_slugs:
            errors.append(f"{recipe['pageId']}: slug '{slug}' trùng công thức {seen_slugs[slug]}")
        seen_slugs[slug] = recipe["pageId"]
    return errors, translated


def stamp() -> None:
    """Hash ghi nhận bản gốc tại thời điểm dịch; sau này bản gốc đổi thì validate báo lỗi."""
    hashes = {r["pageId"]: r["sourceHash"] for r in json.loads((OUT / "recipes_en.json").read_text(encoding="utf-8"))}
    for path in sorted(TRANSLATIONS.glob("batch-*.json")):
        entries = json.loads(path.read_text(encoding="utf-8"))
        if any("sourceHash" not in e and e["pageId"] in hashes for e in entries):
            for entry in entries:
                entry.setdefault("sourceHash", hashes.get(entry["pageId"]))
            path.write_text(json.dumps(entries, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser()
    parser.add_argument("--partial", action="store_true")
    parser.add_argument("--stamp", action="store_true", help="gắn sourceHash cho bản dịch mới chưa có hash")
    args = parser.parse_args()
    if args.stamp:
        stamp()
    errors, translated = validate_all(args.partial)
    for error in errors:
        print(error)
    print(f"Đã dịch {translated}; {len(errors)} lỗi")
    sys.exit(1 if errors else 0)


if __name__ == "__main__":
    main()
