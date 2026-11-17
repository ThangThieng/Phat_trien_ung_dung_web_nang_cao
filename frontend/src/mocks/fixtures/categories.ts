/**
 * Dữ liệu mẫu cho mockup (Buổi 6 — Dev 3, Lab 6): 6 danh mục lấy NGUYÊN từ dữ liệu mẫu của Backend
 * `Infrastructure/Persistence/Seed/Data/categories.json` (tên, mô tả, thứ tự; `recipeCount` = số công thức seed của danh mục).
 * `satisfies Category[]`: đổi kiểu `Category` mà quên cập nhật dữ liệu mẫu là lỗi biên dịch.
 */
import type { Category } from '@/types/category';

export const mockCategories = [
  {
    id: '00000000-0000-4000-8000-000000000006',
    name: 'Bún & Phở',
    slug: 'bun-pho',
    description: 'Các món nước truyền thống của Việt Nam.',
    imageUrl: null,
    orderIndex: 6,
    recipeCount: 8,
  },
  {
    id: '00000000-0000-4000-8000-000000000002',
    name: 'Món chính',
    slug: 'mon-chinh',
    description: 'Các món ăn chính cho bữa cơm gia đình.',
    imageUrl: null,
    orderIndex: 2,
    recipeCount: 8,
  },
  {
    id: '00000000-0000-4000-8000-000000000001',
    name: 'Món khai vị',
    slug: 'mon-khai-vi',
    description: 'Món ăn nhẹ mở đầu bữa ăn: gỏi, chả giò, nộm...',
    imageUrl: null,
    orderIndex: 1,
    recipeCount: 6,
  },
  {
    id: '00000000-0000-4000-8000-000000000003',
    name: 'Canh & Súp',
    slug: 'canh-sup',
    description: 'Canh, súp thanh mát và bổ dưỡng.',
    imageUrl: null,
    orderIndex: 3,
    recipeCount: 6,
  },
  {
    id: '00000000-0000-4000-8000-000000000020',
    name: 'Món Âu',
    slug: 'mon-au',
    description: 'Mì Ý, bít tết, súp kem và các món Âu phổ biến.',
    imageUrl: null,
    orderIndex: 20,
    recipeCount: 4,
  },
  {
    id: '00000000-0000-4000-8000-000000000008',
    name: 'Tráng miệng & Đồ uống',
    slug: 'trang-mieng-do-uong',
    description: 'Chè, kem, sinh tố và các loại nước giải khát.',
    imageUrl: null,
    orderIndex: 8,
    recipeCount: 5,
  },
] satisfies Category[];
