using System.ComponentModel.DataAnnotations;

namespace backend_3_module.Data.DTO.Comment;

public class CommentContentDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(5000)]
    public string Content { get; set; }
}