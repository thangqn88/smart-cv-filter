namespace SmartCVFilter.API.Services.Interfaces;

public interface IGeminiAIService
{
    Task<string> AnalyzeCVAsync(string cvText, string jobDescription, string requiredSkills);
    Task<ScreeningAnalysisResult> PerformScreeningAsync(int applicantId, int jobPostId);
}

public class ScreeningAnalysisResult
{
    public int OverallScore { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string DetailedAnalysis { get; set; } = string.Empty;
}

