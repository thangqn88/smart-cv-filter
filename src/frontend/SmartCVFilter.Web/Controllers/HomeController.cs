using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

public class HomeController : Controller
{
    private readonly IJobPostService _jobPostService;
    private readonly IApplicantService _applicantService;
    private readonly IScreeningService _screeningService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IJobPostService jobPostService,
        IApplicantService applicantService,
        IScreeningService screeningService,
        INotificationService notificationService,
        ILogger<HomeController> logger)
    {
        _jobPostService = jobPostService;
        _applicantService = applicantService;
        _screeningService = screeningService;
        _notificationService = notificationService;
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
                _notificationService.AddWarning("Unable to connect to the backend API. Please ensure the backend service is running.", "Connection Issue");
            }
            else
            {
                _notificationService.AddError("An error occurred while loading the dashboard.", "Dashboard Error");
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

    public IActionResult NotificationDemo()
    {
        ViewData["Title"] = "Notification Demo";
        return View();
    }

    public IActionResult TestNotification()
    {
        _notificationService.AddError("This is a test error notification!", "Test Error");
        _notificationService.AddSuccess("This is a test success notification!", "Test Success");
        _notificationService.AddWarning("This is a test warning notification!", "Test Warning");
        _notificationService.AddInfo("This is a test info notification!", "Test Info");

        return RedirectToAction("Index");
    }
}
