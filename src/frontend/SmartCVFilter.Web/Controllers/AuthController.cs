using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

public class AuthController : Controller
{
    private readonly IApiService _apiService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IApiService apiService, INotificationService notificationService, ILogger<AuthController> logger)
    {
        _apiService = apiService;
        _notificationService = notificationService;
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
    public async Task<IActionResult> Login([FromForm] LoginRequest model, string? returnUrl = null)
    {
        // Debug logging
        _logger.LogInformation("Login POST received - Model is null: {IsNull}, Email: {Email}, Password: {Password}, ModelState.IsValid: {IsValid}",
            model == null,
            model?.Email ?? "NULL",
            string.IsNullOrEmpty(model?.Password) ? "EMPTY" : "***",
            ModelState.IsValid);

        // Log all form values
        foreach (var key in Request.Form.Keys)
        {
            _logger.LogInformation("Form key: {Key} = {Value}", key, Request.Form[key]);
        }

        // If model is null, create a new one
        if (model == null)
        {
            _logger.LogWarning("Model is null, creating new LoginRequest");
            model = new LoginRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            // Add validation errors to notifications
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            _notificationService.AddValidationErrors(errors);

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
                _notificationService.AddSuccess($"Welcome back, {result.User.FirstName}!", "Login Successful");

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
                _notificationService.AddError("Invalid email or password. Please check your credentials and try again.", "Login Failed");
                ViewData["ReturnUrl"] = returnUrl; // Preserve ReturnUrl on login failure
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            _notificationService.AddError("An error occurred during login. Please try again.", "Login Error");
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
            // Add validation errors to notifications
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            _notificationService.AddValidationErrors(errors);

            return View(model);
        }

        try
        {
            var result = await _apiService.RegisterAsync(model);
            if (result != null)
            {
                _notificationService.AddSuccess($"Welcome, {result.User.FirstName}! Your account has been created successfully.", "Registration Successful");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _notificationService.AddError("Registration failed. Please try again.", "Registration Failed");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            _notificationService.AddError("An error occurred during registration. Please try again.", "Registration Error");
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
            _notificationService.AddInfo("You have been logged out successfully.", "Logout");
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            _notificationService.AddError("An error occurred during logout.", "Logout Error");
            return RedirectToAction("Index", "Home");
        }
    }

    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}
