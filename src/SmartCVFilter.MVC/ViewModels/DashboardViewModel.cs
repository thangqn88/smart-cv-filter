namespace SmartCVFilter.API.ViewModels;

public class DashboardViewModel
{
    public int TotalJobPosts { get; set; }
    public int TotalApplicants { get; set; }
    public int PendingApplications { get; set; }
    public int CompletedScreenings { get; set; }
    public List<RecentJobPost> RecentJobPosts { get; set; } = new();
    public List<RecentApplicant> RecentApplicants { get; set; } = new();
}

public class RecentJobPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ApplicantCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime PostedDate { get; set; }
}

public class RecentApplicant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
}
