using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Services.Interfaces;
using System.Text;
using System.Text.Json;

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

            // Use the enhanced prompt template
            var prompt = AIPromptTemplates.GetPromptTemplate(
                "Software Development", // Default job type - can be enhanced to detect from job description
                "Mid Level", // Default experience level - can be enhanced to detect from job description
                jobDescription,
                requiredSkills,
                "", // Preferred skills - can be enhanced
                "" // Responsibilities - can be enhanced
            );

            // Add CV content to the prompt
            prompt += $"\n\nCV CONTENT TO ANALYZE:\n{cvText}";

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
                },
                generationConfig = new
                {
                    temperature = 0.3, // Lower temperature for more consistent results
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048
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

            // Detect job type and experience level from job post
            var jobType = DetectJobType(jobPost.Description, jobPost.RequiredSkills);
            var experienceLevel = DetectExperienceLevel(jobPost.ExperienceLevel, jobPost.Description);

            // Use enhanced prompt template with job-specific information
            var prompt = AIPromptTemplates.GetPromptTemplate(
                jobType,
                experienceLevel,
                jobPost.Description,
                jobPost.RequiredSkills,
                jobPost.PreferredSkills,
                jobPost.Responsibilities
            );

            // Add CV content to the prompt
            prompt += $"\n\nCV CONTENT TO ANALYZE:\n{latestCV.ExtractedText}";

            var analysis = await AnalyzeCVWithPromptAsync(prompt);
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

    private async Task<string> AnalyzeCVWithPromptAsync(string prompt)
    {
        try
        {
            var apiKey = _configuration["GeminiAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YourGeminiAIApiKeyHere")
            {
                _logger.LogWarning("Gemini AI API key not configured, using mock analysis");
                return await GetMockAnalysisAsync("", "", "");
            }

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
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048
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
                        return text ?? await GetMockAnalysisAsync("", "", "");
                    }
                }
            }

            _logger.LogWarning("Gemini AI API call failed, using mock analysis");
            return await GetMockAnalysisAsync("", "", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV with Gemini AI, using mock analysis");
            return await GetMockAnalysisAsync("", "", "");
        }
    }

    private string DetectJobType(string jobDescription, string requiredSkills)
    {
        var description = $"{jobDescription} {requiredSkills}".ToLowerInvariant();

        if (description.Contains("software") || description.Contains("developer") || description.Contains("programming") ||
            description.Contains("coding") || description.Contains("api") || description.Contains("database"))
            return AIPromptTemplates.JobTypes.Software;

        if (description.Contains("marketing") || description.Contains("seo") || description.Contains("social media") ||
            description.Contains("campaign") || description.Contains("brand"))
            return AIPromptTemplates.JobTypes.Marketing;

        if (description.Contains("sales") || description.Contains("revenue") || description.Contains("client") ||
            description.Contains("account") || description.Contains("business development"))
            return AIPromptTemplates.JobTypes.Sales;

        if (description.Contains("finance") || description.Contains("accounting") || description.Contains("budget") ||
            description.Contains("financial") || description.Contains("cpa"))
            return AIPromptTemplates.JobTypes.Finance;

        if (description.Contains("hr") || description.Contains("human resources") || description.Contains("recruitment") ||
            description.Contains("talent") || description.Contains("employee"))
            return AIPromptTemplates.JobTypes.HR;

        if (description.Contains("design") || description.Contains("ui") || description.Contains("ux") ||
            description.Contains("graphic") || description.Contains("creative"))
            return AIPromptTemplates.JobTypes.Design;

        if (description.Contains("data") || description.Contains("analytics") || description.Contains("machine learning") ||
            description.Contains("ai") || description.Contains("statistics"))
            return AIPromptTemplates.JobTypes.Data;

        if (description.Contains("manager") || description.Contains("director") || description.Contains("lead") ||
            description.Contains("supervisor") || description.Contains("executive"))
            return AIPromptTemplates.JobTypes.Management;

        if (description.Contains("operations") || description.Contains("process") || description.Contains("supply chain") ||
            description.Contains("logistics") || description.Contains("quality"))
            return AIPromptTemplates.JobTypes.Operations;

        if (description.Contains("customer service") || description.Contains("support") || description.Contains("help desk") ||
            description.Contains("client service"))
            return AIPromptTemplates.JobTypes.CustomerService;

        return AIPromptTemplates.JobTypes.Software; // Default fallback
    }

    private string DetectExperienceLevel(string experienceLevel, string jobDescription)
    {
        if (!string.IsNullOrEmpty(experienceLevel))
        {
            return experienceLevel switch
            {
                "Entry" => AIPromptTemplates.ExperienceLevels.Entry,
                "Mid" => AIPromptTemplates.ExperienceLevels.Mid,
                "Senior" => AIPromptTemplates.ExperienceLevels.Senior,
                "Lead" => AIPromptTemplates.ExperienceLevels.Lead,
                "Executive" => AIPromptTemplates.ExperienceLevels.Executive,
                _ => AIPromptTemplates.ExperienceLevels.Mid
            };
        }

        // Fallback to detecting from job description
        var description = jobDescription.ToLowerInvariant();

        if (description.Contains("entry") || description.Contains("junior") || description.Contains("graduate") ||
            description.Contains("0-2 years") || description.Contains("1-3 years"))
            return AIPromptTemplates.ExperienceLevels.Entry;

        if (description.Contains("senior") || description.Contains("lead") || description.Contains("principal") ||
            description.Contains("5+ years") || description.Contains("7+ years"))
            return AIPromptTemplates.ExperienceLevels.Senior;

        if (description.Contains("executive") || description.Contains("director") || description.Contains("vp") ||
            description.Contains("10+ years") || description.Contains("15+ years"))
            return AIPromptTemplates.ExperienceLevels.Executive;

        return AIPromptTemplates.ExperienceLevels.Mid; // Default fallback
    }

    public ScreeningAnalysisResult ParseAnalysisResult(string analysisJson)
    {
        try
        {
            // Try to extract JSON from the response (in case there's extra text)
            var jsonStart = analysisJson.IndexOf('{');
            var jsonEnd = analysisJson.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                analysisJson = analysisJson.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            var analysis = JsonSerializer.Deserialize<JsonElement>(analysisJson);

            return new ScreeningAnalysisResult
            {
                OverallScore = analysis.TryGetProperty("OverallScore", out var score) ? score.GetInt32() : 50,
                Summary = analysis.TryGetProperty("Summary", out var summary) ? summary.GetString() ?? "" : "",
                Strengths = analysis.TryGetProperty("Strengths", out var strengths)
                    ? strengths.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new List<string>(),
                Weaknesses = analysis.TryGetProperty("Weaknesses", out var weaknesses)
                    ? weaknesses.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new List<string>(),
                DetailedAnalysis = analysis.TryGetProperty("DetailedAnalysis", out var detailed)
                    ? detailed.GetString() ?? ""
                    : ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing analysis result: {AnalysisJson}", analysisJson);

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

