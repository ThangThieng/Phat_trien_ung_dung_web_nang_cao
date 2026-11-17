"""Ghép cấu trúc gốc + bản dịch + ảnh thành Seed/Data/recipes-wikibooks-*.json và các báo cáo duyệt.

Chỉ ghi khi validate_translations.py sạch, để không có bản dịch hỏng nào lọt vào dữ liệu nhúng.
"""
from __future__ import annotations

import csv
import html
import json
import re
import sys

from mediawiki import OUT
from transform import SEED_DATA, slugify
from validate_translations import load_translations, validate_all

CHUNK = 25
LICENSE_URL = "https://creativecommons.org/licenses/by-sa/4.0/"
SOURCE_PREFIX = "Nguồn dữ liệu: Wikibooks Cookbook"
DIFFICULTY = {1: "Easy", 2: "Easy", 3: "Medium", 4: "Hard", 5: "Expert"}
QUANTITY_TEXT = {"to taste": "vừa ăn", "as needed": "vừa đủ", "optional": "tùy chọn"}
# Theo cách viết của 100 công thức do nhóm biên soạn. Đơn vị đo Mỹ giữ tên, KHÔNG quy đổi giá trị.
UNITS = {
    "cup": "cốc", "c": "cốc", "tablespoon": "thìa canh", "tbsp": "thìa canh", "tbs": "thìa canh", "teaspoon": "thìa cà phê", "tsp": "thìa cà phê",
    "gram": "gram", "g": "gram", "kilogram": "kg", "kg": "kg", "milliliter": "ml", "millilitre": "ml", "ml": "ml", "liter": "lít", "litre": "lít", "l": "lít",
    "pound": "pound", "lb": "pound", "ounce": "oz", "oz": "oz", "fl oz": "fl oz", "fluid ounce": "fl oz", "quart": "quart", "qt": "quart", "pint": "pint", "pt": "pint",
    "gallon": "gallon", "clove": "tép", "can": "lon", "pinch": "nhúm", "dash": "chút", "stick": "thanh", "slice": "lát", "bunch": "bó", "sprig": "nhánh", "piece": "miếng",
    "head": "củ", "stalk": "cọng", "package": "gói", "pkg": "gói", "packet": "gói", "jar": "hũ", "bottle": "chai", "handful": "nắm", "drop": "giọt", "leaf": "lá", "leave": "lá", "mg": "mg",
}


def unit_vi(unit: str | None) -> str | None:
    if not unit:
        return None
    key = unit.lower().rstrip(".").strip()
    key = re.sub(r"^fl\.? ?oz$", "fl oz", key)
    if key in UNITS:
        return UNITS[key]
    for suffix in ("es", "s"):
        if key.endswith(suffix) and key[: -len(suffix)] in UNITS:
            return UNITS[key[: -len(suffix)]]
    raise SystemExit(f"Đơn vị chưa có bản dịch: {unit}")


def range_text(text: str) -> str:
    return re.sub(r"\s*(?:-|–|—|\bto\b)\s*", "–", text)


def merge_ingredient(source: dict, item: dict) -> list:
    quantity = item["q"] if "q" in item else source["quantity"]
    unit = item["u"] if "u" in item else unit_vi(source["unit"])
    if "t" in item:
        text = item["t"]
    elif source["kind"] == "range":
        text = range_text(source["text"])
    else:
        text = QUANTITY_TEXT.get((source["text"] or "").lower())
    if quantity is None and text is None and unit is None:
        text = "vừa đủ"  # INGREDIENT_QUANTITY_REQUIRED không cho rỗng cả ba cột.
    row = [item["n"], quantity if quantity is not None else text, unit]
    if item.get("o"):
        row.append(item["o"])
    return row


def attribution(recipe: dict, reused_page_title: str) -> str:
    image = recipe["image"]
    artist = image["artist"] or "không rõ tác giả"
    url = f"https://en.wikibooks.org/w/index.php?curid={recipe['pageId']}&oldid={recipe['revisionId']}"
    return (f'{SOURCE_PREFIX}, trang "{reused_page_title}" ({url}), các tác giả Wikibooks, giấy phép CC BY-SA 4.0 ({LICENSE_URL}). '
            f"Đã dịch sang tiếng Việt (có hỗ trợ AI) và chuyển đổi định dạng. "
            f"Ảnh: {artist}, {image['license']}, {image['descriptionUrl']}.")


