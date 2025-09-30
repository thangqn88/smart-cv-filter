using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.API.Models;

public class JobPost
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EmploymentType { get; set; } = string.Empty; // Full-time, Part-time, Contract, etc.

    [Required]
    [StringLength(100)]
    public string ExperienceLevel { get; set; } = string.Empty; // Entry, Mid, Senior, etc.

    [StringLength(1000)]
    public string RequiredSkills { get; set; } = string.Empty;

    [StringLength(1000)]
    public string PreferredSkills { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Responsibilities { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Benefits { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Closed

    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosingDate { get; set; }

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();
}

