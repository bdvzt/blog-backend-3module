using backend_3_module.Data;
using backend_3_module.Data.DTO.Author;
using Microsoft.EntityFrameworkCore;

namespace backend_3_module.Services.IServices;

public class AuthorService : IAuthorService
{
    private readonly BlogDbContext _dbContext;

    public AuthorService(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AuthorDTO>> GetAuthors()
    {
        var authors = await _dbContext.Users
            .Include(u => u.MyPosts)
            .Where(u => u.MyPosts.Count > 0)
            .Include(u => u.UserLikes)
            .Select(a => new AuthorDTO
            {
                Id = a.Id,
                FullName = a.FullName,
                BirthDate = a.BirthDate,
                Gender = a.Gender,
                Posts = a.MyPosts.Count,
                Likes = a.UserLikes.Count,
                CreateTime = a.CreateTime
            })
            .OrderBy(a => a.FullName)
            .ToListAsync();
        return authors;
    }
}