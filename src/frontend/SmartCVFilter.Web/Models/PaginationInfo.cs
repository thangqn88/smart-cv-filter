namespace SmartCVFilter.Web.Models;

public class PaginationInfo
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalRecords { get; set; } = 0;
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    public int StartRecord => TotalRecords == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int EndRecord => Math.Min(CurrentPage * PageSize, TotalRecords);

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;
}
