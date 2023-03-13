namespace SharpAlliance.Core;

public partial class Globals
{
    public static TileDefines[] gOpenDoorList = new TileDefines[]
    {
        TileDefines.FIRSTDOOR1,
        TileDefines.SECONDDOOR1,
        TileDefines.THIRDDOOR1,
        TileDefines.FOURTHDOOR1,
        TileDefines.FIRSTDOOR6,
        TileDefines.SECONDDOOR6,
        TileDefines.THIRDDOOR6,
        TileDefines.FOURTHDOOR6,
        TileDefines.FIRSTDOOR11,
        TileDefines.SECONDDOOR11,
        TileDefines.THIRDDOOR11,
        TileDefines.FOURTHDOOR11,
        TileDefines.FIRSTDOOR16,
        TileDefines.SECONDDOOR16,
        TileDefines.THIRDDOOR16,
        TileDefines.FOURTHDOOR16,
        (TileDefines)(-1),
    };

    public static TileDefines[] gOpenDoorShadowList = new TileDefines[]
    {
        TileDefines.FIRSTDOORSH1,
        TileDefines.SECONDDOORSH1,
        TileDefines.THIRDDOORSH1,
        TileDefines.FOURTHDOORSH1,
        TileDefines.FIRSTDOORSH6,
        TileDefines.SECONDDOORSH6,
        TileDefines.THIRDDOORSH6,
        TileDefines.FOURTHDOORSH6,
        TileDefines.FIRSTDOORSH11,
        TileDefines.SECONDDOORSH11,
        TileDefines.THIRDDOORSH11,
        TileDefines.FOURTHDOORSH11,
        TileDefines.FIRSTDOORSH16,
        TileDefines.SECONDDOORSH16,
        TileDefines.THIRDDOORSH16,
        TileDefines.FOURTHDOORSH16,
        (TileDefines)(-1),
    };

    public static TileDefines[] gClosedDoorList = new TileDefines[]
    {
        TileDefines.FIRSTDOOR5,
        TileDefines.SECONDDOOR5,
        TileDefines.THIRDDOOR5,
        TileDefines.FOURTHDOOR5,
        TileDefines.FIRSTDOOR10,
        TileDefines.SECONDDOOR10,
        TileDefines.THIRDDOOR10,
        TileDefines.FOURTHDOOR10,
        TileDefines.FIRSTDOOR15,
        TileDefines.SECONDDOOR15,
        TileDefines.THIRDDOOR15,
        TileDefines.FOURTHDOOR15,
        TileDefines.FIRSTDOOR20,
        TileDefines.SECONDDOOR20,
        TileDefines.THIRDDOOR20,
        TileDefines.FOURTHDOOR20,
        (TileDefines)(-1),
    };

    public static TileDefines[] gClosedDoorShadowList = new TileDefines[]
    {
        TileDefines.FIRSTDOORSH5,
        TileDefines.SECONDDOORSH5,
        TileDefines.THIRDDOORSH5,
        TileDefines.FOURTHDOORSH5,
        TileDefines.FIRSTDOORSH10,
        TileDefines.SECONDDOORSH10,
        TileDefines.THIRDDOORSH10,
        TileDefines.FOURTHDOORSH10,
        TileDefines.FIRSTDOORSH15,
        TileDefines.SECONDDOORSH15,
        TileDefines.THIRDDOORSH15,
        TileDefines.FOURTHDOORSH15,
        TileDefines.FIRSTDOORSH20,
        TileDefines.SECONDDOORSH20,
        TileDefines.THIRDDOORSH20,
        TileDefines.FOURTHDOORSH20,
        (TileDefines)(-1),
    };

