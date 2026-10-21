import LoadingSkeleton from '@/components/shared/LoadingSkeleton';

/** Khung chờ cùng kích thước thẻ đăng nhập/đăng ký (`AuthCard`) — không nhảy bố cục khi trang về. */
export default function AuthLoading() {
  return <LoadingSkeleton variant="form" />;
}
