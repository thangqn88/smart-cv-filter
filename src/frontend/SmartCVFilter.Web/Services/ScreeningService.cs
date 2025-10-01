using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class ScreeningService : IScreeningService
{
    private readonly IApiService _apiService;
    private readonly ILogger<ScreeningService> _logger;

    public ScreeningService(IApiService apiService, ILogger<ScreeningService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<ScreeningResultResponse>($"screening/results/{resultId}", HttpMethod.Get);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening result {ResultId}", resultId);
            return null;
        }
    }

    public async Task<List<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<ScreeningResultResponse>>($"screening/applicants/{applicantId}/results", HttpMethod.Get);
            return response ?? new List<ScreeningResultResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening results for applicant {ApplicantId}", applicantId);
            return new List<ScreeningResultResponse>();
        }
    }

    public async Task<List<ScreenedApplicantResponse>> GetScreenedApplicantsAsync()
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<ScreenedApplicantResponse>>("screening/screened-applicants", HttpMethod.Get);
            return response ?? new List<ScreenedApplicantResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screened applicants");
            return new List<ScreenedApplicantResponse>();
        }
    }

}
