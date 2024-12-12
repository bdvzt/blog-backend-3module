using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Author;
using backend_3_module.Data.Errors;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("api/author")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(List<AuthorDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<AuthorDTO>> GetAuthors()
    {
        return await _authorService.GetAuthors();
    }
}