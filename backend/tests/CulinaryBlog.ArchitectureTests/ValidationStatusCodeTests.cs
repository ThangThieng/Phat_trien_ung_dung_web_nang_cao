using System.Reflection;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>
/// Retrofit D-11 / MT-08 (SRS v1.2.1 §3): mã 422 bị loại bỏ hoàn toàn — mọi lỗi validation/input trả 400.
/// Test quét mã nguồn thay vì quét IL vì <c>StatusCodes.Status422UnprocessableEntity</c> là hằng số int,
/// sau khi biên dịch chỉ còn số 422 lẫn giữa mọi số nguyên khác nên không thể phân biệt ở mức assembly.
/// </summary>
public class ValidationStatusCodeTests
{
    private const string ForbiddenSymbol = "Status422UnprocessableEntity";

    private static readonly string[] ScannedExtensions = [".cs"];

    [Fact]
    public void BackendSource_ShouldNotReference_Http422()
    {
        var offenders = EnumerateSourceFiles()
            .Where(file => File.ReadAllText(file).Contains(ForbiddenSymbol, StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(BackendRoot(), file))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Mã 422 đã bị loại bỏ khỏi hệ thống (D-11 / MT-08) — dùng Status400BadRequest. Vi phạm: {string.Join(", ", offenders)}");
    }

    private static IEnumerable<string> EnumerateSourceFiles() =>
        Directory
            .EnumerateFiles(Path.Combine(BackendRoot(), "src"), "*.*", SearchOption.AllDirectories)
            .Where(file => ScannedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                        && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

    /// <summary>Đi ngược từ thư mục output của test cho tới khi gặp CulinaryBlog.sln (thư mục backend/).</summary>
    private static string BackendRoot()
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CulinaryBlog.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Không tìm thấy CulinaryBlog.sln từ thư mục test.");
    }
}
