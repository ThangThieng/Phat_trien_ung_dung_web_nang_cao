import { highlightSegments } from '../highlight';

/** Tô sáng từ khóa tìm kiếm (FR-SRCH-001) — so khớp không dấu, theo đầu từ; không có từ khóa thì trả nguyên văn bản. */
export default function HighlightText({
  text,
  terms,
}: {
  text: string;
  terms?: readonly string[];
}) {
  if (!terms || terms.length === 0) return <span>{text}</span>;

  return (
    <>
      {highlightSegments(text, terms).map((segment, index) =>
        segment.match ? (
          // eslint-disable-next-line react/no-array-index-key -- các đoạn sinh từ cùng một chuỗi, thứ tự cố định
          <mark key={index} className="rounded bg-yellow-200 px-0.5 text-inherit">
            {segment.text}
          </mark>
        ) : (
          // eslint-disable-next-line react/no-array-index-key -- như trên
          <span key={index}>{segment.text}</span>
        ),
      )}
    </>
  );
}
