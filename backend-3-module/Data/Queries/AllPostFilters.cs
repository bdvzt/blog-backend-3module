using System.ComponentModel.DataAnnotations;

namespace backend_3_module.Data.Queries;

public class AllPostFilters
{
    public List<Guid>? Tags { get; set; } = null;
    public string? Author { get; set; } = null;
    public Sorting? Sorting { get; set; } = null;
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть больше 0.")]
    public int? PageNumber { get; set; } = 1;
    
    [Range(1, 100, ErrorMessage = "Количество постов должно быть в диапазоне от 1 до 100.")]//todo 
    public int? PageSize { get; set; } = 5;
    public bool? OnlyMyCommunities { get; set; } = false;
    public int? MinReadingTime { get; set; } = null;
    public int? MaxReadingTime { get; set; } = null;
}