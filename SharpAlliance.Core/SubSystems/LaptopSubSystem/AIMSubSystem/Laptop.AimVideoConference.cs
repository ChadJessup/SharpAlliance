// The video conference of AimMembers.c: the popup with the merc, the contract length / equipment choices, the
// authorize-payment flow and hiring. The face, dialogue and video effect code it relies on is not ported, so the
// merc does not speak or show a talking face, and the connecting / static / title bar animations are stubbed with
// TODO(port) markers (search "TODO(port)" in this file).
using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{

    public static bool RenderAIMMembersTopLevel()
    {
    	InitCreateDeleteAimPopUpBox( AIM_POPUP.DISPLAY, null, null, 0, 0, 0);

    	return(true);
    }


    private static void BtnContractLengthButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		int	ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );

    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

    		gubContractLength = ubRetValue;
    		DisplaySelectLights(true, false);
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    		DisplaySelectLights(false, false);

    		guiMercAttitudeTime = GetJA2Clock();

    		DisplayMercChargeAmount();
    		ButtonSubSystem.InvalidateButtonArea(btn);

    //		InvalidateRegion(btn->Area.RegionTopLeftX, btn->Area.RegionTopLeftY, btn->Area.RegionBottomRightX, btn->Area.RegionBottomRightY);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnBuyEquipmentButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		gfBuyEquipment = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 ) != 0;
    		DisplaySelectLights(false, true);

    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		DisplaySelectLights(false, false);
    		DisplayMercChargeAmount();

    		guiMercAttitudeTime = GetJA2Clock();

    //		InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);

    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnAuthorizeButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    		return;

    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		int	ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    		gfStopMercFromTalking = true;
    		gubMercAttitudeLevel = QUOTE_DELAY_NO_ACTION;

    		//If we try to hire the merc
    		if( ubRetValue == 0)
    		{
    			StopMercTalking();

    			//can the merc be hired?  (does he like/not like people on the team
    //			if( CanMercBeHired() )
    			{
    				//Was the merc hired
    				if( AimMemberHireMerc() )
    				{
    					// if merc was hired
    					InitCreateDeleteAimPopUpBox(AIM_POPUP.CREATE, AimPopUpText[AIM_MEMBER_FUNDS_TRANSFER_SUCCESFUL], null, AIM_POPUP_BOX_X, AIM_POPUP_BOX_Y, AIM_POPUP_BOX_SUCCESS);
    					DelayMercSpeech( gbCurrentSoldier, QUOTE.CONTRACT_ACCEPTANCE, 750, true, false );

    					//Disable the buttons behind the message box
    					EnableDisableCurrentVideoConferenceButtons( true );

    					ButtonSubSystem.SpecifyDisabledButtonStyle( giBuyEquipmentButton[0], DISABLED_STYLE.NONE );
    					ButtonSubSystem.SpecifyDisabledButtonStyle( giBuyEquipmentButton[1], DISABLED_STYLE.NONE );

    					giIdOfLastHiredMerc = AimMercArray[gbCurrentIndex];
    				}
    			}
    /*
    			else
    			{
    				//else the merc doesnt like a player on the team, hang up when the merc is done complaining

    				//reset ( in case merc was going to say something
    				DelayMercSpeech( 0, (QUOTE)0, 0, false, true );

    				gubVideoConferencingMode = AIM_VIDEO.HIRE_MERC_MODE;
    			}
    */
    		}
    		// else we cancel
    		else
    		{
    			gubVideoConferencingMode = AIM_VIDEO.FIRST_CONTACT_MERC_MODE;
    		}
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static bool AimMemberHireMerc()
    {
    	MERC_HIRE_STRUCT HireMercStruct = new();
    	NPCID ubCurrentSoldier = (NPCID)AimMercArray[gbCurrentIndex];
    	int		bReturnCode;
    	int		sSoldierID=0;
    	int		bTypeOfContract=0;


    	if( LaptopSaveInfo.iCurrentBalance < giContractAmount )
    	{
    		//wasnt hired because of lack of funds
    		InitCreateDeleteAimPopUpBox(AIM_POPUP.CREATE, AimPopUpText[AIM_MEMBER_FUNDS_TRANSFER_FAILED], AimPopUpText[AIM_MEMBER_NOT_ENOUGH_FUNDS], AIM_POPUP_BOX_X, AIM_POPUP_BOX_Y, AIM_POPUP_BOX_FAILURE);

    		//Disable the buttons behind the message box
    		EnableDisableCurrentVideoConferenceButtons( true );

    		ButtonSubSystem.SpecifyDisabledButtonStyle( giBuyEquipmentButton[0], DISABLED_STYLE.NONE );
    		ButtonSubSystem.SpecifyDisabledButtonStyle( giBuyEquipmentButton[1], DISABLED_STYLE.NONE );

    		DelayMercSpeech( gbCurrentSoldier, QUOTE.REFUSAL_TO_JOIN_LACK_OF_FUNDS, 750, true, false );

    		return(false);
    	}

    	HireMercStruct.ubProfileID = ubCurrentSoldier;

    	//DEF: temp
    	HireMercStruct.sSectorX = gsMercArriveSectorX;
    	HireMercStruct.sSectorY = gsMercArriveSectorY;
    	HireMercStruct.fUseLandingZoneForArrival = true;
    	HireMercStruct.ubInsertionCode	= INSERTION_CODE.ARRIVING_GAME;

    	HireMercStruct.fCopyProfileItemsOver = gfBuyEquipment;
    	//if the players is buyibng the equipment
    	if( gfBuyEquipment )
    	{
    		gMercProfiles[(NPCID)(ubCurrentSoldier)].ubMiscFlags |= PROFILE_MISC_FLAG.ALREADY_USED_ITEMS;
    	}

    		//If 1 day
    	if( gubContractLength == AIM_CONTRACT_LENGTH_ONE_DAY)
    	{
    		bTypeOfContract = CONTRACT_EXTEND_1_DAY;
    		HireMercStruct.iTotalContractLength = 1;
    	}
    	else if( gubContractLength == AIM_CONTRACT_LENGTH_ONE_WEEK)
    	{
    		bTypeOfContract = CONTRACT_EXTEND_1_WEEK;
    		HireMercStruct.iTotalContractLength = 7;
    	}
    	else if( gubContractLength == AIM_CONTRACT_LENGTH_TWO_WEEKS)
    	{
    		bTypeOfContract = CONTRACT_EXTEND_2_WEEK;
    		HireMercStruct.iTotalContractLength = 14;
    	}

    	//specify when the merc should arrive
    	HireMercStruct.uiTimeTillMercArrives = MercHiring.GetMercArrivalTimeOfDay( );// + ubCurrentSoldier

    	//Set the time and ID of the last hired merc will arrive
    //	LaptopSaveInfo.sLastHiredMerc.iIdOfMerc = HireMercStruct.ubProfileID;
    //	LaptopSaveInfo.sLastHiredMerc.uiArrivalTime = HireMercStruct.uiTimeTillMercArrives;

    	//if we succesfully hired the merc
    	bReturnCode = MercHiring.HireMerc( HireMercStruct );
    	if( bReturnCode == MercHiring.MERC_HIRE_OVER_20_MERCS_HIRED )
    	{
    		//display a warning saying u cant hire more then 20 mercs
    		DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, AimPopUpText[ AIM_MEMBER_ALREADY_HAVE_20_MERCS ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, null);
    		return(false);
    	}
    	else if( bReturnCode == MercHiring.MERC_HIRE_FAILED )
    	{
    		return(false);
    	}

    	//Set the type of contract the merc is on
    	SOLDIERTYPE? pHired = SoldierProfileSubSystem.FindSoldierByProfileID( ubCurrentSoldier, true );
    	if( pHired is not null )
    		pHired.bTypeOfLastContract = bTypeOfContract;  // TODO(port): the soldier is created once TacticalCreateSoldier is ported

	
    	//add an entry in the finacial page for the hiring of the merc
    	finances.AddTransactionToPlayersBook(FinanceEvent.HIRED_MERC, ubCurrentSoldier, GameClock.GetWorldTotalMin(), -( giContractAmount - gMercProfiles[(NPCID)(gbCurrentSoldier)].sMedicalDepositAmount ) );
	
    	if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedicalDeposit != 0 )
    	{
    		//add an entry in the finacial page for the medical deposit
    		finances.AddTransactionToPlayersBook(	FinanceEvent.MEDICAL_DEPOSIT, ubCurrentSoldier, GameClock.GetWorldTotalMin(), -(gMercProfiles[(NPCID)(gbCurrentSoldier)].sMedicalDepositAmount) );
    	}

    	//add an entry in the history page for the hiring of the merc
    	History.AddHistoryToPlayersLog(HISTORY.HIRED_MERC_FROM_AIM, ubCurrentSoldier, GameClock.GetWorldTotalMin(), -1, (MAP_ROW)(-1) );
    	return(true);
    }


    private static bool DisplayVideoConferencingDisplay()
    {
    	string sMercName = string.Empty;

    	if( ( gubVideoConferencingMode == AIM_VIDEO.NOT_DISPLAYED_MODE ) || ( gubVideoConferencingMode == AIM_VIDEO.POPUP_MODE ) )
    		return(false);

    	DisplayMercsVideoFace();

    	//Title & Name
    	if( gubVideoConferencingMode == AIM_VIDEO.INIT_MODE)
    	{
    		sMercName = wprintf( "%s",  VideoConfercingText[AIM_MEMBER_CONNECTING]);
    		FontSubSystem.DrawTextToScreen(sMercName, new(AIM_MEMBER_VIDEO_NAME_X, AIM_MEMBER_VIDEO_NAME_Y), 0, FontStyle.FONT12ARIAL, AIM_M_VIDEO_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}
    	else
    	{
    		sMercName = wprintf( "%s %s",  VideoConfercingText[AIM_MEMBER_VIDEO_CONF_WITH], gMercProfiles[(NPCID)(gbCurrentSoldier)].zName);
    		FontSubSystem.DrawTextToScreen(sMercName, new(AIM_MEMBER_VIDEO_NAME_X, AIM_MEMBER_VIDEO_NAME_Y), 0, FontStyle.FONT12ARIAL, AIM_M_VIDEO_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}

    	//Display Contract charge text
    	if( gubVideoConferencingMode == AIM_VIDEO.HIRE_MERC_MODE )
    	{
    		// Display the contract charge
    		FontSubSystem.SetFontShadow(AIM_M_VIDEO_NAME_SHADOWCOLOR);	
    		FontSubSystem.DrawTextToScreen(VideoConfercingText[AIM_MEMBER_CONTRACT_CHARGE], new(AIM_CONTRACT_CHARGE_X, AIM_CONTRACT_CHARGE_Y), 0, FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    		FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    	}

    	DisplayMercChargeAmount();

    //	if( gfMercIsTalking && !gfIsAnsweringMachineActive)
    	// TODO(port): the subtitles (PrepareMercPopupBox / RenderMercPopUpBoxFromIndex are not ported)

      video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);

    	return(true);
    }


    private static bool DisplayMercsVideoFace()
    {
      HVOBJECT	hTerminalHandle;
    	string				sFaceLoc = "LibraryNames.FACES\\";

    	// Get and Blt Terminal Frame
    	hTerminalHandle = guiVideoConfTerminal;
    	// TODO(port): ShadowVideoSurfaceImage (the shadow behind the terminal)
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hTerminalHandle, 0,AIM_MEMBER_VIDEO_CONF_TERMINAL_X, AIM_MEMBER_VIDEO_CONF_TERMINAL_Y, VO_BLT.SRCTRANSPARENCY);


    	//Display the Select light on the merc
    	if(gubVideoConferencingMode == AIM_VIDEO.HIRE_MERC_MODE)
    		DisplaySelectLights(false, false);

    	return(true);
    }


    private static void DisplaySelectLights(bool fContractDown, bool fBuyEquipDown)
    {
    	int i, usPosY, usPosX;

    	//First draw the select light for the contract length buttons
    	usPosY = AIM_MEMBER_BUY_CONTRACT_LENGTH_Y;
    	for(i=0; i<3; i++)
    	{
    		// if the if is true, the light is on
    		if( gubContractLength == i)
    		{
    			if( fContractDown)
    			{
    				usPosX = AIM_MEMBER_BUY_CONTRACT_LENGTH_X + AIM_SELECT_LIGHT_ON_X;
    				video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_ON_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_ON_Y+8), new Rgba32(0, 255, 0));
    			}
    			else
    			{
    				usPosX = AIM_MEMBER_BUY_CONTRACT_LENGTH_X + AIM_SELECT_LIGHT_OFF_X;
    				video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_OFF_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_OFF_Y+8), new Rgba32(0, 255, 0));
    			}
    		}
    		else
    		{
    			usPosX = AIM_MEMBER_BUY_CONTRACT_LENGTH_X + AIM_SELECT_LIGHT_OFF_X;
    			video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_OFF_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_OFF_Y+8), new Rgba32(0, 0, 0));
    		}
    		usPosY += AIM_MEMBER_BUY_EQUIPMENT_GAP;
    	}

    	//draw the select light for the buy equipment buttons
    	usPosY = AIM_MEMBER_BUY_CONTRACT_LENGTH_Y;
    	for(i=0; i<2; i++)
    	{
    		if( (gfBuyEquipment ? 1 : 0) == i)
    		{
    			if( fBuyEquipDown)
    			{
    				usPosX = AIM_MEMBER_BUY_EQUIPMENT_X + AIM_SELECT_LIGHT_ON_X;
    				video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_ON_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_ON_Y+8), new Rgba32(0, 255, 0));
    			}
    			else
    			{
    				usPosX = AIM_MEMBER_BUY_EQUIPMENT_X + AIM_SELECT_LIGHT_OFF_X;
    				video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_OFF_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_OFF_Y+8), new Rgba32(0, 255, 0));
    			}
    		}
    		else
    		{
    			usPosX = AIM_MEMBER_BUY_EQUIPMENT_X + AIM_SELECT_LIGHT_OFF_X;
    			video.ColorFillVideoSurfaceArea(SurfaceType.FRAME_BUFFER, Rectangle.FromLTRB(usPosX, usPosY+AIM_SELECT_LIGHT_OFF_Y, usPosX+8, usPosY+AIM_SELECT_LIGHT_OFF_Y+8), new Rgba32(0, 0, 0));
    		}
    		usPosY += AIM_MEMBER_BUY_EQUIPMENT_GAP;
    	}
      video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    }


    private static int DisplayMercChargeAmount()
    {
    	string wTemp = string.Empty;
    	string wDollarTemp = string.Empty;
    	HVOBJECT hImageHandle;


    	if( gubVideoConferencingMode != AIM_VIDEO.HIRE_MERC_MODE )
    		return(0);

    	// Display the 'black hole'for the contract charge  in the video conference terminal
    	hImageHandle = guiVideoContractCharge;
    	video.BltVideoObject(SurfaceType.FRAME_BUFFER, hImageHandle, 0,AIM_MEMBER_VIDEO_CONF_CONTRACT_IMAGE_X, AIM_MEMBER_VIDEO_CONF_CONTRACT_IMAGE_Y, VO_BLT.SRCTRANSPARENCY);

	
    	if( SoldierProfileSubSystem.FindSoldierByProfileID( (NPCID)gbCurrentSoldier, true ) == null )
    	{
    		giContractAmount = 0;

    		//the contract charge amount

    		// Get the salary rate
    		if( gubContractLength == AIM_CONTRACT_LENGTH_ONE_DAY)
    			giContractAmount = gMercProfiles[(NPCID)(gbCurrentSoldier)].sSalary;

    		else if( gubContractLength == AIM_CONTRACT_LENGTH_ONE_WEEK)
    			giContractAmount = gMercProfiles[(NPCID)(gbCurrentSoldier)].uiWeeklySalary;

    		else if( gubContractLength == AIM_CONTRACT_LENGTH_TWO_WEEKS)
    			giContractAmount = gMercProfiles[(NPCID)(gbCurrentSoldier)].uiBiWeeklySalary;

    		// if there is a medical deposit, add it in
    		if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedicalDeposit != 0 )
    		{
    			giContractAmount += gMercProfiles[(NPCID)(gbCurrentSoldier)].sMedicalDepositAmount;
    		}

    		//If hired with the equipment, add it in aswell
    		if( gfBuyEquipment )
    		{
    			giContractAmount += gMercProfiles[(NPCID)(gbCurrentSoldier)].usOptionalGearCost;
    		}
    	}

	
      wDollarTemp = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( giContractAmount ) );

    	//if the merc hasnt just been hired 
    //	if( FindSoldierByProfileID( gbCurrentSoldier, TRUE ) == NULL )
    	{
    		if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedicalDeposit != 0 )
    			wTemp = wprintf( "%s %s", wDollarTemp, VideoConfercingText[AIM_MEMBER_WITH_MEDICAL] );
    		else
    			wTemp = wprintf( "%s", wDollarTemp );

    		FontSubSystem.DrawTextToScreen(wTemp, new(AIM_CONTRACT_CHARGE_AMOUNNT_X+1, AIM_CONTRACT_CHARGE_AMOUNNT_Y+3), 0, AIM_M_VIDEO_CONTRACT_AMOUNT_FONT, AIM_M_VIDEO_CONTRACT_AMOUNT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}

    	return(giContractAmount);
    }


    // were function-local statics of InitCreateDeleteAimPopUpBox
    private static int usPopUpBoxPosX, usPopUpBoxPosY;
    private static string sPopUpString1 = string.Empty, sPopUpString2 = string.Empty;
    private static bool fPopUpBoxActive = false;

    private static bool InitCreateDeleteAimPopUpBox(AIM_POPUP ubFlag, string sString1, string sString2, int usPosX, int usPosY, int ubData)
    {
      HVOBJECT			hPopupBoxHandle;

    	switch( ubFlag )
    	{
    		case AIM_POPUP.CREATE:
    		{
    			if( fPopUpBoxActive )
    				return(false);

    			//Disable the 'X' to close the pop upi video
    			ButtonSubSystem.DisableButton( giXToCloseVideoConfButton );

    			sPopUpString1 = sString1 ?? string.Empty;

    			sPopUpString2 = sString2 ?? string.Empty;

    			usPopUpBoxPosX = usPosX;
    			usPopUpBoxPosY = usPosY;

    			// load the popup box graphic
    			guiPopUpBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\VideoConfPopUp.sti"));

    			hPopupBoxHandle = guiPopUpBox;
    			video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPopupBoxHandle, 0,usPosX, usPosY, VO_BLT.SRCTRANSPARENCY);

    			//Create the popup boxes button
    			guiPopUpImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,2,-1,3,-1 );
    			guiPopUpOkButton = ButtonSubSystem.CreateIconAndTextButton(guiPopUpImage, VideoConfercingText[AIM_MEMBER_OK], FontStyle.FONT14ARIAL, AIM_POPUP_BOX_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_POPUP_BOX_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point((int)(usPosX+AIM_POPUP_BOX_BUTTON_OFFSET_X), (int)(usPosY+AIM_POPUP_BOX_BUTTON_OFFSET_Y)), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH+5, MouseSubSystem.DefaultMoveCallback, BtnPopUpOkButtonCallback);
    			buttonList.Add(guiPopUpOkButton);
    			ButtonSubSystem.SetButtonCursor(guiPopUpOkButton, CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.MSYS_SetBtnUserData( guiPopUpOkButton, 0, ubData);

    			fPopUpBoxActive = true;
    			gubPopUpBoxAction = AIM_POPUP.DISPLAY;


    			// Disable the current video conference buttons
    			//EnableDisableCurrentVideoConferenceButtons(TRUE);
    			if( gubVideoConferencingPreviousMode == AIM_VIDEO.HIRE_MERC_MODE )
    			{
    				// Enable the current video conference buttons
    				EnableDisableCurrentVideoConferenceButtons(false);
    			}



    //
    //	Create a new flag for the PostButtonRendering function
    //
    			fReDrawPostButtonRender = true;
    		}
    		break;

    		case AIM_POPUP.DISPLAY:
    		{
    			int				usTempPosY = usPopUpBoxPosY;

    			if( gubPopUpBoxAction != AIM_POPUP.DISPLAY)
    				return(false);

    			// load and display the popup box graphic
    			hPopupBoxHandle = guiPopUpBox;
    			video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPopupBoxHandle, 0,usPopUpBoxPosX, usPopUpBoxPosY, VO_BLT.SRCTRANSPARENCY);

    			FontSubSystem.SetFontShadow(AIM_M_VIDEO_NAME_SHADOWCOLOR);	

    			usTempPosY += AIM_POPUP_BOX_STRING1_Y;
    			if( sPopUpString1.Length > 0)
    				usTempPosY += FontSubSystem.DisplayWrappedString(new(usPopUpBoxPosX, usTempPosY), AIM_POPUP_BOX_WIDTH, 2, AIM_POPUP_BOX_FONT, AIM_POPUP_BOX_COLOR, sPopUpString1, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    			if( sPopUpString2.Length > 0)
    				FontSubSystem.DisplayWrappedString(new(usPopUpBoxPosX, (int)(usTempPosY+4)), AIM_POPUP_BOX_WIDTH, 2, AIM_POPUP_BOX_FONT, AIM_POPUP_BOX_COLOR, sPopUpString2, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    			FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

    			video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);

    		}
    		break;

    		case AIM_POPUP.DELETE:
    		{
    			if( !fPopUpBoxActive )
    				return(false);

    			//Disable the 'X' to close the pop upi video
    			ButtonSubSystem.EnableButton( giXToCloseVideoConfButton );

    			ButtonSubSystem.UnloadButtonImage( guiPopUpImage );
    			buttonList.Remove(guiPopUpOkButton);
    			ButtonSubSystem.RemoveButton(guiPopUpOkButton);
    			video.DeleteVideoObject( guiPopUpBox );

    			fPopUpBoxActive = false;
    			gubPopUpBoxAction = AIM_POPUP.NOTHING;

    			if( gubVideoConferencingPreviousMode == AIM_VIDEO.HIRE_MERC_MODE )
    			{
    				// Enable the current video conference buttons
    				EnableDisableCurrentVideoConferenceButtons(false);
    			}
    			else if( gubVideoConferencingPreviousMode == AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE )
    			{
    				ButtonSubSystem.EnableButton( giAnsweringMachineButton[1]);
    			}
    		}
    		break;
    	}

    	return(true);
    }


    // was a function-local static of BtnPopUpOkButtonCallback
    private static bool BtnPopUpOkButtonCallback_fInCallback =true;
    private static void BtnPopUpOkButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {


    	if( BtnPopUpOkButtonCallback_fInCallback )
    	{
    		if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
    			return;

    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    		{
    			btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    		{
    			int ubCurPageNum = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );

    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			BtnPopUpOkButtonCallback_fInCallback = false;

    //			gfStopMercFromTalking = TRUE;
			
    			gubPopUpBoxAction = AIM_POPUP.DELETE; 

    			if( gubVideoConferencingMode != AIM_VIDEO.NOT_DISPLAYED_MODE )
    			{
    				if( ubCurPageNum == AIM_POPUP_BOX_SUCCESS)			
    				{
    					gubVideoConferencingMode = AIM_VIDEO.HIRE_MERC_MODE;
    					WaitForMercToFinishTalkingOrUserToClick();
    				}
    					//				gubVideoConferencingMode = AIM_VIDEO_POPDOWN_MODE;
    				else
    					gubVideoConferencingMode = AIM_VIDEO.HIRE_MERC_MODE;
    			}

    			BtnPopUpOkButtonCallback_fInCallback = true;

    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    }


    private static void BtnFirstContactButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			int	ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
	
    //			gfStopMercFromTalking = TRUE;
    			StopMercTalking();

    			gfAimMemberCanMercSayOpeningQuote = false;

    			if( ubRetValue == 0)
    			{
    				if( CanMercBeHired() )
    				{
    					gubVideoConferencingMode = AIM_VIDEO.HIRE_MERC_MODE;
    				}
    			}
    			else
    			{
    				gubVideoConferencingMode = AIM_VIDEO.POPDOWN_MODE;
    			}

    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    		ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnAnsweringMachineButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			int	ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
	
    			if( ubRetValue == 0)
    			{
    				//Set a flag indicating that the merc has a message
    				gMercProfiles[(NPCID)(gbCurrentSoldier)].ubMiscFlags3 |= PROFILE_MISC_FLAG3.PLAYER_LEFT_MSG_FOR_MERC_AT_AIM;
    				WaitForMercToFinishTalkingOrUserToClick();

    				//Display a message box displaying a messsage that the message was recorded
    //				DoLapTopMessageBox( 10, AimPopUpText[ AIM_MEMBER_MESSAGE_RECORDED ], ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG_OK, NULL );
    				InitCreateDeleteAimPopUpBox( AIM_POPUP.CREATE, " ", AimPopUpText[ AIM_MEMBER_MESSAGE_RECORDED ], AIM_POPUP_BOX_X, AIM_POPUP_BOX_Y, AIM_POPUP_BOX_SUCCESS );


    				ButtonSubSystem.SpecifyDisabledButtonStyle( giAnsweringMachineButton[1], DISABLED_STYLE.NONE );
    				ButtonSubSystem.DisableButton( giAnsweringMachineButton[1]);
    				ButtonSubSystem.DisableButton( giAnsweringMachineButton[0] );
    			}
    			else
    			{
    				gubVideoConferencingMode = AIM_VIDEO.POPDOWN_MODE;
    //				gubVideoConferencingMode = AIM_VIDEO_NOT_DISPLAYED_MODE;
    			}

    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnHangUpButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		video.InvalidateRegion(CONTACT_X,CONTACT_BOX_Y,CONTACT_BR_X,CONTACT_BR_Y);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    //			gubVideoConferencingMode = AIM_VIDEO_NOT_DISPLAYED_MODE;
    			gubVideoConferencingMode = AIM_VIDEO.POPDOWN_MODE;

    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    			video.InvalidateRegion(CONTACT_X,CONTACT_BOX_Y,CONTACT_BR_X,CONTACT_BR_Y);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		video.InvalidateRegion(CONTACT_X,CONTACT_BOX_Y,CONTACT_BR_X,CONTACT_BR_Y);
    	}
    }


    // The merc's face in the video conference. The original runs the face through the face system (so it can talk);
    // this shows the merc's still picture, stretched to fill the screen of the terminal.
    // TODO(port): the talking animation and voice (needs the face and dialogue systems).
    private static HVOBJECT? guiVideoFace;
    private static Image<Rgba32>? videoFaceStretched;

    private static bool InitVideoFace(int ubMercID)
    {
        ReleaseVideoFace();

        guiVideoFace = video.GetVideoObject(Utils.FilenameForBPP(wprintf("FACES\\%02d.sti", ubMercID)));
        videoFaceStretched = guiVideoFace.Images[0].Clone(ctx => ctx.Resize(
            AIM_MEMBER_VIDEO_FACE_WIDTH,
            AIM_MEMBER_VIDEO_FACE_HEIGHT,
            KnownResamplers.NearestNeighbor));

        gfVideoFaceActive = true;
        return true;
    }

    private static void ReleaseVideoFace()
    {
        if (guiVideoFace is not null)
        {
            video.DeleteVideoObject(guiVideoFace);
            guiVideoFace = null;
        }

        videoFaceStretched?.Dispose();
        videoFaceStretched = null;
    }


    private static bool InitVideoFaceTalking(int ubMercID, QUOTE usQuoteNum)
    {
        // TODO(port): merc speech (needs the dialogue system).
        return true;
    }


    private static bool DisplayTalkingMercFaceForVideoPopUp(int iFaceIndex)
    {
        // TODO(port): HandleDialogue / HandleTalkingAutoFaces and the face's talking state; the merc never talks
        if (videoFaceStretched is not null)
        {
            var frame = video.Surfaces[SurfaceType.FRAME_BUFFER];
            video.Blt16BPPTo16BPP(
                frame,
                videoFaceStretched,
                new Point(AIM_MEMBER_VIDEO_FACE_X, AIM_MEMBER_VIDEO_FACE_Y),
                new Point(0, 0),
                new Size(AIM_MEMBER_VIDEO_FACE_WIDTH, AIM_MEMBER_VIDEO_FACE_HEIGHT));

            //if the merc is not at home and the players is leaving a message, shade the players face
            if (gfIsAnsweringMachineActive)
            {
                VideoSurfaceManager.ShadowVideoSurfaceRect(
                    SurfaceType.FRAME_BUFFER,
                    Rectangle.FromLTRB(AIM_MEMBER_VIDEO_FACE_X, AIM_MEMBER_VIDEO_FACE_Y, AIM_MEMBER_VIDEO_FACE_X + AIM_MEMBER_VIDEO_FACE_WIDTH - 1, AIM_MEMBER_VIDEO_FACE_Y + AIM_MEMBER_VIDEO_FACE_HEIGHT - 1));

                //display a message over the mercs face
                FontSubSystem.DisplayWrappedString(
                    new(AIM_MEMBER_VIDEO_FACE_X, AIM_MEMBER_VIDEO_FACE_Y + 20),
                    AIM_MEMBER_VIDEO_FACE_WIDTH,
                    2,
                    FontStyle.FONT14ARIAL,
                    (FontColor)145,
                    AimPopUpText[AIM_MEMBER_PRERECORDED_MESSAGE],
                    FontColor.FONT_MCOLOR_BLACK,
                    TextJustifies.CENTER_JUSTIFIED);
            }

            video.InvalidateRegion(AIM_MEMBER_VIDEO_FACE_X, AIM_MEMBER_VIDEO_FACE_Y, AIM_MEMBER_VIDEO_FACE_X + AIM_MEMBER_VIDEO_FACE_WIDTH, AIM_MEMBER_VIDEO_FACE_Y + AIM_MEMBER_VIDEO_FACE_HEIGHT);
        }

        return false;
    }


    private static void DisplayTextForMercFaceVideoPopUp(string pString)
    {
        // TODO(port): subtitles (need the dialogue system).
    }


    private static void SelectShutUpMercRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	bool fInCallBack=true;

    	if(fInCallBack)
    	{
    		if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    		{
    		}
    		else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    		{
    			gfStopMercFromTalking = true;
    		} 
    		else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    		{
    			fInCallBack = false;

    			gfStopMercFromTalking = true;
    			fInCallBack = true;
    		} 
    	}
    }


    private static AIM_VIDEO WillMercAcceptCall()
    {
    	//if merc has hung up on the player twice within a period of time (MERC_ANNOYED_WONT_CONTACT_TIME_MINUTES )the merc cant ber hired
    	if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMercStatus == MercStatus.MERC_ANNOYED_WONT_CONTACT )
    	{
    		return(AIM_VIDEO.MERC_UNAVAILABLE_MODE);
    	}

    	//if the merc is currently on contract, the answering machine will pick up.
    	if( (gMercProfiles[(NPCID)(gbCurrentSoldier)].bMercStatus > 0 ) || (gMercProfiles[(NPCID)(gbCurrentSoldier)].bMercStatus == MercStatus.MERC_HAS_NO_TEXT_FILE  ) || (gMercProfiles[(NPCID)(gbCurrentSoldier)].bMercStatus == MercStatus.MERC_HIRED_BUT_NOT_ARRIVED_YET  ) )
    	{
    		return(AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE);
    	}

    	// if the merc is at home, or if the merc is only slightly annoyed at the player,  he will greet the player
    	if( IsMercHireable( gbCurrentSoldier ) )
    	{
    		return(AIM_VIDEO.FIRST_CONTACT_MERC_MODE);
    	}
    	else
    		return(AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE);
    }


    private static bool CanMercBeHired()
    {
        // TODO(port): the check of who the merc likes/dislikes on the team.
        return true;
    }


    private static bool DisplaySnowBackground()
    {
        // TODO(port): the snow animation while connecting.
        return true;
    }


    private static bool DisplayBlackBackground(int ubMaxNumOfLoops)
    {
        // TODO(port): the black screen animation while connecting.
        return true;
    }


    private static void HandleVideoDistortion()
    {
        // TODO(port): the video static / distortion effect.
    }


    private static int DisplayTransparentSnow(int ubMode, int uiImageIdentifier, int ubMaxImages, bool bForward)
    {
        // TODO(port): the snow animation.
        return 1;
    }


    private static int DisplayDistortionLine(int ubMode, int uiImageIdentifier, int ubMaxImages)
    {
        // TODO(port): the distortion animation.
        return 1;
    }


    private static int DisplayPixelatedImage(int ubMaxImages)
    {
        // TODO(port): the pixelated animation.
        return 1;
    }


    private static void HandleMercAttitude()
    {
        // TODO(port): the merc mood quotes (need the dialogue system).
    }


    private static void StopMercTalking()
    {
    	if( gfIsShutUpMouseRegionActive )
    	{
    	  MouseSubSystem.MSYS_DisableRegion( ref gSelectedShutUpMercRegion ); 

    		gfMercIsTalking = false;
    		guiMercAttitudeTime = GetJA2Clock();
    		gfIsShutUpMouseRegionActive = false;
    		gfRedrawScreen = true;
    	}
    }


    private static void BtnXToCloseVideoConfButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    	{
    		btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    	{
    		if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    		{
    			gubVideoConferencingMode = AIM_VIDEO.POPDOWN_MODE;
    //			gubVideoConferencingMode = AIM_VIDEO_NOT_DISPLAYED_MODE;
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    }


    // was a function-local static of InitDeleteVideoConferencePopUp
    private static bool InitDeleteVideoConferencePopUp_fXRegionActive = false;
    // was a function-local static of InitDeleteVideoConferencePopUp
    private static bool InitDeleteVideoConferencePopUp_fVideoConferenceCreated = false;
    private static bool InitDeleteVideoConferencePopUp()
    {


    	int	i;
    	int	usPosX, usPosY;
      VOBJECT_DESC    VObjectDesc;

    	//remove the face help text
    	gfAimMemberDisplayFaceHelpText = false;

    	//Gets reset to FALSE in the HandleCurrentVideoConfMode() function
    	gfJustSwitchedVideoConferenceMode = true;


    	//remove old mode
    	DeleteVideoConfPopUp();

    	//reset ( in case merc was going to say something
    	DelayMercSpeech( 0, (QUOTE)0, 0, false, true );

    	//if the video conferencing is currently displayed, put the 'x' to close it in the top right corner
    	//and disable the ability to click on the BIG face to go to different screen
    	if( ( gubVideoConferencingMode != AIM_VIDEO.NOT_DISPLAYED_MODE) && ( gubVideoConferencingMode != AIM_VIDEO.POPUP_MODE) )
    	{
    		if( !InitDeleteVideoConferencePopUp_fXRegionActive )
    		{
    			giXToCloseVideoConfButton = ButtonSubSystem.QuickCreateButton(giXToCloseVideoConfButtonImage, new Point(AIM_MEMBER_VIDEO_CONF_XCLOSE_X, AIM_MEMBER_VIDEO_CONF_XCLOSE_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnXToCloseVideoConfButtonCallback);
    			buttonList.Add(giXToCloseVideoConfButton);
    			ButtonSubSystem.SetButtonCursor(giXToCloseVideoConfButton, CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.SpecifyDisabledButtonStyle( giXToCloseVideoConfButton, DISABLED_STYLE.NONE );
    			InitDeleteVideoConferencePopUp_fXRegionActive = true;

    			MouseSubSystem.MSYS_DisableRegion( ref gSelectedFaceRegion ); 
    		}
    	}



    	// The video conference is not displayed
    	if( gubVideoConferencingMode ==  AIM_VIDEO.NOT_DISPLAYED_MODE )
    	{
    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;
    		gfRedrawScreen = true;

    		if( gfVideoFaceActive )
    		{
    			StopMercTalking();

    			//Get rid of the talking face
    			ReleaseVideoFace();
    		}

    		//if the ansering machine is currently on, turn it off
    		if( gfIsAnsweringMachineActive)
    			gfIsAnsweringMachineActive = false;

    		gfVideoFaceActive = false;

    		if( InitDeleteVideoConferencePopUp_fXRegionActive )
    		{

    			buttonList.Remove(giXToCloseVideoConfButton);

    			ButtonSubSystem.RemoveButton(giXToCloseVideoConfButton);
    			InitDeleteVideoConferencePopUp_fXRegionActive = false;
    		}

    		MouseSubSystem.MSYS_DisableRegion( ref gSelectedShutUpMercRegion ); 

    		//Enable the ability to click on the BIG face to go to different screen
    	  MouseSubSystem.MSYS_EnableRegion( gSelectedFaceRegion ); 

    //		EnableDisableCurrentVideoConferenceButtons(FALSE);
    			if( gubVideoConferencingPreviousMode == AIM_VIDEO.HIRE_MERC_MODE )
    			{
    				// Enable the current video conference buttons
    				EnableDisableCurrentVideoConferenceButtons(false);
    			}


    		InitDeleteVideoConferencePopUp_fVideoConferenceCreated = false;

    		Emails.fNewMailFlag = gfIsNewMailFlagSet;
    		gfIsNewMailFlagSet = false;

    	}


    	if( gubVideoConferencingMode == AIM_VIDEO.POPUP_MODE )
    	{

    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		if( gfJustSwitchedVideoConferenceMode )
    		{
    			HVOBJECT hImageHandle;


    			// load the answering machine graphic and add it

    			// Create a background video surface to blt the face onto


    			gfAimMemberCanMercSayOpeningQuote = true;


    		}
    	}


    	// The opening animation of the vc (fuzzy screen, then goes to black)
    	if( gubVideoConferencingMode == AIM_VIDEO.INIT_MODE )
    	{
    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;
     		gubMercAttitudeLevel = 0;
    		gubContractLength = AIM_CONTRACT_LENGTH_ONE_WEEK;

    		if( gMercProfiles[(NPCID)(gbCurrentSoldier)].usOptionalGearCost == 0 )
    			gfBuyEquipment = false;
    		else
    			gfBuyEquipment = true;

    		gfMercIsTalking = false;
    		gfVideoFaceActive = false;
    		guiLastHandleMercTime = 0;
    		gfHangUpMerc = false;

    		InitDeleteVideoConferencePopUp_fVideoConferenceCreated = true;
    	}

    	// The screen in which you first contact the merc, you have the option to hang up or goto hire merc screen
    	if( gubVideoConferencingMode ==  AIM_VIDEO.FIRST_CONTACT_MERC_MODE )
    	{
    		//if the last screen was the init screen, then we need to initialize the video face
    		if( ( gubVideoConferencingPreviousMode == AIM_VIDEO.INIT_MODE) || ( gubVideoConferencingPreviousMode == AIM_VIDEO.NOT_DISPLAYED_MODE) )
    		{
    			//Put the merc face up on the screen
    			InitVideoFace(gbCurrentSoldier);

    //			if( gubVideoConferencingPreviousMode == AIM_VIDEO_INIT_MODE)
    //				InitVideoFaceTalking(gbCurrentSoldier, QUOTE_GREETING);
    		}

    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		// Hang up button
    		usPosX = AIM_MEMBER_AUTHORIZE_PAY_X;
    		guiVideoConferenceButtonImage[2] = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,2,-1,3,-1 );
    		for(i=0; i<2; i++)
    		{
    			giAuthorizeButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[2], VideoConfercingText[i+AIM_MEMBER_HIRE], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_MEMBER_HANG_UP_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnFirstContactButtonCallback);
    			buttonList.Add(giAuthorizeButton[i]);
    			ButtonSubSystem.MSYS_SetBtnUserData( giAuthorizeButton[i], 0, i);
    			ButtonSubSystem.SetButtonCursor(giAuthorizeButton[i], CURSOR.LAPTOP_SCREEN);
    			usPosX += AIM_MEMBER_AUTHORIZE_PAY_GAP;
    		}

    		if( gfWaitingForMercToStopTalkingOrUserToClick )
    		{
    			ButtonSubSystem.DisableButton( giAuthorizeButton[0] );
    			gfWaitingForMercToStopTalkingOrUserToClick = false;

    			//Display a popup msg box telling the user when and where the merc will arrive
    //			DisplayPopUpBoxExplainingMercArrivalLocationAndTime();
    			giIdOfLastHiredMerc = -1;
    		}
    	}


		
    		// The screen in which you set the contract length, and the ability to buy equipment..
    	if( gubVideoConferencingMode == AIM_VIDEO.HIRE_MERC_MODE)
    	{
    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		// Contract Length button
    		guiVideoConferenceButtonImage[0] = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,0,-1,1,-1 );
    		usPosY = AIM_MEMBER_BUY_CONTRACT_LENGTH_Y;
    		for(i=0; i<3; i++)
    		{
    			giContractLengthButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[0], VideoConfercingText[i+AIM_MEMBER_ONE_DAY], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_LJUSTIFIED, new Point(AIM_MEMBER_BUY_CONTRACT_LENGTH_X, usPosY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnContractLengthButtonCallback);
    			buttonList.Add(giContractLengthButton[i]);
    			ButtonSubSystem.SetButtonCursor(giContractLengthButton[i], CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.MSYS_SetBtnUserData( giContractLengthButton[i], 0, i);
    			ButtonSubSystem.SpecifyDisabledButtonStyle( giContractLengthButton[i], DISABLED_STYLE.NONE );
    			usPosY += AIM_MEMBER_BUY_EQUIPMENT_GAP;
    		}

    		// BuyEquipment button
    		usPosY = AIM_MEMBER_BUY_CONTRACT_LENGTH_Y;
    		for(i=0; i<2; i++)
    		{
    			giBuyEquipmentButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[0], VideoConfercingText[i+AIM_MEMBER_NO_EQUIPMENT], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_LJUSTIFIED, new Point(AIM_MEMBER_BUY_EQUIPMENT_X, usPosY), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnBuyEquipmentButtonCallback);
    			buttonList.Add(giBuyEquipmentButton[i]);
    			ButtonSubSystem.SetButtonCursor(giBuyEquipmentButton[i], CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.MSYS_SetBtnUserData( giBuyEquipmentButton[i], 0, i);
    			ButtonSubSystem.SpecifyDisabledButtonStyle( giBuyEquipmentButton[i], DISABLED_STYLE.SHADED );
    			usPosY += AIM_MEMBER_BUY_EQUIPMENT_GAP;
    		}
    		if( gMercProfiles[(NPCID)(gbCurrentSoldier)].usOptionalGearCost == 0 )
    			ButtonSubSystem.DisableButton( giBuyEquipmentButton[1] );


    		// Authorize button
    		usPosX = AIM_MEMBER_AUTHORIZE_PAY_X;
    		guiVideoConferenceButtonImage[1] = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,2,-1,3,-1 );
    		for(i=0; i<2; i++)
    		{
    				giAuthorizeButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[1], VideoConfercingText[i+AIM_MEMBER_TRANSFER_FUNDS], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_MEMBER_AUTHORIZE_PAY_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnAuthorizeButtonCallback);
    				buttonList.Add(giAuthorizeButton[i]);
    			ButtonSubSystem.SetButtonCursor(giAuthorizeButton[i], CURSOR.LAPTOP_SCREEN);
    			ButtonSubSystem.MSYS_SetBtnUserData( giAuthorizeButton[i], 0, i);
    			ButtonSubSystem.SpecifyDisabledButtonStyle( giAuthorizeButton[i], DISABLED_STYLE.NONE );
    			usPosX += AIM_MEMBER_AUTHORIZE_PAY_GAP;
    		}

    //		InitVideoFaceTalking(gbCurrentSoldier, QUOTE_LENGTH_OF_CONTRACT);
    		DelayMercSpeech( gbCurrentSoldier, QUOTE.LENGTH_OF_CONTRACT, 750, true, false );
    	}




    		// The merc is not home and the player gets the answering machine
    	if( gubVideoConferencingMode ==  AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE )
    	{
    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		gfIsAnsweringMachineActive = true;

    		// Leave msg button
    		usPosX = AIM_MEMBER_AUTHORIZE_PAY_X;
    		guiVideoConferenceButtonImage[2] = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,2,-1,3,-1 );

    		giAnsweringMachineButton[0] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[2], VideoConfercingText[AIM_MEMBER_LEAVE_MESSAGE], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_MEMBER_HANG_UP_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnAnsweringMachineButtonCallback);

    		buttonList.Add(giAnsweringMachineButton[0]);
    		ButtonSubSystem.MSYS_SetBtnUserData( giAnsweringMachineButton[0], 0, 0);
    		ButtonSubSystem.SetButtonCursor(giAnsweringMachineButton[0], CURSOR.LAPTOP_SCREEN);

    		//if the user has already left a message, disable the button
    		if( gMercProfiles[(NPCID)(gbCurrentSoldier)].ubMiscFlags3.HasFlag(PROFILE_MISC_FLAG3.PLAYER_LEFT_MSG_FOR_MERC_AT_AIM) )
    			ButtonSubSystem.DisableButton( giAnsweringMachineButton[0] );

    		usPosX += AIM_MEMBER_AUTHORIZE_PAY_GAP;

    		giAnsweringMachineButton[1] = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[2], VideoConfercingText[AIM_MEMBER_HANG_UP], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_MEMBER_HANG_UP_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnAnsweringMachineButtonCallback);

    		buttonList.Add(giAnsweringMachineButton[1]);
    		ButtonSubSystem.MSYS_SetBtnUserData( giAnsweringMachineButton[1], 0, 1);
    		ButtonSubSystem.SetButtonCursor(giAnsweringMachineButton[1], CURSOR.LAPTOP_SCREEN);

    		//The face must be inited even though the face wont appear.  It is so the voice is played
    		InitVideoFace(gbCurrentSoldier);

    		//Make sure the merc doesnt ramble away to the player
    		gubMercAttitudeLevel = QUOTE_DELAY_NO_ACTION;

		
    //
    //DEF: TEST
    //
    /*		// load the answering machine graphic and add it
    		guiAnsweringMachineImage = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\explosion.sti"));
    */
    		gubCurrentStaticMode = VC_NO_STATIC;
    	}



		
    	// The merc is home but for some reason doesnt want to work for player
    	if( gubVideoConferencingMode == AIM_VIDEO.MERC_UNAVAILABLE_MODE)
    	{
    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		// The hangup button 
    		guiVideoConferenceButtonImage[2] = ButtonSubSystem.LoadButtonImage("LAPTOP\\VideoConfButtons.sti", -1,2,-1,3,-1 );

    		giHangUpButton = ButtonSubSystem.CreateIconAndTextButton(guiVideoConferenceButtonImage[2], VideoConfercingText[AIM_MEMBER_HANG_UP], FontStyle.FONT12ARIAL, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, AIM_M_VIDEO_NAME_COLOR, AIM_M_VIDEO_NAME_SHADOWCOLOR, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(AIM_MEMBER_HANG_UP_X, AIM_MEMBER_HANG_UP_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnHangUpButtonCallback);

    		buttonList.Add(giHangUpButton);
    		ButtonSubSystem.MSYS_SetBtnUserData( giHangUpButton, 0, 1);
    		ButtonSubSystem.SetButtonCursor(giHangUpButton, CURSOR.LAPTOP_SCREEN);

    		//set the flag saying specifying that merc is busy
    		gubMercAttitudeLevel = QUOTE_MERC_BUSY;

    		InitVideoFace(gbCurrentSoldier);
    	}

    	if( gubVideoConferencingMode == 	AIM_VIDEO.POPDOWN_MODE )
    	{
    		HVOBJECT hImageHandle;

    		if( gubPopUpBoxAction == AIM_POPUP.DISPLAY )
    		{
    			return( true );
    		}

    		gubVideoConferencingPreviousMode = gubVideoConferencingMode;

    		gfIsAnsweringMachineActive = false;


    		// load the Video conference background graphic and add it

    		// Create a background video surface to blt the face onto



    	}

    //	gfWaitingForMercToStopTalkingOrUserToClick = FALSE;

    	//reset the time in which the merc will get annoyed
    	guiMercAttitudeTime = GetJA2Clock();
    	return(true);
    }


    private static bool DeleteVideoConfPopUp()
    {
    	int i;

    	//reset ( in case merc was going to say something
    	DelayMercSpeech( 0, (QUOTE)0, 0, false, true );

    	switch(	gubVideoConferencingPreviousMode )
    	{
    		// The video conference is not displayed
    		case AIM_VIDEO.NOT_DISPLAYED_MODE:
    		{

    			break;
    		}


    		case AIM_VIDEO.POPUP_MODE:
    		{
    			break;
    		}

    		// The opening animation of the vc (fuzzy screen, then goes to black)
    		case AIM_VIDEO.INIT_MODE:
    		{
			
    			break;
    		}



    		// The screen in which you first contact the merc, you have the option to hang up or goto hire merc screen
    		case AIM_VIDEO.FIRST_CONTACT_MERC_MODE:
    		{
    			//Remove the video conf buttons images
    			ButtonSubSystem.UnloadButtonImage(guiVideoConferenceButtonImage[2]);

    			//Remove the Hangup  buttons
    			for(i=0; i<2; i++)
    			{
    				    buttonList.Remove(giAuthorizeButton[i]);
    				    ButtonSubSystem.RemoveButton(giAuthorizeButton[i]);
    			}
    			break;
    		}



    		// The screen in which you set the contract length, and the ability to buy equipment..
    		case AIM_VIDEO.HIRE_MERC_MODE:
    		{
    			//Remove the video conf buttons images
    			for(i=0; i<2; i++)
    				ButtonSubSystem.UnloadButtonImage(guiVideoConferenceButtonImage[i]);

    			//Remove the Contracy Length button	
    			for(i=0; i<3; i++)
    			{
    				    buttonList.Remove(giContractLengthButton[i]);
    				    ButtonSubSystem.RemoveButton(giContractLengthButton[i]);
    			}
    			for(i=0; i<2; i++)
    			{
    				    buttonList.Remove(giBuyEquipmentButton[i]);
    				    ButtonSubSystem.RemoveButton(giBuyEquipmentButton[i]);
    			}
    			for(i=0; i<2; i++)
    			{
    				    buttonList.Remove(giAuthorizeButton[i]);
    				    ButtonSubSystem.RemoveButton(giAuthorizeButton[i]);
    			}
    			break;
    		}




    		// The merc is not home and the player gets the answering machine
    		case AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE:
    		{
    			if( gubPopUpBoxAction == AIM_POPUP.DISPLAY )
    			{
    //				return( TRUE );
    			}

    			//Remove the video conf buttons images
    			ButtonSubSystem.UnloadButtonImage(guiVideoConferenceButtonImage[2]);

    			//Remove the Answering machine buttons
    			for(i=0; i<2; i++)
    			{
    				    buttonList.Remove(giAnsweringMachineButton[i]);
    				    ButtonSubSystem.RemoveButton(giAnsweringMachineButton[i]);
    			}
    //			DeleteVideoObjectFromIndex(guiAnsweringMachineImage);
    			break;
    		}




    		// The merc is home but doesnt want to work for player
    		case AIM_VIDEO.MERC_UNAVAILABLE_MODE:
    		{
    			buttonList.Remove(giHangUpButton);
    			ButtonSubSystem.RemoveButton(giHangUpButton);
    			ButtonSubSystem.UnloadButtonImage(guiVideoConferenceButtonImage[2]);
    			break;
    		}

    		case AIM_VIDEO.POPDOWN_MODE:
    		{
    			if( gubPopUpBoxAction == AIM_POPUP.DISPLAY )
    			{
    				return( true );
    			}

    			if( gfWaitingForMercToStopTalkingOrUserToClick )
    			{
    				gfWaitingForMercToStopTalkingOrUserToClick = false;

    //				DisplayPopUpBoxExplainingMercArrivalLocationAndTime();
    			}


    			gfWaitingForMercToStopTalkingOrUserToClick = false;
    			break;
    		}

    	}
    	return(true);
    }


    // was a function-local static of HandleCurrentVideoConfMode
    private static int HandleCurrentVideoConfMode_ubCurMode = 0;
    private static bool HandleCurrentVideoConfMode()
    {
    	switch(	gubVideoConferencingMode )
    	{
    		// The video conference is not displayed
    		case AIM_VIDEO.NOT_DISPLAYED_MODE:
    		{
    			gfWaitingForMercToStopTalkingOrUserToClick = false;
		
    			break;
    		}

		
    		case AIM_VIDEO.POPUP_MODE:
    		{
    			bool ubDone;

    			if( gfJustSwitchedVideoConferenceMode )
    				ubDone = DisplayMovingTitleBar( true, true );
    			else 
    				ubDone = DisplayMovingTitleBar( true, false );


    			if(ubDone)
    				gubVideoConferencingMode = AIM_VIDEO.INIT_MODE;

    			break;
    		}

    		// The opening animation of the vc (fuzzy screen, then goes to black)
    		case AIM_VIDEO.INIT_MODE:
    		{

    			bool	fDone;

    			if( HandleCurrentVideoConfMode_ubCurMode == 0 )
    			{
    				fDone = DisplayBlackBackground( 10 );
    				if( fDone )
    					HandleCurrentVideoConfMode_ubCurMode = 1;
    			}
    			else
    				fDone = DisplaySnowBackground();

    			if( fDone && HandleCurrentVideoConfMode_ubCurMode != 0 )
    			{
    				HandleCurrentVideoConfMode_ubCurMode = 0;

    				gubVideoConferencingMode = WillMercAcceptCall();
    			}

    			break;
    		}

    		// The screen in which you first contact the merc, you have the option to hang up or goto hire merc screen
    		case AIM_VIDEO.FIRST_CONTACT_MERC_MODE:
    		{
    			//if the merc is at home, play his greeting 
    //			if( gfJustSwitchedVideoConferenceMode )
    //				InitVideoFaceTalking(gbCurrentSoldier, QUOTE_GREETING);
			
    			break;
    		}

    		// The screen in which you set the contract length, and the ability to buy equipment..
    		case AIM_VIDEO.HIRE_MERC_MODE:
    		{

    			break;
    		}

    		// The merc is not home and the player gets the answering machine
    		case AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE:
    		{
    			//if the merc is not at home, play his answering machine
    			if( gfJustSwitchedVideoConferenceMode )
    			{
    				InitVideoFaceTalking(gbCurrentSoldier, QUOTE.ANSWERING_MACHINE_MSG);
    			}
			
    			break;
    		}

    		// The merc is home but doesnt want to work for player
    		case AIM_VIDEO.MERC_UNAVAILABLE_MODE:
    		{
			
    			break;
    		}

    		case AIM_VIDEO.POPDOWN_MODE:
    		{
    			bool ubDone;

    			if( gfJustSwitchedVideoConferenceMode )
    				ubDone = DisplayMovingTitleBar( false, true );
    			else 
    				ubDone = DisplayMovingTitleBar( false, false );

    			if(ubDone)
    			{
    				gubVideoConferencingMode = AIM_VIDEO.NOT_DISPLAYED_MODE;


    				//display the popup telling the user when the just hired merc is going to land
    				DisplayPopUpBoxExplainingMercArrivalLocationAndTime();

    				//render the screen immediately to get rid of the pop down stuff
    				InitDeleteVideoConferencePopUp( );
    				RenderAIMMembers();
    				gfVideoFaceActive = false;
    			}

    			break;
    		}
    	}

    	//Gets set in the InitDeleteVideoConferencePopUp() function
    //	gfJustSwitchedVideoConferenceMode = FALSE;

    	return(true);
    }


    // was a function-local static of EnableDisableCurrentVideoConferenceButtons
    private static bool EnableDisableCurrentVideoConferenceButtons_fCreated = false;
    private static bool EnableDisableCurrentVideoConferenceButtons(bool fEnable)
    {
    	int	i;

    	if(!fEnable)
    	{
    		if( EnableDisableCurrentVideoConferenceButtons_fCreated )
    		{
    			//enable buttons behind the acknowlegde button
			
    			for( i=0; i<3; i++)
    				ButtonSubSystem.EnableButton( giContractLengthButton[i] );

    			for(i=0; i<2; i++)
    				ButtonSubSystem.EnableButton( giBuyEquipmentButton[i] );

    			for(i=0; i<2; i++)
    				ButtonSubSystem.EnableButton( giAuthorizeButton[i] );

    			EnableDisableCurrentVideoConferenceButtons_fCreated = false;
    		}
    	}
    	else
    	{
    		if( !EnableDisableCurrentVideoConferenceButtons_fCreated )
    		{
    			//disable buttons behind the acknowlegde button
    			for( i=0; i<3; i++)
    				ButtonSubSystem.DisableButton( giContractLengthButton[i] );

    			for(i=0; i<2; i++)
    				ButtonSubSystem.DisableButton( giBuyEquipmentButton[i] );

    			for(i=0; i<2; i++)
    				ButtonSubSystem.DisableButton( giAuthorizeButton[i] );


    			EnableDisableCurrentVideoConferenceButtons_fCreated = true;
    		}
    	}
    	return(true);
    }


    // was a function-local static of HandleAnsweringMachineMessage
    private static bool HandleAnsweringMachineMessage_fDone;
    private static bool HandleAnsweringMachineMessage()
    {


    	if( gfJustSwitchedVideoConferenceMode )
    		HandleAnsweringMachineMessage_fDone = DisplayAnimatedAnsweringMachineMsg( true,  12);
    	else if( !HandleAnsweringMachineMessage_fDone )
    	{
    		HandleAnsweringMachineMessage_fDone = DisplayAnimatedAnsweringMachineMsg( false,  12);
    		if( HandleAnsweringMachineMessage_fDone )
    		{
    			HandleAnsweringMachineMessage_fDone = false;
    		}
    	}

    	return(true);
    }


    private static bool DisplayAnimatedAnsweringMachineMsg(bool fInit, int ubNumSubImages)
    {
        // TODO(port): the answering machine animation.
        return true;
    }


    private static bool DisplayMovingTitleBar(bool fForward, bool fInit)
    {
        // TODO(port): the animated title bar slide.
        return true;
    }


    private static void TempHiringOfMercs(int ubNumberOfMercs, bool fReset)
    {
        // TODO(port): a debug-only hiring helper.
    }


    private static void DelayMercSpeech(int ubMercID, QUOTE usQuoteNum, int usDelay, bool fNewQuote, bool fReset)
    {
        // TODO(port): merc speech (needs the dialogue system).
    }


    private static bool fQuoteWaiting = false;      // a quote is waiting to be said

    private static bool QuickHireMerc()
    {
        // TODO(port): a debug-only hiring helper.
        return false;
    }


    private static void TempHandleAimMemberKeyBoardInput()
    {
        // TODO(port): debug keyboard shortcuts.
    }


    private static void WaitForMercToFinishTalkingOrUserToClick()
    {
    	//if the region is not active
    	if( !gfIsShutUpMouseRegionActive )
    	{
    		//Enables it so if a player clicks, he will shutup the merc
    		MouseSubSystem.MSYS_EnableRegion( gSelectedShutUpMercRegion ); 
    		gfIsShutUpMouseRegionActive = true;
    	}

    	if( gfIsAnsweringMachineActive )
    		gubVideoConferencingMode = AIM_VIDEO.MERC_ANSWERING_MACHINE_MODE;
    	else
    		gubVideoConferencingMode = AIM_VIDEO.FIRST_CONTACT_MERC_MODE;

    	gfWaitingForMercToStopTalkingOrUserToClick = true;
    	gfHangUpMerc = true;
    	gfStopMercFromTalking = false;
    }


    private static bool DisplayShadedStretchedMercFace(int ubMercID, int usPosX, int usPosY)
    {
        // TODO(port): the stretched, shaded face.
        return true;
    }


    private static void DemoHiringOfMercs()
    {
        // TODO(port): a demo-only hiring helper.
    }


    private static void DisplayPopUpBoxExplainingMercArrivalLocationAndTime()
    {
    	string szLocAndTime = string.Empty;
    	SOLDIERTYPE? pSoldier = null;
    	string zTimeString = string.Empty;
    	string zSectorIDString = string.Empty;
    	int		uiHour;

    	//if the id of the merc is invalid, dont display the pop up
    	if( LaptopSaveInfo.sLastHiredMerc.iIdOfMerc == -1 )
    		return;

    	//if the pop up has already been displayed, dont display it again for this occurence of laptop
    	if( LaptopSaveInfo.sLastHiredMerc.fHaveDisplayedPopUpInLaptop )
    		return;

    	pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID( (NPCID)LaptopSaveInfo.sLastHiredMerc.iIdOfMerc, true );

    	if( pSoldier == null )
    		return;

    	//calc the approximate hour the mercs will arrive at
    	uiHour = ( ( LaptopSaveInfo.sLastHiredMerc.uiArrivalTime ) - ( ( ( LaptopSaveInfo.sLastHiredMerc.uiArrivalTime ) / 1440 ) * 1440 ) ) / 60;
	
    	//create the time string
    	zTimeString = wprintf( "%02d:%02d", uiHour, 0 );

    	//get the id string
    	StrategicMap.GetSectorIDString( gsMercArriveSectorX, gsMercArriveSectorY, 0, out zSectorIDString, false );

    	//create the string to display to the user, looks like....
    	//	L"%s should arrive at the designated drop-off point ( sector %d:%d %s ) on day %d, at approximately %s.",		//first %s is mercs name, next is the sector location and name where they will be arriving in, lastely is the day an the time of arrival

    // #ifdef GERMAN
    // //Germans version has a different argument order
    // swprintf( szLocAndTime, pMessageStrings[ MSG_JUST_HIRED_MERC_ARRIVAL_LOCATION_POPUP ], 
    // gMercProfiles[(NPCID)(pSoldier->ubProfile)].zNickname, 
    // LaptopSaveInfo.sLastHiredMerc.uiArrivalTime / 1440, 
    // zTimeString,
    // zSectorIDString );
    // #else
    	szLocAndTime = wprintf( pMessageStrings[ MSG.JUST_HIRED_MERC_ARRIVAL_LOCATION_POPUP ], 
    							gMercProfiles[(NPCID)(pSoldier.ubProfile)].zNickname, 
    							zSectorIDString,
    							LaptopSaveInfo.sLastHiredMerc.uiArrivalTime / 1440, 
    							zTimeString );
    // #endif



    	//display the message box
    	DoLapTopMessageBox( MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, szLocAndTime, ScreenName.LAPTOP_SCREEN, MSG_BOX_FLAG.OK, DisplayPopUpBoxExplainingMercArrivalLocationAndTimeCallBack );

    	//reset the id of the last merc
    	LaptopSaveInfo.sLastHiredMerc.iIdOfMerc = -1;

    	//set the fact that the pop up has been displayed this time in laptop
    	LaptopSaveInfo.sLastHiredMerc.fHaveDisplayedPopUpInLaptop = true;

    }


    private static void DisplayPopUpBoxExplainingMercArrivalLocationAndTimeCallBack(MessageBoxReturnCode bExitValue)
    {
    	//unset the flag so the msgbox WONT dislay its save buffer
    	gfDontOverRideSaveBuffer = false;

    	if( guiCurrentLaptopMode == LAPTOP_MODE.AIM_MEMBERS )
    	{
    		//render the screen
    		gfRedrawScreen = true;
    		RenderAIMMembers();
    	}
    }
}

