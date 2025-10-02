using Microsoft.EntityFrameworkCore;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;
using System.Text;

namespace SmartCVFilter.API.Services;

public class CVUploadService : ICVUploadService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CVUploadService> _logger;
    private readonly IScreeningService _screeningService;

    public CVUploadService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<CVUploadService> logger,
        IScreeningService screeningService)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _screeningService = screeningService;
    }

    public async Task<string> UploadCVAsync(IFormFile file, int applicantId)
    {
        if (!ValidateCVFile(file))
        {
            throw new InvalidOperationException("Invalid file format or size.");
        }

        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "cvs");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var cvFile = new CVFile
        {
            FileName = file.FileName,
            FilePath = filePath,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileExtension = fileExtension,
            ApplicantId = applicantId,
            UploadedDate = DateTime.UtcNow,
            Status = "Uploaded"
        };

        _context.CVFiles.Add(cvFile);
        await _context.SaveChangesAsync();

        // Start text extraction in background
        _ = Task.Run(async () => await ExtractTextFromCVAsync(cvFile.Id));

        return filePath;
    }

    public async Task<bool> DeleteCVAsync(int cvFileId)
    {
        var cvFile = await _context.CVFiles.FindAsync(cvFileId);
        if (cvFile == null)
            return false;

        try
        {
            if (File.Exists(cvFile.FilePath))
            {
                File.Delete(cvFile.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", cvFile.FilePath);
        }

        _context.CVFiles.Remove(cvFile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> ExtractTextFromCVAsync(int cvFileId)
    {
        var cvFile = await _context.CVFiles.FindAsync(cvFileId);
        if (cvFile == null)
            return string.Empty;

        try
        {
            cvFile.Status = "Processing";
            await _context.SaveChangesAsync();

            string extractedText = string.Empty;

            switch (cvFile.FileExtension)
            {
                case ".pdf":
                    extractedText = await ExtractTextFromPdfAsync(cvFile.FilePath);
                    break;
                case ".doc":
                case ".docx":
                    extractedText = await ExtractTextFromWordAsync(cvFile.FilePath);
                    break;
                case ".txt":
                    extractedText = await File.ReadAllTextAsync(cvFile.FilePath);
                    break;
                default:
                    _logger.LogWarning("Unsupported file format: {FileExtension}", cvFile.FileExtension);
                    break;
            }

            cvFile.ExtractedText = extractedText;
            cvFile.Status = "Processed";
            await _context.SaveChangesAsync();

            // Automatically trigger AI screening after successful text extraction
            int applicantId = cvFile.ApplicantId; // FIX: Get applicantId from cvFile
            _ = Task.Run(async () =>
            {
                try
                {
                    // Get the applicant with job post information
                    var applicant = await _context.Applicants
                        .Include(a => a.JobPost)
                        .FirstOrDefaultAsync(a => a.Id == applicantId);

                    if (applicant != null)
                    {
                        await _screeningService.ProcessScreeningAsync(applicantId, applicant.JobPostId);
                        _logger.LogInformation("Automatic screening triggered for applicant {ApplicantId} after CV processing", applicantId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error triggering automatic screening for applicant {ApplicantId}", applicantId);
                }
            });

            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from CV file: {CvFileId}", cvFileId);
            cvFile.Status = "Error";
            await _context.SaveChangesAsync();
            return string.Empty;
        }
    }

    public async Task<byte[]> GetCVFileAsync(int cvFileId)
    {
        var cvFile = await _context.CVFiles.FindAsync(cvFileId);
        if (cvFile == null || !File.Exists(cvFile.FilePath))
        {
            throw new FileNotFoundException("CV file not found.");
        }

        return await File.ReadAllBytesAsync(cvFile.FilePath);
    }

    public bool ValidateCVFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // Check file size (max 10MB)
        var maxSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxSize)
            return false;

        // Check file extension
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return false;

        // Check content type
        var allowedContentTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain"
        };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return false;

        return true;
    }

    private async Task<string> ExtractTextFromPdfAsync(string filePath)
    {
        try
        {
            // Using iText7 for PDF text extraction
            using var pdfReader = new iText.Kernel.Pdf.PdfReader(filePath);
            using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);
            var text = new StringBuilder();

            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
            {
                var page = pdfDocument.GetPage(pageNum);
                var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                var currentText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page, strategy);
                text.AppendLine(currentText);
            }

            return text.ToString().Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
            return "Error extracting text from PDF file. Please ensure the file is not corrupted.";
        }
    }

    private async Task<string> ExtractTextFromWordAsync(string filePath)
    {
        try
        {
            // Using DocumentFormat.OpenXml for Word document text extraction
            using var wordDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(filePath, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;

            if (body == null)
                return string.Empty;

            var text = new StringBuilder();
            ExtractTextFromElement(body, text);

            return text.ToString().Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from Word document: {FilePath}", filePath);
            return "Error extracting text from Word document. Please ensure the file is not corrupted.";
        }
    }

    private void ExtractTextFromElement(DocumentFormat.OpenXml.OpenXmlElement element, StringBuilder text)
    {
        foreach (var child in element.Elements())
        {
            if (child is DocumentFormat.OpenXml.Wordprocessing.Text textElement)
            {
                text.Append(textElement.Text);
            }
            else if (child is DocumentFormat.OpenXml.Wordprocessing.Paragraph)
            {
                ExtractTextFromElement(child, text);
                text.AppendLine();
            }
            else
            {
                ExtractTextFromElement(child, text);
            }
        }
    }

    public async Task<List<CVFileStatusDto>> GetCVFileStatusesAsync(int applicantId)
    {
        try
        {
            var cvFiles = await _context.CVFiles
                .Where(cf => cf.ApplicantId == applicantId)
                .Select(cf => new CVFileStatusDto
                {
                    Id = cf.Id,
                    FileName = cf.FileName,
                    Status = cf.Status,
                    UploadedDate = cf.UploadedDate,
                    LastUpdated = cf.UploadedDate // In a real app, you'd have a LastUpdated field
                })
                .ToListAsync();

            return cvFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV file statuses for applicant {ApplicantId}", applicantId);
            return new List<CVFileStatusDto>();
        }
    }
}

