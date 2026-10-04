using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.ValueObjects;

/// <summary>
/// Value Object <c>Slug</c> (SRS §6.2 – Domain Layer): chuỗi URL-friendly hợp lệ, bất biến. Chỉ gồm chữ thường a–z, số 0–9
/// và dấu "-" ngăn cách (không bắt đầu/kết thúc/lặp dấu "-"). Sinh từ văn bản bằng <see cref="SlugHelper"/>;
/// nếu slug đã có người giữ thì thêm hậu tố số ("pho-bo" → "pho-bo-2") qua <see cref="WithSuffix"/>.
/// </summary>
public readonly record struct Slug
{
    private Slug(string value) => Value = value;

    /// <summary>Giá trị slug; <c>default(Slug)</c> không phải slug hợp lệ và trả chuỗi rỗng.</summary>
    public string Value { get; } = string.Empty;

    /// <summary>Slug trùng danh sách dành riêng (NFR-SEO-004 / MT-53) sẽ không bao giờ truy cập được — người gọi phải thêm hậu tố.</summary>
    public bool IsReserved => ReservedSlugs.Contains(Value);

    /// <summary>Tạo từ một slug đã có sẵn; ném <see cref="ArgumentException"/> nếu sai định dạng.</summary>
    public static Slug From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!IsWellFormed(value))
        {
            throw new ArgumentException(
                $"'{value}' không phải slug hợp lệ (chỉ gồm a–z, 0–9 và dấu '-' ngăn cách).",
                nameof(value));
        }

        return new Slug(value);
    }

    /// <summary>Sinh slug từ tên hiển thị ("Phở bò Hà Nội" → "pho-bo-ha-noi"); tên không chứa chữ/số nào thì ném <see cref="ArgumentException"/>.</summary>
    public static Slug FromText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var generated = SlugHelper.Generate(text);
        return generated.Length == 0
            ? throw new ArgumentException($"Không sinh được slug từ '{text}'.", nameof(text))
            : new Slug(generated);
    }

    /// <summary>Hậu tố số cho lần trùng thứ <paramref name="number"/>: 1 → giữ nguyên, 2 → "-2", 3 → "-3", v.v.</summary>
    public Slug WithSuffix(int number) => new(SlugHelper.WithSuffix(Value, number));

    public override string ToString() => Value;

    private static bool IsWellFormed(string value)
    {
        if (value[0] == '-' || value[^1] == '-')
        {
            return false;
        }

        var previousWasDash = false;
        foreach (var c in value)
        {
            var isDash = c == '-';
            var isAllowed = c is >= 'a' and <= 'z' or >= '0' and <= '9' || isDash;
            if (!isAllowed || (isDash && previousWasDash))
            {
                return false;
            }

            previousWasDash = isDash;
        }

        return true;
    }
}
