"""Lọc và ánh xạ trang Wikibooks thành out/recipes_en.json, kèm báo cáo phễu lọc.

    python transform.py                # sinh out/recipes_en.json và out/report.json
    python transform.py --write-pins   # thêm: ghi pins.json từ danh sách đã chọn
"""
from __future__ import annotations

import argparse
import collections
import csv
import hashlib
import json
import re
import unicodedata
from pathlib import Path

import mwparserfromhell as mw

import taxonomy
import wikitext
from ingredients import parse_line
from mediawiki import OUT, PINS, RAW, ROOT

SEED_DATA = ROOT.parents[1] / "backend/src/CulinaryBlog.Infrastructure/Persistence/Seed/Data"
EXCLUSIONS = ROOT / "exclusions.json"
CATEGORY_OVERRIDES = ROOT / "category_overrides.csv"

HISTORIC = {"1881", "decameron", "hartshorne", "household cyclopedia", "beeton"}
STEP_SECTIONS = ("procedure", "procedures", "method", "directions", "instructions", "steps", "preparation")
MIN_INGREDIENTS, MIN_STEPS, MIN_DESCRIPTION = 10, 5, 40  # SRS §2.6.1; mô tả ngắn hơn thường chỉ là dòng điều hướng.
MAX_DOWNLOAD_BYTES = 5 * 1024 * 1024  # CONS-007
ALLOWED_LICENSE = re.compile(r"^(cc0|cc[- ]?by(-sa)?[- ]\d|public domain)", re.I)
ALLOWED_MIME = {"image/jpeg", "image/png", "image/webp"}
ALLOWED_HOSTS = ("https://upload.wikimedia.org/", "https://thumb.wikimedia.org/")

_DURATION = re.compile(r"(\d+(?:\.\d+)?)\s*(?:[-–]\s*\d+(?:\.\d+)?\s*)?(hours?|hrs?|minutes?|mins?)\b", re.I)
_DAYS = re.compile(r"(\d+)\s*days?", re.I)
_COMPOUND = re.compile(r"(\d+)\s*(?:hours?|hrs?)\s*(?:and\s*)?(\d+)\s*(?:minutes?|mins?)\b", re.I)  # "1 hour and 15 minutes" là MỘT mốc
_HEAT = re.compile(r"\b(?:bak|boil|broil|cook|fry|fries|fried|grill|heat|microwav|poach|roast|saut|sear|simmer|steam|stew|toast)", re.I)
_SERVINGS = re.compile(r"^\s*(?:about |serves |makes )?(\d+)(?:\s*[-–]\s*\d+)?\s*(?:servings?|people|persons?|portions?)?\s*$", re.I)
_YIELD = re.compile(r"^\s*(?:about |makes )?(\d+)(?:\s*[-–]\s*\d+)?\s*(?:ea\.?|pcs\.?|pieces?|cookies|muffins|pancakes|rolls|bars|cups?|dumplings|buns|crepes|scones|biscuits|sandwich(?:es)?|tacos|balls|squares|slices)\b", re.I)

_NUTRITION_NAMED = {"cals": "calories", "carbs": "carbohydrates", "totalfat": "fat", "protein": "protein", "fiber": "fiber", "sodium": "sodium"}
# {{nutritionsummary|khẩu phần|số khẩu phần|kcal|kcal từ béo|béo|béo bão hòa|cholesterol|natri|tinh bột|xơ|đường|đạm|…}}
_NUTRITION_POSITIONAL = {3: "calories", 5: "fat", 8: "sodium", 9: "carbohydrates", 10: "fiber", 12: "protein"}
NUTRITION_TOLERANCE = 0.35


def slugify(text: str) -> str:
    """Giống ``SlugHelper.Generate`` (Domain/Common), để báo trùng slug ngay trong ETL."""
    lowered = text.lower().replace("đ", "d")
    stripped = "".join(c for c in unicodedata.normalize("NFD", lowered) if unicodedata.category(c) != "Mn")
    return re.sub(r"[^a-z0-9]+", "-", stripped).strip("-")


def minutes_of(text: str) -> float:
    text = re.sub(r"([a-z])([A-Z])", r" ", text)  # "Prep: 10 minutesCooking: 5 minutes" sau khi bỏ wikitext.
    total = sum(float(v) * (60 if u.lower().startswith("h") else 1) for v, u in _DURATION.findall(text))
    days = _DAYS.search(text)
    return total + (int(days[1]) * 1440 if days else 0)


