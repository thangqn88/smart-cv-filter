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
            var response = await _apiService.MakeRequestAsync<List<JobPostListResponse>>("api/jobposts", HttpMethod.Get);
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
            var response = await _apiService.MakeRequestAsync<List<JobPostListResponse>>("api/jobposts/all", HttpMethod.Get);
            return response ?? new List<JobPostListResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all job posts");
            return new List<JobPostListResponse>();
        }
    }

    public async Task<JobPostResponse?> GetJobPostAsync(int id)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<JobPostResponse>($"api/jobposts/{id}", HttpMethod.Get);
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
            var response = await _apiService.MakeRequestAsync<JobPostResponse>("api/jobposts", HttpMethod.Post, request);
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
            var response = await _apiService.MakeRequestAsync<JobPostResponse>($"api/jobposts/{id}", HttpMethod.Put, request);
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
            var response = await _apiService.MakeRequestAsync<object>($"api/jobposts/{id}", HttpMethod.Delete);
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId}", id);
            return false;
        }
    }

}
