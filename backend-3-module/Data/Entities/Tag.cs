namespace backend_3_module.Data.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public ICollection<PostTags> PostTags { get; set; } = new List<PostTags>();
}