# TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM (SRS)

**Software Requirements Specification — Tiêu chuẩn IEEE 830 / ISO/IEC/IEEE 29148:2018**

> Giáo trình Phát triển Ứng dụng Web Nâng cao — Phiên bản V4 · .NET 10 + Next.js App Router

**Dự án: Blog Ẩm thực và Nấu ăn (Culinary Blog)**

| Thuộc tính | Giá trị |
| --- | --- |
| Phiên bản tài liệu | 1.2.1 |
| Ngày phát hành | 21/09/2026 |
| Trạng thái | Đã duyệt (Approved) |
| Công nghệ Backend | .NET 10 Minimal APIs, C# |
| Công nghệ Frontend | Next.js App Router, TypeScript |
| Cơ sở dữ liệu | PostgreSQL 16 |
| Object Storage | MinIO (S3-Compatible) |
| Cache | Redis 7 |

*Tài liệu này được biên soạn theo tiêu chuẩn IEEE 830 / ISO/IEC/IEEE 29148:2018.*

---

## LỊCH SỬ THAY ĐỔI TÀI LIỆU

| Phiên bản | Ngày | Tác giả / Vai trò | Nội dung thay đổi | Trạng thái |
| --- | --- | --- | --- | --- |
| 1.2.1 | 21/09/2026 | Principal Systems Architect | Change Request **CR-2026-03** (MT-58): cập nhật §2.6.1 theo yêu cầu của giảng viên cho Buổi 2 — dữ liệu mẫu có **ít nhất 20 danh mục, 100 công thức; mỗi công thức ít nhất 10 nguyên liệu và 5 bước chế biến** (trước đây: 50 công thức, 5 tác giả). Không thay đổi FR, endpoint, mã lỗi hay schema. | Đã duyệt (Approved) |
| 1.2.0 | 19/09/2026 | Principal Systems Architect | Change Request **CR-2026-02**: đối chiếu SRS v1.1.0 với code Buổi 2 đang chạy trên `main`, xử lý 16 mâu thuẫn mới (MT-42 → MT-57) — sửa 3 lỗi kỹ thuật trong chính v1.1.0 (cấu hình Nginx gây 502, lệnh healthcheck không chạy được, mã lỗi Admin tự khóa), chuẩn hóa hình dạng response phân trang, chính thức hóa endpoint upload tệp, bổ sung `FR-AUTH-009` Quản lý phiên và 7 endpoint, chuẩn hóa topology Hangfire worker, làm rõ cơ chế khôi phục dữ liệu và nơi lưu token phía Frontend. | Đã duyệt (Approved) |
| 1.1.0 | 17/09/2026 | Principal Systems Architect | Cập nhật chuẩn hóa toàn bộ tài liệu v1.1.0: Giải quyết triệt để 41 mâu thuẫn kiến trúc, tối ưu Caching, Data Model, API Specs và Security theo đề xuất CR-2026. | Đã duyệt (Approved) |
| 1.0.0 | 04/06/2026 | Senior BA / Architect | Phát hành lần đầu – Bản hoàn chỉnh theo IEEE 830 / ISO 29148. | Approved |
| 0.9.0 | 20/05/2026 | Senior BA | Bổ sung Chương 7 (Data Model), Chương 8 (API Spec) và Phụ lục. | Under Review |
| 0.8.0 | 05/05/2026 | Senior BA | Hoàn thiện Chương 3 (FR), bổ sung FR-FILE, FR-JOB, FR-OBS. | Draft |
| 0.5.0 | 15/04/2026 | Senior BA | Phác thảo ban đầu: Chương 1–4 (skeleton). | Draft |

**Phê duyệt tài liệu:** Tài liệu phiên bản 1.2.1 đã được xem xét và phê duyệt bởi Kiến trúc sư Hệ thống Trưởng (Principal Systems Architect) thông qua Change Request **CR-2026-03**, kế thừa toàn bộ quyết định của **CR-2026-02** (v1.2.0) và **CR-2026** (v1.1.0). Mọi thay đổi từ phiên bản 1.2.0 trở đi đều phải thông qua quy trình Change Request (CR) và được cập nhật vào bảng này. Bản 1.1.0 và 1.2.0 không còn lưu thành tệp riêng: toàn bộ nội dung của chúng đã nằm trong bản này; mọi chỗ thay đổi ở v1.2.0 được đánh dấu `[CR-2026-02 / MT-xx]` (Phụ lục E), ở v1.2.1 được đánh dấu `[CR-2026-03 / MT-58]` (Phụ lục F).

**Phạm vi CR-2026 (v1.1.0):** xử lý toàn bộ 41 điểm mâu thuẫn/bất nhất (MT-01 → MT-41) được ghi nhận trong `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`. Mọi quyết định đều được chọn theo tiêu chí **kiến trúc dài hạn**: toàn vẹn dữ liệu (DDD/ràng buộc DB), cách ly bảo mật cache, giảm gánh nặng cho client, và chuẩn hóa RESTful API. Bảng truy vết đầy đủ 41 quyết định nằm tại **Phụ lục D**.

**Phạm vi CR-2026-02 (v1.2.0):** lượt rà soát thứ tư, lần đầu tiên **đối chiếu SRS với code đã chạy thật** (nhánh `main`, commit `f313e95`, và `SPEC/BAO_CAO_BUOI_2.md`) thay vì chỉ đối chiếu các chương với nhau. Lượt này phát hiện 16 điểm (MT-42 → MT-57) thuộc ba loại: **(a) lỗi kỹ thuật trong chính v1.1.0** — những đặc tả nếu làm đúng từng chữ sẽ gây lỗi khi chạy (ví dụ cấu hình Nginx tái tạo lỗi 502 mà Buổi 2 đã sửa); **(b) yêu cầu mồ côi còn sót** — chức năng được hứa ở một chỗ nhưng không có API/FR nào đỡ; **(c) mâu thuẫn giữa SRS và thiết kế đã được kiểm chứng** mà thiết kế trong code tốt hơn về dài hạn. Mọi thay đổi được đánh dấu `[CR-2026-02 / MT-xx]` tại chỗ; bảng truy vết nằm tại **Phụ lục E**.

**Phạm vi CR-2026-03 (v1.2.1):** giảng viên đánh số lại lộ trình thành 8 buổi (Buổi 1 là buổi đọc đặc tả) và giao cho Buổi 2 yêu cầu về dữ liệu mẫu, khác với §2.6.1 của v1.2.0. Yêu cầu được đưa vào SRS trước khi hiện thực để mọi con số trong database truy vết được về đặc tả (MT-58). Chỉ §2.6.1 thay đổi; bảng truy vết nằm tại **Phụ lục F**.

---

## MỤC LỤC

- [Chương 1. Giới thiệu](#chương-1-giới-thiệu)
  - [1.1. Mục đích Tài liệu](#11-mục-đích-tài-liệu)
  - [1.2. Phạm vi Sản phẩm](#12-phạm-vi-sản-phẩm)
  - [1.3. Định nghĩa, Từ viết tắt và Ký hiệu](#13-định-nghĩa-từ-viết-tắt-và-ký-hiệu)
  - [1.4. Tài liệu Tham chiếu](#14-tài-liệu-tham-chiếu)
  - [1.5. Tổng quan Tài liệu](#15-tổng-quan-tài-liệu)
- [Chương 2. Mô tả Tổng quan Hệ thống](#chương-2-mô-tả-tổng-quan-hệ-thống)
  - [2.1. Bối cảnh Sản phẩm](#21-bối-cảnh-sản-phẩm)
  - [2.2. Chức năng Sản phẩm Tổng quát](#22-chức-năng-sản-phẩm-tổng-quát)
  - [2.3. Các Lớp Người dùng và Đặc điểm](#23-các-lớp-người-dùng-và-đặc-điểm)
  - [2.4. Môi trường Vận hành](#24-môi-trường-vận-hành)
  - [2.5. Ràng buộc Thiết kế và Hiện thực](#25-ràng-buộc-thiết-kế-và-hiện-thực)
  - [2.6. Giả định và Phụ thuộc](#26-giả-định-và-phụ-thuộc)
- [Chương 3. Yêu cầu Chức năng Chi tiết](#chương-3-yêu-cầu-chức-năng-chi-tiết)
  - [3.1. Module Xác thực và Quản lý Người dùng (FR-AUTH)](#31-module-xác-thực-và-quản-lý-người-dùng-fr-auth)
  - [3.2. Module Quản lý Danh mục (FR-CAT)](#32-module-quản-lý-danh-mục-fr-cat)
  - [3.3. Module Quản lý Công thức Nấu ăn (FR-RCP)](#33-module-quản-lý-công-thức-nấu-ăn-fr-rcp)
  - [3.4. Module Tìm kiếm và Phân trang (FR-SRCH)](#34-module-tìm-kiếm-và-phân-trang-fr-srch)
  - [3.5. Module Quản lý Tệp tin (FR-FILE)](#35-module-quản-lý-tệp-tin-fr-file)
  - [3.6. Module Background Jobs (FR-JOB)](#36-module-background-jobs-fr-job)
  - [3.7. Module Quan sát Hệ thống (FR-OBS)](#37-module-quan-sát-hệ-thống-fr-obs)
- [Chương 4. Yêu cầu Phi Chức năng (NFR)](#chương-4-yêu-cầu-phi-chức-năng-nfr)
- [Chương 5. Yêu cầu Giao diện Ngoài](#chương-5-yêu-cầu-giao-diện-ngoài)
- [Chương 6. Kiến trúc Hệ thống](#chương-6-kiến-trúc-hệ-thống)
- [Chương 7. Mô hình Dữ liệu](#chương-7-mô-hình-dữ-liệu)
- [Chương 8. Đặc tả REST API](#chương-8-đặc-tả-rest-api)
- [Phụ lục A – HTTP Status Codes](#phụ-lục-a--http-status-codes)
- [Phụ lục B – Application Error Codes](#phụ-lục-b--application-error-codes)
- [Phụ lục C – Từ điển Thuật ngữ](#phụ-lục-c--từ-điển-thuật-ngữ)
- [Phụ lục D – Bảng Truy vết Quyết định Kiến trúc CR-2026](#phụ-lục-d--bảng-truy-vết-quyết-định-kiến-trúc-cr-2026)
- [Phụ lục E – Bảng Truy vết Quyết định CR-2026-02](#phụ-lục-e--bảng-truy-vết-quyết-định-cr-2026-02)
- [Phụ lục F – Bảng Truy vết Quyết định CR-2026-03](#phụ-lục-f--bảng-truy-vết-quyết-định-cr-2026-03)

---

## CHƯƠNG 1. GIỚI THIỆU

### 1.1. Mục đích Tài liệu

Tài liệu Đặc tả Yêu cầu Phần mềm (Software Requirements Specification – SRS) này được biên soạn theo tiêu chuẩn IEEE 830-1998 và ISO/IEC/IEEE 29148:2018 nhằm mô tả đầy đủ, chính xác và nhất quán toàn bộ yêu cầu chức năng (Functional Requirements) và yêu cầu phi chức năng (Non-Functional Requirements) của dự án ứng dụng web **Blog Ẩm thực và Nấu ăn (Culinary Blog)**.

Tài liệu này phục vụ các đối tượng sau:

- **Nhóm phát triển Backend (.NET 10/C#):** Căn cứ thiết kế API, domain model, và business rules.
- **Nhóm phát triển Frontend (Next.js/TypeScript):** Căn cứ thiết kế giao diện, luồng người dùng và tích hợp API.
- **Kỹ sư Kiểm thử (QA/QC):** Cơ sở xây dựng test cases, kiểm thử chấp nhận (acceptance testing).
- **Kiến trúc sư Hệ thống:** Tham chiếu khi đưa ra quyết định kiến trúc (architecture decisions).
- **Giảng viên và Sinh viên:** Tài liệu học thuật mẫu cho dự án thực hành xuyên suốt giáo trình.
- **Stakeholder / Product Owner:** Phê duyệt phạm vi và ưu tiên tính năng.

**Phạm vi hiệu lực:** Tài liệu này có hiệu lực từ phiên bản 1.0.0 và là tài liệu nền tảng (baseline) cho toàn bộ vòng đời phát triển dự án. Mọi thay đổi yêu cầu sau khi tài liệu được phê duyệt phải tuân theo quy trình quản lý thay đổi (Change Management Process).

### 1.2. Phạm vi Sản phẩm

#### 1.2.1. Tên và Định danh

| Thuộc tính | Giá trị |
| --- | --- |
| Tên sản phẩm | Culinary Blog – Blog Ẩm thực và Nấu ăn |
| Định danh dự án | CULINARY-BLOG-V1 |
| Loại hệ thống | Ứng dụng Web Full-Stack (API-Driven Architecture) |
| Phiên bản sản phẩm | 1.0.0 |
| Môi trường đích | Cloud/On-premise (Docker Compose + Nginx) |

#### 1.2.2. Mô tả Sản phẩm

Culinary Blog là một nền tảng web cho phép người dùng chia sẻ, khám phá và lưu trữ các công thức nấu ăn từ nhiều nền ẩm thực khác nhau. Ứng dụng cung cấp hệ sinh thái hoàn chỉnh bao gồm:

- **Nền tảng chia sẻ công thức:** Tác giả (Author) đăng tải công thức với hình ảnh, danh sách nguyên liệu chi tiết, hướng dẫn từng bước thực hiện và thông tin dinh dưỡng.
- **Tổ chức nội dung:** Phân loại công thức theo danh mục (Category), độ khó (Difficulty Level), thời gian chuẩn bị và nấu.
- **Tìm kiếm thông minh:** Full-Text Search tiếng Việt sử dụng PostgreSQL `tsvector`/`tsquery` với `unaccent` extension.
- **Bảo mật đa lớp:** Xác thực JWT stateless, phân quyền theo vai trò (RBAC) và theo tài nguyên (Resource-Based Authorization), đăng nhập Google OAuth 2.0.
- **Tối ưu hiệu năng và SEO:** Redis distributed cache, Next.js ISR, Open Graph Protocol, JSON-LD Schema.org Recipe markup.
- **Quan sát hệ thống:** Structured logging (Serilog), distributed tracing (OpenTelemetry), health check endpoints.

#### 1.2.3. Những gì KHÔNG thuộc phạm vi

Các tính năng sau đây nằm ngoài phạm vi phiên bản 1.0.0:

- Hệ thống bình luận (Comment System) và đánh giá sao (Rating System).
- Tính năng lưu/đánh dấu công thức yêu thích (Bookmark/Favorite).
- Thông báo real-time (SignalR/WebSocket).
- Ứng dụng di động native (iOS/Android).
- Thanh toán / Tính năng thương mại điện tử.
- Hệ thống nhắn tin trực tiếp giữa người dùng.
- GraphQL API (định hướng sau khóa học).
- **Khôi phục công thức đã xóa qua giao diện/API** (thùng rác cho người dùng). Dữ liệu soft-deleted vẫn được **giữ 30 ngày** và có thể được quản trị viên vận hành khôi phục trực tiếp trên cơ sở dữ liệu; chức năng tự phục vụ thuộc phiên bản sau. *(CR-2026-02 / MT-49)*

### 1.3. Định nghĩa, Từ viết tắt và Ký hiệu

| Thuật ngữ / Viết tắt | Định nghĩa đầy đủ |
| --- | --- |
| SRS | Software Requirements Specification – Đặc tả Yêu cầu Phần mềm. |
| FR | Functional Requirement – Yêu cầu chức năng. |
| NFR | Non-Functional Requirement – Yêu cầu phi chức năng. |
| API | Application Programming Interface – Giao diện lập trình ứng dụng. |
| REST | Representational State Transfer – Kiểu kiến trúc API phổ biến nhất. |
| JWT | JSON Web Token – Chuẩn token xác thực stateless (RFC 7519). |
| RBAC | Role-Based Access Control – Kiểm soát truy cập dựa trên vai trò. |
| CQRS | Command Query Responsibility Segregation – Pattern tách biệt lệnh và truy vấn. |
| DDD | Domain-Driven Design – Phương pháp thiết kế phần mềm lấy domain làm trung tâm. |
| ORM | Object-Relational Mapper – Công cụ ánh xạ object-database (EF Core). |
| FTS | Full-Text Search – Tìm kiếm toàn văn bản. |
| ISR | Incremental Static Regeneration – Kỹ thuật tái tạo trang tĩnh của Next.js. |
| LCP | Largest Contentful Paint – Core Web Vital đo tốc độ tải nội dung lớn nhất. |
| CLS | Cumulative Layout Shift – Core Web Vital đo độ ổn định bố cục trang. |
| INP | Interaction to Next Paint – Core Web Vital đo thời gian phản hồi tương tác. |
| CI/CD | Continuous Integration / Continuous Delivery – Tích hợp và triển khai liên tục. |
| TTL | Time-To-Live – Thời gian sống của dữ liệu trong cache. |
| SSR | Server-Side Rendering – Render HTML trên server. |
| SSG | Static Site Generation – Tạo trang tĩnh lúc build time. *(Không sử dụng trong v1.1.0 — hệ thống dùng ISR/SSR/CSR, xem mục 5.1.)* |
| CSR | Client-Side Rendering – Render phía trình duyệt. |
| Owned Entity | Thực thể phụ thuộc trong DDD, không có định danh/vòng đời độc lập; EF Core ánh xạ các cột của nó vào chính bảng của chủ sở hữu (ví dụ `RecipeNutrition`). |
| MoSCoW | Must Have / Should Have / Could Have / Won't Have – Mô hình phân loại ưu tiên. |
| RFC | Request For Comments – Tài liệu tiêu chuẩn kỹ thuật (e.g., RFC 7807). |
| ERD | Entity Relationship Diagram – Sơ đồ quan hệ thực thể. |
| PBKDF2 | Password-Based Key Derivation Function 2 – Thuật toán hash mật khẩu an toàn. |
| CDN | Content Delivery Network – Mạng phân phối nội dung. |
| MIME | Multipurpose Internet Mail Extensions – Chuẩn định dạng tệp trên Internet. |
| JSON-LD | JavaScript Object Notation for Linked Data – Định dạng dữ liệu có cấu trúc cho SEO. |

### 1.4. Tài liệu Tham chiếu

| STT | Tài liệu / Tiêu chuẩn | Nguồn / URL |
| --- | --- | --- |
| 1 | IEEE Std 830-1998 – Recommended Practice for Software Requirements Specifications | https://ieeexplore.ieee.org/document/720574 |
| 2 | ISO/IEC/IEEE 29148:2018 – Requirements Engineering | https://www.iso.org/standard/72089.html |
| 3 | OWASP Top 10:2021 – Top 10 Web Application Security Risks | https://owasp.org/www-project-top-ten/ |
| 4 | RFC 7807 – Problem Details for HTTP APIs | https://datatracker.ietf.org/doc/html/rfc7807 |
| 5 | RFC 7519 – JSON Web Token (JWT) | https://datatracker.ietf.org/doc/html/rfc7519 |
| 6 | RFC 6749 – The OAuth 2.0 Authorization Framework | https://datatracker.ietf.org/doc/html/rfc6749 |
| 7 | .NET 10 Minimal APIs – Microsoft Learn | https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis |
| 8 | ASP.NET Core Identity – Microsoft Learn | https://learn.microsoft.com/aspnet/core/security/authentication/identity |
| 9 | Entity Framework Core 10 Documentation | https://learn.microsoft.com/ef/core/ |
| 10 | Next.js 15 App Router Documentation | https://nextjs.org/docs |
| 11 | PostgreSQL 16 Documentation – Full-Text Search | https://www.postgresql.org/docs/16/textsearch.html |
| 12 | Redis 7 Documentation | https://redis.io/docs/ |
| 13 | MinIO S3-Compatible Object Storage | https://min.io/docs/ |
| 14 | Google Web Vitals – Core Web Vitals | https://web.dev/explore/learn-core-web-vitals |
| 15 | Schema.org Recipe – Structured Data | https://schema.org/Recipe |
| 16 | OpenTelemetry .NET Documentation | https://opentelemetry.io/docs/languages/dotnet/ |
| 17 | Serilog Documentation | https://serilog.net/ |
| 18 | Hangfire Documentation | https://docs.hangfire.io/ |
| 19 | FluentValidation Documentation | https://docs.fluentvalidation.net/ |
| 20 | Giáo trình Phát triển Ứng dụng Web Nâng cao V4 – Nội bộ | N/A (tài liệu nội bộ) |

### 1.5. Tổng quan Tài liệu

Tài liệu SRS này được tổ chức thành 8 chương chính và **5 phụ lục**, theo cấu trúc từ tổng quan đến chi tiết:

- **Chương 2 – Mô tả Tổng quan:** Bối cảnh sản phẩm, chức năng tóm tắt, các lớp người dùng, môi trường vận hành và ràng buộc thiết kế.
- **Chương 3 – Yêu cầu Chức năng:** 37 FR được đặc tả chi tiết theo format chuẩn, nhóm thành 7 module chức năng.
- **Chương 4 – Yêu cầu Phi chức năng:** Hiệu năng, bảo mật, khả năng sử dụng, độ tin cậy, khả năng bảo trì/mở rộng và SEO.
- **Chương 5 – Giao diện Ngoài:** Tích hợp với các hệ thống và dịch vụ ngoài (Google OAuth, MinIO, Redis, SMTP/MailKit).
- **Chương 6 – Kiến trúc Hệ thống:** Clean Architecture Backend, Next.js App Router Frontend, chiến lược caching và deployment.
- **Chương 7 – Mô hình Dữ liệu:** ERD mô tả văn bản và bảng định nghĩa chi tiết từng entity/table, kèm Bảng Giới hạn Dữ liệu Chuẩn (mục 7.9) là nguồn sự thật duy nhất cho mọi validator và cột DB.
- **Chương 8 – Đặc tả API REST:** Quy ước, chuẩn lỗi RFC 7807, và bảng tổng hợp tất cả **44 endpoint** (37 của v1.1.0 + 7 bổ sung ở CR-2026-02).
- **Phụ lục A–E:** HTTP Status Codes, Application Error Codes, Từ điển thuật ngữ, Bảng truy vết CR-2026 (v1.1.0) và Bảng truy vết CR-2026-02 (v1.2.0).

> **Cách đếm FR:** 37 FR được liệt kê ở bảng mục 2.2. Trong đó **26 FR** có mục đặc tả đầy đủ theo template chuẩn (FR-AUTH-001 → 009, FR-CAT, FR-RCP, FR-SRCH-001); **11 FR** (FR-SRCH-002/003/004, FR-FILE-001/002, FR-JOB-001/002/003, FR-OBS-001/002/003) được trình bày dạng bảng gộp vì nội dung ngắn — nhưng từ v1.1.0 tất cả đều **có đầy đủ mức ưu tiên MoSCoW**.

---

## CHƯƠNG 2. MÔ TẢ TỔNG QUAN HỆ THỐNG

### 2.1. Bối cảnh Sản phẩm

#### 2.1.1. Vị trí trong Hệ sinh thái

Culinary Blog vận hành theo mô hình **API-Driven Architecture**, trong đó Backend (.NET 10) và Frontend (Next.js) là hai hệ thống độc lập giao tiếp hoàn toàn qua HTTP/JSON RESTful API. Không có server-side rendering truyền thống (MVC Razor/Blazor) hay shared view engine giữa hai tầng.

**Sơ đồ bối cảnh hệ thống (Context Diagram):**

```text
┌─────────────────────────────────────────────────────────────────┐
│                    CULINARY BLOG SYSTEM                         │
│                                                                 │
│   ┌──────────────────┐        ┌───────────────────────────────┐ │
│   │  NEXT.JS FRONTEND│◄──────►│    .NET 10 BACKEND API        │ │
│   │  (App Router)    │  REST  │    (Minimal APIs + Clean Arch)│ │
│   │  Port: 3000      │  JSON  │    Port: 5000                 │ │
│   └──────────────────┘        └──────────────┬────────────────┘ │
│                                              │                  │
│   ┌──────┐ ┌────────┐  ┌────────┐  ┌────────┐ ┌───────────┐     │
│   │ Pgsql│ │ Redis  │  │ MinIO  │  │Hangfire│ │Google Auth│     │
│   │:5432 │ │:6379   │  │:9000   │  │ Jobs   │ │ OAuth2.0  │     │
│   └──────┘ └────────┘  └────────┘  └────────┘ └───────────┘     │
└─────────────────────────────────────────────────────────────────┘
```

*Hình 2.1. Sơ đồ bối cảnh hệ thống Culinary Blog*

#### 2.1.2. Quan hệ với Hệ thống Ngoài

| Hệ thống Ngoài | Vai trò | Giao thức / Chuẩn | Hướng tích hợp |
| --- | --- | --- | --- |
| PostgreSQL 16 | Hệ quản trị CSDL quan hệ chính (RDBMS) | TCP + Npgsql Driver (EF Core) | Backend → PostgreSQL |
| Redis 7 | Distributed Cache & Session Store | TCP + StackExchange.Redis | Backend → Redis |
| MinIO (S3) | Object Storage cho ảnh công thức | HTTP/S3 API + `AWSSDK.S3` (`ForcePathStyle = true`, `ServiceURL` override) | Backend → MinIO |
| Google OAuth 2.0 | Đăng nhập bên thứ ba (Identity Provider) | HTTPS + OpenID Connect (ID Token verification phía Backend) | Client → Google → Client → Backend |
| Hangfire | Background Job Processing | **Container worker riêng** dùng chung codebase/image với API (`Hangfire__WorkerOnly=true`); API chỉ enqueue job và phục vụ Dashboard | Backend (internal) — hàng đợi dùng chung trên PostgreSQL |
| Serilog / Seq | Structured Log Aggregation (development) | HTTP Sink → Seq | Backend → Seq |
| OpenTelemetry Collector | Distributed Tracing & Metrics (production) | OTLP / gRPC | Backend → Collector |
| Nginx (Reverse Proxy) | SSL termination, load balancing, static serving | HTTP/HTTPS | Client → Nginx → Services |

### 2.2. Chức năng Sản phẩm Tổng quát

Culinary Blog cung cấp 7 nhóm chức năng chính, được hiện thực hóa qua **37 Functional Requirements** chi tiết tại Chương 3:

| Nhóm chức năng | Mã nhóm | Số FR | Mô tả tóm tắt |
| --- | --- | --- | --- |
| Xác thực & Quản lý Người dùng | FR-AUTH | 9 | Đăng ký, đăng nhập (email + Google), JWT refresh token rotation, logout, quản lý profile, khóa/mở khóa tài khoản (Admin), quản lý phiên đăng nhập (FR-AUTH-009). |
| Quản lý Danh mục | FR-CAT | 5 | CRUD danh mục công thức (Category) – phân quyền Admin. |
| Quản lý Công thức nấu ăn | FR-RCP | 11 | CRUD recipe, publish/unpublish/archive/unarchive, quản lý ảnh/bước/nguyên liệu, danh sách công thức cá nhân (`/recipes/mine`). |
| Tìm kiếm & Phân trang | FR-SRCH | 4 | Full-Text Search (PostgreSQL), filter, sort, offset pagination. |
| Quản lý Tệp tin | FR-FILE | 2 | Upload/Delete ảnh trên MinIO S3-compatible. |
| Background Jobs | FR-JOB | 3 | Email chào mừng, thumbnail generation, dọn dữ liệu soft-deleted vĩnh viễn (Hangfire). |
| Quan sát Hệ thống | FR-OBS | 3 | Health checks, structured logging, distributed tracing. |

### 2.3. Các Lớp Người dùng và Đặc điểm

Hệ thống định nghĩa 3 loại tác nhân (Actor) với quyền hạn khác nhau:

| Vai trò | Mô tả | Điều kiện | Quyền hạn chính | Ưu tiên phục vụ |
| --- | --- | --- | --- | --- |
| Khách (Guest / Anonymous) | Người dùng chưa xác thực, truy cập ứng dụng mà không có tài khoản. | Không cần tài khoản | Xem danh sách & chi tiết recipe (Published), xem danh mục, tìm kiếm. KHÔNG được tạo/sửa/xóa. | Cao (đây là đại đa số người dùng) |
| Tác giả (Author) | Người dùng đã đăng ký và xác thực thành công. Được tự động gán khi đăng ký. | Có tài khoản & JWT hợp lệ | + Tất cả quyền của Guest. + Tạo/sửa/xóa recipe CỦA MÌNH. + Upload ảnh, quản lý steps/ingredients. + Publish/Archive recipe của mình. + Xem và thu hồi các phiên đăng nhập của chính mình (FR-AUTH-009). | Cao (nhà sản xuất nội dung) |
| Quản trị viên (Admin) | Người quản lý hệ thống với quyền cao nhất. Được gán thủ công qua database seeding. | Có tài khoản & role Admin | + Tất cả quyền của Author. + Quản lý (CRUD) danh mục. + Sửa/xóa bất kỳ recipe của bất kỳ Author. + Xem danh sách, khóa/mở khóa tài khoản người dùng (FR-AUTH-008). + Truy cập Hangfire Dashboard (bảo vệ ở tầng Nginx, xem mục 5.3). + Xem structured logs qua công cụ vận hành (Seq/Elastic — ngoài phạm vi ứng dụng). | Trung bình (số lượng ít) |

**Ghi chú về phân quyền:** Hệ thống triển khai **2 tầng phân quyền** ở tầng ứng dụng.

1. **Role-Based Authorization:** phân biệt quyền dựa trên role (Guest/Author/Admin) qua policy `"AuthorPolicy"` và `"AdminPolicy"` (không hardcode chuỗi role trong endpoint).
2. **Resource-Based Authorization:** Author chỉ sửa/xóa được recipe của chính mình (`AuthorId == currentUserId`), kiểm tra qua `RecipeAuthorizationHandler`. Admin bypass resource ownership check.

> **[CR-2026 / MT-21]** Policy `"VerifiedAuthor"` (yêu cầu email đã xác nhận) **bị loại khỏi phạm vi v1.1.0**: hệ thống không có luồng xác nhận email, nên policy này sẽ chặn vĩnh viễn mọi thao tác ghi. Trường `emailConfirmed` không còn xuất hiện trong DTO công khai.
>
> **[CR-2026 / MT-33.5]** Role trong ASP.NET Core Identity **không phân cấp**. Vì vậy: (a) khi seeding, tài khoản Admin được gán **cả hai** role `Author` và `Admin`; (b) `"AuthorPolicy"` được khai báo `RequireRole("Author", "Admin")`. Ký hiệu "(Author ⊂ Admin)" trong Chương 8 chỉ là quy ước đọc tài liệu, không phải cơ chế của framework.

### 2.4. Môi trường Vận hành

#### 2.4.1. Môi trường Server (Production)

| Thành phần | Yêu cầu tối thiểu | Khuyến nghị | Ghi chú |
| --- | --- | --- | --- |
| Hệ điều hành | Linux Ubuntu 22.04 LTS | Ubuntu 22.04 LTS / Debian 12 | Docker phải được cài đặt |
| .NET Runtime | .NET 10.0 Runtime (aspnet) | .NET 10.0.x latest patch | Cung cấp qua Docker image `mcr.microsoft.com/dotnet/aspnet:10.0` |
| Node.js | Node.js 20 LTS (build only) | Node.js 22 LTS | Chỉ cần lúc build Next.js; production dùng standalone output |
| PostgreSQL | PostgreSQL 16.x | PostgreSQL 16.x | Extensions: `unaccent`, `pg_trgm` bắt buộc |
| Redis | Redis 7.x | Redis 7.2.x | Persistent mode với AOF |
| MinIO | MinIO RELEASE.2024+ | MinIO latest stable | Bucket policy: public-read cho recipe images |
| Docker | Docker Engine 24.x | Docker Engine 27.x + Compose v2 | Docker Compose cho local dev, staging **và production** (`docker-compose.prod.yml`) |
| Nginx | Nginx 1.24+ | Nginx 1.26+ (stable) | Reverse proxy, SSL termination |
| RAM | 4 GB minimum | 8 GB+ | RAM cần tăng nếu Redis cache lớn |
| CPU | 2 vCPU minimum | 4 vCPU+ | CPU-intensive: FTS indexing, image processing |
| Disk | 20 GB SSD minimum | 50 GB+ SSD | MinIO object storage tốn nhiều disk |

#### 2.4.2. Môi trường Phát triển (Development)

| Thành phần | Yêu cầu |
| --- | --- |
| .NET 10 SDK | dotnet SDK 10.0.x (bao gồm CLI và runtime) |
| Node.js | Node.js 20+ LTS với npm 10+ |
| Docker Desktop | Docker Desktop 4.x+ (Windows/macOS) hoặc Docker Engine (Linux) – để chạy PostgreSQL, Redis, MinIO local |
| IDE / Editor | Visual Studio 2022 v17.12+ / Rider 2024+ / VS Code với C# Dev Kit extension |
| Git | Git 2.40+ với Git LFS (nếu lưu asset lớn) |
| Postman / Scalar | Postman hoặc Scalar UI (tích hợp sẵn, chạy tại `/scalar`) để test API |

#### 2.4.3. Yêu cầu Trình duyệt Client

Mốc hỗ trợ được chốt theo mức hỗ trợ **ES2020** thực tế của build output (Next.js 15), thống nhất với mục 5.4.

| Trình duyệt | Phiên bản tối thiểu | Ghi chú |
| --- | --- | --- |
| Google Chrome | 112+ | Khuyến nghị chính – tốt nhất cho Developer Tools |
| Mozilla Firefox | 113+ | Hỗ trợ đầy đủ |
| Microsoft Edge | 112+ (Chromium) | Hỗ trợ đầy đủ (Chromium-based) |
| Safari | 16+ (macOS 12+) | Hỗ trợ đầy đủ; Safari 15 trở xuống KHÔNG đảm bảo |
| Mobile Chrome (Android) | 112+ | Responsive design, touch-friendly |
| Mobile Safari (iOS) | iOS 16+ | Hỗ trợ đầy đủ |
| Internet Explorer | Mọi phiên bản | KHÔNG hỗ trợ (EOL) |

### 2.5. Ràng buộc Thiết kế và Hiện thực

Các ràng buộc sau đây là **bắt buộc và không thể thương lượng** trong suốt quá trình phát triển:

| Mã ràng buộc | Loại | Mô tả ràng buộc |
| --- | --- | --- |
| CONS-001 | Kiến trúc | Backend PHẢI tuân thủ Clean Architecture với 4 tầng riêng biệt: Domain, Application, Infrastructure, Presentation. Tầng Domain không được phụ thuộc bất kỳ thư viện ngoài nào. |
| CONS-002 | Pattern | CQRS với MediatR là pattern bắt buộc cho tầng Application. Mỗi use case được hiện thực dưới dạng Command hoặc Query Handler riêng biệt. |
| CONS-003 | Ngôn ngữ / Framework | Backend: .NET 10 Minimal APIs (không dùng MVC Controllers). Frontend: Next.js App Router (không dùng Pages Router). |
| CONS-004 | Bảo mật | Xác thực PHẢI sử dụng JWT stateless (access token 15 phút, refresh token 7 ngày). Mật khẩu PHẢI được hash với PBKDF2 qua ASP.NET Core Identity. |
| CONS-005 | API Design | API PHẢI tuân thủ RESTful design. Phản hồi lỗi PHẢI theo RFC 7807 (`application/problem+json`). API versioning qua URL path (`/api/v1/`). |
| CONS-006 | Database | PostgreSQL là DBMS duy nhất. Migrations qua EF Core Code-First. **Không viết raw SQL cho truy vấn dữ liệu** — mọi query đi qua LINQ/EF Core có parameterization. **DDL đặc thù PostgreSQL** (`CREATE EXTENSION`, generated column, index GIN/partial/composite, hàm wrapper `IMMUTABLE`) được phép khai báo trong migration EF Core, vì đây là định nghĩa schema chứ không phải truy vấn dữ liệu người dùng. |
| CONS-007 | File Upload | Kích thước tệp tải lên tối đa 5 MB. Định dạng chỉ chấp nhận: `image/jpeg`, `image/png`, `image/webp`, `image/avif`. Kiểm tra MIME type (không chỉ extension). |
| CONS-008 | Validation | Input validation PHẢI qua FluentValidation kết hợp MediatR Pipeline Behavior. Không validation trong Endpoint handler. |
| CONS-009 | Container | Ứng dụng PHẢI được đóng gói Docker. Dockerfile multi-stage build (SDK → aspnet runtime). Docker Compose cho local development. |
| CONS-010 | Logging | Structured logging với Serilog là bắt buộc. Mọi log entry PHẢI có CorrelationId, RequestPath, UserId (khi đã xác thực). |

### 2.6. Giả định và Phụ thuộc

#### 2.6.1. Giả định

- Môi trường development có kết nối Internet để pull Docker images và package NuGet/npm.
- PostgreSQL, Redis và MinIO được cung cấp qua Docker Compose trong development và dưới dạng managed service (hoặc VPS) trong production.
- Người dùng cuối có trình duyệt hiện đại và kết nối Internet đủ ổn định để load ảnh từ MinIO.
- Dữ liệu test (seed) có **ít nhất 20 danh mục và 100 công thức; mỗi công thức có ít nhất 10 nguyên liệu và ít nhất 5 bước chế biến**, cùng 5 tác giả mẫu. Nội dung món ăn (tên, mô tả, nguyên liệu, định lượng, các bước, thời gian) phải đúng với món; thư viện Bogus sinh các giá trị ngẫu nhiên (tác giả, trạng thái, ngày xuất bản, dinh dưỡng) với seed cố định. Việc seed là idempotent: chạy lại chỉ bổ sung phần còn thiếu, không ghi đè dữ liệu do người dùng tạo hoặc đã chỉnh sửa.

> **[CR-2026-03 / MT-58]** v1.2.0 ghi *"Bogus với 50 recipe mẫu và 5 tác giả mẫu"*. Yêu cầu mới của giảng viên cho Buổi 2 nâng lên ≥ 20 danh mục, ≥ 100 công thức, ≥ 10 nguyên liệu và ≥ 5 bước mỗi công thức. Dữ liệu này dùng để kiểm thử tìm kiếm tiếng Việt, phân trang, lọc và trình diễn, nên phải là công thức có nghĩa chứ không phải chuỗi sinh ngẫu nhiên.
- Email service **SMTP (gửi qua MailKit)** được cấu hình sẵn khi triển khai production để gửi email chào mừng; môi trường development dùng Mailhog.
- Giới hạn dữ liệu kỳ vọng (initial scale): ≤ 10,000 công thức, ≤ 5,000 người dùng, ≤ 50 danh mục – phù hợp với single-server deployment.

#### 2.6.2. Phụ thuộc Bên ngoài

| Phụ thuộc | Phiên bản | Mức độ ảnh hưởng nếu không khả dụng | Kế hoạch dự phòng |
| --- | --- | --- | --- |
| Google OAuth 2.0 API | v2 (OpenID Connect) | Cao – Mất chức năng đăng nhập Google | Vẫn có đăng nhập email/password. Hiển thị thông báo "Google login tạm thời không khả dụng". |
| MinIO / S3 | MinIO RELEASE.2024+ | Cao – Không upload/xem được ảnh | Fallback về local FileSystem storage (development only). Production cần MinIO. |
| Redis | 7.x | Trung bình – Mất cache, hiệu năng giảm | Hệ thống tiếp tục hoạt động nhưng mọi request đều query database. Cache miss graceful degradation. |
| PostgreSQL | 16.x | Rất cao – Toàn bộ hệ thống ngừng | Backup định kỳ (pg_dump). Readiness probe sẽ fail, Nginx trả 503. |
| Hangfire (container worker) | v1.8+ | Thấp – Background jobs bị trễ | Job queue được persist trên PostgreSQL: job đã enqueue **vẫn nằm trong hàng đợi và chạy lại khi worker khởi động** — chỉ bị trễ, không bị mất. Recurring jobs bỏ qua chu kỳ đang lỡ. Không ảnh hưởng core functionality. |

---

## CHƯƠNG 3. YÊU CẦU CHỨC NĂNG CHI TIẾT

Chương này đặc tả chi tiết **37 Functional Requirements (FR)** được nhóm thành 7 module chức năng. Mỗi FR được mô tả theo template chuẩn bao gồm: Mã yêu cầu, Tên, Nhóm chức năng, Tác nhân, Mức ưu tiên (MoSCoW), Mô tả, Điều kiện tiên quyết, Luồng chính, Luồng thay thế/Ngoại lệ, HTTP Endpoint, Kết quả mong đợi và HTTP Status Code.

**Quy ước mức ưu tiên MoSCoW:** M (Must Have – Bắt buộc), S (Should Have – Nên có), C (Could Have – Có thể có), W (Won't Have – Không trong scope hiện tại; xem danh sách "ngoài phạm vi" tại mục 1.2.3).

> **Quy tắc ưu tiên [CR-2026 / MT-40]:** *Một FR mức **Must Have** không được phụ thuộc vào FR có mức ưu tiên thấp hơn.* Áp dụng quy tắc này, v1.1.0 nâng FR-RCP-006 (Archive/Unarchive), FR-AUTH-003 (Google OAuth) và FR-AUTH-006 (Xem hồ sơ) lên mức **M**, đồng thời gán mức ưu tiên cho toàn bộ 11 FR trước đây bị bỏ trống.

**Quy ước mã lỗi HTTP dùng xuyên suốt Chương 3 [CR-2026 / MT-08, MT-09]:**

| Tình huống | Mã HTTP | Ghi chú |
| --- | --- | --- |
| Mọi lỗi validation đầu vào (FluentValidation), body malformed, tham số query sai, file sai MIME/kích thước | **400 Bad Request** | Trả `ValidationProblemDetails` (RFC 7807) với object `errors{}` |
| Vi phạm bất biến nghiệp vụ trước khi đổi trạng thái (ví dụ publish thiếu step/ingredient) | **400 Bad Request** | Kèm Application Error Code cụ thể |
| Mọi xung đột trạng thái tài nguyên: trùng unique (email/slug/name), xóa category còn recipe, **Optimistic Concurrency (RowVersion)**, chuyển trạng thái không hợp lệ | **409 Conflict** | Frontend phân biệt qua trường `type` (Phụ lục B) |
| Tài khoản bị khóa tạm thời do sai mật khẩu nhiều lần | **423 Locked** | `AUTH_ACCOUNT_LOCKED` |
| Tài khoản bị Admin vô hiệu hóa (`IsActive = false`) | **403 Forbidden** | `AUTH_ACCOUNT_DISABLED` |

> **Mã 422 Unprocessable Entity bị loại bỏ hoàn toàn khỏi hệ thống.** Mọi vị trí trước đây dùng 422 đã được chuyển sang 400 (validation) hoặc 409 (xung đột trạng thái).

### 3.1. Module Xác thực và Quản lý Người dùng (FR-AUTH)

Module này quản lý toàn bộ vòng đời xác thực người dùng: từ đăng ký, đăng nhập đa phương thức, duy trì phiên làm việc với cơ chế token rotation, đến quản lý hồ sơ cá nhân. Backend sử dụng ASP.NET Core Identity kết hợp JWT và OAuth 2.0.

#### FR-AUTH-001: Đăng ký Tài khoản (User Registration)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-001 |
| **Tên yêu cầu** | Đăng ký Tài khoản Mới |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Khách (Guest / Anonymous User) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have (Bắt buộc) |
| **Mô tả** | Hệ thống cho phép người dùng chưa có tài khoản tạo một tài khoản mới bằng cách cung cấp `email`, `password` và `displayName`. Sau khi đăng ký thành công, người dùng tự động được gán role "Author" và **nhận ngay cặp token để truy cập (auto-login sau đăng ký)**. `UserName` của Identity **do server tự sinh** từ phần trước ký tự `@` của email (thêm hậu tố số nếu trùng) — người dùng không nhập. Hệ thống kích hoạt job gửi email chào mừng bất đồng bộ qua Hangfire. |
| **Điều kiện tiên quyết** | 1. Người dùng chưa đăng nhập vào hệ thống. 2. Endpoint `POST /api/v1/auth/register` đang hoạt động. 3. PostgreSQL database đang kết nối thành công. |
| **HTTP Method & Endpoint** | `POST /api/v1/auth/register` |
| **Kết quả mong đợi** | Tài khoản mới được tạo trong database, role "Author" được gán, **hash** của refresh token được persist, email chào mừng được đẩy vào Hangfire queue. Client nhận được `AuthResponseDto` (access token + refresh token). |
| **HTTP Status Code trả về** | `201 Created` – Đăng ký thành công (body là `AuthResponseDto`). `400 Bad Request` – Dữ liệu không hợp lệ / password không đủ mạnh. `409 Conflict` – Email đã tồn tại (`AUTH_EMAIL_EXISTS`). `429 Too Many Requests` – Vượt rate limit. `500 Internal Server Error` – Lỗi hệ thống. |

**Luồng chính (Happy Path):**

1. Người dùng (client) gửi HTTP POST đến `/api/v1/auth/register` với JSON body: `{ "email": "...", "password": "...", "displayName": "..." }`.
2. `RegisterCommand` được tạo và dispatch đến MediatR.
3. `ValidationBehavior` chạy `RegisterCommandValidator`: `email` đúng format và ≤ 256 ký tự; `displayName` 2–100 ký tự; `password` ≥ 8 ký tự chứa **đủ 4 loại ký tự: 1 chữ hoa, 1 chữ thường, 1 chữ số, 1 ký tự đặc biệt** (thống nhất với NFR-SEC-001 và `IdentityOptions.Password`).
4. `RegisterCommandHandler` kiểm tra email chưa tồn tại trong database (`UserManager.FindByEmailAsync`).
5. Sinh `UserName` từ prefix email: `SlugHelper.NormalizeUserName(email.Split('@')[0])`; nếu đã tồn tại thì thêm hậu tố số tăng dần cho đến khi unique.
6. Tạo `ApplicationUser` mới qua factory method `ApplicationUser.Create(email, userName, displayName)`.
7. `UserManager.CreateAsync(user, password)` – ASP.NET Core Identity tự hash password với PBKDF2.
8. `UserManager.AddToRoleAsync(user, "Author")` – gán role mặc định.
9. `JwtService.GenerateAccessToken()` – tạo JWT access token (HS256, 15 phút).
10. `JwtService.GenerateRefreshToken()` – tạo refresh token ngẫu nhiên **256-bit (32 bytes qua `RandomNumberGenerator.GetBytes(32)`, encode Base64URL), TTL 7 ngày**.
11. Lưu bản ghi `RefreshToken` vào bảng `RefreshTokens`: **chỉ lưu `TokenHash = SHA256(rawToken)` (64 ký tự hex), không bao giờ lưu raw token**; ghi `CreatedByIp` lấy từ `X-Forwarded-For` (xem NFR-SEC-003).
12. `BackgroundJob.Enqueue<WelcomeEmailJob>()` – đẩy job gửi email chào mừng vào Hangfire queue (fire-and-forget).
13. Trả về HTTP 201 Created với `AuthResponseDto`: `{ accessToken, refreshToken, expiresAt, user: { id, email, displayName, avatarUrl, bio, roles } }`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Email đã tồn tại:** Tại bước 4, nếu email đã được đăng ký → Throw `ConflictException` → `GlobalExceptionMiddleware` trả về HTTP **409 Conflict** (`AUTH_EMAIL_EXISTS`) với RFC 7807 body.
- **A2 – Password không đủ mạnh:** Tại bước 3 hoặc 7, `UserManager.CreateAsync` trả về `IdentityError` → Throw `ValidationException` → HTTP **400 Bad Request** với danh sách lỗi chi tiết.
- **A3 – Dữ liệu đầu vào không hợp lệ:** Tại bước 3, FluentValidation fail → HTTP **400** với từng field lỗi (theo RFC 7807 `ValidationProblemDetails`, code `VALIDATION_ERROR`).
- **A4 – Database không kết nối:** EF Core ném `DbUpdateException` → HTTP 500 Internal Server Error (`GlobalExceptionMiddleware` log lỗi, không expose stack trace).

> **[CR-2026 / MT-10, MT-12]** Endpoint này **trả về cặp token** — Frontend chuyển thẳng người dùng vào `/dashboard` sau khi đăng ký, không cần gọi `/auth/login`. Trường `fullName` bị loại bỏ khỏi toàn hệ thống; tên hiển thị công khai duy nhất là `displayName` (khớp cột `DisplayName` tại mục 7.7).

#### FR-AUTH-002: Đăng nhập bằng Email/Mật khẩu (Local Login)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-002 |
| **Tên yêu cầu** | Đăng nhập bằng Email và Mật khẩu |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Tác giả đã đăng ký (Author) hoặc Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have (Bắt buộc) |
| **Mô tả** | Hệ thống cho phép người dùng đã có tài khoản đăng nhập bằng email và mật khẩu. Mỗi lần đăng nhập thành công tạo ra một cặp access token mới (JWT, 15 phút) và refresh token mới (7 ngày). Cơ chế Token Rotation: refresh token cũ KHÔNG bị xóa ngay mà được đánh dấu đã sử dụng (để phát hiện token reuse attack). |
| **Điều kiện tiên quyết** | 1. Người dùng đã có tài khoản hợp lệ trong hệ thống. 2. Tài khoản chưa bị lockout (chưa đến lockout deadline). 3. Tài khoản chưa bị Admin vô hiệu hóa (`IsActive == true`). |
| **HTTP Method & Endpoint** | `POST /api/v1/auth/login` |
| **Kết quả mong đợi** | Access token và refresh token mới được tạo và trả về. Hash của refresh token được lưu vào database. |
| **HTTP Status Code trả về** | `200 OK` – Đăng nhập thành công. `400 Bad Request` – Dữ liệu không hợp lệ. `401 Unauthorized` – Sai email/mật khẩu. `403 Forbidden` – Tài khoản bị vô hiệu hóa (`AUTH_ACCOUNT_DISABLED`). `423 Locked` – Tài khoản bị khóa tạm thời (`AUTH_ACCOUNT_LOCKED`). `429 Too Many Requests` – Vượt rate limit. |

**Luồng chính (Happy Path):**

1. Client gửi `POST /api/v1/auth/login` với body: `{ "email": "...", "password": "..." }`.
2. `LoginCommand` được dispatch qua MediatR.
3. `ValidationBehavior` kiểm tra email format và password không rỗng.
4. `LoginCommandHandler` tìm user: `UserManager.FindByEmailAsync(email)`.
5. Xác minh mật khẩu: `UserManager.CheckPasswordAsync(user, password)` – so sánh với PBKDF2 hash.
6. Kiểm tra tài khoản không bị lockout: `UserManager.IsLockedOutAsync(user)`.
7. **Kiểm tra `user.IsActive == true`** — nếu false, dừng luồng và trả 403 (nhánh A4).
8. Tạo access token mới: `JwtService.GenerateAccessToken(user, roles)`.
9. Tạo refresh token mới (256-bit): `JwtService.GenerateRefreshToken(userId)`.
10. Lưu `TokenHash = SHA256(rawToken)` cùng `CreatedByIp` vào database.
11. Ghi nhận đăng nhập thành công: `UserManager.ResetAccessFailedCountAsync(user)`.
12. Trả về HTTP 200 OK với `AuthResponseDto`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Tài khoản không tồn tại hoặc mật khẩu sai:** HTTP 401 Unauthorized với message generic "Email hoặc mật khẩu không đúng" (KHÔNG tiết lộ tài khoản có tồn tại hay không – tránh User Enumeration Attack).
- **A2 – Tài khoản bị lockout:** HTTP 423 Locked (`AUTH_ACCOUNT_LOCKED`) với thông báo thời gian unlock còn lại.
- **A3 – Vượt quá số lần thử sai (5 lần):** `AccessFailedCount` tăng lên, sau 5 lần → tài khoản bị lockout 15 phút (cấu hình qua `LockoutOptions`).
- **A4 – Tài khoản bị Admin vô hiệu hóa (`IsActive == false`):** HTTP **403 Forbidden** với `AUTH_ACCOUNT_DISABLED`. *(Bước kiểm tra này là điều kiện để mã lỗi trong Phụ lục B thực sự được sinh ra — xem FR-AUTH-008.)*
- **A5 – Dữ liệu đầu vào không hợp lệ:** HTTP 400 Bad Request.

#### FR-AUTH-003: Đăng nhập bằng Google OAuth 2.0

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-003 |
| **Tên yêu cầu** | Đăng nhập / Đăng ký bằng Google OAuth 2.0 |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Khách (Guest) – lần đầu / Người dùng đã đăng ký trước đó qua Google |
| **Mức ưu tiên (MoSCoW)** | **M – Must Have** *(nâng từ S: mục 1.2.2 liệt kê Google OAuth là một trong các tính năng cốt lõi và mục 5.1 bắt buộc nút Google trên `/auth/login`)* |
| **Mô tả** | Hệ thống hỗ trợ đăng nhập qua tài khoản Google theo mô hình **ID Token verification**: Frontend lấy `idToken` từ Google Identity Services (Google Sign-In JS SDK), Backend **tự xác minh chữ ký ID Token** bằng `Google.Apis.Auth` trước khi tin tưởng bất kỳ thông tin nào. Nếu đây là lần đăng nhập Google đầu tiên, hệ thống tự động tạo tài khoản mới từ thông tin Google profile (email, `displayName`, avatar URL) và gán role "Author". Nếu email đã tồn tại từ đăng ký thủ công trước đó, hệ thống liên kết Google login với tài khoản hiện có. |
| **Điều kiện tiên quyết** | 1. `GoogleClientId` đã được cấu hình ở cả Backend (để verify `aud`) và Frontend. 2. Origin của Frontend đã được đăng ký trong Google Cloud Console (Authorized JavaScript origins). 3. Người dùng có tài khoản Google hợp lệ. |
| **HTTP Method & Endpoint** | `POST /api/v1/auth/google` |
| **Kết quả mong đợi** | Người dùng được đăng nhập (hoặc tự động đăng ký), nhận `AuthResponseDto`. Tài khoản mới (nếu có) được tạo với role "Author". |
| **HTTP Status Code trả về** | `200 OK` – Đăng nhập/đăng ký thành công. `400 Bad Request` – `idToken` thiếu/sai định dạng hoặc thiếu claim `email` (`AUTH_GOOGLE_TOKEN_INVALID`). `401 Unauthorized` – ID Token không vượt qua kiểm tra chữ ký/`aud`/`exp`. `403 Forbidden` – Tài khoản liên kết đã bị vô hiệu hóa. `502 Bad Gateway` – Không truy cập được Google để lấy khóa công khai (`AUTH_GOOGLE_UNAVAILABLE`). |

**Luồng chính (Happy Path):**

1. Frontend khởi tạo Google Identity Services với `GoogleClientId` và scopes `openid`, `email`, `profile`.
2. Người dùng xác nhận cấp quyền trên Google Consent Screen; Google trả **ID Token (JWT)** trực tiếp cho Frontend.
3. Frontend gửi `POST /api/v1/auth/google` với body `{ "idToken": "..." }`.
4. `GoogleLoginCommandHandler` gọi `GoogleJsonWebSignature.ValidateAsync(idToken, settings)` — xác minh chữ ký Google, `aud == GoogleClientId`, `iss ∈ {accounts.google.com, https://accounts.google.com}`, `exp` chưa hết hạn. **Không có bất kỳ dữ liệu nào từ Frontend được tin tưởng trước bước này.**
5. Trích xuất `sub` (providerKey), `email`, `name`, `picture` từ payload đã verify.
6. Tìm user bằng `UserManager.FindByLoginAsync("Google", providerKey)`.
7. Nếu chưa có external login: tìm theo email → nếu email chưa tồn tại thì tạo `ApplicationUser` mới (`displayName` = `name`, `avatarUrl` = `picture`, `UserName` sinh từ prefix email như FR-AUTH-001), gán role "Author" → `AddLoginAsync`.
8. Nếu email đã tồn tại (đã đăng ký thủ công): liên kết Google login → `AddLoginAsync` với tài khoản hiện có.
9. Kiểm tra `user.IsActive == true`.
10. Tạo access token và refresh token (lưu hash), trả về HTTP 200 OK với `AuthResponseDto`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – ID Token không hợp lệ, sai `aud` hoặc hết hạn:** HTTP 401 Unauthorized (`AUTH_GOOGLE_TOKEN_INVALID`).
- **A2 – ID Token thiếu claim `email` hoặc `email_verified == false`:** HTTP 400 Bad Request.
- **A3 – Google API không khả dụng (không lấy được JWKS):** HTTP 502 Bad Gateway (`AUTH_GOOGLE_UNAVAILABLE`); Frontend hiển thị thông báo dự phòng theo mục 2.6.2.
- **A4 – Tài khoản liên kết bị vô hiệu hóa:** HTTP 403 Forbidden (`AUTH_ACCOUNT_DISABLED`).

> **[CR-2026 / MT-11]** Phương án `ExternalLoginInfo` bị loại bỏ (đây là kiểu nội bộ của ASP.NET Core Identity, không serialize qua HTTP được) và **Authorization Code + PKCE cũng bị loại** khỏi v1.1.0. Backend **không** có redirect URI `/api/v1/auth/google/callback`; toàn bộ vòng lặp redirect do Google Identity Services xử lý phía trình duyệt.

#### FR-AUTH-004: Làm mới Access Token (Token Refresh)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-004 |
| **Tên yêu cầu** | Làm mới Access Token bằng Refresh Token |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) – có refresh token hợp lệ |
| **Mức ưu tiên (MoSCoW)** | M – Must Have (Bắt buộc) |
| **Mô tả** | Khi access token hết hạn (sau 15 phút), client sử dụng refresh token còn hiệu lực để lấy cặp token mới mà không cần người dùng đăng nhập lại. Cơ chế Token Rotation bắt buộc: mỗi lần refresh, refresh token cũ bị vô hiệu hóa bằng cách **gán `RevokedAt = DateTime.UtcNow`** và một refresh token MỚI được tạo ra, liên kết ngược qua `ReplacedByTokenHash`. Đây là biện pháp chống Refresh Token Reuse Attack. |
| **Điều kiện tiên quyết** | 1. Client có refresh token hợp lệ — định nghĩa hợp lệ là **`RevokedAt IS NULL AND ExpiresAt > NOW()`**. 2. Người dùng tương ứng vẫn tồn tại, `IsActive == true` và chưa bị lockout. |
| **HTTP Method & Endpoint** | `POST /api/v1/auth/refresh` |
| **Kết quả mong đợi** | Refresh token cũ bị invalidate. Access token mới (15 phút) và refresh token mới (7 ngày) được tạo và trả về. |
| **HTTP Status Code trả về** | `200 OK` – Refresh thành công. `400 Bad Request` – Thiếu trường `refreshToken`. `401 Unauthorized` – Token không hợp lệ, hết hạn hoặc đã bị revoke. `403 Forbidden` – Tài khoản đã bị vô hiệu hóa. |

**Luồng chính (Happy Path):**

1. Client gửi `POST /api/v1/auth/refresh` với body: `{ "refreshToken": "..." }`.
2. `RefreshTokenCommand` dispatch qua MediatR.
3. Handler **tính `SHA256(refreshToken)`** rồi tra cứu theo cột `TokenHash` (có UNIQUE index), include User navigation property. *Raw token không bao giờ được dùng làm khóa tra cứu và không bao giờ được ghi vào log.*
4. Kiểm tra: token tồn tại, **`RevokedAt == null`**, `ExpiresAt > DateTime.UtcNow`, `user.IsActive == true`, user chưa bị lockout.
5. Tạo refresh token mới (256-bit) và tính hash của nó.
6. Đánh dấu token cũ trong cùng transaction: `RevokedAt = DateTime.UtcNow`, **`ReplacedByTokenHash = SHA256(newToken)`**.
7. Tạo access token mới cho user; lưu bản ghi `RefreshToken` mới (chỉ hash) kèm `CreatedByIp`.
8. `SaveChangesAsync()` — cả hai thay đổi nằm trong một transaction.
9. Trả về HTTP 200 OK với `AuthResponseDto` chứa cặp token mới.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Hash không tìm thấy trong database:** HTTP 401 Unauthorized (`AUTH_TOKEN_INVALID`).
- **A2 – Refresh token đã hết hạn (`ExpiresAt <= NOW()`):** HTTP 401 Unauthorized (`AUTH_REFRESH_TOKEN_EXPIRED`), client phải đăng nhập lại.
- **A3 – Refresh token đã bị revoke (`RevokedAt != null`) → Reuse Attack detected:** HTTP 401 Unauthorized (`AUTH_REFRESH_TOKEN_REVOKED`). LOG SECURITY ALERT mức WARNING và **revoke toàn bộ token family của user đó** (lần theo chuỗi `ReplacedByTokenHash`).
- **A4 – User bị vô hiệu hóa (`IsActive == false`) sau khi token được cấp:** HTTP 403 Forbidden (`AUTH_ACCOUNT_DISABLED`).
- **A5 – User bị xóa sau khi token được cấp:** HTTP 401 Unauthorized.

> **[CR-2026 / MT-14, MT-15]** Cột `IsRevoked` **không tồn tại** trong schema (mục 7.8) và không được dùng ở bất kỳ đâu — trạng thái thu hồi chỉ có một nguồn sự thật duy nhất là `RevokedAt`. Raw token chỉ tồn tại trong response HTTP; DB chỉ lưu SHA-256 hash.

#### FR-AUTH-005: Đăng xuất (Logout / Token Revocation)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-005 |
| **Tên yêu cầu** | Đăng xuất và Thu hồi Refresh Token |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Người dùng đăng xuất khỏi hệ thống. Vì JWT access token là stateless (không thể revoke trực tiếp trước khi hết hạn), hành động logout chủ yếu là revoke refresh token tương ứng trong database. Client có trách nhiệm xóa access token khỏi bộ nhớ (localStorage/cookie) phía client. |
| **Điều kiện tiên quyết** | 1. Người dùng đang đăng nhập với access token hợp lệ trong Authorization header. 2. Client gửi refresh token muốn revoke. |
| **HTTP Method & Endpoint** | `POST /api/v1/auth/logout` |
| **Kết quả mong đợi** | Refresh token được đánh dấu `RevokedAt = DateTime.UtcNow` trong database. Các lần refresh tiếp theo với token này sẽ thất bại. |
| **HTTP Status Code trả về** | `204 No Content` – Đăng xuất thành công (hoặc token không tồn tại – idempotent). `401 Unauthorized` – Access token không hợp lệ. |

**Luồng chính (Happy Path):**

1. Client gửi `POST /api/v1/auth/logout` với `Authorization: Bearer {accessToken}` header và body: `{ "refreshToken": "..." }`.
2. Middleware xác thực JWT (`UseAuthentication`) xác minh access token.
3. `LogoutCommandHandler` tính `SHA256(refreshToken)` và tra cứu theo `TokenHash`.
4. Nếu tìm thấy, thuộc về user hiện tại và `RevokedAt == null`: gán `RevokedAt = DateTime.UtcNow`.
5. Lưu thay đổi vào database.
6. Trả về HTTP 204 No Content.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Refresh token không tìm thấy:** Vẫn trả về HTTP 204 (idempotent – không tiết lộ trạng thái).
- **A2 – Access token đã hết hạn:** Vẫn cho phép logout nếu refresh token hợp lệ; hoặc HTTP 401 nếu không cung cấp refresh token.

#### FR-AUTH-006: Xem Hồ sơ Cá nhân (View Profile)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-006 |
| **Tên yêu cầu** | Xem Hồ sơ Cá nhân |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập |
| **Mức ưu tiên (MoSCoW)** | **M – Must Have** *(nâng từ S: đây là cách duy nhất Frontend lấy được `roles` để điều hướng các route `/dashboard*` ở mục 5.1)* |
| **Mô tả** | Trả về thông tin hồ sơ của người dùng hiện đang đăng nhập, dựa trên UserId được trích xuất từ JWT claims. Không bao giờ trả về `PasswordHash` hoặc `SecurityStamp`. Response **không được cache** (`Cache-Control: no-store`) vì phụ thuộc danh tính. |
| **Điều kiện tiên quyết** | 1. Người dùng đang đăng nhập với access token hợp lệ. |
| **HTTP Method & Endpoint** | `GET /api/v1/auth/me` |
| **Kết quả mong đợi** | Trả về thông tin hồ sơ đầy đủ của người dùng (không có thông tin nhạy cảm như password hash). |
| **HTTP Status Code trả về** | `200 OK` – Thành công. `401 Unauthorized` – Chưa đăng nhập hoặc token trỏ tới user không còn hợp lệ. |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/auth/me` với `Authorization: Bearer {accessToken}`.
2. Middleware xác thực JWT, trích xuất UserId từ claim `NameIdentifier`.
3. `GetCurrentUserQuery` dispatch qua MediatR.
4. Handler tìm user: `UserManager.FindByIdAsync(userId)`.
5. Map sang `UserProfileDto`: `{ id, email, displayName, avatarUrl, bio, roles, createdAt }`.
6. Trả về HTTP 200 OK với `UserProfileDto` kèm header `Cache-Control: no-store`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – User không còn tồn tại trong database sau khi token được cấp:** HTTP **401 Unauthorized** — token trỏ tới một danh tính không còn hợp lệ. *(Hệ thống không có FR xóa người dùng và `AspNetUsers` không kế thừa `BaseEntity`, nên nhánh 404 của v1.0.0 bị loại bỏ.)*

> **[CR-2026 / MT-12, MT-21]** DTO dùng `displayName` + `bio` (khớp mục 7.7 và Chương 8); các trường `fullName`, `userName`, `emailConfirmed` bị loại khỏi DTO công khai.

#### FR-AUTH-007: Cập nhật Hồ sơ Cá nhân (Update Profile)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-007 |
| **Tên yêu cầu** | Cập nhật Hồ sơ Cá nhân |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập |
| **Mức ưu tiên (MoSCoW)** | S – Should Have |
| **Mô tả** | Người dùng có thể cập nhật `DisplayName`, `AvatarUrl` và `Bio` của mình. Email và UserName không thể thay đổi qua endpoint này (`UserName` do hệ thống sinh, xem FR-AUTH-001). Sử dụng PATCH (partial update) để chỉ cập nhật các field được cung cấp. |
| **Điều kiện tiên quyết** | 1. Người dùng đang đăng nhập. 2. Dữ liệu mới phải hợp lệ theo Bảng Giới hạn Dữ liệu Chuẩn (mục 7.9). |
| **HTTP Method & Endpoint** | `PATCH /api/v1/auth/me` |
| **Kết quả mong đợi** | Hồ sơ người dùng được cập nhật trong database. Trả về hồ sơ mới. |
| **HTTP Status Code trả về** | `200 OK` – Cập nhật thành công. `400 Bad Request` – Dữ liệu không hợp lệ. `401 Unauthorized` – Chưa đăng nhập. |

**Luồng chính (Happy Path):**

1. Client gửi `PATCH /api/v1/auth/me` với body: `{ "displayName"?: "...", "avatarUrl"?: "...", "bio"?: "..." }`.
2. `UpdateProfileCommand` dispatch qua MediatR, UserId lấy từ JWT claims.
3. `ValidationBehavior` kiểm tra: `displayName` 2–100 ký tự, `bio` ≤ 1000 ký tự, `avatarUrl` là URL hợp lệ ≤ 500 ký tự (nếu cung cấp).
4. Handler tìm user, cập nhật các field được gửi lên.
5. `UserManager.UpdateAsync(user)`.
6. Trả về HTTP 200 OK với `UserProfileDto` đã cập nhật.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Dữ liệu không hợp lệ:** HTTP 400 Bad Request (`VALIDATION_ERROR`).

#### FR-AUTH-008: Quản lý Tài khoản Người dùng — Xem danh sách, Khóa / Mở khóa [Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-008 |
| **Tên yêu cầu** | Xem danh sách người dùng; Khóa (Deactivate) / Mở khóa (Reactivate) Tài khoản |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | S – Should Have |
| **Mô tả** | Admin **tìm người dùng** trong danh sách phân trang, rồi bật/tắt cờ `IsActive` của một tài khoản để chặn tài khoản spam hoặc vi phạm. Khi khóa (`isActive = false`), hệ thống **revoke toàn bộ refresh token còn hiệu lực** của người dùng đó ("force revoke"), buộc phiên làm việc kết thúc ngay ở lần refresh kế tiếp. Access token đã cấp vẫn còn sống tối đa 15 phút — đây là đánh đổi có chủ đích của thiết kế JWT stateless (CONS-004) và được chấp nhận. |
| **Điều kiện tiên quyết** | 1. Admin đang đăng nhập với role Admin. 2. Người dùng đích tồn tại. 3. Admin không thể tự khóa chính tài khoản của mình. 4. Danh sách: `page >= 1`, `pageSize ∈ [1, 50]` (mặc định 12). |
| **HTTP Method & Endpoint** | `GET /api/v1/users?page&pageSize&search&isActive` · `PATCH /api/v1/users/{id}/status` |
| **Kết quả mong đợi** | **Danh sách:** `PagedResult<UserAdminDto>` với `UserAdminDto = { id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }` — **không** chứa `PasswordHash`, `SecurityStamp`, `UserName`. **Khóa/mở khóa:** `IsActive` được cập nhật; khi khóa, mọi `RefreshToken` của user được gán `RevokedAt = DateTime.UtcNow`. Ghi audit log (userId thao tác + userId bị tác động + lý do + timestamp). |
| **HTTP Status Code trả về** | `200 OK` – Thành công. `400 Bad Request` – Tham số/body không hợp lệ. `403 Forbidden` – Không có quyền Admin, **hoặc Admin tự khóa chính mình**. `404 Not Found` – User không tồn tại. |

**Luồng chính — Xem danh sách:**

1. Admin gửi `GET /api/v1/users?page=1&pageSize=12&search=an&isActive=true`.
2. `"AdminPolicy"` kiểm tra role; `GetUsersQuery` lọc theo `search` (khớp một phần `Email` hoặc `DisplayName`, không phân biệt hoa thường) và `isActive`.
3. Trả `PagedResult<UserAdminDto>` sắp theo `CreatedAt` giảm dần. **Không cache** — dữ liệu quản trị, thay đổi ngay sau mỗi thao tác khóa/mở khóa.

**Luồng chính — Khóa / Mở khóa:**

1. Admin gửi `PATCH /api/v1/users/{id}/status` với body: `{ "isActive": false, "reason": "Spam" }`.
2. `"AdminPolicy"` kiểm tra role.
3. `SetUserStatusCommand` dispatch qua MediatR.
4. Handler kiểm tra `id != currentUserId`; tìm user qua `UserManager.FindByIdAsync(id)`.
5. Gán `user.IsActive = isActive`, `UserManager.UpdateAsync(user)`.
6. Nếu `isActive == false`: `UPDATE "RefreshTokens" SET "RevokedAt" = NOW() WHERE "UserId" = @id AND "RevokedAt" IS NULL` (qua repository, trong cùng transaction).
7. Ghi audit log qua Serilog theo NFR-SEC-006.
8. Trả về HTTP 200 OK với `{ id, email, displayName, isActive }`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Thiếu role Admin:** HTTP 403 Forbidden.
- **A2 – Admin tự khóa chính mình:** HTTP 403 Forbidden với thông báo rõ lý do.
- **A3 – User không tồn tại:** HTTP 404 Not Found.

> **[CR-2026 / MT-22]** FR này tồn tại để cột `IsActive` (mục 7.7) và mã lỗi `AUTH_ACCOUNT_DISABLED` (Phụ lục B) có chủ thể thực thi. Việc kiểm tra `IsActive` khi đăng nhập/refresh được đặc tả tại FR-AUTH-002 (bước 7) và FR-AUTH-004 (bước 4).
>
> **[CR-2026-02 / MT-44, MT-52]** v1.1.0 chỉ có endpoint khóa/mở khóa theo `id`, trong khi route `/dashboard/users` (mục 5.1) cần hiển thị **danh sách** để Admin chọn người — không có API nào trả được danh sách đó, tức là giao diện không dựng được. Endpoint `GET /api/v1/users` được gộp vào **chính FR này** (không tách FR mới) vì nó chỉ phục vụ đúng một mục đích: tìm người để khóa/mở khóa. Ngoài ra bảng Chương 8.1 của v1.1.0 ghi Admin tự khóa mình trả **409** trong khi luồng thay thế A2 ở trên ghi **403** — v1.2.0 thống nhất **403**: yêu cầu hợp lệ về cú pháp và không xung đột trạng thái tài nguyên, chỉ là *người gọi không được phép thực hiện hành động này lên chính mình*.

#### FR-AUTH-009: Quản lý Phiên Đăng nhập của Chính mình

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-AUTH-009 |
| **Tên yêu cầu** | Xem và Thu hồi các Phiên Đăng nhập (Session Management) |
| **Nhóm chức năng** | Module Xác thực và Quản lý Người dùng (FR-AUTH) |
| **Tác nhân** | Người dùng đã đăng nhập (Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | C – Could Have *(không FR mức M/S nào phụ thuộc vào FR này; cơ chế Reuse Detection của FR-AUTH-004 đã tự phát hiện token bị đánh cắp — FR này bổ sung khả năng chủ động thu hồi)* |
| **Mô tả** | Mỗi refresh token còn hiệu lực tương ứng với **một phiên đăng nhập** trên một thiết bị/trình duyệt. Người dùng xem được danh sách phiên của mình (thời điểm tạo, IP tạo, thời điểm hết hạn, phiên nào là phiên hiện tại) và **tự thu hồi** một phiên cụ thể hoặc tất cả phiên — ví dụ khi quên đăng xuất ở máy công cộng hoặc nghi ngờ tài khoản bị lộ. |
| **Điều kiện tiên quyết** | 1. JWT hợp lệ. 2. Chỉ thao tác được trên refresh token **thuộc chính người gọi**. |
| **HTTP Method & Endpoint** | `GET /api/v1/auth/sessions` · `DELETE /api/v1/auth/sessions/{id}` · `POST /api/v1/auth/sessions/revoke-all` |
| **Kết quả mong đợi** | Danh sách `SessionDto { id, createdAt, createdByIp, expiresAt, isCurrent }` — **tuyệt đối không trả `TokenHash`**. Thu hồi: `RevokedAt = DateTime.UtcNow` cho phiên được chọn (hoặc mọi phiên). |
| **HTTP Status Code trả về** | `200 OK` – Danh sách. `204 No Content` – Thu hồi thành công (idempotent). `401 Unauthorized` – Thiếu/sai token. `404 Not Found` – Phiên không tồn tại **hoặc thuộc người khác**. |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/auth/sessions` kèm Bearer token.
2. Handler truy vấn `RefreshTokens` với `UserId == currentUserId AND RevokedAt IS NULL AND ExpiresAt > NOW()`, sắp theo `CreatedAt` giảm dần. `isCurrent` được xác định bằng claim **`sid`** trong access token — `sid` là `Id` của bản ghi `RefreshToken` được phát cùng cặp token (gán ở mọi lần phát token: đăng ký, đăng nhập, Google, refresh). Client không phải gửi thêm dữ liệu nào.
3. Client gửi `DELETE /api/v1/auth/sessions/{id}` → handler tìm token theo `id` **và** `UserId == currentUserId`; gán `RevokedAt`. Trả 204.
4. `POST /api/v1/auth/sessions/revoke-all` → thu hồi mọi phiên của người gọi (kể cả phiên hiện tại); client xóa phiên cục bộ và chuyển về trang đăng nhập.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Phiên thuộc người khác:** HTTP **404** (không phải 403) — không tiết lộ rằng `id` đó có tồn tại.
- **A2 – Phiên đã thu hồi trước đó:** HTTP 204 (idempotent).

> **[CR-2026-02 / MT-45]** Yêu cầu này phát sinh từ lộ trình phát triển (Buổi 7) nhưng v1.1.0 **không có FR nào** đỡ — nếu hiện thực thẳng thì hệ thống sẽ có 3 endpoint không truy vết được về yêu cầu nào, phá vỡ nguyên tắc "SRS là nguồn sự thật". FR được đặt riêng (không gộp vào FR-AUTH-005 Đăng xuất) vì phạm vi khác nhau: FR-AUTH-005 thu hồi **phiên đang dùng**, còn FR này quản lý **mọi phiên trên mọi thiết bị**. Mức **C** được chọn có chủ đích để tuân thủ quy tắc MoSCoW của mục 3: cắt FR này không làm hỏng bất kỳ chức năng Must Have nào. Trả **404 cho phiên của người khác** để endpoint không thể bị dùng để dò id token hợp lệ.

### 3.2. Module Quản lý Danh mục (FR-CAT)

Module quản lý danh mục (Category) phân loại công thức nấu ăn. Danh mục được tạo và duy trì bởi Admin; Author và Guest chỉ có quyền đọc. Mỗi danh mục có Slug duy nhất phục vụ URL thân thiện SEO.

> **[CR-2026 / MT-16, MT-17]** Danh mục được cache bằng **Redis (`IDistributedCache` qua `RedisCacheService`)** với khóa `categories:all`, **TTL 30 phút** theo bảng TTL chuẩn duy nhất tại NFR-PERF-003. `IMemoryCache` **không được dùng cho bất kỳ shared state nào** trong hệ thống: khi chạy nhiều API instance sau Nginx (NFR-SCALE-003), cache in-memory sẽ stale và không có cách invalidate xuyên instance.

#### FR-CAT-001: Xem Danh sách Danh mục

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-CAT-001 |
| **Tên yêu cầu** | Xem Danh sách Tất cả Danh mục |
| **Nhóm chức năng** | Module Quản lý Danh mục (FR-CAT) |
| **Tác nhân** | Tất cả (Guest / Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Trả về danh sách tất cả danh mục công thức hiện có trong hệ thống (`IsDeleted = false`), kèm số lượng công thức **đã xuất bản (Published)** trong mỗi danh mục. Kết quả được cache trên **Redis với khóa `categories:all`, TTL 30 phút (absolute expiration)** và sắp xếp theo `OrderIndex` tăng dần, sau đó theo `Name`. Dữ liệu hoàn toàn công khai, không phụ thuộc danh tính người gọi nên an toàn để cache dùng chung. |
| **Điều kiện tiên quyết** | 1. Ít nhất một danh mục tồn tại trong database (hoặc trả về mảng rỗng). 2. Không yêu cầu xác thực. |
| **HTTP Method & Endpoint** | `GET /api/v1/categories` |
| **Kết quả mong đợi** | Mảng `CategoryDto[]` với các field: `{ id, name, slug, description, imageUrl, orderIndex, recipeCount }`. Kết quả được serve từ cache khi có. |
| **HTTP Status Code trả về** | `200 OK` – Thành công (kể cả khi trống). |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/categories`.
2. `GetCategoriesQuery` (implements `ICacheable`) dispatch qua MediatR.
3. `CachingBehavior` kiểm tra **Redis** với key `"categories:all"`.
4. Cache hit: trả về dữ liệu từ cache.
5. Cache miss: query database (`IUnitOfWork.Categories.GetAllWithPublishedRecipeCount()`), map sang `CategoryDto[]`.
6. Lưu vào Redis với **TTL 30 phút (absolute expiration)**.
7. Trả về HTTP 200 OK với `CategoryDto[]`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Không có danh mục nào:** HTTP 200 OK với mảng rỗng `[]`.

#### FR-CAT-002: Xem Chi tiết Danh mục và Công thức

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-CAT-002 |
| **Tên yêu cầu** | Xem Chi tiết Danh mục và Danh sách Công thức thuộc Danh mục |
| **Nhóm chức năng** | Module Quản lý Danh mục (FR-CAT) |
| **Tác nhân** | Tất cả (Guest / Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Trả về thông tin chi tiết của một danh mục cụ thể (theo Slug) kèm danh sách phân trang các công thức đã xuất bản (Published) thuộc danh mục đó. **Đây là endpoint hoàn toàn công khai: kết quả chỉ chứa `Status == Published` cho mọi người gọi, không ngoại lệ** — kể cả Author và Admin. Nhờ vậy response không phụ thuộc danh tính và được cache dùng chung an toàn (TTL 2 phút, khóa `categories:detail:{slug}:{queryHash}`). Author/Admin muốn xem Draft/Archived của mình dùng `GET /recipes/mine` (FR-RCP-011). |
| **Điều kiện tiên quyết** | 1. Danh mục với slug tương ứng phải tồn tại và chưa bị soft delete. 2. **Không yêu cầu xác thực và cũng không đọc danh tính người gọi.** |
| **HTTP Method & Endpoint** | `GET /api/v1/categories/{slug}?page={n}&pageSize={n}&sortBy={field}&sortOrder={asc\|desc}` |
| **Kết quả mong đợi** | `{ category: CategoryDto, recipes: { items: RecipeSummaryDto[], totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage } }` |
| **HTTP Status Code trả về** | `200 OK` – Thành công. `400 Bad Request` – Tham số phân trang/sắp xếp không hợp lệ. `404 Not Found` – Slug không tồn tại. |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/categories/{slug}?page=1&pageSize=12&sortBy=publishedAt&sortOrder=desc`.
2. `GetCategoryBySlugQuery` dispatch qua MediatR.
3. `ValidationBehavior` kiểm tra `page >= 1`, `pageSize ∈ [1, 50]`, `sortBy` thuộc whitelist (mục 8, Quy ước Pagination), `sortOrder ∈ {asc, desc}`.
4. Handler tìm category theo slug: `_unitOfWork.Categories.GetBySlugAsync(slug)`.
5. Query recipes thuộc category với **`Status == Published`** (Global Query Filter đã loại `IsDeleted`).
6. Apply sorting theo `sortBy`/`sortOrder`, rồi pagination (OFFSET-based: `SKIP (page-1)*pageSize TAKE pageSize`).
7. Map sang `CategoryDetailDto` kèm `PagedResult<RecipeSummaryDto>`.
8. Trả về HTTP 200 OK.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Slug không tồn tại hoặc category đã soft delete:** HTTP 404 Not Found với RFC 7807 body (`CATEGORY_NOT_FOUND`).
- **A2 – Tham số query không hợp lệ:** HTTP 400 Bad Request.

> **[CR-2026 / MT-34, MT-41.1, MT-41.2]** v1.0.0 mô tả endpoint này là "không yêu cầu xác thực" nhưng lại đọc `currentUser` để bổ sung Draft — một response phụ thuộc danh tính nằm dưới khóa cache công khai. v1.1.0 áp dụng **quy tắc hiển thị dùng chung cho mọi endpoint danh sách công khai**: chỉ trả `Published`, không đọc danh tính, cache thoải mái.

#### FR-CAT-003: Tạo Danh mục Mới [Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-CAT-003 |
| **Tên yêu cầu** | Tạo Danh mục Công thức Mới |
| **Nhóm chức năng** | Module Quản lý Danh mục (FR-CAT) |
| **Tác nhân** | Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Admin tạo danh mục công thức mới. Slug được tự động sinh từ Name (slugify: chuyển sang chữ thường, bỏ dấu, thay khoảng trắng bằng "-"). Nếu Slug đã tồn tại, hệ thống thêm suffix số (e.g., `"mon-chinh-2"`). **Name phải là duy nhất** — hệ thống kiểm tra ở tầng Application trước khi ghi, thay vì để ràng buộc UNIQUE của PostgreSQL ném `DbUpdateException` thành 500. Sau khi tạo, cache `categories:all` trên Redis bị invalidate. |
| **Điều kiện tiên quyết** | 1. Người dùng đang đăng nhập với role Admin. 2. Name chưa tồn tại trong database. |
| **HTTP Method & Endpoint** | `POST /api/v1/categories` |
| **Kết quả mong đợi** | Danh mục mới được tạo trong database. Cache danh mục bị xóa trên Redis. Location header trỏ đến `/api/v1/categories/{newSlug}`. |
| **HTTP Status Code trả về** | `201 Created` – Tạo thành công. `400 Bad Request` – Dữ liệu không hợp lệ. `403 Forbidden` – Không có quyền Admin. `409 Conflict` – Name đã tồn tại (`CATEGORY_NAME_EXISTS`). |

**Luồng chính (Happy Path):**

1. Admin gửi `POST /api/v1/categories` với `Authorization: Bearer {adminJwt}` và body: `{ "name": "...", "description"?: "...", "imageUrl"?: "...", "orderIndex"?: 0 }`.
2. `RequireAuthorization("AdminPolicy")` middleware kiểm tra role.
3. `CreateCategoryCommand` dispatch qua MediatR.
4. `ValidationBehavior`: `name` **2–100 ký tự** (khớp `varchar(100)` tại mục 7.6 và Bảng 7.9), không chứa HTML; `description` ≤ 2000; `imageUrl` ≤ 500 ký tự.
5. **Kiểm tra Name trùng:** `_unitOfWork.Categories.ExistsByNameAsync(name)` → nếu đã tồn tại, throw `ConflictException` (nhánh A3).
6. `SlugHelper.Generate(name)` tạo slug; slug không được trùng **danh sách slug dành riêng chung** tại NFR-SEO-004 (một nguồn duy nhất cho cả Recipe và Category).
7. Kiểm tra slug chưa tồn tại. Nếu trùng, thêm "-2", "-3",... cho đến khi unique.
8. `Category.Create(name, slug, description, imageUrl, orderIndex)` tạo entity.
9. `_unitOfWork.Categories.AddAsync(entity)`.
10. `_unitOfWork.SaveChangesAsync()`.
11. `CacheInvalidationBehavior` xóa khóa Redis `categories:all`.
12. Trả về HTTP 201 Created với `CategoryDto` và Location header.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Thiếu role Admin:** HTTP 403 Forbidden.
- **A2 – Dữ liệu không hợp lệ:** HTTP 400 Bad Request.
- **A3 – Name đã tồn tại:** HTTP **409 Conflict** với `CATEGORY_NAME_EXISTS`.
- **A4 – Đua (race) giữa hai request cùng tên:** PostgreSQL trả lỗi UNIQUE `23505`; `GlobalExceptionMiddleware` dịch mã `23505` trên cột `Name` thành **409 `CATEGORY_NAME_EXISTS`** (lớp phòng vệ thứ hai, không bao giờ để lọt thành 500).

> **[CR-2026 / MT-36]** v1.0.0 khai báo status 409 và điều kiện tiên quyết "Name chưa tồn tại" nhưng luồng chính không có bước nào sinh ra 409 — tạo trùng tên sẽ rơi xuống DB và trả 500. Bước 5 và nhánh A4 đóng lỗ hổng này.

#### FR-CAT-004: Cập nhật Danh mục [Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-CAT-004 |
| **Tên yêu cầu** | Cập nhật Thông tin Danh mục |
| **Nhóm chức năng** | Module Quản lý Danh mục (FR-CAT) |
| **Tác nhân** | Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Admin cập nhật Name, Description, ImageUrl và/hoặc OrderIndex của danh mục. Slug KHÔNG thay đổi khi đổi tên (để tránh broken links). Giống FR-CAT-003, **Name mới phải kiểm tra trùng trước khi ghi**. Sau khi cập nhật, cache `categories:all` bị invalidate. |
| **Điều kiện tiên quyết** | 1. Admin đang đăng nhập. 2. Danh mục với ID tương ứng tồn tại. 3. Name mới chưa được dùng bởi danh mục khác. |
| **HTTP Method & Endpoint** | `PUT /api/v1/categories/{id:guid}` |
| **Kết quả mong đợi** | Thông tin danh mục được cập nhật. Cache invalidated. |
| **HTTP Status Code trả về** | `200 OK` – Cập nhật thành công. `400 Bad Request` – Dữ liệu không hợp lệ. `403 Forbidden`. `404 Not Found`. `409 Conflict` – Name đã thuộc về danh mục khác (`CATEGORY_NAME_EXISTS`). |

**Luồng chính (Happy Path):**

1. Admin gửi `PUT /api/v1/categories/{id}` với body: `{ "name": "...", "description"?: "...", "imageUrl"?: "...", "orderIndex"?: 0 }`.
2. Kiểm tra role Admin (`"AdminPolicy"`).
3. `UpdateCategoryCommand` dispatch qua MediatR; validator áp dụng cùng giới hạn độ dài như FR-CAT-003.
4. Tìm category theo ID → 404 nếu không có.
5. **Kiểm tra `ExistsByNameAsync(name, excludeId: id)`** → nếu trùng, throw `ConflictException` → 409.
6. Cập nhật các field, `SaveChangesAsync()`, invalidate khóa Redis `categories:all`.
7. Trả về HTTP 200 OK với `CategoryDto` đã cập nhật.

**Luồng thay thế / Ngoại lệ:**

- **A1 – ID không tồn tại:** HTTP 404 (`CATEGORY_NOT_FOUND`).
- **A2 – Thiếu role Admin:** HTTP 403.
- **A3 – Name đã tồn tại ở danh mục khác:** HTTP 409 (`CATEGORY_NAME_EXISTS`); lỗi `23505` từ DB cũng được dịch thành 409.
- **A4 – Dữ liệu không hợp lệ:** HTTP 400.

#### FR-CAT-005: Xóa Danh mục [Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-CAT-005 |
| **Tên yêu cầu** | Xóa Danh mục |
| **Nhóm chức năng** | Module Quản lý Danh mục (FR-CAT) |
| **Tác nhân** | Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | S – Should Have |
| **Mô tả** | Admin xóa một danh mục theo cơ chế **Soft Delete** (`IsDeleted = true`) — thống nhất với chiến lược xóa toàn hệ thống (mục 7.1, NFR-REL-003). Quy tắc nghiệp vụ: KHÔNG được xóa danh mục còn chứa công thức (dù là Published, Draft hay Archived, tính theo các recipe chưa bị soft delete). Admin phải chuyển tất cả công thức sang danh mục khác trước khi xóa. |
| **Điều kiện tiên quyết** | 1. Admin đang đăng nhập. 2. Danh mục tồn tại và không còn công thức nào tham chiếu. |
| **HTTP Method & Endpoint** | `DELETE /api/v1/categories/{id:guid}` |
| **Kết quả mong đợi** | Danh mục được đánh dấu `IsDeleted = true` và biến mất khỏi mọi truy vấn qua Global Query Filter. HTTP 204 được trả về. |
| **HTTP Status Code trả về** | `204 No Content` – Xóa thành công. `403 Forbidden`. `404 Not Found`. `409 Conflict` – Danh mục còn recipe (`CATEGORY_DELETE_HAS_RECIPES`). |

**Luồng chính (Happy Path):**

1. Admin gửi `DELETE /api/v1/categories/{id}`.
2. Kiểm tra role Admin.
3. `DeleteCategoryCommand` dispatch.
4. Đếm số recipe trong category (`IsDeleted = false`): nếu > 0 → Throw `ConflictException("Danh mục còn chứa {count} công thức.")`.
5. Đánh dấu `category.IsDeleted = true` (soft delete), `SaveChangesAsync()`, invalidate khóa Redis `categories:all`.
6. Trả về HTTP 204 No Content.

> **[CR-2026 / MT-05, MT-20.11]** FR-CAT-005 dùng **soft delete**, khớp với Chương 8 và với `BaseEntity`. Ràng buộc FK `Recipes.CategoryId → Categories.Id` giữ `ON DELETE RESTRICT` chỉ như lớp bảo vệ cuối cùng cho trường hợp xóa vật lý bởi job dọn dữ liệu (FR-JOB-003).

**Luồng thay thế / Ngoại lệ:**

- **A1 – Danh mục có recipe:** HTTP 409 Conflict với thông báo số lượng recipe.
- **A2 – ID không tồn tại:** HTTP 404.

### 3.3. Module Quản lý Công thức Nấu ăn (FR-RCP)

Module cốt lõi của hệ thống. Recipe là aggregate root chứa các child entity: `RecipeStep`, `RecipeIngredient`, `RecipeImage` và Owned Entity `RecipeNutrition`. Tất cả mutation (Create/Update/Delete) đi qua UnitOfWork để đảm bảo tính nhất quán transaction. Concurrency được xử lý qua RowVersion (Timestamp) để phát hiện lost update khi hai Author cùng sửa một recipe → trả **409 Conflict**.

#### Nguyên tắc cách ly Public / Private (bắt buộc — CR-2026 / MT-34)

Đây là quyết định kiến trúc nền tảng của module, quyết định hình dạng mọi endpoint danh sách:

| Nhóm endpoint | Dữ liệu trả về | Đọc danh tính? | Caching |
| --- | --- | --- | --- |
| **Công khai** — `GET /recipes`, `GET /recipes/{slug}`, `GET /recipes/search`, `GET /categories/{slug}` | **Chỉ `Status == Published`**, cho mọi người gọi không ngoại lệ (kể cả Admin) | Không | Redis cache-aside dùng chung, TTL theo NFR-PERF-003 |
| **Cá nhân** — `GET /recipes/mine` (FR-RCP-011) | Draft / Published / Archived **của chính người gọi**; Admin có thể truyền `?authorId=` | Có (bắt buộc JWT) | **Cấm cache.** Bắt buộc header `Cache-Control: no-store` |

Ba quy tắc thực thi kèm theo:

1. **Không bao giờ đặt dữ liệu phụ thuộc danh tính vào cache dùng chung khóa công khai** (nguyên tắc được ghi vào NFR-SEC-006).
2. Cấu hình cache **bỏ qua mọi request có header `Authorization`** — lớp phòng vệ thứ hai phòng khi một endpoint mới quên áp dụng quy tắc 1.
3. Recipe không ở trạng thái Published, khi truy cập qua endpoint công khai, trả **404 Not Found** (không trả 403) để không tiết lộ sự tồn tại của bản nháp người khác.

> **Lý do:** v1.0.0 đặc tả Output Cache với khóa `{path}?{queryString}` cho những endpoint mà nội dung response lại thay đổi theo vai trò người gọi. Hệ quả: một request của Admin nạp Draft của mọi Author vào cache, và mọi Guest gọi cùng URL trong 15 phút sau đó đọc được nguyên response đó. Đây là lỗi rò rỉ dữ liệu giữa các tài khoản, vi phạm NFR-SEC-006 và vô hiệu hóa Resource-Based Authorization.

#### FR-RCP-001: Xem Danh sách Công thức (Paginated + Filtered + Sorted)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-001 |
| **Tên yêu cầu** | Xem Danh sách Công thức Nấu ăn với Phân trang, Lọc và Sắp xếp |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tất cả (Guest / Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Trả về danh sách phân trang các công thức **đã xuất bản**. Theo nguyên tắc cách ly Public/Private ở đầu mục 3.3, endpoint này **chỉ trả `Status == Published` cho mọi người gọi** và **không đọc danh tính** — nhờ vậy response thuần công khai và được cache dùng chung an toàn. Hỗ trợ lọc theo `categoryId`, `difficulty`, `maxCookTime`, `maxPrepTime`, `minServings`; sắp xếp qua cặp tham số `sortBy` + `sortOrder`. Kết quả được cache trên **Redis (cache-aside) với khóa `recipes:list:{queryHash}`, TTL 2 phút**. |
| **Điều kiện tiên quyết** | 1. Không yêu cầu xác thực. 2. `page >= 1`, `pageSize ∈ [1, 50]` (mặc định 12). 3. `sortBy` thuộc whitelist, `sortOrder ∈ {asc, desc}`. |
| **HTTP Method & Endpoint** | `GET /api/v1/recipes?page={n}&pageSize={n}&categoryId={guid}&difficulty={level}&maxCookTime={min}&maxPrepTime={min}&minServings={n}&sortBy={field}&sortOrder={asc\|desc}` |
| **Kết quả mong đợi** | `PagedResult<RecipeSummaryDto>`: `{ items[], totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`. Mỗi `RecipeSummaryDto` chứa **cả `id` lẫn `slug`** (đọc theo slug, ghi theo id — xem quy ước Chương 8). |
| **HTTP Status Code trả về** | `200 OK` – Thành công (kể cả items rỗng). `400 Bad Request` – Tham số không hợp lệ (bao gồm `sortBy` ngoài whitelist). |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/recipes?page=1&pageSize=12&categoryId={guid}&difficulty=Easy&maxCookTime=30&sortBy=publishedAt&sortOrder=desc`.
2. `GetRecipesQuery` (implements `ICacheable`) dispatch qua MediatR.
3. `ValidationBehavior` kiểm tra: `page >= 1`; `pageSize ∈ [1,50]`; **`sortBy ∈ { createdAt, publishedAt, title, cookTime, prepTime }`**; `sortOrder ∈ { asc, desc }`; `difficulty ∈ { Easy, Medium, Hard, Expert }`. Giá trị ngoài whitelist → **400** (không im lặng bỏ qua — đây cũng là rào chắn chống SQL injection qua tên cột).
4. `CachingBehavior` tra Redis theo khóa `recipes:list:{sha256(normalizedQueryString)}`; cache hit → trả ngay.
5. Handler xây dựng `IQueryable` với filters từ query params và **cố định điều kiện `Status == Published`** (Global Query Filter đã loại `IsDeleted`).
6. Apply sorting: ánh xạ `sortBy` → biểu thức cột qua một dictionary whitelist ở tầng Application (không ghép chuỗi SQL), `sortOrder` quyết định ASC/DESC. Mặc định: `sortBy=createdAt&sortOrder=desc`.
7. COUNT total trước khi pagination.
8. Apply OFFSET-LIMIT pagination (`SKIP (page-1)*pageSize TAKE pageSize`).
9. Map sang `PagedResult<RecipeSummaryDto>`, lưu vào Redis TTL 2 phút.
10. Trả về HTTP 200 OK.

**Luồng thay thế / Ngoại lệ:**

- **A1 – `page`/`pageSize` không hợp lệ:** HTTP 400 Bad Request.
- **A2 – `sortBy` hoặc `sortOrder` ngoài whitelist:** HTTP 400 Bad Request với `VALIDATION_ERROR`.
- **A3 – `categoryId` không tồn tại:** HTTP 200 với items rỗng (không throw 404).

> **[CR-2026 / MT-01]** Quy ước sắp xếp một tham số `sort=-field` bị loại bỏ khỏi toàn tài liệu. Toàn hệ thống dùng **`sortBy` + `sortOrder`**, dễ validate độc lập bằng hai rule FluentValidation, dễ bind vào record C# và ánh xạ 1-1 với cặp dropdown "cột"/"chiều" ở Frontend.
>
> **[CR-2026 / MT-23]** Trang chủ lấy "công thức nổi bật" bằng chính endpoint này với `?sortBy=publishedAt&sortOrder=desc&pageSize=8` — tận dụng index `IDX_Recipe_PublishedAt` sẵn có, không thêm cột `IsFeatured`/`ViewCount`.

#### FR-RCP-002: Xem Chi tiết Công thức

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-002 |
| **Tên yêu cầu** | Xem Chi tiết Công thức Nấu ăn |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tất cả (Guest / Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Trả về toàn bộ thông tin chi tiết của một công thức **đã xuất bản**, bao gồm: thông tin cơ bản, danh sách nguyên liệu (`RecipeIngredient[]`) sắp xếp theo **`OrderIndex`**, các bước thực hiện (`RecipeStep[]`) sắp xếp theo `StepNumber`, ảnh minh họa (`RecipeImage[]`), thông tin dinh dưỡng (`RecipeNutrition`), thông tin danh mục và tác giả. Theo nguyên tắc cách ly Public/Private, endpoint này **chỉ phục vụ `Status == Published`**; Draft/Archived xem qua `GET /recipes/mine` (FR-RCP-011). Kết quả được cache trên **Redis (cache-aside) với khóa `recipe:{slug}`, TTL 5 phút**. |
| **Điều kiện tiên quyết** | 1. Recipe với slug tương ứng tồn tại, chưa bị soft delete và **đang ở trạng thái Published**. 2. Không yêu cầu xác thực và không đọc danh tính người gọi. |
| **HTTP Method & Endpoint** | `GET /api/v1/recipes/{slug}` |
| **Kết quả mong đợi** | `RecipeDetailDto` đầy đủ gồm tất cả nested data (steps, ingredients, images, nutrition, category, author). |
| **HTTP Status Code trả về** | `200 OK` – Thành công. `404 Not Found` – Slug không tồn tại, đã soft delete, **hoặc chưa/không còn ở trạng thái Published** (`RECIPE_NOT_FOUND`). |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/recipes/{slug}`.
2. `GetRecipeBySlugQuery` (implements `ICacheable`) dispatch qua MediatR.
3. `CachingBehavior` tra Redis khóa `recipe:{slug}`; cache hit → trả ngay.
4. Handler query Recipe với Eager Loading: `Include(Steps).Include(Ingredients).Include(Images).Include(Category).Include(Author)` + Owned `Nutrition` (đi kèm bảng `Recipes`, không cần Include riêng), **kèm điều kiện `Status == Published`**.
5. Kiểm tra null → `NotFoundException` nếu không tìm thấy.
6. Map sang `RecipeDetailDto` (bao gồm tất cả nested collections), lưu Redis TTL 5 phút.
7. Trả về HTTP 200 OK.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Slug không tồn tại:** HTTP 404 Not Found (`RECIPE_NOT_FOUND`).
- **A2 – Recipe tồn tại nhưng ở trạng thái Draft/Archived:** HTTP **404 Not Found** với cùng thông điệp như A1 — không tiết lộ sự tồn tại của bản nháp. *(v1.0.0 trả 403; điều này vừa lộ thông tin vừa mâu thuẫn với việc endpoint có cache dùng chung.)*

> **[CR-2026 / MT-34, MT-17]** TTL giảm từ 60 phút (Output Cache in-memory) xuống **5 phút (Redis cache-aside)** theo bảng TTL chuẩn NFR-PERF-003. Cơ chế cache duy nhất toàn hệ thống là Redis cache-aside qua `CachingBehavior`/`CacheInvalidationBehavior`; **Output Cache của .NET không được sử dụng** vì mặc định lưu in-memory (stale khi multi-instance) và khóa mặc định không phân biệt danh tính.

#### FR-RCP-003: Tạo Công thức Nấu ăn Mới [Author/Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-003 |
| **Tên yêu cầu** | Tạo Công thức Nấu ăn Mới |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Author hoặc Admin tạo mới một công thức nấu ăn. Trạng thái ban đầu luôn là Draft (chưa công khai). Slug được tự động sinh từ Title. Steps và Ingredients có thể được tạo cùng lúc (trong cùng request) hoặc thêm riêng lẻ sau qua FR-RCP-009/010. **`nutrition` là Owned Entity** — chỉ tạo/sửa **cùng recipe** qua `POST /recipes` và `PUT /recipes/{id}`; hệ thống **không có endpoint riêng cho nutrition**. |
| **Điều kiện tiên quyết** | 1. Người dùng đang đăng nhập với role Author hoặc Admin. 2. CategoryId tham chiếu đến danh mục đã tồn tại và chưa bị soft delete. |
| **HTTP Method & Endpoint** | `POST /api/v1/recipes` |
| **Kết quả mong đợi** | Recipe mới được tạo với `Status = Draft`, `PublishedAt = NULL`, Slug được sinh tự động. Các khóa cache `recipes:list:*` bị invalidate. |
| **HTTP Status Code trả về** | `201 Created` – Tạo thành công. `400 Bad Request` – Dữ liệu không hợp lệ (bao gồm CategoryId không tồn tại). `401`/`403` – Chưa đăng nhập / Không có quyền. `409 Conflict` – Slug đã tồn tại sau khi hết phương án sinh suffix (`RECIPE_SLUG_EXISTS`). |

**Luồng chính (Happy Path):**

1. Author gửi `POST /api/v1/recipes` với body: `{ title, description, categoryId, prepTime, cookTime, servings, difficulty, instructions?, nutrition?: { calories?, protein?, carbohydrates?, fat?, fiber?, sodium? }, steps?: [{ title, description, timerMinutes?, imageUrl? }], ingredients?: [{ name, quantity?, quantityText?, unit?, notes?, orderIndex? }] }`.
2. Kiểm tra xác thực (`RequireAuthorization("AuthorPolicy")`).
3. `CreateRecipeCommand` dispatch.
4. `ValidationBehavior` theo Bảng Giới hạn Dữ liệu Chuẩn (mục 7.9): `title` 5–200 ký tự; `description` 20–2000 ký tự; `prepTime > 0`; `cookTime >= 0`; `servings > 0`; `difficulty ∈ {Easy, Medium, Hard, Expert}`; `categoryId` là Guid hợp lệ và tồn tại.
5. `SlugHelper.Generate(title)`, kiểm tra slug unique **trong phạm vi các recipe chưa bị soft delete** (partial unique index tại mục 7.2); nếu trùng thì thêm suffix số. Slug **không được trùng danh sách slug dành riêng** tại NFR-SEO-004 (`search`, `mine`, `sitemap`, `new`, `edit`) — nếu trùng thì thêm suffix `-2`.
6. `Recipe.Create(title, description, categoryId, authorId, prepTime, cookTime, servings, difficulty)`.
7. Nếu có steps: thêm từng `RecipeStep.Create()` vào `recipe.Steps`; **`StepNumber` do server đánh số tuần tự 1..n theo thứ tự mảng gửi lên — client không truyền `stepNumber`**.
8. Nếu có ingredients: thêm từng `RecipeIngredient.Create()` vào `recipe.Ingredients`; `OrderIndex` mặc định theo thứ tự mảng nếu client không gửi.
9. Nếu có nutrition: `recipe.SetNutrition(calories, protein, carbohydrates, fat, fiber, sodium)` — ghi vào các cột `Nutrition_*` ngay trong bảng `Recipes`, cùng một transaction.
10. `_unitOfWork.Recipes.AddAsync(recipe)`, `SaveChangesAsync()`.
11. `CacheInvalidationBehavior` xóa các khóa Redis theo prefix `recipes:list:` và `categories:` (vì `recipeCount` thay đổi).
12. Trả về HTTP 201 Created với `RecipeDto` và Location header `/api/v1/recipes/{slug}`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Không có quyền Author/Admin:** HTTP 401/403.
- **A2 – CategoryId không tồn tại:** HTTP **400 Bad Request** với lỗi field-level "Category không hợp lệ."
- **A3 – Slug đã tồn tại và không sinh được biến thể unique:** HTTP 409 Conflict (`RECIPE_SLUG_EXISTS`).
- **A4 – Dữ liệu không hợp lệ:** HTTP 400 Bad Request.

> **[CR-2026 / MT-02]** `RecipeNutrition` là Owned Entity (các cột `Nutrition_*` nằm trong bảng `Recipes`, mục 7.2.1), nên **không có vòng đời độc lập**: không repository riêng, không DTO/command riêng, không endpoint `/recipes/{id}/nutrition`. Nếu Frontend dùng wizard nhiều bước thì gom state ở client rồi submit một lần.
>
> **[CR-2026 / MT-20.6]** Tên trường thời gian thống nhất là **`prepTime` / `cookTime`** (khớp Chương 7 và 8), không dùng `prepTimeMinutes`/`cookTimeMinutes`.

#### FR-RCP-004: Cập nhật Công thức [Author-Owner/Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-004 |
| **Tên yêu cầu** | Cập nhật Thông tin Công thức Nấu ăn |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu (Author – Owner) / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Cập nhật thông tin của một công thức. Resource-Based Authorization được áp dụng: chỉ Author sở hữu recipe (`AuthorId == currentUserId`) hoặc Admin được phép. Concurrency control qua RowVersion (ETag pattern): client phải gửi RowVersion hiện tại trong `If-Match` header; nếu mismatch → **409 Conflict**. **Slug chỉ được sinh lại khi recipe còn ở trạng thái Draft**; sau lần Publish đầu tiên slug bị khóa vĩnh viễn. `nutrition` được cập nhật kèm trong cùng request (Owned Entity). |
| **Điều kiện tiên quyết** | 1. Author/Admin đang đăng nhập. 2. Recipe với ID tương ứng tồn tại và chưa bị soft delete. 3. Client cung cấp RowVersion hợp lệ trong `If-Match` header (hoặc trong request body). |
| **HTTP Method & Endpoint** | `PUT /api/v1/recipes/{id:guid}` |
| **Kết quả mong đợi** | Recipe được cập nhật, cache bị invalidate, trả về `RecipeDto` mới nhất. |
| **HTTP Status Code trả về** | `200 OK`. `400 Bad Request` – Dữ liệu không hợp lệ. `403 Forbidden` – Không phải owner. `404 Not Found`. `409 Conflict` – Xung đột đồng thời (`RECIPE_CONCURRENCY_CONFLICT`) hoặc slug trùng (`RECIPE_SLUG_EXISTS`). |

**Luồng chính (Happy Path):**

1. Author gửi `PUT /api/v1/recipes/{id}` với header `If-Match: {rowVersion}` và body: `{ title?, description?, categoryId?, prepTime?, cookTime?, servings?, difficulty?, instructions?, nutrition? }`.
2. Kiểm tra xác thực.
3. `UpdateRecipeCommand` dispatch; `ValidationBehavior` áp dụng cùng bộ giới hạn như FR-RCP-003.
4. Lấy recipe từ database theo ID (bỏ qua các bản ghi `IsDeleted = true`).
5. `IAuthorizationService.AuthorizeAsync(user, recipe, Operations.Update)` – kiểm tra resource-based auth.
6. **Nếu Title thay đổi VÀ `recipe.Status == Draft`:** sinh lại slug qua `SlugHelper.Generate(newTitle)` và kiểm tra unique. **Nếu `Status != Draft`: giữ nguyên slug**, bỏ qua bước sinh lại (không lỗi).
7. Update các field của recipe entity qua domain method `recipe.Update(...)`.
8. Cập nhật Nutrition nếu body có `nutrition` (ghi đè các cột `Nutrition_*`).
9. `SaveChangesAsync()` – nếu RowVersion mismatch tại đây, EF Core ném `DbUpdateConcurrencyException` → `ConcurrencyException` → HTTP **409**.
10. Invalidate cache Redis: xóa khóa `recipe:{slug}` (cả slug cũ lẫn slug mới nếu vừa đổi) và xóa theo prefix `recipes:list:`.
11. Trả về HTTP 200 OK với `RecipeDto` đã cập nhật (kèm `rowVersion` mới để client dùng cho lần sửa kế tiếp).

**Luồng thay thế / Ngoại lệ:**

- **A1 – Không phải owner (Author khác):** HTTP 403 Forbidden (`RECIPE_FORBIDDEN`).
- **A2 – Concurrency conflict (RowVersion mismatch):** HTTP **409 Conflict** với `RECIPE_CONCURRENCY_CONFLICT` – "Dữ liệu đã bị thay đổi bởi người dùng khác, vui lòng tải lại."
- **A3 – ID không tồn tại hoặc đã soft delete:** HTTP 404 (`RECIPE_NOT_FOUND`).
- **A4 – Slug mới trùng với recipe khác:** HTTP 409 (`RECIPE_SLUG_EXISTS`).
- **A5 – Dữ liệu không hợp lệ:** HTTP 400 Bad Request.

> **[CR-2026 / MT-09]** Optimistic Concurrency dùng **409**, không phải 422 — 409 mới đúng ngữ nghĩa "yêu cầu xung đột với trạng thái hiện tại của tài nguyên" và là thông lệ của mô hình ETag/If-Match. Frontend phân biệt 409 do concurrency với 409 do trùng unique qua trường `type` (Phụ lục B).
>
> **[CR-2026 / MT-27]** Slug khóa sau Publish, nên hệ thống **không cần bảng lịch sử slug và không cần 301 redirect** (xem NFR-SEO-004).

#### FR-RCP-005: Xuất bản / Hủy Xuất bản Công thức

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-005 |
| **Tên yêu cầu** | Xuất bản (Publish) / Hủy Xuất bản (Unpublish) Công thức |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu (Author – Owner) / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Thay đổi trạng thái công thức: Draft → Published (xuất bản) hoặc Published → Draft (hủy xuất bản). Business rule: **KHÔNG thể publish nếu recipe không có đồng thời ít nhất 1 bước thực hiện (`Steps.Count > 0`) VÀ ít nhất 1 nguyên liệu (`Ingredients.Count > 0`)**. Khi publish, Recipe bắt đầu xuất hiện trong danh sách công khai và trong kết quả tìm kiếm (vì các truy vấn đó chỉ lấy `Status == Published`). Ở **lần publish đầu tiên**, hệ thống gán `PublishedAt = DateTime.UtcNow`; các lần publish sau **không ghi đè** giá trị này. |
| **Điều kiện tiên quyết** | 1. Recipe tồn tại, người dùng là owner hoặc Admin. 2. Để publish: recipe phải có ≥ 1 RecipeStep **và** ≥ 1 RecipeIngredient. 3. Chuyển trạng thái phải hợp lệ theo máy trạng thái bên dưới (publish chỉ từ Draft; unpublish chỉ từ Published). |
| **HTTP Method & Endpoint** | `PATCH /api/v1/recipes/{id:guid}/publish` \| `PATCH /api/v1/recipes/{id:guid}/unpublish` |
| **Kết quả mong đợi** | Status recipe được thay đổi thành Published hoặc Draft; `PublishedAt` được gán ở lần publish đầu tiên. Cache bị invalidate. |
| **HTTP Status Code trả về** | `200 OK` – Thành công. `400 Bad Request` – Thiếu step hoặc ingredient (`RECIPE_PUBLISH_INCOMPLETE`). `403 Forbidden`. `404 Not Found`. `409 Conflict` – Chuyển trạng thái không hợp lệ (`RECIPE_INVALID_STATE_TRANSITION`). |

**Luồng chính (Happy Path):**

1. Author gửi `PATCH /api/v1/recipes/{id}/publish` (để xuất bản) hoặc `PATCH /api/v1/recipes/{id}/unpublish`.
2. `PublishRecipeCommand` dispatch với `isPublish = true/false`.
3. Kiểm tra resource-based authorization.
4. Gọi domain method: `recipe.Publish()` hoặc `recipe.Unpublish()`.
5. `recipe.Publish()` kiểm tra bất biến nghiệp vụ: `Steps.Count == 0 || Ingredients.Count == 0` → Throw `DomainException` với code `RECIPE_PUBLISH_INCOMPLETE`; `Status == Archived` → Throw `InvalidStateTransitionException`.
6. Set `Status = Published/Draft`, `UpdatedAt = DateTime.UtcNow`; **nếu `PublishedAt == null` thì gán `PublishedAt = DateTime.UtcNow`** (chỉ lần đầu).
7. `SaveChangesAsync()`, invalidate khóa Redis `recipe:{slug}` và prefix `recipes:list:`, `search:`.
8. Trả về HTTP 200 OK với `RecipeDto`.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Recipe thiếu step hoặc ingredient:** HTTP **400 Bad Request** với `RECIPE_PUBLISH_INCOMPLETE` và thông điệp chỉ rõ thiếu phần nào.
- **A2 – Recipe đã ở đúng trạng thái mong muốn (publish một recipe Published):** Idempotent, trả về HTTP 200 OK, **không** ghi đè `PublishedAt`.
- **A3 – Chuyển trạng thái không hợp lệ (ví dụ publish một recipe Archived):** HTTP **409 Conflict** với `RECIPE_INVALID_STATE_TRANSITION`.

#### Máy trạng thái `RecipeStatus` (chuẩn hóa — CR-2026 / MT-35)

```text
                    publish                   archive
     ┌────────┐ ───────────────► ┌───────────┐ ─────────► ┌──────────┐
     │ Draft  │                  │ Published │            │ Archived │
     │(mặc    │ ◄─────────────── │           │            │          │
     │ định)  │    unpublish     └───────────┘            └──────────┘
     └────────┘                                                │
         ▲  │                     archive                      │
         │  └────────────────────────────────────────────────►─┘
         │                                                     │
         └──────────────────── unarchive ──────────────────────┘
```

| Từ | Thao tác | Endpoint | Đến | Ghi chú |
| --- | --- | --- | --- | --- |
| (mới tạo) | create | `POST /recipes` | **Draft** | `PublishedAt = NULL` |
| Draft | publish | `PATCH /recipes/{id}/publish` | **Published** | Yêu cầu ≥1 step và ≥1 ingredient; gán `PublishedAt` nếu đang NULL |
| Published | unpublish | `PATCH /recipes/{id}/unpublish` | **Draft** | Giữ nguyên `PublishedAt` |
| Draft **hoặc** Published | archive | `PATCH /recipes/{id}/archive` | **Archived** | Archive được từ cả hai trạng thái |
| Archived | unarchive | `PATCH /recipes/{id}/unarchive` | **Draft** | Buộc rà soát lại nội dung trước khi công khai lần nữa |
| Bất kỳ | delete | `DELETE /recipes/{id}` | *(soft deleted)* | `IsDeleted = true`, Status giữ nguyên |

Mọi chuyển trạng thái nằm ngoài bảng trên đều bị Domain từ chối và trả **409 Conflict** với `RECIPE_INVALID_STATE_TRANSITION`.

> **[CR-2026 / MT-06, MT-35, MT-41.3]** Ba sửa đổi quan trọng: (1) điều kiện publish bổ sung `Ingredients.Count > 0` — một công thức không nguyên liệu vừa vô nghĩa về nghiệp vụ vừa làm trượt `recipeIngredient[]` của JSON-LD (NFR-SEO-001); (2) `PublishedAt` thực sự được gán, nhờ đó index `IDX_Recipe_PublishedAt`, `datePublished` trong JSON-LD và cách sắp xếp trang chủ (MT-23) mới hoạt động; (3) mô tả "được đưa vào index tìm kiếm" được sửa cho đúng cơ chế — `SearchVector` là generated column phụ thuộc Title/Description, **không** phụ thuộc Status; việc lọc Published nằm ở câu truy vấn.

#### FR-RCP-006: Lưu trữ Công thức (Archive)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-006 |
| **Tên yêu cầu** | Lưu trữ Công thức (Archive / Unarchive) |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | **M – Must Have** *(nâng từ S: FR-RCP-001/002 mức M và NFR-SEO-002 đều đặc tả hành vi cho trạng thái Archived — cắt FR này sẽ biến các nhánh xử lý đó thành mã chết)* |
| **Mô tả** | Chuyển Recipe sang trạng thái Archived, **và đưa trở lại (Unarchive)**. Recipe Archived không hiển thị trong danh sách công khai nhưng không bị xóa khỏi database (soft hide). Archive được **từ cả Draft lẫn Published**; Unarchive luôn đưa recipe về **Draft**, buộc tác giả rà soát lại nội dung trước khi công khai lần nữa. Nhờ vậy vòng đời trạng thái đóng kín — không còn "hố đen" Archived như v1.0.0. |
| **Điều kiện tiên quyết** | 1. Recipe tồn tại, người dùng là owner hoặc Admin. 2. Archive: `Status ∈ {Draft, Published}`. 3. Unarchive: `Status == Archived`. |
| **HTTP Method & Endpoint** | `PATCH /api/v1/recipes/{id:guid}/archive` \| `PATCH /api/v1/recipes/{id:guid}/unarchive` |
| **Kết quả mong đợi** | Archive: `Status = Archived`, recipe biến mất khỏi mọi endpoint công khai. Unarchive: `Status = Draft`. `PublishedAt` **không** bị xóa trong cả hai thao tác. |
| **HTTP Status Code trả về** | `200 OK`. `403 Forbidden`. `404 Not Found`. `409 Conflict` – Chuyển trạng thái không hợp lệ (`RECIPE_INVALID_STATE_TRANSITION`). |

**Luồng chính (Happy Path):**

*Archive:*

1. Author gửi `PATCH /api/v1/recipes/{id}/archive`.
2. Kiểm tra resource-based authorization.
3. `recipe.Archive()` — Domain kiểm tra `Status ∈ {Draft, Published}` → `Status = Archived`, `UpdatedAt = DateTime.UtcNow`.
4. `SaveChangesAsync()`, invalidate khóa Redis `recipe:{slug}` và prefix `recipes:list:`, `search:`.
5. HTTP 200 OK với `RecipeDto`.

*Unarchive:*

6. Author gửi `PATCH /api/v1/recipes/{id}/unarchive`.
7. Kiểm tra authorization.
8. `recipe.Unarchive()` — Domain kiểm tra `Status == Archived` → `Status = Draft`.
9. `SaveChangesAsync()`, invalidate cache. HTTP 200 OK.

**Luồng thay thế / Ngoại lệ:**

- **A1 – ID không tồn tại hoặc đã soft delete:** HTTP 404 (`RECIPE_NOT_FOUND`).
- **A2 – Không có quyền:** HTTP 403 (`RECIPE_FORBIDDEN`).
- **A3 – Archive một recipe đã Archived, hoặc unarchive một recipe không ở trạng thái Archived:** HTTP **409 Conflict** (`RECIPE_INVALID_STATE_TRANSITION`).

#### FR-RCP-007: Xóa Công thức [Author-Owner/Admin]

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-007 |
| **Tên yêu cầu** | Xóa Công thức Nấu ăn (Soft Delete) |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu (Author – Owner) / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Xóa một công thức theo cơ chế **Soft Delete**: gán `IsDeleted = true`; Global Query Filter (`.Where(x => !x.IsDeleted)`) khiến recipe và toàn bộ dữ liệu con biến mất khỏi mọi truy vấn. **Child entity (Steps/Ingredients/Images) KHÔNG bị xóa** — chúng vô hình cùng recipe. **File ảnh trên MinIO KHÔNG bị xóa ở bước này**, vì recipe vẫn có thể khôi phục; việc dọn file được chuyển sang job dọn vĩnh viễn (FR-JOB-003) chạy sau thời gian lưu giữ 30 ngày. |
| **Điều kiện tiên quyết** | 1. Recipe tồn tại và chưa bị soft delete. 2. Người dùng là owner hoặc Admin. |
| **HTTP Method & Endpoint** | `DELETE /api/v1/recipes/{id:guid}` |
| **Kết quả mong đợi** | Recipe được đánh dấu `IsDeleted = true` và không còn xuất hiện ở bất kỳ endpoint nào. Slug được giải phóng cho recipe mới (nhờ partial unique index). Dữ liệu vẫn có thể khôi phục đến khi job dọn chạy. |
| **HTTP Status Code trả về** | `204 No Content` – Xóa thành công. `403 Forbidden`. `404 Not Found`. |

**Luồng chính (Happy Path):**

1. Author/Admin gửi `DELETE /api/v1/recipes/{id}`.
2. Kiểm tra xác thực và resource-based authorization.
3. `DeleteRecipeCommand` dispatch; lấy recipe theo ID.
4. Gán `recipe.IsDeleted = true`, `recipe.UpdatedAt = DateTime.UtcNow` (`AuditInterceptor` xử lý), `SaveChangesAsync()`.
5. Invalidate cache Redis: xóa khóa `recipe:{slug}`, xóa theo prefix `recipes:list:`, `search:` và khóa `categories:all` (vì `recipeCount` thay đổi).
6. Trả về HTTP 204 No Content.

**Luồng thay thế / Ngoại lệ:**

- **A1 – ID không tồn tại hoặc đã bị soft delete:** HTTP 404 (`RECIPE_NOT_FOUND`).
- **A2 – Không phải owner:** HTTP 403 (`RECIPE_FORBIDDEN`).

> **[CR-2026 / MT-05]** Đây là mâu thuẫn nặng nhất của v1.0.0 (4 mục nói soft delete / 1 mục nói hard delete). v1.1.0 chốt **Soft Delete đồng bộ toàn hệ thống** với ba hệ quả bắt buộc ghi vào SRS:
>
> 1. **Unique index của Slug đổi thành partial unique:** `CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;` — nếu không, slug của recipe đã xóa sẽ chiếm chỗ vĩnh viễn (xem mục 7.2).
> 2. **Child entity không bị xóa theo.** FK `ON DELETE CASCADE` được giữ nguyên trong schema, nhưng chỉ có tác dụng khi job dọn vĩnh viễn thực sự xóa vật lý.
> 3. **Job xóa file MinIO không chạy khi soft delete.** Nếu chạy, khôi phục recipe sẽ mất toàn bộ ảnh. Việc dọn file thuộc về FR-JOB-003.

#### FR-RCP-008: Quản lý Ảnh Công thức (Upload / Set Primary / Delete)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-008 |
| **Tên yêu cầu** | Upload Ảnh, Đặt Ảnh Chính, Xóa Ảnh Công thức |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Author quản lý ảnh minh họa cho công thức của mình. Upload dùng `multipart/form-data`. Ảnh được lưu trên MinIO với path: `recipes/{recipeId}/{uuid}.{ext}`. Ảnh đầu tiên tự động được đặt làm ảnh chính (`IsPrimary = true`). Hỗ trợ 3 thao tác: Upload (POST), **cập nhật metadata gộp (PATCH)** và Xóa (DELETE). Validation bắt buộc: MIME type (`image/jpeg`, `image/png`, `image/webp`, `image/avif`) và kích thước tối đa 5MB. |
| **Điều kiện tiên quyết** | 1. Author/Admin đang đăng nhập. 2. Recipe tồn tại và người dùng có quyền. |
| **HTTP Method & Endpoint** | `POST /api/v1/recipes/{id}/images` \| `PATCH /api/v1/recipes/{id}/images/{imageId}` \| `DELETE /api/v1/recipes/{id}/images/{imageId}` |
| **Kết quả mong đợi** | Ảnh được upload lên MinIO, URL lưu vào database. Bất biến "chỉ 1 ảnh primary / recipe" luôn được giữ. |
| **HTTP Status Code trả về** | Upload: `201 Created`. Cập nhật metadata: `200 OK`. Delete: `204 No Content`. `400 Bad Request` – File/metadata không hợp lệ. `403`/`404` – Lỗi quyền/không tìm thấy. **`503 Service Unavailable` – MinIO không khả dụng.** |

**Luồng chính (Happy Path):**

*Upload:*

1. `POST /api/v1/recipes/{id}/images` với `multipart/form-data` chứa các field: **`file` (bắt buộc), `altText?`, `isPrimary?`**.
2. Validate MIME type: chỉ chấp nhận `image/jpeg`, `image/png`, `image/webp`, `image/avif`.
3. Validate kích thước: `file.Length <= 5*1024*1024` bytes (5MB) — kiểm tra trước khi đọc stream.
4. Validate magic bytes: đọc 4 bytes đầu để xác nhận định dạng thực sự (JPEG: `FF D8 FF`; PNG: `89 50 4E 47`; WebP: `52 49 46 46`; AVIF: box `ftyp`).
5. `IFileStorageService.UploadAsync(file, "recipes/{id}")` → trả về URL công khai.
6. `RecipeImage.Create(originalUrl, altText, isPrimary: isPrimary ?? !recipe.Images.Any(), orderIndex: recipe.Images.Count)` → thêm vào recipe. Nếu `isPrimary = true`, các ảnh khác được gán `IsPrimary = false` trong cùng transaction.
7. `SaveChangesAsync()`, enqueue `ImageResizeJob` (FR-JOB-002), invalidate cache.
8. HTTP 201 Created với `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }` — `mediumUrl`/`thumbnailUrl` là `null` cho đến khi FR-JOB-002 chạy xong.

*Cập nhật metadata ảnh (gộp):*

9. `PATCH /api/v1/recipes/{id}/images/{imageId}` với body `{ altText?, isPrimary?, orderIndex? }`.
10. Tìm image theo imageId trong phạm vi recipe. Cập nhật các field được gửi.
11. **Nếu `isPrimary = true`:** đặt tất cả ảnh khác của recipe đó `IsPrimary = false` trong **cùng một `SaveChangesAsync()`** để bất biến "duy nhất 1 primary" không bị vỡ giữa chừng.
12. HTTP 200 OK với DTO ảnh đã cập nhật.

*Delete image:*

13. `DELETE /api/v1/recipes/{id}/images/{imageId}`.
14. Xóa entity khỏi database (`IsDeleted = true`).
15. **Nếu ảnh bị xóa là primary và recipe còn ảnh khác:** tự động đặt primary cho ảnh có **`OrderIndex` nhỏ nhất; nếu `OrderIndex` bằng nhau thì lấy ảnh có `CreatedAt` sớm nhất**.
16. `BackgroundJob.Enqueue` xóa file trên MinIO *(chỉ ở thao tác xóa ảnh đơn lẻ này — không áp dụng khi soft delete cả recipe, xem FR-RCP-007)*.
17. HTTP 204 No Content.

**Luồng thay thế / Ngoại lệ:**

- **A1 – MIME type không hợp lệ:** HTTP 400 Bad Request (`FILE_MIME_INVALID`).
- **A2 – File vượt quá 5MB:** HTTP 400 (`FILE_SIZE_EXCEEDED`) với message "Kích thước file vượt quá giới hạn 5MB."
- **A3 – Magic bytes không khớp MIME type:** HTTP 400 (`FILE_MIME_INVALID`) "File không hợp lệ."
- **A4 – MinIO không khả dụng:** HTTP 503 Service Unavailable (`FILE_STORAGE_UNAVAILABLE`).
- **A5 – `orderIndex` âm hoặc `altText` > 200 ký tự:** HTTP 400 Bad Request.

> **[CR-2026 / MT-19, MT-41.4, MT-41.5, MT-41.6]** Endpoint hành động riêng `/images/{imgId}/primary` bị thay bằng **PATCH metadata gộp** (RESTful hơn: một tài nguyên, một endpoint, sửa được nhiều trường trong một request). Hai quy tắc nghiệp vụ của v1.0.0 được giữ lại và ghi chính thức vào cả Chương 8: (1) đặt `isPrimary=true` cho một ảnh thì tự bỏ primary các ảnh khác; (2) xóa ảnh primary thì ảnh kế tiếp tự lên primary theo tiêu chí ở bước 15.

#### FR-RCP-009: Quản lý Nguyên liệu (CRUD RecipeIngredient)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-009 |
| **Tên yêu cầu** | Thêm / Cập nhật / Xóa Nguyên liệu Công thức |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Author quản lý danh sách nguyên liệu (`RecipeIngredient`) của công thức. Mỗi nguyên liệu có: `Name` (tên), **`Quantity`** (số lượng dạng số — phục vụ tính toán, scale khẩu phần và `recipeIngredient` trong JSON-LD), **`QuantityText`** (nguyên văn người dùng nhập — "1/2 muỗng", "nửa củ", "vừa đủ"), `Unit` (đơn vị: gram/ml/muỗng/cái/củ...), `Notes` (ghi chú tùy chọn), `OrderIndex` (thứ tự hiển thị). Endpoint hỗ trợ thêm mới (POST), cập nhật (PUT), xóa (DELETE) từng nguyên liệu riêng lẻ. |
| **Điều kiện tiên quyết** | 1. Recipe tồn tại và người dùng có quyền. 2. `Name` 1–200 ký tự (mục 7.9). 3. `Quantity`, `QuantityText`, `Unit` đều **tùy chọn**, nhưng **không được rỗng cả ba** cùng lúc. 4. Nếu có `Quantity` thì phải `> 0`. |
| **HTTP Method & Endpoint** | `POST/PUT/DELETE /api/v1/recipes/{id}/ingredients/{ingId?}` |
| **Kết quả mong đợi** | Danh sách nguyên liệu được cập nhật chính xác, sắp xếp theo `OrderIndex`. Cache `recipe:{slug}` bị invalidate. |
| **HTTP Status Code trả về** | `201`/`200`/`204` – Thành công. `400` – Dữ liệu không hợp lệ (`VALIDATION_ERROR`, `INGREDIENT_QUANTITY_REQUIRED`). `403`/`404` – Lỗi tương ứng. |

**Luồng chính (Happy Path):**

*Thêm nguyên liệu:*

1. `POST /api/v1/recipes/{id}/ingredients` với body: `{ name, quantity?, quantityText?, unit?, notes?, orderIndex? }`.
2. Validate theo Điều kiện tiên quyết; tạo `RecipeIngredient.Create(recipeId, name, quantity, quantityText, unit, notes, orderIndex)`.
3. `_unitOfWork.Recipes` (qua navigation) thêm ingredient, `SaveChangesAsync()`.
4. HTTP 201 Created.

*Cập nhật nguyên liệu:*

5. `PUT /api/v1/recipes/{id}/ingredients/{ingId}` với body fields cần cập nhật.
6. Tìm ingredient, cập nhật, `SaveChangesAsync()`. HTTP 200 OK.

*Xóa nguyên liệu:*

7. `DELETE /api/v1/recipes/{id}/ingredients/{ingId}`.
8. Xóa entity, `SaveChangesAsync()`. HTTP 204 No Content.

**Quy tắc hiển thị (Frontend):** ưu tiên `QuantityText` nếu có; ngược lại format từ `Quantity` + `Unit`. Chức năng scale khẩu phần (×2, ×0.5) chỉ áp dụng cho nguyên liệu có `Quantity`; nguyên liệu chỉ có `QuantityText` được giữ nguyên văn.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Recipe/Ingredient không tồn tại:** HTTP 404.
- **A2 – Không có quyền:** HTTP 403.
- **A3 – Dữ liệu không hợp lệ:** HTTP **400 Bad Request** (`VALIDATION_ERROR`).
- **A4 – Rỗng cả `Quantity`, `QuantityText` lẫn `Unit`:** HTTP **400 Bad Request** (`INGREDIENT_QUANTITY_REQUIRED`).

> **[CR-2026 / MT-04, MT-20.4, MT-41.7]** `Quantity` dùng **chiến lược hai cột song song**: `Quantity decimal(10,3) NULL` giữ giá trị tính toán được, `QuantityText varchar(50) NULL` giữ nguyên văn người dùng nhập. Đây là lựa chọn dài hạn duy nhất giữ được **cả hai** khả năng: kiểu `decimal` đơn thuần không biểu diễn được "1/2 muỗng" hay "vừa đủ" (mất dữ liệu người dùng), còn kiểu `varchar` đơn thuần giết chết scale khẩu phần và `recipeIngredient` của JSON-LD (NFR-SEO-001). Ràng buộc "Quantity > 0 bắt buộc" của v1.0.0 bị gỡ bỏ vì mâu thuẫn trực tiếp với cột `NULL` ở mục 7.4; thay bằng ràng buộc mềm "không rỗng cả ba". Tên trường thứ tự thống nhất là **`orderIndex`** (khớp schema mục 7.4), bỏ hẳn `sortOrder`.

#### FR-RCP-010: Quản lý Các bước Thực hiện (CRUD RecipeStep)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-010 |
| **Tên yêu cầu** | Thêm / Cập nhật / Xóa Bước Thực hiện Công thức |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả sở hữu / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Author quản lý các bước thực hiện (`RecipeStep`) của công thức. Mỗi bước có: `StepNumber` (thứ tự — **do Server toàn quyền gán, Client không bao giờ gửi**), `Title` (tên bước ngắn gọn, **bắt buộc**), `Description` (mô tả chi tiết), `TimerMinutes` (thời gian cần cho bước, tùy chọn), `ImageUrl` (ảnh minh họa riêng cho bước, tùy chọn). Khi thêm bước, Server gán `StepNumber = Max(StepNumber) + 1`. Khi xóa hoặc sắp xếp lại bước, Server **tự động đánh lại số thứ tự liên tục (1, 2, 3...)** cho toàn bộ bước còn lại. |
| **Điều kiện tiên quyết** | 1. Recipe tồn tại, người dùng có quyền. 2. `Title` 1–200 ký tự, `Description` không rỗng và ≤ 2000 ký tự (mục 7.9). 3. Body **không được chứa** `stepNumber` — nếu có, trường này bị bỏ qua. |
| **HTTP Method & Endpoint** | `POST/PUT/DELETE /api/v1/recipes/{id}/steps/{stepId?}` · `PATCH /api/v1/recipes/{id}/steps/reorder` |
| **Kết quả mong đợi** | Danh sách steps được cập nhật với `StepNumber` liên tục từ 1, không trùng, không khuyết. Cache `recipe:{slug}` bị invalidate. |
| **HTTP Status Code trả về** | `201`/`200`/`204` – Thành công. `400` – Dữ liệu không hợp lệ (`VALIDATION_ERROR`). `403`/`404` – Lỗi tương ứng. |

**Luồng chính (Happy Path):**

*Thêm bước:*

1. `POST /api/v1/recipes/{id}/steps` với body: `{ title, description, timerMinutes?, imageUrl? }`.
2. Server tính `StepNumber = recipe.Steps.Max(s => s.StepNumber) + 1` (hoặc 1 nếu chưa có bước nào).
3. `RecipeStep.Create(recipeId, stepNumber, title, description, timerMinutes, imageUrl)`.
4. `SaveChangesAsync()`. HTTP 201 Created.

*Cập nhật bước:*

5. `PUT /api/v1/recipes/{id}/steps/{stepId}` với body `{ title?, description?, timerMinutes?, imageUrl? }` — **không có `stepNumber`**; muốn đổi thứ tự phải dùng endpoint reorder ở bước 9.
6. Tìm step, cập nhật, `SaveChangesAsync()`. HTTP 200 OK.

*Xóa bước:*

7. `DELETE /api/v1/recipes/{id}/steps/{stepId}`.
8. Xóa step, sau đó renumber toàn bộ steps còn lại về 1..N theo thứ tự hiện tại; **toàn bộ thao tác nằm trong một `SaveChangesAsync()` duy nhất** (xem ghi chú bên dưới). HTTP 204 No Content.

*Sắp xếp lại các bước:*

9. `PATCH /api/v1/recipes/{id}/steps/reorder` với body: `{ stepIds: [guid, guid, ...] }` — mảng **đầy đủ** id các bước theo thứ tự mới mong muốn.
10. Validate: mảng không rỗng, không trùng id, và tập id **khớp chính xác** tập step hiện có của recipe (thiếu hoặc thừa → HTTP 400).
11. Server gán lại `StepNumber = vị trí trong mảng + 1` cho từng step, ghi trong **một transaction duy nhất**. HTTP 200 OK kèm danh sách steps đã sắp xếp.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Recipe không tồn tại:** HTTP 404.
- **A2 – Không có quyền:** HTTP 403.
- **A3 – Dữ liệu không hợp lệ:** HTTP **400 Bad Request** (`VALIDATION_ERROR`).
- **A4 – `stepIds` không khớp tập step hiện có (thiếu, thừa hoặc trùng id):** HTTP **400 Bad Request** (`VALIDATION_ERROR`) với message chỉ rõ id sai.

> **[CR-2026 / MT-03, MT-20.5, MT-20.7]** Client **không bao giờ gửi `stepNumber`** — kể cả ở POST lẫn PUT. Đây là quyết định về **toàn vẹn dữ liệu**: ràng buộc `UNIQUE (RecipeId, StepNumber)` ở mục 7.3 sẽ bị vi phạm ngay khi hai request thêm bước chạy gần như đồng thời, hoặc khi người dùng nhập trùng số — kết quả là lỗi `DbUpdateException` → HTTP 500 thay vì một lỗi nghiệp vụ rõ ràng. Giao quyền đánh số cho Server cũng xóa bỏ gánh nặng "nhớ mình đang ở bước mấy" khỏi phía người dùng.
>
> **Ràng buộc kỹ thuật bắt buộc khi renumber:** mọi thao tác gán lại `StepNumber` phải nằm trong **một** `SaveChangesAsync()` / một transaction; nếu không, trạng thái trung gian (ví dụ hai bước cùng mang số 2 trong chốc lát) sẽ làm vỡ UNIQUE giữa chừng. Cách xử lý bắt buộc là khai báo ràng buộc dưới dạng **UNIQUE CONSTRAINT** `DEFERRABLE INITIALLY DEFERRED` để PostgreSQL dời việc kiểm tra đến cuối transaction. ⚠️ Phải là **constraint**, không phải **unique index**: PostgreSQL kiểm tra unique index ngay sau từng câu lệnh và **không cho phép index là DEFERRABLE** — nếu khai báo bằng `HasIndex(...).IsUnique()` của EF Core (sinh ra index), mọi thao tác đổi chỗ hai bước sẽ lỗi `23505` ở câu `UPDATE` đầu tiên. Constraint được tạo bằng DDL trong migration (hợp lệ theo CONS-006). *(CR-2026-02 / MT-56)*
>
> Tên trường thời lượng thống nhất là **`timerMinutes`** (khớp cột `TimerMinutes` mục 7.3), bỏ hẳn `durationMinutes`. Trường **`title` được bổ sung vào body và là bắt buộc**, vì cột `RecipeStep.Title` là `NOT NULL` — v1.0.0 thiếu trường này nên mọi lệnh POST đều sẽ thất bại ở tầng DB.

#### FR-RCP-011: Xem Danh sách Công thức Cá nhân (My Recipes)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-RCP-011 |
| **Tên yêu cầu** | Xem Danh sách Công thức của Chính mình (bao gồm Draft / Archived) |
| **Nhóm chức năng** | Module Quản lý Công thức Nấu ăn (FR-RCP) |
| **Tác nhân** | Tác giả (Author) / Quản trị viên (Admin) |
| **Mức ưu tiên (MoSCoW)** | **M – Must Have** *(FR-RCP-001 và FR-RCP-002 ở mức M chỉ phục vụ được dữ liệu Published nhờ có endpoint này gánh phần riêng tư; không có nó thì Author không có đường nào xem lại bản nháp của mình)* |
| **Mô tả** | Trả về danh sách phân trang các công thức **thuộc sở hữu của chính người gọi**, ở **mọi trạng thái** (Draft, Published, Archived). Đây là endpoint **riêng tư duy nhất** của module Recipe: response phụ thuộc hoàn toàn vào danh tính người gọi, do đó **bị cấm cache** ở mọi tầng. Admin có thể truyền `?authorId=` để xem danh sách của một Author bất kỳ phục vụ công việc kiểm duyệt. Endpoint này là phần đối xứng của nguyên tắc cách ly Public/Private khai báo ở đầu mục 3.3. |
| **Điều kiện tiên quyết** | 1. **Bắt buộc** JWT hợp lệ (role Author hoặc Admin). 2. `page >= 1`, `pageSize ∈ [1, 50]` (mặc định 12). 3. `sortBy` thuộc whitelist, `sortOrder ∈ {asc, desc}`. 4. `status` (nếu có) ∈ `{Draft, Published, Archived}`. 5. `authorId` (nếu có) chỉ được chấp nhận khi người gọi có role Admin. |
| **HTTP Method & Endpoint** | `GET /api/v1/recipes/mine?status={s}&page={n}&pageSize={n}&sortBy={field}&sortOrder={asc\|desc}&authorId={guid}` |
| **Kết quả mong đợi** | `PagedResult<RecipeSummaryDto>` chứa công thức của người gọi ở mọi trạng thái, kèm header **`Cache-Control: no-store`**. |
| **HTTP Status Code trả về** | `200 OK` – Thành công (kể cả items rỗng). `400 Bad Request` – Tham số không hợp lệ (bao gồm `sortBy`/`status` ngoài whitelist). `401 Unauthorized` – Thiếu hoặc sai token. `403 Forbidden` – Người gọi không phải Admin nhưng truyền `authorId`. |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/recipes/mine?status=Draft&page=1&pageSize=12` kèm `Authorization: Bearer <access_token>`.
2. Middleware xác thực JWT; `ICurrentUser` cung cấp `UserId` và `Roles`.
3. `GetMyRecipesQuery` dispatch qua MediatR. **Query này KHÔNG implement `ICacheable`** — `CachingBehavior` bỏ qua hoàn toàn.
4. `ValidationBehavior` kiểm tra `page`, `pageSize`, `sortBy`, `sortOrder`, `status`; nếu có `authorId` thì kiểm tra người gọi có role Admin, nếu không → 403.
5. Handler xác định `targetAuthorId` = `authorId` (khi Admin truyền) hoặc `ICurrentUser.UserId`.
6. Truy vấn `Recipes.Where(r => r.AuthorId == targetAuthorId)` — Global Query Filter đã tự loại `IsDeleted = true`. Lọc thêm theo `status` nếu có.
7. Apply sorting qua dictionary whitelist (giống FR-RCP-001, bước 6), mặc định `sortBy=createdAt&sortOrder=desc`.
8. Apply pagination, map sang `PagedResult<RecipeSummaryDto>` (mỗi item mang cả `id` lẫn `slug` theo quy ước Chương 8).
9. Endpoint gắn header **`Cache-Control: no-store`** vào response trước khi trả về. HTTP 200 OK.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Không có hoặc sai access token:** HTTP 401 Unauthorized.
- **A2 – Author truyền `authorId` của người khác:** HTTP 403 Forbidden (`RECIPE_FORBIDDEN`).
- **A3 – `sortBy`, `sortOrder` hoặc `status` ngoài whitelist:** HTTP 400 Bad Request (`VALIDATION_ERROR`).
- **A4 – Người dùng chưa có công thức nào:** HTTP 200 với `items = []` (không phải 404).

> **[CR-2026 / MT-34]** Đây là **nửa còn lại** của lỗi nghiêm trọng nhất trong v1.0.0. Ở v1.0.0, `GET /recipes` vừa lọc theo danh tính (Guest thấy Published, Author thấy thêm Draft của mình, Admin thấy tất cả) **vừa** được Output Cache lưu dưới khóa chỉ gồm `{path}?{queryString}`. Hệ quả: một lượt truy cập của Admin nạp toàn bộ Draft của mọi Author vào cache, rồi **mọi Guest gọi đúng URL đó trong 15 phút tiếp theo đều đọc được** — một lỗi rò rỉ dữ liệu giữa các tài khoản, sinh ra từ hai câu cách nhau 5 dòng trong cùng một FR.
>
> Giải pháp dài hạn không phải là thêm `userId` vào khóa cache (hit rate sụp đổ, và chỉ cần quên `VaryBy` ở **một** endpoint là lỗ hổng quay lại), mà là **tách bạch về mặt kiến trúc**: endpoint công khai chỉ phục vụ dữ liệu công khai nên cache thoải mái; dữ liệu phụ thuộc danh tính đi qua endpoint riêng và **không bao giờ được cache**. Nguyên tắc này được ghi thành chuẩn tại NFR-SEC-006.

---

### 3.4. Module Tìm kiếm và Phân trang (FR-SRCH)

#### FR-SRCH-001: Tìm kiếm Toàn văn bản (Full-Text Search)

| Trường | Nội dung |
| --- | --- |
| **Mã yêu cầu** | FR-SRCH-001 |
| **Tên yêu cầu** | Tìm kiếm Toàn văn bản Công thức (Full-Text Search) |
| **Nhóm chức năng** | Module Tìm kiếm và Phân trang (FR-SRCH) |
| **Tác nhân** | Tất cả (Guest / Author / Admin) |
| **Mức ưu tiên (MoSCoW)** | M – Must Have |
| **Mô tả** | Hệ thống cung cấp tính năng tìm kiếm toàn văn bản (FTS) cho công thức sử dụng PostgreSQL `tsvector`/`tsquery` với text search configuration **`simple`** kết hợp **`unaccent`**. Cột `SearchVector` là **generated column `STORED`** — PostgreSQL tự tính lại mỗi khi `Title` hoặc `Description` thay đổi, không cần trigger và không thể quên đồng bộ. Kết quả được xếp hạng bởi `ts_rank()`. Bỏ dấu tiếng Việt nên "pho" tìm được "phở". |
| **Điều kiện tiên quyết** | 1. PostgreSQL extensions `unaccent` và `pg_trgm` đã được install. 2. Hàm wrapper `unaccent_immutable(text)` đã được tạo (xem mục 7.2). 3. GIN index trên cột `SearchVector` đã được tạo. 4. Tham số `q` không rỗng, tối thiểu 2 ký tự. |
| **HTTP Method & Endpoint** | `GET /api/v1/recipes/search?q={searchTerm}&page={n}&pageSize={n}&categoryId={guid}&difficulty={d}` |
| **Kết quả mong đợi** | `PagedResult<RecipeSummaryDto>` được xếp hạng theo độ liên quan (`ts_rank`), chỉ chứa công thức `Published`. Hỗ trợ tìm kiếm không dấu tiếng Việt. |
| **HTTP Status Code trả về** | `200 OK` – Thành công (kể cả kết quả rỗng). `400 Bad Request` – Query không hợp lệ (`VALIDATION_ERROR`). |

**Luồng chính (Happy Path):**

1. Client gửi `GET /api/v1/recipes/search?q=pho+bo&page=1&pageSize=12`.
2. `SearchRecipesQuery` dispatch với `SearchTerm = "pho bo"`, `Page = 1`, `PageSize = 12`.
3. Handler xây dựng tsquery từ search terms: `"pho:* & bo:*"` (prefix matching).
4. LINQ query với EF Core: `.Where(r => r.SearchVector.Matches(EF.Functions.ToTsQuery("simple", query)))`.
5. Apply `ORDER BY ts_rank(SearchVector, query) DESC` để kết quả liên quan nhất lên đầu.
6. **Chỉ trả về `Status == Published`** cho mọi người gọi, không ngoại lệ — response thuần công khai, không phụ thuộc danh tính.
7. Apply pagination, trả về `PagedResult<RecipeSummaryDto>` với field `relevanceScore`.
8. Lưu kết quả vào **Redis cache-aside với khóa `search:{queryHash}`, TTL 1 phút** (bảng TTL chuẩn tại NFR-PERF-003). Cache này **không được invalidate chủ động** — TTL ngắn nên để hết hạn tự nhiên.

**Luồng thay thế / Ngoại lệ:**

- **A1 – Query rỗng hoặc < 2 ký tự:** HTTP **400 Bad Request** (`VALIDATION_ERROR`).
- **A2 – Không tìm thấy kết quả:** HTTP 200 với `items = []` và message gợi ý.
- **A3 – Ký tự đặc biệt trong query (SQL injection attempt):** EF Core parameterize tự động; tsquery sanitization loại bỏ ký tự nguy hiểm.

> **[CR-2026 / MT-25]** v1.0.0 mắc **ba lỗi chồng nhau** ở cùng một tính năng, trong đó một lỗi làm tìm kiếm chết hẳn lúc chạy:
>
> 1. *"computed column được cập nhật bởi trigger"* — hai cơ chế loại trừ nhau trong cùng một câu. v1.1.0 chốt **generated column `STORED`** khai báo qua `.HasComputedColumnSql(..., stored: true)` trong `OnModelCreating`: PostgreSQL tự bảo đảm đồng bộ, không có đường nào quên cập nhật.
> 2. `ToTsQuery("vietnamese", ...)` — PostgreSQL 16 **không có sẵn** text search configuration tên `vietnamese`, và mục 2.4.1 chỉ cài `unaccent` + `pg_trgm`. Câu truy vấn này sẽ ném `text search configuration "vietnamese" does not exist` **lúc chạy**, không phải lúc biên dịch. v1.1.0 dùng **`simple` + `unaccent`** — chạy được ngay trên PostgreSQL 16 gốc, không cần build image riêng, và vẫn đạt đúng yêu cầu bỏ dấu đã nêu. Tiếng Việt vốn không có biến cách nên mất stemming gần như không ảnh hưởng chất lượng kết quả.
> 3. Xung đột với CONS-006 ("không viết raw SQL") — đã được diễn giải lại ở mục 2.5: ràng buộc này nhắm vào **truy vấn dữ liệu**, còn DDL đặc thù PostgreSQL (extension, generated column, hàm `IMMUTABLE`, index GIN) được phép khai báo trong migration.

#### FR-SRCH-002/003/004: Lọc, Sắp xếp và Phân trang (Tóm tắt)

Ba FR còn lại của module Search được tích hợp sẵn vào FR-RCP-001, FR-RCP-011 và FR-SRCH-001. Bảng tóm tắt:

| Mã FR | Tên | Ưu tiên | Tham số Query | Mô tả |
| --- | --- | --- | --- | --- |
| FR-SRCH-002 | Lọc công thức | **M** | `categoryId={guid}`, `difficulty={Easy\|Medium\|Hard\|Expert}`, `maxCookTime={minutes}`, `maxPrepTime={minutes}`, `minServings={n}` | Lọc kết quả theo một hoặc nhiều tiêu chí. Các filter kết hợp bằng AND logic. Giá trị `difficulty` ngoài enum → HTTP 400. |
| FR-SRCH-003 | Sắp xếp kết quả | **M** | `sortBy={field}` với `field ∈ { createdAt, publishedAt, title, cookTime, prepTime }`; `sortOrder ∈ { asc, desc }` | Hai tham số độc lập. Mặc định: `sortBy=createdAt&sortOrder=desc` (mới nhất trước). Giá trị ngoài whitelist → **HTTP 400**, không im lặng bỏ qua. |
| FR-SRCH-004 | Phân trang (Offset-based) | **M** | `page={n}` (default: 1), `pageSize={n}` (default: 12, max: 50) | Offset-based pagination (SKIP/TAKE). Response bao gồm `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`. |

> **[CR-2026 / MT-01, MT-20.13, MT-20.14, MT-20.15, MT-40]** Quy ước một tham số `sort=-field` của v1.0.0 bị loại bỏ khỏi toàn tài liệu; toàn hệ thống dùng **`sortBy` + `sortOrder`**. Lý do dài hạn: hai rule FluentValidation độc lập (`sortBy ∈ whitelist`, `sortOrder ∈ {asc,desc}`) dễ viết và dễ kiểm chứng hơn hẳn việc tự parse tiền tố `-` — vốn còn dễ bị nuốt mất khi quên URL-encode, dẫn tới sai thứ tự **âm thầm** mà không có lỗi nào được ném ra. Quan trọng hơn, whitelist đóng vai trò **rào chắn chống SQL injection qua tên cột**: `sortBy` được ánh xạ sang biểu thức cột qua một dictionary ở tầng Application, không bao giờ ghép chuỗi SQL.
>
> `pageSize` mặc định chốt **12** (chia hết cho lưới 2/3/4 cột của Frontend), bỏ giá trị 10 ở Chương 8. Bộ lọc chốt đủ 5 tham số; `difficulty` bổ sung giá trị **`Expert`** vốn đã tồn tại trong enum `RecipeDifficulty` (mục 7.2) nhưng bị sót khỏi danh sách hợp lệ. Cả ba FR được gán mức **M** vì đã tích hợp sẵn trong FR-RCP-001 (mức M) — theo quy tắc *"một FR mức Must Have không được phụ thuộc vào FR có mức ưu tiên thấp hơn"*.

### 3.5. Module Quản lý Tệp tin (FR-FILE)

Module xử lý tất cả thao tác với file binary trên hệ thống lưu trữ đối tượng (Object Storage) MinIO S3-compatible. Abstraction layer `IFileStorageService` cho phép swap implementation (MinIO ↔ AWS S3 ↔ local filesystem) mà không cần thay đổi Application Layer.

| Mã FR | Tên | Ưu tiên | Mô tả | Ràng buộc kỹ thuật |
| --- | --- | --- | --- | --- |
| FR-FILE-001 | Upload File lên MinIO | **M** | **Service:** `IFileStorageService.UploadAsync(IFormFile, folder, ct)` → `string` (public URL). Tạo unique filename = `{folder}/{Guid.NewGuid()}{ext}` để ngăn path traversal. Preserve MIME type gốc. **Endpoint HTTP tổng quát:** `POST /api/v1/files/upload` (Bearer, `AuthorPolicy`, `multipart/form-data`) → `201 { key, url, contentType, size }`, lưu tại `uploads/{userId}/{guid}{ext}`. Đây là **đường duy nhất** để có URL cho các trường chỉ nhận URL: `avatarUrl` (FR-AUTH-007), `imageUrl` danh mục (FR-CAT-003/004), `imageUrl` bước nấu (FR-RCP-010). Ảnh công thức dùng endpoint riêng `POST /recipes/{id}/images` (FR-RCP-008) vì cần gắn metadata và kích hoạt FR-JOB-002. | Max size: 5MB. MIME: JPEG/PNG/WebP/AVIF. Magic bytes validation. Bucket: `"culinary-blog"`. Policy: public-read. Implementation qua `AWSSDK.S3` với `ForcePathStyle = true`. Rate limit policy `upload` (NFR-SEC-003). Lỗi: 400 `FILE_SIZE_EXCEEDED`/`FILE_MIME_INVALID`, 401, 503 `FILE_STORAGE_UNAVAILABLE`. |
| FR-FILE-002 | Xóa File khỏi MinIO | **M** | **Service:** `IFileStorageService.DeleteAsync(fileUrl, ct)`. Trích xuất object key từ URL, gọi `DeleteObjectAsync()` của `AWSSDK.S3`. **Chỉ được gọi từ FR-JOB-003 (job dọn vĩnh viễn), từ thao tác xóa ảnh đơn lẻ (FR-RCP-008) và từ endpoint dưới đây — KHÔNG bao giờ gọi khi soft delete recipe.** **Endpoint HTTP:** `DELETE /api/v1/files/{**key}` (Bearer) — chỉ xóa được tệp nằm trong `uploads/{userId}/` **của chính người gọi** (Admin xóa được mọi tệp); tệp của người khác → **403 `FILE_FORBIDDEN`**; key chứa `..` → 400. | Nếu object không tồn tại trên MinIO → không throw exception, trả 204 (idempotent). Lỗi kết nối MinIO → Hangfire retry tối đa 3 lần (khi gọi từ job) hoặc 503 (khi gọi qua HTTP). |

> **[CR-2026 / MT-05, MT-18, MT-40]** Hai FR này được gán mức **M** vì FR-RCP-008 (mức M) gọi thẳng vào chúng — cắt đi thì tính năng quản lý ảnh mức Must Have thành mã chết.
>
> Ràng buộc quan trọng nhất ở đây là **thời điểm** xóa file: v1.0.0 cho job xóa file MinIO chạy ngay sau khi xóa recipe. Khi hệ thống chuyển sang Soft Delete toàn cục (MT-05), hành vi đó trở thành lỗi mất dữ liệu — recipe khôi phục được nhưng ảnh đã bị xóa vĩnh viễn khỏi MinIO. v1.1.0 chuyển toàn bộ việc dọn file sang **FR-JOB-003**, chạy sau thời gian lưu giữ 30 ngày.
>
> SDK thống nhất là **`AWSSDK.S3`** (khớp Chương 5.3, 6.1, 6.2), không dùng MinIO .NET SDK: `IFileStorageService` được thiết kế để swap implementation, và với `AWSSDK.S3` thì việc chuyển sang AWS S3 thật chỉ là bỏ dòng override `ServiceURL` — trong khi SDK chính chủ MinIO sẽ khóa chặt hệ thống vào một nhà cung cấp. Lưu ý phương thức tương ứng là `DeleteObjectAsync()`, không phải `RemoveObjectAsync()` của MinIO SDK.
>
> **[CR-2026-02 / MT-43, MT-46]** v1.1.0 có ba FR chỉ nhận **URL ảnh** — `avatarUrl` (FR-AUTH-007), `imageUrl` danh mục (FR-CAT-003) và `imageUrl` bước nấu (FR-RCP-010) — nhưng **không có endpoint nào để đưa ảnh lên và nhận lại URL**: FR-FILE-001/002 chỉ đặc tả tầng service, còn endpoint upload duy nhất trong Chương 8 (`/recipes/{id}/images`) gắn chặt với công thức. Người dùng vì vậy không có cách hợp lệ nào để đổi avatar. Buổi 2 đã phải tự bổ sung `POST /api/v1/files/upload` và `DELETE /api/v1/files/{**key}` ngoài SRS. v1.2.0 **chính thức hóa** hai endpoint này thay vì tạo ba endpoint con (`/auth/me/avatar`, `/categories/{id}/image`, `/recipes/{id}/steps/{stepId}/image`): một endpoint tổng quát giữ toàn bộ quy tắc bảo mật tệp (giới hạn 5MB, magic bytes, tên GUID, phạm vi thư mục theo người dùng) **ở một chỗ duy nhất**, trong khi ba endpoint con sẽ lặp lại chúng ba lần. Đi kèm là hai mã lỗi mà code đã dùng thực tế nhưng Phụ lục B chưa có: `FILE_FORBIDDEN` (403) và `FILE_STORAGE_UNAVAILABLE` (503).

### 3.6. Module Background Jobs (FR-JOB)

Module xử lý các tác vụ nền không đồng bộ sử dụng Hangfire. Hangfire dùng chung codebase và image với API nhưng **triển khai thành container worker riêng** (`hangfire`, `Hangfire__WorkerOnly=true`): container `api` chỉ **enqueue** job và phục vụ Dashboard (`Hangfire__ServerEnabled=false`), còn **toàn bộ job được xử lý ở worker**. Hàng đợi lưu bền trên PostgreSQL (schema `hangfire`) — **job đã enqueue vẫn nằm trong hàng đợi và chạy lại khi worker khởi động; chỉ bị trễ chứ không mất**. Dashboard quản lý jobs tại `/hangfire`, **được bảo vệ bằng HTTP Basic Auth tại tầng Nginx** (xem mục 5.3 và 6.5). Hỗ trợ 3 loại job: Fire-and-forget (chạy ngay), Delayed (chạy sau N giây/phút) và Recurring (lịch cron).

| Mã FR | Tên Job | Ưu tiên | Loại | Trigger | Mô tả | Retry Policy |
| --- | --- | --- | --- | --- | --- | --- |
| FR-JOB-001 | Welcome Email Job | S | Fire-and-forget | Sau FR-AUTH-001 thành công (`BackgroundJob.Enqueue`) | Gửi email HTML chào mừng đến địa chỉ email vừa đăng ký, qua `IEmailService` (MailKit + SMTP). Email template bao gồm: `DisplayName` của người dùng và link đến ứng dụng. **Không có link kích hoạt email** — hệ thống không có luồng xác nhận email (xem MT-21 tại mục 2.3). | Tự động retry 3 lần với exponential backoff (1 phút, 5 phút, 30 phút). Sau 3 lần fail → chuyển sang Failed state, log error. |
| FR-JOB-002 | Image Resize / Thumbnail Job | S | Fire-and-forget | Sau FR-RCP-008 upload ảnh thành công | Tạo thumbnail (300×300px) và medium image (800×600px) từ ảnh gốc. Lưu cả 3 phiên bản lên MinIO. Cập nhật `MediumUrl`/`ThumbnailUrl` vào database. | Retry 3 lần. Nếu fail: ảnh gốc vẫn hiển thị, chỉ thiếu thumbnail. |
| FR-JOB-003 | **Permanent Purge Job** (dọn dữ liệu soft-deleted vĩnh viễn) | S | Recurring | Hàng ngày lúc 03:30 AM UTC (cron: `"30 3 * * *"`) | Quét các recipe có `IsDeleted = true AND UpdatedAt < NOW() - INTERVAL '30 days'` (bỏ qua Global Query Filter bằng `IgnoreQueryFilters()`). Với mỗi recipe: **(1)** thu thập toàn bộ URL ảnh thuộc `RecipeImages` (original, medium, thumbnail) và `RecipeStep.ImageUrl`; **(2)** hard-delete recipe trong một transaction — FK `ON DELETE CASCADE` tự dọn Steps/Ingredients/Images; **(3)** sau khi transaction commit thành công mới gọi `IFileStorageService.DeleteAsync()` cho từng file (FR-FILE-002). Thứ tự này bảo đảm không bao giờ xóa file của một recipe mà transaction DB bị rollback. Job chạy trong **container worker `hangfire`**; khi scale nhiều worker hoặc khi một lượt bị kích hoạt lại lúc lượt trước chưa xong, job phải lấy **distributed lock** (NFR-SCALE-001) để không chạy chồng. | Retry 2 lần nếu fail. Job **idempotent**: file đã xóa không gây lỗi. Log số recipe đã dọn và số file đã xóa qua Serilog. |

> **[CR-2026 / MT-05, MT-21, MT-28, MT-33.2, MT-39, MT-40]** FR-JOB-003 được **định nghĩa lại hoàn toàn**. v1.0.0 giao cho nó việc sinh `sitemap.xml`, nhưng đặc tả đó không nghiệm thu được vì hai lý do độc lập:
>
> 1. **Sai vị trí phục vụ.** Crawler tìm sitemap tại `https://domain.com/sitemap.xml` — domain do **Next.js** phục vụ. File sinh ở BE lại nằm trên MinIO hoặc trong `wwwroot` của container API (`api.culinaryblog.com`), không khớp `robots.txt` của site chính. Tệ hơn, `wwwroot` nằm trong container: mỗi lần deploy là mất file, và khi chạy nhiều instance thì mỗi instance giữ một bản khác nhau.
> 2. **Ping Google đã bị khai tử.** Google ngừng hỗ trợ endpoint `https://www.google.com/ping?sitemap=` từ tháng 6/2023 — gọi vào chỉ nhận 404.
>
> v1.1.0 chuyển trách nhiệm sitemap sang **Next.js `app/sitemap.ts` + `app/robots.ts`** (xem NFR-SEO-003) — đúng vị trí, không cần lưu file, không cần distributed lock cho việc này. Khe FR-JOB-003 được dùng cho nhu cầu thực sự phát sinh từ quyết định Soft Delete toàn cục (MT-05): **phải có một chỗ dọn dữ liệu và file vĩnh viễn**, nếu không MinIO sẽ phình mãi mãi và slug của recipe đã xóa chiếm chỗ không bao giờ được giải phóng. Mức ưu tiên **S**: FR-RCP-007 (mức M) ủy thác việc dọn file cho job này, nhưng cắt job đi chỉ để lại file mồ côi chứ không làm hỏng chức năng xóa — không hạ xuống C vì như vậy sẽ thấp hơn mức của FR phụ thuộc vào nó.

> **[CR-2026-02 / MT-47]** v1.1.0 (kế thừa nguyên văn từ v1.0.0) ghi *"Hangfire chạy in-process trong .NET API"*, trong khi hệ thống thực tế từ Buổi 2 chạy Hangfire ở **container worker riêng** dùng chung image với API — và 41 mâu thuẫn của CR-2026 chưa từng phân tích điểm này. v1.2.0 chuẩn hóa theo **worker riêng** vì ba lý do dài hạn: **(1)** FR-JOB-002 resize ảnh bằng ImageSharp là tác vụ **nặng CPU** — chạy chung tiến trình với API thì mỗi lượt upload ảnh sẽ tranh CPU với chính các request đang phục vụ người đọc, đe dọa trực tiếp mục tiêu p95 ≤ 500ms (NFR-PERF-001); **(2)** hai loại tải được **scale độc lập** — `--scale api=3` không kéo theo 3 job server; **(3)** chi phí vận hành đã được trả xong ở Buổi 2 (khóa DataProtection dùng chung qua volume, worker chờ DB sẵn sàng trước khi chạy). Phương án gộp vào API chỉ đơn giản hơn ở số lượng container, đổi lại mất cả ba lợi ích trên.

### 3.7. Module Quan sát Hệ thống (FR-OBS)

Module cung cấp khả năng quan sát (Observability) toàn diện theo ba trụ cột: Logging (Serilog), Metrics (OpenTelemetry), và Distributed Tracing (OpenTelemetry). Đây là yêu cầu bắt buộc cho production deployment.

| Mã FR | Tên | Ưu tiên | Mô tả | Kỹ thuật / Công cụ |
| --- | --- | --- | --- | --- |
| FR-OBS-001 | Health Check Endpoints | **M** | Hệ thống cung cấp 3 endpoint health check với mục đích khác nhau: • `GET /health` – tổng hợp tất cả components (database, Redis, MinIO). • `GET /health/live` – Liveness probe (chỉ kiểm tra process còn sống). • `GET /health/ready` – Readiness probe (kiểm tra kết nối database và Redis). | `IHealthCheck`, `AspNetCore.HealthChecks.NpgSql`, `AspNetCore.HealthChecks.Redis`, `AspNetCore.HealthChecks.Minio`. Liveness chỉ trả healthy. Readiness fail khi DB/Redis down → **khối `healthcheck:` của Docker Compose đánh dấu container unhealthy, Nginx chuyển traffic sang instance khác qua `proxy_next_upstream`** (xem mục 6.5). |
| FR-OBS-002 | Structured Logging | **M** | Mọi HTTP request được log với: CorrelationId (`X-Correlation-ID` header), HTTP method/path/status, elapsed time (ms), UserId (khi đã xác thực). MediatR Pipeline Behavior (`LoggingBehavior`) log tất cả Commands/Queries và **cảnh báo khi request > 500ms**. Ngưỡng > 100ms của truy vấn database do **EF Core command interceptor** đảm nhiệm, không phải behavior này (xem NFR-PERF-004). | Serilog + `CorrelationIdMiddleware`. Sinks: Console (structured JSON), File (rolling daily), Seq (development). Log levels: Debug (development), Information (production), Warning/Error (luôn luôn). **Raw refresh token không bao giờ được ghi vào log** (NFR-SEC-007). |
| FR-OBS-003 | Distributed Tracing & Metrics | C | OpenTelemetry instrumentation cho: HTTP request traces (`ActivitySource`), EF Core database operation traces, custom business metrics (recipe created/published count). Traces được export đến Seq (development) hoặc Jaeger/Grafana Tempo (production). | OpenTelemetry .NET SDK, OTLP exporter. `Activity.TraceId` được include trong structured log (log correlation với trace). Metrics: request count, duration histogram, error rate. |

> **[CR-2026 / MT-29, MT-31, MT-40]** Mọi nhắc đến **Kubernetes** bị gỡ khỏi tài liệu: hạ tầng đã chốt chỉ có Docker Compose (CONS-009, mục 6.5), không có manifest hay Helm chart nào trong phạm vi. Quan trọng hơn một thay đổi biên tập: **Docker Compose không tự đọc `/health/ready`** như Kubernetes, nên nếu chỉ xóa chữ "Kubernetes" thì cơ chế "ngừng route traffic khi DB down" **không có ai thực thi** — NFR-REL-001 sẽ không nghiệm thu được. Vì vậy mục 6.5 được bổ sung khối `healthcheck:` cho `api`/`postgres`/`redis` kèm `depends_on: condition: service_healthy`, và Nginx được cấu hình `proxy_next_upstream`.
>
> Mức ưu tiên: FR-OBS-001 = **M** (NFR-REL-001 cần readiness probe để đo uptime), FR-OBS-002 = **M** (CONS-010 quy định structured logging là bắt buộc), FR-OBS-003 = **C** (tracing/metrics là phần nâng cao, cắt được mà không ảnh hưởng FR nào ở mức M).

---

## CHƯƠNG 4. YÊU CẦU PHI CHỨC NĂNG (NFR)

Phần này mô tả các thuộc tính chất lượng hệ thống theo mô hình ISO/IEC 25010 (FURPS+). Mỗi yêu cầu phi chức năng được gán mã định danh, mức ưu tiên và tiêu chí đo lường định lượng cụ thể. Các NFR này ràng buộc thiết kế kiến trúc và lựa chọn công nghệ toàn bộ hệ thống.

| Mã NFR | Danh mục | Số yêu cầu | Ưu tiên |
| --- | --- | --- | --- |
| NFR-PERF | Hiệu năng (Performance) | 5 | Cao |
| NFR-SEC | Bảo mật (Security) | 7 | Rất cao |
| NFR-USE | Khả năng sử dụng (Usability) | 4 | Trung bình |
| NFR-REL | Độ tin cậy (Reliability) | 3 | Cao |
| NFR-MAINT | Khả năng bảo trì (Maintainability) | 4 | Trung bình |
| NFR-SCALE | Khả năng mở rộng (Scalability) | 3 | Cao |
| NFR-SEO | Tối ưu SEO (SEO) | 4 | Cao |

### 4.1. Hiệu năng (NFR-PERF)

Toàn bộ các chỉ số hiệu năng được đo trong môi trường production với tải thực tế. Các ngưỡng dưới đây áp dụng cho trường hợp cache warm (Redis hit rate ≥ 80%).

| Mã | Nội dung |
| --- | --- |
| **NFR-PERF-001** Response Time API | Thời gian phản hồi API: • p50 ≤ 150ms — cho tất cả GET endpoints với dữ liệu cache. • p95 ≤ 500ms — cho tất cả API endpoints (kể cả write operations). • p99 ≤ 1000ms — không vượt quá 1 giây trong mọi trường hợp. Đo bằng: OpenTelemetry + Grafana / k6 load test. |
| **NFR-PERF-002** Throughput | Hệ thống xử lý đồng thời ≥ 100 concurrent users mà không degradation: • Trên phần cứng: 2 vCPU, 4GB RAM (single instance). • Horizontal scaling: thêm instance tăng tuyến tính. Đo bằng: k6 smoke test → load test → stress test. |
| **NFR-PERF-003** Cache Effectiveness | Redis Cache hit rate ≥ 80% trong điều kiện steady-state. **Cơ chế cache duy nhất toàn hệ thống là Redis cache-aside** qua `CachingBehavior` / `CacheInvalidationBehavior` (mục 6.3); Output Cache của .NET **không được sử dụng**. Bảng TTL dưới đây là **nguồn sự thật duy nhất** — mọi chương khác chỉ được *tham chiếu*, không được ghi lại con số riêng. |
| **NFR-PERF-004** Database Query | Tất cả queries đến PostgreSQL: • Không có N+1 query problem — bắt buộc dùng `.Include()`/`.ThenInclude()` và projection. • **Index: mọi truy vấn danh sách/tìm kiếm PHẢI dùng Index Scan, chứng minh bằng `EXPLAIN ANALYZE`; không chấp nhận Seq Scan trên bảng `Recipes`.** Ba composite index bắt buộc (mục 7.2): `(IsDeleted, Status, PublishedAt DESC)`, `(Status, CategoryId, PublishedAt DESC)`, `(Status, CookTime)`. • Slow query log: cảnh báo khi query > 100ms — thực thi bởi **EF Core command interceptor**, không phải MediatR behavior. • `EXPLAIN ANALYZE` phải pass review trước khi merge. |

**Bảng TTL cache chuẩn (nguồn sự thật duy nhất):**

| Khóa cache | Đối tượng | TTL | Invalidate khi |
| --- | --- | --- | --- |
| `categories:all` | Danh sách danh mục | **30 phút** | Create / Update / Delete category |
| `categories:detail:{slug}:{queryHash}` | Chi tiết danh mục + recipe thuộc danh mục | **2 phút** | Thay đổi category đó hoặc bất kỳ recipe nào thuộc nó |
| `recipes:list:{queryHash}` | Danh sách công thức công khai | **2 phút** | Bất kỳ thay đổi recipe nào (xóa theo prefix `recipes:list:`) |
| `recipe:{slug}` | Chi tiết một công thức | **5 phút** | Update / Publish / Unpublish / Archive / Unarchive / Delete chính recipe đó |
| `search:{queryHash}` | Kết quả tìm kiếm | **1 phút** | Không invalidate — để hết hạn tự nhiên |

> **[CR-2026 / MT-16, MT-17, MT-26, MT-31, MT-34]** v1.0.0 mô tả **hai kiến trúc cache khác nhau cho cùng một endpoint**: Chương 3 dùng Output Cache (.NET 10) tag-based với TTL 15–60 phút, còn NFR-PERF-003 dùng cache-aside qua Redis với TTL 1–30 phút. v1.1.0 chốt **cache-aside qua Redis** làm cơ chế duy nhất, vì Output Cache mặc định lưu **in-memory** — khi chạy nhiều API instance sau Nginx (NFR-SCALE-003) thì mỗi instance giữ một bản khác nhau và không có cách invalidate xuyên instance. Quan trọng hơn, khóa mặc định của Output Cache chỉ gồm `{path}?{queryString}`, **không phân biệt danh tính người gọi** — chính là gốc rễ của lỗi rò rỉ dữ liệu ở MT-34.
>
> **Cấm tuyệt đối:** không endpoint nào phụ thuộc danh tính được phép xuất hiện trong bảng trên. `GET /recipes/mine` (FR-RCP-011) nằm ngoài bảng này và mang header `Cache-Control: no-store`.
>
> NFR-PERF-004 đổi từ tiêu chí *"mọi cột WHERE/ORDER BY đều có B-tree index"* sang tiêu chí **đo được bằng `EXPLAIN ANALYZE`**. Lý do dài hạn: yêu cầu cũ dẫn tới việc tạo một loạt index đơn cột làm chậm mọi lệnh ghi và tốn dung lượng, trong khi PostgreSQL thường chỉ chọn **một** index cho một truy vấn — phần lớn số index đó nằm không. Ba composite index được chọn theo hình dạng truy vấn thật (mọi truy vấn danh sách đều lọc `Status = Published` trước rồi mới sort), nên phủ đúng nhu cầu với số index ít hơn.
| **NFR-PERF-005** Frontend Performance (Core Web Vitals) | Next.js frontend đạt chuẩn Google Core Web Vitals (đo bằng Lighthouse CI): • LCP (Largest Contentful Paint) ≤ 2.5s. • CLS (Cumulative Layout Shift) ≤ 0.1. • INP (Interaction to Next Paint) ≤ 200ms. • First Load JS Bundle ≤ 200KB (gzipped). Kỹ thuật: ISR (Incremental Static Regeneration), Image Optimization (`next/image`), Code Splitting. |

### 4.2. Bảo mật (NFR-SEC)

Toàn bộ yêu cầu bảo mật tuân thủ OWASP Top 10 (2021) và được kiểm thử qua security review trước khi release production.

> **[CR-2026 / MT-13, MT-14, MT-15, MT-34, MT-38]** Bốn thay đổi bảo mật cốt lõi ở chương này:
>
> 1. **Refresh token 256-bit** (v1.0.0 ghi 512-bit ở FR-AUTH-001 và 128-bit ở NFR-SEC-002 — lệch nhau 4 lần). Chọn 256-bit vì khớp đúng độ dài output SHA-256 dùng để lưu DB (`TokenHash varchar(64)` = 64 ký tự hex) và là mức OWASP khuyến nghị; 512-bit là thừa thãi vì entropy vượt 256-bit không tăng thực chất sau khi hash.
> 2. **Chỉ lưu hash.** Nếu lưu raw token, kẻ đọc được DB (SQL injection, backup rò rỉ, log lộ) có thể mạo danh **mọi** người dùng cho đến khi token hết hạn — đúng rủi ro mà chính NFR-SEC-002 cam kết chặn.
> 3. **Bỏ `IsRevoked`.** Hai cột cùng biểu diễn một trạng thái (`IsRevoked` + `RevokedAt`) là nguồn bug kinh điển: chỉ cần một đường code quên cập nhật một cột là hệ thống có token "đã thu hồi nhưng vẫn dùng được". Một nguồn sự thật duy nhất: `RevokedAt`.
> 4. **`UseForwardedHeaders`.** Khi API đứng sau Nginx mà không cấu hình header chuyển tiếp, `HttpContext.Connection.RemoteIpAddress` trả về IP của **container Nginx** — giống hệt nhau cho mọi người dùng. Hậu quả không phải là "rate limit kém chính xác" mà là **biến biện pháp bảo mật thành lỗi từ chối dịch vụ tự gây ra**: tổng cộng 10 request/phút tới `/auth/*` cho tất cả người dùng cộng lại, chỉ vài người đăng nhập cùng lúc là cả hệ thống nhận 429. Đồng thời `CreatedByIp` mất sạch giá trị audit vì mọi token đều ghi cùng một IP nội bộ. Bắt buộc khai báo đúng dải mạng proxy tin cậy (`KnownIPNetworks` — xem NFR-SEC-003), nếu không client có thể **giả mạo** `X-Forwarded-For` để né rate limit.
>
> **[CR-2026-02 / MT-54, MT-55]** Hai chỗ trong chương này được làm rõ ở v1.2.0 vì cách viết của v1.1.0 dẫn tới hiện thực sai:
>
> - **MT-54 — tên API để khai báo proxy tin cậy.** v1.1.0 ghi "`KnownProxies` giới hạn ở dải mạng Docker" — nhưng `KnownProxies` chỉ nhận **từng địa chỉ IP**, không nhận dải; còn thuộc tính nhận dải quen thuộc trong tài liệu cũ (`KnownNetworks`) đã bị đánh dấu **obsolete trên .NET 10**. Dự án bật `TreatWarningsAsErrors`, nên làm theo đúng chữ của v1.1.0 hoặc theo tài liệu cũ đều dẫn tới hoặc hệ thống không tin được proxy (IP container thay đổi), hoặc **gãy build**. v1.2.0 chỉ đích danh `KnownIPNetworks`.
> - **MT-55 — nơi lưu token phía Frontend.** v1.1.0 quy định refresh token đi trong body (không cookie) nhưng **không nói Frontend được lưu token ở đâu**. Khoảng trống này khiến Buổi 2 lưu cả access token lẫn refresh token vào `localStorage` — nơi bất kỳ script nào (kể cả script bị tiêm qua XSS) đều đọc được. v1.2.0 quy định: access token **chỉ trong bộ nhớ**, refresh token là thứ duy nhất được lưu bền, và CSP được nâng thành yêu cầu tường minh ở NFR-SEC-005. Phương án chuyển refresh token sang cookie `HttpOnly` an toàn hơn trước XSS nhưng **trái §5.2** và kéo theo cơ chế chống CSRF — không chọn trong phạm vi v1.2.0.

| Mã | Nội dung |
| --- | --- |
| **NFR-SEC-001** Password & Hashing | Mật khẩu phải được hash bằng ASP.NET Core Identity mặc định (PBKDF2-HMACSHA512, iteration count ≥ 100.000). Không bao giờ lưu plaintext password. Yêu cầu độ phức tạp: ≥ 8 ký tự, chứa ít nhất 1 chữ hoa + 1 chữ thường + 1 số + 1 ký tự đặc biệt (cấu hình qua `IdentityOptions.Password`). |
| **NFR-SEC-002** JWT Token Security | Access Token: JWT signed bằng HS256, TTL = 15 phút, claim: `userId`, `email`, `roles`, `jti`, **`sid`** (Id của bản ghi `RefreshToken` phát cùng cặp — dùng để nhận biết phiên hiện tại ở FR-AUTH-009). **Lưu trữ phía Frontend:** access token **chỉ giữ trong bộ nhớ** (biến/React state — không ghi `localStorage`, `sessionStorage` hay cookie); refresh token là dữ liệu xác thực **duy nhất** được lưu bền phía client, và khi tải lại trang client dùng nó để lấy access token mới. Rủi ro XSS còn lại đối với refresh token được kiểm soát bằng Rotation + Reuse Detection (bên dưới) và Content-Security-Policy (NFR-SEC-005). Refresh Token: **256-bit (32 bytes) cryptographically secure random bytes** (`RandomNumberGenerator.GetBytes(32)`, encode Base64URL), **hash SHA-256 trước khi lưu DB — DB chỉ chứa `TokenHash`, không bao giờ chứa raw token**, TTL = 7 ngày. Trạng thái thu hồi có **một nguồn sự thật duy nhất**: `RevokedAt IS NULL AND ExpiresAt > NOW()` (không tồn tại cột `IsRevoked`). Rotation: Refresh token bị revoke ngay sau khi dùng, cấp token mới, lưu `ReplacedByTokenHash` để trace token family. Detection: Nếu refresh token đã bị revoke được dùng lại → revoke toàn bộ family (Reuse Detection). |
| **NFR-SEC-003** Rate Limiting | Giới hạn yêu cầu theo IP để ngăn brute force và DDoS: • Auth endpoints (`/auth/*`): 10 request/phút/IP. • API chung: 100 request/phút/IP. • Upload endpoints: 5 request/phút/IP. Implementation: ASP.NET Core Rate Limiting middleware (Fixed Window, sliding window cho auth). HTTP 429 khi vượt giới hạn với `Retry-After` header. **Bắt buộc:** API PHẢI bật **`UseForwardedHeaders`** với `ForwardedHeaders.XForwardedFor \| ForwardedHeaders.XForwardedProto` và chỉ tin header này khi request đến từ **dải mạng Docker nội bộ** — trên .NET 10 khai báo bằng **`ForwardedHeadersOptions.KnownIPNetworks`** (kiểu `System.Net.IPNetwork`, ví dụ `172.16.0.0/12`, cấu hình được qua appsettings). **Không dùng** `KnownProxies` (chỉ nhận từng IP cố định — không phù hợp vì IP container đổi mỗi lần tạo lại) và **không dùng** `KnownNetworks` (đã obsolete trên .NET 10 — với `TreatWarningsAsErrors` sẽ làm gãy build). `UseForwardedHeaders` phải đứng **đầu pipeline**, trước `UseRateLimiter` và `UseAuthentication`. Rate limit và `RefreshToken.CreatedByIp` lấy IP thật từ header `X-Forwarded-For`. Nginx phải set `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for` (mục 6.5). |
| **NFR-SEC-004** Input Validation & File Upload Security | Toàn bộ input được validate tại Application Layer (FluentValidation) TRƯỚC khi xử lý: • SQL Injection: EF Core parameterized queries (không raw SQL với user input). • XSS: Input sanitization + Content-Security-Policy header. • MIME Validation: Đọc magic bytes (không tin vào Content-Type header) khi upload. • File size: Kiểm tra trước khi read stream (không buffer toàn bộ vào memory trước). • Path Traversal: GUID-based filename generation (không dùng tên file của user). |
| **NFR-SEC-005** HTTPS, CORS & CSP | Toàn bộ traffic phải qua HTTPS (TLS 1.2+): • Nginx: redirect HTTP → HTTPS, HSTS header (`max-age=31536000`). • CORS Policy: Chỉ cho phép origin được cấu hình qua appsettings (không wildcard `*`). • Allowed Origins: `http://localhost:3000` (dev), `https://domain.com` (prod). • **Content-Security-Policy** chặt (`script-src 'self'` + nonce cho script inline của Next.js, `object-src 'none'`, `frame-ancestors 'none'`) — lớp phòng vệ cuối cho refresh token lưu phía client (NFR-SEC-002). • Cookie: hệ thống **không dùng cookie** cho xác thực (SRS §5.2); nếu một phiên bản sau chuyển refresh token sang cookie thì bắt buộc `HttpOnly`, `SameSite=Strict`, `Secure=true` và phải qua Change Request. |
| **NFR-SEC-006** Authorization & Resource Ownership | Kiểm tra phân quyền tại Application Layer (không chỉ ở Presentation Layer): • Authorization Handler: `RecipeAuthorizationHandler` xác minh ResourceOwnership (Author chỉ xóa recipe của mình). • Role-based policies: `"AuthorPolicy"`, `"AdminPolicy"` (không hardcode role string). Role của ASP.NET Core Identity **không phân cấp** — `AuthorPolicy` khai báo `RequireRole("Author", "Admin")` và tài khoản Admin được seed **cả hai** role. • Sensitive endpoints (DELETE, PATCH publish): double-check user ID trước khi commit. • Audit trail: Log mọi write operation với userId + timestamp (Serilog). • **Nguyên tắc cách ly cache (bắt buộc):** *Không bao giờ đặt dữ liệu phụ thuộc danh tính người gọi vào cache dùng chung dưới khóa công khai.* Endpoint công khai chỉ phục vụ dữ liệu công khai; dữ liệu riêng tư đi qua endpoint riêng và mang `Cache-Control: no-store`. Kiểm tra phân quyền ở handler là **vô hiệu** nếu response được trả thẳng từ cache trước khi tới handler. |
| **NFR-SEC-007** Secrets Management | Không bao giờ commit secrets vào Git: • Development: ASP.NET Core User Secrets (`dotnet user-secrets`). • Production: Environment variables (Docker Compose `env_file`, quyền đọc giới hạn). • Rotation: Khuyến nghị rotate JWT signing key mỗi 90 ngày. • Scanning: Pre-commit hook kiểm tra với truffleHog/gitleaks. • **Cấm ghi log dữ liệu nhạy cảm:** raw refresh token, password, `Authorization` header và Google `idToken` **không bao giờ** được đưa vào log ở bất kỳ sink nào; raw refresh token chỉ tồn tại trong response HTTP trả về client. |

### 4.3. Khả năng Sử dụng (NFR-USE)

| Mã | Nội dung |
| --- | --- |
| **NFR-USE-001** Responsive Design | Giao diện hiển thị chính xác trên tất cả breakpoints: • Mobile: 320px – 767px (single column, touch-friendly). • Tablet: 768px – 1199px (2-column grid). • Desktop: ≥ 1200px (full layout). Framework: Tailwind CSS utility-first. Không sử dụng CSS framework override. Kiểm thử: Chrome DevTools responsive mode + BrowserStack (iOS, Android). |
| **NFR-USE-002** Accessibility (a11y) | Tuân thủ WCAG 2.1 Level AA: • Semantic HTML5: `<article>`, `<nav>`, `<main>`, `<aside>`. • ARIA attributes: `aria-label`, `aria-expanded`, `role` trên interactive elements. • Keyboard navigation: tất cả chức năng dùng được bằng bàn phím (Tab, Enter, Escape). • Color contrast ratio ≥ 4.5:1 (text) và ≥ 3:1 (UI components). • Screen reader: test với NVDA (Windows) và VoiceOver (macOS/iOS). |
| **NFR-USE-003** Error Messages | Thông báo lỗi phải rõ ràng và actionable: • API: trả về RFC 7807 Problem Details (`type`, `title`, `status`, `detail`, `errors{}`). • Frontend: hiển thị ngay bên cạnh field lỗi (React Hook Form inline validation). • Server errors (5xx): hiển thị thông báo thân thiện, không lộ stack trace. • I18n-ready: error messages sử dụng error code (không hardcode tiếng Việt/Anh). |
| **NFR-USE-004** Loading States | Mọi async operation phải có visual feedback: • Loading skeleton: hiển thị trong khi fetch data (không blank screen). • Optimistic update: UI cập nhật ngay, rollback nếu API fail. • Toast notification: xác nhận thành công/thất bại sau write operation. • Progress indicator: upload ảnh hiển thị progress bar (%) realtime. |

### 4.4. Độ tin cậy (NFR-REL)

| Mã | Nội dung |
| --- | --- |
| **NFR-REL-001** Uptime SLA | Hệ thống có uptime ≥ 99.5% (≈ 3.65 giờ downtime/năm). • Maintenance window: công bố trước 48 giờ qua banner thông báo. • Health check: `/health/ready` được probe mỗi 10 giây bởi khối `healthcheck:` của Docker Compose (`interval: 10s`, `retries: 3`); container unhealthy bị Nginx loại khỏi upstream pool qua `proxy_next_upstream` (mục 6.5). • Monitoring: Uptime Robot / Better Uptime gửi alert khi down > 1 phút. |
| **NFR-REL-002** Error Handling & Resilience | Hệ thống xử lý lỗi gracefully, không crash toàn bộ: • Global Exception Handler Middleware: bắt tất cả unhandled exceptions → trả 500 Problem Details + log. • Database connection pool: tự reconnect, timeout 30s. • Redis failover: nếu Redis down → fallback database (không cache), không throw exception. • Hangfire retry: mỗi job tối đa 3 retry với exponential backoff. • Circuit Breaker: (tùy chọn nâng cao) Polly cho external HTTP calls. |
| **NFR-REL-003** Data Durability | Dữ liệu không bị mất trong trường hợp restart hoặc crash: • PostgreSQL WAL (Write-Ahead Logging): đảm bảo ACID. • Backup: `pg_dump` tự động hàng ngày lúc 03:00 AM, lưu 30 ngày. • MinIO: dữ liệu file trên volume persistent (không ephemeral container storage). • Refresh tokens: lưu DB (không Redis) để survive restart. • Soft delete: Recipe được đánh dấu `IsDeleted` thay vì xóa vật lý. Dữ liệu (bản ghi + ảnh) được **giữ nguyên 30 ngày** và trong khoảng này có thể được **quản trị viên vận hành khôi phục trực tiếp trên cơ sở dữ liệu** (gán lại `IsDeleted = false`); v1.2.0 **không** cung cấp chức năng khôi phục qua giao diện/API (mục 1.2.3). Sau 30 ngày FR-JOB-003 dọn vĩnh viễn cả bản ghi lẫn file MinIO. *(CR-2026-02 / MT-49)* |

### 4.5. Khả năng Bảo trì (NFR-MAINT)

| Mã | Nội dung |
| --- | --- |
| **NFR-MAINT-001** Code Quality | Toàn bộ code phải pass static analysis trước khi merge: • .NET: SonarAnalyzer, StyleCop, EditorConfig (indent, naming conventions). • TypeScript/React: ESLint (Airbnb ruleset), Prettier. • Không có compiler warnings trong build CI. • Code review: ít nhất 1 reviewer phê duyệt Pull Request. |
| **NFR-MAINT-002** Test Coverage | Độ phủ test tối thiểu: • Unit tests: ≥ 80% line coverage (Application layer commands, queries, validators). • Integration tests: tất cả API endpoints có ít nhất 1 happy path + 1 error case. • E2E tests: 5 critical user flows (register, login, create recipe, publish, search). Tool: xUnit (backend), Jest + Testing Library (frontend), Playwright (E2E). |
| **NFR-MAINT-003** Documentation | Tài liệu kỹ thuật bắt buộc: • `README.md`: hướng dẫn setup dev environment (Docker Compose) trong < 5 phút. • API documentation: tự động sinh từ XML comments + Scalar/Swagger UI tại `/scalar`. • Architecture Decision Records (ADR): ghi lại mọi quyết định kiến trúc quan trọng. • `CHANGELOG.md`: cập nhật mỗi release (theo Keep a Changelog + SemVer). |
| **NFR-MAINT-004** Clean Architecture Compliance | Tuân thủ nghiêm ngặt dependency rules của Clean Architecture: • Domain layer: KHÔNG dependency vào bất kỳ layer nào khác và **KHÔNG có bất kỳ NuGet package nào — chỉ .NET BCL**. Phân vai rõ: **FluentValidation** kiểm tra *dữ liệu đầu vào* (định dạng, độ dài, bắt buộc) ở tầng **Application**; **`DomainException`** bảo vệ *bất biến nghiệp vụ* (ví dụ "Recipe phải có ≥ 1 step và ≥ 1 ingredient mới publish được") ở tầng **Domain**, viết bằng `if/throw` thuần. • Application layer: chỉ depend vào Domain. KHÔNG reference Infrastructure. • Infrastructure layer: depend vào Application (implements interfaces). • Vi phạm: được phát hiện qua ArchUnit.NET tests hoặc custom Architecture test project. • CQRS: Commands thay đổi state, Queries đọc data — không trộn lẫn. |

### 4.6. Khả năng Mở rộng (NFR-SCALE)

| Mã | Nội dung |
| --- | --- |
| **NFR-SCALE-001** Stateless Backend | API được thiết kế stateless để hỗ trợ horizontal scaling: • JWT authentication (không session server-side). • Distributed cache (Redis, không in-memory `IMemoryCache`) cho mọi shared state. • Distributed lock (RedLock) cho các recurring job phải chạy đúng một lần trên toàn cluster — cụ thể là **FR-JOB-003 (Permanent Purge Job)**; nếu hai lượt cùng chạy (nhiều worker, hoặc lượt trước chưa xong đã bị kích hoạt lại), chúng sẽ tranh nhau xóa cùng một tập bản ghi. • Hangfire: xử lý job ở **container worker riêng** (FR-JOB, mục 3.6), scale độc lập với API; nhiều worker dùng chung hàng đợi PostgreSQL. Khóa DataProtection dùng chung giữa `api` và `hangfire` qua volume để dữ liệu do một container bảo vệ đọc được ở container kia. |
| **NFR-SCALE-002** Database Scaling | Chiến lược database scaling: • Connection pooling: Npgsql built-in pool (max 100 connections/instance). • Read replica (tùy chọn): EF Core split queries + `IQueryable` routing qua `IDbContextFactory`. • Index strategy: B-tree cho equality/range, GIN cho full-text search (`tsvector`). • Table partitioning: (nâng cao) partition Recipe by CreatedAt khi > 1 triệu rows. |
| **NFR-SCALE-003** Infrastructure Scaling | Hạ tầng có thể scale theo chiều ngang: • Docker: mỗi service là container riêng biệt (API, Hangfire worker, Postgres, Redis, MinIO, Nginx). • Nginx: phân giải tên `api` **động** qua DNS nội bộ Docker (`resolver 127.0.0.11 valid=10s` + `proxy_pass` qua biến) — Docker DNS trả về mọi replica và Nginx tự luân phiên; replica mới được nhận trong ≤ 10 giây mà không cần reload Nginx. **Không dùng khối `upstream { server api:8080; }`** vì nó chỉ phân giải DNS lúc Nginx khởi động (mục 6.5, MT-50). • **`docker-compose.prod.yml` PHẢI bỏ khối `ports` của service `api`** (chỉ expose trong network nội bộ) và khai báo `deploy: replicas: 3`; file dev giữ `5000:8080` để gọi Scalar/Postman trực tiếp. • MinIO: Distributed Mode (4+ nodes) cho production storage scaling. • CDN: static assets (Next.js `_next/static`) được serve qua CDN (Cloudflare). |

> **[CR-2026 / MT-29, MT-30, MT-32]** `docker compose up --scale api=3` với file compose của v1.0.0 sẽ **lỗi ngay lập tức**: ba container không thể cùng bind cổng 5000 của host. Nghĩa là mọi yêu cầu scale ngang trong NFR-SCALE và NFR-PERF-002 đều không thực hiện được với cấu hình đã đặc tả. Tách hai file (dev giữ `ports` cho tiện gọi thẳng API, prod bỏ `ports` + `replicas: 3`) giải quyết được cả hai nhu cầu mà không đánh đổi.
>
> NFR-MAINT-004 bỏ cụm *"ngoài FluentValidation"*: 4/5 vị trí trong tài liệu quy định Domain phải sạch tuyệt đối, và NFR-MAINT-004 yêu cầu **ArchUnit.NET test tự động kiểm tra** — viết test theo câu nào thì build sẽ đỏ theo câu đó, nên mâu thuẫn này buộc phải chốt trước khi viết dòng test đầu tiên.

### 4.7. Tối ưu SEO (NFR-SEO)

| Mã | Nội dung |
| --- | --- |
| **NFR-SEO-001** Structured Data | Mỗi trang công thức nấu ăn phải có JSON-LD Schema.org Recipe markup: • `@type: "Recipe"` • Thuộc tính: `name`, `description`, `image`, `author`, `datePublished`, `prepTime`, `cookTime`, `totalTime`, `recipeYield`, `recipeIngredient[]`, `recipeInstructions[]`, `nutrition`. • Validate: Google Rich Results Test — phải pass 100%. • Kết quả: Rich Snippets trên Google Search (**thời gian nấu, khẩu phần, nguyên liệu, calo**). |
| **NFR-SEO-002** Meta Tags & Open Graph | Mỗi trang phải có đầy đủ: • `<title>`: "{Recipe Name} \| Culinary Blog" — **cắt ở 60 ký tự khi render** (cắt theo ranh giới từ, thêm `…`); đây là quy tắc hiển thị, **không** phải ràng buộc dữ liệu. • `<meta name="description">`: **lấy 160 ký tự đầu của `Recipe.Description`**, cắt theo từ. • Open Graph: `og:title`, `og:description`, `og:image` (1200×630px), `og:url`, `og:type`. • Twitter Card: `summary_large_image`. • Canonical URL: tránh duplicate content (slug-based URL). • Robots: `index, follow` (published) \| `noindex` (draft/archived). |
| **NFR-SEO-003** Sitemap & Robots | Sitemap XML tự động, **do Frontend Next.js sinh** — không phải background job của Backend: • **`app/sitemap.ts`** lấy danh sách slug + `lastmod` qua endpoint gọn nhẹ **`GET /api/v1/recipes/sitemap`** (Chương 8.3 — không phân trang, cache 1 giờ), phục vụ tại đúng `https://domain.com/sitemap.xml`, revalidate theo giờ (ISR). • Bao gồm: tất cả Published recipes + category pages + trang tĩnh. • Format: `sitemap.xml` chuẩn, có `<loc>`, `<lastmod>`, `<changefreq>`, `<priority>`. • **`app/robots.ts`** sinh `robots.txt`: cho phép tất cả crawlers, khai báo Sitemap URL. • **Không ping Google Search Console** — Google đã ngừng hỗ trợ endpoint `ping?sitemap=` từ tháng 6/2023; khai báo sitemap trong `robots.txt` là cách Google khuyến nghị hiện nay. |
| **NFR-SEO-004** URL Structure | URL phải thân thiện SEO: • Recipes: `/recipes/{slug}` — slug là chữ thường, gạch nối, không dấu. • Categories: `/categories/{slug}`. • Slug generation: tự động từ title, unique trong phạm vi các bản ghi chưa soft-delete (partial unique index, mục 7.2). • **Vòng đời slug:** slug **có thể được sinh lại khi recipe còn ở trạng thái `Draft`** (lúc này chưa ai biết URL); **sau lần Publish đầu tiên, slug bị khóa vĩnh viễn** và không bao giờ đổi nữa. **Do đó hệ thống không cần cơ chế 301 redirect.** • **Danh sách slug dành riêng (nguồn duy nhất cho Recipe và Category):** `search`, `mine`, `sitemap`, `new`, `edit` — là các segment literal của route (`/recipes/search`, `/recipes/mine`, `/recipes/sitemap`) hoặc route giao diện (`new`, `edit`). Một slug trùng các từ này sẽ **không bao giờ truy cập được** qua `/recipes/{slug}` vì ASP.NET Core ưu tiên segment literal hơn tham số; khi trùng, hệ thống thêm hậu tố `-2`. *(CR-2026-02 / MT-53)* • Không dùng query params cho nội dung chính (chỉ dùng cho filter/sort/pagination). |

> **[CR-2026 / MT-23, MT-24, MT-27, MT-28, MT-37]** Bốn điều chỉnh, tất cả đều nhằm làm cho NFR-SEO **nghiệm thu được**:
>
> - **Bỏ "star rating".** Danh sách thuộc tính JSON-LD của chính NFR-SEO-001 **không có** `aggregateRating`, mà Rating System lại nằm ngoài phạm vi v1.0 (mục 1.2.3). Không có `aggregateRating` thì Google không bao giờ hiển thị sao — hạng mục này sẽ trượt nghiệm thu không phải vì làm sai mà vì **yêu cầu tự mâu thuẫn với phạm vi**. Nhúng rating giả để "có sao" là vi phạm chính sách structured data của Google, tuyệt đối không làm.
> - **Giới hạn độ dài thành quy tắc render.** `<title>` yêu cầu ≤ 60 ký tự, hậu tố `" | Culinary Blog"` đã chiếm 16 → tên công thức chỉ còn ≤ 44 ký tự, trong khi cột `Title` cho phép 200. Ép người dùng đặt tên món ≤ 44 ký tự để chiều Google là ràng buộc vô lý; cắt chuỗi lúc render mới là đúng chỗ của yêu cầu này. Tương tự với `<meta description>` 150–160 ký tự so với `Description` ≤ 2000.
> - **Slug khóa sau publish.** v1.0.0 yêu cầu 301 redirect từ slug cũ, nhưng **không có bảng hay cột nào lưu slug cũ** — yêu cầu này không có cách nào hiện thực. Khóa slug sau lần publish đầu tiên giải quyết triệt để: link công khai không bao giờ chết, nên không cần lịch sử slug lẫn cơ chế redirect; vẫn cho sửa lỗi chính tả trong giai đoạn Draft.
> - **Sitemap về đúng nơi phục vụ nó** (xem giải thích đầy đủ tại FR-JOB-003).

---

## CHƯƠNG 5. YÊU CẦU GIAO DIỆN NGOÀI

Chương này mô tả tất cả giao diện giữa hệ thống Culinary Blog với các thực thể bên ngoài: người dùng cuối, phần cứng, phần mềm bên thứ ba và giao tiếp mạng. Mọi giao tiếp đều qua HTTPS (TLS 1.2+) trong môi trường production.

### 5.1. Giao diện Người dùng (UI)

Hệ thống cung cấp giao diện web duy nhất trên nền Next.js App Router, hoạt động như Single Page Application (SPA) với Server-Side Rendering (SSR) và Incremental Static Regeneration (ISR).

| Màn hình / Route | Mô tả | Loại Rendering | Yêu cầu Auth |
| --- | --- | --- | --- |
| `/` | Trang chủ: **danh sách công thức mới xuất bản** (`GET /recipes?sortBy=publishedAt&sortOrder=desc&pageSize=8`) + categories | ISR (revalidate=120) | Không |
| `/recipes` | Danh sách tất cả recipes với filter/sort/search | SSR (dynamic) | Không |
| `/recipes/[slug]` | Chi tiết recipe: ingredients, steps, nutrition, JSON-LD | ISR (revalidate=300) | Không |
| `/categories` | Danh sách category | ISR (revalidate=1800) | Không |
| `/categories/[slug]` | Danh sách recipe theo category (phân trang qua `?page=`) | **SSR + Data Cache** (`fetch` với `revalidate: 120`) | Không |
| `/auth/login` | Form đăng nhập (email/password + Google OAuth button) | CSR | Không (redirect nếu đã login) |
| `/auth/register` | Form đăng ký tài khoản mới | CSR | Không |
| `/dashboard` | Trang tổng quan của Author/Admin | CSR | Bắt buộc (Author/Admin) |
| `/dashboard/recipes` | Quản lý danh sách recipe của user — gọi **`GET /recipes/mine`** (FR-RCP-011), không dùng `GET /recipes` | CSR | Bắt buộc |
| `/dashboard/recipes/new` | Form tạo recipe mới (multi-step wizard) | CSR | Bắt buộc (Author/Admin) |
| `/dashboard/recipes/[id]/edit` | Form chỉnh sửa recipe | CSR | Bắt buộc (Owner/Admin) |
| `/dashboard/categories` | Quản lý categories (chỉ Admin) | CSR | Bắt buộc (Admin) |
| `/dashboard/users` | Danh sách người dùng + khóa / mở khóa tài khoản (FR-AUTH-008) | CSR | Bắt buộc (Admin) |
| `/profile` | Xem và chỉnh sửa thông tin cá nhân; tab "Phiên đăng nhập" (FR-AUTH-009) | CSR | Bắt buộc |
| `/search` | Trang kết quả full-text search | SSR | Không |

> **[CR-2026 / MT-23, MT-33.3, MT-34]** Trang chủ v1.0.0 hứa *"danh sách recipe nổi bật"* nhưng bảng `Recipes` **không có** cột `IsFeatured`/`ViewCount`/`Rating` nào, và `GET /recipes` không có tham số nào lấy "nổi bật" — Frontend không có cách nào dựng đúng đặc tả. v1.1.0 định nghĩa lại "nổi bật" = **mới xuất bản nhất**, dùng ngay index `IDX_Recipe_PublishedAt` có sẵn: không thêm cột, không thêm endpoint, không đụng mục tiêu p95 ≤ 500ms (phương án đếm lượt xem sẽ tạo write nóng mỗi lượt truy cập, phá cả cache lẫn hiệu năng).
>
> Giá trị `revalidate` của ISR được đồng bộ **≤ TTL cache API tương ứng** ở bảng NFR-PERF-003 — nếu ISR dài hơn TTL thì tầng Next.js trở thành tầng cache cũ nhất và mọi nỗ lực invalidate ở Backend đều vô nghĩa. Riêng `/dashboard/*` là CSR và gọi endpoint riêng tư nên không có tầng cache nào.
>
> **[CR-2026-02 / MT-57]** v1.1.0 ghi `/categories/[slug]` là **ISR**, nhưng trang này phân trang bằng tham số truy vấn `?page=` — trong Next.js App Router, một trang đọc `searchParams` **bắt buộc render động theo request** và không thể dựng tĩnh (ISR). Buổi 2 đã gặp đúng điều này khi build (`BAO_CAO_BUOI_2.md` §4.5). Yêu cầu thật sự đằng sau chữ "ISR" là *dữ liệu không được cũ hơn TTL của API*; v1.2.0 diễn đạt đúng cơ chế đạt được yêu cầu đó: **SSR + Data Cache** với `fetch(..., { next: { revalidate: 120 } })`. Quy tắc "thời gian tái sinh ≤ TTL cache API" ở trên áp dụng cho **cả** `revalidate` của ISR lẫn `revalidate` của Data Cache. Phương án chuyển phân trang sang segment đường dẫn (`/categories/[slug]/page/[n]`) để giữ ISR thuần bị loại vì làm URL phức tạp và phải sinh trước số trang chưa biết trước.

### 5.2. Giao diện Phần mềm – REST API

Backend cung cấp RESTful API theo chuẩn JSON. Toàn bộ endpoints được tiền tố `/api/v1`. Xem chi tiết tại Chương 8.

| Hạng mục | Nội dung |
| --- | --- |
| Giao thức | HTTP/1.1 và HTTP/2 qua HTTPS (TLS 1.2+). Nginx termination SSL. |
| Base URL (dev) | `http://localhost:5000/api/v1` |
| Base URL (prod) | `https://api.culinaryblog.com/api/v1` |
| Content-Type | `application/json; charset=utf-8` (request và response). `multipart/form-data` cho file upload endpoints. |
| Authentication | Bearer Token trong Authorization header: `Authorization: Bearer <access_token>`. Refresh token: trong request body (không dùng cookie để tránh CSRF). |
| Response Format | **Đối tượng đơn:** trả thẳng DTO (ví dụ `RecipeDetailDto`). **Danh sách phân trang:** `PagedResult<T>` = `{ "items": [...], "page": 1, "pageSize": 12, "totalCount": 100, "totalPages": 9, "hasNextPage": true, "hasPreviousPage": false }` (FR-SRCH-004). **Lỗi:** RFC 7807 Problem Details `{ "type", "title", "status", "detail", "errors":{} }`. *(CR-2026-02 / MT-42)* |
| Versioning | URL Path versioning: `/api/v1/`. Khi có breaking changes → `/api/v2/` (v1 được duy trì tối thiểu 6 tháng). |
| CORS Headers | `Access-Control-Allow-Origin: <configured-origins>`; `Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS`; `Access-Control-Allow-Headers: Content-Type, Authorization, X-Correlation-ID` |
| Rate Limit Headers | `X-RateLimit-Limit: 100`; `X-RateLimit-Remaining: 87`; `X-RateLimit-Reset: 1700000000` (Unix timestamp); `Retry-After: 30` (seconds, khi 429) |
| Correlation ID | `X-Correlation-ID` header: sinh tự động nếu không có trong request, trả về trong response. Gán vào tất cả log entries (Serilog MDC). |

> **[CR-2026-02 / MT-42]** v1.1.0 mô tả **hai hình dạng response danh sách**: mục này và quy ước Chương 8 ghi `{ "data": [...], "meta": { page, pageSize, total, totalPages } }` (kèm `pageSize: 10` lệch với mặc định 12 ở mọi chỗ khác), trong khi FR-RCP-001, FR-SRCH-004 và code Buổi 2 dùng `PagedResult { items, totalCount, totalPages, hasNextPage, hasPreviousPage }`. Mâu thuẫn này sót khỏi cả 41 MT của CR-2026. v1.2.0 chốt **`PagedResult`**: (1) FR-SRCH-004 là yêu cầu chi tiết và đòi đúng các trường `hasNextPage`/`hasPreviousPage` mà hình dạng `{ data, meta }` không có; (2) code và Frontend Buổi 2 đã dùng nó — đổi sang `{ data, meta }` là thay đổi phá vỡ mọi màn hình danh sách mà không đem lại lợi ích chức năng nào; (3) đối tượng đơn trả thẳng DTO, không bọc `data`, để tránh lớp vỏ thừa ở mọi response.

### 5.3. Giao diện Dịch vụ Bên thứ ba

| Dịch vụ | Mục đích | Giao thức / SDK | Cấu hình / Secrets |
| --- | --- | --- | --- |
| Google OAuth 2.0 | Đăng nhập / đăng ký bằng tài khoản Google | **ID Token flow:** Frontend dùng Google Identity Services lấy `idToken`, gửi lên `POST /auth/google`; Backend verify bằng `Google.Apis.Auth` → `GoogleJsonWebSignature.ValidateAsync` (kiểm tra chữ ký, `aud == GoogleClientId`, `iss`, `exp`). **Backend không có redirect URI, không dùng Authorization Code + PKCE.** Scopes: `openid`, `email`, `profile`. | `GoogleClientId` (bắt buộc, dùng để kiểm tra `aud`). Backend **không cần** `GoogleClientSecret` vì không chạy code exchange. Google Cloud Console → OAuth 2.0 Client ID. |
| MinIO (S3-compatible) | Lưu trữ file ảnh công thức | AWS SDK for .NET (`AWSSDK.S3`). Endpoint override cho MinIO. Presigned URL cho direct browser upload (optional). | `MinIO__Endpoint`, `MinIO__AccessKey`, `MinIO__SecretKey`, `MinIO__BucketName`. Docker service: `minio:9000`. |
| Hangfire | Background job processing | NuGet: `Hangfire.Core`, `Hangfire.AspNetCore`, `Hangfire.PostgreSql`. Job server chạy ở **container worker riêng** (`hangfire`); container `api` chỉ enqueue job và phục vụ Dashboard (mục 3.6, MT-47). Dashboard `/hangfire` được bảo vệ bằng **HTTP Basic Auth tại tầng Nginx** (`auth_basic` + `htpasswd`), **không dùng JWT** — JWT Bearer không khả dụng cho điều hướng trình duyệt. | Dùng chung ConnectionString với PostgreSQL. `HANGFIRE_SCHEMA = hangfire`. Thông tin đăng nhập dashboard quản lý riêng qua file `htpasswd`, không dùng tài khoản Admin của ứng dụng. |
| Serilog + Seq | Structured logging & log aggregation | `Serilog.Sinks.Console` (JSON), `Serilog.Sinks.File`, `Serilog.Sinks.Seq`. HTTP ingest API. | `Seq__ServerUrl = http://seq:5341` (Docker). Production: Elastic / Azure Monitor. |
| OpenTelemetry | Distributed tracing & metrics | OpenTelemetry .NET SDK. OTLP exporter. Tracing: HttpClient, EF Core, AspNetCore. | `OTEL_EXPORTER_OTLP_ENDPOINT`. Development: Seq OTLP. Production: Grafana Tempo / Jaeger. |
| SMTP / Email | Gửi welcome email (FR-JOB-001) | MailKit qua interface **`IEmailService`** (không đặt tên `IEmailSender` để tránh trùng với `IEmailSender` của ASP.NET Core Identity). Kết nối qua SMTP với TLS. | `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`. Development: Mailhog (Docker). |

> **[CR-2026 / MT-11, MT-33.1, MT-39]** Ba chỗ trong v1.0.0 mô tả **ba kiến trúc Google OAuth khác nhau**: FR-AUTH-003 nói Authorization Code + PKCE với Auth.js xử lý callback, mục 5.3 khai báo redirect URI ở Backend, Chương 8 nhận `{ idToken }` (tức Implicit/ID-token flow). Thêm nữa, `ExternalLoginInfo` mà FR-AUTH-003 yêu cầu Frontend gửi lên là **kiểu nội bộ của ASP.NET Core Identity — không serialize qua HTTP được**, nên đặc tả đó không khả thi. v1.1.0 chốt **ID Token flow**: đơn giản nhất, Backend stateless chỉ cần `GoogleClientId`, không phải quản lý state/PKCE verifier hay vòng redirect qua lại. Điều kiện an toàn bắt buộc: Backend **phải tự verify chữ ký** ID Token, tuyệt đối không tin dữ liệu người dùng Frontend gửi lên.
>
> Dòng **Google Search Console đã bị xóa** khỏi bảng này — endpoint ping đã bị Google khai tử (xem FR-JOB-003 và NFR-SEO-003).
>
> Hangfire Dashboard là một **trang HTML mở trực tiếp trong trình duyệt**: khi Admin gõ `https://domain.com/hangfire`, trình duyệt gửi request điều hướng thông thường — **không có cách nào đính kèm header `Authorization: Bearer`**, mà mục 5.2 lại loại bỏ cookie để tránh CSRF. Kết quả ở v1.0.0: `IDashboardAuthorizationFilter` luôn thấy người dùng ẩn danh, **Admin không bao giờ vào được dashboard** dù mục 2.3 liệt kê đây là quyền của Admin. Basic Auth ở tầng Nginx giải quyết bằng 3 dòng cấu hình, không đụng gì tới JWT của ứng dụng và không mở lại cánh cửa cookie.

### 5.4. Giao diện Phần cứng

Hệ thống là web application, không giao tiếp trực tiếp với phần cứng chuyên biệt. Yêu cầu phần cứng tối thiểu cho server:

| Thành phần | Development (local) | Production (minimum) |
| --- | --- | --- |
| CPU | 2 cores (Intel/AMD/ARM64 — Apple M-series được hỗ trợ qua Docker) | 2 vCPU (VPS/Cloud instance, x86_64) |
| RAM | 8 GB (chạy Docker Compose đầy đủ: API + PG + Redis + MinIO + Seq) | 4 GB (API + dependencies riêng lẻ) |
| Storage | 20 GB SSD (cho Docker images + database data + MinIO volumes) | 50 GB SSD (production data growth) |
| Network | Kết nối internet (npm/nuget packages, Google OAuth) | Bandwidth ≥ 1 Gbps, IP tĩnh |
| Browser Client | Chrome 112+, Firefox 113+, Safari 16+, Edge 112+ (ES2020+) | Tương tự — không hỗ trợ IE11 |

---

## CHƯƠNG 6. KIẾN TRÚC HỆ THỐNG

Chương này mô tả tổng quan kiến trúc phần mềm của hệ thống Culinary Blog. Hệ thống được thiết kế theo mô hình Client-Server với hai tầng riêng biệt: Frontend (Next.js) và Backend (.NET 10 Minimal API), giao tiếp qua REST API. Backend tuân thủ nguyên tắc Clean Architecture kết hợp CQRS pattern.

### 6.1. Tổng quan Kiến trúc

| Tầng | Technology | Vai trò | Giao tiếp với |
| --- | --- | --- | --- |
| Client (Browser/Mobile) | Browser (Chrome/Firefox/Safari) | Người dùng tương tác qua giao diện web | Next.js App |
| Frontend | **Next.js 15** App Router, TypeScript, Tailwind CSS, **Google Identity Services** (lấy `idToken`), TanStack Query, React Hook Form + Zod | Rendering UI, route management, client-side state. SSR/ISR cho SEO. Sinh `sitemap.xml` và `robots.txt` (NFR-SEO-003). | Backend REST API |
| Nginx Reverse Proxy | Nginx Alpine (Docker) | SSL termination, load balancing bằng **phân giải DNS động** (`resolver 127.0.0.11` + `proxy_pass` qua biến — không dùng khối `upstream`, xem 6.5), static file caching, phục vụ ảnh công khai tại `/media/` (proxy tới MinIO), chuyển tiếp `X-Forwarded-For`, Basic Auth cho `/hangfire`. | Frontend :3000, Backend API qua network nội bộ `api:8080` |
| Backend API | ASP.NET Core .NET 10 Minimal API | Business logic, authentication, data access; **enqueue** background job (không xử lý job). | PostgreSQL, Redis, MinIO, Email |
| Background Worker | Container `hangfire` — cùng image với Backend API, `Hangfire__WorkerOnly=true` | Xử lý mọi background job (FR-JOB-001/002/003), tách tải nặng CPU/I-O khỏi luồng phục vụ request (MT-47). | PostgreSQL (hàng đợi), MinIO, SMTP |
| Cache Layer | Redis 7 | Distributed cache cho recipe/category/search results. Rate limiting counters. | Backend API |
| Object Storage | MinIO (S3-compatible) | Lưu file ảnh: original, medium (800×600), thumbnail (300×300). | Backend API (via `AWSSDK.S3`) |
| Database | PostgreSQL 16 | Persistent relational data storage. Full-text search via `tsvector`. | Backend API (via EF Core) |
| Observability | Serilog + Seq, OpenTelemetry + Grafana/Jaeger | Logging, metrics, distributed tracing. | Backend API |

### 6.2. Kiến trúc Backend – Clean Architecture

Backend tuân thủ Clean Architecture (Robert C. Martin) với nguyên tắc Dependency Rule: dependency chỉ đi vào trong (hướng Domain). Không bao giờ có reference từ Domain/Application ra Infrastructure.

| Tầng | Nội dung |
| --- | --- |
| **Domain Layer** (`CulinaryBlog.Domain`) | Nhân lõi hệ thống. Chứa: • Entities: `Recipe`, `Category`, `ApplicationUser`, `RecipeStep`, `RecipeIngredient`, `RecipeImage`. • Value Objects: `Slug`, `EmailAddress`. • Owned Entities: `RecipeNutrition`. • Domain Events (optional): `RecipePublishedEvent`. • Enums: `RecipeDifficulty`, `RecipeStatus`. • Interfaces: `IRepository<T>`, `IRecipeRepository`, `ICategoryRepository`. • Không có NuGet dependencies (chỉ .NET BCL). |
| **Application Layer** (`CulinaryBlog.Application`) | Orchestration Layer. Chứa: • Commands (CQRS write): `CreateRecipeCommand`, `PublishRecipeCommand`, `LoginCommand`... • Queries (CQRS read): `GetRecipesQuery`, `GetRecipeBySlugQuery`... • Handlers (MediatR `IRequestHandler`): xử lý logic business cho mỗi command/query. • DTOs / Response models: `RecipeDto`, `UserDto`, `PagedResult<T>`. • Validators (FluentValidation): validation rules cho mỗi command. • Pipeline Behaviors (đúng 4, khớp mục 6.3): `LoggingBehavior`, `ValidationBehavior`, `CachingBehavior`, `CacheInvalidationBehavior`. **Không có `PerformanceBehavior` riêng** — việc đo thời gian và cảnh báo > 500ms là trách nhiệm tự nhiên của `LoggingBehavior`. • Service interfaces: `IEmailService`, `IJwtService`, `IFileStorageService`, `ICurrentUser`. |
| **Infrastructure Layer** (`CulinaryBlog.Infrastructure`) | Implements application interfaces. Chứa: • EF Core: `CulinaryBlogDbContext`, configurations, migrations, repositories. • Repository implementations: `RecipeRepository` (LINQ + EF Core + FTS), `CategoryRepository`. • JWT Service: `JwtService` (`System.IdentityModel.Tokens.Jwt`). • File Storage: `MinioFileStorageService` (`AWSSDK.S3`). • Email: `MailKitEmailService`. • Cache: `RedisCacheService` (`StackExchange.Redis`). • Hangfire job registrations. • EF Core Interceptors: `AuditInterceptor` (auto set CreatedAt/UpdatedAt). |
| **Presentation Layer** (`CulinaryBlog.API`) | HTTP interface. Chứa: • Minimal API Endpoint Groups: `AuthEndpoints`, `RecipesEndpoints`, `CategoriesEndpoints`. • Middleware: `GlobalExceptionMiddleware`, `CorrelationIdMiddleware`, `RateLimitingMiddleware`. • DI Configuration: `Program.cs` + Extension methods (`AddApplication`, `AddInfrastructure`, `AddPresentation`). • OpenAPI: Scalar UI tại `/scalar`, XML documentation comments. • Authentication: JWT Bearer; Google ID Token được verify bằng `Google.Apis.Auth` (không dùng ASP.NET Google provider, không có redirect callback). • `UseForwardedHeaders` với `KnownIPNetworks` = dải mạng Docker nội bộ (không dùng `KnownProxies`/`KnownNetworks` — xem NFR-SEC-003), đặt **đầu pipeline**, trước middleware rate limiting và authentication, để lấy đúng IP thật. |

### 6.3. CQRS + MediatR Pipeline

CQRS (Command Query Responsibility Segregation) tách biệt read và write models. Mỗi request đi qua MediatR Pipeline Behaviors theo thứ tự:

| Thứ tự | Pipeline Behavior | Trách nhiệm | Áp dụng cho |
| --- | --- | --- | --- |
| 1 | `LoggingBehavior` | Log request type, parameters, elapsed time. Cảnh báo nếu > 500ms. | Tất cả Commands và Queries |
| 2 | `ValidationBehavior` | Chạy FluentValidation validators đã đăng ký. Throw `ValidationException` nếu có lỗi. | Tất cả Commands và Queries có Validator |
| 3 | `CachingBehavior` | Kiểm tra Redis cache trước khi xử lý. Implements `ICacheable` interface trên Query. | Queries implements `ICacheable` (GET endpoints) |
| 4 | Handler (`IRequestHandler`) | Thực thi business logic: gọi repositories, raise domain events, tạo response DTO. | Tất cả (bắt buộc) |
| 5 | `CacheInvalidationBehavior` | Xóa cache liên quan sau khi Command thành công, theo cột "Invalidate khi" của bảng TTL (NFR-PERF-003). Implements `ICacheInvalidator`. | Commands thay đổi data (Create/Update/Delete) |

> **[CR-2026 / MT-31]** v1.0.0 liệt kê hai danh sách behavior lệch nhau: mục 6.2 có `PerformanceBehavior` nhưng không có `CacheInvalidationBehavior`, mục 6.3 thì ngược lại. v1.1.0 chốt **4 behavior** theo đúng thứ tự bảng trên, gộp việc đo hiệu năng vào `LoggingBehavior` — ít lớp hơn và đúng thực tế, vì đo thời gian vốn là việc tự nhiên của logging.
>
> Hai ngưỡng cảnh báo thuộc **hai chủ thể khác nhau**, không được nhầm lẫn: **> 500ms** cho toàn bộ request là của `LoggingBehavior` (FR-OBS-002); **> 100ms** cho một truy vấn database là của **EF Core command interceptor** (NFR-PERF-004).

### 6.4. Mô hình Quan hệ Thực thể (ERD tóm tắt)

Hệ thống sử dụng PostgreSQL 16 với EF Core Code First. Tất cả entities kế thừa `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted, RowVersion).

| Thực thể | Quan hệ | Bảng PostgreSQL |
| --- | --- | --- |
| Recipe | Nhiều RecipeStep (1:N); Nhiều RecipeIngredient (1:N); Nhiều RecipeImage (1:N); Một RecipeNutrition (1:1 **Owned Entity**); Một Category (N:1); Một Author/ApplicationUser (N:1) | `Recipes`, `RecipeSteps`, `RecipeIngredients`, `RecipeImages` (**bảng riêng, 1:N**), `Categories`, `AspNetUsers`. **`RecipeNutrition` không có bảng riêng** — các cột `Nutrition_*` nằm ngay trong `Recipes` (mục 7.2.1) |
| ApplicationUser | Nhiều Recipe (Author, 1:N); Nhiều RefreshToken (1:N) | `AspNetUsers` (Identity), `RefreshTokens` |
| Category | Nhiều Recipe (1:N) | `Categories` |
| RefreshToken | Một ApplicationUser (N:1) | `RefreshTokens` |

### 6.5. Triển khai – Docker Compose

Toàn bộ hệ thống được containerized với Docker Compose. Development dùng `docker-compose.yml`, Production dùng `docker-compose.prod.yml` với optimized build + secrets management. Docker Compose được dùng cho **local dev, staging và production**.

| Service | Image | Port (host:container) | Healthcheck | Volume / Dependency |
| --- | --- | --- | --- | --- |
Hệ thống gồm **7 service chạy ở mọi môi trường** (`nginx`, `api`, `hangfire`, `frontend`, `postgres`, `redis`, `minio`) và **2 service chỉ dùng khi phát triển** (`seq`, `mailhog`).

| Service | Image | Port (host:container) | Healthcheck | Volume / Dependency |
| --- | --- | --- | --- | --- |
| nginx | `nginx:alpine` | 80:80, 443:443 | — | Depends: `api`, `frontend` (`condition: service_healthy`). Volume: `./nginx/templates/` (cấu hình, nạp secret qua `envsubst`), `./nginx/htpasswd` (không commit), `./ssl/` |
| api | `culinaryblog-api` (Dockerfile) | **dev: 5000:8080 · prod: không map port** (chỉ trong network nội bộ) | `CMD curl -fsS http://127.0.0.1:8080/health/ready`<br>`interval: 10s`, `timeout: 3s`, `retries: 3`, `start_period: 30s`<br>*(image `aspnet:10.0` không có sẵn `curl` — cài ở stage runtime của Dockerfile)* | Depends: postgres, redis, minio (`condition: service_healthy`). Env: `Hangfire__ServerEnabled=false` (chỉ enqueue). **prod:** `deploy: replicas: 3` |
| hangfire | `culinaryblog-api` (cùng image) | — (không nhận HTTP từ bên ngoài) | `CMD curl -fsS http://127.0.0.1:8080/` | Env: `Hangfire__WorkerOnly=true`. Depends: postgres, redis (`condition: service_healthy`). Volume `dpkeys` dùng chung với `api` (khóa DataProtection). **prod:** 1 replica, scale độc lập khi cần |
| frontend | `culinaryblog-web` (Dockerfile) | 3000:3000 | `CMD wget -qO- http://127.0.0.1:3000 >/dev/null`<br>*(`node:22-alpine` chỉ có `wget` của busybox)* | Depends: api |
| postgres | `postgres:16-alpine` | 5432:5432 | `CMD-SHELL pg_isready -h 127.0.0.1 -U $POSTGRES_USER -d $POSTGRES_DB`<br>`interval: 10s`, `retries: 5`<br>*(bắt buộc `-h 127.0.0.1` để kiểm qua TCP)* | Volume: `pgdata:/var/lib/postgresql/data`. Env: `POSTGRES_DB`, `USER`, `PASSWORD` |
| redis | `redis:7-alpine` | 6379:6379 | `CMD redis-cli ping`<br>`interval: 10s`, `retries: 5` | Volume: `redisdata:/data`. Command: `redis-server --appendonly yes` |
| minio | `minio/minio` (ghim phiên bản cụ thể) | dev: 9000:9000, 9001:9001 (Console) · prod: không map port (ảnh phục vụ qua Nginx `/media/`) | `CMD mc ready local`<br>*(image MinIO không có `curl`)* | Volume: `miniodata:/data`. Command: `server /data --console-address :9001` |
| seq | `datalust/seq` (ghim phiên bản cụ thể) | 5341:80 | — | Volume: `seqdata:/data`. **Dev only** — không deploy production |
| mailhog | `mailhog/mailhog` | 8025:8025 (UI), 1025:1025 (SMTP) | — | **Dev only** — test email |

**Cấu hình Nginx bắt buộc:**

```nginx
# DNS nội bộ của Docker: phân giải lại tên service mỗi 10 giây.
# KHÔNG dùng "upstream { server api:8080; }" — khối upstream chỉ phân giải DNS MỘT LẦN
# lúc Nginx khởi động; container api được tạo lại (đổi IP) sẽ làm Nginx trả 502 (MT-50).
resolver 127.0.0.11 valid=10s ipv6=off;

server {
    # proxy_pass dùng BIẾN để buộc Nginx phân giải qua resolver ở runtime.
    # Khi --scale api=3, Docker DNS trả 3 bản ghi A và Nginx tự luân phiên giữa chúng.
    set $api_upstream      http://api:8080;
    set $frontend_upstream http://frontend:3000;
    set $minio_upstream    http://minio:9000;

    # Chuyển tiếp IP thật cho rate limiting và audit (NFR-SEC-003)
    proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Real-IP         $remote_addr;

    location /api/ {
        proxy_pass $api_upstream;
        # Chuyển sang instance khác khi một instance lỗi (NFR-REL-001)
        proxy_next_upstream error timeout http_502 http_503 http_504;
    }

    # Ảnh công khai: một địa chỉ duy nhất cho cả trình duyệt lẫn container frontend
    location /media/ {
        proxy_pass $minio_upstream/culinary-blog/;
        expires 30d;
    }

    # Hangfire Dashboard: Basic Auth tại Nginx, không dùng JWT (MT-39)
    location /hangfire {
        auth_basic           "Hangfire Dashboard";
        auth_basic_user_file /etc/nginx/htpasswd;
        # Header bí mật chỉ Nginx biết — Dashboard từ chối mọi request không có nó,
        # kể cả khi ai đó gọi thẳng cổng của api mà không đi qua Nginx.
        proxy_set_header X-Hangfire-Gate ${HANGFIRE_GATE_SECRET};
        proxy_pass $api_upstream;
    }

    location / {
        proxy_pass $frontend_upstream;
    }
}
```

> **[CR-2026-02 / MT-50, MT-51]** Hai lỗi kỹ thuật của v1.1.0 được sửa ở mục này. Cả hai đều thuộc loại nguy hiểm nhất: **làm đúng từng chữ của đặc tả thì hệ thống hỏng khi chạy thật.**
>
> - **MT-50 — khối `upstream` gây 502 (🔴).** Cấu hình Nginx của v1.1.0 dùng `upstream api_pool { server api:8080; }`. Nginx bản open-source phân giải tên trong khối `upstream` **một lần duy nhất lúc khởi động**. Buổi 2 đã gặp đúng lỗi này khi chạy thật (`BAO_CAO_BUOI_2.md` §5.1 lỗi #2): container `api` được tạo lại và đổi IP từ `172.30.2.3` sang `172.30.2.2` → Nginx vẫn gửi request tới IP cũ → **toàn bộ `/api/*` trả 502** cho tới khi khởi động lại Nginx. Lỗi đã được sửa trong code (commit `064f582`), nhưng v1.1.0 lại đặc tả ngược về cách cũ — ai làm theo SRS sẽ tái tạo nguyên vẹn sự cố. Cơ chế `resolver` + biến vừa sửa tận gốc vừa **tốt hơn cho chính mục tiêu scale ngang** (NFR-SCALE-003): khi tăng từ 3 lên 5 replica, Nginx tự nhận 2 replica mới trong 10 giây mà không cần reload. Phương án thay thế `server api:8080 resolve;` trong khối `upstream` chỉ có ở Nginx Plus hoặc bản open-source rất mới, nên không được chọn làm chuẩn.
> - **MT-51 — lệnh healthcheck không chạy được (🟡).** v1.1.0 dùng `curl` cho `api`, `frontend`, `minio`: image `aspnet:10.0` **không có** `curl`, `minio/minio` **không có** `curl`, `node:22-alpine` **chỉ có** `wget` của busybox — một healthcheck không chạy được sẽ khiến container luôn `unhealthy`, kéo theo mọi service `depends_on: service_healthy` không bao giờ khởi động. Ngoài ra `pg_isready` không có `-h` kiểm tra qua unix socket và báo "ready" **khi Postgres chưa mở cổng TCP** — chính là nguyên nhân lỗi `57P03 the database system is starting up` Buổi 2 đã gặp (§5.1 lỗi #1). v1.2.0 dùng đúng các lệnh đã được kiểm chứng trên hệ thống thật.
> - Đồng thời bổ sung service **`hangfire`** (worker — MT-47) vào bảng, đường dẫn **`/media/`** để ảnh có một địa chỉ công khai duy nhất (trước đây URL `localhost:9000` truy cập được từ trình duyệt nhưng **không** từ container frontend, khiến tối ưu ảnh của Next.js không chạy được), và **ghim phiên bản** thay cho tag `latest` để bản build tái lập được.

> **[CR-2026 / MT-29, MT-32, MT-33.7, MT-38, MT-39]** Ba bổ sung ở mục này đều nhằm làm cho các NFR đã cam kết **có người thực thi**:
>
> - Khối **`healthcheck:`** là thứ duy nhất khiến `/health/ready` có tác dụng thật trong môi trường Docker Compose — nếu không có nó, NFR-REL-001 và FR-OBS-001 chỉ là endpoint không ai gọi.
> - **Bỏ `ports` của `api` ở prod** là điều kiện cần để `--scale api=3` chạy được (xem NFR-SCALE-003); giữ ở dev để gọi thẳng Scalar/Postman cho nhanh.
> - **`proxy_set_header X-Forwarded-For`** là nửa còn lại của MT-38: bật `UseForwardedHeaders` ở API mà Nginx không gửi header thì cũng vô nghĩa.

---

## CHƯƠNG 7. MÔ HÌNH DỮ LIỆU

Chương này đặc tả cấu trúc dữ liệu đầy đủ của hệ thống Culinary Blog. Tất cả entities kế thừa `BaseEntity` và sử dụng Soft Delete pattern (`IsDeleted` flag). Database: PostgreSQL 16 với EF Core 10 Code First.

### 7.1. BaseEntity (Abstract)

Tất cả thực thể kế thừa từ `BaseEntity`. Không tạo bảng riêng (Table-Per-Hierarchy không được dùng ở đây — mỗi entity có bảng riêng với các cột kế thừa).

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` (Guid) | PRIMARY KEY, DEFAULT `gen_random_uuid()` | Khóa chính UUID v4 — tránh sequential ID guessing. |
| CreatedAt | `timestamptz` | NOT NULL, DEFAULT `NOW()` | Thời điểm tạo bản ghi. Set bởi `AuditInterceptor` (EF Core). |
| UpdatedAt | `timestamptz` | NULL | Thời điểm cập nhật cuối. Set bởi `AuditInterceptor` khi SaveChanges. |
| IsDeleted | `boolean` | NOT NULL, DEFAULT `false` | Soft delete flag. Global Query Filter: `.Where(x => !x.IsDeleted)`. |
| RowVersion | `bytea` (timestamp) | NOT NULL, Concurrency Token | Optimistic concurrency control. EF Core `[Timestamp]` annotation. |

### 7.2. Recipe

Thực thể trung tâm của hệ thống. Một Recipe thuộc một Category và một Author. Chứa Owned Entity `RecipeNutrition` và các Collection Navigation Properties.

| Column | Kiểu dữ liệu | Ràng buộc | Index | Mô tả |
| --- | --- | --- | --- | --- |
| Id | `uuid` | PK (kế thừa) | PK | (BaseEntity) |
| Title | `varchar(200)` | NOT NULL | `IDX_Recipe_Title` (GIN trigram — optional) | Tiêu đề công thức. Unique không bắt buộc (có thể trùng title khác nhau slug). |
| Slug | `varchar(220)` | NOT NULL, **PARTIAL UNIQUE** | `IDX_Recipe_Slug` — `CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;` | URL-friendly identifier. Sinh từ Title + chuẩn hóa (lowercase, bỏ dấu, replace space → `-`). Sinh lại được khi `Status = Draft`; **khóa vĩnh viễn sau lần Publish đầu tiên** (NFR-SEO-004). Không được trùng từ khóa dành riêng (`search`, `new`, `edit`). |
| Description | `text` | NOT NULL | — | Mô tả ngắn, **20–2000 ký tự** (mục 7.9). Hiển thị trong card preview; là nguồn cho SEO meta description (cắt 160 ký tự khi render). |
| Instructions | `text` | **NULL** | — | Hướng dẫn tổng quan dạng markdown (**legacy field** — đã được `RecipeSteps` thay thế). Nullable vì body `POST /recipes` khai báo `instructions?` là tùy chọn. |
| PrepTime | `integer` | NOT NULL, CHECK > 0 | — | Thời gian chuẩn bị (phút). |
| CookTime | `integer` | NOT NULL, CHECK >= 0 | — | Thời gian nấu (phút). 0 cho "No cook" recipes. |
| Servings | `integer` | NOT NULL, CHECK > 0 | — | Số khẩu phần (portions). |
| Difficulty | `smallint` (enum) | NOT NULL, DEFAULT 1 | `IDX_Recipe_Difficulty` | `RecipeDifficulty`: 1=Easy, 2=Medium, 3=Hard, 4=Expert. |
| Status | `smallint` (enum) | NOT NULL, DEFAULT 0 | `IDX_Recipe_Status` | `RecipeStatus`: 0=Draft, 1=Published, 2=Archived. |
| CategoryId | `uuid` | NOT NULL, FK → `Categories.Id` | `IDX_Recipe_CategoryId` (B-tree) | Khóa ngoại đến Category. ON DELETE RESTRICT (không xóa category có recipe). |
| AuthorId | `varchar(450)` | NOT NULL, FK → `AspNetUsers.Id` | `IDX_Recipe_AuthorId` (B-tree) | Khóa ngoại đến ApplicationUser (Author). |
| SearchVector | `tsvector` | **GENERATED ALWAYS AS (...) STORED** | `IDX_Recipe_Search` (GIN) | Full-text search vector. **Generated column**, PostgreSQL tự tính lại khi `Title`/`Description` thay đổi — không dùng trigger. Biểu thức: `to_tsvector('simple', unaccent_immutable(coalesce("Title",'') \|\| ' ' \|\| coalesce("Description",'')))`. |
| PublishedAt | `timestamptz` | NULL | `IDX_Recipe_PublishedAt` | Thời điểm publish. **Chỉ được gán ở lần publish đầu tiên** (`PublishedAt == null`); các lần publish sau **không ghi đè**. NULL nếu chưa từng publish. Là cột sắp xếp của trang chủ (mục 5.1) và nguồn của `datePublished` trong JSON-LD. |
| CreatedAt | `timestamptz` | NOT NULL | — | (BaseEntity) |
| UpdatedAt | `timestamptz` | NULL | — | (BaseEntity) |
| IsDeleted | `boolean` | NOT NULL | `IDX_Recipe_IsDeleted` (partial) | (BaseEntity) — Global Query Filter. |
| RowVersion | `bytea` | NOT NULL | — | (BaseEntity) — Optimistic concurrency. |

**Index composite bắt buộc (NFR-PERF-004):** mọi truy vấn danh sách đều lọc `Status = Published` trước rồi mới sắp xếp, nên ba index dưới đây phủ đúng hình dạng truy vấn thật — hiệu quả hơn hẳn việc tạo B-tree đơn cho từng cột:

| Index | Định nghĩa | Phục vụ truy vấn |
| --- | --- | --- |
| `IDX_Recipe_List` | `("IsDeleted", "Status", "PublishedAt" DESC)` | FR-RCP-001 (danh sách công khai), trang chủ sắp xếp theo `publishedAt` |
| `IDX_Recipe_ByCategory` | `("Status", "CategoryId", "PublishedAt" DESC)` | FR-CAT-002 (recipe theo danh mục), filter `categoryId` |
| `IDX_Recipe_CookTime` | `("Status", "CookTime")` | FR-SRCH-002 filter `maxCookTime`, FR-RCP-001 sort `cookTime` |

**DDL đặc thù PostgreSQL cần khai báo trong migration** (hợp lệ theo CONS-006):

```sql
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- unaccent() mặc định KHÔNG immutable nên không dùng trực tiếp trong generated column;
-- bọc bằng wrapper IMMUTABLE:
CREATE OR REPLACE FUNCTION unaccent_immutable(text)
RETURNS text AS $$ SELECT unaccent('unaccent', $1) $$
LANGUAGE sql IMMUTABLE STRICT;

CREATE UNIQUE INDEX "IDX_Recipe_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;
```

> **[CR-2026 / MT-05, MT-20.8, MT-25, MT-26, MT-35]** Bốn thay đổi schema có hệ quả dài hạn:
>
> 1. **Partial unique index cho `Slug`.** Với Soft Delete toàn cục, unique index thường khiến slug của recipe đã xóa **chiếm chỗ vĩnh viễn** — người dùng không bao giờ tạo lại được công thức cùng tên. Mệnh đề `WHERE "IsDeleted" = false` giải phóng slug ngay khi recipe bị xóa mềm, mà vẫn giữ nguyên ràng buộc duy nhất cho dữ liệu đang sống.
> 2. **`SearchVector` là generated column, không phải trigger.** Generated column được khai báo trọn vẹn trong `OnModelCreating` nên không vi phạm CONS-006, và PostgreSQL bảo đảm đồng bộ — không có đường nào cập nhật `Title` mà quên cập nhật vector. Ràng buộc kèm theo: biểu thức phải `IMMUTABLE`, trong khi `unaccent()` mặc định thì không, nên bắt buộc có hàm wrapper `unaccent_immutable`.
> 3. **`Instructions` chuyển sang NULL.** Cột này `NOT NULL` trong khi `POST /recipes` khai báo `instructions?` là tùy chọn — mọi request không gửi `instructions` sẽ thất bại ở tầng DB. Chính schema gọi đây là "legacy field" đã được `RecipeSteps` thay thế, nên nullable là đúng bản chất.
> 4. **`PublishedAt` chỉ gán một lần.** v1.0.0 khai báo cột này nhưng FR-RCP-005 **không có bước nào gán giá trị** — cột sẽ mãi `NULL`, kéo theo index `IDX_Recipe_PublishedAt` vô dụng và `datePublished` trong JSON-LD rỗng. Quy tắc "không ghi đè ở các lần publish sau" giữ đúng ngữ nghĩa "ngày xuất bản": unpublish rồi publish lại không được làm bài viết cũ nhảy lên đầu trang chủ.

#### 7.2.1. RecipeNutrition (Owned Entity — cột trong bảng Recipes)

Owned Entity — không có bảng riêng. Các cột được nhúng trực tiếp vào bảng `Recipes` với tiền tố `"Nutrition_"`.

> **[CR-2026 / MT-02]** Vì là **Owned Entity**, `RecipeNutrition` **không có vòng đời độc lập** như `RecipeStep`/`RecipeIngredient`: nó được tạo và sửa **chỉ cùng với recipe**, qua `nutrition?` trong body `POST /recipes` và `PUT /recipes/{id}`. Hệ thống **không có** repository, DTO độc lập hay endpoint `/recipes/{id}/nutrition` riêng — thêm chúng sẽ tạo ra lớp kiến trúc thừa cho một thứ vốn chỉ là nhóm cột trong bảng `Recipes`, và mở ra khả năng hai đường code cùng ghi một nhóm cột. Nếu Frontend dùng wizard nhiều bước thì gom state ở client rồi submit một lần.

| Column trong DB | Property C# | Kiểu | Mô tả |
| --- | --- | --- | --- |
| Nutrition_Calories | Calories | `decimal(8,2)?` | Năng lượng (kcal / serving). Nullable. |
| Nutrition_Protein | Protein | `decimal(8,2)?` | Đạm (gram / serving). Nullable. |
| Nutrition_Carbohydrates | Carbohydrates | `decimal(8,2)?` | Tinh bột (gram / serving). Nullable. |
| Nutrition_Fat | Fat | `decimal(8,2)?` | Chất béo (gram / serving). Nullable. |
| Nutrition_Fiber | Fiber | `decimal(8,2)?` | Chất xơ (gram / serving). Nullable. |
| Nutrition_Sodium | Sodium | `decimal(8,2)?` | Natri (mg / serving). Nullable. |

### 7.3. RecipeStep

Các bước thực hiện chi tiết của một Recipe, được sắp xếp theo StepNumber.

| Column | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` | PK (BaseEntity) | UUID khóa chính. |
| RecipeId | `uuid` | NOT NULL, FK → `Recipes.Id`, ON DELETE CASCADE | Khóa ngoại. Cascade delete: xóa Recipe → xóa tất cả Steps. |
| StepNumber | `integer` | NOT NULL, CHECK > 0, **UNIQUE (RecipeId, StepNumber)** — khai báo là **UNIQUE CONSTRAINT `DEFERRABLE INITIALLY DEFERRED`** (không phải unique index — index không thể deferrable) | Thứ tự bước (1, 2, 3...), **liên tục từ 1, do Server toàn quyền gán** (FR-RCP-010). Client không bao giờ gửi giá trị này. Mọi thao tác renumber phải nằm trong **một** transaction. |
| Title | `varchar(200)` | NOT NULL | Tên bước ngắn gọn (ví dụ: "Sơ chế nguyên liệu"). **Bắt buộc trong body POST** (mục 7.9: 1–200 ký tự). |
| Description | `text` | NOT NULL | Mô tả chi tiết bước thực hiện (≤ 2000 ký tự, mục 7.9). |
| TimerMinutes | `integer` | NULL, CHECK >= 0 | Thời gian cần cho bước này (phút). NULL nếu không áp dụng. Tên trường API: **`timerMinutes`** (không phải `durationMinutes`). |
| ImageUrl | `varchar(500)` | NULL | URL ảnh minh họa bước (trên MinIO). Nullable. |

### 7.4. RecipeIngredient

| Column | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` | PK (BaseEntity) | UUID khóa chính. |
| RecipeId | `uuid` | NOT NULL, FK → `Recipes.Id`, ON DELETE CASCADE | Khóa ngoại với cascade delete. |
| Name | `varchar(200)` | NOT NULL | Tên nguyên liệu (ví dụ: "Thịt bò thăn"). 1–200 ký tự (mục 7.9). |
| Quantity | `decimal(10,3)` | NULL, CHECK (`Quantity IS NULL OR Quantity > 0`) | **Giá trị số** phục vụ tính toán: scale khẩu phần (×2, ×0.5), cộng dồn nguyên liệu, `recipeIngredient` trong JSON-LD. NULL khi không định lượng được. |
| **QuantityText** | `varchar(50)` | NULL | **Nguyên văn người dùng nhập** — "1/2 muỗng", "nửa củ", "1–2 quả", "vừa đủ". Được ưu tiên khi hiển thị. NULL khi người dùng nhập số thuần. |
| Unit | `varchar(50)` | NULL | Đơn vị đo lường (gram, ml, thìa canh, quả...). **Tùy chọn.** |
| Notes | `varchar(500)` | NULL | Ghi chú tùy chọn (ví dụ: "thái lát mỏng"). Nullable. |
| OrderIndex | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị trong danh sách nguyên liệu. Tên trường API: **`orderIndex`** (không phải `sortOrder`). |

**Ràng buộc mức bản ghi:** `Quantity`, `QuantityText` và `Unit` đều nullable, nhưng **không được rỗng cả ba** cùng lúc — validator ở tầng Application trả `INGREDIENT_QUANTITY_REQUIRED` (HTTP 400). **Quy tắc hiển thị:** ưu tiên `QuantityText` nếu có; ngược lại format từ `Quantity` + `Unit`. **Quy tắc scale khẩu phần:** chỉ áp dụng cho nguyên liệu có `Quantity`; nguyên liệu chỉ có `QuantityText` giữ nguyên văn.

### 7.5. RecipeImage

| Column | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` | PK (BaseEntity) | UUID khóa chính. |
| RecipeId | `uuid` | NOT NULL, FK → `Recipes.Id`, ON DELETE CASCADE | Khóa ngoại với cascade delete. |
| OriginalUrl | `varchar(500)` | NOT NULL | URL ảnh gốc trên MinIO (ví dụ: `.../recipes/{recipeId}/{guid}.jpg`). |
| MediumUrl | `varchar(500)` | NULL | URL ảnh medium 800×600 (sinh bởi FR-JOB-002). Nullable khi job chưa chạy. |
| ThumbnailUrl | `varchar(500)` | NULL | URL ảnh thumbnail 300×300 (sinh bởi FR-JOB-002). Nullable. |
| AltText | `varchar(200)` | NULL | Alt text cho accessibility. Nullable. |
| IsPrimary | `boolean` | NOT NULL, DEFAULT false; **partial unique index** `IDX_RecipeImage_Primary` trên `(RecipeId)` `WHERE "IsPrimary" = true AND "IsDeleted" = false` | Ảnh chính (hiển thị đầu tiên). Chỉ có 1 ảnh `IsPrimary=true` / Recipe — **bảo đảm ở tầng database**, không chỉ ở code. Vì index có `WHERE` nên không thể chuyển thành constraint deferrable; do đó thao tác đổi ảnh chính phải thực hiện **hai bước trong một transaction tường minh**: hạ primary cũ (`SaveChanges` lần 1) rồi nâng primary mới (`SaveChanges` lần 2). *(CR-2026-02 / MT-56)* |
| OrderIndex | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị gallery. |

### 7.6. Category

| Column | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` | PK (BaseEntity) | UUID khóa chính. |
| Name | `varchar(100)` | NOT NULL, UNIQUE | Tên danh mục (ví dụ: "Món khai vị"). **2–100 ký tự** (mục 7.9). Trùng tên được **kiểm tra chủ động ở tầng Application** trước khi ghi → HTTP 409 `CATEGORY_NAME_EXISTS`; ràng buộc UNIQUE của DB chỉ là lớp phòng vệ thứ hai cho tình huống đua (bắt mã lỗi PostgreSQL `23505`). |
| Slug | `varchar(120)` | NOT NULL, UNIQUE, `IDX_Category_Slug` | URL-friendly name. Sinh từ Name. |
| Description | `text` | NULL | Mô tả danh mục. Nullable. |
| ImageUrl | `varchar(500)` | NULL | URL ảnh đại diện category. Nullable. |
| OrderIndex | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị trên navigation. |

### 7.7. ApplicationUser (extends IdentityUser)

Kế thừa từ ASP.NET Core Identity `IdentityUser<string>`. Bảng: `"AspNetUsers"`. Thêm các custom columns:

| Column (custom) | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| DisplayName | `varchar(100)` | NOT NULL | **Tên hiển thị công khai duy nhất của hệ thống** (không phải username). Trường `FullName` **không tồn tại** ở bất kỳ đâu. |
| AvatarUrl | `varchar(500)` | NULL | URL ảnh avatar. Nullable. Sinh từ Google Avatar khi đăng ký OAuth. |
| Bio | `text` | NULL | Tiểu sử ngắn của tác giả. Nullable. Hiển thị trên author profile. |
| IsActive | `boolean` | NOT NULL, DEFAULT true | Trạng thái tài khoản. Admin có thể deactivate user (ban). |
| CreatedAt | `timestamptz` | NOT NULL, DEFAULT `NOW()` | Ngày tạo tài khoản. |

**Identity columns (kế thừa):** `Id` (varchar 450), `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`.

> **[CR-2026 / MT-12, MT-21, MT-22, MT-33.9]** Ghi chú vận hành cho bảng này:
>
> - **`UserName` do Backend sinh tự động** từ phần trước `@` của email, thêm hậu tố số khi trùng (`an.nguyen`, `an.nguyen2`). Người dùng **không nhập** `UserName` khi đăng ký; form đăng ký chỉ có `{ email, password, displayName }`.
> - **`EmailConfirmed`** (kế thừa từ Identity) luôn `false` và **không xuất hiện trong bất kỳ DTO công khai nào** — hệ thống không có luồng xác nhận email trong v1.1.0, nên policy `"VerifiedAuthor"` đã bị gỡ bỏ (mục 2.3). Nếu gắn policy đó vào các endpoint ghi, **sẽ không người dùng nào tạo được công thức**, vì không tồn tại đường nào chuyển `EmailConfirmed` thành `true`.
> - **`IsActive`** được kiểm tra thực sự ở FR-AUTH-002 (login) và FR-AUTH-004 (refresh) — nếu không, mã lỗi `AUTH_ACCOUNT_DISABLED` ở Phụ lục B sẽ là mã chết. Khi Admin khóa tài khoản (FR-AUTH-008), toàn bộ refresh token của user đó bị revoke; access token đã cấp vẫn sống tối đa 15 phút — đây là đánh đổi chấp nhận được của mô hình JWT stateless.
> - `AspNetUsers` **không kế thừa `BaseEntity`**, tức không có `IsDeleted`: hệ thống không có chức năng xóa người dùng. Vì vậy FR-AUTH-006 trả **401** (token trỏ tới user không còn hợp lệ) chứ không phải 404.

### 7.8. RefreshToken

| Column | Kiểu | Ràng buộc | Mô tả |
| --- | --- | --- | --- |
| Id | `uuid` | PK | UUID khóa chính. |
| UserId | `varchar(450)` | NOT NULL, FK → `AspNetUsers.Id`, ON DELETE CASCADE | Chủ sở hữu token. |
| TokenHash | `varchar(64)` | NOT NULL, UNIQUE, `IDX_RefreshToken_Hash` | SHA-256 hash của raw token. Không lưu raw token. |
| ExpiresAt | `timestamptz` | NOT NULL | Thời hạn token (7 ngày kể từ CreatedAt). |
| RevokedAt | `timestamptz` | NULL | Thời điểm revoke. NULL = còn hiệu lực. |
| ReplacedByTokenHash | `varchar(64)` | NULL | Hash của token mới (khi rotation). Để trace token family. |
| CreatedAt | `timestamptz` | NOT NULL, DEFAULT `NOW()` | Thời điểm tạo. |
| CreatedByIp | `varchar(45)` | NULL | IP address tạo token, lấy từ header `X-Forwarded-For` qua `UseForwardedHeaders` (NFR-SEC-003). Lưu để audit. |

> **[CR-2026 / MT-14, MT-15]** Bảng này **không có cột `IsRevoked`**. Token được coi là hợp lệ khi và chỉ khi `RevokedAt IS NULL AND ExpiresAt > NOW()`. Đây là quyết định về tính toàn vẹn: hai cột cùng biểu diễn một trạng thái luôn có nguy cơ lệch nhau (`IsRevoked = true` mà `RevokedAt = null`), và chỉ cần một đường code quên đồng bộ là hệ thống có token "đã thu hồi nhưng vẫn dùng được". Tương tự, `ReplacedByTokenHash` lưu **hash** chứ không phải raw token — DB không bao giờ chứa giá trị token có thể dùng để mạo danh.

### 7.9. Bảng Giới hạn Dữ liệu Chuẩn

Bảng dưới đây là **nguồn sự thật duy nhất** về giới hạn độ dài. Validator ở tầng Application và định nghĩa cột ở Database **PHẢI bằng nhau** — mọi FR ở Chương 3 và mọi schema ở Chương 7 đều tham chiếu về đây thay vì ghi lại con số riêng.

| Trường | Giới hạn chuẩn | Kiểu cột DB | Bắt buộc |
| --- | --- | --- | --- |
| `Recipe.Title` | 5–200 ký tự | `varchar(200)` | Có |
| `Recipe.Slug` | ≤ 220 ký tự | `varchar(220)` | Có (sinh tự động) |
| `Recipe.Description` | **20–2000 ký tự** | `text` | Có |
| `Recipe.Instructions` | ≤ 5000 ký tự | `text` | Không (legacy) |
| `RecipeStep.Title` | 1–200 ký tự | `varchar(200)` | Có |
| `RecipeStep.Description` | 1–2000 ký tự | `text` | Có |
| `RecipeIngredient.Name` | **1–200 ký tự** | `varchar(200)` | Có |
| `RecipeIngredient.QuantityText` | ≤ 50 ký tự | `varchar(50)` | Không |
| `RecipeIngredient.Unit` | ≤ 50 ký tự | `varchar(50)` | Không |
| `RecipeIngredient.Notes` | ≤ 500 ký tự | `varchar(500)` | Không |
| `Category.Name` | **2–100 ký tự** | `varchar(100)` | Có |
| `Category.Slug` | ≤ 120 ký tự | `varchar(120)` | Có (sinh tự động) |
| `RecipeImage.AltText` | ≤ 200 ký tự | `varchar(200)` | Không |
| `ApplicationUser.DisplayName` | 2–100 ký tự | `varchar(100)` | Có |
| `ApplicationUser.Bio` | ≤ 1000 ký tự | `text` | Không |

> **[CR-2026 / MT-37]** v1.0.0 để validator và schema lệch nhau ở nhiều chỗ, có chỗ lệch **2 lần**: `Category.Name` validator 2–50 nhưng cột `varchar(100)`; `RecipeIngredient.Name` validator 1–100 nhưng cột `varchar(200)`; `Recipe.Description` có ghi chú "≤ 2000 ký tự" trong schema nhưng **không validator nào thực thi**.
>
> **Nguyên tắc chốt cho v1.1.0:** validator và độ dài cột phải **bằng nhau**, không phải "validator chặt hơn cho an toàn". Khi hai con số lệch nhau, không ai biết con số nào mới là yêu cầu thật, QA không biết lấy đâu làm chuẩn để viết test, và dữ liệu nhập qua seeding hoặc migration sẽ lọt qua validator rồi vẫn nằm được trong DB. Riêng các giới hạn phục vụ SEO (`<title>` ≤ 60, `<meta description>` 150–160) **không** được đưa vào bảng này — chúng là **quy tắc hiển thị lúc render**, không phải ràng buộc dữ liệu (xem NFR-SEO-002).

---

## CHƯƠNG 8. ĐẶC TẢ REST API

Chương này liệt kê tất cả API endpoints của hệ thống Culinary Blog. Base URL: `/api/v1`. Tài liệu chi tiết (request/response schemas) được sinh tự động qua Scalar UI tại `/scalar`.

| Quy ước | Nội dung |
| --- | --- |
| Convention | HTTP Method + Path (prefixed `/api/v1`); `auth required` = Bearer JWT Access Token bắt buộc; `role` = Role tối thiểu cần thiết. **Role trong ASP.NET Core Identity KHÔNG phân cấp** — không có chuyện "Author ⊂ Admin" tự động. Tài khoản Admin được seed **cả hai role** (`Author` + `Admin`), và `AuthorPolicy` khai báo `RequireRole("Author", "Admin")`. |
| Định danh tài nguyên | **Đọc theo `slug`** (`GET /recipes/{slug}`), **ghi theo `id`** (`PUT`/`PATCH`/`DELETE /recipes/{id}`). Vì vậy `RecipeSummaryDto` và `RecipeDetailDto` **luôn mang cả `id` lẫn `slug`** để Frontend không phải tra cứu thêm. Các route literal `/recipes/search`, `/recipes/mine`, `/recipes/sitemap` **luôn thắng** `/recipes/{slug}` vì ASP.NET Core endpoint routing ưu tiên segment literal hơn tham số — **bất kể thứ tự khai báo**; hệ quả cần xử lý là các chuỗi đó phải nằm trong **danh sách slug dành riêng** (NFR-SEO-004) để không recipe nào nhận slug không truy cập được. *(CR-2026-02 / MT-53)* |
| Pagination | Query params: `?page=1&pageSize=12&sortBy=createdAt&sortOrder=desc`. **`pageSize` mặc định 12, tối đa 50.** `sortBy ∈ { createdAt, publishedAt, title, cookTime, prepTime }`, `sortOrder ∈ { asc, desc }`; giá trị ngoài whitelist → **HTTP 400**, không im lặng bỏ qua. Response: **`PagedResult<T>`** = `{ items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }` — không bọc `{ data, meta }` (§5.2, MT-42). |
| Error Format | RFC 7807 Problem Details: `{ "type":"about:blank", "title":"...", "status":400, "detail":"...", "errors":{"field":["msg"]} }`. Trường `type` mang Application Error Code (Phụ lục B) để Frontend phân biệt các lỗi cùng status — ví dụ 409 do trùng unique với 409 do concurrency conflict. |
| Cache | Mọi endpoint GET công khai dùng Redis cache-aside theo bảng TTL tại NFR-PERF-003. **Cấm cache** mọi endpoint phụ thuộc danh tính hoặc dữ liệu quản trị: `GET /recipes/mine`, `GET /auth/sessions`, `GET /users` — trả header `Cache-Control: no-store`. |

### 8.1. Authentication Module (/auth)

| Method | Endpoint | Mô tả | Auth | Request Body / Params | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/auth/register` | Đăng ký tài khoản mới (**auto-login**) | Không | `{ email, password, displayName }` | **201: `AuthResponseDto`** = `{ accessToken, refreshToken, expiresAt, user: { id, email, displayName, avatarUrl, bio, roles } }`; 400: validation errors; 409: email đã tồn tại (`AUTH_EMAIL_EXISTS`) |
| POST | `/auth/login` | Đăng nhập email/password | Không | `{ email, password }` | 200: `AuthResponseDto`; 400: validation; 401: sai credentials; 403: tài khoản bị vô hiệu hóa (`AUTH_ACCOUNT_DISABLED`); **423: tài khoản bị khóa tạm thời** (`AUTH_ACCOUNT_LOCKED`); 429: quá giới hạn rate limit |
| POST | `/auth/google` | Đăng nhập / đăng ký bằng Google | Không | `{ idToken }` — ID Token từ Google Identity Services | 200: `AuthResponseDto`; 400: `idToken` thiếu/sai định dạng (`AUTH_GOOGLE_TOKEN_INVALID`); 401: chữ ký/`aud`/`exp` không hợp lệ; 403: tài khoản liên kết bị vô hiệu hóa; **502: không truy cập được Google** (`AUTH_GOOGLE_UNAVAILABLE`) |
| POST | `/auth/refresh` | Làm mới Access Token (rotation) | Không (dùng refreshToken) | `{ refreshToken }` | 200: `AuthResponseDto`; 401: token hết hạn / bị revoke / reuse detection; 403: tài khoản bị vô hiệu hóa |
| POST | `/auth/logout` | Đăng xuất, revoke Refresh Token | Bearer JWT | `{ refreshToken }` | 204: No Content; 401: Unauthorized |
| GET | `/auth/me` | Lấy thông tin user hiện tại | Bearer JWT | — | 200: `{ id, email, displayName, avatarUrl, bio, roles }`; 401: Unauthorized |
| PATCH | `/auth/me` | Cập nhật profile người dùng | Bearer JWT | `{ displayName?, avatarUrl?, bio? }` | 200: `{ id, email, displayName, avatarUrl, bio, roles }`; 400: validation; 401: Unauthorized |
| GET | `/users` | **Danh sách người dùng** (FR-AUTH-008) — *mới ở v1.2.0* | Bearer + **Admin** | `?page&pageSize&search&isActive` | 200: `PagedResult<UserAdminDto>` = `{ id, email, displayName, avatarUrl, roles, isActive, createdAt, recipeCount }`; 400; 401; 403. Không cache |
| PATCH | `/users/{id}/status` | **Khóa / mở khóa tài khoản** (FR-AUTH-008) | Bearer + **Admin** | `{ isActive: boolean, reason? }` | 200: `{ id, email, displayName, isActive }`; 400: validation; **403: không phải Admin, hoặc Admin tự khóa chính mình**; 404: user không tồn tại |
| GET | `/auth/sessions` | **Danh sách phiên đăng nhập của chính mình** (FR-AUTH-009) — *mới ở v1.2.0* | Bearer JWT | — | 200: `[{ id, createdAt, createdByIp, expiresAt, isCurrent }]` — **không bao giờ trả `TokenHash`**; 401. `Cache-Control: no-store` |
| DELETE | `/auth/sessions/{id}` | **Thu hồi một phiên** (FR-AUTH-009) — *mới ở v1.2.0* | Bearer JWT | — | 204 (idempotent); 401; **404: không tồn tại hoặc thuộc người khác** |
| POST | `/auth/sessions/revoke-all` | **Thu hồi mọi phiên** — đăng xuất trên mọi thiết bị (FR-AUTH-009) — *mới ở v1.2.0* | Bearer JWT | — | 204; 401 |

> **[CR-2026 / MT-10, MT-12, MT-20.10, MT-22]** `POST /auth/register` **trả về cặp token** (auto-login). v1.0.0 mâu thuẫn trực tiếp ở điểm này: FR-AUTH-001 mô tả 12 bước có cả việc persist refresh token và ghi rõ "auto-login sau đăng ký", trong khi Chương 8 lại trả `{ userId, email, displayName }` không có token. Quyết định này thay đổi hẳn luồng Frontend — có token thì vào thẳng `/dashboard`, không có token thì phải đẩy sang `/auth/login` — nên làm sai một bên là phải viết lại màn hình đăng ký. Auto-login không tạo rủi ro vì SRS đã loại xác thực email khỏi phạm vi (MT-21).
>
> Endpoint **`PATCH /users/{id}/status`** được bổ sung để cột `IsActive` (mục 7.7) và mã lỗi `AUTH_ACCOUNT_DISABLED` (Phụ lục B) không còn là những lời hứa suông: v1.0.0 có cả cột lẫn mã lỗi nhưng **không có FR hay endpoint nào** bật/tắt được cờ này, và FR-AUTH-002 cũng không kiểm tra nó — nghĩa là kể cả khi Admin sửa tay trong DB, người dùng bị khóa **vẫn đăng nhập được**.
>
> **[CR-2026-02 / MT-44, MT-45, MT-52]** v1.2.0 bổ sung 4 endpoint cho module này: `GET /users` (Admin tìm người cần khóa — route `/dashboard/users` của mục 5.1 không dựng được nếu thiếu nó) và 3 endpoint quản lý phiên của FR-AUTH-009. Đồng thời sửa mã lỗi khi Admin tự khóa mình từ **409 (`VALIDATION_ERROR`) thành 403**: bản thân v1.1.0 tự mâu thuẫn ba chiều ở đây — FR-AUTH-008 A2 ghi 403, bảng này ghi 409, còn Phụ lục B quy định `VALIDATION_ERROR` luôn là 400. Giải trình chi tiết tại cuối FR-AUTH-008 và FR-AUTH-009.

### 8.2. Categories Module (/categories)

| Method | Endpoint | Mô tả | Auth / Role | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/categories` | Lấy danh sách tất cả categories | Không | — | 200: `[{ id, name, slug, description, imageUrl, orderIndex, recipeCount }]` — sắp xếp theo `orderIndex` rồi `name`; `recipeCount` chỉ đếm recipe **Published** |
| GET | `/categories/{slug}` | Lấy chi tiết category + danh sách recipes **đã xuất bản** | Không | `?page=1&pageSize=12&sortBy=...&sortOrder=...` | 200: `{ category, recipes: PagedResult }`; 404: Category not found |
| POST | `/categories` | Tạo category mới | Bearer + Admin | `{ name, description?, imageUrl?, orderIndex? }` | 201: `{ id, name, slug, description, imageUrl, orderIndex }`; 400: validation; 403: Forbidden; **409: name đã tồn tại** (`CATEGORY_NAME_EXISTS` — kiểm tra chủ động trước khi ghi) |
| PUT | `/categories/{id}` | Cập nhật category | Bearer + Admin | `{ name, description?, imageUrl?, orderIndex? }` | 200: category updated; 400/403/404; **409: đổi sang tên đã tồn tại** (`CATEGORY_NAME_EXISTS`) |
| DELETE | `/categories/{id}` | Xóa category (**soft delete**) | Bearer + Admin | — | 204: No Content; 403: Forbidden; 404: Not found; 409: Có recipes thuộc category này (`CATEGORY_DELETE_HAS_RECIPES`) |

> **[CR-2026 / MT-33.8, MT-36]** `CategoryDto` bổ sung `imageUrl` và `orderIndex` — hai trường mà cả schema (mục 7.6) lẫn API đều có nhưng FR-CAT-001 bỏ sót.
>
> Mã **409 khi trùng `Name`** ở v1.0.0 là mã **không bao giờ được sinh ra**: luồng FR-CAT-003 chỉ kiểm tra slug (và tự thêm hậu tố `-2`, `-3`), không kiểm tra `Name`. Tạo danh mục trùng tên lần thứ hai sẽ đi hết luồng rồi mới bị ràng buộc UNIQUE của PostgreSQL chặn → `DbUpdateException` → **HTTP 500**, tức người dùng nhận lỗi hệ thống thay vì lỗi nghiệp vụ, và log bị nhiễu bởi exception không đáng có. v1.1.0 kiểm tra `Name` chủ động ở tầng Application (ném `ConflictException`), giữ ràng buộc UNIQUE của DB làm lớp phòng vệ thứ hai cho tình huống đua. `PUT /categories/{id}` có cùng lỗ hổng nên cũng được bổ sung bước kiểm tra tương tự.

### 8.3. Recipes Module (/recipes)

> **Quy ước Public/Private của module này:** `GET /recipes`, `GET /recipes/{slug}` và `GET /recipes/search` **chỉ trả `Status == Published` cho mọi người gọi, không ngoại lệ — kể cả Admin**. Dữ liệu Draft/Archived **chỉ** truy cập được qua `GET /recipes/mine`.

| Method | Endpoint | Mô tả | Auth / Role | Request | Status Code |
| --- | --- | --- | --- | --- | --- |
| GET | `/recipes` | Danh sách recipes (**chỉ Published**, paginated) | Không | `?page&pageSize&sortBy&sortOrder&categoryId&difficulty&maxCookTime&maxPrepTime&minServings` | 200; 400 (tham số ngoài whitelist) |
| GET | `/recipes/mine` | **Danh sách công thức của chính mình** (Draft/Published/Archived) — **cấm cache**, trả `Cache-Control: no-store` | **Bearer bắt buộc** (Author/Admin) | `?status&page&pageSize&sortBy&sortOrder&authorId` (`authorId` chỉ Admin) | 200; 400; 401; 403 |
| GET | `/recipes/search` | Full-text search công thức (**chỉ Published**) | Không | `?q={keyword}&page&pageSize&categoryId&difficulty` | 200; 400 (`q` < 2 ký tự) |
| GET | `/recipes/sitemap` | **Danh sách slug phục vụ sitemap** (NFR-SEO-003) — *mới ở v1.2.0*: `[{ slug, updatedAt }]` của **mọi** recipe Published, không phân trang, cache Redis 1 giờ | Không | — | 200 |
| GET | `/recipes/{slug}` | Chi tiết recipe theo slug (**chỉ Published**; kèm steps, ingredients, images, nutrition) | Không | — | 200; 404 |
| POST | `/recipes` | Tạo recipe mới (trạng thái Draft) | Bearer (Author/Admin) | `{ title, description, categoryId, prepTime, cookTime, servings, difficulty, instructions?, nutrition?, steps?, ingredients? }` | 201; 400; 401; 403; 404 (categoryId không tồn tại) |
| PUT | `/recipes/{id}` | Cập nhật thông tin cơ bản recipe | Bearer (Owner/Admin) | `{ title?, description?, categoryId?, prepTime?, cookTime?, servings?, difficulty?, instructions?, nutrition?, rowVersion }` | 200; 400; 403; 404; **409** (`RECIPE_CONCURRENCY_CONFLICT`) |
| PATCH | `/recipes/{id}/publish` | Publish recipe (Draft → Published) | Bearer (Owner/Admin) | — | 200; **400** (`RECIPE_PUBLISH_INCOMPLETE`); 403; 404; **409** (`RECIPE_INVALID_STATE_TRANSITION`) |
| PATCH | `/recipes/{id}/unpublish` | Unpublish recipe (Published → Draft) | Bearer (Owner/Admin) | — | 200; 403; 404; 409 |
| PATCH | `/recipes/{id}/archive` | Archive recipe (từ Draft **hoặc** Published → Archived) | Bearer (Owner/Admin) | — | 200; 403; 404; 409 |
| PATCH | `/recipes/{id}/unarchive` | **Unarchive recipe (Archived → Draft)** | Bearer (Owner/Admin) | — | 200; 403; 404; 409 |
| DELETE | `/recipes/{id}` | Xóa recipe (**soft delete**) | Bearer (Owner/Admin) | — | 204; 403; 404 |

> **[CR-2026 / MT-33.4, MT-34, MT-35]** Hai endpoint được bổ sung:
>
> - **`GET /recipes/mine`** (FR-RCP-011) là điều kiện tiên quyết để các endpoint công khai trở nên thuần công khai và cache được an toàn — xem phân tích đầy đủ tại FR-RCP-011 và NFR-SEC-006.
> - **`PATCH /recipes/{id}/unarchive`** đóng kín máy trạng thái. v1.0.0 đặt tên yêu cầu là "Archive / **Unarchive**" nhưng không có endpoint unarchive nào, khiến **Archived trở thành hố đen**: vào được, không ra được — mâu thuẫn trực tiếp với chính mô tả *"ẩn recipe cũ mà không mất dữ liệu"*, vì ẩn vĩnh viễn không lấy lại được thì với người dùng không khác gì xóa.
> - **`GET /recipes/sitemap`** *(CR-2026-02 / MT-48)* phục vụ `app/sitemap.ts` của Frontend (NFR-SEO-003). Sitemap cần **toàn bộ** slug Published trong một lần, trong khi `GET /recipes` giới hạn `pageSize ≤ 50` — dùng nó thì phải gọi hàng trăm request, và dữ liệu có thể xê dịch giữa các trang trong lúc duyệt (bản ghi mới publish chen vào làm lặp hoặc sót slug). Một endpoint chỉ trả `{ slug, updatedAt }` gọn nhẹ, cache 1 giờ (bằng chu kỳ tái sinh sitemap) giải quyết cả hai vấn đề mà không nới giới hạn `pageSize` cho mọi người gọi.
>
> **Thứ tự route (đã sửa ở v1.2.0 — MT-53):** ASP.NET Core ưu tiên segment literal (`mine`, `search`, `sitemap`) hơn tham số `{slug}` nên thứ tự khai báo **không** quyết định kết quả khớp route — v1.1.0 ghi "phải đăng ký trước" là giải thích sai cơ chế. Điều thực sự bắt buộc là khi sinh slug phải cấm trùng **danh sách slug dành riêng** tại NFR-SEO-004, vì recipe mang slug `search` sẽ không bao giờ truy cập được.

### 8.4. Recipe Images (/recipes/{id}/images)

| Method | Endpoint | Mô tả | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/recipes/{id}/images` | Upload ảnh mới cho recipe | Bearer (Owner/Admin) | `multipart/form-data`: `file` (image), `altText?`, `isPrimary?`, `orderIndex?` | 201: `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }`; 400: MIME invalid / size > 5MB; 403/404; **503: MinIO không khả dụng** |
| PATCH | `/recipes/{id}/images/{imageId}` | Cập nhật metadata ảnh (altText, isPrimary, orderIndex) | Bearer (Owner/Admin) | `{ altText?, isPrimary?, orderIndex? }` | 200: image updated; 400: validation; 403/404 |
| DELETE | `/recipes/{id}/images/{imageId}` | Xóa ảnh (file MinIO xóa async qua Hangfire) | Bearer (Owner/Admin) | — | 204: No Content; 403/404 |

**Hai quy tắc nghiệp vụ bắt buộc của module ảnh:**

1. Đặt `isPrimary = true` cho một ảnh thì hệ thống **tự động bỏ `isPrimary` của mọi ảnh khác** thuộc cùng recipe — bảo đảm bất biến "chỉ 1 ảnh primary / recipe" (mục 7.5).
2. Xóa ảnh đang là primary thì ảnh còn lại có **`OrderIndex` nhỏ nhất** tự động lên primary; nếu `OrderIndex` bằng nhau thì lấy ảnh có **`CreatedAt` sớm nhất**.

`mediumUrl` và `thumbnailUrl` có thể `null` ngay sau khi upload — chúng được điền bởi FR-JOB-002 (Image Resize Job) chạy bất đồng bộ; Frontend hiển thị `originalUrl` trong lúc chờ.

### 8.5. Recipe Steps (/recipes/{id}/steps)

| Method | Endpoint | Mô tả | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/recipes/{id}/steps` | Thêm bước mới vào recipe (**Server tự gán `StepNumber = Max + 1`**) | Bearer (Owner/Admin) | `{ title, description, timerMinutes?, imageUrl? }` — **không có `stepNumber`** | 201: `RecipeStepDto`; 400/403/404 |
| PUT | `/recipes/{id}/steps/{stepId}` | Cập nhật nội dung một bước | Bearer (Owner/Admin) | `{ title?, description?, timerMinutes?, imageUrl? }` — **không có `stepNumber`** | 200: `RecipeStepDto`; 400/403/404 |
| PATCH | `/recipes/{id}/steps/reorder` | **Sắp xếp lại thứ tự các bước** (kéo-thả ở Frontend) | Bearer (Owner/Admin) | `{ stepIds: [guid, ...] }` — mảng **đầy đủ** id theo thứ tự mới | 200: `RecipeStepDto[]` đã renumber 1..N; 400 (`stepIds` thiếu/thừa/trùng); 403/404 |
| DELETE | `/recipes/{id}/steps/{stepId}` | Xóa một bước (**Server tự renumber các bước còn lại**) | Bearer (Owner/Admin) | — | 204: No Content; 403/404 |

> **[CR-2026 / MT-03]** Trường `stepNumber` **bị loại khỏi body của cả POST lẫn PUT**. Để client tự gửi số thứ tự là giao trách nhiệm toàn vẹn dữ liệu cho phía không kiểm soát được: ràng buộc `UNIQUE (RecipeId, StepNumber)` sẽ vỡ ngay khi hai request thêm bước chạy gần như đồng thời, và người dùng nhận HTTP 500 từ DB thay vì một lỗi nghiệp vụ rõ ràng. Muốn đổi thứ tự thì dùng endpoint `reorder` — Server nhận **ý định** ("thứ tự mới là mảng này") chứ không nhận **giá trị cột**, rồi tự renumber 1..N trong một transaction duy nhất.

### 8.6. Recipe Ingredients (/recipes/{id}/ingredients)

| Method | Endpoint | Mô tả | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/recipes/{id}/ingredients` | Thêm nguyên liệu | Bearer (Owner/Admin) | `{ name, quantity?, quantityText?, unit?, notes?, orderIndex? }` | 201: `RecipeIngredientDto`; 400 (`VALIDATION_ERROR`, `INGREDIENT_QUANTITY_REQUIRED`); 403/404 |
| PUT | `/recipes/{id}/ingredients/{ingId}` | Cập nhật nguyên liệu | Bearer (Owner/Admin) | `{ name?, quantity?, quantityText?, unit?, notes?, orderIndex? }` | 200: `RecipeIngredientDto`; 400; 403/404 |
| DELETE | `/recipes/{id}/ingredients/{ingId}` | Xóa nguyên liệu | Bearer (Owner/Admin) | — | 204: No Content; 403/404 |

`RecipeIngredientDto` = `{ id, name, quantity, quantityText, unit, notes, orderIndex }`. Validator bắt buộc: **không được rỗng cả `quantity`, `quantityText` lẫn `unit`**; nếu có `quantity` thì phải `> 0` (mục 7.4).

### 8.7. Health Check Endpoints

| Method | Endpoint | Mô tả | Auth | Response |
| --- | --- | --- | --- | --- |
| GET | `/health` | Tổng hợp health tất cả dependencies (DB, Redis, MinIO) | Không | 200: Healthy \| 503: Unhealthy — `{ "status":"Healthy", "entries":{"database":{"status":"Healthy"},...} }` |
| GET | `/health/live` | Liveness probe — chỉ kiểm tra process còn sống | Không | 200: Healthy (luôn luôn, trừ khi process crashed) |
| GET | `/health/ready` | Readiness probe — kiểm tra DB và Redis sẵn sàng | Không | 200: Healthy (DB + Redis up); 503: Unhealthy (không nhận traffic) |


### 8.8. Files Module (/files) — *mới ở v1.2.0*

Endpoint upload/xóa tệp **tổng quát**, dùng cho mọi trường chỉ nhận URL ảnh (`avatarUrl` — FR-AUTH-007, `imageUrl` danh mục — FR-CAT-003/004, `imageUrl` bước nấu — FR-RCP-010). Ảnh công thức dùng endpoint riêng `POST /recipes/{id}/images` (8.4).

| Method | Endpoint | Mô tả | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/files/upload` | Upload một ảnh (FR-FILE-001) | Bearer (Author/Admin) · rate limit `upload` 5 req/phút/IP | `multipart/form-data`: `file` | 201: `{ key, url, contentType, size }` — lưu tại `uploads/{userId}/{guid}{ext}`; 400: `FILE_SIZE_EXCEEDED` / `FILE_MIME_INVALID`; 401; 429; 503: `FILE_STORAGE_UNAVAILABLE` |
| DELETE | `/files/{**key}` | Xóa một tệp đã upload (FR-FILE-002) | Bearer (Author/Admin) | — | 204 (idempotent); 400: key không hợp lệ (chứa `..`); 401; **403 `FILE_FORBIDDEN`**: tệp không nằm trong thư mục của người gọi (Admin được xóa mọi tệp) |

> **[CR-2026-02 / MT-43]** Hai endpoint này đã tồn tại trong code từ Buổi 2 nhưng nằm **ngoài SRS**. Chính thức hóa chúng thay vì tạo ba endpoint con riêng cho avatar, ảnh danh mục và ảnh bước nấu, vì một endpoint tổng quát giữ mọi quy tắc bảo mật tệp (5MB, magic bytes, tên GUID, phạm vi thư mục theo người dùng) **ở một chỗ duy nhất**. Tệp đã upload nhưng không được gán vào đâu (người dùng bỏ ngang form) là rác chấp nhận được ở v1.2.0; dọn tệp mồ côi thuộc phiên bản sau.

---

## PHỤ LỤC A – HTTP STATUS CODES

Bảng dưới đây liệt kê tất cả HTTP Status Codes được sử dụng trong API Culinary Blog, cùng ngữ cảnh sử dụng cụ thể.

| Code | Status | Ngữ cảnh sử dụng |
| --- | --- | --- |
| 200 | OK | GET request thành công; PATCH trả về resource đã cập nhật; POST `/auth/login` thành công. |
| 201 | Created | POST tạo resource mới thành công (Recipe, Category, Step, Ingredient, Image). Response body chứa resource vừa tạo. |
| 204 | No Content | DELETE thành công; POST `/auth/logout` thành công. Không có response body. |
| 400 | Bad Request | **Mọi lỗi validation** (FluentValidation), request body malformed, tham số ngoài whitelist (`sortBy`, `sortOrder`, `difficulty`, `status`), file MIME không hợp lệ, vi phạm business rule (ví dụ: publish recipe thiếu step hoặc ingredient). |
| 401 | Unauthorized | Access Token thiếu hoặc invalid; Refresh Token hết hạn / bị revoke / reuse detection; Google ID Token không vượt qua kiểm tra chữ ký. |
| 403 | Forbidden | Đã xác thực nhưng không có quyền: Author truy cập endpoint Admin; Author cố xóa recipe của người khác; **tài khoản bị Admin vô hiệu hóa** (`IsActive = false`); **Admin tự khóa chính mình** (FR-AUTH-008); xóa tệp không thuộc thư mục của mình (`FILE_FORBIDDEN`). |
| 404 | Not Found | Resource không tồn tại hoặc đã soft-delete (`IsDeleted=true`). |
| 409 | Conflict | **Mọi xung đột trạng thái:** trùng lặp unique field (email đã đăng ký, tên/slug category đã tồn tại); xóa category đang có recipes; **Optimistic Concurrency — `RowVersion` không khớp**; **chuyển trạng thái recipe không hợp lệ** (ví dụ publish một recipe đang Archived). Frontend phân biệt các trường hợp qua trường `type` (Phụ lục B). |
| **423** | **Locked** | Tài khoản bị khóa tạm thời do đăng nhập sai quá số lần cho phép (ASP.NET Core Identity Lockout). Response nêu thời gian mở khóa còn lại. |
| 429 | Too Many Requests | Rate limit bị vượt. Response kèm header `Retry-After` (giây). |
| 500 | Internal Server Error | Lỗi không xử lý được (unhandled exception). Trả RFC 7807 với `type = INTERNAL_ERROR`, log đầy đủ qua Serilog kèm CorrelationId. Không lộ stack trace. |
| **502** | **Bad Gateway** | Không truy cập được dịch vụ bên thứ ba bắt buộc — cụ thể là không lấy được khóa công khai (JWKS) của Google để verify ID Token. |
| 503 | Service Unavailable | Health check failed (DB/Redis down); MinIO không khả dụng khi upload hoặc xóa tệp (`FILE_STORAGE_UNAVAILABLE`); hoặc server overloaded. |

> **[CR-2026 / MT-08, MT-09, MT-20.10]** **Mã 422 Unprocessable Entity bị loại bỏ hoàn toàn khỏi hệ thống.** v1.0.0 mâu thuẫn xuyên suốt hai chương: toàn bộ Chương 3 dùng 422 cho validation, trong khi Chương 8 và cả hai phụ lục dùng 400 — ảnh hưởng gần như mọi endpoint và mọi test case. v1.1.0 chốt quy tắc hai vế, đơn giản và không có vùng xám:
>
> - **400 = mọi lỗi đầu vào** (khớp hành vi mặc định `ValidationProblemDetails` của ASP.NET Core, nên không cần code tùy biến).
> - **409 = mọi xung đột trạng thái**, bao gồm cả Optimistic Concurrency. 409 mới đúng ngữ nghĩa HTTP *"yêu cầu xung đột với trạng thái hiện tại của tài nguyên"* — chính xác là tình huống ETag/If-Match; còn 422 nghĩa là "cú pháp đúng, ngữ nghĩa không xử lý được", không phải "xung đột trạng thái".
>
> Hai mã **423** và **502** được bổ sung vì Chương 3 đã dùng chúng (FR-AUTH-002, FR-AUTH-003) nhưng Phụ lục A của v1.0.0 không liệt kê — mọi status code xuất hiện trong tài liệu đều phải có dòng ở bảng này.

## PHỤ LỤC B – APPLICATION ERROR CODES

Hệ thống sử dụng Application Error Codes (mã lỗi tùy chỉnh) trong trường RFC 7807 `"type"` để frontend có thể xử lý lỗi theo programmatic way mà không phụ thuộc vào chuỗi message (có thể thay đổi theo locale).

| Error Code | HTTP Status | Mô tả | Module |
| --- | --- | --- | --- |
| `AUTH_EMAIL_EXISTS` | 409 | Email đã được đăng ký bởi tài khoản khác. | Auth |
| `AUTH_INVALID_CREDENTIALS` | 401 | Email hoặc mật khẩu không đúng. | Auth |
| `AUTH_TOKEN_EXPIRED` | 401 | Access Token đã hết hạn (15 phút). | Auth |
| `AUTH_TOKEN_INVALID` | 401 | Access Token sai định dạng hoặc chữ ký không hợp lệ. | Auth |
| `AUTH_REFRESH_TOKEN_EXPIRED` | 401 | Refresh Token đã hết hạn (7 ngày). | Auth |
| `AUTH_REFRESH_TOKEN_REVOKED` | 401 | Refresh Token đã bị thu hồi (reuse detection). | Auth |
| `AUTH_GOOGLE_TOKEN_INVALID` | 400 | Google ID Token không hợp lệ hoặc đã hết hạn. | Auth |
| `AUTH_GOOGLE_UNAVAILABLE` | **502** | Không truy cập được Google để lấy khóa công khai (JWKS) verify ID Token. | Auth |
| `AUTH_ACCOUNT_DISABLED` | 403 | Tài khoản bị vô hiệu hóa (`IsActive=false`) bởi Admin (FR-AUTH-008). | Auth |
| `AUTH_ACCOUNT_LOCKED` | **423** | Tài khoản bị khóa tạm thời do đăng nhập sai quá số lần cho phép (Identity Lockout). | Auth |
| `RECIPE_NOT_FOUND` | 404 | Recipe với id/slug không tồn tại hoặc đã bị xóa. | Recipe |
| `RECIPE_SLUG_EXISTS` | 409 | Slug đã tồn tại — tự động thêm suffix (`slug-1`, `slug-2`...). | Recipe |
| `RECIPE_PUBLISH_INCOMPLETE` | 400 | Recipe thiếu điều kiện publish: phải có ít nhất 1 ingredient và 1 step. | Recipe |
| `RECIPE_FORBIDDEN` | 403 | User không phải owner và không phải Admin. | Recipe |
| `RECIPE_CONCURRENCY_CONFLICT` | **409** | RowVersion không khớp — resource đã được cập nhật bởi request khác. Client cần reload. | Recipe |
| `RECIPE_INVALID_STATE_TRANSITION` | **409** | Chuyển trạng thái không hợp lệ theo máy trạng thái tại FR-RCP-005/006 (ví dụ: publish một recipe đang Archived, unarchive một recipe không ở trạng thái Archived). | Recipe |
| `INGREDIENT_QUANTITY_REQUIRED` | 400 | Nguyên liệu để rỗng cả `quantity`, `quantityText` lẫn `unit` — phải có ít nhất một trong ba. | Recipe |
| `CATEGORY_NOT_FOUND` | 404 | Category không tồn tại. | Category |
| `CATEGORY_NAME_EXISTS` | 409 | Tên category đã tồn tại. | Category |
| `CATEGORY_DELETE_HAS_RECIPES` | 409 | Không thể xóa category đang có recipes thuộc về. | Category |
| `FILE_SIZE_EXCEEDED` | 400 | File upload vượt quá giới hạn 5MB. | File |
| `FILE_MIME_INVALID` | 400 | Loại file không được phép. Chỉ chấp nhận JPEG, PNG, WebP, AVIF. | File |
| `FILE_FORBIDDEN` | **403** | Xóa tệp không nằm trong thư mục của người gọi (`DELETE /files/{**key}`); Admin được miễn. *(v1.2.0 — MT-46)* | File |
| `FILE_STORAGE_UNAVAILABLE` | **503** | MinIO không khả dụng khi upload/xóa tệp (FR-FILE-001/002, FR-RCP-008 A4). *(v1.2.0 — MT-46)* | File |
| `VALIDATION_ERROR` | 400 | Một hoặc nhiều field không hợp lệ. Xem `"errors"` object. | Common |
| `RATE_LIMIT_EXCEEDED` | 429 | Quá giới hạn request. Xem `Retry-After` header. | Common |
| `INTERNAL_ERROR` | **500** | Lỗi không lường trước. `detail` chỉ chứa thông báo chung và CorrelationId để tra log — không lộ chi tiết kỹ thuật. *(v1.2.0 — MT-46)* | Common |

> **[CR-2026-02 / MT-46]** Phụ lục B tăng từ 24 lên **27 mã**. Ba mã mới không phải phát minh trên giấy: chúng **đã được code Buổi 2 sử dụng thực tế** (`ErrorCodes.cs`) cho những tình huống SRS có mô tả status code nhưng không đặt mã — FR-RCP-008 A4 (MinIO lỗi → 503) và lỗi 500 chung. Để chúng nằm ngoài Phụ lục B nghĩa là Frontend nhận về những giá trị `type` không có trong tài liệu, trái với chính mục đích của phụ lục này (*"để frontend xử lý lỗi theo programmatic way"*). Ngược lại, mã `AUTH_USERNAME_EXISTS` mà code Buổi 2 có **không** được đưa vào: nó chỉ tồn tại vì form đăng ký cũ bắt người dùng tự nhập `userName` — điều trái với MT-12 — và sẽ biến mất khi code được sửa theo SRS.

## PHỤ LỤC C – TỪ ĐIỂN THUẬT NGỮ

| Thuật ngữ | Viết tắt | Định nghĩa |
| --- | --- | --- |
| Access Token | AT | JSON Web Token (JWT) dùng để xác thực API request. TTL = 15 phút. Ký bằng HS256. |
| Application Error Code | AEC | Mã lỗi tùy chỉnh dạng SCREAMING_SNAKE_CASE trong trường `"type"` của RFC 7807 Problem Details. |
| Archive | — | Trạng thái Recipe khi bị ẩn khỏi public listing nhưng không bị xóa. `RecipeStatus.Archived`. |
| Author | — | Role người dùng mặc định sau khi đăng ký. Có thể tạo/quản lý recipe của mình. |
| Background Job | — | Tác vụ xử lý bất đồng bộ chạy ngoài HTTP request cycle, quản lý bởi Hangfire. |
| Cache-Aside | — | Mẫu cache trong đó ứng dụng tự đọc cache trước, nếu miss thì truy vấn DB rồi ghi ngược vào cache. Là cơ chế cache **duy nhất** của hệ thống (NFR-PERF-003). |
| Cách ly Public/Private | — | Nguyên tắc kiến trúc: endpoint công khai chỉ phục vụ dữ liệu công khai (cache được), dữ liệu phụ thuộc danh tính đi qua endpoint riêng và bị cấm cache. Xem NFR-SEC-006 và FR-RCP-011. |
| Change Request | CR | Quy trình thay đổi tài liệu đã phê duyệt. Phiên bản 1.1.0 được ban hành qua **CR-2026** (Phụ lục D). |
| Clean Architecture | CA | Kiến trúc phần mềm của Robert C. Martin tách biệt concerns theo layers (Domain, Application, Infrastructure, Presentation). Dependency chỉ đi vào trong (hướng Domain). |
| Command Query Responsibility Segregation | CQRS | Pattern tách biệt write model (Commands) và read model (Queries) để tối ưu từng luồng riêng. |
| Content Delivery Network | CDN | Mạng phân phối nội dung tĩnh (ảnh, JS, CSS) từ server gần người dùng nhất. |
| Core Web Vitals | CWV | Chỉ số đo lường UX của Google: LCP (tải trang), CLS (ổn định layout), INP (phản hồi tương tác). |
| Docker Compose | — | Công cụ định nghĩa và chạy multi-container Docker application qua file YAML. |
| Draft | — | Trạng thái mặc định của Recipe khi mới tạo. Chỉ Author/Admin thấy. |
| Forwarded Headers | — | Cơ chế ASP.NET Core đọc `X-Forwarded-For` / `X-Forwarded-Proto` để khôi phục IP và scheme thật của client khi đứng sau reverse proxy. Bắt buộc bật kèm danh sách mạng proxy tin cậy — trên .NET 10 là `KnownIPNetworks` (NFR-SEC-003). |
| Full-Text Search | FTS | Tìm kiếm ngôn ngữ tự nhiên trong PostgreSQL qua `tsvector`/`tsquery`, dùng configuration `simple` + `unaccent` extension. |
| Generated Column | — | Cột PostgreSQL có giá trị được tính tự động từ các cột khác (`GENERATED ALWAYS AS (...) STORED`). Dùng cho `Recipe.SearchVector` thay cho trigger (mục 7.2). |
| Hangfire | — | Thư viện .NET xử lý background jobs: fire-and-forget, delayed, recurring. |
| HTTP Status Code | — | Mã phản hồi HTTP chuẩn (RFC 7231) cho biết kết quả xử lý request (2xx: thành công, 4xx: client error, 5xx: server error). |
| Incremental Static Regeneration | ISR | Tính năng Next.js tái sinh (regenerate) trang tĩnh theo chu kỳ (revalidate interval) thay vì build lại toàn bộ. |
| JSON Web Token | JWT | Chuẩn mở (RFC 7519) định nghĩa cách truyền thông tin an toàn giữa các bên dưới dạng JSON object được ký. |
| MediatR | — | Thư viện .NET triển khai Mediator pattern. Dispatch Commands/Queries qua Handler có pipeline behaviors. |
| MinIO | — | Object storage server mã nguồn mở tương thích Amazon S3 API. Dùng để lưu trữ ảnh. |
| Non-Functional Requirement | NFR | Yêu cầu chất lượng hệ thống: hiệu năng, bảo mật, độ tin cậy, khả năng bảo trì... |
| Nginx | — | Web server hiệu năng cao, dùng làm reverse proxy, load balancer và SSL termination. |
| OpenTelemetry | OTEL | Framework quan sát hệ thống phân tán: distributed tracing, metrics, logs. |
| Optimistic Concurrency | — | Kỹ thuật xử lý concurrent writes bằng RowVersion — không lock DB, phát hiện conflict khi save. Xung đột trả **HTTP 409**. |
| Owned Entity | — | Kiểu EF Core không có định danh độc lập, được nhúng thành các cột của bảng chủ. `RecipeNutrition` là Owned Entity của `Recipe` — không có bảng, repository hay endpoint riêng (mục 7.2.1). |
| Partial Unique Index | — | Index duy nhất chỉ áp dụng cho tập bản ghi thỏa điều kiện. `IDX_Recipe_Slug` dùng `WHERE "IsDeleted" = false` để slug được giải phóng sau khi soft delete (mục 7.2). |
| Phiên đăng nhập (Session) | — | Một refresh token còn hiệu lực, tương ứng một thiết bị/trình duyệt đã đăng nhập. Nhận diện phiên hiện tại qua claim `sid` của access token (FR-AUTH-009). |
| Published | — | Trạng thái Recipe khi được công bố công khai. `RecipeStatus.Published`. |
| Rate Limiting | — | Giới hạn số lượng request từ một IP trong khoảng thời gian nhất định để ngăn brute force/DDoS. |
| Refresh Token | RT | Token dài hạn (7 ngày) dùng để lấy Access Token mới mà không cần đăng nhập lại. |
| Refresh Token Rotation | — | Mỗi lần dùng Refresh Token để refresh → token cũ bị revoke, cấp token mới (bảo mật cao hơn). |
| Reuse Detection | — | Cơ chế phát hiện khi Refresh Token đã bị revoke được dùng lại → revoke toàn bộ token family của user. |
| Resolver DNS động (Nginx) | — | Cấu hình `resolver` + `proxy_pass` qua biến khiến Nginx phân giải lại tên service định kỳ, thay vì chỉ một lần lúc khởi động như khối `upstream`. Bắt buộc trong môi trường container vì IP thay đổi khi tạo lại container (mục 6.5, MT-50). |
| Slug | — | Chuỗi URL-friendly, dạng chữ-thường-gạch-nối, duy nhất, dùng để định danh Recipe/Category trên URL. Không được trùng danh sách slug dành riêng (NFR-SEO-004). |
| Soft Delete | — | Đánh dấu `IsDeleted=true` thay vì xóa vật lý khỏi database. Dữ liệu được giữ 30 ngày và có thể được quản trị viên vận hành khôi phục trực tiếp trên DB; sau đó FR-JOB-003 xóa vĩnh viễn (NFR-REL-003). |
| Software Requirements Specification | SRS | Tài liệu đặc tả yêu cầu phần mềm theo IEEE 830 / ISO/IEC/IEEE 29148. |
| TanStack Query | — | Thư viện React quản lý server state: caching, background refetch, optimistic updates. |
| tsvector / tsquery | — | Kiểu dữ liệu PostgreSQL cho full-text search. `tsvector` là chỉ mục đã xử lý, `tsquery` là biểu thức tìm kiếm. |
| Unit of Work | UoW | Pattern đảm bảo nhiều operations được thực hiện trong một transaction duy nhất. |
| Worker (Hangfire) | — | Container `hangfire` chạy cùng image với API nhưng chỉ xử lý background job (`Hangfire__WorkerOnly=true`); API chỉ enqueue (mục 3.6, MT-47). |

---

## PHỤ LỤC D – BẢNG TRUY VẾT QUYẾT ĐỊNH KIẾN TRÚC CR-2026

Bảng dưới đây truy vết toàn bộ **41 điểm mâu thuẫn / bất nhất** (MT-01 → MT-41) được ghi nhận trong `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md` và quyết định tương ứng đã áp dụng vào phiên bản 1.1.0. Mức độ: 🔴 Cao (sai lệch kiến trúc/dữ liệu) · 🟡 Trung bình (lệch hợp đồng API) · 🟢 Thấp (biên tập).

| MT | Chủ đề | Mức | Quyết định đã chốt cho v1.1.0 | Vị trí áp dụng |
| --- | --- | --- | --- | --- |
| MT-01 | Tham số sắp xếp | 🟡 | Dùng `sortBy` + `sortOrder` với whitelist `{createdAt, publishedAt, title, cookTime, prepTime}`; mặc định `createdAt/desc`; ngoài whitelist → 400. Bỏ hẳn `sort=-field`. | FR-RCP-001, FR-RCP-011, FR-SRCH-003, Ch.8 Quy ước |
| MT-02 | Thời điểm tạo Nutrition | 🟡 | Owned Entity: chỉ tạo/sửa cùng recipe qua `POST`/`PUT /recipes`. Không có repository, DTO độc lập hay endpoint riêng. | FR-RCP-003, mục 7.2.1, Ch.8.3 |
| MT-03 | `StepNumber` do ai gán | 🟡 | Server toàn quyền: gán `Max + 1` khi thêm, tự renumber 1..N khi xóa/sắp xếp, trong **một** transaction. Client không bao giờ gửi `stepNumber`. Bổ sung `PATCH /steps/reorder`. | FR-RCP-010, mục 7.3, Ch.8.5 |
| MT-04 | Kiểu dữ liệu `Quantity` | 🟡 | Hai cột song song: `Quantity decimal(10,3) NULL` + `QuantityText varchar(50) NULL`, cả hai nullable. Không rỗng cả `Quantity`/`QuantityText`/`Unit`. | FR-RCP-009, mục 7.4, Ch.8.6, Phụ lục B |
| MT-05 | Hard vs Soft delete | 🔴 | **Soft Delete đồng bộ toàn hệ thống.** Partial unique index cho Slug; child entity không xóa theo; file MinIO dọn bởi FR-JOB-003 sau 30 ngày. | FR-RCP-007, FR-CAT-005, FR-JOB-003, mục 7.2, NFR-REL-003 |
| MT-06 | Điều kiện Publish | 🟡 | Bắt buộc `Steps.Count > 0` **và** `Ingredients.Count > 0`; thiếu → **400** `RECIPE_PUBLISH_INCOMPLETE`. | FR-RCP-005, Ch.8.3, Phụ lục B |
| MT-07 | Đếm sai tổng số FR | 🟢 | Tổng **36 FR** (sau khi thêm FR-AUTH-008 và FR-RCP-011); ghi rõ cách đếm 25 FR đặc tả đầy đủ + 11 FR dạng bảng gộp. *(v1.2.0: 37 FR sau khi thêm FR-AUTH-009 — xem MT-45.)* | mục 1.5, 2.2, đầu Ch.3 |
| MT-08 | Validation 422 hay 400 | 🔴 | **400 cho mọi lỗi validation/input.** Mã 422 bị loại bỏ hoàn toàn khỏi hệ thống. | Toàn Ch.3, Ch.8, Phụ lục A, B |
| MT-09 | Concurrency 409 hay 422 | 🔴 | **409 cho mọi xung đột trạng thái**, gồm `RowVersion` conflict (`RECIPE_CONCURRENCY_CONFLICT`). Frontend phân biệt qua trường `type`. | FR-RCP-004, Ch.8.3, Phụ lục A, B |
| MT-10 | `/auth/register` có trả token | 🔴 | **Có — auto-login.** Trả `AuthResponseDto` gồm cặp token + `user{}`. | FR-AUTH-001, Ch.8.1 |
| MT-11 | Luồng Google OAuth | 🔴 | **ID Token flow:** Frontend gửi `{ idToken }`, Backend verify bằng `Google.Apis.Auth`. Bỏ `ExternalLoginInfo`, bỏ Authorization Code + PKCE, bỏ redirect URI ở Backend. | FR-AUTH-003, mục 5.3, 6.1, 6.2, Ch.8.1 |
| MT-12 | Tên trường hồ sơ user | 🔴 | Dùng **`displayName`**, bỏ hẳn `fullName`. `UserName` sinh tự động từ prefix email. Bổ sung `bio` vào DTO. | FR-AUTH-001/006/007, mục 7.7, Ch.8.1 |
| MT-13 | Độ dài Refresh Token | 🟡 | **256-bit (32 bytes)**, Base64URL — khớp độ dài output SHA-256 và `TokenHash varchar(64)`. | FR-AUTH-001, NFR-SEC-002 |
| MT-14 | Trường `IsRevoked` | 🔴 | **Bỏ `IsRevoked`.** Một nguồn sự thật duy nhất: hợp lệ ⇔ `RevokedAt IS NULL AND ExpiresAt > NOW()`. | FR-AUTH-004/005, mục 7.8, NFR-SEC-002 |
| MT-15 | Lưu raw token hay hash | 🔴 | **Chỉ lưu SHA-256 hash** (`TokenHash`, `ReplacedByTokenHash`). Raw token chỉ tồn tại trong response HTTP, cấm ghi log. | FR-AUTH-004, mục 7.8, NFR-SEC-002, NFR-SEC-007 |
| MT-16 | `IMemoryCache` hay Redis | 🔴 | **Redis (`IDistributedCache`) cho mọi shared cache.** `IMemoryCache` không được dùng cho bất kỳ shared state nào. | mục 3.2, FR-CAT-001…005, NFR-PERF-003, NFR-SCALE-001 |
| MT-17 | TTL cache lệch nhau | 🟡 | **Bảng TTL chuẩn duy nhất** tại NFR-PERF-003 (30′/2′/2′/5′/1′); cơ chế duy nhất là Redis cache-aside, bỏ Output Cache. | NFR-PERF-003, toàn Ch.3, mục 5.1 |
| MT-18 | SDK truy cập MinIO | 🟡 | **`AWSSDK.S3`** (`ForcePathStyle = true`, `ServiceURL` override); phương thức `DeleteObjectAsync()`. | mục 2.1.2, FR-FILE-002, mục 5.3, 6.1, 6.2 |
| MT-19 | API quản lý ảnh | 🟡 | **PATCH metadata gộp** `/recipes/{id}/images/{imageId}`. Response upload đủ 7 trường. Giữ 2 quy tắc primary. | FR-RCP-008, Ch.8.4 |
| MT-20 | Nhóm bất nhất nhỏ đợt 1 (16 mục) | 🟢 | Next.js 15; Chrome 112+/Firefox 113+/Safari 16+/Edge 112+; MailKit + SMTP (bỏ SendGrid); `orderIndex`; `timerMinutes`; `prepTime`/`cookTime`; `title` bắt buộc ở step; `Instructions` NULL; mật khẩu đủ 4 loại ký tự; bổ sung 423/502; category soft delete; thêm unarchive; `pageSize` = 12; bộ filter đầy đủ; thêm `Expert`; đếm lại endpoint. | Rải toàn tài liệu |
| MT-21 | Policy `"VerifiedAuthor"` | 🔴 | **Bỏ khỏi phạm vi v1.1.0** — không có luồng xác nhận email nên policy sẽ chặn vĩnh viễn mọi thao tác ghi. Bỏ `emailConfirmed` khỏi DTO công khai. | mục 2.3, FR-AUTH-001, FR-JOB-001, mục 7.7 |
| MT-22 | Khóa tài khoản không có FR | 🟡 | Thêm **FR-AUTH-008** + `PATCH /users/{id}/status`; bổ sung kiểm tra `IsActive` vào FR-AUTH-002 và FR-AUTH-004; khi khóa thì revoke toàn bộ refresh token. | FR-AUTH-008, mục 2.3, 7.7, Ch.8.1, mục 5.1 |
| MT-23 | "Recipe nổi bật" không có dữ liệu | 🟡 | Định nghĩa lại "nổi bật" = **mới xuất bản nhất**: `GET /recipes?sortBy=publishedAt&sortOrder=desc&pageSize=8`. Không thêm cột, không thêm endpoint. | mục 5.1, FR-RCP-001 |
| MT-24 | Hứa "star rating" ngoài phạm vi | 🟡 | **Bỏ "star rating"** khỏi NFR-SEO-001 → "thời gian nấu, khẩu phần, nguyên liệu, calo". | NFR-SEO-001 |
| MT-25 | Ba vấn đề của Full-Text Search | 🔴 | **Generated column `STORED`** (không trigger) + configuration **`simple`** + hàm wrapper `unaccent_immutable`; diễn giải lại CONS-006 cho phép DDL trong migration. | FR-SRCH-001, mục 7.2, CONS-006 |
| MT-26 | Thiếu index cho cột sort/filter | 🟡 | **3 index composite** theo hình dạng truy vấn thật; NFR-PERF-004 đổi sang tiêu chí đo được bằng `EXPLAIN ANALYZE` (cấm Seq Scan trên `Recipes`). | mục 7.2, NFR-PERF-004 |
| MT-27 | 301 redirect slug cũ | 🟡 | Slug sinh lại được khi **Draft**, **khóa vĩnh viễn sau lần Publish đầu tiên** → không cần cơ chế 301 redirect và không cần bảng lịch sử slug. | NFR-SEO-004, FR-RCP-004, mục 7.2 |
| MT-28 | `sitemap.xml` sinh sai nơi | 🟡 | Chuyển sang **Next.js `app/sitemap.ts` + `app/robots.ts`**; bỏ ping Google (đã khai tử 6/2023); khe FR-JOB-003 dùng cho Permanent Purge Job. | NFR-SEO-003, FR-JOB-003, mục 5.3, NFR-SCALE-001 |
| MT-29 | Kubernetes ngoài phạm vi | 🟢 | Bỏ mọi nhắc đến K8s; thay bằng **`healthcheck:` của Docker Compose** + `depends_on: service_healthy` + Nginx `proxy_next_upstream`. | FR-OBS-001, NFR-REL-001, NFR-SEC-007, mục 6.5 |
| MT-30 | Domain có dùng FluentValidation | 🟡 | **Domain tuyệt đối sạch — chỉ .NET BCL.** FluentValidation ở Application (dữ liệu đầu vào), `DomainException` ở Domain (bất biến nghiệp vụ). | NFR-MAINT-004, CONS-001, mục 6.2 |
| MT-31 | Danh sách Pipeline Behavior | 🟢 | Chốt **4 behavior**: `Logging` (kèm cảnh báo > 500ms) → `Validation` → `Caching` → Handler → `CacheInvalidation`. Bỏ `PerformanceBehavior`. Ngưỡng > 100ms là EF Core interceptor. | mục 6.2, 6.3, FR-OBS-002, NFR-PERF-004 |
| MT-32 | Compose map cổng cố định | 🟡 | Dev giữ `5000:8080`; **`docker-compose.prod.yml` bỏ `ports`, thêm `deploy.replicas: 3`**. *(v1.2.0: Nginx dùng **resolver DNS động** thay cho khối `upstream` — xem MT-50.)* | mục 6.5, NFR-SCALE-003, mục 6.1 |
| MT-33 | Nhóm bất nhất nhỏ đợt 2 (9 mục) | 🟢 | `IEmailService`; job đã enqueue không mất khi restart; ISR ≤ TTL cache; đăng ký `/recipes/search` trước `/{slug}` *(v1.2.0: thứ tự đăng ký không quyết định — ASP.NET Core ưu tiên segment literal; yêu cầu thật là danh sách slug dành riêng — xem MT-53)*; role không phân cấp (seed Admin cả 2 role); quy ước đọc-slug/ghi-id; Compose dùng cả cho production; `CategoryDto` thêm `imageUrl`/`orderIndex`; FR-AUTH-006 trả 401 thay vì 404. | mục 5.3, 6.2, 5.1, Ch.8, mục 2.4.1, 7.7 |
| MT-34 | **Output Cache rò rỉ recipe Draft** | 🔴 | **Tách bạch Public/Private:** `GET /recipes`, `/recipes/{slug}`, `/recipes/search`, `/categories/{slug}` chỉ trả `Published` cho **mọi** người gọi; thêm **`GET /recipes/mine`** (FR-RCP-011) cấm cache, `Cache-Control: no-store`. Ghi nguyên tắc vào NFR-SEC-006. | FR-RCP-001/002/011, FR-CAT-002, NFR-SEC-006, NFR-PERF-003, Ch.8.3 |
| MT-35 | Máy trạng thái không đóng kín | 🔴 | Đóng kín: Draft ↔ Published → Archived → **unarchive về Draft** (`PATCH /recipes/{id}/unarchive`). Archive được từ cả Draft lẫn Published. Chuyển trạng thái sai → 409 `RECIPE_INVALID_STATE_TRANSITION`. `PublishedAt` gán ở **lần publish đầu tiên**, không ghi đè. | FR-RCP-005, FR-RCP-006, mục 7.2, Ch.8.3, Phụ lục B |
| MT-36 | FR-CAT-003 khai 409 không sinh ra được | 🟡 | Thêm bước **kiểm tra `Name` trùng chủ động** trước khi ghi → 409 `CATEGORY_NAME_EXISTS`; UNIQUE của DB là lớp phòng vệ thứ hai (bắt `23505`). Áp dụng cho cả FR-CAT-004. | FR-CAT-003, FR-CAT-004, mục 7.6, Ch.8.2 |
| MT-37 | Giới hạn độ dài lệch nhau | 🟡 | Lập **mục 7.9 – Bảng Giới hạn Dữ liệu Chuẩn** làm nguồn sự thật duy nhất; validator và cột DB phải **bằng nhau**. Giới hạn SEO chuyển thành quy tắc truncate lúc render. | mục 7.9, NFR-SEO-002, toàn bộ validator Ch.3 |
| MT-38 | Rate limit sau Nginx | 🟡 | Bật **`UseForwardedHeaders`** (`XForwardedFor \| XForwardedProto`) với danh sách mạng tin cậy = dải mạng Docker *(v1.2.0: khai báo bằng `KnownIPNetworks` — xem MT-54)*; Nginx set `proxy_set_header X-Forwarded-For`. | NFR-SEC-003, mục 6.2, 6.5, 7.8 |
| MT-39 | Hangfire Dashboard không vào được bằng JWT | 🟡 | Bảo vệ `/hangfire` bằng **HTTP Basic Auth tại tầng Nginx**, không dùng JWT (JWT Bearer không khả dụng cho điều hướng trình duyệt). | mục 3.6, 5.3, 6.5, 2.3 |
| MT-40 | MoSCoW thiếu và sai thứ tự phụ thuộc | 🟡 | Gán đủ MoSCoW cho 11 FR còn thiếu; nâng FR-RCP-006, FR-AUTH-003, FR-AUTH-006 lên **M**; bổ sung quy tắc *"FR mức Must Have không được phụ thuộc FR có mức ưu tiên thấp hơn"*; dùng W cho danh sách ngoài phạm vi. | đầu Ch.3, toàn bộ FR |
| MT-41 | Nhóm bất nhất nhỏ đợt 3 (10 mục) | 🟢 | FR-CAT-002 là xác thực **tùy chọn**; thống nhất quy tắc hiển thị theo MT-34; sửa mô tả cơ chế index tìm kiếm; bổ sung `altText`/`isPrimary` vào form-data; thêm 503 vào FR-RCP-008; làm rõ tiêu chí ảnh primary kế nhiệm; `Unit` tùy chọn; bỏ DXA khỏi mục 1.3; sửa quyền "xem logs" của Admin; bỏ mục tự tham chiếu ở Phụ lục C. | Rải toàn tài liệu |

**Tổng kết:** 41/41 mâu thuẫn đã được xử lý — 13 mục 🔴 Cao, 20 mục 🟡 Trung bình, 8 mục 🟢 Thấp. Bốn quyết định có ảnh hưởng kiến trúc sâu nhất (**MT-05** Soft Delete, **MT-08/09** chuẩn hóa mã lỗi, **MT-16** Redis, **MT-34** cách ly cache) cần được ghi thành **ADR** riêng theo yêu cầu của NFR-MAINT-003.

---

## PHỤ LỤC E – BẢNG TRUY VẾT QUYẾT ĐỊNH CR-2026-02

Bảng dưới đây truy vết **16 điểm** (MT-42 → MT-57) phát hiện ở lượt rà soát thứ tư — lượt đầu tiên đối chiếu SRS với **code đã chạy thật** (nhánh `main`, `SPEC/BAO_CAO_BUOI_2.md`). Phân tích đầy đủ (hiện trạng, phương án, lý do chọn) nằm trong `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`, mục "Lượt 4". Mức độ: 🔴 Cao · 🟡 Trung bình · 🟢 Thấp. Loại: **(a)** lỗi kỹ thuật trong chính v1.1.0 · **(b)** yêu cầu mồ côi · **(c)** SRS mâu thuẫn với thiết kế đã được kiểm chứng.

| MT | Chủ đề | Mức | Loại | Quyết định trong v1.2.0 | Vị trí áp dụng |
| --- | --- | --- | --- | --- | --- |
| MT-42 | Hai hình dạng response danh sách (`{data, meta}` vs `PagedResult`), kèm `pageSize: 10` lệch | 🟡 | (c) | Chốt **`PagedResult<T>`** `{ items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }`; đối tượng đơn trả thẳng DTO | §5.2, quy ước Chương 8 |
| MT-43 | Ba FR chỉ nhận URL ảnh nhưng không có endpoint upload nào | 🟡 | (b) | Chính thức hóa `POST /files/upload`, `DELETE /files/{**key}` (có từ Buổi 2) gắn vào FR-FILE-001/002 | §3.5, Chương 8.8 |
| MT-44 | `/dashboard/users` không có API danh sách người dùng | 🟡 | (b) | Bổ sung `GET /users` vào **FR-AUTH-008** (không tách FR mới) | FR-AUTH-008, Chương 8.1, §5.1 |
| MT-45 | Quản lý phiên có trong lộ trình nhưng không có FR | 🟢 | (b) | Thêm **FR-AUTH-009** (mức **C**) + 3 endpoint; claim `sid` để nhận diện phiên hiện tại; tổng FR 36 → **37** | FR-AUTH-009, NFR-SEC-002, Chương 8.1, §2.2 |
| MT-46 | 3 mã lỗi code đã dùng nhưng thiếu trong Phụ lục B | 🟢 | (c) | Thêm `FILE_FORBIDDEN` (403), `FILE_STORAGE_UNAVAILABLE` (503), `INTERNAL_ERROR` (500) — 24 → **27** mã | Phụ lục A, B, FR-RCP-008 |
| MT-47 | Hangfire "in-process" vs container worker riêng | 🟡 | (c) | Chuẩn hóa **worker riêng** — tách tải nặng CPU (resize ảnh) khỏi luồng request, scale độc lập | §2.1, §2.6.2, §3.6, §5.3, §6.1, §6.5, NFR-SCALE-001 |
| MT-48 | Sitemap cần toàn bộ slug, API danh sách giới hạn 50 | 🟢 | (b) | Thêm `GET /recipes/sitemap` gọn nhẹ, cache 1 giờ | NFR-SEO-003, Chương 8.3 |
| MT-49 | "Có thể khôi phục" nhưng không có chức năng khôi phục | 🟡 | (b) | Làm rõ: khôi phục do **quản trị viên vận hành** trên DB trong 30 ngày; tự phục vụ ngoài phạm vi | §1.2.3, NFR-REL-003, Phụ lục C |
| MT-50 | Cấu hình Nginx `upstream` tái tạo lỗi 502 đã sửa ở Buổi 2 | 🔴 | (a) | **Resolver DNS động** + `proxy_pass` qua biến | §6.1, §6.5, NFR-SCALE-003, Phụ lục D (MT-32) |
| MT-51 | Lệnh healthcheck dùng `curl` không có trong image; `pg_isready` thiếu `-h` | 🟡 | (a) | Lệnh đã kiểm chứng: `curl` cài thêm cho `api`, `wget` cho `frontend`, `mc ready local`, `pg_isready -h 127.0.0.1` | §6.5 |
| MT-52 | Admin tự khóa: 403 (FR) vs 409 `VALIDATION_ERROR` (Chương 8) | 🟡 | (a) | Thống nhất **403** | FR-AUTH-008, Chương 8.1, Phụ lục A |
| MT-53 | Giải thích sai cơ chế route; danh sách slug dành riêng mỗi nơi một kiểu | 🟢 | (a) | Literal luôn thắng `{slug}`; **một danh sách duy nhất** `search, mine, sitemap, new, edit` | NFR-SEO-004, FR-CAT-003, FR-RCP-003, Chương 8, Phụ lục D (MT-33) |
| MT-54 | `KnownProxies` không nhận dải mạng; `KnownNetworks` obsolete trên .NET 10 | 🟢 | (a) | Chỉ đích danh **`KnownIPNetworks`** | NFR-SEC-003, §4.2, §6.2, Phụ lục D (MT-38) |
| MT-55 | Không quy định nơi lưu token phía Frontend | 🟡 | (b) | Access token **chỉ trong bộ nhớ**; refresh token là thứ duy nhất lưu bền; CSP thành yêu cầu tường minh | NFR-SEC-002, NFR-SEC-005 |
| MT-56 | Ràng buộc `DEFERRABLE` không nói phải là constraint; đổi ảnh primary vướng partial index | 🟢 | (a) | Ghi rõ **UNIQUE CONSTRAINT** (index không thể deferrable); đổi ảnh primary làm hai bước trong một transaction | FR-RCP-010, §7.3, §7.5 |
| MT-57 | `/categories/[slug]` ghi là ISR nhưng trang đọc `?page=` không thể dựng tĩnh | 🟢 | (a) | **SSR + Data Cache** `revalidate: 120` | §5.1 |

**Tổng kết:** 16/16 điểm đã xử lý — 1 mục 🔴, 8 mục 🟡, 7 mục 🟢. Toàn tài liệu: **57 mâu thuẫn/bất nhất đã được xử lý** qua hai Change Request. Số liệu sau v1.2.0: **37 FR**, **44 endpoint**, **27 mã lỗi**.

> **Bài học quy trình ghi lại cho các lượt sau:** 7/16 điểm của lượt này thuộc loại (a) — *lỗi do chính bản sửa trước đưa vào*, không có sẵn trong v1.0.0. Chúng chỉ lộ ra khi đối chiếu tài liệu với hệ thống đang chạy. Từ v1.2.0 trở đi, mọi Change Request chạm tới hạ tầng hoặc cấu hình phải được **kiểm chứng trên môi trường thật** trước khi duyệt, không chỉ rà chéo giữa các chương.


## PHỤ LỤC F – BẢNG TRUY VẾT QUYẾT ĐỊNH CR-2026-03

| MT | Vấn đề ở v1.2.0 | Quyết định trong v1.2.1 | Mục SRS thay đổi | Hiện thực |
| --- | --- | --- | --- | --- |
| MT-58 | §2.6.1 chỉ yêu cầu 50 công thức và 5 tác giả mẫu; giảng viên yêu cầu ≥ 20 danh mục, ≥ 100 công thức, mỗi công thức ≥ 10 nguyên liệu và ≥ 5 bước | Nâng ngưỡng dữ liệu mẫu; nội dung món ăn phải đúng với món; Bogus sinh phần ngẫu nhiên; seed idempotent, tự bù cho database đang có | §2.6.1 | `Infrastructure/Persistence/Seed` (`RecipeSeedCatalog`, `DatabaseSeeder`, `Data/*.json`) — Buổi 2, Dev 4 |

---

*— Hết tài liệu —*

> **Nguồn gốc:** bản 1.0.0 được chuyển đổi từ `SPEC/SRS_Culinary_Blog_v1.0.0.pdf` (71 trang) sang Markdown, giữ nguyên 100% nội dung gốc. Bản **1.1.0** áp dụng Change Request **CR-2026** lên bản đó: xử lý toàn bộ 41 điểm mâu thuẫn được ghi nhận trong `SPEC/SRS_MAU_THUAN_VA_GIAI_PHAP.md`. Mọi thay đổi so với 1.0.0 đều được đánh dấu bằng chú thích `[CR-2026 / MT-xx]` tại chỗ và truy vết đầy đủ ở **Phụ lục D**. Bản **1.2.0** áp dụng Change Request **CR-2026-02**: đối chiếu với code đã chạy thật, xử lý thêm 16 điểm (MT-42 → MT-57) — đánh dấu `[CR-2026-02 / MT-xx]` tại chỗ, truy vết ở **Phụ lục E**. Bản **1.2.1** áp dụng Change Request **CR-2026-03**: cập nhật yêu cầu dữ liệu mẫu ở §2.6.1 (MT-58) — đánh dấu `[CR-2026-03 / MT-58]` tại chỗ, truy vết ở **Phụ lục F**. Các bản 1.0.0 và 1.1.0 được giữ nguyên để đối chiếu.



