using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimArchives.c: the A.I.M. alumni gallery (three pages of retired mercs, with a pop-up for each).
public partial class Laptop
{
    private const string AIM_ALUMNI_NAME_FILE = "BINARYDATA\\AlumName.edt";
    private const string AIM_ALUMNI_FILE = "BINARYDATA\\Alumni.edt";

    private const FontStyle AIM_ALUMNI_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_ALUMNI_TITLE_COLOR = AIM_GREEN;
    private const FontStyle AIM_ALUMNI_POPUP_FONT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_ALUMNI_POPUP_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_ALUMNI_POPUP_NAME_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_ALUMNI_POPUP_NAME_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_ALUMNI_NAME_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_ALUMNI_NAME_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_ALUMNI_PAGE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_ALUMNI_PAGE_COLOR_UP = FontColor.FONT_MCOLOR_DKWHITE;
    private const FontColor AIM_ALUMNI_PAGE_COLOR_DOWN = (FontColor)138;

    private const int AIM_ALUMNI_NAME_LINESIZE = 80 * 2;
    private const int AIM_ALUMNI_ALUMNI_LINESIZE = 7 * 80 * 2;

    private const int AIM_ALUMNI_NUM_FACE_COLS = 5;
    private const int AIM_ALUMNI_NUM_FACE_ROWS = 4;
    private const int MAX_NUMBER_OLD_MERCS_ON_PAGE = AIM_ALUMNI_NUM_FACE_ROWS * AIM_ALUMNI_NUM_FACE_COLS;

    private const int AIM_ALUMNI_START_GRID_X = LAPTOP_SCREEN_UL_X + 37;
    private const int AIM_ALUMNI_START_GRID_Y = LAPTOP_SCREEN_WEB_UL_Y + 68;
    private const int AIM_ALUMNI_GRID_OFFSET_X = 90;
    private const int AIM_ALUMNI_GRID_OFFSET_Y = 72;

    private const int AIM_ALUMNI_ALUMNI_FRAME_WIDTH = 66;
    private const int AIM_ALUMNI_ALUMNI_FRAME_HEIGHT = 64;
    private const int AIM_ALUMNI_ALUMNI_FACE_WIDTH = 56;
    private const int AIM_ALUMNI_ALUMNI_FACE_HEIGHT = 50;

    private const int AIM_ALUMNI_NAME_OFFSET_X = 5;
    private const int AIM_ALUMNI_NAME_OFFSET_Y = 55;
    private const int AIM_ALUMNI_NAME_WIDTH = AIM_ALUMNI_ALUMNI_FRAME_WIDTH - AIM_ALUMNI_NAME_OFFSET_X * 2;

    private const int AIM_ALUMNI_PAGE1_X = LAPTOP_SCREEN_UL_X + 100;
    private const int AIM_ALUMNI_PAGE1_Y = LAPTOP_SCREEN_WEB_UL_Y + 357;
    private const int AIM_ALUMNI_PAGE_GAP = BOTTOM_BUTTON_START_WIDTH + 25;

    private const int AIM_ALUMNI_PAGE_END_X = AIM_ALUMNI_PAGE1_X + (BOTTOM_BUTTON_START_WIDTH + BOTTOM_BUTTON_START_WIDTH) * 3;
    private const int AIM_ALUMNI_PAGE_END_Y = AIM_ALUMNI_PAGE1_Y + BOTTOM_BUTTON_START_HEIGHT;

    private const int AIM_ALUMNI_TITLE_X = IMAGE_OFFSET_X + 149;
    private const int AIM_ALUMNI_TITLE_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT;
    private const int AIM_ALUMNI_TITLE_WIDTH = AIM_SYMBOL_WIDTH;

    private const int AIM_POPUP_WIDTH = 309;
    private const int AIM_POPUP_TEXT_WIDTH = 296;
    private const int AIM_POPUP_SECTION_HEIGHT = 9;

    private const int AIM_POPUP_X = LAPTOP_SCREEN_UL_X + (500 - AIM_POPUP_WIDTH) / 2;
    private const int AIM_POPUP_Y = 120 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_POPUP_SHADOW_GAP = 4;
    private const int AIM_POPUP_TEXT_X = AIM_POPUP_X;