    // REVERSE BUDDIES FROM SHADOW BACK TO STRUCT
    public static (TileTypeDefines, TileDefines, TileDefines)[] gReverseShadowBuddys = new (TileTypeDefines, TileDefines, TileDefines)[]
    {
        (TileTypeDefines.FIRSTCLIFFSHADOW, TileDefines.FIRSTCLIFFSHADOW1, TileDefines.FIRSTCLIFF1),
        (TileTypeDefines.FIRSTSHADOW, TileDefines.FIRSTSHADOW1,   TileDefines.FIRSTOSTRUCT1),
        (TileTypeDefines.SECONDSHADOW,TileDefines.SECONDSHADOW1,   TileDefines.SECONDOSTRUCT1),
        (TileTypeDefines.THIRDSHADOW, TileDefines.THIRDSHADOW1,   TileDefines.THIRDOSTRUCT1),
        (TileTypeDefines.FOURTHSHADOW,TileDefines.FOURTHSHADOW1,   TileDefines.FOURTHOSTRUCT1),
        (TileTypeDefines.FIFTHSHADOW, TileDefines.FIFTHSHADOW1,   TileDefines.FIFTHOSTRUCT1),
        (TileTypeDefines.SIXTHSHADOW, TileDefines.SIXTHSHADOW1,   TileDefines.SIXTHOSTRUCT1),
        (TileTypeDefines.SEVENTHSHADOW, TileDefines.SEVENTHSHADOW1,   TileDefines.SEVENTHOSTRUCT1),
        (TileTypeDefines.EIGHTSHADOW, TileDefines.EIGHTSHADOW1, TileDefines.EIGHTOSTRUCT1),
        (TileTypeDefines.FIRSTFULLSHADOW, TileDefines.FIRSTFULLSHADOW1, TileDefines.    FIRSTFULLSTRUCT1),
        (TileTypeDefines.SECONDFULLSHADOW,TileDefines.SECONDFULLSHADOW1, TileDefines.SECONDFULLSTRUCT1),
        (TileTypeDefines.THIRDFULLSHADOW, TileDefines.THIRDFULLSHADOW1, TileDefines.    THIRDFULLSTRUCT1),
        (TileTypeDefines.FOURTHFULLSHADOW,TileDefines.FOURTHFULLSHADOW1,TileDefines.FOURTHFULLSTRUCT1),
        (TileTypeDefines.FIRSTDOORSHADOW, TileDefines.FIRSTDOORSH1,  TileDefines.FIRSTDOOR1),
        (TileTypeDefines.SECONDDOORSHADOW,TileDefines.SECONDDOORSH1, TileDefines.SECONDDOOR1),
        (TileTypeDefines.THIRDDOORSHADOW, TileDefines.THIRDDOORSH1, TileDefines.THIRDDOOR1),
        (TileTypeDefines.FOURTHDOORSHADOW,TileDefines.FOURTHDOORSH1,TileDefines.FOURTHDOOR1),
        (TileTypeDefines.FENCESHADOW, TileDefines.                   FENCESHADOW1, TileDefines.FENCESTRUCT1),
        (TileTypeDefines.FIRSTVEHICLESHADOW,     TileDefines.FIRSTVEHICLESHADOW1, TileDefines.FIRSTVEHICLE1),
        (TileTypeDefines.SECONDVEHICLESHADOW,    TileDefines.SECONDVEHICLESHADOW1,TileDefines.SECONDVEHICLE1),
        (TileTypeDefines.FIRSTDEBRISSTRUCTSHADOW,        TileDefines.FIRSTDEBRISSTRUCTSHADOW1, TileDefines.FIRSTDEBRISSTRUCT1),
        (TileTypeDefines.SECONDDEBRISSTRUCTSHADOW,       TileDefines.SECONDDEBRISSTRUCTSHADOW1, TileDefines.SECONDDEBRISSTRUCT1),
        (TileTypeDefines.NINTHOSTRUCTSHADOW, TileDefines.NINTHOSTRUCTSHADOW1, TileDefines.NINTHOSTRUCT1),
        (TileTypeDefines.TENTHOSTRUCTSHADOW, TileDefines.TENTHOSTRUCTSHADOW1,  TileDefines.TENTHOSTRUCT1),
        (TileTypeDefines.FIRSTLARGEEXPDEBRISSHADOW,  TileDefines.FIRSTLARGEEXPDEBRISSHADOW1,  TileDefines.FIRSTLARGEEXPDEBRIS1),
        (TileTypeDefines.SECONDLARGEEXPDEBRISSHADOW, TileDefines.SECONDLARGEEXPDEBRISSHADOW1, TileDefines.SECONDLARGEEXPDEBRIS1),
        ((TileTypeDefines)(-1), 0,0),
    };


