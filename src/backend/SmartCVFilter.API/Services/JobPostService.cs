using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class JobPostService : IJobPostService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public JobPostService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<JobPostResponse> CreateJobPostAsync(CreateJobPostRequest request, string userId)
    {
        var jobPost = new JobPost
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Department = request.Department,
            EmploymentType = request.EmploymentType,
            ExperienceLevel = request.ExperienceLevel,
            RequiredSkills = request.RequiredSkills,
            PreferredSkills = request.PreferredSkills,
            Responsibilities = request.Responsibilities,
            Benefits = request.Benefits,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            ClosingDate = request.ClosingDate,
            UserId = userId,
            PostedDate = DateTime.UtcNow,
            Status = "Active"
        };

        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();

        return await GetJobPostByIdAsync(jobPost.Id, userId);
    }

    public async Task<JobPostResponse?> GetJobPostByIdAsync(int id, string userId, bool isAdmin = false)
    {
        var query = _context.JobPosts
            .Include(j => j.User)
            .Include(j => j.Applicants)
            .AsQueryable();

        // Admin users can see all job posts, regular users can only see their own
        if (!isAdmin)
        {
            query = query.Where(j => j.UserId == userId);
        }

        var jobPost = await query.FirstOrDefaultAsync(j => j.Id == id);

        if (jobPost == null)
            return null;

        return new JobPostResponse
        {
            Id = jobPost.Id,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Location = jobPost.Location,
            Department = jobPost.Department,
            EmploymentType = jobPost.EmploymentType,
            ExperienceLevel = jobPost.ExperienceLevel,
            RequiredSkills = jobPost.RequiredSkills,
            PreferredSkills = jobPost.PreferredSkills,
            Responsibilities = jobPost.Responsibilities,
            Benefits = jobPost.Benefits,
            SalaryMin = jobPost.SalaryMin,
            SalaryMax = jobPost.SalaryMax,
            Status = jobPost.Status,
            PostedDate = jobPost.PostedDate,
            ClosingDate = jobPost.ClosingDate,
            ApplicantCount = jobPost.Applicants.Count,
            User = new UserInfo
            {
                Id = jobPost.User.Id,
                Email = jobPost.User.Email!,
                FirstName = jobPost.User.FirstName,
                LastName = jobPost.User.LastName,
                CompanyName = jobPost.User.CompanyName
            }
        };
    }

    public async Task<IEnumerable<JobPostListResponse>> GetJobPostsByUserAsync(string userId)
    {
        var jobPosts = await _context.JobPosts
            .Where(j => j.UserId == userId)
            .Include(j => j.Applicants)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();

        return jobPosts.Select(jp => new JobPostListResponse
        {
            Id = jp.Id,
            Title = jp.Title,
            Location = jp.Location,
            Department = jp.Department,
            EmploymentType = jp.EmploymentType,
            ExperienceLevel = jp.ExperienceLevel,
            Status = jp.Status,
            PostedDate = jp.PostedDate,
            ApplicantCount = jp.Applicants.Count
        });
    }

    public async Task<JobPostResponse> UpdateJobPostAsync(int id, UpdateJobPostRequest request, string userId)
    {
        var jobPost = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (jobPost == null)
            throw new InvalidOperationException("Job post not found.");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Title))
            jobPost.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Description))
            jobPost.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Location))
            jobPost.Location = request.Location;
        if (!string.IsNullOrEmpty(request.Department))
            jobPost.Department = request.Department;
        if (!string.IsNullOrEmpty(request.EmploymentType))
            jobPost.EmploymentType = request.EmploymentType;
        if (!string.IsNullOrEmpty(request.ExperienceLevel))
            jobPost.ExperienceLevel = request.ExperienceLevel;
        if (!string.IsNullOrEmpty(request.RequiredSkills))
            jobPost.RequiredSkills = request.RequiredSkills;
        if (!string.IsNullOrEmpty(request.PreferredSkills))
            jobPost.PreferredSkills = request.PreferredSkills;
        if (!string.IsNullOrEmpty(request.Responsibilities))
            jobPost.Responsibilities = request.Responsibilities;
        if (!string.IsNullOrEmpty(request.Benefits))
            jobPost.Benefits = request.Benefits;
        if (request.SalaryMin.HasValue)
            jobPost.SalaryMin = request.SalaryMin;
        if (request.SalaryMax.HasValue)
            jobPost.SalaryMax = request.SalaryMax;
        if (!string.IsNullOrEmpty(request.Status))
            jobPost.Status = request.Status;
        if (request.ClosingDate.HasValue)
            jobPost.ClosingDate = request.ClosingDate;

        await _context.SaveChangesAsync();
        return await GetJobPostByIdAsync(id, userId);
    }

    public async Task<bool> DeleteJobPostAsync(int id, string userId)
    {
        var jobPost = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (jobPost == null)
            return false;

        _context.JobPosts.Remove(jobPost);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<JobPostListResponse>> GetAllJobPostsAsync()
    {
        var jobPosts = await _context.JobPosts
            .Where(j => j.Status == "Active")
            .Include(j => j.Applicants)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();

        return jobPosts.Select(jp => new JobPostListResponse
        {
            Id = jp.Id,
            Title = jp.Title,
            Location = jp.Location,
            Department = jp.Department,
            EmploymentType = jp.EmploymentType,
            ExperienceLevel = jp.ExperienceLevel,
            Status = jp.Status,
            PostedDate = jp.PostedDate,
            ApplicantCount = jp.Applicants.Count
        });
    }

    public async Task<IEnumerable<JobPostListResponse>> GetAllJobPostsForAdminAsync()
    {
        var jobPosts = await _context.JobPosts
            .Include(j => j.Applicants)
            .Include(j => j.User)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();

        return jobPosts.Select(jp => new JobPostListResponse
        {
            Id = jp.Id,
            Title = jp.Title,
            Location = jp.Location,
            Department = jp.Department,
            EmploymentType = jp.EmploymentType,
            ExperienceLevel = jp.ExperienceLevel,
            Status = jp.Status,
            PostedDate = jp.PostedDate,
            ApplicantCount = jp.Applicants.Count
        });
    }

}

