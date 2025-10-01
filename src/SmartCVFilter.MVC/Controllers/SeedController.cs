using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Models;

namespace SmartCVFilter.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class SeedController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeedController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            await SeedData.Initialize(_context, _userManager, _roleManager);
            TempData["Success"] = "Database seeded successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error seeding database: {ex.Message}";
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> ClearData()
    {
        try
        {
            // Clear all data
            _context.ScreeningResults.RemoveRange(_context.ScreeningResults);
            _context.CVFiles.RemoveRange(_context.CVFiles);
            _context.Applicants.RemoveRange(_context.Applicants);
            _context.JobPosts.RemoveRange(_context.JobPosts);

            // Clear users (except admin)
            var users = _userManager.Users.Where(u => u.Email != "admin@smartcvfilter.com");
            foreach (var user in users)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Database cleared successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error clearing database: {ex.Message}";
        }

        return RedirectToAction("Index", "Home");
    }
}
