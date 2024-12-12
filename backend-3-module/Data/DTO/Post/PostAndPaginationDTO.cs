namespace backend_3_module.Data.DTO.Post;

public class PostAndPaginationDTO
{
    public List<PostDTO> Posts { get; set; } 
    public PageInfoDTO PageInfo { get; set; } 
}