using System.Reflection;
using System.Text.RegularExpressions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.API.Middleware.ExceptionMapping;
using CulinaryBlog.Application.Common.Interfaces.Persistence;
using CulinaryBlog.Domain.Exceptions;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>
/// Buổi 3 — luật của nền Domain Exceptions / Repository &amp; Unit of Work / Global Exception Middleware.
/// Các luật này biến quy ước trong kế hoạch thành thứ build kiểm được: quên đăng ký mã HTTP cho một exception,
/// để lọt IQueryable ra khỏi repository, hay để danh mục mã lỗi lệch SRS đều làm test đỏ — không âm thầm sai.
/// </summary>
public partial class DomainExceptionRulesTests
{
    private const string DomainExceptionsNamespace = "CulinaryBlog.Domain.Exceptions";

    /// <summary>Mã có trong code nhưng chưa có trong Phụ lục B — mỗi dòng phải ghi rõ khi nào được gỡ.</summary>
    private static readonly string[] TemporarilyAllowedExtraCodes = [];

    private static readonly Assembly DomainAssembly = typeof(DomainException).Assembly;

    [Fact]
    public void EveryConcreteDomainException_IsExplicitlyMappedToAnHttpStatus()
    {
        var map = ExceptionStatusMap.FromAssembly(typeof(GlobalExceptionMiddleware).Assembly);

        var unmapped = DomainAssembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(DomainException).IsAssignableFrom(t))
            .Where(t => !map.IsExplicitlyMapped(t))
            .Select(t => t.FullName)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var message = "Domain exception chưa có mã HTTP — thêm .Map<T>(status) vào file {Module}ExceptionMappings.cs của module: "
            + string.Join(", ", unmapped);
        Assert.True(unmapped.Length == 0, message);
    }

    [Fact]
    public void DomainExceptions_LiveInTheExceptionsNamespace()
    {
        var misplaced = DomainAssembly.GetTypes()
            .Where(t => typeof(Exception).IsAssignableFrom(t))
            .Where(t => t.Namespace is null || !t.Namespace.StartsWith(DomainExceptionsNamespace, StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToArray();

        Assert.True(misplaced.Length == 0, $"Exception của Domain phải nằm trong {DomainExceptionsNamespace}[.Module]: {string.Join(", ", misplaced)}");
    }

    [Fact]
    public void RepositoryInterfaces_DoNotExposeIQueryable()
    {
        var persistenceInterfaces = typeof(IUnitOfWork).Assembly.GetTypes()
            .Where(t => t.IsInterface && (t.Name.EndsWith("Repository", StringComparison.Ordinal) || t.Name.StartsWith("IRepository", StringComparison.Ordinal) || t == typeof(IUnitOfWork)));

        var leaks = persistenceInterfaces
            .SelectMany(t => t.GetMethods().Select(m => (Type: t, Method: m)))
            .Where(x => ExposesQueryable(x.Method.ReturnType))
            .Select(x => $"{x.Type.Name}.{x.Method.Name}")
            .ToArray();

        Assert.True(leaks.Length == 0, $"Repository không được trả IQueryable (chi tiết EF lọt lên Application): {string.Join(", ", leaks)}");
    }

    [Fact]
    public void GenericRepository_HasNoHardDelete()
    {
        var deleteLike = typeof(IRepository<>).GetMethods()
            .Where(m => m.Name.Contains("Remove", StringComparison.Ordinal) || m.Name.Contains("Delete", StringComparison.Ordinal))
            .Select(m => m.Name)
            .ToArray();

        Assert.True(deleteLike.Length == 0, "IRepository<T> cố ý không có xóa cứng — mọi xóa là xóa mềm (NFR-REL-003).");
    }

    [Fact]
    public void ErrorCodes_MatchSrsAppendixB()
    {
        var srsCodes = ReadAppendixBCodes();
        var codeConstants = typeof(ErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet(StringComparer.Ordinal);

        var missing = srsCodes.Except(codeConstants).Order(StringComparer.Ordinal).ToArray();
        var extra = codeConstants.Except(srsCodes).Except(TemporarilyAllowedExtraCodes).Order(StringComparer.Ordinal).ToArray();

        Assert.True(missing.Length == 0, $"Mã có trong SRS Phụ lục B nhưng thiếu trong ErrorCodes: {string.Join(", ", missing)}");
        Assert.True(extra.Length == 0, $"Mã có trong ErrorCodes nhưng không có trong SRS Phụ lục B (phải qua Change Request): {string.Join(", ", extra)}");
    }

    private static bool ExposesQueryable(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(IQueryable) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
    }

    /// <summary>
    /// Đọc bảng Phụ lục B của bản SRS mới nhất trong SPEC/ — SRS là nguồn sự thật duy nhất của danh mục mã lỗi,
    /// nên test so với chính tài liệu thay vì với một danh sách chép tay (danh sách chép tay sẽ lệch khi SRS đổi).
    /// </summary>
    private static HashSet<string> ReadAppendixBCodes()
    {
        var specDirectory = Path.Combine(Directory.GetParent(BackendRoot())!.FullName, "SPEC");
        var srsFile = Directory.GetFiles(specDirectory, "SRS_Culinary_Blog_v*.md")
            .OrderByDescending(f => Version.Parse(SrsVersionPattern().Match(Path.GetFileName(f)).Groups["v"].Value))
            .First();

        var text = File.ReadAllText(srsFile);
        var start = text.IndexOf("## PHỤ LỤC B", StringComparison.Ordinal);
        var end = text.IndexOf("## PHỤ LỤC C", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, $"Không tìm thấy Phụ lục B trong {srsFile}.");

        var codes = AppendixBRowPattern().Matches(text[start..end])
            .Select(m => m.Groups["code"].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(codes.Count >= 29, $"Chỉ đọc được {codes.Count} mã từ Phụ lục B của {Path.GetFileName(srsFile)} — định dạng bảng đã đổi?");
        return codes;
    }

    private static string BackendRoot()
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CulinaryBlog.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Không tìm thấy thư mục backend (CulinaryBlog.sln).");
    }

    [GeneratedRegex(@"SRS_Culinary_Blog_v(?<v>\d+\.\d+\.\d+)\.md$")]
    private static partial Regex SrsVersionPattern();

    [GeneratedRegex(@"^\|\s*`(?<code>[A-Z][A-Z_]+)`\s*\|", RegexOptions.Multiline)]
    private static partial Regex AppendixBRowPattern();
}
