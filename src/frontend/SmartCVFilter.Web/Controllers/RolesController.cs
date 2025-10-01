using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        ViewData["Title"] = "Role Management";

        try
        {
            var result = await _roleService.GetRolesAsync(page, pageSize, search);

            var viewModel = new RoleIndexViewModel
            {
                Roles = result.Roles,
                TotalRoles = result.TotalRoles,
                ActiveRoles = result.ActiveRoles,
                InactiveRoles = result.InactiveRoles
            };

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Search"] = search;
            ViewData["TotalPages"] = result.TotalPages;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles");
            TempData["Error"] = "An error occurred while loading roles.";

            var viewModel = new RoleIndexViewModel
            {
                Roles = new List<RoleResponse>(),
                TotalRoles = 0,
                ActiveRoles = 0,
                InactiveRoles = 0
            };

            return View(viewModel);
        }
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create Role";
        var model = new CreateRoleRequest();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        ViewData["Title"] = "Edit Role";
        ViewData["RoleId"] = id;

        try
        {
            var role = await _roleService.GetRoleAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction("Index");
            }

            var model = new UpdateRoleRequest
            {
                Name = role.Name,
                Description = role.Description,
                Permissions = role.Permissions,
                IsActive = role.IsActive
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role for edit: {RoleId}", id);
            TempData["Error"] = "An error occurred while loading the role.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _roleService.CreateRoleAsync(model);
            if (result != null)
            {
                TempData["Success"] = "Role created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Failed to create role. Please try again.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            TempData["Error"] = "An error occurred while creating the role.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateRoleRequest model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["RoleId"] = id;
            return View(model);
        }

        try
        {
            var result = await _roleService.UpdateRoleAsync(id, model);
            if (result != null)
            {
                TempData["Success"] = "Role updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Failed to update role. Please try again.";
                ViewData["RoleId"] = id;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            TempData["Error"] = "An error occurred while updating the role.";
            ViewData["RoleId"] = id;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (result)
            {
                TempData["Success"] = "Role deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete role. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            TempData["Error"] = "An error occurred while deleting the role.";
        }

        return RedirectToAction("Index");
    }
}
