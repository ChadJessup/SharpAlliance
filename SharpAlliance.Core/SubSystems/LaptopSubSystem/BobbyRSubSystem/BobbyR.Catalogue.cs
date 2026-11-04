using System;
using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

// Port of BobbyRGuns.c: the catalogue shared by the Bobby Ray's guns, ammo, armour, misc and used pages.
public partial class BobbyR
{
    private const int BOBBYR_GRIDLOC_X = LAPTOP_SCREEN_UL_X + 4;
    private const int BOBBYR_GRIDLOC_Y = LAPTOP_SCREEN_WEB_UL_Y + 45;
    private static int gusFirstGunIndex;
    private static int gusLastGunIndex;
    private static int gubNumGunPages;
    private static int gusFirstAmmoIndex;
    private static int gusLastAmmoIndex;
    private static int gubNumAmmoPages;
    private static int gusFirstMiscIndex;
    private static int gusLastMiscIndex;
    private static int gubNumMiscPages;
    private static int gusFirstArmourIndex;
    private static int gusLastArmourIndex;
    private static int gubNumArmourPages;
    private static int gusFirstUsedIndex;
    private static int gusLastUsedIndex;
    private static int gubNumUsedPages;
    private const int BOBBYR_BACKGROUND_WIDTH = 125;
    private const int BOBBYR_BACKGROUND_HEIGHT = 100;
    private const int BOBBYR_NUM_HORIZONTAL_TILES = 4;
    private const int BOBBYR_NUM_VERTICAL_TILES = 4;
    private const string BOBBYRDESCFILE = "BINARYDATA\\BrayDesc.edt";
    private const int BOBBYR_ITEM_DESC_NAME_SIZE = 160;
    private const int BOBBYR_ITEM_DESC_INFO_SIZE = 640;
    private const int BOBBYR_ITEM_DESC_FILE_SIZE = 800;
    private const FontStyle BOBBYR_GUNS_BUTTON_FONT = FontStyle.FONT10ARIAL;
    private const FontColor BOBBYR_GUNS_TEXT_COLOR_ON = FontColor.FONT_NEARBLACK;
    private const FontColor BOBBYR_GUNS_TEXT_COLOR_OFF = FontColor.FONT_NEARBLACK;
    private const FontShadow BOBBYR_GUNS_SHADOW_COLOR = (FontShadow)169;
    private const IC BOBBYR_USED_ITEMS = unchecked((IC)0xFFFFFFFF);
    private const FontColor BOBBYR_DEFAULT_MENU_COLOR = (FontColor)255;
    private const int BOBBYR_GRID_PIC_WIDTH = 118;
    private const int BOBBYR_GRID_PIC_HEIGHT = 69;
    private const int BOBBYR_GRID_PIC_X = BOBBYR_GRIDLOC_X + 3;
    private const int BOBBYR_GRID_PIC_Y = BOBBYR_GRIDLOC_Y + 3;
    private const int BOBBYR_GRID_OFFSET = 72;
    private const FontStyle BOBBYR_ORDER_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontStyle BOBBYR_ORDER_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor BOBBYR_ORDER_TEXT_COLOR = (FontColor)75;
    private const FontColor BOBBYR_STATIC_TEXT_COLOR = (FontColor)75;
    private const FontStyle BOBBYR_ITEM_DESC_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor BOBBYR_ITEM_DESC_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle BOBBYR_ITEM_NAME_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor BOBBYR_ITEM_NAME_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const int NUM_BOBBYRPAGE_MENU = 6;
    private const int NUM_CATALOGUE_BUTTONS = 5;
    private const int BOBBYR_NUM_WEAPONS_ON_PAGE = 4;
    private const int BOBBYR_BRTITLE_X = LAPTOP_SCREEN_UL_X + 4;
    private const int BOBBYR_BRTITLE_Y = LAPTOP_SCREEN_WEB_UL_Y + 3;
    private const int BOBBYR_BRTITLE_WIDTH = 46;
    private const int BOBBYR_BRTITLE_HEIGHT = 42;
    private const int BOBBYR_TO_ORDER_TITLE_X = 195;
    private const int BOBBYR_TO_ORDER_TITLE_Y = 42 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int BOBBYR_TO_ORDER_TEXT_X = BOBBYR_TO_ORDER_TITLE_X + 75;
    private const int BOBBYR_TO_ORDER_TEXT_Y = 33 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int BOBBYR_TO_ORDER_TEXT_WIDTH = 330;
    private const int BOBBYR_PREVIOUS_BUTTON_X = LAPTOP_SCREEN_UL_X + 5; // BOBBYR_HOME_BUTTON_X + BOBBYR_CATALOGUE_BUTTON_WIDTH + 5
    private const int BOBBYR_PREVIOUS_BUTTON_Y = LAPTOP_SCREEN_WEB_UL_Y + 340; // BOBBYR_HOME_BUTTON_Y
    private const int BOBBYR_NEXT_BUTTON_X = LAPTOP_SCREEN_UL_X + 412; // BOBBYR_ORDER_FORM_X + BOBBYR_ORDER_FORM_WIDTH + 5
    private const int BOBBYR_NEXT_BUTTON_Y = BOBBYR_PREVIOUS_BUTTON_Y; // BOBBYR_PREVIOUS_BUTTON_Y
    private const int BOBBYR_CATALOGUE_BUTTON_START_X = BOBBYR_PREVIOUS_BUTTON_X + 92; // LAPTOP_SCREEN_UL_X + 93 - BOBBYR_CATALOGUE_BUTTON_WIDTH/2
    private const int BOBBYR_CATALOGUE_BUTTON_GAP = ( 318 - NUM_CATALOGUE_BUTTONS * BOBBYR_CATALOGUE_BUTTON_WIDTH) / (NUM_CATALOGUE_BUTTONS + 1) + BOBBYR_CATALOGUE_BUTTON_WIDTH + 1; // 80
    private const int BOBBYR_CATALOGUE_BUTTON_Y = LAPTOP_SCREEN_WEB_UL_Y + 340;
    private const int BOBBYR_CATALOGUE_BUTTON_WIDTH = 56; // 75
    private const int BOBBYR_HOME_BUTTON_X = 120;
    private const int BOBBYR_HOME_BUTTON_Y = 400 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int BOBBYR_CATALOGUE_BUTTON_TEXT_Y = BOBBYR_CATALOGUE_BUTTON_Y + 5;
    private const int BOBBYR_PREVIOUS_PAGE = 0;
    private const int BOBBYR_NEXT_PAGE = 1;
    private const int BOBBYR_ITEM_DESC_START_X = BOBBYR_GRIDLOC_X + 172 + 5;
    private const int BOBBYR_ITEM_DESC_START_Y = BOBBYR_GRIDLOC_Y + 6;
    private const int BOBBYR_ITEM_DESC_START_WIDTH = 214 - 10 + 20;
    private const int BOBBYR_ITEM_NAME_X = BOBBYR_GRIDLOC_X + 6;
    private const int BOBBYR_ITEM_NAME_Y_OFFSET = 54;
    private const int BOBBYR_ORDER_NUM_WIDTH = 15;
    private const int BOBBYR_ORDER_NUM_X = BOBBYR_GRIDLOC_X + 120 - BOBBYR_ORDER_NUM_WIDTH; // BOBBYR_ITEM_STOCK_TEXT_X
    private const int BOBBYR_ORDER_NUM_Y_OFFSET = 1;
    private const int BOBBYR_ITEM_WEIGHT_TEXT_X = BOBBYR_GRIDLOC_X + 409 + 3;
    private const int BOBBYR_ITEM_WEIGHT_TEXT_Y = 3;
    private const int BOBBYR_ITEM_WEIGHT_NUM_X = BOBBYR_GRIDLOC_X + 429 - 2;
    private const int BOBBYR_ITEM_WEIGHT_NUM_Y = 3;
    private const int BOBBYR_ITEM_WEIGHT_NUM_WIDTH = 60;
    private const int BOBBYR_ITEM_SPEC_GAP = 2;
    private const int BOBBYR_ITEM_COST_TEXT_X = BOBBYR_GRIDLOC_X + 125;
    private const int BOBBYR_ITEM_COST_TEXT_Y = BOBBYR_GRIDLOC_Y + 6;
    private const int BOBBYR_ITEM_COST_TEXT_WIDTH = 42;
    private const int BOBBYR_ITEM_COST_NUM_X = BOBBYR_ITEM_COST_TEXT_X;
    private const int BOBBYR_ITEM_COST_NUM_Y = BOBBYR_ITEM_COST_TEXT_Y + 10;
    private const int BOBBYR_ITEM_STOCK_TEXT_X = BOBBYR_ITEM_COST_TEXT_X;
    private const int BOBBYR_ITEM_QTY_TEXT_X = BOBBYR_GRIDLOC_X + 5; // BOBBYR_ITEM_COST_TEXT_X
    private const int BOBBYR_ITEM_QTY_TEXT_Y = BOBBYR_ITEM_COST_TEXT_Y + 28;
    private const int BOBBYR_ITEM_QTY_WIDTH = 95;
    private const int BOBBYR_ITEM_QTY_NUM_X = BOBBYR_GRIDLOC_X + 105; // BOBBYR_ITEM_COST_TEXT_X + 1
    private const int BOBBYR_ITEM_QTY_NUM_Y = BOBBYR_ITEM_QTY_TEXT_Y; // BOBBYR_ITEM_COST_TEXT_Y + 40
    private const int BOBBYR_ITEMS_BOUGHT_X = BOBBYR_GRIDLOC_X + 105 - BOBBYR_ORDER_NUM_WIDTH; // BOBBYR_ITEM_QTY_NUM_X
    private const int BOBBY_RAY_NOT_PURCHASED = 255;
    private const int BOBBY_RAY_MAX_AMOUNT_OF_ITEMS_TO_PURCHASE = 200;
    private const int BOBBYR_ORDER_FORM_X = LAPTOP_SCREEN_UL_X + 200; // 204
    private const int BOBBYR_ORDER_FORM_Y = LAPTOP_SCREEN_WEB_UL_Y + 367;
    private const int BOBBYR_ORDER_FORM_WIDTH = 95;
    private const int BOBBYR_ORDER_SUBTOTAL_X = 490;
    private const int BOBBYR_ORDER_SUBTOTAL_Y = BOBBYR_ORDER_FORM_Y+2; // BOBBYR_HOME_BUTTON_Y
    private const int BOBBYR_PERCENT_FUNTCIONAL_X = BOBBYR_ORDER_SUBTOTAL_X;
    private const int BOBBYR_PERCENT_FUNTCIONAL_Y = BOBBYR_ORDER_SUBTOTAL_Y + 15;