def parse_servings(text: str) -> int | None:
    match = _SERVINGS.match(text) or _YIELD.match(text)
    return (int(match[1]) or None) if match else None


def split_time(total: float, steps: list[str]) -> tuple[int, int, list[int | None]]:
    """Nguồn chỉ ghi tổng thời gian; thời gian nấu được SUY RA từ các mốc trong bước, không phải số nguồn công bố."""
    timers: list[int | None] = []
    for step in steps:
        step = _COMPOUND.sub(lambda m: f"{int(m[1]) * 60 + int(m[2])} minutes", step)
        found = _DURATION.findall(step)
        timers.append(int(minutes_of(step)) if len(found) == 1 else None)
    cook = sum(t for t in timers if t)
    if cook == 0 and any(_HEAT.search(step) for step in steps):
        # Có đun nấu mà nguồn không ghi mốc: ƯỚC LƯỢNG chia đôi, vì để 0 thì trang hiện "Không cần nấu" là sai. Tổng thời gian vẫn đúng nguồn.
        cook = int(total) // 2
    if cook >= total:
        cook = max(int(total) - 1, 0)
    return max(int(total) - cook, 1), cook, timers


def parse_nutrition(code) -> dict | None:
    """Chỉ nhận khi đủ 6 chỉ số và kcal khớp 4×đạm + 4×tinh bột + 9×béo trong ngưỡng cho phép."""
    for template in code.filter_templates():
        if not str(template.name).strip().lower().startswith("nutrition"):
            continue
        values: dict[str, float] = {}
        for param in template.params:
            name = str(param.name).strip()
            key = _NUTRITION_POSITIONAL.get(int(name)) if name.isdigit() else _NUTRITION_NAMED.get(name.lower())
            number = re.search(r"\d+(?:\.\d+)?", str(param.value))
            if key and number:
                values[key] = float(number[0])
        if len(values) != 6 or values["calories"] <= 0:
            return None
        estimate = 4 * values["protein"] + 4 * values["carbohydrates"] + 9 * values["fat"]
        return values if abs(estimate - values["calories"]) / values["calories"] <= NUTRITION_TOLERANCE else None
    return None


def parse_ingredients(body: str, title: str) -> list:
    result, group = [], None
    for line in body.split("\n"):
        heading = re.match(r"^(={3,})\s*(.*?)\s*\1\s*$", line)
        if heading:
            group = wikitext.plain(heading.group(2), title) or None
        elif re.match(r"^\*+\s*\S", line):
            text = wikitext.plain(re.sub(r"^\*+\s*", "", line), title)
            if text:
                result.append(parse_line(text, group))
    return result


def parse_steps(body: str, title: str) -> list[str]:
    steps = [wikitext.plain(re.sub(r"^#+\s*", "", line), title) for line in body.split("\n") if re.match(r"^#+\s*\S", line)]
    return [s for s in steps if s]


def source_hash(title: str, description: str, ingredients: list, steps: list[str]) -> str:
    content = json.dumps([title, description, [i.raw for i in ingredients], steps], ensure_ascii=False)
    return hashlib.sha256(content.encode("utf-8")).hexdigest()


def usable_image(info: dict | None) -> bool:
    if not info or info.get("restrictions") or info.get("mime") not in ALLOWED_MIME:
        return False
    if not ALLOWED_LICENSE.match(info.get("license") or "") or not (info.get("url") or "").startswith(ALLOWED_HOSTS):
        return False
    # URL trả về ảnh gốc khi ảnh hẹp hơn bản thu nhỏ yêu cầu: khi đó kích thước file gốc mới là số tải về thật.
    return "thumbnail_unscaled" not in info["url"] or (info.get("size") or 0) <= MAX_DOWNLOAD_BYTES


