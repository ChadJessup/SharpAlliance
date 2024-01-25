using System;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public class Mercs
{
    private const int NUMBER_OF_MERCS = 11;
    private const FontColor MERC_BUTTON_UP_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontColor MERC_BUTTON_DOWN_COLOR = FontColor.FONT_MCOLOR_DKWHITE;
    private const int LAST_MERC_ID = 10;
    private const int NUMBER_OF_BAD_MERCS = 5;
    private const int NUMBER_MERCS_AFTER_FIRST_MERC_ARRIVES = 6;
    private const int NUMBER_MERCS_AFTER_SECOND_MERC_ARRIVES = 8;
    private const int NUMBER_MERCS_AFTER_THIRD_MERC_ARRIVES = 9;
    private const int NUMBER_MERCS_AFTER_FOURTH_MERC_ARRIVES = 10;
    private const int MERC_NUM_DAYS_TILL_FIRST_WARNING = 7;
    private const int MERC_NUM_DAYS_TILL_ACCOUNT_SUSPENDED = 9;
    private const int MERC_NUM_DAYS_TILL_ACCOUNT_INVALID = 12;
    private const int MERC_LARRY_ROACHBURN = 7;
    private const int DAYS_TIL_M_E_R_C_AVAIL = 3;

    public static NPCID[] gubMercArray = new NPCID[NUMBER_OF_MERCS];
    public static int gubCurMercIndex = 0;
    internal static int iMercPopUpBox;
    internal static GUI_BUTTON guiAccountBoxButton;

    public static void GameInitMercs()
    {
        //	for(i=0; i<NUMBER_OF_MERCS; i++)
        //		gubMercArray[ i ] = i+BIFF;

        //can now be out of order
        gubMercArray[0] = NPCID.BIFF;
        gubMercArray[1] = NPCID.HAYWIRE;
        gubMercArray[2] = NPCID.GASKET;
        gubMercArray[3] = NPCID.RAZOR;
        gubMercArray[4] = NPCID.FLO;
        gubMercArray[5] = NPCID.GUMPY;
        gubMercArray[6] = NPCID.BUBBA;
        gubMercArray[7] = NPCID.LARRY_NORMAL;     //if changing this values, change in GetMercIDFromMERCArray()
        gubMercArray[8] = NPCID.LARRY_DRUNK;      //if changing this values, change in GetMercIDFromMERCArray()
        gubMercArray[9] = NPCID.NUMB;
        gubMercArray[10] = NPCID.COUGAR;

        LaptopSaveInfo.gubPlayersMercAccountStatus = LAPTOP_MODE.MERC_NO_ACCOUNT;
        gubCurMercIndex = 0;
        LaptopSaveInfo.gubLastMercIndex = NUMBER_OF_BAD_MERCS;

        gubCurrentMercVideoMode = MERC_VIDEO.NO_VIDEO_MODE;
        gfMercVideoIsBeingDisplayed = false;

        LaptopSaveInfo.guiNumberOfMercPaymentsInDays = 0;

        gusMercVideoSpeckSpeech = 0;
    }

    internal static void EnterInitMercSite()
    {
    }

    internal static void ExitMercs()
    {
    }

    internal static void ExitMercsAccount()
    {
    }

    internal static void ExitMercsFiles()
    {
    }

    internal static void ExitMercsNoAccount()
    {
    }

    internal static void HandleMercs()
    {
        throw new NotImplementedException();
    }

    internal static void HandleMercsAccount()
    {
        throw new NotImplementedException();
    }

    internal static void HandleMercsFiles()
    {
        throw new NotImplementedException();
    }

    internal static void HandleMercsNoAccount()
    {
        throw new NotImplementedException();
    }

    internal static void InitializeNumDaysMercArrive()
    {
    }
}

//used with the gubArrivedFromMercSubSite variable to signify whcih page the player came from
public enum MERC_CAME_FROM
{
    OTHER_PAGE,
    ACCOUNTS_PAGE,
    HIRE_PAGE,
};

//Merc Video Conferencing Mode
public enum MERC_VIDEO
{
    NO_VIDEO_MODE,
    INIT_VIDEO_MODE,
    VIDEO_MODE,
    EXIT_VIDEO_MODE,
};
