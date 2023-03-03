using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
	public const int MAX_MEANWHILE_PROFILES = 10;

	// BEGIN SERALIZATION 
	public static MEANWHILE_DEFINITION gCurrentMeanwhileDef;
	public static MEANWHILE_DEFINITION[] gMeanwhileDef = new MEANWHILE_DEFINITION[(int)Meanwhiles.NUM_MEANWHILES];
	public static bool gfMeanwhileTryingToStart = false;
	public static bool gfInMeanwhile = false;
	// END SERIALIZATION
	public static int gsOldSectorX;
	public static MAP_ROW gsOldSectorY;
	public static int gsOldSectorZ;
	public static int gsOldSelectedSectorX;
	public static int gsOldSelectedSectorY;
	public static int gsOldSelectedSectorZ;

	public static ScreenName guiOldScreen;
	public static NPC_SAVE_INFO[] gNPCSaveData = new NPC_SAVE_INFO[MAX_MEANWHILE_PROFILES];
	public static int guiNumNPCSaves = 0;
	public static bool gfReloadingScreenFromMeanwhile = false;
	public static int gsOldCurInterfacePanel = 0;
	public static bool gfWorldWasLoaded = false;
	public static Meanwhiles ubCurrentMeanWhileId = 0;

	public static MEANWHILEFLAGS uiMeanWhileFlags;

	public static string[] gzMeanwhileStr =
{
	"End of player's first battle",
	"Drassen Lib. ",
	"Cambria Lib.",
	"Alma Lib.",
	"Grumm lib.",
	"Chitzena Lib.",
	"NW SAM",
	"NE SAM",
	"Central SAM",
	"Flowers",
	"Lost town",
	"Interrogation",
	"Creatures",
	"Kill Chopper",
	"AWOL Madlab",
	"Outskirts Meduna",
	"Balime Lib.",
};


	// the snap to grid nos for meanwhile scenes
	public static int[] gusMeanWhileGridNo =
	{
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	12248,
	8075,
	12248,
	12248,
	12248,
	12248,
	12248,
};
}
