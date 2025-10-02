using Microsoft.AspNetCore.Http;

namespace SmartCVFilter.API.Services.Interfaces;

public interface ICVUploadService
{
    Task<string> UploadCVAsync(IFormFile file, int applicantId);
    Task<bool> DeleteCVAsync(int cvFileId);
    Task<string> ExtractTextFromCVAsync(int cvFileId);
    Task<byte[]> GetCVFileAsync(int cvFileId);
    bool ValidateCVFile(IFormFile file);
    Task<List<CVFileStatusDto>> GetCVFileStatusesAsync(int applicantId);
}

public class CVFileStatusDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}