def build_recipe(recipe: dict, entry: dict) -> dict:
    return {
        "title": entry["title"],
        "category": recipe["category"],
        "description": entry["description"],
        "prep": recipe["prep"],
        "cook": recipe["cook"],
        "servings": recipe["servings"],
        "difficulty": DIFFICULTY[recipe["difficulty"]],
        "ingredients": [merge_ingredient(s, i) for s, i in zip(recipe["ingredients"], entry["ingredients"])],
        "steps": [[i["t"], i["d"], timer] for i, timer in zip(entry["steps"], recipe["timers"])],
        "nutrition": recipe["nutrition"],
        "source": {"pageId": recipe["pageId"], "revisionId": recipe["revisionId"], "title": recipe["pageTitle"]},
        "image": {k: recipe["image"][k] for k in ("file", "url", "artist", "license", "descriptionUrl")},
        "attribution": attribution(recipe, recipe["pageTitle"]),
    }


def dump(recipes: list[dict]) -> str:
    """Mỗi nguyên liệu, mỗi bước một dòng để diff và duyệt dễ đọc."""
    def value(item) -> str:
        return json.dumps(item, ensure_ascii=False)

    blocks = []
    for recipe in recipes:
        lines = []
        for key, item in recipe.items():
            if key in ("ingredients", "steps"):
                body = ",\n".join(f"      {value(row)}" for row in item)
                lines.append(f'    "{key}": [\n{body}\n    ]')
            else:
                lines.append(f'    "{key}": {value(item)}')
        blocks.append("  {\n" + ",\n".join(lines) + "\n  }")
    return "[\n" + ",\n".join(blocks) + "\n]\n"


def write_reports(recipes: list[dict], entries: dict[int, dict]) -> None:
    with (OUT / "review_categories.csv").open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(["pageId", "title", "titleVi", "category", "rule"])
        for recipe in recipes:
            writer.writerow([recipe["pageId"], recipe["title"], entries[recipe["pageId"]]["title"], recipe["category"], recipe["categoryRule"]])
    cards = []
    for recipe in recipes:
        entry = entries[recipe["pageId"]]
        ingredients = "".join(f"<tr><td>{html.escape(s['raw'])}</td><td>{html.escape(str(merge_ingredient(s, i)))}</td></tr>" for s, i in zip(recipe["ingredients"], entry["ingredients"]))
        steps = "".join(f"<tr><td>{html.escape(s)}</td><td><b>{html.escape(i['t'])}</b>: {html.escape(i['d'])}</td></tr>" for s, i in zip(recipe["steps"], entry["steps"]))
        cards.append(f"<article><h2>{html.escape(entry['title'])}</h2><p><i>{html.escape(recipe['title'])}</i> · {html.escape(recipe['category'])} · "
                     f"chuẩn bị {recipe['prep']}′, nấu {recipe['cook']}′ · {recipe['servings']} phần · <a href='{html.escape(recipe['image']['descriptionUrl'])}'>ảnh</a></p>"
                     f"<img src='{html.escape(recipe['image']['url'])}' loading='lazy' width='320'>"
                     f"<table><tr><td>{html.escape(recipe['description'])}</td><td>{html.escape(entry['description'])}</td></tr>{ingredients}{steps}</table></article>")
    page = ("<!doctype html><meta charset='utf-8'><title>Duyệt bản dịch</title><style>body{font:14px system-ui;margin:2rem}table{border-collapse:collapse;width:100%}"
            "td{border:1px solid #ccc;padding:4px;vertical-align:top;width:50%}article{margin-bottom:3rem}</style>" + "".join(cards))
    (OUT / "review_recipes.html").write_text(page, encoding="utf-8")


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8")
    errors, translated = validate_all()
    if errors:
        for error in errors:
            print(error)
        raise SystemExit(f"Không ghi dữ liệu: {len(errors)} lỗi")
    recipes = json.loads((OUT / "recipes_en.json").read_text(encoding="utf-8"))
    entries = load_translations()
    built = [build_recipe(r, entries[r["pageId"]]) for r in recipes]
    built.sort(key=lambda r: slugify(r["title"]))
    for old in SEED_DATA.glob("recipes-wikibooks-*.json"):
        old.unlink()
    for number, start in enumerate(range(0, len(built), CHUNK), 1):
        (SEED_DATA / f"recipes-wikibooks-{number:02d}.json").write_text(dump(built[start : start + CHUNK]), encoding="utf-8")
    write_reports(recipes, entries)
    print(f"XONG: {translated} công thức → Seed/Data/recipes-wikibooks-*.json")


if __name__ == "__main__":
    main()
