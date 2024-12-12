namespace backend_3_module.Data.Queries;

public class CommunityPostFilters
{
    public List<Guid>? Tags { get; set; } = null;
    public Sorting? Sorting { get; set; } = null;
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 5;
    public int? MinReadingTime { get; set; } = null;
    public int? MaxReadingTime { get; set; } = null;
}