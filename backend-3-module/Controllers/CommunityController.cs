using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Community;
using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.Queries;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("api/community")]
[ApiController]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;
    private readonly TokenMiddlware _tokenMiddlwareHelper;

    public CommunityController(ICommunityService communityService, TokenMiddlware tokenMiddlwareHelper)
    {
        _communityService = communityService;
        _tokenMiddlwareHelper = tokenMiddlwareHelper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetCommunityDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<GetCommunityDTO>> GetCommunities()
    {
        return await _communityService.GetCommunities();
    }
    
    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> CreateCommunity([FromBody] CreateCommunityDTO community)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _communityService.CreateCommunity(community, await userId);
        return Ok();
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<GetMyCommunitiesDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<GetMyCommunitiesDTO>> GetMyCommunities()
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        return await _communityService.GetMyCommunities(await userId);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CommunityInfoDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<CommunityInfoDTO> GetCommunityInfo(Guid id)
    {
        return await _communityService.GetCommunityInfo(id);
    }

    [HttpGet("{id}/post")]
    [Authorize]
    [ProducesResponseType(typeof(PostAndPaginationDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<PostAndPaginationDTO> GetCommunityPosts([FromQuery] CommunityPostFilters query, Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        return await _communityService.GetCommunityPosts(query, id, await userId);
    }

    [HttpPost("{id}/post")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> CreatePost(Guid id, [FromBody] CreatePostDTO createPostDto)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _communityService.CreatePost(id, createPostDto, await userId);
        return Ok();
    }

    [HttpGet("{id}/role")]
    [Authorize]
    [ProducesResponseType(typeof(RoleDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<RoleDTO> GetRole(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken(); 
        return await _communityService.GetRole(await userId, id);
    }

    [HttpPost("{id}/subscribe")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> Subcribe(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _communityService.SubscribeToCommunity(await userId, id);
        return Ok();
    }

    [HttpDelete("{id}/unsubscribe")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> Unsubcribe(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _communityService.UnsubscribeFromCommunity(await userId, id);
        return Ok();
    }
}