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
            var result = await _screeningService.GetScreeningResultAsync(resultId);
            if (result == null)
                return NotFound();

            return Ok(result);
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
            var results = await _screeningService.GetScreeningResultsByApplicantAsync(applicantId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening results for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new { message = "An error occurred while retrieving screening results." });
        }
    }
}

