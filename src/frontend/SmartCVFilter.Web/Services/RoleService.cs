using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class RoleService : IRoleService
{
    private readonly IApiService _apiService;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IApiService apiService, ILogger<RoleService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<RoleListResponse> GetRolesAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (page > 1) queryParams.Add($"page={page}");
            if (pageSize != 10) queryParams.Add($"pageSize={pageSize}");
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var endpoint = "roles";
            if (queryParams.Any())
            {
                endpoint += "?" + string.Join("&", queryParams);
            }

            var result = await _apiService.MakeRequestAsync<RoleListResponse>(endpoint, HttpMethod.Get);
            return result ?? new RoleListResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return new RoleListResponse();
        }
    }

    public async Task<RoleResponse?> GetRoleAsync(string id)
    {
        try
        {
            return await _apiService.MakeRequestAsync<RoleResponse>($"roles/{id}", HttpMethod.Get);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role: {RoleId}", id);
            return null;
        }
    }

    public async Task<RoleResponse?> GetRoleByNameAsync(string name)
    {
        try
        {
            return await _apiService.MakeRequestAsync<RoleResponse>($"roles/by-name/{Uri.EscapeDataString(name)}", HttpMethod.Get);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", name);
            return null;
        }
    }

    public async Task<RoleResponse?> CreateRoleAsync(CreateRoleRequest request)
    {
        try
        {
            return await _apiService.MakeRequestAsync<RoleResponse>("roles", HttpMethod.Post, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return null;
        }
    }

    public async Task<RoleResponse?> UpdateRoleAsync(string id, UpdateRoleRequest request)
    {
        try
        {
            return await _apiService.MakeRequestAsync<RoleResponse>($"roles/{id}", HttpMethod.Put, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>($"roles/{id}", HttpMethod.Delete);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            return false;
        }
    }

    public async Task<bool> AssignRoleToUserAsync(AssignRoleRequest request)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>("roles/assign-to-user", HttpMethod.Post, request);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(AssignRoleRequest request)
    {
        try
        {
            var result = await _apiService.MakeRequestAsync<object>("roles/remove-from-user", HttpMethod.Delete, request);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return false;
        }
    }

}
