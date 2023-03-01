using System;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Quests
{
    public const int BOBBYR_SHIPPING_DEST_SECTOR_X = 13;
    public const MAP_ROW BOBBYR_SHIPPING_DEST_SECTOR_Y = (MAP_ROW)2;
    public const int BOBBYR_SHIPPING_DEST_SECTOR_Z = 0;
    public const int BOBBYR_SHIPPING_DEST_GRIDNO = 10112;
    public const int PABLOS_STOLEN_DEST_GRIDNO = 1;
    public const int LOST_SHIPMENT_GRIDNO = 2;

    // omerta positions
    public const int OMERTA_LEAVE_EQUIP_SECTOR_X = 9;
    public const int OMERTA_LEAVE_EQUIP_SECTOR_Y = 1;
    public const int OMERTA_LEAVE_EQUIP_SECTOR_Z = 0;
    public const int OMERTA_LEAVE_EQUIP_GRIDNO = 4868;

    public static bool CheckNPCWithin(NPCID ubFirstNPC, NPCID ubSecondNPC, int ubMaxDistance)
    {
        SOLDIERTYPE? pFirstNPC, pSecondNPC;

        pFirstNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubFirstNPC, false);
        pSecondNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubSecondNPC, false);
        if (pFirstNPC is null || pSecondNPC is null)
        {
            return (false);
        }
        return (PythSpacesAway(pFirstNPC.sGridNo, pSecondNPC.sGridNo) <= ubMaxDistance);
    }

    public static bool CheckForNewShipment()
    {
        ITEM_POOL? pItemPool;

        if ((Globals.gWorldSectorX == BOBBYR_SHIPPING_DEST_SECTOR_X) && (Globals.gWorldSectorY == BOBBYR_SHIPPING_DEST_SECTOR_Y) && (Globals.gbWorldSectorZ == BOBBYR_SHIPPING_DEST_SECTOR_Z))
        {
            if (HandleItems.GetItemPool(BOBBYR_SHIPPING_DEST_GRIDNO, out pItemPool, 0))
            {
                return (!(ITEMPOOL_VISIBLE(pItemPool)));
            }
        }
        return (false);
    }


    public static bool CheckNPCBleeding(NPCID ubProfileID)
    {
        SOLDIERTYPE? pSoldier;

        // the NPC is wounded...
        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pSoldier is not null && pSoldier.bLife > 0 && pSoldier.bBleeding > 0)
        {
            return (true);
        }
        else
        {
            return (false);
        }

    }

    public static bool CheckNPCWounded(NPCID ubProfileID, bool fByPlayerOnly)
    {
        SOLDIERTYPE? pSoldier;

        // is the NPC is wounded at all?
        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pSoldier is not null && pSoldier.bLife < pSoldier.bLifeMax)
        {
            if (fByPlayerOnly)
            {
                if (Globals.gMercProfiles[ubProfileID].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_WOUNDEDBYPLAYER))
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                return (true);
            }
        }
        else
        {
            return (false);

        }
    }

    public static bool CheckNPCIsEPC(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;

        if (Globals.gMercProfiles[ubProfileID].bMercStatus == MercStatus.MERC_IS_DEAD)
        {
            return false;
        }

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, true);
        if (pNPC is null)
        {
            return false;
        }

        return ((pNPC.ubWhatKindOfMercAmI == MERC_TYPE.EPC));
    }

    public static bool CheckGuyVisible(NPCID ubNPC, NPCID ubGuy)
    {
        // NB ONLY WORKS IF ON DIFFERENT TEAMS
        SOLDIERTYPE? pNPC, pGuy;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubNPC, false);
        pGuy = SoldierProfileSubSystem.FindSoldierByProfileID(ubGuy, false);
        if (pNPC is null || pGuy is null)
        {
            return (false);
        }
        if (pNPC.bOppList[pGuy.ubID] == SEEN_CURRENTLY)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    public static bool CheckNPCAt(NPCID ubNPC, int sGridNo)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubNPC, false);
        if (pNPC is null)
        {
            return (false);
        }
        return (pNPC.sGridNo == sGridNo);
    }

    public static bool CheckNPCIsEnemy(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        if (pNPC.bSide == Globals.gbPlayerNum || pNPC.bNeutral > 0)
        {
            if (pNPC.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP)
            {
                // although the soldier is NOW the same side, this civ group could be set to "will become hostile"
                return (Globals.gTacticalStatus.fCivGroupHostile[pNPC.ubCivilianGroup] >= CIV_GROUP_WILL_BECOME_HOSTILE);
            }
            else
            {
                return (false);
            }
        }
        else
        {
            return (true);
        }
    }

    public static bool CheckIfMercIsNearNPC(SOLDIERTYPE? pMerc, NPCID ubProfileId)
    {
        SOLDIERTYPE? pNPC;
        int sGridNo;

        // no merc nearby?
        if (pMerc == null)
        {
            return (false);
        }

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileId, false);
        if (pNPC == null)
        {
            return (false);
        }
        sGridNo = pNPC.sGridNo;

        // is the merc and NPC close enough?
        if (PythSpacesAway(sGridNo, pMerc.sGridNo) <= 9)
        {
            return (true);
        }

        return (false);
    }

    public static void StartQuest(QUEST ubQuest, int sSectorX, MAP_ROW sSectorY)
    {
        InternalStartQuest(ubQuest, sSectorX, sSectorY, true);
    }


    private static void InternalStartQuest(QUEST ubQuest, int sSectorX, MAP_ROW sSectorY, bool fUpdateHistory)
    {
        if (gubQuest[ubQuest] == QUESTNOTSTARTED)
        {
            gubQuest[ubQuest] = QUESTINPROGRESS;

            if (fUpdateHistory)
            {
                History.SetHistoryFact(HISTORY.QUEST_STARTED, ubQuest, GetWorldTotalMin(), sSectorX, sSectorY);
            }
        }
        else
        {
            gubQuest[ubQuest] = QUESTINPROGRESS;
        }
    }

    public static int NumWoundedMercsNearby(NPCID ubProfileID)
    {
        int bNumber = 0;
        int uiLoop;
        SOLDIERTYPE? pNPC;
        SOLDIERTYPE? pSoldier;
        int sGridNo;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (0);
        }
        sGridNo = pNPC.sGridNo;

        for (uiLoop = 0; uiLoop < Globals.guiNumMercSlots; uiLoop++)
        {
            pSoldier = Globals.MercSlots[uiLoop];

            if (pSoldier is not null && pSoldier.bTeam == Globals.gbPlayerNum && pSoldier.bLife > 0 && pSoldier.bLife < pSoldier.bLifeMax && pSoldier.bAssignment != Assignments.ASSIGNMENT_HOSPITAL)
            {
                if (PythSpacesAway(sGridNo, pSoldier.sGridNo) <= HOSPITAL_PATIENT_DISTANCE)
                {
                    bNumber++;
                }
            }
        }

        return (bNumber);
    }

    public static int NumMercsNear(NPCID ubProfileID, int ubMaxDist)
    {
        int bNumber = 0;
        int uiLoop;
        SOLDIERTYPE? pNPC;
        SOLDIERTYPE? pSoldier;
        int sGridNo;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (0);
        }
        sGridNo = pNPC.sGridNo;

        for (uiLoop = 0; uiLoop < Globals.guiNumMercSlots; uiLoop++)
        {
            pSoldier = Globals.MercSlots[uiLoop];

            if (pSoldier is not null
                && pSoldier.bTeam == Globals.gbPlayerNum
                && pSoldier.bLife >= Globals.OKLIFE)
            {
                if (PythSpacesAway(sGridNo, pSoldier.sGridNo) <= ubMaxDist)
                {
                    bNumber++;
                }
            }
        }

        return (bNumber);
    }

    public static bool NPCInRoom(NPCID ubProfileID, int ubRoomID)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null || (Globals.gubWorldRoomInfo[pNPC.sGridNo] != ubRoomID))
        {
            return (false);
        }
        return (true);
    }

    public static bool NPCInRoomRange(NPCID ubProfileID, int ubRoomID1, int ubRoomID2)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null || (Globals.gubWorldRoomInfo[pNPC.sGridNo] < ubRoomID1)
            || (Globals.gubWorldRoomInfo[pNPC.sGridNo] > ubRoomID2))
        {
            return (false);
        }
        return (true);
    }

    public static bool PCInSameRoom(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;
        int ubRoom;
        int bLoop;
        SOLDIERTYPE? pSoldier;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        ubRoom = Globals.gubWorldRoomInfo[pNPC.sGridNo];

        for (bLoop = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID; bLoop <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; bLoop++)
        {
            pSoldier = Globals.MercPtrs[bLoop];
            if (pSoldier is not null && pSoldier.bActive && pSoldier.bInSector)
            {
                if (Globals.gubWorldRoomInfo[pSoldier.sGridNo] == ubRoom)
                {
                    return (true);
                }
            }
        }

        return (false);
    }


    public static bool CheckTalkerStrong()
    {
        if (Globals.gpSrcSoldier is not null && Globals.gpSrcSoldier.bTeam == Globals.gbPlayerNum)
        {
            return (Globals.gpSrcSoldier.bStrength >= 84);
        }
        else if (Globals.gpDestSoldier is not null && Globals.gpDestSoldier.bTeam == Globals.gbPlayerNum)
        {
            return (Globals.gpDestSoldier.bStrength >= 84);
        }
        return (false);
    }

    public static bool CheckTalkerFemale()
    {
        if (Globals.gpSrcSoldier is not null && Globals.gpSrcSoldier.bTeam == Globals.gbPlayerNum && Globals.gpSrcSoldier.ubProfile != NPCID.NO_PROFILE)
        {
            return (Globals.gMercProfiles[Globals.gpSrcSoldier.ubProfile].bSex == Sexes.FEMALE);
        }
        else if (Globals.gpDestSoldier is not null && Globals.gpDestSoldier.bTeam == Globals.gbPlayerNum && Globals.gpDestSoldier.ubProfile != NPCID.NO_PROFILE)
        {
            return (Globals.gMercProfiles[Globals.gpDestSoldier.ubProfile].bSex == Sexes.FEMALE);
        }
        return (false);
    }

    public static bool CheckTalkerUnpropositionedFemale()
    {
        if (Globals.gpSrcSoldier is not null && Globals.gpSrcSoldier.bTeam == Globals.gbPlayerNum && Globals.gpSrcSoldier.ubProfile != NPCID.NO_PROFILE)
        {
            if (!(Globals.gMercProfiles[Globals.gpSrcSoldier.ubProfile].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_ASKED_BY_HICKS)))
            {
                return (Globals.gMercProfiles[Globals.gpSrcSoldier.ubProfile].bSex == Sexes.FEMALE);
            }
        }
        else if (Globals.gpDestSoldier is not null && Globals.gpDestSoldier.bTeam == Globals.gbPlayerNum && Globals.gpDestSoldier.ubProfile != NPCID.NO_PROFILE)
        {
            if (!(Globals.gMercProfiles[Globals.gpDestSoldier.ubProfile].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_ASKED_BY_HICKS)))
            {
                return (Globals.gMercProfiles[Globals.gpDestSoldier.ubProfile].bSex == Sexes.FEMALE);
            }
        }
        return (false);
    }

    public static int NumMalesPresent(NPCID ubProfileID)
    {
        int bNumber = 0;
        int uiLoop;
        SOLDIERTYPE? pNPC;
        SOLDIERTYPE? pSoldier;
        int sGridNo;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (0);
        }
        sGridNo = pNPC.sGridNo;

        for (uiLoop = 0; uiLoop < Globals.guiNumMercSlots; uiLoop++)
        {
            pSoldier = Globals.MercSlots[uiLoop];

            if (pSoldier is not null && pSoldier.bTeam == Globals.gbPlayerNum && pSoldier.bLife >= Globals.OKLIFE)
            {
                if (pSoldier.ubProfile != NPCID.NO_PROFILE && Globals.gMercProfiles[pSoldier.ubProfile].bSex == Sexes.MALE)
                {
                    if (PythSpacesAway(sGridNo, pSoldier.sGridNo) <= 8)
                    {
                        bNumber++;
                    }
                }
            }
        }

        return (bNumber);
    }


    public static bool FemalePresent(NPCID ubProfileID)
    {
        int uiLoop;
        SOLDIERTYPE? pNPC;
        SOLDIERTYPE? pSoldier;
        int sGridNo;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        sGridNo = pNPC.sGridNo;

        for (uiLoop = 0; uiLoop < Globals.guiNumMercSlots; uiLoop++)
        {
            pSoldier = Globals.MercSlots[uiLoop];

            if (pSoldier is not null && pSoldier.bTeam == Globals.gbPlayerNum && pSoldier.bLife >= Globals.OKLIFE)
            {
                if (pSoldier.ubProfile != NPCID.NO_PROFILE && Globals.gMercProfiles[pSoldier.ubProfile].bSex == Sexes.FEMALE)
                {
                    if (PythSpacesAway(sGridNo, pSoldier.sGridNo) <= 10)
                    {
                        return (true);
                    }
                }
            }
        }

        return (false);
    }


    public static bool CheckPlayerHasHead()
    {
        int bLoop;
        SOLDIERTYPE? pSoldier;

        for (bLoop = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID; bLoop <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; bLoop++)
        {
            pSoldier = Globals.MercPtrs[bLoop];

            if (pSoldier.bActive && pSoldier.bLife > 0)
            {
                if (FindObjInObjRange(pSoldier, HEAD_2, HEAD_7) != NO_SLOT)
                {
                    return (true);
                }
            }
        }

        return (false);
    }
    public static bool CheckNPCSector(NPCID ubProfileID, int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        SOLDIERTYPE? pSoldier;

        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, true);

        if (pSoldier is not null)
        {
            if (pSoldier.sSectorX == sSectorX &&
                pSoldier.sSectorY == sSectorY &&
                pSoldier.bSectorZ == bSectorZ)
            {
                return (true);
            }
        }
        else if (Globals.gMercProfiles[ubProfileID].sSectorX == sSectorX
            && Globals.gMercProfiles[ubProfileID].sSectorY == sSectorY
            && Globals.gMercProfiles[ubProfileID].bSectorZ == bSectorZ)
        {
            return (true);
        }

        return (false);
    }

    public static bool AIMMercWithin(int sGridNo, int sDistance)
    {
        int uiLoop;
        SOLDIERTYPE? pSoldier;

        for (uiLoop = 0; uiLoop < Globals.guiNumMercSlots; uiLoop++)
        {
            pSoldier = Globals.MercSlots[uiLoop];

            if (pSoldier is not null && (pSoldier.bTeam == Globals.gbPlayerNum)
                && (pSoldier.bLife >= Globals.OKLIFE)
                && (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC))
            {
                if (PythSpacesAway(sGridNo, pSoldier.sGridNo) <= sDistance)
                {
                    return (true);
                }
            }
        }

        return (false);
    }

    public static bool CheckNPCCowering(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        return (((pNPC.uiStatusFlags.HasFlag(SOLDIER.COWERING))));
    }

    public static int CountBartenders()
    {
        NPCID ubLoop;
        int ubBartenders = 0;

        for (ubLoop = NPCID.HERVE; ubLoop <= NPCID.CARLO; ubLoop++)
        {
            if (Globals.gMercProfiles[ubLoop].bNPCData != 0)
            {
                ubBartenders++;
            }
        }
        return (ubBartenders);
    }

    public static bool CheckNPCIsUnderFire(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        return (pNPC.bUnderFire != 0);
    }

    public static bool NPCHeardShot(NPCID ubProfileID)
    {
        SOLDIERTYPE? pNPC;

        pNPC = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pNPC is null)
        {
            return (false);
        }
        return (pNPC.ubMiscSoldierFlags.HasFlag(SOLDIER_MISC.HEARD_GUNSHOT));
    }

    public static bool InTownSectorWithTrainingLoyalty(int sSectorX, MAP_ROW sSectorY)
    {
        TOWNS ubTown;

        ubTown = GetTownIdForSector(sSectorX, sSectorY);
        if ((ubTown != TOWNS.BLANK_SECTOR) && Globals.gTownLoyalty[ubTown].fStarted && Globals.gfTownUsesLoyalty[ubTown])
        {
            return (Globals.gTownLoyalty[ubTown].ubRating >= Globals.MIN_RATING_TO_TRAIN_TOWN);
        }
        else
        {
            return (false);
        }
    }

    void InternalStartQuest(QUEST ubQuest, int sSectorX, int sSectorY, bool fUpdateHistory)
    {
        if (Globals.gubQuest[ubQuest] == Globals.QUESTNOTSTARTED)
        {
            Globals.gubQuest[ubQuest] = Globals.QUESTINPROGRESS;

            if (fUpdateHistory)
            {
                Facts.SetHistoryFact(HISTORY.QUEST_STARTED, ubQuest, GetWorldTotalMin(), sSectorX, sSectorY);
            }
        }
        else
        {
            Globals.gubQuest[ubQuest] = Globals.QUESTINPROGRESS;
        }
    }

    void EndQuest(QUEST ubQuest, int sSectorX, int sSectorY)
    {
        InternalEndQuest(ubQuest, sSectorX, sSectorY, true);
    }

    private static void InternalEndQuest(QUEST ubQuest, int sSectorX, int sSectorY, bool fUpdateHistory)
    {
        if (Globals.gubQuest[ubQuest] == Globals.QUESTINPROGRESS)
        {
            Globals.gubQuest[ubQuest] = Globals.QUESTDONE;

            if (fUpdateHistory)
            {
                ResetHistoryFact(ubQuest, sSectorX, sSectorY);
            }
        }
        else
        {
            Globals.gubQuest[ubQuest] = Globals.QUESTDONE;
        }

        if (ubQuest == QUEST.RESCUE_MARIA)
        {
            // cheap hack to try to prevent Madame Layla from thinking that you are
            // still in the brothel with Maria...
            Globals.gMercProfiles[NPCID.MADAME].bNPCData = 0;
            Globals.gMercProfiles[NPCID.MADAME].bNPCData2 = 0;
        }
    }
}

