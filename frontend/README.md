# Frontend – Culinary Blog

Next.js App Router (CONS-003: không dùng Pages Router). Mục này ghi lại **hiện trạng đã kiểm chứng** của dự án (Lab 5 – Dev 3);
dự án **không** được khởi tạo lại bằng `create-next-app` (xung đột X-9, §3 – sẽ ghi đè `package.json`, `tsconfig.json`, cấu hình ESLint Airbnb và code các buổi trước).

## Kiến trúc Frontend

### Công nghệ (đối chiếu `package.json`, `tsconfig.json`, `next.config.ts`, `postcss.config.mjs`)

| Hạng mục      | Hiện trạng                                                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Framework     | `next@15.5.25` (App Router), `react@19.1.0`                                                                                            |
| Ngôn ngữ      | TypeScript `strict: true`; alias `@/*` → `src/*`                                                                                       |
| Giao diện     | Tailwind CSS v4 qua `@tailwindcss/postcss`                                                                                             |
| Lint / format | ESLint Airbnb + `jsx-a11y` + `eslint-plugin-import`, Prettier (`npm run lint`, `npm run format`)                                       |
| Test đơn vị   | Jest + Testing Library (`npm test`)                                                                                                    |
| Test e2e      | `@playwright/test` đã có trong devDependencies và script `npm run e2e`; **chưa có** `playwright.config.*` và thư mục `e2e/` trong repo |
| Docker        | `output: 'standalone'` (build nhiều giai đoạn, CONS-009)                                                                               |

### Cấu trúc `src/`

```
src/
├─ app/         # chỉ route: page/layout/loading/error/not-found/route.ts – không chứa nghiệp vụ
├─ components/
│  ├─ ui/       # DÀNH RIÊNG cho shadcn/ui (Buổi 6), tên file kebab-case – hiện để trống
│  ├─ layout/   # SiteHeader… (Header, Footer, DashboardSidebar ở Buổi 6)
│  └─ shared/   # component dùng chung nhiều module: FormField, ImageUploader, Pagination, RecipeGridSkeleton
├─ features/    # code theo module: auth, recipes, categories, system
├─ hooks/       # hook dùng chung nhiều module: use-debounced-value, use-media-query
├─ lib/         # tiện ích không phụ thuộc React/UI: api-client, query-client, config
├─ stores/      # Zustand – CHỈ state phía client (xem quy tắc dưới)
└─ types/       # kiểu hợp đồng API: api, auth, recipe, category, search
```

`components/shared/` (không phải `ui/`) chứa `Pagination.tsx`: Windows/macOS không phân biệt hoa thường, nên `Pagination.tsx` và
`pagination.tsx` do `npx shadcn add pagination` sinh ra ở Buổi 6 sẽ là cùng một file.

### Quy tắc phụ thuộc giữa thư mục (ESLint `import/no-restricted-paths`, kiểm ở mỗi lần `npm run lint`)

| Thư mục                                           | Không được import từ                                                  |
| ------------------------------------------------- | --------------------------------------------------------------------- |
| `types/`                                          | mọi thư mục khác (chỉ chứa kiểu, được import lẫn nhau trong `types/`) |
| `lib/`                                            | `app`, `components`, `features`, `stores`, `hooks`                    |
| `components/ui/`                                  | `app`, `features`, `stores`                                           |
| `features/*`, `components/*`, `hooks/`, `stores/` | `app/`                                                                |

### Quy tắc cho `stores/`

Store chỉ giữ **state phía client**; dữ liệu từ server thuộc **TanStack Query**. Store đầu tiên (`auth-store`) vào ở Buổi 6 – Dev 1.

### Kiểu dữ liệu và kiểm thử hợp đồng

Kiểu TypeScript viết tay trong `types/` (khớp DTO Backend, JSON camelCase). `types/__tests__/*-contract.test.ts` dùng
`satisfies Record<keyof T, true>` để mỗi trường thêm/bớt ở interface bắt buộc sửa bảng khóa, rồi so khóa với Backend:
`docs/api/openapi.v1.json` nếu file có, ngược lại đọc khai báo `record` C# trong `backend/`; không có nguồn nào thì test tự bỏ qua.

## Lệnh

```
npm run typecheck   # tsc --noEmit
npm run lint        # ESLint (gồm quy tắc phụ thuộc thư mục)
npm test            # Jest
npm run build       # next build
```
