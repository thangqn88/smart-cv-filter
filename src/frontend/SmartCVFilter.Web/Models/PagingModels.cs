namespace SmartCVFilter.Web.Models;

/// <summary>
/// Base class for paginated responses
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public int? PreviousPage => HasPreviousPage ? Page - 1 : null;
    public int? NextPage => HasNextPage ? Page + 1 : null;
}

/// <summary>
/// Base class for pagination requests
/// </summary>
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc"; // "asc" or "desc"

    /// <summary>
    /// Sets default values for pagination request
    /// </summary>
    public void SetDefaults(int defaultPageSize = 10)
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = defaultPageSize;
        if (string.IsNullOrEmpty(SortDirection)) SortDirection = "asc";
    }
}

/// <summary>
/// Pagination parameters for job posts
/// </summary>
public class JobPostPagedRequest : PagedRequest
{
    public string? Status { get; set; }
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? ExperienceLevel { get; set; }
}

/// <summary>
/// Pagination parameters for applicants
/// </summary>
public class ApplicantPagedRequest : PagedRequest
{
    public int JobPostId { get; set; }
    public string? Status { get; set; }
    public DateTime? AppliedFrom { get; set; }
    public DateTime? AppliedTo { get; set; }
}

/// <summary>
/// Paginated response for job posts
/// </summary>
public class JobPostPagedResponse : PagedResponse<JobPostListResponse>
{
    public int ActiveJobPosts { get; set; }
    public int InactiveJobPosts { get; set; }
    public int TotalApplicants { get; set; }
}

/// <summary>
/// Paginated response for applicants
/// </summary>
public class ApplicantPagedResponse : PagedResponse<ApplicantResponse>
{
    public int PendingApplicants { get; set; }
    public int ScreenedApplicants { get; set; }
    public int RejectedApplicants { get; set; }
    public int TotalCVFiles { get; set; }
}

/// <summary>
/// Pagination info for views
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int? PreviousPage { get; set; }
    public int? NextPage { get; set; }
    public int StartItem { get; set; }
    public int EndItem { get; set; }
}

/// <summary>
/// View model for paginated job posts
/// </summary>
public class JobPostPagedViewModel
{
    public JobPostPagedResponse Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public JobPostPagedRequest Filters { get; set; } = new();
    public List<string> Departments { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public List<string> EmploymentTypes { get; set; } = new();
    public List<string> ExperienceLevels { get; set; } = new();
}

/// <summary>
/// View model for paginated applicants
/// </summary>
public class ApplicantPagedViewModel
{
    public ApplicantPagedResponse Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public ApplicantPagedRequest Filters { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
    public string JobPostTitle { get; set; } = string.Empty;
    public int JobPostId { get; set; }
}
