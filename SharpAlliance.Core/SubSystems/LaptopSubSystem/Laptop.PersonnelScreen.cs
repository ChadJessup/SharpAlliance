using System;
using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of personnel.c: the personnel program (current team, departed mercs, stats, employment record, inventory and ATM).
public partial class Laptop
{
    private const int NUM_BACKGROUND_REPS = 40;
    private const int BACKGROUND_HEIGHT = 10;
    private const int BACKGROUND_WIDTH = 125;
    private const int IMAGE_BOX_X = 395;
    private const int IMAGE_BOX_Y = LAPTOP_SCREEN_UL_Y+24;
    private const int IMAGE_BOX_WIDTH = 112;
    private const int IMAGE_BOX_WITH_NO_BORDERS = 106;
    private const int IMAGE_BOX_COUNT = 4;
    private const int IMAGE_NAME_WIDTH = 106;
    private const int IMAGE_FULL_NAME_OFFSET_Y = 111;
    private const int TEXT_BOX_WIDTH = 160;
    private const int TEXT_DELTA_OFFSET = 9;
    private const int TEXT_BOX_Y = LAPTOP_SCREEN_UL_Y+188;
    private const int PAGE_BOX_X = LAPTOP_SCREEN_UL_X+250 - 10;
    private const int PAGE_BOX_Y = LAPTOP_SCREEN_UL_Y+3;
    private const int PAGE_BOX_WIDTH = 58;
    private const int PAGE_BOX_HEIGHT = 24;
    private const int MAX_SLOTS = 4;
    private const int PERS_CURR_TEAM_X = LAPTOP_SCREEN_UL_X + 39 - 15;
    private const int PERS_CURR_TEAM_Y = LAPTOP_SCREEN_UL_Y + 218;
    private const int PERS_DEPART_TEAM_Y = LAPTOP_SCREEN_UL_Y + 247;
    private const int BUTTON_Y = LAPTOP_SCREEN_UL_Y+34;
    private const int LEFT_BUTTON_X = LAPTOP_SCREEN_UL_X+3 - 10;
    private const int RIGHT_BUTTON_X = LAPTOP_SCREEN_UL_X+476 - 10;
    private const int PERS_COUNT = 15;
    private const int DELAY_PER_MODE_CHANGE_IN_ATM = 2000;
    internal const int CONTRACT_EXTEND_1_DAY = 0;
    internal const int CONTRACT_EXTEND_1_WEEK = 1;
    internal const int CONTRACT_EXTEND_2_WEEK = 2;
    private const int MAX_STATS = 20;
    private const FontStyle PERS_FONT = FontStyle.FONT10ARIAL;
    private const FontStyle PERS_HEADER_FONT = FontStyle.FONT14ARIAL;
    private const FontStyle CHAR_NAME_FONT = FontStyle.FONT12ARIAL;
    private const int CHAR_NAME_Y = 177;
    private const int CHAR_LOC_Y = 189;
    private const FontColor PERS_TEXT_FONT_COLOR = FontColor.FONT_WHITE; // 146
    private const FontColor PERS_TEXT_FONT_ALTERNATE_COLOR = FontColor.FONT_YELLOW;
    private const FontColor PERS_FONT_COLOR = FontColor.FONT_WHITE;
    private const int PAGE_X = PAGE_BOX_X+2 - 10;
    private const int PAGE_Y = PAGE_BOX_Y+2;
    private const int NEXT_MERC_FACE_X = LAPTOP_SCREEN_UL_X + 448;
    private const int MERC_FACE_SCROLL_Y = LAPTOP_SCREEN_UL_Y + 150;
    private const int PREV_MERC_FACE_X = LAPTOP_SCREEN_UL_X + 285;
    private const int DEPARTED_X = LAPTOP_SCREEN_UL_X + 29 - 10;
    private const int DEPARTED_Y = LAPTOP_SCREEN_UL_Y + 207;
    private const int PERSONNEL_PORTRAIT_NUMBER = 20;
    private const int PERSONNEL_PORTRAIT_NUMBER_WIDTH = 5;
    private const int SMALL_PORTRAIT_WIDTH = 46;
    private const int SMALL_PORTRAIT_HEIGHT = 42;
    private const int SMALL_PORT_WIDTH = 52;
    private const int SMALL_PORT_HEIGHT = 45;
    private const int SMALL_PORTRAIT_WIDTH_NO_BORDERS = 48;
    private const int SMALL_PORTRAIT_START_X = 141 - 10;
    private const int SMALL_PORTRAIT_START_Y = 53;
    private const int PERS_CURR_TEAM_COST_X = LAPTOP_SCREEN_UL_X + 150 - 10;
    private const int PERS_CURR_TEAM_COST_Y = LAPTOP_SCREEN_UL_Y + 218;
    private const int PERS_CURR_TEAM_HIGHEST_Y = PERS_CURR_TEAM_COST_Y + 15;
    private const int PERS_CURR_TEAM_LOWEST_Y = PERS_CURR_TEAM_HIGHEST_Y + 15;
    private const int PERS_CURR_TEAM_WIDTH = 286 - 160;
    private const int PERS_DEPART_TEAM_WIDTH = PERS_CURR_TEAM_WIDTH - 20;
    private const int PERS_STAT_AVG_X = LAPTOP_SCREEN_UL_X + 157 - 10;
    private const int PERS_STAT_AVG_Y = LAPTOP_SCREEN_UL_Y + 274;
    private const int PERS_STAT_AVG_WIDTH = 202 - 159;
    private const int PERS_STAT_LOWEST_X = LAPTOP_SCREEN_UL_X + 72 - 10;
    private const int PERS_STAT_LOWEST_WIDTH = 155 - 75;
    private const int PERS_STAT_HIGHEST_X = LAPTOP_SCREEN_UL_X + 205 - 10;
    private const int PERS_STAT_LIST_X = LAPTOP_SCREEN_UL_X + 33 - 10;
    private const int PERS_TOGGLE_CUR_DEPART_WIDTH = 106 - 35;
    private const int PERS_TOGGLE_CUR_DEPART_HEIGHT = 236 - 212;
    private const int PERS_TOGGLE_CUR_DEPART_X = LAPTOP_SCREEN_UL_X + 35 - 10;
    private const int PERS_TOGGLE_CUR_Y = LAPTOP_SCREEN_UL_Y + 208;
    private const int PERS_TOGGLE_DEPART_Y = LAPTOP_SCREEN_UL_Y + 238;
    private const int PERS_DEPARTED_UP_X = LAPTOP_SCREEN_UL_X + 265 - 10;
    private const int PERS_DEPARTED_UP_Y = LAPTOP_SCREEN_UL_Y + 210;
    private const int PERS_DEPARTED_DOWN_Y = LAPTOP_SCREEN_UL_Y + 237;
    private const int PERS_TITLE_X = 140;
    private const int PERS_TITLE_Y = 33;
    private const int ATM_UL_X = LAPTOP_SCREEN_UL_X + 397;
    private const int ATM_UL_Y = LAPTOP_SCREEN_UL_Y + 27;
    private const FontStyle ATM_FONT = PERS_FONT;
    private const int ATM_BUTTONS_START_Y = 110;
    private const int ATM_BUTTONS_START_X = 510;
    private const int ATM_BUTTON_WIDTH = 15;
    private const int ATM_BUTTON_HEIGHT = 15;
    private const int ATM_DISPLAY_X = 509;
    private const int ATM_DISPLAY_Y = 58;
    private const int ATM_DISPLAY_HEIGHT = 10;
    private const int ATM_DISPLAY_WIDTH = 81;
    private const int NUMBER_OF_INVENTORY_PERSONNEL = 8;
    private const int Y_SIZE_OF_PERSONNEL_SCROLL_REGION = ( 422 - 219 );
    private const int X_SIZE_OF_PERSONNEL_SCROLL_REGION = ( 589 - 573 );
    private const int Y_OF_PERSONNEL_SCROLL_REGION = 219;
    private const int X_OF_PERSONNEL_SCROLL_REGION = 573;
    private const int SIZE_OF_PERSONNEL_CURSOR = 19;

    private const int PrsnlOffSetX = -15;
    private const int Prsnl_DATA_OffSetX = 36;
    private const int PrsnlOffSetY = 10;

    private const string FACES_DIR = "FACES\\BIGFACES\\";
    private const string SMALL_FACES_DIR = "FACES\\";

    // reasons a merc left
    private const int DEPARTED_DEAD = 0;
    private const int DEPARTED_FIRED = 1;
    private const int DEPARTED_OTHER = 2;
    private const int DEPARTED_MARRIED = 3;
    private const int DEPARTED_CONTRACT_EXPIRED = 4;
    private const int DEPARTED_QUIT = 5;

    // ATM side buttons
    private const int OK_ATM = 0;
    private const int DEPOSIT_ATM = 1;
    private const int WIDTHDRAWL_ATM = 2;
    private const int CANCEL_ATM = 3;
    private const int CLEAR_ATM = 4;
    private const int NUMBER_ATM_BUTTONS = 5;

    private const int PERSONNEL_STAT_BTN = 0;
    private const int PERSONNEL_EMPLOYMENT_BTN = 1;
    private const int PERSONNEL_INV_BTN = 2;
    private const int PERSONNEL_NUM_BTN = 3;

    private const int PRSNL_STATS = 0;
    private const int PRSNL_EMPLOYMENT = 1;
    private const int PRSNL_INV = 2;

    // indices into pPersonnelScreenStrings
    private const int PRSNL_TXT_HEALTH = 0;
    private const int PRSNL_TXT_AGILITY = 1;
    private const int PRSNL_TXT_DEXTERITY = 2;
    private const int PRSNL_TXT_STRENGTH = 3;
    private const int PRSNL_TXT_LEADERSHIP = 4;
    private const int PRSNL_TXT_WISDOM = 5;
    private const int PRSNL_TXT_EXP_LVL = 6;
    private const int PRSNL_TXT_MARKSMANSHIP = 7;
    private const int PRSNL_TXT_MECHANICAL = 8;
    private const int PRSNL_TXT_EXPLOSIVES = 9;
    private const int PRSNL_TXT_MEDICAL = 10;
    private const int PRSNL_TXT_MED_DEPOSIT = 11;
    private const int PRSNL_TXT_CURRENT_CONTRACT = 12;
    private const int PRSNL_TXT_KILLS = 13;
    private const int PRSNL_TXT_ASSISTS = 14;
    private const int PRSNL_TXT_DAILY_COST = 15;
    private const int PRSNL_TXT_TOTAL_COST = 16;
    private const int PRSNL_TXT_CONTRACT = 17;
    private const int PRSNL_TXT_TOTAL_SERVICE = 18;
    private const int PRSNL_TXT_UNPAID_AMOUNT = 19;
    private const int PRSNL_TXT_HIT_PERCENTAGE = 20;
    private const int PRSNL_TXT_BATTLES = 21;
    private const int PRSNL_TXT_TIMES_WOUNDED = 22;
    private const int PRSNL_TXT_SKILLS = 23;
    private const int PRSNL_TXT_NOSKILLS = 24;

    private static int gubPersonnelInfoState = PRSNL_STATS;

    private static int uiCurrentInventoryIndex = 0;
    private static int guiSliderPosition;
    private static string sTransferString = string.Empty;
    private static readonly GUI_BUTTON[] giPersonnelATMSideButton = new GUI_BUTTON[NUMBER_ATM_BUTTONS];
    private static readonly ButtonPic[] giPersonnelATMSideButtonImage = new ButtonPic[NUMBER_ATM_BUTTONS];
    private static readonly GUI_BUTTON[] iNumberPadButtons = new GUI_BUTTON[10];
    private static readonly ButtonPic[] iNumberPadButtonsImages = new ButtonPic[10];

    private static readonly Point[] pAtmSideButtonPts =
    {
        new(533, 155),
        new(558, 110),
        new(558, 125),
        new(558, 140),
        new(558, 155),
    };

    private static readonly Point[] pPersonnelScreenPoints =
    {
        new(422 + PrsnlOffSetX, 205 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 215 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 225 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 235 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 245 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 255 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 315 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 270 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 280 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 290 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 300 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 395 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 385 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 415 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 425 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 445 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 380 + PrsnlOffSetY), // for contract price
        new(422 + PrsnlOffSetX, 435 + PrsnlOffSetY),
        new(140, 33),                                 // Personnel Header
        new(422 + PrsnlOffSetX, 330 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 340 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 355 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 365 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 375 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 385 + PrsnlOffSetY),
        new(422 + PrsnlOffSetX, 395 + PrsnlOffSetY),
    };

    private static HVOBJECT guiSCREEN = null!;
    private static HVOBJECT guiFACE = null!;
    private static HVOBJECT guiDEPARTEDTEAM = null!;
    private static HVOBJECT guiCURRENTTEAM = null!;
    private static HVOBJECT guiPersonnelInventory = null!;

    private static readonly GUI_BUTTON[] giPersonnelButton = new GUI_BUTTON[6];
    private static readonly ButtonPic[] giPersonnelButtonImage = new ButtonPic[6];
    private static readonly GUI_BUTTON[] giPersonnelInventoryButtons = new GUI_BUTTON[2];
    private static readonly ButtonPic[] giPersonnelInventoryButtonsImages = new ButtonPic[2];

    private static int iStartPersonId;   // iId of the person who is leftmost on the display
    private static int iLastPersonId;

    private static readonly ButtonPic[] giDepartedButtonImage = new ButtonPic[2];
    private static readonly GUI_BUTTON[] giDepartedButton = new GUI_BUTTON[2];

    private static readonly GUI_BUTTON[] giPersonnelATMStartButton = new GUI_BUTTON[3];
    private static readonly ButtonPic[] giPersonnelATMStartButtonImage = new ButtonPic[3];
    private static GUI_BUTTON giPersonnelATMButton = null!;
    private static ButtonPic giPersonnelATMButtonImage = null!;

    private static int fATMFlags = 0;
    private static int fOldATMFlags = 0;
    private static int iCurrentPersonSelectedId = -1;
    private static int giCurrentUpperLeftPortraitNumber = 0;
    private static bool fCurrentTeamMode = true;
    private static bool fShowAtmPanel = false;
    private static bool fShowAtmPanelStartButton = true;
    private static bool fCreatePeronnelDepartureButton = false;
    private static bool fOneFrameDelayInPersonnel = false;
    private static bool fCreatePersonnelPortraitMouseRegions = false;

    private static readonly MOUSE_REGION[] gPortraitMouseRegions = CreateRegions(20, nameof(gPortraitMouseRegions));
    private static readonly MOUSE_REGION[] gTogglePastCurrentTeam = CreateRegions(2, nameof(gTogglePastCurrentTeam));
    private static MOUSE_REGION gMouseScrollPersonnelINV = new(nameof(gMouseScrollPersonnelINV));

    private static int iCurPortraitId = 0;
    private static bool fCreateRegionsForPastCurrentToggle = false;


    /// <summary>C wcstol for the amounts typed into the ATM: the leading integer of the string, or 0.</summary>
    private static int wcstol(string s, object? end, int radix)
        => int.TryParse(s.Trim(), System.Globalization.NumberStyles.Integer, null, out int value) ? value : 0;

    /// <summary>Profile ids of every departed merc: the dead, then those who left, then the others.</summary>
    private static IEnumerable<int> PastMercIds()
    {
        foreach (int[] list in new[] { LaptopSaveInfo.ubDeadCharactersList, LaptopSaveInfo.ubLeftCharactersList, LaptopSaveInfo.ubOtherCharactersList })
        {
            foreach (int id in list)
            {
                if (id != -1)
                {
                    yield return id;
                }
            }
        }
    }

    public static void EnterPersonnel()
    {
    	fReDrawScreenFlag=true;
      iStartPersonId=-1;

    	iCurrentPersonSelectedId = -1;
	
    	uiCurrentInventoryIndex = 0;
    	guiSliderPosition = 0;

    	iCurPortraitId = 0;

    	// load graphics for screen
    	LoadPersonnelGraphics( );
  
    	// show atm panel
    	fShowAtmPanelStartButton = true;

    	// create buttons needed
    	CreateDestroyButtonsForPersonnelDepartures( );

    	// load personnel
    	LoadPersonnelScreenBackgroundGraphics( );

	
    	// render screen
    	RenderPersonnel( );

    	// how many people do we have?..if you have someone set default to 0
    	if( GetNumberOfMercsDeadOrAliveOnPlayersTeam() > 0 )
    	{
    	  iCurrentPersonSelectedId = GetIdOfFirstDisplayedMerc( );
    	}

    	fCreatePersonnelPortraitMouseRegions = true;

    	CreateDestroyMouseRegionsForPersonnelPortraits( );
    	// set states of en- dis able buttons
    	//SetPersonnelButtonStates( );

    	fCreateRegionsForPastCurrentToggle = true;

    	CreateDestroyCurrentDepartedMouseRegions( );
	
    	// create buttons for screen
    	CreatePersonnelButtons( );

    	// set states of en- dis able buttons
    	SetPersonnelButtonStates( );
	
      return;
    }

    public static void ExitPersonnel()
    {


    	if( fCurrentTeamMode == false )
    	{
    		fCurrentTeamMode = true;
    		CreateDestroyButtonsForDepartedTeamList( );
    		fCurrentTeamMode = false;
    	}

    	// get rid of atm panel buttons
    	fShowAtmPanelStartButton = false;
    	fShowAtmPanel = false;
    	fATMFlags = 0;
    	CreateDestroyStartATMButton( );
    	CreateDestroyATMButton( );

    //	fShowInventory = FALSE;
    	gubPersonnelInfoState = PRSNL_STATS;

    	CreateDestroyPersonnelInventoryScrollButtons( );

    	// get rid of graphics
      RemovePersonnelGraphics( );

    	 DeletePersonnelScreenBackgroundGraphics(  );

    	CreateDestroyButtonsForPersonnelDepartures( );

    	// delete buttons
      DeletePersonnelButtons( );

    	fCreatePersonnelPortraitMouseRegions = false;

    	// delete mouse regions
    	CreateDestroyMouseRegionsForPersonnelPortraits( );

    	fCreateRegionsForPastCurrentToggle = false;

    	CreateDestroyCurrentDepartedMouseRegions( );


	
    	return;
    }

    public static void HandlePersonnel()
    {
       //RenderButtonsFastHelp( );
    	CreateDestroyButtonsForPersonnelDepartures( );

    	// create / destroy buttons for scrolling departed list
    	CreateDestroyButtonsForDepartedTeamList( );

    	// enable / disable departures buttons
    	EnableDisableDeparturesButtons( );

    	// create destroy inv buttons as needed
    	CreateDestroyPersonnelInventoryScrollButtons( );

    	// enable disable buttons as needed
    	EnableDisableInventoryScrollButtons( );

    	HandlePersonnelKeyboard( );

    	// handle timed modes for ATM
    	HandleTimedAtmModes( );

    	 return;
    }

    private static bool LoadPersonnelGraphics()
    {
    	// load graphics needed for personnel screen
  
    	// load graphics

      // title bar
      guiTITLE = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\programtitlebar.sti"));

    	// the background grpahics
      guiSCREEN = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\personnelwindow.sti"));

    	guiPersonnelInventory = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\personnel_inventory.sti"));

    	return(true);
    }

    private static void RemovePersonnelGraphics()
    {
    	// delete graphics needed for personnel screen
 
    	video.DeleteVideoObject(guiSCREEN);
    	video.DeleteVideoObject(guiTITLE);
    	video.DeleteVideoObject(guiPersonnelInventory);


    	return;
    }

    public static void RenderPersonnel()
    {
      HVOBJECT hHandle;
      int iCounter=0;
      // re-renders personnel screen
    	// render main background	
	
      // blit title
      hHandle = guiTITLE;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y - 2, VO_BLT.SRCTRANSPARENCY,null );
  
