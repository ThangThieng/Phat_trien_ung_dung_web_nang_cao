using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Files;

/// <summary>FR-FILE-002 – xóa file theo object key. Chủ sở hữu (uploads/{userId}/…) hoặc Admin. Idempotent.</summary>
public sealed record DeleteFileCommand(string FileKey) : IRequest;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty()
            .MaximumLength(500)
            .Must(k => !k.Contains("..", StringComparison.Ordinal) && !k.StartsWith('/'))
            .WithMessage("File key không hợp lệ.");
    }
}

public sealed class DeleteFileCommandHandler(IFileStorageService storage, ICurrentUser currentUser)
    : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException(ErrorCodes.AuthTokenInvalid, "Yêu cầu đăng nhập.");

        var ownsFile = request.FileKey.StartsWith($"uploads/{userId}/", StringComparison.Ordinal);
        if (!ownsFile && !currentUser.IsAdmin)
        {
            throw new ForbiddenException(ErrorCodes.FileForbidden, "Bạn không có quyền xóa file này.");
        }

        await storage.DeleteAsync(request.FileKey, cancellationToken).ConfigureAwait(false);
    }
}
