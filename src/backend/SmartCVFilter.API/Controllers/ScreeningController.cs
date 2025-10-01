using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScreeningController : ControllerBase
{
    private readonly IScreeningService _screeningService;
    private readonly ILogger<ScreeningController> _logger;

    public ScreeningController(IScreeningService screeningService, ILogger<ScreeningController> logger)
    {
        _screeningService = screeningService;
        _logger = logger;
    }

    [HttpGet("results/{resultId}")]
    public async Task<ActionResult<ScreeningResultResponse>> GetScreeningResult(int resultId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _screeningService.GetScreeningResultAsync(resultId, userId, userRole == "Admin");
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening result {ResultId}", resultId);
            return StatusCode(500, new { message = "An error occurred while retrieving the screening result." });
        }
    }

    [HttpGet("applicants/{applicantId}/results")]
    public async Task<ActionResult<IEnumerable<ScreeningResultResponse>>> GetScreeningResultsByApplicant(int applicantId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var results = await _screeningService.GetScreeningResultsByApplicantAsync(applicantId, userId, userRole == "Admin");
            return Ok(results);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening results for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new { message = "An error occurred while retrieving screening results." });
        }
    }

    [HttpGet("screened-applicants")]
    public async Task<ActionResult<IEnumerable<ScreenedApplicantResponse>>> GetScreenedApplicants()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var applicants = await _screeningService.GetScreenedApplicantsAsync(userId, userRole == "Admin");
            return Ok(applicants);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screened applicants");
            return StatusCode(500, new { message = "An error occurred while retrieving screened applicants." });
        }
    }
}

