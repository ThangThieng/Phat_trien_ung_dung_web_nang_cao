"""Quy tắc xếp danh mục. Kết quả là gợi ý: người duyệt sửa qua ``category_overrides.csv``."""
from __future__ import annotations

import re

# Món thuộc nhóm đang thưa được ưu tiên theo tiêu đề, trước khi xét ẩm thực.
_PRIORITY = [
    ("Hải sản", r"fish|seafood|shrimp|prawn|crab|salmon|tuna|squid|mussel|clam|lobster|\bcod\b|paella|arroz negro|almejas"),
    ("Món hấp", r"steam|dumpling|tamale|momo|dhokla"),
    ("Món nướng", r"\bgrill|barbecue|bbq|roast|kebab|satay|broil|tandoori|tikka"),
    ("Món khai vị", r"appetizer|starter|\bdip\b|hummus|\bsauce\b|chimichurri|salsa|gravy"),
]
_CUISINE = [
    ("Món Ấn Độ", r"india"),
    ("Món Âu", r"italy|france|spain|germany|austria|the united kingdom|greece|ukraine|bulgaria|hungary|poland|portugal|netherlands|sweden|russia"),
    ("Món Nhật Bản", r"japan"),
    ("Món Thái", r"thailand"),
    ("Món Hàn Quốc", r"korea"),
    ("Món châu Á khác", r"china|indonesia|the philippines|malaysia|singapore|taiwan"),
    ("Món châu Phi", r"ethiopia|nigeria|ghana|south africa|morocco|kenya|senegal|tanzania"),
]
_CUISINE_CATEGORY = [
    ("Món châu Phi", r"ethiopian|nigerian|african|moroccan|ghanaian"),
    ("Món Ấn Độ", r"indian recipes"),
    ("Món châu Á khác", r"chinese|indonesian|filipino|malaysian"),
    ("Món Nhật Bản", r"japanese"),
    ("Món Thái", r"thai recipes"),
    ("Món Âu", r"italian|french|spanish|german recipes|british|greek"),
]
_TYPE = [
    ("Mì & Pasta", r"\b(pasta|noodle|spaghetti|lasagn|macaroni|ramen|udon|soba|linguine|fettuccine|ziti)"),
    ("Canh & Súp", r"soup|chowder|broth|bisque|gumbo"),
    ("Món hầm & Cà ri", r"stew|curry|casserole|goulash|braise|chili"),
    ("Gỏi & Salad", r"salad"),
    ("Cơm & Xôi", r"\brice\b|risotto|paella|pilaf|biryani|pulao"),
    ("Tráng miệng & Đồ uống", r"dessert|beverage|drink|confection|ice cream|pudding|custard|candy|smoothie|punch"),
    ("Bánh", r"cake|cookie|pastr|\bpie\b|tart|muffin|brownie|doughnut|donut|bread|biscuit|scone|pancake|waffle|\bbun\b|loaf|pizza|flatbread|crepe"),
    ("Món chính", r"chicken|turkey|duck"),
    ("Món chay", r"vegan|vegetarian|tofu|lentil|\bdal\b|eggplant|cabbage|vegetable recipes|bean recipes"),
    ("Món chiên", r"fried|fritter|tempura|croquette|\bfry\b"),
    ("Ăn vặt", r"snack|sandwich|burger|nachos|chips|breakfast|egg recipes|omelet"),
]
DEFAULT = "Món chính"


def categorize(title: str, category_param: str, cuisine: str, page_categories: str) -> tuple[str, str]:
    """Trả về (danh mục, quy tắc đã khớp) để người duyệt biết vì sao món bị xếp vào đó."""
    title_l = title.lower()
    param = category_param.lower()
    cuisine_l = cuisine.lower()
    for name, pattern in _PRIORITY:
        if re.search(pattern, title_l):
            return name, f"tiêu đề: {pattern[:24]}"
    for name, pattern in _CUISINE:
        if re.search(pattern, cuisine_l):
            return name, f"ẩm thực: {cuisine}"
    for name, pattern in _CUISINE_CATEGORY:
        if re.search(pattern, param):
            return name, f"Category: {category_param}"
    if re.search(r"the united states|tex-mex|american", f"{cuisine_l} {param}"):
        return "Món Mỹ", "ẩm thực Mỹ"
    for scope in (f"{title_l} {param}", page_categories.lower()):
        for name, pattern in _TYPE:
            if re.search(pattern, scope):
                return name, f"loại món: {pattern[:24]}"
    return DEFAULT, "mặc định"
