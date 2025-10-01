using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IScreeningService
{
    Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId, string userId, bool isAdmin = false);
    Task<IEnumerable<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId, string userId, bool isAdmin = false);
    Task<bool> ProcessScreeningAsync(int applicantId, int jobPostId);
    Task UpdateScreeningStatusAsync(int resultId, string status, string? errorMessage = null);
}

