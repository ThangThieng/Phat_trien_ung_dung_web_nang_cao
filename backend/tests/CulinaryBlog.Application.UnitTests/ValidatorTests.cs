using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Recipes;

namespace CulinaryBlog.Application.UnitTests;

public class ValidatorTests
{
    private readonly RegisterUserCommandValidator _register = new();

    [Fact]
    public void Register_ValidCommand_Passes() =>
        Assert.True(_register.Validate(new RegisterUserCommand("Nguyễn Văn A", "a@example.com", "nguyen.a", "Passw0rd!")).IsValid);

    [Theory]
    [InlineData("short1!")] // < 8 ký tự
    [InlineData("password1!")] // thiếu chữ hoa
    [InlineData("PASSWORD1!")] // thiếu chữ thường
    [InlineData("Password!")] // thiếu số
    [InlineData("Password1")] // thiếu ký tự đặc biệt
    public void Register_WeakPassword_Fails(string password)
    {
        var result = _register.Validate(new RegisterUserCommand("Nguyễn Văn A", "a@example.com", "nguyen.a", password));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Theory]
    [InlineData("not-an-email", "nguyen.a")]
    [InlineData("a@example.com", "có dấu")]
    public void Register_InvalidEmailOrUserName_Fails(string email, string userName) =>
        Assert.False(_register.Validate(new RegisterUserCommand("Nguyễn Văn A", email, userName, "Passw0rd!")).IsValid);

    [Theory]
    [InlineData(0, 12, false)]
    [InlineData(1, 51, false)]
    [InlineData(1, 50, true)]
    public void GetRecipes_PagingBounds(int page, int pageSize, bool valid) =>
        Assert.Equal(valid, new GetRecipesQueryValidator().Validate(new GetRecipesQuery(page, pageSize)).IsValid);

    [Theory]
    [InlineData(null, RecipeSortField.CreatedAt, true, true)]
    [InlineData("title", RecipeSortField.Title, false, true)]
    [InlineData("-cookTime", RecipeSortField.CookTime, true, true)]
    [InlineData("password", RecipeSortField.CreatedAt, false, false)]
    public void SortParser_AcceptsWhitelistOnly(string? sort, RecipeSortField field, bool descending, bool ok)
    {
        var parsed = RecipeSortParser.TryParse(sort, out var actualField, out var actualDescending);

        Assert.Equal(ok, parsed);
        if (ok)
        {
            Assert.Equal(field, actualField);
            Assert.Equal(descending, actualDescending);
        }
    }
}
