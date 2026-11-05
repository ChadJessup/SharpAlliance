using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimMembers.c (constants): the A.I.M. member files and the video conferencing screen.
public partial class Laptop
{
    private const FontStyle AIM_M_FONT_PREV_NEXT_CONTACT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_M_FONT_PREV_NEXT_CONTACT_COLOR_UP = FontColor.FONT_MCOLOR_DKWHITE;
    private const FontColor AIM_M_FONT_PREV_NEXT_CONTACT_COLOR_DOWN = (FontColor)138;
    private const FontStyle AIM_M_FONT_STATIC_TEXT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_M_COLOR_STATIC_TEXT = (FontColor)146; // 75
    private const FontStyle AIM_M_FONT_DYNAMIC_TEXT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_M_COLOR_DYNAMIC_TEXT = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_M_WEAPON_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_M_WEAPON_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_M_NUMBER_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_M_NUMBER_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontColor AIM_M_ACTIVE_MEMBER_TITLE_COLOR = AIM_GREEN;
    private const FontColor AIM_M_FEE_CONTRACT_COLOR = AIM_GREEN;
    private const FontColor AIM_M_VIDEO_TITLE_COLOR = AIM_FONT_GOLD;
    private const FontColor AIM_M_VIDEO_NAME_COLOR = FontColor.FONT_MCOLOR_BLACK;
    private const FontShadow AIM_M_VIDEO_NAME_SHADOWCOLOR = (FontShadow)AIM_FONT_GOLD;
    private const FontStyle AIM_M_VIDEO_CONTRACT_LENGTH_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_M_VIDEO_CONTRACT_LENGTH_COLOR = FontColor.FONT_MCOLOR_BLACK;
    private const FontStyle AIM_M_VIDEO_CONTRACT_AMOUNT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_M_VIDEO_CONTRACT_AMOUNT_COLOR = (FontColor)183;
    private const FontStyle AIM_POPUP_BOX_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_POPUP_BOX_COLOR = FontColor.FONT_MCOLOR_BLACK;
    private const FontColor HIGH_STAT_COLOR = FontColor.FONT_MCOLOR_WHITE; // FONT_MCOLOR_LTGREEN
    private const FontColor MED_STAT_COLOR = FontColor.FONT_MCOLOR_DKWHITE; // FONT_MCOLOR_WHITE
    private const FontColor LOW_STAT_COLOR = FontColor.FONT_MCOLOR_DKWHITE; // FONT_MCOLOR_DKGRAY
    private const int SIZE_MERC_BIO_INFO = 400 * 2;
    private const int SIZE_MERC_ADDITIONAL_INFO = 160 * 2;
    private const int MERC_ANNOYED_WONT_CONTACT_TIME_MINUTES = 6 * 60;
    private const int NUMBER_HATED_MERCS_ONTEAM = 3;
    private const int STATS_X = IMAGE_OFFSET_X + 121;
    private const int STATS_Y = IMAGE_OFFSET_Y + 66; // 69
    private const int PRICE_X = IMAGE_OFFSET_X + 377;
    private const int PRICE_Y = STATS_Y;
    private const int PRICE_WIDTH = 116;
    private const int PORTRAIT_X = IMAGE_OFFSET_X + 8;
    private const int PORTRAIT_Y = STATS_Y;
    private const int PORTRAIT_WIDTH = 110;
    private const int PORTRAIT_HEIGHT = 126;
    private const int FACE_X = PORTRAIT_X + 2;
    private const int FACE_Y = PORTRAIT_Y + 2;
    private const int FACE_WIDTH = 106;
    private const int FACE_HEIGHT = 122;
    private const int WEAPONBOX_X = IMAGE_OFFSET_X + 6;
    private const int WEAPONBOX_Y = IMAGE_OFFSET_Y + 296; // 299
    private const int WEAPONBOX_SIZE_X = 61;
    private const int WEAPONBOX_SIZE_Y = 31;
    private const int WEAPONBOX_NUMBER = 8;
    private const int SPACE_BN_LINES = 15; // 13
    private const int STATS_FIRST_COL = STATS_X + 9;
    private const int STATS_SECOND_COL = STATS_FIRST_COL + 129;
    private const int STATS_FIRST_NUM = STATS_X + 111; // 91
    private const int STATS_SECOND_NUM = STATS_X + 235;
    private const int HEALTH_Y = STATS_Y + 34;
    private const int AGILITY_Y = HEALTH_Y + SPACE_BN_LINES;
    private const int DEXTERITY_Y = AGILITY_Y + SPACE_BN_LINES;
    private const int STRENGTH_Y = DEXTERITY_Y + SPACE_BN_LINES;
    private const int LEADERSHIP_Y = STRENGTH_Y + SPACE_BN_LINES;
    private const int WISDOM_Y = LEADERSHIP_Y + SPACE_BN_LINES;
    private const int EXPLEVEL_Y = HEALTH_Y;
    private const int MARKSMAN_Y = AGILITY_Y;
    private const int MECHANAICAL_Y = DEXTERITY_Y;
    private const int EXPLOSIVE_Y = STRENGTH_Y;
    private const int MEDICAL_Y = LEADERSHIP_Y;
    private const int NAME_X = STATS_FIRST_COL;
    private const int NAME_Y = STATS_Y + 7;
    private const int FEE_X = PRICE_X + 7;
    private const int FEE_Y = NAME_Y;
    private const int FEE_WIDTH = 37; // 33
    private const int AIM_CONTRACT_X = PRICE_X + 51;
    private const int AIM_CONTRACT_Y = FEE_Y;
    private const int AIM_CONTRACT_WIDTH = 59;
    private const int ONEDAY_X = AIM_CONTRACT_X;
    private const int ONEWEEK_X = AIM_CONTRACT_X;
    private const int TWOWEEK_X = AIM_CONTRACT_X;
    private const int PREVIOUS_X = 224;
    private const int PREVIOUS_Y = 386 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int PREVIOUS_BOX_Y = PREVIOUS_Y - 4;
    private const int PREVIOUS_BR_X = PREVIOUS_X + BOTTOM_BUTTON_START_WIDTH;
    private const int PREVIOUS_BR_Y = PREVIOUS_BOX_Y + BOTTOM_BUTTON_START_HEIGHT;
    private const int CONTACT_X = 331;
    private const int CONTACT_Y = PREVIOUS_Y;
    private const int CONTACT_BOX_Y = CONTACT_Y - 4;
    private const int CONTACT_BOX_WIDTH = 75;
    private const int CONTACT_BOX_HEIGHT = 18;
    private const int CONTACT_BR_X = CONTACT_X + BOTTOM_BUTTON_START_WIDTH;
    private const int CONTACT_BR_Y = CONTACT_BOX_Y + BOTTOM_BUTTON_START_HEIGHT;
    private const int NEXT_X = 431;
    private const int NEXT_Y = PREVIOUS_Y;
    private const int NEXT_BOX_Y = NEXT_Y - 4;
    private const int NEXT_BR_X = NEXT_X + BOTTOM_BUTTON_START_WIDTH;
    private const int NEXT_BR_Y = NEXT_BOX_Y + BOTTOM_BUTTON_START_HEIGHT;
    private const int AIM_MERC_INFO_X = 124;
    private const int AIM_MERC_INFO_Y = 223 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_MERC_ADD_X = AIM_MERC_ADD_INFO_X;
    private const int AIM_MERC_ADD_Y = 269 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_MERC_ADD_INFO_X = AIM_MERC_INFO_X;
    private const int AIM_MERC_ADD_INFO_Y = AIM_MERC_ADD_Y + 15;
    private const int AIM_MERC_INFO_WIDTH = 470;
    private const int AIM_MEDICAL_DEPOSIT_X = PRICE_X + 5;
    private const int AIM_MEDICAL_DEPOSIT_Y = LEADERSHIP_Y;
    private const int AIM_MEDICAL_DEPOSIT_WIDTH = PRICE_WIDTH - 6;
    private const int AIM_MEMBER_ACTIVE_TEXT_X = IMAGE_OFFSET_X + 149;
    private const int AIM_MEMBER_ACTIVE_TEXT_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT - 1; // + 1
    private const int AIM_MEMBER_ACTIVE_TEXT_WIDTH = AIM_SYMBOL_WIDTH;
    private const int AIM_MEMBER_OPTIONAL_GEAR_X = AIM_MERC_INFO_X;
    private const int AIM_MEMBER_OPTIONAL_GEAR_Y = WEAPONBOX_Y - 13;
    private const int AIM_MEMBER_WEAPON_NAME_X = WEAPONBOX_X;
    private const int AIM_MEMBER_WEAPON_NAME_Y = WEAPONBOX_Y + WEAPONBOX_SIZE_Y + 1;
    private const int AIM_MEMBER_WEAPON_NAME_WIDTH = WEAPONBOX_SIZE_X - 2;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_X = 125;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_Y = 97 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_WIDTH = 368;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_HEIGHT = 150;
    private const int AIM_MEMBER_VIDEO_TITLE_BAR_WIDTH = 368;
    private const int AIM_MEMBER_VIDEO_TITLE_BAR_HEIGHT = 21;
    private const int AIM_MEMBER_VIDEO_TITLE_ITERATIONS = 18;
    private const int AIM_MEMBER_VIDEO_TITLE_START_Y = 382 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_MEMBER_VIDEO_TITLE_END_Y = 96;
    private const int AIM_MEMBER_VIDEO_TITLE_START_X = 330;
    private const int AIM_MEMBER_VIDEO_TITLE_END_X = 125;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_RIGHT = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + AIM_MEMBER_VIDEO_CONF_TERMINAL_WIDTH;
    private const int AIM_MEMBER_VIDEO_CONF_TERMINAL_BOTTOM = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TERMINAL_HEIGHT;
    private const int AIM_MEMBER_VIDEO_CONF_CONTRACT_IMAGE_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 6;
    private const int AIM_MEMBER_VIDEO_CONF_CONTRACT_IMAGE_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + 130;
    private const int AIM_MEMBER_VIDEO_CONF_XCLOSE_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 348;
    private const int AIM_MEMBER_VIDEO_CONF_XCLOSE_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + 3;
    private const int AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y = 20;
    private const int AIM_MEMBER_BUY_CONTRACT_LENGTH_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 113;
    private const int AIM_MEMBER_BUY_CONTRACT_LENGTH_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 15;
    private const int AIM_MEMBER_BUY_EQUIPMENT_GAP = 23;
    private const int AIM_MEMBER_BUY_EQUIPMENT_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 235;
    private const int AIM_MEMBER_AUTHORIZE_PAY_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 113;
    private const int AIM_MEMBER_AUTHORIZE_PAY_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 92;
    private const int AIM_MEMBER_AUTHORIZE_PAY_WIDTH = 116;
    private const int AIM_MEMBER_AUTHORIZE_PAY_GAP = 122;
    private const int AIM_MEMBER_VIDEO_FACE_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 7 + 1;
    private const int AIM_MEMBER_VIDEO_FACE_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 6 + 1;
    private const int AIM_MEMBER_VIDEO_FACE_WIDTH = 96;
    private const int AIM_MEMBER_VIDEO_FACE_HEIGHT = 86;
    private const int AIM_MEMBER_VIDEO_NAME_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 7;
    private const int AIM_MEMBER_VIDEO_NAME_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + 5;
    private const int AIM_CONTRACT_CHARGE_X = AIM_MEMBER_VIDEO_NAME_X;
    private const int AIM_CONTRACT_CHARGE_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 98;
    private const int AIM_CONTRACT_LENGTH_ONE_DAY = 0;
    private const int AIM_CONTRACT_LENGTH_ONE_WEEK = 1;
    private const int AIM_CONTRACT_LENGTH_TWO_WEEKS = 2;
    private const int AIM_SELECT_LIGHT_ON_X = 105;
    private const int AIM_SELECT_LIGHT_ON_Y = 8;
    private const int AIM_SELECT_LIGHT_OFF_X = 105;
    private const int AIM_SELECT_LIGHT_OFF_Y = 7;
    private const int AIM_CONTRACT_CHARGE_AMOUNNT_X = AIM_MEMBER_VIDEO_CONF_TERMINAL_X + 7; // 8
    private const int AIM_CONTRACT_CHARGE_AMOUNNT_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 111; // 114
    private const int AIM_CONTRACT_CHARGE_AMOUNNT_WIDTH = 98;
    private const int AIM_CONTRACT_CHARGE_AMOUNNT_HEIGHT = 12;
    private const int AIM_POPUP_BOX_X = 260;
    private const int AIM_POPUP_BOX_Y = 140 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_POPUP_BOX_WIDTH = 162;
    private const int AIM_POPUP_BOX_HEIGHT = 100;
    private const int AIM_POPUP_BOX_STRING1_Y = 6;
    private const int AIM_POPUP_BOX_BUTTON_OFFSET_X = 20;
    private const int AIM_POPUP_BOX_BUTTON_OFFSET_Y = 62;
    private const int AIM_POPUP_BOX_SUCCESS = 0;
    private const int AIM_POPUP_BOX_FAILURE = 1;
    private const int AIM_MEMBER_HANG_UP_X = 290;
    private const int AIM_MEMBER_HANG_UP_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 42;
    private const int AIM_MEMBER_VIDEO_TALKING_TEXT_X = AIM_MEMBER_AUTHORIZE_PAY_X;
    private const int AIM_MEMBER_VIDEO_TALKING_TEXT_Y = AIM_MEMBER_VIDEO_CONF_TERMINAL_Y + AIM_MEMBER_VIDEO_CONF_TITLE_BAR_HEIGHT_Y + 30;
    private const int AIM_MEMBER_VIDEO_TALKING_TEXT_WIDTH = 240;
    private const int VC_CONTACT_STATIC_TIME = 30;
    private const int VC_CONTACT_FUZZY_LINE_TIME = 100;
    private const int VC_NUM_LINES_SNOW = 6;
    private const int VC_NUM_FUZZ_LINES = 10;
    private const int VC_NUM_STRAIGHT_LINES = 9;
    private const int VC_ANSWER_IMAGE_DELAY = 100;
    private const int QUOTE_FIRST_ATTITUDE_TIME = 3000;
    private const int QUOTE_ATTITUDE_TIME = 10000;
    private const int QUOTE_DELAY_SMALL_TALK = 1;
    private const int QUOTE_DELAY_IMPATIENT_TALK = 2;
    private const int QUOTE_DELAY_VERY_IMPATIENT_TALK = 3;
    private const int QUOTE_DELAY_HANGUP_TALK = 4;
    private const int QUOTE_DELAY_NO_ACTION = 5;
    private const int QUOTE_MERC_BUSY = 6;
    private const int TEXT_POPUP_WINDOW_X = 180;
    private const int TEXT_POPUP_WINDOW_Y = 255 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int TEXT_POPUP_STRING_SIZE = 512;
    private const int FIRST_COLUMN_DOT = 328; // 308
    private const int SECOND_COLUMN_DOT = 451;
    private const int MINIMUM_TALKING_TIME_FOR_MERC = 1500;
    private const int AIM_TEXT_SPEECH_MODIFIER = 80;
    private const int AIM_WEAPONBOX_NAME_WIDTH = 93;

