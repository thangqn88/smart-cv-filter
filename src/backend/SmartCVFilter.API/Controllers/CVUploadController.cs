using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/applicants/{applicantId}/[controller]")]
[Authorize]
public class CVUploadController : ControllerBase
{
    private readonly ICVUploadService _cvUploadService;
    private readonly ILogger<CVUploadController> _logger;

    public CVUploadController(ICVUploadService cvUploadService, ILogger<CVUploadController> logger)
    {
        _cvUploadService = cvUploadService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult> UploadCV(int applicantId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            if (!_cvUploadService.ValidateCVFile(file))
                return BadRequest(new { message = "Invalid file format or size. Please upload PDF, DOC, DOCX, or TXT files (max 10MB)." });

            var filePath = await _cvUploadService.UploadCVAsync(file, applicantId);
            return Ok(new { message = "File uploaded successfully.", filePath });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV for applicant {ApplicantId}", applicantId);
            return StatusCode(500, new { message = "An error occurred while uploading the file." });
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
}

