namespace CulinaryBlog.Domain.Enums;

/// <summary>Hành động làm đổi trạng thái công thức (máy trạng thái SRS §3.3 — MT-35). Xóa mềm không đổi Status nên không có ở đây.</summary>
public enum RecipeAction
{
    Publish,
    Unpublish,
    Archive,
    Unarchive,
}
