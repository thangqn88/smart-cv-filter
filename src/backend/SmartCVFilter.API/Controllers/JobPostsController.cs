using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCVFilter.API.Configuration;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobPostListResponse>>> GetJobPosts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Admin users can see all job posts, regular users can only see their own
            var jobPosts = userRole == "Admin"
                ? await _jobPostService.GetAllJobPostsForAdminAsync()
                : await _jobPostService.GetJobPostsByUserAsync(userId);

            return Ok(jobPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job posts");
            return StatusCode(500, new { message = "An error occurred while retrieving job posts." });
        }
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<JobPostListResponse>>> GetAllJobPosts()
    {
        try
        {
            var jobPosts = await _jobPostService.GetAllJobPostsAsync();
            return Ok(jobPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all job posts");
            return StatusCode(500, new { message = "An error occurred while retrieving job posts." });
        }
    }

    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<JobPostListResponse>>> GetAllJobPostsForAdmin()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Only admin users can access this endpoint
            if (userRole != "Admin")
                return Forbid();

            var jobPosts = await _jobPostService.GetAllJobPostsForAdminAsync();
            return Ok(jobPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all job posts for admin");
            return StatusCode(500, new { message = "An error occurred while retrieving job posts." });
        }
    }

    /// <summary>
    /// Get job posts with pagination and filtering
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<JobPostPagedResponse>> GetJobPostsPaged([FromQuery] JobPostPagedRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAdmin = IsCurrentUserAdmin();
            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;

            var result = await _jobPostService.GetJobPostsPagedAsync(request, userId, isAdmin);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting job posts");
        }
    }

    /// <summary>
    /// Get all active job posts with pagination and filtering (public endpoint)
    /// </summary>
    [HttpGet("paged/all")]
    [AllowAnonymous]
    public async Task<ActionResult<JobPostPagedResponse>> GetAllJobPostsPaged([FromQuery] JobPostPagedRequest request)
    {
        try
        {
            // Set defaults and validate pagination parameters
            request.SetDefaults(GetDefaultPageSize());
            var (page, pageSize) = ValidatePaginationParameters(request.Page, request.PageSize);
            request.Page = page;
            request.PageSize = pageSize;

            var result = await _jobPostService.GetAllJobPostsPagedAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "getting all job posts");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobPostResponse>> GetJobPost(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Admin users can see all job posts, regular users can only see their own
            var jobPost = await _jobPostService.GetJobPostByIdAsync(id, userId, userRole == "Admin");
            if (jobPost == null)
                return NotFound();

            return Ok(jobPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job post {JobPostId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the job post." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobPostResponse>> CreateJobPost(CreateJobPostRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Debug logging
            _logger.LogInformation("CreateJobPost - User.Identity.IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("CreateJobPost - User.Identity.Name: {Name}", User.Identity?.Name);
            _logger.LogInformation("CreateJobPost - UserId from claims: {UserId}", userId);
            _logger.LogInformation("CreateJobPost - All claims: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CreateJobPost - UserId is null or empty, returning Unauthorized");
                return Unauthorized();
            }

            var jobPost = await _jobPostService.CreateJobPostAsync(request, userId);
            return CreatedAtAction(nameof(GetJobPost), new { id = jobPost.Id }, jobPost);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating job post: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job post");
            return StatusCode(500, new { message = "An error occurred while creating the job post." });
        }
    }

    [HttpGet("test-auth")]
    public ActionResult TestAuth()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            _logger.LogInformation("TestAuth - User.Identity.IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("TestAuth - UserId: {UserId}", userId);
            _logger.LogInformation("TestAuth - UserRole: {UserRole}", userRole);
            _logger.LogInformation("TestAuth - UserName: {UserName}", userName);
            _logger.LogInformation("TestAuth - UserEmail: {UserEmail}", userEmail);
            _logger.LogInformation("TestAuth - All claims: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));

            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserId = userId,
                UserRole = userRole,
                UserName = userName,
                UserEmail = userEmail,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestAuth");
            return StatusCode(500, new { message = "An error occurred while testing authentication." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JobPostResponse>> UpdateJobPost(int id, UpdateJobPostRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var jobPost = await _jobPostService.UpdateJobPostAsync(id, request, userId);
            return Ok(jobPost);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job post {JobPostId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the job post." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJobPost(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _jobPostService.DeleteJobPostAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the job post." });
        }
    }
}

