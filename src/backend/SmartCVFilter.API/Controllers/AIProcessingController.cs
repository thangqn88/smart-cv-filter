using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIProcessingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AIProcessingController> _logger;

    public AIProcessingController(ApplicationDbContext context, ILogger<AIProcessingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get AI processing status for an applicant
    /// </summary>
    [HttpGet("applicant/{applicantId}/status")]
    public async Task<ActionResult<AIProcessingStatusResponse>> GetApplicantProcessingStatus(int applicantId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var applicant = await _context.Applicants
                .Include(a => a.CVFiles)
                .Include(a => a.ScreeningResults)
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                return NotFound();

            // Check if user has access to this applicant
            if (userRole != "Admin" && applicant.JobPost.UserId != userId)
                return Forbid();

            var latestCV = applicant.CVFiles
                .OrderByDescending(cv => cv.UploadedDate)
                .FirstOrDefault();

            var latestScreening = applicant.ScreeningResults
                .OrderByDescending(sr => sr.CreatedAt)
                .FirstOrDefault();

            var status = new AIProcessingStatusResponse
            {
                ApplicantId = applicantId,
                ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                JobPostId = applicant.JobPostId,
                JobTitle = applicant.JobPost.Title,
                CVProcessingStatus = latestCV?.Status ?? "No CV uploaded",
                CVProcessingProgress = GetCVProcessingProgress(latestCV?.Status),
                ScreeningStatus = latestScreening?.Status ?? "Not started",
                ScreeningProgress = GetScreeningProgress(latestScreening?.Status),
                LastUpdated = latestScreening?.CreatedAt ?? latestCV?.UploadedDate ?? applicant.AppliedDate,
                OverallProgress = CalculateOverallProgress(latestCV?.Status, latestScreening?.Status)
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI processing status for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new { message = "An error occurred while retrieving processing status." });
        }
    }

    /// <summary>
    /// Get AI processing status for all applicants in a job post
    /// </summary>
    [HttpGet("jobpost/{jobPostId}/status")]
    public async Task<ActionResult<IEnumerable<AIProcessingStatusResponse>>> GetJobPostProcessingStatus(int jobPostId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var jobPost = await _context.JobPosts.FindAsync(jobPostId);
            if (jobPost == null)
                return NotFound();

            // Check if user has access to this job post
            if (userRole != "Admin" && jobPost.UserId != userId)
                return Forbid();

            var applicants = await _context.Applicants
                .Include(a => a.CVFiles)
                .Include(a => a.ScreeningResults)
                .Where(a => a.JobPostId == jobPostId)
                .ToListAsync();

            var statuses = applicants.Select(applicant =>
            {
                var latestCV = applicant.CVFiles
                    .OrderByDescending(cv => cv.UploadedDate)
                    .FirstOrDefault();

                var latestScreening = applicant.ScreeningResults
                    .OrderByDescending(sr => sr.CreatedAt)
                    .FirstOrDefault();

                return new AIProcessingStatusResponse
                {
                    ApplicantId = applicant.Id,
                    ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                    JobPostId = applicant.JobPostId,
                    JobTitle = jobPost.Title,
                    CVProcessingStatus = latestCV?.Status ?? "No CV uploaded",
                    CVProcessingProgress = GetCVProcessingProgress(latestCV?.Status),
                    ScreeningStatus = latestScreening?.Status ?? "Not started",
                    ScreeningProgress = GetScreeningProgress(latestScreening?.Status),
                    LastUpdated = latestScreening?.CreatedAt ?? latestCV?.UploadedDate ?? applicant.AppliedDate,
                    OverallProgress = CalculateOverallProgress(latestCV?.Status, latestScreening?.Status)
                };
            }).ToList();

            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI processing status for job post {JobPostId}", jobPostId);
            return StatusCode(500, new { message = "An error occurred while retrieving processing status." });
        }
    }

    private static int GetCVProcessingProgress(string? status)
    {
        return status switch
        {
            "Uploaded" => 25,
            "Processing" => 50,
            "Processed" => 100,
            "Error" => 0,
            _ => 0
        };
    }

    private static int GetScreeningProgress(string? status)
    {
        return status switch
        {
            "Processing" => 50,
            "Completed" => 100,
            "Failed" => 0,
            _ => 0
        };
    }

    private static int CalculateOverallProgress(string? cvStatus, string? screeningStatus)
    {
        var cvProgress = GetCVProcessingProgress(cvStatus);
        var screeningProgress = GetScreeningProgress(screeningStatus);

        // If CV is not processed, overall progress is just CV progress
        if (cvStatus != "Processed")
            return cvProgress;

        // If CV is processed but screening hasn't started, progress is 50%
        if (string.IsNullOrEmpty(screeningStatus))
            return 50;

        // If screening is in progress or completed, calculate average
        return (cvProgress + screeningProgress) / 2;
    }
}

public class AIProcessingStatusResponse
{
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CVProcessingStatus { get; set; } = string.Empty;
    public int CVProcessingProgress { get; set; }
    public string ScreeningStatus { get; set; } = string.Empty;
    public int ScreeningProgress { get; set; }
    public DateTime LastUpdated { get; set; }
    public int OverallProgress { get; set; }
}
