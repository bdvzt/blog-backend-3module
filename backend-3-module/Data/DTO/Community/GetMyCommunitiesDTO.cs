namespace backend_3_module.Data.DTO.Community;

public class GetMyCommunitiesDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Role Role { get; set; }
}