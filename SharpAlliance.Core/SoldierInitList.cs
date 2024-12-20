﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core;

public class SoldierInitList
{
}

public class SOLDIERINITNODE
{
    public int ubNodeID;
    public int ubSoldierID;
    public BASIC_SOLDIERCREATE_STRUCT? pBasicPlacement;
    public SOLDIERCREATE_STRUCT? pDetailedPlacement;
    public SOLDIERTYPE? pSoldier;
    public SOLDIERINITNODE? prev;
    public SOLDIERINITNODE? next;
}

public partial class Globals
{
    public const int SOLDIER_CREATE_AUTO_TEAM = -1;
    public const int MAX_INDIVIDUALS = 148;
    public const string gzIronManModeWarningText = "You have chosen IRON MAN mode. This setting makes the game considerably more challenging as you will not be able to save your game when in a sector occupied by enemies. This setting will affect the entire course of the game.  Are you sure want to play in IRON MAN mode?\r\n";
    public static SOLDIERINITNODE? gSoldierInitHead;
    public static SOLDIERINITNODE? gSoldierInitTail;
    public static Rectangle giClip;
    public static int giImageWidth;
    internal static int gsMercArriveSectorX;
    internal static MAP_ROW gsMercArriveSectorY;
    internal static bool[] fRefuelingSiteAvailable = new bool[(int)REFUELING_SITE.NUMBER_OF_REFUEL_SITES];
    internal static bool gfPreBattleInterfaceActive;
    internal static SurfaceType guiRENDERBUFFER;

    public static bool gfEstimatePath { get; internal set; }
    public static int gubScreenCount { get; internal set; }
    public static bool fPausedReDrawScreenFlag { get; internal set; }
    public static int gubCambriaMedicalObjects { get; internal set; }
    public static int gubBoxersRests { get; internal set; }
    public static int gubBoxingMatchesWon { get; internal set; }

    internal static void gprintf(int x, int y, string format, params object?[] arguments)
    {
    }

    internal static void mprintf(int sNewX, int sNewY, string format, params object?[] args)
    {

    }

    public static bool saveEnabled = false;
    public static Image<Rgba32> Save(Image<Rgba32> image, string name)
    {
        if (saveEnabled)
        {
            try
            {
                image.SaveAsPng($@"C:\temp\{name}");
            }
            catch { }
        }

        return image;
    }

    internal static string wcscat(string src1, string src2) => src1.TrimEnd('\0') + src2.TrimEnd('\0');

    internal static int wcscmp(string? value1, string value2) => string.Compare(value1, value2, ignoreCase: true);

    internal static string wcscpy(string src)
    {
        return src;
    }

    internal static void wcsncat(string pFinishedString, char v, int iLength)
    {
        throw new NotImplementedException();
    }

    internal static string? wcsstr(char v, string sMercName)
    {
        throw new NotImplementedException();
    }

    internal static bool _KeyDown(Key key)
    {
        throw new NotImplementedException();
    }
}
