using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimPolicies.c: the A.I.M. policy pages, with the agree / disagree gate in front of them.
public partial class Laptop
{
    private const int NUM_AIM_POLICY_TOC_BUTTONS = 9;
    private const string AIMPOLICYFILE = "BINARYDATA\\AimPol.edt";
    private const int AIM_POLICY_LINE_SIZE = 80 * 5 * 2; // 80 columns of 5 lines that are wide chars, 800 bytes total

    private const FontStyle AIM_POLICY_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_POLICY_TITLE_COLOR = AIM_GREEN;
    private const FontStyle AIM_POLICY_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_POLICY_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_POLICY_TOC_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_POLICY_TOC_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_POLICY_SUBTITLE_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_POLICY_SUBTITLE_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontColor AIM_POLICY_AGREE_TOC_COLOR_ON = FontColor.FONT_MCOLOR_WHITE;
    private const FontColor AIM_POLICY_AGREE_TOC_COLOR_OFF = FontColor.FONT_MCOLOR_DKWHITE;

    private const int AIM_POLICY_MENU_X = LAPTOP_SCREEN_UL_X + 40;
    private const int AIM_POLICY_MENU_Y = 390 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_POLICY_MENU_BUTTON_AMOUNT = 4;
    private const int AIM_POLICY_GAP_X = 40 + BOTTOM_BUTTON_START_WIDTH;

    private const int AIM_POLICY_TITLE_X = IMAGE_OFFSET_X + 149;
    private const int AIM_POLICY_TITLE_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT + 11;
    private const int AIM_POLICY_TITLE_WIDTH = AIM_SYMBOL_WIDTH;

    private const int AIM_POLICY_TITLE_STATEMENT_WIDTH = 300;
    private const int AIM_POLICY_TITLE_STATEMENT_X = IMAGE_OFFSET_X + (500 - AIM_POLICY_TITLE_STATEMENT_WIDTH) / 2 + 5;
    private const int AIM_POLICY_TITLE_STATEMENT_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT + 75;

    private const int AIM_POLICY_SUBTITLE_NUMBER = AIM_POLICY_TITLE_STATEMENT_X - 75;
    private const int AIM_POLICY_SUBTITLE_X = AIM_POLICY_SUBTITLE_NUMBER + 20;
    private const int AIM_POLICY_SUBTITLE_Y = 115 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_POLICY_PARAGRAPH_NUMBER = AIM_POLICY_SUBTITLE_X - 12;
    private const int AIM_POLICY_PARAGRAPH_X = AIM_POLICY_PARAGRAPH_NUMBER + 23;
    private const int AIM_POLICY_PARAGRAPH_Y = AIM_POLICY_SUBTITLE_Y + 20;
    private const int AIM_POLICY_PARAGRAPH_WIDTH = 380;
    private const int AIM_POLICY_PARAGRAPH_GAP = 6;
    private const int AIM_POLICY_SUBPARAGRAPH_NUMBER = AIM_POLICY_PARAGRAPH_X;
    private const int AIM_POLICY_SUBPARAGRAPH_X = AIM_POLICY_SUBPARAGRAPH_NUMBER + 25;

    private const int AIM_POLICY_TOC_X = 259;
    private const int AIM_POLICY_TOC_Y = AIM_POLICY_SUBTITLE_Y;
    private const int AIM_POLICY_TOC_GAP_Y = 25;
    private const int AIM_POLICY_TOC_TEXT_OFFSET_X = 5;
    private const int AIM_POLICY_TOC_TEXT_OFFSET_Y = 5;

    private const int AIM_POLICY_AGREEMENT_X = IMAGE_OFFSET_X + 150;
    private const int AIM_POLICY_AGREEMENT_Y = 350 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_POLICY_DISAGREEMENT_X = AIM_POLICY_AGREEMENT_X + 125;
    private const int AIM_POLICY_DISAGREEMENT_Y = AIM_POLICY_AGREEMENT_Y;

    private const int AIM_POLICY_TOC_PAGE = 1;
    private const int AIM_POLICY_LAST_PAGE = 10;

    private const int AIM_POLICY_AGREE_PAGE = 0;

    // indices into AimPolicyText
    private const int AIM_POLICIES_DISAGREE = 4;

