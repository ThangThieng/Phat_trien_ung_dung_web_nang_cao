'use client';

/**
 * BẢN TẠM (seam) của Buổi 7 — Dev 4. Giao diện `/search` (Lab 7) cần `Sheet`, `Select`, `Badge` của shadcn/ui, nhưng phần
 * khởi tạo shadcn/ui của Dev 2 (Buổi 6, PR #52) chưa vào `develop` nên `components/ui/` còn trống. File này cung cấp bản tối
 * giản **cùng tên và cùng props đang dùng** để khi Dev 2 merge, nơi dùng chỉ đổi đúng một dòng import rồi xóa phần tương ứng
 * (xóa cả file khi hết chỗ dùng):
 *
 * | Bản tạm                                                              | Thay bằng                | Ai    |
 * |----------------------------------------------------------------------|--------------------------|-------|
 * | `Sheet`, `SheetContent`, `SheetHeader`, `SheetTitle`, `SheetFooter`   | `components/ui/sheet`    | Dev 2 |
 * | `Select`, `SelectTrigger`, `SelectValue`, `SelectContent`, `SelectItem` | `components/ui/select` | Dev 2 |
 * | `Badge`                                                              | `components/ui/badge`    | Dev 2 |
 *
 * Nơi dùng: `features/search/components/SearchFilterPanel.tsx`, `features/search/components/SearchResults.tsx`.
 *
 * Hành vi giữ đúng phần Radix UI bảo đảm (WCAG — NFR-USE-002): `Sheet` là `role="dialog"` + `aria-modal`, nhận focus khi mở,
 * giữ Tab trong khung, Escape/bấm nền để đóng, trả focus về nút đã mở, khóa cuộn trang nền. `Select` dựng bằng `<select>`
 * gốc của trình duyệt (bàn phím, trình đọc màn hình, bảng chọn của điện thoại có sẵn) và có `name` để gửi được trong form GET
 * khi chưa có JavaScript — như prop `name` của Radix Select.
 */
import {
  Children,
  createContext,
  isValidElement,
  useContext,
  useEffect,
  useId,
  useMemo,
  useRef,
  type HTMLAttributes,
  type ReactNode,
} from 'react';
import { createPortal } from 'react-dom';
import clsx from 'clsx';
import { X } from 'lucide-react';

// ---------------------------------------------------------------- Badge

const BADGE_VARIANTS = {
  default: 'border-transparent bg-orange-700 text-white',
  secondary: 'border-transparent bg-orange-100 text-orange-900',
  outline: 'border-gray-300 text-gray-700',
} as const;

export function Badge({
  variant = 'default',
  className,
  ...props
}: HTMLAttributes<HTMLSpanElement> & { variant?: keyof typeof BADGE_VARIANTS }) {
  return (
    <span
      data-slot="badge"
      className={clsx(
        'inline-flex shrink-0 items-center justify-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap',
        BADGE_VARIANTS[variant],
        className,
      )}
      {...props}
    />
  );
}

// ---------------------------------------------------------------- Sheet

interface SheetContextValue {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  titleId: string;
}

const SheetContext = createContext<SheetContextValue | null>(null);

function useSheet(component: string): SheetContextValue {
  const context = useContext(SheetContext);
  if (!context) throw new Error(`${component} phải nằm trong <Sheet>.`);
  return context;
}

/** Chỉ dùng kiểu điều khiển (`open` + `onOpenChange`) — đúng cách `/search` dùng `Sheet` của shadcn. */
export function Sheet({
  open,
  onOpenChange,
  children,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: ReactNode;
}) {
  const titleId = useId();
  const value = useMemo(() => ({ open, onOpenChange, titleId }), [open, onOpenChange, titleId]);
  return <SheetContext.Provider value={value}>{children}</SheetContext.Provider>;
}

const FOCUSABLE =
  'a[href], button:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

const SIDE_CLASS = {
  left: 'inset-y-0 left-0 h-full w-11/12 max-w-sm border-r',
  right: 'inset-y-0 right-0 h-full w-11/12 max-w-sm border-l',
  bottom: 'inset-x-0 bottom-0 max-h-[85vh] border-t rounded-t-2xl',
} as const;

