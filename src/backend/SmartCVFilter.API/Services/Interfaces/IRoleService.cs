using SmartCVFilter.API.DTOs;

namespace SmartCVFilter.API.Services.Interfaces;

public interface IRoleService
{
    Task<RoleListResponse> GetRolesAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<RoleResponse?> GetRoleByIdAsync(string id);
    Task<RoleResponse?> GetRoleByNameAsync(string name);
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleResponse?> UpdateRoleAsync(string id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(string id);
    Task<bool> AssignRoleToUserAsync(string userId, string roleName);
    Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<List<string>> GetRolePermissionsAsync(string roleName);
    Task<bool> UpdateRolePermissionsAsync(string roleName, List<string> permissions);
    Task<List<string>> GetAllPermissionsAsync();
    Task<Dictionary<string, string>> GetPermissionDescriptionsAsync();
}
