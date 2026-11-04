"""Chạy: python -m pytest tools/recipe-etl/tests (không gọi mạng)."""
import sys
from pathlib import Path

import mwparserfromhell as mw
import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import build_seed  # noqa: E402
import taxonomy  # noqa: E402
import transform  # noqa: E402
import validate_translations as vt  # noqa: E402
import wikitext  # noqa: E402
from ingredients import parse_line  # noqa: E402


@pytest.mark.parametrize(
    "line, name, quantity, text, unit",
    [
        ("2 cups (480 ml) milk", "milk", 2.0, None, "cups"),
        ("½ cup light corn syrup", "light corn syrup", 0.5, None, "cup"),
        ("1 1/2 tsp salt", "salt", 1.5, None, "tsp"),
        ("1½ tsp salt", "salt", 1.5, None, "tsp"),
        ("1/3 cup oil", "oil", 0.333, None, "cup"),
        ("2 eggs, beaten", "eggs", 2.0, None, None),
        ("2–3 Tbsp miso paste", "miso paste", None, "2–3", "Tbsp"),
        ("Salt and pepper to taste", "Salt and pepper", None, "to taste", None),
        ("Freshly-ground black pepper", "Freshly-ground black pepper", None, "as needed", None),
        ("Vegetable oil for brushing griddle", "Vegetable oil", None, "for brushing griddle", None),
    ],
)
def test_parse_line(line, name, quantity, text, unit):
    parsed = parse_line(line)
    assert (parsed.name, parsed.quantity, parsed.text, parsed.unit) == (name, quantity, text, unit)


def test_parse_line_giu_chu_thich_va_nhom_vao_notes():
    assert parse_line("2 eggs, beaten", "Cake").notes == "beaten, Cake"
    assert parse_line("2 cups (480 ml) milk").notes == "480 ml"


def test_plain_bo_lien_ket_va_giu_gia_tri_template():
    assert wikitext.plain("2 [[Cookbook:Onion|onions]]") == "2 onions"
    assert wikitext.plain("Bake at {{convert|350|F|C}}") == "Bake at 350 F"
    assert wikitext.plain("{{fraction|1|2}} cup") == "1/2 cup"
    assert wikitext.plain("'''{{PAGENAME}}''' is good", "Pho") == "Pho is good"
    assert wikitext.plain("Preheat to {{#invoke:Temperature|f|350}}.") == "Preheat to 350 °F."
    assert wikitext.plain("In {{w|Viet Nam}} today") == "In Viet Nam today"


def test_plain_bo_trich_dan_ref():
    assert wikitext.plain("Cookie dough<ref>The ingredients come from a book.</ref>") == "Cookie dough"
    assert wikitext.plain('Soup<ref name="a" /> base') == "Soup base"


def test_split_sections_giu_muc_con_trong_section_cap_2():
    lead, sections = wikitext.split_sections("Lead\n==Ingredients==\n===Broth===\n* a\n===Soup===\n* b\n==Procedure==\n# c")
    assert lead == "Lead"
    assert "* a" in sections["ingredients"] and "* b" in sections["ingredients"]
    assert sections["procedure"].strip() == "# c"


def test_parse_ingredients_ghi_nhom_vao_notes():
    items = transform.parse_ingredients("===Broth===\n* 1 cup water\n===Soup===\n* 2 tsp salt", "x")
    assert [i.notes for i in items] == ["Broth", "Soup"]


@pytest.mark.parametrize(
    "text, expected",
    [("1 hour", 60), ("45 minutes", 45), ("Prep: 10 minutesCooking: 5 minutes per skillet", 15), ("1 hour 30 minutes", 90), ("2 days", 2880), ("", 0)],
)
def test_minutes_of(text, expected):
    assert transform.minutes_of(text) == expected


@pytest.mark.parametrize("text, expected", [("4", 4), ("4–6", 4), ("8 ea. 4-inch pancakes", 8), ("9 pcs.", 9), ("2 cups", 2), ("One 9-inch cake", None), ("", None)])
def test_parse_servings(text, expected):
    assert transform.parse_servings(text) == expected


