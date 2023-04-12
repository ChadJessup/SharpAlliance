using System;
using System.Collections.Generic;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MERCPROFILESTRUCT
{
    public string zName;
    public string zNickname;
    public int uiAttnSound;
    public int uiCurseSound;
    public int uiDieSound;
    public int uiGoodSound;
    public int uiGruntSound;
    public int uiGrunt2Sound;
    public int uiOkSound;
    public int ubFaceIndex;
    public PaletteRepID PANTS;
    public PaletteRepID VEST;
    public PaletteRepID SKIN;
    public PaletteRepID HAIR;
    public Sexes bSex;
    public int bArmourAttractiveness;
    public CharacterEvolution bEvolution;
    public ProfileMiscFlags1 ubMiscFlags;
    public ProfileMiscFlags2 ubMiscFlags2;
    public SexistLevels bSexist;
    public int bLearnToHate;

    // skills
    public int bStealRate;
    public int bVocalVolume;
    public int ubQuoteRecord;
    public int bDeathRate;
    public int bScientific;

    public int sExpLevelGain;
    public int sLifeGain;
    public int sAgilityGain;
    public int sDexterityGain;
    public int sWisdomGain;
    public int sMarksmanshipGain;
    public int sMedicalGain;
    public int sMechanicGain;
    public int sExplosivesGain;

    public SoldierBodyTypes ubBodyType;
    public int bMedical;

    public int usEyesX;
    public int usEyesY;
    public int usMouthX;
    public int usMouthY;
    public int uiEyeDelay;
    public int uiMouthDelay;
    public int uiBlinkFrequency;
    public int uiExpressionFrequency;
    public int sSectorX;
    public MAP_ROW sSectorY;

    public int uiDayBecomesAvailable;           //day the merc will be available.  used with the bMercStatus

    public uint bStrength;

    public int bLifeMax;
    public int bExpLevelDelta;
    public int bLifeDelta;
    public int bAgilityDelta;
    public int bDexterityDelta;
    public int bWisdomDelta;
    public int bMarksmanshipDelta;
    public int bMedicalDelta;
    public int bMechanicDelta;
    public int bExplosivesDelta;
    public int bStrengthDelta;
    public int bLeadershipDelta;
    public int usKills;
    public int usAssists;
    public int usShotsFired;
    public int usShotsHit;
    public int usBattlesFought;
    public int usTimesWounded;
    public int usTotalDaysServed;

    public int sLeadershipGain;
    public int sStrengthGain;

    // BODY TYPE SUBSITUTIONS
    public int uiBodyTypeSubFlags;

    public int sSalary;
    public int bLife;
    public int bDexterity;        // dexterity (hand coord) value
    public PersonalityTrait bPersonalityTrait;
    public SkillTrait bSkillTrait;

    public int bReputationTolerance;
    public int bExplosive;
    public SkillTrait bSkillTrait2;
    public int bLeadership;

    public MERCPROFILESTRUCT[] bBuddy = new MERCPROFILESTRUCT[5];
    public MERCPROFILESTRUCT[] bHated = new MERCPROFILESTRUCT[5];
    public int bExpLevel;     // general experience level

    public int bMarksmanship;
    public int bMinService;
    public int bWisdom;
    public int bResigned;
    public int bActive;

    public int[] bInvStatus = new int[19];
    public int[] bInvNumber = new int[19];
    public int[] usApproachFactor = new int[4];

    public int bMainGunAttractiveness;
    public int bAgility;          // agility (speed) value

    public bool fUseProfileInsertionInfo;               // Set to various flags, ( contained in TacticalSave.h )
    public int sGridNo;                                              // The Gridno the NPC was in before leaving the sector
    public int ubQuoteActionID;
    public int bMechanical;

    public int ubInvUndroppable;
    public int[] ubRoomRangeStart = new int[2];
    public Items[] inv = new Items[19];
    public int[] bMercTownReputation = new int[20];

    public Dictionary<Stat, int> usStatChangeChances = new();     // used strictly for balancing, never shown!
    public Dictionary<Stat, int> usStatChangeSuccesses = new();   // used strictly for balancing, never shown!

    public int ubStrategicInsertionCode;

    public int[] ubRoomRangeEnd = new int[2];

    public int[] bPadding = new int[4];

    public int ubLastQuoteSaid;

    public int bRace;
    public int bNationality;
    public int bAppearance;
    public int bAppearanceCareLevel;
    public int bRefinement;
    public int bRefinementCareLevel;
    public int bHatedNationality;
    public int bHatedNationalityCareLevel;
    public int bRacist;
    public int uiWeeklySalary;
    public int uiBiWeeklySalary;
    public int bMedicalDeposit;
    public ATT bAttitude;
    public int bBaseMorale;
    public int sMedicalDepositAmount;

    public int bLearnToLike;
    public int[] ubApproachVal = new int[4];
    public int[,] ubApproachMod = new int[3, 4];
    public TOWNS bTown;
    public int bTownAttachment;
    public int usOptionalGearCost;
    public Dictionary<NPCID, int> bMercOpinion = new();
    public int bApproached;

    //The status of the merc.  If negative, see flags at the top of this file.  Positive:  The number of days the merc is away for.  0:  Not hired but ready to be.
    public MercStatus bMercStatus { get; set; }
    public int[] bHatedTime = new int[5];
    public int bLearnToLikeTime;
    public int bLearnToHateTime;
    public int[] bHatedCount = new int[5];
    public int bLearnToLikeCount;
    public int bLearnToHateCount;
    public int ubLastDateSpokenTo;
    public int bLastQuoteSaidWasSpecial;
    public int bSectorZ;
    public int usStrategicInsertionData;
    public int bFriendlyOrDirectDefaultResponseUsedRecently;
    public int bRecruitDefaultResponseUsedRecently;
    public int bThreatenDefaultResponseUsedRecently;
    public int bNPCData;          // NPC specific
    public int iBalance;
    public int sTrueSalary; // for use when the person is working for us for free but has a positive salary value
    public int ubCivilianGroup;
    public int ubNeedForSleep;
    public int uiMoney;
    public int bNPCData2;     // NPC specific

    public ProfileMiscFlags3 ubMiscFlags3;

    public int ubDaysOfMoraleHangover;       // used only when merc leaves team while having poor morale
    public int ubNumTimesDrugUseInLifetime;      // The # times a drug has been used in the player's lifetime...

    // Flags used for the precedent to repeating oneself in Contract negotiations.  Used for quote 80 -  ~107.  Gets reset every day
    public int uiPrecedentQuoteSaid;
    public int uiProfileChecksum;
    public int sPreCombatGridNo;
    public int ubTimeTillNextHatedComplaint;
    public int ubSuspiciousDeath;

    public int iMercMercContractLength;      //Used for MERC mercs, specifies how many days the merc has gone since last page

    public int uiTotalCostToDate;           // The total amount of money that has been paid to the merc for their salary
    public int[] ubBuffer = new int[4];
}