    private const string MERCBIOSFILENAME = "BINARYDATA\\aimbios.edt";




    // static / distortion modes of the video conference display
    private const int VC_NO_STATIC = 0;
    private const int VC_FUZZY_LINE = 1;
    private const int VC_STRAIGHTLINE = 2;
    private const int VC_STATIC_IMAGE = 3;
    private const int VC_BW_SNOW = 4;
    private const int VC_PIXELATE = 5;
    private const int VC_TRANS_SNOW_IN = 6;     // fade from clear to snowy
    private const int VC_TRANS_SNOW_OUT = 7;    // fade from snowy to clear

    private static HVOBJECT guiStats = null!;
    private static HVOBJECT guiPrice = null!;
    private static HVOBJECT guiPortrait = null!;
    private static HVOBJECT guiWeaponBox = null!;
    private static HVOBJECT guiFace = null!;
    private static HVOBJECT guiVideoConfPopup = null!;
    private static HVOBJECT guiVideoConfTerminal = null!;
    private static HVOBJECT guiPopUpBox = null!;
    private static HVOBJECT guiVideoFaceBackground = null!;
    private static HVOBJECT guiBWSnow = null!;
    private static HVOBJECT guiFuzzLine = null!;
    private static HVOBJECT guiStraightLine = null!;
    private static HVOBJECT guiTransSnow = null!;
    private static HVOBJECT guiVideoContractCharge = null!;
    private static HVOBJECT guiVideoTitleBar = null!;

