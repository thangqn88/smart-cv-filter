using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IJobPostService
{
    Task<JobPostResponse> CreateJobPostAsync(CreateJobPostRequest request, string userId);
    Task<JobPostResponse?> GetJobPostByIdAsync(int id, string userId, bool isAdmin = false);
    Task<IEnumerable<JobPostListResponse>> GetJobPostsByUserAsync(string userId);
    Task<JobPostResponse> UpdateJobPostAsync(int id, UpdateJobPostRequest request, string userId);
    Task<bool> DeleteJobPostAsync(int id, string userId);
    Task<IEnumerable<JobPostListResponse>> GetAllJobPostsAsync();
    Task<IEnumerable<JobPostListResponse>> GetAllJobPostsForAdminAsync();

    // Paged methods
    Task<JobPostPagedResponse> GetJobPostsPagedAsync(JobPostPagedRequest request, string userId, bool isAdmin = false);
    Task<JobPostPagedResponse> GetAllJobPostsPagedAsync(JobPostPagedRequest request);
}

