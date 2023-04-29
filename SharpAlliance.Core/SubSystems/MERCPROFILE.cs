using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MERCPROFILESTRUCT
{
    public const int Size = 716;
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
    public string PANTS;
    public PaletteRepID VEST;
    public PaletteRepID SKIN;
    public PaletteRepID HAIR;
    public Sexes bSex;
    public int bArmourAttractiveness;
    public CharacterEvolution bEvolution;
    public PROFILE_MISC_FLAG ubMiscFlags;
    public PROFILE_MISC_FLAG2 ubMiscFlags2;
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

    public int bStrength;

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

    public sbyte[] bBuddyIndexes = new sbyte[5];
    public MERCPROFILESTRUCT[] bBuddy = new MERCPROFILESTRUCT[5];

    public sbyte[] bHatedIndexes = new sbyte[5];
    public MERCPROFILESTRUCT[] bHated = new MERCPROFILESTRUCT[5];
    public int bExpLevel;     // general experience level

    public int bMarksmanship;
    public int bMinService;
    public int bWisdom;
    public int bResigned;
    public int bActive;

    public byte[] bInvStatus = new byte[19];
    public byte[] bInvNumber = new byte[19];
    public ushort[] usApproachFactor = new ushort[4];

    public int bMainGunAttractiveness;
    public int bAgility;          // agility (speed) value

    public int fUseProfileInsertionInfo;               // Set to various flags, ( contained in TacticalSave.h )
    public int sGridNo;                                              // The Gridno the NPC was in before leaving the sector
    public int ubQuoteActionID;
    public int bMechanical;

    public int ubInvUndroppable;
    public byte[] ubRoomRangeStart = new byte[2];
    public ushort[] invIndexes = new ushort[19];
    public Dictionary<InventorySlot, Items> inv = new();// Items[19];
    public sbyte[] bMercTownReputation = new sbyte[20];

    public ushort[] usStatChangeChancesIndexes = new ushort[12];     // used strictly for balancing, never shown!
    public ushort[] usStatChangeSuccessesIndexes = new ushort[12];   // used strictly for balancing, never shown!

    public Dictionary<Stat, int> usStatChangeChances = new();     // used strictly for balancing, never shown!
    public Dictionary<Stat, int> usStatChangeSuccesses = new();   // used strictly for balancing, never shown!

    public int ubStrategicInsertionCode;

    public byte[] ubRoomRangeEnd = new byte[2];

    public byte[] bPadding = new byte[4];

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
    public byte[] ubApproachVal = new byte[4];
    public byte[][] ubApproachMod = new byte[3][];
    public TOWNS bTown;
    public int bTownAttachment;
    public int usOptionalGearCost;

    public sbyte[] bMercOpinionIndexes = new sbyte[75];
    public Dictionary<NPCID, int> bMercOpinion = new();
    public int bApproached;

    //The status of the merc.  If negative, see flags at the top of this file.  Positive:  The number of days the merc is away for.  0:  Not hired but ready to be.
    public MercStatus bMercStatus { get; set; }
    public sbyte[] bHatedTime = new sbyte[5];
    public int bLearnToLikeTime;
    public int bLearnToHateTime;
    public sbyte[] bHatedCount = new sbyte[5];
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

    public PROFILE_MISC_FLAG3 ubMiscFlags3;

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
    public byte[] ubBuffer = new byte[4];

    public static MERCPROFILESTRUCT LoadFromBytes(ReadOnlySpan<byte> mercProfileBytes)
    {
        var profile = new MERCPROFILESTRUCT();
        int index = 0;

        var bytes = mercProfileBytes.ToArray();

        profile.zName = MemoryMarshal.Cast<byte, char>(mercProfileBytes[..60]).ToString().TrimEnd('\0');
        profile.zNickname = MemoryMarshal.Cast<byte, char>(mercProfileBytes[60..80]).ToString().TrimEnd('\0');

        profile.uiAttnSound = MemoryMarshal.Read<int>(mercProfileBytes[80..]);
        profile.uiCurseSound = MemoryMarshal.Read<int>(mercProfileBytes[84..]);
        profile.uiDieSound = MemoryMarshal.Read<int>(mercProfileBytes[88..]);
        profile.uiGoodSound = MemoryMarshal.Read<int>(mercProfileBytes[92..]);
        profile.uiGruntSound = MemoryMarshal.Read<int>(mercProfileBytes[96..]);
        profile.uiGrunt2Sound = MemoryMarshal.Read<int>(mercProfileBytes[100..]);
        profile.uiOkSound = MemoryMarshal.Read<int>(mercProfileBytes[104..]);
        profile.ubFaceIndex = MemoryMarshal.Read<byte>(mercProfileBytes[108..]);

        profile.PANTS = Encoding.ASCII.GetString(mercProfileBytes[109..139]).TrimEnd('\0');
        profile.VEST = Encoding.ASCII.GetString(mercProfileBytes[139..169]).TrimEnd('\0');
        profile.SKIN = Encoding.ASCII.GetString(mercProfileBytes[169..199]).TrimEnd('\0');
        profile.HAIR = Encoding.ASCII.GetString(mercProfileBytes[199..229]).TrimEnd('\0');

        profile.bSex = (Sexes)MemoryMarshal.Read<sbyte>(mercProfileBytes[229..]);
        profile.bArmourAttractiveness = MemoryMarshal.Read<sbyte>(mercProfileBytes[230..]);
        profile.ubMiscFlags2 = (PROFILE_MISC_FLAG2)MemoryMarshal.Read<byte>(mercProfileBytes[231..]);
        profile.bEvolution = (CharacterEvolution)MemoryMarshal.Read<sbyte>(mercProfileBytes[232..]);
        profile.ubMiscFlags = (PROFILE_MISC_FLAG)MemoryMarshal.Read<sbyte>(mercProfileBytes[233..]);
        profile.bSexist = (SexistLevels)MemoryMarshal.Read<byte>(mercProfileBytes[234..]);
        profile.bLearnToHate = MemoryMarshal.Read<sbyte>(mercProfileBytes[235..]);

        profile.bStealRate = MemoryMarshal.Read<sbyte>(mercProfileBytes[236..]);
        profile.bVocalVolume = MemoryMarshal.Read<sbyte>(mercProfileBytes[237..]);
        profile.ubQuoteRecord = MemoryMarshal.Read<byte>(mercProfileBytes[238..]);
        profile.bDeathRate = MemoryMarshal.Read<sbyte>(mercProfileBytes[239..]);
        profile.bScientific = MemoryMarshal.Read<sbyte>(mercProfileBytes[240..]);

        profile.sExpLevelGain = MemoryMarshal.Read<short>(mercProfileBytes[241..]);
        profile.sLifeGain = MemoryMarshal.Read<short>(mercProfileBytes[243..]);
        profile.sAgilityGain = MemoryMarshal.Read<short>(mercProfileBytes[245..]);
        profile.sDexterityGain = MemoryMarshal.Read<short>(mercProfileBytes[247..]);
        profile.sWisdomGain = MemoryMarshal.Read<short>(mercProfileBytes[249..]);
        profile.sMarksmanshipGain = MemoryMarshal.Read<short>(mercProfileBytes[251..]);
        profile.sMedicalGain = MemoryMarshal.Read<short>(mercProfileBytes[253..]);
        profile.sMechanicGain = MemoryMarshal.Read<short>(mercProfileBytes[255..]);
        profile.sExplosivesGain = MemoryMarshal.Read<short>(mercProfileBytes[257..]);

        profile.ubBodyType = (SoldierBodyTypes)MemoryMarshal.Read<byte>(mercProfileBytes[259..]);
        profile.bMedical = MemoryMarshal.Read<sbyte>(mercProfileBytes[261..]);

        profile.usEyesX = MemoryMarshal.Read<ushort>(mercProfileBytes[262..]);
        profile.usEyesY = MemoryMarshal.Read<ushort>(mercProfileBytes[264..]);
        profile.usMouthX = MemoryMarshal.Read<ushort>(mercProfileBytes[266..]);
        profile.usMouthY = MemoryMarshal.Read<ushort>(mercProfileBytes[268..]);
        profile.uiEyeDelay = MemoryMarshal.Read<int>(mercProfileBytes[272..]);
        profile.uiMouthDelay = MemoryMarshal.Read<int>(mercProfileBytes[276..]);
        profile.uiBlinkFrequency = MemoryMarshal.Read<int>(mercProfileBytes[280..]);
        profile.uiExpressionFrequency = MemoryMarshal.Read<int>(mercProfileBytes[284..]);
        profile.sSectorX = MemoryMarshal.Read<ushort>(mercProfileBytes[287..]);
        profile.sSectorY = (MAP_ROW)MemoryMarshal.Read<ushort>(mercProfileBytes[289..]);

        profile.uiDayBecomesAvailable = MemoryMarshal.Read<int>(mercProfileBytes[291..]);           //day the merc will be available.  used with the bMercStatus

        profile.bStrength = MemoryMarshal.Read<sbyte>(mercProfileBytes[296..]);

        profile.bLifeMax = MemoryMarshal.Read<sbyte>(mercProfileBytes[297..]);
        profile.bExpLevelDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[298..]);
        profile.bLifeDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[299..]);
        profile.bAgilityDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[300..]);
        profile.bDexterityDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[301..]);
        profile.bWisdomDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[302..]);
        profile.bMarksmanshipDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[303..]);
        profile.bMedicalDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[304..]);
        profile.bMechanicDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[305..]);
        profile.bExplosivesDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[306..]);
        profile.bStrengthDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[307..]);
        profile.bLeadershipDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[308..]);
        profile.usKills = MemoryMarshal.Read<ushort>(mercProfileBytes[309..]);
        profile.usAssists = MemoryMarshal.Read<ushort>(mercProfileBytes[311..]);
        profile.usShotsFired = MemoryMarshal.Read<ushort>(mercProfileBytes[313..]);
        profile.usShotsHit = MemoryMarshal.Read<ushort>(mercProfileBytes[315..]);
        profile.usBattlesFought = MemoryMarshal.Read<ushort>(mercProfileBytes[317..]);
        profile.usTimesWounded = MemoryMarshal.Read<ushort>(mercProfileBytes[319..]);
        profile.usTotalDaysServed = MemoryMarshal.Read<ushort>(mercProfileBytes[321..]);

        profile.sLeadershipGain = MemoryMarshal.Read<short>(mercProfileBytes[323..]);
        profile.sStrengthGain = MemoryMarshal.Read<short>(mercProfileBytes[325..]);



        // BODY TYPE SUBSITUTIONS
        profile.uiBodyTypeSubFlags = MemoryMarshal.Read<int>(mercProfileBytes[327..]);

        profile.sSalary = MemoryMarshal.Read<short>(mercProfileBytes[332..]);

        profile.bLife = MemoryMarshal.Read<sbyte>(mercProfileBytes[334..]);
        profile.bDexterity = MemoryMarshal.Read<sbyte>(mercProfileBytes[335..]);        // dexterity (hand coord) value
        profile.bPersonalityTrait = (PersonalityTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[336..]);
        profile.bSkillTrait = (SkillTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[337..]);
        profile.bReputationTolerance = MemoryMarshal.Read<sbyte>(mercProfileBytes[338..]);
        profile.bExplosive = MemoryMarshal.Read<sbyte>(mercProfileBytes[339..]);
        profile.bSkillTrait2 = (SkillTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[340..]);
        profile.bLeadership = MemoryMarshal.Read<sbyte>(mercProfileBytes[341..]);
        profile.bBuddyIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[342..347]).ToArray();
        profile.bHatedIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[347..352]).ToArray();
        profile.bExpLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[352..]);     // general experience level
        profile.bMarksmanship = MemoryMarshal.Read<sbyte>(mercProfileBytes[353..]);

        profile.bMinService = MemoryMarshal.Read<byte>(mercProfileBytes[354..]);
        profile.bWisdom = MemoryMarshal.Read<sbyte>(mercProfileBytes[355..]);
        profile.bResigned = MemoryMarshal.Read<byte>(mercProfileBytes[356..]);
        profile.bActive = MemoryMarshal.Read<byte>(mercProfileBytes[357..]);

        profile.bInvStatus = mercProfileBytes[358..377].ToArray();
        profile.bInvNumber = mercProfileBytes[377..396].ToArray();
        profile.usApproachFactor = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[396..404]).ToArray();

        profile.bMainGunAttractiveness = MemoryMarshal.Read<sbyte>(mercProfileBytes[404..]);
        profile.bAgility = MemoryMarshal.Read<sbyte>(mercProfileBytes[405..]);          // agility (speed) value

        profile.fUseProfileInsertionInfo = MemoryMarshal.Read<byte>(mercProfileBytes[406..]); // Set to various flags, ( contained in TacticalSave.h )
        profile.sGridNo = MemoryMarshal.Read<short>(mercProfileBytes[407..]);                                              // The Gridno the NPC was in before leaving the sector
        profile.ubQuoteActionID = MemoryMarshal.Read<byte>(mercProfileBytes[410..]);
        profile.bMechanical = MemoryMarshal.Read<sbyte>(mercProfileBytes[411..]);

        profile.ubInvUndroppable = MemoryMarshal.Read<byte>(mercProfileBytes[412..]);
        profile.ubRoomRangeStart = mercProfileBytes[413..415].ToArray();
        profile.invIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[416..454]).ToArray();//[19]
        profile.bMercTownReputation = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[454..474]).ToArray();

        profile.usStatChangeChancesIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[475..499]).ToArray();     // used strictly for balancing, never shown!
        profile.usStatChangeSuccessesIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[500..524]).ToArray();   // used strictly for balancing, never shown!

        profile.ubStrategicInsertionCode = MemoryMarshal.Read<byte>(mercProfileBytes[525..]);

        profile.ubRoomRangeEnd = mercProfileBytes[526..527].ToArray();

        profile.bPadding = mercProfileBytes[527..529].ToArray();

        profile.ubLastQuoteSaid = MemoryMarshal.Read<byte>(mercProfileBytes[529..]);

        profile.bRace = MemoryMarshal.Read<sbyte>(mercProfileBytes[530..]);
        profile.bNationality = MemoryMarshal.Read<sbyte>(mercProfileBytes[531..]);
        profile.bAppearance = MemoryMarshal.Read<sbyte>(mercProfileBytes[532..]);
        profile.bAppearanceCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[533..]);
        profile.bRefinement = MemoryMarshal.Read<sbyte>(mercProfileBytes[534..]);
        profile.bRefinementCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[535..]);
        profile.bHatedNationality = MemoryMarshal.Read<sbyte>(mercProfileBytes[536..]);
        profile.bHatedNationalityCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[537..]);
        profile.bRacist = MemoryMarshal.Read<sbyte>(mercProfileBytes[538..]);
        profile.uiWeeklySalary = MemoryMarshal.Read<int>(mercProfileBytes[540..]);
        profile.uiBiWeeklySalary = MemoryMarshal.Read<int>(mercProfileBytes[544..]);
        profile.bMedicalDeposit = MemoryMarshal.Read<sbyte>(mercProfileBytes[548..]);
        profile.bAttitude = (ATT)MemoryMarshal.Read<sbyte>(mercProfileBytes[549..]);
        profile.bBaseMorale = MemoryMarshal.Read<sbyte>(mercProfileBytes[550..]);
        profile.sMedicalDepositAmount = MemoryMarshal.Read<ushort>(mercProfileBytes[551..]);

        profile.bLearnToLike = MemoryMarshal.Read<sbyte>(mercProfileBytes[554..]);
        profile.ubApproachVal = mercProfileBytes[555..559].ToArray();

        //profile.ubApproachMod[3][4] = MemoryMarshal.Read<byte>(mercProfileBytes[556..]);




        profile.bTown = (TOWNS)MemoryMarshal.Read<sbyte>(mercProfileBytes[568..]);
        profile.bTownAttachment = MemoryMarshal.Read<sbyte>(mercProfileBytes[569..]);
        profile.usOptionalGearCost = MemoryMarshal.Read<ushort>(mercProfileBytes[570..]);
        profile.bMercOpinionIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[576..651]).ToArray();
        profile.bApproached = MemoryMarshal.Read<sbyte>(mercProfileBytes[651..]);
        profile.bMercStatus = (MercStatus)MemoryMarshal.Read<sbyte>(mercProfileBytes[652..]);                               //The status of the merc.  If negative, see flags at the top of this file.  Positive:  The number of days the merc is away for.  0:  Not hired but ready to be.
        profile.bHatedTime = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[653..658]).ToArray();
        profile.bLearnToLikeTime = MemoryMarshal.Read<sbyte>(mercProfileBytes[658..]);
        profile.bLearnToHateTime = MemoryMarshal.Read<sbyte>(mercProfileBytes[659..]);
        profile.bHatedCount = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[660..665]).ToArray();
        profile.bLearnToLikeCount = MemoryMarshal.Read<sbyte>(mercProfileBytes[665..]);
        profile.bLearnToHateCount = MemoryMarshal.Read<sbyte>(mercProfileBytes[666..]);
        profile.ubLastDateSpokenTo = MemoryMarshal.Read<byte>(mercProfileBytes[667..]);
        profile.bLastQuoteSaidWasSpecial = MemoryMarshal.Read<byte>(mercProfileBytes[668..]);
        profile.bSectorZ = MemoryMarshal.Read<sbyte>(mercProfileBytes[669..]);
        profile.usStrategicInsertionData = MemoryMarshal.Read<ushort>(mercProfileBytes[670..]);


        profile.bFriendlyOrDirectDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[679..]);
        profile.bRecruitDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[680..]);
        profile.bThreatenDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[681..]);
        profile.bNPCData = MemoryMarshal.Read<sbyte>(mercProfileBytes[682..]);          // NPC specifi
        profile.iBalance = MemoryMarshal.Read<int>(mercProfileBytes[679..]);
        profile.sTrueSalary = MemoryMarshal.Read<short>(mercProfileBytes[680..]); // for use when the person is working for us for free but has a positive salary value
        profile.ubCivilianGroup = MemoryMarshal.Read<byte>(mercProfileBytes[682..]);
        profile.ubNeedForSleep = MemoryMarshal.Read<byte>(mercProfileBytes[683..]);
        profile.uiMoney = MemoryMarshal.Read<int>(mercProfileBytes[684..]);
        profile.bNPCData2 = MemoryMarshal.Read<sbyte>(mercProfileBytes[688..]);     // NPC specific

        profile.ubMiscFlags3 = (PROFILE_MISC_FLAG3)MemoryMarshal.Read<byte>(mercProfileBytes[689..]);

        profile.ubDaysOfMoraleHangover = MemoryMarshal.Read<byte>(mercProfileBytes[690..]);       // used only when merc leaves team while having poor morale
        profile.ubNumTimesDrugUseInLifetime = MemoryMarshal.Read<byte>(mercProfileBytes[691..]);      // The # times a drug has been used in the player's lifetime...

        // Flags used for the precedent to repeating oneself in Contract negotiations.  Used for quote 80 -  ~107.  Gets reset every day
        profile.uiPrecedentQuoteSaid = MemoryMarshal.Read<int>(mercProfileBytes[692..]);
        profile.uiProfileChecksum = MemoryMarshal.Read<int>(mercProfileBytes[696..]);
        profile.sPreCombatGridNo = MemoryMarshal.Read<short>(mercProfileBytes[700..]);
        profile.ubTimeTillNextHatedComplaint = MemoryMarshal.Read<byte>(mercProfileBytes[702..]);
        profile.ubSuspiciousDeath = MemoryMarshal.Read<byte>(mercProfileBytes[703..]);

        profile.iMercMercContractLength = MemoryMarshal.Read<int>(mercProfileBytes[704..]);      //Used for MERC mercs, specifies how many days the merc has gone since last page

        profile.uiTotalCostToDate = MemoryMarshal.Read<int>(mercProfileBytes[708..]);           // The total amount of money that has been paid to the merc for their salary
        profile.ubBuffer = mercProfileBytes[708..712].ToArray();

        // Whew! We loaded some indexes above, so let's fill in the dictionaries and tie up some loose ends.
        for (int i = 0; i < profile.invIndexes.Length; i++)
        {
            profile.inv.Add((InventorySlot)i, (Items)profile.invIndexes[i]);
        }

        return profile;
    }
}

