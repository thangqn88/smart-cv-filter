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
        _logger.LogInformation("UploadCVAsync called. ApplicantId: {ApplicantId}, FileName: {FileName}, FileSize: {FileSize}, ContentType: {ContentType}",
            applicantId, file?.FileName, file?.Length, file?.ContentType);

        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("UploadCVAsync: File is null or empty. ApplicantId: {ApplicantId}", applicantId);
                return false;
            }

            using var httpClient = new HttpClient();
            var token = _apiService.GetToken();
            
            _logger.LogDebug("UploadCVAsync: Token retrieved. HasToken: {HasToken}, TokenLength: {TokenLength}",
                !string.IsNullOrEmpty(token), token?.Length ?? 0);

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("UploadCVAsync: Authorization header set");
            }
            else
            {
                _logger.LogWarning("UploadCVAsync: No authentication token available");
            }

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            _logger.LogDebug("UploadCVAsync: MultipartFormDataContent created. FileName: {FileName}, ContentType: {ContentType}",
                file.FileName, file.ContentType);

            // Get base URL from ApiService configuration instead of hardcoding
            var baseUrl = _apiService.GetBaseUrl();
            var uploadUrl = $"{baseUrl.TrimEnd('/')}/applicants/{applicantId}/cvupload/upload";
            
            _logger.LogInformation("UploadCVAsync: Sending request to {UploadUrl}", uploadUrl);

            var startTime = DateTime.UtcNow;
            var response = await httpClient.PostAsync(uploadUrl, content);
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("UploadCVAsync: Response received. StatusCode: {StatusCode}, StatusText: {StatusText}, Duration: {Duration}ms",
                (int)response.StatusCode, response.StatusCode, duration);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("UploadCVAsync: Upload successful. Response: {Response}", responseContent);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("UploadCVAsync: Upload failed. StatusCode: {StatusCode}, Response: {Response}",
                    (int)response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, 
                "HttpRequestException in UploadCVAsync. ApplicantId: {ApplicantId}, Message: {Message}, InnerException: {InnerException}",
                applicantId, ex.Message, ex.InnerException?.Message);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, 
                "TaskCanceledException (timeout) in UploadCVAsync. ApplicantId: {ApplicantId}, Message: {Message}",
                applicantId, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception in UploadCVAsync. ApplicantId: {ApplicantId}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                applicantId, ex.GetType().Name, ex.Message, ex.StackTrace);
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
            var baseUrl = _apiService.GetBaseUrl();
            var response = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/applicants/{applicantId}/cvupload/{cvFileId}/download");

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
