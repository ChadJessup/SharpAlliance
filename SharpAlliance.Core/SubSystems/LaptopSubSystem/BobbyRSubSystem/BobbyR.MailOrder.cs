namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

public partial class BobbyR
{
    public static void GameInitBobbyRMailOrder()
    {
        gubSelectedLight = 0;

        gpNewBobbyrShipments.Clear();
        giNumberOfNewBobbyRShipment = 0;
    }
}

public struct BobbyROrderLocationStruct
{
    public string psCityLoc;
    public int usOverNightExpress;
    public int us2DaysService;
    public int usStandardService;
}

//drop down menu
public enum BR_DROP_DOWN
{
    NO_ACTION,
    CREATE,
    DESTROY,
    DISPLAY,
};


//enums for the various destinations that are available in the bobbyR dest drop down box
public enum BR
{
    AUSTIN,
    BAGHDAD,
    DRASSEN,
    HONG_KONG,
    BEIRUT,
    LONDON,
    LOS_ANGELES,
    MEDUNA,
    METAVIRA,
    MIAMI,
    MOSCOW,
    NEW_YORK,
    OTTAWA,
    PARIS,
    TRIPOLI,
    TOKYO,
    VANCOUVER,
};
