using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

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

    public static bool gfEstimatePath { get; internal set; }
    public static int gubScreenCount { get; internal set; }
    public static bool fPausedReDrawScreenFlag { get; internal set; }

    internal static void gprintf(int x, int y, string format, params object?[] arguments)
    {
        throw new NotImplementedException();
    }

    internal static void mprintf(int sNewX, int sNewY, string format, params object?[] args)
    {
        throw new NotImplementedException();
    }

    internal static string wcscat(string src1, string src2) => src1 + src2;

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
