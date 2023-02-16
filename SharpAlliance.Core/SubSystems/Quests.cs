namespace SharpAlliance.Core.SubSystems;

public class Quests
{
    public const int BOBBYR_SHIPPING_DEST_SECTOR_X = 13;
    public const int BOBBYR_SHIPPING_DEST_SECTOR_Y = 2;
    public const int BOBBYR_SHIPPING_DEST_SECTOR_Z = 0;
    public const int BOBBYR_SHIPPING_DEST_GRIDNO = 10112;
    public const int PABLOS_STOLEN_DEST_GRIDNO = 1;
    public const int LOST_SHIPMENT_GRIDNO = 2;

    // omerta positions
    public const int OMERTA_LEAVE_EQUIP_SECTOR_X = 9;
    public const int OMERTA_LEAVE_EQUIP_SECTOR_Y = 1;
    public const int OMERTA_LEAVE_EQUIP_SECTOR_Z = 0;
    public const int OMERTA_LEAVE_EQUIP_GRIDNO = 4868;

}

public enum MAP_ROW
{
    A = 1,
    B = 2,
    C = 3,
    D = 4,
    E = 5,
    F = 6,
    G = 7,
    H = 8,
    I = 9,
    J = 10,
    K = 11,
    L = 12,
    M = 13,
    N = 14,
    O = 15,
    P = 16,
}

public enum QUEST
{
    DELIVER_LETTER = 0,
    FOOD_ROUTE,
    KILL_TERRORISTS,
    KINGPIN_IDOL,
    KINGPIN_MONEY,
    RUNAWAY_JOEY,
    RESCUE_MARIA,
    CHITZENA_IDOL,
    HELD_IN_ALMA,
    INTERROGATION,
    ARMY_FARM, // 10
    FIND_SCIENTIST,
    DELIVER_VIDEO_CAMERA,
    BLOODCATS,
    FIND_HERMIT,
    CREATURES,
    CHOPPER_PILOT,
    ESCORT_SKYRIDER,
    FREE_DYNAMO,
    ESCORT_TOURISTS,
    FREE_CHILDREN,    // 20
    LEATHER_SHOP_DREAM,
    KILL_DEIDRANNA = 25
}
