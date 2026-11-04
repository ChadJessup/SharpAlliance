namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Text and string indices used by AimMembers.c (from Text.h and _EnglishText.c).
public partial class Laptop
{
    private const int AIM_MEMBER_CLICK_INSTRUCTIONS = 0;
    private const int AIM_MEMBER_HEALTH = 0;
    private const int AIM_MEMBER_AGILITY = 1;
    private const int AIM_MEMBER_DEXTERITY = 2;
    private const int AIM_MEMBER_STRENGTH = 3;
    private const int AIM_MEMBER_LEADERSHIP = 4;
    private const int AIM_MEMBER_WISDOM = 5;
    private const int AIM_MEMBER_EXP_LEVEL = 6;
    private const int AIM_MEMBER_MARKSMANSHIP = 7;
    private const int AIM_MEMBER_MECHANICAL = 8;
    private const int AIM_MEMBER_EXPLOSIVE = 9;
    private const int AIM_MEMBER_MEDICAL = 10;
    private const int AIM_MEMBER_FEE = 11;
    private const int AIM_MEMBER_CONTRACT = 12;
    private const int AIM_MEMBER_1_DAY = 13;
    private const int AIM_MEMBER_1_WEEK = 14;
    private const int AIM_MEMBER_2_WEEKS = 15;
    private const int AIM_MEMBER_PREVIOUS = 16;
    private const int AIM_MEMBER_CONTACT = 17;
    private const int AIM_MEMBER_NEXT = 18;
    private const int AIM_MEMBER_ADDTNL_INFO = 19;
    private const int AIM_MEMBER_ACTIVE_MEMBERS = 20;
    private const int AIM_MEMBER_OPTIONAL_GEAR = 21;
    private const int AIM_MEMBER_MEDICAL_DEPOSIT_REQ = 22;
    private const int AIM_MEMBER_CONTRACT_CHARGE = 0;
    private const int AIM_MEMBER_ONE_DAY = 1;
    private const int AIM_MEMBER_ONE_WEEK = 2;
    private const int AIM_MEMBER_TWO_WEEKS = 3;
    private const int AIM_MEMBER_NO_EQUIPMENT = 4;
    private const int AIM_MEMBER_BUY_EQUIPMENT = 5;
    private const int AIM_MEMBER_TRANSFER_FUNDS = 6;
    private const int AIM_MEMBER_CANCEL = 7;
    private const int AIM_MEMBER_HIRE = 8;
    private const int AIM_MEMBER_HANG_UP = 9;
    private const int AIM_MEMBER_OK = 10;
    private const int AIM_MEMBER_LEAVE_MESSAGE = 11;
    private const int AIM_MEMBER_VIDEO_CONF_WITH = 12;
    private const int AIM_MEMBER_CONNECTING = 13;
    private const int AIM_MEMBER_WITH_MEDICAL = 14;
    private const int AIM_MEMBER_FUNDS_TRANSFER_SUCCESFUL = 0;
    private const int AIM_MEMBER_FUNDS_TRANSFER_FAILED = 1;
    private const int AIM_MEMBER_NOT_ENOUGH_FUNDS = 2;
    private const int AIM_MEMBER_ON_ASSIGNMENT = 3;
    private const int AIM_MEMBER_LEAVE_MSG = 4;
    private const int AIM_MEMBER_DEAD = 5;
    private const int AIM_MEMBER_ALREADY_HAVE_20_MERCS = 6;
    private const int AIM_MEMBER_PRERECORDED_MESSAGE = 7;
    private const int AIM_MEMBER_MESSAGE_RECORDED = 8;

    public static readonly string[] AimMemberText =
    {
        "Left Click",
        "to Contact Merc.",
        "Right Click",
        "for Mug Shot Index.",
    };

    public static readonly string[] CharacterInfo =
    {
        "Health",
        "Agility",
        "Dexterity",
        "Strength",
        "Leadership",
        "Wisdom",
        "Experience Lvl",
        "Marksmanship",
        "Mechanical",
        "Explosive",
        "Medical",
        "Fee",
        "Contract",
        "one day",
        "one week",
        "two weeks",
        "Previous",
        "Contact",
        "Next",
        "Additional Info",
        "Active Members",
        "Optional Gear:",
        "MEDICAL deposit required",
    };

    public static readonly string[] VideoConfercingText =
    {
        "Contract Charge:",
        "One Day",
        "One Week",
        "Two Weeks",
        "No Equipment",
        "Buy Equipment",
        "TRANSFER FUNDS",
        "CANCEL",
        "HIRE",
        "HANG UP",
        "OK",
        "LEAVE MESSAGE",
        "Video Conferencing with",
        "Connecting. . .",
        "with medical",
    };

    public static readonly string[] AimPopUpText =
    {
        "ELECTRONIC FUNDS TRANSFER SUCCESSFUL",
        "UNABLE TO PROCESS TRANSFER",
        "INSUFFICIENT FUNDS",
        "On Assignment",
        "Please Leave Message",
        "Deceased",
        "You have a full team of 18 mercs already.",
        "Pre-recorded message",
        "Message recorded",
    };
}
