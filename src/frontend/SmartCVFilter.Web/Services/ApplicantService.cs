using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using SmartCVFilter.Web.Models;
using System.Text;

namespace SmartCVFilter.Web.Services;

public class ApplicantService : IApplicantService
{
    private readonly HttpClient _httpClient;
    private readonly IApiService _apiService;
    private readonly ILogger<ApplicantService> _logger;

    public ApplicantService(HttpClient httpClient, IApiService apiService, ILogger<ApplicantService> logger)
    {
        _httpClient = httpClient;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<ApplicantResponse>> GetApplicantsAsync(int jobPostId)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync($"jobposts/{jobPostId}/applicants");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ApplicantResponse>>(content) ?? new List<ApplicantResponse>();
            }

            return new List<ApplicantResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicants for job post {JobPostId}", jobPostId);
            return new List<ApplicantResponse>();
        }
    }

    public async Task<ApplicantResponse?> GetApplicantAsync(int jobPostId, int id)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync($"jobposts/{jobPostId}/applicants/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicantResponse>(content);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return null;
        }
    }

    public async Task<ApplicantResponse?> CreateApplicantAsync(int jobPostId, CreateApplicantRequest request)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"jobposts/{jobPostId}/applicants", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicantResponse>(responseContent);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating applicant for job post {JobPostId}", jobPostId);
            return null;
        }
    }

    public async Task<ApplicantResponse?> UpdateApplicantAsync(int jobPostId, int id, UpdateApplicantRequest request)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"jobposts/{jobPostId}/applicants/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApplicantResponse>(responseContent);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return null;
        }
    }

    public async Task<bool> DeleteApplicantAsync(int jobPostId, int id)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.DeleteAsync($"jobposts/{jobPostId}/applicants/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting applicant {ApplicantId} for job post {JobPostId}", id, jobPostId);
            return false;
        }
    }

    public async Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"jobposts/{jobPostId}/applicants/screen", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screening for job post {JobPostId}", jobPostId);
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
