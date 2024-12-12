using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Queries;

namespace backend_3_module.Services.IServices;

public interface IPostService
{
    public Task<PostAndPaginationDTO> GetAllPosts(AllPostFilters query, Guid? userId);
    public Task CreatePost(CreatePostDTO postDto, Guid userId);
    public Task<PostInfoDTO> GetPostInfo(Guid postId, Guid userId);
    public Task LikePost(Guid postId, Guid userId);
    public Task UnlikePost(Guid postId, Guid userId);
}