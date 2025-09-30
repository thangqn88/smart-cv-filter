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
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Login";
        ViewData["ReturnUrl"] = returnUrl;
        _logger.LogInformation("Login page accessed with ReturnUrl: {ReturnUrl}", returnUrl);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl; // Preserve ReturnUrl on validation errors
            return View(model);
        }

        try
        {
            _logger.LogInformation("Attempting login for user: {Email} with ReturnUrl: {ReturnUrl}", model.Email, returnUrl);
            var result = await _apiService.LoginAsync(model);
            if (result != null)
            {
                _logger.LogInformation("Login successful for user: {Email}", model.Email);
                TempData["Success"] = "Login successful!";

                // Redirect to returnUrl if provided and valid, otherwise go to Home
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Redirecting to returnUrl: {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                _logger.LogInformation("Redirecting to Home/Index");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Email}", model.Email);
                ModelState.AddModelError("", "Invalid email or password.");
                ViewData["ReturnUrl"] = returnUrl; // Preserve ReturnUrl on login failure
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
            ViewData["ReturnUrl"] = returnUrl; // Preserve ReturnUrl on error
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
