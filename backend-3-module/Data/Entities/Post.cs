namespace backend_3_module.Data.Entities;

public class Post
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int ReadingTime { get; set; }
    public string? Image { get; set; }
    public Guid AuthorId { get; set; }
    public string Author { get; set; }
    public Guid? CommunityId { get; set; }
    public string? CommunityName { get; set; }
    public Guid? AddressId { get; set; }
    public User User { get; set; }
    public Community Community { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<UserLikes> UserLikes { get; set; } = new List<UserLikes>();
    public ICollection<PostTags> PostTags { get; set; } = new List<PostTags>();
}