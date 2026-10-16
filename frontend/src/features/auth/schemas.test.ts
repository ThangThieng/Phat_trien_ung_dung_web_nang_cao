import { loginSchema, registerSchema } from './schemas';

const valid = {
  displayName: 'Nguyễn Văn A',
  email: 'a@example.com',
  password: 'Passw0rd!',
  confirmPassword: 'Passw0rd!',
};

describe('registerSchema (mirror backend RegisterUserCommandValidator)', () => {
  it('chấp nhận dữ liệu hợp lệ', () => {
    expect(registerSchema.safeParse(valid).success).toBe(true);
  });

  it.each([
    ['short1!', 'thiếu độ dài'],
    ['password1!', 'thiếu chữ hoa'],
    ['PASSWORD1!', 'thiếu chữ thường'],
    ['Password!', 'thiếu số'],
    ['Password1', 'thiếu ký tự đặc biệt'],
  ])('từ chối mật khẩu yếu %s (%s)', (password) => {
    const result = registerSchema.safeParse({ ...valid, password, confirmPassword: password });
    expect(result.success).toBe(false);
  });

  it('báo lỗi khi xác nhận mật khẩu không khớp', () => {
    const result = registerSchema.safeParse({ ...valid, confirmPassword: 'Other0rd!' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0].path).toEqual(['confirmPassword']);
  });

});

describe('loginSchema', () => {
  it('yêu cầu email hợp lệ và mật khẩu', () => {
    expect(loginSchema.safeParse({ email: 'x', password: '' }).success).toBe(false);
    expect(loginSchema.safeParse({ email: 'a@example.com', password: 'x' }).success).toBe(true);
  });
});
