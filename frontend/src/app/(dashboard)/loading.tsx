import LoadingSkeleton from '@/components/shared/LoadingSkeleton';

/** Trang dashboard phần lớn là bảng/danh sách — khung chờ dạng bảng trong khung sidebar + thanh trên cùng. */
export default function DashboardLoading() {
  return <LoadingSkeleton variant="table" />;
}
