namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static int gubCurrentSortMode;
    private static int gubOldSortMode;
    private static int gubCurrentListMode;

    private static int AIM_ASCEND = 6;
    private static int AIM_DESCEND = 7;

    private static int gubOldListMode;

    private static void GameInitAimSort()
    {
        gubCurrentSortMode = 0;
        gubOldSortMode = 0;
        gubCurrentListMode = AIM_DESCEND;
        gubOldListMode = AIM_DESCEND;
    }
}
