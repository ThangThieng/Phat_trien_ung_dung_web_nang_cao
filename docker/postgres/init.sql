-- Chạy một lần khi volume pgdata được tạo lần đầu (SRS §2.4.1: unaccent, pg_trgm bắt buộc).
-- Chỉ tạo extension. Mọi đối tượng schema mà code phụ thuộc (hàm unaccent_immutable, cột SearchVector, GIN index) nằm trong
-- migration EF Core B4_Search_FTS — file này KHÔNG chạy trong Testcontainers hay môi trường mới (retrofit D-18, MT-25).
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
