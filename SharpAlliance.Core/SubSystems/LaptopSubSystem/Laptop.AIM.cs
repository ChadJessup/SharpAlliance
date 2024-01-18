namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static bool gfInitAdArea;

    private static void GameInitAIM()
    {
        LaptopInitAim();
    }

    private static void LaptopInitAim()
    {
        gfInitAdArea = true;
    }
}
