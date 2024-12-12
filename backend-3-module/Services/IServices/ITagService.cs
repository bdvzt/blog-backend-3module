using backend_3_module.Data.DTO.Tag;
using backend_3_module.Data.Entities;

namespace backend_3_module.Services.IServices;

public interface ITagService
{
    public Task<List<TagInfoDTO>> GetTags();
    public Task CreateTag(TagDTO tagDto);
}