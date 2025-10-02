using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class JobPostService : IJobPostService
{
    private readonly IApiService _apiService;
    private readonly ILogger<JobPostService> _logger;

    public JobPostService(IApiService apiService, ILogger<JobPostService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<JobPostListResponse>> GetJobPostsAsync()
    {
        try
        {
            // Use ApiService to make the request with proper configuration
            var response = await _apiService.MakeRequestAsync<List<JobPostListResponse>>("jobposts", HttpMethod.Get);
            return response ?? new List<JobPostListResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job posts");
            return new List<JobPostListResponse>();
        }
    }

    public async Task<List<JobPostListResponse>> GetAllJobPostsAsync()
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<JobPostListResponse>>("jobposts/all", HttpMethod.Get);
            return response ?? new List<JobPostListResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all job posts");
            return new List<JobPostListResponse>();
        }
    }

    public async Task<List<JobPostListResponse>> GetAllJobPostsForAdminAsync()
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<JobPostListResponse>>("jobposts/admin/all", HttpMethod.Get);
            return response ?? new List<JobPostListResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all job posts for admin");
            return new List<JobPostListResponse>();
        }
    }

    public async Task<JobPostResponse?> GetJobPostAsync(int id)
    {
        try
        {
            _logger.LogInformation("Making API request for job post ID: {JobPostId}", id);
            var response = await _apiService.MakeRequestAsync<JobPostResponse>($"jobposts/{id}", HttpMethod.Get);
            _logger.LogInformation("API response for job post {JobPostId}: {Response}", id, response != null ? "Success" : "Null");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job post {JobPostId}", id);
            return null;
        }
    }

    public async Task<JobPostResponse?> CreateJobPostAsync(CreateJobPostRequest request)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<JobPostResponse>("jobposts", HttpMethod.Post, request);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job post");
            return null;
        }
    }

    public async Task<JobPostResponse?> UpdateJobPostAsync(int id, UpdateJobPostRequest request)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<JobPostResponse>($"jobposts/{id}", HttpMethod.Put, request);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job post {JobPostId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteJobPostAsync(int id)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<object>($"jobposts/{id}", HttpMethod.Delete);
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId}", id);
            return false;
        }
    }

    public async Task<JobPostPagedResponse?> GetJobPostsPagedAsync(JobPostPagedRequest request)
    {
        try
        {
            var queryParams = new List<string>();

            queryParams.Add($"page={request.Page}");
            queryParams.Add($"pageSize={request.PageSize}");

            if (!string.IsNullOrEmpty(request.Search))
                queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
            if (!string.IsNullOrEmpty(request.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");
            if (!string.IsNullOrEmpty(request.SortDirection))
                queryParams.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection)}");
            if (!string.IsNullOrEmpty(request.Status))
                queryParams.Add($"status={Uri.EscapeDataString(request.Status)}");
            if (!string.IsNullOrEmpty(request.Department))
                queryParams.Add($"department={Uri.EscapeDataString(request.Department)}");
            if (!string.IsNullOrEmpty(request.Location))
                queryParams.Add($"location={Uri.EscapeDataString(request.Location)}");
            if (!string.IsNullOrEmpty(request.EmploymentType))
                queryParams.Add($"employmentType={Uri.EscapeDataString(request.EmploymentType)}");
            if (!string.IsNullOrEmpty(request.ExperienceLevel))
                queryParams.Add($"experienceLevel={Uri.EscapeDataString(request.ExperienceLevel)}");

            var endpoint = $"jobposts/paged?{string.Join("&", queryParams)}";
            var response = await _apiService.MakeRequestAsync<JobPostPagedResponse>(endpoint, HttpMethod.Get);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged job posts");
            return null;
        }
    }

    public async Task<JobPostPagedResponse?> GetAllJobPostsPagedAsync(JobPostPagedRequest request)
    {
        try
        {
            var queryParams = new List<string>();

            queryParams.Add($"page={request.Page}");
            queryParams.Add($"pageSize={request.PageSize}");

            if (!string.IsNullOrEmpty(request.Search))
                queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");
            if (!string.IsNullOrEmpty(request.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");
            if (!string.IsNullOrEmpty(request.SortDirection))
                queryParams.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection)}");
            if (!string.IsNullOrEmpty(request.Department))
                queryParams.Add($"department={Uri.EscapeDataString(request.Department)}");
            if (!string.IsNullOrEmpty(request.Location))
                queryParams.Add($"location={Uri.EscapeDataString(request.Location)}");
            if (!string.IsNullOrEmpty(request.EmploymentType))
                queryParams.Add($"employmentType={Uri.EscapeDataString(request.EmploymentType)}");
            if (!string.IsNullOrEmpty(request.ExperienceLevel))
                queryParams.Add($"experienceLevel={Uri.EscapeDataString(request.ExperienceLevel)}");

            var endpoint = $"jobposts/paged/all?{string.Join("&", queryParams)}";
            var response = await _apiService.MakeRequestAsync<JobPostPagedResponse>(endpoint, HttpMethod.Get);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all paged job posts");
            return null;
        }
    }

}
