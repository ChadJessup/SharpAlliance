using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

public partial class BobbyR
{
    private const int BOBBYR_DEFAULT_MENU_COLOR = 255;
    private static int guiTempCurrentMode;

    private static BobbyRayPurchaseStruct[] BobbyRayPurchases = new BobbyRayPurchaseStruct[MAX_PURCHASE_AMOUNT];

    public static void GameInitBobbyRGuns()
    {
        guiTempCurrentMode = 0;

        Array.Fill(BobbyRayPurchases, new());
    }

}