//ONLY HAVE 8 MISC FLAGS.. SHOULD BE ENOUGH
[Flags]
public enum ProfileMiscFlags1
{
    PROFILE_MISC_FLAG_RECRUITED = 0x01,
    PROFILE_MISC_FLAG_HAVESEENCREATURE = 0x02,
    PROFILE_MISC_FLAG_FORCENPCQUOTE = 0x04,
    PROFILE_MISC_FLAG_WOUNDEDBYPLAYER = 0x08,
    PROFILE_MISC_FLAG_TEMP_NPC_QUOTE_DATA_EXISTS = 0x10,
    PROFILE_MISC_FLAG_SAID_HOSTILE_QUOTE = 0x20,
    PROFILE_MISC_FLAG_EPCACTIVE = 0x40,
    //The player has already purchased the mercs items.
    PROFILE_MISC_FLAG_ALREADY_USED_ITEMS = 0x80,
};

[Flags]
public enum ProfileMiscFlags2
{
    PROFILE_MISC_FLAG2_DONT_ADD_TO_SECTOR = 0x01,
    PROFILE_MISC_FLAG2_LEFT_COUNTRY = 0x02,
    PROFILE_MISC_FLAG2_BANDAGED_TODAY = 0x04,
    PROFILE_MISC_FLAG2_SAID_FIRSTSEEN_QUOTE = 0x08,
    PROFILE_MISC_FLAG2_NEEDS_TO_SAY_HOSTILE_QUOTE = 0x10,
    PROFILE_MISC_FLAG2_MARRIED_TO_HICKS = 0x20,
    PROFILE_MISC_FLAG2_ASKED_BY_HICKS = 0x40,
};


[Flags]
public enum ProfileMiscFlags3
{
    // In the aimscreen, the merc was away and the player left a message
    PROFILE_MISC_FLAG3_PLAYER_LEFT_MSG.FOR_MERC_AT_AIM = 0x01,
    PROFILE_MISC_FLAG3_PERMANENT_INSERTION_CODE = 0x02,

    // player's had a chance to hire this merc
    PROFILE_MISC_FLAG3_PLAYER_HAD_CHANCE_TO_HIRE = 0x04,
    PROFILE_MISC_FLAG3_HANDLE_DONE_TRAVERSAL = 0x08,

    PROFILE_MISC_FLAG3_NPC_PISSED_OFF = 0x10,

    // In the merc site, the merc has died and Speck quote for the dead merc has been said
    PROFILE_MISC_FLAG3_MERC_MERC_IS_DEAD_AND_QUOTE_SAID = 0x20,

    PROFILE_MISC_FLAG3_TOWN_DOESNT_CARE_ABOUT_DEATH = 0x40,
    PROFILE_MISC_FLAG3_GOODGUY = 0x80,
};
