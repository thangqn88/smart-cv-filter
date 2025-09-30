using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[Authorize]
public class JobPostsMvcController : Controller
{
    private readonly IJobPostService _jobPostService;
    private readonly ILogger<JobPostsMvcController> _logger;

    public JobPostsMvcController(IJobPostService jobPostService, ILogger<JobPostsMvcController> logger)
    {
        _jobPostService = jobPostService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewData["Title"] = "Job Posting Management";
            var jobPosts = await _jobPostService.GetAllJobPostsAsync();
            return View(jobPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading job posts");
            TempData["Error"] = "An error occurred while loading job posts.";
            return View(new List<JobPostResponse>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var jobPost = await _jobPostService.GetJobPostByIdAsync(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            if (jobPost == null)
            {
                TempData["Error"] = "Job post not found.";
                return RedirectToAction("Index");
            }

            ViewData["Title"] = $"Job Post Details - {jobPost.Title}";
            return View(jobPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading job post details for ID {JobPostId}", id);
            TempData["Error"] = "An error occurred while loading job post details.";
            return RedirectToAction("Index");
        }
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Job Post";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJobPostRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _jobPostService.CreateJobPostAsync(model, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            if (result != null)
            {
                TempData["Success"] = "Job post created successfully!";
                return RedirectToAction("Details", new { id = result.Id });
            }
            else
            {
                ModelState.AddModelError("", "Failed to create job post. Please try again.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job post");
            ModelState.AddModelError("", "An error occurred while creating the job post. Please try again.");
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var jobPost = await _jobPostService.GetJobPostByIdAsync(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            if (jobPost == null)
            {
                TempData["Error"] = "Job post not found.";
                return RedirectToAction("Index");
            }

            ViewData["Title"] = $"Edit Job Post - {jobPost.Title}";

            var updateModel = new UpdateJobPostRequest
            {
                Title = jobPost.Title,
                Description = jobPost.Description,
                Location = jobPost.Location,
                Department = jobPost.Department,
                EmploymentType = jobPost.EmploymentType,
                ExperienceLevel = jobPost.ExperienceLevel,
                RequiredSkills = jobPost.RequiredSkills,
                PreferredSkills = jobPost.PreferredSkills,
                Responsibilities = jobPost.Responsibilities,
                Benefits = jobPost.Benefits,
                SalaryMin = jobPost.SalaryMin,
                SalaryMax = jobPost.SalaryMax,
                Status = jobPost.Status,
                ClosingDate = jobPost.ClosingDate
            };

            return View(updateModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading job post for edit, ID {JobPostId}", id);
            TempData["Error"] = "An error occurred while loading the job post for editing.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateJobPostRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _jobPostService.UpdateJobPostAsync(id, model, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            if (result != null)
            {
                TempData["Success"] = "Job post updated successfully!";
                return RedirectToAction("Details", new { id = result.Id });
            }
            else
            {
                ModelState.AddModelError("", "Failed to update job post. Please try again.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job post {JobPostId}", id);
            ModelState.AddModelError("", "An error occurred while updating the job post. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _jobPostService.DeleteJobPostAsync(id, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
            if (result)
            {
                TempData["Success"] = "Job post deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete job post.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId}", id);
            TempData["Error"] = "An error occurred while deleting the job post.";
        }

        return RedirectToAction("Index");
    }
}
