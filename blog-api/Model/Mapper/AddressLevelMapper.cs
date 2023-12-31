﻿using blog_api.Data.Models;

namespace blog_api.Model.Mapper;

public static class AddressLevelMapper
{
    public static Tuple<GarAddressLevel, string> GetAddressLevel(uint level)
        => level switch
        {
            1 => new Tuple<GarAddressLevel, string>(GarAddressLevel.Region, "Субъект РФ"),
            2 => new Tuple<GarAddressLevel, string>(GarAddressLevel.AdministrativeArea, "Административный район"),
            3 => new Tuple<GarAddressLevel, string>(GarAddressLevel.MunicipalArea, "Муниципальный район"),
            4 => new Tuple<GarAddressLevel, string>(GarAddressLevel.RuralUrbanSettlement,
                "Сельское/городское поселение"),
            5 => new Tuple<GarAddressLevel, string>(GarAddressLevel.City, "Город"),
            6 => new Tuple<GarAddressLevel, string>(GarAddressLevel.Locality, "Населенный пункт"),
            7 => new Tuple<GarAddressLevel, string>(GarAddressLevel.ElementOfPlanningStructure,
                "Элемент планировочной структуры"),
            8 => new Tuple<GarAddressLevel, string>(GarAddressLevel.ElementOfRoadNetwork,
                "Элемент улично-дорожной сети"),
            9 => new Tuple<GarAddressLevel, string>(GarAddressLevel.Land, "Земельный участок"),
            10 => new Tuple<GarAddressLevel, string>(GarAddressLevel.Building, "Здание (сооружение)"),
            11 => new Tuple<GarAddressLevel, string>(GarAddressLevel.Room, "Помещение"),
            12 => new Tuple<GarAddressLevel, string>(GarAddressLevel.RoomInRooms, "Помещения в пределах помещения"),
            13 => new Tuple<GarAddressLevel, string>(GarAddressLevel.AutonomousRegionLevel,
                "Уровень автономного округа"),
            14 => new Tuple<GarAddressLevel, string>(GarAddressLevel.IntracityLevel,
                "Уровень внутригородской территории"),
            15 => new Tuple<GarAddressLevel, string>(GarAddressLevel.AdditionalTerritoriesLevel,
                "Уровень дополнительных территорий"),
            16 => new Tuple<GarAddressLevel, string>(GarAddressLevel.LevelOfObjectsInAdditionalTerritories,
                "Уровень объектов на дополнительных территориях"),
            17 => new Tuple<GarAddressLevel, string>(GarAddressLevel.CarPlace, "Машиноместо"),
            _ => throw new ArgumentException()
        };

    public static string GetHouseTypeString(uint number)
        => number switch
        {
            1 => "владение",
            2 => "дом",
            3 => "домовладение",
            4 => "гараж",
            5 => "здание",
            6 => "шахта",
            7 => "строение",
            8 => "сооружение",
            9 => "литера",
            10 => "корпус",
            11 => "подвал",
            12 => "котельная",
            13 => "погреб",
            14 => "объект незавершенного строительства (ОНС)",
            _ => throw new ArgumentException()
        };

    public static string GetAdditionalHouseTypeString(uint number)
        => number switch
        {
            1 => "корпус",
            2 => "строение",
            3 => "сооружение",
            4 => "литера",
            _ => throw new ArgumentException()
        };
}