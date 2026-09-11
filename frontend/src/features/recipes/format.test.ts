import { formatMinutes, formatQuantity } from './format';

describe('formatMinutes', () => {
  it.each([
    [0, 'Không cần nấu'],
    [45, '45 phút'],
    [60, '1 giờ'],
    [95, '1 giờ 35 phút'],
  ])('%i phút → "%s"', (minutes, expected) => {
    expect(formatMinutes(minutes)).toBe(expected);
  });
});

describe('formatQuantity', () => {
  it('hiển thị số lượng kèm đơn vị', () => {
    expect(formatQuantity(500, 'gram')).toBe('500 gram');
  });

  it('nguyên liệu không có số lượng → "vừa đủ"', () => {
    expect(formatQuantity(null, 'thìa canh')).toBe('vừa đủ (thìa canh)');
    expect(formatQuantity(null, null)).toBe('vừa đủ');
  });
});
