using System;

using static SharpAlliance.Core.Globals;


namespace SharpAlliance.Core.SubSystems;

public class Laptop
{
    internal void LaptopScreenInit()
    {
        throw new NotImplementedException();
    }

    internal void InitLaptopAndLaptopScreens()
    {
        throw new NotImplementedException();
    }

    public static bool DrawLapTopIcons()
    {
        return (true);
    }

    public static void LapTopScreenCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {

            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            HandleLeftButtonUpEvent();
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            HandleRightButtonUpEvent();
            return;
        }

        return;
    }

    public static void HandleLeftButtonUpEvent()
    {

        // will handle the left button up event

        if (gfShowBookmarks)
        {
            // get rid of bookmarks
            gfShowBookmarks = false;

            // force redraw
            fReDrawScreenFlag = true;
            RenderLapTopImage();
            RenderLaptop();
        }
        else if (fShowBookmarkInfo)
        {
            fShowBookmarkInfo = false;
        }
    }

    public static void RenderLaptop()
    {
        LAPTOP_MODE uiTempMode = 0;

        if ((fMaximizingProgram == true) || (fMinizingProgram == true))
        {
            gfShowBookmarks = false;
            return;
        }

        if (fLoadPendingFlag)
        {
            uiTempMode = guiCurrentLaptopMode;
            guiCurrentLaptopMode = guiPreviousLaptopMode;
        }

        switch (guiCurrentLaptopMode)
        {
            case (LAPTOP_MODE.NONE):
                DrawDeskTopBackground();
                break;
            case LAPTOP_MODE.AIM:
                RenderAIM();
                break;
            case LAPTOP_MODE.AIM_MEMBERS:
                RenderAIMMembers();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX:
                RenderAimFacialIndex();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES:
                RenderAimSort();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_ARCHIVES:
                RenderAimArchives();
                break;
            case LAPTOP_MODE.AIM_POLICIES:
                RenderAimPolicies();
                break;
            case LAPTOP_MODE.AIM_LINKS:
                RenderAimLinks();
                break;
            case LAPTOP_MODE.AIM_HISTORY:
                RenderAimHistory();
                break;
            case LAPTOP_MODE.MERC:
                RenderMercs();
                break;
            case LAPTOP_MODE.MERC_FILES:
                RenderMercsFiles();
                break;
            case LAPTOP_MODE.MERC_ACCOUNT:
                RenderMercsAccount();
                break;
            case LAPTOP_MODE.MERC_NO_ACCOUNT:
                RenderMercsNoAccount();
                break;

            case LAPTOP_MODE.BOBBY_R:
                RenderBobbyR();
                break;

            case LAPTOP_MODE.BOBBY_R_GUNS:
                RenderBobbyRGuns();
                break;
            case LAPTOP_MODE.BOBBY_R_AMMO:
                RenderBobbyRAmmo();
                break;
            case LAPTOP_MODE.BOBBY_R_ARMOR:
                RenderBobbyRArmour();
                break;
            case LAPTOP_MODE.BOBBY_R_MISC:
                RenderBobbyRMisc();
                break;
            case LAPTOP_MODE.BOBBY_R_USED:
                RenderBobbyRUsed();
                break;
            case LAPTOP_MODE.BOBBY_R_MAILORDER:
                RenderBobbyRMailOrder();
                break;
            case LAPTOP_MODE.CHAR_PROFILE:
                RenderCharProfile();
                break;
            case LAPTOP_MODE.FLORIST:
                RenderFlorist();
                break;
            case LAPTOP_MODE.FLORIST_FLOWER_GALLERY:
                RenderFloristGallery();
                break;
            case LAPTOP_MODE.FLORIST_ORDERFORM:
                RenderFloristOrderForm();
                break;
            case LAPTOP_MODE.FLORIST_CARD_GALLERY:
                RenderFloristCards();
                break;

            case LAPTOP_MODE.INSURANCE:
                RenderInsurance();
                break;

            case LAPTOP_MODE.INSURANCE_INFO:
                RenderInsuranceInfo();
                break;

            case LAPTOP_MODE.INSURANCE_CONTRACT:
                RenderInsuranceContract();
                break;

            case LAPTOP_MODE.INSURANCE_COMMENTS:
                RenderInsuranceComments();
                break;

            case LAPTOP_MODE.FUNERAL:
                RenderFuneral();
                break;
            case LAPTOP_MODE.SIRTECH:
                RenderSirTech();
                break;
            case LAPTOP_MODE.FINANCES:
                RenderFinances();
                break;
            case LAPTOP_MODE.PERSONNEL:
                RenderPersonnel();
                break;
            case LAPTOP_MODE.HISTORY:
                RenderHistory();
                break;
            case LAPTOP_MODE.FILES:
                RenderFiles();
                break;
            case LAPTOP_MODE.EMAIL:
                RenderEmail();
                break;
            case (LAPTOP_MODE.WWW):
                DrawDeskTopBackground();
                RenderWWWProgramTitleBar();
                break;
            case (LAPTOP_MODE.BROKEN_LINK):
                RenderBrokenLink();
                break;

            case LAPTOP_MODE.BOBBYR_SHIPMENTS:
                RenderBobbyRShipments();
                break;
        }

        if (guiCurrentLaptopMode >= LAPTOP_MODE.WWW)
        {
            // render program bar for www program
            RenderWWWProgramTitleBar();
        }



        if (fLoadPendingFlag)
        {
            guiCurrentLaptopMode = uiTempMode;
            return;
        }

        DisplayProgramBoundingBox(FALSE);

        // mark the buttons dirty at this point
        MarkButtonsDirty();
    }
}

