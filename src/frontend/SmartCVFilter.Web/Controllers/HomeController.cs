using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

public class HomeController : Controller
{
    private readonly IJobPostService _jobPostService;
    private readonly IApplicantService _applicantService;
    private readonly IScreeningService _screeningService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IJobPostService jobPostService,
        IApplicantService applicantService,
        IScreeningService screeningService,
        ILogger<HomeController> logger)
    {
        _jobPostService = jobPostService;
        _applicantService = applicantService;
        _screeningService = screeningService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewData["Title"] = "Dashboard";

            // Get dashboard statistics
            var jobPosts = await _jobPostService.GetJobPostsAsync();
            var totalJobPosts = jobPosts.Count;
            var activeJobPosts = jobPosts.Count(jp => jp.Status == "Active");
            var totalApplicants = jobPosts.Sum(jp => jp.ApplicantCount);

            var dashboardData = new
            {
                TotalJobPosts = totalJobPosts,
                ActiveJobPosts = activeJobPosts,
                TotalApplicants = totalApplicants,
                RecentJobPosts = jobPosts.Take(5).ToList()
            };

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");

            // If it's a connection error, show a more user-friendly message
            if (ex.Message.Contains("BaseAddress") || ex.Message.Contains("connection"))
            {
                TempData["Warning"] = "Unable to connect to the backend API. Please ensure the backend service is running.";
            }
            else
            {
                TempData["Error"] = "An error occurred while loading the dashboard.";
            }

            // Return empty dashboard data
            var emptyDashboardData = new
            {
                TotalJobPosts = 0,
                ActiveJobPosts = 0,
                TotalApplicants = 0,
                RecentJobPosts = new List<object>()
            };

            return View(emptyDashboardData);
        }
    }

    public IActionResult Error()
    {
        ViewData["Title"] = "Error";
        return View();
    }
}
