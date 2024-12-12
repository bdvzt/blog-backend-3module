namespace backend_3_module.Data.Entities;

public class CommunityUser
{
    public Guid CommunityId { get; set; }
    public Community Community { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Role Role { get; set; }
}