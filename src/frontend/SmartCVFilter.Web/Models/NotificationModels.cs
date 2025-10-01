using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.Web.Models;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsDismissible { get; set; } = true;
    public int Duration { get; set; } = 5000; // Auto-dismiss after 5 seconds (0 = no auto-dismiss)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Icon { get; set; }
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
}

public class NotificationOptions
{
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsDismissible { get; set; } = true;
    public int Duration { get; set; } = 5000;
    public string? Icon { get; set; }
    public string? ActionText { get; set; }
    public string? ActionUrl { get; set; }
}

