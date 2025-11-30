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
    string GetBaseUrl();
}

public interface IJobPostService
{
    Task<List<JobPostListResponse>> GetJobPostsAsync();
    Task<List<JobPostListResponse>> GetAllJobPostsAsync();
    Task<List<JobPostListResponse>> GetAllJobPostsForAdminAsync();
    Task<JobPostResponse?> GetJobPostAsync(int id);
    Task<JobPostResponse?> CreateJobPostAsync(CreateJobPostRequest request);
    Task<JobPostResponse?> UpdateJobPostAsync(int id, UpdateJobPostRequest request);
    Task<bool> DeleteJobPostAsync(int id);

    // Paged methods
    Task<JobPostPagedResponse?> GetJobPostsPagedAsync(JobPostPagedRequest request);
    Task<JobPostPagedResponse?> GetAllJobPostsPagedAsync(JobPostPagedRequest request);
}

public interface IApplicantService
{
    Task<List<ApplicantResponse>> GetApplicantsAsync(int jobPostId);
    Task<ApplicantResponse?> GetApplicantAsync(int jobPostId, int id);
    Task<ApplicantResponse?> CreateApplicantAsync(int jobPostId, CreateApplicantRequest request);
    Task<ApplicantResponse?> UpdateApplicantAsync(int jobPostId, int id, UpdateApplicantRequest request);
    Task<bool> DeleteApplicantAsync(int jobPostId, int id);
    Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request);

    // Paged methods
    Task<ApplicantPagedResponse?> GetApplicantsPagedAsync(ApplicantPagedRequest request);
    
    // Search existing applicants
    Task<List<ApplicantResponse>> SearchApplicantsAsync(string searchTerm);
}

public interface ICVUploadService
{
    Task<bool> UploadCVAsync(int applicantId, IFormFile file);
    Task<bool> DeleteCVAsync(int applicantId, int cvFileId);
    Task<byte[]> DownloadCVAsync(int applicantId, int cvFileId);
    Task<string> ExtractTextFromCVAsync(int applicantId, int cvFileId);
    Task<List<CVFileStatusResponse>> GetCVFileStatusesAsync(int applicantId);
}

public class CVFileStatusResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public interface IScreeningService
{
    Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId);
    Task<List<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId);
    Task<List<ScreenedApplicantResponse>> GetScreenedApplicantsAsync();
}

public interface IUserService
{
    Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null);
    Task<UserResponse?> GetUserAsync(string id);
    Task<UserResponse?> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(string id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ChangePasswordAsync(string id, ChangePasswordRequest request);
    Task<bool> AssignRoleAsync(string id, AssignRoleRequest request);
    Task<bool> RemoveRoleAsync(string id, string roleName);
    Task<List<string>> GetUserRolesAsync(string id);
    Task<bool> ToggleUserStatusAsync(string id);
}

public interface IRoleService
{
    Task<RoleListResponse> GetRolesAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<RoleResponse?> GetRoleAsync(string id);
    Task<RoleResponse?> GetRoleByNameAsync(string name);
    Task<RoleResponse?> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleResponse?> UpdateRoleAsync(string id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(string id);
    Task<bool> AssignRoleToUserAsync(AssignRoleRequest request);
    Task<bool> RemoveRoleFromUserAsync(AssignRoleRequest request);
}