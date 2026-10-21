'use client';

import { Component, type ErrorInfo, type ReactNode } from 'react';
import { QueryErrorResetBoundary } from '@tanstack/react-query';
import ErrorFallback from './ErrorFallback';

export interface ErrorBoundaryFallbackProps {
  error: unknown;
  /** Xóa lỗi của boundary VÀ cho TanStack Query gọi lại các truy vấn đã lỗi bên trong. */
  reset: () => void;
}

interface ErrorBoundaryProps {
  children: ReactNode;
  /** Giao diện khi lỗi. Mặc định: `ErrorFallback` dạng `inline`. */
  fallback?: (props: ErrorBoundaryFallbackProps) => ReactNode;
  /** Gọi khi người dùng bấm "Thử lại", sau khi đã reset truy vấn (ví dụ để báo cho cha). */
  onReset?: () => void;
}

interface InnerProps extends ErrorBoundaryProps {
  resetQueries: () => void;
}

interface InnerState {
  error: unknown;
  hasError: boolean;
}

/**
 * Lớp (class component) vì React CHỈ cho class bắt lỗi xảy ra lúc render (`getDerivedStateFromError`) — chưa có hook tương
 * đương. `error.tsx` của App Router bắt lỗi ở cấp route; boundary này dùng ở cấp COMPONENT để một khối hỏng (ví dụ "công thức
 * mới") không kéo sập cả trang.
 */
class InnerErrorBoundary extends Component<InnerProps, InnerState> {
  constructor(props: InnerProps) {
    super(props);
    this.state = { error: null, hasError: false };
  }

  static getDerivedStateFromError(error: unknown): InnerState {
    return { error, hasError: true };
  }

  componentDidCatch(error: unknown, info: ErrorInfo) {
    // Chỉ ghi ở môi trường phát triển: lỗi API đã có CorrelationId trong log Backend; không gửi stack trace đi đâu.
    if (process.env.NODE_ENV !== 'production') {
      // eslint-disable-next-line no-console
      console.error('[ErrorBoundary]', error, info.componentStack);
    }
  }

  reset = () => {
    const { resetQueries, onReset } = this.props;
    resetQueries();
    onReset?.();
    this.setState({ error: null, hasError: false });
  };

  render() {
    const { hasError, error } = this.state;
    const { children, fallback } = this.props;
    if (!hasError) return children;
    return fallback ? (
      fallback({ error, reset: this.reset })
    ) : (
      <ErrorFallback error={error} reset={this.reset} variant="inline" />
    );
  }
}

/**
 * Error boundary cấp component, nối với `QueryErrorResetBoundary` của TanStack Query: nút "Thử lại" vừa reset boundary vừa
 * cho các truy vấn lỗi bên trong (`throwOnError`/`useSuspenseQuery`) gọi lại API — không phải tải lại cả trang.
 */
export default function ErrorBoundary(props: ErrorBoundaryProps) {
  return (
    <QueryErrorResetBoundary>
      {({ reset }) => <InnerErrorBoundary {...props} resetQueries={reset} />}
    </QueryErrorResetBoundary>
  );
}
