using System;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static void GameInitPersonnel()
    {
        // init past characters lists
        int iCounter = 0;
        InitPastCharactersList();

    }

    private static void InitPastCharactersList()
    {
        // inits the past characters list
        Array.Fill(LaptopSaveInfo.ubDeadCharactersList, -1);
        Array.Fill(LaptopSaveInfo.ubLeftCharactersList, -1);
        Array.Fill(LaptopSaveInfo.ubOtherCharactersList, -1);
    }
}
