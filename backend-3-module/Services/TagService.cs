using backend_3_module.Data;
using backend_3_module.Data.DTO.Tag;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using Microsoft.EntityFrameworkCore;

namespace backend_3_module.Services.IServices;

public class TagService : ITagService
{
    private readonly BlogDbContext _dbContext;

    public TagService(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TagInfoDTO>> GetTags()
    {
        var tags = await _dbContext.Tags
            .Select(tag => new TagInfoDTO
            {
                Id = tag.Id,
                CreateTime = tag.CreateTime,
                Name = tag.Name
            })
            .ToListAsync();

        return tags;
    }

    public async Task CreateTag(TagDTO tagDto)
    {
        if (await _dbContext.Tags.AnyAsync(t => t.Name == tagDto.Name))
            throw new BadRequestException("Тег с таким названием уже существует.");
        
        var newTag = new Tag
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Name = tagDto.Name
        };

        _dbContext.Tags.Add(newTag);
        await _dbContext.SaveChangesAsync();
    }
}