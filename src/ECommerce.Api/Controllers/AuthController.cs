using ECommerce.Api.Controllers;
using ECommerce.Application.Auth.DTOs;
using ECommerce.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return HandleSuccess(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleUnauthorized(ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return HandleCreated(result);
        }
        catch (InvalidOperationException ex)
        {
            return HandleBadRequest(ex.Message);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return HandleSuccess(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleUnauthorized(ex.Message);
        }
    }
}
