namespace SmartCVFilter.API.Configuration;

public class FileUploadSettings
{
    public long MaxFileSize { get; set; } = 10485760; // 10MB
    public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx", ".txt" };
    public string UploadPath { get; set; } = "uploads/cvs";
}

