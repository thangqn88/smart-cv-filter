using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Controllers;

[Authorize]
public class ScreeningMvcController : Controller
{
    private readonly IScreeningService _screeningService;
    private readonly IApplicantService _applicantService;
    private readonly ILogger<ScreeningMvcController> _logger;

    public ScreeningMvcController(
        IScreeningService screeningService,
        IApplicantService applicantService,
        ILogger<ScreeningMvcController> logger)
    {
        _screeningService = screeningService;
        _applicantService = applicantService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? applicantId)
    {
        try
        {
            ViewData["Title"] = "AI Screening Results";

            List<ScreeningResultResponse> results = new();

            if (applicantId.HasValue)
            {
                results = (await _screeningService.GetScreeningResultsByApplicantAsync(applicantId.Value)).ToList();
                ViewData["ApplicantId"] = applicantId.Value;
            }

            return View(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading screening results");
            TempData["Error"] = "An error occurred while loading screening results.";
            return View(new List<ScreeningResultResponse>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var result = await _screeningService.GetScreeningResultAsync(id);
            if (result == null)
            {
                TempData["Error"] = "Screening result not found.";
                return RedirectToAction("Index");
            }

            ViewData["Title"] = $"Screening Result Details - ID {id}";
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading screening result details for ID {ResultId}", id);
            TempData["Error"] = "An error occurred while loading screening result details.";
            return RedirectToAction("Index");
        }
    }
}