    private static HVOBJECT guiGunBackground = null!;
    private static HVOBJECT guiGunsGrid = null!;
    private static HVOBJECT guiBrTitle = null!;
    private static int gusCurWeaponIndex;
    private static int gubCurPage;

    private static readonly LAPTOP_MODE[] ubCatalogueButtonValues =
    {
        LAPTOP_MODE.BOBBY_R_GUNS,
        LAPTOP_MODE.BOBBY_R_AMMO,
        LAPTOP_MODE.BOBBY_R_ARMOR,
        LAPTOP_MODE.BOBBY_R_MISC,
        LAPTOP_MODE.BOBBY_R_USED,
    };

    private static int gusLastItemIndex = 0;
    private static int gusFirstItemIndex = 0;
    private static int gubNumItemsOnScreen;
    private static int gubNumPages;
    private static bool gfBigImageMouseRegionCreated;
    private static readonly int[] gusItemNumberForItemsOnScreen = new int[BOBBYR_NUM_WEAPONS_ON_PAGE];
    private static bool gfOnUsedPage;
    private static int gusOldItemNumOnTopOfPage = 65535;

    private static readonly GUI_BUTTON[] guiBobbyRPageMenu = new GUI_BUTTON[NUM_CATALOGUE_BUTTONS];
    private static ButtonPic guiBobbyRPageMenuImage = null!;
    private static GUI_BUTTON guiBobbyRPreviousPage = null!;
    private static ButtonPic guiBobbyRPreviousPageImage = null!;
    private static GUI_BUTTON guiBobbyRNextPage = null!;
    private static ButtonPic guiBobbyRNextPageImage = null!;

    private static readonly MOUSE_REGION[] gSelectedBigImageRegion = CreateRegionArray(BOBBYR_NUM_WEAPONS_ON_PAGE, nameof(gSelectedBigImageRegion));

    private static GUI_BUTTON guiBobbyROrderForm = null!;
    private static ButtonPic guiBobbyROrderFormImage = null!;
    private static GUI_BUTTON guiBobbyRHome = null!;
    private static ButtonPic guiBobbyRHomeImage = null!;

    private static MOUSE_REGION gSelectedTitleImageLinkRegion = new(nameof(gSelectedTitleImageLinkRegion));

    private static MOUSE_REGION[] CreateRegionArray(int count, string name)
    {
        var regions = new MOUSE_REGION[count];
        for (int i = 0; i < count; i++)
        {
            regions[i] = new MOUSE_REGION($"{name}[{i}]");
        }

        return regions;
    }

    /// <summary>Tiles a background graphic over the web page area (WebPageTileBackground in the original).</summary>
    private static void ShadowRect(int left, int top, int right, int bottom)
        => VideoSurfaceManager.ShadowVideoSurfaceRect(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(left, top, right, bottom));

    private static bool WebPageTileBackground(int ubNumX, int ubNumY, int usWidth, int usHeight, HVOBJECT hBackGroundHandle)
    {
        int uiPosY = LAPTOP_SCREEN_WEB_UL_Y;
        for (int y = 0; y < ubNumY; y++)
        {
            int uiPosX = LAPTOP_SCREEN_UL_X;
            for (int x = 0; x < ubNumX; x++)
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, hBackGroundHandle, 0, uiPosX, uiPosY, VO_BLT.SRCTRANSPARENCY);
                uiPosX += usWidth;
            }

