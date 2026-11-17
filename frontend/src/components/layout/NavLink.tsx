'use client';

import type { ReactNode } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';

interface NavLinkProps {
  href: string;
  children: ReactNode;
  className?: string;
  activeClassName?: string;
  /** `true`: chỉ sáng khi trùng khớp tuyệt đối (trang chủ `/`). Mặc định: sáng cả ở trang con (`/recipes/...`). */
  exact?: boolean;
  onClick?: () => void;
}

/**
 * Link điều hướng biết mình đang được mở (`aria-current="page"` — NFR-USE-002). Tách thành Client Component nhỏ để `Header`
 * vẫn là Server Component: chỉ phần cần `usePathname()` mới gửi JavaScript xuống trình duyệt.
 */
export default function NavLink({
  href,
  children,
  className = '',
  activeClassName = '',
  exact = false,
  onClick,
}: NavLinkProps) {
  const pathname = usePathname();
  const active = exact ? pathname === href : pathname === href || pathname.startsWith(`${href}/`);
  return (
    <Link
      href={href}
      onClick={onClick}
      aria-current={active ? 'page' : undefined}
      className={`${className} ${active ? activeClassName : ''}`}
    >
      {children}
    </Link>
  );
}
