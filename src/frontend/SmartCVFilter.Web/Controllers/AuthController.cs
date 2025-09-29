using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

public class AuthController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IApiService apiService, ILogger<AuthController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Login";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.LoginAsync(model);
            if (result != null)
            {
                TempData["Success"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Register";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiService.RegisterAsync(model);
            if (result != null)
            {
                TempData["Success"] = "Registration successful! You are now logged in.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _apiService.LogoutAsync();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            TempData["Error"] = "An error occurred during logout.";
            return RedirectToAction("Index", "Home");
        }
    }

    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}