    private const int AIM_ALUMNI_FACE_PANEL_X = AIM_POPUP_X + 6;
    private const int AIM_ALUMNI_FACE_PANEL_Y = AIM_POPUP_Y + 6;
    private const int AIM_ALUMNI_FACE_PANEL_WIDTH = 58;
    private const int AIM_ALUMNI_FACE_PANEL_HEIGHT = 52;

    private const int AIM_ALUMNI_POPUP_NAME_X = AIM_ALUMNI_FACE_PANEL_X + AIM_ALUMNI_FACE_PANEL_WIDTH + 10;
    private const int AIM_ALUMNI_POPUP_NAME_Y = AIM_ALUMNI_FACE_PANEL_Y + 20;

    private const int AIM_ALUMNI_POPUP_DESC_X = AIM_POPUP_X + 8;
    private const int AIM_ALUMNI_POPUP_DESC_Y = AIM_ALUMNI_FACE_PANEL_Y + AIM_ALUMNI_FACE_PANEL_HEIGHT + 5;

    private const int AIM_ALUMNI_DONE_WIDTH = 36;
    private const int AIM_ALUMNI_DONE_HEIGHT = 16;
    private const int AIM_ALUMNI_DONE_X = AIM_POPUP_X + AIM_POPUP_WIDTH - AIM_ALUMNI_DONE_WIDTH - 7;

    private const int AIM_ALUMNI_NAME_SIZE = 80 * 2;
    private const int AIM_ALUMNI_DECRIPTION_SIZE = 80 * 7 * 2;
    private const int AIM_ALUMNI_FILE_RECORD_SIZE = 80 * 8 * 2;
    private const int AIM_ALUMNI_FULL_NAME_SIZE = 80 * 2;

    // indices into AimAlumniText
    private const int AIM_ALUMNI_ALUMNI = 3;
    private const int AIM_ALUMNI_DONE = 4;

    private static HVOBJECT guiAlumniFrame = null!;
    private static HVOBJECT guiOldAim = null!;
    private static HVOBJECT guiPageButtons = null!;
    private static HVOBJECT guiAlumniPopUp = null!;
    private static HVOBJECT guiPopUpPic = null!;
    private static HVOBJECT guiDoneButton = null!;

    private static int gubPageNum;
    private static int gunAlumniButtonDown = 255;
    private static bool gfExitingAimArchives;
    private static int gubDrawOldMerc;
    private static bool gfDrawPopUpBox = false;
    private static bool gfDestroyPopUpBox;
    private static bool gfFaceMouseRegionsActive;
    private static bool gfReDrawScreen = false;

    private static readonly MOUSE_REGION[] gMercAlumniFaceMouseRegions = CreateRegions(MAX_NUMBER_OLD_MERCS_ON_PAGE + 1, nameof(gMercAlumniFaceMouseRegions));
    private static MOUSE_REGION gDoneRegion = new(nameof(gDoneRegion));

    private static readonly GUI_BUTTON[] guiAlumniPageButton = new GUI_BUTTON[3];
    private static ButtonPic guiAlumniPageButtonImage = null!;

    private static void GameInitAimArchives()
    {
    }

    private static void ShadowRect(int left, int top, int right, int bottom)
        => VideoSurfaceManager.ShadowVideoSurfaceRect(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(left, top, right, bottom));

    public static bool EnterAimArchives()
    {
          	int	usPosX, i;


    	gfExitingAimArchives = false;
    //	gubDrawOldMerc = 255;
    	gfDrawPopUpBox=false;
    	gfDestroyPopUpBox=false;

    	InitAimDefaults();
    	InitAimMenuBar();

    	gubPageNum = giCurrentSubPage;

    	// load the Alumni Frame and add it
    	guiAlumniFrame = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\AlumniFrame.sti"));

    	// load the 1st set of faces and add it
    	guiOldAim = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Old_Aim.sti"));

    	// load the Bottom Buttons graphic and add it
    	guiPageButtons = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\BottomButton.sti"));

    	// load the PopupPic graphic and add it
    	guiPopUpPic = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\PopupPicFrame.sti"));

    		// load the AlumniPopUp graphic and add it
    	guiAlumniPopUp = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\AlumniPopUp.sti"));

    		// load the Done Button graphic and add it
    	guiDoneButton = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\DoneButton.sti"));

    	InitAlumniFaceRegions();

    	//Load graphic for buttons
    	guiAlumniPageButtonImage =  ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1,0,-1,1,-1 );