def test_split_time_cook_khong_vuot_tong_va_prep_luon_duong():
    assert transform.split_time(60, ["Boil for 20 minutes", "Serve"]) == (40, 20, [20, None])
    assert transform.split_time(10, ["Bake for 2 hours"]) == (1, 9, [120])
    assert transform.split_time(30, ["Boil 5 minutes then rest 10 minutes"]) == (15, 15, [None])


def test_split_time_moc_ghep_la_mot_moc():
    assert transform.split_time(90, ["Bake 1 hour and 15 minutes or until done"]) == (15, 75, [75])


def test_split_time_chi_de_0_khi_khong_dun_nau():
    assert transform.split_time(20, ["Slice the oranges", "Toss with the dressing"]) == (20, 0, [None, None])
    assert transform.split_time(60, ["Fry the pickles until golden"]) == (30, 30, [None])


def test_parse_nutrition_dang_ten_va_vi_tri():
    named = "{{Nutrition Summary| |Cals=487|TotalFat=22.8 g|Sodium=825 mg|Carbs=44.2 g|Fiber=4.9 g|Protein=26.3 g}}"
    assert transform.parse_nutrition(mw.parse(named))["calories"] == 487
    positional = "{{nutritionsummary|1 cup (250 ml)|4|26|3|0.3 g|0.2 g|1 mg|6 mg|4.8 g|0 g|4.7 g|0.4 g|0%|0%|1%|0%}}"
    assert transform.parse_nutrition(mw.parse(positional)) == {"calories": 26.0, "fat": 0.3, "sodium": 6.0, "carbohydrates": 4.8, "fiber": 0.0, "protein": 0.4}


def test_parse_nutrition_tu_choi_so_khong_hop_ly_va_thieu_chi_so():
    implausible = "{{Nutrition Summary| |Cals=148|TotalFat=1.2 g|Sodium=45 mg|Carbs=0.2 g|Fiber=0.1 g|Protein=0.4 g}}"
    assert transform.parse_nutrition(mw.parse(implausible)) is None
    assert transform.parse_nutrition(mw.parse("{{Nutrition Summary| |Cals=100}}")) is None
    assert transform.parse_nutrition(mw.parse("Không có template")) is None


def image(**overrides):
    return {"license": "CC BY-SA 4.0", "restrictions": "", "mime": "image/jpeg", "url": "https://upload.wikimedia.org/a.jpg", "size": 1000, **overrides}


def test_usable_image():
    assert transform.usable_image(image())
    assert transform.usable_image(image(license="Public domain"))
    assert transform.usable_image(image(url="https://thumb.wikimedia.org/a.jpg"))
    assert not transform.usable_image(None)
    assert not transform.usable_image(image(license="Attribution"))
    assert not transform.usable_image(image(mime="image/svg+xml"))
    assert not transform.usable_image(image(restrictions="trademarked"))
    assert not transform.usable_image(image(url="https://example.com/a.jpg"))
    assert not transform.usable_image(image(url="https://upload.wikimedia.org/a.jpg?x=thumbnail_unscaled", size=6 * 1024 * 1024))


def test_slugify_giong_slug_helper():
    assert transform.slugify("Phở Bò (Vietnamese Beef)") == "pho-bo-vietnamese-beef"
    assert transform.slugify("Đậu hũ") == "dau-hu"


@pytest.mark.parametrize(
    "title, category, cuisine, pages, expected",
    [
        ("Paella Valenciana", "Rice recipes", "Spain", "", "Hải sản"),
        ("Chicken Tikka", "Chicken recipes", "India", "", "Món nướng"),
        ("Black Forest Cake", "Cake recipes", "Germany", "", "Món Âu"),
        ("Apple Pie", "Pie recipes", "the United States", "", "Món Mỹ"),
        ("Tomato Soup", "Soup recipes", "", "", "Canh & Súp"),
        ("Spaghetti", "Pasta recipes", "", "", "Mì & Pasta"),
        ("Unknown", "", "", "", taxonomy.DEFAULT),
    ],
)
def test_categorize(title, category, cuisine, pages, expected):
    assert taxonomy.categorize(title, category, cuisine, pages)[0] == expected


def en_recipe():
    return {
        "pageId": 1, "title": "Soup", "description": "A soup of 2 things.", "sourceHash": "h",
        "ingredients": [{"raw": "1 cup water", "name": "water", "quantity": 1.0, "text": None, "unit": "cup", "notes": None, "kind": "numeric"}],
        "steps": ["Boil for 10 minutes at 350 F."],
    }


