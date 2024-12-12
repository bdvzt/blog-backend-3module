namespace backend_3_module.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string? Content { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? DeleteDate { get; set; }
    public Guid AuthorId { get; set; }
    public string Author { get; set; }
    public int SubComments { get; set; }
    public Post Post { get; set; }
    public Guid PostId { get; set; } 
}