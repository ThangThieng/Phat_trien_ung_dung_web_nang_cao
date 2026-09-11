using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth;

/// <summary>NFR-SEC-001: ≥ 8 ký tự, ít nhất 1 chữ hoa, 1 chữ thường, 1 số, 1 ký tự đặc biệt.</summary>
public static class PasswordRules
{
    public const int MinimumLength = 8;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(MinimumLength).WithMessage("Mật khẩu tối thiểu 8 ký tự.")
            .MaximumLength(128).WithMessage("Mật khẩu tối đa 128 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt.");
}
