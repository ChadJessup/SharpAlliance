using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.EnglishText;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int giMessageId = -1;
    public static int giPrevMessageId = -1;
    public static int giMessagePage = -1;
    public static int giNumberOfPagesToCurrentEmail = -1;
    public static HVOBJECT guiEmailWarning;

    public const int EMAIL_TOP_BAR_HEIGHT = 22;

    public const int LIST_MIDDLE_COUNT = 18;
    // object positions
    // chad: TITLE_X and Y are duplicates with different values!
    public const int TITLE_X = 0 + LAPTOP_SCREEN_UL_X;
    public const int TITLE_Y = 0 + LAPTOP_SCREEN_UL_Y;

    // public const int TITLE_X = 140;
    // public const int TITLE_Y = 33;


    public const int STAMP_X = 344 + LAPTOP_SCREEN_UL_X;
    public const int STAMP_Y = 0 + LAPTOP_SCREEN_UL_Y;
    /*
    public const int TOP_X 0+LAPTOP_SCREEN_UL_X
    public const int TOP_Y 62+LAPTOP_SCREEN_UL_Y

    public const int BOTTOM_X 0+LAPTOP_SCREEN_UL_X
    public const int BOTTOM_Y 359+LAPTOP_SCREEN_UL_Y
    */
    public const int MIDDLE_X = 0 + LAPTOP_SCREEN_UL_X;
    public const int MIDDLE_Y = 72 + EMAIL_TOP_BAR_HEIGHT;
    public const int MIDDLE_WIDTH = 19;


    // new graphics
    public const int EMAIL_LIST_WINDOW_Y = 22;
    public const int EMAIL_TITLE_BAR_X = 5;

    // email columns
    public const int SENDER_X = LAPTOP_SCREEN_UL_X + 65;
    public const int SENDER_WIDTH = 246 - 158;

    public const int DATE_X = LAPTOP_SCREEN_UL_X + 428;
    public const int DATE_WIDTH = 592 - 527;

    public const int SUBJECT_X = LAPTOP_SCREEN_UL_X + 175;
    public const int SUBJECT_WIDTH = 254;//526-245 
    public const int INDIC_X = 128;
    public const int INDIC_WIDTH = 155 - 123;
    public const int INDIC_HEIGHT = 145 - 128;

    // chad: There are two LINE_WIDTHs! Rename when you find out which goes where
    public const int LINE_WIDTH = 592 - 121;
    //public const int LINE_WIDTH = 320;

    public const int MESSAGE_X = 5;//17
    public const int MESSAGE_Y = 35;
    public const int MESSAGE_WIDTH = 528 - 125;//150
    public const FontColor MESSAGE_COLOR = FontColor.FONT_BLACK;
    public const int MESSAGE_GAP = 2;

    public const int MESSAGE_HEADER_WIDTH = 209 - 151;
    public const int MESSAGE_HEADER_X = VIEWER_X + 4;


    public const int VIEWER_HEAD_X = 140;
    public const int VIEWER_HEAD_Y = 9;
    public const int VIEWER_HEAD_WIDTH = 445 - VIEWER_HEAD_X;
    public const int MAX_BUTTON_COUNT = 1;
    public const int VIEWER_WIDTH = 500;
    public const int VIEWER_HEIGHT = 195;

    public const int MESSAGEX_X = 425;
    public const int MESSAGEX_Y = 6;

    public const int EMAIL_WARNING_X = 210;
    public const int EMAIL_WARNING_Y = 140;
    public const int EMAIL_WARNING_WIDTH = 254;
    public const int EMAIL_WARNING_HEIGHT = 138;


    public const int NEW_BTN_X = EMAIL_WARNING_X + (338 - 245);
    public const int NEW_BTN_Y = EMAIL_WARNING_Y + (278 - 195);

    public const FontStyle EMAIL_TEXT_FONT = FontStyle.FONT10ARIAL;
    public const FontStyle TRAVERSE_EMAIL_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle EMAIL_BOX_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle MESSAGE_FONT = EMAIL_TEXT_FONT;
    public const FontStyle EMAIL_HEADER_FONT = FontStyle.FONT14ARIAL;
    public const FontStyle EMAIL_WARNING_FONT = FontStyle.FONT12ARIAL;


    // the max number of pages to an email
    public const int MAX_NUMBER_EMAIL_PAGES = 100;

    public const int PREVIOUS_PAGE = 0;
    public const int NEXT_PAGE = 1;

    public const int NEXT_PAGE_X = LAPTOP_UL_X + 562;
    public const int NEXT_PAGE_Y = 51;

    public const int PREVIOUS_PAGE_X = NEXT_PAGE_X - 21;
    public const int PREVIOUS_PAGE_Y = NEXT_PAGE_Y;

    public const int ENVELOPE_BOX_X = 116;

    public const int FROM_BOX_X = 166;
    public const int FROM_BOX_WIDTH = 246 - 160;

    public const int SUBJECT_BOX_X = 276;
    public const int SUBJECT_BOX_WIDTH = 528 - 249;

    public const int DATE_BOX_X = 530;
    public const int DATE_BOX_WIDTH = 594 - 530;

    public const int FROM_BOX_Y = 51 + EMAIL_TOP_BAR_HEIGHT;
    public const int TOP_HEIGHT = 118 - 95;

    public const FontStyle EMAIL_TITLE_FONT = FontStyle.FONT14ARIAL;
    public const int EMAIL_TITLE_X = 140;
    public const int EMAIL_TITLE_Y = 33;
    public const int VIEWER_MESSAGE_BODY_START_Y = VIEWER_Y + 72;
    public const int MIN_MESSAGE_HEIGHT_IN_LINES = 5;

    public const int INDENT_Y_OFFSET = 310;
    public const int INDENT_X_OFFSET = 325;
    public const int INDENT_X_WIDTH = 544 - 481;

    // the position of the page number being displayed in the email program
    public const int PAGE_NUMBER_X = 516;
    public const int PAGE_NUMBER_Y = 58;

    // chad: dupes again
    //public const int PAGE_NUMBER_X = TOP_X + 20;
    //public const int PAGE_NUMBER_Y = TOP_Y + 33;

    // defines for location of message 'title'/'headers'

    public const int CHECK_X = 15;
    public const int CHECK_Y = 13;
    public const int VIEWER_X = 155;
    public const int VIEWER_Y = 70 + 21;
    public const int MAIL_STRING_SIZE = 640;


    public const int MESSAGE_FROM_Y = VIEWER_Y + 28;
    public const int MESSAGE_DATE_Y = MESSAGE_FROM_Y;
    public const int MESSAGE_SUBJECT_Y = MESSAGE_DATE_Y + 16;


    public const int SUBJECT_LINE_X = VIEWER_X + 47;
    public const int SUBJECT_LINE_Y = VIEWER_Y + 42;
    public const int SUBJECT_LINE_WIDTH = 278 - 47;


    // maximum size of a email message page, so not to overrun the bottom of the screen
