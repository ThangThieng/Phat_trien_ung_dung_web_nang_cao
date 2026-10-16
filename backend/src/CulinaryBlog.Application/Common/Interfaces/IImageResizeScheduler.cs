namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>
/// FR-JOB-002 – xếp hàng job đổi kích thước ảnh (medium 800×600, thumbnail 300×300) sau khi tải ảnh công thức lên.
/// Bất đồng bộ vì đổi kích thước ảnh 5MB tốn 1–3 giây CPU, và thất bại của nó KHÔNG được làm hỏng việc tải lên:
/// ảnh gốc vẫn hiển thị bình thường khi mediumUrl/thumbnailUrl còn null.
/// </summary>
public interface IImageResizeScheduler
{
    void ScheduleResize(Guid imageId);
}
