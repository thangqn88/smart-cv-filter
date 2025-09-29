using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using SmartCVFilter.Web.Models;
using System.Text;

namespace SmartCVFilter.Web.Services;

public class JobPostService : IJobPostService
{
    private readonly HttpClient _httpClient;
    private readonly IApiService _apiService;
    private readonly ILogger<JobPostService> _logger;

    public JobPostService(HttpClient httpClient, IApiService apiService, ILogger<JobPostService> logger)
    {
        _httpClient = httpClient;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<JobPostListResponse>> GetJobPostsAsync()
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync("jobposts");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JobPostListResponse>>(content) ?? new List<JobPostListResponse>();
            }

            return new List<JobPostListResponse>();
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
            var response = await _httpClient.GetAsync("jobposts/all");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JobPostListResponse>>(content) ?? new List<JobPostListResponse>();
            }

            return new List<JobPostListResponse>();
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
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync($"jobposts/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JobPostResponse>(content);
            }

            return null;
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
            await EnsureAuthenticatedAsync();
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("jobposts", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JobPostResponse>(responseContent);
            }

            return null;
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
            await EnsureAuthenticatedAsync();
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"jobposts/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JobPostResponse>(responseContent);
            }

            return null;
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
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.DeleteAsync($"jobposts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId}", id);
            return false;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = _apiService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