def vi_recipe():
    return {
        "pageId": 1, "sourceHash": "h", "title": "Súp đơn giản", "description": "Món súp gồm 2 thành phần, nấu rất nhanh.",
        "ingredients": [{"n": "nước"}], "steps": [{"t": "Đun sôi", "d": "Đun sôi 10 phút ở 350 °F (175 °C)."}],
    }


def test_validate_recipe_hop_le():
    assert vt.validate_recipe(en_recipe(), vi_recipe()) == []


@pytest.mark.parametrize(
    "change, message",
    [
        (lambda v: v.update(sourceHash="x"), "sourceHash"),
        (lambda v: v["steps"].clear(), "bước"),
        (lambda v: v["ingredients"].append({"n": "muối"}), "nguyên liệu"),
        (lambda v: v["steps"][0].update(d="Đun sôi 5 phút ở 350 °F."), "mất số"),
        (lambda v: v["steps"][0].update(d="Boil with water 10 phút 350"), "tiếng Anh"),
        (lambda v: v["steps"][0].update(d="Boil 10 350"), "không có chữ tiếng Việt"),
        (lambda v: v.update(title="Soup"), "tiêu đề"),
        (lambda v: v.update(description="Súp ngắn 2."), "mô tả"),
    ],
)
def test_validate_recipe_bat_loi(change, message):
    vi = vi_recipe()
    change(vi)
    assert any(message in error for error in vt.validate_recipe(en_recipe(), vi))


def test_validate_bat_ten_nguyen_lieu_trung():
    en = en_recipe()
    en["ingredients"] = en["ingredients"] * 2
    vi = vi_recipe()
    vi["ingredients"] = [{"n": "Nước"}, {"n": "nước"}]
    assert any("trùng tên" in error for error in vt.validate_recipe(en, vi))


def test_numbers_chuan_hoa_phan_so():
    assert vt.numbers("½ cốc") == vt.numbers("1/2 cốc")
    assert "1.5" in vt.numbers("1,5 lít")


@pytest.mark.parametrize("unit, expected", [("cups", "cốc"), ("Tbsp", "thìa canh"), ("tsp.", "thìa cà phê"), ("g", "gram"), ("cloves", "tép"), ("pinches", "nhúm"), ("lbs", "pound"), ("fl oz", "fl oz"), (None, None)])
def test_unit_vi(unit, expected):
    assert build_seed.unit_vi(unit) == expected


def test_merge_ingredient():
    numeric = {"quantity": 2.0, "text": None, "unit": "cups", "kind": "numeric"}
    assert build_seed.merge_ingredient(numeric, {"n": "sữa", "o": "480 ml"}) == ["sữa", 2.0, "cốc", "480 ml"]
    assert build_seed.merge_ingredient({"quantity": None, "text": "2 to 3", "unit": "Tbsp", "kind": "range"}, {"n": "miso"}) == ["miso", "2–3", "thìa canh"]
    assert build_seed.merge_ingredient({"quantity": None, "text": "to taste", "unit": None, "kind": "taste"}, {"n": "muối"}) == ["muối", "vừa ăn", None]
    assert build_seed.merge_ingredient({"quantity": None, "text": "as needed", "unit": None, "kind": "free"}, {"n": "tiêu"}) == ["tiêu", "vừa đủ", None]
    assert build_seed.merge_ingredient({"quantity": 1.0, "text": None, "unit": None, "kind": "numeric"}, {"n": "tỏi", "q": 2, "u": "tép"}) == ["tỏi", 2, "tép"]


def test_attribution_du_yeu_cau_giay_phep():
    recipe = {"pageId": 7, "revisionId": 9, "image": {"artist": "An", "license": "CC0", "descriptionUrl": "https://c/f"}}
    text = build_seed.attribution(recipe, "Cookbook:Soup")
    assert text.startswith(build_seed.SOURCE_PREFIX)
    for part in ("curid=7&oldid=9", "CC BY-SA 4.0", build_seed.LICENSE_URL, "có hỗ trợ AI", "Ảnh: An, CC0, https://c/f"):
        assert part in text
