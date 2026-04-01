using ECommerce.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleSuccess<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.SuccessResponse(data, message));

    protected IActionResult HandleCreated<T>(T data, string? message = null)
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        return Created($"/{typeof(T).Name.ToLower()}", response);
    }

    protected IActionResult HandleNoContent(string? message = null) =>
        Ok(ApiResponse<object>.SuccessResponse(message ?? "Operation completed successfully"));

    protected IActionResult HandleNotFound(string message = "Resource not found") =>
        NotFound(ApiResponse<object>.ErrorResponse(message));

    protected IActionResult HandleBadRequest(string message = "Bad request", Dictionary<string, string[]>? errors = null) =>
        BadRequest(ApiResponse<object>.ErrorResponse(message, errors));

    protected IActionResult HandleUnauthorized(string message = "Unauthorized") =>
        Unauthorized(ApiResponse<object>.ErrorResponse(message));

    protected IActionResult HandleForbidden(string message = "Forbidden") =>
        StatusCode(403, ApiResponse<object>.ErrorResponse(message));
}