//    public static int MAX_EMAIL_MESSAGE_PAGE_SIZE => (FontSubSystem.GetFontHeight(MESSAGE_FONT) + MESSAGE_GAP) * 20;

    // X button position
    public const int BUTTON_X = VIEWER_X + 396;
    public const int BUTTON_Y = VIEWER_Y + 3; // was + 25
    public const int BUTTON_LOWER_Y = BUTTON_Y + 22;
    public const int PREVIOUS_PAGE_BUTTON_X = VIEWER_X + 302;
    public const int NEXT_PAGE_BUTTON_X = VIEWER_X + 395;
    public const int DELETE_BUTTON_X = NEXT_PAGE_BUTTON_X;
    public const int LOWER_BUTTON_Y = BUTTON_Y + 299;

    //buttons
    public static GUI_BUTTON[] giMessageButton = new GUI_BUTTON[MAX_BUTTON_COUNT];
    public static ButtonPic[] giMessageButtonImage = new ButtonPic[MAX_BUTTON_COUNT];
    public static GUI_BUTTON[] giDeleteMailButton = new GUI_BUTTON[2];
    public static ButtonPic[] giDeleteMailButtonImage = new ButtonPic[2];
    public static GUI_BUTTON[] giSortButton = new GUI_BUTTON[4];
    public static ButtonPic[] giSortButtonImage = new ButtonPic[4];
    public static GUI_BUTTON[] giNewMailButton = new GUI_BUTTON[1]
    {
        new GUI_BUTTON(),
    };
    public static ButtonPic[] giNewMailButtonImage = new ButtonPic[1];
    public static GUI_BUTTON[] giMailMessageButtons = new GUI_BUTTON[3];
    public static ButtonPic[] giMailMessageButtonsImage = new ButtonPic[3];
    public static GUI_BUTTON[] giMailPageButtons = new GUI_BUTTON[2];
    public static ButtonPic[] giMailPageButtonsImage = new ButtonPic[2];

    // video handles
    public static HVOBJECT guiEmailTitle;
    public static HVOBJECT guiEmailStamp;
    public static HVOBJECT guiEmailBackground;
    public static HVOBJECT guiEmailIndicator;
    public static HVOBJECT guiEmailMessage;
    public static HVOBJECT guiMAILDIVIDER;

    // position of header text on the email list
    public const int FROM_X = 205;
    public const int FROM_Y = FROM_BOX_Y + 5;
    public const int SUBJECTHEAD_X = 368;
    public const int RECD_X = 550;

    // size of prev/next strings
    public static int PREVIOUS_WIDTH => FontSubSystem.StringPixLength(pTraverseStrings[(int)EMAILTRAVERSALBUTTON.PREVIOUS_BUTTON], TRAVERSE_EMAIL_FONT);
    public static int NEXT_WIDTH => FontSubSystem.StringPixLength(pTraverseStrings[(int)EMAILTRAVERSALBUTTON.NEXT_BUTTON], TRAVERSE_EMAIL_FONT);
