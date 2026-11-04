"""Đọc giấy phép và URL của ảnh trong raw/pages.json vào raw/images.json. Không tải file ảnh."""
from __future__ import annotations

import json
import re

import wikitext
from mediawiki import RAW, batches, query

WIDTH = 1280  # Cỡ chuẩn của Wikimedia (Common_thumbnail_sizes), đủ cho biến thể 800×600; 12 ảnh gốc đo được vượt 5 MB (CONS-007).
METADATA = "LicenseShortName|Artist|Restrictions"


def clean_html(value: str) -> str:
    return re.sub(r"\s+", " ", re.sub(r"<[^>]+>", "", value)).strip()


def main() -> None:
    pages = json.loads((RAW / "pages.json").read_text(encoding="utf-8"))
    names = set()
    for page in pages:
        if page.get("revisions"):
            content = page["revisions"][0]["slots"]["main"]["content"]
            name = wikitext.first_image(content, wikitext.infobox(content))
            if name:
                names.add(name)
    images: dict[str, dict | None] = {}
    for batch in batches(sorted(names), 40):
        data = query({"action": "query", "prop": "imageinfo", "iiprop": "url|size|mime|extmetadata", "iiurlwidth": WIDTH,
                      "iiextmetadatafilter": METADATA, "titles": "|".join("File:" + n for n in batch)})
        for item in data["query"]["pages"]:
            name = item["title"].removeprefix("File:")
            if "imageinfo" not in item:
                images[name] = None
                continue
            info = item["imageinfo"][0]
            meta = info.get("extmetadata", {})
            images[name] = {
                "license": meta.get("LicenseShortName", {}).get("value", ""),
                "artist": clean_html(meta.get("Artist", {}).get("value", "")),
                "restrictions": meta.get("Restrictions", {}).get("value", ""),
                "mime": info.get("mime"),
                "size": info.get("size"),
                "width": info.get("width"),
                "height": info.get("height"),
                "url": info.get("thumburl") or info.get("url"),
                "descriptionUrl": info.get("descriptionurl"),
            }
        print(f"đã đọc {len(images)}/{len(names)} ảnh", flush=True)
    (RAW / "images.json").write_text(json.dumps(images, ensure_ascii=False, indent=1), encoding="utf-8")
    print("XONG → raw/images.json")


if __name__ == "__main__":
    main()
