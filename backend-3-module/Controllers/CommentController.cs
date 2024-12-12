using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Comment;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("api/comment")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly TokenMiddlware _tokenMiddlwareHelper;

    public CommentController(ICommentService commentService, TokenMiddlware tokenMiddlwareHelper)
    {
        _commentService = commentService;
        _tokenMiddlwareHelper = tokenMiddlwareHelper;
    }

    [HttpGet("{id}/tree")]
    [Authorize]
    [ProducesResponseType(typeof(List<CommentInfoDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<CommentInfoDTO>> GetReplies(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        return await _commentService.GetAllNestedComments(id, await userId);
    }

    [HttpPost("post/{id}/create")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] CommentDTO commentDto)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _commentService.AddComment(id, await userId, commentDto);
        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> EditComment(Guid id, [FromBody] CommentContentDTO commentContentDto)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _commentService.EditComment(id, await userId, commentContentDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _commentService.DeleteComment(id, await userId);
        return Ok();
    }
}