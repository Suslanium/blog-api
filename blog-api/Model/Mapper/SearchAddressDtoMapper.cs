using System.Text;
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
        
        var dtoText = new StringBuilder();
        if (from.Housetype != null)
            dtoText.Append(AddressLevelMapper.GetHouseTypeString((uint)from.Housetype));
        dtoText.Append($" {from.Housenum}");
        if (from is { Addtype1: not null, Addnum1: not null })
            dtoText.Append($" {AddressLevelMapper.GetAdditionalHouseTypeString((uint)from.Addtype1)} {from.Addnum1}");
        if (from is { Addtype2: not null, Addnum2: not null })
            dtoText.Append($" {AddressLevelMapper.GetAdditionalHouseTypeString((uint)from.Addtype2)} {from.Addnum2}");
        
        return new SearchAddressDto
        {
            ObjectGuid = from.Objectguid,
            ObjectId = from.Objectid,
            Text = dtoText.ToString(),
            ObjectLevel = addressLevel.Item1,
            ObjectLevelText = addressLevel.Item2
        };
    }
}