    	// blit screen
    	hHandle = guiSCREEN;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y + 22, VO_BLT.SRCTRANSPARENCY,null );

      // render pictures of mercs on scnree
     	//RenderPersonnelPictures( );

    	// display header for screen
    	//DisplayHeader( );

    	// what page are we on?..display it
    	//DrawPageNumber( );

    		// display border
    	//GetVideoObject( out hHandle, guiLaptopBACKGROUND );
    	//BltVideoObject(FRAME_BUFFER, hHandle, 0,108, 23, VO_BLT_SRCTRANSPARENCY,NULL);

	
    	// invalidte the region we blitted to
    	//InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_LR_Y);

    	// render personnel screen background
    	RenderPersonnelScreenBackground( );




    	// show team
    	DisplayPicturesOfCurrentTeam( );

    	DisplayPastMercsPortraits( );

    	// show selected merc
    	DisplayFaceOfDisplayedMerc( );

    	// show current team size
    	DisplayNumberOnCurrentTeam( );


    	// show departed team size
    	DisplayNumberDeparted( );

    	// list stats row headers for team stats list
    	DisplayPersonnelTeamStats( );

    	// showinventory of selected guy if applicable
    	DisplayInventoryForSelectedChar( );

    	// the average stats for the current team
    	DisplayAverageStatValuesForCurrentTeam( );

    	// lowest stat values
    	DisplayLowestStatValuesForCurrentTeam( );
	

    	// past team
    	DisplayStateOfPastTeamMembers( );



    	// title bar
    	BlitTitleBarIcons(  );

    	// show text on titlebar
    	DisplayPersonnelTextOnTitleBar( );

    	// the highest stats
    	DisplayHighestStatValuesForCurrentTeam( );
	
    	// render the atm panel
    	RenderAtmPanel( );

    	DisplayAmountOnCurrentMerc( );

    	// en-dis-able start button
    	UpDateStateOfStartButton( );

    	return; 
    }

    private static bool RenderPersonnelPictures()
    {
    	// will render portraits of personnel onscreen
    	// find person with iStartPersonId, unless it is -1, then find first bActive Merc on Staff
    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;
    	bool fFound=false;
    	int iCounter=0;
    	int iSlot=0;
    	int cnt=0;
    	int iCurrentId = 0;


    	pSoldier = MercPtrs[ cnt ];
    	pTeamSoldier = pSoldier;


    	if(iStartPersonId==-1)
    	{
       cnt = gTacticalStatus.Team[ pSoldier.bTeam ].bFirstID;
       for ( pSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	 {	
    		 if ( pSoldier.bLife >= OKLIFE && pSoldier.bActive )
    		 {
    			 fFound = true;
    			 iStartPersonId=cnt;
    			 break;
    		 }
    	 }
       if(!fFound)
    		return (false);
    	}
    	else
    	{
       iCurrentId=iStartPersonId;
    	 fFound=true;
    	 cnt=iCurrentId;
    	}



    	while(fFound)
    	{
    	  // the soldier's ID is found
    	  // render Face
    	  fFound=false;
    		RenderPersonnelFace(iCurrentId, iSlot, false, false, false );
    		// draw stats
        RenderPersonnelStats(iCurrentId, iSlot);
    		DisplayCharName( iCurrentId, iSlot);
    		//find next guy
    		pSoldier=MercPtrs[iCurrentId];
    		cnt++;		
        for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    		{	
    		  if ( pTeamSoldier.bLife >= OKLIFE && pTeamSoldier.bActive )
    			{
    				if( pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) )
    				{
    					return( false );
    				}

    			  fFound = true;
    				iSlot++;
    			  break;
    			}
    		}
    		if(iSlot>=MAX_SLOTS)
    			fFound=false;
        iCurrentId=cnt;
    	}
  
    	return(true);
    }

    public static void RenderPersonnelStats(int iId, int iSlot)
    {

    	int iCounter=0;
    	// will render the stats of person iId in slot iSlot
    	FontSubSystem.SetFont(PERS_FONT);
    	FontSubSystem.SetFontForeground(PERS_TEXT_FONT_COLOR);
    	FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

    	//for(iCounter=0; iCounter <PERS_COUNT; iCounter++)
    	// mprintf((INT16)(pPersonnelScreenPoints[iCounter].X+(iSlot*IMAGE_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);


    	if( gubPersonnelInfoState == PERSONNEL_STAT_BTN )
    	{
    		DisplayCharStats(iId, iSlot);
    	}
    	else if( gubPersonnelInfoState == PERSONNEL_EMPLOYMENT_BTN )
    	{
    		DisplayEmploymentinformation( iId, iSlot );		  
    	}
    }

    private static bool RenderPersonnelFace(int iId, int iSlot, bool fDead, bool fFired, bool fOther)
    {
    	string sTemp = string.Empty;
    	HVOBJECT hFaceHandle;
    	int iCounter = 7;

    	// draw face to soldier iId in slot iSlot
	
    	// special case?..player generated merc
    	if( fCurrentTeamMode == true )
    	{
	
    		if( ( 50 < (int)MercPtrs[iId].ubProfile )&&( 57 > (int)MercPtrs[iId].ubProfile ) )
    		{
    			sTemp = wprintf( "%s%03d.sti", FACES_DIR, 	gMercProfiles[(NPCID)(MercPtrs[iId] . ubProfile)].ubFaceIndex );	
    		}
    		else
    		{
    			sTemp = wprintf( "%s%02d.sti", FACES_DIR,	Menptr[ iId ].ubProfile);
    		}
    	}
    	else
    	{
    		//if this is not a valid merc
    		if( !fDead && !fFired && !fOther )
    		{
    			return( true );
    		}

    		if( ( 50 < 	iId )&&( 57 > 	iId ) )
    		{
    			sTemp = wprintf( "%s%03d.sti", FACES_DIR, 	gMercProfiles[(NPCID)(iId)].ubFaceIndex );	
    		}
    		else
    		{
    			sTemp = wprintf( "%s%02d.sti", FACES_DIR,	 iId );
    		}
    	}

    	if( fCurrentTeamMode == true )
    	{
    		if( MercPtrs[iId].uiStatusFlags.HasFlag(SOLDIER.VEHICLE) )
    		{
    			return( true );
    		}
    	}

    	guiFACE = video.GetVideoObject(Utils.FilenameForBPP(sTemp));

    	//Blt face to screen to 
    	hFaceHandle = guiFACE;

    	if( fCurrentTeamMode == true )
    	{
    		if( MercPtrs[iId].bLife <= 0 )
    		{
    			hFaceHandle.pShades[ 0 ]		= video.Create16BPPPaletteShaded( hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true );
			
    			//set the red pallete to the face
    			guiFACE.ShadeCurrentPixels = guiFACE.pShades[0];
    		}
    	}
    	else
    	{
    		if( fDead == true )
    		{
    			hFaceHandle.pShades[ 0 ]		= video.Create16BPPPaletteShaded( hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true );
			
    			//set the red pallete to the face
    			guiFACE.ShadeCurrentPixels = guiFACE.pShades[0];
    		}
    	}
	

      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0,IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH), IMAGE_BOX_Y, VO_BLT.SRCTRANSPARENCY);
  

    	//if the merc is dead, display it
    	if( !fCurrentTeamMode )
    	{
    		int iHeightOfText;

    		iHeightOfText = FontSubSystem.DisplayWrappedString(new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y)), IMAGE_NAME_WIDTH, 1, PERS_FONT, PERS_FONT_COLOR, gMercProfiles[(NPCID)(iId)].zName, 0, TextJustifies.CENTER_JUSTIFIED | TextJustifies.DONT_DISPLAY_TEXT);

    		//if the string will rap
    		if( ( iHeightOfText - 2 ) > FontSubSystem.GetFontHeight( PERS_FONT ) )
    		{
    			//raise where we display it, and rap it
    			FontSubSystem.DisplayWrappedString(new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y - FontSubSystem.GetFontHeight( PERS_FONT ))), IMAGE_NAME_WIDTH, 1, PERS_FONT, PERS_FONT_COLOR, gMercProfiles[(NPCID)(iId)].zName, 0, TextJustifies.CENTER_JUSTIFIED);
    		}
    		else
    		{
    			FontSubSystem.DrawTextToScreen(gMercProfiles[(NPCID)(iId)].zName, new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y)), IMAGE_NAME_WIDTH, PERS_FONT, PERS_FONT_COLOR, 0, TextJustifies.CENTER_JUSTIFIED);
    		}
    //		FontSubSystem.DrawTextToScreen(gMercProfiles[(NPCID)(iId)].zName, new(( INT16 ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( INT16 ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, PERS_FONT, PERS_FONT_COLOR, FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}

    /*
    removed cause we already show this under the picture, instead display the mercs FULL name ( above )
    	if( fCurrentTeamMode == true )
    	{
    		if( Menptr[ iId ].bLife <= 0 )
    		{
    			//if the merc is dead, display it
    			FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[0], new(( int ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( int ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}
    	}
    	else
    	{
    		if( fDead )
    		{
    			//if the merc is dead, display it
    			FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[0], new(( int ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( int ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    //			FontSubSystem.DrawTextToScreen(AimPopUpText[ AIM_MEMBER_DEAD ], new(( INT16 ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( INT16 ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, FONT14ARIAL, 145, FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}
    		else if( fFired )
    		{
    			//if the merc is dead, display it
    			FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[1], new(( int ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( int ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}
    		else if( fOther )
    		{
    			//if the merc is dead, display it
    			FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[2], new(( int ) ( IMAGE_BOX_X+(iSlot*IMAGE_BOX_WIDTH) ), ( int ) ( IMAGE_BOX_Y + 107 )), IMAGE_BOX_WITH_NO_BORDERS, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}
    	}

    */	

    	video.DeleteVideoObject(guiFACE);
	
	
    	return (true);
    }

    private static bool NextPersonnelFace()
    {

    	if( iCurrentPersonSelectedId == -1 )
    	{
    		return ( true );
    	}

    	if( fCurrentTeamMode == true )
    	{
    		// wrap around?
    		if( iCurrentPersonSelectedId == GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) - 1 )
    		{
    			iCurrentPersonSelectedId = 0;
    			return( false );	//def added 3/14/99 to enable disable buttons properly
    		}
    		else
    		{
    			iCurrentPersonSelectedId++;
    		}
    	}
    	else
    	{
    		if ( ( ( iCurPortraitId + 1) == ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) - giCurrentUpperLeftPortraitNumber ) )
    		{
    			// about to go off the end
    			giCurrentUpperLeftPortraitNumber = 0;
    			iCurPortraitId = 0;

    		}
    		else if(  iCurPortraitId == 19 ) 
    		{
    			giCurrentUpperLeftPortraitNumber += 20;
    			iCurPortraitId = 0;
    		}
    		else
    		{
    			iCurPortraitId++;
    		}
    		// get of this merc in this slot

    		iCurrentPersonSelectedId =  iCurPortraitId;
    		fReDrawScreenFlag = true;
    	}

    		return ( true );
    }

    private static bool PrevPersonnelFace()
    {
	
    	if( iCurrentPersonSelectedId == -1 )
    	{
    		return ( true );
    	}

      if( fCurrentTeamMode == true )
    	{
    		// wrap around?
    		if( iCurrentPersonSelectedId == 0  )
    		{
    			iCurrentPersonSelectedId = GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) - 1;

    			if( iCurrentPersonSelectedId == 0 )
    			{
    				return( false );	//def added 3/14/99 to enable disable buttons properly
    			}
    		}
    		else
    		{
    			iCurrentPersonSelectedId--;
    		}
    	}
    	else
    	{
    		if  ( ( iCurPortraitId == 0 ) && ( giCurrentUpperLeftPortraitNumber == 0 ) )
    		{
    			// about to go off the end
    			giCurrentUpperLeftPortraitNumber = ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) - ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) % 20;
    			iCurPortraitId = ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) % 20 ;
    			iCurPortraitId--;

    		}
    		else if(  iCurPortraitId == 0 ) 
    		{
    			giCurrentUpperLeftPortraitNumber -= 20;
    			iCurPortraitId = 19;
    		}
    		else
    		{
    			iCurPortraitId--;
    		}
    		// get of this merc in this slot

    		iCurrentPersonSelectedId =  iCurPortraitId;
    		fReDrawScreenFlag = true;
    	}

    	return ( true );
    }

    private static void CreatePersonnelButtons()
    {

    	// left button
    	giPersonnelButtonImage[0]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\personnelbuttons.sti" ,-1,0,-1,1,-1 );
    	giPersonnelButton[0] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[0], new Point(PREV_MERC_FACE_X, MERC_FACE_SCROLL_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)LeftButtonCallBack);
	
    	// right button
    	giPersonnelButtonImage[1]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\personnelbuttons.sti" ,-1,2,-1,3,-1 );
    	giPersonnelButton[1] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[1], new Point(NEXT_MERC_FACE_X, MERC_FACE_SCROLL_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)RightButtonCallBack);
	

	
    	/*
    	// left button
    	giPersonnelButtonImage[0]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\arrows.sti" ,-1,0,-1,1,-1 );
    	giPersonnelButton[0] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[0], new Point(LEFT_BUTTON_X, BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)LeftButtonCallBack);
	
    	// right button
    	giPersonnelButtonImage[1]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\arrows.sti" ,-1,6,-1,7,-1 );
    	giPersonnelButton[1] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[1], new Point(RIGHT_BUTTON_X, BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)RightButtonCallBack);
	
    	// left FF button
    	giPersonnelButtonImage[2]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\arrows.sti" ,-1,3,-1,4,-1 );
    	giPersonnelButton[2] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[2], new Point(LEFT_BUTTON_X, BUTTON_Y + 22), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)LeftFFButtonCallBack);
  
    	// right ff button
    	giPersonnelButtonImage[3]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\arrows.sti" ,-1,9,-1,10,-1 );
    	giPersonnelButton[3] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[3], new Point(RIGHT_BUTTON_X, BUTTON_Y + 22), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)RightFFButtonCallBack);
      */
    	// set up cursors
    	ButtonSubSystem.SetButtonCursor(giPersonnelButton[0], CURSOR.LAPTOP_SCREEN);
    	ButtonSubSystem.SetButtonCursor(giPersonnelButton[1], CURSOR.LAPTOP_SCREEN);
    	//SetButtonCursor(giPersonnelButton[2], CURSOR_LAPTOP_SCREEN);
    	//SetButtonCursor(giPersonnelButton[3], CURSOR_LAPTOP_SCREEN);


    	return;
    }

    private static void DeletePersonnelButtons()
    {

      ButtonSubSystem.RemoveButton(giPersonnelButton[0] );
    	ButtonSubSystem.UnloadButtonImage(giPersonnelButtonImage[0] );
    	ButtonSubSystem.RemoveButton(giPersonnelButton[1] );
    	ButtonSubSystem.UnloadButtonImage( giPersonnelButtonImage[1] );
    	/*ButtonSubSystem.RemoveButton(giPersonnelButton[2] );
    	ButtonSubSystem.UnloadButtonImage( giPersonnelButtonImage[2] );
    	ButtonSubSystem.RemoveButton(giPersonnelButton[3] );
    	ButtonSubSystem.UnloadButtonImage( giPersonnelButtonImage[3] );
    	*/
    	return;
    }

    private static void LeftButtonCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    		 btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    		 fReDrawScreenFlag=true;
    		 PrevPersonnelFace( );
    		 uiCurrentInventoryIndex = 0;
    		 guiSliderPosition = 0;
		 
    		}
    	}
    }

    private static void LeftFFButtonCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    		 btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    		 fReDrawScreenFlag=true;
    		 PrevPersonnelFace( );
         PrevPersonnelFace( );
         PrevPersonnelFace( );
    		 PrevPersonnelFace( );
		 
    		 // set states
    		 SetPersonnelButtonStates( );
    		}
    	}
    }

    private static void RightButtonCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{ 
         fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    		 btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    		 fReDrawScreenFlag=true;
         NextPersonnelFace( );
    		 uiCurrentInventoryIndex = 0;
    		 guiSliderPosition = 0;
		 

    		}
    	}
    }

    private static void RightFFButtonCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{ 
         fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    		 btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    		 fReDrawScreenFlag=true;
         NextPersonnelFace( );
         NextPersonnelFace( );
         NextPersonnelFace( );
         NextPersonnelFace( );

    		 // set states
    		 SetPersonnelButtonStates( ); 
    		}
    	}
    }

    private static void DisplayHeader()
    {
      FontSubSystem.SetFont(PERS_HEADER_FONT);
    	FontSubSystem.SetFontForeground(PERS_FONT_COLOR);
    	FontSubSystem.SetFontBackground( 0 );

    	mprintf(pPersonnelScreenPoints[18].X,pPersonnelScreenPoints[18].Y,pPersonnelTitle[0]);
	
    	return;
    }

    private static void DisplayCharName(int iId, int iSlot)
    {
      // get merc's nickName, assignment, and sector location info
    	int sX, sY;
      SOLDIERTYPE? pSoldier;
    	string sString = string.Empty;
    	string sTownName = string.Empty;
    	TOWNS bTownId = TOWNS.BLANK_SECTOR;
    	int	iHeightOfText;

    	sTownName = string.Empty;

    	pSoldier=MercPtrs[iId];

      FontSubSystem.SetFont(CHAR_NAME_FONT);
    	FontSubSystem.SetFontForeground(PERS_TEXT_FONT_COLOR);
    	FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
	
    	if( pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) )
    	{
    		return;
    	}

    	if( Menptr[iId].bAssignment == Assignment.ASSIGNMENT_POW )
    	{
    	}
    	else if( Menptr[iId].bAssignment == Assignment.IN_TRANSIT )
    	{
    	}
    	else
      {
    		// name of town, if any
    		bTownId = StrategicMap.GetTownIdForSector( Menptr[iId].sSectorX, Menptr[iId].sSectorY );

    		if( bTownId != TOWNS.BLANK_SECTOR )
    		{
    			sTownName = wprintf( "%s", pTownNames[ bTownId ] );
    		}
    	}



    	if( sTownName.Length > 0 )
    	{
    		//nick name - town name
    		sString = wprintf( "%s - %s", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].zNickname, sTownName );
    	}
    	else
    	{
    		//nick name
    		sString = wprintf( "%s", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].zNickname );
    	}


    	// nick name - assignment
    	FontSubSystem.FindFontCenterCoordinates( IMAGE_BOX_X-5, 0, IMAGE_BOX_WIDTH + 90, 0, sString, CHAR_NAME_FONT, out sX, out sY );

    	// check to see if we are going to go off the left edge
    	if( sX < pPersonnelScreenPoints[ 0 ].X )
    	{
    		sX = ( int )pPersonnelScreenPoints[ 0 ].X;
    	}
	
    	//Display the mercs name
    	mprintf(sX+iSlot*IMAGE_BOX_WIDTH, CHAR_NAME_Y, sString );

    	sString = wprintf( "%s", pPersonnelAssignmentStrings[(int)Menptr[iId].bAssignment]);

    	// nick name - assignment
    	FontSubSystem.FindFontCenterCoordinates( IMAGE_BOX_X-5, 0, IMAGE_BOX_WIDTH + 90, 0, sString, CHAR_NAME_FONT, out sX, out sY );

    	// check to see if we are going to go off the left edge
    	if( sX < pPersonnelScreenPoints[ 0 ].X )
    	{
    		sX = ( int )pPersonnelScreenPoints[ 0 ].X;
    	}
	
    	mprintf(sX+iSlot*IMAGE_BOX_WIDTH, CHAR_LOC_Y, sString );


    	//
    	// Display the mercs FULL name over top of their portrait
    	//

    	//first get height of text to be displayed
    	iHeightOfText = FontSubSystem.DisplayWrappedString(new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y)), IMAGE_NAME_WIDTH, 1, PERS_FONT, PERS_FONT_COLOR, gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].zName, 0, TextJustifies.CENTER_JUSTIFIED | TextJustifies.DONT_DISPLAY_TEXT);

    	//if the string will rap
    	if( ( iHeightOfText - 2 ) > FontSubSystem.GetFontHeight( PERS_FONT ) )
    	{
    		//raise where we display it, and rap it
    		FontSubSystem.DisplayWrappedString(new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y - FontSubSystem.GetFontHeight( PERS_FONT ))), IMAGE_NAME_WIDTH, 1, PERS_FONT, PERS_FONT_COLOR, gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].zName, 0, TextJustifies.CENTER_JUSTIFIED);
    	}
    	else
    	{
    		FontSubSystem.DrawTextToScreen(gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].zName, new(IMAGE_BOX_X, (int)(IMAGE_BOX_Y+IMAGE_FULL_NAME_OFFSET_Y)), IMAGE_NAME_WIDTH, PERS_FONT, PERS_FONT_COLOR, 0, TextJustifies.CENTER_JUSTIFIED);
    	}

    /*
    Moved so the name of the town will be in the same line as the name


    	if( Menptr[iId].bAssignment == Assignment.ASSIGNMENT_POW )
    	{
    //		FindFontCenterCoordinates( IMAGE_BOX_X-5, 0, IMAGE_BOX_WIDTH, 0, pPOWStrings[ 1 ], CHAR_NAME_FONT, out sX, out sY );
    //	  mprintf(sX+iSlot*IMAGE_BOX_WIDTH, CHAR_NAME_Y+20,pPOWStrings[ 1 ] );
    	}
    	else if( Menptr[iId].bAssignment == Assignment.IN_TRANSIT )
    	{
    		return;
    	}
    	else
      {
    		// name of town, if any
    		bTownId = StrategicMap.GetTownIdForSector( Menptr[iId].sSectorX, Menptr[iId].sSectorY );

    		if( bTownId != TOWNS.BLANK_SECTOR )
    		{
    			FontSubSystem.FindFontCenterCoordinates( IMAGE_BOX_X - 5, 0, IMAGE_BOX_WIDTH, 0, pTownNames[ bTownId ], CHAR_NAME_FONT, out sX, out sY );
    			mprintf( sX + ( iSlot * IMAGE_BOX_WIDTH ), CHAR_NAME_Y + 20, pTownNames[ bTownId ]);
    		}
    	}
    */
  
    	return;
    }

    private static void DisplayCharStats(int iId, int iSlot)
    {
    	int iCounter=0;
    	string sString = string.Empty;
    //	wchar_t sStringA[ 50 ];
    	int sX, sY;
    	int uiHits = 0;
    	SOLDIERTYPE? pSoldier = Menptr[iId];  // TODO(port): address-of
    	bool	fAmIaRobot = AM_A_ROBOT( pSoldier );
	
	
    	if( pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) )
    	{
    		return;
    	}

    	// display the stats for a char
    	for(iCounter=0;iCounter <MAX_STATS; iCounter++)
    	{
    		switch(iCounter)
    		{
    		 case 0:
    			 // health
    			 if( Menptr[iId].bAssignment != Assignment.ASSIGNMENT_POW )
    			 {
    				if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bLifeDelta > 0 )
    				{
    					sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bLifeDelta);
    				  FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 	  mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			
    				}

    			 //else
    			 //{
    				 sString = wprintf( "%d/%d",Menptr[iId].bLife,Menptr[iId].bLifeMax);
    			 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( pPOWStrings[ 1 ] );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_HEALTH]);		  
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			 break;
    		 case 1:
    			 // agility
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bAgilityDelta > 0 )
    				 {
    					 sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bAgilityDelta);
    					 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 		 mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    					 sString = wprintf( "%d",Menptr[iId].bAgility);
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		 
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			 break;
    		 case 2:
    		  // dexterity
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bDexterityDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bDexterityDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bDexterity);
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			break;
    		 case 3:
    		 // strength
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bStrengthDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bStrengthDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bStrength );
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 4:
    		  // leadership
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bLeadershipDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bLeadershipDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20 +TEXT_DELTA_OFFSET), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bLeadership );
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 5:
    		  // wisdom
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bWisdomDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bWisdomDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 
    				 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bWisdom );
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 6:
    		  // exper
    			 if( !fAmIaRobot )
    			 {

    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bExpLevelDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bExpLevelDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20 +TEXT_DELTA_OFFSET), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);
    				 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bExpLevel );
    				 //}
    			 }				
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }


           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 7:
    			 //mrkmanship
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMarksmanshipDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMarksmanshipDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bMarksmanship);
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }


           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
         case 8:
    		  // mech
    			 if( !fAmIaRobot )
    			 {

    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMechanicDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMechanicDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    					 sString = wprintf( "%d",Menptr[iId].bMechanical);
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
         case 9:
    		  // exp
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bExplosivesDelta > 0 )
    				 {
    						sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bExplosivesDelta);
    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20+TEXT_DELTA_OFFSET ), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 			mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bExplosive);
    				 //}

    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }

           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 10:
           // med
    			 if( !fAmIaRobot )
    			 {
    				 if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMedicalDelta > 0 )
    				 {
    					 sString = wprintf( "( %+d )", gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bMedicalDelta);
    					 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20 +TEXT_DELTA_OFFSET), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    			 		 mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);			 }
    				 //else
    				 //{
    				 		sString = wprintf( "%d",Menptr[iId].bMedical);
    				 //}
    			 }
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }


    			 mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);			
    			 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    			 mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);	
    		 break;

    		 case 14:
    		 // kills
          mprintf((int)(pPersonnelScreenPoints[21].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[21].Y,pPersonnelScreenStrings[PRSNL_TXT_KILLS]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usKills);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[21].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[21].Y,sString);
    		 break;
    		 case 15:
    			// assists
    			mprintf((int)(pPersonnelScreenPoints[22].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[22].Y,pPersonnelScreenStrings[PRSNL_TXT_ASSISTS]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usAssists);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[22].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[22].Y,sString);
    		 break;
    		  case 16:
    			// shots/hits
    			mprintf((int)(pPersonnelScreenPoints[23].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[23].Y,pPersonnelScreenStrings[PRSNL_TXT_HIT_PERCENTAGE]);		 
    			uiHits = ( int )gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usShotsHit;
    			uiHits *= 100;
			
    			// check we have shot at least once
    			if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usShotsFired > 0 )
    			{
    			  uiHits /= ( int )gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usShotsFired;
    			}
    			else
    			{
    				// no, set hit % to 0
    				uiHits = 0;
    			}


    			sString = wprintf( "%d %%%%",uiHits);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[23].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          sX += FontSubSystem.StringPixLength( "%",  PERS_FONT );
    			mprintf(sX,pPersonnelScreenPoints[23].Y,sString);
    		 break;
    		  case 17:
    			// battles
    			mprintf((int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[24].Y,pPersonnelScreenStrings[PRSNL_TXT_BATTLES]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usBattlesFought);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[24].Y,sString);
    		 break;
    		 case 18:
    			// wounds
    			mprintf((int)(pPersonnelScreenPoints[25].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[25].Y,pPersonnelScreenStrings[PRSNL_TXT_TIMES_WOUNDED]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usTimesWounded);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[25].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[25].Y,sString);
    		 break;

    			// The Mercs Skills
    			case 19:
    			{
    				int iWidth;
    				int iMinimumX;
    				int	bScreenLocIndex = 19;	//if you change the '19', change it below in the if statement

    				//Display the 'Skills' text
    				mprintf( (int)( pPersonnelScreenPoints[bScreenLocIndex].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[bScreenLocIndex].Y,pPersonnelScreenStrings[ PRSNL_TXT_SKILLS ] );

    				//KM: April 16, 1999
    				//Added support for the German version, which has potential string overrun problems.  For example, the text "Skills:" can
    				//overlap "NightOps (Expert)" because the German strings are much longer.  In these cases, I ensure that the right 
    				//justification of the traits don't overlap.  If it would, I move it over to the right.
    				iWidth = FontSubSystem.StringPixLength( pPersonnelScreenStrings[ PRSNL_TXT_SKILLS ], PERS_FONT );
    				iMinimumX = iWidth + pPersonnelScreenPoints[bScreenLocIndex].X + iSlot * TEXT_BOX_WIDTH + 2;

    				if( !fAmIaRobot )
    				{
    					SkillTrait bSkill1 = gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bSkillTrait;
    					SkillTrait bSkill2 = gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].bSkillTrait2;

    					//if the 2 skills are the same, add the '(expert)' at the end
    					if( bSkill1 == bSkill2 && bSkill1 != SkillTrait.NO_SKILLTRAIT )
    					{
    						sString = wprintf( "%s %s", gzMercSkillText[(int)(bSkill1)], gzMercSkillText[(int)(SkillTrait.NUM_SKILLTRAITS)] );

    						FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[bScreenLocIndex].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );

    						//KM: April 16, 1999
    						//Perform the potential overrun check
    						if( sX <= iMinimumX )
    						{
    							FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH) + TEXT_BOX_WIDTH-20 +TEXT_DELTA_OFFSET), 0, 30, 0, sString, PERS_FONT, out sX, out sY );
    							sX = (int)Math.Max( sX, iMinimumX );
    						}
						
    						mprintf(sX,pPersonnelScreenPoints[bScreenLocIndex].Y,sString);
    					}
    					else
    					{
    						//Display the first skill
    						if( bSkill1 != SkillTrait.NO_SKILLTRAIT )
    						{
    							sString = wprintf( "%s", gzMercSkillText[(int)(bSkill1)] );

    							FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[bScreenLocIndex].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );

    							//KM: April 16, 1999
    							//Perform the potential overrun check
    							sX = (int)Math.Max( sX, iMinimumX );
							
    							mprintf(sX,pPersonnelScreenPoints[bScreenLocIndex].Y,sString);

    							bScreenLocIndex++;
    						}

    						//Display the second skill
    						if( bSkill2 != SkillTrait.NO_SKILLTRAIT )
    						{
    							sString = wprintf( "%s", gzMercSkillText[(int)(bSkill2)] );

    							FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[bScreenLocIndex].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );

    							//KM: April 16, 1999
    							//Perform the potential overrun check
    							sX = (int)Math.Max( sX, iMinimumX );

    							mprintf(sX,pPersonnelScreenPoints[bScreenLocIndex].Y,sString);

    							bScreenLocIndex++;
    						}

    						//if no skill was displayed
    						if( bScreenLocIndex == 19 )
    						{
    							sString = wprintf( "%s", pPersonnelScreenStrings[ PRSNL_TXT_NOSKILLS ] );

    							FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[bScreenLocIndex].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    							mprintf(sX,pPersonnelScreenPoints[bScreenLocIndex].Y,sString);
    						}
    					}
    				}
    			 else
    			 {
    				 sString = wprintf( "%s", "N/A" );
    			 }
    			}
    			break;
    /*
    		 case 19:
    			 // total contract time served
    			mprintf((int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[24].Y,pPersonnelScreenStrings[18]);		 
    			if( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usTotalDaysServed > 0 )
    			{
    				sString = wprintf( "%d %s",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usTotalDaysServed - 1, "d" );
    			}
    			else
    			{
    				sString = wprintf( "%d %s",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usTotalDaysServed, "d" );
    			}
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[24].Y,sString);
    		 break;
    */
    		}
    	}
    	return;
    }

    private static int GetLastMercId()
    {
    	// rolls through list of mercs and returns how many on team
      SOLDIERTYPE? pSoldier = null, pTeamSoldier = null;
      int cnt=0;
    	int iCounter=0;
    	pSoldier = MercPtrs[0];	
	
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    		{
    		 if( ( pTeamSoldier.bActive ) && ( pTeamSoldier.bLife > 0 ) )
    			 iCounter++;
    		}
    	return iCounter;
    }

    private static void DrawPageNumber()
    {
    	// draws the page number
	
    	string sString = string.Empty;
    	int sX, sY;
      int iPageNumber, iLastPage;
  
    	return;

    	// get last page number, and current page too
    	iLastPage=GetLastMercId()/MAX_SLOTS;
    	iPageNumber=iStartPersonId/MAX_SLOTS;
      iPageNumber++;
    	if(iLastPage==0)
    	 iLastPage++;

    	// get current and last pages
    	sString = wprintf( "%d/%d", iPageNumber, iLastPage);
  
    	// set up font
    	FontSubSystem.SetFont(PERS_FONT);
    	FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
    	FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
    	FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
	
    	// center
    	FontSubSystem.FindFontCenterCoordinates( PAGE_X, PAGE_Y, PAGE_BOX_WIDTH, PAGE_BOX_HEIGHT, sString, PERS_FONT, out sX, out sY );
	
    	// print page number
    	mprintf(sX, sY, sString);
  
    	// reset shadow
    	FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

    	return;
    }

    private static void SetPersonnelButtonStates()
    {
    	// this function will look at what page we are viewing, enable and disable buttons as needed
  
    	if( ! PrevPersonnelFace( ) )
    	{
    		// first page, disable left buttons
    
    //		DisableButton( 	giPersonnelButton[ 2 ] );
    		ButtonSubSystem.DisableButton( 	giPersonnelButton[ 0 ] );
    	}
    	else
    	{
    		// enable buttons
        NextPersonnelFace( );

    		// enable buttons
    //		EnableButton( giPersonnelButton[ 2 ] );
    		ButtonSubSystem.EnableButton( giPersonnelButton[ 0 ] );
    	}

    	if( ! NextPersonnelFace( ) )
    	{
	 
    //		DisableButton( 	giPersonnelButton[ 3 ] );
    		ButtonSubSystem.DisableButton( 	giPersonnelButton[ 1 ] );
    	}
    	else
    	{
        // decrement page
        PrevPersonnelFace( );
    			// enable buttons
    //		EnableButton( giPersonnelButton[ 3 ] );
    		ButtonSubSystem.EnableButton( giPersonnelButton[ 1 ] );
    	}

    	return;
    }

    public static void RenderPersonnelScreenBackground()
    {

    	HVOBJECT hHandle;

    	// this fucntion will render the background for the personnel screen
    	if( fCurrentTeamMode == true )
    	{
    		// blit title
        hHandle = guiCURRENTTEAM;
  
    	}
    	else
    	{
    			// blit title
        hHandle = guiDEPARTEDTEAM;
    	}

    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,DEPARTED_X, DEPARTED_Y, VO_BLT.SRCTRANSPARENCY,null );

    	return;
    }

    private static bool LoadPersonnelScreenBackgroundGraphics()
    {
    	// will load the graphics for the personeel screen background
	
    	// departed bar
      guiDEPARTEDTEAM = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\departed.sti"));

    	// current bar
      guiCURRENTTEAM = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\CurrentTeam.sti"));
	

    	return ( true );
    }

    private static void DeletePersonnelScreenBackgroundGraphics()
    {
    	// delete background V/O's

    	video.DeleteVideoObject( guiCURRENTTEAM );
    	video.DeleteVideoObject( guiDEPARTEDTEAM );

    }

    // was a function-local static of CreateDestroyButtonsForPersonnelDepartures
    private static bool CreateDestroyButtonsForPersonnelDepartures_fCreated = false;
    private static void CreateDestroyButtonsForPersonnelDepartures()
    {


    	// create/ destroy personnel departures buttons as needed

    	// create button?..if not created
    	if( ( fCreatePeronnelDepartureButton == true)&&( CreateDestroyButtonsForPersonnelDepartures_fCreated == false ) )
    	{
    		CreateDestroyButtonsForPersonnelDepartures_fCreated = true;
    	}
    	else if( ( fCreatePeronnelDepartureButton == false ) && ( CreateDestroyButtonsForPersonnelDepartures_fCreated == true ) )
    	{
    		CreateDestroyButtonsForPersonnelDepartures_fCreated = false;
    	}


    	return;
    }

    private static int GetNumberOfMercsOnPlayersTeam()
    {

    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;	
    	int cnt=0;
    	int iCounter = 0;

	
	
 
      // grab number on team
    	pSoldier = MercPtrs[0];	
	
    	// no soldiers
	
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	{
    		if( ( pTeamSoldier.bActive) && !( pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) && ( pTeamSoldier.bLife > 0 ) )
    			iCounter++;
    	}

    	return ( iCounter );
    }

    private static int GetNumberOfMercsDeadOrAliveOnPlayersTeam()
    {

    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;	
    	int cnt=0;
    	int iCounter = 0;

	
	
 
      // grab number on team
    	pSoldier = MercPtrs[0];	
	
    	// no soldiers
	
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	{
    		if( ( pTeamSoldier.bActive) && !( pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) )
    			iCounter++;
    	}

    	return ( iCounter );
    }

    // was a function-local static of CreateDestroyMouseRegionsForPersonnelPortraits
    private static bool CreateDestroyMouseRegionsForPersonnelPortraits_fCreated = false;
    private static void CreateDestroyMouseRegionsForPersonnelPortraits()
    {
    	// creates/ destroys mouse regions for portraits


    	int sCounter = 0;

    	if( ( CreateDestroyMouseRegionsForPersonnelPortraits_fCreated == false )&&( fCreatePersonnelPortraitMouseRegions == true ) )
    	{
    		// create regions
    		for( sCounter = 0; sCounter < PERSONNEL_PORTRAIT_NUMBER; sCounter++ )
    		{
    			MouseSubSystem.MSYS_DefineRegion( gPortraitMouseRegions[ sCounter ], ( int ) ( SMALL_PORTRAIT_START_X + ( sCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y +  ( sCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT ), ( int ) ( ( SMALL_PORTRAIT_START_X ) + ( ( sCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH )  +  SMALL_PORTRAIT_WIDTH ), ( int )( SMALL_PORTRAIT_START_Y + ( sCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORTRAIT_HEIGHT ), MSYS_PRIORITY.HIGHEST, CURSOR.LAPTOP_SCREEN, MSYS_NO_CALLBACK, PersonnelPortraitCallback );
    			MouseSubSystem.MSYS_SetRegionUserData( gPortraitMouseRegions[ sCounter ], 0, sCounter );
    			MouseSubSystem.MSYS_AddRegion( ref gPortraitMouseRegions[ sCounter ] );
    		}

    		CreateDestroyMouseRegionsForPersonnelPortraits_fCreated = true;

    	}
    	else if( ( CreateDestroyMouseRegionsForPersonnelPortraits_fCreated == true ) && ( fCreatePersonnelPortraitMouseRegions == false ) )
    	{
    		// destroy regions
    		for( sCounter = 0; sCounter < PERSONNEL_PORTRAIT_NUMBER; sCounter++ )
    		{
    		  MouseSubSystem.MSYS_RemoveRegion( gPortraitMouseRegions[ sCounter ] );
    		}

    		CreateDestroyMouseRegionsForPersonnelPortraits_fCreated = false;
    	}
    	return;
    }

    private static bool DisplayPicturesOfCurrentTeam()
    {
    	int iCounter = 0;
    	int iTotalOnTeam = 0;
    	string sTemp = string.Empty;
    	HVOBJECT hFaceHandle;
    	SOLDIERTYPE? pSoldier;
    	int iId = 0;
    	int iCnt = 0;
	
    	// will display the 20 small portraits of the current team

    	// get number of mercs on team
    	iTotalOnTeam = GetNumberOfMercsDeadOrAliveOnPlayersTeam( );


    	if( ( iTotalOnTeam == 0)||( fCurrentTeamMode == false ) )
    	{
    		// nobody on team, leave
    		return ( true );
    	}

    	pSoldier = MercPtrs[ iCounter ];

    	// start id
    	iId= gTacticalStatus.Team[ pSoldier.bTeam ].bFirstID;
  
    	for( iCounter = 0; iCounter < iTotalOnTeam; iCnt++ )
    	{

    		if( ( MercPtrs[iId + iCnt ] . bActive == true ) )
    		{
    			// found the next actual guy
    			if( ( 50 < (int)MercPtrs[iId + iCnt ].ubProfile )&&( 57 > (int)MercPtrs[iId + iCnt ].ubProfile ) )
    			{
    				sTemp = wprintf( "%s%03d.sti", SMALL_FACES_DIR, 	gMercProfiles[(NPCID)(MercPtrs[ iId + iCnt ] . ubProfile)].ubFaceIndex );	
    			}
    			else
    			{
    				if( (int)Menptr[ iId + iCnt ].ubProfile < 100 )
    				{
    					sTemp = wprintf( "%s%02d.sti", SMALL_FACES_DIR,	Menptr[ iId + iCnt ].ubProfile);
    				}
    				else
    				{
    					sTemp = wprintf( "%s%03d.sti", SMALL_FACES_DIR,	Menptr[ iId + iCnt ].ubProfile);
    				}
    			}
	
    		guiFACE = video.GetVideoObject(Utils.FilenameForBPP(sTemp));

    		//Blt face to screen to 
    		hFaceHandle = guiFACE;

    		if( Menptr[ iId + iCnt ].bLife <= 0 )
    		{
    			hFaceHandle.pShades[ 0 ]		= video.Create16BPPPaletteShaded( hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true );
			
    			//set the red pallete to the face
    			guiFACE.ShadeCurrentPixels = guiFACE.pShades[0];
    		}

		
		
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0,( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT ), VO_BLT.SRCTRANSPARENCY);
		
    		if( Menptr[ iId + iCnt ].bLife <= 0 )
    		{
    			//if the merc is dead, display it
    			FontSubSystem.DrawTextToScreen(AimPopUpText[AIM_MEMBER_DEAD], new(( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORT_HEIGHT / 2 )), SMALL_PORTRAIT_WIDTH_NO_BORDERS, FontStyle.FONT10ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}

    		video.DeleteVideoObject(guiFACE);
    		iCounter++;
    		}
    	}

    	return ( true );
    }

    private static void PersonnelPortraitCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	int iPortraitId = 0;
    	int iOldPortraitId;

    	iPortraitId = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
    	iOldPortraitId = iCurrentPersonSelectedId;

    	// callback handler for the minize region that is attatched to the laptop program icon
    	if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
      {
    		// get id of portrait

    		if( fCurrentTeamMode == true )
    		{
    		  // valid portrait, set up id
    			if( iPortraitId >= GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) )
    			{
    			  // not a valid id, leave
    			  return;
    			}

    		  iCurrentPersonSelectedId = iPortraitId;
    			fReDrawScreenFlag = true;
    		}
    		else
    		{
    			if( iPortraitId >= GetNumberOfPastMercsOnPlayersTeam( ) )
    			{
    				return;
    			}
    			iCurrentPersonSelectedId = iPortraitId;
    			fReDrawScreenFlag = true;
    			iCurPortraitId = iPortraitId;
    		}

    		//if the selected merc is valid, and they are a POW, change to the inventory display
    		if( iCurrentPersonSelectedId != -1 && Menptr[ GetIdOfThisSlot( iCurrentPersonSelectedId ) ].bAssignment == Assignment.ASSIGNMENT_POW && gubPersonnelInfoState == PERSONNEL_INV_BTN )
    		{
    			gubPersonnelInfoState = PERSONNEL_STAT_BTN;
    		}

    		if( iOldPortraitId != iPortraitId )
    		{
    			uiCurrentInventoryIndex = 0;
    			guiSliderPosition = 0;
    		}
    	}

    	if( iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP) )
    	{
    		if( fCurrentTeamMode == true )
    		{
    		  // valid portrait, set up id
    			if( iPortraitId >= GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) )
    			{
    			  // not a valid id, leave
    			  return;
    			}

    			//if the user is rigt clicking on the same face
    			if( iCurrentPersonSelectedId == iPortraitId )
    			{
    				//increment the info page when the user right clicks
    				if( gubPersonnelInfoState < PERSONNEL_NUM_BTN-1 )
    					gubPersonnelInfoState++;
    				else
    					gubPersonnelInfoState = PERSONNEL_STAT_BTN;
    			}
			
    			iCurrentPersonSelectedId = iPortraitId;
    			fReDrawScreenFlag = true;

    			uiCurrentInventoryIndex = 0;
    			guiSliderPosition = 0;

    			//if the selected merc is valid, and they are a POW, change to the inventory display
    			if( iCurrentPersonSelectedId != -1 && Menptr[ GetIdOfThisSlot( iCurrentPersonSelectedId ) ].bAssignment == Assignment.ASSIGNMENT_POW && gubPersonnelInfoState == PERSONNEL_INV_BTN)
    			{
    				gubPersonnelInfoState = PERSONNEL_STAT_BTN;
    			}
    		}
    	}
    }

    private static void DisplayFaceOfDisplayedMerc()
    {
    	// valid person?, display
	
    	if( iCurrentPersonSelectedId != -1 )
    	{
    		// highlight it
    		DisplayHighLightBox( );

    		// if showing inventory, leave
		
    		if( fCurrentTeamMode == true )
    		{
    			RenderPersonnelFace(  GetIdOfThisSlot( iCurrentPersonSelectedId ), 0, false, false, false );
    			DisplayCharName(  GetIdOfThisSlot( iCurrentPersonSelectedId ), 0 );

    //			if( fShowInventory == TRUE )
    			if( gubPersonnelInfoState == PRSNL_INV )
    			{
    				return;
    			}

    	    RenderPersonnelStats(  GetIdOfThisSlot( iCurrentPersonSelectedId ), 0 );
		  
    		}
    		else
    		{
    			RenderPersonnelFace(  GetIdOfPastMercInSlot( iCurrentPersonSelectedId ), 0, IsPastMercDead( iCurrentPersonSelectedId ), IsPastMercFired( iCurrentPersonSelectedId ), IsPastMercOther( iCurrentPersonSelectedId ) );
    			DisplayDepartedCharName(  GetIdOfPastMercInSlot( iCurrentPersonSelectedId ), 0, GetTheStateOfDepartedMerc( GetIdOfPastMercInSlot( iCurrentPersonSelectedId  ) ) );

    //			if( fShowInventory == TRUE )
    			if( gubPersonnelInfoState == PRSNL_INV )
    			{
    				return;
    			}

    			DisplayDepartedCharStats( GetIdOfPastMercInSlot( iCurrentPersonSelectedId ), 0, GetTheStateOfDepartedMerc( GetIdOfPastMercInSlot( iCurrentPersonSelectedId  ) ) );

    		}
		

    	}

    	return;
    }

    private static void DisplayInventoryForSelectedChar()
    {

    	// display the inventory for this merc
    //	if( fShowInventory == FALSE )
    	if( gubPersonnelInfoState != PRSNL_INV )
    	{
    		return;
    	}

    	CreateDestroyPersonnelInventoryScrollButtons( );

    	if( fCurrentTeamMode == true )
    	{
    		RenderInventoryForCharacter( GetIdOfThisSlot( iCurrentPersonSelectedId ), 0 );
    	}
    	else
    	{
    		RenderInventoryForCharacter( GetIdOfPastMercInSlot( iCurrentPersonSelectedId ), 0 );
    	}

    	return;
    }

    private static void RenderInventoryForCharacter(int iId, int iSlot)
    {
    	int PosX, PosY, sX, sY;
    	int ubItemCount = 0;
    	int ubUpToCount = 0;
    	string sString;

    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiPersonnelInventory, 0, 397, 200, VO_BLT.SRCTRANSPARENCY);

    	if( fCurrentTeamMode == false )
    	{
    		return;
    	}

    	// render the bar for the character
    	RenderSliderBarForPersonnelInventory( );

    	SOLDIERTYPE pSoldier = Menptr[ iId ];

    	//if this is a robot, dont display any inventory
    	if( AM_A_ROBOT( pSoldier ) )
    	{
    		return;
    	}

    	for( InventorySlot ubCounter = 0; ubCounter < NUM_INV_SLOTS && ubItemCount < NUMBER_OF_INVENTORY_PERSONNEL; ubCounter++ )
    	{
    		PosX = 397 + 3;
    		PosY = 200 + 8 +( ubItemCount * ( 29 ) );

    		// the robot only shows what is in its hand
    		if( pSoldier.ubProfile == NPCID.ROBOT && ubCounter != InventorySlot.HANDPOS )
    		{
    			continue;
    		}

    		OBJECTTYPE pObject = pSoldier.inv[ ubCounter ];
    		if( pObject.ubNumberOfObjects == 0 )
    		{
    			continue;
    		}

    		if( uiCurrentInventoryIndex > ubUpToCount )
    		{
    			ubUpToCount++;
    			continue;
    		}

    		Items sIndex = pObject.usItem;

    		// TODO(port): blit the item's graphic - the item interface graphics are not loaded in the port yet

    		FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    		FontSubSystem.SetFontForeground( FontColor.FONT_WHITE );
    		FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );

    		// item name, shortened if needed
    		sString = ShortItemNames.TryGetValue( sIndex, out string? name ) ? name : string.Empty;
    		if( FontSubSystem.StringPixLength( sString, FontStyle.FONT10ARIAL ) > ( 171 - 75 ) )
    		{
    			WordWrap.ReduceStringLength( ref sString, ( 171 - 75 ), FontStyle.FONT10ARIAL );
    		}

    		mprintf( PosX + 65, PosY + 3, sString );

    		// condition, or the rounds left for ammo
    		if( Item[ sIndex ].usItemClass.HasFlag(IC.AMMO) )
    		{
    			int iTotalAmmo = 0;
    			for( int cnt = 0; cnt < pObject.ubNumberOfObjects; cnt++ )
    			{
    				iTotalAmmo += pObject.ubShotsLeft[ cnt ];
    			}

    			sString = wprintf( "%d", iTotalAmmo );
    			FontSubSystem.FindFontRightCoordinates( PosX + 65, PosY + 15, 171 - 75, FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ), sString, FontStyle.FONT10ARIAL, out sX, out sY );
    		}
    		else
    		{
    			sString = wprintf( "%2d%%", pObject.bStatus[ 0 ] );
    			FontSubSystem.FindFontRightCoordinates( PosX + 65, PosY + 15, 171 - 75, FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ), sString, FontStyle.FONT10ARIAL, out sX, out sY );
    		}

    		mprintf( sX, sY, sString );

    		// more than one?
    		if( pObject.ubNumberOfObjects > 1 )
    		{
    			sString = wprintf( "x%d", pObject.ubNumberOfObjects );
    			FontSubSystem.FindFontRightCoordinates( PosX, PosY + 15, 58, FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ), sString, FontStyle.FONT10ARIAL, out sX, out sY );
    			mprintf( sX, sY, sString );
    		}

    		ubItemCount++;
    	}
    }

    private static void InventoryUpButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int iValue = 0;
    	SOLDIERTYPE? pSoldier = MercPtrs[ 0 ];
    	int cnt = 0;
    	int iId = 0;

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;


    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if( (btn.uiFlags & (ButtonFlags.BUTTON_CLICKED_ON)) != 0 )  // TODO(port): bit test: check flag type
    		{
    			btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);

    			if( uiCurrentInventoryIndex == 0 )
    			{
    				return;
    			}
			
    			// up one element
    			uiCurrentInventoryIndex--;
    			fReDrawScreenFlag = true;

    			FindPositionOfPersInvSlider( );
    		}
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT) )
    	{
    		if( uiCurrentInventoryIndex == 0 )
    		{
    			return;
    		}

    		// up one element
    		uiCurrentInventoryIndex--;
    		fReDrawScreenFlag = true;
    		FindPositionOfPersInvSlider( );
    	}
    }

    private static void InventoryDownButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int iValue = 0;
    	SOLDIERTYPE? pSoldier = MercPtrs[ 0 ];
    	int cnt = 0;
    	int iId = 0;

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;


    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT) )
    	{
    		if( ( int )uiCurrentInventoryIndex >= ( int )( GetNumberOfInventoryItemsOnCurrentMerc( ) - NUMBER_OF_INVENTORY_PERSONNEL ) )
    		{
    			return;
    		}

    		// up one element
    		uiCurrentInventoryIndex++;
    		fReDrawScreenFlag = true;
    		FindPositionOfPersInvSlider( );

    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if( (btn.uiFlags & (ButtonFlags.BUTTON_CLICKED_ON)) != 0 )  // TODO(port): bit test: check flag type
    		{
    			btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);

    			if( ( int )uiCurrentInventoryIndex >= ( int )( GetNumberOfInventoryItemsOnCurrentMerc( ) - NUMBER_OF_INVENTORY_PERSONNEL ) )
    			{
    				return;
    			}
			
    			// up one element
    			uiCurrentInventoryIndex++;
    			fReDrawScreenFlag = true;

    			FindPositionOfPersInvSlider( );

    		}
    	}
	
    }

    private static void EnableDisableInventoryScrollButtons()
    {
    //	if( fShowInventory == FALSE )
    	if( gubPersonnelInfoState != PRSNL_INV )
    	{
    		return;
    	}

    	if( uiCurrentInventoryIndex == 0 )
    	{
    		giPersonnelInventoryButtons[ 0 ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.DisableButton( giPersonnelInventoryButtons[ 0 ] );	
    	}
    	else
    	{
    		ButtonSubSystem.EnableButton( giPersonnelInventoryButtons[ 0 ] );
    	}
	
	
    	if( ( int )uiCurrentInventoryIndex >= ( int )( GetNumberOfInventoryItemsOnCurrentMerc( ) - NUMBER_OF_INVENTORY_PERSONNEL ) )
    	{
    		giPersonnelInventoryButtons[ 1 ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.DisableButton( giPersonnelInventoryButtons[ 1 ] );
    	}
    	else
    	{
    		ButtonSubSystem.EnableButton( giPersonnelInventoryButtons[ 1 ] );
    	}


    	return;
    }

    private static int GetNumberOfInventoryItemsOnCurrentMerc()
    {
    	int iId = 0;
    	InventorySlot ubCounter = 0;
    	int ubCount = 0;
    	SOLDIERTYPE? pSoldier;

    	// in current team mode?..nope...move on
    	if( fCurrentTeamMode == false )
    	{
    		return( 0 );
    	}

    	iId = GetIdOfThisSlot( iCurrentPersonSelectedId );

    	pSoldier = Menptr[ iId ];  // TODO(port): address-of

    	for( ubCounter = 0; ubCounter < NUM_INV_SLOTS; ubCounter++ )
    	{
    		if( ( pSoldier.inv[ ubCounter ].ubNumberOfObjects != 0 ) && ( pSoldier.inv[ ubCounter ].usItem != Items.NONE ) )
    		{
    			ubCount++;
    		}
    	}

    	return( ubCount );
    }

    // was a function-local static of CreateDestroyPersonnelInventoryScrollButtons
    private static bool CreateDestroyPersonnelInventoryScrollButtons_fCreated = false;
    private static void CreateDestroyPersonnelInventoryScrollButtons()
    {


    //	if( ( fShowInventory == TRUE ) && ( CreateDestroyPersonnelInventoryScrollButtons_fCreated == FALSE ) )
    	if( ( gubPersonnelInfoState == PRSNL_INV ) && ( CreateDestroyPersonnelInventoryScrollButtons_fCreated == false ) )
    	{
    		// create buttons
    		giPersonnelInventoryButtonsImages[ 0 ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\personnel_inventory.sti" ,-1,1,-1,2,-1 );
    	  giPersonnelInventoryButtons[ 0 ] = ButtonSubSystem.QuickCreateButton(giPersonnelInventoryButtonsImages[0], new Point(176 + 397, 2 + 200), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)InventoryUpButtonCallback);

    		giPersonnelInventoryButtonsImages[ 1 ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\personnel_inventory.sti" ,-1,3,-1,4,-1 );
    	  giPersonnelInventoryButtons[ 1 ] = ButtonSubSystem.QuickCreateButton(giPersonnelInventoryButtonsImages[1], new Point(397 + 176, 200 + 223), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)InventoryDownButtonCallback);

    			// set up cursors for these buttons
    		ButtonSubSystem.SetButtonCursor( giPersonnelInventoryButtons[ 0 ], CURSOR.LAPTOP_SCREEN);
    	  ButtonSubSystem.SetButtonCursor( giPersonnelInventoryButtons[ 1 ], CURSOR.LAPTOP_SCREEN);

    		MouseSubSystem.MSYS_DefineRegion( gMouseScrollPersonnelINV, X_OF_PERSONNEL_SCROLL_REGION, Y_OF_PERSONNEL_SCROLL_REGION, X_OF_PERSONNEL_SCROLL_REGION + X_SIZE_OF_PERSONNEL_SCROLL_REGION, Y_OF_PERSONNEL_SCROLL_REGION + Y_SIZE_OF_PERSONNEL_SCROLL_REGION, MSYS_PRIORITY.HIGHEST - 3, CURSOR.LAPTOP_SCREEN, MSYS_NO_CALLBACK, HandleSliderBarClickCallback );

    		CreateDestroyPersonnelInventoryScrollButtons_fCreated = true;
    	}
    //	else if( ( CreateDestroyPersonnelInventoryScrollButtons_fCreated == TRUE ) && ( fShowInventory == FALSE ) )
    	else if( ( CreateDestroyPersonnelInventoryScrollButtons_fCreated == true ) && ( gubPersonnelInfoState != PERSONNEL_INV_BTN ) )
    	{
    		// destroy buttons
    		ButtonSubSystem.RemoveButton(giPersonnelInventoryButtons[0] );
    	  ButtonSubSystem.UnloadButtonImage(giPersonnelInventoryButtonsImages[0] );
    	  ButtonSubSystem.RemoveButton(giPersonnelInventoryButtons[1] );
    	  ButtonSubSystem.UnloadButtonImage(giPersonnelInventoryButtonsImages[1] );

    		MouseSubSystem.MSYS_RemoveRegion( gMouseScrollPersonnelINV );

    		CreateDestroyPersonnelInventoryScrollButtons_fCreated = false;
    	}
    }

    private static void DisplayNumberOnCurrentTeam()
    {
    	// display number on team
    	string sString = string.Empty;
    	int sX = 0, sY = 0;


    	// font stuff
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	if( fCurrentTeamMode == true )
    	{
    		sString = wprintf( "%s ( %d )", pPersonelTeamStrings[ 0 ], GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) );
    		sX = PERS_CURR_TEAM_X;
    	}
    	else
    	{
    		sString = wprintf( "%s", pPersonelTeamStrings[ 0 ] );
    		FontSubSystem.FindFontCenterCoordinates( PERS_CURR_TEAM_X, 0, 65, 0, sString, FontStyle.FONT10ARIAL, out sX, out sY );

    	}

    	mprintf( sX, PERS_CURR_TEAM_Y, sString );
	
    	// now the cost of the current team, if applicable
    	DisplayCostOfCurrentTeam( );

    	return;
    }

    private static void DisplayNumberDeparted()
    {
    	// display number departed from team
    	string sString = string.Empty;
    	int sX = 0, sY = 0;


    	// font stuff
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	if( fCurrentTeamMode == false )
    	{
    		sString = wprintf( "%s ( %d )", pPersonelTeamStrings[ 1 ], GetNumberOfPastMercsOnPlayersTeam( ) );
    		sX = PERS_CURR_TEAM_X;
    	}
    	else
    	{
    		sString = wprintf( "%s", pPersonelTeamStrings[ 1 ] );
    		FontSubSystem.FindFontCenterCoordinates( PERS_CURR_TEAM_X, 0, 65, 0, sString, FontStyle.FONT10ARIAL, out sX, out sY );
    	}

    	mprintf( sX, PERS_DEPART_TEAM_Y, sString );

	
    	return;
    }

    private static int GetTotalDailyCostOfCurrentTeam()
    {
    	// will return the total daily cost of the current team

    	SOLDIERTYPE? pSoldier;	
    	int cnt=0;
    	int iCounter = 0;
    	int iCostOfTeam = 0;


    	 // first grunt
    	pSoldier = MercPtrs[0];	

    	// not active?..return cost of zero

    	// run through active soldiers
    	for ( pSoldier = MercPtrs[ 0 ]; cnt <= gTacticalStatus.Team[ OUR_TEAM ].bLastID; cnt++ )
    	{
    		pSoldier = MercPtrs[cnt];	

    		 if( ( pSoldier.bActive) && ( pSoldier.bLife > 0 ) )
    		 {
			 
    			 // valid soldier, get cost
    			 if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    			 {
    				 // daily rate
    				 if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_2_WEEK )
    				 {
    					 // 2 week contract
    				   iCostOfTeam += gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiBiWeeklySalary / 14;
    				 }
    				 else if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_1_WEEK )
    				 {
    					 // 1 week contract
    				   iCostOfTeam += gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiWeeklySalary / 7 ;
    				 }
    				 else
    				 {

    					 iCostOfTeam += gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    				 }
    			 }
    			 else if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    			 {
    				 // MERC Merc
    				 iCostOfTeam += gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    			 }
    			 else
    			 {
    					// no cost
    				 iCostOfTeam += 0;
    			 }
    		}
    	}
    	return iCostOfTeam;
    }

    private static int GetLowestDailyCostOfCurrentTeam()
    {
    	// will return the lowest daily cost of the current team

    	SOLDIERTYPE? pSoldier;	
    	int cnt=0;
    	int iCounter = 0;
    	int iLowest = 999999;
    //	INT32 iId =0;
    	int iCost = 0;

    	 // first grunt
    	pSoldier = MercPtrs[0];	

    	// not active?..return cost of zero

    	// run through active soldiers
    	for ( pSoldier = MercPtrs[ 0 ]; cnt <= gTacticalStatus.Team[ OUR_TEAM ].bLastID; cnt++ )
    	{
    		pSoldier = MercPtrs[cnt];

    		if( ( pSoldier.bActive ) && !( pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) && ( pSoldier.bLife > 0 ) )
    		{
    			 // valid soldier, get cost
    			 if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    			 {
    				 // daily rate
    				 if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_2_WEEK )
    				 {
    					 // 2 week contract
    				   iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiBiWeeklySalary / 14;
    				 }
    				 else if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_1_WEEK )
    				 {
    					 // 1 week contract
    				   iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiWeeklySalary / 7 ;
    				 }
    				 else
    				 {

    					 iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    				 }
    			 }
    			 else if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    			 {
    				 // MERC Merc
    				 iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    			 }
    			 else
    			 {
    					// no cost
    				 iCost = 0;
    			 }

    			 if( iCost <= iLowest )
    			 {
    				 iLowest = iCost;
    			 }
    		 }
		 
    		}

    	// if no mercs, send 0
    	if( iLowest == 999999 )
    	{
    		iLowest = 0;
    	}


    	return iLowest;
    }

    private static int GetHighestDailyCostOfCurrentTeam()
    {
    	// will return the lowest daily cost of the current team

    	SOLDIERTYPE? pSoldier;	
    	int cnt=0;
    	int iCounter = 0;
    	int iHighest = 0;
    //	INT32 iId =0;
    	int iCost = 0;

    	 // first grunt
    	pSoldier = MercPtrs[0];	

    	// not active?..return cost of zero

    	// run through active soldiers
    	for ( pSoldier = MercPtrs[0]; cnt <= gTacticalStatus.Team[ OUR_TEAM ].bLastID; cnt++)
    	{
    		pSoldier = MercPtrs[cnt];

    		 if( ( pSoldier.bActive) && !( pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) && ( pSoldier.bLife > 0 ) )
    		 {
			 
    			 // valid soldier, get cost
    			 if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    			 {
    				 // daily rate
    				 if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_2_WEEK )
    				 {
    					 // 2 week contract
    				   iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiBiWeeklySalary / 14;
    				 }
    				 else if( pSoldier.bTypeOfLastContract == CONTRACT_EXTEND_1_WEEK  )
    				 {
    					 // 1 week contract
    				   iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].uiWeeklySalary / 7 ;
    				 }
    				 else
    				 {

    					 iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    				 }
    			 }
    			 else if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    			 {
    				 // MERC Merc
    				 iCost = gMercProfiles[(NPCID)(pSoldier.ubProfile)].sSalary;
    			 }
    			 else
    			 {
    					// no cost
    				 iCost = 0;
    			 }

			 
    			 if( iCost >= iHighest )
    			 {
    				 iHighest = iCost;
    			 }
    		 }
		 
    		}
    	return iHighest;
    }

    private static void DisplayCostOfCurrentTeam()
    {
    	// display number on team
    	string sString = string.Empty;
    	int sX, sY;
	
	
    	// font stuff
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	if( fCurrentTeamMode == true )
    	{

    		// daily cost
    		mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_COST_Y, pPersonelTeamStrings[ 2 ] );

    	  sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( GetTotalDailyCostOfCurrentTeam( )  ) );

    		FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_CURR_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );

    		mprintf( sX ,PERS_CURR_TEAM_COST_Y, sString );

    		// highest cost
    		mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_HIGHEST_Y, pPersonelTeamStrings[ 3 ] );

    	  sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( GetHighestDailyCostOfCurrentTeam( )  ) );

    		FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_CURR_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );

    		mprintf( sX ,PERS_CURR_TEAM_HIGHEST_Y, sString );

    		// the lowest cost
    		mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_LOWEST_Y, pPersonelTeamStrings[ 4 ] );

    	  sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( GetLowestDailyCostOfCurrentTeam( )  ) );

    		FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_CURR_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );

    		mprintf( sX ,PERS_CURR_TEAM_LOWEST_Y, sString );


    	}
    	else
    	{
    		// do nothing
    		return;
    	}
    }

    private static int GetIdOfDepartedMercWithHighestStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	int iId = -1;
    	int iValue =0;
    	MERCPROFILESTRUCT? pTeamSoldier;
    	int cnt=0;
    	int iCounter =0;
    	SOLDIERTYPE? pSoldier;


    	// run through active soldiers
    //	while( fNotDone )
    	foreach (int pastMercId in PastMercIds())
    	{
    		cnt = pastMercId;
    		pTeamSoldier = gMercProfiles[(NPCID)(cnt)];

    		switch( iStat )
    		{
    			case 0:
    			// health

    				//if the soldier is a pow, dont use the health cause it aint known 
    			pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID((NPCID) (int)cnt, false );
    			if( pSoldier is not null && pSoldier.bAssignment == Assignment.ASSIGNMENT_POW )
    			{
    				continue;
    			}

    			if( pTeamSoldier .  bLife >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bLife;
    			}
    			break;
    		  case 1:
    		  // agility
    			if( pTeamSoldier .  bAgility >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bAgility;
    			}
    			break;
    			case 2:
    			// dexterity
    			if( pTeamSoldier .  bDexterity >= iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bDexterity;
    			}
    			break;
    			case 3:
    			// strength
          if(  pTeamSoldier . bStrength >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bStrength;
    			}	 
    			break;
    			case 4:
    			// leadership
          if(  pTeamSoldier . bLeadership >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bLeadership;
    			}	 
    			break;
    			case 5:
    		  // wisdom
    			if(  pTeamSoldier . bWisdom >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bWisdom;
    			}
    			break;
    			case 6:
    			// exper
          if( pTeamSoldier . bExpLevel >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bExpLevel;
    			}  

    			break;
    			case 7:
    			//mrkmanship
    			if(  pTeamSoldier . bMarksmanship >= iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bMarksmanship;
    			}
       			 
    		  break;
    			case 8:
    			// mech
    			if(  pTeamSoldier . bMechanical >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bMechanical;
    			}
    		  break;
    			case 9:
    			// exp
    			if(pTeamSoldier . bExplosive >= iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bExplosive;
    			}
    			break;
    			case 10:
    			// med
    			if(  pTeamSoldier . bMedical >= iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bMedical;
    			}
    			break;
    		}
    	}	


    	return( iId );
    }

    private static int GetIdOfDepartedMercWithLowestStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	int iId = -1;
    	int iValue =9999999;
    	MERCPROFILESTRUCT? pTeamSoldier;
    	int cnt=0;
    	int iCounter =0;
    	SOLDIERTYPE? pSoldier;

    	// run through active soldiers
    //	while( fNotDone )
    	foreach (int pastMercId in PastMercIds())
    	{
    		cnt = pastMercId;
    		pTeamSoldier = gMercProfiles[(NPCID)(cnt)];


    		switch( iStat )
    		{
    			case 0:
    			// health

    			pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID((NPCID) (int)cnt, false );
    			if( pSoldier is not null && pSoldier.bAssignment == Assignment.ASSIGNMENT_POW )
    			{
    				continue;
    			}

    			if( pTeamSoldier .  bLife < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bLife;
    			}
    			break;
    		  case 1:
    		  // agility
    			if( pTeamSoldier .  bAgility < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bAgility;
    			}
    			break;
    			case 2:
    			// dexterity
    			if( pTeamSoldier .  bDexterity < iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bDexterity;
    			}
    			break;
    			case 3:
    			// strength
          if(  pTeamSoldier . bStrength < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bStrength;
    			}	 
    			break;
    			case 4:
    			// leadership
          if(  pTeamSoldier . bLeadership < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bLeadership;
    			}	 
    			break;
    			case 5:
    		  // wisdom
    			if(  pTeamSoldier . bWisdom < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bWisdom;
    			}
    			break;
    			case 6:
    			// exper
          if( pTeamSoldier . bExpLevel < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bExpLevel;
    			}  

    			break;
    			case 7:
    			//mrkmanship
    			if(  pTeamSoldier . bMarksmanship < iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bMarksmanship;
    			}
       			 
    		  break;
    			case 8:
    			// mech
    			if(  pTeamSoldier . bMechanical < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bMechanical;
    			}
    		  break;
    			case 9:
    			// exp
    			if(pTeamSoldier . bExplosive < iValue )
    			{
    				iId = cnt;
    				iValue = pTeamSoldier . bExplosive;
    			}
    			break;
    			case 10:
    			// med
    			if(  pTeamSoldier . bMedical < iValue )
    			{
    					iId = cnt;
    					iValue = pTeamSoldier . bMedical;
    			}
    			break;
    		}
    	}	


    	return( iId );
    }

    private static int GetIdOfMercWithHighestStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	int iId = -1;
    	int iValue =0;
    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;	
    	int cnt=0;

    	 // first grunt
    	pSoldier = MercPtrs[0];	


    	// run through active soldiers
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	{
    		 if( ( pTeamSoldier.bActive) && !( pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) && ( pTeamSoldier.bLife > 0 ) && !AM_A_ROBOT( pTeamSoldier ) )
    		 {
    			 switch( iStat )
    			 {
    				 case 0:
    			   // health
    						if( pTeamSoldier.bAssignment == Assignment.ASSIGNMENT_POW )
    						{
    							continue;
    						}

    					 if( pTeamSoldier .  bLifeMax >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bLifeMax;
    					 }
    				   break;
    				 case 1:
    					 // agility
    					 if( pTeamSoldier .  bAgility >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bAgility;
    					 }
    					 break;
    				 case 2:
    					// dexterity
    					 if( pTeamSoldier .  bDexterity >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bDexterity;
    					 }
    					break;
    				 case 3:
    				 // strength
           		 if(  pTeamSoldier . bStrength >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bStrength;
    					 }	 
    				 break;
    				 case 4:
    					// leadership
           		 if(  pTeamSoldier . bLeadership >= iValue )
    					 {
    						 iId = cnt;
    						  iValue = pTeamSoldier . bLeadership;
    					 }	 
    				 break;
    				 case 5:
    					// wisdom
    					 if(  pTeamSoldier . bWisdom >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bWisdom;
    					 }
    				 break;
    				 case 6:
    					// exper
               if( pTeamSoldier . bExpLevel >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bExpLevel;
    					 }  

    				 break;
    				 case 7:
    					 //mrkmanship
    					 if(  pTeamSoldier . bMarksmanship >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMarksmanship;
    					 }
       			 
    				 break;
    				 case 8:
    					// mech
    					 if(  pTeamSoldier . bMechanical >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMechanical;
    					 }
    				 break;
    				 case 9:
    					// exp
    					 if(pTeamSoldier . bExplosive >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bExplosive;
    					 }
    				 break;
    				 case 10:
    					 // med
    					 if(  pTeamSoldier . bMedical >= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMedical;
    					 }
    				 break;
    				 }
    		 }
    	}	


    	return( iId );
    }

    private static int GetIdOfMercWithLowestStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	int iId = -1;
    	int iValue =999999;
    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;	
    	int cnt=0;

    	 // first grunt
    	pSoldier = MercPtrs[0];	


    	// run through active soldiers
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	{
    		 if(( pTeamSoldier.bActive) && !( pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE) ) && ( pTeamSoldier.bLife > 0 ) && !AM_A_ROBOT( pTeamSoldier ) )
    		 {

    			 switch( iStat )
    			 {
    				 case 0:
    			   // health

    					if( pTeamSoldier.bAssignment == Assignment.ASSIGNMENT_POW )
    					{
    						continue;
    					}

    					 if( pTeamSoldier . bLifeMax <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bLifeMax;
    					 }
    				   break;
    				 case 1:
    					 // agility
    					 if( pTeamSoldier . bAgility <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bAgility;
    					 }
    					 break;
    				 case 2:
    					// dexterity
    					 if(  pTeamSoldier . bDexterity <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bDexterity;
    					 }
    					break;
    				 case 3:
    				 // strength
           		 if(  pTeamSoldier . bStrength <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bStrength;
    					 }	 
    				 break;
    				 case 4:
    					// leadership
           		 if( pTeamSoldier . bLeadership <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bLeadership;
    					 }	 
    				 break;
    				 case 5:
    					// wisdom
    					 if( pTeamSoldier . bWisdom <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bWisdom;
    					 }
    				 break;
    				 case 6:
    					// exper
               if(  pTeamSoldier . bExpLevel <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bExpLevel;
    					 }  

    				 break;
    				 case 7:
    					 //mrkmanship
    					 if(  pTeamSoldier . bMarksmanship <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMarksmanship;
    					 }
       			 
    				 break;
    				 case 8:
    					// mech
    					 if(  pTeamSoldier . bMechanical <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMechanical;
    					 }
    				 break;
    				 case 9:
    					// exp
    					 if(  pTeamSoldier . bExplosive <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bExplosive;
    					 }
    				 break;
    				 case 10:
    					 // med
    					 if(  pTeamSoldier . bMedical <= iValue )
    					 {
    						 iId = cnt;
    						 iValue = pTeamSoldier . bMedical;
    					 }
    				 break;
    				 }
    		 }
    	}	


    	return( iId );
    }

    private static int GetAvgStatOfCurrentTeamStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	SOLDIERTYPE? pTeamSoldier = null, pSoldier = null;	
    	int cnt=0;
    	int iTotalStatValue = 0;
    	int	bNumberOfPows = 0;
    	int	ubNumberOfMercsInCalculation = 0;


    	 // first grunt
    	pSoldier = MercPtrs[0];	

    	// run through active soldiers
    	for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pTeamSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    	{
    		 if(( pTeamSoldier.bActive)&&( pTeamSoldier.bLife > 0 ) && !AM_A_ROBOT( pTeamSoldier ) )
    		 {
    			 switch( iStat )
    			 {
    				 case 0:
    			   // health

    						//if this is a pow, dont count his stats
    						if( pTeamSoldier.bAssignment == Assignment.ASSIGNMENT_POW )
    						{
    							bNumberOfPows++;
    							continue;
    						}

    					  iTotalStatValue += pTeamSoldier . bLifeMax;
					 
    				   break;
    				 case 1:
    					 // agility
    					 iTotalStatValue +=pTeamSoldier . bAgility;

    					 break;
    				 case 2:
    					// dexterity
    					 iTotalStatValue +=  pTeamSoldier . bDexterity;

    					break;
    				 case 3:
    				 // strength
    				 iTotalStatValue +=  pTeamSoldier . bStrength;

    				 break;
    				 case 4:
    					// leadership
    					 iTotalStatValue +=  pTeamSoldier . bLeadership;

    				 break;
    				 case 5:
    					// wisdom
					
    					 iTotalStatValue += pTeamSoldier . bWisdom;
    				 break;
    				 case 6:
    					// exper
           
    					 iTotalStatValue +=  pTeamSoldier . bExpLevel;

    				 break;
    				 case 7:
    					 //mrkmanship
					 
    					 iTotalStatValue +=  pTeamSoldier . bMarksmanship;
       			 
    				 break;
    				 case 8:
    					// mech
					 
    					 iTotalStatValue +=  pTeamSoldier . bMechanical;
    				 break;
    				 case 9:
    					// exp
					 
    					 iTotalStatValue +=  pTeamSoldier .bExplosive;
    				 break;
    				 case 10:
    					 // med
					 
    					 iTotalStatValue +=  pTeamSoldier . bMedical;
    				 break;
    			 }

    			 ubNumberOfMercsInCalculation++;
    		 }
    	}	


    	//if the stat is health, and there are only pow's
    	if( GetNumberOfMercsOnPlayersTeam( ) != 0 && GetNumberOfMercsOnPlayersTeam( ) == bNumberOfPows && iStat == 0 )
    	{
    		return( - 1 );
    	}
    	else if( ( ubNumberOfMercsInCalculation - bNumberOfPows ) > 0 )
    	{
    		return( iTotalStatValue / ( ubNumberOfMercsInCalculation - bNumberOfPows ) );
    	}
    	else
    	{
    		return( 0 );
    	}
    }

    private static int GetAvgStatOfPastTeamStat(int iStat)
    {
    	// will return the id value of the merc on the players team with highest in this stat
      // -1 means error
    	int cnt=0;
    	int iTotalStatValue = 0;
    	int iId = -1;
    	MERCPROFILESTRUCT? pTeamSoldier;
    	int iCounter =0;

    	// run through active soldiers
	
    	//while( fNotDone )
    	foreach (int pastMercId in PastMercIds())
    	{
    		cnt = pastMercId;
    		pTeamSoldier = gMercProfiles[(NPCID)(cnt)];

    		switch( iStat )
    		{
    			case 0:
    			// health
			
    				iTotalStatValue += pTeamSoldier . bLife;
			
    			break;
    		  case 1:
    		  // agility
			
    				iTotalStatValue += pTeamSoldier . bAgility;
			
    			break;
    			case 2:
    			// dexterity
			
    					iTotalStatValue += pTeamSoldier . bDexterity;
			
    			break;
    			case 3:
    			// strength
      
    				iTotalStatValue += pTeamSoldier . bStrength;
			 
    			break;
    			case 4:
    			// leadership
      
    				iTotalStatValue += pTeamSoldier . bLeadership;
				 
    			break;
    			case 5:
    		  // wisdom
			
    				iTotalStatValue += pTeamSoldier . bWisdom;
			
    			break;
    			case 6:
    			// exper
     
    				iTotalStatValue += pTeamSoldier . bExpLevel;
		 

    			break;
    			case 7:
    			//mrkmanship
			
    					iId = cnt;
    					iTotalStatValue += pTeamSoldier . bMarksmanship;
			
       			 
    		  break;
    			case 8:
    			// mech
			
    				iTotalStatValue += pTeamSoldier . bMechanical;
			
    		  break;
    			case 9:
    			// exp
			
    				iTotalStatValue += pTeamSoldier . bExplosive;
			
    			break;
    			case 10:
    			// med
			
    				iTotalStatValue += pTeamSoldier . bMedical;
    			break;
    		}
    	}	

    	if( GetNumberOfPastMercsOnPlayersTeam( ) > 0 )
    	{
    	  return( iTotalStatValue / GetNumberOfPastMercsOnPlayersTeam( ) );
    	}
    	else
    	{
    		return( 0 );
    	}
    }

    private static void DisplayAverageStatValuesForCurrentTeam()
    {
    	// will display the average values for stats for the current team
    	int sX, sY;
    	int iCounter = 0;
    	string sString = string.Empty;

    	// set up font
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	// display header

    	// center
    	FontSubSystem.FindFontCenterCoordinates( PERS_STAT_AVG_X, 0, PERS_STAT_AVG_WIDTH, 0, pPersonnelCurrentTeamStatsStrings[ 1 ], FontStyle.FONT10ARIAL, out sX, out sY );

    	mprintf( sX, PERS_STAT_AVG_Y, pPersonnelCurrentTeamStatsStrings[ 1 ] );
	
    	// nobody on team leave
      if( ( GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) == 0 ) && (  fCurrentTeamMode == true ) )
    	{
    		return;
    	}

    	// check if in past team and nobody on past team
    	if( ( GetNumberOfPastMercsOnPlayersTeam( ) == 0 ) && (  fCurrentTeamMode == false ) )
    	{
    		return;
    	}


    	for( iCounter = 0; iCounter < 11; iCounter++ )
    	{

    		// even or odd?..color black or yellow?
    		if( iCounter % 2 == 0 )
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_ALTERNATE_COLOR );
    		}
    		else
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );
    		}

    		if(  fCurrentTeamMode == true )
    		{
    			int	iValue = GetAvgStatOfCurrentTeamStat( iCounter );

    			//if there are no values
    			if( iValue == -1 )
    				sString = wprintf( "%s", pPOWStrings[ 1 ] );
    			else
    				sString = wprintf( "%d", iValue );

    		}
    		else
    		{
    			sString = wprintf( "%d", GetAvgStatOfPastTeamStat( iCounter ) );
    		}
    		// center
    	  FontSubSystem.FindFontCenterCoordinates( PERS_STAT_AVG_X, 0, PERS_STAT_AVG_WIDTH, 0, sString, FontStyle.FONT10ARIAL, out sX, out sY );

    	  mprintf( sX, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), sString );
    	}

    	return;
    }

    private static void DisplayLowestStatValuesForCurrentTeam()
    {
    	// will display the average values for stats for the current team
    	int sX, sY;
    	int iCounter = 0;
    	string sString = string.Empty;
    	int iStat = 0;
    	int iDepartedId=0;
    	int	iId = 0;

    	// set up font
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	// display header

    	// center
    	FontSubSystem.FindFontCenterCoordinates( PERS_STAT_LOWEST_X, 0, PERS_STAT_LOWEST_WIDTH, 0, pPersonnelCurrentTeamStatsStrings[ 0 ], FontStyle.FONT10ARIAL, out sX, out sY );

    	mprintf( sX, PERS_STAT_AVG_Y, pPersonnelCurrentTeamStatsStrings[ 0 ] );

    	// nobody on team leave
      if( ( GetNumberOfMercsOnPlayersTeam( ) == 0 ) && ( fCurrentTeamMode == true) )
    	{
    		return;
    	}

    	if( ( GetNumberOfPastMercsOnPlayersTeam( ) == 0 ) && ( fCurrentTeamMode == false ) )
    	{
    		return;
    	}

    	for( iCounter = 0; iCounter < 11; iCounter++ )
    	{
    		if( fCurrentTeamMode == true )
    		{
    			iId = GetIdOfMercWithLowestStat( iCounter );
    //			if( iId == -1 )
    //				continue;
    		}
    		else
    		{
    			iDepartedId = GetIdOfDepartedMercWithLowestStat( iCounter );
    			if( iDepartedId == -1 )
    				continue;
    		}

    			// even or odd?..color black or yellow?
    		if( iCounter % 2 == 0 )
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_ALTERNATE_COLOR );
    		}
    		else
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );
    		}

    		if( fCurrentTeamMode == true )
    		{
    			// get name
    			if( iId == -1 )
    				sString = wprintf( "%s", pPOWStrings[1] );
    			else
    				sString = wprintf( "%s", MercPtrs[ iId ] . name );
    		}
    		else
    		{
    			// get name
    			sString = wprintf( "%s", gMercProfiles[(NPCID)(iDepartedId)].zNickname );
    		}
    		// print name
    		mprintf( PERS_STAT_LOWEST_X, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), sString );
		
    		switch( iCounter )
    		{
    				 case 0:
    					 // health
    					 if( fCurrentTeamMode == true )
    					 {
    						 if( iId == -1 )
    							 iStat = -1;
    						 else
    							iStat = MercPtrs[ iId ] . bLifeMax;
    					 }
    					 else
    					 {
    						 iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bLife;
    					 }
    					 break;
    					 case 1:
    						 // agility
    						 if( fCurrentTeamMode == true )
    						 {
    								iStat = MercPtrs[ iId ] . bAgility;
    						 }
    						 else
    						 {
    							 	iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bAgility;
    						 }

    						 break;
    					 case 2:
    						// dexterity
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bDexterity;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bDexterity;
    						}

    						break;
    					 case 3:
    					 // strength
    					 if( fCurrentTeamMode == true )
    					 {
    					   iStat = MercPtrs[ iId ] . bStrength;
    					 }
    					 else
    					 {
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bStrength;
    					 }

    					 break;
    					 case 4:
    						// leadership
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bLeadership;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bLeadership;
    						}
    					 break;
    					 case 5:
    						// wisdom
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bWisdom;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bWisdom;
    						}
    					 break;
    					 case 6:
    						// exper
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bExpLevel;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bExpLevel;
    						}
    					 break;
    					 case 7:
    						 //mrkmanship
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMarksmanship;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bMarksmanship;
    						}
    					 break;
    					 case 8:
    						// mech
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMechanical;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bMechanical;
    						}
    					 break;
    					 case 9:
    						// exp
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bExplosive;
    						}
    						else
    						{
    							iStat = gMercProfiles[(NPCID)(iDepartedId)] . bExplosive;
    						}
    					 break;
    					 case 10:
    						 // med
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMedical;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iDepartedId)] . bMedical;
    						}
    					 break;
    			}

    			if( iStat == -1 )
    				sString = wprintf( "%s", pPOWStrings[1] );
    			else
    				sString = wprintf( "%d", iStat );

    		// right justify
    	  FontSubSystem.FindFontRightCoordinates( PERS_STAT_LOWEST_X, 0, PERS_STAT_LOWEST_WIDTH, 0, sString, FontStyle.FONT10ARIAL, out sX, out sY );

    	  mprintf( sX, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), sString );
    	}

    	return;
    }

    private static void DisplayHighestStatValuesForCurrentTeam()
    {
    	// will display the average values for stats for the current team
    	int sX, sY;
    	int iCounter = 0;
    	string sString = string.Empty;
    	int iStat = 0;
    	int iId=0;

    	// set up font
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	// display header

    	// center
    	FontSubSystem.FindFontCenterCoordinates( PERS_STAT_HIGHEST_X, 0, PERS_STAT_LOWEST_WIDTH, 0, pPersonnelCurrentTeamStatsStrings[ 2 ], FontStyle.FONT10ARIAL, out sX, out sY );

    	mprintf( sX, PERS_STAT_AVG_Y, pPersonnelCurrentTeamStatsStrings[ 2 ] );

    	// nobody on team leave
      if( ( GetNumberOfMercsOnPlayersTeam( ) == 0 ) && ( fCurrentTeamMode == true) )
    	{
    		return;
    	}

    	if( ( GetNumberOfPastMercsOnPlayersTeam( ) == 0 ) && ( fCurrentTeamMode == false ) )
    	{
    		return;
    	}

    	for( iCounter = 0; iCounter < 11; iCounter++ )
    	{
    		if( fCurrentTeamMode == true )
    			iId = GetIdOfMercWithHighestStat( iCounter );
    		else
    			iId = GetIdOfDepartedMercWithHighestStat( iCounter );

    //		if( iId == -1 )
    //			continue;

    			// even or odd?..color black or yellow?
    		if( iCounter % 2 == 0 )
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_ALTERNATE_COLOR );
    		}
    		else
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );
    		}


    		if( fCurrentTeamMode == true )
    		{
    			// get name
    			if( iId == -1 )
    				sString = wprintf( "%s", pPOWStrings[1] );
    			else
    				sString = wprintf( "%s", MercPtrs[ iId ] . name );
    		}
    		else
    		{
    			// get name
    			sString = wprintf( "%s", gMercProfiles[(NPCID)(iId)].zNickname );
    		}
    		// print name
    		mprintf( PERS_STAT_HIGHEST_X, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), sString );
		
    		switch( iCounter )
    		{
    				 case 0:
    					 // health
    					 if( fCurrentTeamMode == true )
    					 {
    						 if( iId == -1 )
    							iStat = -1;
    						 else
    							iStat = MercPtrs[ iId ] . bLifeMax;
    					 }
    					 else
    					 {
    						 iStat =  gMercProfiles[(NPCID)(iId)] . bLife;
    					 }
    					 break;
    					 case 1:
    						 // agility
    						 if( fCurrentTeamMode == true )
    						 {
    								iStat = MercPtrs[ iId ] . bAgility;
    						 }
    						 else
    						 {
    							 	iStat =  gMercProfiles[(NPCID)(iId)] . bAgility;
    						 }

    						 break;
    					 case 2:
    						// dexterity
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bDexterity;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bDexterity;
    						}

    						break;
    					 case 3:
    					 // strength
    					 if( fCurrentTeamMode == true )
    					 {
    					   iStat = MercPtrs[ iId ] . bStrength;
    					 }
    					 else
    					 {
    							iStat =  gMercProfiles[(NPCID)(iId)] . bStrength;
    					 }

    					 break;
    					 case 4:
    						// leadership
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bLeadership;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bLeadership;
    						}
    					 break;
    					 case 5:
    						// wisdom
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bWisdom;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bWisdom;
    						}
    					 break;
    					 case 6:
    						// exper
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bExpLevel;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bExpLevel;
    						}
    					 break;
    					 case 7:
    						 //mrkmanship
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMarksmanship;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bMarksmanship;
    						}
    					 break;
    					 case 8:
    						// mech
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMechanical;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bMechanical;
    						}
    					 break;
    					 case 9:
    						// exp
    						if( fCurrentTeamMode == true )
    						{
    							iStat = MercPtrs[ iId ] . bExplosive;
    						}
    						else
    						{
    							iStat = gMercProfiles[(NPCID)(iId)] . bExplosive;
    						}
    					 break;
    					 case 10:
    						 // med
    						if( fCurrentTeamMode == true )
    						{
    						  iStat = MercPtrs[ iId ] . bMedical;
    						}
    						else
    						{
    							iStat =  gMercProfiles[(NPCID)(iId)] . bMedical;
    						}
    					 break;
    			}
		
    			if( iStat == -1 )
    				sString = wprintf( "%s", pPOWStrings[1] );
    			else
    				sString = wprintf( "%d", iStat );

    		// right justify
    	  FontSubSystem.FindFontRightCoordinates( PERS_STAT_HIGHEST_X, 0, PERS_STAT_LOWEST_WIDTH, 0, sString, FontStyle.FONT10ARIAL, out sX, out sY );

    	  mprintf( sX, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), sString );
    	}

    	return;
    }

    private static void DisplayPersonnelTeamStats()
    {
    	// displays the stat title for each row in the team stat list
    	int iCounter =0;


    	// set up font
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( FontColor.FONT_WHITE );


    	// display titles for each row
    	for( iCounter = 0; iCounter < 11; iCounter++ )
    	{
    			// even or odd?..color black or yellow?
    		if( iCounter % 2 == 0 )
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_ALTERNATE_COLOR );
    		}
    		else
    		{
    			FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );
    		}

    		mprintf( PERS_STAT_LIST_X, PERS_STAT_AVG_Y + ( iCounter + 1 ) * ( FontSubSystem.GetFontHeight( FontStyle.FONT10ARIAL ) + 3 ), pPersonnelTeamStatsStrings[ iCounter ] );
    	}


    	return;
    }

    private static int GetNumberOfPastMercsOnPlayersTeam()
    {
    	int iPastNumberOfMercs = 0;
    	int iCounter = 0;
    	// will run through the alist of past mercs on the players team and return thier number

    	// dead
    	iPastNumberOfMercs += GetNumberOfDeadOnPastTeam( );
	
    	// left
    	iPastNumberOfMercs += GetNumberOfLeftOnPastTeam( );

    	// other
    	iPastNumberOfMercs += GetNumberOfOtherOnPastTeam( );


    	return iPastNumberOfMercs;
    }


    private static int GetNumberOfDeadOnPastTeam()
    {

    	int iNumberDead = 0;
    	int iCounter = 0;

    //	for( iCounter = 0; ( ( iCounter < 256) && ( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] != -1 ) ) ; iCounter ++ )
    	for( iCounter = 0; iCounter < 256 ; iCounter ++ )
    	{
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] != -1 )
    			iNumberDead++;
    	}


    	return( iNumberDead );

    }

    private static int GetNumberOfLeftOnPastTeam()
    {

    	int iNumberLeft = 0;
    	int iCounter = 0;

    	for( iCounter = 0; iCounter < 256 ; iCounter ++ )
    	{
    		if( LaptopSaveInfo.ubLeftCharactersList[ iCounter ] != -1 )
    			iNumberLeft++;
    	}


    	return( iNumberLeft );

    }

    private static int GetNumberOfOtherOnPastTeam()
    {

    	int iNumberOther = 0;
    	int iCounter = 0;

    	for( iCounter = 0; iCounter < 256 ; iCounter ++ )
    	{
    		if( LaptopSaveInfo.ubOtherCharactersList[ iCounter ] != -1 )
    			iNumberOther++;
    	}


    	return( iNumberOther );

    }

    private static void DisplayStateOfPastTeamMembers()
    {
    	int sX, sY;
    	string sString = string.Empty;


    	// font stuff
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );


    	// diplsya numbers fired, dead and othered
    	if( fCurrentTeamMode == false )
    	{

    		// dead
    	   mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_COST_Y, pPersonelTeamStrings[ 5 ] );
    		 sString = wprintf( "%d", GetNumberOfDeadOnPastTeam( ) );
		 
    		 FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_DEPART_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );
		 
    		 mprintf( sX ,PERS_CURR_TEAM_COST_Y, sString );
		 
    		 // fired
    		 mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_HIGHEST_Y, pPersonelTeamStrings[ 6 ] );
    	   sString = wprintf( "%d", GetNumberOfLeftOnPastTeam( ) );
		 
    		 FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_DEPART_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );
		 
    		 mprintf( sX ,PERS_CURR_TEAM_HIGHEST_Y, sString );

    		 // other
    		 mprintf(  PERS_CURR_TEAM_COST_X, PERS_CURR_TEAM_LOWEST_Y, pPersonelTeamStrings[ 7 ] );
    		 sString = wprintf( "%d", GetNumberOfOtherOnPastTeam( ) );
		 
    		 FontSubSystem.FindFontRightCoordinates( (int)(PERS_CURR_TEAM_COST_X), 0, PERS_DEPART_TEAM_WIDTH, 0, sString, PERS_FONT, out sX, out sY );
		 
    		 mprintf( sX ,PERS_CURR_TEAM_LOWEST_Y, sString );
    	}
    	else
    	{
    		// do nothing
    	}
    	return;
    }

    // was a function-local static of CreateDestroyCurrentDepartedMouseRegions
    private static bool CreateDestroyCurrentDepartedMouseRegions_fCreated = false;
    private static void CreateDestroyCurrentDepartedMouseRegions()
    {



    	// will arbitrate the creation/deletion of mouse regions for current/past team toggles


    	if( ( fCreateRegionsForPastCurrentToggle == true ) && ( CreateDestroyCurrentDepartedMouseRegions_fCreated == false ) )
    	{
    		// not created, create
    		MouseSubSystem.MSYS_DefineRegion( gTogglePastCurrentTeam[ 0 ], PERS_TOGGLE_CUR_DEPART_X, PERS_TOGGLE_CUR_Y, PERS_TOGGLE_CUR_DEPART_X + PERS_TOGGLE_CUR_DEPART_WIDTH, PERS_TOGGLE_CUR_Y + PERS_TOGGLE_CUR_DEPART_HEIGHT, MSYS_PRIORITY.HIGHEST - 3, CURSOR.LAPTOP_SCREEN, MSYS_NO_CALLBACK, PersonnelCurrentTeamCallback );
		
    		MouseSubSystem.MSYS_AddRegion( ref gTogglePastCurrentTeam[ 0 ] );

    		MouseSubSystem.MSYS_DefineRegion( gTogglePastCurrentTeam[ 1 ], PERS_TOGGLE_CUR_DEPART_X, PERS_TOGGLE_DEPART_Y, PERS_TOGGLE_CUR_DEPART_X + PERS_TOGGLE_CUR_DEPART_WIDTH, PERS_TOGGLE_DEPART_Y + PERS_TOGGLE_CUR_DEPART_HEIGHT, MSYS_PRIORITY.HIGHEST - 3, CURSOR.LAPTOP_SCREEN, MSYS_NO_CALLBACK, PersonnelDepartedTeamCallback );

    		MouseSubSystem.MSYS_AddRegion( ref gTogglePastCurrentTeam[ 1 ] );

    		CreateDestroyCurrentDepartedMouseRegions_fCreated = true;

    	}
    	else if( ( fCreateRegionsForPastCurrentToggle == false ) && ( CreateDestroyCurrentDepartedMouseRegions_fCreated == true ) )
    	{
    		// created, get rid of

    		MouseSubSystem.MSYS_RemoveRegion( gTogglePastCurrentTeam[ 0 ] );
    		MouseSubSystem.MSYS_RemoveRegion( gTogglePastCurrentTeam[ 1 ] );
    		CreateDestroyCurrentDepartedMouseRegions_fCreated = false;
    	}

    	return;
    }

    private static void PersonnelCurrentTeamCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
      {
    		fCurrentTeamMode = true;

    		if( fCurrentTeamMode == true )
    		{
    		  iCurrentPersonSelectedId = -1;
			
    			// how many people do we have?..if you have someone set default to 0
    			if( GetNumberOfMercsDeadOrAliveOnPlayersTeam() > 0 )
    			{
    				// get id of first merc in list

    				iCurrentPersonSelectedId = GetIdOfFirstDisplayedMerc( );
    			}
    		}

    		fCurrentTeamMode = true;
    		fReDrawScreenFlag = true;	
    	}

    }

    private static void PersonnelDepartedTeamCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
      {

    		fCurrentTeamMode = false;

    		if( fCurrentTeamMode == false )
    		{
    		  iCurrentPersonSelectedId = -1;

    			// how many departed people?
    			if( GetNumberOfPastMercsOnPlayersTeam( ) > 0 )
    			{
    				iCurrentPersonSelectedId = 0;
    			}

    			//Switch the panel on the right to be the stat panel
    			gubPersonnelInfoState = PERSONNEL_STAT_BTN;
    		}

    		fReDrawScreenFlag = true;
    	}

    }

    // was a function-local static of CreateDestroyButtonsForDepartedTeamList
    private static bool CreateDestroyButtonsForDepartedTeamList_fCreated = false;
    private static void CreateDestroyButtonsForDepartedTeamList()
    {
    	// creates/ destroys the buttons for cdeparted team list


    	if( ( fCurrentTeamMode == false ) && ( CreateDestroyButtonsForDepartedTeamList_fCreated == false ) )
    	{
    		// not created. create
    		giPersonnelButtonImage[ 4 ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\departuresbuttons.sti" ,-1,0,-1,2,-1 );
    	  giPersonnelButton[ 4 ] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[4], new Point(PERS_DEPARTED_UP_X, PERS_DEPARTED_UP_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)DepartedUpCallBack);
		
    		// right button
    		giPersonnelButtonImage[ 5 ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\departuresbuttons.sti" ,-1,1,-1,3,-1 );
    		giPersonnelButton[ 5 ] = ButtonSubSystem.QuickCreateButton(giPersonnelButtonImage[5], new Point(PERS_DEPARTED_UP_X, PERS_DEPARTED_DOWN_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)DepartedDownCallBack);
		
    		// set up cursors for these buttons
    		ButtonSubSystem.SetButtonCursor( giPersonnelButton[ 4 ], CURSOR.LAPTOP_SCREEN);
    	  ButtonSubSystem.SetButtonCursor( giPersonnelButton[ 5 ], CURSOR.LAPTOP_SCREEN);


    		CreateDestroyButtonsForDepartedTeamList_fCreated = true;
    	}
    	else if( ( fCurrentTeamMode == true ) && ( CreateDestroyButtonsForDepartedTeamList_fCreated == true ) )
    	{
    		// created. destroy
    		ButtonSubSystem.RemoveButton(giPersonnelButton[4] );
    	  ButtonSubSystem.UnloadButtonImage(giPersonnelButtonImage[4] );
    	  ButtonSubSystem.RemoveButton(giPersonnelButton[5] );
    	  ButtonSubSystem.UnloadButtonImage(giPersonnelButtonImage[5] );
    		CreateDestroyButtonsForDepartedTeamList_fCreated = false;
    		fReDrawScreenFlag = true;
    	}
    }

    private static void DepartedUpCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
			
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    		  btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);

    		  if(  giCurrentUpperLeftPortraitNumber - 20 >= 0 )
    			{
    			  giCurrentUpperLeftPortraitNumber-=20;
    			  fReDrawScreenFlag = true;
    			}
    		}
    	}
    }

    private static void DepartedDownCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
			
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    			if( ( giCurrentUpperLeftPortraitNumber + 20 ) < ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) )
    			{
    				giCurrentUpperLeftPortraitNumber+=20;
    				fReDrawScreenFlag = true;
    			}
    		}
    	}
    }

    private static void DisplayPastMercsPortraits()
    {
    	// display past mercs portraits, starting at giCurrentUpperLeftPortraitNumber and going up 20 mercs
    	// start at dead mercs, then fired, then other

    	int iCounter = 0;
    	int iCounterA = 0;
    	int iStartArray = 0; // 0 = dead list, 1 = fired list, 2 = other list
    	bool fFound = false;

    	// not time to display
    	if( fCurrentTeamMode == true )
    	{
    		return;
    	}

    	// go through dead list
    //	for( iCounterA = 0; ( ( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ] != -1 ) && ( iCounter < giCurrentUpperLeftPortraitNumber ) ); iCounter++, iCounterA++ );
    	for( iCounterA = 0; ( iCounter < giCurrentUpperLeftPortraitNumber ); iCounterA++ )
    	{
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ] != -1 )
    			iCounter++;
    	}

    	if( iCounter < giCurrentUpperLeftPortraitNumber)
    	{
    			// now the fired list
    //			for( iCounterA = 0; ( ( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ] != -1 ) && ( iCounter < giCurrentUpperLeftPortraitNumber ) ); iCounter++, iCounterA++ );
    			for( iCounterA = 0; ( ( iCounter < giCurrentUpperLeftPortraitNumber ) ); iCounterA++ )
    			{
    				if( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ] != -1 )
    				{
    					iCounter++;
    				}
    			}
			
    			if( iCounter < 20 )
    			{
    			  iStartArray = 0;
    			}
    			else
    			{
    				iStartArray = 1;
    			}
    	}
    	else
    	{
    		iStartArray = 0;
    	}

    	if( ( iCounter < giCurrentUpperLeftPortraitNumber ) && ( iStartArray != 0 ) )
    	{
    			// now the fired list
    //			for( iCounterA = 0; ( ( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ] != -1 ) && ( iCounter < giCurrentUpperLeftPortraitNumber ) ); iCounter++, iCounterA++ );
    			for( iCounterA = 0; ( iCounter < giCurrentUpperLeftPortraitNumber ); iCounterA++ )
    			{
    				if( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ] != -1 )
    					iCounter++;
    			}
			
    			if( iCounter < 20 )
    			{
    			  iStartArray = 1;
    			}
    			else
    			{
    				iStartArray = 2;
    			}
    	}
    	else if( iStartArray != 0 )
    	{
    		iStartArray = 1;
    	}

    	//; we now have the array to start in, the position

    	iCounter = 0;

    	if( iStartArray == 0 )
    	{
    		// run through list and display
    //	  for( ; ( ( iCounter < 20 ) && ( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ] != -1 ) ); iCounter++, iCounterA++ )
    	  for( ; iCounter < 20 && iCounterA < 256; iCounterA++ )
    		{
    			// show dead pictures
    			if( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ] != -1 )
    			{
    				DisplayPortraitOfPastMerc( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ], iCounter, true, false, false );
    				iCounter++;
    			}
    		}

    		// reset counter A for the next array, if applicable
    		iCounterA = 0;
    	}
    	if( iStartArray <= 1 )
    	{
    //		for( ; ( ( iCounter < 20 ) && ( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ] != -1 ) ); iCounter++, iCounterA++  )
    		for( ; ( iCounter < 20  && iCounterA < 256 ); iCounterA++  )
    		{
    			// show fired pics
    			if( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ] != -1 )
    			{
    				DisplayPortraitOfPastMerc( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ], iCounter, false, true, false );
    				iCounter++;
    			}
    		}
    		// reset counter A for the next array, if applicable
    		iCounterA = 0;
    	}

    	if( iStartArray <= 2 )
    	{
    //		for( ; ( ( iCounter < 20 ) && ( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ] != -1 ) ) ; iCounter++, iCounterA++ )
    		for( ; ( iCounter < 20  && iCounterA < 256 ); iCounterA++ )
    		{
    			// show other pics
    			if( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ] != -1 )
    			{
    				DisplayPortraitOfPastMerc( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ], iCounter, false, false, true );
    				iCounter++;
    			}
    		}
    		// reset counter A for the next array, if applicable
    		iCounterA = 0;
    	}



    	return;
    }

    private static int GetIdOfPastMercInSlot(int iSlot)
    {

    	int iCounter =-1;
    	int iCounterA =0;
    	// returns ID of Merc in this slot

    	// not time to display
    	if( fCurrentTeamMode == true )
    	{
    		return -1;
    	}

    	if( iSlot > ( ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) - giCurrentUpperLeftPortraitNumber ) )
    	{
    		// invalid slot
    		return iCurrentPersonSelectedId;
    	}
    	// go through dead list
    	for( iCounterA = 0; ( ( iCounter ) < iSlot + giCurrentUpperLeftPortraitNumber);  iCounterA++ )
    	{
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounterA ] != -1 )
    			iCounter++;
    	}

    	if( iSlot + giCurrentUpperLeftPortraitNumber == iCounter)
    	{
    		return  ( LaptopSaveInfo.ubDeadCharactersList[ iCounterA - 1] );
    	}

    	// now the fired list
    	iCounterA =0;
    	for( iCounterA = 0; ( ( ( iCounter  )< iSlot + giCurrentUpperLeftPortraitNumber) ); iCounterA++ )
    	{
    		if( LaptopSaveInfo.ubLeftCharactersList[ iCounterA ] != -1 )
    			iCounter++;
    	}
		
    	if( iSlot + giCurrentUpperLeftPortraitNumber == iCounter)
    	{
    		return  ( LaptopSaveInfo.ubLeftCharactersList[ iCounterA  - 1 ] );
    	}
		

    	// now the fired list
    	iCounterA =0;
    	for( iCounterA = 0; ( ( ( iCounter ) < ( iSlot + giCurrentUpperLeftPortraitNumber ) ) ); iCounterA++ )
    	{
    		if( LaptopSaveInfo.ubOtherCharactersList[ iCounterA ] != -1 )
    			iCounter++;
    	}

    	return( LaptopSaveInfo.ubOtherCharactersList[ iCounterA  - 1] );

    }

    private static bool DisplayPortraitOfPastMerc(int iId, int iCounter, bool fDead, bool fFired, bool fOther)
    {

	
    	string sTemp = string.Empty;
    	HVOBJECT hFaceHandle;
	


    	if( ( 50 < 	iId   )&&( 57 > 	iId   ) )
    	{
    		sTemp = wprintf( "%s%03d.sti", SMALL_FACES_DIR, 	 gMercProfiles[(NPCID)(iId)].ubFaceIndex );	
    	}
    	else
    	{
    		if(  iId  < 100 )
    		{
    			 sTemp = wprintf( "%s%02d.sti", SMALL_FACES_DIR, iId );
    		}
    		else
    		{
    			sTemp = wprintf( "%s%03d.sti", SMALL_FACES_DIR,	 iId  );
    		}
    	}

    	guiFACE = video.GetVideoObject(Utils.FilenameForBPP(sTemp));


    	//Blt face to screen to 
    	hFaceHandle = guiFACE;

    	if( fDead )
    	{
    		hFaceHandle.pShades[ 0 ]		= video.Create16BPPPaletteShaded( hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true );
			
    		//set the red pallete to the face
    		guiFACE.ShadeCurrentPixels = guiFACE.pShades[0];
    	}

    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0,( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT ), VO_BLT.SRCTRANSPARENCY);

	
    /*
     text on the Small portrait
    	if( fDead )
    	{
    		//if the merc is dead, display it
    //		FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[0], new(( INT16 ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( INT16 ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORT_HEIGHT / 2 )), SMALL_PORTRAIT_WIDTH_NO_BORDERS, FONT10ARIAL, 145, FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		FontSubSystem.DrawTextToScreen(AimPopUpText[ AIM_MEMBER_DEAD ], new(( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORT_HEIGHT / 2 )), SMALL_PORTRAIT_WIDTH_NO_BORDERS, FontStyle.FONT10ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}
    	else if( fFired )
    	{
    		FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[1], new(( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORT_HEIGHT / 2 )), SMALL_PORTRAIT_WIDTH_NO_BORDERS, FontStyle.FONT10ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}
    	else if( fOther )
    	{
    		FontSubSystem.DrawTextToScreen(pDepartedMercPortraitStrings[2], new(( int ) ( SMALL_PORTRAIT_START_X+ ( iCounter % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCounter / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT + SMALL_PORT_HEIGHT / 2 )), SMALL_PORTRAIT_WIDTH_NO_BORDERS, FontStyle.FONT10ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}
    */
	
    	video.DeleteVideoObject(guiFACE);
	
    	return ( true );
    }

    private static void DisplayDepartedCharStats(int iId, int iSlot, int iState)
    {
    	int iCounter=0;
    	string sString = string.Empty;
    	int sX, sY;
    	int uiHits = 0;

    		// font stuff
    	FontSubSystem.SetFont( FontStyle.FONT10ARIAL );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
    	FontSubSystem.SetFontForeground( PERS_TEXT_FONT_COLOR );

    	// display the stats for a char
    	for(iCounter=0;iCounter <MAX_STATS; iCounter++)
    	{
    		switch(iCounter)
    		{
    		 case 0:
    			 // health

    			 // dead?
    			 if( iState == 0 )
    			 {
    			   sString = wprintf( "%d/%d",0,gMercProfiles[(NPCID)(iId)].bLife);
    			 }
    			 else
    			 {
    				 sString = wprintf( "%d/%d",gMercProfiles[(NPCID)(iId)].bLife,gMercProfiles[(NPCID)(iId)].bLife);
    			 }
       
    			 mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		  
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			 break;
    		 case 1:
    			 // agility
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bAgility);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		 
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			 break;
    		 case 2:
    		  // dexterity
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bDexterity);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    			break;
    		 case 3:
    		 // strength
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bStrength);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 4:
    		  // leadership
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bLeadership);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 5:
    		  // wisdom
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bWisdom);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 6:
    		  // exper
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bExpLevel);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 7:
    			 //mrkmanship
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bMarksmanship);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
         case 8:
    		  // mech
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bMechanical);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
         case 9:
    		  // exp
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bExplosive);
           mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		 break;
    		 case 10:
           // med
    			 mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[iCounter]);		
        
           sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].bMedical);
			 
				
    			 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    			 mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);	
    		 break;


    		 case 14:
    		 // kills
          mprintf((int)(pPersonnelScreenPoints[21].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[21].Y,pPersonnelScreenStrings[PRSNL_TXT_KILLS]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].usKills);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[21].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[21].Y,sString);
    		 break;
    		 case 15:
    			// assists
    			mprintf((int)(pPersonnelScreenPoints[22].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[22].Y,pPersonnelScreenStrings[PRSNL_TXT_ASSISTS]);		 
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].usAssists);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[22].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[22].Y,sString);
    		 break;
    		  case 16:
    			// shots/hits
    			mprintf((int)(pPersonnelScreenPoints[23].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[23].Y,pPersonnelScreenStrings[PRSNL_TXT_HIT_PERCENTAGE]);		 
    			uiHits = ( int )gMercProfiles[(NPCID)(iId)].usShotsHit;
    			uiHits *= 100;
			
    			// check we have shot at least once
    			if( gMercProfiles[(NPCID)(iId)].usShotsFired > 0 )
    			{
    			  uiHits /= ( int )gMercProfiles[(NPCID)(iId)].usShotsFired;
    			}
    			else
    			{
    				// no, set hit % to 0
    				uiHits = 0;
    			}

    			sString = wprintf( "%d %%%%",uiHits);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[23].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          sX += FontSubSystem.StringPixLength( "%",  PERS_FONT );
    			mprintf(sX,pPersonnelScreenPoints[23].Y,sString);
    		 break;
    		  case 17:
    			// battles
    			mprintf((int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[24].Y,pPersonnelScreenStrings[PRSNL_TXT_BATTLES]);
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].usBattlesFought);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[24].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[24].Y,sString);
    		 break;
    		 case 18:
    			// wounds
    			mprintf((int)(pPersonnelScreenPoints[25].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[25].Y,pPersonnelScreenStrings[PRSNL_TXT_TIMES_WOUNDED]);
    			sString = wprintf( "%d",gMercProfiles[(NPCID)(iId)].usTimesWounded);
          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[25].X+(iSlot*TEXT_BOX_WIDTH)), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[25].Y,sString);
    		 break;
    		}
    	}

    	return;
    }

    private static void EnableDisableDeparturesButtons()
    {

    	// will enable or disable departures buttons based on upperleft picutre index value
    	if( ( fCurrentTeamMode == true )||( Emails.fNewMailFlag == true ) )
    	{
    		return;
    	}

    	// disable both buttons
    	ButtonSubSystem.DisableButton( giPersonnelButton[ 4 ] );
    	ButtonSubSystem.DisableButton( giPersonnelButton[ 5 ] );


    	if( giCurrentUpperLeftPortraitNumber != 0 )
    	{
    		// enable up button
    		ButtonSubSystem.EnableButton( giPersonnelButton[ 4 ] );
    	}
    	if( ( GetNumberOfDeadOnPastTeam( ) + GetNumberOfLeftOnPastTeam( ) + GetNumberOfOtherOnPastTeam( ) ) - giCurrentUpperLeftPortraitNumber  >= 20 )
    	{
    		// enable down button
    		ButtonSubSystem.EnableButton( giPersonnelButton[ 5 ] );
    	}


    	return;
    }

    private static void DisplayDepartedCharName(int iId, int iSlot, int iState)
    {
      // get merc's nickName, assignment, and sector location info
    	int sX, sY;
    	string sString = string.Empty;

      FontSubSystem.SetFont(CHAR_NAME_FONT);
    	FontSubSystem.SetFontForeground(PERS_TEXT_FONT_COLOR);
    	FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
	
    	if( ( iState == -1 )||( iId == -1 ) )
    	{
    		return;
    	}

    	sString = wprintf( "%s", gMercProfiles[(NPCID)(iId)].zNickname );

    		// nick name - assignment
    	FontSubSystem.FindFontCenterCoordinates( IMAGE_BOX_X-5, 0, IMAGE_BOX_WIDTH + 90, 0, sString, CHAR_NAME_FONT, out sX, out sY );

    	// cehck to se eif we are going to go off the left edge
    	if( sX < pPersonnelScreenPoints[ 0 ].X )
    	{
    		sX = ( int )pPersonnelScreenPoints[ 0 ].X;
    	}

    	mprintf( sX + iSlot * IMAGE_BOX_WIDTH, CHAR_NAME_Y, sString );
  

    	// state
    	if( gMercProfiles[(NPCID)(iId)].ubMiscFlags2.HasFlag(PROFILE_MISC_FLAG2.MARRIED_TO_HICKS) )
    	{
    		//displaye 'married'
    		sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_MARRIED ] );
    	}
    	else if(  iState == DEPARTED_DEAD )
    	{
    		sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_DEAD ] );
    	}

    	//if the merc is an AIM merc
    	else if( iId < (int)NPCID.BIFF )
    	{
    		//if dismissed
    		if( iState == DEPARTED_FIRED )
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_FIRED ] );
    		else
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_CONTRACT_EXPIRED ] );
    	}

    	//else if its a MERC merc
    	else if( iId >= (int)NPCID.BIFF && iId <= (int)NPCID.BUBBA )
    	{
    		if( iState == DEPARTED_FIRED )
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_FIRED ] );
    		else
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_QUIT ] );
    	}
    	//must be a RPC
    	else
    	{
    		if( iState == DEPARTED_FIRED )
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_FIRED ] );
    		else
    			sString = wprintf( "%s", pPersonnelDepartedStateStrings[ DEPARTED_QUIT ] );
    	}

    //	swprintf( sString, L"%s", pPersonnelDepartedStateStrings[ iState ] );

    		// nick name - assignment
    	FontSubSystem.FindFontCenterCoordinates( IMAGE_BOX_X-5, 0, IMAGE_BOX_WIDTH + 90, 0, sString, CHAR_NAME_FONT, out sX, out sY );

    	// cehck to se eif we are going to go off the left edge
    	if( sX < pPersonnelScreenPoints[ 0 ].X )
    	{
    		sX = ( int )pPersonnelScreenPoints[ 0 ].X;
    	}

    	mprintf( sX + iSlot * IMAGE_BOX_WIDTH, CHAR_NAME_Y + 10 , sString );

  
  
    	return;
    }

    private static int GetTheStateOfDepartedMerc(int iId)
    {

    	int iCounter =0;
    	// will runt hrough each list until merc is found, if not a -1 is returned

    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if ( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] == iId )
    		{
    			return( DEPARTED_DEAD );
    		}
    	}

    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if ( LaptopSaveInfo.ubLeftCharactersList[ iCounter ] == iId )
    		{
    			return( DEPARTED_FIRED );
    		}
    	}

    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if ( LaptopSaveInfo.ubOtherCharactersList[ iCounter ] == iId )
    		{
    			return( DEPARTED_OTHER );
    		}
    	}

    	return( -1 );
    }

    private static void DisplayPersonnelTextOnTitleBar()
    {
    	// draw email screen title text

    	// font stuff
      FontSubSystem.SetFont( FontStyle.FONT14ARIAL );
      FontSubSystem.SetFontForeground( FontColor.FONT_WHITE );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );
	
    	// printf the title
    	mprintf( PERS_TITLE_X, PERS_TITLE_Y, pPersTitleText[0] );
  
    	// reset the shadow
	
    }

    private static bool DisplayHighLightBox()
    {
    	// will display highlight box around selected merc
    	 HVOBJECT uiBox;
    	 HVOBJECT hHandle;
  
    	// load graphics

    	// is the current selected face valid?
    	if( iCurrentPersonSelectedId == -1 )
    	{
    		// no, leave
    		return ( false );
    	}

      // bounding
      uiBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\PicBorde.sti"));

    	// blit it
    	hHandle = uiBox;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,( int ) ( SMALL_PORTRAIT_START_X+ ( iCurrentPersonSelectedId % PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_WIDTH - 2 ), ( int ) ( SMALL_PORTRAIT_START_Y + ( iCurrentPersonSelectedId / PERSONNEL_PORTRAIT_NUMBER_WIDTH ) * SMALL_PORT_HEIGHT - 3 ), VO_BLT.SRCTRANSPARENCY);


    	// deleteit
    	video.DeleteVideoObject( uiBox );


    	return ( true );
    }

    private static void AddCharacterToDeadList(SOLDIERTYPE? pSoldier)
    {
    	int iCounter = 0;


    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] == -1 )
    		{
    			// valid slot, merc not found yet, inset here
    			LaptopSaveInfo.ubDeadCharactersList[ iCounter ] = (int)pSoldier.ubProfile;

    			// leave
    			return;
    		}

    		// are they already in the list?
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] == (int)pSoldier.ubProfile )
    		{

    			return;
    		}
    	}
    }

    private static void AddCharacterToFiredList(SOLDIERTYPE? pSoldier)
    {
    	int iCounter = 0;


    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if( LaptopSaveInfo.ubLeftCharactersList[ iCounter ] == -1 )
    		{
    			// valid slot, merc not found yet, inset here
    			LaptopSaveInfo.ubLeftCharactersList[ iCounter ] = (int)pSoldier.ubProfile;

    			// leave
    			return;
    		}

    		// are they already in the list?
    		if( LaptopSaveInfo.ubLeftCharactersList[ iCounter ] == (int)pSoldier.ubProfile )
    		{

    			return;
    		}
    	}
    }

    private static void AddCharacterToOtherList(SOLDIERTYPE? pSoldier)
    {
    	int iCounter = 0;


    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		if( LaptopSaveInfo.ubOtherCharactersList[ iCounter ] == -1 )
    		{
    			// valid slot, merc not found yet, inset here
    			LaptopSaveInfo.ubOtherCharactersList[ iCounter ] = (int)pSoldier.ubProfile;

    			// leave
    			return;
    		}

    		// are they already in the list?
    		if( LaptopSaveInfo.ubOtherCharactersList[ iCounter ] == (int)pSoldier.ubProfile )
    		{

    			return;
    		}
    	}
    }

    private static bool RemoveNewlyHiredMercFromPersonnelDepartedList(int ubProfile)
    {
    	int iCounter = 0;

    	for( iCounter = 0; iCounter < 256; iCounter++ )
    	{
    		// are they already in the Dead list?
    		if( LaptopSaveInfo.ubDeadCharactersList[ iCounter ] == ubProfile )
    		{
    			//Reset the fact that they were once hired
    			LaptopSaveInfo.ubDeadCharactersList[ iCounter ] = -1;
    			return( true );
    		}

    		// are they already in the other list?
    		if( LaptopSaveInfo.ubLeftCharactersList[ iCounter ] == ubProfile )
    		{
    			//Reset the fact that they were once hired
    			LaptopSaveInfo.ubLeftCharactersList[ iCounter ] = -1;
    			return( true );
    		}

    		// are they already in the list?
    		if( LaptopSaveInfo.ubOtherCharactersList[ iCounter ] == ubProfile )
    		{
    			//Reset the fact that they were once hired
    			LaptopSaveInfo.ubOtherCharactersList[ iCounter ] = -1;
    			return( true );
    		}
    	}

    	return( false );
    }

    private static int GetIdOfFirstDisplayedMerc()
    {
    	SOLDIERTYPE? pSoldier;
    	int cnt = 0, iCounter = 0;

    	// set current soldier
    	pSoldier = MercPtrs[ cnt ];

      if( fCurrentTeamMode == true )
    	{
    		// run through list of soldiers on players current team
    		//cnt = gTacticalStatus.Team[ pSoldier->bTeam ].bFirstID;
        for ( pSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    		{	
    		   if( ( pSoldier.bActive )&&( pSoldier.bLife > 0 ) )
    			 {
    				 return( 0 );
    			 }
    		}
    		return( -1 );
    	}
    	else
    	{
    		// run through list of soldier on players old team...the slot id will be translated
    		return 0;
    	}
    }

    private static int GetIdOfThisSlot(int iSlot)
    {
    		SOLDIERTYPE? pSoldier;
    	int cnt = 0;
    	int iCounter = 0;

    	// set current soldier
    	pSoldier = MercPtrs[ cnt ];

      if( fCurrentTeamMode == true )
    	{
    		// run through list of soldiers on players current team
    		cnt = gTacticalStatus.Team[ pSoldier.bTeam ].bFirstID;
        for ( pSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++, pSoldier = MercPtrs[Math.Min(cnt, MercPtrs.Length - 1)] )
    		{	
    		   if( ( pSoldier.bActive )  )
    			 {
				 

    				 // same character as slot, return this value
    				 if( iCounter == iSlot )
    				 {
    						return( cnt );
    				 }

    				 // found another soldier
    				 iCounter++;
    			 }
    		}
    	}
    	else
    	{
    		// run through list of soldier on players old team...the slot id will be translated
    		return iSlot;
    	}


    	return( 0 );
    }

    private static bool RenderAtmPanel()
    {
    	 HVOBJECT uiBox;
    	 HVOBJECT hHandle;


    	// render the ATM panel
    	if( fShowAtmPanel )
    	{
    		uiBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\AtmButtons.sti"));

    		// blit it
    		hHandle = uiBox;
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,( int ) ( ATM_UL_X ), ( int ) ( ATM_UL_Y ), VO_BLT.SRCTRANSPARENCY);

    		video.DeleteVideoObject( uiBox );

    		// show amount
    		DisplayATMAmount( );
    		RenderRectangleForPersonnelTransactionAmount( );

    		// create destroy
    		CreateDestroyStartATMButton( );
    		CreateDestroyATMButton( );

    		// display strings for ATM
    		DisplayATMStrings( );

    		// handle states
    		HandleStateOfATMButtons( );

    		//DisplayAmountOnCurrentMerc( );

    	}
    	else
    	{
    		// just show basic panel
    		// bounding
    		uiBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\AtmButtons.sti"));

    		hHandle = uiBox;
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0,( int ) ( ATM_UL_X ), ( int ) ( ATM_UL_Y ), VO_BLT.SRCTRANSPARENCY);

    		// blit it
    		hHandle = uiBox;
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 1,( int ) ( ATM_UL_X + 1 ), ( int ) ( ATM_UL_Y + 18), VO_BLT.SRCTRANSPARENCY);


    		video.DeleteVideoObject( uiBox );

    		// display strings for ATM
    		DisplayATMStrings( );

    		//DisplayAmountOnCurrentMerc( );

    		// create destroy
    		CreateDestroyStartATMButton( );
    		CreateDestroyATMButton( );
    	}
    	return( true );
    }

    // was a function-local static of CreateDestroyStartATMButton
    private static bool CreateDestroyStartATMButton_fCreated = false;
    private static void CreateDestroyStartATMButton()
    {

    	// create/destroy atm start button as needed
	

    	if( ( CreateDestroyStartATMButton_fCreated == false ) && ( fShowAtmPanelStartButton == true ) )
    	{
    		// not created, must create

    		/*
    		// the ATM start button
    		giPersonnelATMStartButtonImage[ 0 ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,2,-1,3,-1 );
    		giPersonnelATMStartButton[ 0 ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMStartButtonImage[ 0 ], new Point(519, 87), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)ATMStartButtonCallback);

    		// set text and what not
    		ButtonSubSystem.SpecifyButtonText( giPersonnelATMStartButton[ 0 ] ,gsAtmStartButtonText[ 0 ] );
    		ButtonSubSystem.SpecifyButtonUpTextColors( giPersonnelATMStartButton[ 0 ], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);
    		ButtonSubSystem.SpecifyButtonFont( giPersonnelATMStartButton[ 0 ], PERS_FONT );
    		ButtonSubSystem.SetButtonCursor(giPersonnelATMStartButton[ 0 ], CURSOR.LAPTOP_SCREEN);
    */
    		// the stats button
    		giPersonnelATMStartButtonImage[ PERSONNEL_STAT_BTN ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,2,-1,3,-1 );
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMStartButtonImage[ PERSONNEL_STAT_BTN ], new Point(519, 80), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK?)null, (GUI_CALLBACK)PersonnelStatStartButtonCallback);

    		// set text and what not
    		ButtonSubSystem.SpecifyButtonText( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] ,gsAtmStartButtonText[ 1 ] );
    		ButtonSubSystem.SpecifyButtonUpTextColors( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);
    		ButtonSubSystem.SpecifyButtonFont( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ], PERS_FONT );
    		ButtonSubSystem.SetButtonCursor(giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ], CURSOR.LAPTOP_SCREEN);

    		// the Employment selection button
    		giPersonnelATMStartButtonImage[ PERSONNEL_EMPLOYMENT_BTN ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,2,-1,3,-1 );
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMStartButtonImage[ PERSONNEL_EMPLOYMENT_BTN ], new Point(519, 110), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK?)null, (GUI_CALLBACK)EmployementInfoButtonCallback);

    		// set text and what not
    		ButtonSubSystem.SpecifyButtonText( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] ,gsAtmStartButtonText[ 3 ] );
    		ButtonSubSystem.SpecifyButtonUpTextColors( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);
    		ButtonSubSystem.SpecifyButtonFont( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ], PERS_FONT );
    		ButtonSubSystem.SetButtonCursor(giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ], CURSOR.LAPTOP_SCREEN);

    		// the inventory selection button
    		giPersonnelATMStartButtonImage[ PERSONNEL_INV_BTN ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,2,-1,3,-1 );
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMStartButtonImage[ PERSONNEL_INV_BTN ], new Point(519, 140), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK?)null, (GUI_CALLBACK)PersonnelINVStartButtonCallback);

    		// set text and what not
    		ButtonSubSystem.SpecifyButtonText( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] ,gsAtmStartButtonText[ 2 ] );
    		ButtonSubSystem.SpecifyButtonUpTextColors( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);
    		ButtonSubSystem.SpecifyButtonFont( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ], PERS_FONT );
    		ButtonSubSystem.SetButtonCursor(giPersonnelATMStartButton[ PERSONNEL_INV_BTN ], CURSOR.LAPTOP_SCREEN);

		
    		CreateDestroyStartATMButton_fCreated = true;
    	}
    	else if( ( CreateDestroyStartATMButton_fCreated == true ) && ( fShowAtmPanelStartButton == false ) )
    	{
    		// stop showing
    		/*
    		ButtonSubSystem.RemoveButton( giPersonnelATMStartButton[ 0 ] );
    		ButtonSubSystem.UnloadButtonImage( giPersonnelATMStartButtonImage[ 0 ] );
    		*/
    		ButtonSubSystem.RemoveButton( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] );
    		ButtonSubSystem.UnloadButtonImage( giPersonnelATMStartButtonImage[ PERSONNEL_STAT_BTN ] );
    		ButtonSubSystem.RemoveButton( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] );
    		ButtonSubSystem.UnloadButtonImage( giPersonnelATMStartButtonImage[ PERSONNEL_EMPLOYMENT_BTN ] );
    		ButtonSubSystem.RemoveButton( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] );
    		ButtonSubSystem.UnloadButtonImage( giPersonnelATMStartButtonImage[ PERSONNEL_INV_BTN ] );

    		CreateDestroyStartATMButton_fCreated = false;
    	}
    }

    private static void FindPositionOfPersInvSlider()
    {
    	int iValue = 0;
    	int iNumberOfItems = 0;
    	int sSizeOfEachSubRegion = 0;
    	int sYPositionOnBar = 0;
    	int iCurrentItemValue = 0;

    	// find out how many there are
    	iValue = ( int )( GetNumberOfInventoryItemsOnCurrentMerc( ) );

    	// otherwise there are more than one item
    	iNumberOfItems = iValue - NUMBER_OF_INVENTORY_PERSONNEL;

    	if( iValue <= 0 )
    	{
    		iValue = 1;
    	}

    	// get the subregion sizes
    	sSizeOfEachSubRegion = ( int )( ( int )( Y_SIZE_OF_PERSONNEL_SCROLL_REGION - SIZE_OF_PERSONNEL_CURSOR ) / ( int )( iNumberOfItems  ) );

    	// get slider position
    	guiSliderPosition = uiCurrentInventoryIndex * sSizeOfEachSubRegion;

    }

    private static void HandleSliderBarClickCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	int iValue = 0;
    	int iNumberOfItems = 0;
    	Point MousePos;
    	int sSizeOfEachSubRegion = 0;
    	int sYPositionOnBar = 0;
    	int iCurrentItemValue = 0;

    	if( ( iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) ) || ( iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT) ) ) 
      {
    		// find out how many there are
    		iValue = ( int )( GetNumberOfInventoryItemsOnCurrentMerc( ) );

    		// make sure there are more than one page 
    		if( ( int )uiCurrentInventoryIndex >=  iValue - NUMBER_OF_INVENTORY_PERSONNEL + 1 )
    		{
    			return;
    		}

    		// otherwise there are more than one item
    		iNumberOfItems = iValue - NUMBER_OF_INVENTORY_PERSONNEL;

    		// number of items is 0
    		if( iNumberOfItems == 0 )
    		{
    			return;
    		}

    		// find the x,y on the slider bar
    	  MousePos = new Point(gusMouseXPos, gusMouseYPos);

    		// get the subregion sizes
    		sSizeOfEachSubRegion = ( int )( ( int )( Y_SIZE_OF_PERSONNEL_SCROLL_REGION - SIZE_OF_PERSONNEL_CURSOR ) / ( int )( iNumberOfItems  ) );

    		// get the cursor placement
    		sYPositionOnBar = MousePos.Y - Y_OF_PERSONNEL_SCROLL_REGION;

    		if( sSizeOfEachSubRegion == 0 )
    		{
    			return;
    		}

    		// get the actual item position
    		iCurrentItemValue = sYPositionOnBar / sSizeOfEachSubRegion;

    		if( uiCurrentInventoryIndex != iCurrentItemValue )
    		{
    			// get slider position
    			guiSliderPosition = iCurrentItemValue * sSizeOfEachSubRegion;

    			// set current inventory value
    			uiCurrentInventoryIndex = ( int )iCurrentItemValue;

    			// force update
    			fReDrawScreenFlag = true;
    		}
    	}

    }

    private static void RenderSliderBarForPersonnelInventory()
    {
    	HVOBJECT hHandle;

    	// render slider bar for personnel
    	hHandle = guiPersonnelInventory;
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 5,( int ) ( X_OF_PERSONNEL_SCROLL_REGION ), ( int ) ( guiSliderPosition + Y_OF_PERSONNEL_SCROLL_REGION), VO_BLT.SRCTRANSPARENCY);

    }

    // was a function-local static of CreateDestroyATMButton
    private static bool CreateDestroyATMButton_fCreated = false;
    private static void CreateDestroyATMButton()
    {
    	/*

    	string sString = string.Empty;

	

    	// create/destroy atm start button as needed
    	int iCounter = 0;

    	if( ( CreateDestroyATMButton_fCreated == false ) && ( fShowAtmPanel == true ) )
    	{
		
    		for( iCounter = 0; iCounter < 10; iCounter++ )
    		{
    			if( iCounter != 9 )
    			{
    				iNumberPadButtonsImages[ iCounter ]=ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,4,-1,6,-1 );
    				sString = wprintf( "%d", iCounter+1 );
    			}
    			else
    			{
    				iNumberPadButtonsImages[ iCounter ]=ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,7,-1,9,-1 );
    				sString = wprintf( "%d", iCounter - 9 );
    			}

    			iNumberPadButtons[ iCounter ] = ButtonSubSystem.QuickCreateButton(iNumberPadButtonsImages[ iCounter ], new Point(( int )( ATM_BUTTONS_START_X + ( ATM_BUTTON_WIDTH * ( int )( iCounter % 3 )) ), ( int )( ATM_BUTTONS_START_Y + ( int )( ATM_BUTTON_HEIGHT * ( iCounter / 3 )))), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)ATMNumberButtonCallback);
			
    			if( iCounter != 9)
    			{
    				ButtonSubSystem.MSYS_SetBtnUserData(iNumberPadButtons[iCounter],0,iCounter + 1 );
    			}
    			else
    			{
    				ButtonSubSystem.MSYS_SetBtnUserData(iNumberPadButtons[iCounter],0, 0 );
    			}
    			ButtonSubSystem.SetButtonCursor(iNumberPadButtons[iCounter], CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.SpecifyButtonFont( iNumberPadButtons[iCounter], PERS_FONT );
    			ButtonSubSystem.SpecifyButtonText( iNumberPadButtons[iCounter], sString );
    			ButtonSubSystem.SpecifyButtonUpTextColors( iNumberPadButtons[iCounter], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);

    		}


    		// now slap down done, cancel, dep, withdraw
    		for( iCounter = OK_ATM; iCounter < NUMBER_ATM_BUTTONS ;iCounter++ )
    		{
    			if( iCounter == OK_ATM )
    			{
    				giPersonnelATMSideButtonImage[ iCounter ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,7,-1,9,-1 );
    			}
    			else
    			{
    				giPersonnelATMSideButtonImage[ iCounter ]=  ButtonSubSystem.LoadButtonImage( "LAPTOP\\AtmButtons.sti" ,-1,10,-1,12,-1 );
    			}
			
    			if( ( iCounter != DEPOSIT_ATM ) && ( iCounter != WIDTHDRAWL_ATM ) )
    			{
    				giPersonnelATMSideButton[ iCounter ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMSideButtonImage[ iCounter ], new Point(( int )( pAtmSideButtonPts[ iCounter ].X ), ( int )( pAtmSideButtonPts[ iCounter ].Y )), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)ATMOtherButtonCallback);
    			}
    			else
    			{
    				giPersonnelATMSideButton[ iCounter ] = ButtonSubSystem.QuickCreateButton(giPersonnelATMSideButtonImage[ iCounter ], new Point(( int )( pAtmSideButtonPts[ iCounter ].X ), ( int )( pAtmSideButtonPts[ iCounter ].Y )), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1, (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)ATMOther2ButtonCallback);
    			}
    			ButtonSubSystem.MSYS_SetBtnUserData(giPersonnelATMSideButton[iCounter],0,iCounter );
    			ButtonSubSystem.SpecifyButtonFont( giPersonnelATMSideButton[iCounter], PERS_FONT );
    			ButtonSubSystem.SetButtonCursor(giPersonnelATMSideButton[iCounter], CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.SpecifyButtonUpTextColors( giPersonnelATMSideButton[iCounter], FontColor.FONT_BLACK, (FontShadow)FontColor.FONT_BLACK);
    			ButtonSubSystem.SpecifyButtonText( giPersonnelATMSideButton[iCounter], gsAtmSideButtonText[ iCounter ] );
    		}


    		//SetButtonCursor(giPersonnelATMStartButton, CURSOR_LAPTOP_SCREEN);
    		CreateDestroyATMButton_fCreated = true;
    	}
    	else if( ( CreateDestroyATMButton_fCreated == true ) && ( fShowAtmPanel == false ) )
    	{
    		// stop showing
    		//RemoveButton( giPersonnelATMButton );
    		//UnloadButtonImage( giPersonnelATMButtonImage );
	
    		for( iCounter = 0; iCounter < 10; iCounter++ )
    		{
    			ButtonSubSystem.UnloadButtonImage( iNumberPadButtonsImages[ iCounter ] );
    			ButtonSubSystem.RemoveButton( iNumberPadButtons[ iCounter ] );
    		}

    		for( iCounter = OK_ATM; iCounter < NUMBER_ATM_BUTTONS ;iCounter++ )
    		{
    			ButtonSubSystem.RemoveButton( giPersonnelATMSideButton[ iCounter ]  );
    			ButtonSubSystem.UnloadButtonImage( giPersonnelATMSideButtonImage[ iCounter ] );
    		}

    		CreateDestroyATMButton_fCreated = false;
    	}

    	*/
    }

    private static void ATMStartButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    			fReDrawScreenFlag=true;
    			fShowAtmPanel = true;
    			fShowAtmPanelStartButton = false;
    			fATMFlags = 0;

    		}
    	}
    }

    private static void PersonnelINVStartButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		fReDrawScreenFlag=true;
        btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
    //		fShowInventory = TRUE;
    		gubPersonnelInfoState = PRSNL_INV;
    	}
    }

    private static void PersonnelStatStartButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{	
    		fReDrawScreenFlag=true;
        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);	
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);	
    //		fShowInventory = FALSE;
    		gubPersonnelInfoState = PRSNL_STATS;
    	}
    }

    private static void EmployementInfoButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{	
    		fReDrawScreenFlag=true;
        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);	
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ].uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);	
    		gubPersonnelInfoState = PRSNL_EMPLOYMENT;
    	}
    }

    private static void ATMOther2ButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int iValue = 0;
    	SOLDIERTYPE? pSoldier = MercPtrs[ 0 ];
    	int cnt = 0;
    	int iId = 0;

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;


    	iValue = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);
		
    		switch( iValue )
    		{
    			case( DEPOSIT_ATM ):
    				fATMFlags = 2;
    				fReDrawScreenFlag=true;
    				giPersonnelATMSideButton[ WIDTHDRAWL_ATM ].uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    			break;
    			case( WIDTHDRAWL_ATM ):
    				fATMFlags = 3;
    				fReDrawScreenFlag=true;
    				giPersonnelATMSideButton[ DEPOSIT_ATM ].uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    			break;
    		}
    	}

    }

    private static void ATMOtherButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int iValue = 0;
    	SOLDIERTYPE? pSoldier = MercPtrs[ 0 ];
    	int cnt = 0;
    	int iId = 0;

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;


    	iValue = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);

    			if( iCurrentPersonSelectedId != -1 )
    			{
    				if( fCurrentTeamMode == true )
    				{
    					iId = GetIdOfThisSlot( iCurrentPersonSelectedId );
    					cnt = 0;

    					// set soldier
    					pSoldier = MercPtrs[ iId ];


    					switch( iValue )
    					{
    						case( OK_ATM ):
    							if( fATMFlags == 0 )
    							{
    								fATMFlags = 1;
    								fReDrawScreenFlag=true;
    								fOneFrameDelayInPersonnel = true;
    							}
    							else if( fATMFlags == 2 )
    							{
    								// deposit from merc to account
    								if( GetFundsOnMerc( pSoldier ) >= wcstol( sTransferString, null, 10 ) )  // TODO(port): string function
    								{
    									if( ( wcstol( sTransferString, null, 10 ) % 10 ) != 0 )  // TODO(port): string function
    									{
    										fOldATMFlags = fATMFlags;
    										fATMFlags = 5;

    										iValue = ( wcstol( sTransferString, null, 10 ) - ( wcstol( sTransferString, null, 10 ) % 10 ) );  // TODO(port): string function
    										sTransferString = wprintf( "%d", iValue );
    										fReDrawScreenFlag=true;
    									}
    									else
    									{
    										// transfer
    										TransferFundsFromMercToBank( pSoldier, wcstol( sTransferString, null, 10 ) );  // TODO(port): string function
    										sTransferString = string.Empty;
    										fReDrawScreenFlag=true;
    									}
    								}
    								else
    								{
    									fOldATMFlags = fATMFlags;
    									fATMFlags = 4;
    									iValue = GetFundsOnMerc( pSoldier );
    									sTransferString = wprintf( "%d", iValue );
    									fReDrawScreenFlag=true;
    								}
    							}
    							else if( fATMFlags == 3 )
    							{
    								// deposit from merc to account
    								if( LaptopSaveInfo.iCurrentBalance >= wcstol( sTransferString, null, 10 ) )  // TODO(port): string function
    								{
    									if( ( wcstol( sTransferString, null, 10 ) % 10 ) != 0 )  // TODO(port): string function
    									{
    										fOldATMFlags = fATMFlags;
    										fATMFlags = 5;

    										iValue = ( wcstol( sTransferString, null, 10 ) - ( wcstol( sTransferString, null, 10 ) % 10 ) );  // TODO(port): string function
    										sTransferString = wprintf( "%d", iValue );
    										fReDrawScreenFlag=true;
    									}
    									else
    									{
    										// transfer
    										TransferFundsFromBankToMerc( pSoldier, wcstol( sTransferString, null, 10 ) );  // TODO(port): string function
    										sTransferString = string.Empty;
    										fReDrawScreenFlag=true;
    									}
    								}
    								else
    								{
    									fOldATMFlags = fATMFlags;
    									fATMFlags = 4;
    									iValue = LaptopSaveInfo.iCurrentBalance;
    									sTransferString = wprintf( "%d", iValue );
    									fReDrawScreenFlag=true;
    								}
    							}
    							else if( fATMFlags == 4 )
    							{
    								fATMFlags = fOldATMFlags;
    								fReDrawScreenFlag=true;
    							}
    						break;
    						case( DEPOSIT_ATM ):
    							fATMFlags = 2;
    							fReDrawScreenFlag=true;

    						break;
    						case( WIDTHDRAWL_ATM ):
    							fATMFlags = 3;
    							fReDrawScreenFlag=true;
    						break;
    						case( CANCEL_ATM ):
    							if( sTransferString[ 0 ] != 0 )
    							{
    								sTransferString = string.Empty;
    							}
    							else if( fATMFlags != 0 )
    							{
    								fATMFlags = 0;
    								giPersonnelATMSideButton[ WIDTHDRAWL_ATM ].uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    								giPersonnelATMSideButton[ DEPOSIT_ATM ].uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    							}
    							else
    							{
    								fShowAtmPanel = false;
    								fShowAtmPanelStartButton = true;
								
    							}
    							fReDrawScreenFlag=true;
    						break;
    						case( CLEAR_ATM ):
    							sTransferString = string.Empty;
    							fReDrawScreenFlag=true;
    						break;
    					}
    				}
    			}
    		}
    	}
    }

    private static void ATMNumberButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

    	int iValue = 0;
    	int iCounter = 0;
    	string sZero = "0";

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	iValue = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		if(!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
    		{
    			fReDrawScreenFlag=true;
    		}
        btn.uiFlags|=(ButtonFlags.BUTTON_CLICKED_ON);   
    	}
    	else if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			btn.uiFlags&=~(ButtonFlags.BUTTON_CLICKED_ON);
    			// find position in value string, append character at end
    			for( iCounter = 0; iCounter < ( int )wcslen( sTransferString ) ; iCounter++ );  // TODO(port): string function
    			sTransferString = sTransferString.Substring(0, Math.Min(iCounter, sTransferString.Length)) + (char)( sZero[ 0 ] + iValue );
    			fReDrawScreenFlag=true;
			
    			// gone too far
    			if( FontSubSystem.StringPixLength( sTransferString, ATM_FONT ) >= ATM_DISPLAY_WIDTH - 10 )
    			{
    				sTransferString = sTransferString.Substring(0, Math.Min(iCounter, sTransferString.Length));
    			}
    		}
    	}
    }

    private static void DisplayATMAmount()
    {
    	if( fShowAtmPanel == false )
    	{
    		return;
    	}

    	// the typed digits (without leading zeros), as a dollar figure
    	int.TryParse( sTransferString, out int iAmount );
    	string sTempString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( iAmount ) );

    	// set font
    	FontSubSystem.SetFont( ATM_FONT );

    	// set back and foreground
    	FontSubSystem.SetFontForeground( FontColor.FONT_WHITE );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );

    	// right justify
    	FontSubSystem.FindFontRightCoordinates( ATM_DISPLAY_X, ATM_DISPLAY_Y + 37, ATM_DISPLAY_WIDTH, ATM_DISPLAY_HEIGHT, sTempString, ATM_FONT, out int sX, out int sY );

    	// print string
    	mprintf( sX, sY, sTempString );
    }

    private static void HandleStateOfATMButtons()
    {
    	int iCounter = 0;

    	// disable buttons based on state
    	if( ( fATMFlags == 0 ) )
    	{
    		for( iCounter = 0; iCounter < 10 ; iCounter++ )
    		{
    			ButtonSubSystem.DisableButton( iNumberPadButtons[ iCounter ] );
    		}

    		for( iCounter = 0; iCounter < NUMBER_ATM_BUTTONS; iCounter++ )
    		{
    			if( ( iCounter != DEPOSIT_ATM) && ( iCounter != WIDTHDRAWL_ATM ) && ( iCounter != CANCEL_ATM )  )
    			{
    				ButtonSubSystem.DisableButton( giPersonnelATMSideButton[ iCounter ] );
    			}
    		}
    	}
    	else
    	{
    		for( iCounter = 0; iCounter < 10; iCounter++ )
    		{
    			ButtonSubSystem.EnableButton( iNumberPadButtons[ iCounter ] );
    		}

    		for( iCounter = 0; iCounter < NUMBER_ATM_BUTTONS; iCounter++ )
    		{
    			ButtonSubSystem.EnableButton( giPersonnelATMSideButton[ iCounter ] );
    		}
    	}
    }

    private static int GetFundsOnMerc(SOLDIERTYPE? pSoldier)
    {
    	int iCurrentAmount = 0;
    	InventorySlot iCurrentPocket = 0;
    	// run through mercs pockets, if any money in them, add to total

    	// error check
    	if( pSoldier == null )
    	{
    		return 0;
    	}

    	// run through grunts pockets and count all the spare change
    	for( iCurrentPocket = 0; iCurrentPocket < NUM_INV_SLOTS; iCurrentPocket++ )
    	{
    		if ( Item[ pSoldier.inv[ iCurrentPocket ] .usItem ].usItemClass == IC.MONEY )
    		{
    			iCurrentAmount += pSoldier.inv[ iCurrentPocket ].uiMoneyAmount; 
    		}
    	}

    	return iCurrentAmount;
    }

    private static bool TransferFundsFromMercToBank(SOLDIERTYPE? pSoldier, int iCurrentBalance)
    {
    	InventorySlot iCurrentPocket = 0;
    	int iAmountLeftToTake = iCurrentBalance;
    	OBJECTTYPE ObjectToRemove = new();


    	// move this amount of money from the grunt to the bank
    	// error check
    	if( pSoldier == null )
    	{
    		return false;
    	}

    	// run through grunts pockets and count all the spare change
    	for( iCurrentPocket = 0; iCurrentPocket < NUM_INV_SLOTS; iCurrentPocket++ )
    	{
    		if ( Item[ pSoldier.inv[ iCurrentPocket ] .usItem ].usItemClass == IC.MONEY )
    		{

    			// is there more left to go, or does this pocket finish it off?
    			if( pSoldier.inv[ iCurrentPocket ].uiMoneyAmount > ( int )iAmountLeftToTake )
    			{
    				pSoldier.inv[ iCurrentPocket ].uiMoneyAmount -= iAmountLeftToTake;
    				iAmountLeftToTake = 0;
    			}
    			else
    			{
    				iAmountLeftToTake  -= pSoldier.inv[ iCurrentPocket ].uiMoneyAmount;  
    				pSoldier.inv[ iCurrentPocket ].uiMoneyAmount = 0;

    				//Remove the item out off the merc
    				ItemSubSystem.RemoveObjectFromSlot( pSoldier, iCurrentPocket, ObjectToRemove );
    			}
    		}	
    	}

    	if( iAmountLeftToTake != 0 )
    	{
    		// something wrong
    		finances.AddTransactionToPlayersBook ( FinanceEvent.TRANSFER_FUNDS_FROM_MERC, 	pSoldier.ubProfile, GameClock.GetWorldTotalMin() , ( iCurrentBalance - iAmountLeftToTake ) );
    		return ( false );
    	}
    	else
    	{
    		// everything ok
    		finances.AddTransactionToPlayersBook ( FinanceEvent.TRANSFER_FUNDS_FROM_MERC, 	pSoldier.ubProfile, GameClock.GetWorldTotalMin() , ( iCurrentBalance ) );
    		return ( true );
    	}
    }

    private static bool TransferFundsFromBankToMerc(SOLDIERTYPE? pSoldier, int iCurrentBalance)
    {
    	OBJECTTYPE pMoneyObject = new();

    	// move this amount of money from the grunt to the bank
    	// error check
    	if( pSoldier == null )
    	{
    		return false;
    	}

    	// make sure we are giving them some money
    	if( iCurrentBalance <= 0 )
    	{
    		return ( false );
    	}

    	// current balance
    	if( iCurrentBalance > LaptopSaveInfo.iCurrentBalance )
    	{
    		iCurrentBalance = LaptopSaveInfo.iCurrentBalance;
    	}


    	// set up object
    	// pMoneyObject starts out zeroed

    	// set up money object
    	pMoneyObject.usItem = Items.MONEY;
    	pMoneyObject.ubNumberOfObjects = 1;
    	pMoneyObject.bMoneyStatus = 100;
    	pMoneyObject.	bStatus[0] = 100;	
    	pMoneyObject.uiMoneyAmount = iCurrentBalance;
	

    	// now auto place money object
    	if( ItemSubSystem.AutoPlaceObject( pSoldier, ( pMoneyObject ), true ) == true )
    	{
    		// now place transaction
    		finances.AddTransactionToPlayersBook ( FinanceEvent.TRANSFER_FUNDS_TO_MERC, pSoldier.ubProfile, GameClock.GetWorldTotalMin() , -( iCurrentBalance ) );
    	}
    	else
    	{
    		// error, notify player that merc doesn't have the spce for this much cash
    	}

    	return( true );
    }

    private static void DisplayATMStrings()
    {
    	// display strings for ATM

    	switch( fATMFlags )
    	{
    		case( 0 ):
    			if( fShowAtmPanelStartButton == false )
    			{
    				FontSubSystem.DisplayWrappedString(new(509, ( int )( 80 )), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 3 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    			}
    		break;
    		case( 2 ):
    			if( sTransferString[ 0 ] != 0 )
    			{
    				FontSubSystem.DisplayWrappedString(new(509, 80), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 0 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    				//FontSubSystem.DisplayWrappedString(new(509, ( INT16 )( 80 + GetFontHeight( ATM_FONT ) )), 81, 2, ATM_FONT, FONT_WHITE, sATMText[ 1 ], FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    			}
    			else
    			{
    				FontSubSystem.DisplayWrappedString(new(509, 80), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 2 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    			}
    		break;
    		case( 3 ):
    			if( sTransferString[ 0 ] != 0 )
    			{
    				FontSubSystem.DisplayWrappedString(new(509, 80), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 0 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    				//FontSubSystem.DisplayWrappedString(new(509, ( INT16 )( 80 + GetFontHeight( ATM_FONT ) )), 81, 2, ATM_FONT, FONT_WHITE, sATMText[ 1 ], FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    			}
    			else
    			{
    				FontSubSystem.DisplayWrappedString(new(509, 80), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 2 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    			}
    		break;
    		case( 4 ):
    			// not enough money
    			FontSubSystem.DisplayWrappedString(new(509, 80), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 4 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		break;
    		case( 5 ):
    			// not enough money
    			FontSubSystem.DisplayWrappedString(new(509, 73), 81, 2, ATM_FONT, FontColor.FONT_WHITE, sATMText[ 5 ], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		break;

    	}
    }

    private static void UpDateStateOfStartButton()
    {
    	int iId = 0;

    	// start button being shown?
    	if( fShowAtmPanelStartButton == false )
    	{
    		return;
    	}

    //	if( fShowInventory == TRUE )
    	if( gubPersonnelInfoState == PRSNL_INV )
    	{
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    	}
    	else if( gubPersonnelInfoState == PRSNL_STATS )
    	{
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    	}
    	else
    	{
    		giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
    		giPersonnelATMStartButton[ PERSONNEL_INV_BTN ].uiFlags &= ~( ButtonFlags.BUTTON_CLICKED_ON );
    		giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    	}

    	// if in current mercs and the currently selected guy is valid, enable button, else disable it
    	if( fCurrentTeamMode == true )
    	{
    		// is the current guy valid
    		if( GetNumberOfMercsDeadOrAliveOnPlayersTeam( ) > 0 )
    		{
    			//EnableButton( giPersonnelATMStartButton[ 0 ] );
    			ButtonSubSystem.EnableButton( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] );
    			ButtonSubSystem.EnableButton( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] );
    			ButtonSubSystem.EnableButton( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] );

    			iId = GetIdOfThisSlot( iCurrentPersonSelectedId );

    			if( iId != -1 )
    			{
    				if( Menptr[ iId ].bAssignment == Assignment.ASSIGNMENT_POW )
    				{
    					//DisableButton( giPersonnelATMStartButton[ 0 ] );
    					ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] );

    //					if( fShowInventory == TRUE )
    					if( gubPersonnelInfoState == PRSNL_INV )
    					{
    //						fShowInventory = FALSE;
    						gubPersonnelInfoState = PRSNL_STATS;

    						fPausedReDrawScreenFlag = true;
    					}

    					if( fATMFlags != 0 )
    					{
    						fATMFlags = 0;
    						fPausedReDrawScreenFlag = true;
    					}
    				}
    			}

			
    		}
    		else
    		{
    			// not valid, disable
    			//DisableButton( giPersonnelATMStartButton[ 0 ] );
    			ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] );
    			ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] );
    			ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] );
    		}

		
    	}
    	else
    	{
    		// disable button
    		//DisableButton( giPersonnelATMStartButton[ 0 ] );
    		ButtonSubSystem.EnableButton( giPersonnelATMStartButton[ PERSONNEL_STAT_BTN ] );
    		ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_INV_BTN ] );
    		ButtonSubSystem.DisableButton( giPersonnelATMStartButton[ PERSONNEL_EMPLOYMENT_BTN ] );
    	}
    }

    private static void DisplayAmountOnCurrentMerc()
    {
    	// will display the amount that the merc is carrying on him or herself
    	int iId;
    	SOLDIERTYPE? pSoldier = null;
    	int iFunds;
    	string sString = string.Empty;
    	int sX, sY;

    	iId = GetIdOfThisSlot( iCurrentPersonSelectedId );

    	if( iId == -1 )
    	{
    		pSoldier = null;
    	}
    	else
    	{
    		// set soldier
    		pSoldier = MercPtrs[ iId ];
    	}

    	iFunds = GetFundsOnMerc( pSoldier ); 

    	sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( iFunds  ) );
		 
    	// set font
    	FontSubSystem.SetFont( ATM_FONT );

    	// set back and foreground
    	FontSubSystem.SetFontForeground( FontColor.FONT_WHITE );
    	FontSubSystem.SetFontBackground( FontColor.FONT_BLACK );

    	// right justify
    	FontSubSystem.FindFontRightCoordinates( ATM_DISPLAY_X, ATM_DISPLAY_Y, ATM_DISPLAY_WIDTH, ATM_DISPLAY_HEIGHT, sString, ATM_FONT, out sX, out sY );

    	// print string
    	mprintf( sX, sY, sString );

    	return;
    }

    private static void HandlePersonnelKeyboard()
    {
    	// TODO(port): typing an amount into the ATM with the keyboard (needs the input event queue)
    }

    private static void RenderRectangleForPersonnelTransactionAmount()
    {
    	// TODO(port): the yellow box drawn around the amount being typed (needs a rectangle drawing routine)
    }

    // was a function-local static of HandleTimedAtmModes
    private static int HandleTimedAtmModes_fOldAtmMode = 0;
    // was a function-local static of HandleTimedAtmModes
    private static int HandleTimedAtmModes_uiBaseTime = 0;
    private static void HandleTimedAtmModes()
    {



    	if( fShowAtmPanel == false )
    	{
    		return;
    	}

    	// update based on modes
    	if( fATMFlags != HandleTimedAtmModes_fOldAtmMode )
    	{
    		HandleTimedAtmModes_uiBaseTime = (int)GetJA2Clock();
    		HandleTimedAtmModes_fOldAtmMode = fATMFlags;
    		fPausedReDrawScreenFlag = true;
     	}


    	if( ( (int)GetJA2Clock() - HandleTimedAtmModes_uiBaseTime ) > DELAY_PER_MODE_CHANGE_IN_ATM )
    	{
    		switch( fATMFlags )
    		{
    			case( 4 ):
    			case( 5 ):
    				// insufficient funds ended
    				fATMFlags = fOldATMFlags;
    				fPausedReDrawScreenFlag = true;
    			break;
    		}
    	}
    }

    private static bool IsPastMercDead(int iId)
    {
    	if( GetTheStateOfDepartedMerc( GetIdOfPastMercInSlot( iId ) ) == DEPARTED_DEAD )
    	{
    		return( true );
    		}
    	else
    	{
    		return( false );
    	}

    }

    private static bool IsPastMercFired(int iId)
    {
    	if( GetTheStateOfDepartedMerc( GetIdOfPastMercInSlot( iId )  ) == DEPARTED_FIRED )
    	{
    		return( true );
    		}
    	else
    	{
    		return( false );
    	}
    }

    private static bool IsPastMercOther(int iId)
    {
    	if( GetTheStateOfDepartedMerc( GetIdOfPastMercInSlot( iId )  ) == DEPARTED_OTHER )
    	{
    		return( true );
    		}
    	else
    	{
    		return( false );
    	}
    }

    private static void DisplayEmploymentinformation(int iId, int iSlot)
    {
    	int iCounter=0;
    	string sString = string.Empty;
    	string sStringA = string.Empty;
    	int sX, sY;
    	int uiHits = 0;
	
	
    	if( Menptr[iId].uiStatusFlags.HasFlag(SOLDIER.VEHICLE) )
    	{
    		return;
    	}

    	// display the stats for a char
    	for(iCounter=0;iCounter <MAX_STATS; iCounter++)
    	{
    		switch(iCounter)
    		{

    //		 case 12:

    		//Remaining Contract:
    		case 0:
    		{
    			int uiTimeUnderThisDisplayAsHours = 24*60;
    			int uiMinutesInDay = 24 * 60;

    				if(Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC || Menptr[iId].ubProfile == NPCID.SLAY )
    				{
    					int iTimeLeftOnContract = CalcTimeLeftOnMercContract( Menptr[iId] );

    					//if the merc is in transit
    					if( Menptr[iId ].bAssignment == Assignment.IN_TRANSIT )
    					{
    						//and if the ttime left on the cotract is greater then the contract time
    						if( iTimeLeftOnContract > (int)( Menptr[iId].iTotalContractLength * uiMinutesInDay ) )
    						{
    							iTimeLeftOnContract = ( Menptr[iId].iTotalContractLength * uiMinutesInDay );
    						}
    					}
    					// if there is going to be a both days and hours left on the contract
    					if( iTimeLeftOnContract / uiMinutesInDay != 0 )
    					{
    						sString = wprintf( "%d%s %d%s / %d%s",( iTimeLeftOnContract / uiMinutesInDay ), "d", (iTimeLeftOnContract % uiMinutesInDay)/60, "h", Menptr[iId].iTotalContractLength, "d");
    						mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_CURRENT_CONTRACT]);		
    					}

    					//else there is under a day left
    					else
    					{
    						//DEF: removed 2/7/99
    						sString = wprintf( "%d%s / %d%s", (iTimeLeftOnContract % uiMinutesInDay)/60, "h", Menptr[iId].iTotalContractLength, "d");
    						mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_CURRENT_CONTRACT]);		
    					}

    				}
    				else if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    				{
    //					swprintf(sString, L"%d%s / %d%s",Menptr[iId].iTotalContractLength, "d", ( GetWorldTotalMin( ) -Menptr[iId].iStartContractTime ) / ( 24 * 60 ), "d" );

    					sString = "N/A";
    					mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_CURRENT_CONTRACT]);
    				}
    				else
    				{
    					sString = "N/A";
    					mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_CURRENT_CONTRACT]);
    				}

    		   FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
           mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);		 
    		}
    		break;

    //		 case 11:
    //		 case 19:
    		 case 1:

    			 // total contract time served
    			mprintf((int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter].Y,pPersonnelScreenStrings[PRSNL_TXT_TOTAL_SERVICE]);		 

    			//./DEF 2/4/99: total service days used to be calced as 'days -1'

    			sString = wprintf( "%d %s",gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].usTotalDaysServed, "d" );

          FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
          mprintf(sX,pPersonnelScreenPoints[iCounter].Y,sString);
    		 break;

    //		 case 13:
    		 case 3:
    		   // cost (PRSNL_TXT_TOTAL_COST)

    /*
    			 if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    			 {
    				 int uiDailyCost = 0;

    				 if( Menptr[iId].bTypeOfLastContract == CONTRACT_EXTEND_2_WEEK )
    				 {
    					 // 2 week contract
    					 uiDailyCost = gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiBiWeeklySalary / 14;
    				 }
    				 else if( Menptr[iId].bTypeOfLastContract == CONTRACT_EXTEND_1_WEEK )
    				 {
    					 // 1 week contract
    					 uiDailyCost = gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiWeeklySalary / 7;
    				 }
    				 else
    				 {
    					 uiDailyCost = gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary;
    				 }

    //				 swprintf( sString, L"%d",uiDailyCost * Menptr[ iId ].iTotalContractLength );
    				 sString = wprintf( "%d", gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiTotalCostToDate );
    			 }
    			 else if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    			 {
    //					swprintf( sString, L"%d",gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary * gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].iMercMercContractLength );
    					sString = wprintf( "%d", gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiTotalCostToDate );
    			 }
    			 else
    			 {
    				 //Display a $0 amount
    //				 swprintf( sString, L"0" );

    				 sString = wprintf( "%d", gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiTotalCostToDate );
    			 }
    */
    				sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiTotalCostToDate  ) );

    /*
    DEF:3/19/99:
    			 if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.MERC )
    			 {
    			   sStringA = wprintf( "%s", pPersonnelScreenStrings[ PRSNL_TXT_UNPAID_AMOUNT ] );
    			 }
    			 else
    */
    			 {
    				 sStringA = wprintf( "%s", pPersonnelScreenStrings[ PRSNL_TXT_TOTAL_COST ]  );
    			 }

    			 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    			 mprintf( (int)(pPersonnelScreenPoints[iCounter].X +(iSlot*TEXT_BOX_WIDTH) ),pPersonnelScreenPoints[ iCounter ].Y,sStringA);

    			 // print contract cost
    			 mprintf( ( int ) ( sX ) , pPersonnelScreenPoints[iCounter].Y,sString);
     	 
    			 if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    			 {
    				 // daily rate
    				 if( Menptr[iId].bTypeOfLastContract == CONTRACT_EXTEND_2_WEEK )
    				 {
    					 // 2 week contract
    					 sStringA = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiBiWeeklySalary / 14  ) );
    					 sString = wprintf( "%s", sStringA );
    				 }
    				 else if( Menptr[iId].bTypeOfLastContract == CONTRACT_EXTEND_1_WEEK )
    				 {
    					 // 1 week contract
    					 sStringA = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].uiWeeklySalary / 7  ) );
    					 sString = wprintf( "%s",  sStringA );
    				 }
    				 else
    				 {

    					 sStringA = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary  ) );
    					 sString = wprintf( "%s", sStringA );
    				 }
    			 }
    			 else if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    			 {
    //DEF: 99/2/7
    //				 swprintf( sStringA, L"%d", gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary * Menptr[ iId ].iTotalContractLength);
    				 sStringA = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary  ) );
    				 sString = wprintf( "%s", sStringA );
    			 }

    			 else
    			 {
    				 //Display a $0 amount
    //				 swprintf( sString, L"0" );
    //				 InsertDollarSignInToString( sString );
    				 sStringA = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary  ) );
    				 sString = wprintf( "%s", sStringA );
    			 }

    			 FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );

    //			 iCounter++;
    			 iCounter++;

           // now print daily rate
    			 mprintf( ( int )( sX ),pPersonnelScreenPoints[ iCounter+1 ].Y,sString);
    			 mprintf( (int)(pPersonnelScreenPoints[iCounter+1].X +(iSlot*TEXT_BOX_WIDTH) ),pPersonnelScreenPoints[ iCounter +1].Y, pPersonnelScreenStrings[PRSNL_TXT_DAILY_COST]);
			
    			 break;

    		 case 5:
    		   // medical deposit		 

    			 //if its a merc merc, display the salary oweing
    			if( Menptr[iId].ubWhatKindOfMercAmI == MERC_TYPE.MERC )
    			{
    				mprintf((int)(pPersonnelScreenPoints[iCounter-1].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter-1].Y,pPersonnelScreenStrings[PRSNL_TXT_UNPAID_AMOUNT]);

    				sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].sSalary * gMercProfiles[(NPCID)(Menptr[ iId ].ubProfile)].iMercMercContractLength  ) );

    				FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter-1].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    				mprintf(sX,pPersonnelScreenPoints[iCounter-1].Y,sString);
    			}
    			else
    			{
    				mprintf((int)(pPersonnelScreenPoints[iCounter-1].X+(iSlot*TEXT_BOX_WIDTH)),pPersonnelScreenPoints[iCounter-1].Y,pPersonnelScreenStrings[PRSNL_TXT_MED_DEPOSIT]);		

    				sString = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(Menptr[iId].ubProfile)].sMedicalDepositAmount ) );

    				FontSubSystem.FindFontRightCoordinates( (int)(pPersonnelScreenPoints[iCounter-1].X+(iSlot*TEXT_BOX_WIDTH)+Prsnl_DATA_OffSetX), 0, TEXT_BOX_WIDTH-20, 0, sString, PERS_FONT, out sX, out sY );
    				mprintf(sX,pPersonnelScreenPoints[iCounter-1].Y,sString);
    			}

				 
    		 break;

    		}
    	}
    }

    private static int CalcTimeLeftOnMercContract(SOLDIERTYPE? pSoldier)
    {
    	int iTimeLeftOnContract = -1;

    	if(pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC)
    	{
    		iTimeLeftOnContract = (int)(pSoldier.iEndofContractTime-GameClock.GetWorldTotalMin());

    		if( iTimeLeftOnContract < 0 )
    			iTimeLeftOnContract = 0;
    	}
    	else if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.MERC)
    	{
    		iTimeLeftOnContract = gMercProfiles[(NPCID)(pSoldier.ubProfile)].iMercMercContractLength;
    	}

    	else if( pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.PLAYER_CHARACTER )
    	{
    		iTimeLeftOnContract = pSoldier.iTotalContractLength;
    	}

    	else
    	{
    		iTimeLeftOnContract = -1;
    	}

    	return( iTimeLeftOnContract );
    }
}
