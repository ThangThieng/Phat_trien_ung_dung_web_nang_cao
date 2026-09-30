using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Files;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Exceptions;
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

        // Kích thước → MIME khai báo → magic bytes (NFR-SEC-004) — dùng chung với FR-RCP-008.
        var image = await ImageUploadInspector
            .InspectAsync(request.Content, request.Length, request.DeclaredContentType, cancellationToken)
            .ConfigureAwait(false);

        await using var content = image.Content;
        var stored = await storage
            .UploadAsync(content, $"uploads/{userId}", image.Format.Extension, image.Format.ContentType, cancellationToken)
            .ConfigureAwait(false);

        return new FileUploadResultDto(stored.Key, stored.Url, image.Format.ContentType, request.Length);
    }
}
