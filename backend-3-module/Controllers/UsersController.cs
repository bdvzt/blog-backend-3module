using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text;
using backend_3_module.Data;
using backend_3_module.Data.DTO;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using backend_3_module.Helpers;
using backend_3_module.Services;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace backend_3_module.Controllers;

[Route("api/account")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly TokenMiddlware _tokenMiddlwareHelper;

    public UsersController(IUserService userService, TokenMiddlware tokenMiddlwareHelper)
    {
        _userService = userService;
        _tokenMiddlwareHelper = tokenMiddlwareHelper;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponseDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<TokenResponseDTO> Register([FromBody] RegistrationDTO registrationDto)
    {
        return await _userService.Register(registrationDto);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<TokenResponseDTO> Login([FromBody] LoginDTO loginDto)
    {
        return await _userService.Login(loginDto);
    }

    [HttpGet("logout")] //TODO logout
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = _tokenMiddlwareHelper.GetToken();
        await _userService.Logout(token);
        return Ok();
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDTO), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<UserDTO> GetProfile()
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        return await _userService.GetProfile(await userId);
    }

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 401)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<IActionResult> EditProfile([FromBody] EditUserDTO editUserDto)
    {
        var userId = _tokenMiddlwareHelper.GetUserIdFromToken();
        await _userService.EditProfile(await userId, editUserDto);
        return Ok();
    }
}