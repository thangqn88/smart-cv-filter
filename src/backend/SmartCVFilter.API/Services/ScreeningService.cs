using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class ScreeningService : IScreeningService
{
    private readonly ApplicationDbContext _context;
    private readonly IGeminiAIService _geminiAIService;
    private readonly ILogger<ScreeningService> _logger;

    public ScreeningService(
        ApplicationDbContext context,
        IGeminiAIService geminiAIService,
        ILogger<ScreeningService> logger)
    {
        _context = context;
        _geminiAIService = geminiAIService;
        _logger = logger;
    }

    public async Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId, string userId, bool isAdmin = false)
    {
        var query = _context.ScreeningResults
            .Include(sr => sr.Applicant)
            .Include(sr => sr.JobPost)
            .AsQueryable();

        // Admin users can access any screening result, regular users can only access results from their job posts
        if (!isAdmin)
        {
            query = query.Where(sr => sr.JobPost.UserId == userId);
        }

        var result = await query.FirstOrDefaultAsync(sr => sr.Id == resultId);

        if (result == null)
            return null;

        return new ScreeningResultResponse
        {
            Id = result.Id,
            OverallScore = result.OverallScore,
            Summary = result.Summary,
            Strengths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Strengths) ?? new List<string>(),
            Weaknesses = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Weaknesses) ?? new List<string>(),
            DetailedAnalysis = result.DetailedAnalysis,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            CompletedAt = result.CompletedAt
        };
    }

    public async Task<IEnumerable<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId, string userId, bool isAdmin = false)
    {
        var query = _context.ScreeningResults
            .Include(sr => sr.Applicant)
            .Include(sr => sr.JobPost)
            .Where(sr => sr.ApplicantId == applicantId)
            .AsQueryable();

        // Admin users can access any screening results, regular users can only access results from their job posts
        if (!isAdmin)
        {
            query = query.Where(sr => sr.JobPost.UserId == userId);
        }

        var results = await query
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();

        return results.Select(result => new ScreeningResultResponse
        {
            Id = result.Id,
            OverallScore = result.OverallScore,
            Summary = result.Summary,
            Strengths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Strengths) ?? new List<string>(),
            Weaknesses = System.Text.Json.JsonSerializer.Deserialize<List<string>>(result.Weaknesses) ?? new List<string>(),
            DetailedAnalysis = result.DetailedAnalysis,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            CompletedAt = result.CompletedAt
        });
    }

    public async Task<IEnumerable<ScreenedApplicantResponse>> GetScreenedApplicantsAsync(string userId, bool isAdmin = false)
    {
        var query = _context.Applicants
            .Include(a => a.JobPost)
            .Include(a => a.ScreeningResults)
            .Where(a => a.ScreeningResults.Any()) // Only applicants who have been screened
            .AsQueryable();

        // Admin users can access any screened applicants, regular users can only access their own job posts' applicants
        if (!isAdmin)
        {
            query = query.Where(a => a.JobPost.UserId == userId);
        }

        var applicants = await query
            .OrderByDescending(a => a.ScreeningResults.Max(sr => sr.CreatedAt))
            .ToListAsync();

        return applicants.Select(applicant =>
        {
            var latestScreening = applicant.ScreeningResults
                .OrderByDescending(sr => sr.CreatedAt)
                .First();

            return new ScreenedApplicantResponse
            {
                ApplicantId = applicant.Id,
                FirstName = applicant.FirstName,
                LastName = applicant.LastName,
                Email = applicant.Email,
                PhoneNumber = applicant.PhoneNumber,
                Status = applicant.Status,
                AppliedDate = applicant.AppliedDate,
                JobPostId = applicant.JobPostId,
                JobTitle = applicant.JobPost.Title,
                JobLocation = applicant.JobPost.Location,
                JobDepartment = applicant.JobPost.Department,
                LatestScore = latestScreening.OverallScore,
                LatestScoreStatus = latestScreening.Status,
                LatestScreeningDate = latestScreening.CreatedAt,
                TotalScreenings = applicant.ScreeningResults.Count
            };
        });
    }

    public async Task<bool> ProcessScreeningAsync(int applicantId, int jobPostId)
    {
        try
        {
            // Create a new screening result record
            var screeningResult = new ScreeningResult
            {
                ApplicantId = applicantId,
                JobPostId = jobPostId,
                Status = "Processing",
                CreatedAt = DateTime.UtcNow
            };

            _context.ScreeningResults.Add(screeningResult);
            await _context.SaveChangesAsync();

            // Start the AI screening process in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    var analysis = await _geminiAIService.PerformScreeningAsync(applicantId, jobPostId);

                    // Update the screening result with the analysis
                    screeningResult.OverallScore = analysis.OverallScore;
                    screeningResult.Summary = analysis.Summary;
                    screeningResult.Strengths = System.Text.Json.JsonSerializer.Serialize(analysis.Strengths);
                    screeningResult.Weaknesses = System.Text.Json.JsonSerializer.Serialize(analysis.Weaknesses);
                    screeningResult.DetailedAnalysis = analysis.DetailedAnalysis;
                    screeningResult.Status = "Completed";
                    screeningResult.CompletedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Update applicant status
                    var applicant = await _context.Applicants.FindAsync(applicantId);
                    if (applicant != null)
                    {
                        applicant.Status = "Screened";
                        applicant.LastUpdated = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing screening for applicant {ApplicantId}", applicantId);

                    screeningResult.Status = "Failed";
                    screeningResult.ErrorMessage = ex.Message;
                    await _context.SaveChangesAsync();
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting screening process for applicant {ApplicantId}", applicantId);
            return false;
        }
    }

    public async Task UpdateScreeningStatusAsync(int resultId, string status, string? errorMessage = null)
    {
        var result = await _context.ScreeningResults.FindAsync(resultId);
        if (result == null)
            return;

        result.Status = status;
        if (status == "Completed")
            result.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(errorMessage))
            result.ErrorMessage = errorMessage;

        await _context.SaveChangesAsync();
    }
}

