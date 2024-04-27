namespace MyBoards;

public class PageQueryProperties
{
    public string Filter { get; set; }
    public string SortBy { get; set; }
    public bool SortByDescending { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    // public static bool TryParse(
    //     string? value,
    //     out PageQueryProperties? pageQueryProperties
    // ) {
    //     var trimmedValue = value?.TrimStart('(').TrimEnd(')');

    // }
}
