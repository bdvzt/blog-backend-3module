using backend_3_module.Data;
using backend_3_module.Data.DTO.Addresses;
using backend_3_module.Data.Maps;
using backend_3_module.Data.Queries;
using backend_3_module.Models.Address;
using backend_3_module.Services.IServices;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = backend_3_module.Data.Errors.KeyNotFoundException;

namespace backend_3_module.Services;

public class AddressesService : IAddressesService
{
    private readonly AddressDbContext _garDbContext;

    public AddressesService(AddressDbContext garDbContext)
    {
        _garDbContext = garDbContext;
    }

    public async Task<List<SearchAddressDTO>> GetChain(Guid objectGuid)
    {
        var address = await _garDbContext.AsAddrObjs
            .FirstOrDefaultAsync(address => address.Objectguid == objectGuid);

        var house = await _garDbContext.AsHouses
            .FirstOrDefaultAsync(house => house.Objectguid == objectGuid);
        string? pathWithDots;
        if (house != null)
        {
            pathWithDots = await _garDbContext.AsAdmHierarchies
                .Where(h => h.Objectid == house.Objectid)
                .Select(h => h.Path)
                .FirstOrDefaultAsync();
        }
        else if (address != null)
        {
            pathWithDots = await _garDbContext.AsAdmHierarchies
                .Where(a => a.Objectid == address.Objectid)
                .Select(a => a.Path)
                .FirstOrDefaultAsync();
        }
        else
        {
            throw new KeyNotFoundException($"Адрес не найден по uuid {objectGuid}");
        }

        var path = pathWithDots?.Split('.')
            .Select(long.Parse)
            .ToArray();

        var chain = new List<SearchAddressDTO>();

        if (path != null)
            foreach (var objId in path)
            {
                var addr = await _garDbContext.AsAddrObjs.FirstOrDefaultAsync(a => a.Objectid == objId);
                var searchAddressDto = new SearchAddressDTO();
                if (addr != null)
                {
                    searchAddressDto.Objectid = addr.Objectid;
                    searchAddressDto.Objectguid = addr.Objectguid;
                    searchAddressDto.Text = addr.Typename + " " + addr.Name;
                    searchAddressDto.ObjectLevel = GarLevelMap.Levels[addr.Level].ToString();
                    searchAddressDto.ObjectLevelText = GarLevelTranslation.Levels[addr.Level];
                }
                else
                {
                    var hou = await _garDbContext.AsHouses.FirstOrDefaultAsync(h => h.Objectid == objId);
                    if (hou != null)
                    {
                        searchAddressDto.Objectid = hou.Objectid;
                        searchAddressDto.Objectguid = hou.Objectguid;
                        searchAddressDto.Text = hou.Housenum;
                        searchAddressDto.ObjectLevel = GarLevelMap.Levels["10"].ToString();
                        searchAddressDto.ObjectLevelText = GarLevelTranslation.Levels["10"];
                    }
                }

                chain.Add(searchAddressDto);
            }

        return chain;
    }

    private Task<List<SearchAddressDTO>> GetAddressObjectsAsync(SearchAddress query)
    {
        var levelMapping = GarLevelMap.Levels;
        var levelTranslation = GarLevelTranslation.Levels;

        return _garDbContext.AsAdmHierarchies
            .Join(_garDbContext.AsAddrObjs,
                asAdmHierarchy => asAdmHierarchy.Objectid,
                asAddrObj => asAddrObj.Objectid,
                (asAdmHierarchy, asAddrObj) => new { asAdmHierarchy, asAddrObj })
            .Where(joined =>
                joined.asAdmHierarchy.Parentobjid == query.ParentObjectid)
            .Select(joined => new SearchAddressDTO
            {
                Objectid = joined.asAddrObj.Objectid,
                Objectguid = joined.asAddrObj.Objectguid,
                Text = joined.asAddrObj.Typename + " " + joined.asAddrObj.Name,
                ObjectLevel = levelMapping.ContainsKey(joined.asAddrObj.Level)
                    ? levelMapping[joined.asAddrObj.Level].ToString()
                    : "-",
                ObjectLevelText = levelTranslation.ContainsKey(joined.asAddrObj.Level)
                    ? levelTranslation[joined.asAddrObj.Level]
                    : "-"
            })
            .ToListAsync();
    }

    private Task<List<SearchAddressDTO>> GetHousesAsync(SearchAddress query)
    {
        var levelMapping = GarLevelMap.Levels;
        var levelTranslation = GarLevelTranslation.Levels;

        return _garDbContext.AsAdmHierarchies
            .Join(_garDbContext.AsHouses,
                asAdmHierarchy => asAdmHierarchy.Objectid,
                asHouse => asHouse.Objectid,
                (asAdmHierarchy, asHouse) => new { asAdmHierarchy, asHouse })
            .Where(joined =>
                joined.asAdmHierarchy.Parentobjid == query.ParentObjectid)
            .Select(joined => new SearchAddressDTO
            {
                Objectid = joined.asHouse.Objectid,
                Objectguid = joined.asHouse.Objectguid,
                Text = joined.asHouse.Housenum,
                ObjectLevel = levelMapping.ContainsKey("10")
                    ? levelMapping["10"].ToString()
                    : "-",
                ObjectLevelText = levelTranslation.ContainsKey("10")
                    ? levelTranslation["10"]
                    : "-"
            })
            .ToListAsync();
    }

    public async Task<List<SearchAddressDTO>> Search(SearchAddress query)
    {
        if (query.ParentObjectid == null)
            query.ParentObjectid = 0;

        var addresses = await GetAddressObjectsAsync(query);
        var houses = await GetHousesAsync(query);
        var result = addresses.Concat(houses).ToList();
        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            result = result.Where(r =>
                    r.ObjectLevelText != null && r.Text != null && (r.Text.Contains(query.Text) ||
                                                                    r.ObjectLevelText.Contains(query.Text)))
                .ToList();
        }

        return result;
    }
}