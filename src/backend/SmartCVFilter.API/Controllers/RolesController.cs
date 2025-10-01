using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RoleListResponse>> GetRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        try
        {
            var result = await _roleService.GetRolesAsync(page, pageSize, search);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleResponse>> GetRole(string id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound();

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role: {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Get role by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<RoleResponse>> GetRoleByName(string name)
    {
        try
        {
            var role = await _roleService.GetRoleByNameAsync(name);
            if (role == null)
                return NotFound();

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", name);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating role");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, "An error occurred while creating the role");
        }
    }

    /// <summary>
    /// Update role information
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleResponse>> UpdateRole(string id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _roleService.UpdateRoleAsync(id, request);
            if (role == null)
                return NotFound();

            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating role: {RoleId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            return StatusCode(500, "An error occurred while updating the role");
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(string id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while deleting role: {RoleId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            return StatusCode(500, "An error occurred while deleting the role");
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("assign-to-user")]
    public async Task<ActionResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AssignRoleToUserAsync(request.UserId, request.RoleName);
            if (!result)
                return NotFound();

            return Ok(new { message = "Role assigned to user successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return StatusCode(500, "An error occurred while assigning the role");
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("remove-from-user")]
    public async Task<ActionResult> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.RemoveRoleFromUserAsync(request.UserId, request.RoleName);
            if (!result)
                return NotFound();

            return Ok(new { message = "Role removed from user successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {RoleName}", request.UserId, request.RoleName);
            return StatusCode(500, "An error occurred while removing the role");
        }
    }

    /// <summary>
    /// Get role permissions
    /// </summary>
    [HttpGet("{roleName}/permissions")]
    public async Task<ActionResult<List<string>>> GetRolePermissions(string roleName)
    {
        try
        {
            var permissions = await _roleService.GetRolePermissionsAsync(roleName);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role permissions: {RoleName}", roleName);
            return StatusCode(500, "An error occurred while retrieving role permissions");
        }
    }

    /// <summary>
    /// Update role permissions
    /// </summary>
    [HttpPut("{roleName}/permissions")]
    public async Task<ActionResult> UpdateRolePermissions(string roleName, [FromBody] List<string> permissions)
    {
        try
        {
            var result = await _roleService.UpdateRolePermissionsAsync(roleName, permissions);
            if (!result)
                return NotFound();

            return Ok(new { message = "Role permissions updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role permissions: {RoleName}", roleName);
            return StatusCode(500, "An error occurred while updating role permissions");
        }
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet("permissions/all")]
    public async Task<ActionResult<List<string>>> GetAllPermissions()
    {
        try
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }

    /// <summary>
    /// Get permission descriptions
    /// </summary>
    [HttpGet("permissions/descriptions")]
    public async Task<ActionResult<Dictionary<string, string>>> GetPermissionDescriptions()
    {
        try
        {
            var descriptions = await _roleService.GetPermissionDescriptionsAsync();
            return Ok(descriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission descriptions");
            return StatusCode(500, "An error occurred while retrieving permission descriptions");
        }
    }
}
