using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    bool ValidateToken(string token);
    Task<UserInfo> GetUserInfoAsync(string userId);
}

