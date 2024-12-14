namespace backend_3_module.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string? PhoneNumber { get; set; }
    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();
    public ICollection<Post> MyPosts { get; set; } = new List<Post>();
    public ICollection<UserLikes> UserLikes { get; set; } = new List<UserLikes>();
}