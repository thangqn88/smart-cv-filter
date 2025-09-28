using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IApplicantService
{
    Task<ApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request, int jobPostId);
    Task<ApplicantResponse?> GetApplicantByIdAsync(int id);
    Task<IEnumerable<ApplicantResponse>> GetApplicantsByJobPostAsync(int jobPostId, string userId);
    Task<ApplicantResponse> UpdateApplicantAsync(int id, UpdateApplicantRequest request);
    Task<bool> DeleteApplicantAsync(int id);
    Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request, string userId);
}

