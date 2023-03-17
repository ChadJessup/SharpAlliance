using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class DialogControl
{
    private readonly SoldierProfileSubSystem soldiers;
    private readonly IFileManager fileManager;
    private readonly QuestEngine quests;
    private readonly InterfaceDialogSubSystem interfaceDialog;
    private readonly Faces faces;

    public DialogControl(
        IFileManager fileManager,
        SoldierProfileSubSystem soldiers,
        QuestEngine questEngine,
        InterfaceDialogSubSystem interfaceDialogSubSystem,
        Faces faces)
    {
        this.soldiers = soldiers;
        this.fileManager = fileManager;
        this.quests = questEngine;
        this.interfaceDialog = interfaceDialogSubSystem;
        this.faces = faces;

        // no better place..heh?.. will load faces for profiles that are 'extern'.....won't have soldiertype instances
        this.InitalizeStaticExternalNPCFaces();
    }


    private bool fExternFacesLoaded = false;
    private bool gfUseAlternateDialogueFile = false;

    public int[] uiExternalStaticNPCFaces = new int[(int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES];

    public int[] uiExternalFaceProfileIds = new int[(int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES]
    {
        97,
        106,
        148,
        156,
        157,
        158,
    };

    public static void ShutDownLastQuoteTacticalTextBox()
    {
        if (Globals.fDialogueBoxDueToLastMessage)
        {
            RenderDirty.RemoveVideoOverlay(Globals.giTextBoxOverlay);
            Globals.giTextBoxOverlay = -1;

            if (Globals.fTextBoxMouseRegionCreated)
            {
                MouseSubSystem.MSYS_RemoveRegion(Globals.gTextBoxMouseRegion);
                Globals.fTextBoxMouseRegionCreated = false;
            }

            Globals.fDialogueBoxDueToLastMessage = false;
        }
    }

    // Used to see if the dialog text file exists
    public bool DialogueDataFileExistsForProfile(NPCID ubCharacterNum, int usQuoteNum, bool fWavFile, out string ppStr)
    {
        string pFilename = this.GetDialogueDataFilename(ubCharacterNum, usQuoteNum, fWavFile);

        ppStr = pFilename;

        return FileManager.FileExists(pFilename);
    }

    private string GetDialogueDataFilename(NPCID ubCharacterNum, int usQuoteNum, bool fWavFile)
    {
        string zFileName = string.Empty;
        int ubFileNumID;

        // Are we an NPC OR an RPC that has not been recruited?
        // ATE: Did the || clause here to allow ANY RPC that talks while the talking menu is up to use an npc quote file
        if (this.gfUseAlternateDialogueFile)
        {
            if (fWavFile)
            {
                // build name of wav file (characternum + quotenum)
                // sprintf(zFileName, "NPC_SPEECH\\d_%03d_%03d.wav", ubCharacterNum, usQuoteNum);
            }
            else
            {
                // assume EDT files are in EDT directory on HARD DRIVE
                // sprintf(zFileName, "NPCDATA\\d_%03d.EDT", ubCharacterNum);
            }
        }
        else if (ubCharacterNum >= Globals.FIRST_RPC &&
                (!Globals.gMercProfiles[ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)
                || this.interfaceDialog.ProfileCurrentlyTalkingInDialoguePanel(ubCharacterNum)
                || Globals.gMercProfiles[ubCharacterNum].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_FORCENPCQUOTE))
                )
        {
            ubFileNumID = (int)ubCharacterNum;

            // ATE: If we are merc profile ID #151-154, all use 151's data....
            if ((int)ubCharacterNum >= 151 && (int)ubCharacterNum <= 154)
            {
                ubFileNumID = 151;
            }

            // If we are character #155, check fact!
            if ((int)ubCharacterNum == 155 && Globals.gubFact[(FACT)220] == 0)
            {
                ubFileNumID = 155;
            }


            if (fWavFile)
            {
                // sprintf(zFileName, "NPC_SPEECH\\%03d_%03d.wav", ubFileNumID, usQuoteNum);
            }
            else
            {
                // assume EDT files are in EDT directory on HARD DRIVE
                // sprintf(zFileName, "NPCDATA\\%03d.EDT", ubFileNumID);
            }
        }
        else
        {
            if (fWavFile)
            {
                // build name of wav file (characternum + quotenum)
                // sprintf(zFileName, "SPEECH\\%03d_%03d.wav", ubCharacterNum, usQuoteNum);
            }
            else
            {
                // assume EDT files are in EDT directory on HARD DRIVE
                // sprintf(zFileName, "MERCEDT\\%03d.EDT", ubCharacterNum);
            }
        }

        return zFileName;
    }

    public void EmptyDialogueQueue()
    {
    }

    public void InitalizeStaticExternalNPCFaces()
    {
        int iCounter = 0;
        // go and grab all external NPC faces that are needed for the game who won't exist as soldiertypes

        if (this.fExternFacesLoaded == true)
        {
            return;
        }

        this.fExternFacesLoaded = true;

        for (iCounter = 0; iCounter < (int)ExternalFaces.NUMBER_OF_EXTERNAL_NPC_FACES; iCounter++)
        {
            this.uiExternalStaticNPCFaces[iCounter] = (int)this.faces.InitFace(this.uiExternalFaceProfileIds[iCounter], Globals.NOBODY, FaceFlags.FACE_FORCE_SMALL);
        }

        return;
    }

    public ValueTask<bool> InitalizeDialogueControl()
    {
        return ValueTask.FromResult(true);
    }
}

public enum ExternalFaces
{
    SKYRIDER_EXTERNAL_FACE = 0,
    MINER_FRED_EXTERNAL_FACE,
    MINER_MATT_EXTERNAL_FACE,
    MINER_OSWALD_EXTERNAL_FACE,
    MINER_CALVIN_EXTERNAL_FACE,
    MINER_CARL_EXTERNAL_FACE,
    NUMBER_OF_EXTERNAL_NPC_FACES,
};


[Flags]
public enum DIALOGUE_SPECIAL_EVENT : uint
{
    GIVE_ITEM = 0x00000001,
    TRIGGER_NPC = 0x00000002,
    GOTO_GRIDNO = 0x00000004,
    DO_ACTION = 0x00000008,
    CLOSE_PANEL = 0x00000010,
    PCTRIGGERNPC = 0x00000020,
    BEGINPREBATTLEINTERFACE = 0x00000040,
    SKYRIDERMAPSCREENEVENT = 0x00000080,
    SHOW_CONTRACT_MENU = 0x00000100,
    MINESECTOREVENT = 0x00000200,
    SHOW_UPDATE_MENU = 0x00000400,
    ENABLE_AI = 0x00000800,
    USE_ALTERNATE_FILES = 0x00001000,
    CONTINUE_TRAINING_MILITIA = 0x00002000,
    CONTRACT_ENDING = 0x00004000,
    MULTIPURPOSE = 0x00008000,
    SLEEP = 0x00010000,
    DO_BATTLE_SND = 0x00020000,
    SIGNAL_ITEM_LOCATOR_START = 0x00040000,
    SHOPKEEPER = 0x00080000,
    SKIP_A_FRAME = 0x00100000,
    EXIT_MAP_SCREEN = 0x00200000,
    DISPLAY_STAT_CHANGE = 0x00400000,
    UNSET_ARRIVES_FLAG = 0x00800000,
    TRIGGERPREBATTLEINTERFACE = 0x01000000,
    DIALOGUE_ADD_EVENT_FOR_SOLDIER_UPDATE_BOX = 0x02000000,
    ENTER_MAPSCREEN = 0x04000000,
    LOCK_INTERFACE = 0x08000000,
    REMOVE_EPC = 0x10000000,
    CONTRACT_WANTS_TO_RENEW = 0x20000000,
    CONTRACT_NOGO_TO_RENEW = 0x40000000,
    CONTRACT_ENDING_NO_ASK_EQUIP = 0x80000000,
}

public enum MULTIPURPOSE_SPECIAL_EVENT_
{
    DONE_KILLING_DEIDRANNA = 0x00000001,
    TEAM_MEMBERS_DONE_TALKING = 0x00000002,
}

// soldier up-date box reasons
public enum UPDATE_BOX_REASON
{
    ADDSOLDIER = 0,
    SET_REASON,
    SHOW_BOX,
}

// An enumeration for dialog quotes
public enum QUOTE
{
    // 0
    SEE_ENEMY = 0,
    SEE_ENEMY_VARIATION,
    IN_TROUBLE_SLASH_IN_BATTLE,
    SEE_CREATURE,
    FIRSTTIME_GAME_SEE_CREATURE,
    TRACES_OF_CREATURE_ATTACK,
    HEARD_SOMETHING,
    SMELLED_CREATURE,
    WEARY_SLASH_SUSPUCIOUS,
    WORRIED_ABOUT_CREATURE_PRESENCE,

    //10
    ATTACKED_BY_MULTIPLE_CREATURES,
    SPOTTED_SOMETHING_ONE,
    SPOTTED_SOMETHING_TWO,
    OUT_OF_AMMO,
    SERIOUSLY_WOUNDED,
    BUDDY_ONE_KILLED,
    BUDDY_TWO_KILLED,
    LEARNED_TO_LIKE_MERC_KILLED,
    FORGETFULL_SLASH_CONFUSED,
    JAMMED_GUN,

    //20
    UNDER_HEAVY_FIRE,
    TAKEN_A_BREATING,
    CLOSE_CALL,
    NO_LINE_OF_FIRE,
    STARTING_TO_BLEED,
    NEED_SLEEP,
    OUT_OF_BREATH,
    KILLED_AN_ENEMY,
    KILLED_A_CREATURE,
    HATED_MERC_ONE,

    //30
    HATED_MERC_TWO,
    LEARNED_TO_HATE_MERC,
    AIM_KILLED_MIKE,
    MERC_QUIT_LEARN_TO_HATE = AIM_KILLED_MIKE,
    HEADSHOT,
    PERSONALITY_TRAIT,
    ASSIGNMENT_COMPLETE,
    REFUSING_ORDER,
    KILLING_DEIDRANNA,
    KILLING_QUEEN,
    ANNOYING_PC,

    //40
    STARTING_TO_WHINE,
    NEGATIVE_COMPANY,
    AIR_RAID,
    WHINE_EQUIPMENT,
    SOCIAL_TRAIT,
    PASSING_DISLIKE,
    EXPERIENCE_GAIN,
    PRE_NOT_SMART,
    POST_NOT_SMART,
    HATED_1_ARRIVES,
    MERC_QUIT_HATED1 = HATED_1_ARRIVES,

    //50
    HATED_2_ARRIVES,
    MERC_QUIT_HATED2 = HATED_2_ARRIVES,
    BUDDY_1_GOOD,
    BUDDY_2_GOOD,
    LEARNED_TO_LIKE_WITNESSED,
    DELAY_CONTRACT_RENEWAL,
    NOT_GETTING_PAID = DELAY_CONTRACT_RENEWAL,
    AIM_SEEN_MIKE,
    PC_DROPPED_OMERTA = AIM_SEEN_MIKE,
    BLINDED,
    DEFINITE_CANT_DO,
    LISTEN_LIKABLE_PERSON,
    ENEMY_PRESENCE,

    //60
    WARNING_OUTSTANDING_ENEMY_AFTER_RT,
    FOUND_SOMETHING_SPECIAL,
    SATISFACTION_WITH_GUN_AFTER_KILL,
    SPOTTED_JOEY,
    RESPONSE_TO_MIGUEL_SLASH_MERC_OR_RPC_LETGO,
    SECTOR_SAFE,
    STUFF_MISSING_DRASSEN,
    KILLED_FACTORY_MANAGER,
    SPOTTED_BLOODCAT,
    END_GAME_COMMENT,

    //70
    ENEMY_RETREATED,
    GOING_TO_AUTO_SLEEP,
    WORK_UP_AND_RETURNING_TO_ASSIGNMENT, // woke up from auto sleep, going back to wo
    ME_TOO, // me too quote, in agreement with whatever the merc previous said
    USELESS_ITEM,
    BOOBYTRAP_ITEM,
    SUSPICIOUS_GROUND,
    DROWNING,
    MERC_REACHED_DESTINATION,
    SPARE2,

    //80
    REPUTATION_REFUSAL,
    DEATH_RATE_REFUSAL, //= 99,
    LAME_REFUSAL, //= 82,
    WONT_RENEW_CONTRACT_LAME_REFUSAL,         // ARM: now unused
    ANSWERING_MACHINE_MSG,
    DEPARTING_COMMENT_CONTRACT_NOT_RENEWED_OR_48_OR_MORE,
    HATE_MERC_1_ON_TEAM,// = 100,
    HATE_MERC_2_ON_TEAM,// = 101,
    LEARNED_TO_HATE_MERC_ON_TEAM,// = 102,
    CONTRACTS_OVER,// = 89,

    //90
    ACCEPT_CONTRACT_RENEWAL,
    CONTRACT_ACCEPTANCE,
    JOINING_CAUSE_BUDDY_1_ON_TEAM,// = 103,
    JOINING_CAUSE_BUDDY_2_ON_TEAM,// = 104,
    JOINING_CAUSE_LEARNED_TO_LIKE_BUDDY_ON_TEAM,// = 105,
    REFUSAL_RENEW_DUE_TO_MORALE,// = 95,
    PRECEDENT_TO_REPEATING_ONESELF,// = 106,
    REFUSAL_TO_JOIN_LACK_OF_FUNDS,// = 107,
    DEPART_COMMET_CONTRACT_NOT_RENEWED_OR_TERMINATED_UNDER_48,// = 98,
    DEATH_RATE_RENEWAL,

    //100
    HATE_MERC_1_ON_TEAM_WONT_RENEW,
    HATE_MERC_2_ON_TEAM_WONT_RENEW,
    LEARNED_TO_HATE_MERC_1_ON_TEAM_WONT_RENEW,
    RENEWING_CAUSE_BUDDY_1_ON_TEAM,
    RENEWING_CAUSE_BUDDY_2_ON_TEAM,
    RENEWING_CAUSE_LEARNED_TO_LIKE_BUDDY_ON_TEAM,
    PRECEDENT_TO_REPEATING_ONESELF_RENEW,
    RENEW_REFUSAL_DUE_TO_LACK_OF_FUNDS,
    GREETING,
    SMALL_TALK,

    //110
    IMPATIENT_QUOTE,
    LENGTH_OF_CONTRACT,
    COMMENT_BEFORE_HANG_UP,
    PERSONALITY_BIAS_WITH_MERC_1,
    PERSONALITY_BIAS_WITH_MERC_2,
    MERC_LEAVING_ALSUCO_SOON,
    MERC_GONE_UP_IN_PRICE,
}
