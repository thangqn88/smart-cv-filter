using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.Web.Configuration;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;
using SmartCVFilter.Web.ViewModels;

namespace SmartCVFilter.Web.Controllers;

public class ApplicantsController : BaseController
{
    private readonly IApplicantService _applicantService;
    private readonly IJobPostService _jobPostService;
    private readonly ICVUploadService _cvUploadService;
    private readonly ILogger<ApplicantsController> _logger;

    public ApplicantsController(
        IApplicantService applicantService,
        IJobPostService jobPostService,
        ICVUploadService cvUploadService,
        ILogger<ApplicantsController> logger,
        IOptions<PaginationSettings> paginationSettings)
        : base(logger, paginationSettings)
    {
        _applicantService = applicantService;
        _jobPostService = jobPostService;
        _cvUploadService = cvUploadService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? jobPostId, ApplicantPagedRequest request)
    {
        if (jobPostId.HasValue)
        {
            // Set defaults for the request
            request.JobPostId = jobPostId.Value;
            request.SetDefaults(GetDefaultPageSize());
            if (string.IsNullOrEmpty(request.SortBy)) request.SortBy = "applieddate";
            if (string.IsNullOrEmpty(request.SortDirection)) request.SortDirection = "desc";

            // Use the same logic as Paged action
            try
            {
                ViewData["Title"] = "Applicant Management";

                // Set defaults and validate pagination parameters
                request.SetDefaults(GetDefaultPageSize());
                var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
                request.Page = page;
                request.PageSize = pageSize;
                request.JobPostId = jobPostId.Value;

                // Get job post info
                var jobPost = await _jobPostService.GetJobPostAsync(jobPostId.Value);
                if (jobPost == null)
                {
                    TempData["Error"] = "Job post not found.";
                    return RedirectToAction("Index");
                }

                // Get paged data
                var response = await _applicantService.GetApplicantsPagedAsync(request);
                if (response == null)
                {
                    TempData["Error"] = "An error occurred while loading applicants.";
                    return View(new ApplicantPagedViewModel());
                }

                // Create view model
                var viewModel = new ApplicantPagedViewModel
                {
                    Data = response,
                    Pagination = CreatePaginationInfo(response),
                    Filters = request,
                    Statuses = GetApplicantStatuses(),
                    JobPostTitle = jobPost.Title,
                    JobPostId = jobPostId.Value
                };

                // Debug information
                _logger.LogInformation("Applicants pagination - Page: {Page}, PageSize: {PageSize}, TotalItems: {TotalItems}, TotalPages: {TotalPages}",
                    response.Page, response.PageSize, response.TotalItems, response.TotalPages);

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "loading applicants", jobPostId);
            }
        }
        else
        {
            // Show job post selection (existing logic)
            try
            {
                ViewData["Title"] = "Applicant Management";
                var jobPosts = await _jobPostService.GetAllJobPostsAsync();

                var viewModel = new ApplicantIndexViewModel
                {
                    Applicants = new List<ApplicantResponse>(),
                    JobPosts = jobPosts,
                    SelectedJobPostId = null
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job posts for applicant selection");
                TempData["Error"] = "An error occurred while loading job posts.";
                return View(new ApplicantIndexViewModel
                {
                    Applicants = new List<ApplicantResponse>(),
                    JobPosts = new List<JobPostListResponse>(),
                    SelectedJobPostId = null
                });
            }
        }
    }

    /// <summary>
    /// Paged applicants with filtering and sorting
    /// </summary>
    public async Task<IActionResult> Paged(int jobPostId, ApplicantPagedRequest request)
    {
        try
        {
            ViewData["Title"] = "Applicant Management";

            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;
            request.JobPostId = jobPostId;

            // Get job post info
            var jobPost = await _jobPostService.GetJobPostAsync(jobPostId);
            if (jobPost == null)
            {
                TempData["Error"] = "Job post not found.";
                return RedirectToAction("Index");
            }

            // Get paged data
            var response = await _applicantService.GetApplicantsPagedAsync(request);
            if (response == null)
            {
                TempData["Error"] = "An error occurred while loading applicants.";
                return View(new ApplicantPagedViewModel());
            }

            // Create view model
            var viewModel = new ApplicantPagedViewModel
            {
                Data = response,
                Pagination = CreatePaginationInfo(response),
                Filters = request,
                Statuses = GetApplicantStatuses(),
                JobPostTitle = jobPost.Title,
                JobPostId = jobPostId
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "loading applicants", jobPostId);
        }
    }

