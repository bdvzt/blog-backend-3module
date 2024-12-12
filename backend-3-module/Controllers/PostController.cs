using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.Queries;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("api/post")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly TokenMiddlware _tokenMiddlwareHelper;

    public PostController(IPostService postService, TokenMiddlware tokenMiddlwareHelper)
    {
        _postService = postService;
        _tokenMiddlwareHelper = tokenMiddlwareHelper;
    }

    [HttpGet()]
    [ProducesResponseType(typeof(PostAndPaginationDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<PostAndPaginationDTO> GetAllPosts([FromQuery] AllPostFilters query)
    {
        Guid? userId = null;

        try
        {
            userId = await _tokenMiddlwareHelper.GetUserIdFromToken();
        }
        catch (UnauthorizedAccessException)
        {
        }

        return await _postService.GetAllPosts(query, userId);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PostInfoDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<PostInfoDTO> GetPostInfo(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        return await _postService.GetPostInfo(id, await userId);
    }

    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDTO postDto)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _postService.CreatePost(postDto, await userId);
        return Ok();
    }

    [HttpPost("{postId}/like")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> LikePost(Guid postId)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _postService.LikePost(postId, await userId);
        return Ok();
    }

    [HttpDelete("{postId}/unlike")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> UnlikePost(Guid postId)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _postService.UnlikePost(postId, await userId);
        return Ok();
    }
}