using Moq;
using SmartCVFilter.API.Controllers;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Services.Interfaces;
using Xunit;

namespace SmartCVFilter.API.Tests;

public class AIProcessingTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<GeminiAIService>> _mockLogger;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<IScreeningService> _mockScreeningService;

    public AIProcessingTests()
    {
        _mockContext = new Mock<ApplicationDbContext>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<GeminiAIService>>();
        _mockHttpClient = new Mock<HttpClient>();
        _mockScreeningService = new Mock<IScreeningService>();
    }

    [Fact]
    public void AIPromptTemplates_ShouldGenerateValidPrompt()
    {
        // Arrange
        var jobType = "Software Development";
        var experienceLevel = "Mid Level";
        var jobDescription = "We are looking for a software developer with experience in C# and .NET";
        var requiredSkills = "C#, .NET, SQL Server, Entity Framework";
        var preferredSkills = "Azure, Docker, Microservices";
        var responsibilities = "Develop web applications, maintain existing code, collaborate with team";

        // Act
        var prompt = AIPromptTemplates.GetPromptTemplate(
            jobType,
            experienceLevel,
            jobDescription,
            requiredSkills,
            preferredSkills,
            responsibilities
        );

        // Assert
        Assert.NotNull(prompt);
        Assert.Contains("Software Development", prompt);
        Assert.Contains("Mid Level", prompt);
        Assert.Contains(jobDescription, prompt);
        Assert.Contains(requiredSkills, prompt);
        Assert.Contains("JSON format", prompt);
    }

    [Fact]
    public void AIPromptTemplates_ShouldDetectJobTypes()
    {
        // Test Software Development detection
        var jobType = AIPromptTemplates.JobTypes.Software;
        Assert.Equal("Software Development", jobType);

        // Test Marketing detection
        var marketingJobType = AIPromptTemplates.JobTypes.Marketing;
        Assert.Equal("Marketing", marketingJobType);
    }

    [Fact]
    public void AIPromptTemplates_ShouldDetectExperienceLevels()
    {
        // Test Entry Level
        var entryLevel = AIPromptTemplates.ExperienceLevels.Entry;
        Assert.Equal("Entry Level", entryLevel);

        // Test Senior Level
        var seniorLevel = AIPromptTemplates.ExperienceLevels.Senior;
        Assert.Equal("Senior Level", seniorLevel);
    }

    [Fact]
    public void GeminiAIService_ShouldParseAnalysisResult()
    {
        // Arrange
        var analysisJson = """
        {
            "OverallScore": 85,
            "Summary": "Strong candidate with relevant experience",
            "Strengths": ["Strong technical background", "Relevant work experience"],
            "Weaknesses": ["Limited leadership experience"],
            "DetailedAnalysis": "The candidate shows strong technical skills..."
        }
        """;

        var service = new GeminiAIService(
            _mockContext.Object,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockHttpClient.Object
        );

        // Act
        ScreeningAnalysisResult result = service.ParseAnalysisResult(analysisJson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(85, result.OverallScore);
        Assert.Equal("Strong candidate with relevant experience", result.Summary);
        Assert.Equal(2, result.Strengths.Count);
        Assert.Equal(1, result.Weaknesses.Count);
        Assert.Contains("Strong technical background", result.Strengths);
        Assert.Contains("Limited leadership experience", result.Weaknesses);
    }

    [Fact]
    public void GeminiAIService_ShouldHandleInvalidJson()
    {
        // Arrange
        var invalidJson = "Invalid JSON response";
        var service = new GeminiAIService(
            _mockContext.Object,
            _mockConfiguration.Object,
            _mockLogger.Object,
            _mockHttpClient.Object
        );

        // Act
        var result = service.ParseAnalysisResult(invalidJson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.OverallScore);
        Assert.Equal("Analysis could not be completed due to processing error.", result.Summary);
        Assert.Contains("Unable to determine", result.Strengths);
        Assert.Contains("Analysis failed", result.Weaknesses);
    }

    [Fact]
    public void AIProcessingStatusResponse_ShouldHaveCorrectProperties()
    {
        // Arrange
        var status = new AIProcessingStatusResponse
        {
            ApplicantId = 1,
            ApplicantName = "John Doe",
            JobPostId = 1,
            JobTitle = "Software Developer",
            CVProcessingStatus = "Processed",
            CVProcessingProgress = 100,
            ScreeningStatus = "Completed",
            ScreeningProgress = 100,
            LastUpdated = DateTime.UtcNow,
            OverallProgress = 100
        };

        // Assert
        Assert.Equal(1, status.ApplicantId);
        Assert.Equal("John Doe", status.ApplicantName);
        Assert.Equal(1, status.JobPostId);
        Assert.Equal("Software Developer", status.JobTitle);
        Assert.Equal("Processed", status.CVProcessingStatus);
        Assert.Equal(100, status.CVProcessingProgress);
        Assert.Equal("Completed", status.ScreeningStatus);
        Assert.Equal(100, status.ScreeningProgress);
        Assert.Equal(100, status.OverallProgress);
    }
}