    /// <summary>
    /// AJAX endpoint for paged applicants
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(int jobPostId, ApplicantPagedRequest request)
    {
        try
        {
            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;
            request.JobPostId = jobPostId;

            // Get paged data
            var response = await _applicantService.GetApplicantsPagedAsync(request);
            if (response == null)
            {
                return CreateErrorResponse("An error occurred while loading applicants.");
            }

            return CreateSuccessResponse(response);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse($"An error occurred while loading applicants: {ex.Message}");
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
        _logger.LogInformation("Create GET called with JobPostId: {JobPostId}", jobPostId);
        ViewData["Title"] = "Add New Applicant";
        ViewData["JobPostId"] = jobPostId;

        // Initialize the model to ensure proper binding
        var model = new CreateApplicantRequest();
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult TestCreate(int jobPostId)
    {
        _logger.LogInformation("TestCreate GET called with JobPostId: {JobPostId}", jobPostId);
        ViewData["Title"] = "Test Create Applicant";
        ViewData["JobPostId"] = jobPostId;

        // Initialize the model
        var model = new CreateApplicantRequest();
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult DebugCreate(int jobPostId)
    {
        _logger.LogInformation("DebugCreate GET called with JobPostId: {JobPostId}", jobPostId);
        ViewData["Title"] = "Debug Create Applicant";
        ViewData["JobPostId"] = jobPostId;

        // Initialize the model
        var model = new CreateApplicantRequest();
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult TestCreate(int jobPostId, CreateApplicantRequest model)
    {
        _logger.LogInformation("TestCreate POST called with JobPostId: {JobPostId}", jobPostId);
        _logger.LogInformation("Model is null: {IsNull}", model == null);

        // Log all form data
        _logger.LogInformation("Request.Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
        foreach (var key in Request.Form.Keys)
        {
            _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
        }

        if (model != null)
        {
            _logger.LogInformation("Model data - FirstName: '{FirstName}', LastName: '{LastName}', Email: '{Email}'",
                model.FirstName, model.LastName, model.Email);
        }

        return Json(new
        {
            success = true,
            message = "Test form submitted successfully",
            modelData = model,
            formData = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString())
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult DebugCreate(int jobPostId, CreateApplicantRequest model)
    {
        _logger.LogInformation("DebugCreate POST called with JobPostId: {JobPostId}", jobPostId);
        _logger.LogInformation("Model is null: {IsNull}", model == null);

        // Log all form data
        _logger.LogInformation("Request.Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
        foreach (var key in Request.Form.Keys)
        {
            _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
        }

        // Log request details
        _logger.LogInformation("Request.ContentType: {ContentType}", Request.ContentType);
        _logger.LogInformation("Request.Method: {Method}", Request.Method);
        _logger.LogInformation("Request.HasFormContentType: {HasFormContentType}", Request.HasFormContentType);
        _logger.LogInformation("Request.ContentLength: {ContentLength}", Request.ContentLength);

        if (model != null)
        {
            _logger.LogInformation("Model data - FirstName: '{FirstName}', LastName: '{LastName}', Email: '{Email}'",
                model.FirstName, model.LastName, model.Email);
        }

        return Json(new
        {
            success = true,
            message = "Debug form submitted successfully",
            modelData = model,
            formData = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()),
            requestDetails = new
            {
                contentType = Request.ContentType,
                method = Request.Method,
                hasFormContentType = Request.HasFormContentType,
                contentLength = Request.ContentLength
            }
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int jobPostId, CreateApplicantRequest model)
    {
        _logger.LogInformation("Create POST called with JobPostId: {JobPostId}", jobPostId);
        _logger.LogInformation("Model is null: {IsNull}", model == null);

        // Log all form data
        _logger.LogInformation("Request.Form keys: {Keys}", string.Join(", ", Request.Form.Keys));
        foreach (var key in Request.Form.Keys)
        {
            _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
        }

        // Log request details
        _logger.LogInformation("Request.ContentType: {ContentType}", Request.ContentType);
        _logger.LogInformation("Request.Method: {Method}", Request.Method);
        _logger.LogInformation("Request.HasFormContentType: {HasFormContentType}", Request.HasFormContentType);
        _logger.LogInformation("Request.ContentLength: {ContentLength}", Request.ContentLength);

        // Log all headers
        _logger.LogInformation("Request Headers:");
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("  {Key}: {Value}", header.Key, header.Value.ToString());
        }

        // Log the raw request body if possible
        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation("Raw request body: {Body}", body);
            Request.Body.Position = 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read request body");
        }

        // Always create model from form data to ensure binding works
        if (model == null)
        {
            _logger.LogWarning("Model is null - creating new instance");
            model = new CreateApplicantRequest();
        }

        // Check if model binding worked properly, if not, manually bind from form data
        if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName) || string.IsNullOrEmpty(model.Email))
        {
            _logger.LogWarning("Model binding failed or incomplete - manually binding from form data");

            // Manually bind form data to model
            if (Request.Form.ContainsKey("FirstName"))
                model.FirstName = Request.Form["FirstName"].ToString() ?? string.Empty;
            if (Request.Form.ContainsKey("LastName"))
                model.LastName = Request.Form["LastName"].ToString() ?? string.Empty;
            if (Request.Form.ContainsKey("Email"))
                model.Email = Request.Form["Email"].ToString() ?? string.Empty;
            if (Request.Form.ContainsKey("PhoneNumber"))
                model.PhoneNumber = Request.Form["PhoneNumber"].ToString();
            if (Request.Form.ContainsKey("LinkedInProfile"))
                model.LinkedInProfile = Request.Form["LinkedInProfile"].ToString();
            if (Request.Form.ContainsKey("PortfolioUrl"))
                model.PortfolioUrl = Request.Form["PortfolioUrl"].ToString();
            if (Request.Form.ContainsKey("CoverLetter"))
                model.CoverLetter = Request.Form["CoverLetter"].ToString();
        }

        _logger.LogInformation("Model data - FirstName: '{FirstName}', LastName: '{LastName}', Email: '{Email}'",
            model.FirstName, model.LastName, model.Email);

        // Additional debugging - check if the model has the required data
        if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName) || string.IsNullOrEmpty(model.Email))
        {
            _logger.LogError("Model still has empty required fields after binding - FirstName: '{FirstName}', LastName: '{LastName}', Email: '{Email}'",
                model.FirstName, model.LastName, model.Email);

            // Try to get the data from Request.Form one more time
            _logger.LogInformation("Attempting final manual binding from Request.Form");
            foreach (var key in Request.Form.Keys)
            {
                _logger.LogInformation("Final Form[{Key}] = {Value}", key, Request.Form[key]);
            }
        }

        // Clear ModelState and re-validate manually
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join(", ", errors));

            // Log specific field errors
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    _logger.LogWarning("Field '{Key}' has errors: {Errors}", key, string.Join(", ", state.Errors.Select(e => e.ErrorMessage)));
                }
            }

            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }

        try
        {
            var result = await _applicantService.CreateApplicantAsync(jobPostId, model);
            if (result != null)
            {
                _logger.LogInformation("Successfully created applicant {ApplicantId} for job post {JobPostId}", result.Id, jobPostId);
                TempData["Success"] = "Applicant added successfully!";
                return RedirectToAction("Details", new { jobPostId, id = result.Id });
            }
            else
            {
                _logger.LogWarning("Failed to create applicant for job post {JobPostId}", jobPostId);
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCVStatuses(int applicantId)
    {
        try
        {
            var statuses = await _cvUploadService.GetCVFileStatusesAsync(applicantId);
            return Json(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV statuses for applicant {ApplicantId}", applicantId);
            return Json(new List<object>());
        }
    }

    private List<string> GetApplicantStatuses()
    {
        return new List<string> { "Pending", "Screened", "Rejected", "Accepted" };
    }
}
