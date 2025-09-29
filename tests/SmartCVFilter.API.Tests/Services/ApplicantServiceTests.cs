using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Mapping;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Services.Interfaces;
using SmartCVFilter.API.Tests.TestBase;
using Xunit;

namespace SmartCVFilter.API.Tests.Services;

public class ApplicantServiceTests : TestBaseClass, IDisposable
{
    private ApplicantService _applicantService = null!;
    private Mock<IScreeningService> _mockScreeningService = null!;
    private IMapper _mapper = null!;

    public ApplicantServiceTests()
    {
        Setup();
        SetupMocks();
        SetupMapper();
        _applicantService = new ApplicantService(Context, _mockScreeningService.Object);
    }

    private void SetupMocks()
    {
        _mockScreeningService = new Mock<IScreeningService>();
    }

    private void SetupMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateApplicantAsync_WithValidData_ShouldReturnApplicantResponse()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var request = new CreateApplicantRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "1234567890",
            LinkedInProfile = "https://linkedin.com/in/johndoe",
            PortfolioUrl = "https://johndoe.com",
            CoverLetter = "I am interested in this position."
        };

        // Act
        var result = await _applicantService.CreateApplicantAsync(request, jobPost.Id);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.LinkedInProfile.Should().Be(request.LinkedInProfile);
        result.PortfolioUrl.Should().Be(request.PortfolioUrl);
        result.CoverLetter.Should().Be(request.CoverLetter);
        result.Status.Should().Be("Applied");
        result.JobPostId.Should().Be(jobPost.Id);
        result.JobTitle.Should().Be(jobPost.Title);
        result.AppliedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateApplicantAsync_WithInvalidJobPostId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new CreateApplicantRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act & Assert
        await _applicantService.Invoking(s => s.CreateApplicantAsync(request, 999))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Job post not found.");
    }

    [Fact]
    public async Task GetApplicantByIdAsync_WithValidId_ShouldReturnApplicantResponse()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant = await CreateTestApplicantAsync(jobPost.Id);

        // Act
        var result = await _applicantService.GetApplicantByIdAsync(applicant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(applicant.Id);
        result.FirstName.Should().Be(applicant.FirstName);
        result.LastName.Should().Be(applicant.LastName);
        result.Email.Should().Be(applicant.Email);
        result.JobPostId.Should().Be(jobPost.Id);
        result.JobTitle.Should().Be(jobPost.Title);
    }

    [Fact]
    public async Task GetApplicantByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _applicantService.GetApplicantByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicantsByJobPostAsync_WithValidJobPost_ShouldReturnApplicants()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant1 = await CreateTestApplicantAsync(jobPost.Id);
        var applicant2 = await CreateTestApplicantAsync(jobPost.Id);
        applicant2.FirstName = "Jane";
        applicant2.Email = "jane.doe@example.com";
        await Context.SaveChangesAsync();

        // Act
        var result = await _applicantService.GetApplicantsByJobPostAsync(jobPost.Id, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == applicant1.Id);
        result.Should().Contain(a => a.Id == applicant2.Id);
    }

    [Fact]
    public async Task GetApplicantsByJobPostAsync_WithInvalidJobPost_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("user1@example.com");
        var user2 = await CreateTestUserAsync("user2@example.com");
        var jobPost = await CreateTestJobPostAsync(user1.Id);

        // Act & Assert
        await _applicantService.Invoking(s => s.GetApplicantsByJobPostAsync(jobPost.Id, user2.Id))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this job post.");
    }

    [Fact]
    public async Task UpdateApplicantAsync_WithValidData_ShouldReturnUpdatedApplicant()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant = await CreateTestApplicantAsync(jobPost.Id);
        var request = new UpdateApplicantRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Status = "Under Review"
        };

        // Act
        var result = await _applicantService.UpdateApplicantAsync(applicant.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.Status.Should().Be(request.Status);
        result.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateApplicantAsync_WithInvalidId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new UpdateApplicantRequest
        {
            FirstName = "Updated"
        };

        // Act & Assert
        await _applicantService.Invoking(s => s.UpdateApplicantAsync(999, request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Applicant not found.");
    }

    [Fact]
    public async Task DeleteApplicantAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant = await CreateTestApplicantAsync(jobPost.Id);

        // Act
        var result = await _applicantService.DeleteApplicantAsync(applicant.Id);

        // Assert
        result.Should().BeTrue();
        var deletedApplicant = await Context.Applicants.FindAsync(applicant.Id);
        deletedApplicant.Should().BeNull();
    }

    [Fact]
    public async Task DeleteApplicantAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _applicantService.DeleteApplicantAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task StartScreeningAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var applicant1 = await CreateTestApplicantAsync(jobPost.Id);
        var applicant2 = await CreateTestApplicantAsync(jobPost.Id);
        var request = new ScreeningRequest
        {
            ApplicantIds = new List<int> { applicant1.Id, applicant2.Id }
        };

        _mockScreeningService
            .Setup(x => x.ProcessScreeningAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _applicantService.StartScreeningAsync(jobPost.Id, request, user.Id);

        // Assert
        result.Should().BeTrue();
        _mockScreeningService.Verify(x => x.ProcessScreeningAsync(applicant1.Id, jobPost.Id), Times.Once);
        _mockScreeningService.Verify(x => x.ProcessScreeningAsync(applicant2.Id, jobPost.Id), Times.Once);
    }

    [Fact]
    public async Task StartScreeningAsync_WithInvalidJobPost_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("user1@example.com");
        var user2 = await CreateTestUserAsync("user2@example.com");
        var jobPost = await CreateTestJobPostAsync(user1.Id);
        var request = new ScreeningRequest
        {
            ApplicantIds = new List<int> { 1, 2 }
        };

        // Act & Assert
        await _applicantService.Invoking(s => s.StartScreeningAsync(jobPost.Id, request, user2.Id))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this job post.");
    }

    [Fact]
    public async Task StartScreeningAsync_WithInvalidApplicants_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var request = new ScreeningRequest
        {
            ApplicantIds = new List<int> { 999, 998 } // Non-existent applicants
        };

        // Act & Assert
        await _applicantService.Invoking(s => s.StartScreeningAsync(jobPost.Id, request, user.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Some applicants don't belong to this job post.");
    }

    public void Dispose()
    {
        Cleanup();
    }
}
