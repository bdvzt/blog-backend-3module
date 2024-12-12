using backend_3_module.Data.DTO.Author;

namespace backend_3_module.Services.IServices;

public interface IAuthorService
{
    public Task<List<AuthorDTO>> GetAuthors();
}