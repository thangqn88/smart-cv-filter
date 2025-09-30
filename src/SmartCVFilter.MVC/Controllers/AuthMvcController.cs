using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;
using System.Security.Claims;

namespace SmartCVFilter.API.Controllers;

public class AuthMvcController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthMvcController> _logger;

    public AuthMvcController(IAuthService authService, ILogger<AuthMvcController> logger)
    {
        _authService = authService;
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
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        try
        {
            _logger.LogInformation("Attempting login for user: {Email} with ReturnUrl: {ReturnUrl}", model.Email, returnUrl);
            var result = await _authService.LoginAsync(model);
            if (result != null)
            {
                _logger.LogInformation("Login successful for user: {Email}", model.Email);
                TempData["Success"] = "Login successful!";

                // Sign in user with cookie authentication
                await SignInUserAsync(result.User);

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
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
            ViewData["ReturnUrl"] = returnUrl;
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
            var result = await _authService.RegisterAsync(model);
            if (result != null)
            {
                TempData["Success"] = "Registration successful! You are now logged in.";

                // Sign in user with cookie authentication
                await SignInUserAsync(result.User);

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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

    private async Task SignInUserAsync(UserInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, "User"),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new("CompanyName", user.CompanyName ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
