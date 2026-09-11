import { z } from 'zod';

/** Mirror RegisterUserCommandValidator (backend) – NFR-SEC-001. */
export const registerSchema = z
  .object({
    fullName: z
      .string()
      .trim()
      .min(2, 'Họ tên phải từ 2 đến 100 ký tự.')
      .max(100, 'Họ tên phải từ 2 đến 100 ký tự.'),
    email: z
      .string()
      .trim()
      .min(1, 'Email không được để trống.')
      .email('Email không đúng định dạng.'),
    userName: z
      .string()
      .trim()
      .min(3, 'Tên đăng nhập phải từ 3 đến 50 ký tự.')
      .max(50, 'Tên đăng nhập phải từ 3 đến 50 ký tự.')
      .regex(/^[a-zA-Z0-9_.]+$/, 'Tên đăng nhập chỉ gồm chữ, số, dấu chấm và gạch dưới.'),
    password: z
      .string()
      .min(8, 'Mật khẩu tối thiểu 8 ký tự.')
      .max(128, 'Mật khẩu tối đa 128 ký tự.')
      .regex(/[A-Z]/, 'Mật khẩu phải có ít nhất 1 chữ hoa.')
      .regex(/[a-z]/, 'Mật khẩu phải có ít nhất 1 chữ thường.')
      .regex(/[0-9]/, 'Mật khẩu phải có ít nhất 1 chữ số.')
      .regex(/[^a-zA-Z0-9]/, 'Mật khẩu phải có ít nhất 1 ký tự đặc biệt.'),
    confirmPassword: z.string(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Mật khẩu xác nhận không khớp.',
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Email không được để trống.')
    .email('Email không đúng định dạng.'),
  password: z.string().min(1, 'Mật khẩu không được để trống.'),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
