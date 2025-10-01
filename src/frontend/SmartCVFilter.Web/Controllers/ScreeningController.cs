using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Models;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

[Authorize]
public class ScreeningController : Controller
{
    private readonly IScreeningService _screeningService;
    private readonly IApplicantService _applicantService;
    private readonly ILogger<ScreeningController> _logger;

    public ScreeningController(
        IScreeningService screeningService,
        IApplicantService applicantService,
        ILogger<ScreeningController> logger)
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

            if (applicantId.HasValue)
            {
                // Show specific applicant's screening results
                var results = await _screeningService.GetScreeningResultsByApplicantAsync(applicantId.Value);
                ViewData["ApplicantId"] = applicantId.Value;
                return View("ApplicantResults", results);
            }
            else
            {
                // Show list of all screened applicants
                var screenedApplicants = await _screeningService.GetScreenedApplicantsAsync();
                return View("ScreenedApplicants", screenedApplicants);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading screening results");
            TempData["Error"] = "An error occurred while loading screening results.";
            return View("ScreenedApplicants", new List<ScreenedApplicantResponse>());
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
