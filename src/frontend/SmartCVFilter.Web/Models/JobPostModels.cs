using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.Web.Models;

public class CreateJobPostRequest
{
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
    public string EmploymentType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ExperienceLevel { get; set; } = string.Empty;

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

    public DateTime? ClosingDate { get; set; }
}

public class UpdateJobPostRequest
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(50)]
    public string? EmploymentType { get; set; }

    [StringLength(100)]
    public string? ExperienceLevel { get; set; }

    [StringLength(1000)]
    public string? RequiredSkills { get; set; }

    [StringLength(1000)]
    public string? PreferredSkills { get; set; }

    [StringLength(1000)]
    public string? Responsibilities { get; set; }

    [StringLength(1000)]
    public string? Benefits { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? ClosingDate { get; set; }
}

public class JobPostResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string RequiredSkills { get; set; } = string.Empty;
    public string PreferredSkills { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;
    public string Benefits { get; set; } = string.Empty;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public int ApplicantCount { get; set; }
    public UserInfo User { get; set; } = new();
}

public class JobPostListResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; }
    public int ApplicantCount { get; set; }
}
