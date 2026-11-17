import { z } from 'zod';
import { vi } from 'zod/locales';

/** Thông báo mặc định của zod bằng tiếng Việt, cho ô nào chưa khai báo câu riêng. Chỉ đổi câu chữ, không đổi điều kiện kiểm tra. */
z.config(vi());
