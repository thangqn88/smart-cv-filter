using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IScreeningService
{
    Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId);
    Task<IEnumerable<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId);
    Task<bool> ProcessScreeningAsync(int applicantId, int jobPostId);
    Task UpdateScreeningStatusAsync(int resultId, string status, string? errorMessage = null);
}

