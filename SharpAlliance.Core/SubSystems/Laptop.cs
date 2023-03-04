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
