namespace backend_3_module.Data.Entities;

public class UserLikes
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid PostId { get; set; }
    public Post Post { get; set; }
}