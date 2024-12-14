using backend_3_module.Data.DTO.Community;
using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Queries;

namespace backend_3_module.Services.IServices;

public interface ICommunityService
{
    public Task<List<GetCommunityDTO>> GetCommunities();
    public Task CreateCommunity(CreateCommunityDTO createCommunityDto, Guid userId);
    public Task<List<Guid>> GetIdMyCommunity(Guid? userId);
    public Task CreatePost(Guid communityId, CreatePostDTO createPostDto, Guid userId);
    public Task<PostAndPaginationDTO> GetCommunityPosts(CommunityPostFilters query, Guid communityId, Guid? userId);
    public Task<List<GetMyCommunitiesDTO>> GetMyCommunities(Guid userId);
    public Task<CommunityInfoDTO> GetCommunityInfo(Guid communityId);
    public Task<RoleDTO> GetRole(Guid userId, Guid id);
    public Task SubscribeToCommunity(Guid userId, Guid communityId);
    public Task UnsubscribeFromCommunity(Guid userId, Guid communityId);
}