            uiPosY += usHeight;
        }

        return true;
    }

    public static bool EnterBobbyRGuns()
    {

    	gfBigImageMouseRegionCreated = false;

    	// load the background graphic and add it
    	guiGunBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\gunbackground.sti"));

    	// load the gunsgrid graphic and add it
    	guiGunsGrid = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\gunsgrid.sti"));

    	InitBobbyBrTitle();


    	SetFirstLastPagesForNew( IC.BOBBY_GUN );
    //	CalculateFirstAndLastIndexs();
    /*
    	if(giCurrentSubPage == 0)
    		gusCurWeaponIndex = gusFirstGunIndex;
    	else
    		gusCurWeaponIndex = (int)giCurrentSubPage;
    */
    	//Draw menu bar
    	InitBobbyMenuBar();

    	// render once
    	RenderBobbyRGuns( );

    //	RenderBobbyRGuns();
    	return(true);
    }

    public static void ExitBobbyRGuns()
    {
    	video.DeleteVideoObject(guiGunBackground);
    	video.DeleteVideoObject(guiGunsGrid);
    	DeleteBobbyBrTitle();
    	DeleteBobbyMenuBar();

    	DeleteMouseRegionForBigImage();

    	giCurrentSubPage = gusCurWeaponIndex;
    	guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R_GUNS;
    }

    public static void HandleBobbyRGuns()
    {
    }

    public static void RenderBobbyRGuns()
    {
      HVOBJECT hPixHandle;

    	WebPageTileBackground(BOBBYR_NUM_HORIZONTAL_TILES, BOBBYR_NUM_VERTICAL_TILES, BOBBYR_BACKGROUND_WIDTH, BOBBYR_BACKGROUND_HEIGHT, guiGunBackground);

    	//Display title at top of page
    	DisplayBobbyRBrTitle();

    	// GunForm
    	hPixHandle = guiGunsGrid;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBYR_GRIDLOC_X, BOBBYR_GRIDLOC_Y, VO_BLT.SRCTRANSPARENCY);

    //	DeleteMouseRegionForBigImage();
    	DisplayItemInfo( IC.BOBBY_GUN );
    	UpdateButtonText(guiCurrentLaptopMode);
      ButtonSubSystem.MarkButtonsDirty(Laptop.buttonList);
    	Laptop.RenderWWWProgramTitleBar( );
      video.InvalidateRegion(0,0,640,480);
    }

    private static bool DisplayBobbyRBrTitle()
    {
      HVOBJECT hPixHandle;

    	// BR title
    	hPixHandle = guiBrTitle;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBYR_BRTITLE_X, BOBBYR_BRTITLE_Y, VO_BLT.SRCTRANSPARENCY);

    	// To Order Text
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_TO_ORDER], new(BOBBYR_TO_ORDER_TITLE_X, BOBBYR_TO_ORDER_TITLE_Y), 0, BOBBYR_ORDER_TITLE_FONT, BOBBYR_ORDER_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//First put a shadow behind the image
    	ShadowRect( BOBBYR_TO_ORDER_TEXT_X-2, BOBBYR_TO_ORDER_TEXT_Y-2, BOBBYR_TO_ORDER_TEXT_X+BOBBYR_TO_ORDER_TEXT_WIDTH, BOBBYR_TO_ORDER_TEXT_Y+31);

    	//To Order text
    	FontSubSystem.DisplayWrappedString(new(BOBBYR_TO_ORDER_TEXT_X, BOBBYR_TO_ORDER_TEXT_Y), BOBBYR_TO_ORDER_TEXT_WIDTH, 2, BOBBYR_ORDER_TEXT_FONT, BOBBYR_ORDER_TEXT_COLOR, BobbyRText[BOBBYR_GUNS_CLICK_ON_ITEMS], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	return(true);
    }

    private static bool InitBobbyBrTitle()
    {

    	// load the br title graphic and add it
    	guiBrTitle = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\br.sti"));

    	//initialize the link to the homepage by clicking on the title
    	MouseSubSystem.MSYS_DefineRegion( gSelectedTitleImageLinkRegion, BOBBYR_BRTITLE_X, BOBBYR_BRTITLE_Y, (BOBBYR_BRTITLE_X + BOBBYR_BRTITLE_WIDTH), (int)(BOBBYR_BRTITLE_Y + BOBBYR_BRTITLE_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectTitleImageLinkRegionCallBack );
    	MouseSubSystem.MSYS_AddRegion( ref gSelectedTitleImageLinkRegion ); 
	

    	gusOldItemNumOnTopOfPage=65535;

    	return(true);
    }

    private static bool DeleteBobbyBrTitle()
    {
    	video.DeleteVideoObject(guiBrTitle);

    	MouseSubSystem.MSYS_RemoveRegion( gSelectedTitleImageLinkRegion );

    	DeleteMouseRegionForBigImage();

    	return(true);
    }

    private static void SelectTitleImageLinkRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    	{
    	}
    	else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    	{
    		guiCurrentLaptopMode = LAPTOP_MODE.BOBBY_R;
    	}
    }

    private static bool InitBobbyMenuBar()
    {
    	int	i;
    	int	usPosX;
    	int		bCurMode;

    	// Previous button
    	guiBobbyRPreviousPageImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\PreviousButton.sti", -1,0,-1,1,-1 );
    	guiBobbyRPreviousPage = ButtonSubSystem.CreateIconAndTextButton(guiBobbyRPreviousPageImage, BobbyRText[BOBBYR_GUNS_PREVIOUS_ITEMS], BOBBYR_GUNS_BUTTON_FONT, BOBBYR_GUNS_TEXT_COLOR_ON, BOBBYR_GUNS_SHADOW_COLOR, BOBBYR_GUNS_TEXT_COLOR_OFF, BOBBYR_GUNS_SHADOW_COLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(BOBBYR_PREVIOUS_BUTTON_X, BOBBYR_PREVIOUS_BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBobbyRNextPreviousPageCallback);
    	Laptop.buttonList.Add(guiBobbyRPreviousPage);
    	ButtonSubSystem.SetButtonCursor(guiBobbyRPreviousPage, CURSOR.LAPTOP_SCREEN);
    	ButtonSubSystem.MSYS_SetBtnUserData( guiBobbyRPreviousPage, 0, BOBBYR_PREVIOUS_PAGE);
    	ButtonSubSystem.SpecifyDisabledButtonStyle( guiBobbyRPreviousPage, DISABLED_STYLE.SHADED );

 
    	guiBobbyRPageMenuImage =  ButtonSubSystem.LoadButtonImage("LAPTOP\\CatalogueButton1.sti", -1,0,-1,1,-1 );

    	// Next button
    	guiBobbyRNextPageImage  = ButtonSubSystem.LoadButtonImage("LAPTOP\\NextButton.sti", -1,0,-1,1,-1 );
    	guiBobbyRNextPage = ButtonSubSystem.CreateIconAndTextButton(guiBobbyRNextPageImage, BobbyRText[BOBBYR_GUNS_MORE_ITEMS], BOBBYR_GUNS_BUTTON_FONT, BOBBYR_GUNS_TEXT_COLOR_ON, BOBBYR_GUNS_SHADOW_COLOR, BOBBYR_GUNS_TEXT_COLOR_OFF, BOBBYR_GUNS_SHADOW_COLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(BOBBYR_NEXT_BUTTON_X, BOBBYR_NEXT_BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBobbyRNextPreviousPageCallback);
    	Laptop.buttonList.Add(guiBobbyRNextPage);
    	ButtonSubSystem.SetButtonCursor(guiBobbyRNextPage, CURSOR.LAPTOP_SCREEN);
    	ButtonSubSystem.MSYS_SetBtnUserData( guiBobbyRNextPage, 0, BOBBYR_NEXT_PAGE);
    	ButtonSubSystem.SpecifyDisabledButtonStyle( guiBobbyRNextPage, DISABLED_STYLE.SHADED );


    	bCurMode = 0;
    	usPosX = BOBBYR_CATALOGUE_BUTTON_START_X;
    	for(i=0; i<NUM_CATALOGUE_BUTTONS; i++)
    	{
    		// Catalogue Buttons button
    		guiBobbyRPageMenu[i] = ButtonSubSystem.CreateIconAndTextButton(guiBobbyRPageMenuImage, BobbyRText[BOBBYR_GUNS_GUNS+i], BOBBYR_GUNS_BUTTON_FONT, BOBBYR_GUNS_TEXT_COLOR_ON, BOBBYR_GUNS_SHADOW_COLOR, BOBBYR_GUNS_TEXT_COLOR_OFF, BOBBYR_GUNS_SHADOW_COLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, BOBBYR_CATALOGUE_BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBobbyRPageMenuCallback);
    		Laptop.buttonList.Add(guiBobbyRPageMenu[i]);

    		ButtonSubSystem.SetButtonCursor(guiBobbyRPageMenu[i], CURSOR.LAPTOP_SCREEN);

    		ButtonSubSystem.MSYS_SetBtnUserData( guiBobbyRPageMenu[i], 0, ubCatalogueButtonValues[bCurMode]);

    		usPosX += BOBBYR_CATALOGUE_BUTTON_GAP;
    		bCurMode++;
    	}

    	// Order Form button
    	guiBobbyROrderFormImage  = ButtonSubSystem.LoadButtonImage("LAPTOP\\OrderFormButton.sti", -1,0,-1,1,-1 );
    	guiBobbyROrderForm = ButtonSubSystem.CreateIconAndTextButton(guiBobbyROrderFormImage, BobbyRText[BOBBYR_GUNS_ORDER_FORM], BOBBYR_GUNS_BUTTON_FONT, BOBBYR_GUNS_TEXT_COLOR_ON, BOBBYR_GUNS_SHADOW_COLOR, BOBBYR_GUNS_TEXT_COLOR_OFF, BOBBYR_GUNS_SHADOW_COLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(BOBBYR_ORDER_FORM_X, BOBBYR_ORDER_FORM_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBobbyROrderFormCallback);
    	Laptop.buttonList.Add(guiBobbyROrderForm);

    	ButtonSubSystem.SetButtonCursor(guiBobbyROrderForm, CURSOR.LAPTOP_SCREEN);


    	// Home button
	
    	guiBobbyRHomeImage  = ButtonSubSystem.LoadButtonImage("LAPTOP\\CatalogueButton.sti", -1,0,-1,1,-1 );
    	guiBobbyRHome = ButtonSubSystem.CreateIconAndTextButton(guiBobbyRHomeImage, BobbyRText[BOBBYR_GUNS_HOME], BOBBYR_GUNS_BUTTON_FONT, BOBBYR_GUNS_TEXT_COLOR_ON, BOBBYR_GUNS_SHADOW_COLOR, BOBBYR_GUNS_TEXT_COLOR_OFF, BOBBYR_GUNS_SHADOW_COLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(BOBBYR_HOME_BUTTON_X, BOBBYR_HOME_BUTTON_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBobbyRHomeButtonCallback);
    	Laptop.buttonList.Add(guiBobbyRHome);
    	ButtonSubSystem.SetButtonCursor(guiBobbyRHome, CURSOR.LAPTOP_SCREEN);

    	return(true);
    }

    private static bool DeleteBobbyMenuBar()
    {
    	int	i;

    	ButtonSubSystem.UnloadButtonImage( guiBobbyRNextPageImage );
    	ButtonSubSystem.UnloadButtonImage( guiBobbyRPreviousPageImage );
    	ButtonSubSystem.UnloadButtonImage( guiBobbyRPageMenuImage );
    	ButtonSubSystem.UnloadButtonImage( guiBobbyROrderFormImage );
    	ButtonSubSystem.UnloadButtonImage( guiBobbyRHomeImage );

    	Laptop.buttonList.Remove( guiBobbyRNextPage );
    	ButtonSubSystem.RemoveButton( guiBobbyRNextPage );
    	Laptop.buttonList.Remove( guiBobbyRPreviousPage );
    	ButtonSubSystem.RemoveButton( guiBobbyRPreviousPage );
    	Laptop.buttonList.Remove( guiBobbyROrderForm );
    	ButtonSubSystem.RemoveButton( guiBobbyROrderForm );
    	Laptop.buttonList.Remove( guiBobbyRHome );
    	ButtonSubSystem.RemoveButton( guiBobbyRHome );

    	for(i=0; i<NUM_CATALOGUE_BUTTONS; i++)
    	{
    		Laptop.buttonList.Remove( guiBobbyRPageMenu[i] );
    		ButtonSubSystem.RemoveButton( guiBobbyRPageMenu[i] );
    	}

    	return(true);
    }

    private static void BtnBobbyRPageMenuCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	LAPTOP_MODE bNewValue;
    	bNewValue = (LAPTOP_MODE)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );

    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

    		guiTempCurrentMode = (int)bNewValue;

    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    		guiTempCurrentMode = (int)BOBBYR_DEFAULT_MENU_COLOR;
    		UpdateButtonText(guiCurrentLaptopMode);
    		guiCurrentLaptopMode = bNewValue;

    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		guiTempCurrentMode = (int)BOBBYR_DEFAULT_MENU_COLOR;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }

    private static void BtnBobbyRNextPreviousPageCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	LAPTOP_MODE bNewValue;

    	bNewValue = (LAPTOP_MODE)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);


    		//if previous screen
    		if( bNewValue == BOBBYR_PREVIOUS_PAGE)
    		{
    			if( gubCurPage > 0 )
    				gubCurPage--;
    		}
    		//else next screen
    		else
    		{
    			if( gubCurPage < gubNumPages-1 )
    				gubCurPage++;
    		}


    		DeleteMouseRegionForBigImage();

    		fReDrawScreenFlag = true;
    		fPausedReDrawScreenFlag = true;		
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }

    private static bool DisplayItemInfo(IC uiItemClass)
    {
    	int	i;
    	int		ubCount=0;
    	int	PosY, usTextPosY;
    	int		ubFirstIndex=0;
    	Items usItemIndex;
    	string sDollarTemp = string.Empty;
    	string sTemp = string.Empty;
    	int[] pItemNumbers = new int[ BOBBYR_NUM_WEAPONS_ON_PAGE ];

    	PosY = BOBBYR_GRID_PIC_Y;
    	usTextPosY = BOBBYR_ITEM_DESC_START_Y;

    //	InitFirstAndLastGlobalIndex( uiItemClass );

    	//if there are no items then return
    	if( gusFirstItemIndex == (int)BOBBYR_NO_ITEMS )
    	{
    		if( fExitingLaptopFlag )
    			return( true );

    		DisableBobbyRButtons();

    		//Display a popup saying we are out of stock
    		Laptop.DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, BobbyRText[ BOBBYR_NO_MORE_STOCK ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, OutOfStockMessageBoxCallBack );
    		return( true );
    	}


    	if( uiItemClass == BOBBYR_USED_ITEMS )
    		CalcFirstIndexForPage( LaptopSaveInfo.BobbyRayUsedInventory, uiItemClass );
    	else
    		CalcFirstIndexForPage( LaptopSaveInfo.BobbyRayInventory, uiItemClass );

    	DisableBobbyRButtons();

    	if( gusOldItemNumOnTopOfPage != gusCurWeaponIndex )
    	{
    		DeleteMouseRegionForBigImage();

    	}



    	for(i=gusCurWeaponIndex; ((i<=gusLastItemIndex) && (ubCount < 4)); i++)
    	{
    		if( uiItemClass == BOBBYR_USED_ITEMS )
    		{
    			//If there is not items in stock
    			if( LaptopSaveInfo.BobbyRayUsedInventory[ i ].ubQtyOnHand == 0 )
    				continue;

    			usItemIndex = LaptopSaveInfo.BobbyRayUsedInventory[ i ].usItemIndex;
    			gfOnUsedPage = true;
    		}
    		else
    		{
    			//If there is not items in stock
    			if( LaptopSaveInfo.BobbyRayInventory[ i ].ubQtyOnHand == 0 )
    				continue;

    			usItemIndex = LaptopSaveInfo.BobbyRayInventory[ i ].usItemIndex;
    			gfOnUsedPage = false;
    		}

    		// skip items that aren't of the right item class
    		if( ! ( Item[ usItemIndex ].usItemClass.HasFlag(uiItemClass) ) )
    		{
    			continue;
    		}


    		pItemNumbers[ ubCount ] = (int)usItemIndex;

    		switch( Item[ usItemIndex ].usItemClass )
    		{
    			case IC.GUN:
    			case IC.LAUNCHER:
    				gusItemNumberForItemsOnScreen[ ubCount ] = i;

    				DisplayBigItemImage( usItemIndex, PosY);
			
    				//Display Items Name
    				DisplayItemNameAndInfo(usTextPosY, usItemIndex, i, gfOnUsedPage);

    				DisplayGunInfo(usItemIndex, usTextPosY, gfOnUsedPage, i);

    				PosY += BOBBYR_GRID_OFFSET;
    				usTextPosY += BOBBYR_GRID_OFFSET;
    				ubCount++;
    				break;

    			case IC.AMMO:
    				gusItemNumberForItemsOnScreen[ ubCount ] = i;

    				DisplayBigItemImage( usItemIndex, PosY);
		
    				//Display Items Name
    				DisplayItemNameAndInfo(usTextPosY, usItemIndex, i, gfOnUsedPage);

    				DisplayAmmoInfo( usItemIndex, usTextPosY, gfOnUsedPage, i);

    				PosY += BOBBYR_GRID_OFFSET;
    				usTextPosY += BOBBYR_GRID_OFFSET;
    				ubCount++;
    				break;

    			case IC.ARMOUR:
    				gusItemNumberForItemsOnScreen[ ubCount ] = i;

    				DisplayBigItemImage( usItemIndex, PosY);
			
    				//Display Items Name
    				DisplayItemNameAndInfo(usTextPosY, usItemIndex, i, gfOnUsedPage);

    				DisplayArmourInfo( usItemIndex, usTextPosY, gfOnUsedPage, i);

    				PosY += BOBBYR_GRID_OFFSET;
    				usTextPosY += BOBBYR_GRID_OFFSET;
    				ubCount++;
    				break;

    			case IC.BLADE:
    			case IC.THROWING_KNIFE:
    			case IC.PUNCH:
    				gusItemNumberForItemsOnScreen[ ubCount ] = i;

    				DisplayBigItemImage( usItemIndex, PosY);
			
    				//Display Items Name
    				DisplayItemNameAndInfo(usTextPosY, usItemIndex, i, gfOnUsedPage);

    				DisplayNonGunWeaponInfo(usItemIndex, usTextPosY, gfOnUsedPage, i);

    				PosY += BOBBYR_GRID_OFFSET;
    				usTextPosY += BOBBYR_GRID_OFFSET;
    				ubCount++;
    				break;

    			case IC.GRENADE:
    			case IC.BOMB:
    			case IC.MISC:
    			case IC.MEDKIT:
    			case IC.KIT:
    			case IC.FACE:
    				gusItemNumberForItemsOnScreen[ ubCount ] = i;

    				DisplayBigItemImage( usItemIndex, PosY);
			
    				//Display Items Name
    				DisplayItemNameAndInfo(usTextPosY, usItemIndex, i, gfOnUsedPage);

    				DisplayMiscInfo( usItemIndex, usTextPosY, gfOnUsedPage, i);

    				PosY += BOBBYR_GRID_OFFSET;
    				usTextPosY += BOBBYR_GRID_OFFSET;
    				ubCount++;
    				break;
    		}
    	}

    	if( gusOldItemNumOnTopOfPage != gusCurWeaponIndex )
    	{
    		CreateMouseRegionForBigImage(BOBBYR_GRID_PIC_Y, ubCount, pItemNumbers );
	
    		gusOldItemNumOnTopOfPage = gusCurWeaponIndex;
    	}

    	//Display the subtotal at the bottom of the screen
    	sDollarTemp = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( CalculateTotalPurchasePrice()  ) );
    	sTemp = wprintf( "%s %s", BobbyRText[BOBBYR_GUNS_SUB_TOTAL], sDollarTemp );
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ORDER_SUBTOTAL_X, BOBBYR_ORDER_SUBTOTAL_Y), 0, BOBBYR_ORDER_TITLE_FONT, BOBBYR_ORDER_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED | TextJustifies.TEXT_SHADOWED);

    	//Display the Used item disclaimer
    	if( gfOnUsedPage )
    	{
    		FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_PERCENT_FUNCTIONAL], new(BOBBYR_PERCENT_FUNTCIONAL_X, BOBBYR_PERCENT_FUNTCIONAL_Y), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ORDER_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED | TextJustifies.TEXT_SHADOWED);
    	}

    	return(true);
    }

    private static bool DisplayGunInfo(Items usIndex, int usTextPosY, bool fUsed, int usBobbyIndex)
    {
    	int	usHeight;
    	int	uiStartLoc=0;
    	int usFontHeight;
    	usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    	//Display Items Name
    	//DisplayItemNameAndInfo(usTextPosY, usIndex, fUsed);

    	usHeight = usTextPosY;
    	//Display the weight, caliber, mag, rng, dam, rof text

    	//Weight
    	usHeight = DisplayWeight(usHeight, usIndex, usFontHeight);

    	//Caliber
    	usHeight = DisplayCaliber(usHeight, usIndex, usFontHeight);

    	//Magazine
    	usHeight = DisplayMagazine(usHeight, usIndex, usFontHeight);

    	//Range
    	usHeight = DisplayRange(usHeight, usIndex, usFontHeight);
	
    	//Damage
    	usHeight = DisplayDamage(usHeight, usIndex, usFontHeight);

    	//ROF
    	usHeight = DisplayRof(usHeight, usIndex, usFontHeight);

    	//Display the Cost and the qty bought and on hand
    	usHeight = DisplayCostAndQty(usTextPosY, usIndex, usFontHeight, usBobbyIndex, fUsed);

    	return(true);
    }

    private static bool DisplayNonGunWeaponInfo(Items usIndex, int usTextPosY, bool fUsed, int usBobbyIndex)
    {
    	int	usHeight;
    	int	uiStartLoc=0;
    	int usFontHeight;
    	usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    	//Display Items Name
    //	DisplayItemNameAndInfo(usTextPosY, usIndex, fUsed);

    	usHeight = usTextPosY;
    	//Display the weight, caliber, mag, rng, dam, rof text

    	//Weight
    	usHeight = DisplayWeight(usHeight, usIndex, usFontHeight);

    	//Damage
    	usHeight = DisplayDamage(usHeight, usIndex, usFontHeight);

    	//Display the Cost and the qty bought and on hand
    	usHeight = DisplayCostAndQty(usTextPosY, usIndex, usFontHeight, usBobbyIndex, fUsed);

    	return(true);
    }

    private static bool DisplayAmmoInfo(Items usIndex, int usTextPosY, bool fUsed, int usBobbyIndex)
    {
    	int	usHeight;
    	int	uiStartLoc=0;
    	int usFontHeight;
    	usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    	//Display Items Name
    //	DisplayItemNameAndInfo(usTextPosY, usIndex, fUsed);

    	usHeight = usTextPosY;
    	//Display the weight, caliber, mag, rng, dam, rof text

    	//Caliber
    	usHeight = DisplayCaliber(usHeight, usIndex, usFontHeight);

    	//Magazine
    //	usHeight = DisplayMagazine(usHeight, usIndex, usFontHeight);

    	//Display the Cost and the qty bought and on hand
    	usHeight = DisplayCostAndQty(usTextPosY, usIndex, usFontHeight, usBobbyIndex, fUsed);

    	return(true);
    }

    private static bool DisplayBigItemImage(Items usIndex, int PosY)
    {
    	// TODO(port): draw the item's picture, centred in its frame (the item tile graphics are not loaded in the port yet)
    	return(true);
    }

    private static bool DisplayArmourInfo(Items usIndex, int usTextPosY, bool fUsed, int usBobbyIndex)
    {
    	int	usHeight;
    	int	uiStartLoc=0;
    	int usFontHeight;
    	usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    	//Display Items Name
    //	DisplayItemNameAndInfo(usTextPosY, usIndex, fUsed);

    	usHeight = usTextPosY;
    	//Display the weight, caliber, mag, rng, dam, rof text

    	//Weight
    	usHeight = DisplayWeight(usHeight, usIndex, usFontHeight);

    	//Display the Cost and the qty bought and on hand
    	usHeight = DisplayCostAndQty(usTextPosY, usIndex, usFontHeight, usBobbyIndex, fUsed);

    	return(true);
    }

    private static bool DisplayMiscInfo(Items usIndex, int usTextPosY, bool fUsed, int usBobbyIndex)
    {
    	int	usHeight;
    	int	uiStartLoc=0;
    	int usFontHeight;
    	usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    	//Display Items Name
    //	DisplayItemNameAndInfo(usTextPosY, usIndex, fUsed);

    	//Display the Cost and the qty bought and on hand
    	usHeight = DisplayCostAndQty(usTextPosY, usIndex, usFontHeight, usBobbyIndex, fUsed);

    	return(true);
    }

    private static int DisplayCostAndQty(int usPosY, Items usIndex, int usFontHeight, int usBobbyIndex, bool fUsed)
    {
    	string sTemp = string.Empty;
    //	UINT8	ubPurchaseNumber;

    	//
    	//Display the cost and the qty
    	//

    	//Display the cost
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_COST], new(BOBBYR_ITEM_COST_TEXT_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	usPosY += usFontHeight + 2;

    	sTemp = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( CalcBobbyRayCost( usIndex, usBobbyIndex, fUsed ) ) );

    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_COST_NUM_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;



    /*
    	//Display the # bought
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_QTY_ON_ORDER], new(BOBBYR_ITEM_QTY_TEXT_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	usPosY += usFontHeight + 2;

    	ubPurchaseNumber = CheckIfItemIsPurchased(usBobbyIndex);
    	if( ubPurchaseNumber == BOBBY_RAY_NOT_PURCHASED)
    		sTemp = wprintf( "% 4d", 0);
    	else
    		sTemp = wprintf( "% 4d", BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased);

    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEMS_BOUGHT_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    */

    	//Display Weight Number
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_WGHT], new(BOBBYR_ITEM_STOCK_TEXT_X, (int)(usPosY)), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	usPosY += usFontHeight + 2;


    	sTemp = wprintf( "%3.2f", (Item[ usIndex ].ubWeight * (GameSettings.fOptions[TOPTION.USE_METRIC_SYSTEM] ? 1.0 : 2.2)) / 10.0 );
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_STOCK_TEXT_X, (int)(usPosY)), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;


    	//Display the # In Stock
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_IN_STOCK], new(BOBBYR_ITEM_STOCK_TEXT_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	usPosY += usFontHeight + 2;

    	if( fUsed )
    		sTemp = wprintf( "% 4d", LaptopSaveInfo.BobbyRayUsedInventory[ usBobbyIndex ].ubQtyOnHand);
    	else
    		sTemp = wprintf( "% 4d", LaptopSaveInfo.BobbyRayInventory[ usBobbyIndex ].ubQtyOnHand);

    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_STOCK_TEXT_X, (int)usPosY), BOBBYR_ITEM_COST_TEXT_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;


    	return(usPosY);
    }

    private static int DisplayRof(int usPosY, Items usIndex, int usFontHeight)
    {
    	string sTemp = string.Empty;

    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_ROF], new(BOBBYR_ITEM_WEIGHT_TEXT_X, (int)usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	if( WeaponROF[ (int)usIndex ] == -1 )
    		sTemp = wprintf( "? %s", pMessageStrings[ MSG.RPM ] );
    	else
    		sTemp = wprintf( "%3d/%s", WeaponROF[ (int)usIndex ], pMessageStrings[ MSG.MINUTE_ABBREVIATION ]);
	

    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, (int)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static int DisplayDamage(int usPosY, Items usIndex, int usFontHeight)
    {
    	string sTemp = string.Empty;

    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_DAMAGE], new(BOBBYR_ITEM_WEIGHT_TEXT_X, (int)usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	sTemp = wprintf( "%4d", WeaponTypes.Weapon[ usIndex ].ubImpact);
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, (int)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static int DisplayRange(int usPosY, Items usIndex, int usFontHeight)
    {
    	string sTemp = string.Empty;

    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_RANGE], new(BOBBYR_ITEM_WEIGHT_TEXT_X, (int)usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	sTemp = wprintf( "%3d %s", WeaponTypes.Weapon[ usIndex ].usRange, pMessageStrings[ MSG.METER_ABBREVIATION ] );
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, (int)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static int DisplayMagazine(int usPosY, Items usIndex, int usFontHeight)
    {
    	string sTemp = string.Empty;

    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_MAGAZINE], new(BOBBYR_ITEM_WEIGHT_TEXT_X, (int)usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	sTemp = wprintf( "%3d %s", WeaponTypes.Weapon[usIndex].ubMagSize, pMessageStrings[ MSG.ROUNDS_ABBREVIATION ] );
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, (int)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static int DisplayCaliber(int usPosY, Items usIndex, int usFontHeight)
    {
    	string zTemp = string.Empty;
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_CALIBRE], new(BOBBYR_ITEM_WEIGHT_TEXT_X, (int)usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//	if ammo is begin drawn
    	if( Item[ usIndex].usItemClass == IC.AMMO )
    	{
    		zTemp = wprintf( "%s", BobbyRayAmmoCaliber[ (int)(WeaponTypes.Magazine[ Item[ usIndex ].ubClassIndex ].ubCalibre)] );
    //		FontSubSystem.DrawTextToScreen(AmmoCaliber[ WeaponTypes.Magazine[ Item[ usIndex ].ubClassIndex ].ubCalibre)], new(BOBBYR_ITEM_WEIGHT_NUM_X, (UINT16)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	}
    	else
    	{
    		//else a gun is being displayed
    		zTemp = wprintf( "%s", BobbyRayAmmoCaliber[ (int)WeaponTypes.Weapon[ (Items)Item[ usIndex ].ubClassIndex ].ubCalibre ] );
    //		FontSubSystem.DrawTextToScreen(AmmoCaliber[ WeaponTypes.Weapon[ Item[ usIndex ].ubClassIndex ].ubCalibre ], new(BOBBYR_ITEM_WEIGHT_NUM_X, (UINT16)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	}


    	if( FontSubSystem.StringPixLength( zTemp, BOBBYR_ITEM_DESC_TEXT_FONT ) > BOBBYR_ITEM_WEIGHT_NUM_WIDTH )
    		WordWrap.ReduceStringLength( ref zTemp, BOBBYR_GRID_PIC_WIDTH, BOBBYR_ITEM_NAME_TEXT_FONT );

    	FontSubSystem.DrawTextToScreen(zTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, (int)usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);


    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static int DisplayWeight(int usPosY, Items usIndex, int usFontHeight)
    {
    	//display the 'weight' string
    	FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_WEIGHT], new(BOBBYR_ITEM_WEIGHT_TEXT_X, usPosY), 0, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	// weights are stored in tenths of a kilogram
    	bool fMetric = GameSettings.fOptions[TOPTION.USE_METRIC_SYSTEM];
    	double dWeight = Item[ usIndex ].ubWeight / 10.0 * (fMetric ? 1.0 : 2.2);
    	string sTemp = wprintf( "%3.2f %s", dWeight, fMetric ? "kg" : "lbs" );
    	FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEM_WEIGHT_NUM_X, usPosY), BOBBYR_ITEM_WEIGHT_NUM_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	usPosY += usFontHeight + 2;
    	return(usPosY);
    }

    private static void DisplayItemNameAndInfo(int usPosY, Items usIndex, int usBobbyIndex, bool fUsed)
    {
    	string sText = string.Empty;
    	string sTemp = string.Empty;
    	int	uiStartLoc=0;

    	int	ubPurchaseNumber;
    	int usFontHeight = FontSubSystem.GetFontHeight(BOBBYR_ITEM_DESC_TEXT_FONT);

    		//Display Items Name
    	uiStartLoc = BOBBYR_ITEM_DESC_FILE_SIZE * (int)usIndex;
    	files.LoadEncryptedDataFromFile(BOBBYRDESCFILE, out sText, uiStartLoc, BOBBYR_ITEM_DESC_NAME_SIZE);

    	if( FontSubSystem.StringPixLength( sText, BOBBYR_ITEM_NAME_TEXT_FONT ) > ( BOBBYR_GRID_PIC_WIDTH - 6 ) )
    		WordWrap.ReduceStringLength( ref sText, BOBBYR_GRID_PIC_WIDTH - 6, BOBBYR_ITEM_NAME_TEXT_FONT );
	
    	FontSubSystem.DrawTextToScreen(sText, new(BOBBYR_ITEM_NAME_X, (int)(usPosY+BOBBYR_ITEM_NAME_Y_OFFSET)), 0, BOBBYR_ITEM_NAME_TEXT_FONT, BOBBYR_ITEM_NAME_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//number bought
    	//Display the # bought
    	ubPurchaseNumber = CheckIfItemIsPurchased(usBobbyIndex);
    	if( ubPurchaseNumber != BOBBY_RAY_NOT_PURCHASED)
    	{
    		FontSubSystem.DrawTextToScreen(BobbyRText[BOBBYR_GUNS_QTY_ON_ORDER], new(BOBBYR_ITEM_QTY_TEXT_X, (int)usPosY), BOBBYR_ITEM_QTY_WIDTH, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_STATIC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

    		if( ubPurchaseNumber != BOBBY_RAY_NOT_PURCHASED)
    		{
    			sTemp = wprintf( "% 4d", BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased);
    			FontSubSystem.DrawTextToScreen(sTemp, new(BOBBYR_ITEMS_BOUGHT_X, (int)usPosY), 0, FontStyle.FONT14ARIAL, BOBBYR_ITEM_DESC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    		}
    	}




    	//if it's a used item, display how damaged the item is
    	if( fUsed )
    	{
    		sTemp = wprintf( "*%3d%%%%", LaptopSaveInfo.BobbyRayUsedInventory[ usBobbyIndex ].ubItemQuality );
    		FontSubSystem.DrawTextToScreen(sTemp, new((int)(BOBBYR_ITEM_NAME_X-2), (int)(usPosY - BOBBYR_ORDER_NUM_Y_OFFSET)), BOBBYR_ORDER_NUM_WIDTH, BOBBYR_ITEM_NAME_TEXT_FONT, BOBBYR_ITEM_NAME_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}

    	//Display Items description
    	uiStartLoc = (BOBBYR_ITEM_DESC_FILE_SIZE * (int)usIndex) + BOBBYR_ITEM_DESC_NAME_SIZE;
    	files.LoadEncryptedDataFromFile(BOBBYRDESCFILE, out sText, uiStartLoc, BOBBYR_ITEM_DESC_INFO_SIZE);
    	FontSubSystem.DisplayWrappedString(new(BOBBYR_ITEM_DESC_START_X, usPosY), BOBBYR_ITEM_DESC_START_WIDTH, 2, BOBBYR_ITEM_DESC_TEXT_FONT, BOBBYR_ITEM_DESC_TEXT_COLOR, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    }

    private static void InitFirstAndLastGlobalIndex(IC uiItemClass)
    {
    	switch(uiItemClass)
    	{
    		case IC.BOBBY_GUN:
    			gusLastItemIndex = gusLastGunIndex ;
    			gusFirstItemIndex = gusFirstGunIndex;
    			gubNumPages = gubNumGunPages;
    			break;
    		case IC.BOBBY_MISC:
    			gusLastItemIndex = gusLastMiscIndex ;
    			gusFirstItemIndex = gusFirstMiscIndex;
    			gubNumPages = gubNumMiscPages;
    			break;
    		case IC.AMMO:
    			gusLastItemIndex = gusLastAmmoIndex ;
    			gusFirstItemIndex = gusFirstAmmoIndex;
    			gubNumPages = gubNumAmmoPages;
    			break;
    		case IC.ARMOUR:
    			gusLastItemIndex = gusLastArmourIndex;
    			gusFirstItemIndex = gusFirstArmourIndex;
    			gubNumPages = gubNumArmourPages;
    			break;
    		case BOBBYR_USED_ITEMS:
    			gusLastItemIndex = gusLastUsedIndex;
    			gusFirstItemIndex = gusFirstUsedIndex;
    			gubNumPages = gubNumUsedPages;
    			break;
    		default:
    			System.Diagnostics.Debug.Fail("unknown Bobby Ray item class");
    			break;
    	}
    }


    private static void SetFirstLastPagesForNew(IC uiClassMask)
    {
    	int i;
    	int sFirst = -1;
    	int sLast = -1;
    	int	ubPages=0;
    	int	ubNumItems=0;

    	gubCurPage = 0;

    	//First loop through to get the first and last index indexs
    	for(i=0; i<LaptopSaveInfo.BobbyRayInventory.Count; i++)
    	{
    		//If we have some of the inventory on hand
    		if( LaptopSaveInfo.BobbyRayInventory[ i ].ubQtyOnHand != 0 )
    		{
    			if( Item[ LaptopSaveInfo.BobbyRayInventory[ i ].usItemIndex ].usItemClass.HasFlag(uiClassMask) )
    			{
    				ubNumItems++;

    				if( sFirst == -1 )
    					sFirst = i;
    				sLast = i;
    			}
    		}
    	}

    	if( ubNumItems == 0 )
    	{
    		gusFirstItemIndex = (int)BOBBYR_NO_ITEMS;
    		gusLastItemIndex = (int)BOBBYR_NO_ITEMS;
    		gubNumPages = 0;
    		return;
    	}
	
    	gusFirstItemIndex = sFirst;
    	gusLastItemIndex = sLast;
    	gubNumPages = (int)( ubNumItems / (float)BOBBYR_NUM_WEAPONS_ON_PAGE );
    	if( (ubNumItems % BOBBYR_NUM_WEAPONS_ON_PAGE ) != 0 )
    		gubNumPages += 1;
    }

    private static void SetFirstLastPagesForUsed()
    {
    	int i;
    	int sFirst = -1;
    	int sLast = -1;
    	int	ubPages=0;
    	int	ubNumItems=0;

    	gubCurPage = 0;

    	//First loop through to get the first and last index indexs
    	for(i=0; i<LaptopSaveInfo.BobbyRayUsedInventory.Count; i++)
    	{
    		//If we have some of the inventory on hand
    		if( LaptopSaveInfo.BobbyRayUsedInventory[ i ].ubQtyOnHand != 0 )
    		{
    			ubNumItems++;

    			if( sFirst == -1 )
    				sFirst = i;
    			sLast = i;
    		}
    	}
    	if( sFirst == -1 )
    	{
    		gusFirstItemIndex = (int)BOBBYR_NO_ITEMS;
    		gusLastItemIndex = (int)BOBBYR_NO_ITEMS;
    		gubNumPages = 0;
    		return;
    	}

    	gusFirstItemIndex = sFirst;
    	gusLastItemIndex = sLast;
    	gubNumPages = (int)( ubNumItems / (float)BOBBYR_NUM_WEAPONS_ON_PAGE );
    	if( (ubNumItems % BOBBYR_NUM_WEAPONS_ON_PAGE ) != 0 )
    		gubNumPages += 1;
    }

    // note: pItemNumbers: pointer-to-primitive -> ref (use out if only written)
    private static void CreateMouseRegionForBigImage(int usPosY, int ubCount, int[] pItemNumbers)
    {
    	int	i;
    	string zItemName = string.Empty;
    	int			ubItemCount=0;

    	if( gfBigImageMouseRegionCreated )
    		return;

    	for(i=0; i<ubCount; i++)
    	{
    		//Mouse region for the Big Item Image
    		MouseSubSystem.MSYS_DefineRegion( gSelectedBigImageRegion[ i ], BOBBYR_GRID_PIC_X, usPosY, (BOBBYR_GRID_PIC_X + BOBBYR_GRID_PIC_WIDTH), (int)(usPosY + BOBBYR_GRID_PIC_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectBigImageRegionCallBack ); 
    		MouseSubSystem.MSYS_AddRegion( ref gSelectedBigImageRegion[ i ] ); 
    		MouseSubSystem.MSYS_SetRegionUserData( gSelectedBigImageRegion[ i ], 0, i );

    		//specify the help text only if the items is ammo
    		if( Item[ (Items)pItemNumbers[ i ] ].usItemClass == IC.AMMO )
    		{
    			//and only if the user has an item that can use the particular type of ammo
    			ubItemCount = CheckPlayersInventoryForGunMatchingGivenAmmoID( (Items)pItemNumbers[ i ] );
    			if( ubItemCount != 0 )
    			{				
    				zItemName = wprintf( "%s %d %s",BobbyRText[BOBBYR_GUNS_NUM_GUNS_THAT_USE_AMMO_1], ubItemCount, BobbyRText[BOBBYR_GUNS_NUM_GUNS_THAT_USE_AMMO_2] );
    			}
    			else
    				zItemName = string.Empty;
    		}
    		else
    			zItemName = string.Empty;

    		MouseSubSystem.SetRegionFastHelpText( gSelectedBigImageRegion[ i ], zItemName );
    		// TODO(port): SetRegionHelpEndCallback( gSelectedBigImageRegion[ i ], BobbyrRGunsHelpTextDoneCallBack );

    		usPosY += BOBBYR_GRID_OFFSET;
    	}

    	gubNumItemsOnScreen = ubCount;
    	gfBigImageMouseRegionCreated = true;
    }

    private static void DeleteMouseRegionForBigImage()
    {
    	int i;

    	if( !gfBigImageMouseRegionCreated )
    		return;

    	for(i=0; i<gubNumItemsOnScreen; i++)
    		MouseSubSystem.MSYS_RemoveRegion( gSelectedBigImageRegion[i] );

    	gfBigImageMouseRegionCreated = false;
    	gusOldItemNumOnTopOfPage = 65535;
    	gubNumItemsOnScreen = 0;
    }

    private static void SelectBigImageRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    	{

    	}
    	else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    	{
    		int usItemNum = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );

    		PurchaseBobbyRayItem( gusItemNumberForItemsOnScreen[ usItemNum] );

    		fReDrawScreenFlag = true;
    		fPausedReDrawScreenFlag = true;		
    	}
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    	{
    		int usItemNum = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );

    		UnPurchaseBobbyRayItem( gusItemNumberForItemsOnScreen[ usItemNum] );
    		fReDrawScreenFlag = true;
    		fPausedReDrawScreenFlag = true;		
    	}
    	else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT))
    	{
    		int usItemNum = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );

    		PurchaseBobbyRayItem( gusItemNumberForItemsOnScreen[ usItemNum] );
    		fReDrawScreenFlag = true;
    		fPausedReDrawScreenFlag = true;		
    	}
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_REPEAT))
    	{
    		int usItemNum = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );

    		UnPurchaseBobbyRayItem( gusItemNumberForItemsOnScreen[ usItemNum] );
    		fReDrawScreenFlag = true;
    		fPausedReDrawScreenFlag = true;		
    	}
    }

    private static void PurchaseBobbyRayItem(int usItemNumber)
    {
    	int	ubPurchaseNumber;

    	ubPurchaseNumber = CheckIfItemIsPurchased(usItemNumber);

    	//if we are in the used page
    	if( guiCurrentLaptopMode == LAPTOP_MODE.BOBBY_R_USED )
    	{
    		//if there is enough inventory in stock to cover the purchase
    		if( ubPurchaseNumber == BOBBY_RAY_NOT_PURCHASED || LaptopSaveInfo.BobbyRayUsedInventory[ usItemNumber ].ubQtyOnHand >= ( BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased + 1) )
    		{
    			// If the item has not yet been purchased
    			if( ubPurchaseNumber == BOBBY_RAY_NOT_PURCHASED )
    			{
    				ubPurchaseNumber = GetNextPurchaseNumber();

    				if( ubPurchaseNumber != BOBBY_RAY_NOT_PURCHASED )
    				{
    					BobbyRayPurchases[ ubPurchaseNumber ].usItemIndex = (int)LaptopSaveInfo.BobbyRayUsedInventory[ usItemNumber ].usItemIndex;
    					BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased = 1;
    					BobbyRayPurchases[ ubPurchaseNumber ].bItemQuality = LaptopSaveInfo.BobbyRayUsedInventory[ usItemNumber ].ubItemQuality;
    					BobbyRayPurchases[ ubPurchaseNumber ].usBobbyItemIndex = usItemNumber;
    					BobbyRayPurchases[ ubPurchaseNumber ].fUsed = true;
    				}
    				else
    				{
    					//display error popup because the player is trying to purchase more thenn 10 items
    					Laptop.DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, BobbyRText[ BOBBYR_MORE_THEN_10_PURCHASES ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, null);

    				}
    			}
    			// Else If the item is already purchased increment purchase amount.  Only if ordering less then the max amount!
    			else
    			{
    				if( BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased <= BOBBY_RAY_MAX_AMOUNT_OF_ITEMS_TO_PURCHASE)
    					BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased++;
    			}
    		}
    		else
    		{
    			Laptop.DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, BobbyRText[ BOBBYR_MORE_NO_MORE_IN_STOCK ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, null);

    // #ifdef JA2BETAVERSION
    // ReportBobbyROrderError( usItemNumber, ubPurchaseNumber, LaptopSaveInfo.BobbyRayUsedInventory[ usItemNumber ].ubQtyOnHand, BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased );
    // #endif
    		}
    	}
    	//else the player is on a any other page except the used page
    	else
    	{
    		//if there is enough inventory in stock to cover the purchase
    		if( ubPurchaseNumber == BOBBY_RAY_NOT_PURCHASED || LaptopSaveInfo.BobbyRayInventory[ usItemNumber ].ubQtyOnHand >= ( BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased + 1) )
    		{
    			// If the item has not yet been purchased
    			if( ubPurchaseNumber == BOBBY_RAY_NOT_PURCHASED )
    			{
    				ubPurchaseNumber = GetNextPurchaseNumber();

    				if( ubPurchaseNumber != BOBBY_RAY_NOT_PURCHASED )
    				{
    					BobbyRayPurchases[ ubPurchaseNumber ].usItemIndex = (int)LaptopSaveInfo.BobbyRayInventory[ usItemNumber ].usItemIndex;
    					BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased = 1;
    					BobbyRayPurchases[ ubPurchaseNumber ].bItemQuality = 100;
    					BobbyRayPurchases[ ubPurchaseNumber ].usBobbyItemIndex = usItemNumber;
    					BobbyRayPurchases[ ubPurchaseNumber ].fUsed = false;
    				}
    				else
    				{
    					//display error popup because the player is trying to purchase more thenn 10 items
    					Laptop.DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, BobbyRText[ BOBBYR_MORE_THEN_10_PURCHASES ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, null);
    				}
    			}
    			// Else If the item is already purchased increment purchase amount.  Only if ordering less then the max amount!
    			else
    			{
    				if( BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased <= BOBBY_RAY_MAX_AMOUNT_OF_ITEMS_TO_PURCHASE)
    					BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased++;
    			}
    		}
    		else
    		{
    			Laptop.DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, BobbyRText[ BOBBYR_MORE_NO_MORE_IN_STOCK ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, null);

    // #ifdef JA2BETAVERSION
    // ReportBobbyROrderError( usItemNumber, ubPurchaseNumber, LaptopSaveInfo.BobbyRayInventory[ usItemNumber ].ubQtyOnHand, BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased );
    // #endif
    		}
    	}
    }

    private static int CheckIfItemIsPurchased(int usItemNumber)
    {
    	int	i;

    	for(i=0; i<MAX_PURCHASE_AMOUNT; i++)
    	{
    		if( ( usItemNumber == BobbyRayPurchases[i].usBobbyItemIndex ) && ( BobbyRayPurchases[i].ubNumberPurchased != 0 ) && ( BobbyRayPurchases[i].fUsed == gfOnUsedPage ) )
    			return(i);
    	}
    	return(BOBBY_RAY_NOT_PURCHASED);
    }

    private static int GetNextPurchaseNumber()
    {
    	int	i;

    	for(i=0; i<MAX_PURCHASE_AMOUNT; i++)
    	{
    		if( ( BobbyRayPurchases[i].usBobbyItemIndex == 0) && ( BobbyRayPurchases[i].ubNumberPurchased == 0 ) )
    			return(i);
    	}
    	return(BOBBY_RAY_NOT_PURCHASED);
    }

    private static void UnPurchaseBobbyRayItem(int usItemNumber)
    {
    	int	ubPurchaseNumber;

    	ubPurchaseNumber = CheckIfItemIsPurchased(usItemNumber);

    	if( ubPurchaseNumber != BOBBY_RAY_NOT_PURCHASED )
    	{
    		if( BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased > 1)
    			BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased--;
    		else
    		{
    			BobbyRayPurchases[ ubPurchaseNumber ].ubNumberPurchased = 0;
    			BobbyRayPurchases[ ubPurchaseNumber ].usBobbyItemIndex = 0;
    		}
    	}
    }

    private static void BtnBobbyROrderFormCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		guiCurrentLaptopMode = LAPTOP_MODE.BOBBY_R_MAILORDER;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }

    private static void BtnBobbyRHomeButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		guiCurrentLaptopMode = LAPTOP_MODE.BOBBY_R;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }

    private static void UpdateButtonText(LAPTOP_MODE uiCurPage)
    {
    	switch( uiCurPage )
    	{
    		case LAPTOP_MODE.BOBBY_R_GUNS:
    			ButtonSubSystem.DisableButton( guiBobbyRPageMenu[0] );
    			break;

    		case LAPTOP_MODE.BOBBY_R_AMMO:
    			ButtonSubSystem.DisableButton( guiBobbyRPageMenu[1] );
    			break;

    		case LAPTOP_MODE.BOBBY_R_ARMOR:
    			ButtonSubSystem.DisableButton( guiBobbyRPageMenu[2] );
    			break;

    		case LAPTOP_MODE.BOBBY_R_MISC:
    			ButtonSubSystem.DisableButton( guiBobbyRPageMenu[3] );
    			break;

    		case LAPTOP_MODE.BOBBY_R_USED:
    			ButtonSubSystem.DisableButton( guiBobbyRPageMenu[4] );
    			break;
    	}
    }

    private static int CalcBobbyRayCost(Items usIndex, int usBobbyIndex, bool fUsed)
    {
    	double value;
    	if( fUsed )
    		value = Item[ LaptopSaveInfo.BobbyRayUsedInventory[ usBobbyIndex ].usItemIndex ].usPrice * 
    								( .5 + .5 * ( LaptopSaveInfo.BobbyRayUsedInventory[ usBobbyIndex ].ubItemQuality ) / 100 ) + .5;
    	else
    		value = Item[ LaptopSaveInfo.BobbyRayInventory[ usBobbyIndex ].usItemIndex ].usPrice;

    	return( (int) value);
    }

    private static int CalculateTotalPurchasePrice()
    {
    	int	i;
    	int	uiTotal = 0;

    	for(i=0; i<MAX_PURCHASE_AMOUNT; i++)
    	{
    		//if the item was purchased
    		if( BobbyRayPurchases[ i ].ubNumberPurchased != 0 )
    		{
    			uiTotal += CalcBobbyRayCost( (Items)BobbyRayPurchases[ i ].usItemIndex, BobbyRayPurchases[ i ].usBobbyItemIndex, BobbyRayPurchases[ i ].fUsed ) * BobbyRayPurchases[ i ].ubNumberPurchased;
    		}
    	}

    	return(uiTotal);
    }

    private static void DisableBobbyRButtons()
    {
    	//if it is the last page, disable the next page button
    	if( gubNumPages == 0 )
    		ButtonSubSystem.DisableButton( guiBobbyRNextPage );
    	else
    	{
    		if( gubCurPage >= gubNumPages-1 )
    			ButtonSubSystem.DisableButton( guiBobbyRNextPage );
    		else
    			ButtonSubSystem.EnableButton( guiBobbyRNextPage );
    	}
		

    	// if it is the first page, disable the prev page buitton
    	if( gubCurPage == 0 )
    		ButtonSubSystem.DisableButton( guiBobbyRPreviousPage );
    	else
    		ButtonSubSystem.EnableButton( guiBobbyRPreviousPage );

    /*
    	//if it is the last page, disable the next page button
    	if( !(gusCurWeaponIndex < (gusLastItemIndex - BOBBYR_NUM_WEAPONS_ON_PAGE) ) )
    		ButtonSubSystem.DisableButton( guiBobbyRNextPage );
    	else
    		ButtonSubSystem.EnableButton( guiBobbyRNextPage );
		

    	// if it is the first page, disable the prev page buitton
    	if( (gusCurWeaponIndex == gusFirstItemIndex ) )
    		ButtonSubSystem.DisableButton( guiBobbyRPreviousPage );
    	else
    		ButtonSubSystem.EnableButton( guiBobbyRPreviousPage );
    */
    }

    private static void CalcFirstIndexForPage(List<STORE_INVENTORY> pInv, IC uiItemClass)
    {
    	int	i;
    	int	usNumItems=0;

    	//Reset the Current weapon Index
    	gusCurWeaponIndex = 0;

    	if( uiItemClass == BOBBYR_USED_ITEMS )
    	{
    		//Get to the first index on the page
    		for(i=gusFirstItemIndex; i<=gusLastItemIndex; i++)
    		{
    			//If we have some of the inventory on hand
    			if( pInv[ i ].ubQtyOnHand != 0 )
    			{
    				if( gubCurPage == 0 )
    				{
    					gusCurWeaponIndex = i;
    					break;
    				}

    				if( usNumItems <= ( gubCurPage * 4 ) )
    					gusCurWeaponIndex = i;

    				usNumItems++;
    			}
    		}
    	}
    	else
    	{
    		//Get to the first index on the page
    		for(i=gusFirstItemIndex; i<=gusLastItemIndex; i++)
    		{
    			if( Item[ pInv[ i ].usItemIndex ].usItemClass.HasFlag(uiItemClass) )
    			{
    				//If we have some of the inventory on hand
    				if( pInv[ i ].ubQtyOnHand != 0 )
    				{
    					if( gubCurPage == 0 )
    					{
    						gusCurWeaponIndex = i;
    						break;
    					}

    					if( usNumItems <= ( gubCurPage * 4 ) )
    						gusCurWeaponIndex = i;

    					usNumItems++;
    				}
    			}
    		}
    	}
    }

    private static void OutOfStockMessageBoxCallBack(MessageBoxReturnCode bExitValue)
    {
    	// yes, load the game
      if( bExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_OK )
    	{
    //		guiCurrentLaptopMode  = LAPTOP_MODE_BOBBY_R;
    	}
    }

    private static int CheckPlayersInventoryForGunMatchingGivenAmmoID(Items sItemID)
    {
    	int	ubItemCount=0;
    	int	ubMercCount;
    	InventorySlot ubPocketCount;

    	int	ubFirstID = gTacticalStatus.Team[ OUR_TEAM ].bFirstID;
    	int	ubLastID = gTacticalStatus.Team[ OUR_TEAM ].bLastID;


    	//loop through all the mercs on the team
    	for( ubMercCount = ubFirstID; ubMercCount <= ubLastID; ubMercCount++ )
    	{
    		if( Menptr[ ubMercCount ].bActive )
    		{
    			//loop through all the pockets on the merc
    			for( ubPocketCount=0; ubPocketCount<NUM_INV_SLOTS; ubPocketCount++)
    			{
    				//if there is a weapon here
    				if( Item[ Menptr[ ubMercCount ].inv[ ubPocketCount ].usItem ].usItemClass == IC.GUN )
    				{
    					//if the weapon uses the same kind of ammo as the one passed in, return true
    					if( WeaponTypes.Weapon[ Menptr[ ubMercCount ].inv[ ubPocketCount ].usItem ].ubCalibre == WeaponTypes.Magazine[ Item[ sItemID ].ubClassIndex ].ubCalibre )
    					{
    						ubItemCount++;
    					}
    				}
    			}
    		}
    	}

    	return( ubItemCount );
    }

    private static void BobbyrRGunsHelpTextDoneCallBack()
    {
    	fReDrawScreenFlag = true;
    	fPausedReDrawScreenFlag = true;		
    }

    private static void ReportBobbyROrderError(int usItemNumber, int ubPurchaseNum, int ubQtyOnHand, int ubNumPurchasing)
    {
    }
}
