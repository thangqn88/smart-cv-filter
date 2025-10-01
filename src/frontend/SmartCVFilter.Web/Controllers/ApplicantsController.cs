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
    private readonly ILogger<ApplicantsController> _logger;

    public ApplicantsController(
        IApplicantService applicantService,
        IJobPostService jobPostService,
        ILogger<ApplicantsController> logger)
    {
        _applicantService = applicantService;
        _jobPostService = jobPostService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? jobPostId, int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        try
        {
            ViewData["Title"] = "Applicant Management";
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Search"] = search;
            ViewData["Status"] = status;

            List<ApplicantResponse> applicants = new();
            List<JobPostListResponse> jobPosts = new();

            if (jobPostId.HasValue)
            {
                var allApplicants = await _applicantService.GetApplicantsAsync(jobPostId.Value);

                // Apply filtering
                var filteredApplicants = allApplicants.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    filteredApplicants = filteredApplicants.Where(a =>
                        a.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        a.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        a.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (a.PhoneNumber != null && a.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    filteredApplicants = filteredApplicants.Where(a => a.Status == status);
                }

                // Apply pagination
                var totalRecords = filteredApplicants.Count();
                applicants = filteredApplicants
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewData["TotalRecords"] = totalRecords;
                ViewData["TotalPages"] = (int)Math.Ceiling((double)totalRecords / pageSize);

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
}
