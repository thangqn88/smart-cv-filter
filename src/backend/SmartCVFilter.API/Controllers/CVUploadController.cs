using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/applicants/{applicantId}/[controller]")]
[Authorize]
public class CVUploadController : ControllerBase
{
    private readonly ICVUploadService _cvUploadService;
    private readonly IScreeningService _screeningService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CVUploadController> _logger;

    public CVUploadController(ICVUploadService cvUploadService, IScreeningService screeningService, ApplicationDbContext context, ILogger<CVUploadController> logger)
    {
        _cvUploadService = cvUploadService;
        _screeningService = screeningService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadCV(int applicantId, IFormFile file)
    {
        _logger.LogInformation("UploadCV endpoint called. ApplicantId: {ApplicantId}, FileName: {FileName}, FileSize: {FileSize}, ContentType: {ContentType}",
            applicantId, file?.FileName, file?.Length, file?.ContentType);

        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("UploadCV: File is null or empty. ApplicantId: {ApplicantId}", applicantId);
                return BadRequest(new { message = "No file uploaded." });
            }

            _logger.LogDebug("UploadCV: Validating file. ApplicantId: {ApplicantId}, FileName: {FileName}", applicantId, file.FileName);
            if (!_cvUploadService.ValidateCVFile(file))
            {
                _logger.LogWarning("UploadCV: File validation failed. ApplicantId: {ApplicantId}, FileName: {FileName}, Size: {Size}, ContentType: {ContentType}",
                    applicantId, file.FileName, file.Length, file.ContentType);
                return BadRequest(new { message = "Invalid file format or size. Please upload PDF, DOC, DOCX, or TXT files (max 10MB)." });
            }

            _logger.LogInformation("UploadCV: File validation passed. Starting upload. ApplicantId: {ApplicantId}", applicantId);
            var startTime = DateTime.UtcNow;
            var filePath = await _cvUploadService.UploadCVAsync(file, applicantId);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("UploadCV: Upload completed successfully. ApplicantId: {ApplicantId}, FilePath: {FilePath}, Duration: {Duration}ms",
                applicantId, filePath, duration);
            return Ok(new { message = "File uploaded successfully.", filePath });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "InvalidOperationException in UploadCV. ApplicantId: {ApplicantId}, Message: {Message}",
                applicantId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception in UploadCV. ApplicantId: {ApplicantId}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                applicantId, ex.GetType().Name, ex.Message, ex.StackTrace);
            return StatusCode(500, new { message = "An error occurred while uploading the file.", error = ex.Message });
        }
    }

    [HttpGet("{cvFileId}/download")]
    public async Task<ActionResult> DownloadCV(int applicantId, int cvFileId)
    {
        try
        {
            var fileBytes = await _cvUploadService.GetCVFileAsync(cvFileId);
            return File(fileBytes, "application/octet-stream");
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "CV file not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            return StatusCode(500, new { message = "An error occurred while downloading the file." });
        }
    }

    [HttpDelete("{cvFileId}")]
    public async Task<ActionResult> DeleteCV(int applicantId, int cvFileId)
    {
        try
        {
            var result = await _cvUploadService.DeleteCVAsync(cvFileId);
            if (!result)
                return NotFound();

            return Ok(new { message = "CV file deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            return StatusCode(500, new { message = "An error occurred while deleting the file." });
        }
    }

    [HttpPost("{cvFileId}/extract-text")]
    public async Task<ActionResult> ExtractText(int applicantId, int cvFileId)
    {
        try
        {
            var extractedText = await _cvUploadService.ExtractTextFromCVAsync(cvFileId);
            return Ok(new { extractedText });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            return StatusCode(500, new { message = "An error occurred while extracting text from the file." });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetCVFileStatuses(int applicantId)
    {
        try
        {
            var statuses = await _cvUploadService.GetCVFileStatusesAsync(applicantId);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV file statuses for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new { message = "An error occurred while getting file statuses." });
        }
    }

    [HttpPost("screening")]
    public async Task<ActionResult> StartScreening(int applicantId)
    {
        try
        {
            // Get the applicant to find the job post ID
            var applicant = await _context.Applicants
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                return NotFound(new { message = "Applicant not found." });

            if (applicant.JobPost == null)
                return BadRequest(new { message = "No job post associated with this applicant." });

            // Check if there are any processed CV files
            var hasProcessedCV = await _context.CVFiles
                .AnyAsync(cf => cf.ApplicantId == applicantId && cf.Status == "Processed");

            if (!hasProcessedCV)
                return BadRequest(new { message = "No processed CV files found. Please upload and process a CV first." });

            _logger.LogInformation("Starting synchronous screening for applicant {ApplicantId}", applicantId);

            // Start the screening process and wait for completion
            var result = await _screeningService.ProcessScreeningAsync(applicantId, applicant.JobPostId);

            if (result)
            {
                _logger.LogInformation("Screening completed successfully for applicant {ApplicantId}", applicantId);
                return Ok(new
                {
                    message = "CV screening completed successfully!",
                    success = true,
                    applicantId = applicantId
                });
            }
            else
            {
                _logger.LogWarning("Screening failed for applicant {ApplicantId}", applicantId);
                return StatusCode(500, new
                {
                    message = "CV screening failed. Please try again.",
                    success = false,
                    applicantId = applicantId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during screening for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new
            {
                message = "An error occurred during CV screening. Please try again.",
                success = false,
                applicantId = applicantId
            });
        }
    }
}

