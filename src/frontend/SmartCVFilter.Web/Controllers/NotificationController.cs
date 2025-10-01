using Microsoft.AspNetCore.Mvc;
using SmartCVFilter.Web.Services;

namespace SmartCVFilter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("remove")]
    public IActionResult RemoveNotification([FromBody] RemoveNotificationRequest request)
    {
        try
        {
            _notificationService.RemoveNotification(request.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing notification {NotificationId}", request.Id);
            return StatusCode(500, "Error removing notification");
        }
    }

    [HttpPost("clear")]
    public IActionResult ClearAllNotifications()
    {
        try
        {
            _notificationService.ClearAll();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing notifications");
            return StatusCode(500, "Error clearing notifications");
        }
    }

    [HttpGet]
    public IActionResult GetNotifications()
    {
        try
        {
            var notifications = _notificationService.GetNotifications();
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, "Error getting notifications");
        }
    }
}

public class RemoveNotificationRequest
{
    public string Id { get; set; } = string.Empty;
}
