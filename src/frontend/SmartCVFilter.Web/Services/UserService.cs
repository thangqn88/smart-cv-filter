using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class UserService : IUserService
{
    private readonly IApiService _apiService;
    private readonly ILogger<UserService> _logger;

    public UserService(IApiService apiService, ILogger<UserService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (page > 1) queryParams.Add($"page={page}");
            if (pageSize != 10) queryParams.Add($"pageSize={pageSize}");
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrEmpty(role)) queryParams.Add($"role={Uri.EscapeDataString(role)}");

            var endpoint = "users";
            if (queryParams.Any())
            {
                endpoint += "?" + string.Join("&", queryParams);
            }

            var result = await _apiService.MakeRequestAsync<UserListResponse>(endpoint, HttpMethod.Get);
            return result ?? new UserListResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new UserListResponse();
        }
    }

    public async Task<UserResponse?> GetUserAsync(string id)
    {
        try
        {
            return await _apiService.MakeRequestAsync<UserResponse>($"users/{id}", HttpMethod.Get);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", id);
            return null;
        }
    }

    public async Task<UserResponse?> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            return await _apiService.MakeRequestAsync<UserResponse>("users", HttpMethod.Post, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return null;
        }
    }

    public async Task<UserResponse?> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        try
        {
            return await _apiService.MakeRequestAsync<UserResponse>($"users/{id}", HttpMethod.Put, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"users/{id}", HttpMethod.Delete);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string id, ChangePasswordRequest request)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"users/{id}/change-password", HttpMethod.Post, request);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", id);
            return false;
        }
    }

    public async Task<bool> AssignRoleAsync(string id, AssignRoleRequest request)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"users/{id}/assign-role", HttpMethod.Post, request);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}", id);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(string id, string roleName)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"users/{id}/roles/{Uri.EscapeDataString(roleName)}", HttpMethod.Delete);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {RoleName}", id, roleName);
            return false;
        }
    }

    public async Task<List<string>> GetUserRolesAsync(string id)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<List<string>>($"users/{id}/roles", HttpMethod.Get);
            return result ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles: {UserId}", id);
            return new List<string>();
        }
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"users/{id}/toggle-status", HttpMethod.Post);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status: {UserId}", id);
            return false;
        }
    }
}
