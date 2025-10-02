using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.Web.Configuration;
using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Controllers;

/// <summary>
/// Base controller with common paging functionality
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly ILogger Logger;
    protected readonly PaginationSettings PaginationSettings;

    protected BaseController(ILogger logger, IOptions<PaginationSettings> paginationSettings)
    {
        Logger = logger;
        PaginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Creates pagination info from a paged response
    /// </summary>
    protected PaginationInfo CreatePaginationInfo<T>(PagedResponse<T> response)
    {
        var startItem = ((response.Page - 1) * response.PageSize) + 1;
        var endItem = Math.Min(response.Page * response.PageSize, response.TotalItems);

        return new PaginationInfo
        {
            CurrentPage = response.Page,
            TotalPages = response.TotalPages,
            PageSize = response.PageSize,
            TotalItems = response.TotalItems,
            HasPreviousPage = response.HasPreviousPage,
            HasNextPage = response.HasNextPage,
            PreviousPage = response.PreviousPage,
            NextPage = response.NextPage,
            StartItem = startItem,
            EndItem = endItem
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
    /// Handles common exceptions and returns appropriate responses
    /// </summary>
    protected IActionResult HandleException(Exception ex, string operation, int? id = null)
    {
        Logger.LogError(ex, "Error during {Operation} {Id}", operation, id);

        TempData["Error"] = $"An error occurred during {operation.ToLower()}.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Creates a standardized error response for AJAX calls
    /// </summary>
    protected JsonResult CreateErrorResponse(string message, int statusCode = 500)
    {
        Response.StatusCode = statusCode;
        return Json(new { success = false, message });
    }

    /// <summary>
    /// Creates a standardized success response for AJAX calls
    /// </summary>
    protected JsonResult CreateSuccessResponse<T>(T data, string? message = null)
    {
        return Json(new { success = true, data, message });
    }
}