def parse_page(page: dict, images: dict) -> tuple[dict | None, str]:
    """Trả về (công thức, "ok") hoặc (None, lý do loại). Thứ tự kiểm tra là thứ tự của phễu trong kế hoạch."""
    revision = page["revisions"][0]
    content = revision["slots"]["main"]["content"]
    title = page["title"].removeprefix("Cookbook:")
    if "/" in title:
        return None, "trang con"
    templates = wikitext.template_names(content)
    if templates & HISTORIC or "incomplete recipe" in templates:
        return None, "sách cổ hoặc chưa hoàn chỉnh"
    lead, sections = wikitext.split_sections(content)
    ingredient_body = next((body for name, body in sections.items() if name.startswith("ingredient")), "")
    step_body = next((sections[name] for name in STEP_SECTIONS if name in sections), "")
    if "{|" in ingredient_body:
        return None, "nguyên liệu dạng bảng"
    ingredients = parse_ingredients(ingredient_body, title)
    steps = parse_steps(step_body, title)
    if len(ingredients) < MIN_INGREDIENTS:
        return None, "dưới 10 nguyên liệu"
    if len(steps) < MIN_STEPS:
        return None, "dưới 5 bước"
    description = wikitext.description(lead, title)
    if len(description) < MIN_DESCRIPTION:
        return None, "không có mô tả"
    box = wikitext.infobox(content)
    servings = parse_servings(wikitext.plain(box.get("servings", ""), title) or wikitext.plain(box.get("yield", ""), title))
    if not servings:
        return None, "khẩu phần không rõ"
    total = minutes_of(wikitext.plain(box.get("time", ""), title))
    if total <= 0:
        return None, "không có thời gian"
    difficulty = wikitext.plain(box.get("difficulty", ""), title)
    if difficulty not in {"1", "2", "3", "4", "5"}:
        return None, "không có độ khó"
    image_name = wikitext.first_image(content, box)
    image_info = images.get(image_name) if image_name else None
    if not usable_image(image_info):
        return None, "không có ảnh giấy phép mở"
    prep, cook, timers = split_time(total, steps)
    cuisine = (re.findall(r"\[\[Cookbook:Cuisine of ([^|\]#]+)", content) or [""])[0]
    page_categories = " ".join(re.findall(r"\[\[Category:([^|\]]+)", content))
    category, rule = taxonomy.categorize(title, wikitext.plain(box.get("category", ""), title), cuisine, page_categories)
    return {
        "pageId": page["pageid"],
        "revisionId": revision["revid"],
        "pageTitle": page["title"],
        "title": title,
        "description": description,
        "servings": servings,
        "prep": prep,
        "cook": cook,
        "difficulty": int(difficulty),
        "ingredients": [i.to_dict() for i in ingredients],
        "steps": steps,
        "timers": timers,
        "nutrition": parse_nutrition(mw.parse(content)),
        "image": {"file": image_name, **image_info},
        "category": category,
        "categoryRule": rule,
        "sourceHash": source_hash(title, description, ingredients, steps),
    }, "ok"


def legacy_slugs(seed_data: Path) -> set[str]:
    slugs: set[str] = set()
    for path in sorted(seed_data.glob("recipes-0*.json")):
        slugs |= {slugify(r["title"]) for r in json.loads(path.read_text(encoding="utf-8"))}
    return slugs


def category_overrides() -> dict[int, str]:
    if not CATEGORY_OVERRIDES.exists():
        return {}
    with CATEGORY_OVERRIDES.open(encoding="utf-8", newline="") as handle:
        return {int(row["pageId"]): row["category"] for row in csv.DictReader(handle)}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--write-pins", action="store_true")
    args = parser.parse_args()
    pages = json.loads((RAW / "pages.json").read_text(encoding="utf-8"))
    images = json.loads((RAW / "images.json").read_text(encoding="utf-8"))
    exclusions = json.loads(EXCLUSIONS.read_text(encoding="utf-8")) if EXCLUSIONS.exists() else {}
    existing = legacy_slugs(SEED_DATA)
    overrides = category_overrides()
    funnel: collections.Counter = collections.Counter()
    selected: list[dict] = []
    for page in pages:
        if not page.get("revisions"):
            continue
        funnel["trang"] += 1
        recipe, reason = parse_page(page, images)
        if recipe is None:
            funnel[reason] += 1
        elif str(recipe["pageId"]) in exclusions:
            funnel["loại khi xem ảnh"] += 1
        elif slugify(recipe["title"]) in existing:
            funnel["trùng món đã có"] += 1
        else:
            recipe["category"] = overrides.get(recipe["pageId"], recipe["category"])
            selected.append(recipe)
    selected.sort(key=lambda r: slugify(r["title"]))
    OUT.mkdir(exist_ok=True)
    (OUT / "recipes_en.json").write_text(json.dumps(selected, ensure_ascii=False, indent=1), encoding="utf-8")
    report = {"funnel": dict(funnel), "selected": len(selected), "categories": dict(collections.Counter(r["category"] for r in selected).most_common())}
    (OUT / "report.json").write_text(json.dumps(report, ensure_ascii=False, indent=1), encoding="utf-8")
    if args.write_pins:
        pins = {str(r["pageId"]): r["revisionId"] for r in sorted(selected, key=lambda r: r["pageId"])}
        PINS.write_text(json.dumps(pins, indent=1) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main()
