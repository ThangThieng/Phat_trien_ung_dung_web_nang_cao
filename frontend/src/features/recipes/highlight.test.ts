import { highlightSegments, splitSearchTerms } from './highlight';

const matched = (text: string, terms: string[]) =>
  highlightSegments(text, terms)
    .filter((s) => s.match)
    .map((s) => s.text);

describe('splitSearchTerms', () => {
  it('bỏ dấu, tách từ, bỏ ký tự đặc biệt, loại trùng', () => {
    expect(splitSearchTerms('Phở & bò!! phở')).toEqual(['pho', 'bo']);
    expect(splitSearchTerms('Đậu hũ')).toEqual(['dau', 'hu']);
    expect(splitSearchTerms('   ')).toEqual([]);
  });
});

describe('highlightSegments', () => {
  it('"pho" tô sáng "Phở" — tìm không dấu như backend', () => {
    expect(matched('Phở bò Hà Nội', ['pho'])).toEqual(['Phở']);
  });

  it('khớp theo đầu từ (tiền tố) như tsquery `pho:*`', () => {
    expect(matched('Phở bò', ['ph'])).toEqual(['Ph']);
    expect(matched('Tôm chiên', ['om'])).toEqual([]);
  });

  it('nhiều từ khóa, giữ nguyên dấu và chữ hoa của văn bản gốc', () => {
    expect(matched('Bún Bò Huế', ['bun', 'bo'])).toEqual(['Bún', 'Bò']);
  });

  it('chuỗi ghép lại đúng bằng văn bản gốc', () => {
    const text = 'Gà nướng mật ong, ăn kèm đồ chua';
    const joined = highlightSegments(text, ['ga', 'mat', 'do'])
      .map((s) => s.text)
      .join('');
    expect(joined).toBe(text);
  });

  it('hoạt động với chữ có dấu tách rời (NFD)', () => {
    const decomposed = 'Phở bò'; // "Phở bò" dạng tổ hợp
    expect(matched(decomposed, ['pho'])).toEqual(['Phở']);
  });

  it('gộp các khoảng chồng nhau', () => {
    expect(matched('Phở phở', ['ph', 'pho'])).toEqual(['Phở', 'phở']);
  });

  it('không có từ khóa hoặc không khớp → một đoạn không tô', () => {
    expect(highlightSegments('Cháo gà', [])).toEqual([{ text: 'Cháo gà', match: false }]);
    expect(highlightSegments('Cháo gà', ['xyz'])).toEqual([{ text: 'Cháo gà', match: false }]);
    expect(highlightSegments('', ['a'])).toEqual([{ text: '', match: false }]);
  });
});