    // SHADOW BUDDIES FROM STRUCT FORWARD TO SHADOW
    public static (TileTypeDefines, TileDefines, TileDefines)[] gForwardShadowBuddys = new (TileTypeDefines, TileDefines, TileDefines)[]
    {
        (TileTypeDefines.FIRSTCLIFF,     TileDefines.FIRSTCLIFF1, TileDefines.FIRSTCLIFFSHADOW1),
        (TileTypeDefines.FIRSTOSTRUCT,           TileDefines.FIRSTOSTRUCT1,  TileDefines.FIRSTSHADOW1),
        (TileTypeDefines.SECONDOSTRUCT,          TileDefines.SECONDOSTRUCT1,     TileDefines.SECONDSHADOW1),
        (TileTypeDefines.THIRDOSTRUCT,           TileDefines.THIRDOSTRUCT1,  TileDefines.THIRDSHADOW1),
        (TileTypeDefines.FOURTHOSTRUCT,          TileDefines.FOURTHOSTRUCT1,     TileDefines.FOURTHSHADOW1),
        (TileTypeDefines.FIFTHOSTRUCT,           TileDefines.FIFTHOSTRUCT1,  TileDefines.FIFTHSHADOW1),
        (TileTypeDefines.SIXTHOSTRUCT,           TileDefines.SIXTHOSTRUCT1,  TileDefines.SIXTHSHADOW1),
        (TileTypeDefines.SEVENTHOSTRUCT,         TileDefines.SEVENTHOSTRUCT1,    TileDefines.SEVENTHSHADOW1),
        (TileTypeDefines.EIGHTOSTRUCT,           TileDefines.EIGHTOSTRUCT1,  TileDefines.EIGHTSHADOW1),
        (TileTypeDefines.FIRSTFULLSTRUCT,        TileDefines.FIRSTFULLSTRUCT1,   TileDefines.FIRSTFULLSHADOW1),
        (TileTypeDefines.SECONDFULLSTRUCT,       TileDefines.SECONDFULLSTRUCT1,  TileDefines.SECONDFULLSHADOW1),
        (TileTypeDefines.THIRDFULLSTRUCT,        TileDefines.THIRDFULLSTRUCT1,   TileDefines.THIRDFULLSHADOW1),
        (TileTypeDefines.FOURTHFULLSTRUCT,       TileDefines.FOURTHFULLSTRUCT1,  TileDefines.FOURTHFULLSHADOW1),
        (TileTypeDefines.FIRSTDOOR,              TileDefines.FIRSTDOOR1,     TileDefines.FIRSTDOORSH1),
        (TileTypeDefines.SECONDDOOR,             TileDefines.SECONDDOOR1,    TileDefines.SECONDDOORSH1),
        (TileTypeDefines.THIRDDOOR,              TileDefines.THIRDDOOR1,     TileDefines.THIRDDOORSH1),
        (TileTypeDefines.FOURTHDOOR,             TileDefines.FOURTHDOOR1,    TileDefines.FOURTHDOORSH1),
        (TileTypeDefines.FENCESTRUCT,            TileDefines.FENCESTRUCT1,   TileDefines.FENCESHADOW1),
        (TileTypeDefines.FIRSTVEHICLE,           TileDefines.FIRSTVEHICLE1,  TileDefines.FIRSTVEHICLESHADOW1),
        (TileTypeDefines.SECONDVEHICLE,          TileDefines.SECONDVEHICLE1,     TileDefines.SECONDVEHICLESHADOW1),
        (TileTypeDefines.FIRSTDEBRISSTRUCT,      TileDefines.FIRSTDEBRISSTRUCT1,     TileDefines.FIRSTDEBRISSTRUCTSHADOW1),
        (TileTypeDefines.SECONDDEBRISSTRUCT,     TileDefines.SECONDDEBRISSTRUCT1,    TileDefines.SECONDDEBRISSTRUCTSHADOW1),
        (TileTypeDefines.NINTHOSTRUCT,           TileDefines.NINTHOSTRUCT1,  TileDefines.NINTHOSTRUCTSHADOW1),
        (TileTypeDefines.TENTHOSTRUCT,           TileDefines.TENTHOSTRUCT1,  TileDefines.TENTHOSTRUCTSHADOW1),
        (TileTypeDefines.FIRSTLARGEEXPDEBRIS,    TileDefines.FIRSTLARGEEXPDEBRIS1,   TileDefines.FIRSTLARGEEXPDEBRISSHADOW1),
        (TileTypeDefines.SECONDLARGEEXPDEBRIS,   TileDefines.SECONDLARGEEXPDEBRIS1, TileDefines.SECONDLARGEEXPDEBRISSHADOW1),
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
        TileDefines.FIRSTTEXTURE35     - TileDefines.FIRSTTEXTURE1 + 1,
        TileDefines.SECONDTEXTURE35    - TileDefines.SECONDTEXTURE1 + 1,
        TileDefines.THIRDTEXTURE35     - TileDefines.THIRDTEXTURE1 + 1,
        TileDefines.FOURTHTEXTURE35    - TileDefines.FOURTHTEXTURE1 + 1,
        TileDefines.FIFTHTEXTURE35     - TileDefines.FIFTHTEXTURE1 + 1,
        TileDefines.SIXTHTEXTURE37       - TileDefines.SIXTHTEXTURE1 + 1,
        TileDefines.SEVENTHTEXTURE49   - TileDefines.SEVENTHTEXTURE1 + 1,
        TileDefines.REGWATERTEXTURE50  - TileDefines.REGWATERTEXTURE1 + 1,
        TileDefines.DEEPWATERTEXTURE37 - TileDefines.DEEPWATERTEXTURE1 + 1,
        TileDefines.FIRSTCLIFFHANG17     - TileDefines.FIRSTCLIFFHANG1 + 1,
        TileDefines.FIRSTCLIFF17             - TileDefines.FIRSTCLIFF1 + 1,
        TileDefines.FIRSTCLIFFSHADOW17 - TileDefines.FIRSTCLIFFSHADOW1 + 1,	// Med hill
    	TileDefines.FIRSTOSTRUCT12       - TileDefines.FIRSTOSTRUCT1 + 1,
        TileDefines.SECONDOSTRUCT12      - TileDefines.SECONDOSTRUCT1 + 1,
        TileDefines.THIRDOSTRUCT12       - TileDefines.THIRDOSTRUCT1 + 1,
        TileDefines.FOURTHOSTRUCT12    - TileDefines.FOURTHOSTRUCT1 + 1,     // Fourth OSTRUCT
    	TileDefines.FIFTHOSTRUCT12       - TileDefines.FIFTHOSTRUCT1 + 1,      // Fifth OSTRUCT	
    	TileDefines.SIXTHOSTRUCT12       - TileDefines.SIXTHOSTRUCT1 + 1,      // Sixth OSTRUCT
    	TileDefines.SEVENTHOSTRUCT12   - TileDefines.SEVENTHOSTRUCT1 + 1,    // Seventh OSTRUCT
    	TileDefines.EIGHTOSTRUCT12       - TileDefines.EIGHTOSTRUCT1 + 1,     // Eigth OSTRUCT
    	TileDefines.FIRSTFULLSTRUCT12    - TileDefines.FIRSTFULLSTRUCT1 + 1,
        TileDefines.SECONDFULLSTRUCT12 - TileDefines.SECONDFULLSTRUCT1 + 1,
        TileDefines.THIRDFULLSTRUCT2     - TileDefines.THIRDFULLSTRUCT1 + 1,
        TileDefines.FOURTHFULLSTRUCT2 - TileDefines.FOURTHFULLSTRUCT1 + 1,
        TileDefines.FIRSTSHADOW12            - TileDefines.FIRSTSHADOW1 + 1,
        TileDefines.SECONDSHADOW12     - TileDefines.SECONDSHADOW1 + 1,
        TileDefines.THIRDSHADOW12            - TileDefines.THIRDSHADOW1 + 1,
        TileDefines.FOURTHSHADOW12       - TileDefines.FOURTHSHADOW1 + 1,
        TileDefines.FIFTHSHADOW12            - TileDefines.FIFTHSHADOW1 + 1,
        TileDefines.SIXTHSHADOW12            - TileDefines.SIXTHSHADOW1 + 1,
        TileDefines.SEVENTHSHADOW12    - TileDefines.SEVENTHSHADOW1 + 1,
        TileDefines.EIGHTSHADOW12            - TileDefines.EIGHTSHADOW1 + 1,
        TileDefines.FIRSTFULLSHADOW12  - TileDefines.FIRSTFULLSHADOW1 + 1,
        TileDefines.SECONDFULLSHADOW12 - TileDefines.SECONDFULLSHADOW1 + 1,
        TileDefines.THIRDFULLSHADOW2  - TileDefines.THIRDFULLSHADOW1 + 1,
        TileDefines.FOURTHFULLSHADOW2 - TileDefines.FOURTHFULLSHADOW1 + 1,
        TileDefines.FIRSTWALL65              - TileDefines.FIRSTWALL1 + 1,
        TileDefines.SECONDWALL65             - TileDefines.SECONDWALL1 + 1,
        TileDefines.THIRDWALL65              - TileDefines.THIRDWALL1 + 1,
        TileDefines.FOURTHWALL65             - TileDefines.FOURTHWALL1 + 1,
        TileDefines.FIRSTDOOR20              - TileDefines.FIRSTDOOR1 + 1,
        TileDefines.SECONDDOOR20             - TileDefines.SECONDDOOR1 + 1,
        TileDefines.THIRDDOOR20              - TileDefines.THIRDDOOR1 + 1,
        TileDefines.FOURTHDOOR20             - TileDefines.FOURTHDOOR1 + 1,
        TileDefines.FIRSTDOORSH20            - TileDefines.FIRSTDOORSH1 + 1,
        TileDefines.SECONDDOORSH20       - TileDefines.SECONDDOORSH1 + 1,
        TileDefines.THIRDDOORSH20            - TileDefines.THIRDDOORSH1 + 1,
        TileDefines.FOURTHDOORSH20       - TileDefines.FOURTHDOORSH1 + 1,
        TileDefines.SLANTROOFCEILING2    - TileDefines.SLANTROOFCEILING1 + 1,
        TileDefines.ANOTHERDEBRIS10      - TileDefines.ANOTHERDEBRIS1 + 1,
        TileDefines.ROADPIECES400            - TileDefines.ROADPIECES001 + 1,
        TileDefines.FOURTHWINDOW2            - TileDefines.FOURTHWINDOW1 + 1,
        TileDefines.FIRSTDECORATIONS10   - TileDefines.FIRSTDECORATIONS1 + 1,
        TileDefines.SECONDDECORATIONS10 - TileDefines.SECONDDECORATIONS1 + 1,
        TileDefines.THIRDDECORATIONS10   - TileDefines.THIRDDECORATIONS1 + 1,
        TileDefines.FOURTHDECORATIONS10 - TileDefines.FOURTHDECORATIONS1 + 1,
        TileDefines.FIRSTWALLDECAL10     - TileDefines.FIRSTWALLDECAL1 + 1,
        TileDefines.SECONDWALLDECAL10    - TileDefines.SECONDWALLDECAL1 + 1,
        TileDefines.THIRDWALLDECAL10     - TileDefines.THIRDWALLDECAL1 + 1,
        TileDefines.FOURTHWALLDECAL10    - TileDefines.FOURTHWALLDECAL1 + 1,
        TileDefines.FIRSTFLOOR8              - TileDefines.FIRSTFLOOR1 + 1,
        TileDefines.SECONDFLOOR8             - TileDefines.SECONDFLOOR1 + 1,
        TileDefines.THIRDFLOOR8              - TileDefines.THIRDFLOOR1 + 1,
        TileDefines.FOURTHFLOOR8             - TileDefines.FOURTHFLOOR1 + 1,
        TileDefines.FIRSTROOF14              - TileDefines.FIRSTROOF1 + 1,
        TileDefines.SECONDROOF14             - TileDefines.SECONDROOF1 + 1,
        TileDefines.THIRDROOF14              - TileDefines.THIRDROOF1 + 1,
        TileDefines.FOURTHROOF14             - TileDefines.FOURTHROOF1 + 1,
        TileDefines.FIRSTSLANTROOF20     - TileDefines.FIRSTSLANTROOF1 + 1,
        TileDefines.SECONDSLANTROOF20    - TileDefines.SECONDSLANTROOF1 + 1,
        TileDefines.FIRSTONROOF12            - TileDefines.FIRSTONROOF1 + 1,
        TileDefines.SECONDONROOF12       - TileDefines.SECONDONROOF1 + 1,
        1,
        TileDefines.FIRSTISTRUCT24       - TileDefines.FIRSTISTRUCT1 + 1,
        TileDefines.SECONDISTRUCT24      - TileDefines.SECONDISTRUCT1 + 1,
        TileDefines.THIRDISTRUCT24       - TileDefines.THIRDISTRUCT1 + 1,
        TileDefines.FOURTHISTRUCT24      - TileDefines.FOURTHISTRUCT1 + 1,
        TileDefines.FIRSTCISTRUCT24      - TileDefines.FIRSTCISTRUCT1 + 1,
        TileDefines.FIRSTROAD35              - TileDefines.FIRSTROAD1 + 1,
        TileDefines.DEBRISROCKS10            - TileDefines.DEBRISROCKS1 + 1,
        TileDefines.DEBRISWOOD10             - TileDefines.DEBRISWOOD1 + 1,
        TileDefines.DEBRISWEEDS10      - TileDefines.DEBRISWEEDS1 + 1,
        TileDefines.DEBRISGRASS10      - TileDefines.DEBRISGRASS1 + 1,
        TileDefines.DEBRISSAND10       - TileDefines.DEBRISSAND1 + 1,
        TileDefines.DEBRISMISC10       - TileDefines.DEBRISMISC1 + 1,
        TileDefines.ANIOSTRUCT20       - TileDefines.ANIOSTRUCT1 + 1,
        TileDefines.FENCESTRUCT23      - TileDefines.FENCESTRUCT1 + 1,
        TileDefines.FENCESHADOW23      - TileDefines.FENCESHADOW1 + 1,
        TileDefines.FIRSTVEHICLE12                  - TileDefines.FIRSTVEHICLE1 + 1,
        TileDefines.SECONDVEHICLE12                 - TileDefines.SECONDVEHICLE1 + 1,
        TileDefines.FIRSTVEHICLESHADOW12        - TileDefines.FIRSTVEHICLESHADOW1 + 1,
        TileDefines.SECONDVEHICLESHADOW12       - TileDefines.SECONDVEHICLESHADOW1 + 1,
        TileDefines.DEBRIS2MISC10      - TileDefines.DEBRIS2MISC1 + 1,
        TileDefines.FIRSTDEBRISSTRUCT10                     - TileDefines.FIRSTDEBRISSTRUCT1 + 1,
        TileDefines.SECONDDEBRISSTRUCT10                    - TileDefines.SECONDDEBRISSTRUCT1 + 1,
        TileDefines.FIRSTDEBRISSTRUCTSHADOW10           - TileDefines.FIRSTDEBRISSTRUCTSHADOW1 + 1,
        TileDefines.SECONDDEBRISSTRUCTSHADOW10      - TileDefines.SECONDDEBRISSTRUCTSHADOW1 + 1,
        TileDefines.NINTHOSTRUCT12                              - TileDefines.NINTHOSTRUCT1 + 1,
        TileDefines.TENTHOSTRUCT12                              - TileDefines.TENTHOSTRUCT1 + 1,
        TileDefines.NINTHOSTRUCTSHADOW12                    - TileDefines.NINTHOSTRUCTSHADOW1 + 1,
        TileDefines.TENTHOSTRUCTSHADOW12                    - TileDefines.TENTHOSTRUCTSHADOW1 + 1,
        TileDefines.FIRSTEXPLDEBRIS40                           - TileDefines.FIRSTEXPLDEBRIS1 + 1,
        TileDefines.SECONDEXPLDEBRIS40                      - TileDefines.SECONDEXPLDEBRIS1 + 1,
        TileDefines.FIRSTLARGEEXPDEBRIS10                   - TileDefines.FIRSTLARGEEXPDEBRIS1 + 1,
        TileDefines.SECONDLARGEEXPDEBRIS10              - TileDefines.SECONDLARGEEXPDEBRIS1 + 1,
        TileDefines.FIRSTLARGEEXPDEBRISSHADOW10     - TileDefines.FIRSTLARGEEXPDEBRISSHADOW1 + 1,
        TileDefines.SECONDLARGEEXPDEBRISSHADOW10    - TileDefines.SECONDLARGEEXPDEBRISSHADOW1 + 1,
        TileDefines.FIFTHISTRUCT24                              - TileDefines.FIFTHISTRUCT1 + 1,
        TileDefines.SIXTHISTRUCT24                              - TileDefines.SIXTHISTRUCT1 + 1,
        TileDefines.SEVENTHISTRUCT24                            - TileDefines.SEVENTHISTRUCT1 + 1,
        TileDefines.EIGHTISTRUCT24                              - TileDefines.EIGHTISTRUCT1 + 1,
        TileDefines.FIRSTHIGHROOF15                             - TileDefines.FIRSTHIGHROOF1 + 1,
        TileDefines.SECONDHIGHROOF15                            - TileDefines.SECONDHIGHROOF1 + 1,
        TileDefines.FIFTHWALLDECAL10                          - TileDefines.FIFTHWALLDECAL1 + 1,
        TileDefines.SIXTHWALLDECAL10                          - TileDefines.SIXTHWALLDECAL1 + 1,
        TileDefines.SEVENTHWALLDECAL10                      - TileDefines.SEVENTHWALLDECAL1 + 1,
        TileDefines.EIGTHWALLDECAL10                          - TileDefines.EIGTHWALLDECAL1 + 1,
        TileDefines.HUMANBLOOD16                                    - TileDefines.HUMANBLOOD1 + 1,
        TileDefines.CREATUREBLOOD16                             - TileDefines.CREATUREBLOOD1 + 1,
        TileDefines.FIRSTSWITCHES21                             - TileDefines.FIRSTSWITCHES1 + 1,
        // NO SAVED STUFF AFTER HERE!
    	TileDefines.REVEALEDSLANTROOFS8                     - TileDefines.REVEALEDSLANTROOFS1 + 1,
        TileDefines.FIRSTREVEALEDHIGHROOFS11            - TileDefines.FIRSTREVEALEDHIGHROOFS1 + 1,
        TileDefines.SECONDREVEALEDHIGHROOFS11           - TileDefines.SECONDREVEALEDHIGHROOFS1 + 1,
        TileDefines.GUN60                            - TileDefines.GUN1 + 1,
        TileDefines.P1ITEM149                    - TileDefines.P1ITEM1 + 1,
        TileDefines.P2ITEM45                     - TileDefines.P2ITEM1 + 1,
        TileDefines.WINDOWSHATTER20    - TileDefines.WINDOWSHATTER1 + 1,
        TileDefines.P3ITEM16                   - TileDefines.P3ITEM1 + 1,
        TileDefines.BODYEXPLOSION15      - TileDefines.BODYEXPLOSION1 + 1,
        TileDefines.EXITTEXTURE35            - TileDefines.EXITTEXTURE1 + 1,
        TileDefines.FOOTPRINTS80             - TileDefines.FOOTPRINTS1 + 1,
        TileDefines.FIRSTPOINTERS24      - TileDefines.FIRSTPOINTERS1 + 1,
        TileDefines.SECONDPOINTERS8      - TileDefines.SECONDPOINTERS1 + 1,
        TileDefines.THIRDPOINTERS3       - TileDefines.THIRDPOINTERS1 + 1,
        TileDefines.GOODRUN11                    - TileDefines.GOODRUN1 + 1,
        TileDefines.GOODWALK11               - TileDefines.GOODWALK1 + 1,
        TileDefines.GOODSWAT11               - TileDefines.GOODSWAT1 + 1,
        TileDefines.GOODPRONE11              - TileDefines.GOODPRONE1 + 1,
        TileDefines.CONFIRMMOVE11            - TileDefines.CONFIRMMOVE1 + 1,
        TileDefines.VEHICLEMOVE10            - TileDefines.VEHICLEMOVE1 + 1,
        TileDefines.ACTIONTWO11              - TileDefines.ACTIONTWO1 + 1,
        TileDefines.BADMARKER11              - TileDefines.BADMARKER1 + 1,
        TileDefines.GOODRING10               - TileDefines.GOODRING1 + 1,
        TileDefines.ROTATINGKEY8             - TileDefines.ROTATINGKEY1 + 1,
        TileDefines.SELRING10                - TileDefines.SELRING1 + 1,
        TileDefines.SPECIALTILE_COVER_5      - TileDefines.SPECIALTILE_MAPEXIT + 1,
        TileDefines.BULLETTILE2              - TileDefines.BULLETTILE1 + 1,
        TileDefines.FIRSTMISS5               - TileDefines.FIRSTMISS1 + 1,
        TileDefines.SECONDMISS5              - TileDefines.SECONDMISS1 + 1,
        TileDefines.THIRDMISS14              - TileDefines.THIRDMISS1 + 1,
        TileDefines.WIREFRAMES15             - TileDefines.WIREFRAMES1 + 1
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
    public static int[] gTileTypeMovementCost = new int[(int)TerrainTypeDefines.NUM_TERRAIN_TYPES];

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
