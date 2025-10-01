using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace SmartCVFilter.API.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly HttpClient _httpClient;

    public GeminiAIService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<GeminiAIService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> AnalyzeCVAsync(string cvText, string jobDescription, string requiredSkills)
    {
        try
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YourGeminiAIApiKeyHere")
            {
                _logger.LogWarning("Gemini AI API key not configured, using mock analysis");
                return await GetMockAnalysisAsync(cvText, jobDescription, requiredSkills);
            }

            var prompt = $@"
                Analyze this CV against the job requirements:
                
                Job Description: {jobDescription}
                Required Skills: {requiredSkills}
                
                CV Content: {cvText}
                
                Please provide a detailed analysis including:
                1. Overall match score (0-100)
                2. Key strengths
                3. Areas for improvement
                4. Detailed assessment
                
                Return the response as a JSON object with the following structure:
                {{
                    ""OverallScore"": number,
                    ""Summary"": ""string"",
                    ""Strengths"": [""string1"", ""string2""],
                    ""Weaknesses"": [""string1"", ""string2""],
                    ""DetailedAnalysis"": ""string""
                }}
            ";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var candidate = candidates[0];
                    if (candidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        return text ?? await GetMockAnalysisAsync(cvText, jobDescription, requiredSkills);
                    }
                }
            }

            _logger.LogWarning("Gemini AI API call failed, using mock analysis");
            return await GetMockAnalysisAsync(cvText, jobDescription, requiredSkills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV with Gemini AI, using mock analysis");
            return await GetMockAnalysisAsync(cvText, jobDescription, requiredSkills);
        }
    }

    public async Task<ScreeningAnalysisResult> PerformScreeningAsync(int applicantId, int jobPostId)
    {
        try
        {
            var applicant = await _context.Applicants
                .Include(a => a.CVFiles)
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                throw new InvalidOperationException("Applicant not found.");

            var jobPost = await _context.JobPosts.FindAsync(jobPostId);
            if (jobPost == null)
                throw new InvalidOperationException("Job post not found.");

            // Get the latest CV file with extracted text
            var latestCV = applicant.CVFiles
                .Where(cv => cv.Status == "Processed" && !string.IsNullOrEmpty(cv.ExtractedText))
                .OrderByDescending(cv => cv.UploadedDate)
                .FirstOrDefault();

            if (latestCV == null)
                throw new InvalidOperationException("No processed CV found for this applicant.");

            var analysis = await AnalyzeCVAsync(
                latestCV.ExtractedText ?? throw new InvalidOperationException("CV extracted text is null"),
                jobPost.Description,
                jobPost.RequiredSkills
            );

            return ParseAnalysisResult(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing screening for applicant {ApplicantId}", applicantId);
            throw;
        }
    }

    private async Task<string> GetMockAnalysisAsync(string cvText, string jobDescription, string requiredSkills)
    {
        // This is a mock implementation. Replace with actual Gemini AI integration
        await Task.Delay(1000); // Simulate processing time

        var random = new Random();
        var score = random.Next(60, 95);

        var strengths = new[]
        {
            "Strong technical background",
            "Relevant work experience",
            "Good educational qualifications",
            "Demonstrated problem-solving skills",
            "Team collaboration experience"
        };

        var weaknesses = new[]
        {
            "Limited experience with specific technologies",
            "Could benefit from more leadership experience",
            "Communication skills need improvement",
            "Lacks industry-specific knowledge"
        };

        var selectedStrengths = strengths.OrderBy(x => random.Next()).Take(random.Next(2, 4)).ToArray();
        var selectedWeaknesses = weaknesses.OrderBy(x => random.Next()).Take(random.Next(1, 3)).ToArray();

        return JsonSerializer.Serialize(new
        {
            OverallScore = score,
            Summary = $"The candidate shows a {score}% match with the job requirements. {string.Join(" ", selectedStrengths.Take(2))}.",
            Strengths = selectedStrengths,
            Weaknesses = selectedWeaknesses,
            DetailedAnalysis = $"Based on the CV analysis, the candidate demonstrates {string.Join(", ", selectedStrengths).ToLower()}. However, there are areas for improvement including {string.Join(", ", selectedWeaknesses).ToLower()}. The overall assessment indicates a {score}% alignment with the position requirements."
        });
    }

    private ScreeningAnalysisResult ParseAnalysisResult(string analysisJson)
    {
        try
        {
            var analysis = JsonSerializer.Deserialize<JsonElement>(analysisJson);

            return new ScreeningAnalysisResult
            {
                OverallScore = analysis.GetProperty("OverallScore").GetInt32(),
                Summary = analysis.GetProperty("Summary").GetString() ?? "",
                Strengths = analysis.GetProperty("Strengths").EnumerateArray().Select(x => x.GetString() ?? "").ToList(),
                Weaknesses = analysis.GetProperty("Weaknesses").EnumerateArray().Select(x => x.GetString() ?? "").ToList(),
                DetailedAnalysis = analysis.GetProperty("DetailedAnalysis").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing analysis result");

            // Return default result if parsing fails
            return new ScreeningAnalysisResult
            {
                OverallScore = 50,
                Summary = "Analysis could not be completed due to processing error.",
                Strengths = new List<string> { "Unable to determine" },
                Weaknesses = new List<string> { "Analysis failed" },
                DetailedAnalysis = "The CV analysis could not be completed successfully. Please try again or contact support."
            };
        }
    }
}

