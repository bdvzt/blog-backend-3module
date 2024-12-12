using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Addresses;
using backend_3_module.Data.DTO.Author;
using backend_3_module.Data.Queries;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend_3_module.Controllers;

[Route("address")]
[ApiController]
public class AddressesController : Controller
{
    private readonly IAddressesService _addressesServiceService;

    public AddressesController(IAddressesService addressesService)
    {
        _addressesServiceService = addressesService;
    }
    
    [HttpGet("chain")]
    [ProducesResponseType(typeof(List<SearchAddressDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<SearchAddressDTO>> GetChain(Guid objectGuidd)
    {
        return await _addressesServiceService.GetChain(objectGuidd);
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<SearchAddressDTO>), 200)]
    [ProducesResponseType(typeof(ErrorDTO), 400)]
    [ProducesResponseType(typeof(ErrorDTO), 500)]
    public async Task<List<SearchAddressDTO>> SearchAddress([FromQuery] SearchAddress query)
    {
        return await _addressesServiceService.Search(query);
    }
}