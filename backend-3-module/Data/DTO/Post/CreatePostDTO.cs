using System.ComponentModel.DataAnnotations;
using backend_3_module.Data.DTO.Tag;

namespace backend_3_module.Data.DTO.Post;

public class CreatePostDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Title { get; set; }
    [Required]
    [MinLength(1)]
    public string Description { get; set; }
    [Required]
    [Range(1, 1000)]
    public int ReadingTime { get; set; }
    public string? Image { get; set; }
    public Guid? AddressId { get; set; }
    public List<Guid>? Tags { get; set; }
}