    // These represent which paragraph they are located in the AimPol.edt file
    private const int AIM_STATEMENT_OF_POLICY = 0;
    private const int AIM_STATEMENT_OF_POLICY_1 = 1;
    private const int AIM_STATEMENT_OF_POLICY_2 = 2;
    private const int DEFINITIONS = 3;
    private const int DEFINITIONS_1 = 4;
    private const int DEFINITIONS_2 = 5;
    private const int DEFINITIONS_3 = 6;
    private const int DEFINITIONS_4 = 7;
    private const int LENGTH_OF_ENGAGEMENT = 8;
    private const int LENGTH_OF_ENGAGEMENT_1 = 9;
    private const int LENGTH_OF_ENGAGEMENT_1_1 = 10;
    private const int LENGTH_OF_ENGAGEMENT_1_2 = 11;
    private const int LENGTH_OF_ENGAGEMENT_1_3 = 12;
    private const int LENGTH_OF_ENGAGEMENT_2 = 13;
    private const int LOCATION_0F_ENGAGEMENT = 14;
    private const int LOCATION_0F_ENGAGEMENT_1 = 15;
    private const int LOCATION_0F_ENGAGEMENT_2 = 16;
    private const int LOCATION_0F_ENGAGEMENT_2_1 = 17;
    private const int LOCATION_0F_ENGAGEMENT_2_2 = 18;
    private const int LOCATION_0F_ENGAGEMENT_2_3 = 19;
    private const int LOCATION_0F_ENGAGEMENT_2_4 = 20;
    private const int LOCATION_0F_ENGAGEMENT_3 = 21;
    private const int CONTRACT_EXTENSIONS = 22;
    private const int CONTRACT_EXTENSIONS_1 = 23;
    private const int CONTRACT_EXTENSIONS_2 = 24;
    private const int CONTRACT_EXTENSIONS_3 = 25;
    private const int TERMS_OF_PAYMENT = 26;
    private const int TERMS_OF_PAYMENT_1 = 27;
    private const int TERMS_OF_ENGAGEMENT = 28;
    private const int TERMS_OF_ENGAGEMENT_1 = 29;
    private const int TERMS_OF_ENGAGEMENT_2A = 30;
    private const int TERMS_OF_ENGAGEMENT_2B = 31;
    private const int ENGAGEMENT_TERMINATION = 32;
    private const int ENGAGEMENT_TERMINATION_1 = 33;
    private const int ENGAGEMENT_TERMINATION_1_1 = 34;
    private const int ENGAGEMENT_TERMINATION_1_2 = 35;
    private const int ENGAGEMENT_TERMINATION_1_3 = 36;
    private const int EQUIPMENT_AND_INVENTORY = 37;
    private const int EQUIPMENT_AND_INVENTORY_1 = 38;
    private const int EQUIPMENT_AND_INVENTORY_2 = 39;
    private const int POLICY_MEDICAL = 40;
    private const int POLICY_MEDICAL_1 = 41;
    private const int POLICY_MEDICAL_2 = 42;
    private const int POLICY_MEDICAL_3A = 43;
    private const int POLICY_MEDICAL_3B = 44;
    private const int POLICY_MEDICAL_4 = 45;
    private const int NUM_AIM_POLICY_LOCATIONS = 46;

    // Toc menu mouse regions
    private static readonly MOUSE_REGION[] gSelectedPolicyTocMenuRegion = CreateRegions(NUM_AIM_POLICY_TOC_BUTTONS, nameof(gSelectedPolicyTocMenuRegion));

    // Agree/Disagree menu buttons
    private static readonly GUI_BUTTON[] guiPoliciesAgreeButton = new GUI_BUTTON[2];
    private static ButtonPic guiPoliciesButtonImage = null!;

    // Bottom menu buttons
    private static readonly GUI_BUTTON[] guiPoliciesMenuButton = new GUI_BUTTON[AIM_POLICY_MENU_BUTTON_AMOUNT];
    private static ButtonPic guiPoliciesMenuButtonImage = null!;

