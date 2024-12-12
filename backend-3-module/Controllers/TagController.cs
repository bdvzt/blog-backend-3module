using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Tag;
using backend_3_module.Data.Entities;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("api/tag")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TagInfoDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<TagInfoDTO>> GetTags()
    {
        return await _tagService.GetTags();
    }

    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> CreateTag([FromBody] TagDTO tagDto)
    {
        await _tagService.CreateTag(tagDto);
        return Ok();
    }
}