export function SheetContent({
  side = 'right',
  className,
  children,
}: {
  side?: keyof typeof SIDE_CLASS;
  className?: string;
  children: ReactNode;
}) {
  const { open, onOpenChange, titleId } = useSheet('SheetContent');
  const panelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return undefined;
    const opener = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    const { overflow } = document.body.style;
    document.body.style.overflow = 'hidden';
    const panel = panelRef.current;
    (panel?.querySelector<HTMLElement>(FOCUSABLE) ?? panel)?.focus();

    // Escape đóng; Tab/Shift+Tab xoay vòng trong khung (focus trap như Radix Dialog).
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.stopPropagation();
        onOpenChange(false);
        return;
      }
      if (event.key !== 'Tab' || !panel) return;
      const focusable = Array.from(panel.querySelectorAll<HTMLElement>(FOCUSABLE));
      if (focusable.length === 0) return;
      const firstItem = focusable[0];
      const lastItem = focusable[focusable.length - 1];
      const outside = !panel.contains(document.activeElement);
      if (event.shiftKey && (document.activeElement === firstItem || outside)) {
        event.preventDefault();
        lastItem.focus();
      } else if (!event.shiftKey && (document.activeElement === lastItem || outside)) {
        event.preventDefault();
        firstItem.focus();
      }
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.style.overflow = overflow;
      opener?.focus();
    };
  }, [open, onOpenChange]);

  if (!open) return null;

  return createPortal(
    <div data-slot="sheet-portal">
      <div
        data-slot="sheet-overlay"
        aria-hidden
        onClick={() => onOpenChange(false)}
        className="fixed inset-0 z-50 bg-black/40"
      />
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
        data-slot="sheet-content"
        data-side={side}
        className={clsx(
          'fixed z-50 flex flex-col gap-4 overflow-y-auto border-gray-200 bg-white text-sm shadow-xl focus:outline-none',
          SIDE_CLASS[side],
          className,
        )}
      >
        {children}
        <button
          type="button"
          onClick={() => onOpenChange(false)}
          className="absolute top-3 right-3 rounded-md p-1.5 text-gray-600 hover:bg-orange-50 focus-visible:outline-2 focus-visible:outline-orange-600"
        >
          <X aria-hidden className="h-5 w-5" />
          <span className="sr-only">Đóng</span>
        </button>
      </div>
    </div>,
    document.body,
  );
}

export function SheetHeader({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="sheet-header"
      className={clsx('flex flex-col gap-1 p-4 pr-12', className)}
      {...props}
    />
  );
}

export function SheetFooter({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="sheet-footer"
      className={clsx('mt-auto flex flex-col gap-2 p-4', className)}
      {...props}
    />
  );
}

export function SheetTitle({ className, children, ...props }: HTMLAttributes<HTMLHeadingElement>) {
  const { titleId } = useSheet('SheetTitle');
  return (
    <h2
      id={titleId}
      data-slot="sheet-title"
      className={clsx('text-base font-semibold text-gray-900', className)}
      {...props}
    >
      {children}
    </h2>
  );
}

// ---------------------------------------------------------------- Select

/* eslint-disable react/no-unused-prop-types -- props của `SelectTrigger` được `Select` đọc qua `child.props`. */
interface SelectTriggerProps {
  id?: string;
  className?: string;
  'aria-label'?: string;
  'aria-labelledby'?: string;
  children?: ReactNode;
}
/* eslint-enable react/no-unused-prop-types */

/**
 * Các phần dưới đây chỉ là "khai báo" như cấu trúc của Radix Select; `Select` đọc props của chúng rồi dựng một `<select>`
 * gốc. Bản shadcn thật render từng phần — nơi dùng không đổi gì ngoài dòng import.
 */
export function SelectTrigger({ children }: SelectTriggerProps): ReactNode {
  return children ?? null;
}

export function SelectValue({ placeholder }: { placeholder?: string }): ReactNode {
  return placeholder ?? null;
}

export function SelectContent({ children }: { children: ReactNode }): ReactNode {
  return children;
}

export function SelectItem({ children }: { value: string; children: string }): ReactNode {
  return children;
}

export function Select({
  value,
  onValueChange,
  name,
  disabled,
  children,
}: {
  value?: string;
  onValueChange?: (value: string) => void;
  name?: string;
  disabled?: boolean;
  children: ReactNode;
}) {
  let trigger: SelectTriggerProps = {};
  let placeholder: string | undefined;
  const items: { value: string; label: string }[] = [];

  Children.forEach(children, (child) => {
    if (!isValidElement(child)) return;
    if (child.type === SelectTrigger) {
      trigger = child.props as SelectTriggerProps;
      Children.forEach(trigger.children, (part) => {
        if (isValidElement<{ placeholder?: string }>(part) && part.type === SelectValue) {
          placeholder = part.props.placeholder;
        }
      });
    } else if (child.type === SelectContent) {
      Children.forEach((child.props as { children: ReactNode }).children, (item) => {
        if (isValidElement<{ value: string; children: string }>(item) && item.type === SelectItem) {
          items.push({ value: item.props.value, label: item.props.children });
        }
      });
    }
  });

  return (
    <select
      id={trigger.id}
      aria-label={trigger['aria-label']}
      aria-labelledby={trigger['aria-labelledby']}
      name={name}
      disabled={disabled}
      value={value ?? ''}
      onChange={(event) => onValueChange?.(event.target.value)}
      data-slot="select-trigger"
      className={clsx(
        'h-9 w-full rounded-lg border border-gray-300 bg-white px-2 text-sm text-gray-800 focus:border-orange-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-orange-300',
        trigger.className,
      )}
    >
      {value === undefined && placeholder !== undefined && (
        <option value="" disabled hidden>
          {placeholder}
        </option>
      )}
      {items.map((item) => (
        <option key={item.value} value={item.value}>
          {item.label}
        </option>
      ))}
    </select>
  );
}