    private static int iAimMembersBoxId = -1;
    private static int gbCurrentSoldier = 0;
    private static bool gfJustSwitchedVideoConferenceMode;
    private static bool gfMercIsTalking = false;
    private static int gubContractLength;
    private static bool gfBuyEquipment;
    private static string gsTalkingMercText = string.Empty;
    private static uint guiTimeThatMercStartedTalking;
    private static int gubCurrentStaticMode;
    private static uint guiMercAttitudeTime;      // time the user is in a screen; if over a certain time, the merc gets miffed
    private static int gubMercAttitudeLevel;      // the current level the merc is annoyed at the caller
    private static bool gfHangUpMerc;             // cancel the video conferencing after the merc is finished talking
    private static bool gfIsShutUpMouseRegionActive;
    private static bool gfIsAnsweringMachineActive;
    private static bool gfRenderTopLevel;
    private static bool gfStopMercFromTalking;
    private static int usAimMercSpeechDuration = 0;
    private static bool gfWaitingForMercToStopTalkingOrUserToClick = false;
    private static int giIdOfLastHiredMerc = -1;
    private static bool gfAimMemberDisplayFaceHelpText = false;
    private static bool gfAimMemberCanMercSayOpeningQuote = true;

    private static ButtonPic guiPreviousContactNextButtonImage = null!;
    private static GUI_BUTTON giPreviousButton = null!;
    private static GUI_BUTTON giContactButton = null!;
    private static GUI_BUTTON giNextButton = null!;

    private static readonly ButtonPic[] guiVideoConferenceButtonImage = new ButtonPic[3];
    private static readonly GUI_BUTTON[] giContractLengthButton = new GUI_BUTTON[3];
    private static readonly GUI_BUTTON[] giBuyEquipmentButton = new GUI_BUTTON[2];
    private static readonly GUI_BUTTON[] giAuthorizeButton = new GUI_BUTTON[2];
    private static GUI_BUTTON giHangUpButton = null!;
    private static GUI_BUTTON guiPopUpOkButton = null!;
    private static ButtonPic guiPopUpImage = null!;
    private static readonly GUI_BUTTON[] giFirstContactButton = new GUI_BUTTON[2];
    private static readonly GUI_BUTTON[] giAnsweringMachineButton = new GUI_BUTTON[2];
    private static ButtonPic giXToCloseVideoConfButtonImage = null!;
    private static GUI_BUTTON giXToCloseVideoConfButton = null!;

    private static MOUSE_REGION gSelectedFaceRegion = new(nameof(gSelectedFaceRegion));
    private static MOUSE_REGION gSelectedShutUpMercRegion = new(nameof(gSelectedShutUpMercRegion));
    internal static bool gfIsNewMailFlagSet;

    public static void GameInitAIMMembers()
    {
    }

    // Note: the video conference is ported without the face/dialogue systems, see Laptop.AimMembers.Pending.cs: the merc
    // does not speak, and the connecting / static animations are skipped.
    public static bool EnterAIMMembers()
    {
        // load the stats graphic and add it
        guiStats = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\stats.sti"));

        // load the Price graphic and add it
        guiPrice = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\price.sti"));

        // load the Portait graphic and add it
        guiPortrait = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\portrait.sti"));

        // load the WeaponBox graphic and add it
        guiWeaponBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\weaponbox.sti"));

        // load the videoconf Popup graphic and add it
        guiVideoConfPopup = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\VideoConfPopup.sti"));

        // load the video conf terminal graphic and add it
        guiVideoConfTerminal = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\VideoConfTerminal.sti"));

        // load the contract charge graphic and add it
        guiVideoContractCharge = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\VideoContractCharge.sti"));

        // ** Mouse Regions **
        MouseSubSystem.MSYS_DefineRegion(gSelectedFaceRegion, PORTRAIT_X, PORTRAIT_Y, PORTRAIT_X + PORTRAIT_WIDTH, PORTRAIT_Y + PORTRAIT_HEIGHT, MSYS_PRIORITY.HIGH, CURSOR.WWW, SelectFaceMovementRegionCallBack, SelectFaceRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedFaceRegion);

        // if user clicks in the area, the merc will shut up!
        MouseSubSystem.MSYS_DefineRegion(gSelectedShutUpMercRegion, LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y, MSYS_PRIORITY.HIGH - 1, CURSOR.LAPTOP_SCREEN, null, SelectShutUpMercRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedShutUpMercRegion);

        // have it disabled at first
        MouseSubSystem.MSYS_DisableRegion(ref gSelectedShutUpMercRegion);

        // Button Regions
        giXToCloseVideoConfButtonImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\x_button.sti", -1, 0, -1, 1, -1);
        guiPreviousContactNextButtonImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1, 0, -1, 1, -1);

