using SmartCVFilter.Web.Models;

namespace SmartCVFilter.Web.ViewModels;

public class ApplicantIndexViewModel
{
    public List<ApplicantResponse> Applicants { get; set; } = new();
    public List<JobPostListResponse> JobPosts { get; set; } = new();
    public int? SelectedJobPostId { get; set; }
    public string? JobPostTitle { get; set; }
}
