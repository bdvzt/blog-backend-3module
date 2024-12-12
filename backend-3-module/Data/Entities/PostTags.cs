namespace backend_3_module.Data.Entities;

public class PostTags
{
    public Guid PostId { get; set; }
    public Post Post { get; set; }
    
    public Guid TagId { get; set; }
    public Tag Tag { get; set; }
}