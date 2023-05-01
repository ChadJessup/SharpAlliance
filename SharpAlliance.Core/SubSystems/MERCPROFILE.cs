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
    public int Index { get; set; }
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

    public static MERCPROFILESTRUCT LoadFromBytes(ReadOnlySpan<byte> mercProfileBytes, NPCID index)
    {
        var profile = new MERCPROFILESTRUCT
        {
            Index = (int)index,
            zName = MemoryMarshal.Cast<byte, char>(mercProfileBytes[..60]).ToString().TrimEnd('\0'),
            zNickname = MemoryMarshal.Cast<byte, char>(mercProfileBytes[60..80]).ToString().TrimEnd('\0'),

            uiAttnSound = MemoryMarshal.Read<int>(mercProfileBytes[80..]),
            uiCurseSound = MemoryMarshal.Read<int>(mercProfileBytes[84..]),
            uiDieSound = MemoryMarshal.Read<int>(mercProfileBytes[88..]),
            uiGoodSound = MemoryMarshal.Read<int>(mercProfileBytes[92..]),
            uiGruntSound = MemoryMarshal.Read<int>(mercProfileBytes[96..]),
            uiGrunt2Sound = MemoryMarshal.Read<int>(mercProfileBytes[100..]),
            uiOkSound = MemoryMarshal.Read<int>(mercProfileBytes[104..]),
            ubFaceIndex = MemoryMarshal.Read<byte>(mercProfileBytes[108..]),

            PANTS = Encoding.ASCII.GetString(mercProfileBytes[109..139]).TrimEnd('\0'),
            VEST = Encoding.ASCII.GetString(mercProfileBytes[139..169]).TrimEnd('\0'),
            SKIN = Encoding.ASCII.GetString(mercProfileBytes[169..199]).TrimEnd('\0'),
            HAIR = Encoding.ASCII.GetString(mercProfileBytes[199..229]).TrimEnd('\0'),

            bSex = (Sexes)MemoryMarshal.Read<sbyte>(mercProfileBytes[229..]),
            bArmourAttractiveness = MemoryMarshal.Read<sbyte>(mercProfileBytes[230..]),
            ubMiscFlags2 = (PROFILE_MISC_FLAG2)MemoryMarshal.Read<byte>(mercProfileBytes[231..]),
            bEvolution = (CharacterEvolution)MemoryMarshal.Read<sbyte>(mercProfileBytes[232..]),
            ubMiscFlags = (PROFILE_MISC_FLAG)MemoryMarshal.Read<sbyte>(mercProfileBytes[233..]),
            bSexist = (SexistLevels)MemoryMarshal.Read<byte>(mercProfileBytes[234..]),
            bLearnToHate = MemoryMarshal.Read<sbyte>(mercProfileBytes[235..]),

            bStealRate = MemoryMarshal.Read<sbyte>(mercProfileBytes[236..]),
            bVocalVolume = MemoryMarshal.Read<sbyte>(mercProfileBytes[237..]),
            ubQuoteRecord = MemoryMarshal.Read<byte>(mercProfileBytes[238..]),
            bDeathRate = MemoryMarshal.Read<sbyte>(mercProfileBytes[239..]),
            bScientific = MemoryMarshal.Read<sbyte>(mercProfileBytes[240..]),

            sExpLevelGain = MemoryMarshal.Read<short>(mercProfileBytes[241..]),
            sLifeGain = MemoryMarshal.Read<short>(mercProfileBytes[243..]),
            sAgilityGain = MemoryMarshal.Read<short>(mercProfileBytes[245..]),
            sDexterityGain = MemoryMarshal.Read<short>(mercProfileBytes[247..]),
            sWisdomGain = MemoryMarshal.Read<short>(mercProfileBytes[249..]),
            sMarksmanshipGain = MemoryMarshal.Read<short>(mercProfileBytes[251..]),
            sMedicalGain = MemoryMarshal.Read<short>(mercProfileBytes[253..]),
            sMechanicGain = MemoryMarshal.Read<short>(mercProfileBytes[255..]),
            sExplosivesGain = MemoryMarshal.Read<short>(mercProfileBytes[257..]),

            ubBodyType = (SoldierBodyTypes)MemoryMarshal.Read<byte>(mercProfileBytes[259..]),
            bMedical = MemoryMarshal.Read<sbyte>(mercProfileBytes[261..]),

            usEyesX = MemoryMarshal.Read<ushort>(mercProfileBytes[262..]),
            usEyesY = MemoryMarshal.Read<ushort>(mercProfileBytes[264..]),
            usMouthX = MemoryMarshal.Read<ushort>(mercProfileBytes[266..]),
            usMouthY = MemoryMarshal.Read<ushort>(mercProfileBytes[268..]),
            uiEyeDelay = MemoryMarshal.Read<int>(mercProfileBytes[272..]),
            uiMouthDelay = MemoryMarshal.Read<int>(mercProfileBytes[276..]),
            uiBlinkFrequency = MemoryMarshal.Read<int>(mercProfileBytes[280..]),
            uiExpressionFrequency = MemoryMarshal.Read<int>(mercProfileBytes[284..]),
            sSectorX = MemoryMarshal.Read<ushort>(mercProfileBytes[287..]),
            sSectorY = (MAP_ROW)MemoryMarshal.Read<ushort>(mercProfileBytes[289..]),

            uiDayBecomesAvailable = MemoryMarshal.Read<int>(mercProfileBytes[291..]),           //day the merc will be available.  used with the bMercStatus

            bStrength = MemoryMarshal.Read<sbyte>(mercProfileBytes[296..]),

            bLifeMax = MemoryMarshal.Read<sbyte>(mercProfileBytes[297..]),
            bExpLevelDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[298..]),
            bLifeDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[299..]),
            bAgilityDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[300..]),
            bDexterityDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[301..]),
            bWisdomDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[302..]),
            bMarksmanshipDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[303..]),
            bMedicalDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[304..]),
            bMechanicDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[305..]),
            bExplosivesDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[306..]),
            bStrengthDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[307..]),
            bLeadershipDelta = MemoryMarshal.Read<sbyte>(mercProfileBytes[308..]),
            usKills = MemoryMarshal.Read<ushort>(mercProfileBytes[309..]),
            usAssists = MemoryMarshal.Read<ushort>(mercProfileBytes[311..]),
            usShotsFired = MemoryMarshal.Read<ushort>(mercProfileBytes[313..]),
            usShotsHit = MemoryMarshal.Read<ushort>(mercProfileBytes[315..]),
            usBattlesFought = MemoryMarshal.Read<ushort>(mercProfileBytes[317..]),
            usTimesWounded = MemoryMarshal.Read<ushort>(mercProfileBytes[319..]),
            usTotalDaysServed = MemoryMarshal.Read<ushort>(mercProfileBytes[321..]),

            sLeadershipGain = MemoryMarshal.Read<short>(mercProfileBytes[323..]),
            sStrengthGain = MemoryMarshal.Read<short>(mercProfileBytes[325..]),



            // BODY TYPE SUBSITUTIONS
            uiBodyTypeSubFlags = MemoryMarshal.Read<int>(mercProfileBytes[327..]),

            sSalary = MemoryMarshal.Read<short>(mercProfileBytes[332..]),

            bLife = MemoryMarshal.Read<sbyte>(mercProfileBytes[334..]),
            bDexterity = MemoryMarshal.Read<sbyte>(mercProfileBytes[335..]),        // dexterity (hand coord) value
            bPersonalityTrait = (PersonalityTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[336..]),
            bSkillTrait = (SkillTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[337..]),
            bReputationTolerance = MemoryMarshal.Read<sbyte>(mercProfileBytes[338..]),
            bExplosive = MemoryMarshal.Read<sbyte>(mercProfileBytes[339..]),
            bSkillTrait2 = (SkillTrait)MemoryMarshal.Read<sbyte>(mercProfileBytes[340..]),
            bLeadership = MemoryMarshal.Read<sbyte>(mercProfileBytes[341..]),
            bBuddyIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[342..347]).ToArray(),
            bHatedIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[347..352]).ToArray(),
            bExpLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[352..]),     // general experience level
            bMarksmanship = MemoryMarshal.Read<sbyte>(mercProfileBytes[353..]),

            bMinService = MemoryMarshal.Read<byte>(mercProfileBytes[354..]),
            bWisdom = MemoryMarshal.Read<sbyte>(mercProfileBytes[355..]),
            bResigned = MemoryMarshal.Read<byte>(mercProfileBytes[356..]),
            bActive = MemoryMarshal.Read<byte>(mercProfileBytes[357..]),

            bInvStatus = mercProfileBytes[358..377].ToArray(),
            bInvNumber = mercProfileBytes[377..396].ToArray(),
            usApproachFactor = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[396..404]).ToArray(),

            bMainGunAttractiveness = MemoryMarshal.Read<sbyte>(mercProfileBytes[404..]),
            bAgility = MemoryMarshal.Read<sbyte>(mercProfileBytes[405..]),          // agility (speed) value

            fUseProfileInsertionInfo = MemoryMarshal.Read<byte>(mercProfileBytes[406..]), // Set to various flags, ( contained in TacticalSave.h )
            sGridNo = MemoryMarshal.Read<short>(mercProfileBytes[407..]),                                              // The Gridno the NPC was in before leaving the sector
            ubQuoteActionID = MemoryMarshal.Read<byte>(mercProfileBytes[410..]),
            bMechanical = MemoryMarshal.Read<sbyte>(mercProfileBytes[411..]),

            ubInvUndroppable = MemoryMarshal.Read<byte>(mercProfileBytes[412..]),
            ubRoomRangeStart = mercProfileBytes[413..415].ToArray(),
            invIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[416..454]).ToArray(),//[19]
            bMercTownReputation = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[454..474]).ToArray(),

            usStatChangeChancesIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[475..499]).ToArray(),     // used strictly for balancing, never shown!
            usStatChangeSuccessesIndexes = MemoryMarshal.Cast<byte, ushort>(mercProfileBytes[500..524]).ToArray(),   // used strictly for balancing, never shown!

            ubStrategicInsertionCode = MemoryMarshal.Read<byte>(mercProfileBytes[525..]),

            ubRoomRangeEnd = mercProfileBytes[526..527].ToArray(),

            bPadding = mercProfileBytes[527..529].ToArray(),

            ubLastQuoteSaid = MemoryMarshal.Read<byte>(mercProfileBytes[529..]),

            bRace = MemoryMarshal.Read<sbyte>(mercProfileBytes[530..]),
            bNationality = MemoryMarshal.Read<sbyte>(mercProfileBytes[531..]),
            bAppearance = MemoryMarshal.Read<sbyte>(mercProfileBytes[532..]),
            bAppearanceCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[533..]),
            bRefinement = MemoryMarshal.Read<sbyte>(mercProfileBytes[534..]),
            bRefinementCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[535..]),
            bHatedNationality = MemoryMarshal.Read<sbyte>(mercProfileBytes[536..]),
            bHatedNationalityCareLevel = MemoryMarshal.Read<sbyte>(mercProfileBytes[537..]),
            bRacist = MemoryMarshal.Read<sbyte>(mercProfileBytes[538..]),
            uiWeeklySalary = MemoryMarshal.Read<int>(mercProfileBytes[540..]),
            uiBiWeeklySalary = MemoryMarshal.Read<int>(mercProfileBytes[544..]),
            bMedicalDeposit = MemoryMarshal.Read<sbyte>(mercProfileBytes[548..]),
            bAttitude = (ATT)MemoryMarshal.Read<sbyte>(mercProfileBytes[549..]),
            bBaseMorale = MemoryMarshal.Read<sbyte>(mercProfileBytes[550..]),
            sMedicalDepositAmount = MemoryMarshal.Read<ushort>(mercProfileBytes[551..]),

            bLearnToLike = MemoryMarshal.Read<sbyte>(mercProfileBytes[554..]),
            ubApproachVal = mercProfileBytes[555..559].ToArray(),

            // We do some further parsing for the jagged arrays here outside of this initializer.

            bTown = (TOWNS)MemoryMarshal.Read<sbyte>(mercProfileBytes[568..]),
            bTownAttachment = MemoryMarshal.Read<sbyte>(mercProfileBytes[569..]),
            usOptionalGearCost = MemoryMarshal.Read<ushort>(mercProfileBytes[570..]),
            bMercOpinionIndexes = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[576..651]).ToArray(),
            bApproached = MemoryMarshal.Read<sbyte>(mercProfileBytes[651..]),
            bMercStatus = (MercStatus)MemoryMarshal.Read<sbyte>(mercProfileBytes[652..]),                               //The status of the merc.  If negative, see flags at the top of this file.  Positive:  The number of days the merc is away for.  0:  Not hired but ready to be.
            bHatedTime = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[653..658]).ToArray(),
            bLearnToLikeTime = MemoryMarshal.Read<sbyte>(mercProfileBytes[658..]),
            bLearnToHateTime = MemoryMarshal.Read<sbyte>(mercProfileBytes[659..]),
            bHatedCount = MemoryMarshal.Cast<byte, sbyte>(mercProfileBytes[660..665]).ToArray(),
            bLearnToLikeCount = MemoryMarshal.Read<sbyte>(mercProfileBytes[665..]),
            bLearnToHateCount = MemoryMarshal.Read<sbyte>(mercProfileBytes[666..]),
            ubLastDateSpokenTo = MemoryMarshal.Read<byte>(mercProfileBytes[667..]),
            bLastQuoteSaidWasSpecial = MemoryMarshal.Read<byte>(mercProfileBytes[668..]),
            bSectorZ = MemoryMarshal.Read<sbyte>(mercProfileBytes[669..]),
            usStrategicInsertionData = MemoryMarshal.Read<ushort>(mercProfileBytes[670..]),


            bFriendlyOrDirectDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[679..]),
            bRecruitDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[680..]),
            bThreatenDefaultResponseUsedRecently = MemoryMarshal.Read<sbyte>(mercProfileBytes[681..]),
            bNPCData = MemoryMarshal.Read<sbyte>(mercProfileBytes[682..]),          // NPC specifi
            iBalance = MemoryMarshal.Read<int>(mercProfileBytes[679..]),
            sTrueSalary = MemoryMarshal.Read<short>(mercProfileBytes[680..]), // for use when the person is working for us for free but has a positive salary value
            ubCivilianGroup = MemoryMarshal.Read<byte>(mercProfileBytes[682..]),
            ubNeedForSleep = MemoryMarshal.Read<byte>(mercProfileBytes[683..]),
            uiMoney = MemoryMarshal.Read<int>(mercProfileBytes[684..]),
            bNPCData2 = MemoryMarshal.Read<sbyte>(mercProfileBytes[688..]),     // NPC specific

            ubMiscFlags3 = (PROFILE_MISC_FLAG3)MemoryMarshal.Read<byte>(mercProfileBytes[689..]),

            ubDaysOfMoraleHangover = MemoryMarshal.Read<byte>(mercProfileBytes[690..]),       // used only when merc leaves team while having poor morale
            ubNumTimesDrugUseInLifetime = MemoryMarshal.Read<byte>(mercProfileBytes[691..]),      // The # times a drug has been used in the player's lifetime...

            // Flags used for the precedent to repeating oneself in Contract negotiations.  Used for quote 80 -  ~107.  Gets reset every day
            uiPrecedentQuoteSaid = MemoryMarshal.Read<int>(mercProfileBytes[692..]),
            uiProfileChecksum = MemoryMarshal.Read<int>(mercProfileBytes[696..]),
            sPreCombatGridNo = MemoryMarshal.Read<short>(mercProfileBytes[700..]),
            ubTimeTillNextHatedComplaint = MemoryMarshal.Read<byte>(mercProfileBytes[702..]),
            ubSuspiciousDeath = MemoryMarshal.Read<byte>(mercProfileBytes[703..]),

            iMercMercContractLength = MemoryMarshal.Read<int>(mercProfileBytes[704..]),      //Used for MERC mercs, specifies how many days the merc has gone since last page

            uiTotalCostToDate = MemoryMarshal.Read<int>(mercProfileBytes[708..]),           // The total amount of money that has been paid to the merc for their salary
            ubBuffer = mercProfileBytes[708..712].ToArray()
        };

        // Jagged array on disk here, have to parse a little differently.
        profile.ubApproachMod[0] = mercProfileBytes[559..563].ToArray();
        profile.ubApproachMod[1] = mercProfileBytes[563..567].ToArray();
        profile.ubApproachMod[2] = mercProfileBytes[567..571].ToArray();

        // Whew! We loaded some indexes above, so let's fill in the dictionaries and tie up some loose ends.
        for (int i = 0; i < profile.invIndexes.Length; i++)
        {
            profile.inv.Add((InventorySlot)i, (Items)profile.invIndexes[i]);
        }

        for (int i = 0; i < profile.usStatChangeChancesIndexes.Length; i++)
        {
            profile.usStatChangeChances.Add((Stat)i, profile.usStatChangeChancesIndexes[i]);
        }

        for (int i = 0; i < profile.usStatChangeSuccessesIndexes.Length; i++)
        {
            profile.usStatChangeSuccesses.Add((Stat)i, profile.usStatChangeSuccessesIndexes[i]);
        }

        for (int i = 0; i < profile.bMercOpinionIndexes.Length; i++)
        {
            profile.bMercOpinion.Add((NPCID)i, profile.bMercOpinionIndexes[i]);
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