// icons text id's
public enum LaptopIcons
{
    MAIL = 0,
    WWW,
    FINANCIAL,
    PERSONNEL,
    HISTORY,
    FILES,
    MAX_ICON_COUNT,
};

public enum LaptopRegions
{
    NO_REGION = 0,
    EMAIL_REGION,
    WWW_REGION,
    FINANCIAL_REGION,
    PERSONNEL_REGION,
    HISTORY_REGION,
    FILES_REGION,
};

// laptop programs
public enum LAPTOP_PROGRAM
{
    MAILER,
    WEB_BROWSER,
    FILES,
    PERSONNEL,
    FINANCES,
    HISTORY,

};

// laptop program states
public enum LAPTOP_PROGRAM_STATES
{
    MINIMIZED,
    OPEN,
};

public enum LAPTOP_MODE
{
    NONE = 0,
    FINANCES,
    PERSONNEL,
    HISTORY,
    FILES,
    FILES_ENRICO,
    FILES_PLANS,
    EMAIL,
    EMAIL_NEW,
    EMAIL_VIEW,
    WWW,
    AIM,
    AIM_MEMBERS,
    AIM_MEMBERS_FACIAL_INDEX,
    AIM_MEMBERS_SORTED_FILES,
    AIM_MEMBERS_SORTED_FILES_VIDEO,
    AIM_MEMBERS_ARCHIVES,
    AIM_POLICIES,
    AIM_HISTORY,
    AIM_LINKS,
    MERC,
    MERC_ACCOUNT,
    MERC_NO_ACCOUNT,
    MERC_FILES,
    BOBBY_R,
    BOBBY_R_GUNS,
    BOBBY_R_AMMO,
    BOBBY_R_ARMOR,
    BOBBY_R_MISC,
    BOBBY_R_USED,
    BOBBY_R_MAILORDER,
    CHAR_PROFILE,
    CHAR_PROFILE_QUESTIONAIRE,
    FLORIST,
    FLORIST_FLOWER_GALLERY,
    FLORIST_ORDERFORM,
    FLORIST_CARD_GALLERY,
    INSURANCE,
    INSURANCE_INFO,
    INSURANCE_CONTRACT,
    INSURANCE_COMMENTS,
    FUNERAL,
    SIRTECH,
    BROKEN_LINK,
    BOBBYR_SHIPMENTS,
};

// bookamrks for WWW bookmark list
public enum LaptopPanel
{
    FIRST_SIDE_PANEL = 1,
    SECOND_SIDE_PANEL,
};

public enum LAPTOP
{
    NO_CURSOR = 0,
    PANEL_CURSOR,
    SCREEN_CURSOR,
    WWW_CURSOR,
};
