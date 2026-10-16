/**
 * FR-SRCH-001 (Buổi 5): tô sáng từ khóa trong kết quả tìm kiếm. Backend tìm KHÔNG DẤU và theo TIỀN TỐ ("pho" ra "Phở bò",
 * tsquery `pho:* & bo:*`) nên việc tô sáng cũng phải so khớp không dấu, theo đầu từ — nếu so khớp chính xác thì "pho" tìm
 * được "Phở" nhưng không có chữ nào được tô, trông như kết quả sai.
 */

export interface TextSegment {
  text: string;
  match: boolean;
}

const COMBINING_MARK = /[̀-ͯ]/;
const WORD_CHAR = /[a-z0-9]/;

/** Bỏ dấu một ký tự: NFD tách dấu, `đ` → `d`, chữ thường. Dấu rời (đã tách sẵn) → chuỗi rỗng. */
function foldChar(char: string): string {
  return char.normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/[đĐ]/g, 'd').toLowerCase();
}

/** Chuỗi đã bỏ dấu + bảng ánh xạ vị trí ký tự đã bỏ dấu → vị trí trong chuỗi gốc. */
function fold(text: string): { folded: string; origin: number[] } {
  let folded = '';
  const origin: number[] = [];
  for (let i = 0; i < text.length; i += 1) {
    const piece = foldChar(text[i]);
    for (let j = 0; j < piece.length; j += 1) {
      folded += piece[j];
      origin.push(i);
    }
  }
  return { folded, origin };
}

/** Tách từ khóa người dùng thành các từ không dấu, chỉ giữ a-z0-9 (giống `SearchTermBuilder` phía backend). */
export function splitSearchTerms(query: string): string[] {
  const terms = fold(query)
    .folded.split(/[^a-z0-9]+/)
    .filter((term) => term.length > 0);
  return Array.from(new Set(terms));
}

/** Cắt `text` thành các đoạn, đánh dấu đoạn khớp một từ khóa ở đầu từ (so khớp không dấu). */
export function highlightSegments(text: string, terms: readonly string[]): TextSegment[] {
  if (!text || terms.length === 0) return [{ text, match: false }];

  const { folded, origin } = fold(text);
  const ranges: [number, number][] = [];

  terms.forEach((term) => {
    if (!term) return;
    let from = folded.indexOf(term);
    while (from !== -1) {
      const atWordStart = from === 0 || !WORD_CHAR.test(folded[from - 1]);
      if (atWordStart) {
        let end = origin[from + term.length - 1] + 1;
        while (end < text.length && COMBINING_MARK.test(text[end])) end += 1;
        ranges.push([origin[from], end]);
      }
      from = folded.indexOf(term, from + 1);
    }
  });

  if (ranges.length === 0) return [{ text, match: false }];

  ranges.sort((a, b) => a[0] - b[0]);
  const merged: [number, number][] = [];
  ranges.forEach((range) => {
    const last = merged[merged.length - 1];
    if (last && range[0] <= last[1]) {
      last[1] = Math.max(last[1], range[1]);
    } else {
      merged.push([range[0], range[1]]);
    }
  });

  const segments: TextSegment[] = [];
  let cursor = 0;
  merged.forEach(([start, end]) => {
    if (start > cursor) segments.push({ text: text.slice(cursor, start), match: false });
    segments.push({ text: text.slice(start, end), match: true });
    cursor = end;
  });
  if (cursor < text.length) segments.push({ text: text.slice(cursor), match: false });
  return segments;
}
