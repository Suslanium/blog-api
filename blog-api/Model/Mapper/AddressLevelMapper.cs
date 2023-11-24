using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class AddressLevelMapper
{
    public static Tuple<GarAddressLevel, String> GetAddressLevel(uint level)
    {
        switch (level)
        {
            case 1 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.Region, "Субъект РФ");
            case 2 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.AdministrativeArea, "Административный район");
            case 3 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.MunicipalArea, "Муниципальный район");
            case 4 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.RuralUrbanSettlement, "Сельское/городское поселение");
            case 5 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.City, "Город");
            case 6 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.Locality, "Населенный пункт");
            case 7 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.ElementOfPlanningStructure, "Элемент планировочной структуры");
            case 8 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.ElementOfRoadNetwork, "Элемент улично-дорожной сети");
            case 9 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.Land, "Земельный участок");
            case 10 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.Building, "Здание (сооружение)");
            case 11 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.Room, "Помещение");
            case 12 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.RoomInRooms, "Помещения в пределах помещения");
            case 13 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.AutonomousRegionLevel, "Уровень автономного округа");
            case 14 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.IntracityLevel, "Уровень внутригородской территории");
            case 15 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.AdditionalTerritoriesLevel, "Уровень дополнительных территорий");
            case 16 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.LevelOfObjectsInAdditionalTerritories, "Уровень объектов на дополнительных территориях");
            case 17 : return new Tuple<GarAddressLevel, string>
                (GarAddressLevel.CarPlace, "Машиноместо");
            default: throw new ArgumentException();
        }
    }
}