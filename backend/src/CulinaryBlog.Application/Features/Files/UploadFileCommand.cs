using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Files;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Files;

/// <summary>FR-FILE-001 – upload ảnh lên MinIO vào "uploads/{userId}/{Guid}{ext}".</summary>
public sealed record UploadFileCommand(Stream Content, long Length, string? DeclaredContentType) : IRequest<FileUploadResultDto>;

public sealed record FileUploadResultDto(string Key, string Url, string ContentType, long Size);

public sealed class UploadFileCommandHandler(IFileStorageService storage, ICurrentUser currentUser)
    : IRequestHandler<UploadFileCommand, FileUploadResultDto>
{
    public async Task<FileUploadResultDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");

        // 1. Kích thước – kiểm tra TRƯỚC khi đọc stream (NFR-SEC-004)
        if (request.Length <= 0 || request.Length > ImageFileInspector.MaxFileSizeBytes)
        {
            throw new BadRequestException(ErrorCodes.FileSizeExceeded, "Kích thước file vượt quá giới hạn 5MB.");
        }

        // 2. MIME type khai báo
        if (!ImageFileInspector.IsAllowedContentType(request.DeclaredContentType))
        {
            throw new BadRequestException(ErrorCodes.FileMimeInvalid, "Loại file không được phép. Chỉ chấp nhận JPEG, PNG, WebP, AVIF.");
        }

        // 3. Magic bytes – định dạng thật phải khớp MIME khai báo
        var header = new byte[ImageFileInspector.HeaderLength];
        var read = await request.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
        var detected = ImageFileInspector.Detect(header.AsSpan(0, read));

        if (detected is null || !string.Equals(detected.ContentType, request.DeclaredContentType!.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(ErrorCodes.FileMimeInvalid, "File không hợp lệ.");
        }

        // 4. Upload: header đã đọc + phần còn lại của stream (không buffer toàn bộ file vào memory)
        await using var fullContent = new PrefixedReadStream(header.AsMemory(0, read), request.Content, request.Length);
        var stored = await storage
            .UploadAsync(fullContent, $"uploads/{userId}", detected.Extension, detected.ContentType, cancellationToken)
            .ConfigureAwait(false);

        return new FileUploadResultDto(stored.Key, stored.Url, detected.ContentType, request.Length);
    }
}
