using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

public class JobPostsController : Controller
{
    private readonly IJobPostService _jobPostService;
    private readonly ILogger<JobPostsController> _logger;

    public JobPostsController(IJobPostService jobPostService, ILogger<JobPostsController> logger)
    {
        _jobPostService = jobPostService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        try
        {
            ViewData["Title"] = "Job Posting Management";
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Search"] = search;
            ViewData["Status"] = status;

            // Check if user is admin
            var isAdmin = User.IsInRole("Admin");
            var jobPosts = isAdmin
                ? await _jobPostService.GetAllJobPostsForAdminAsync()
                : await _jobPostService.GetAllJobPostsAsync();

            // Apply client-side filtering for now (in a real app, this would be server-side)
            var filteredJobPosts = jobPosts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                filteredJobPosts = filteredJobPosts.Where(jp =>
                    jp.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    jp.Location.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    jp.Department.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                filteredJobPosts = filteredJobPosts.Where(jp => jp.Status == status);
            }

            var totalRecords = filteredJobPosts.Count();
            var paginatedJobPosts = filteredJobPosts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["TotalRecords"] = totalRecords;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalRecords / pageSize);

            return View(paginatedJobPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading job posts");
            TempData["Error"] = "An error occurred while loading job posts.";
            return View(new List<JobPostListResponse>());
        }
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var jobPost = await _jobPostService.GetJobPostAsync(id);
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

    [Authorize]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Job Post";
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJobPostRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _jobPostService.CreateJobPostAsync(model);
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

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to load job post for edit, ID: {JobPostId}", id);

            var jobPost = await _jobPostService.GetJobPostAsync(id);
            _logger.LogInformation("Job post retrieved: {JobPost}", jobPost != null ? "Success" : "Null");

            if (jobPost == null)
            {
                _logger.LogWarning("Job post not found for ID: {JobPostId}", id);
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

            _logger.LogInformation("Update model created with Title: {Title}", updateModel.Title);
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
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateJobPostRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _jobPostService.UpdateJobPostAsync(id, model);
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
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _jobPostService.DeleteJobPostAsync(id);
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
