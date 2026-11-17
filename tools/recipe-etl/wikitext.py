"""Làm sạch wikitext của Wikibooks Cookbook: template, liên kết, trích dẫn và tách section."""
from __future__ import annotations

import re

import mwparserfromhell as mw

_REF = re.compile(r"<ref\b[^>]*/>|<ref\b[^>]*>.*?</ref>", re.I | re.S)
_MAGIC = re.compile(r"__[A-Za-z]+__")
_NAV_LINE = re.compile(r"\{\{\s*recipe\s*(\|[^}]*)?\}\}\s*(\|\s*\[\[[^\]]*\]\]\s*)*", re.I)
_SUMMARY = re.compile(r"\{\{\s*recipe ?summary.*?\n\s*\}\}", re.I | re.S)
_HEADING = re.compile(r"^(={2,})\s*(.*?)\s*\1\s*$")


def _render_templates(code, title: str):
    """Giữ lại giá trị của các template mang số liệu; ``strip_code`` sẽ xóa hết nếu không xử lý trước."""
    for template in list(code.filter_templates(recursive=True)):
        name = str(template.name).strip().lower()
        params = template.params
        try:
            if name in ("convert", "cvt") and len(params) >= 2:
                code.replace(template, f"{params[0].value} {params[1].value}")
            elif name in ("fraction", "frac") and len(params) >= 2:
                code.replace(template, f"{params[0].value}/{params[1].value}")
            elif name in ("pagename", "subpagename"):
                code.replace(template, title)
            elif name == "lang" and len(params) >= 2:
                code.replace(template, str(params[1].value))
            elif name == "#invoke:temperature" and len(params) >= 2:
                code.replace(template, f"{params[1].value} °{str(params[0].value).strip().upper()}")
            elif name == "w" and params:
                code.replace(template, str(params[-1].value))
            elif name == "!":
                code.replace(template, "|")
        except ValueError:
            continue  # Template lồng đã bị thay cùng template cha.
    return code


def plain(text: str, title: str = "") -> str:
    """Wikitext → văn bản thuần, một dòng."""
    text = _REF.sub("", text)
    code = mw.parse(text)
    for link in list(code.filter_wikilinks(recursive=False)):
        target = str(link.title).strip().lower()
        if target.startswith(("file:", "image:", "category:")):
            code.remove(link)
    rendered = _render_templates(code, title).strip_code(normalize=True, collapse=True)
    return re.sub(r"\s+", " ", _MAGIC.sub("", str(rendered))).strip()


def split_sections(wikitext: str) -> tuple[str, dict[str, str]]:
    """Tách theo tiêu đề cấp 2. Mục con ``===...===`` nằm trong thân section để không bị đếm thiếu."""
    lead: list[str] = []
    sections: list[tuple[str, list[str]]] = []
    current: list[str] | None = None
    for line in wikitext.split("\n"):
        match = _HEADING.match(line)
        if match and len(match.group(1)) == 2:
            current = []
            sections.append((match.group(2).strip().lower(), current))
            continue
        (lead if current is None else current).append(line)
    result: dict[str, str] = {}
    for name, body in sections:
        result.setdefault(name, "\n".join(body))
    return "\n".join(lead), result


def description(lead: str, title: str) -> str:
    """Đoạn mở đầu trang, không gồm hộp thông tin và dòng điều hướng."""
    cleaned = _SUMMARY.sub("", lead)
    cleaned = _NAV_LINE.sub("", cleaned)
    return plain(cleaned, title).strip(" |")


def infobox(wikitext: str) -> dict[str, str]:
    """Tham số của ``{{Recipe summary}}``, tên viết thường, giá trị là wikitext gốc."""
    for template in mw.parse(wikitext).filter_templates(recursive=False):
        if str(template.name).strip().lower() in ("recipe summary", "recipesummary"):
            return {str(p.name).strip().lower(): str(p.value).strip() for p in template.params}
    return {}


def template_names(wikitext: str) -> set[str]:
    return {str(t.name).strip().lower() for t in mw.parse(wikitext).filter_templates()}


def first_image(wikitext: str, box: dict[str, str]) -> str | None:
    """Ảnh trong hộp thông tin, nếu không có thì ảnh đầu tiên của trang."""
    pattern = r"(?:File|Image):([^|\]]+)"
    source = box.get("image", "")
    match = re.search(pattern, source, re.I)
    if match:
        return match.group(1).strip().replace("_", " ")
    if source and "[[" not in source:
        return source.strip().replace("_", " ")
    match = re.search(r"\[\[" + pattern, wikitext, re.I)
    return match.group(1).strip().replace("_", " ") if match else None
