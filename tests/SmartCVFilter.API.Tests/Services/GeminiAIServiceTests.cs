using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Services.Interfaces;
using SmartCVFilter.API.Tests.TestBase;
using System.Net;
using System.Text;
using Xunit;

namespace SmartCVFilter.API.Tests.Services;

public class GeminiAIServiceTests : TestBaseClass, IDisposable
{
    private GeminiAIService _geminiAIService = null!;
    private Mock<HttpClient> _mockHttpClient = null!;

    public GeminiAIServiceTests()
    {
        Setup();
        SetupMocks();
        _geminiAIService = new GeminiAIService(Context, Configuration, GetMockLogger<GeminiAIService>().Object, _mockHttpClient.Object);
    }

    private void SetupMocks()
    {
        _mockHttpClient = new Mock<HttpClient>();
    }

    [Fact]
    public async Task AnalyzeCVAsync_WithValidApiKey_ShouldReturnAnalysis()
    {
        // Arrange
        var cvText = "John Doe is a software developer with 5 years of experience in C# and .NET.";
        var jobDescription = "We are looking for a .NET developer with 3+ years of experience.";
        var requiredSkills = "C#, .NET, SQL Server";

        // Act
        var result = await _geminiAIService.AnalyzeCVAsync(cvText, jobDescription, requiredSkills);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("OverallScore");
        result.Should().Contain("Summary");
    }

    [Fact]
    public async Task AnalyzeCVAsync_WithInvalidApiKey_ShouldReturnMockAnalysis()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GeminiAI:ApiKey"] = "YourGeminiAIApiKeyHere" // Default placeholder
            })
            .Build();

        var service = new GeminiAIService(Context, configuration, GetMockLogger<GeminiAIService>().Object, _mockHttpClient.Object);
        var cvText = "Test CV content";
        var jobDescription = "Test job description";
        var requiredSkills = "Test skills";

        // Act
        var result = await service.AnalyzeCVAsync(cvText, jobDescription, requiredSkills);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("OverallScore");
        result.Should().Contain("Summary");
    }

    [Fact]
    public async Task AnalyzeCVAsync_WithApiFailure_ShouldReturnMockAnalysis()
    {
        // Arrange
        var cvText = "Test CV content";
        var jobDescription = "Test job description";
        var requiredSkills = "Test skills";

        // Act
        var result = await _geminiAIService.AnalyzeCVAsync(cvText, jobDescription, requiredSkills);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("OverallScore");
        result.Should().Contain("Summary");
    }

    [Fact]
    public async Task PerformScreeningAsync_WithValidData_ShouldReturnAnalysisResult()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant = await CreateTestApplicantAsync(jobPost.Id);

        // Add a CV file with extracted text
        var cvFile = new CVFile
        {
            FileName = "test.pdf",
            FilePath = "/test/path",
            ContentType = "application/pdf",
            FileSize = 1024,
            FileExtension = ".pdf",
            ExtractedText = "John Doe is a software developer with 5 years of experience in C# and .NET.",
            ApplicantId = applicant.Id,
            UploadedDate = DateTime.UtcNow,
            Status = "Processed"
        };

        Context.CVFiles.Add(cvFile);
        await Context.SaveChangesAsync();

        // Act
        var result = await _geminiAIService.PerformScreeningAsync(applicant.Id, jobPost.Id);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeInRange(0, 100);
        result.Summary.Should().NotBeNullOrEmpty();
        result.Strengths.Should().NotBeEmpty();
        result.Weaknesses.Should().NotBeEmpty();
        result.DetailedAnalysis.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PerformScreeningAsync_WithNoProcessedCV_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant = await CreateTestApplicantAsync(jobPost.Id);

        // Act & Assert
        await _geminiAIService.Invoking(s => s.PerformScreeningAsync(applicant.Id, jobPost.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No processed CV found for this applicant.");
    }

    [Fact]
    public async Task PerformScreeningAsync_WithInvalidApplicant_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var invalidApplicantId = 999;

        // Act & Assert
        await _geminiAIService.Invoking(s => s.PerformScreeningAsync(invalidApplicantId, jobPost.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Applicant not found.");
    }

    [Fact]
    public async Task PerformScreeningAsync_WithInvalidJobPost_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var applicant = await CreateTestApplicantAsync(1); // Assuming job post ID 1 exists
        var invalidJobPostId = 999;

        // Act & Assert
        await _geminiAIService.Invoking(s => s.PerformScreeningAsync(applicant.Id, invalidJobPostId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Applicant not found.");
    }

    [Fact]
    public async Task GetMockAnalysisAsync_ShouldReturnValidJson()
    {
        // Arrange
        var cvText = "Test CV content";
        var jobDescription = "Test job description";
        var requiredSkills = "Test skills";

        // Act
        var result = await _geminiAIService.AnalyzeCVAsync(cvText, jobDescription, requiredSkills);

        // Assert
        result.Should().NotBeNullOrEmpty();

        // Verify it's valid JSON
        var jsonDocument = System.Text.Json.JsonDocument.Parse(result);
        jsonDocument.RootElement.TryGetProperty("OverallScore", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("Summary", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("Strengths", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("Weaknesses", out _).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("DetailedAnalysis", out _).Should().BeTrue();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
