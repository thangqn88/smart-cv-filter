using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.Services;

public interface INotificationService
{
    void AddSuccess(string message, string? title = null, int? duration = null);
    void AddError(string message, string? title = null, int? duration = null);
    void AddWarning(string message, string? title = null, int? duration = null);
    void AddInfo(string message, string? title = null, int? duration = null);
    void AddNotification(NotificationOptions options);
    List<Notification> GetNotifications();
    void RemoveNotification(string id);
    void ClearAll();
    void AddValidationErrors(Dictionary<string, string[]> errors);
    void AddApiError(string message, int? statusCode = null);
}