    private static bool gfInPolicyToc = false;
    private static bool gfInAgreementPage = false;
    private static bool gfAimPolicyMenuBarLoaded = false;
    private static bool gfExitingPolicesAgreeButton;
    private static int gubPoliciesAgreeButtonDown;
    private static int gubAimPolicyMenuButtonDown = 255;
    private static bool gfExitingAimPolicy;

    private static void GameInitAimPolicies()
    {
    }

    // these two live with their own pages in the C code but are still empty
    private static void GameInitAimLinks()
    {
    }

    private static void GameInitAimHistory()
    {
    }

    public static bool EnterAimPolicies()
    {
        InitAimDefaults();

        gubCurPageNum = giCurrentSubPage;

        gfAimPolicyMenuBarLoaded = false;
        gfExitingAimPolicy = false;

        gubPoliciesAgreeButtonDown = 255;
        gubAimPolicyMenuButtonDown = 255;

        if (gubCurPageNum != 0)
        {
            InitAimPolicyMenuBar();
        }

        gfInPolicyToc = false;

        // load the Bottom Buttons graphic and add it
        guiBottomButton = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\BottomButton.sti"));
        guiBottomButton2 = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\BottomButton2.sti"));

        // load the Content Buttons graphic and add it
        guiContentButton = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\ContentButton.sti"));

        RenderAimPolicies();
        return true;
    }

    public static void ExitAimPolicies()
    {
        gfExitingAimPolicy = true;

        video.DeleteVideoObject(guiBottomButton);
        video.DeleteVideoObject(guiBottomButton2);
        video.DeleteVideoObject(guiContentButton);

        if (gfAimPolicyMenuBarLoaded)
        {
            ExitAimPolicyMenuBar();
        }

        if (gfInPolicyToc)
        {
            ExitAimPolicyTocMenu();
        }

        if (gfInAgreementPage)
        {
            ExitAgreementButton();
        }

        RemoveAimDefaults();
        giCurrentSubPage = gubCurPageNum;
    }

    public static void HandleAimPolicies()
    {
        if (!gfAimPolicyMenuBarLoaded && gubCurPageNum != 0)
        {
            InitAimPolicyMenuBar();
            fPausedReDrawScreenFlag = true;
        }
    }



