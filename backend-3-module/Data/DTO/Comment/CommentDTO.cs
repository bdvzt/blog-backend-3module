using System.ComponentModel.DataAnnotations;

namespace backend_3_module.Data.DTO.Comment;

public class CommentDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Content { get; set; }
    public Guid? ParentId { get; set; }
}