    	usPosX = AIM_ALUMNI_PAGE1_X;
    	for(i=0; i<3; i++)
    	{
    		guiAlumniPageButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiAlumniPageButtonImage, AimAlumniText[i], AIM_ALUMNI_PAGE_FONT, AIM_ALUMNI_PAGE_COLOR_UP, FontShadow.DEFAULT_SHADOW, AIM_ALUMNI_PAGE_COLOR_DOWN, FontShadow.DEFAULT_SHADOW, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_ALUMNI_PAGE1_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnAlumniPageButtonCallback);
    		ButtonSubSystem.SetButtonCursor(guiAlumniPageButton[i], CURSOR.WWW);
    		ButtonSubSystem.MSYS_SetBtnUserData( guiAlumniPageButton[i], 0, i);
    		buttonList.Add(guiAlumniPageButton[i]);

    		usPosX += AIM_ALUMNI_PAGE_GAP;
    	}

    	DisableAimArchiveButton();
    	RenderAimArchives();
    	return(true);
    }

    public static void ExitAimArchives()
    {
    	int i;

    	gfExitingAimArchives = true;

    	video.DeleteVideoObject(guiAlumniFrame);
    	video.DeleteVideoObject(guiOldAim);
    	video.DeleteVideoObject(guiAlumniPopUp);
    	video.DeleteVideoObject(guiPopUpPic);
    	video.DeleteVideoObject(guiDoneButton);


    	RemoveAimAlumniFaceRegion();

    	ButtonSubSystem.UnloadButtonImage( guiAlumniPageButtonImage );
    	for(i=0; i<3; i++)
     		{
    			buttonList.Remove(guiAlumniPageButton[i]);
    			ButtonSubSystem.RemoveButton( guiAlumniPageButton[i] );
    		}

    	RemoveAimDefaults();
    	ExitAimMenuBar();
    	giCurrentSubPage = gubPageNum;

    	CreateDestroyDoneMouseRegion(0);
    	gfDestroyPopUpBox = false;
    	gfDrawPopUpBox = false;
    }

    public static void HandleAimArchives()
    {
    	if( gfReDrawScreen )
    	{
    //		RenderAimArchives();
    		fPausedReDrawScreenFlag = true;

    		gfReDrawScreen = false;
    	}
    	if( gfDestroyPopUpBox )
    	{
    		gfDestroyPopUpBox = false;

    		CreateDestroyDoneMouseRegion(0);
    		InitAlumniFaceRegions();
    		gfDestroyPopUpBox = false;
    	}
    }

    public static void RenderAimArchives()
    {
    	HVOBJECT	hFrameHandle;
    	HVOBJECT	hFaceHandle;
    //  HVOBJECT	hBottomButtonHandle;
    	int		usPosX, usPosY,x,y,i=0;
    	int			ubNumRows=0;
    	int			uiStartLoc=0;
    	string sText = string.Empty;

	
    	DrawAimDefaults();
    	DisableAimButton();

    	//Draw Link Title
    	FontSubSystem.DrawTextToScreen(AimAlumniText[AIM_ALUMNI_ALUMNI], new(AIM_ALUMNI_TITLE_X, AIM_ALUMNI_TITLE_Y), AIM_ALUMNI_TITLE_WIDTH, AIM_ALUMNI_TITLE_FONT, AIM_ALUMNI_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    	//Draw the mug shot border and face
    	hFrameHandle = guiAlumniFrame;
    	hFaceHandle = guiOldAim;

    	switch(gubPageNum)
    	{
    		case 0:
    			ubNumRows = AIM_ALUMNI_NUM_FACE_ROWS;
    			i=0;
    			break;
    		case 1:
    			ubNumRows = AIM_ALUMNI_NUM_FACE_ROWS;
    			i=20;
    			break;
    		case 2:
    			ubNumRows = 2;
    			i=40;
    			break;
    		default:
    			System.Diagnostics.Debug.Fail("invalid alumni page");
    			break;
    	}

    	usPosX = AIM_ALUMNI_START_GRID_X;
    	usPosY = AIM_ALUMNI_START_GRID_Y;
    	for(y=0; y<ubNumRows; y++)
    	{
    		for(x=0; x<AIM_ALUMNI_NUM_FACE_COLS; x++)
    		{
    			//Blt face to screen
    			video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, i,usPosX+4, usPosY+4, VO_BLT.SRCTRANSPARENCY);
	
    			//Blt the alumni frame background
    			video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFrameHandle, 0,usPosX, usPosY, VO_BLT.SRCTRANSPARENCY);

    			//Display the merc's name
    			uiStartLoc = AIM_ALUMNI_NAME_LINESIZE * i;
    			files.LoadEncryptedDataFromFile(AIM_ALUMNI_NAME_FILE, out sText, uiStartLoc, AIM_ALUMNI_NAME_SIZE);
    			FontSubSystem.DrawTextToScreen(sText, new((int)(usPosX + AIM_ALUMNI_NAME_OFFSET_X), (int)(usPosY + AIM_ALUMNI_NAME_OFFSET_Y)), AIM_ALUMNI_NAME_WIDTH, AIM_ALUMNI_NAME_FONT, AIM_ALUMNI_NAME_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
		
    			usPosX += AIM_ALUMNI_GRID_OFFSET_X;
    			i++;
    		}
    		usPosX = AIM_ALUMNI_START_GRID_X;
    		usPosY += AIM_ALUMNI_GRID_OFFSET_Y;
    	}

    	//the 3rd page now has an additional row with 1 merc on it, so add a new row
    	if( gubPageNum == 2 )
    	{
    		//Blt face to screen
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, i,usPosX+4, usPosY+4, VO_BLT.SRCTRANSPARENCY);

    		//Blt the alumni frame background
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFrameHandle, 0,usPosX, usPosY, VO_BLT.SRCTRANSPARENCY);

    		//Display the merc's name
    		uiStartLoc = AIM_ALUMNI_NAME_LINESIZE * i;
    		files.LoadEncryptedDataFromFile(AIM_ALUMNI_NAME_FILE, out sText, uiStartLoc, AIM_ALUMNI_NAME_SIZE);
    		FontSubSystem.DrawTextToScreen(sText, new((int)(usPosX + AIM_ALUMNI_NAME_OFFSET_X), (int)(usPosY + AIM_ALUMNI_NAME_OFFSET_Y)), AIM_ALUMNI_NAME_WIDTH, AIM_ALUMNI_NAME_FONT, AIM_ALUMNI_NAME_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);			
	
    		usPosX += AIM_ALUMNI_GRID_OFFSET_X;
    	}




    //	GetVideoObject( out hBottomButtonHandle, guiPageButtons );
    	usPosX = AIM_ALUMNI_PAGE1_X;

    	if( gfDrawPopUpBox )
    	{
    		DisplayAlumniOldMercPopUp();
    		RemoveAimAlumniFaceRegion();
    	}


      ButtonSubSystem.MarkButtonsDirty(buttonList);

    	RenderWWWProgramTitleBar( );
  
    	video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void SelectAlumniFaceRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    	{
    	}
    	else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    	{
    		gfDrawPopUpBox = true;
    		gfReDrawScreen = true;

    		gubDrawOldMerc = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );
    	}
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    	{
    	} 
    }

    private static void BtnAlumniPageButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int	ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

    		gunAlumniButtonDown=ubRetValue;
		
    		video.InvalidateRegion(AIM_ALUMNI_PAGE1_X,AIM_ALUMNI_PAGE1_Y, AIM_ALUMNI_PAGE_END_X,AIM_ALUMNI_PAGE_END_Y);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    			RemoveAimAlumniFaceRegion();

    			ChangingAimArchiveSubPage( ubRetValue );

    			gubPageNum = ubRetValue;
			
    			gfReDrawScreen = true;

    			gfDestroyPopUpBox = true;

    			gunAlumniButtonDown=255;
    			ResetAimArchiveButtons();
    			DisableAimArchiveButton();
    			gfDrawPopUpBox = false;

    			video.InvalidateRegion(AIM_ALUMNI_PAGE1_X,AIM_ALUMNI_PAGE1_Y, AIM_ALUMNI_PAGE_END_X,AIM_ALUMNI_PAGE_END_Y);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		gunAlumniButtonDown=255;
    		DisableAimArchiveButton();
    		video.InvalidateRegion(AIM_ALUMNI_PAGE1_X,AIM_ALUMNI_PAGE1_Y, AIM_ALUMNI_PAGE_END_X,AIM_ALUMNI_PAGE_END_Y);
    	}
    }

    private static void ResetAimArchiveButtons()
    {
    	int i=0;

    	for(i=0; i<3; i++)
    	{
    		guiAlumniPageButton[i].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON; 
    	}
    }

    private static void DisableAimArchiveButton()
    {
    	if( gfExitingAimArchives == true)
    		return;

    	if( (gubPageNum == 0 ) )
    	{
    		guiAlumniPageButton[ 0 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    	}
    	else if( gubPageNum == 1 )
    	{
    		guiAlumniPageButton[ 1 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    	}
    	else if( gubPageNum == 2 )
    	{
    		guiAlumniPageButton[ 2 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    	}
    }

    private static void DisplayAlumniOldMercPopUp()
    {
    	int			i,ubNumLines=11; //17
    	int		usPosY, usTextPosY;
    	int			ubFontHeight, ubNumDescLines;
    	HVOBJECT	hAlumniPopUpHandle;
    	HVOBJECT	hDoneHandle;
    	HVOBJECT	hFacePaneHandle;
    	HVOBJECT	hFaceHandle;
    //	WRAPPED_STRING *pFirstWrappedString, *pTempWrappedString;
    	int	usHeight = FontSubSystem.GetFontHeight(AIM_ALUMNI_POPUP_FONT);
    	string sName = string.Empty;
    	string sDesc = string.Empty;
    	int		uiStartLoc;
    	int	usStringPixLength;

    	hAlumniPopUpHandle = guiAlumniPopUp;
    	hDoneHandle = guiDoneButton;
    	hFacePaneHandle = guiPopUpPic;
    	hFaceHandle = guiOldAim;

    	ubFontHeight = (int)FontSubSystem.GetFontHeight(AIM_ALUMNI_POPUP_FONT);

    	//Load the description
    	uiStartLoc = AIM_ALUMNI_FILE_RECORD_SIZE * gubDrawOldMerc + AIM_ALUMNI_FULL_NAME_SIZE;
    	files.LoadEncryptedDataFromFile(AIM_ALUMNI_FILE, out sDesc, uiStartLoc, AIM_ALUMNI_DECRIPTION_SIZE);

    	usStringPixLength = FontSubSystem.StringPixLength( sDesc, AIM_ALUMNI_POPUP_FONT);
    	ubNumDescLines = (int) (usStringPixLength / AIM_POPUP_TEXT_WIDTH); 

    	ubNumLines += ubNumDescLines;

    	usTextPosY = AIM_POPUP_Y + 5;
    	usPosY = AIM_POPUP_Y;

    	//draw top line of the popup background
    	ShadowRect( AIM_POPUP_X+AIM_POPUP_SHADOW_GAP, usPosY+AIM_POPUP_SHADOW_GAP, AIM_POPUP_X + AIM_POPUP_WIDTH+AIM_POPUP_SHADOW_GAP, usPosY + AIM_POPUP_SECTION_HEIGHT+AIM_POPUP_SHADOW_GAP-1);
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAlumniPopUpHandle, 0,AIM_POPUP_X, usPosY, VO_BLT.SRCTRANSPARENCY);
    	//draw mid section of the popup background
    	usPosY += AIM_POPUP_SECTION_HEIGHT;
    	for(i=0; i<ubNumLines; i++)
    	{
    		ShadowRect( AIM_POPUP_X+AIM_POPUP_SHADOW_GAP, usPosY+AIM_POPUP_SHADOW_GAP, AIM_POPUP_X + AIM_POPUP_WIDTH+AIM_POPUP_SHADOW_GAP, usPosY + AIM_POPUP_SECTION_HEIGHT+AIM_POPUP_SHADOW_GAP-1);
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAlumniPopUpHandle, 1,AIM_POPUP_X, usPosY, VO_BLT.SRCTRANSPARENCY);
    		usPosY += AIM_POPUP_SECTION_HEIGHT;
    	}
    	//draw the bottom line and done button
    	ShadowRect( AIM_POPUP_X+AIM_POPUP_SHADOW_GAP, usPosY+AIM_POPUP_SHADOW_GAP, AIM_POPUP_X + AIM_POPUP_WIDTH+AIM_POPUP_SHADOW_GAP, usPosY + AIM_POPUP_SECTION_HEIGHT+AIM_POPUP_SHADOW_GAP-1);
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAlumniPopUpHandle, 2,AIM_POPUP_X, usPosY, VO_BLT.SRCTRANSPARENCY);
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hDoneHandle, 0,AIM_ALUMNI_DONE_X, usPosY-AIM_ALUMNI_DONE_HEIGHT, VO_BLT.SRCTRANSPARENCY);
    	FontSubSystem.DrawTextToScreen(AimAlumniText[AIM_ALUMNI_DONE], new((int)(AIM_ALUMNI_DONE_X+1), (int)(usPosY-AIM_ALUMNI_DONE_HEIGHT+3)), AIM_ALUMNI_DONE_WIDTH, AIM_ALUMNI_POPUP_NAME_FONT, AIM_ALUMNI_POPUP_NAME_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    	CreateDestroyDoneMouseRegion(usPosY);

    	///blt face panale and the mecs fce
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFacePaneHandle, 0,AIM_ALUMNI_FACE_PANEL_X, AIM_ALUMNI_FACE_PANEL_Y, VO_BLT.SRCTRANSPARENCY);
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, gubDrawOldMerc, AIM_ALUMNI_FACE_PANEL_X+1, AIM_ALUMNI_FACE_PANEL_Y+1, VO_BLT.SRCTRANSPARENCY);

    	//Load and display the name
    //	uiStartLoc = AIM_ALUMNI_NAME_SIZE * gubDrawOldMerc;
    //	files.LoadEncryptedDataFromFile(AIM_ALUMNI_NAME_FILE, out sName, uiStartLoc, AIM_ALUMNI_NAME_SIZE);
    	uiStartLoc = AIM_ALUMNI_FILE_RECORD_SIZE * gubDrawOldMerc;
    	files.LoadEncryptedDataFromFile(AIM_ALUMNI_FILE, out sName, uiStartLoc, AIM_ALUMNI_FULL_NAME_SIZE);

    	FontSubSystem.DrawTextToScreen(sName, new(AIM_ALUMNI_POPUP_NAME_X, AIM_ALUMNI_POPUP_NAME_Y), 0, AIM_ALUMNI_POPUP_NAME_FONT, AIM_ALUMNI_POPUP_NAME_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//Display the description
    	FontSubSystem.DisplayWrappedString(new(AIM_ALUMNI_POPUP_DESC_X, AIM_ALUMNI_POPUP_DESC_Y), AIM_POPUP_TEXT_WIDTH, 2, AIM_ALUMNI_POPUP_FONT, AIM_ALUMNI_POPUP_COLOR, sDesc, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
  
    	video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void DestroyPopUpBox()
    {
    	gfDestroyPopUpBox = false;
    	RenderAimArchives();
    }

    private static void InitAlumniFaceRegions()
    {
    	int	usPosX, usPosY,i,x,y, usNumRows;

    	if(gfFaceMouseRegionsActive)
    		return;

    	if( gubPageNum == 2 )
    		usNumRows = 2;
    	else
    		usNumRows = AIM_ALUMNI_NUM_FACE_ROWS;

    	usPosX = AIM_ALUMNI_START_GRID_X;
    	usPosY = AIM_ALUMNI_START_GRID_Y;
    	i=0;
    	for(y=0; y<usNumRows; y++)
    	{
    		for(x=0; x<AIM_ALUMNI_NUM_FACE_COLS; x++)
    		{

    			MouseSubSystem.MSYS_DefineRegion( gMercAlumniFaceMouseRegions[ i ], usPosX, usPosY, (int)(usPosX + AIM_ALUMNI_ALUMNI_FACE_WIDTH), (int)(usPosY + AIM_ALUMNI_ALUMNI_FACE_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectAlumniFaceRegionCallBack ); 
    			// Add region
    			MouseSubSystem.MSYS_AddRegion( ref gMercAlumniFaceMouseRegions[ i ] );
    			MouseSubSystem.MSYS_SetRegionUserData( gMercAlumniFaceMouseRegions[ i ], 0, i+(20*gubPageNum) );

    			usPosX += AIM_ALUMNI_GRID_OFFSET_X;
    			i++;
    		}
    		usPosX = AIM_ALUMNI_START_GRID_X;
    		usPosY += AIM_ALUMNI_GRID_OFFSET_Y;
    	}

    	//the 3rd page now has an additional row with 1 merc on it, so add a new row
    	if( gubPageNum == 2 )
    	{
    			MouseSubSystem.MSYS_DefineRegion( gMercAlumniFaceMouseRegions[ i ], usPosX, usPosY, (int)(usPosX + AIM_ALUMNI_ALUMNI_FACE_WIDTH), (int)(usPosY + AIM_ALUMNI_ALUMNI_FACE_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectAlumniFaceRegionCallBack ); 
    			// Add region
    			MouseSubSystem.MSYS_AddRegion( ref gMercAlumniFaceMouseRegions[ i ] );
    			MouseSubSystem.MSYS_SetRegionUserData( gMercAlumniFaceMouseRegions[ i ], 0, i+(20*gubPageNum) );
    	}



    	gfFaceMouseRegionsActive = true;
    }

    private static void RemoveAimAlumniFaceRegion()
    {
    	int i;
    	int usNumber=0;

    	if(!gfFaceMouseRegionsActive)
    		return;

    	switch(gubPageNum)
    	{
    		case 0:
    			usNumber = AIM_ALUMNI_NUM_FACE_ROWS * AIM_ALUMNI_NUM_FACE_COLS;
    			break;
    		case 1:
    			usNumber = AIM_ALUMNI_NUM_FACE_ROWS * AIM_ALUMNI_NUM_FACE_COLS;
    			break;
    		case 2:
    			usNumber = 2 * AIM_ALUMNI_NUM_FACE_COLS + 1;
    			break;

    		default:
    			break;
    	}

    	for(i=0; i<usNumber; i++)
    	{
    	  MouseSubSystem.MSYS_RemoveRegion( gMercAlumniFaceMouseRegions[ i ] );
    	}
    	gfFaceMouseRegionsActive = false;
    }

    // was a function-local static of CreateDestroyDoneMouseRegion
    private static bool CreateDestroyDoneMouseRegion_DoneRegionCreated =false;
    private static void CreateDestroyDoneMouseRegion(int usPosY)
    {


    	if( ( !CreateDestroyDoneMouseRegion_DoneRegionCreated ) && ( usPosY != 0) )
    	{
    		usPosY -= AIM_ALUMNI_DONE_HEIGHT;
    		MouseSubSystem.MSYS_DefineRegion( gDoneRegion, AIM_ALUMNI_DONE_X-2, usPosY, (AIM_ALUMNI_DONE_X-2 + AIM_ALUMNI_DONE_WIDTH), (int)(usPosY + AIM_ALUMNI_DONE_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectAlumniDoneRegionCallBack ); 
    		// Add region
    		MouseSubSystem.MSYS_AddRegion( ref gDoneRegion );
    		CreateDestroyDoneMouseRegion_DoneRegionCreated = true;
    	}

    	if( CreateDestroyDoneMouseRegion_DoneRegionCreated && usPosY == 0)
    	{
    		MouseSubSystem.MSYS_RemoveRegion( gDoneRegion );
    		CreateDestroyDoneMouseRegion_DoneRegionCreated = false;
    //		gfDestroyDoneRegion = FALSE;
    	}
    }

    private static void SelectAlumniDoneRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
    	if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    	{
    	}
    	else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    	{
    		gfDestroyPopUpBox = true;
    		gfDrawPopUpBox = false;
    		gfReDrawScreen = true;
    	}
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    	{
    	} 
    }

    private static void ChangingAimArchiveSubPage(int ubSubPageNumber)
    {
    	fLoadPendingFlag = true;

    	if( AimArchivesSubPagesVisitedFlag[ ubSubPageNumber ] == false )
    	{
    		fConnectingToSubPage = true;
    		fFastLoadFlag = false;

    		AimArchivesSubPagesVisitedFlag[ ubSubPageNumber ] = true;
    	}
    	else
    	{
    		fConnectingToSubPage = true;
    		fFastLoadFlag = true;
    	}
    }
}
