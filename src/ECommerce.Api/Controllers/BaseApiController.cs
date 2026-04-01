using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleSuccess<T>(T data) => Ok(data);
    protected IActionResult HandleCreated<T>(T data, string? location = null)
    {
        if (!string.IsNullOrEmpty(location))
            return Created(location, data);
        return Created($"/{typeof(T).Name.ToLower()}", data);
    }
    protected IActionResult HandleNoContent() => NoContent();
    protected IActionResult HandleNotFound(string message = "Resource not found") => 
        NotFound(new { message });
    protected IActionResult HandleBadRequest(string message = "Bad request") => 
        BadRequest(new { message });
    protected IActionResult HandleUnauthorized(string message = "Unauthorized") => 
        Unauthorized(new { message });
    protected IActionResult HandleForbidden(string message = "Forbidden") => 
        StatusCode(403, new { message });
}
