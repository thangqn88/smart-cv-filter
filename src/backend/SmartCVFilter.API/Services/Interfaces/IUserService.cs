using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IUserService
{
    Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null);
    Task<UserResponse?> GetUserByIdAsync(string id);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(string id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<bool> AssignRoleAsync(string userId, string roleName);
    Task<bool> RemoveRoleAsync(string userId, string roleName);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<bool> ToggleUserStatusAsync(string userId);
}