        giPreviousButton = CreatePrevContactNextButton(CharacterInfo[AIM_MEMBER_PREVIOUS], PREVIOUS_X, PREVIOUS_BOX_Y, BtnPreviousButtonCallback);
        giContactButton = CreatePrevContactNextButton(CharacterInfo[AIM_MEMBER_CONTACT], CONTACT_X, CONTACT_BOX_Y, BtnContactButtonCallback);
        giNextButton = CreatePrevContactNextButton(CharacterInfo[AIM_MEMBER_NEXT], NEXT_X, NEXT_BOX_Y, BtnNextButtonCallback);

        gbCurrentSoldier = AimMercArray[gbCurrentIndex];

        gfStopMercFromTalking = false;
        gubVideoConferencingMode = (AIM_VIDEO)giCurrentSubPage;
        gubVideoConferencingPreviousMode = AIM_VIDEO.NOT_DISPLAYED_MODE;

        gfRenderTopLevel = false;

        // if we are re-entering but the video conference should still be up
        if (gubVideoConferencingMode != AIM_VIDEO.NOT_DISPLAYED_MODE)
        {
            // if we need to re initialize the talking face
            if (gubVideoConferencingMode != AIM_VIDEO.FIRST_CONTACT_MERC_MODE)
            {
                InitVideoFace(gbCurrentSoldier);
            }

            InitDeleteVideoConferencePopUp();
        }

        InitAimMenuBar();
        InitAimDefaults();

        RenderAIMMembers();
        gfIsNewMailFlagSet = false;
        gfAimMemberCanMercSayOpeningQuote = true;

