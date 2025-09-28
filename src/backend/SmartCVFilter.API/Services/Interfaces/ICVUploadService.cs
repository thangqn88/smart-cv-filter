using Microsoft.AspNetCore.Http;

namespace SmartCVFilter.API.Services.Interfaces;

public interface ICVUploadService
{
    Task<string> UploadCVAsync(IFormFile file, int applicantId);
    Task<bool> DeleteCVAsync(int cvFileId);
    Task<string> ExtractTextFromCVAsync(int cvFileId);
    Task<byte[]> GetCVFileAsync(int cvFileId);
    Task<bool> ValidateCVFileAsync(IFormFile file);
}

