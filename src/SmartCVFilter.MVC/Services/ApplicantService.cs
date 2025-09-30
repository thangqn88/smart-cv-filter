using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class ApplicantService : IApplicantService
{
    private readonly ApplicationDbContext _context;
    private readonly IScreeningService _screeningService;

    public ApplicantService(ApplicationDbContext context, IScreeningService screeningService)
    {
        _context = context;
        _screeningService = screeningService;
    }

    public async Task<ApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request, int jobPostId)
    {
        var jobPost = await _context.JobPosts.FindAsync(jobPostId);
        if (jobPost == null)
            throw new InvalidOperationException("Job post not found.");

        var applicant = new Applicant
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            LinkedInProfile = request.LinkedInProfile,
            PortfolioUrl = request.PortfolioUrl,
            CoverLetter = request.CoverLetter,
            JobPostId = jobPostId,
            AppliedDate = DateTime.UtcNow,
            Status = "Applied"
        };

        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();

        return await GetApplicantByIdAsync(applicant.Id);
    }

    public async Task<ApplicantResponse?> GetApplicantByIdAsync(int id)
    {
        var applicant = await _context.Applicants
            .Include(a => a.JobPost)
            .Include(a => a.CVFiles)
            .Include(a => a.ScreeningResults)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (applicant == null)
            return null;

        return new ApplicantResponse
        {
            Id = applicant.Id,
            FirstName = applicant.FirstName,
            LastName = applicant.LastName,
            Email = applicant.Email,
            PhoneNumber = applicant.PhoneNumber,
            LinkedInProfile = applicant.LinkedInProfile,
            PortfolioUrl = applicant.PortfolioUrl,
            CoverLetter = applicant.CoverLetter,
            Status = applicant.Status,
            AppliedDate = applicant.AppliedDate,
            LastUpdated = applicant.LastUpdated,
            JobPostId = applicant.JobPostId,
            JobTitle = applicant.JobPost.Title,
            CVFiles = applicant.CVFiles.Select(cv => new CVFileResponse
            {
                Id = cv.Id,
                FileName = cv.FileName,
                ContentType = cv.ContentType,
                FileSize = cv.FileSize,
                FileExtension = cv.FileExtension,
                UploadedDate = cv.UploadedDate,
                Status = cv.Status
            }).ToList(),
            ScreeningResults = applicant.ScreeningResults.Select(sr => new ScreeningResultResponse
            {
                Id = sr.Id,
                OverallScore = sr.OverallScore,
                Summary = sr.Summary,
                Strengths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sr.Strengths) ?? new List<string>(),
                Weaknesses = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sr.Weaknesses) ?? new List<string>(),
                DetailedAnalysis = sr.DetailedAnalysis,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                CompletedAt = sr.CompletedAt
            }).ToList()
        };
    }

    public async Task<IEnumerable<ApplicantResponse>> GetApplicantsByJobPostAsync(int jobPostId, string userId)
    {
        // Verify that the job post belongs to the user
        var jobPost = await _context.JobPosts
            .FirstOrDefaultAsync(jp => jp.Id == jobPostId && jp.UserId == userId);

        if (jobPost == null)
            throw new UnauthorizedAccessException("You don't have access to this job post.");

        var applicants = await _context.Applicants
            .Where(a => a.JobPostId == jobPostId)
            .Include(a => a.JobPost)
            .Include(a => a.CVFiles)
            .Include(a => a.ScreeningResults)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        return applicants.Select(applicant => new ApplicantResponse
        {
            Id = applicant.Id,
            FirstName = applicant.FirstName,
            LastName = applicant.LastName,
            Email = applicant.Email,
            PhoneNumber = applicant.PhoneNumber,
            LinkedInProfile = applicant.LinkedInProfile,
            PortfolioUrl = applicant.PortfolioUrl,
            CoverLetter = applicant.CoverLetter,
            Status = applicant.Status,
            AppliedDate = applicant.AppliedDate,
            LastUpdated = applicant.LastUpdated,
            JobPostId = applicant.JobPostId,
            JobTitle = applicant.JobPost.Title,
            CVFiles = applicant.CVFiles.Select(cv => new CVFileResponse
            {
                Id = cv.Id,
                FileName = cv.FileName,
                ContentType = cv.ContentType,
                FileSize = cv.FileSize,
                FileExtension = cv.FileExtension,
                UploadedDate = cv.UploadedDate,
                Status = cv.Status
            }).ToList(),
            ScreeningResults = applicant.ScreeningResults.Select(sr => new ScreeningResultResponse
            {
                Id = sr.Id,
                OverallScore = sr.OverallScore,
                Summary = sr.Summary,
                Strengths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sr.Strengths) ?? new List<string>(),
                Weaknesses = System.Text.Json.JsonSerializer.Deserialize<List<string>>(sr.Weaknesses) ?? new List<string>(),
                DetailedAnalysis = sr.DetailedAnalysis,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                CompletedAt = sr.CompletedAt
            }).ToList()
        });
    }

    public async Task<ApplicantResponse> UpdateApplicantAsync(int id, UpdateApplicantRequest request)
    {
        var applicant = await _context.Applicants.FindAsync(id);
        if (applicant == null)
            throw new InvalidOperationException("Applicant not found.");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.FirstName))
            applicant.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            applicant.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Email))
            applicant.Email = request.Email;
        if (request.PhoneNumber != null)
            applicant.PhoneNumber = request.PhoneNumber;
        if (request.LinkedInProfile != null)
            applicant.LinkedInProfile = request.LinkedInProfile;
        if (request.PortfolioUrl != null)
            applicant.PortfolioUrl = request.PortfolioUrl;
        if (request.CoverLetter != null)
            applicant.CoverLetter = request.CoverLetter;
        if (!string.IsNullOrEmpty(request.Status))
            applicant.Status = request.Status;

        applicant.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetApplicantByIdAsync(id);
    }

    public async Task<bool> DeleteApplicantAsync(int id)
    {
        var applicant = await _context.Applicants.FindAsync(id);
        if (applicant == null)
            return false;

        _context.Applicants.Remove(applicant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request, string userId)
    {
        // Verify that the job post belongs to the user
        var jobPost = await _context.JobPosts
            .FirstOrDefaultAsync(jp => jp.Id == jobPostId && jp.UserId == userId);

        if (jobPost == null)
            throw new UnauthorizedAccessException("You don't have access to this job post.");

        // Verify that all applicants belong to this job post
        var applicants = await _context.Applicants
            .Where(a => request.ApplicantIds.Contains(a.Id) && a.JobPostId == jobPostId)
            .ToListAsync();

        if (applicants.Count != request.ApplicantIds.Count)
            throw new InvalidOperationException("Some applicants don't belong to this job post.");

        // Start screening for each applicant
        foreach (var applicant in applicants)
        {
            await _screeningService.ProcessScreeningAsync(applicant.Id, jobPostId);
        }

        return true;
    }
}

