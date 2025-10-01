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
                jobPosts = await _jobPostService.GetJobPostsAsync();
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
    public async Task<IActionResult> Edit(int jobPostId, int id, UpdateApplicantRequest model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["JobPostId"] = jobPostId;
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
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating applicant {ApplicantId}", id);
            ModelState.AddModelError("", "An error occurred while updating the applicant. Please try again.");
            ViewData["JobPostId"] = jobPostId;
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