//ONLY HAVE 8 MISC FLAGS.. SHOULD BE ENOUGH
[Flags]
public enum PROFILE_MISC_FLAG
{
    RECRUITED = 0x01,
    HAVESEENCREATURE = 0x02,
    FORCENPCQUOTE = 0x04,
    WOUNDEDBYPLAYER = 0x08,
    TEMP_NPC_QUOTE_DATA_EXISTS = 0x10,
    SAID_HOSTILE_QUOTE = 0x20,
    EPCACTIVE = 0x40,
    //The player has already purchased the mercs items.
    ALREADY_USED_ITEMS = 0x80,
};

[Flags]
public enum PROFILE_MISC_FLAG2
{
    DONT_ADD_TO_SECTOR = 0x01,
    LEFT_COUNTRY = 0x02,
    BANDAGED_TODAY = 0x04,
    SAID_FIRSTSEEN_QUOTE = 0x08,
    NEEDS_TO_SAY_HOSTILE_QUOTE = 0x10,
    MARRIED_TO_HICKS = 0x20,
    ASKED_BY_HICKS = 0x40,
};


[Flags]
public enum PROFILE_MISC_FLAG3
{
    // In the aimscreen, the merc was away and the player left a message
    PLAYER_LEFT_MSG_FOR_MERC_AT_AIM = 0x01,
    PERMANENT_INSERTION_CODE = 0x02,

