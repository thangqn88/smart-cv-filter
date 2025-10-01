using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public interface IApiService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync();
    Task LogoutAsync();
    string? GetToken();
    void SetToken(string token);
    Task<T?> MakeRequestAsync<T>(string endpoint, HttpMethod method, object? content = null);
}

public interface IJobPostService
{
    Task<List<JobPostListResponse>> GetJobPostsAsync();
    Task<List<JobPostListResponse>> GetAllJobPostsAsync();
    Task<JobPostResponse?> GetJobPostAsync(int id);
    Task<JobPostResponse?> CreateJobPostAsync(CreateJobPostRequest request);
    Task<JobPostResponse?> UpdateJobPostAsync(int id, UpdateJobPostRequest request);
    Task<bool> DeleteJobPostAsync(int id);
}

public interface IApplicantService
{
    Task<List<ApplicantResponse>> GetApplicantsAsync(int jobPostId);
    Task<ApplicantResponse?> GetApplicantAsync(int jobPostId, int id);
    Task<ApplicantResponse?> CreateApplicantAsync(int jobPostId, CreateApplicantRequest request);
    Task<ApplicantResponse?> UpdateApplicantAsync(int jobPostId, int id, UpdateApplicantRequest request);
    Task<bool> DeleteApplicantAsync(int jobPostId, int id);
    Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request);
}

public interface IScreeningService
{
    Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId);
    Task<List<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId);
}
