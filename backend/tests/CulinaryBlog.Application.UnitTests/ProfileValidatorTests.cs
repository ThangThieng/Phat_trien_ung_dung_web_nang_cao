using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Auth;
using NSubstitute;

namespace CulinaryBlog.Application.UnitTests;

public sealed class ProfileValidatorTests
{
    private readonly IFileStorageService _storage = Substitute.For<IFileStorageService>();

    [Theory]
    [InlineData(2, 1000, true)]
    [InlineData(100, 1000, true)]
    [InlineData(101, 1000, false)]
    [InlineData(2, 1001, false)]
    public void ProfileEnforcesLengthBounds(int nameLength, int bioLength, bool valid)
    {
        var validator = new UpdateProfileCommandValidator(_storage);
        Assert.Equal(valid, validator.Validate(new UpdateProfileCommand(new string('a', nameLength), null, new string('b', bioLength))).IsValid);
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
        if (url is not null)
        {
            _storage.IsStoredFileUrl(url).Returns(valid);
        }

        var validator = new UpdateProfileCommandValidator(_storage);
        Assert.Equal(valid, validator.Validate(new UpdateProfileCommand(null, url, null)).IsValid);
    }

    [Fact]
    public void EmptyPatchIsValid()
    {
        var validator = new UpdateProfileCommandValidator(_storage);
        Assert.True(validator.Validate(new UpdateProfileCommand(null, null, null)).IsValid);
    }
}
