using backend_3_module.Data.Entities;

namespace backend_3_module.Data.DTO.Community;

public class CommunityInfoDTO
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsClosed { get; set; }
    public int SubscribersCount { get; set; }
    public List<UserDTO> Administrators { get; set; }
}