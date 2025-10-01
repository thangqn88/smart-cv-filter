using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? role = null)
    {
        ViewData["Title"] = "User Management";

        try
        {
            var result = await _userService.GetUsersAsync(page, pageSize, search, role);

            var viewModel = new UserIndexViewModel
            {
                Users = result.Users,
                TotalUsers = result.TotalUsers,
                ActiveUsers = result.ActiveUsers,
                InactiveUsers = result.InactiveUsers
            };

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Search"] = search;
            ViewData["Role"] = role;
            ViewData["TotalPages"] = result.TotalPages;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            TempData["Error"] = "An error occurred while loading users.";

            var viewModel = new UserIndexViewModel
            {
                Users = new List<UserResponse>(),
                TotalUsers = 0,
                ActiveUsers = 0,
                InactiveUsers = 0
            };

            return View(viewModel);
        }
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create User";
        var model = new CreateUserRequest();
        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        ViewData["Title"] = "Edit User";
        ViewData["UserId"] = id;

        try
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var model = new UpdateUserRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CompanyName = user.CompanyName,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            TempData["Error"] = "An error occurred while loading the user.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _userService.CreateUserAsync(model);
            if (result != null)
            {
                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Failed to create user. Please try again.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            TempData["Error"] = "An error occurred while creating the user.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["UserId"] = id;
            return View(model);
        }

        try
        {
            var result = await _userService.UpdateUserAsync(id, model);
            if (result != null)
            {
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Failed to update user. Please try again.";
                ViewData["UserId"] = id;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            TempData["Error"] = "An error occurred while updating the user.";
            ViewData["UserId"] = id;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            TempData["Error"] = "An error occurred while deleting the user.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        try
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (result)
            {
                TempData["Success"] = "User status updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update user status. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status: {UserId}", id);
            TempData["Error"] = "An error occurred while updating user status.";
        }

        return RedirectToAction("Index");
    }
}
