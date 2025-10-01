using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.API.Models;

public class ScreeningResult
{
    public int Id { get; set; }

    [Required]
    public int ApplicantId { get; set; }

    [Required]
    public int JobPostId { get; set; }

    [Required]
    [Range(0, 100)]
    public int OverallScore { get; set; }

    [Required]
    [StringLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Strengths { get; set; } = string.Empty; // JSON array of strengths

    [Required]
    public string Weaknesses { get; set; } = string.Empty; // JSON array of weaknesses

    [Required]
    [StringLength(2000)]
    public string DetailedAnalysis { get; set; } = string.Empty; // Detailed AI analysis

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Processing"; // Processing, Completed, Failed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Applicant Applicant { get; set; } = null!;
    public virtual JobPost JobPost { get; set; } = null!;
}

