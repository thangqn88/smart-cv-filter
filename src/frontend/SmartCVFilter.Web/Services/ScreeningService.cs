using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class ScreeningService : IScreeningService
{
    private readonly HttpClient _httpClient;
    private readonly IApiService _apiService;
    private readonly ILogger<ScreeningService> _logger;

    public ScreeningService(HttpClient httpClient, IApiService apiService, ILogger<ScreeningService> logger)
    {
        _httpClient = httpClient;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId)
    {
        try
        {
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync($"screening/results/{resultId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ScreeningResultResponse>(content);
            }

            return null;
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
            await EnsureAuthenticatedAsync();
            var response = await _httpClient.GetAsync($"screening/applicants/{applicantId}/results");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ScreeningResultResponse>>(content) ?? new List<ScreeningResultResponse>();
            }

            return new List<ScreeningResultResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screening results for applicant {ApplicantId}", applicantId);
            return new List<ScreeningResultResponse>();
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
