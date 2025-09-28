using System.ComponentModel.DataAnnotations;

namespace SmartCVFilter.API.Models;

public class CVFile
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [StringLength(50)]
    public string FileExtension { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ExtractedText { get; set; } // Text extracted from PDF/DOC files

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Uploaded"; // Uploaded, Processing, Processed, Error

    // Foreign Keys
    [Required]
    public int ApplicantId { get; set; }

    // Navigation properties
    public virtual Applicant Applicant { get; set; } = null!;
}

