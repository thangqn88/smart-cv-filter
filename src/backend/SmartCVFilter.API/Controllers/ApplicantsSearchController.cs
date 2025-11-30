using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicantsSearchController : ControllerBase
{
    private readonly IApplicantService _applicantService;
    private readonly ILogger<ApplicantsSearchController> _logger;

    public ApplicantsSearchController(IApplicantService applicantService, ILogger<ApplicantsSearchController> logger)
    {
        _applicantService = applicantService;
        _logger = logger;
    }

    /// <summary>
    /// Search existing applicants by name or email
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicantResponse>>> Search([FromQuery] string search)
    {
        var searchTerm = search ?? string.Empty;
        _logger.LogInformation("Search applicants request received. SearchTerm: {SearchTerm}, Length: {Length}",
            searchTerm, searchTerm.Length);

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            _logger.LogInformation("User authentication check - UserId: {UserId}, Role: {Role}, IsAuthenticated: {IsAuthenticated}",
                userId ?? "null", userRole ?? "null", !string.IsNullOrEmpty(userId));

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized search attempt. UserId is null or empty.");
                return Unauthorized(new { message = "Authentication required to search applicants." });
            }

            _logger.LogInformation("Starting applicant search. SearchTerm: '{SearchTerm}', UserId: {UserId}, IsAdmin: {IsAdmin}",
                searchTerm, userId, userRole == "Admin");

            var applicants = await _applicantService.SearchApplicantsAsync(searchTerm, userId, userRole == "Admin");

            var resultCount = applicants?.Count() ?? 0;
            _logger.LogInformation("Applicant search completed successfully. SearchTerm: '{SearchTerm}', ResultsFound: {ResultCount}",
                searchTerm, resultCount);

            return Ok(applicants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error searching applicants. SearchTerm: '{SearchTerm}', ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                searchTerm,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);

            // Return detailed error information for debugging
            return StatusCode(500, new
            {
                message = "An error occurred while searching applicants.",
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
}

