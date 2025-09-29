using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services;
using SmartCVFilter.API.Services.Interfaces;
using SmartCVFilter.API.Tests.TestBase;
using System.Security.Claims;
using Xunit;

namespace SmartCVFilter.API.Tests.Services;

public class AuthServiceTests : TestBaseClass, IDisposable
{
    private AuthService _authService = null!;
    private Mock<SignInManager<ApplicationUser>> _mockSignInManager = null!;

    public AuthServiceTests()
    {
        Setup();
        SetupMocks();
        _authService = new AuthService(UserManager, _mockSignInManager.Object, Configuration, Context);
    }

    private void SetupMocks()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            UserManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User",
            CompanyName = "New Company"
        };

        // Ensure roles are created first
        await InitializeAsync();

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(request.Email);
        result.User.FirstName.Should().Be(request.FirstName);
        result.User.LastName.Should().Be(request.LastName);
        result.User.CompanyName.Should().Be(request.CompanyName);
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await CreateTestUserAsync("existing@example.com");
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act & Assert
        await _authService.Invoking(s => s.RegisterAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com");
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(request.Email);
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        await CreateTestUserAsync("test@example.com");
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com");
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var authResponse = await _authService.LoginAsync(loginRequest);

        // Act
        var result = await _authService.ValidateTokenAsync(authResponse.Token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserInfoAsync_WithValidUserId_ShouldReturnUserInfo()
    {
        // Arrange
        var user = await CreateTestUserAsync("test@example.com");

        // Act
        var result = await _authService.GetUserInfoAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.CompanyName.Should().Be(user.CompanyName);
    }

    [Fact]
    public async Task GetUserInfoAsync_WithInvalidUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidUserId = "invalid-user-id";

        // Act & Assert
        await _authService.Invoking(s => s.GetUserInfoAsync(invalidUserId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found.");
    }

    public void Dispose()
    {
        Cleanup();
    }
}
