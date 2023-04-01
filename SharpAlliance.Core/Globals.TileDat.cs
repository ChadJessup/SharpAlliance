using System.Collections.Generic;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static TileIndexes[] gOpenDoorList = new TileIndexes[]
    {
        TileIndexes.FIRSTDOOR1,
        TileIndexes.SECONDDOOR1,
        TileIndexes.THIRDDOOR1,
        TileIndexes.FOURTHDOOR1,
        TileIndexes.FIRSTDOOR6,
        TileIndexes.SECONDDOOR6,
        TileIndexes.THIRDDOOR6,
        TileIndexes.FOURTHDOOR6,
        TileIndexes.FIRSTDOOR11,
        TileIndexes.SECONDDOOR11,
        TileIndexes.THIRDDOOR11,
        TileIndexes.FOURTHDOOR11,
        TileIndexes.FIRSTDOOR16,
        TileIndexes.SECONDDOOR16,
        TileIndexes.THIRDDOOR16,
        TileIndexes.FOURTHDOOR16,
        (TileIndexes)(-1),
    };

    public static TileIndexes[] gOpenDoorShadowList = new TileIndexes[]
    {
        TileIndexes.FIRSTDOORSH1,
        TileIndexes.SECONDDOORSH1,
        TileIndexes.THIRDDOORSH1,
        TileIndexes.FOURTHDOORSH1,
        TileIndexes.FIRSTDOORSH6,
        TileIndexes.SECONDDOORSH6,
        TileIndexes.THIRDDOORSH6,
        TileIndexes.FOURTHDOORSH6,
        TileIndexes.FIRSTDOORSH11,
        TileIndexes.SECONDDOORSH11,
        TileIndexes.THIRDDOORSH11,
        TileIndexes.FOURTHDOORSH11,
        TileIndexes.FIRSTDOORSH16,
        TileIndexes.SECONDDOORSH16,
        TileIndexes.THIRDDOORSH16,
        TileIndexes.FOURTHDOORSH16,
        (TileIndexes)(-1),
    };

    public static TileIndexes[] gClosedDoorList = new TileIndexes[]
    {
        TileIndexes.FIRSTDOOR5,
        TileIndexes.SECONDDOOR5,
        TileIndexes.THIRDDOOR5,
        TileIndexes.FOURTHDOOR5,
        TileIndexes.FIRSTDOOR10,
        TileIndexes.SECONDDOOR10,
        TileIndexes.THIRDDOOR10,
        TileIndexes.FOURTHDOOR10,
        TileIndexes.FIRSTDOOR15,
        TileIndexes.SECONDDOOR15,
        TileIndexes.THIRDDOOR15,
        TileIndexes.FOURTHDOOR15,
        TileIndexes.FIRSTDOOR20,
        TileIndexes.SECONDDOOR20,
        TileIndexes.THIRDDOOR20,
        TileIndexes.FOURTHDOOR20,
        (TileIndexes)(-1),
    };

    public static TileIndexes[] gClosedDoorShadowList = new TileIndexes[]
    {
        TileIndexes.FIRSTDOORSH5,
        TileIndexes.SECONDDOORSH5,
        TileIndexes.THIRDDOORSH5,
        TileIndexes.FOURTHDOORSH5,
        TileIndexes.FIRSTDOORSH10,
        TileIndexes.SECONDDOORSH10,
        TileIndexes.THIRDDOORSH10,
        TileIndexes.FOURTHDOORSH10,
        TileIndexes.FIRSTDOORSH15,
        TileIndexes.SECONDDOORSH15,
        TileIndexes.THIRDDOORSH15,
        TileIndexes.FOURTHDOORSH15,
        TileIndexes.FIRSTDOORSH20,
        TileIndexes.SECONDDOORSH20,
        TileIndexes.THIRDDOORSH20,
        TileIndexes.FOURTHDOORSH20,
        (TileIndexes)(-1),
    };

    // REVERSE BUDDIES FROM SHADOW BACK TO STRUCT
    public static (TileTypeDefines, TileIndexes, TileIndexes)[] gReverseShadowBuddys = new (TileTypeDefines, TileIndexes, TileIndexes)[]
    {
        (TileTypeDefines.FIRSTCLIFFSHADOW, TileIndexes.FIRSTCLIFFSHADOW1, TileIndexes.FIRSTCLIFF1),
        (TileTypeDefines.FIRSTSHADOW, TileIndexes.FIRSTSHADOW1,   TileIndexes.FIRSTOSTRUCT1),
        (TileTypeDefines.SECONDSHADOW,TileIndexes.SECONDSHADOW1,   TileIndexes.SECONDOSTRUCT1),
        (TileTypeDefines.THIRDSHADOW, TileIndexes.THIRDSHADOW1,   TileIndexes.THIRDOSTRUCT1),
        (TileTypeDefines.FOURTHSHADOW,TileIndexes.FOURTHSHADOW1,   TileIndexes.FOURTHOSTRUCT1),
        (TileTypeDefines.FIFTHSHADOW, TileIndexes.FIFTHSHADOW1,   TileIndexes.FIFTHOSTRUCT1),
        (TileTypeDefines.SIXTHSHADOW, TileIndexes.SIXTHSHADOW1,   TileIndexes.SIXTHOSTRUCT1),
        (TileTypeDefines.SEVENTHSHADOW, TileIndexes.SEVENTHSHADOW1,   TileIndexes.SEVENTHOSTRUCT1),
        (TileTypeDefines.EIGHTSHADOW, TileIndexes.EIGHTSHADOW1, TileIndexes.EIGHTOSTRUCT1),
        (TileTypeDefines.FIRSTFULLSHADOW, TileIndexes.FIRSTFULLSHADOW1, TileIndexes.    FIRSTFULLSTRUCT1),
        (TileTypeDefines.SECONDFULLSHADOW,TileIndexes.SECONDFULLSHADOW1, TileIndexes.SECONDFULLSTRUCT1),
        (TileTypeDefines.THIRDFULLSHADOW, TileIndexes.THIRDFULLSHADOW1, TileIndexes.    THIRDFULLSTRUCT1),
        (TileTypeDefines.FOURTHFULLSHADOW,TileIndexes.FOURTHFULLSHADOW1,TileIndexes.FOURTHFULLSTRUCT1),
        (TileTypeDefines.FIRSTDOORSHADOW, TileIndexes.FIRSTDOORSH1,  TileIndexes.FIRSTDOOR1),
        (TileTypeDefines.SECONDDOORSHADOW,TileIndexes.SECONDDOORSH1, TileIndexes.SECONDDOOR1),
        (TileTypeDefines.THIRDDOORSHADOW, TileIndexes.THIRDDOORSH1, TileIndexes.THIRDDOOR1),
        (TileTypeDefines.FOURTHDOORSHADOW,TileIndexes.FOURTHDOORSH1,TileIndexes.FOURTHDOOR1),
        (TileTypeDefines.FENCESHADOW, TileIndexes.                   FENCESHADOW1, TileIndexes.FENCESTRUCT1),
        (TileTypeDefines.FIRSTVEHICLESHADOW,     TileIndexes.FIRSTVEHICLESHADOW1, TileIndexes.FIRSTVEHICLE1),
        (TileTypeDefines.SECONDVEHICLESHADOW,    TileIndexes.SECONDVEHICLESHADOW1,TileIndexes.SECONDVEHICLE1),
        (TileTypeDefines.FIRSTDEBRISSTRUCTSHADOW,        TileIndexes.FIRSTDEBRISSTRUCTSHADOW1, TileIndexes.FIRSTDEBRISSTRUCT1),
        (TileTypeDefines.SECONDDEBRISSTRUCTSHADOW,       TileIndexes.SECONDDEBRISSTRUCTSHADOW1, TileIndexes.SECONDDEBRISSTRUCT1),
        (TileTypeDefines.NINTHOSTRUCTSHADOW, TileIndexes.NINTHOSTRUCTSHADOW1, TileIndexes.NINTHOSTRUCT1),
        (TileTypeDefines.TENTHOSTRUCTSHADOW, TileIndexes.TENTHOSTRUCTSHADOW1,  TileIndexes.TENTHOSTRUCT1),
        (TileTypeDefines.FIRSTLARGEEXPDEBRISSHADOW,  TileIndexes.FIRSTLARGEEXPDEBRISSHADOW1,  TileIndexes.FIRSTLARGEEXPDEBRIS1),
        (TileTypeDefines.SECONDLARGEEXPDEBRISSHADOW, TileIndexes.SECONDLARGEEXPDEBRISSHADOW1, TileIndexes.SECONDLARGEEXPDEBRIS1),
        ((TileTypeDefines)(-1), 0,0),
    };


    // SHADOW BUDDIES FROM STRUCT FORWARD TO SHADOW
    public static (TileTypeDefines, TileIndexes, TileIndexes)[] gForwardShadowBuddys = new (TileTypeDefines, TileIndexes, TileIndexes)[]
    {
        (TileTypeDefines.FIRSTCLIFF,     TileIndexes.FIRSTCLIFF1, TileIndexes.FIRSTCLIFFSHADOW1),
        (TileTypeDefines.FIRSTOSTRUCT,           TileIndexes.FIRSTOSTRUCT1,  TileIndexes.FIRSTSHADOW1),
        (TileTypeDefines.SECONDOSTRUCT,          TileIndexes.SECONDOSTRUCT1,     TileIndexes.SECONDSHADOW1),
        (TileTypeDefines.THIRDOSTRUCT,           TileIndexes.THIRDOSTRUCT1,  TileIndexes.THIRDSHADOW1),
        (TileTypeDefines.FOURTHOSTRUCT,          TileIndexes.FOURTHOSTRUCT1,     TileIndexes.FOURTHSHADOW1),
        (TileTypeDefines.FIFTHOSTRUCT,           TileIndexes.FIFTHOSTRUCT1,  TileIndexes.FIFTHSHADOW1),
        (TileTypeDefines.SIXTHOSTRUCT,           TileIndexes.SIXTHOSTRUCT1,  TileIndexes.SIXTHSHADOW1),
        (TileTypeDefines.SEVENTHOSTRUCT,         TileIndexes.SEVENTHOSTRUCT1,    TileIndexes.SEVENTHSHADOW1),
        (TileTypeDefines.EIGHTOSTRUCT,           TileIndexes.EIGHTOSTRUCT1,  TileIndexes.EIGHTSHADOW1),
        (TileTypeDefines.FIRSTFULLSTRUCT,        TileIndexes.FIRSTFULLSTRUCT1,   TileIndexes.FIRSTFULLSHADOW1),
        (TileTypeDefines.SECONDFULLSTRUCT,       TileIndexes.SECONDFULLSTRUCT1,  TileIndexes.SECONDFULLSHADOW1),
        (TileTypeDefines.THIRDFULLSTRUCT,        TileIndexes.THIRDFULLSTRUCT1,   TileIndexes.THIRDFULLSHADOW1),
        (TileTypeDefines.FOURTHFULLSTRUCT,       TileIndexes.FOURTHFULLSTRUCT1,  TileIndexes.FOURTHFULLSHADOW1),
        (TileTypeDefines.FIRSTDOOR,              TileIndexes.FIRSTDOOR1,     TileIndexes.FIRSTDOORSH1),
        (TileTypeDefines.SECONDDOOR,             TileIndexes.SECONDDOOR1,    TileIndexes.SECONDDOORSH1),
        (TileTypeDefines.THIRDDOOR,              TileIndexes.THIRDDOOR1,     TileIndexes.THIRDDOORSH1),
        (TileTypeDefines.FOURTHDOOR,             TileIndexes.FOURTHDOOR1,    TileIndexes.FOURTHDOORSH1),
        (TileTypeDefines.FENCESTRUCT,            TileIndexes.FENCESTRUCT1,   TileIndexes.FENCESHADOW1),
        (TileTypeDefines.FIRSTVEHICLE,           TileIndexes.FIRSTVEHICLE1,  TileIndexes.FIRSTVEHICLESHADOW1),
        (TileTypeDefines.SECONDVEHICLE,          TileIndexes.SECONDVEHICLE1,     TileIndexes.SECONDVEHICLESHADOW1),
        (TileTypeDefines.FIRSTDEBRISSTRUCT,      TileIndexes.FIRSTDEBRISSTRUCT1,     TileIndexes.FIRSTDEBRISSTRUCTSHADOW1),
        (TileTypeDefines.SECONDDEBRISSTRUCT,     TileIndexes.SECONDDEBRISSTRUCT1,    TileIndexes.SECONDDEBRISSTRUCTSHADOW1),
        (TileTypeDefines.NINTHOSTRUCT,           TileIndexes.NINTHOSTRUCT1,  TileIndexes.NINTHOSTRUCTSHADOW1),
        (TileTypeDefines.TENTHOSTRUCT,           TileIndexes.TENTHOSTRUCT1,  TileIndexes.TENTHOSTRUCTSHADOW1),
        (TileTypeDefines.FIRSTLARGEEXPDEBRIS,    TileIndexes.FIRSTLARGEEXPDEBRIS1,   TileIndexes.FIRSTLARGEEXPDEBRISSHADOW1),
        (TileTypeDefines.SECONDLARGEEXPDEBRIS,   TileIndexes.SECONDLARGEEXPDEBRIS1, TileIndexes.SECONDLARGEEXPDEBRISSHADOW1),
        ((TileTypeDefines)(-1),0,0),
    };

    // Global variable used to initialize tile database with full tile spec
    public static int[] gFullBaseTileValues = new int[]
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // First Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Second Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Third Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Forth Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Fifth Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,

        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Sixth Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Seventh Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0,

        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // Water1 Texture
    	0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

        1, 1, 1, 1, 1, 1, 1, 1, 1, 1 // Water2 Texture
    };


    public static string[] gTileSurfaceName = new string[]// (int)TileTypeDefines.NUMBEROFTILETYPES]
    {
    "TEXTURE1",
    "TEXTURE2",
    "TEXTURE3",
    "TEXTURE4",
    "TEXTURE5",
    "TEXTURE6",
    "TEXTURE7",
    "WATER1",
    "DEEP WATER",
    "FIRSTCLIFFHANG",
    "FIRSTCLIFF",
    "FIRSTCLIFFSHADOW",
    "OSTRUCT1",
    "OSTRUCT2",
    "OSTRUCT3",
    "OSTRUCT4",
    "OSTRUCT5",
    "OSTRUCT6",
    "OSTRUCT7",
    "OSTRUCT8",
    "OFSTRUCT1",
    "OFSTRUCT2",
    "PLACEHOLDER1",
    "PLACEHOLDER2",

    "SHADOW1",
    "SHADOW2",
    "SHADOW3",
    "SHADOW4",
    "SHADOW5",
    "SHADOW6",
    "SHADOW7",
    "SHADOW8",
    "FSHADOW1",
    "FSHADOW2",
    "PLACEHOLDER3",
    "PLACEHOLDER4",

    "WALL1",
    "WALL2",
    "WALL3",
    "WALL4",
    "DOOR1",
    "DOOR2",
    "DOOR3",
    "DOOR4",
    "DOORSH1",
    "DOORSH2",
    "DOORSH3",
    "DOORSH4",
    "SLANTFLATPEICE",
    "ANOTHERDEBRIS",
    "ROADPIECES",
    "WINDOW4",
    "DECORATIONS1",
    "DECORATIONS2",
    "DECORATIONS3",
    "DECORATIONS4",
    "WALLDECAL1",
    "WALLDECAL2",
    "WALLDECAL3",
    "WALLDECAL4",
    "FLOOR1",
    "FLOOR2",
    "FLOOR3",
    "FLOOR4",
    "ROOF1",
    "ROOF2",
    "ROOF3",
    "ROOF4",
    "SROOF1",
    "SROOF2",
    "ONROOF1",
    "ONROOF2",
    "MOCKF1",

    "ISTRUCT1",
    "ISTRUCT2",
    "ISTRUCT3",
    "ISTRUCT4",

    "FIRSTCISTRCUT",

    "FIRSTROAD",

    "ROCKS",
    "WOOD",
    "WEEDS",
    "GRASS",
    "SAND",
    "MISC",

    "ANIOSTRUCT",
    "FENCESTRUCT",
    "FENCESHADOW",

    "FIRSTVEHICLE",
    "SECONDVEHICLE",
    "FIRSTVEHICLESHADOW",
    "SECONDVEHICLESHADOW",
    "MISC2",
    "FIRSTDEBRISSTRUCT",
    "SECONDDEBRISSTRUCT",
    "FIRSTDEBRISSTRUCTSHADOW",
    "SECONDDEBRISSTRUCTSHADOW",
    "NINTHOSTRUCT",
    "TENTHOSTRUCT",
    "NINTHOSTRUCTSHADOW",
    "TENTHOSTRUCTSHADOW",

    "FIRSTEXPLODEDEBRIS",
    "SECONDEXPLODEDEBRIS",
    "FIRSTLARGEEXPLODEDEBRIS",
    "SECONDLARGEEXPLODEDEBRIS",
    "FIRSTLARGEEXPLODEDEBRISSHADOW",
    "SECONDLARGEEXPLODEDEBRISSHADOW",

    "FIFTHISTRUCT",
    "SIXTHISTRUCT",
    "SEVENTHISTRUCT",
    "EIGHTISTRUCT",

    "FIRSTHIGHROOF",
    "SECONDHIGHROOF",

    "WALLDECAL5",
    "WALLDECAL6",
    "WALLDECAL7",
    "WALLDECAL8",

    "HUMANBLOOD",
    "CREATUREBLOOD",
    "FIRSTSWITCHES",
    
    // ABSOLUTELY NO STUFF PAST HERE!
    // CAN BE SAVED IN THE MAP DIRECTLY!
    "REVEALEDSLANTROOF",
    "1stREVEALEDHIGHROOF",
    "2ndREVEALEDHIGHROOF",

    "GUNS",
    "ITEMS",
    "ITEMS2",

    "GLASSSHATTER",
    "ITEMS3",
    "BODYBLOW",

    "EXITTEXTURE",
    "FOOTPRINTS",
    "POINTERS",
    "POINTERS2",
    "POINTERS3",

    "GOODRUN",
    "GOODWALK",
    "GOODSWAT",
    "GOODSCOOT",
    "CONFIRMMOVE",
    "VEHICLEMOVE",
    "ACTIONTWO",
    "BADMARKER",
    "GRING",
    "ROTATINGKEY",
    "SELRING",
    "SPECIAL",
    "BULLET",
    "MISS1",
    "MISS2",
    "MISS3",
    "WIREFRAME"
    };

    public int[] gNumTilesPerType = new int[(int)TileTypeDefines.NUMBEROFTILETYPES]
    {
        TileIndexes.FIRSTTEXTURE35     - TileIndexes.FIRSTTEXTURE1 + 1,
        TileIndexes.SECONDTEXTURE35    - TileIndexes.SECONDTEXTURE1 + 1,
        TileIndexes.THIRDTEXTURE35     - TileIndexes.THIRDTEXTURE1 + 1,
        TileIndexes.FOURTHTEXTURE35    - TileIndexes.FOURTHTEXTURE1 + 1,
        TileIndexes.FIFTHTEXTURE35     - TileIndexes.FIFTHTEXTURE1 + 1,
        TileIndexes.SIXTHTEXTURE37       - TileIndexes.SIXTHTEXTURE1 + 1,
        TileIndexes.SEVENTHTEXTURE49   - TileIndexes.SEVENTHTEXTURE1 + 1,
        TileIndexes.REGWATERTEXTURE50  - TileIndexes.REGWATERTEXTURE1 + 1,
        TileIndexes.DEEPWATERTEXTURE37 - TileIndexes.DEEPWATERTEXTURE1 + 1,
        TileIndexes.FIRSTCLIFFHANG17     - TileIndexes.FIRSTCLIFFHANG1 + 1,
        TileIndexes.FIRSTCLIFF17             - TileIndexes.FIRSTCLIFF1 + 1,
        TileIndexes.FIRSTCLIFFSHADOW17 - TileIndexes.FIRSTCLIFFSHADOW1 + 1,	// Med hill
    	TileIndexes.FIRSTOSTRUCT12       - TileIndexes.FIRSTOSTRUCT1 + 1,
        TileIndexes.SECONDOSTRUCT12      - TileIndexes.SECONDOSTRUCT1 + 1,
        TileIndexes.THIRDOSTRUCT12       - TileIndexes.THIRDOSTRUCT1 + 1,
        TileIndexes.FOURTHOSTRUCT12    - TileIndexes.FOURTHOSTRUCT1 + 1,     // Fourth OSTRUCT
    	TileIndexes.FIFTHOSTRUCT12       - TileIndexes.FIFTHOSTRUCT1 + 1,      // Fifth OSTRUCT	
    	TileIndexes.SIXTHOSTRUCT12       - TileIndexes.SIXTHOSTRUCT1 + 1,      // Sixth OSTRUCT
    	TileIndexes.SEVENTHOSTRUCT12   - TileIndexes.SEVENTHOSTRUCT1 + 1,    // Seventh OSTRUCT
    	TileIndexes.EIGHTOSTRUCT12       - TileIndexes.EIGHTOSTRUCT1 + 1,     // Eigth OSTRUCT
    	TileIndexes.FIRSTFULLSTRUCT12    - TileIndexes.FIRSTFULLSTRUCT1 + 1,
        TileIndexes.SECONDFULLSTRUCT12 - TileIndexes.SECONDFULLSTRUCT1 + 1,
        TileIndexes.THIRDFULLSTRUCT2     - TileIndexes.THIRDFULLSTRUCT1 + 1,
        TileIndexes.FOURTHFULLSTRUCT2 - TileIndexes.FOURTHFULLSTRUCT1 + 1,
        TileIndexes.FIRSTSHADOW12            - TileIndexes.FIRSTSHADOW1 + 1,
        TileIndexes.SECONDSHADOW12     - TileIndexes.SECONDSHADOW1 + 1,
        TileIndexes.THIRDSHADOW12            - TileIndexes.THIRDSHADOW1 + 1,
        TileIndexes.FOURTHSHADOW12       - TileIndexes.FOURTHSHADOW1 + 1,
        TileIndexes.FIFTHSHADOW12            - TileIndexes.FIFTHSHADOW1 + 1,
        TileIndexes.SIXTHSHADOW12            - TileIndexes.SIXTHSHADOW1 + 1,
        TileIndexes.SEVENTHSHADOW12    - TileIndexes.SEVENTHSHADOW1 + 1,
        TileIndexes.EIGHTSHADOW12            - TileIndexes.EIGHTSHADOW1 + 1,
        TileIndexes.FIRSTFULLSHADOW12  - TileIndexes.FIRSTFULLSHADOW1 + 1,
        TileIndexes.SECONDFULLSHADOW12 - TileIndexes.SECONDFULLSHADOW1 + 1,
        TileIndexes.THIRDFULLSHADOW2  - TileIndexes.THIRDFULLSHADOW1 + 1,
        TileIndexes.FOURTHFULLSHADOW2 - TileIndexes.FOURTHFULLSHADOW1 + 1,
        TileIndexes.FIRSTWALL65              - TileIndexes.FIRSTWALL1 + 1,
        TileIndexes.SECONDWALL65             - TileIndexes.SECONDWALL1 + 1,
        TileIndexes.THIRDWALL65              - TileIndexes.THIRDWALL1 + 1,
        TileIndexes.FOURTHWALL65             - TileIndexes.FOURTHWALL1 + 1,
        TileIndexes.FIRSTDOOR20              - TileIndexes.FIRSTDOOR1 + 1,
        TileIndexes.SECONDDOOR20             - TileIndexes.SECONDDOOR1 + 1,
        TileIndexes.THIRDDOOR20              - TileIndexes.THIRDDOOR1 + 1,
        TileIndexes.FOURTHDOOR20             - TileIndexes.FOURTHDOOR1 + 1,
        TileIndexes.FIRSTDOORSH20            - TileIndexes.FIRSTDOORSH1 + 1,
        TileIndexes.SECONDDOORSH20       - TileIndexes.SECONDDOORSH1 + 1,
        TileIndexes.THIRDDOORSH20            - TileIndexes.THIRDDOORSH1 + 1,
        TileIndexes.FOURTHDOORSH20       - TileIndexes.FOURTHDOORSH1 + 1,
        TileIndexes.SLANTROOFCEILING2    - TileIndexes.SLANTROOFCEILING1 + 1,
        TileIndexes.ANOTHERDEBRIS10      - TileIndexes.ANOTHERDEBRIS1 + 1,
        TileIndexes.ROADPIECES400            - TileIndexes.ROADPIECES001 + 1,
        TileIndexes.FOURTHWINDOW2            - TileIndexes.FOURTHWINDOW1 + 1,
        TileIndexes.FIRSTDECORATIONS10   - TileIndexes.FIRSTDECORATIONS1 + 1,
        TileIndexes.SECONDDECORATIONS10 - TileIndexes.SECONDDECORATIONS1 + 1,
        TileIndexes.THIRDDECORATIONS10   - TileIndexes.THIRDDECORATIONS1 + 1,
        TileIndexes.FOURTHDECORATIONS10 - TileIndexes.FOURTHDECORATIONS1 + 1,
        TileIndexes.FIRSTWALLDECAL10     - TileIndexes.FIRSTWALLDECAL1 + 1,
        TileIndexes.SECONDWALLDECAL10    - TileIndexes.SECONDWALLDECAL1 + 1,
        TileIndexes.THIRDWALLDECAL10     - TileIndexes.THIRDWALLDECAL1 + 1,
        TileIndexes.FOURTHWALLDECAL10    - TileIndexes.FOURTHWALLDECAL1 + 1,
        TileIndexes.FIRSTFLOOR8              - TileIndexes.FIRSTFLOOR1 + 1,
        TileIndexes.SECONDFLOOR8             - TileIndexes.SECONDFLOOR1 + 1,
        TileIndexes.THIRDFLOOR8              - TileIndexes.THIRDFLOOR1 + 1,
        TileIndexes.FOURTHFLOOR8             - TileIndexes.FOURTHFLOOR1 + 1,
        TileIndexes.FIRSTROOF14              - TileIndexes.FIRSTROOF1 + 1,
        TileIndexes.SECONDROOF14             - TileIndexes.SECONDROOF1 + 1,
        TileIndexes.THIRDROOF14              - TileIndexes.THIRDROOF1 + 1,
        TileIndexes.FOURTHROOF14             - TileIndexes.FOURTHROOF1 + 1,
        TileIndexes.FIRSTSLANTROOF20     - TileIndexes.FIRSTSLANTROOF1 + 1,
        TileIndexes.SECONDSLANTROOF20    - TileIndexes.SECONDSLANTROOF1 + 1,
        TileIndexes.FIRSTONROOF12            - TileIndexes.FIRSTONROOF1 + 1,
        TileIndexes.SECONDONROOF12       - TileIndexes.SECONDONROOF1 + 1,
        1,
        TileIndexes.FIRSTISTRUCT24       - TileIndexes.FIRSTISTRUCT1 + 1,
        TileIndexes.SECONDISTRUCT24      - TileIndexes.SECONDISTRUCT1 + 1,
        TileIndexes.THIRDISTRUCT24       - TileIndexes.THIRDISTRUCT1 + 1,
        TileIndexes.FOURTHISTRUCT24      - TileIndexes.FOURTHISTRUCT1 + 1,
        TileIndexes.FIRSTCISTRUCT24      - TileIndexes.FIRSTCISTRUCT1 + 1,
        TileIndexes.FIRSTROAD35              - TileIndexes.FIRSTROAD1 + 1,
        TileIndexes.DEBRISROCKS10            - TileIndexes.DEBRISROCKS1 + 1,
        TileIndexes.DEBRISWOOD10             - TileIndexes.DEBRISWOOD1 + 1,
        TileIndexes.DEBRISWEEDS10      - TileIndexes.DEBRISWEEDS1 + 1,
        TileIndexes.DEBRISGRASS10      - TileIndexes.DEBRISGRASS1 + 1,
        TileIndexes.DEBRISSAND10       - TileIndexes.DEBRISSAND1 + 1,
        TileIndexes.DEBRISMISC10       - TileIndexes.DEBRISMISC1 + 1,
        TileIndexes.ANIOSTRUCT20       - TileIndexes.ANIOSTRUCT1 + 1,
        TileIndexes.FENCESTRUCT23      - TileIndexes.FENCESTRUCT1 + 1,
        TileIndexes.FENCESHADOW23      - TileIndexes.FENCESHADOW1 + 1,
        TileIndexes.FIRSTVEHICLE12                  - TileIndexes.FIRSTVEHICLE1 + 1,
        TileIndexes.SECONDVEHICLE12                 - TileIndexes.SECONDVEHICLE1 + 1,
        TileIndexes.FIRSTVEHICLESHADOW12        - TileIndexes.FIRSTVEHICLESHADOW1 + 1,
        TileIndexes.SECONDVEHICLESHADOW12       - TileIndexes.SECONDVEHICLESHADOW1 + 1,
        TileIndexes.DEBRIS2MISC10      - TileIndexes.DEBRIS2MISC1 + 1,
        TileIndexes.FIRSTDEBRISSTRUCT10                     - TileIndexes.FIRSTDEBRISSTRUCT1 + 1,
        TileIndexes.SECONDDEBRISSTRUCT10                    - TileIndexes.SECONDDEBRISSTRUCT1 + 1,
        TileIndexes.FIRSTDEBRISSTRUCTSHADOW10           - TileIndexes.FIRSTDEBRISSTRUCTSHADOW1 + 1,
        TileIndexes.SECONDDEBRISSTRUCTSHADOW10      - TileIndexes.SECONDDEBRISSTRUCTSHADOW1 + 1,
        TileIndexes.NINTHOSTRUCT12                              - TileIndexes.NINTHOSTRUCT1 + 1,
        TileIndexes.TENTHOSTRUCT12                              - TileIndexes.TENTHOSTRUCT1 + 1,
        TileIndexes.NINTHOSTRUCTSHADOW12                    - TileIndexes.NINTHOSTRUCTSHADOW1 + 1,
        TileIndexes.TENTHOSTRUCTSHADOW12                    - TileIndexes.TENTHOSTRUCTSHADOW1 + 1,
        TileIndexes.FIRSTEXPLDEBRIS40                           - TileIndexes.FIRSTEXPLDEBRIS1 + 1,
        TileIndexes.SECONDEXPLDEBRIS40                      - TileIndexes.SECONDEXPLDEBRIS1 + 1,
        TileIndexes.FIRSTLARGEEXPDEBRIS10                   - TileIndexes.FIRSTLARGEEXPDEBRIS1 + 1,
        TileIndexes.SECONDLARGEEXPDEBRIS10              - TileIndexes.SECONDLARGEEXPDEBRIS1 + 1,
        TileIndexes.FIRSTLARGEEXPDEBRISSHADOW10     - TileIndexes.FIRSTLARGEEXPDEBRISSHADOW1 + 1,
        TileIndexes.SECONDLARGEEXPDEBRISSHADOW10    - TileIndexes.SECONDLARGEEXPDEBRISSHADOW1 + 1,
        TileIndexes.FIFTHISTRUCT24                              - TileIndexes.FIFTHISTRUCT1 + 1,
        TileIndexes.SIXTHISTRUCT24                              - TileIndexes.SIXTHISTRUCT1 + 1,
        TileIndexes.SEVENTHISTRUCT24                            - TileIndexes.SEVENTHISTRUCT1 + 1,
        TileIndexes.EIGHTISTRUCT24                              - TileIndexes.EIGHTISTRUCT1 + 1,
        TileIndexes.FIRSTHIGHROOF15                             - TileIndexes.FIRSTHIGHROOF1 + 1,
        TileIndexes.SECONDHIGHROOF15                            - TileIndexes.SECONDHIGHROOF1 + 1,
        TileIndexes.FIFTHWALLDECAL10                          - TileIndexes.FIFTHWALLDECAL1 + 1,
        TileIndexes.SIXTHWALLDECAL10                          - TileIndexes.SIXTHWALLDECAL1 + 1,
        TileIndexes.SEVENTHWALLDECAL10                      - TileIndexes.SEVENTHWALLDECAL1 + 1,
        TileIndexes.EIGTHWALLDECAL10                          - TileIndexes.EIGTHWALLDECAL1 + 1,
        TileIndexes.HUMANBLOOD16                                    - TileIndexes.HUMANBLOOD1 + 1,
        TileIndexes.CREATUREBLOOD16                             - TileIndexes.CREATUREBLOOD1 + 1,
        TileIndexes.FIRSTSWITCHES21                             - TileIndexes.FIRSTSWITCHES1 + 1,
        // NO SAVED STUFF AFTER HERE!
    	TileIndexes.REVEALEDSLANTROOFS8                     - TileIndexes.REVEALEDSLANTROOFS1 + 1,
        TileIndexes.FIRSTREVEALEDHIGHROOFS11            - TileIndexes.FIRSTREVEALEDHIGHROOFS1 + 1,
        TileIndexes.SECONDREVEALEDHIGHROOFS11           - TileIndexes.SECONDREVEALEDHIGHROOFS1 + 1,
        TileIndexes.GUN60                            - TileIndexes.GUN1 + 1,
        TileIndexes.P1ITEM149                    - TileIndexes.P1ITEM1 + 1,
        TileIndexes.P2ITEM45                     - TileIndexes.P2ITEM1 + 1,
        TileIndexes.WINDOWSHATTER20    - TileIndexes.WINDOWSHATTER1 + 1,
        TileIndexes.P3ITEM16                   - TileIndexes.P3ITEM1 + 1,
        TileIndexes.BODYEXPLOSION15      - TileIndexes.BODYEXPLOSION1 + 1,
        TileIndexes.EXITTEXTURE35            - TileIndexes.EXITTEXTURE1 + 1,
        TileIndexes.FOOTPRINTS80             - TileIndexes.FOOTPRINTS1 + 1,
        TileIndexes.FIRSTPOINTERS24      - TileIndexes.FIRSTPOINTERS1 + 1,
        TileIndexes.SECONDPOINTERS8      - TileIndexes.SECONDPOINTERS1 + 1,
        TileIndexes.THIRDPOINTERS3       - TileIndexes.THIRDPOINTERS1 + 1,
        TileIndexes.GOODRUN11                    - TileIndexes.GOODRUN1 + 1,
        TileIndexes.GOODWALK11               - TileIndexes.GOODWALK1 + 1,
        TileIndexes.GOODSWAT11               - TileIndexes.GOODSWAT1 + 1,
        TileIndexes.GOODPRONE11              - TileIndexes.GOODPRONE1 + 1,
        TileIndexes.CONFIRMMOVE11            - TileIndexes.CONFIRMMOVE1 + 1,
        TileIndexes.VEHICLEMOVE10            - TileIndexes.VEHICLEMOVE1 + 1,
        TileIndexes.ACTIONTWO11              - TileIndexes.ACTIONTWO1 + 1,
        TileIndexes.BADMARKER11              - TileIndexes.BADMARKER1 + 1,
        TileIndexes.GOODRING10               - TileIndexes.GOODRING1 + 1,
        TileIndexes.ROTATINGKEY8             - TileIndexes.ROTATINGKEY1 + 1,
        TileIndexes.SELRING10                - TileIndexes.SELRING1 + 1,
        TileIndexes.SPECIALTILE_COVER_5      - TileIndexes.SPECIALTILE_MAPEXIT + 1,
        TileIndexes.BULLETTILE2              - TileIndexes.BULLETTILE1 + 1,
        TileIndexes.FIRSTMISS5               - TileIndexes.FIRSTMISS1 + 1,
        TileIndexes.SECONDMISS5              - TileIndexes.SECONDMISS1 + 1,
        TileIndexes.THIRDMISS14              - TileIndexes.THIRDMISS1 + 1,
        TileIndexes.WIREFRAMES15             - TileIndexes.WIREFRAMES1 + 1
    };

    public static int[] gTileTypeLogicalHeight = new int[]// (int)TileTypeDefines.NUMBEROFTILETYPES]
    {
        2,						// First texture
    	2,						// Second texture
    	2,						// Third texture
    	2,						// Forth texture
    	2,						// Fifth texture
    	2,						// Sixth texture
    	2,						// Seventh texture
    	10,						// First water
    	10							// Second water
    };

    public static int gTileDatabaseSize;
    public static int[] gTileTypeStartIndex = new int[(int)TileTypeDefines.NUMBEROFTILETYPES];
    public static int gusNumAnimatedTiles;
    public static int[] gusAnimatedTiles = new int[TileDefine.MAX_ANIMATED_TILES];
    public static Dictionary<TerrainTypeDefines, int> gTileTypeMovementCost = new();

    //==========================================================================
    // Quick defines for finding last type entry in tile types
    public const TileTypeDefines LASTTEXTURE =      (TileTypeDefines.DEEPWATERTEXTURE - 1);
    public const TileTypeDefines LASTBANKS =        (TileTypeDefines.FIRSTCLIFFSHADOW - 1);
    public const TileTypeDefines LASTCLIFFHANG =    (TileTypeDefines.FIRSTCLIFF - 1);
    public const TileTypeDefines LASTCLIFFSHADOW =  (TileTypeDefines.FIRSTOSTRUCT - 1);
    public const TileTypeDefines LASTOSTRUCT =      (TileTypeDefines.THIRDFULLSTRUCT - 1);
    public const TileTypeDefines LASTSHADOW =       (TileTypeDefines.FIRSTWALL - 1);
    public const TileTypeDefines LASTWALL =         (TileTypeDefines.FIRSTDOOR - 1);
    public const TileTypeDefines LASTDOOR =         (TileTypeDefines.FIRSTDOORSHADOW - 1);
    public const TileTypeDefines LASTDOORSHADOW =   (TileTypeDefines.SLANTROOFCEILING - 1);
    public const TileTypeDefines LASTDECORATIONS =  (TileTypeDefines.FIRSTWALLDECAL - 1);
    public const TileTypeDefines LASTWALLDECAL =    (TileTypeDefines.FIRSTFLOOR - 1);
    public const TileTypeDefines LASTFLOOR =        (TileTypeDefines.FIRSTROOF - 1);
    public const TileTypeDefines LASTROOF =         (TileTypeDefines.FIRSTSLANTROOF - 1);
    public const TileTypeDefines LASTSLANTROOF =    (TileTypeDefines.FIRSTONROOF - 1);
    public const TileTypeDefines LASTMOCKFLOOR =    (TileTypeDefines.FIRSTISTRUCT - 1);
    public const TileTypeDefines LASTISTRUCT =      (TileTypeDefines.FIRSTROAD - 1);
    public const TileTypeDefines LASTROAD =         (TileTypeDefines.DEBRISROCKS - 1);
    public const TileTypeDefines LASTDEBRIS =       (TileTypeDefines.ANIOSTRUCT - 1);
    public const TileTypeDefines LASTITEM =         (TileTypeDefines.WINDOWSHATTER - 1);
    // public const TileTypeDefines LASTDEBRIS =    (TileTypeDefines.FOOTPRINTS - 1);
    public const TileTypeDefines LASTFOOTPRINTS =   (TileTypeDefines.FIRSTPOINTERS - 1);
    public const TileTypeDefines LASTPOINTERS =     (TileTypeDefines.SELRING - 1);
    public const TileTypeDefines LASTUIELEM =       (TileTypeDefines.WIREFRAMES - 1);		// Change this entry if adding new types to the end
    public const TileTypeDefines LASTTIELSETELEM =  (TileTypeDefines.GUNS - 1);
}
