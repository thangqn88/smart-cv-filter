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

    public async Task<ScreeningResultResponse?> GetScreeningResultAsync(int resultId)
    {
        var result = await _context.ScreeningResults
            .Include(sr => sr.Applicant)
            .Include(sr => sr.JobPost)
            .FirstOrDefaultAsync(sr => sr.Id == resultId);

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

    public async Task<IEnumerable<ScreeningResultResponse>> GetScreeningResultsByApplicantAsync(int applicantId)
    {
        var results = await _context.ScreeningResults
            .Where(sr => sr.ApplicantId == applicantId)
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