//    public static int PREVIOUS_HEIGHT => FontSubSystem.GetFontHeight(TRAVERSE_EMAIL_FONT);
//    public static int NEXT_HEIGHT => FontSubSystem.GetFontHeight(TRAVERSE_EMAIL_FONT);

    // defines
    public const int MAX_EMAIL_LINES = 10; //max number of lines can be shown in a message
    public const int MAX_MESSAGES_PAGE = 18; // max number of messages per page


    public const int IMP_EMAIL_INTRO = 0;
    public const int IMP_EMAIL_INTRO_LENGTH = 10;
    public const int ENRICO_CONGRATS = IMP_EMAIL_INTRO + IMP_EMAIL_INTRO_LENGTH;
    public const int ENRICO_CONGRATS_LENGTH = 3;
    public const int IMP_EMAIL_AGAIN = ENRICO_CONGRATS + ENRICO_CONGRATS_LENGTH;
    public const int IMP_EMAIL_AGAIN_LENGTH = 6;
    public const int MERC_INTRO = IMP_EMAIL_AGAIN + IMP_EMAIL_AGAIN_LENGTH;
    public const int MERC_INTRO_LENGTH = 5;
    public const int MERC_NEW_SITE_ADDRESS = MERC_INTRO + MERC_INTRO_LENGTH;
    public const int MERC_NEW_SITE_ADDRESS_LENGTH = 2;
    public const int AIM_MEDICAL_DEPOSIT_REFUND = MERC_NEW_SITE_ADDRESS + MERC_NEW_SITE_ADDRESS_LENGTH;
    public const int AIM_MEDICAL_DEPOSIT_REFUND_LENGTH = 3;
    public const int IMP_EMAIL_PROFILE_RESULTS = AIM_MEDICAL_DEPOSIT_REFUND + AIM_MEDICAL_DEPOSIT_REFUND_LENGTH;
    public const int IMP_EMAIL_PROFILE_RESULTS_LENGTH = 1;
    public const int MERC_WARNING = IMP_EMAIL_PROFILE_RESULTS_LENGTH + IMP_EMAIL_PROFILE_RESULTS;
    public const int MERC_WARNING_LENGTH = 2;
    public const int MERC_INVALID = MERC_WARNING + MERC_WARNING_LENGTH;
    public const int MERC_INVALID_LENGTH = 2;
    public const int NEW_MERCS_AT_MERC = MERC_INVALID + MERC_INVALID_LENGTH;
    public const int NEW_MERCS_AT_MERC_LENGTH = 2;
    public const int MERC_FIRST_WARNING = NEW_MERCS_AT_MERC + NEW_MERCS_AT_MERC_LENGTH;
    public const int MERC_FIRST_WARNING_LENGTH = 2;
    // merc up a level emails
    public const int MERC_UP_LEVEL_BIFF = MERC_FIRST_WARNING + MERC_FIRST_WARNING_LENGTH;
    public const int MERC_UP_LEVEL_LENGTH_BIFF = 2;
    public const int MERC_UP_LEVEL_HAYWIRE = MERC_UP_LEVEL_LENGTH_BIFF + MERC_UP_LEVEL_BIFF;
    public const int MERC_UP_LEVEL_LENGTH_HAYWIRE = 2;
    public const int MERC_UP_LEVEL_GASKET = MERC_UP_LEVEL_LENGTH_HAYWIRE + MERC_UP_LEVEL_HAYWIRE;
    public const int MERC_UP_LEVEL_LENGTH_GASKET = 2;
    public const int MERC_UP_LEVEL_RAZOR = MERC_UP_LEVEL_LENGTH_GASKET + MERC_UP_LEVEL_GASKET;
    public const int MERC_UP_LEVEL_LENGTH_RAZOR = 2;
    public const int MERC_UP_LEVEL_FLO = MERC_UP_LEVEL_LENGTH_RAZOR + MERC_UP_LEVEL_RAZOR;
    public const int MERC_UP_LEVEL_LENGTH_FLO = 2;
    public const int MERC_UP_LEVEL_GUMPY = MERC_UP_LEVEL_LENGTH_FLO + MERC_UP_LEVEL_FLO;
    public const int MERC_UP_LEVEL_LENGTH_GUMPY = 2;
    public const int MERC_UP_LEVEL_LARRY = MERC_UP_LEVEL_LENGTH_GUMPY + MERC_UP_LEVEL_GUMPY;
    public const int MERC_UP_LEVEL_LENGTH_LARRY = 2;
    public const int MERC_UP_LEVEL_COUGAR = MERC_UP_LEVEL_LENGTH_LARRY + MERC_UP_LEVEL_LARRY;
    public const int MERC_UP_LEVEL_LENGTH_COUGAR = 2;
    public const int MERC_UP_LEVEL_NUMB = MERC_UP_LEVEL_LENGTH_COUGAR + MERC_UP_LEVEL_COUGAR;
    public const int MERC_UP_LEVEL_LENGTH_NUMB = 2;
    public const int MERC_UP_LEVEL_BUBBA = MERC_UP_LEVEL_LENGTH_NUMB + MERC_UP_LEVEL_NUMB;
    public const int MERC_UP_LEVEL_LENGTH_BUBBA = 2;
    // merc left-me-a-message-and-now-I'm-back emails
    public const int AIM_REPLY_BARRY = MERC_UP_LEVEL_LENGTH_BUBBA + MERC_UP_LEVEL_BUBBA;
    public const int AIM_REPLY_LENGTH_BARRY = 2;
    public const int AIM_REPLY_MELTDOWN = AIM_REPLY_BARRY + (39 * AIM_REPLY_LENGTH_BARRY);
    public const int AIM_REPLY_LENGTH_MELTDOWN = AIM_REPLY_LENGTH_BARRY;

    // old EXISTING emails when player starts game. They must look "read"
    public const int OLD_ENRICO_1 = AIM_REPLY_LENGTH_MELTDOWN + AIM_REPLY_MELTDOWN;
    public const int OLD_ENRICO_1_LENGTH = 3;
    public const int OLD_ENRICO_2 = OLD_ENRICO_1 + OLD_ENRICO_1_LENGTH;
    public const int OLD_ENRICO_2_LENGTH = 3;
    public const int RIS_REPORT = OLD_ENRICO_2 + OLD_ENRICO_2_LENGTH;
    public const int RIS_REPORT_LENGTH = 2;
    public const int OLD_ENRICO_3 = RIS_REPORT + RIS_REPORT_LENGTH;
    public const int OLD_ENRICO_3_LENGTH = 3;

    // emails that occur from Enrico once player accomplishes things
    public const int ENRICO_MIGUEL = OLD_ENRICO_3 + OLD_ENRICO_3_LENGTH;
    public const int ENRICO_MIGUEL_LENGTH = 3;
    public const int ENRICO_PROG_20 = ENRICO_MIGUEL + ENRICO_MIGUEL_LENGTH;
    public const int ENRICO_PROG_20_LENGTH = 3;
    public const int ENRICO_PROG_55 = ENRICO_PROG_20 + ENRICO_PROG_20_LENGTH;
    public const int ENRICO_PROG_55_LENGTH = 3;
    public const int ENRICO_PROG_80 = ENRICO_PROG_55 + ENRICO_PROG_55_LENGTH;
    public const int ENRICO_PROG_80_LENGTH = 3;
    public const int ENRICO_SETBACK = ENRICO_PROG_80 + ENRICO_PROG_80_LENGTH;
    public const int ENRICO_SETBACK_LENGTH = 3;
    public const int ENRICO_SETBACK_2 = ENRICO_SETBACK + ENRICO_SETBACK_LENGTH;
    public const int ENRICO_SETBACK_2_LENGTH = 3;
    public const int ENRICO_CREATURES = ENRICO_SETBACK_2 + ENRICO_SETBACK_2_LENGTH;
    public const int ENRICO_CREATURES_LENGTH = 3;

    // insurance company emails
    public const int INSUR_PAYMENT = ENRICO_CREATURES + ENRICO_CREATURES_LENGTH;
    public const int INSUR_PAYMENT_LENGTH = 3;
    public const int INSUR_SUSPIC = INSUR_PAYMENT + INSUR_PAYMENT_LENGTH;
    public const int INSUR_SUSPIC_LENGTH = 3;
    public const int INSUR_INVEST_OVER = INSUR_SUSPIC + INSUR_SUSPIC_LENGTH;
    public const int INSUR_INVEST_OVER_LENGTH = 3;
    public const int INSUR_SUSPIC_2 = INSUR_INVEST_OVER + INSUR_INVEST_OVER_LENGTH;
    public const int INSUR_SUSPIC_2_LENGTH = 3;

    public const int BOBBYR_NOW_OPEN = INSUR_SUSPIC_2 + INSUR_SUSPIC_2_LENGTH;
    public const int BOBBYR_NOW_OPEN_LENGTH = 3;

    public const int KING_PIN_LETTER = BOBBYR_NOW_OPEN + BOBBYR_NOW_OPEN_LENGTH;
    public const int KING_PIN_LETTER_LENGTH = 4;

    public const int LACK_PLAYER_PROGRESS_1 = KING_PIN_LETTER + KING_PIN_LETTER_LENGTH;
    public const int LACK_PLAYER_PROGRESS_1_LENGTH = 3;

    public const int LACK_PLAYER_PROGRESS_2 = LACK_PLAYER_PROGRESS_1 + LACK_PLAYER_PROGRESS_1_LENGTH;
    public const int LACK_PLAYER_PROGRESS_2_LENGTH = 3;

    public const int LACK_PLAYER_PROGRESS_3 = LACK_PLAYER_PROGRESS_2 + LACK_PLAYER_PROGRESS_2_LENGTH;
    public const int LACK_PLAYER_PROGRESS_3_LENGTH = 3;

    //A package from bobby r has arrived in Drassen
    public const int BOBBYR_SHIPMENT_ARRIVED = LACK_PLAYER_PROGRESS_3 + LACK_PLAYER_PROGRESS_3_LENGTH;
    public const int BOBBYR_SHIPMENT_ARRIVED_LENGTH = 4;

    // John Kulba has left the gifts for theplayers in drassen
    public const int JOHN_KULBA_GIFT_IN_DRASSEN = BOBBYR_SHIPMENT_ARRIVED + BOBBYR_SHIPMENT_ARRIVED_LENGTH;
    public const int JOHN_KULBA_GIFT_IN_DRASSEN_LENGTH = 4;

    //when a merc dies on ANOTHER assignment ( ie not with the player )
    public const int MERC_DIED_ON_OTHER_ASSIGNMENT = JOHN_KULBA_GIFT_IN_DRASSEN + JOHN_KULBA_GIFT_IN_DRASSEN_LENGTH;
    public const int MERC_DIED_ON_OTHER_ASSIGNMENT_LENGTH = 5;

    public const int INSUR_1HOUR_FRAUD = MERC_DIED_ON_OTHER_ASSIGNMENT + MERC_DIED_ON_OTHER_ASSIGNMENT_LENGTH;
    public const int INSUR_1HOUR_FRAUD_LENGTH = 3;

    //when a merc is fired, and is injured
    public const int AIM_MEDICAL_DEPOSIT_PARTIAL_REFUND = INSUR_1HOUR_FRAUD + INSUR_1HOUR_FRAUD_LENGTH;
    public const int AIM_MEDICAL_DEPOSIT_PARTIAL_REFUND_LENGTH = 3;

    //when a merc is fired, and is dead
    public const int AIM_MEDICAL_DEPOSIT_NO_REFUND = AIM_MEDICAL_DEPOSIT_PARTIAL_REFUND + AIM_MEDICAL_DEPOSIT_PARTIAL_REFUND_LENGTH;
    public const int AIM_MEDICAL_DEPOSIT_NO_REFUND_LENGTH = 3;

    public const int BOBBY_R_MEDUNA_SHIPMENT = AIM_MEDICAL_DEPOSIT_NO_REFUND + AIM_MEDICAL_DEPOSIT_NO_REFUND_LENGTH;
    public const int BOBBY_R_MEDUNA_SHIPMENT_LENGTH = 4;

    public const int IMP_RESULTS_INTRO_LENGTH = 9;
    public const int IMP_RESULTS_PERSONALITY_INTRO = IMP_RESULTS_INTRO_LENGTH;
    public const int IMP_RESULTS_PERSONALITY_INTRO_LENGTH = 5;
    public const int IMP_PERSONALITY_NORMAL = IMP_RESULTS_PERSONALITY_INTRO + IMP_RESULTS_PERSONALITY_INTRO_LENGTH;
    public const int IMP_PERSONALITY_LENGTH = 4;
    public const int IMP_PERSONALITY_HEAT = IMP_PERSONALITY_NORMAL + IMP_PERSONALITY_LENGTH;
    public const int IMP_PERSONALITY_NERVOUS = IMP_PERSONALITY_HEAT + IMP_PERSONALITY_LENGTH;
    public const int IMP_PERSONALITY_CLAUSTROPHOBIC = IMP_PERSONALITY_NERVOUS + IMP_PERSONALITY_LENGTH;
    public const int IMP_PERSONALITY_NONSWIMMER = IMP_PERSONALITY_CLAUSTROPHOBIC + IMP_PERSONALITY_LENGTH;
    public const int IMP_PERSONALITY_FEAR_OF_INSECTS = IMP_PERSONALITY_NONSWIMMER + IMP_PERSONALITY_LENGTH;
    public const int IMP_PERSONALITY_FORGETFUL = IMP_PERSONALITY_FEAR_OF_INSECTS + IMP_PERSONALITY_LENGTH + 1;
    public const int IMP_PERSONALITY_PSYCHO = IMP_PERSONALITY_FORGETFUL + IMP_PERSONALITY_LENGTH;
    public const int IMP_RESULTS_ATTITUDE_INTRO = IMP_PERSONALITY_PSYCHO + IMP_PERSONALITY_LENGTH + 1;
    public const int IMP_RESULTS_ATTITUDE_LENGTH = 5;
    public const int IMP_ATTITUDE_LENGTH = 5;
    public const int IMP_ATTITUDE_NORMAL = IMP_RESULTS_ATTITUDE_INTRO + IMP_RESULTS_ATTITUDE_LENGTH;
    public const int IMP_ATTITUDE_FRIENDLY = IMP_ATTITUDE_NORMAL + IMP_ATTITUDE_LENGTH;
    public const int IMP_ATTITUDE_LONER = IMP_ATTITUDE_FRIENDLY + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_OPTIMIST = IMP_ATTITUDE_LONER + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_PESSIMIST = IMP_ATTITUDE_OPTIMIST + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_AGGRESSIVE = IMP_ATTITUDE_PESSIMIST + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_ARROGANT = IMP_ATTITUDE_AGGRESSIVE + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_ASSHOLE = IMP_ATTITUDE_ARROGANT + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_ATTITUDE_COWARD = IMP_ATTITUDE_ASSHOLE + IMP_ATTITUDE_LENGTH;
    public const int IMP_RESULTS_SKILLS = IMP_ATTITUDE_COWARD + IMP_ATTITUDE_LENGTH + 1;
    public const int IMP_RESULTS_SKILLS_LENGTH = 7;
    public const int IMP_SKILLS_IMPERIAL_SKILLS = IMP_RESULTS_SKILLS + IMP_RESULTS_SKILLS_LENGTH + 1;
    public const int IMP_SKILLS_IMPERIAL_MARK = IMP_SKILLS_IMPERIAL_SKILLS + 1;
    public const int IMP_SKILLS_IMPERIAL_MECH = IMP_SKILLS_IMPERIAL_SKILLS + 2;
    public const int IMP_SKILLS_IMPERIAL_EXPL = IMP_SKILLS_IMPERIAL_SKILLS + 3;
    public const int IMP_SKILLS_IMPERIAL_MED = IMP_SKILLS_IMPERIAL_SKILLS + 4;
    public const int IMP_SKILLS_NEED_TRAIN_SKILLS = IMP_SKILLS_IMPERIAL_MED + 1;
    public const int IMP_SKILLS_NEED_TRAIN_MARK = IMP_SKILLS_NEED_TRAIN_SKILLS + 1;
    public const int IMP_SKILLS_NEED_TRAIN_MECH = IMP_SKILLS_NEED_TRAIN_SKILLS + 2;
    public const int IMP_SKILLS_NEED_TRAIN_EXPL = IMP_SKILLS_NEED_TRAIN_SKILLS + 3;
    public const int IMP_SKILLS_NEED_TRAIN_MED = IMP_SKILLS_NEED_TRAIN_SKILLS + 4;
    public const int IMP_SKILLS_NO_SKILL = IMP_SKILLS_NEED_TRAIN_MED + 1;
    public const int IMP_SKILLS_NO_SKILL_MARK = IMP_SKILLS_NO_SKILL + 1;
    public const int IMP_SKILLS_NO_SKILL_MECH = IMP_SKILLS_NO_SKILL + 2;
    public const int IMP_SKILLS_NO_SKILL_EXPL = IMP_SKILLS_NO_SKILL + 3;
    public const int IMP_SKILLS_NO_SKILL_MED = IMP_SKILLS_NO_SKILL + 4;
    public const int IMP_SKILLS_SPECIAL_INTRO = IMP_SKILLS_NO_SKILL_MED + 1;
    public const int IMP_SKILLS_SPECIAL_INTRO_LENGTH = 2;
    public const int IMP_SKILLS_SPECIAL_LOCK = IMP_SKILLS_SPECIAL_INTRO + IMP_SKILLS_SPECIAL_INTRO_LENGTH;
    public const int IMP_SKILLS_SPECIAL_HAND = IMP_SKILLS_SPECIAL_LOCK + 1;
    public const int IMP_SKILLS_SPECIAL_ELEC = IMP_SKILLS_SPECIAL_HAND + 1;
    public const int IMP_SKILLS_SPECIAL_NIGHT = IMP_SKILLS_SPECIAL_ELEC + 1;
    public const int IMP_SKILLS_SPECIAL_THROW = IMP_SKILLS_SPECIAL_NIGHT + 1;
    public const int IMP_SKILLS_SPECIAL_TEACH = IMP_SKILLS_SPECIAL_THROW + 1;
    public const int IMP_SKILLS_SPECIAL_HEAVY = IMP_SKILLS_SPECIAL_TEACH + 1;
    public const int IMP_SKILLS_SPECIAL_AUTO = IMP_SKILLS_SPECIAL_HEAVY + 1;
    public const int IMP_SKILLS_SPECIAL_STEALTH = IMP_SKILLS_SPECIAL_AUTO + 1;
    public const int IMP_SKILLS_SPECIAL_AMBI = IMP_SKILLS_SPECIAL_STEALTH + 1;
    public const int IMP_SKILLS_SPECIAL_THIEF = IMP_SKILLS_SPECIAL_AMBI + 1;
    public const int IMP_SKILLS_SPECIAL_MARTIAL = IMP_SKILLS_SPECIAL_THIEF + 1;
    public const int IMP_SKILLS_SPECIAL_KNIFE = IMP_SKILLS_SPECIAL_MARTIAL + 1;
    public const int IMP_RESULTS_PHYSICAL = IMP_SKILLS_SPECIAL_KNIFE + 1;
    public const int IMP_RESULTS_PHYSICAL_LENGTH = 7;
    public const int IMP_PHYSICAL_SUPER = IMP_RESULTS_PHYSICAL + IMP_RESULTS_PHYSICAL_LENGTH;
    public const int IMP_PHYSICAL_SUPER_LENGTH = 1;
    public const int IMP_PHYSICAL_SUPER_HEALTH = IMP_PHYSICAL_SUPER + IMP_PHYSICAL_SUPER_LENGTH;
    public const int IMP_PHYSICAL_SUPER_AGILITY = IMP_PHYSICAL_SUPER_HEALTH + 1;
    public const int IMP_PHYSICAL_SUPER_DEXTERITY = IMP_PHYSICAL_SUPER_AGILITY + 1;
    public const int IMP_PHYSICAL_SUPER_STRENGTH = IMP_PHYSICAL_SUPER_DEXTERITY + 1;
    public const int IMP_PHYSICAL_SUPER_LEADERSHIP = IMP_PHYSICAL_SUPER_STRENGTH + 1;
    public const int IMP_PHYSICAL_SUPER_WISDOM = IMP_PHYSICAL_SUPER_LEADERSHIP + 1;
    public const int IMP_PHYSICAL_LOW = IMP_PHYSICAL_SUPER_WISDOM + 1;
    public const int IMP_PHYSICAL_LOW_LENGTH = 1;
    public const int IMP_PHYSICAL_LOW_HEALTH = IMP_PHYSICAL_LOW + IMP_PHYSICAL_LOW_LENGTH;
    public const int IMP_PHYSICAL_LOW_AGILITY = IMP_PHYSICAL_LOW_HEALTH + 1;
    public const int IMP_PHYSICAL_LOW_DEXTERITY = IMP_PHYSICAL_LOW_AGILITY + 2;
    public const int IMP_PHYSICAL_LOW_STRENGTH = IMP_PHYSICAL_LOW_DEXTERITY + 1;
    public const int IMP_PHYSICAL_LOW_LEADERSHIP = IMP_PHYSICAL_LOW_STRENGTH + 1;
    public const int IMP_PHYSICAL_LOW_WISDOM = IMP_PHYSICAL_LOW_LEADERSHIP + 1;
    public const int IMP_PHYSICAL_VERY_LOW = IMP_PHYSICAL_LOW_WISDOM + 1;
    public const int IMP_PHYSICAL_VERY_LOW_LENGTH = 1;
    public const int IMP_PHYSICAL_VERY_LOW_HEALTH = IMP_PHYSICAL_VERY_LOW + IMP_PHYSICAL_VERY_LOW_LENGTH;
    public const int IMP_PHYSICAL_VERY_LOW_AGILITY = IMP_PHYSICAL_VERY_LOW_HEALTH + 1;
    public const int IMP_PHYSICAL_VERY_LOW_DEXTERITY = IMP_PHYSICAL_VERY_LOW_AGILITY + 1;
    public const int IMP_PHYSICAL_VERY_LOW_STRENGTH = IMP_PHYSICAL_VERY_LOW_DEXTERITY + 1;
    public const int IMP_PHYSICAL_VERY_LOW_LEADERSHIP = IMP_PHYSICAL_VERY_LOW_STRENGTH + 1;
    public const int IMP_PHYSICAL_VERY_LOW_WISDOM = IMP_PHYSICAL_VERY_LOW_LEADERSHIP + 1;
    public const int IMP_PHYSICAL_END = IMP_PHYSICAL_VERY_LOW_WISDOM + 1;
    public const int IMP_PHYSICAL_END_LENGTH = 3;
    public const int IMP_RESULTS_PORTRAIT = IMP_PHYSICAL_END + IMP_PHYSICAL_END_LENGTH;
    public const int IMP_RESULTS_PORTRAIT_LENGTH = 6;
    public const int IMP_PORTRAIT_MALE_1 = IMP_RESULTS_PORTRAIT + IMP_RESULTS_PORTRAIT_LENGTH;
    public const int IMP_PORTRAIT_MALE_2 = IMP_PORTRAIT_MALE_1 + 4;
    public const int IMP_PORTRAIT_MALE_3 = IMP_PORTRAIT_MALE_2 + 4;
    public const int IMP_PORTRAIT_MALE_4 = IMP_PORTRAIT_MALE_3 + 4;
    public const int IMP_PORTRAIT_MALE_5 = IMP_PORTRAIT_MALE_4 + 4;
    public const int IMP_PORTRAIT_MALE_6 = IMP_PORTRAIT_MALE_5 + 4;
    public const int IMP_PORTRAIT_FEMALE_1 = IMP_PORTRAIT_MALE_6 + 4;
    public const int IMP_PORTRAIT_FEMALE_2 = IMP_PORTRAIT_FEMALE_1 + 4;
    public const int IMP_PORTRAIT_FEMALE_3 = IMP_PORTRAIT_FEMALE_2 + 4;
    public const int IMP_PORTRAIT_FEMALE_4 = IMP_PORTRAIT_FEMALE_3 + 4;
    public const int IMP_PORTRAIT_FEMALE_5 = IMP_PORTRAIT_FEMALE_4 + 4;
    public const int IMP_PORTRAIT_FEMALE_6 = IMP_PORTRAIT_FEMALE_5 + 4;
    public const int IMP_RESULTS_END = IMP_PORTRAIT_FEMALE_6 + 1;
    public const int IMP_RESULTS_END_LENGTH = 3;
}
