using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class CVUploadService : ICVUploadService
{
    private readonly IApiService _apiService;
    private readonly ILogger<CVUploadService> _logger;

    public CVUploadService(IApiService apiService, ILogger<CVUploadService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<bool> UploadCVAsync(int applicantId, IFormFile file)
    {
        try
        {
            using var httpClient = new HttpClient();
            var token = _apiService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var baseUrl = "http://localhost:4000/api";
            var response = await httpClient.PostAsync($"{baseUrl}/applicants/{applicantId}/cvupload/upload", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV for applicant {ApplicantId}", applicantId);
            return false;
        }
    }

    public async Task<bool> DeleteCVAsync(int applicantId, int cvFileId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<object>($"api/applicants/{applicantId}/cvupload/{cvFileId}", HttpMethod.Delete);
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            return false;
        }
    }

    public async Task<byte[]> DownloadCVAsync(int applicantId, int cvFileId)
    {
        try
        {
            using var httpClient = new HttpClient();
            var token = _apiService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Use the same base URL as the API service
            var baseUrl = "http://localhost:4000/api";
            var response = await httpClient.GetAsync($"{baseUrl}/applicants/{applicantId}/cvupload/{cvFileId}/download");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new HttpRequestException($"Failed to download CV: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            throw;
        }
    }

    public async Task<string> ExtractTextFromCVAsync(int applicantId, int cvFileId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<object>($"api/applicants/{applicantId}/cvupload/{cvFileId}/extract-text", HttpMethod.Post);
            if (response is System.Text.Json.JsonElement jsonElement && jsonElement.TryGetProperty("extractedText", out var textElement))
            {
                return textElement.GetString() ?? string.Empty;
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from CV {CvFileId} for applicant {ApplicantId}", cvFileId, applicantId);
            return string.Empty;
        }
    }

    public async Task<List<CVFileStatusResponse>> GetCVFileStatusesAsync(int applicantId)
    {
        try
        {
            var response = await _apiService.MakeRequestAsync<List<CVFileStatusResponse>>($"api/applicants/{applicantId}/cvupload/status", HttpMethod.Get);
            return response ?? new List<CVFileStatusResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV file statuses for applicant {ApplicantId}", applicantId);
            return new List<CVFileStatusResponse>();
        }
    }
}
