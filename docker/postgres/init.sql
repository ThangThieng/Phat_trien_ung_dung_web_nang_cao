-- Chạy một lần khi volume pgdata được tạo lần đầu (SRS §2.4.1: unaccent, pg_trgm bắt buộc)
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Text search config tiếng Việt không dấu cho FR-SRCH-001 ("pho" tìm được "phở")
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_ts_config WHERE cfgname = 'vietnamese_unaccent') THEN
        CREATE TEXT SEARCH CONFIGURATION vietnamese_unaccent (COPY = simple);
        ALTER TEXT SEARCH CONFIGURATION vietnamese_unaccent
            ALTER MAPPING FOR hword, hword_part, word WITH unaccent, simple;
    END IF;
END
$$;
