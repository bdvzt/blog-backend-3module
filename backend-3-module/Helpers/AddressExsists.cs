using backend_3_module.Data;
using backend_3_module.Data.DTO.Addresses;
using backend_3_module.Models.Address;
using backend_3_module.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend_3_module.Helpers;

public class AddressExsists
{
    private readonly AddressDbContext _garDbContextContext;

    public AddressExsists(AddressDbContext garDbContextContext)
    {
        _garDbContextContext = garDbContextContext;
    }

    public async Task<bool> AddressExistsAsync(Guid addressId)
    {
        var houses = await _garDbContextContext.AsAdmHierarchies
            .Join(_garDbContextContext.AsHouses,
                asAdmHierarchy => asAdmHierarchy.Objectid,
                asHouse => asHouse.Objectid,
                (asAdmHierarchy, asHouse) => new { asAdmHierarchy, asHouse })
            .Select(h => new GarAddressDTO
            {
                Objectguid = h.asHouse.Objectguid
            })
            .ToListAsync();

        var addresses = await _garDbContextContext.AsAdmHierarchies
            .Join(_garDbContextContext.AsAddrObjs,
                asAdmHierarchy => asAdmHierarchy.Objectid,
                asAddrObj => asAddrObj.Objectid,
                (asAdmHierarchy, asAddrObj) => new { asAdmHierarchy, asAddrObj })
            .Select(a => new GarAddressDTO
            {
                Objectguid = a.asAddrObj.Objectguid
            })
            .ToListAsync();

        var gar = addresses.Concat(houses).ToList();

        return gar.Any(g => g.Objectguid == addressId);
    }
}