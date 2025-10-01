using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.Web.Models;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();
}

public class UpdateRoleRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = new();

    public bool IsActive { get; set; } = true;
}

public class RoleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class RoleIndexViewModel
{
    public List<RoleResponse> Roles { get; set; } = new();
    public int TotalRoles { get; set; }
    public int ActiveRoles { get; set; }
    public int InactiveRoles { get; set; }
}

public static class RolePermissions
{
    public static readonly List<string> AllPermissions = new()
    {
        "Users.View",
        "Users.Create",
        "Users.Edit",
        "Users.Delete",
        "Roles.View",
        "Roles.Create",
        "Roles.Edit",
        "Roles.Delete",
        "JobPosts.View",
        "JobPosts.Create",
        "JobPosts.Edit",
        "JobPosts.Delete",
        "Applicants.View",
        "Applicants.Create",
        "Applicants.Edit",
        "Applicants.Delete",
        "Screening.View",
        "Screening.Create",
        "Screening.Edit",
        "Screening.Delete",
        "Dashboard.View",
        "Reports.View",
        "Settings.View",
        "Settings.Edit"
    };

    public static readonly Dictionary<string, string> PermissionDescriptions = new()
    {
        { "Users.View", "View user list and details" },
        { "Users.Create", "Create new users" },
        { "Users.Edit", "Edit user information" },
        { "Users.Delete", "Delete users" },
        { "Roles.View", "View role list and details" },
        { "Roles.Create", "Create new roles" },
        { "Roles.Edit", "Edit role information" },
        { "Roles.Delete", "Delete roles" },
        { "JobPosts.View", "View job postings" },
        { "JobPosts.Create", "Create job postings" },
        { "JobPosts.Edit", "Edit job postings" },
        { "JobPosts.Delete", "Delete job postings" },
        { "Applicants.View", "View applicants" },
        { "Applicants.Create", "Create applicants" },
        { "Applicants.Edit", "Edit applicants" },
        { "Applicants.Delete", "Delete applicants" },
        { "Screening.View", "View screening results" },
        { "Screening.Create", "Create screening processes" },
        { "Screening.Edit", "Edit screening results" },
        { "Screening.Delete", "Delete screening results" },
        { "Dashboard.View", "View dashboard" },
        { "Reports.View", "View reports" },
        { "Settings.View", "View system settings" },
        { "Settings.Edit", "Edit system settings" }
    };
}

