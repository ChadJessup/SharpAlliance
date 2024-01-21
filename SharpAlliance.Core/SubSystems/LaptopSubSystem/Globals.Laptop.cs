using System.Collections.Generic;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int MAX_BOOKMARKS = 20;
    public const int SPECK_QUOTE__ALREADY_TOLD_PLAYER_THAT_LARRY_RELAPSED = 0x00000001;
    public const int SPECK_QUOTE__SENT_EMAIL_ABOUT_LACK_OF_PAYMENT = 0x00000002;

    public static int gusMercVideoSpeckSpeech;
    public static bool gfMercVideoIsBeingDisplayed;
    public static MERC_VIDEO gubCurrentMercVideoMode;

    // MERC site info
    public static int gubPlayersMercAccountStatus;
    public static int guiPlayersMercAccountNumber;
    public static int gubLastMercIndex;

    public const int LAPTOP_ICONS_X = 33;
    public const int LAPTOP_ICONS_MAIL_Y = 35 - 5;
    public const int LAPTOP_ICONS_WWW_Y = 102 - 10 - 5;
    public const int LAPTOP_ICONS_FINANCIAL_Y = 172 - 10 - 5;
    public const int LAPTOP_ICONS_PERSONNEL_Y = 263 - 20 - 5;
    public const int LAPTOP_ICONS_HISTORY_Y = 310 - 5;
    public const int LAPTOP_ICONS_FILES_Y = 365 - 5 - 5;
    public const int LAPTOP_ICON_TEXT_X = 24;
    public const int LAPTOP_ICON_TEXT_WIDTH = 103 - 24;
    public const int LAPTOP_ICON_TEXT_HEIGHT = 6;
    public const int LAPTOP_ICON_TEXT_MAIL_Y = 82 - 5;
    public const int LAPTOP_ICON_TEXT_WWW_Y = 153 + 4 - 10 - 5;
    public const int LAPTOP_ICON_TEXT_FINANCIAL_Y = 229 - 10 - 5;
    public const int LAPTOP_ICON_TEXT_PERSONNEL_Y = 291 + 5 + 5 - 5;
    public const int LAPTOP_ICON_TEXT_HISTORY_Y = 346 + 10 + 5 - 5;
    public const int LAPTOP_ICON_TEXT_FILES_Y = 412 + 5 + 3 - 5;
    public const FontStyle LAPTOPICONFONT = FontStyle.FONT10ARIAL;
    public const FontStyle BOOK_FONT = FontStyle.FONT10ARIAL;
    public const FontStyle DOWNLOAD_FONT = FontStyle.FONT12ARIAL;
    public const FontStyle ERROR_TITLE_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle ERROR_FONT = FontStyle.FONT12ARIAL;

    public const int HISTORY_ICON_OFFSET_X = 0;
    public const int FILES_ICON_OFFSET_X = 3;
    public const int FINANCIAL_ICON_OFFSET_X = 0;
    public const int LAPTOP_ICON_WIDTH = 80;
    public const int ON_BUTTON = 0;
    public const int GLOW_DELAY = 70;
    public const int WWW_COUNT = 6;
    public const int ICON_INTERVAL = 150;
    public const int BOOK_X = 111;
    public const int BOOK_TOP_Y = 79;
    public const int BOOK_HEIGHT = 12;
    public const int DOWN_HEIGHT = 19;
    public const int BOOK_WIDTH = 100;
    public const int SCROLL_MIN = -100;
    public const int SCROLL_DIFFERENCE = 10;
    public const int LONG_UNIT_TIME = 120;
    public const int UNIT_TIME = 40;
    public const int LOAD_TIME = UNIT_TIME * 30;
    public const int FAST_UNIT_TIME = 3;
    public const int FASTEST_UNIT_TIME = 2;
    public const int ALMOST_FAST_UNIT_TIME = 25;
    public const int ALMOST_FAST_LOAD_TIME = ALMOST_FAST_UNIT_TIME * 30;
    public const int FAST_LOAD_TIME = FAST_UNIT_TIME * 30;
    public const int LONG_LOAD_TIME = LONG_UNIT_TIME * 30;
    public const int FASTEST_LOAD_TIME = FASTEST_UNIT_TIME * 30;
    public const int DOWNLOAD_X = 300;
    public const int DOWNLOAD_Y = 200;
    public const int LAPTOP_WINDOW_X = DOWNLOAD_X + 12;
    public const int LAPTOP_WINDOW_Y = DOWNLOAD_Y + 25;
    public const int LAPTOP_BAR_Y = LAPTOP_WINDOW_Y + 2;
    public const int LAPTOP_BAR_X = LAPTOP_WINDOW_X + 1;
    public const int UNIT_WIDTH = 4;
    public const int LAPTOP_WINDOW_WIDTH = 331 - 181;
    public const int LAPTOP_WINDOW_HEIGHT = 240 - 190;
    public const int DOWN_STRING_X = DOWNLOAD_X + 47;
    public const int DOWN_STRING_Y = DOWNLOAD_Y + 5;
    public const int ERROR_X = 300;
    public const int ERROR_Y = 200;
    public const int ERROR_BTN_X = 43;
    public const int ERROR_BTN_Y = ERROR_Y + 70;
    public const int ERROR_TITLE_X = ERROR_X + 3;
    public const int ERROR_TITLE_Y = ERROR_Y + 3;
    public const int ERROR_BTN_TEXT_X = 20;
    public const int ERROR_BTN_TEXT_Y = 9;
    public const int ERROR_TEXT_X = 0;
    public const int ERROR_TEXT_Y = 15;
    public const int LAPTOP_TITLE_ICONS_X = 113;
    public const int LAPTOP_TITLE_ICONS_Y = 27;
    // HD flicker times
    public const int HD_FLICKER_TIME = 3000;
    public const int FLICKER_TIME = 50;


    public const int NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS = 18;
    public const int LAPTOP_TITLE_BAR_WIDTH = 500;
    public const int LAPTOP_TITLE_BAR_HEIGHT = 24;

    public const int LAPTOP_TITLE_BAR_TOP_LEFT_X = 111;
    public const int LAPTOP_TITLE_BAR_TOP_LEFT_Y = 25;
    public const int LAPTOP_TITLE_BAR_TOP_RIGHT_X = 610;
    public const int LAPTOP_TITLE_BAR_TOP_RIGHT_Y = LAPTOP_TITLE_BAR_TOP_LEFT_Y;
    public const int LAPTOP_TITLE_BAR_ICON_OFFSET_X = 5;
    public const int LAPTOP_TITLE_BAR_ICON_OFFSET_Y = 2;
    public const int LAPTOP_TITLE_BAR_TEXT_OFFSET_X = 29;//18	
    public const int LAPTOP_TITLE_BAR_TEXT_OFFSET_Y = 8;
    public const int LAPTOP_PROGRAM_ICON_X = LAPTOP_TITLE_BAR_TOP_LEFT_X;
    public const int LAPTOP_PROGRAM_ICON_Y = LAPTOP_TITLE_BAR_TOP_LEFT_Y;
    public const int LAPTOP_PROGRAM_ICON_WIDTH = 20;
    public const int LAPTOP_PROGRAM_ICON_HEIGHT = 20;
    public const int DISPLAY_TIME_FOR_WEB_BOOKMARK_NOTIFY = 2000;

    // the wait time for closing of laptop animation/delay
    public const int EXIT_LAPTOP_DELAY_TIME = 100;
    public static SurfaceType guiTitleBarSurface;
    public static bool gfTitleBarSurfaceAlreadyActive = false;
    public const int LAPTOP__NEW_FILE_ICON_X = 83;
    public const int LAPTOP__NEW_FILE_ICON_Y = 412;//(405+19)
    public const int LAPTOP__NEW_EMAIL_ICON_X = 83 - 16;
    public const int LAPTOP__NEW_EMAIL_ICON_Y = LAPTOP__NEW_FILE_ICON_Y;

    // Mode values
    public static LAPTOP_MODE guiCurrentLaptopMode;
    public static LAPTOP_MODE guiPreviousLaptopMode;
    public static LAPTOP_MODE guiCurrentWWWMode = LAPTOP_MODE.NONE;
    public static int giCurrentSubPage;
    public static LAPTOP_CURSOR guiCurrentLapTopCursor;
    public static LAPTOP_CURSOR guiPreviousLapTopCursor;
    public static LaptopPanel guiCurrentSidePanel; // the current navagation panel on the leftside of the laptop screen
    public static LaptopPanel guiPreviousSidePanel;
    public static int guiVSurfaceSize;
    public static int iHighLightBookLine = -1;
    public static bool fFastLoadFlag = false;
    public static bool gfSideBarFlag;
    public static bool gfEnterLapTop = true;
    public static bool gfShowBookmarks = false;

    // in progress of loading a page?
    public static bool fLoadPendingFlag = false;
    public static bool fErrorFlag;

    // mark buttons dirty?
    public static bool fMarkButtonsDirtyFlag = true;

    // redraw afer rendering buttons?
    public static bool fReDrawPostButtonRender = false;

    // intermediate refresh flag
    public static bool fIntermediateReDrawFlag = false;

    // in laptop right now?
    public static bool fCurrentlyInLaptop = false;

    // exit due to a message box pop up?..don't really leave LAPTOP
    public static bool fExitDueToMessageBox = false;

    // have we visited IMP yety?
    public static bool fNotVistedImpYet = true;

    // exit laptop during a load?
    public static bool fExitDuringLoad = false;

    // done loading?
    public static bool fDoneLoadPending = false;

    // re connecting to a web page?
    public static bool fReConnectingFlag = false;

    // going a subpage of a web page?..faster access time
    public static bool fConnectingToSubPage = false;

    // is this our first time in laptop?
    public static bool fFirstTimeInLaptop = true;

    // redraw the book mark info panel .. for blitting on top of animations
    public static bool fReDrawBookMarkInfo = false;

    // show the 2 second info about bookmarks being accessed by clicking on web
    public static bool fShowBookmarkInfo = false;

    // show start button for ATM panel?
    public static bool fShowAtmPanelStartButton;


    //TEMP!	Disables the loadpending delay when switching b/n www pages
    public static bool gfTemporaryDisablingOfLoadPendingFlag = false;

    //GLOBAL FOR WHICH SCREEN TO EXIT TO FOR LAPTOP
    public static ScreenName guiExitScreen = ScreenName.MAP_SCREEN;
    public static MOUSE_REGION gLaptopRegion;
    // Laptop screen graphic handle
    public static HVOBJECT guiLAPTOP;
    public static bool fNewWWWDisplay = true;

    public static bool fNewWWW = true;

    public static BOOKMARK giRainDelayInternetSite = (BOOKMARK)(-1);

    // have we visitied this site already?
    //bool fVisitedBookmarkAlready[20];


    // the laptop icons
    public static HVOBJECT guiFILESICON;
    public static HVOBJECT guiFINANCIALICON;
    public static HVOBJECT guiHISTORYICON;
    public static HVOBJECT guiMAILICON;
    public static HVOBJECT guiPERSICON;
    public static HVOBJECT guiWWWICON;
    public static HVOBJECT guiBOOKTOP;
    public static HVOBJECT guiBOOKHIGH;
    public static HVOBJECT guiBOOKMID;
    public static HVOBJECT guiBOOKBOT;
    public static HVOBJECT guiBOOKMARK;
    public static HVOBJECT guiGRAPHWINDOW;
    public static HVOBJECT guiGRAPHBAR;
    public static HVOBJECT guiLaptopBACKGROUND;
    public static HVOBJECT guiDOWNLOADTOP;
    public static HVOBJECT guiDOWNLOADMID;
    public static HVOBJECT guiDOWNLOADBOT;
    public static HVOBJECT guiTITLEBARLAPTOP;
    public static HVOBJECT guiLIGHTS;
    public static HVOBJECT guiTITLEBARICONS;
    public static SurfaceType guiDESKTOP;

    // email notification
    public static int guiUNREAD;
    public static int guiNEWMAIL;

    //laptop button
    public static int guiLAPTOPBUTTON;
    // the sidepanel handle
    public static int guiLAPTOPSIDEPANEL;

    //bool		gfNewGameLaptop = true;

    // enter new laptop mode due to sliding bars
    public static bool fEnteredNewLapTopDueToHandleSlidingBars = false;

    // laptop pop up messages index value
    public static int iLaptopMessageBox = -1;

    // whether or not we are initing the slide in title bar
    public static bool fInitTitle = true;

    // tab handled
    public static bool fTabHandled = false;

    // are we maxing or mining?
    public static bool fForward = true;

    // BUTTON IMAGES
    public static int[] giLapTopButton = new int[MAX_BUTTON_COUNT];
    public static int[] giLapTopButtonImage = new int[MAX_BUTTON_COUNT];
    public static int[] giErrorButton = new int[1];
    public static int[] giErrorButtonImage = new int[1];
    public static GUI_BUTTON[] gLaptopButtons = new GUI_BUTTON[7];
    public static ButtonPic[] gLaptopButtonImage = new ButtonPic[7];

    // minimize button
    public static int[] gLaptopMinButton = new int[1];
    public static int[] gLaptopMinButtonImage = new int[1];

    public static Dictionary<LAPTOP_PROGRAM, LAPTOP_PROGRAM_STATES> gLaptopProgramStates = new();

    // process of mazimizing
    public static bool fMaximizingProgram = false;

    // program we are maximizing
    public static LAPTOP_PROGRAM bProgramBeingMaximized = (LAPTOP_PROGRAM)(-1);

    // are we minimizing 
    public static bool fMinizingProgram = false;

    // process openned queue
    public static Dictionary<LAPTOP_PROGRAM, int> gLaptopProgramQueueList = new();

    // state of createion of minimize button
    public static bool fCreateMinimizeButton = false;

    public static bool fExitingLaptopFlag = false;

    // HD and power lights on
    public static bool fPowerLightOn = true;
    public static bool fHardDriveLightOn = false;

    // HD flicker
    public static bool fFlickerHD = false;

    // the screens limiting rect
    public static Rectangle LaptopScreenRect = new(LAPTOP_UL_X, LAPTOP_UL_Y - 5, LAPTOP_SCREEN_LR_X + 2, LAPTOP_SCREEN_LR_Y + 5 + 19);


    // the sub pages vistsed or not status within the web browser
    public static bool[] gfWWWaitSubSitesVisitedFlags = new bool[(int)LAPTOP_MODE.SIRTECH - (int)LAPTOP_MODE.WWW];

    //int iBookMarkList[MAX_BOOKMARKS];

    // mouse regions
    public static MOUSE_REGION gEmailRegion = new(nameof(gEmailRegion));
    public static MOUSE_REGION gWWWRegion = new(nameof(gWWWRegion));
    public static MOUSE_REGION gFinancialRegion = new(nameof(gFinancialRegion));
    public static MOUSE_REGION gPersonnelRegion = new(nameof(gPersonnelRegion));
    public static MOUSE_REGION gHistoryRegion = new(nameof(gHistoryRegion));
    public static MOUSE_REGION gFilesRegion = new(nameof(gFilesRegion));
    public static MOUSE_REGION gLapTopScreenRegion = new(nameof(gLapTopScreenRegion));
    public static MOUSE_REGION[] gBookmarkMouseRegions = new MOUSE_REGION[MAX_BOOKMARKS];
    public static MOUSE_REGION pScreenMask = new(nameof(pScreenMask));
    public static MOUSE_REGION gLapTopProgramMinIcon = new(nameof(gLapTopProgramMinIcon));
    public static MOUSE_REGION gNewMailIconRegion = new(nameof(gNewMailIconRegion));
    public static MOUSE_REGION gNewFileIconRegion = new(nameof(gNewFileIconRegion));

    // highlighted mouse region
    public LaptopRegions giHighLightRegion = LaptopRegions.NO_REGION;

    // highlighted regions
    public static LaptopRegions giCurrentRegion = LaptopRegions.NO_REGION;
    public static LaptopRegions giOldRegion = LaptopRegions.NO_REGION;

    //used for global variables that need to be saved
    public static LaptopSaveInfoStruct LaptopSaveInfo = new();


    public static int[,] storeInventory =
    {
    //
    // The first column is for Bobby Rays new inventory,					BOBBY_RAY.NEW,
    // The second column is for Bobby Rays used inventory,				BOBBY_RAY.USED,
    //
    	{0,   0 },		/* nothing */
    
    //---WEAPONS---
    	{10,    1 },		/* Glock 17        */	
    	{3,     1 },		/* Glock 18        */	
    	{10,    1 },		/* Beretta 92F     */	
    	{2,     1 },		/* Beretta 93R     */	
    	{15,    1 },		/* .38 S&W Special */	
    	{8,     1 },		/* .357 Barracuda  */	
    	{6,     1 },		/* .357 DesertEagle*/ 
    	{5,     1 },		/* .45 M1911			 */ 
    	{2,     1 },		/* H&K MP5K      	 */	
    	{1,     1 },		/* .45 MAC-10	     */			// 10
    
    	{1,     1 },		/* Thompson M1A1   */	
    	{1,     1 },		/* Colt Commando   */	
    	{1,     1 },		/* H&K MP53		 		 */	
    	{1,     1 },		/* AKSU-74         */ 
    	{0,     0 },		/* 5.7mm FN P90    */ 
    	{3,     1 },		/* Type-85         */ 
    	{1,     1 },		/* SKS             */ 
    	{1,     1 },		/* Dragunov        */ 
    	{1,     1 },		/* M24             */ 
    	{1,     1 },		/* Steyr AUG       */			//20
    
    	{1,     1 },		/* H&K G41         */ 
    	{2,     1 },		/* Ruger Mini-14   */ 
    	{1,     1 },		/* C-7             */ 
    	{1,     1 },		/* FA-MAS          */ 
    	{1,     1 },		/* AK-74           */ 
    	{1,     1 },		/* AKM             */ 
    	{1,     1 },		/* M-14            */ 
    	{1,     1 },		/* FN-FAL          */	
    	{1,     1 },		/* H&K G3A3        */ 
    	{0,     0 },		/* H&K G11         */			// 30
    
    	{5,     1 },		/* Remington M870  */	
    	{1,     1 },		/* SPAS-15         */ 
    	{0,     0 },		/* CAWS            */
    	{1,     1 },		/* FN Minimi       */
    	{1,     1 },		/* RPK-74          */
    	{1,     1 },		/* H&K 21E         */	
    	{5,     0 },		/* combat knife    */
    	{5,     0 },		/* throwing knife  */
    	{0,     0 },		/* rock            */
    	{1,     0 },		/* grenade launcher*/		//	40
    
    	{1,     0 },		/* mortar */
    	{0,     0 },		/* another rock    */
    	{0,     0 },		/* claws */			
    	{0,     0 },		/* claws */
    	{0,     0 },		/* claws */
    	{0,     0 },		/* claws */
    	{0,     0 },		/* tentacles */
    	{0,     0 },		/* spit  */
    	{1,     0 },		/* brass knuckles */
    	{1,     0 },		/* underslung g.l. */				// 50
    
    	{3,     0 },		/* rocket launcher */
    	{0,     0 },		/* bloodcat claws */
    	{0,     0 },		/* bloodcat bite */
    	{0,     0 },		/* machete */
    	{0,     0 },		/* rocket rifle */
    	{0,     0 },		/* Automag III */
    	{0,     0 },		/* spit */
    	{0,     0 },		/* spit */
    	{0,     0 },		/* spit */
    	{0,     0 },		/* tank cannon */			// 60
    
    	{0,     0 },		/* dart gun */
    	{0,     0 },		/* bloody throwing knife */
    	{0,     0 },		/* flamethrower */
    	{3,     0 },		/* Crowbar       */	
    	{0,     0 },		/* Auto Rocket Rifle */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */			// 70
    
    //---AMMO---
    	{50,    0 },		/* CLIP9_15 */
    	{40,    0 },		/* CLIP9_30 */
    	{8,     0 },		/* CLIP9_15_AP */			
    	{4,     0 },		/* CLIP9_30_AP */	
    	{7,     0 },		/* CLIP9_15_HP */	
    	{4,     0 },		/* CLIP9_30_HP */	
    	{50,    0 },		/* CLIP38_6 */
    	{8,     0 },		/* CLIP38_6_AP */
    	{6,     0 },		/* CLIP38_6_HP */
    	{40,    0 },		/* CLIP45_7 */				// 80
    
    	{25,    0 },		/* CLIP45_30 */
    	{6,     0 },		/* CLIP45_7_AP */		
    	{8,     0 },		/* CLIP45_30_AP */	
    	{6,     0 },		/* CLIP45_7_HP */		
    	{5,     0 },		/* CLIP45_30_HP */	
    	{40,    0 },		/* CLIP357_6 */			
    	{20,    0 },		/* CLIP357_9 */			
    	{6,     0 },		/* CLIP357_6_AP */	
    	{5,     0 },		/* CLIP357_9_AP */	
    	{4,     0 },		/* CLIP357_6_HP */			//90
    
    	{4,     0 },		/* CLIP357_9_HP */	
    	{25,    0 },		/* CLIP545_30_AP */	
    	{5,     0 },		/* CLIP545_30_HP */	
    	{15,    0 },		/* CLIP556_30_AP */	
    	{5,     0 },		/* CLIP556_30_HP */	
    	{15,    0 },		/* CLIP762W_10_AP */
    	{12,    0 },		/* CLIP762W_30_AP */
    	{4,     0 },		/* CLIP762W_10_HP */
    	{5,     0 },		/* CLIP762W_30_HP */
    	{10,    0 },		/* CLIP762N_5_AP */			//100
    
    	{10,    0 },		/* CLIP762N_20_AP */
    	{5,     0 },		/* CLIP762N_5_HP */	
    	{5,     0 },		/* CLIP762N_20_HP */	
    	{0,     0 },		/* CLIP47_50_SAP */		
    	{0,     0 },		/* CLIP57_50_AP */		
    	{0,     0 },		/* CLIP57_50_HP */		
    	{20,    0 },		/* CLIP12G_7 */				
    	{40,    0 },		/* CLIP12G_7_BUCKSHOT */
    	{0,     0 },		/* CLIPCAWS_10_SAP */			
    	{0,     0 },		/* CLIPCAWS_10_FLECH */			//110
    
    	{0,     0 },		/* CLIPROCKET_AP */
    	{0,     0 },		/* CLIPROCKET_HE */	
    	{0,     0 },		/* CLIPROCKET_HEAT  */	
    	{0,     0 },		/* sleep dart */
    	{0,     0 },		/* Clip Flame */
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */						//120
    
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */	
    	{0,     0 },		/* nothing */						//130
    
    //---EXPLOSIVES---
    	{5,     0 },		/* stun grenade				*/
    	{5,     0 },		/* tear gas grenade   */
    	{3,     0 },		/* mustard gas grenade*/
    	{5,     0 },		/* mini hand grenade  */
    	{4,     0 },		/* reg hand grenade   */
    	{0,     0 },		/* RDX                */
    	{0,     0 },		/* TNT (="explosives")*/
    	{0,     0 },		/* HMX (=RDX+TNT)     */
    	{0,     0 },		/* C1  (=RDX+min oil) */
    	{3,     0 },		/* mortar shell       */		//140
    
    	{0,     0 },		/* mine               */
    	{0,     0 },		/* C4  ("plastique")  */
    	{0,     0 },		/* trip flare				  */
    	{0,     0 },		/* trip klaxon        */
    	{0,     0 },		/* shaped charge */
    	{0,     0 },		/* break light */
    	{3,     0 },		/* 40mm HE grenade */
    	{3,     0 },		/* 40mm gas grenade */
    	{3,     0 },		/* 40mm stun grenade */
    	{3,     0 },		/* 40mm smoke grenade */						//150
    
    	{5,     0 },		/* smoke hand grenade */
    	{0,     0 },		/* tank shell */
    	{0,     0 },		/* structure ignite */
    	{0,     0 },		/* creature cocktail */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */					//160
    
    //---ARMOUR---
    	{2,     1 },		/* Flak jacket     */
    	{0,     0 },		/* Flak jacket w X */
    	{0,     0 },		/* Flak jacket w Y */
    	{1,     1 },		/* Kevlar vest   */
    	{0,     0 },		/* Kevlar vest w X */
    	{0,     0 },		/* Kevlar vest w Y */
    	{1,     1 },		/* Spectra vest  */
    	{0,     0 },		/* Spectra vest w X*/
    	{0,     0 },		/* Spectra vest w Y*/
    	{1,     1 },		/* Kevlar leggings */			//170
    
    	{0,     0 },		/* Kevlar legs w X */
    	{0,     0 },		/* Kevlar legs w Y */
    	{1,     1 },		/* Spectra leggings*/
    	{0,     0 },		/* Spectra legs w X*/
    	{0,     0 },		/* Spectra legs w Y*/
    	{3,     1 },		/* Steel helmet    */
    	{1,     1 },		/* Kevlar helmet   */
    	{0,     0 },		/* Kevlar helm w X */
    	{0,     0 },		/* Kevlar helm w Y */
    	{1,     1 },		/* Spectra helmet  */				//180
    
    	{0,     0 },		/* Spectra helm w X*/
    	{0,     0 },		/* Spectra helm w Y*/
    	{0,     0 },		/* Ceramic plates  */ 
    	{0,     0 },		/* hide */
    	{0,     0 },		/* hide */
    	{0,     0 },		/* hide */
    	{0,     0 },		/* hide */
    	{0,     0 },		/* Leather jacket */
    	{0,     0 },		/* Leather jacket w kevlar */
    	{0,     0 },		/* Leather jacket w kevlar 18 */						//190
    
    	{0,     0 },		/* Leather jacket w kevlar Y */
    	{0,     0 },		/* hide */
    	{0,     0 },		/* hide */
    	{0,     0 },		/* T-shirt (Arulco) */
    	{0,     0 },		/* T-shirt (Deidranna) */
    	{1,     1 },		/* Kevlar 2 Vest */
    	{0,     0 },		/* Kevlar 2 Vest w 18 */
    	{0,     0 },		/* Kevlar 2 Vest w Y */
    	{0,     0 },		/* nothing */
    	{0,     0 },		/* nothing */				//200
    
    //---MISC---
    	{8,     0 },		/* First aid kit */
    	{6,     0 },		/* Medical Kit   */
    	{4,     1 },		/* Tool Kit	     */
    	{3,     1 },	  /* Locksmith kit */
    	{2,     0 },	  /* Camouflage kit*/
    	{0,     0 },	  /* nothing */					// Boobytrap kit - unused
    	{2,     1 },		/* Silencer      */
    	{2,     1 },		/* Sniper scope  */
    	{2,     1 },		/* Bipod         */
    	{2,     1 },		/* Extended ear	 */		// 210
    
    	{2,     1 },		/* Night goggles */
    	{5,     0 },		/* Sun goggles	 */
    	{4,     1 },		/* Gas mask   	 */
    	{10,    0 },		/* Canteen       */
    	{2,     0 },		/* Metal detector*/	
    	{0,     0 },		/* Compound 18	 */	
    	{0,     0 },		/* Jar w/Queen Blood */	
    	{0,     0 },		/* Jar w/Elixir */
    	{0,     0 },		/* Money         */
    	{0,     0 },		/* Glass jar		 */		//220
    
    	{0,     0 },		/* Jar w/Creature Blood */
    	{0,     0 },		/* Adrenaline Booster */
    	{0,     0 },		/* Detonator     */	
    	{0,     0 },		/* Rem Detonator */	
    	{0,     0 },		/* VideoTape     */	
    	{0,     0 },		/* Deed					 */	
    	{0,     0 },		/* Letter				 */
    	{0,     0 },		/* Terrorist Info */	
    	{0,     0 },		/* Chalice       */	
    	{0,     0 },		/* Mission4      */		//230
    
    	{0,     0 },		/* Mission5      */
    	{0,     0 },		/* Mission6      */	
    	{0,     0 },		/* Switch        */
    	{0,     0 },		/* Action Item   */	
    	{0,     0 },		/* Syringe2      */	
    	{0,     0 },		/* Syringe3      */
    	{0,     0 },		/* Syringe4      */
    	{0,     0 },		/* Syringe5      */
    	{0,     0 },		/* Jar w/Human blood   */
    	{0,     0 },		/* Ownership item */		//240
    
    	{1,     1 },		/* Laser scope   */
    	{0,     0 },		/* Remote trigger*/
    	{0,     0 },		/* Wirecutters   */			//243
    	{3,     0 },		/* Duckbill      */		
    	{0,     0 },	/* Alcohol  */				
    	{1,     1 },	/* UV Goggles */
    	{0,     0 },	/* Discarded LAW */
    	{0,     0 },	/* head - generic */	
    	{0,     0 },	/* head - Imposter*/	
    	{0,     0 },	/* head - T-Rex */			// 250
    
    	{0,     0 },	/* head - Slay */			
    	{0,     0 },	/* head - Druggist */	
    	{0,     0 },	/* head - Matron */		
    	{0,     0 },	/* head - Tiffany */	
    	{0,     0 },	/* wine     */				
    	{0,     0 },	/* beer    */				
    	{0,     0 },	/* pornos  */				
    	{0,     0 },	/* video camera */				
    	{0,     0 },	/* robot remote control */				
    	{0,     0 },	/* creature claws */		// 260
    
    	{0,     0 },	/* creature flesh */				
    	{0,     0 },	/* creature organ */				
    	{0,     0 },	/* remote trigger */				
    	{0,     0 },	/* gold watch */				
    	{0,     0 },	/* golf clubs */				
    	{0,     0 },	/* walkman */				
    	{0,     0 },	/* portable TV */				
    	{0,     0 },	/* Money for player's account */				
    	{0,     0 },	/* cigars */				
    	{0,     0 },	/* nothing */				// 270
    
    	{0,     0 },	/* key */						// 271
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    
    	{0,     0 },	/* key */						// 281
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    
    	{0,     0 },	/* key */						// 291
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    	{0,     0 },	/* key */				
    
    	{0,     0 },	/* key */						// 301
    	{0,     0 },	/* key */				
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */		
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    
    	{0,     0 },	/* nothing */				// 311
    	{0,     0 },	/* nothing */		
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    
    	{0,     0 },	/* nothing */				// 321
    	{0,     0 },	/* nothing */		
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    
    	{0,     0 },	/* nothing */				// 331
    	{0,     0 },	/* nothing */		
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    
    	{0,     0 },	/* nothing */				// 341
    	{0,     0 },	/* nothing */		
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    	{0,     0 },	/* nothing */
    };



    public static int[] WeaponROF =
    {
        0,		/* Nothing */	
    
    	40,		/* Glock 17        */	
    	1300,		/* Glock 18        */	
    	45,		/* Beretta 92F     */	
    	1100,		/* Beretta 93R     */	
    	25,		/* .38 S&W Special */	
    	23,		/* .357 Barracuda  */	
    	27,		/* .357 DesertEagle*/ 
    	35,		/* .45 M1911			 */ 
    	900,		/* H&K MP5K      	 */	
    	1090,		/* .45 MAC-10	     */			// 10
    
    	700,		/* Thompson M1A1   */	
    	900,		/* Colt Commando   */	
    	700,		/* H&K MP53		 		 */	
    	800,		/* AKSU-74         */ 
    	900,		/* 5.7mm FN P90    */ 
    	780,		/* Type-85         */ 
    	40,		/* SKS             */ 
    	20,		/* Dragunov        */ 
    	20,		/* M24             */ 
    	650,		/* Steyr AUG       */			//20
    
    	850,		/* H&K G41         */ 
    	750,		/* Ruger Mini-14   */ 
    	600,		/* C-7             */ 
    	900,		/* FA-MAS          */ 
    	650,		/* AK-74           */ 
    	600,		/* AKM             */ 
    	750,		/* M-14            */ 
    	650,		/* FN-FAL          */	
    	500,		/* H&K G3A3        */ 
    	600,		/* H&K G11         */			// 30
    
    	21,		/* Remington M870  */	
    	30,		/* SPAS-15         */ 
    	-1,		/* CAWS            */
    	750,		/* FN Minimi       */
    	800,		/* RPK-74          */
    	800,		/* H&K 21E         */	
    	0,			/* combat knife */
    	0,			/* throwing knife */
    	0,			/* rock */
    	1,			/* grenade launcher */		// 40
    
    	1,		/* mortar */
    	0,		/* another rock    */
    	0,		/* claws */			
    	0,		/* claws */
    	0,		/* claws */
    	0,		/* claws */
    	0,		/* tentacles */
    	0,		/* spit  */
    	0,		/* brass knuckles */
    	1,		/* underslung g.l. */				// 50
    
    	1,		/* rocket launcher */
    	0,		/* nothing */
    	0,		/* nothing */
    	0,		/* machete */
    	1,		/* rocket rifle */
    	666,	/* Automag III */
    	0,		/* spit */
    	0,		/* spit */
    	0,		/* spit */
    	1,		/* tank cannon */
    	1,		/* dart gun */
    };
}
