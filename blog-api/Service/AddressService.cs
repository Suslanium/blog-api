using blog_api.Data;
using blog_api.Exception;
using blog_api.Model;
using blog_api.Model.Mapper;
using Microsoft.EntityFrameworkCore;

namespace blog_api.Service;

public class AddressService(FiasDbContext dbContext) : IAddressService
{
    public async Task<IEnumerable<SearchAddressDto>> Search(long parentObjectId, string? query)
    {
        var loweredQuery = query?.ToLower();
        var filteredAddressObjects = dbContext.AsAddrObjs.Where(obj =>
            obj.Isactual == 1 && (loweredQuery == null || obj.Name.ToLower().Contains(loweredQuery)));

        var resultAddressObjects =
            await dbContext.AsAdmHierarchies.Where(hierarchyElement => hierarchyElement.Parentobjid == parentObjectId)
                .Join(
                    filteredAddressObjects,
                    hierarchyElement => hierarchyElement.Objectid,
                    obj => obj.Objectid,
                    (hierarchyElement, obj) => SearchAddressDtoMapper.GetSearchAddressDto(obj)
                ).Take(10).ToListAsync();

        var filteredHouseObjects = dbContext.AsHouses.Where(obj =>
            obj.Isactual == 1 &&
            (loweredQuery == null || obj.Housenum != null && obj.Housenum.ToLower().Contains(loweredQuery)));

        var resultHouseObjects =
            await dbContext.AsAdmHierarchies.Where(hierarchyElement => hierarchyElement.Parentobjid == parentObjectId)
                .Join(
                    filteredHouseObjects,
                    hierarchyElement => hierarchyElement.Objectid,
                    obj => obj.Objectid,
                    (hierarchyElement, obj) => SearchAddressDtoMapper.GetSearchAddressDto(obj)
                ).Take(10).ToListAsync();

        return resultAddressObjects.Concat(resultHouseObjects);
    }

    public async Task<List<SearchAddressDto>> Chain(Guid objectGuid)
    {
        var addressObjects = dbContext.AsAddrObjs.Where(obj => obj.Objectguid == objectGuid && obj.Isactual == 1)
            .Select(obj => obj.Objectid);
        var houses = dbContext.AsHouses.Where(house => house.Objectguid == objectGuid && house.Isactual == 1)
            .Select(house => house.Objectid);
        var foundObjects = await addressObjects.Concat(houses).ToListAsync();

        if (foundObjects.Count < 1)
            throw new BlogApiArgumentException("Object with specified guid does not exist");
        var objectList = (await dbContext.AsAdmHierarchies
                .Where(hierarchyElement => hierarchyElement.Objectid == foundObjects[0])
                .OrderByDescending(hierarchyElement => hierarchyElement.Isactive)
                .Select(hierarchyElement => hierarchyElement.Path).ToListAsync())[0]
            ?.Split('.');

        if (objectList == null)
            throw new ArgumentNullException();
        var result = new List<SearchAddressDto>();

        foreach (var objectId in objectList)
        {
            var convertedObjectId = long.Parse(objectId);
            var addressObject =
                await dbContext.AsAddrObjs.FirstOrDefaultAsync(obj => obj.Objectid == convertedObjectId);
            if (addressObject != null)
            {
                result.Add(SearchAddressDtoMapper.GetSearchAddressDto(addressObject));
                continue;
            }

            var houseObject =
                await dbContext.AsHouses.FirstOrDefaultAsync(house => house.Objectid == convertedObjectId);
            if (houseObject != null)
            {
                result.Add(SearchAddressDtoMapper.GetSearchAddressDto(houseObject));
            }
        }

        return result;
    }
}