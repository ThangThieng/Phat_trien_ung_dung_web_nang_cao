"""Tách một dòng nguyên liệu tiếng Anh thành Name / Quantity / QuantityText / Unit / Notes (FR-RCP-009)."""
from __future__ import annotations

import re
from dataclasses import asdict, dataclass

FRACTIONS = {"½": 1 / 2, "¼": 1 / 4, "¾": 3 / 4, "⅓": 1 / 3, "⅔": 2 / 3, "⅛": 1 / 8, "⅜": 3 / 8, "⅝": 5 / 8, "⅞": 7 / 8, "⅕": 1 / 5, "⅙": 1 / 6}
_FC = "".join(FRACTIONS)
_UNITS = (
    r"cups?|c\.|tablespoons?|tbsp\.?|tbs\.?|teaspoons?|tsp\.?|grams?|g|kilograms?|kg|milliliters?|millilitres?|ml|"
    r"liters?|litres?|l|pounds?|lbs?\.?|ounces?|oz\.?|quarts?|qt\.?|pints?|pt\.?|gallons?|cloves?|cans?|pinch(?:es)?|"
    r"dash(?:es)?|sticks?|slices?|bunch(?:es)?|sprigs?|pieces?|heads?|stalks?|packages?|pkg\.?|packets?|jars?|bottles?|"
    r"handfuls?|drops?|leaves|leaf|fl\.? ?oz\.?|fluid ounces?|mg"
)
_NUMBER = rf"(?:\d+\s*[-‐–—]?\s*\d+/\d+|\d+/\d+|\d+(?:[.,]\d+)?(?:\s*[-‐–—]?\s*[{_FC}])?|[{_FC}])"
_LINE = re.compile(
    rf"^(?P<q>{_NUMBER})(?P<range>\s*(?:-|–|—|to)\s*{_NUMBER})?\s*(?P<p1>\([^)]*\)\s*)?"
    rf"(?P<unit>(?:{_UNITS})(?![a-z]))?\s*(?P<p2>\([^)]*\)\s*)?(?:of\s+)?(?P<rest>.*)$",
    re.I,
)
_TASTE = re.compile(r"^(?P<name>.+?)[,\s]+(?P<text>to taste|as needed|for [a-z ]+|optional)\s*$", re.I)

TO_TASTE = "to taste"
AS_NEEDED = "as needed"


@dataclass(frozen=True)
class Ingredient:
    raw: str
    name: str
    quantity: float | None
    text: str | None
    unit: str | None
    notes: str | None
    kind: str  # numeric | range | taste | free

    def to_dict(self) -> dict:
        return asdict(self)


def parse_number(value: str) -> float | None:
    value = value.replace(",", ".").strip()
    if value in FRACTIONS:
        return FRACTIONS[value]
    match = re.fullmatch(r"(\d+(?:\.\d+)?)\s*[-‐–—]?\s*(\d+)/(\d+)", value)
    if match and int(match[3]):
        return float(match[1]) + int(match[2]) / int(match[3])
    match = re.fullmatch(r"(\d+)/(\d+)", value)
    if match:
        return int(match[1]) / int(match[2]) if int(match[2]) else None
    match = re.fullmatch(rf"(\d+(?:\.\d+)?)\s*[-‐–—]?\s*([{_FC}])", value)
    if match:
        return float(match[1]) + FRACTIONS[match[2]]
    try:
        return float(value)
    except ValueError:
        return None


def _join(*parts: str | None) -> str | None:
    return ", ".join(p for p in parts if p) or None


def parse_line(line: str, group: str | None = None) -> Ingredient:
    match = _LINE.match(line)
    if match and parse_number(match["q"]) is not None:
        name, _, note = match["rest"].strip().partition(",")
        extra = (match["p1"] or match["p2"] or "").strip().strip("() ")
        notes = _join(note.strip(), extra, group)
        unit = (match["unit"] or "").strip() or None
        if match["range"]:
            return Ingredient(line, name.strip() or line, None, (match["q"] + match["range"]).strip(), unit, notes, "range")
        return Ingredient(line, name.strip() or line, round(parse_number(match["q"]), 3), None, unit, notes, "numeric")
    taste = _TASTE.match(line)
    if taste:
        return Ingredient(line, taste["name"].strip(), None, taste["text"].strip(), None, group, "taste")
    name, _, note = line.partition(",")
    return Ingredient(line, name.strip(), None, AS_NEEDED, None, _join(note.strip(), group), "free")
