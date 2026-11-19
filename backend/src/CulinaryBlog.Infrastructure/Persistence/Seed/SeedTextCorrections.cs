namespace CulinaryBlog.Infrastructure.Persistence.Seed;

/// <summary>Phần văn bản của công thức mẫu được sửa.</summary>
public enum SeedTextField
{
    /// <summary>Tên công thức — đổi kèm slug (slug sinh từ tên).</summary>
    Title,

    /// <summary>Mô tả công thức.</summary>
    Description,

    /// <summary>Tên một nguyên liệu (so khớp nguyên văn cả tên).</summary>
    IngredientName,

    /// <summary>Một đoạn trong mô tả bước (thay đúng đoạn đó).</summary>
    StepText,
}

/// <summary>Một chỗ sửa: công thức Wikibooks (theo pageId nguồn), phần cần sửa, văn bản cũ và văn bản mới.</summary>
public sealed record SeedTextCorrection(long PageId, SeedTextField Field, string Old, string New);

/// <summary>
/// Sửa bản dịch cho database đã seed từ trước (19/11/2026: nhóm duyệt 101 công thức Wikibooks — §B.7 bước 10 của kế hoạch).
/// Seeder không ghi đè công thức đã có, nên file JSON đã sửa chỉ có tác dụng với database mới. Danh sách này đưa đúng các chỗ sửa vào
/// database cũ: chỉ áp dụng khi giá trị hiện tại CÒN ĐÚNG văn bản cũ và công thức thuộc tác giả mẫu — ai đã tự sửa thì giữ nguyên.
/// Chạy lại nhiều lần vô hại (lần sau không còn văn bản cũ để thay).
/// </summary>
public static class SeedTextCorrections
{
    public static IReadOnlyList<SeedTextCorrection> All { get; } =
    [
        new(295828, SeedTextField.StepText, "ở giá thứ 3 từ dưới lên;", "ở giá thuộc phần dưới cùng khi chia chiều cao lò làm 3 phần;"),
        new(160002, SeedTextField.IngredientName, "bạch đậu khấu đen xay", "thảo quả xay (bạch đậu khấu đen)"),
        new(34618, SeedTextField.IngredientName, "sô-cô-la nguyên chất", "sô-cô-la đen"),
        new(156848, SeedTextField.IngredientName, "hành tím lớn", "hành tây tím"),
        new(156848, SeedTextField.StepText, "Bóc vỏ và băm hành tím.", "Bóc vỏ và băm hành tây tím."),
        new(156848, SeedTextField.StepText, "Cho hành tím và seitan vào", "Cho hành tây tím và seitan vào"),
        new(185952, SeedTextField.IngredientName, "đậu đen tách vỏ nguyên hạt (urad dal)", "đậu urad (đậu đen Ấn Độ) tách vỏ nguyên hạt"),
        new(185952, SeedTextField.StepText, "ngâm đậu đen cùng đậu thận", "ngâm đậu urad cùng đậu thận"),
        new(185952, SeedTextField.StepText, "Nấu đậu đen, đậu thận", "Nấu đậu urad, đậu thận"),
        new(263111, SeedTextField.IngredientName, "đậu đen tách vỏ (urad dal)", "đậu urad (đậu đen Ấn Độ) tách vỏ"),
        new(263111, SeedTextField.Description, "vùng Gujarat làm từ đậu đen.", "vùng Gujarat làm từ đậu urad (đậu đen Ấn Độ)."),
        new(480539, SeedTextField.Title, "Dürüm bữa sáng (Bánh cuốn trứng, khoai và bò)", "Dürüm bữa sáng (Bánh mì cuộn trứng, khoai và bò)"),
        new(480539, SeedTextField.Description, "tương tự bánh cuốn.", "tương tự bánh mì cuộn (wrap)."),
        new(127879, SeedTextField.StepText, "trứng muối chua", "trứng ngâm giấm"),
        new(127705, SeedTextField.IngredientName, "thịt bò bắp, giò heo hoặc da heo", "thịt nạm bò, giò heo hoặc da heo"),
        new(212336, SeedTextField.StepText, "đạt 185°F và nhân đạt 165°F.", "đạt 185°F (85 °C) và nhân đạt 165°F (74 °C)."),
        new(31180, SeedTextField.IngredientName, "màng bọc dạ dày bò (beef caps)", "ruột bò làm vỏ dồi (beef caps)"),
        new(31180, SeedTextField.StepText, "trừ màng bọc, giấm", "trừ ruột bò, giấm"),
        new(31180, SeedTextField.StepText, "Rửa màng bọc trong", "Rửa ruột bò trong"),
        new(31180, SeedTextField.StepText, "Nhồi vào màng bọc và", "Nhồi vào ruột bò và"),
    ];

    /// <summary>Ghi chú nguồn của công thức Wikibooks chứa <c>curid={pageId}&amp;</c> — cách nhận ra công thức cũ khi tên (và slug) đã đổi.</summary>
    public static string SourceMarker(long pageId) => $"curid={pageId}&";
}
