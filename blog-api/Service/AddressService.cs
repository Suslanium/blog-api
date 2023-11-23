using blog_api.Data;
using blog_api.Model;
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
                    (hierarchyElement, obj) => new SearchAddressDto
                    {
                        ObjectGuid = obj.Objectguid,
                        ObjectId = obj.Objectid,
                        Text = $"{obj.Typename} {obj.Name}"
                    }
                ).ToListAsync();

        var filteredHouseObjects = dbContext.AsHouses.Where(obj =>
            obj.Isactual == 1 &&
            (loweredQuery == null || obj.Housenum != null && obj.Housenum.ToLower().Contains(loweredQuery)));
        
        var resultHouseObjects =
            await dbContext.AsAdmHierarchies.Where(hierarchyElement => hierarchyElement.Parentobjid == parentObjectId)
                .Join(
                    filteredHouseObjects,
                    hierarchyElement => hierarchyElement.Objectid,
                    obj => obj.Objectid,
                    (hierarchyElement, obj) => new SearchAddressDto
                    {
                        ObjectGuid = obj.Objectguid,
                        ObjectId = obj.Objectid,
                        Text = obj.Housenum
                    }
                ).ToListAsync();

        return resultAddressObjects.Concat(resultHouseObjects);
    }

    public async Task<List<SearchAddressDto>> Chain(Guid objectGuid)
    {
        throw new NotImplementedException();
    }
}