        return true;
    }

    private static GUI_BUTTON CreatePrevContactNextButton(string text, int x, int y, GUI_CALLBACK callback)
    {
        GUI_BUTTON button = ButtonSubSystem.CreateIconAndTextButton(
            guiPreviousContactNextButtonImage,
            text,
            AIM_M_FONT_PREV_NEXT_CONTACT,
            AIM_M_FONT_PREV_NEXT_CONTACT_COLOR_UP,
            FontShadow.DEFAULT_SHADOW,
            AIM_M_FONT_PREV_NEXT_CONTACT_COLOR_DOWN,
            FontShadow.DEFAULT_SHADOW,
            ButtonTextJustifies.TEXT_CJUSTIFIED,
            new Point(x, y),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.DefaultMoveCallback,
            callback);
        ButtonSubSystem.SetButtonCursor(button, CURSOR.WWW);
        buttonList.Add(button);

        return button;
    }

    public static void ExitAIMMembers()
    {
        RemoveAimDefaults();

        // if we are exiting and the transfer of funds popup is enable, make sure we dont come back to it
        giCurrentSubPage = gubPopUpBoxAction != AIM_POPUP.NOTHING ? (int)AIM_VIDEO.NOT_DISPLAYED_MODE : (int)gubVideoConferencingMode;
        gubVideoConferencingMode = AIM_VIDEO.NOT_DISPLAYED_MODE;

        InitDeleteVideoConferencePopUp();

        video.DeleteVideoObject(guiVideoConfPopup);
        video.DeleteVideoObject(guiVideoConfTerminal);
        video.DeleteVideoObject(guiVideoContractCharge);
        video.DeleteVideoObject(guiStats);
        video.DeleteVideoObject(guiPrice);
        video.DeleteVideoObject(guiPortrait);
        video.DeleteVideoObject(guiWeaponBox);

        ButtonSubSystem.UnloadButtonImage(guiPreviousContactNextButtonImage);

        foreach (GUI_BUTTON button in new[] { giPreviousButton, giContactButton, giNextButton })
        {
            buttonList.Remove(button);
            ButtonSubSystem.RemoveButton(button);
        }

        MouseSubSystem.MSYS_RemoveRegion(gSelectedFaceRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedShutUpMercRegion);

        ButtonSubSystem.UnloadButtonImage(giXToCloseVideoConfButtonImage);

        ExitAimMenuBar();
        InitCreateDeleteAimPopUpBox(AIM_POPUP.DELETE, null, null, 0, 0, 0);
    }

    public static void HandleAIMMembers()
    {

        //determine if the merc has a quote that is waiting to be said
        DelayMercSpeech(0, (QUOTE)0, 0, false, false);

        if (gfHangUpMerc && !gfMercIsTalking)
        {
            if (gubVideoConferencingMode != AIM_VIDEO.NOT_DISPLAYED_MODE)
            {
                gubVideoConferencingMode = AIM_VIDEO.POPDOWN_MODE;
            }

            gfHangUpMerc = false;
        }

        if (gfStopMercFromTalking)
        {
            StopMercTalking();
            gfStopMercFromTalking = false;
        }

        // If we have to change video conference modes, change to new mode
        if (gubVideoConferencingMode != gubVideoConferencingPreviousMode && gubPopUpBoxAction != AIM_POPUP.DISPLAY)
        {
            InitDeleteVideoConferencePopUp();

            //if we are exiting to display a popup box, dont rerender the display
            if (!fExitDueToMessageBox)
            {
                gfRedrawScreen = true;
            }
        }

        // If we have to get rid of the popup box
        if (gubPopUpBoxAction == AIM_POPUP.DELETE)
        {
            InitCreateDeleteAimPopUpBox(AIM_POPUP.DELETE, null, null, 0, 0, 0);

            //if we are exiting to display a popup box, dont rerender the display
            if (!fExitDueToMessageBox)
            {
                gfRedrawScreen = true;
            }
        }

        // Handle the current video conference screen
        HandleCurrentVideoConfMode();

        // if the face is active, display the talking face
        if (gfVideoFaceActive)
        {
            gfMercIsTalking = DisplayTalkingMercFaceForVideoPopUp(giMercFaceIndex);

            //put the noise lines on the screen
            if (!gfIsAnsweringMachineActive)
            {
                HandleVideoDistortion();
            }

            //to handle when/if the merc is getting po'ed (waiting for player to do something)
            if (!gfMercIsTalking)
            {
                HandleMercAttitude();
            }
        }

        //if we have to rerender the popup, set the flag to render the PostButtonRender function in laptop.c
        if (gubPopUpBoxAction == AIM_POPUP.DISPLAY)
        {
            fReDrawPostButtonRender = true;
        }

        //Gets set in the InitDeleteVideoConferencePopUp() function
        if (gfJustSwitchedVideoConferenceMode)
        {
            gfJustSwitchedVideoConferenceMode = false;
        }

        if (gfRedrawScreen)
        {
            RenderAIMMembers();
            gfRedrawScreen = false;
        }

        ButtonSubSystem.MarkButtonsDirty(buttonList);
    }


    public static bool RenderAIMMembers()
    {
      HVOBJECT	hStatsHandle;
      HVOBJECT	hPriceHandle;
      HVOBJECT	hWeaponBoxHandle;
    	int		x, uiPosX;
    	string wTemp = string.Empty;

    	DrawAimDefaults();

    	// Stats
    	hStatsHandle = guiStats;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hStatsHandle, 0,STATS_X, STATS_Y, VO_BLT.SRCTRANSPARENCY);

    	// Price
    	hPriceHandle = guiPrice;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPriceHandle, 0,PRICE_X, PRICE_Y, VO_BLT.SRCTRANSPARENCY);

    	// WeaponBox
    	hWeaponBoxHandle = guiWeaponBox;

    	uiPosX = WEAPONBOX_X;
    	for(x=0; x<WEAPONBOX_NUMBER; x++)
    	{
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hWeaponBoxHandle, 0, uiPosX, WEAPONBOX_Y, VO_BLT.SRCTRANSPARENCY);
    		uiPosX += WEAPONBOX_SIZE_X;
    	}

      UpdateMercInfo();

    	//Draw fee & contract
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_FEE], new(FEE_X, FEE_Y), 0, AIM_M_FONT_PREV_NEXT_CONTACT, AIM_M_FEE_CONTRACT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_CONTRACT], new(AIM_CONTRACT_X, AIM_CONTRACT_Y), AIM_CONTRACT_WIDTH, AIM_M_FONT_PREV_NEXT_CONTACT, AIM_M_FEE_CONTRACT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

    	//Draw pay period (day, week, 2 week)
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_1_DAY], new(ONEDAY_X, EXPLEVEL_Y), AIM_CONTRACT_WIDTH, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_1_WEEK], new(ONEWEEK_X, MARKSMAN_Y), AIM_CONTRACT_WIDTH, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_2_WEEKS], new(TWOWEEK_X, MECHANAICAL_Y), AIM_CONTRACT_WIDTH, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

    	//Display AIM Member text
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_ACTIVE_MEMBERS], new(AIM_MEMBER_ACTIVE_TEXT_X, AIM_MEMBER_ACTIVE_TEXT_Y), AIM_MEMBER_ACTIVE_TEXT_WIDTH, AIM_MAINTITLE_FONT, AIM_M_ACTIVE_MEMBER_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

    	//Display Option Gear Cost text
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_OPTIONAL_GEAR], new(AIM_MEMBER_OPTIONAL_GEAR_X, AIM_MEMBER_OPTIONAL_GEAR_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	wTemp = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(gbCurrentSoldier)].usOptionalGearCost ) );
    	uiPosX = AIM_MEMBER_OPTIONAL_GEAR_X + FontSubSystem.StringPixLength( CharacterInfo[AIM_MEMBER_OPTIONAL_GEAR], AIM_M_FONT_STATIC_TEXT) + 5;
    	FontSubSystem.DrawTextToScreen(wTemp, new(uiPosX, AIM_MEMBER_OPTIONAL_GEAR_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_DYNAMIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	DisableAimButton();

    	DisplayMercsInventory((NPCID)gbCurrentSoldier);


    	DisplayMercsFace();

    	if (gubVideoConferencingMode != AIM_VIDEO.NOT_DISPLAYED_MODE)
    	{
    		DisplayMercStats();
    		DisplayVideoConferencingDisplay();
    	}
    	else
    	{
    		// Display the mercs stats and face
    		DisplayMercStats();

    		gubMercAttitudeLevel = 0;
    		gfIsAnsweringMachineActive = false;
    	}

    //	InitCreateDeleteAimPopUpBox( AIM_POPUP_DISPLAY, NULL, NULL, 0, 0, 0);

    	//check to see if the merc is dead if so disable the contact button
    	if( IsMercDead( gbCurrentSoldier ) )
    	{
    		ButtonSubSystem.DisableButton( giContactButton );
    	}
    	else
    	{
    		ButtonSubSystem.EnableButton( giContactButton );
    	}


    	//if we are to renbder the 'click face' text
    	if(	gfAimMemberDisplayFaceHelpText )
    	{
    		DisplayAimMemberClickOnFaceHelpText();
    	}




    	RenderWWWProgramTitleBar( );
    	DisplayProgramBoundingBox( true );
    	fReDrawScreenFlag = true;
	
    	return(true);
    }


    private static bool DrawNumeralsToScreen(int iNumber, int bWidth, int usLocX, int usLocY, FontStyle ulFont, FontColor ubColor)
    {
    	string sStr = string.Empty;

    	sStr = wprintf( "%d", iNumber); 

    	FontSubSystem.DrawTextToScreen(sStr, new(usLocX, usLocY), bWidth, ulFont, ubColor, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

    	return(true);
    }


    private static bool DrawMoneyToScreen(int iNumber, int bWidth, int usLocX, int usLocY, FontStyle ulFont, FontColor ubColor)
    {
    	string sStr = string.Empty;

    	sStr = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( iNumber ) );

    //	FontSubSystem.DrawTextToScreen(L"$", new(usLocX, usLocY), 0, ulFont, ubColor, FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	FontSubSystem.DrawTextToScreen(sStr, new(usLocX, usLocY), bWidth, ulFont, ubColor, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

    	return(true);
    }


    private static void SelectFaceRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
    	{
    	}
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
    	{
    		guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX;
    	} 
    	else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
    	{
    		//if the merc is not dead, video conference with the merc
    		if( !IsMercDead( gbCurrentSoldier ) )
    		{
    			gubVideoConferencingMode = AIM_VIDEO.POPUP_MODE;
    			gfFirstTimeInContactScreen = true;
    		}
    	} 
    }


    private static void SelectFaceMovementRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    { 
    	if( iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
    	{
    		gfAimMemberDisplayFaceHelpText = false;
    		gfRedrawScreen = true;
    	}
    	else if( iReason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE) )
    	{
    		gfAimMemberDisplayFaceHelpText = true;
    		gfRedrawScreen = true;
    	}
    	else if( iReason.HasFlag(MSYS_CALLBACK_REASON.MOVE) )
    	{
    	}
    }


    private static bool UpdateMercInfo()
    {
    	int					PosY = 300;
      string MercInfoString = string.Empty;
      string AdditionalInfoString = string.Empty;

    	//Display the salaries
    	DrawMoneyToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].sSalary, FEE_WIDTH, FEE_X, HEALTH_Y, AIM_M_NUMBER_FONT, AIM_M_COLOR_DYNAMIC_TEXT	);
    	DrawMoneyToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].uiWeeklySalary, FEE_WIDTH, FEE_X, AGILITY_Y, AIM_M_NUMBER_FONT, AIM_M_COLOR_DYNAMIC_TEXT	);
    	DrawMoneyToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].uiBiWeeklySalary, FEE_WIDTH, FEE_X, DEXTERITY_Y, AIM_M_NUMBER_FONT, AIM_M_COLOR_DYNAMIC_TEXT	);

    	//if medical deposit is required
    	if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedicalDeposit != 0 )
    	{
    		string zTemp = string.Empty;
    		string sMedicalString = string.Empty;

    		// Display the medical cost
    		zTemp = Finances.InsertDollarSignInToString( Finances.InsertCommasForDollarFigure( gMercProfiles[(NPCID)(gbCurrentSoldier)].sMedicalDepositAmount ) );

    		sMedicalString = wprintf( "%s %s", zTemp, CharacterInfo[AIM_MEMBER_MEDICAL_DEPOSIT_REQ] ); 
	
    		// If the string will be displayed in more then 2 lines, recenter the string
    		if( ( FontSubSystem.DisplayWrappedString(new(0, 0), AIM_MEDICAL_DEPOSIT_WIDTH, 2, AIM_FONT12ARIAL, AIM_M_COLOR_DYNAMIC_TEXT, sMedicalString, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED | TextJustifies.DONT_DISPLAY_TEXT) / FontSubSystem.GetFontHeight( AIM_FONT12ARIAL ) ) > 2 )
    		{
    			FontSubSystem.DisplayWrappedString(new(AIM_MEDICAL_DEPOSIT_X, (int)(AIM_MEDICAL_DEPOSIT_Y - FontSubSystem.GetFontHeight( AIM_FONT12ARIAL ) )), AIM_MEDICAL_DEPOSIT_WIDTH, 2, AIM_FONT12ARIAL, AIM_M_COLOR_DYNAMIC_TEXT, sMedicalString, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    		}
    		else
    			FontSubSystem.DisplayWrappedString(new(AIM_MEDICAL_DEPOSIT_X, AIM_MEDICAL_DEPOSIT_Y), AIM_MEDICAL_DEPOSIT_WIDTH, 2, AIM_FONT12ARIAL, AIM_M_COLOR_DYNAMIC_TEXT, sMedicalString, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}

    	LoadMercBioInfo( gbCurrentSoldier, out MercInfoString, out AdditionalInfoString);
    	if( MercInfoString.Length > 0)
    	{
    		FontSubSystem.DisplayWrappedString(new(AIM_MERC_INFO_X, AIM_MERC_INFO_Y), AIM_MERC_INFO_WIDTH, 2, AIM_M_FONT_DYNAMIC_TEXT, AIM_FONT_MCOLOR_WHITE, MercInfoString, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}
    	if( AdditionalInfoString.Length > 0)
    	{
    		FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_ADDTNL_INFO], new(AIM_MERC_ADD_X, AIM_MERC_ADD_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    		FontSubSystem.DisplayWrappedString(new(AIM_MERC_ADD_INFO_X, AIM_MERC_ADD_INFO_Y), AIM_MERC_INFO_WIDTH, 2, AIM_M_FONT_DYNAMIC_TEXT, AIM_FONT_MCOLOR_WHITE, AdditionalInfoString, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}

    	return(true);
    }


    private static bool LoadMercBioInfo(int ubIndex, out string pInfoString, out string pAddInfo)
    {
        // Get current mercs bio info (the file is "encrypted": every character above 32 is stored incremented by one)
        int uiStartSeekAmount = (SIZE_MERC_BIO_INFO + SIZE_MERC_ADDITIONAL_INFO) * ubIndex;
        pInfoString = string.Empty;
        pAddInfo = string.Empty;

        if (!files.LoadEncryptedDataFromFile(MERCBIOSFILENAME, out pInfoString, uiStartSeekAmount, SIZE_MERC_BIO_INFO))
        {
            return false;
        }

        // Get the additional info
        uiStartSeekAmount += SIZE_MERC_BIO_INFO;

        return files.LoadEncryptedDataFromFile(MERCBIOSFILENAME, out pAddInfo, uiStartSeekAmount, SIZE_MERC_ADDITIONAL_INFO);
    }


    private static readonly System.Collections.Generic.Dictionary<Image<Rgba32>, Image<Rgba32>> itemPictures = [];

    /// <summary>
    /// The item pictures use pure green for the pixels the original highlights when an item is selected; drawn
    /// normally they are dark. TODO(port): the shade tables the original draws item pictures through.
    /// </summary>
    private static Image<Rgba32> ItemPictureWithoutOutline(Image<Rgba32> picture)
    {
        if (!itemPictures.TryGetValue(picture, out var plain))
        {
            plain = picture.Clone();
            plain.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (ref Rgba32 pixel in accessor.GetRowSpan(y))
                    {
                        if (pixel.A != 0 && pixel.R == 0 && pixel.G == 255 && pixel.B == 0)
                        {
                            pixel = new Rgba32(16, 16, 16, 255);
                        }
                    }
                }
            });

            itemPictures[picture] = plain;
        }

        return plain;
    }

    /// <summary>The shadow of an item: every pixel the item covers darkens the picture behind it (as the original's shadow shade table does).</summary>
    private static void DrawItemShadow(Image<Rgba32> picture, int left, int top)
    {
        Image<Rgba32> frame = video.Surfaces[SurfaceType.FRAME_BUFFER];

        frame.ProcessPixelRows(picture, (target, source) =>
        {
            for (int y = 0; y < source.Height; y++)
            {
                int ty = top + y;
                if (ty < 0 || ty >= target.Height)
                {
                    continue;
                }

                Span<Rgba32> targetRow = target.GetRowSpan(ty);
                Span<Rgba32> sourceRow = source.GetRowSpan(y);
                for (int x = 0; x < sourceRow.Length; x++)
                {
                    int tx = left + x;
                    if (sourceRow[x].A == 0 || tx < 0 || tx >= targetRow.Length)
                    {
                        continue;
                    }

                    ref Rgba32 pixel = ref targetRow[tx];
                    pixel.R = (byte)(pixel.R * 45 / 100);
                    pixel.G = (byte)(pixel.G * 45 / 100);
                    pixel.B = (byte)(pixel.B * 45 / 100);
                }
            }
        });
    }

    private static bool DisplayMercsInventory(NPCID ubMercID)
    {
        MERCPROFILESTRUCT profile = gMercProfiles[ubMercID];
        int ubItemCount = 0;

        // if the mercs inventory has already been purchased, dont display the inventory
        if (profile.ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.ALREADY_USED_ITEMS))
        {
            return true;
        }

        int PosY = WEAPONBOX_Y;
        int PosX = WEAPONBOX_X + 3;   // + 3 ( 1 to take care of the shadow, +2 to get past the weapon box border )
        for (InventorySlot i = 0; i < NUM_INV_SLOTS; i++)
        {
            Items usItem = profile.inv[i];

            // if its a valid item AND we are only displaying less then 8 items
            if (usItem != Items.NONE && ubItemCount < WEAPONBOX_NUMBER)
            {
                // increase the item count
                ubItemCount++;

                // the item's picture, centred in its box, with a shadow behind it
                INVTYPE pItem = Item[usItem];
                HVOBJECT itemGraphics = Interface.GetInterfaceGraphicForItem(pItem);
                if (pItem.ubGraphicNum >= 0 && pItem.ubGraphicNum < itemGraphics.Images.Length)
                {
                    Image<Rgba32> picture = ItemPictureWithoutOutline(itemGraphics.Images[pItem.ubGraphicNum]);
                    int sCenX = PosX + Math.Abs(WEAPONBOX_SIZE_X - 3 - picture.Width) / 2;
                    int sCenY = PosY + Math.Abs(WEAPONBOX_SIZE_Y - picture.Height) / 2;

                    // shadow: darkens what is behind the item's shape, two pixels down and to the left
                    DrawItemShadow(picture, sCenX - 2, sCenY + 2);
                    video.BlitSurfaceToSurface(picture, SurfaceType.FRAME_BUFFER, Point.Empty, new(sCenX, sCenY));
                }

                // if there are more then 1 piece of equipment in the current slot, display how many there are
                if (profile.bInvNumber[(int)i] > 1)
                {
                    string zTempStr = wprintf("x%d", profile.bInvNumber[(int)i]);

                    FontSubSystem.DrawTextToScreen(zTempStr, new(PosX - 1, PosY + 20), AIM_MEMBER_WEAPON_NAME_WIDTH, AIM_M_FONT_DYNAMIC_TEXT, AIM_M_WEAPON_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
                }

                string gzItemName = ShortItemNames.TryGetValue(usItem, out string? name) ? name : string.Empty;

                // if this will only be a single line, center it in the box
                if ((FontSubSystem.DisplayWrappedString(new(PosX - 1, AIM_MEMBER_WEAPON_NAME_Y), AIM_MEMBER_WEAPON_NAME_WIDTH, 2, AIM_M_WEAPON_TEXT_FONT, AIM_M_WEAPON_TEXT_COLOR, gzItemName, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED | TextJustifies.DONT_DISPLAY_TEXT) / FontSubSystem.GetFontHeight(AIM_M_WEAPON_TEXT_FONT)) == 1)
                {
                    FontSubSystem.DisplayWrappedString(new(PosX - 1, AIM_MEMBER_WEAPON_NAME_Y + FontSubSystem.GetFontHeight(AIM_M_WEAPON_TEXT_FONT) / 2), AIM_MEMBER_WEAPON_NAME_WIDTH, 2, AIM_M_WEAPON_TEXT_FONT, AIM_M_WEAPON_TEXT_COLOR, gzItemName, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
                }
                else
                {
                    FontSubSystem.DisplayWrappedString(new(PosX - 1, AIM_MEMBER_WEAPON_NAME_Y), AIM_MEMBER_WEAPON_NAME_WIDTH, 2, AIM_M_WEAPON_TEXT_FONT, AIM_M_WEAPON_TEXT_COLOR, gzItemName, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
                }

                PosX += WEAPONBOX_SIZE_X;
            }
        }

        return true;
    }


    private static void BtnPreviousButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
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
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    			if( gbCurrentIndex > 0)
    				gbCurrentIndex--;
    			else
    				gbCurrentIndex = MAX_NUMBER_MERCS - 1;

    			gfRedrawScreen = true;

    			gbCurrentSoldier = AimMercArray[gbCurrentIndex];

    			gubVideoConferencingMode = AIM_VIDEO.NOT_DISPLAYED_MODE;
    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnContactButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
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
    			//if we are not already in the video conferemce mode, go in to it
    			if( gubVideoConferencingMode == AIM_VIDEO.NOT_DISPLAYED_MODE )
    			{
    				gubVideoConferencingMode = AIM_VIDEO.POPUP_MODE;
    				gfFirstTimeInContactScreen = true;
    			}

    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );

    			InitCreateDeleteAimPopUpBox(AIM_POPUP.DELETE, null, null, 0, 0, 0);

    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static void BtnNextButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
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
    			btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    			if( gbCurrentIndex < MAX_NUMBER_MERCS-1 )
    				gbCurrentIndex++;
    			else
    				gbCurrentIndex = 0;

    			gbCurrentSoldier = AimMercArray[gbCurrentIndex];

    			gfRedrawScreen = true;			

    			gubVideoConferencingMode = AIM_VIDEO.NOT_DISPLAYED_MODE;

    			ButtonSubSystem.InvalidateButtonArea(btn);
    		}
    	}
    	if(reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
    	{
    		btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON );
    		ButtonSubSystem.InvalidateButtonArea(btn);
    	}
    }


    private static bool DisplayMercsFace()
    {
      HVOBJECT hFaceHandle;
      HVOBJECT hPortraitHandle;
    	string							sFaceLoc = "FACES\\BIGFACES\\";
    	string sTemp = string.Empty;
    	SOLDIERTYPE? pSoldier=null;

    	//See if the merc is currently hired
    	pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID( (NPCID)gbCurrentSoldier, true );

    	// Portrait Frame
    	hPortraitHandle = guiPortrait;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPortraitHandle, 0,PORTRAIT_X, PORTRAIT_Y, VO_BLT.SRCTRANSPARENCY);

    	// load the Face graphic and add it
      sTemp = wprintf( "%s%02d.sti", sFaceLoc, gbCurrentSoldier);
    	guiFace = video.GetVideoObject(Utils.FilenameForBPP(sTemp));

    	//Blt face to screen 
    	hFaceHandle = guiFace;
      video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0,FACE_X, FACE_Y, VO_BLT.SRCTRANSPARENCY);

    	//if the merc is dead
    	if( IsMercDead( gbCurrentSoldier ) )
    	{
    		//shade the face red, (to signif that he is dead)
    		hFaceHandle.pShades[ 0 ]		= video.Create16BPPPaletteShaded( hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true );

    		//set the red pallete to the face
    		hFaceHandle.ShadeCurrentPixels = hFaceHandle.pShades[ 0 ];

    		//Blt face to screen 
    		video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0,FACE_X, FACE_Y, VO_BLT.SRCTRANSPARENCY);

    		//if the merc is dead, display it
    		FontSubSystem.DrawTextToScreen(AimPopUpText[AIM_MEMBER_DEAD], new(FACE_X+1, FACE_Y+107), FACE_WIDTH, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}

    	//else if the merc is currently a POW or, the merc was fired as a pow
    	else if( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMercStatus == MercStatus.MERC_FIRED_AS_A_POW || ( pSoldier is not null && pSoldier.bAssignment == Assignment.ASSIGNMENT_POW ) )
    	{
    		ShadowRect( FACE_X, FACE_Y, FACE_X + FACE_WIDTH, FACE_Y + FACE_HEIGHT);
    		FontSubSystem.DrawTextToScreen(pPOWStrings[0], new(FACE_X+1, FACE_Y+107), FACE_WIDTH, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}
	

    	//else if the merc has already been hired
    	else if( pSoldier is not null )
    	{
    		ShadowRect( FACE_X, FACE_Y, FACE_X + FACE_WIDTH, FACE_Y + FACE_HEIGHT);
    		FontSubSystem.DrawTextToScreen(MercInfo[MERC_FILES_ALREADY_HIRED], new(FACE_X+1, FACE_Y+107), FACE_WIDTH, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}

    	else if( !IsMercHireable( gbCurrentSoldier ) )
    	{
    		//else if the merc has a text file and the merc is not away
    		ShadowRect( FACE_X, FACE_Y, FACE_X + FACE_WIDTH, FACE_Y + FACE_HEIGHT);
    		FontSubSystem.DrawTextToScreen(AimPopUpText[AIM_MEMBER_ON_ASSIGNMENT], new(FACE_X+1, FACE_Y+107), FACE_WIDTH, FontStyle.FONT14ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    	}

    	video.DeleteVideoObject(guiFace);

    	return( true );
    }


    private static void DisplayMercStats()
    {
    	FontColor ubColor;

    	//
    	// Display all the static text
    	//

    	//First column in stats box.  Health, Agility, dexterity, strength, leadership, wisdom
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_HEALTH], new(STATS_FIRST_COL, HEALTH_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, HEALTH_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_HEALTH]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_AGILITY], new(STATS_FIRST_COL, AGILITY_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, AGILITY_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_AGILITY]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_DEXTERITY], new(STATS_FIRST_COL, DEXTERITY_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, DEXTERITY_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_DEXTERITY]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_STRENGTH], new(STATS_FIRST_COL, STRENGTH_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, STRENGTH_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_STRENGTH]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_LEADERSHIP], new(STATS_FIRST_COL, LEADERSHIP_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, LEADERSHIP_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_LEADERSHIP]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_WISDOM], new(STATS_FIRST_COL, WISDOM_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_FIRST_COL, WISDOM_Y, FIRST_COLUMN_DOT, CharacterInfo[AIM_MEMBER_WISDOM]);


    	//Second column in stats box.  Exp.Level, Markmanship, mechanical, explosive, medical
    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_EXP_LEVEL], new(STATS_SECOND_COL, EXPLEVEL_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_SECOND_COL, EXPLEVEL_Y, SECOND_COLUMN_DOT, CharacterInfo[AIM_MEMBER_EXP_LEVEL]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_MARKSMANSHIP], new(STATS_SECOND_COL, MARKSMAN_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_SECOND_COL, MARKSMAN_Y, SECOND_COLUMN_DOT, CharacterInfo[AIM_MEMBER_MARKSMANSHIP]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_MECHANICAL], new(STATS_SECOND_COL, MECHANAICAL_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_SECOND_COL, MECHANAICAL_Y, SECOND_COLUMN_DOT, CharacterInfo[AIM_MEMBER_MECHANICAL]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_EXPLOSIVE], new(STATS_SECOND_COL, EXPLOSIVE_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_SECOND_COL, EXPLOSIVE_Y, SECOND_COLUMN_DOT, CharacterInfo[AIM_MEMBER_EXPLOSIVE]);

    	FontSubSystem.DrawTextToScreen(CharacterInfo[AIM_MEMBER_MEDICAL], new(STATS_SECOND_COL, MEDICAL_Y), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	DisplayDots(STATS_SECOND_COL, MEDICAL_Y, SECOND_COLUMN_DOT, CharacterInfo[AIM_MEMBER_MEDICAL]);


    	//
    	// Display all the Merc dynamic stat info
    	//

    	//Name
    	FontSubSystem.DrawTextToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].zName, new(NAME_X, NAME_Y), 0, FontStyle.FONT14ARIAL, AIM_M_COLOR_DYNAMIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

    	// Numbers for above.   Health, Agility, dexterity, strength, leadership, wisdom

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bLife );
    	DrawNumeralsToScreen( gMercProfiles[(NPCID)(gbCurrentSoldier)].bLife, 3, STATS_FIRST_NUM, HEALTH_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bAgility );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bAgility, 3, STATS_FIRST_NUM, AGILITY_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bDexterity );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bDexterity, 3, STATS_FIRST_NUM, DEXTERITY_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bStrength );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bStrength, 3, STATS_FIRST_NUM, STRENGTH_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bLeadership );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bLeadership, 3, STATS_FIRST_NUM, LEADERSHIP_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bWisdom );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bWisdom, 3, STATS_FIRST_NUM, WISDOM_Y, AIM_M_NUMBER_FONT, ubColor	);

    	//Second column in stats box.  Exp.Level, Markmanship, mechanical, explosive, medical

    //	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bExpLevel );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bExpLevel, 3, STATS_SECOND_NUM, EXPLEVEL_Y, AIM_M_NUMBER_FONT, FontColor.FONT_MCOLOR_WHITE	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMarksmanship );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bMarksmanship, 3, STATS_SECOND_NUM, MARKSMAN_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMechanical );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bMechanical, 3, STATS_SECOND_NUM, MECHANAICAL_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bExplosive );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bExplosive, 3, STATS_SECOND_NUM, EXPLOSIVE_Y, AIM_M_NUMBER_FONT, ubColor	);

    	ubColor = GetStatColor( gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedical );
    	DrawNumeralsToScreen(gMercProfiles[(NPCID)(gbCurrentSoldier)].bMedical, 3, STATS_SECOND_NUM, MEDICAL_Y, AIM_M_NUMBER_FONT, ubColor	);
    }


    private static FontColor GetStatColor(int bStat)
    {
    	if( bStat >= 80 )
    		return( HIGH_STAT_COLOR );
    	else if( bStat >= 50 )
    		return( MED_STAT_COLOR );
    	else
    		return( LOW_STAT_COLOR );
    }


    private static void DisplayDots(int usNameX, int usNameY, int usStatX, string pString)
    {
    	int sNumberOfDots;
    	int usStringLength = FontSubSystem.StringPixLength(pString, AIM_M_FONT_STATIC_TEXT);
    	int	 i;
    	int usPosX;

    	sNumberOfDots = (usStatX - usNameX - usStringLength) / 7;

    	usPosX = usStatX;
    	for(i=usNameX + usStringLength; i <= usPosX; usPosX-=7)
    	{
    		FontSubSystem.DrawTextToScreen(".", new((int)usPosX, usNameY), 0, AIM_M_FONT_STATIC_TEXT, AIM_M_COLOR_STATIC_TEXT, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
    	}
    }


    private static void ResetMercAnnoyanceAtPlayer(NPCID ubMercID)
    {
    	//if merc is still annoyed, reset back to 0

    	if ( ubMercID == NPCID.LARRY_NORMAL  )
    	{
    		if ( Facts.CheckFact( FACT.LARRY_CHANGED, 0 ) )
    		{
    			ubMercID = NPCID.LARRY_DRUNK;
    		}
    	}
    	else if ( ubMercID == NPCID.LARRY_DRUNK )
    	{
    		if ( Facts.CheckFact( FACT.LARRY_CHANGED, 0 ) == false )
    		{
    			ubMercID = NPCID.LARRY_NORMAL;
    		}
    	}
    	if( ( gMercProfiles[(NPCID)(ubMercID)].bMercStatus == MercStatus.MERC_ANNOYED_WONT_CONTACT ) || ( gMercProfiles[(NPCID)(ubMercID)].bMercStatus == MercStatus.MERC_ANNOYED_BUT_CAN_STILL_CONTACT ) )
    		gMercProfiles[(NPCID)(ubMercID)].bMercStatus = 0;
    }


    private static bool DisableNewMailMessage()
    {
    	if( Emails.fNewMailFlag && gubVideoConferencingMode != 0 )
    	{
    		gfIsNewMailFlagSet = true;
    		Emails.fNewMailFlag = false;
    		gfRedrawScreen = true;

    		return(true);

    	}
    	return(false);
    }


    private static void DisplayAimMemberClickOnFaceHelpText()
    {
    	//display the 'left and right click' onscreen help msg
    	FontSubSystem.DrawTextToScreen(AimMemberText[0], new(AIM_FI_LEFT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_TITLE_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);			
    	FontSubSystem.DrawTextToScreen(AimMemberText[1], new(AIM_FI_LEFT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y+AIM_FI_CLICK_DESC_TEXT_Y_OFFSET), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);			

    	FontSubSystem.DrawTextToScreen(AimMemberText[2], new(AIM_FI_RIGHT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_TITLE_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);			
    	FontSubSystem.DrawTextToScreen(AimMemberText[3], new(AIM_FI_RIGHT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y+AIM_FI_CLICK_DESC_TEXT_Y_OFFSET), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);			
    }
}
