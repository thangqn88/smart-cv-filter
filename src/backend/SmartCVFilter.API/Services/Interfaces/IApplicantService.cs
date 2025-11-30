using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IApplicantService
{
    Task<ApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request, int jobPostId);
    Task<ApplicantResponse?> GetApplicantByIdAsync(int id, string userId, bool isAdmin = false);
    Task<IEnumerable<ApplicantResponse>> GetApplicantsByJobPostAsync(int jobPostId, string userId, bool isAdmin = false);
    Task<ApplicantResponse> UpdateApplicantAsync(int id, UpdateApplicantRequest request);
    Task<bool> DeleteApplicantAsync(int id);
    Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request, string userId, bool isAdmin = false);

    // Paged methods
    Task<ApplicantPagedResponse> GetApplicantsPagedAsync(ApplicantPagedRequest request, string userId, bool isAdmin = false);
    
    // Search existing applicants
    Task<IEnumerable<ApplicantResponse>> SearchApplicantsAsync(string searchTerm, string userId, bool isAdmin = false);
}

