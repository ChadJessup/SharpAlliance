using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class Globals
{
    public const int SHOW_MIN_FPS = 0;
    public const int SHOW_FULL_FPS = 1;

    public string? gubErrorText;
    public bool gfAniEditMode;
    public bool gfEditMode;
    public bool fFirstTimeInGameScreen;
    public bool fDirtyRectangleMode;
    public string? gDebugStr;
    public string? gSystemDebugStr;

    public bool gfMode;
    public int gsCurrentActionPoints;
    public int gbFPSDisplay;
    public bool gfResetInputCheck;
    public bool gfGlobalError;

    public int guiGameCycleCounter;

    // VIDEO OVERLAYS 
    public int giFPSOverlay;
    public int giCounterPeriodOverlay;
    public bool gfProgramIsRunning { get; set; } // Turn this to FALSE to exit program

    // World Data
    public List<MAP_ELEMENT> gpWorldLevelData { get; set; } = new();

    // World Movement Costs
    public int[,,] gubWorldMovementCosts = new int[World.WORLD_MAX, World.MAXDIR, 2];
    public int[,] gszTerrain = new int[(int)Traversability.NUM_TRAVTERRAIN_TYPES, 15];
}
