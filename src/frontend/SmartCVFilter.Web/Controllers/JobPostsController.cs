using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.Web.Configuration;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

[Authorize]
public class JobPostsController : BaseController
{
    private readonly IJobPostService _jobPostService;
    private readonly ILogger<JobPostsController> _logger;

    public JobPostsController(IJobPostService jobPostService, ILogger<JobPostsController> logger, IOptions<PaginationSettings> paginationSettings)
        : base(logger, paginationSettings)
    {
        _jobPostService = jobPostService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(JobPostPagedRequest request)
    {
        // If no parameters provided, set defaults
        if (request.Page == 0)
        {
            request.Page = 1;
            request.PageSize = GetDefaultPageSize();
            request.Search = string.Empty;
            request.SortBy = "posteddate";
            request.SortDirection = "desc";
        }

        // Use the same logic as Paged action
        try
        {
            ViewData["Title"] = "Job Posting Management";

            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;

            // Get paged data
            var isAdmin = IsCurrentUserAdmin();
            var response = await _jobPostService.GetJobPostsPagedAsync(request);

            if (response == null)
            {
                TempData["Error"] = "An error occurred while loading job posts.";
                return View(new JobPostPagedViewModel());
            }

            // Create view model
            var viewModel = new JobPostPagedViewModel
            {
                Data = response,
                Pagination = CreatePaginationInfo(response),
                Filters = request,
                Departments = await GetDistinctDepartments(),
                Locations = await GetDistinctLocations(),
                EmploymentTypes = GetEmploymentTypes(),
                ExperienceLevels = GetExperienceLevels()
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "loading job posts");
        }
    }

    /// <summary>
    /// Paged job posts with filtering and sorting
    /// </summary>
    public async Task<IActionResult> Paged(JobPostPagedRequest request)
    {
        try
        {
            ViewData["Title"] = "Job Posting Management";

            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;

            // Get paged data
            var isAdmin = IsCurrentUserAdmin();
            var response = isAdmin
                ? await _jobPostService.GetJobPostsPagedAsync(request)
                : await _jobPostService.GetAllJobPostsPagedAsync(request);

            if (response == null)
            {
                TempData["Error"] = "An error occurred while loading job posts.";
                return View(new JobPostPagedViewModel());
            }

            // Create view model
            var viewModel = new JobPostPagedViewModel
            {
                Data = response,
                Pagination = CreatePaginationInfo(response),
                Filters = request,
                Departments = await GetDistinctDepartments(),
                Locations = await GetDistinctLocations(),
                EmploymentTypes = GetEmploymentTypes(),
                ExperienceLevels = GetExperienceLevels()
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "loading job posts");
        }
    }

    /// <summary>
    /// AJAX endpoint for paged job posts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(JobPostPagedRequest request)
    {
        try
        {
            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;

            // Get paged data
            var isAdmin = IsCurrentUserAdmin();
            var response = isAdmin
                ? await _jobPostService.GetJobPostsPagedAsync(request)
                : await _jobPostService.GetAllJobPostsPagedAsync(request);

            if (response == null)
            {
                return CreateErrorResponse("An error occurred while loading job posts.");
            }

            return CreateSuccessResponse(response);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse($"An error occurred while loading job posts: {ex.Message}");
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
        _logger.LogInformation("Creating job post: {Title}", model?.Title ?? "Unknown");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join(", ", errors));
            return View(model);
        }

        try
        {
            var result = await _jobPostService.CreateJobPostAsync(model!);
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

    private async Task<List<string>> GetDistinctDepartments()
    {
        try
        {
            var jobPosts = await _jobPostService.GetAllJobPostsAsync();
            return jobPosts.Select(jp => jp.Department).Distinct().OrderBy(d => d).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<List<string>> GetDistinctLocations()
    {
        try
        {
            var jobPosts = await _jobPostService.GetAllJobPostsAsync();
            return jobPosts.Select(jp => jp.Location).Distinct().OrderBy(l => l).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private List<string> GetEmploymentTypes()
    {
        return new List<string> { "Full-time", "Part-time", "Contract", "Internship", "Freelance" };
    }

    private List<string> GetExperienceLevels()
    {
        return new List<string> { "Entry Level", "Mid Level", "Senior Level", "Executive" };
    }
}
