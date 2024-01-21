﻿using System;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static bool gfInitAdArea;
    private const int NUM_AIM_HISTORY_PAGES = 5;

    private static void GameInitAIM()
    {
        LaptopInitAim();
    }

    private static void LaptopInitAim()
    {
        gfInitAdArea = true;
    }

    private static bool RenderAIMMembersTopLevel()
    {
        InitCreateDeleteAimPopUpBox(AIM_POPUP.DISPLAY, null, null, 0, 0, 0);

        return true;
    }

    private static void InitCreateDeleteAimPopUpBox(AIM_POPUP ubFlag, string? sString1, string? sString2, int usPosX, int usPosY, int ubData)
    {
    }

    public static void EnterAIM() { }
    public static void ExitAIM() { }
    public static void EnterAimArchives() { }
    public static void ExitAimArchives() { }

}

// Enumerated types used for the Pop Up Box
public enum AIM_POPUP
{
    NOTHING,
    CREATE,
    DISPLAY,
    DELETE,
};
