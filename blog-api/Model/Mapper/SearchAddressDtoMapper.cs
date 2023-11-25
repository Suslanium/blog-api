using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class SearchAddressDtoMapper
{
    public static SearchAddressDto GetSearchAddressDto(AsAddrObj from)
    {
        var addressLevel = AddressLevelMapper.GetAddressLevel(uint.Parse(from.Level));
        return new SearchAddressDto
        {
            ObjectGuid = from.Objectguid,
            ObjectId = from.Objectid,
            Text = $"{from.Typename} {from.Name}",
            ObjectLevel = addressLevel.Item1,
            ObjectLevelText = addressLevel.Item2
        };
    }
    
    public static SearchAddressDto GetSearchAddressDto(AsHouse from)
    {
        var addressLevel = AddressLevelMapper.GetAddressLevel(10);
        return new SearchAddressDto
        {
            ObjectGuid = from.Objectguid,
            ObjectId = from.Objectid,
            Text = from.Housenum,
            ObjectLevel = addressLevel.Item1,
            ObjectLevelText = addressLevel.Item2
        };
    }
}