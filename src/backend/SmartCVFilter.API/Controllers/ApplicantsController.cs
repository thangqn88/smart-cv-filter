using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.API.Configuration;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/jobposts/{jobPostId}/[controller]")]
[Authorize]
public class ApplicantsController : BaseController
{
    private readonly IApplicantService _applicantService;
    private readonly ILogger<ApplicantsController> _logger;

    public ApplicantsController(IApplicantService applicantService, ILogger<ApplicantsController> logger, IOptions<PaginationSettings> paginationSettings)
        : base(logger, paginationSettings)
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

    /// <summary>
    /// Get applicants with pagination and filtering
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<ApplicantPagedResponse>> GetApplicantsPaged(int jobPostId, [FromQuery] ApplicantPagedRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAdmin = IsCurrentUserAdmin();
            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;
            request.JobPostId = jobPostId;

            var result = await _applicantService.GetApplicantsPagedAsync(request, userId, isAdmin);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting applicants", jobPostId);
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
    public async Task<ActionResult<ApplicantResponse>> CreateApplicant(int jobPostId, [FromBody] CreateApplicantRequest request)
    {
        try
        {
            _logger.LogInformation("CreateApplicant called with JobPostId: {JobPostId}", jobPostId);

            if (request == null)
            {
                _logger.LogWarning("Request body is null for job post {JobPostId}", jobPostId);
                return BadRequest(new { message = "Request body is required." });
            }

            _logger.LogInformation("Request data - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}",
                request.FirstName, request.LastName, request.Email);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                _logger.LogWarning("FirstName is null or empty for job post {JobPostId}", jobPostId);
                return BadRequest(new { message = "First name is required." });
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                _logger.LogWarning("LastName is null or empty for job post {JobPostId}", jobPostId);
                return BadRequest(new { message = "Last name is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Email is null or empty for job post {JobPostId}", jobPostId);
                return BadRequest(new { message = "Email is required." });
            }

            if (!(new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email)))
            {
                _logger.LogWarning("Invalid email format: {Email} for job post {JobPostId}", request.Email, jobPostId);
                return BadRequest(new { message = "Please enter a valid email address." });
            }

            var applicant = await _applicantService.CreateApplicantAsync(request, jobPostId);
            _logger.LogInformation("Successfully created applicant {ApplicantId} for job post {JobPostId}", applicant.Id, jobPostId);
            return CreatedAtAction(nameof(GetApplicant), new { jobPostId, id = applicant.Id }, applicant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating applicant for job post {JobPostId}", jobPostId);
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
            _logger.LogInformation("UpdateApplicant called with JobPostId: {JobPostId}, Id: {Id}", jobPostId, id);
            _logger.LogInformation("Request data - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}",
                request.FirstName, request.LastName, request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

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

    [HttpGet("create-example")]
    public ActionResult GetCreateExample(int jobPostId)
    {
        var example = new CreateApplicantRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            LinkedInProfile = "https://linkedin.com/in/johndoe",
            PortfolioUrl = "https://johndoe.dev",
            CoverLetter = "I am interested in this position..."
        };

        return Ok(new
        {
            message = "Example request body for creating an applicant",
            jobPostId = jobPostId,
            example = example,
            requiredFields = new[] { "FirstName", "LastName", "Email" },
            optionalFields = new[] { "PhoneNumber", "LinkedInProfile", "PortfolioUrl", "CoverLetter" }
        });
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

