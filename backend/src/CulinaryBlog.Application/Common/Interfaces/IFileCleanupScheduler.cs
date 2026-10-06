namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// Xóa tệp trên MinIO BẤT ĐỒNG BỘ qua Hangfire (FR-RCP-008 bước 16): request không phải chờ MinIO, và MinIO lỗi tạm thời
/// thì job tự thử lại thay vì làm hỏng thao tác xóa ảnh đã commit. KHÔNG dùng khi xóa mềm công thức (FR-RCP-007).
/// </summary>
public interface IFileCleanupScheduler
{
    void ScheduleDelete(IReadOnlyCollection<string> fileUrls);
}
