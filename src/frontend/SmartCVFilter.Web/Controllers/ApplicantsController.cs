using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;
using SmartCVFilter.Web.ViewModels;

namespace SmartCVFilter.Web.Controllers;

public class ApplicantsController : Controller
{
    private readonly IApplicantService _applicantService;
    private readonly IJobPostService _jobPostService;
    private readonly ICVUploadService _cvUploadService;
    private readonly ILogger<ApplicantsController> _logger;

    public ApplicantsController(
        IApplicantService applicantService,
        IJobPostService jobPostService,
        ICVUploadService cvUploadService,
        ILogger<ApplicantsController> logger)
    {
        _applicantService = applicantService;
        _jobPostService = jobPostService;
        _cvUploadService = cvUploadService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? jobPostId)
    {
        try
        {
            ViewData["Title"] = "Applicant Management";

            List<ApplicantResponse> applicants = new();
            List<JobPostListResponse> jobPosts = new();

            if (jobPostId.HasValue)
            {
                applicants = await _applicantService.GetApplicantsAsync(jobPostId.Value);
                var jobPost = await _jobPostService.GetJobPostAsync(jobPostId.Value);
                if (jobPost != null)
                {
                    ViewData["JobPostTitle"] = jobPost.Title;
                    ViewData["JobPostId"] = jobPostId.Value;
                }
            }
            else
            {
                jobPosts = await _jobPostService.GetAllJobPostsAsync();
            }

            var viewModel = new ApplicantIndexViewModel
            {
                Applicants = applicants,
                JobPosts = jobPosts,
                SelectedJobPostId = jobPostId
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading applicants");
            TempData["Error"] = "An error occurred while loading applicants.";
            return View(new ApplicantIndexViewModel
            {
                Applicants = new List<ApplicantResponse>(),
                JobPosts = new List<JobPostListResponse>(),
                SelectedJobPostId = null
            });
        }
    }

    public async Task<IActionResult> Details(int jobPostId, int id)
    {
        try
        {
            var applicant = await _applicantService.GetApplicantAsync(jobPostId, id);
            if (applicant == null)
            {
                TempData["Error"] = "Applicant not found.";
                return RedirectToAction("Index", new { jobPostId });
            }

            ViewData["Title"] = $"Applicant Details - {applicant.FirstName} {applicant.LastName}";
            return View(applicant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading applicant details for ID {ApplicantId}", id);
            TempData["Error"] = "An error occurred while loading applicant details.";
            return RedirectToAction("Index", new { jobPostId });
        }
    }

    [Authorize]
    public IActionResult Create(int jobPostId)
    {
        ViewData["Title"] = "Add New Applicant";
        ViewData["JobPostId"] = jobPostId;
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int jobPostId, CreateApplicantRequest model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }

        try
        {
            var result = await _applicantService.CreateApplicantAsync(jobPostId, model);
            if (result != null)
            {
                TempData["Success"] = "Applicant added successfully!";
                return RedirectToAction("Details", new { jobPostId, id = result.Id });
            }
            else
            {
                ModelState.AddModelError("", "Failed to add applicant. Please try again.");
                ViewData["JobPostId"] = jobPostId;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating applicant for job post {JobPostId}", jobPostId);
            ModelState.AddModelError("", "An error occurred while adding the applicant. Please try again.");
            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int jobPostId, int id)
    {
        try
        {
            var applicant = await _applicantService.GetApplicantAsync(jobPostId, id);
            if (applicant == null)
            {
                TempData["Error"] = "Applicant not found.";
                return RedirectToAction("Index", new { jobPostId });
            }

            ViewData["Title"] = $"Edit Applicant - {applicant.FirstName} {applicant.LastName}";
            ViewData["JobPostId"] = jobPostId;
            ViewData["ApplicantId"] = id;

            var updateModel = new UpdateApplicantRequest
            {
                FirstName = applicant.FirstName,
                LastName = applicant.LastName,
                Email = applicant.Email,
                PhoneNumber = applicant.PhoneNumber,
                LinkedInProfile = applicant.LinkedInProfile,
                PortfolioUrl = applicant.PortfolioUrl,
                CoverLetter = applicant.CoverLetter,
                Status = applicant.Status
            };

            return View(updateModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading applicant for edit, ID {ApplicantId}", id);
            TempData["Error"] = "An error occurred while loading the applicant for editing.";
            return RedirectToAction("Index", new { jobPostId });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> Edit(int jobPostId, int id, UpdateApplicantRequest model)
    {
        _logger.LogInformation("Edit POST called with JobPostId: {JobPostId}, Id: {Id}", jobPostId, id);
        _logger.LogInformation("Model is null: {IsNull}", model == null);

        if (model != null)
        {
            _logger.LogInformation("Model data - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}",
                model.FirstName, model.LastName, model.Email);
        }
        else
        {
            _logger.LogWarning("Model is null - checking form data manually");
            _logger.LogInformation("Request.Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
            }
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ViewData["JobPostId"] = jobPostId;
            ViewData["ApplicantId"] = id;
            return View(model);
        }

        try
        {
            var result = await _applicantService.UpdateApplicantAsync(jobPostId, id, model);
            if (result != null)
            {
                TempData["Success"] = "Applicant updated successfully!";
                return RedirectToAction("Details", new { jobPostId, id = result.Id });
            }
            else
            {
                ModelState.AddModelError("", "Failed to update applicant. Please try again.");
                ViewData["JobPostId"] = jobPostId;
                ViewData["ApplicantId"] = id;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating applicant {ApplicantId}", id);
            ModelState.AddModelError("", "An error occurred while updating the applicant. Please try again.");
            ViewData["JobPostId"] = jobPostId;
            ViewData["ApplicantId"] = id;
            return View(model);
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int jobPostId, int id)
    {
        try
        {
            var result = await _applicantService.DeleteApplicantAsync(jobPostId, id);
            if (result)
            {
                TempData["Success"] = "Applicant deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete applicant.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting applicant {ApplicantId}", id);
            TempData["Error"] = "An error occurred while deleting the applicant.";
        }

        return RedirectToAction("Index", new { jobPostId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartScreening(int jobPostId, ScreeningRequest model)
    {
        try
        {
            var result = await _applicantService.StartScreeningAsync(jobPostId, model);
            if (result)
            {
                TempData["Success"] = "AI screening process started successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to start screening process.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screening for job post {JobPostId}", jobPostId);
            TempData["Error"] = "An error occurred while starting the screening process.";
        }

        return RedirectToAction("Index", new { jobPostId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCV(int jobPostId, int id, IFormFile cvFile)
    {
        try
        {
            if (cvFile == null || cvFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Details", new { jobPostId, id });
            }

            // Validate file type and size
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
            var fileExtension = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "Invalid file format. Please upload PDF, DOC, DOCX, or TXT files.";
                return RedirectToAction("Details", new { jobPostId, id });
            }

            var maxSize = 10 * 1024 * 1024; // 10MB
            if (cvFile.Length > maxSize)
            {
                TempData["Error"] = "File size too large. Maximum size is 10MB.";
                return RedirectToAction("Details", new { jobPostId, id });
            }

            var result = await _cvUploadService.UploadCVAsync(id, cvFile);
            if (result)
            {
                TempData["Success"] = "CV uploaded successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to upload CV. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV for applicant {ApplicantId}", id);
            TempData["Error"] = "An error occurred while uploading the CV.";
        }

        return RedirectToAction("Details", new { jobPostId, id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DownloadCV(int jobPostId, int id, int cvFileId)
    {
        try
        {
            var fileBytes = await _cvUploadService.DownloadCVAsync(id, cvFileId);

            // Get the CV file info to determine the correct filename
            var applicant = await _applicantService.GetApplicantAsync(jobPostId, id);
            var cvFile = applicant?.CVFiles.FirstOrDefault(f => f.Id == cvFileId);
            var fileName = cvFile?.FileName ?? $"cv_{cvFileId}";

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading CV {CvFileId} for applicant {ApplicantId}", cvFileId, id);
            TempData["Error"] = "An error occurred while downloading the CV.";
            return RedirectToAction("Details", new { jobPostId, id });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCV(int jobPostId, int id, int cvFileId)
    {
        try
        {
            var result = await _cvUploadService.DeleteCVAsync(id, cvFileId);
            if (result)
            {
                TempData["Success"] = "CV deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete CV.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV {CvFileId} for applicant {ApplicantId}", cvFileId, id);
            TempData["Error"] = "An error occurred while deleting the CV.";
        }

        return RedirectToAction("Details", new { jobPostId, id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExtractText(int jobPostId, int id, int cvFileId)
    {
        try
        {
            var extractedText = await _cvUploadService.ExtractTextFromCVAsync(id, cvFileId);
            TempData["ExtractedText"] = extractedText;
            TempData["CvFileId"] = cvFileId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from CV {CvFileId} for applicant {ApplicantId}", cvFileId, id);
            TempData["Error"] = "An error occurred while extracting text from the CV.";
        }

        return RedirectToAction("Details", new { jobPostId, id });
    }
}
