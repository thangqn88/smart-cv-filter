using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RoleListResponse> GetRolesAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var query = _roleManager.Roles.AsQueryable();

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                var searchPattern = $"%{search}%";
                query = query.Where(r => EF.Functions.ILike(r.Name!, searchPattern));
            }

            var totalRoles = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRoles / pageSize);

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleResponses = new List<RoleResponse>();
            foreach (var role in roles)
            {
                var roleResponse = _mapper.Map<RoleResponse>(role);

                // Get user count for this role
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleResponse.UserCount = usersInRole.Count;
                roleResponse.IsActive = true; // For now, all roles are active

                roleResponses.Add(roleResponse);
            }

            var activeRoles = roleResponses.Count(r => r.IsActive);
            var inactiveRoles = totalRoles - activeRoles;

            return new RoleListResponse
            {
                Roles = roleResponses,
                TotalRoles = totalRoles,
                ActiveRoles = activeRoles,
                InactiveRoles = inactiveRoles,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            throw;
        }
    }

    public async Task<RoleResponse?> GetRoleByIdAsync(string id)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            var roleResponse = _mapper.Map<RoleResponse>(role);

            // Get user count for this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleResponse.UserCount = usersInRole.Count;
            roleResponse.IsActive = true;

            return roleResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by ID: {RoleId}", id);
            throw;
        }
    }

    public async Task<RoleResponse?> GetRoleByNameAsync(string name)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null) return null;

            var roleResponse = _mapper.Map<RoleResponse>(role);

            // Get user count for this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleResponse.UserCount = usersInRole.Count;
            roleResponse.IsActive = true;

            return roleResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", name);
            throw;
        }
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
    {
        try
        {
            var role = new IdentityRole
            {
                Name = request.Name,
                NormalizedName = request.Name.ToUpperInvariant()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role: {errors}");
            }

            var roleResponse = _mapper.Map<RoleResponse>(role);
            roleResponse.Description = request.Description;
            roleResponse.Permissions = request.Permissions;
            roleResponse.IsActive = true;
            roleResponse.UserCount = 0;

            return roleResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            throw;
        }
    }

    public async Task<RoleResponse?> UpdateRoleAsync(string id, UpdateRoleRequest request)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return null;

            // Update role name if provided
            if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
            {
                role.Name = request.Name;
                role.NormalizedName = request.Name.ToUpperInvariant();

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to update role: {errors}");
                }
            }

            var roleResponse = _mapper.Map<RoleResponse>(role);
            roleResponse.Description = request.Description ?? roleResponse.Description;
            roleResponse.Permissions = request.Permissions;
            roleResponse.IsActive = request.IsActive;

            // Get user count for this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleResponse.UserCount = usersInRole.Count;

            return roleResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(string id)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return false;

            // Check if any users are assigned to this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                throw new InvalidOperationException("Cannot delete role that has users assigned to it");
            }

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            throw;
        }
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}, Role: {RoleName}", userId, roleName);
            throw;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {RoleName}", userId, roleName);
            throw;
        }
    }

}
