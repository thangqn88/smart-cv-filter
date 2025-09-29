using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Controllers;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Services.Interfaces;
using SmartCVFilter.API.Tests.TestBase;
using Xunit;
using System.Security.Claims;

namespace SmartCVFilter.API.Tests.Controllers;

public class AuthControllerTests : TestBaseClass, IDisposable
{
    private AuthController _controller = null!;
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<ILogger<AuthController>> _mockLogger = null!;

    public AuthControllerTests()
    {
        Setup();
        SetupMocks();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    private void SetupMocks()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            CompanyName = "Test Company"
        };

        var authResponse = new AuthResponse
        {
            Token = "test-token",
            Expiration = DateTime.UtcNow.AddDays(7),
            User = new UserInfo
            {
                Id = "user-id",
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyName = request.CompanyName
            }
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(authResponse);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("User with this email already exists."));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { message = "User with this email already exists." });
    }

    [Fact]
    public async Task Register_WithServerError_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var authResponse = new AuthResponse
        {
            Token = "test-token",
            Expiration = DateTime.UtcNow.AddDays(7),
            User = new UserInfo
            {
                Id = "user-id",
                Email = request.Email,
                FirstName = "Test",
                LastName = "User",
                CompanyName = "Test Company"
            }
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(authResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeEquivalentTo(new { message = "Invalid email or password." });
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var userInfo = new UserInfo
        {
            Id = "user-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            CompanyName = "Test Company"
        };

        _mockAuthService
            .Setup(x => x.GetUserInfoAsync("user-id"))
            .ReturnsAsync(userInfo);

        // Setup controller context with claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-id"),
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

        // Act
        var result = await _controller.ValidateToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(userInfo);
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.GetUserInfoAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Invalid token"));

        // Setup controller context without claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal()
            }
        };

        // Act
        var result = await _controller.ValidateToken();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
