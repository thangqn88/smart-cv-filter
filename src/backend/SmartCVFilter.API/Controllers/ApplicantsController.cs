using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/jobposts/{jobPostId}/[controller]")]
[Authorize]
public class ApplicantsController : ControllerBase
{
    private readonly IApplicantService _applicantService;
    private readonly ILogger<ApplicantsController> _logger;

    public ApplicantsController(IApplicantService applicantService, ILogger<ApplicantsController> logger)
    {
        _applicantService = applicantService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicantResponse>>> GetApplicants(int jobPostId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Admin users can access all applicants, regular users can only access their own job post's applicants
            var applicants = await _applicantService.GetApplicantsByJobPostAsync(jobPostId, userId, userRole == "Admin");
            return Ok(applicants);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicants for job post {JobPostId}", jobPostId);
            return StatusCode(500, new { message = "An error occurred while retrieving applicants." });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicantResponse>> GetApplicant(int jobPostId, int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var applicant = await _applicantService.GetApplicantByIdAsync(id, userId, userRole == "Admin");
            if (applicant == null || applicant.JobPostId != jobPostId)
                return NotFound();

            return Ok(applicant);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return StatusCode(500, new { message = "An error occurred while retrieving the applicant." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApplicantResponse>> CreateApplicant(int jobPostId, CreateApplicantRequest request)
    {
        try
        {
            var applicant = await _applicantService.CreateApplicantAsync(request, jobPostId);
            return CreatedAtAction(nameof(GetApplicant), new { jobPostId, id = applicant.Id }, applicant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating applicant for job post {JobPostId}", jobPostId);
            return StatusCode(500, new { message = "An error occurred while creating the applicant." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicantResponse>> UpdateApplicant(int jobPostId, int id, UpdateApplicantRequest request)
    {
        try
        {
            var applicant = await _applicantService.UpdateApplicantAsync(id, request);
            if (applicant.JobPostId != jobPostId)
                return NotFound();

            return Ok(applicant);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return StatusCode(500, new { message = "An error occurred while updating the applicant." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteApplicant(int jobPostId, int id)
    {
        try
        {
            var result = await _applicantService.DeleteApplicantAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return StatusCode(500, new { message = "An error occurred while deleting the applicant." });
        }
    }

    [HttpPost("screen")]
    public async Task<ActionResult> StartScreening(int jobPostId, ScreeningRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _applicantService.StartScreeningAsync(jobPostId, request, userId, userRole == "Admin");
            if (!result)
                return BadRequest(new { message = "Failed to start screening process." });

            return Ok(new { message = "Screening process started successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screening for job post {JobPostId}", jobPostId);
            return StatusCode(500, new { message = "An error occurred while starting the screening process." });
        }
    }
}

