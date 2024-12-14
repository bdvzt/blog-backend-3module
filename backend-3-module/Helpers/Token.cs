using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_3_module.Data.DataBases;
using backend_3_module.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace backend_3_module.Helpers;

public class Token
{
    private readonly string secretKey;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RedisDbContext _redisDbContext;

    public Token(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, RedisDbContext redisDbContext)
    {
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        _httpContextAccessor = httpContextAccessor;
        _redisDbContext = redisDbContext;
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(90),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GetToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request?.Headers.TryGetValue("Authorization", out var authorizationHeader) == true)
        {
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.ToString().StartsWith("Bearer "))
            {
                return authorizationHeader.ToString().Substring("Bearer ".Length);
            }
        }

        return null;
    }

    public async Task<Guid> GetUserIdFromToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Request?.Headers.TryGetValue("Authorization", out var authorizationHeader) == true)
        {
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.ToString().StartsWith("Bearer "))
            {
                var token = authorizationHeader.ToString().Substring("Bearer ".Length);
                if (await _redisDbContext.IsBlacklisted(token))
                {
                    throw new UnauthorizedAccessException("Токен истек");
                }

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);

                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");

                    if (Guid.TryParse(userIdClaim?.Value, out var userId))
                    {
                        return userId;
                    }
                }
                catch (Exception ex)
                {
                    throw new UnauthorizedAccessException("Ошибка при извлечении ID из токена.", ex);
                }
            }
        }

        throw new UnauthorizedAccessException("Токен отсутствует или имеет неверный формат.");
    }
}