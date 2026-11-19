namespace CulinaryBlog.Infrastructure.Persistence.Seed;

/// <summary>
/// Ảnh mẫu đã được nhóm thay (19/11/2026: ảnh kém hấp dẫn, mờ hoặc chụp lúc đang nấu). Catalog đã trỏ sang ảnh mới, nhưng database
/// seed từ trước vẫn giữ ảnh cũ vì <see cref="Jobs.SeedImageImportJob"/> chỉ nhập cho công thức chưa có ảnh. Danh sách này ghi trang
/// mô tả (Wikimedia Commons) của ảnh CŨ theo slug: công thức nào còn ghi công ảnh cũ trong <c>Instructions</c> — tức chưa ai sửa phần
/// đó — được lên lịch xóa ảnh cũ và nhập ảnh mới. Người dùng đã tự đổi ảnh hay sửa ghi chú thì không khớp nên giữ nguyên.
/// </summary>
public static class SeedImageReplacements
{
    public static IReadOnlyDictionary<string, string> OldImageBySlug { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bap-cai-cuon-ga-tay-va-lua-mach-voi-thi-la-tuoi"] = "https://commons.wikimedia.org/wiki/File:Turkey_and_Barley_Cabbage_Roll_in_Tomato_Sauce.jpg",
        ["suon-xao-chua-ngot"] = "https://commons.wikimedia.org/wiki/File:Suon_xao_chua_ngot.png",
        ["tom-rim-man-ngot"] = "https://commons.wikimedia.org/wiki/File:N%E1%BA%A5u_t%C3%B4m_rim_(%C4%91%E1%BA%A3o_t%C3%B4m_cho_ch%C3%ADn)._T4_n%C4%83m_2018_(1).jpg",
        ["com-tam-suon-bi-cha"] = "https://commons.wikimedia.org/wiki/File:C%C6%A1m_T%E1%BA%A5m_B%C3%AC_S%C6%B0%E1%BB%9Dn_Ch%E1%BA%A3.jpg",
    };

    /// <summary>Công thức còn ghi công ảnh cũ (ảnh hiện tại là ảnh mẫu cũ, chưa bị ai thay).</summary>
    public static bool IsOutdated(string slug, string? instructions) =>
        OldImageBySlug.TryGetValue(slug, out var old) && instructions is not null && instructions.Contains(old, StringComparison.Ordinal);
}
