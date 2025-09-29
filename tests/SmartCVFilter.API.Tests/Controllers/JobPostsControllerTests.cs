using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Controllers;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;
using SmartCVFilter.API.Tests.TestBase;
using Xunit;
using System.Security.Claims;

namespace SmartCVFilter.API.Tests.Controllers;

public class JobPostsControllerTests : TestBaseClass, IDisposable
{
    private JobPostsController _controller = null!;
    private Mock<IJobPostService> _mockJobPostService = null!;
    private Mock<ILogger<JobPostsController>> _mockLogger = null!;

    public JobPostsControllerTests()
    {
        Setup();
        SetupMocks();
        _controller = new JobPostsController(_mockJobPostService.Object, _mockLogger.Object);
    }

    private void SetupMocks()
    {
        _mockJobPostService = new Mock<IJobPostService>();
        _mockLogger = new Mock<ILogger<JobPostsController>>();
    }

    private void SetupControllerContext(string userId = "test-user-id")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetJobPosts_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        SetupControllerContext();
        var jobPosts = new List<JobPostListResponse>
        {
            new() { Id = 1, Title = "Test Job 1", Location = "Location 1" },
            new() { Id = 2, Title = "Test Job 2", Location = "Location 2" }
        };

        _mockJobPostService
            .Setup(x => x.GetJobPostsByUserAsync("test-user-id"))
            .ReturnsAsync(jobPosts);

        // Act
        var result = await _controller.GetJobPosts();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(jobPosts);
    }

    [Fact]
    public async Task GetJobPosts_WithServerError_ShouldReturnInternalServerError()
    {
        // Arrange
        SetupControllerContext();
        _mockJobPostService
            .Setup(x => x.GetJobPostsByUserAsync("test-user-id"))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetJobPosts();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAllJobPosts_ShouldReturnOkResult()
    {
        // Arrange
        var jobPosts = new List<JobPostListResponse>
        {
            new() { Id = 1, Title = "Public Job 1", Location = "Location 1" },
            new() { Id = 2, Title = "Public Job 2", Location = "Location 2" }
        };

        _mockJobPostService
            .Setup(x => x.GetAllJobPostsAsync())
            .ReturnsAsync(jobPosts);

        // Act
        var result = await _controller.GetAllJobPosts();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(jobPosts);
    }

    [Fact]
    public async Task GetJobPost_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        SetupControllerContext();
        var jobPost = new JobPostResponse
        {
            Id = 1,
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
            SalaryMax = 70000,
            Status = "Active",
            PostedDate = DateTime.UtcNow,
            ApplicantCount = 0,
            User = new UserInfo
            {
                Id = "test-user-id",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CompanyName = "Test Company"
            }
        };

        _mockJobPostService
            .Setup(x => x.GetJobPostByIdAsync(1, "test-user-id"))
            .ReturnsAsync(jobPost);

        // Act
        var result = await _controller.GetJobPost(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(jobPost);
    }

    [Fact]
    public async Task GetJobPost_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupControllerContext();
        _mockJobPostService
            .Setup(x => x.GetJobPostByIdAsync(999, "test-user-id"))
            .ReturnsAsync((JobPostResponse?)null);

        // Act
        var result = await _controller.GetJobPost(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateJobPost_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        SetupControllerContext();
        var request = new CreateJobPostRequest
        {
            Title = "New Job",
            Description = "New Description",
            Location = "New Location",
            Department = "New Department",
            EmploymentType = "Full-time",
            ExperienceLevel = "Senior",
            RequiredSkills = "C#, .NET, Azure",
            PreferredSkills = "Vue.js, React",
            Responsibilities = "Lead development team",
            Benefits = "Competitive salary, health insurance",
            SalaryMin = 80000,
            SalaryMax = 120000
        };

        var jobPost = new JobPostResponse
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Department = request.Department,
            EmploymentType = request.EmploymentType,
            ExperienceLevel = request.ExperienceLevel,
            RequiredSkills = request.RequiredSkills,
            PreferredSkills = request.PreferredSkills,
            Responsibilities = request.Responsibilities,
            Benefits = request.Benefits,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Status = "Active",
            PostedDate = DateTime.UtcNow,
            ApplicantCount = 0,
            User = new UserInfo
            {
                Id = "test-user-id",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CompanyName = "Test Company"
            }
        };

        _mockJobPostService
            .Setup(x => x.CreateJobPostAsync(request, "test-user-id"))
            .ReturnsAsync(jobPost);

        // Act
        var result = await _controller.CreateJobPost(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(jobPost);
        createdResult.ActionName.Should().Be(nameof(JobPostsController.GetJobPost));
        createdResult.RouteValues!["id"].Should().Be(1);
    }

    [Fact]
    public async Task UpdateJobPost_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        SetupControllerContext();
        var request = new UpdateJobPostRequest
        {
            Title = "Updated Job",
            Description = "Updated Description",
            Status = "Inactive"
        };

        var jobPost = new JobPostResponse
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Location = "Original Location",
            Department = "Original Department",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid",
            RequiredSkills = "C#, .NET",
            PreferredSkills = "Vue.js",
            Responsibilities = "Original responsibilities",
            Benefits = "Original benefits",
            SalaryMin = 50000,
            SalaryMax = 70000,
            PostedDate = DateTime.UtcNow,
            ApplicantCount = 0,
            User = new UserInfo
            {
                Id = "test-user-id",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CompanyName = "Test Company"
            }
        };

        _mockJobPostService
            .Setup(x => x.UpdateJobPostAsync(1, request, "test-user-id"))
            .ReturnsAsync(jobPost);

        // Act
        var result = await _controller.UpdateJobPost(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(jobPost);
    }

    [Fact]
    public async Task UpdateJobPost_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupControllerContext();
        var request = new UpdateJobPostRequest
        {
            Title = "Updated Job"
        };

        _mockJobPostService
            .Setup(x => x.UpdateJobPostAsync(999, request, "test-user-id"))
            .ThrowsAsync(new InvalidOperationException("Job post not found."));

        // Act
        var result = await _controller.UpdateJobPost(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeEquivalentTo(new { message = "Job post not found." });
    }

    [Fact]
    public async Task DeleteJobPost_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        SetupControllerContext();
        _mockJobPostService
            .Setup(x => x.DeleteJobPostAsync(1, "test-user-id"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJobPost(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteJobPost_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupControllerContext();
        _mockJobPostService
            .Setup(x => x.DeleteJobPostAsync(999, "test-user-id"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteJobPost(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
