using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.CompanyName.Contains(search));
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole?.Select(u => u.Id).ToList() ?? new List<string>();
                query = query.Where(u => userIds.Contains(u.Id));
            }

            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var users = await query
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userResponses = new List<UserResponse>();
            foreach (var user in users)
            {
                var userResponse = _mapper.Map<UserResponse>(user);
                var userRoles = await _userManager.GetRolesAsync(user);
                userResponse.Role = userRoles.FirstOrDefault() ?? "User";
                userResponse.IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;
                userResponses.Add(userResponse);
            }

            var activeUsers = userResponses.Count(u => u.IsActive);
            var inactiveUsers = totalUsers - activeUsers;

            return new UserListResponse
            {
                Users = userResponses,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var userResponse = _mapper.Map<UserResponse>(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            userResponse.Role = userRoles.FirstOrDefault() ?? "User";
            userResponse.IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            return userResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var userResponse = _mapper.Map<UserResponse>(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            userResponse.Role = userRoles.FirstOrDefault() ?? "User";
            userResponse.IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            return userResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyName = request.CompanyName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Assign role
            if (!string.IsNullOrEmpty(request.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(request.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.Role = request.Role;
            userResponse.IsActive = true;

            return userResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<UserResponse?> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            // Update basic properties
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.UserName = request.Email;
            }

            if (!string.IsNullOrEmpty(request.CompanyName))
                user.CompanyName = request.CompanyName;

            // Update lockout status
            if (request.IsActive)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            // Update role if provided
            if (!string.IsNullOrEmpty(request.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                var roleExists = await _roleManager.RoleExistsAsync(request.Role);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            userResponse.Role = userRoles.FirstOrDefault() ?? "User";
            userResponse.IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            return userResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName)
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

    public async Task<bool> RemoveRoleAsync(string userId, string roleName)
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

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
            {
                // Lock the user
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                // Unlock the user
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status: {UserId}", userId);
            throw;
        }
    }
}
