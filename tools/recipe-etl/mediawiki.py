"""Client MediaWiki API có điều tiết: Wikimedia trả 429 nếu gọi liên tục, kể cả khi chỉ đọc."""
from __future__ import annotations

import json
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")  # Console Windows mặc định cp1252, không in được tiếng Việt.

API = "https://en.wikibooks.org/w/api.php"
USER_AGENT = "CulinaryBlog-RecipeETLBot/1.0 (https://github.com/ThangThieng/Phat_trien_ung_dung_web_nang_cao)"  # Chính sách User-Agent của Wikimedia: có cách liên hệ
ROOT = Path(__file__).resolve().parent
RAW = ROOT / "raw"
OUT = ROOT / "out"
PINS = ROOT / "pins.json"


def query(params: dict, pause: float = 3.0, attempts: int = 8) -> dict:
    """Gọi API dạng formatversion=2; tôn trọng Retry-After và maxlag."""
    url = API + "?" + urllib.parse.urlencode({**params, "format": "json", "formatversion": 2, "maxlag": 5})
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    for attempt in range(attempts):
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                data = json.load(response)
            if data.get("error", {}).get("code") == "maxlag":
                time.sleep(10)
                continue
            time.sleep(pause)
            return data
        except urllib.error.HTTPError as error:
            if error.code not in (429, 503):
                raise
            time.sleep(int(error.headers.get("Retry-After") or 20 * (attempt + 1)))
        except OSError:
            time.sleep(10 * (attempt + 1))
    raise RuntimeError(f"MediaWiki API không phản hồi sau {attempts} lần: {params.get('action')}")


def batches(items: list, size: int):
    for start in range(0, len(items), size):
        yield items[start : start + size]
