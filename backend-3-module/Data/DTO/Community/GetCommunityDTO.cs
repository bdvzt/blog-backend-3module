namespace backend_3_module.Data.DTO.Community;

public class GetCommunityDTO
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsClosed { get; set; }
    public int SubscribersCount { get; set; }
}