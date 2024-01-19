namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static int gubCurrentSortMode;
    private static int gubOldSortMode;
    private static AIM gubCurrentListMode;

    private static AIM gubOldListMode;

    private static void GameInitAimSort()
    {
        gubCurrentSortMode = 0;
        gubOldSortMode = 0;
        gubCurrentListMode = AIM.DESCEND;
        gubOldListMode = AIM.DESCEND;
    }
}

public enum AIM
{
    ASCEND = 6,
    DESCEND = 7,
}