    public static void RenderAimPolicies()
    {
    	int	usNumPixles;

    	DrawAimDefaults();

    	DisplayAimPolicyTitleText();

    	if( gfInAgreementPage )
    		ExitAgreementButton();		

    	switch( gubCurPageNum )
    	{
    		case 0:
    			DisplayAimPolicyStatement();
    			InitAgreementRegion();
    			break;

    		case 1:
    			InitAimPolicyTocMenu();
    			InitAimPolicyMenuBar();
    			DisableAimPolicyButton();
    			DrawAimPolicyMenu();
    			break;

    		case 2:
    			//Display the Definitions title
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, DEFINITIONS, (float)1.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, DEFINITIONS_1, (float)1.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, DEFINITIONS_2, (float)1.2) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, DEFINITIONS_3, (float)1.3) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, DEFINITIONS_4, (float)1.4);
    			break;

    		case 3:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, LENGTH_OF_ENGAGEMENT, (float)2.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LENGTH_OF_ENGAGEMENT_1, (float)2.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LENGTH_OF_ENGAGEMENT_1_1, (float)2.11) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LENGTH_OF_ENGAGEMENT_1_2, (float)2.12) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LENGTH_OF_ENGAGEMENT_1_3, (float)2.13) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LENGTH_OF_ENGAGEMENT_2, (float)2.2) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 4:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, LOCATION_0F_ENGAGEMENT, (float)3.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_1, (float)3.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2, (float)3.2) + AIM_POLICY_PARAGRAPH_GAP;

    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2_1, (float)3.21) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2_2, (float)3.22) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2_3, (float)3.23) + AIM_POLICY_PARAGRAPH_GAP;
    //			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2_4, (FLOAT)3.24) + AIM_POLICY_PARAGRAPH_GAP;

    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_2_4, (float)3.3) + AIM_POLICY_PARAGRAPH_GAP;

    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, LOCATION_0F_ENGAGEMENT_3, (float)3.4) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 5:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, CONTRACT_EXTENSIONS, (float)4.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, CONTRACT_EXTENSIONS_1, (float)4.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, CONTRACT_EXTENSIONS_2, (float)4.2) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, CONTRACT_EXTENSIONS_3, (float)4.3) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 6:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, TERMS_OF_PAYMENT, (float)5.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, TERMS_OF_PAYMENT_1, (float)5.1) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 7:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, TERMS_OF_ENGAGEMENT, (float)6.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, TERMS_OF_ENGAGEMENT_1, (float)6.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, TERMS_OF_ENGAGEMENT_2A, (float)6.2) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, TERMS_OF_ENGAGEMENT_2B, (float)0.0) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 8:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, ENGAGEMENT_TERMINATION, (float)7.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, ENGAGEMENT_TERMINATION_1, (float)7.1) + AIM_POLICY_PARAGRAPH_GAP;

    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, ENGAGEMENT_TERMINATION_1_1, (float)7.11) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, ENGAGEMENT_TERMINATION_1_2, (float)7.12) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicySubParagraph(usNumPixles, ENGAGEMENT_TERMINATION_1_3, (float)7.13) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 9:
    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, EQUIPMENT_AND_INVENTORY, (float)8.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, EQUIPMENT_AND_INVENTORY_1, (float)8.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, EQUIPMENT_AND_INVENTORY_2, (float)8.2) + AIM_POLICY_PARAGRAPH_GAP;
    			break;

    		case 10:
    			DisableAimPolicyButton();

    			DisplayAimPolicyTitle(AIM_POLICY_SUBTITLE_Y, POLICY_MEDICAL, (float)9.0);
    			usNumPixles = AIM_POLICY_PARAGRAPH_Y;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, POLICY_MEDICAL_1, (float)9.1) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, POLICY_MEDICAL_2, (float)9.2) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, POLICY_MEDICAL_3A, (float)9.3) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, POLICY_MEDICAL_3B, (float)0.0) + AIM_POLICY_PARAGRAPH_GAP;
    			usNumPixles += DisplayAimPolicyParagraph(usNumPixles, POLICY_MEDICAL_4, (float)9.4) + AIM_POLICY_PARAGRAPH_GAP;
    			break;
    	}

      ButtonSubSystem.MarkButtonsDirty(buttonList);

    	RenderWWWProgramTitleBar( );

      video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static bool InitAimPolicyMenuBar()
    {
    	int					i, usPosX;

    	if(gfAimPolicyMenuBarLoaded)
    		return(true);

    	//Load graphic for buttons
    	guiPoliciesMenuButtonImage =  ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1,0,-1,1,-1 );

    	usPosX = AIM_POLICY_MENU_X;
    	for(i=0; i<AIM_POLICY_MENU_BUTTON_AMOUNT; i++)
    	{

    //		guiPoliciesMenuButton[i] = ButtonSubSystem.QuickCreateButton(guiPoliciesMenuButtonImage, new Point(usPosX, AIM_POLICY_MENU_Y), //																	BUTTON_TOGGLE, MSYS_PRIORITY_HIGH, //																	MouseSubSystem.DefaultMoveCallback, (GUI_CALLBACK)BtnPoliciesMenuButtonCallback);
    //		SetButtonCursor(guiPoliciesMenuButton[i], CURSOR_WWW);
    //		MSYS_SetBtnUserData( guiPoliciesMenuButton[i], 0, i);

    		guiPoliciesMenuButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiPoliciesMenuButtonImage, AimPolicyText[i], FontStyle.FONT10ARIAL, AIM_BUTTON_ON_COLOR, FontShadow.DEFAULT_SHADOW, AIM_BUTTON_OFF_COLOR, FontShadow.DEFAULT_SHADOW, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_POLICY_MENU_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnPoliciesMenuButtonCallback);
    		ButtonSubSystem.SetButtonCursor(guiPoliciesMenuButton[i], CURSOR.WWW);
    		buttonList.Add(guiPoliciesMenuButton[i]);
    		ButtonSubSystem.MSYS_SetBtnUserData( guiPoliciesMenuButton[i], 0, i);
		


    		usPosX += AIM_POLICY_GAP_X;
    	}




    	gfAimPolicyMenuBarLoaded = true;

    	return(true);
    }

    private static bool ExitAimPolicyMenuBar()
    {
    	int i;

    	if( !gfAimPolicyMenuBarLoaded ) 
    		return( false );

    	for(i=0; i<AIM_POLICY_MENU_BUTTON_AMOUNT; i++)
    	{
    		buttonList.Remove(guiPoliciesMenuButton[i]);
    		ButtonSubSystem.RemoveButton( guiPoliciesMenuButton[i]);
    	}

    	ButtonSubSystem.UnloadButtonImage( guiPoliciesMenuButtonImage );

    	gfAimPolicyMenuBarLoaded = false;

    	return(true);
    }

    private static bool DrawAimPolicyMenu()
    {
    	int			i, usPosY;
    	int			usHeight;
    	int			uiStartLoc=0;
    	string sText = string.Empty;
      HVOBJECT		hContentButtonHandle;
    	int[] ubLocInFile =		
    								{	DEFINITIONS,
    									LENGTH_OF_ENGAGEMENT,
    									LOCATION_0F_ENGAGEMENT,
    									CONTRACT_EXTENSIONS,
    									TERMS_OF_PAYMENT,
    									TERMS_OF_ENGAGEMENT,
    									ENGAGEMENT_TERMINATION,
    									EQUIPMENT_AND_INVENTORY,
    									POLICY_MEDICAL};

    	hContentButtonHandle = guiContentButton;

    	usHeight = FontSubSystem.GetFontHeight(AIM_POLICY_TOC_FONT);
    	usPosY = AIM_POLICY_TOC_Y;
    	for(i=0; i<NUM_AIM_POLICY_TOC_BUTTONS; i++)
    	{
    	  video.BltVideoObject(SurfaceType.FRAME_BUFFER, hContentButtonHandle, 0,AIM_POLICY_TOC_X, usPosY, VO_BLT.SRCTRANSPARENCY,null);

    		uiStartLoc = AIM_POLICY_LINE_SIZE * ubLocInFile[i];
    		files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_HISTORY_LINE_SIZE);
    		FontSubSystem.DrawTextToScreen(sText, new(AIM_POLICY_TOC_X + AIM_POLICY_TOC_TEXT_OFFSET_X, (int)(usPosY + AIM_POLICY_TOC_TEXT_OFFSET_Y)), (int)AIM_CONTENTBUTTON_WIDTH, AIM_POLICY_TOC_FONT, AIM_POLICY_TOC_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    		usPosY += AIM_POLICY_TOC_GAP_Y;
    	}
    	gfInPolicyToc = true;

    	return(true);
    }

    private static bool InitAimPolicyTocMenu()
    {
    	int			i, usPosY;
    	int			usHeight;
    	int			uiStartLoc=0;

    	if(gfInPolicyToc)
    		return(true);

    	usHeight = FontSubSystem.GetFontHeight(AIM_POLICY_TOC_FONT);
    	usPosY = AIM_POLICY_TOC_Y;
    	for(i=0; i<NUM_AIM_POLICY_TOC_BUTTONS; i++)
    	{
    		//Mouse region for the toc buttons
    		MouseSubSystem.MSYS_DefineRegion( gSelectedPolicyTocMenuRegion[i], AIM_POLICY_TOC_X, usPosY, (int)(AIM_POLICY_TOC_X + (int)AIM_CONTENTBUTTON_WIDTH), (int)(usPosY + (int)AIM_CONTENTBUTTON_HEIGHT), MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, SelectPolicyTocMenuRegionCallBack ); 
    		MouseSubSystem.MSYS_AddRegion( ref gSelectedPolicyTocMenuRegion[i] ); 
    		MouseSubSystem.MSYS_SetRegionUserData( gSelectedPolicyTocMenuRegion[i], 0, i+2 );

    		usPosY += AIM_POLICY_TOC_GAP_Y;
    	}
    	gfInPolicyToc = true;

    	return(true);
    }

    private static bool ExitAimPolicyTocMenu()
    {
    	int i;

    	gfInPolicyToc = false;
    	for(i=0; i<NUM_AIM_POLICY_TOC_BUTTONS; i++)
    		MouseSubSystem.MSYS_RemoveRegion( gSelectedPolicyTocMenuRegion[i] );

    	return(true);
    }

    private static void SelectPolicyTocMenuRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	if(gfInPolicyToc)
    	{
    		if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    		{
    		}
    		else if(iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    		{
    			gubCurPageNum = (int)MouseSubSystem.MSYS_GetRegionUserData( ref pRegion, 0 );

    			ChangingAimPoliciesSubPage( gubCurPageNum );

    			ExitAimPolicyTocMenu();
    			ResetAimPolicyButtons();
    			DisableAimPolicyButton();
    		}
    		else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    		{
    		}
    	}
    }

    private static bool DisplayAimPolicyTitleText()
    {
    	string sText = string.Empty;
    	int	uiStartLoc = 0;

    	//Load anfd display title
    	uiStartLoc = AIM_POLICY_LINE_SIZE * AIM_STATEMENT_OF_POLICY;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);

    	if(gubCurPageNum == 0)
    		FontSubSystem.DrawTextToScreen(sText, new(AIM_POLICY_TITLE_X, AIM_POLICY_TITLE_STATEMENT_Y-25), AIM_POLICY_TITLE_WIDTH, AIM_POLICY_TITLE_FONT, AIM_POLICY_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	else
    		FontSubSystem.DrawTextToScreen(sText, new(AIM_POLICY_TITLE_X, AIM_POLICY_TITLE_Y), AIM_POLICY_TITLE_WIDTH, AIM_POLICY_TITLE_FONT, AIM_POLICY_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    	return(true);
    }

    private static bool DisplayAimPolicyStatement()
    {
    	string sText = string.Empty;
    	int	uiStartLoc = 0;
    	int	usNumPixels;

    	//load and display the statment of policies
    	uiStartLoc = AIM_POLICY_LINE_SIZE * AIM_STATEMENT_OF_POLICY_1;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);
    	usNumPixels = FontSubSystem.DisplayWrappedString(new(AIM_POLICY_TITLE_STATEMENT_X, AIM_POLICY_TITLE_STATEMENT_Y), AIM_POLICY_TITLE_STATEMENT_WIDTH, 2, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//load and display the statment of policies
    	uiStartLoc = AIM_POLICY_LINE_SIZE * AIM_STATEMENT_OF_POLICY_2;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);
    	FontSubSystem.DisplayWrappedString(new(AIM_POLICY_TITLE_STATEMENT_X, (int)(AIM_POLICY_TITLE_STATEMENT_Y + usNumPixels+15)), AIM_POLICY_TITLE_STATEMENT_WIDTH, 2, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	return(true);
    }

    private static bool InitAgreementRegion()
    {
    	int	usPosX,i;

    	gfExitingPolicesAgreeButton = false;

    	//Load graphic for buttons
    	guiPoliciesButtonImage =  ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1,0,-1,1,-1 );

    	usPosX = AIM_POLICY_AGREEMENT_X;
    	for(i=0; i < 2; i++)
    	{
    //		guiPoliciesAgreeButton[i] = ButtonSubSystem.QuickCreateButton(guiPoliciesButtonImage, new Point(usPosX, AIM_POLICY_AGREEMENT_Y), //																	BUTTON_TOGGLE, MSYS_PRIORITY_HIGH, //																	BUTTON_NO_CALLBACK, (GUI_CALLBACK)BtnPoliciesAgreeButtonCallback);
    //		SetButtonCursor(guiPoliciesAgreeButton[i], CURSOR_WWW);
    //		MSYS_SetBtnUserData( guiPoliciesAgreeButton[i], 0, i);

    		guiPoliciesAgreeButton[i] = ButtonSubSystem.CreateIconAndTextButton(guiPoliciesButtonImage, AimPolicyText[i+AIM_POLICIES_DISAGREE], AIM_POLICY_TOC_FONT, AIM_POLICY_AGREE_TOC_COLOR_ON, FontShadow.DEFAULT_SHADOW, AIM_POLICY_AGREE_TOC_COLOR_OFF, FontShadow.DEFAULT_SHADOW, ButtonTextJustifies.TEXT_CJUSTIFIED, new Point(usPosX, AIM_POLICY_AGREEMENT_Y), ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGH, MouseSubSystem.DefaultMoveCallback, BtnPoliciesAgreeButtonCallback);
    		ButtonSubSystem.SetButtonCursor(guiPoliciesAgreeButton[i], CURSOR.WWW);
    		buttonList.Add(guiPoliciesAgreeButton[i]);
    		ButtonSubSystem.MSYS_SetBtnUserData( guiPoliciesAgreeButton[i], 0, i);
		


    		usPosX += 125;
    	}
    	gfInAgreementPage = true;
    	return(true);
    }

    private static bool ExitAgreementButton()
    {
    	int i;

    	gfExitingPolicesAgreeButton = true;

    	ButtonSubSystem.UnloadButtonImage( guiPoliciesButtonImage);

    	for(i=0; i<2; i++)
    	{
    		buttonList.Remove(guiPoliciesAgreeButton[i]);
    		ButtonSubSystem.RemoveButton( guiPoliciesAgreeButton[ i ]);
    	}

    	gfInAgreementPage = false;

    	return(true);
    }

    private static bool DisplayAimPolicyTitle(int usPosY, int ubPageNum, float fNumber)
    {
    	string sText = string.Empty;
    	int	uiStartLoc = 0;

    	//Load and display title
    	uiStartLoc = AIM_POLICY_LINE_SIZE * ubPageNum;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);
    	FontSubSystem.DrawTextToScreen(sText, new(AIM_POLICY_SUBTITLE_NUMBER, usPosY), 0, AIM_POLICY_SUBTITLE_FONT, AIM_POLICY_SUBTITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	return(true);
    }

    private static int DisplayAimPolicyParagraph(int usPosY, int ubPageNum, float fNumber)
    {
    	string sText = string.Empty;
    	string sTemp = string.Empty;
    	int	uiStartLoc=0;
    	int	usNumPixels;

    	uiStartLoc = AIM_POLICY_LINE_SIZE * ubPageNum;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);

    	if(fNumber != 0.0)
    	{
    		//Display the section number
    		sTemp = wprintf( "%2.1f", fNumber);
    		FontSubSystem.DrawTextToScreen(sTemp, new(AIM_POLICY_PARAGRAPH_NUMBER, usPosY), 0, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}

    	//Display the text beside the section number
    	usNumPixels = FontSubSystem.DisplayWrappedString(new(AIM_POLICY_PARAGRAPH_X, usPosY), AIM_POLICY_PARAGRAPH_WIDTH, 2, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	return(usNumPixels);
    }

    private static int DisplayAimPolicySubParagraph(int usPosY, int ubPageNum, float fNumber)
    {
    	string sText = string.Empty;
    	string sTemp = string.Empty;
    	int	uiStartLoc=0;
    	int	usNumPixels;

    	uiStartLoc = AIM_POLICY_LINE_SIZE * ubPageNum;
    	files.LoadEncryptedDataFromFile(AIMPOLICYFILE, out sText, uiStartLoc, AIM_POLICY_LINE_SIZE);

    	//Display the section number
    	sTemp = wprintf( "%2.2f", fNumber);
    	FontSubSystem.DrawTextToScreen(sTemp, new(AIM_POLICY_SUBPARAGRAPH_NUMBER, usPosY), 0, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	//Display the text beside the section number
    	usNumPixels = FontSubSystem.DisplayWrappedString(new(AIM_POLICY_SUBPARAGRAPH_X, usPosY), AIM_POLICY_PARAGRAPH_WIDTH, 2, AIM_POLICY_TEXT_FONT, AIM_POLICY_TEXT_COLOR, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	return(usNumPixels);
    }

    // was a function-local static of BtnPoliciesAgreeButtonCallback
    private static bool BtnPoliciesAgreeButtonCallback_fOnPage =true;
    private static void BtnPoliciesAgreeButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int	ubRetValue;

    	if(BtnPoliciesAgreeButtonCallback_fOnPage)
    	{
    		ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    		{
    			btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    			video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    			gubPoliciesAgreeButtonDown = ubRetValue;
    		}

    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    		{
    			if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    			{
    				btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    				//Agree
    				BtnPoliciesAgreeButtonCallback_fOnPage = false;
    				if(ubRetValue == 1)
    				{
    					gubCurPageNum++;
    					ChangingAimPoliciesSubPage( gubCurPageNum );
    				}

    				//Disagree
    				else
    				{
    					guiCurrentLaptopMode = LAPTOP_MODE.AIM;
    				}
    				video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    				BtnPoliciesAgreeButtonCallback_fOnPage = true;
    				gubPoliciesAgreeButtonDown = 255;
    			}
    		}
    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    		{
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			gubPoliciesAgreeButtonDown = 255;
    			video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    		}
    	} 
    }

    // was a function-local static of BtnPoliciesMenuButtonCallback
    private static bool BtnPoliciesMenuButtonCallback_fOnPage =true;
    private static void BtnPoliciesMenuButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
    	int	ubRetValue;

    	if(BtnPoliciesMenuButtonCallback_fOnPage)
    	{
    		ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData( btn, 0 );
    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) )
    		{
    			btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    			gubAimPolicyMenuButtonDown	= ubRetValue;
    			video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    		}

    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) )
    		{
    			if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
    			{
    				btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    				gubAimPolicyMenuButtonDown	= 255;
    				//If previous Page
    				if( ubRetValue == 0 )
    				{
    					if( gubCurPageNum > 1)
    					{
    						gubCurPageNum--;
    						ChangingAimPoliciesSubPage( gubCurPageNum );
    					}
    				}

    				// Home Page
    				else if( ubRetValue == 1 )
    				{
    					guiCurrentLaptopMode = LAPTOP_MODE.AIM;
    				}

    				//Company policies index
    				else if( ubRetValue == 2 )
    				{
    					if( gubCurPageNum != 1 )
    					{
    						gubCurPageNum=1;
    						ChangingAimPoliciesSubPage( gubCurPageNum );
    					}
    				}

    				//Next Page
    				else if( ubRetValue == 3 )
    				{
    					if( gubCurPageNum < NUM_AIM_POLICY_PAGES-1 )
    					{
    						gubCurPageNum++;
    						ChangingAimPoliciesSubPage( gubCurPageNum );

    						BtnPoliciesMenuButtonCallback_fOnPage = false;
    						if(gfInPolicyToc)
    						{
    							ExitAimPolicyTocMenu();
    						}
    						BtnPoliciesMenuButtonCallback_fOnPage = true;
    					}
    				}
    				video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    				ResetAimPolicyButtons();
    				DisableAimPolicyButton();
    				BtnPoliciesMenuButtonCallback_fOnPage = true;
    			}
    		}
    		if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    		{
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			gubAimPolicyMenuButtonDown	= 255;
    			DisableAimPolicyButton();
    			video.InvalidateRegion(LAPTOP_SCREEN_UL_X,LAPTOP_SCREEN_WEB_UL_Y,LAPTOP_SCREEN_LR_X,LAPTOP_SCREEN_WEB_LR_Y);
    		}
    	} 
    }

    private static void ResetAimPolicyButtons()
    {
    	int i=0;

    	for(i=0; i<AIM_POLICY_MENU_BUTTON_AMOUNT; i++)
    	{
    		guiPoliciesMenuButton[i].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON; 
    	}
    }

    private static void DisableAimPolicyButton()
    {
    	if( gfExitingAimPolicy == true || gfAimPolicyMenuBarLoaded == false )
    		return;

    	if( (gubCurPageNum == AIM_POLICY_TOC_PAGE ) )
    	{
    		guiPoliciesMenuButton[ 0 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    		guiPoliciesMenuButton[ 2 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    	}
    	else if(  ( gubCurPageNum == AIM_POLICY_LAST_PAGE ) ) 
    	{
    		guiPoliciesMenuButton[ 3 ].uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON );
    	}
    }

    private static void ChangingAimPoliciesSubPage(int ubSubPageNumber)
    {
    	fLoadPendingFlag = true;

    	if( AimPoliciesSubPagesVisitedFlag[ ubSubPageNumber ] == false )
    	{
    		fConnectingToSubPage = true;
    		fFastLoadFlag = false;

    		AimPoliciesSubPagesVisitedFlag[ ubSubPageNumber ] = true;
    	}
    	else
    	{
    		fConnectingToSubPage = true;
    		fFastLoadFlag = true;
    	}
    }
}
