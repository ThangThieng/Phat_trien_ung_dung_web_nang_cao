/** Tiền tố do seeder ghi vào `instructions` (CR-2026-07); phải khớp `RecipeSeedCatalog.SourceNotePrefix` ở Backend. */
export const SOURCE_NOTE_PREFIX = 'Nguồn dữ liệu:';

/**
 * URL ảnh Commons có thể chứa dấu phẩy và ngoặc trong tên file (`File:Phở_bò,_Hà_Nội_(1).jpg`), nên không thể dừng ở các ký tự đó.
 * Lấy đến trước khoảng trắng, chỉ bỏ dấu câu nằm SÁT CUỐI ngoài URL như `(https://a/b),` hoặc `https://a/b.`.
 */
const URL_PATTERN = /(https:\/\/\S+?)(?=[.,)]*(?:\s|$))/g;

/**
 * Ghi công theo CC BY-SA 4.0 cho công thức và ảnh lấy từ Wikibooks, Wikimedia Commons (Server Component).
 * `instructions` là cột legacy, nên công thức do người dùng tạo vẫn không hiển thị cột này như trước.
 * React tự escape nội dung; chỉ URL `https://` mới thành liên kết nên không tiêm được HTML hay `javascript:`.
 */
export default function RecipeSourceNote({ instructions }: { instructions: string | null }) {
  if (!instructions?.startsWith(SOURCE_NOTE_PREFIX)) return null;

  return (
    <aside aria-label="Nguồn dữ liệu" className="mt-12 border-t border-line pt-4 text-xs text-ink-soft">
      <p className="break-words">
        {instructions.split(URL_PATTERN).map((part, index) =>
          // `split` với nhóm bắt: phần tử ở vị trí lẻ luôn là URL khớp mẫu.
          index % 2 === 1 ? (
            <a
              key={part}
              href={part}
              target="_blank"
              rel="nofollow noopener noreferrer"
              className="text-gold-ink underline hover:text-gold-ink"
            >
              {part}
            </a>
          ) : (
            part
          ),
        )}
      </p>
    </aside>
  );
}
