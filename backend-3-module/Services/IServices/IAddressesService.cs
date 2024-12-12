using backend_3_module.Data.DTO.Addresses;
using backend_3_module.Data.Queries;

namespace backend_3_module.Services.IServices;

public interface IAddressesService
{
    public Task<List<SearchAddressDTO>> GetChain(Guid objectId);
    public Task<List<SearchAddressDTO>> Search(SearchAddress query);
}