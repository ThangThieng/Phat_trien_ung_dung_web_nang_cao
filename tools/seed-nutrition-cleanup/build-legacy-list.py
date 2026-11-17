"""Sinh legacy-recipes.sql từ 100 công thức do nhóm biên soạn. Chạy lại khi các file recipes-0*.json đổi.

Dấu vân tay md5(Title|Description) để chỉ đổi công thức còn đúng bản gốc: bản đã có người sửa thì không động tới (SRS §2.6.1).
"""
from __future__ import annotations

import hashlib
import json
import re
import unicodedata
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SEED_DATA = ROOT.parents[1] / "backend/src/CulinaryBlog.Infrastructure/Persistence/Seed/Data"


def slugify(text: str) -> str:
    """Giống ``SlugHelper.Generate`` (Domain/Common)."""
    lowered = text.lower().replace("đ", "d")
    stripped = "".join(c for c in unicodedata.normalize("NFD", lowered) if unicodedata.category(c) != "Mn")
    return re.sub(r"[^a-z0-9]+", "-", stripped).strip("-")


def main() -> None:
    rows = []
    for path in sorted(SEED_DATA.glob("recipes-0*.json")):
        for recipe in json.loads(path.read_text(encoding="utf-8")):
            fingerprint = hashlib.md5(f"{recipe['title']}|{recipe['description']}".encode("utf-8")).hexdigest()  # noqa: S324 - so khớp, không phải bảo mật
            rows.append(f"    ('{slugify(recipe['title'])}', '{fingerprint}')")
    body = ",\n".join(rows)
    sql = f"""-- SINH TỰ ĐỘNG bởi build-legacy-list.py, không sửa tay.
-- Bảng tạm: công thức mẫu còn nguyên bản và còn dinh dưỡng, là đối tượng duy nhất của clear-seed-nutrition.sql.
CREATE TEMP TABLE legacy_recipes (slug text PRIMARY KEY, fingerprint text NOT NULL);
INSERT INTO legacy_recipes (slug, fingerprint) VALUES
{body};

CREATE TEMP TABLE seed_nutrition_targets AS
SELECT r.*
FROM "Recipes" r
JOIN "AspNetUsers" u ON u."Id" = r."AuthorId"
JOIN legacy_recipes l ON l.slug = r."Slug"
WHERE NOT r."IsDeleted"
  AND r."Nutrition_Calories" IS NOT NULL
  AND u."Email" ~ '^author[1-5]@culinaryblog\\.local$'
  AND md5(r."Title" || '|' || r."Description") = l.fingerprint;
"""
    (ROOT / "legacy-recipes.sql").write_text(sql, encoding="utf-8", newline="\n")
    print(f"XONG: {len(rows)} công thức → legacy-recipes.sql")


if __name__ == "__main__":
    main()
