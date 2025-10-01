using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class ApplicantService : IApplicantService
{
    private readonly IApiService _apiService;
    private readonly ILogger<ApplicantService> _logger;

    public ApplicantService(IApiService apiService, ILogger<ApplicantService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<ApplicantResponse>> GetApplicantsAsync(int jobPostId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<ApplicantResponse>>($"api/jobposts/{jobPostId}/applicants", HttpMethod.Get);
            return response ?? new List<ApplicantResponse>();
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
            var response = await _apiService.MakeRequestAsync<ApplicantResponse>($"api/jobposts/{jobPostId}/applicants/{id}", HttpMethod.Get);
            return response;
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
            var response = await _apiService.MakeRequestAsync<ApplicantResponse>($"api/jobposts/{jobPostId}/applicants", HttpMethod.Post, request);
            return response;
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
            var response = await _apiService.MakeRequestAsync<ApplicantResponse>($"api/jobposts/{jobPostId}/applicants/{id}", HttpMethod.Put, request);
            return response;
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
            var response = await _apiService.MakeRequestAsync<object>($"api/jobposts/{jobPostId}/applicants/{id}", HttpMethod.Delete);
            return response != null;
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
            var response = await _apiService.MakeRequestAsync<object>($"api/jobposts/{jobPostId}/applicants/screen", HttpMethod.Post, request);
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screening for job post {JobPostId}", jobPostId);
            return false;
        }
    }

}