public enum MERC_TYPE
{
    PLAYER_CHARACTER,
    AIM_MERC,
    MERC,
    NPC,
    EPC,
    NPC_WITH_UNEXTENDABLE_CONTRACT,
    VEHICLE,
}

public enum MAP_ROW
{
    A = 1,
    B = 2,
    C = 3,
    D = 4,
    E = 5,
    F = 6,
    G = 7,
    H = 8,
    I = 9,
    J = 10,
    K = 11,
    L = 12,
    M = 13,
    N = 14,
    O = 15,
    P = 16,
}

public enum QUEST
{
    DELIVER_LETTER = 0,
    FOOD_ROUTE,
    KILL_TERRORISTS,
    KINGPIN_IDOL,
    KINGPIN_MONEY,
    RUNAWAY_JOEY,
    RESCUE_MARIA,
    CHITZENA_IDOL,
    HELD_IN_ALMA,
    INTERROGATION,
    ARMY_FARM, // 10
    FIND_SCIENTIST,
    DELIVER_VIDEO_CAMERA,
    BLOODCATS,
    FIND_HERMIT,
    CREATURES,
    CHOPPER_PILOT,
    ESCORT_SKYRIDER,
    FREE_DYNAMO,
    ESCORT_TOURISTS,
    FREE_CHILDREN,    // 20
    LEATHER_SHOP_DREAM,
    KILL_DEIDRANNA = 25
}
