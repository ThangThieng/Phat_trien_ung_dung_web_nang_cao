using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Features.Files;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints;

/// <summary>FR-FILE-001/002 – upload/xóa ảnh trên MinIO (dùng lại cho avatar, ảnh bước, ảnh danh mục).</summary>
public static class FilesEndpoints
{
    /// <summary>Giới hạn body của request (kiểm tra 5MB chính xác nằm trong handler để trả FILE_SIZE_EXCEEDED).</summary>
    private const long MaxRequestBodyBytes = 10 * 1024 * 1024;

    public static RouteGroupBuilder MapFilesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/files").WithTags("Files").RequireAuthorization(AuthorizationPolicies.Author);

        group.MapPost("/upload", UploadAsync)
            .WithName("UploadFile")
            .WithSummary("FR-FILE-001 – Upload ảnh (JPEG/PNG/WebP/AVIF, ≤ 5MB, kiểm tra magic bytes)")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodyBytes))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResultDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapDelete("/{**fileId}", DeleteAsync)
            .WithName("DeleteFile")
            .WithSummary("FR-FILE-002 – Xóa ảnh theo object key (idempotent)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return api;
    }

    private static async Task<IResult> UploadAsync(IFormFile file, ISender sender, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var result = await sender.Send(new UploadFileCommand(stream, file.Length, file.ContentType), ct).ConfigureAwait(false);
        return Results.Created(result.Url, result);
    }

    private static async Task<IResult> DeleteAsync(string fileId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new DeleteFileCommand(Uri.UnescapeDataString(fileId)), ct).ConfigureAwait(false);
        return Results.NoContent();
    }
}
