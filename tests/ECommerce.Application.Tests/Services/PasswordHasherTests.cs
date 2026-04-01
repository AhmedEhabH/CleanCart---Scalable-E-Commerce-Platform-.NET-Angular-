using ECommerce.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace ECommerce.Application.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut;

    public PasswordHasherTests()
    {
        _sut = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        var password = "TestPassword123!";

        var result = _sut.HashPassword(password);

        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(password);
        result.Should().StartWith("$2"); // BCrypt hash prefix
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        var password = "TestPassword123!";

        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        hash1.Should().NotBe(hash2); // BCrypt generates unique salts
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        var password = "TestPassword123!";
        var hashedPassword = _sut.HashPassword(password);

        var result = _sut.VerifyPassword(password, hashedPassword);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedPassword = _sut.HashPassword(password);

        var result = _sut.VerifyPassword(wrongPassword, hashedPassword);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
    {
        var password = "TestPassword123!";
        var invalidHash = "invalidhash";

        var result = _sut.VerifyPassword(password, invalidHash);

        result.Should().BeFalse();
    }
}
