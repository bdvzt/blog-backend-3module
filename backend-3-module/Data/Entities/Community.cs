using System.ComponentModel.DataAnnotations.Schema;
using backend_3_module.Data.DTO;

namespace backend_3_module.Data.Entities;

public class Community
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsClosed { get; set; }
    public ICollection<Post> CommunityPosts { get; set; } = new List<Post>();
    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();
}