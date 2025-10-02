using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.API.Configuration;
using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Controllers;

/// <summary>
/// Base controller with common paging functionality
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger Logger;
    protected readonly PaginationSettings PaginationSettings;

    protected BaseController(ILogger logger, IOptions<PaginationSettings> paginationSettings)
    {
        Logger = logger;
        PaginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Creates a paginated response with common properties
    /// </summary>
    protected PagedResponse<T> CreatePagedResponse<T>(
        List<T> items,
        int totalItems,
        int page,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PagedResponse<T>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Validates pagination parameters using configuration settings
    /// </summary>
    protected (int page, int pageSize) ValidatePaginationParameters(int page, int pageSize)
    {
        // Ensure page is at least 1
        page = Math.Max(1, page);

        // Ensure page size is within configured limits
        pageSize = Math.Max(PaginationSettings.MinPageSize, Math.Min(PaginationSettings.MaxPageSize, pageSize));

        return (page, pageSize);
    }

    /// <summary>
    /// Gets the default page size from configuration
    /// </summary>
    protected int GetDefaultPageSize()
    {
        return PaginationSettings.DefaultPageSize;
    }

    /// <summary>
    /// Gets available page size options from configuration
    /// </summary>
    protected int[] GetPageSizeOptions()
    {
        return PaginationSettings.PageSizeOptions;
    }

    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    protected string? GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the current user role from claims
    /// </summary>
    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Checks if the current user is an admin
    /// </summary>
    protected bool IsCurrentUserAdmin()
    {
        return GetCurrentUserRole() == "Admin";
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    protected ActionResult CreateErrorResponse(string message, int statusCode = 500)
    {
        return StatusCode(statusCode, new { message });
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    protected ActionResult CreateSuccessResponse<T>(T data, string? message = null)
    {
        var response = new { data, message };
        return Ok(response);
    }

    /// <summary>
    /// Handles common exceptions and returns appropriate responses
    /// </summary>
    protected ActionResult HandleException(Exception ex, string operation, int? id = null)
    {
        Logger.LogError(ex, "Error during {Operation} {Id}", operation, id);

        return ex switch
        {
            UnauthorizedAccessException => Unauthorized(new { message = ex.Message }),
            InvalidOperationException => BadRequest(new { message = ex.Message }),
            KeyNotFoundException => NotFound(new { message = ex.Message }),
            _ => CreateErrorResponse($"An error occurred during {operation.ToLower()}")
        };
    }
}
