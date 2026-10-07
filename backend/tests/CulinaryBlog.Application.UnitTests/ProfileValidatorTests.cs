using CulinaryBlog.Application.Features.Auth;

namespace CulinaryBlog.Application.UnitTests;

public sealed class ProfileValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator = new();

    [Theory]
    [InlineData(2, 1000, true)]
    [InlineData(100, 1000, true)]
    [InlineData(101, 1000, false)]
    [InlineData(2, 1001, false)]
    public void ProfileEnforcesLengthBounds(int nameLength, int bioLength, bool valid)
    {
        var update = new ProfileUpdate(new string('a', nameLength), true, null, false, new string('b', bioLength), true);
        Assert.Equal(valid, _validator.Validate(new UpdateProfileCommand(update, "http://localhost:9000/culinary-blog")).IsValid);
    }

    [Theory]
    [InlineData("http://localhost:9000/culinary-blog/uploads/a.png", true)]
    [InlineData("http://LOCALHOST:9000/culinary-blog/uploads/a.png", true)]
    [InlineData("http://localhost:9000/culinary-blog/../other/a.png", false)]
    [InlineData("http://localhost:9000/culinary-blog/%2e%2e%2fother/a.png", false)]
    [InlineData("http://localhost:9000/culinary-blog/", false)]
    [InlineData("http://localhost:9001/culinary-blog/a.png", false)]
    [InlineData("http://user@localhost:9000/culinary-blog/a.png", false)]
    [InlineData("https://localhost:9000/culinary-blog/a.png", false)]
    [InlineData(null, true)]
    public void AvatarMustResolveToAnObjectInConfiguredBucket(string? url, bool valid)
    {
        var update = new ProfileUpdate(null, false, url, true, null, false);
        Assert.Equal(valid, _validator.Validate(new UpdateProfileCommand(update, "http://localhost:9000/culinary-blog")).IsValid);
    }

    [Fact]
    public void EmptyPatchIsValid()
    {
        var update = new ProfileUpdate(null, false, null, false, null, false);
        Assert.True(_validator.Validate(new UpdateProfileCommand(update, "http://localhost:9000/culinary-blog")).IsValid);
    }
}
