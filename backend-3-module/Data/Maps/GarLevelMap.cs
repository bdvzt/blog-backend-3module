namespace backend_3_module.Data.Maps;

public static class GarLevelMap
{
    public static readonly Dictionary<string, GarAddressLevel> Levels = new Dictionary<string, GarAddressLevel>
    {
        { "1", GarAddressLevel.Region },
        { "2", GarAddressLevel.AdministrativeArea },
        { "3", GarAddressLevel.MunicipalArea },
        { "4", GarAddressLevel.RuralUrbanSettlement },
        { "5", GarAddressLevel.City },
        { "6", GarAddressLevel.Locality },
        { "7", GarAddressLevel.ElementOfPlanningStructure },
        { "8", GarAddressLevel.ElementOfRoadNetwork },
        { "9", GarAddressLevel.Land },
        { "10", GarAddressLevel.Building },
        { "11", GarAddressLevel.Room },
        { "12", GarAddressLevel.RoomInRooms },
        { "13", GarAddressLevel.AutonomousRegionLevel },
        { "14", GarAddressLevel.IntracityLevel },
        { "15", GarAddressLevel.AdditionalTerritoriesLevel },
        { "16", GarAddressLevel.LevelOfObjectsInAdditionalTerritories },
        { "17", GarAddressLevel.CarPlace }
    };
}