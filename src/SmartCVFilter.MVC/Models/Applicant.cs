using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.API.Models;

public class Applicant
{
    public int Id { get; set; }

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

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Applied"; // Applied, Under Review, Shortlisted, Rejected, Hired

    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }

    // Foreign Keys
    [Required]
    public int JobPostId { get; set; }

    // Navigation properties
    public virtual JobPost JobPost { get; set; } = null!;
    public virtual ICollection<CVFile> CVFiles { get; set; } = new List<CVFile>();
    public virtual ICollection<ScreeningResult> ScreeningResults { get; set; } = new List<ScreeningResult>();
}

