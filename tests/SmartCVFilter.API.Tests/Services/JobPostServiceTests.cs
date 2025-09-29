using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Mapping;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Tests.TestBase;
using Xunit;

namespace SmartCVFilter.API.Tests.Services;

public class JobPostServiceTests : TestBaseClass, IDisposable
{
    private JobPostService _jobPostService = null!;
    private IMapper _mapper = null!;

    public JobPostServiceTests()
    {
        Setup();
        SetupMapper();
        _jobPostService = new JobPostService(Context, _mapper);
    }

    private void SetupMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateJobPostAsync_WithValidData_ShouldReturnJobPostResponse()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var request = new CreateJobPostRequest
        {
            Title = "Test Job",
            Description = "Test Description",
            Location = "Test Location",
            Department = "Test Department",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid",
            RequiredSkills = "C#, .NET",
            PreferredSkills = "Vue.js",
            Responsibilities = "Test responsibilities",
            Benefits = "Test benefits",
            SalaryMin = 50000,
            SalaryMax = 70000
        };

        // Act
        var result = await _jobPostService.CreateJobPostAsync(request, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Location.Should().Be(request.Location);
        result.Department.Should().Be(request.Department);
        result.EmploymentType.Should().Be(request.EmploymentType);
        result.ExperienceLevel.Should().Be(request.ExperienceLevel);
        result.RequiredSkills.Should().Be(request.RequiredSkills);
        result.PreferredSkills.Should().Be(request.PreferredSkills);
        result.Responsibilities.Should().Be(request.Responsibilities);
        result.Benefits.Should().Be(request.Benefits);
        result.SalaryMin.Should().Be(request.SalaryMin);
        result.SalaryMax.Should().Be(request.SalaryMax);
        result.Status.Should().Be("Active");
        result.PostedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.ApplicantCount.Should().Be(0);
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetJobPostByIdAsync_WithValidId_ShouldReturnJobPostResponse()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);

        // Act
        var result = await _jobPostService.GetJobPostByIdAsync(jobPost.Id, user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(jobPost.Id);
        result.Title.Should().Be(jobPost.Title);
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetJobPostByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var invalidId = 999;

        // Act
        var result = await _jobPostService.GetJobPostByIdAsync(invalidId, user.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetJobPostByIdAsync_WithWrongUser_ShouldReturnNull()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("user1@example.com");
        var user2 = await CreateTestUserAsync("user2@example.com");
        var jobPost = await CreateTestJobPostAsync(user1.Id);

        // Act
        var result = await _jobPostService.GetJobPostByIdAsync(jobPost.Id, user2.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetJobPostsByUserAsync_WithValidUser_ShouldReturnJobPosts()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost1 = await CreateTestJobPostAsync(user.Id);
        var jobPost2 = await CreateTestJobPostAsync(user.Id);
        jobPost2.Title = "Second Job";
        await Context.SaveChangesAsync();

        // Act
        var result = await _jobPostService.GetJobPostsByUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(jp => jp.Id == jobPost1.Id);
        result.Should().Contain(jp => jp.Id == jobPost2.Id);
    }

    [Fact]
    public async Task GetJobPostsByUserAsync_WithNoJobPosts_ShouldReturnEmptyList()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        // Act
        var result = await _jobPostService.GetJobPostsByUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateJobPostAsync_WithValidData_ShouldReturnUpdatedJobPost()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);
        var request = new UpdateJobPostRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Location = "Updated Location",
            Status = "Inactive"
        };

        // Act
        var result = await _jobPostService.UpdateJobPostAsync(jobPost.Id, request, user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Location.Should().Be(request.Location);
        result.Status.Should().Be(request.Status);
    }

    [Fact]
    public async Task UpdateJobPostAsync_WithInvalidId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var request = new UpdateJobPostRequest
        {
            Title = "Updated Title"
        };

        // Act & Assert
        await _jobPostService.Invoking(s => s.UpdateJobPostAsync(999, request, user.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Job post not found.");
    }

    [Fact]
    public async Task DeleteJobPostAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var jobPost = await CreateTestJobPostAsync(user.Id);

        // Act
        var result = await _jobPostService.DeleteJobPostAsync(jobPost.Id, user.Id);

        // Assert
        result.Should().BeTrue();
        var deletedJobPost = await Context.JobPosts.FindAsync(jobPost.Id);
        deletedJobPost.Should().BeNull();
    }

    [Fact]
    public async Task DeleteJobPostAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        // Act
        var result = await _jobPostService.DeleteJobPostAsync(999, user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllJobPostsAsync_ShouldReturnActiveJobPosts()
    {
        // Arrange
        var user1 = await CreateTestUserAsync("user1@example.com");
        var user2 = await CreateTestUserAsync("user2@example.com");
        var activeJobPost = await CreateTestJobPostAsync(user1.Id);
        var inactiveJobPost = await CreateTestJobPostAsync(user2.Id);
        inactiveJobPost.Status = "Inactive";
        await Context.SaveChangesAsync();

        // Act
        var result = await _jobPostService.GetAllJobPostsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(jp => jp.Id == activeJobPost.Id);
        result.Should().NotContain(jp => jp.Id == inactiveJobPost.Id);
    }

    public void Dispose()
    {
        Cleanup();
    }
}