    // player's had a chance to hire this merc
    PLAYER_HAD_CHANCE_TO_HIRE = 0x04,
    HANDLE_DONE_TRAVERSAL = 0x08,

    NPC_PISSED_OFF = 0x10,

    // In the merc site, the merc has died and Speck quote for the dead merc has been said
    MERC_MERC_IS_DEAD_AND_QUOTE_SAID = 0x20,

    TOWN_DOESNT_CARE_ABOUT_DEATH = 0x40,
    GOODGUY = 0x80,
};


[StructLayout(LayoutKind.Sequential)]
public unsafe struct MERCPROFILESTRUCTDISK
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Globals.NAME_LENGTH)] ushort[] zName;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Globals.NICKNAME_LENGTH)] ushort[] zNickname;
    uint uiAttnSound;
    uint uiCurseSound;
    uint uiDieSound;
    uint uiGoodSound;
    uint uiGruntSound;
    uint uiGrunt2Sound;
    uint uiOkSound;
    byte ubFaceIndex;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] sbyte[] PANTS;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] sbyte[] VEST;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] sbyte[] SKIN;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] sbyte[] HAIR;
    sbyte bSex;
    sbyte bArmourAttractiveness;
    byte ubMiscFlags2;
    sbyte bEvolution;
    byte ubMiscFlags;
    byte bSexist;
    sbyte bLearnToHate;

    // skills
    sbyte bStealRate;
    sbyte bVocalVolume;
    byte ubQuoteRecord;
    sbyte bDeathRate;
    sbyte bScientific;

    short sExpLevelGain;
    short sLifeGain;
    short sAgilityGain;
    short sDexterityGain;
    short sWisdomGain;
    short sMarksmanshipGain;
    short sMedicalGain;
    short sMechanicGain;
    short sExplosivesGain;

    byte ubBodyType;
    sbyte bMedical;

    ushort usEyesX;
    ushort usEyesY;
    ushort usMouthX;
    ushort usMouthY;
    uint uiEyeDelay;
    uint uiMouthDelay;
    uint uiBlinkFrequency;
    uint uiExpressionFrequency;
    ushort sSectorX;
    ushort sSectorY;

    uint uiDayBecomesAvailable;           //day the merc will be available.  used with the bMercStatus

    sbyte bStrength;

    sbyte bLifeMax;
    sbyte bExpLevelDelta;
    sbyte bLifeDelta;
    sbyte bAgilityDelta;
    sbyte bDexterityDelta;
    sbyte bWisdomDelta;
    sbyte bMarksmanshipDelta;
    sbyte bMedicalDelta;
    sbyte bMechanicDelta;
    sbyte bExplosivesDelta;
    sbyte bStrengthDelta;
    sbyte bLeadershipDelta;
    ushort usKills;
    ushort usAssists;
    ushort usShotsFired;
    ushort usShotsHit;
    ushort usBattlesFought;
    ushort usTimesWounded;
    ushort usTotalDaysServed;

    short sLeadershipGain;
    short sStrengthGain;



    // BODY TYPE SUBSITUTIONS
    uint uiBodyTypeSubFlags;

    short sSalary;
    sbyte bLife;
    sbyte bDexterity;        // dexterity (hand coord) value
    sbyte bPersonalityTrait;
    sbyte bSkillTrait;

    sbyte bReputationTolerance;
    sbyte bExplosive;
    sbyte bSkillTrait2;
    sbyte bLeadership;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] sbyte[] bBuddy;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] sbyte[] bHated;
    sbyte bExpLevel;     // general experience level

    sbyte bMarksmanship;
    byte bMinService;
    sbyte bWisdom;
    byte bResigned;
    byte bActive;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] byte[] bInvStatus;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] byte[] bInvNumber;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] ushort[] usApproachFactor;

    sbyte bMainGunAttractiveness;
    sbyte bAgility;          // agility (speed) value

    bool fUseProfileInsertionInfo;               // Set to various flags, ( contained in TacticalSave.h )
    short sGridNo;                                              // The Gridno the NPC was in before leaving the sector
    byte ubQuoteActionID;
    sbyte bMechanical;

    byte ubInvUndroppable;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] byte[] ubRoomRangeStart;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] ushort[] inv;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] sbyte[] bMercTownReputation;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] ushort[] usStatChangeChances;     // used strictly for balancing, never shown!
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] ushort[] usStatChangeSuccesses;   // used strictly for balancing, never shown!

    byte ubStrategicInsertionCode;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] byte[] ubRoomRangeEnd;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] sbyte[] bPadding;

    byte ubLastQuoteSaid;

    sbyte bRace;
    sbyte bNationality;
    sbyte bAppearance;
    sbyte bAppearanceCareLevel;
    sbyte bRefinement;
    sbyte bRefinementCareLevel;
    sbyte bHatedNationality;
    sbyte bHatedNationalityCareLevel;
    sbyte bRacist;
    uint uiWeeklySalary;
    uint uiBiWeeklySalary;
    sbyte bMedicalDeposit;
    sbyte bAttitude;
    sbyte bBaseMorale;
    ushort sMedicalDepositAmount;

    sbyte bLearnToLike;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] byte[] ubApproachVal;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] byte[] ubApproachMod;//[4];
    sbyte bTown;
    sbyte bTownAttachment;
    ushort usOptionalGearCost;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 75)] sbyte[] bMercOpinion;
    sbyte bApproached;
    sbyte bMercStatus;                               //The status of the merc.  If negative, see flags at the top of this file.  Positive:  The number of days the merc is away for.  0:  Not hired but ready to be.
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] sbyte[] bHatedTime;
    sbyte bLearnToLikeTime;
    sbyte bLearnToHateTime;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] sbyte[] bHatedCount;
    sbyte bLearnToLikeCount;
    sbyte bLearnToHateCount;
    byte ubLastDateSpokenTo;
    byte bLastQuoteSaidWasSpecial;
    sbyte bSectorZ;
    ushort usStrategicInsertionData;
    sbyte bFriendlyOrDirectDefaultResponseUsedRecently;
    sbyte bRecruitDefaultResponseUsedRecently;
    sbyte bThreatenDefaultResponseUsedRecently;
    sbyte bNPCData;          // NPC specific
    int iBalance;
    short sTrueSalary; // for use when the person is working for us for free but has a positive salary value
    byte ubCivilianGroup;
    byte ubNeedForSleep;
    uint uiMoney;
    sbyte bNPCData2;     // NPC specific

    byte ubMiscFlags3;

    byte ubDaysOfMoraleHangover;       // used only when merc leaves team while having poor morale
    byte ubNumTimesDrugUseInLifetime;      // The # times a drug has been used in the player's lifetime...

    // Flags used for the precedent to repeating oneself in Contract negotiations.  Used for quote 80 -  ~107.  Gets reset every day
    uint uiPrecedentQuoteSaid;
    uint uiProfileChecksum;
    short sPreCombatGridNo;
    byte ubTimeTillNextHatedComplaint;
    byte ubSuspiciousDeath;

    int iMercMercContractLength;      //Used for MERC mercs, specifies how many days the merc has gone since last page

    uint uiTotalCostToDate;           // The total amount of money that has been paid to the merc for their salary

    fixed byte ubBuffer2[4];
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] byte[] ubBuffer;
}

