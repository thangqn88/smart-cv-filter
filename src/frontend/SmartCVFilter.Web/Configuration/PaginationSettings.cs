namespace SmartCVFilter.Web.Configuration;

/// <summary>
/// Configuration settings for pagination
/// </summary>
public class PaginationSettings
{
    public const string SectionName = "Pagination";

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Maximum allowed page size
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Minimum allowed page size
    /// </summary>
    public int MinPageSize { get; set; } = 1;

    /// <summary>
    /// Available page size options for UI
    /// </summary>
    public int[] PageSizeOptions { get; set; } = { 5, 10, 25, 50, 100 };
}
