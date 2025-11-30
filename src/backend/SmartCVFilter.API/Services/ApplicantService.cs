using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;
using SmartCVFilter.API.Services.Interfaces;

namespace SmartCVFilter.API.Services;

public class ApplicantService : IApplicantService
{
    private readonly ApplicationDbContext _context;
    private readonly IScreeningService _screeningService;
    private readonly ILogger<ApplicantService> _logger;

    public ApplicantService(ApplicationDbContext context, IScreeningService screeningService, ILogger<ApplicantService> logger)
    {
        _context = context;
        _screeningService = screeningService;
        _logger = logger;
    }

    private static List<string> DeserializeStringList(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
        }
        catch (System.Text.Json.JsonException)
        {
            // If JSON deserialization fails, return empty list
            return new List<string>();
        }
    }

    public async Task<ApplicantResponse> CreateApplicantAsync(CreateApplicantRequest request, int jobPostId)
    {
        var jobPost = await _context.JobPosts.FindAsync(jobPostId);
        if (jobPost == null)
            throw new InvalidOperationException("Job post not found.");

        var applicant = new Applicant
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            LinkedInProfile = request.LinkedInProfile,
            PortfolioUrl = request.PortfolioUrl,
            CoverLetter = request.CoverLetter,
            JobPostId = jobPostId,
            AppliedDate = DateTime.UtcNow,
            Status = "Applied"
        };

        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync();

        return await GetApplicantByIdAsync(applicant.Id, "", true) ?? throw new InvalidOperationException("Failed to retrieve created applicant");
    }

    public async Task<ApplicantResponse?> GetApplicantByIdAsync(int id, string userId, bool isAdmin = false)
    {
        var query = _context.Applicants
            .Include(a => a.JobPost)
            .Include(a => a.CVFiles)
            .Include(a => a.ScreeningResults)
            .AsQueryable();

        // Admin users can access any applicant, regular users can only access applicants from their job posts
        if (!isAdmin)
        {
            query = query.Where(a => a.JobPost.UserId == userId);
        }

        var applicant = await query.FirstOrDefaultAsync(a => a.Id == id);

        if (applicant == null)
            return null;

        return new ApplicantResponse
        {
            Id = applicant.Id,
            FirstName = applicant.FirstName,
            LastName = applicant.LastName,
            Email = applicant.Email,
            PhoneNumber = applicant.PhoneNumber,
            LinkedInProfile = applicant.LinkedInProfile,
            PortfolioUrl = applicant.PortfolioUrl,
            CoverLetter = applicant.CoverLetter,
            Status = applicant.Status,
            AppliedDate = applicant.AppliedDate,
            LastUpdated = applicant.LastUpdated,
            JobPostId = applicant.JobPostId,
            JobTitle = applicant.JobPost.Title,
            CVFiles = applicant.CVFiles.Select(cv => new CVFileResponse
            {
                Id = cv.Id,
                FileName = cv.FileName,
                ContentType = cv.ContentType,
                FileSize = cv.FileSize,
                FileExtension = cv.FileExtension,
                UploadedDate = cv.UploadedDate,
                Status = cv.Status
            }).ToList(),
            ScreeningResults = applicant.ScreeningResults.Select(sr => new ScreeningResultResponse
            {
                Id = sr.Id,
                OverallScore = sr.OverallScore,
                Summary = sr.Summary,
                Strengths = DeserializeStringList(sr.Strengths),
                Weaknesses = DeserializeStringList(sr.Weaknesses),
                DetailedAnalysis = sr.DetailedAnalysis,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                CompletedAt = sr.CompletedAt
            }).ToList()
        };
    }

    public async Task<IEnumerable<ApplicantResponse>> GetApplicantsByJobPostAsync(int jobPostId, string userId, bool isAdmin = false)
    {
        // Admin users can access any job post's applicants, regular users can only access their own job post's applicants
        if (!isAdmin)
        {
            var jobPost = await _context.JobPosts
                .FirstOrDefaultAsync(jp => jp.Id == jobPostId && jp.UserId == userId);

            if (jobPost == null)
                throw new UnauthorizedAccessException("You don't have access to this job post.");
        }

        var applicants = await _context.Applicants
            .Where(a => a.JobPostId == jobPostId)
            .Include(a => a.JobPost)
            .Include(a => a.CVFiles)
            .Include(a => a.ScreeningResults)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        return applicants.Select(applicant => new ApplicantResponse
        {
            Id = applicant.Id,
            FirstName = applicant.FirstName,
            LastName = applicant.LastName,
            Email = applicant.Email,
            PhoneNumber = applicant.PhoneNumber,
            LinkedInProfile = applicant.LinkedInProfile,
            PortfolioUrl = applicant.PortfolioUrl,
            CoverLetter = applicant.CoverLetter,
            Status = applicant.Status,
            AppliedDate = applicant.AppliedDate,
            LastUpdated = applicant.LastUpdated,
            JobPostId = applicant.JobPostId,
            JobTitle = applicant.JobPost.Title,
            CVFiles = applicant.CVFiles.Select(cv => new CVFileResponse
            {
                Id = cv.Id,
                FileName = cv.FileName,
                ContentType = cv.ContentType,
                FileSize = cv.FileSize,
                FileExtension = cv.FileExtension,
                UploadedDate = cv.UploadedDate,
                Status = cv.Status
            }).ToList(),
            ScreeningResults = applicant.ScreeningResults.Select(sr => new ScreeningResultResponse
            {
                Id = sr.Id,
                OverallScore = sr.OverallScore,
                Summary = sr.Summary,
                Strengths = DeserializeStringList(sr.Strengths),
                Weaknesses = DeserializeStringList(sr.Weaknesses),
                DetailedAnalysis = sr.DetailedAnalysis,
                Status = sr.Status,
                CreatedAt = sr.CreatedAt,
                CompletedAt = sr.CompletedAt
            }).ToList()
        });
    }

    public async Task<ApplicantResponse> UpdateApplicantAsync(int id, UpdateApplicantRequest request)
    {
        var applicant = await _context.Applicants.FindAsync(id);
        if (applicant == null)
            throw new InvalidOperationException("Applicant not found.");

        // Update all fields (they are all provided in the form)
        if (request.FirstName != null)
            applicant.FirstName = request.FirstName;
        if (request.LastName != null)
            applicant.LastName = request.LastName;
        if (request.Email != null)
            applicant.Email = request.Email;
        if (request.PhoneNumber != null)
            applicant.PhoneNumber = request.PhoneNumber;
        if (request.LinkedInProfile != null)
            applicant.LinkedInProfile = request.LinkedInProfile;
        if (request.PortfolioUrl != null)
            applicant.PortfolioUrl = request.PortfolioUrl;
        if (request.CoverLetter != null)
            applicant.CoverLetter = request.CoverLetter;
        if (request.Status != null)
            applicant.Status = request.Status;

        applicant.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetApplicantByIdAsync(id, "", true) ?? throw new InvalidOperationException("Failed to retrieve updated applicant");
    }

    public async Task<bool> DeleteApplicantAsync(int id)
    {
        var applicant = await _context.Applicants.FindAsync(id);
        if (applicant == null)
            return false;

        _context.Applicants.Remove(applicant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartScreeningAsync(int jobPostId, ScreeningRequest request, string userId, bool isAdmin = false)
    {
        // Admin users can start screening for any job post, regular users can only start screening for their own job posts
        if (!isAdmin)
        {
            var jobPost = await _context.JobPosts
                .FirstOrDefaultAsync(jp => jp.Id == jobPostId && jp.UserId == userId);

            if (jobPost == null)
                throw new UnauthorizedAccessException("You don't have access to this job post.");
        }

        // Verify that all applicants belong to this job post
        var applicants = await _context.Applicants
            .Where(a => request.ApplicantIds.Contains(a.Id) && a.JobPostId == jobPostId)
            .ToListAsync();

        if (applicants.Count != request.ApplicantIds.Count)
            throw new InvalidOperationException("Some applicants don't belong to this job post.");

        // Start screening for each applicant
        foreach (var applicant in applicants)
        {
            await _screeningService.ProcessScreeningAsync(applicant.Id, jobPostId);
        }

        return true;
    }

    public async Task<ApplicantPagedResponse> GetApplicantsPagedAsync(ApplicantPagedRequest request, string userId, bool isAdmin = false)
    {
        var query = _context.Applicants
            .Include(a => a.JobPost)
            .Include(a => a.CVFiles)
            .AsQueryable();

        // Apply job post filter
        query = query.Where(a => a.JobPostId == request.JobPostId);

        // Apply user filter (admin can see all, regular users see only their own job post's applicants)
        if (!isAdmin)
        {
            query = query.Where(a => a.JobPost.UserId == userId);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        if (request.AppliedFrom.HasValue)
        {
            query = query.Where(a => a.AppliedDate >= request.AppliedFrom.Value);
        }

        if (request.AppliedTo.HasValue)
        {
            query = query.Where(a => a.AppliedDate <= request.AppliedTo.Value);
        }

        // Apply search (case-insensitive)
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchPattern = $"%{request.Search}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.FirstName, searchPattern) ||
                EF.Functions.ILike(a.LastName, searchPattern) ||
                EF.Functions.ILike(a.Email, searchPattern) ||
                (a.PhoneNumber != null && EF.Functions.ILike(a.PhoneNumber, searchPattern)));
        }

        // Apply sorting
        query = ApplyApplicantSorting(query, request.SortBy, request.SortDirection);

        // Get total count
        var totalItems = await query.CountAsync();

        // Apply pagination
        var applicants = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to response
        var items = applicants.Select(a => new ApplicantResponse
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            PhoneNumber = a.PhoneNumber,
            Status = a.Status,
            AppliedDate = a.AppliedDate,
            JobPostId = a.JobPostId,
            CVFiles = a.CVFiles.Select(cf => new CVFileResponse
            {
                Id = cf.Id,
                FileName = cf.FileName,
                ContentType = cf.ContentType,
                FileSize = cf.FileSize,
                FileExtension = cf.FileExtension,
                Status = cf.Status,
                UploadedDate = cf.UploadedDate
            }).ToList()
        }).ToList();

        // Calculate statistics
        var allApplicants = await _context.Applicants
            .Where(a => a.JobPostId == request.JobPostId)
            .ToListAsync();

        var pendingApplicants = allApplicants.Count(a => a.Status == "Pending");
        var screenedApplicants = allApplicants.Count(a => a.Status == "Screened");
        var rejectedApplicants = allApplicants.Count(a => a.Status == "Rejected");

        var totalCVFiles = await _context.CVFiles
            .Where(cf => cf.Applicant.JobPostId == request.JobPostId)
            .CountAsync();

        return new ApplicantPagedResponse
        {
            Items = items,
            TotalItems = totalItems,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize),
            PendingApplicants = pendingApplicants,
            ScreenedApplicants = screenedApplicants,
            RejectedApplicants = rejectedApplicants,
            TotalCVFiles = totalCVFiles
        };
    }

    private IQueryable<Applicant> ApplyApplicantSorting(IQueryable<Applicant> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderByDescending(a => a.AppliedDate);
        }

        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(a => a.FirstName) : query.OrderBy(a => a.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(a => a.LastName) : query.OrderBy(a => a.LastName),
            "email" => isDescending ? query.OrderByDescending(a => a.Email) : query.OrderBy(a => a.Email),
            "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "applieddate" => isDescending ? query.OrderByDescending(a => a.AppliedDate) : query.OrderBy(a => a.AppliedDate),
            _ => query.OrderByDescending(a => a.AppliedDate)
        };
    }

    public async Task<IEnumerable<ApplicantResponse>> SearchApplicantsAsync(string searchTerm, string userId, bool isAdmin = false)
    {
        _logger.LogInformation("SearchApplicantsAsync called. SearchTerm: '{SearchTerm}', UserId: {UserId}, IsAdmin: {IsAdmin}",
            searchTerm, userId, isAdmin);

        try
        {
            var query = _context.Applicants
                .Include(a => a.JobPost)
                .AsQueryable();

            var totalApplicants = await _context.Applicants.CountAsync();
            _logger.LogDebug("Initial query created. Total applicants in database: {Count}", totalApplicants);

            // Apply user filter (admin can see all, regular users see only their own job post's applicants)
            if (!isAdmin)
            {
                query = query.Where(a => a.JobPost.UserId == userId);
                var userApplicantsCount = await query.CountAsync();
                _logger.LogDebug("Applied user filter. UserId: {UserId}, User's applicants count: {Count}", userId, userApplicantsCount);
            }
            else
            {
                _logger.LogDebug("Admin user - no user filter applied");
            }

            // Apply search (case-insensitive) - trim whitespace for better matching
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var trimmedSearch = searchTerm.Trim();
                var searchPattern = $"%{trimmedSearch}%";
                
                // Search across multiple fields with OR logic
                // Note: We search individual fields separately since EF doesn't support string concatenation in ILike
                query = query.Where(a =>
                    EF.Functions.ILike(a.FirstName, searchPattern) ||
                    EF.Functions.ILike(a.LastName, searchPattern) ||
                    EF.Functions.ILike(a.Email, searchPattern) ||
                    (a.PhoneNumber != null && EF.Functions.ILike(a.PhoneNumber, searchPattern)));

                _logger.LogDebug("Applied search filter. SearchTerm: '{SearchTerm}', SearchPattern: '{SearchPattern}'", trimmedSearch, searchPattern);
                
                var afterSearchCount = await query.CountAsync();
                _logger.LogDebug("After search filter, found {Count} matching applicants", afterSearchCount);
            }
            else
            {
                _logger.LogDebug("No search term provided - returning all matching applicants");
            }

            // Get all matching applicants first
            var allApplicants = await query
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            _logger.LogInformation("Query executed successfully. Found {Count} matching applicants before deduplication", allApplicants.Count);

            // Group by email (case-insensitive) to handle cases where same applicant applied to multiple jobs
            // Since one applicant can apply to many jobs, we show the most recent application for each unique email
            // This prevents showing duplicate entries for the same person
            var uniqueApplicants = allApplicants
                .GroupBy(a => a.Email.ToLowerInvariant())
                .Select(g => g.OrderByDescending(a => a.AppliedDate).First())
                .OrderByDescending(a => a.AppliedDate)
                .Take(20)
                .ToList();

            _logger.LogInformation("After grouping by email, found {Count} unique applicants (showing most recent application per email)", uniqueApplicants.Count);
            
            // Log all found emails for debugging
            if (uniqueApplicants.Any())
            {
                var foundEmails = string.Join(", ", uniqueApplicants.Select(a => a.Email));
                _logger.LogDebug("Found applicant emails: {Emails}", foundEmails);
            }

            var results = uniqueApplicants.Select(a => new ApplicantResponse
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                LinkedInProfile = a.LinkedInProfile,
                PortfolioUrl = a.PortfolioUrl,
                CoverLetter = a.CoverLetter,
                Status = a.Status,
                AppliedDate = a.AppliedDate,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost?.Title ?? "Unknown"
            }).ToList();

            // Log sample results for debugging
            if (results.Any())
            {
                _logger.LogDebug("Sample results: {SampleResults}", 
                    string.Join(", ", results.Take(3).Select(r => $"{r.FirstName} {r.LastName} ({r.Email})")));
            }
            else
            {
                _logger.LogWarning("No results found for search term: '{SearchTerm}'. Total applicants in DB: {Total}, User's applicants: {UserCount}",
                    searchTerm, totalApplicants, !isAdmin ? await _context.Applicants.CountAsync(a => a.JobPost.UserId == userId) : totalApplicants);
            }

            _logger.LogInformation("SearchApplicantsAsync completed successfully. Returning {Count} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in SearchApplicantsAsync. SearchTerm: '{SearchTerm}', UserId: {UserId}, IsAdmin: {IsAdmin}, ExceptionType: {ExceptionType}, Message: {Message}",
                searchTerm, userId, isAdmin, ex.GetType().Name, ex.Message);

            // Re-throw to be handled by the controller
            throw;
        }
    }
}

