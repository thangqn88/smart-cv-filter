using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public class NotificationService : INotificationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NotificationService> _logger;
    private const string NotificationsKey = "Notifications";

    public NotificationService(IHttpContextAccessor httpContextAccessor, ILogger<NotificationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public void AddSuccess(string message, string? title = null, int? duration = null)
    {
        var notification = new Notification
        {
            Type = NotificationType.Success,
            Title = title ?? "Success",
            Message = message,
            Icon = "bi-check-circle-fill",
            Duration = duration ?? 5000
        };
        AddNotification(notification);
    }

    public void AddError(string message, string? title = null, int? duration = null)
    {
        _logger.LogInformation("Adding error notification: {Title} - {Message}", title ?? "Error", message);
        var notification = new Notification
        {
            Type = NotificationType.Error,
            Title = title ?? "Error",
            Message = message,
            Icon = "bi-exclamation-triangle-fill",
            Duration = duration ?? 0 // Errors don't auto-dismiss by default
        };
        AddNotification(notification);
    }

    public void AddWarning(string message, string? title = null, int? duration = null)
    {
        var notification = new Notification
        {
            Type = NotificationType.Warning,
            Title = title ?? "Warning",
            Message = message,
            Icon = "bi-exclamation-circle-fill",
            Duration = duration ?? 7000
        };
        AddNotification(notification);
    }

    public void AddInfo(string message, string? title = null, int? duration = null)
    {
        var notification = new Notification
        {
            Type = NotificationType.Info,
            Title = title ?? "Information",
            Message = message,
            Icon = "bi-info-circle-fill",
            Duration = duration ?? 5000
        };
        AddNotification(notification);
    }

    public void AddNotification(NotificationOptions options)
    {
        var notification = new Notification
        {
            Type = options.Type,
            Title = options.Title,
            Message = options.Message,
            IsDismissible = options.IsDismissible,
            Duration = options.Duration,
            Icon = options.Icon ?? GetDefaultIcon(options.Type),
            ActionText = options.ActionText,
            ActionUrl = options.ActionUrl
        };
        AddNotification(notification);
    }

    public void AddValidationErrors(Dictionary<string, string[]> errors)
    {
        var errorMessages = errors.SelectMany(kvp => kvp.Value).ToList();
        var message = errorMessages.Count == 1
            ? errorMessages.First()
            : $"Please fix the following {errorMessages.Count} validation errors:";

        AddError(message, "Validation Failed");
    }

    public void AddApiError(string message, int? statusCode = null)
    {
        var title = statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Server Error",
            _ => "API Error"
        };

        AddError(message, title);
    }

    public List<Notification> GetNotifications()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session == null)
            {
                _logger.LogWarning("HttpContext or Session is null");
                return new List<Notification>();
            }

            var notificationsJson = httpContext.Session.GetString(NotificationsKey);
            _logger.LogInformation("Session notifications JSON: {NotificationsJson}", notificationsJson);

            if (string.IsNullOrEmpty(notificationsJson))
            {
                _logger.LogInformation("No notifications found in session");
                return new List<Notification>();
            }

            var notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
            _logger.LogInformation("Retrieved {Count} notifications from session", notifications?.Count ?? 0);
            return notifications ?? new List<Notification>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications from session");
            return new List<Notification>();
        }
    }

    public void RemoveNotification(string id)
    {
        try
        {
            var notifications = GetNotifications();
            notifications.RemoveAll(n => n.Id == id);
            SaveNotifications(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing notification {NotificationId}", id);
        }
    }

    public void ClearAll()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext?.Session.Remove(NotificationsKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing notifications");
        }
    }

    private void AddNotification(Notification notification)
    {
        try
        {
            _logger.LogInformation("Adding notification to session: {Type} - {Title}", notification.Type, notification.Title);
            var notifications = GetNotifications();
            notifications.Add(notification);

            _logger.LogInformation("Total notifications after adding: {Count}", notifications.Count);

            // Limit to 10 notifications to prevent session bloat
            if (notifications.Count > 10)
            {
                notifications = notifications.OrderByDescending(n => n.CreatedAt).Take(10).ToList();
            }

            SaveNotifications(notifications);
            _logger.LogInformation("Notification saved to session successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding notification");
        }
    }

    private void SaveNotifications(List<Notification> notifications)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session == null)
            {
                _logger.LogWarning("HttpContext or Session is null when saving notifications");
                return;
            }

            var notificationsJson = JsonConvert.SerializeObject(notifications);
            _logger.LogInformation("Saving {Count} notifications to session: {Json}", notifications.Count, notificationsJson);
            httpContext.Session.SetString(NotificationsKey, notificationsJson);
            _logger.LogInformation("Notifications saved to session successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving notifications to session");
        }
    }

    private static string GetDefaultIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Success => "bi-check-circle-fill",
            NotificationType.Error => "bi-exclamation-triangle-fill",
            NotificationType.Warning => "bi-exclamation-circle-fill",
            NotificationType.Info => "bi-info-circle-fill",
            _ => "bi-info-circle-fill"
        };
    }
}
