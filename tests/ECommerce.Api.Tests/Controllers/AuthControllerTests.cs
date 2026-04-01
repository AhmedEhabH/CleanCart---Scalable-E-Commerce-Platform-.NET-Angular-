using ECommerce.Api.Controllers;
using ECommerce.Api.Models;
using ECommerce.Application.Auth.DTOs;
using ECommerce.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    #region Login Tests

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        var request = new LoginRequest("user@example.com", "Password123!");
        var authResponse = CreateAuthResponse();

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var request = new LoginRequest("user@example.com", "WrongPassword");

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password"));

        var result = await _controller.Login(request);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password"));

        var result = await _controller.Login(request);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenRegistrationIsSuccessful()
    {
        var request = new RegisterRequest("newuser@example.com", "Password123!", "John", "Doe", "+1234567890");
        var authResponse = CreateAuthResponse();

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Register(request);

        var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var apiResponse = createdResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserAlreadyExists()
    {
        var request = new RegisterRequest("existing@example.com", "Password123!", "John", "Doe");

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists"));

        var result = await _controller.Register(request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenTokenIsValid()
    {
        var request = new RefreshTokenRequest("valid-refresh-token");
        var authResponse = CreateAuthResponse();

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(request.RefreshToken))
            .ReturnsAsync(authResponse);

        var result = await _controller.RefreshToken(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsInvalid()
    {
        var request = new RefreshTokenRequest("invalid-refresh-token");

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(request.RefreshToken))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));

        var result = await _controller.RefreshToken(request);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        var request = new RefreshTokenRequest("expired-refresh-token");

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(request.RefreshToken))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));

        var result = await _controller.RefreshToken(request);

        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    private static AuthResponse CreateAuthResponse()
    {
        return new AuthResponse(
            AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test-token",
            RefreshToken: "refresh-token-value",
            ExpiresAt: DateTime.UtcNow.AddMinutes(60),
            Email: "user@example.com",
            FullName: "John Doe",
            Role: "User"
        );
    }
}
