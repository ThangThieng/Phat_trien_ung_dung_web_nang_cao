"""Sinh Seed/Data/legacy-recipe-images.json từ legacy_image_picks.json (ảnh đã xem bằng mắt, đúng món).

Ảnh chỉ gần đúng món ghi lý do trong legacy_image_notes.json; không có ảnh gần đúng thì công thức giữ không ảnh, không dùng ảnh gần đúng.
"""
from __future__ import annotations

import json
import re

from fetch_images import METADATA, WIDTH, clean_html
from mediawiki import ROOT, batches, query
from transform import SEED_DATA, slugify, usable_image

PICKS = ROOT / "legacy_image_picks.json"


def main() -> None:
    picks: dict[str, str] = json.loads(PICKS.read_text(encoding="utf-8"))
    titles: list[str] = []
    for path in sorted(SEED_DATA.glob("recipes-0*.json")):
        titles += [r["title"] for r in json.loads(path.read_text(encoding="utf-8"))]
    unknown = sorted(set(picks) - set(titles))
    if unknown:
        raise SystemExit(f"Không có công thức cũ tên: {unknown}")
    info: dict[str, dict] = {}
    for batch in batches(sorted(set(picks.values())), 40):
        data = query({"action": "query", "prop": "imageinfo", "iiprop": "url|size|mime|extmetadata", "iiurlwidth": WIDTH,
                      "iiextmetadatafilter": METADATA, "titles": "|".join("File:" + n for n in batch)})
        for item in data["query"]["pages"]:
            if "imageinfo" not in item:
                continue
            raw = item["imageinfo"][0]
            meta = raw.get("extmetadata", {})
            info[item["title"].removeprefix("File:")] = {
                "license": meta.get("LicenseShortName", {}).get("value", ""),
                "artist": clean_html(meta.get("Artist", {}).get("value", "")),
                "restrictions": meta.get("Restrictions", {}).get("value", ""),
                "mime": raw.get("mime"),
                "size": raw.get("size"),
                "url": raw.get("thumburl") or raw.get("url"),
                "descriptionUrl": raw.get("descriptionurl"),
            }
    rows, rejected = [], []
    for title in sorted(picks, key=slugify):
        name = re.sub(r"^File:", "", picks[title])
        image = info.get(name)
        if not usable_image(image):
            rejected.append(f"{title}: {name}")
            continue
        rows.append({"slug": slugify(title), "file": name, "url": image["url"], "artist": image["artist"], "license": image["license"], "descriptionUrl": image["descriptionUrl"]})
    # Ảnh Flickr tải qua API của Openverse (host ảnh Flickr chặn client tự động), giấy phép đã kiểm tra khi chọn; có thể là phi thương mại (CC BY-NC).
    rows += json.loads((ROOT / "legacy_image_external.json").read_text(encoding="utf-8"))
    rows.sort(key=lambda row: row["slug"])
    (SEED_DATA / "legacy-recipe-images.json").write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"XONG: {len(rows)} ảnh; bị loại {len(rejected)}: {rejected}")


if __name__ == "__main__":
    main()
