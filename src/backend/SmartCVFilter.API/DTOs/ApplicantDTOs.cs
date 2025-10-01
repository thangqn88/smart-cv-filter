using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.API.DTOs;

public class CreateApplicantRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? LinkedInProfile { get; set; }

    [StringLength(200)]
    public string? PortfolioUrl { get; set; }

    [StringLength(1000)]
    public string? CoverLetter { get; set; }
}

public class UpdateApplicantRequest
{
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name is required and must be between 1 and 100 characters.")]
    public string? FirstName { get; set; }

    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name is required and must be between 1 and 100 characters.")]
    public string? LastName { get; set; }

    [StringLength(200, MinimumLength = 1, ErrorMessage = "Email address is required and must be between 1 and 200 characters.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? LinkedInProfile { get; set; }

    [StringLength(200)]
    public string? PortfolioUrl { get; set; }

    [StringLength(1000)]
    public string? CoverLetter { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
}

public class ApplicantResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? CoverLetter { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public List<CVFileResponse> CVFiles { get; set; } = new();
    public List<ScreeningResultResponse> ScreeningResults { get; set; } = new();
}

public class CVFileResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScreeningResultResponse
{
    public int Id { get; set; }
    public int OverallScore { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string DetailedAnalysis { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ScreeningRequest
{
    [Required]
    public List<int> ApplicantIds { get; set; } = new();
}

public class ScreenedApplicantResponse
{
    public int ApplicantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobLocation { get; set; } = string.Empty;
    public string JobDepartment { get; set; } = string.Empty;
    public int LatestScore { get; set; }
    public string LatestScoreStatus { get; set; } = string.Empty;
    public DateTime LatestScreeningDate { get; set; }
    public int TotalScreenings { get; set; }
}

