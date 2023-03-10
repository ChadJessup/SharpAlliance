using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Screens;
using System;
using System.Collections.Generic;
using Veldrid;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class Facts
{
    public const int MAX_FACTS = 65536;
    public const int NUM_FACTS = 500;	//If you increase this number, add entries to the fact text list in QuestText.c

    public static Dictionary<FACT, bool> gubFact = new(); // this has to be updated when we figure out how many facts we have
    private readonly ILogger<Facts> logger;

    public Facts(ILogger<Facts> logger)
    {
        this.logger = logger;
    }

    public static bool CheckNPCInOkayHealth(NPCID ubProfileID)
    {
        SOLDIERTYPE? pSoldier;

        // is the NPC at better than half health?
        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(ubProfileID, false);
        if (pSoldier is not null && pSoldier.bLife > (pSoldier.bLifeMax / 2) && pSoldier.bLife > 30)
        {
            return (true);
        }
        else
        {
            return (false);
        }
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

    public static bool CheckFact(FACT usFact, NPCID ubProfileID)
    {
        TOWNS bTown = (TOWNS)(-1);


        switch (usFact)
        {
            case FACT.DIMITRI_DEAD:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.DIMITRI].bMercStatus == MercStatus.MERC_IS_DEAD);
                break;
            case FACT.CURRENT_SECTOR_IS_SAFE:
                gubFact[FACT.CURRENT_SECTOR_IS_SAFE] = !(((Globals.gTacticalStatus.fEnemyInSector && Quests.NPCHeardShot(ubProfileID)) || Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)));
                break;
            case FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT:
            case FACT.NEW_BOBBYRAY_SHIPMENT_WAITING:
                if (gubFact[FACT.PABLO_PUNISHED_BY_PLAYER] == true && gubFact[FACT.PABLO_RETURNED_GOODS] == false && Globals.gMercProfiles[NPCID.PABLO].bMercStatus != MercStatus.MERC_IS_DEAD)
                {
                    gubFact[FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT] = false;
                    gubFact[FACT.NEW_BOBBYRAY_SHIPMENT_WAITING] = false;
                }
                else
                {
                    if (Quests.CheckForNewShipment()) // if new stuff waiting unseen in Drassen
                    {
                        gubFact[FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT] = false;
                        gubFact[FACT.NEW_BOBBYRAY_SHIPMENT_WAITING] = true;
                    }
                    else if (CountNumberOfBobbyPurchasesThatAreInTransit() > 0) // if stuff in transit
                    {
                        if (gubFact[FACT.PACKAGE_DAMAGED] == true)
                        {
                            gubFact[FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT] = false;
                        }
                        else
                        {
                            gubFact[FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT] = true;
                        }
                        gubFact[FACT.NEW_BOBBYRAY_SHIPMENT_WAITING] = false;
                    }
                    else
                    {
                        gubFact[FACT.BOBBYRAY_SHIPMENT_IN_TRANSIT] = false;
                        gubFact[FACT.NEW_BOBBYRAY_SHIPMENT_WAITING] = false;
                    }
                }
                break;
            case FACT.NPC_WOUNDED:
                gubFact[FACT.NPC_WOUNDED] = Quests.CheckNPCWounded(ubProfileID, false);
                break;
            case FACT.NPC_WOUNDED_BY_PLAYER:
                gubFact[FACT.NPC_WOUNDED_BY_PLAYER] = Quests.CheckNPCWounded(ubProfileID, true);
                break;
            case FACT.IRA_NOT_PRESENT:
                gubFact[FACT.IRA_NOT_PRESENT] = !Quests.CheckNPCWithin(ubProfileID, NPCID.IRA, 10);
                break;
            case FACT.IRA_TALKING:
                gubFact[FACT.IRA_TALKING] = (Globals.gubSrcSoldierProfile == (NPCID)59);
                break;
            case FACT.IRA_UNHIRED_AND_ALIVE:
                if (Globals.gMercProfiles[NPCID.IRA].bMercStatus != MercStatus.MERC_IS_DEAD && Quests.CheckNPCSector(NPCID.IRA, 10, (MAP_ROW)1, 1) && !(Globals.gMercProfiles[NPCID.IRA].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)))
                {
                    gubFact[FACT.IRA_UNHIRED_AND_ALIVE] = true;
                }
                else
                {
                    gubFact[FACT.IRA_UNHIRED_AND_ALIVE] = false;
                }
                break;
            case FACT.NPC_BLEEDING:
                gubFact[FACT.NPC_BLEEDING] = Quests.CheckNPCBleeding(ubProfileID);
                break;
            case FACT.NPC_BLEEDING_BUT_OKAY:
                if (CheckNPCBleeding(ubProfileID) && CheckNPCInOkayHealth(ubProfileID))
                {
                    gubFact[FACT.NPC_BLEEDING_BUT_OKAY] = true;
                }
                else
                {
                    gubFact[FACT.NPC_BLEEDING_BUT_OKAY] = false;
                }
                break;

            case FACT.PLAYER_HAS_HEAD_AND_CARMEN_IN_SAN_MONA:
                gubFact[usFact] = (Quests.CheckNPCSector(NPCID.CARMEN, 5, MAP_ROW.C, 0) && Quests.CheckPlayerHasHead());
                break;

            case FACT.PLAYER_HAS_HEAD_AND_CARMEN_IN_CAMBRIA:
                gubFact[usFact] = (Quests.CheckNPCSector(NPCID.CARMEN, 9, MAP_ROW.G, 0) && Quests.CheckPlayerHasHead());
                break;

            case FACT.PLAYER_HAS_HEAD_AND_CARMEN_IN_DRASSEN:
                gubFact[usFact] = (Quests.CheckNPCSector(NPCID.CARMEN, 13, MAP_ROW.C, 0) && Quests.CheckPlayerHasHead());
                break;

            case FACT.NPC_OWED_MONEY:
                gubFact[FACT.NPC_OWED_MONEY] = (Globals.gMercProfiles[ubProfileID].iBalance < 0);
                break;

            case FACT.FATHER_DRUNK:
                gubFact[FACT.FATHER_DRUNK] = (Globals.gMercProfiles[NPCID.FATHER].bNPCData >= 5);
                break;

            case FACT.MICKY_DRUNK:
                gubFact[FACT.MICKY_DRUNK] = (Globals.gMercProfiles[NPCID.MICKY].bNPCData >= 5);
                break;

            case FACT.BRENDA_IN_STORE_AND_ALIVE:
                // ensure alive
                if (Globals.gMercProfiles[(NPCID)85].bMercStatus == MercStatus.MERC_IS_DEAD)
                {
                    gubFact[FACT.BRENDA_IN_STORE_AND_ALIVE] = false;
                }
                // ensure in a building and nearby
                else if (!(Quests.NPCInRoom((NPCID)85, 47)))
                {
                    gubFact[FACT.BRENDA_IN_STORE_AND_ALIVE] = false;
                }
                else
                {
                    gubFact[FACT.BRENDA_IN_STORE_AND_ALIVE] = CheckNPCWithin(ubProfileID, (NPCID)85, 12);
                }
                break;
            case FACT.BRENDA_DEAD:
                gubFact[FACT.BRENDA_DEAD] = (Globals.gMercProfiles[(NPCID)85].bMercStatus == MercStatus.MERC_IS_DEAD);
                break;
            case FACT.NPC_IS_ENEMY:
                gubFact[FACT.NPC_IS_ENEMY] = Quests.CheckNPCIsEnemy(ubProfileID) || Globals.gMercProfiles[ubProfileID].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_NEEDS_TO_SAY_HOSTILE_QUOTE);
                break;
            /*
		case FACT.SKYRIDER_CLOSE_TO_CHOPPER:
			SetUpHelicopterForPlayer( 13, MAP_ROW.B );
			break;
			*/
            case FACT.SPIKE_AT_DOOR:
                gubFact[FACT.SPIKE_AT_DOOR] = Quests.CheckNPCAt((NPCID)93, 9817);
                break;
            case FACT.WOUNDED_MERCS_NEARBY:
                gubFact[usFact] = (Quests.NumWoundedMercsNearby(ubProfileID) > 0);
                break;
            case FACT.ONE_WOUNDED_MERC_NEARBY:
                gubFact[usFact] = (Quests.NumWoundedMercsNearby(ubProfileID) == 1);
                break;
            case FACT.MULTIPLE_WOUNDED_MERCS_NEARBY:
                gubFact[usFact] = (Quests.NumWoundedMercsNearby(ubProfileID) > 1);
                break;
            case FACT.HANS_AT_SPOT:
                gubFact[usFact] = Quests.CheckNPCAt((NPCID)117, 13523);
                break;
            case FACT.MULTIPLE_MERCS_CLOSE:
                gubFact[usFact] = (Quests.NumMercsNear(ubProfileID, 3) > 1);
                break;
            case FACT.SOME_MERCS_CLOSE:
                gubFact[usFact] = (Quests.NumMercsNear(ubProfileID, 3) > 0);
                break;
            case FACT.MARIA_ESCORTED:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.MARIA);
                break;
            case FACT.JOEY_ESCORTED:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.JOEY);
                break;
            case FACT.ESCORTING_SKYRIDER:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.SKYRIDER);
                break;
            case FACT.MARIA_ESCORTED_AT_LEATHER_SHOP:
                gubFact[usFact] = (Quests.CheckNPCIsEPC(NPCID.MARIA) && (Quests.NPCInRoom(NPCID.MARIA, 2)));
                break;
            case FACT.PC_STRONG_AND_LESS_THAN_3_MALES_PRESENT:
                gubFact[usFact] = (Quests.CheckTalkerStrong() && (Quests.NumMalesPresent(ubProfileID) < 3));
                break;
            case FACT.PC_STRONG_AND_3_PLUS_MALES_PRESENT:
                gubFact[usFact] = (Quests.CheckTalkerStrong() && (Quests.NumMalesPresent(ubProfileID) >= 3));
                break;
            case FACT.FEMALE_SPEAKING_TO_NPC:
                gubFact[usFact] = Quests.CheckTalkerFemale();
                break;
            case FACT.CARMEN_IN_C5:
                gubFact[usFact] = Quests.CheckNPCSector((NPCID)78, 5, MAP_ROW.C, 0);
                break;
            case FACT.JOEY_IN_C5:
                gubFact[usFact] = Quests.CheckNPCSector((NPCID)90, 5, MAP_ROW.C, 0);
                break;
            case FACT.JOEY_NEAR_MARTHA:
                gubFact[usFact] = CheckNPCWithin((NPCID)90, (NPCID)109, 5) && (Quests.CheckGuyVisible(NPCID.MARTHA, NPCID.JOEY) || Quests.CheckGuyVisible(NPCID.JOEY, NPCID.MARTHA));
                break;
            case FACT.JOEY_DEAD:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.JOEY].bMercStatus == MercStatus.MERC_IS_DEAD;
                break;
            case FACT.MERC_NEAR_MARTHA:
                gubFact[usFact] = (NumMercsNear(ubProfileID, 5) > 0);
                break;
            case FACT.REBELS_HATE_PLAYER:
                gubFact[usFact] = (Globals.gTacticalStatus.fCivGroupHostile[REBEL_CIV_GROUP] == CIV_GROUP_HOSTILE);
                break;
            case FACT.CURRENT_SECTOR_G9:
                gubFact[usFact] = (Globals.gWorldSectorX == 9 && Globals.gWorldSectorY == MAP_ROW.G && Globals.gbWorldSectorZ == 0);
                break;
            case FACT.CURRENT_SECTOR_C5:
                gubFact[usFact] = (Globals.gWorldSectorX == 5 && Globals.gWorldSectorY == MAP_ROW.C && Globals.gbWorldSectorZ == 0);
                break;
            case FACT.CURRENT_SECTOR_C13:
                gubFact[usFact] = (Globals.gWorldSectorX == 13 && Globals.gWorldSectorY == MAP_ROW.C && Globals.gbWorldSectorZ == 0);
                break;
            case FACT.CARMEN_HAS_TEN_THOUSAND:
                gubFact[usFact] = (Globals.gMercProfiles[(NPCID)78].uiMoney >= 10000);
                break;
            case FACT.SLAY_IN_SECTOR:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.SLAY].sSectorX == Globals.gWorldSectorX && Globals.gMercProfiles[NPCID.SLAY].sSectorY == Globals.gWorldSectorY && Globals.gMercProfiles[NPCID.SLAY].bSectorZ == Globals.gbWorldSectorZ);
                break;
            case FACT.SLAY_HIRED_AND_WORKED_FOR_48_HOURS:
                gubFact[usFact] = ((Globals.gMercProfiles[NPCID.SLAY].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)) && (Globals.gMercProfiles[NPCID.SLAY].usTotalDaysServed > 1));
                break;
            case FACT.SHANK_IN_SQUAD_BUT_NOT_SPEAKING:
                gubFact[usFact] = ((SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.SHANK, true) != null) && (Globals.gMercProfiles[NPCID.SHANK].ubMiscFlags.HasFlag(ProfileMiscFlags1.PROFILE_MISC_FLAG_RECRUITED)) && (Globals.gpSrcSoldier == null || Globals.gpSrcSoldier.ubProfile != NPCID.SHANK));
                break;
            case FACT.SHANK_NOT_IN_SECTOR:
                gubFact[usFact] = (SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.SHANK, false) == null);
                break;
            case FACT.QUEEN_DEAD:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.QUEEN].bMercStatus == MercStatus.MERC_IS_DEAD);
                break;
            case FACT.MINE_EMPTY:
                gubFact[usFact] = StrategicMines.IsHisMineEmpty(ubProfileID);
                break;
            case FACT.MINE_RUNNING_OUT:
                gubFact[usFact] = StrategicMines.IsHisMineRunningOut(ubProfileID);
                break;
            case FACT.MINE_PRODUCING_BUT_LOYALTY_LOW:
                gubFact[usFact] = StrategicMines.HasHisMineBeenProducingForPlayerForSomeTime(ubProfileID) && IsHisMineDisloyal(ubProfileID);
                break;
            case FACT.CREATURES_IN_MINE:
                gubFact[usFact] = StrategicMines.IsHisMineInfested(ubProfileID);
                break;
            case FACT.PLAYER_LOST_MINE:
                gubFact[usFact] = StrategicMines.IsHisMineLostAndRegained(ubProfileID);
                break;
            case FACT.MINE_AT_FULL_PRODUCTION:
                gubFact[usFact] = StrategicMines.IsHisMineAtMaxProduction(ubProfileID);
                break;
            case FACT.DYNAMO_IN_J9:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.DYNAMO, 9, MAP_ROW.J, 0) && QueenCommand.NumEnemiesInAnySector(9, (MAP_ROW)10, 0) > 0;
                break;
            case FACT.DYNAMO_ALIVE:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.DYNAMO].bMercStatus != MercStatus.MERC_IS_DEAD);
                break;
            case FACT.DYNAMO_SPEAKING_OR_NEARBY:
                gubFact[usFact] = (Globals.gpSrcSoldier != null && (Globals.gpSrcSoldier.ubProfile == NPCID.DYNAMO || (CheckNPCWithin(Globals.gpSrcSoldier.ubProfile, NPCID.DYNAMO, 10) && Quests.CheckGuyVisible(Globals.gpSrcSoldier.ubProfile, NPCID.DYNAMO))));
                break;
            case FACT.JOHN_EPC:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.JOHN);
                break;
            case FACT.MARY_EPC:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.MARY);
                break;
            case FACT.JOHN_AND_MARY_EPCS:
                gubFact[usFact] = Quests.CheckNPCIsEPC(NPCID.JOHN) && Quests.CheckNPCIsEPC(NPCID.MARY);
                break;
            case FACT.MARY_ALIVE:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.MARY].bMercStatus != MercStatus.MERC_IS_DEAD);
                break;
            case FACT.MARY_BLEEDING:
                gubFact[usFact] = Quests.CheckNPCBleeding(NPCID.MARY);
                break;
            case FACT.JOHN_ALIVE:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.JOHN].bMercStatus != MercStatus.MERC_IS_DEAD);
                break;
            case FACT.JOHN_BLEEDING:
                gubFact[usFact] = Quests.CheckNPCBleeding(NPCID.JOHN);
                break;
            case FACT.MARY_DEAD:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.MARY].bMercStatus == MercStatus.MERC_IS_DEAD);
                break;

            case FACT.ANOTHER_FIGHT_POSSIBLE:
                gubFact[usFact] = AnotherFightPossible();
                break;

            case FACT.RECEIVING_INCOME_FROM_DCAC:
                gubFact[usFact] = (
                    (PredictDailyIncomeFromAMine(MINE.DRASSEN) > 0) &&
                    (PredictDailyIncomeFromAMine(MINE.ALMA) > 0) &&
                    (PredictDailyIncomeFromAMine(MINE.CAMBRIA) > 0) &&
                    (PredictDailyIncomeFromAMine(MINE.CHITZENA) > 0));
                break;

            case FACT.PLAYER_BEEN_TO_K4:
                {
                    UNDERGROUND_SECTORINFO? pUnderGroundSector;

                    pUnderGroundSector = FindUnderGroundSector(4, MAP_ROW.K, 1);
                    gubFact[usFact] = pUnderGroundSector.fVisited > 0;
                }
                break;
            case FACT.WARDEN_DEAD:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.WARDEN].bMercStatus == MercStatus.MERC_IS_DEAD);
                break;

            case FACT.PLAYER_PAID_FOR_TWO_IN_BROTHEL:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.MADAME].bNPCData > 1);
                break;

            case FACT.LOYALTY_OKAY:
                bTown = Globals.gMercProfiles[ubProfileID].bTown;
                if ((bTown != TOWNS.BLANK_SECTOR) && Globals.gTownLoyalty[bTown].fStarted && Globals.gfTownUsesLoyalty[bTown])
                {
                    gubFact[usFact] = ((Globals.gTownLoyalty[bTown].ubRating >= Globals.LOYALTY_LOW_THRESHOLD) && (Globals.gTownLoyalty[bTown].ubRating < Globals.LOYALTY_OK_THRESHOLD));
                }
                else
                {
                    gubFact[usFact] = false;
                }
                break;

            case FACT.LOYALTY_LOW:
                bTown = Globals.gMercProfiles[ubProfileID].bTown;
                if ((bTown != TOWNS.BLANK_SECTOR) && Globals.gTownLoyalty[bTown].fStarted && Globals.gfTownUsesLoyalty[bTown])
                {
                    // if Skyrider, ignore low loyalty until he has monologues, and wait at least a day since the latest monologue to avoid a hot/cold attitude
                    if ((ubProfileID == NPCID.SKYRIDER)
                        && ((Globals.guiHelicopterSkyriderTalkState == 0)
                        || ((GameClock.GetWorldTotalMin() - Globals.guiTimeOfLastSkyriderMonologue) < (24 * 60))))
                    {
                        gubFact[usFact] = false;
                    }
                    else
                    {
                        gubFact[usFact] = (Globals.gTownLoyalty[bTown].ubRating < Globals.LOYALTY_LOW_THRESHOLD);
                    }
                }
                else
                {
                    gubFact[usFact] = false;
                }
                break;

            case FACT.LOYALTY_HIGH:
                bTown = Globals.gMercProfiles[ubProfileID].bTown;
                if ((bTown != TOWNS.BLANK_SECTOR) && Globals.gTownLoyalty[bTown].fStarted
                    && Globals.gfTownUsesLoyalty[bTown])
                {
                    gubFact[usFact] = (Globals.gTownLoyalty[Globals.gMercProfiles[ubProfileID].bTown].ubRating >= Globals.LOYALTY_HIGH_THRESHOLD);
                }
                else
                {
                    gubFact[usFact] = false;
                }
                break;

            case FACT.ELGIN_ALIVE:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.DRUGGIST].bMercStatus != MercStatus.MERC_IS_DEAD);
                break;

            case FACT.SPEAKER_AIM_OR_AIM_NEARBY:
                gubFact[usFact] = Globals.gpDestSoldier && AIMMercWithin(Globals.gpDestSoldier.sGridNo, 10);
                break;

            case FACT.MALE_SPEAKING_FEMALE_PRESENT:
                gubFact[usFact] = (!Quests.CheckTalkerFemale() && Quests.FemalePresent(ubProfileID));
                break;

            case FACT.PLAYER_OWNS_2_TOWNS_INCLUDING_OMERTA:
                gubFact[usFact] = ((StrategicTownLoyalty.GetNumberOfWholeTownsUnderControl() == 3) && StrategicTownLoyalty.IsTownUnderCompleteControlByPlayer(TOWNS.OMERTA));
                break;

            case FACT.PLAYER_OWNS_3_TOWNS_INCLUDING_OMERTA:
                gubFact[usFact] = ((StrategicTownLoyalty.GetNumberOfWholeTownsUnderControl() == 5) && StrategicTownLoyalty.IsTownUnderCompleteControlByPlayer(TOWNS.OMERTA));
                break;

            case FACT.PLAYER_OWNS_4_TOWNS_INCLUDING_OMERTA:
                gubFact[usFact] = ((StrategicTownLoyalty.GetNumberOfWholeTownsUnderControl() >= 6) && StrategicTownLoyalty.IsTownUnderCompleteControlByPlayer(TOWNS.OMERTA));
                break;

            case FACT.PLAYER_FOUGHT_THREE_TIMES_TODAY:
                gubFact[usFact] = !BoxerAvailable();
                break;

            case FACT.PLAYER_DOING_POORLY:
                gubFact[usFact] = (Campaign.CurrentPlayerProgressPercentage() < 20);
                break;

            case FACT.PLAYER_DOING_WELL:
                gubFact[usFact] = (Campaign.CurrentPlayerProgressPercentage() > 50);
                break;

            case FACT.PLAYER_DOING_VERY_WELL:
                gubFact[usFact] = (Campaign.CurrentPlayerProgressPercentage() > 80);
                break;

            case FACT.FATHER_DRUNK_AND_SCIFI_OPTION_ON:
                gubFact[usFact] = ((Globals.gMercProfiles[NPCID.FATHER].bNPCData >= 5) && Globals.gGameOptions.SciFi);
                break;

            case FACT.BLOODCAT_QUEST_STARTED_TWO_DAYS_AGO:
                gubFact[usFact] = ((Globals.gubQuest[QUEST.BLOODCATS] != Globals.QUESTNOTSTARTED)
                    && (GameClock.GetWorldTotalMin() - GetTimeQuestWasStarted(QUEST.BLOODCATS) > 2 * Globals.NUM_SEC_IN_DAY / Globals.NUM_SEC_IN_MIN));
                break;

            case FACT.NOTHING_REPAIRED_YET:
                gubFact[usFact] = RepairmanIsFixingItemsButNoneAreDoneYet(ubProfileID);
                break;

            case FACT.NPC_COWERING:
                gubFact[usFact] = Quests.CheckNPCCowering(ubProfileID);
                break;

            case FACT.TOP_AND_BOTTOM_LEVELS_CLEARED:
                gubFact[usFact] = (gubFact[FACT.TOP_LEVEL_CLEARED]
                    & gubFact[FACT.BOTTOM_LEVEL_CLEARED]);
                break;

            case FACT.FIRST_BARTENDER:
                gubFact[usFact] = (Globals.gMercProfiles[ubProfileID].bNPCData == 1
                    || (Globals.gMercProfiles[ubProfileID].bNPCData == 0
                    & Quests.CountBartenders() == 0));
                break;

            case FACT.SECOND_BARTENDER:
                gubFact[usFact] = (Globals.gMercProfiles[ubProfileID].bNPCData == 2
                    || (Globals.gMercProfiles[ubProfileID].bNPCData == 0
                    && Quests.CountBartenders() == 1));
                break;

            case FACT.THIRD_BARTENDER:
                gubFact[usFact] = (Globals.gMercProfiles[ubProfileID].bNPCData == 3
                    || (Globals.gMercProfiles[ubProfileID].bNPCData == 0
                    && Quests.CountBartenders() == 2));
                break;

            case FACT.FOURTH_BARTENDER:
                gubFact[usFact] = (Globals.gMercProfiles[ubProfileID].bNPCData == 4
                    || (Globals.gMercProfiles[ubProfileID].bNPCData == 0
                    && Quests.CountBartenders() == 3));
                break;

            case FACT.NPC_NOT_UNDER_FIRE:
                gubFact[usFact] = !Quests.CheckNPCIsUnderFire(ubProfileID);
                break;

            case FACT.KINGPIN_NOT_IN_OFFICE:
                gubFact[usFact] = !(Globals.gWorldSectorX == 5
                    && Globals.gWorldSectorY == MAP_ROW.D
                    && Quests.NPCInRoomRange(NPCID.KINGPIN, 30, 39));
                // 30 to 39
                break;

            case FACT.DONT_OWE_KINGPIN_MONEY:
                gubFact[usFact] = (Globals.gubQuest[QUEST.KINGPIN_MONEY] != Globals.QUESTINPROGRESS);
                break;

            case FACT.NO_CLUB_FIGHTING_ALLOWED:
                gubFact[usFact] = (Globals.gubQuest[QUEST.KINGPIN_MONEY] == Globals.QUESTINPROGRESS || Globals.gfBoxersResting);// plus other conditions
                break;

            case FACT.MADDOG_IS_SPEAKER:
                gubFact[usFact] = (Globals.gubSrcSoldierProfile == NPCID.MADDOG);
                break;

            case FACT.PC_HAS_CONRADS_RECRUIT_OPINION:
                gubFact[usFact] = (Globals.gpDestSoldier && (CalcDesireToTalk(Globals.gpDestSoldier.ubProfile, Globals.gubSrcSoldierProfile, APPROACH_RECRUIT) >= 50));
                break;

            case FACT.NPC_HOSTILE_OR_PISSED_OFF:
                gubFact[usFact] = Quests.CheckNPCIsEnemy(ubProfileID) || (Globals.gMercProfiles[ubProfileID].ubMiscFlags3.HasFlag(ProfileMiscFlags3.PROFILE_MISC_FLAG3_NPC_PISSED_OFF));
                break;

            case FACT.TONY_IN_BUILDING:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.TONY, 5, MAP_ROW.C, 0) && Quests.NPCInRoom(NPCID.TONY, 50);
                break;

            case FACT.SHANK_SPEAKING:
                gubFact[usFact] = (Globals.gpSrcSoldier is not null && Globals.gpSrcSoldier.ubProfile == NPCID.SHANK);
                break;

            case FACT.ROCKET_RIFLE_EXISTS:
                gubFact[usFact] = HandleItems.ItemTypeExistsAtLocation(10472, Items.ROCKET_RIFLE, 0, null);
                break;

            case FACT.DOREEN_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.DOREEN].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.WALDO_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.WALDO].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.PERKO_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.PERKO].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.TONY_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.TONY].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.VINCE_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.VINCE].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.JENNY_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.JENNY].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.ARNOLD_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.ARNIE].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.I16_BLOODCATS_KILLED:
                gubFact[usFact] = (Globals.SectorInfo[SEC.I16].bBloodCats == 0);
                break;

            case FACT.NPC_BANDAGED_TODAY:
                gubFact[usFact] = (Globals.gMercProfiles[ubProfileID].ubMiscFlags2.HasFlag(ProfileMiscFlags2.PROFILE_MISC_FLAG2_BANDAGED_TODAY));
                break;

            case FACT.PLAYER_IN_SAME_ROOM:
                gubFact[usFact] = Quests.PCInSameRoom(ubProfileID);
                break;

            case FACT.PLAYER_SPOKE_TO_DRASSEN_MINER:
                gubFact[usFact] = StrategicMines.SpokenToHeadMiner(MINE.DRASSEN);
                break;
            case FACT.PLAYER_IN_CONTROLLED_DRASSEN_MINE:
                gubFact[usFact] = (StrategicMines.GetIdOfMineForSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ) == MINE.DRASSEN && !(Globals.strategicMap[Globals.gWorldSectorX + Globals.MAP_WORLD_X * (int)Globals.gWorldSectorY].fEnemyControlled));
                break;
            case FACT.PLAYER_SPOKE_TO_CAMBRIA_MINER:
                gubFact[usFact] = StrategicMines.SpokenToHeadMiner(MINE.CAMBRIA);
                break;
            case FACT.PLAYER_IN_CONTROLLED_CAMBRIA_MINE:
                gubFact[usFact] = (StrategicMines.GetIdOfMineForSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ) == MINE.CAMBRIA && !(Globals.strategicMap[Globals.gWorldSectorX + Globals.MAP_WORLD_X * (int)Globals.gWorldSectorY].fEnemyControlled));
                break;
            case FACT.PLAYER_SPOKE_TO_CHITZENA_MINER:
                gubFact[usFact] = StrategicMines.SpokenToHeadMiner(MINE.CHITZENA);
                break;
            case FACT.PLAYER_IN_CONTROLLED_CHITZENA_MINE:
                gubFact[usFact] = (StrategicMines.GetIdOfMineForSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ) == MINE.CHITZENA && !(Globals.strategicMap[Globals.gWorldSectorX + Globals.MAP_WORLD_X * (int)Globals.gWorldSectorY].fEnemyControlled));
                break;
            case FACT.PLAYER_SPOKE_TO_ALMA_MINER:
                gubFact[usFact] = StrategicMines.SpokenToHeadMiner(MINE.ALMA);
                break;
            case FACT.PLAYER_IN_CONTROLLED_ALMA_MINE:
                gubFact[usFact] = (StrategicMines.GetIdOfMineForSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ) == MINE.ALMA && !(Globals.strategicMap[Globals.gWorldSectorX + Globals.MAP_WORLD_X * (int)Globals.gWorldSectorY].fEnemyControlled));
                break;
            case FACT.PLAYER_SPOKE_TO_GRUMM_MINER:
                gubFact[usFact] = StrategicMines.SpokenToHeadMiner(MINE.GRUMM);
                break;
            case FACT.PLAYER_IN_CONTROLLED_GRUMM_MINE:
                gubFact[usFact] = (StrategicMines.GetIdOfMineForSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ) == MINE.GRUMM && !(Globals.strategicMap[Globals.gWorldSectorX + Globals.MAP_WORLD_X * (int)Globals.gWorldSectorY].fEnemyControlled));
                break;

            case FACT.ENOUGH_LOYALTY_TO_TRAIN_MILITIA:
                gubFact[usFact] = Quests.InTownSectorWithTrainingLoyalty(Globals.gWorldSectorX, Globals.gWorldSectorY);
                break;
            case FACT.WALKER_AT_BAR:
                gubFact[usFact] = (Globals.gMercProfiles[NPCID.FATHER].sSectorX == 13 && Globals.gMercProfiles[NPCID.FATHER].sSectorY == MAP_ROW.C);
                break;

            case FACT.JOEY_ALIVE:
                gubFact[usFact] = Globals.gMercProfiles[NPCID.JOEY].bMercStatus != MercStatus.MERC_IS_DEAD;
                break;

            case FACT.UNPROPOSITIONED_FEMALE_SPEAKING_TO_NPC:
                gubFact[usFact] = Quests.CheckTalkerUnpropositionedFemale();
                break;

            case FACT.NUM_84_AND_85_true:
                gubFact[usFact] = CheckFact((FACT)84, ubProfileID) && CheckFact(FACT.HANS_AT_SPOT, ubProfileID);
                break;

            case FACT.SKYRIDER_IN_B15:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.SKYRIDER, 15, MAP_ROW.B, 0);
                break;

            case FACT.SKYRIDER_IN_C16:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.SKYRIDER, 16, MAP_ROW.C, 0);
                break;
            case FACT.SKYRIDER_IN_E14:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.SKYRIDER, 14, MAP_ROW.E, 0);
                break;
            case FACT.SKYRIDER_IN_D12:
                gubFact[usFact] = Quests.CheckNPCSector(NPCID.SKYRIDER, 12, MAP_ROW.D, 0);
                break;

            case FACT.KINGPIN_IS_ENEMY:
                gubFact[usFact] = (Globals.gTacticalStatus.fCivGroupHostile[CIV_GROUP.KINGPIN_CIV_GROUP] >= CIV_GROUP_WILL_BECOME_HOSTILE);
                break;

            case FACT.DYNAMO_NOT_SPEAKER:
                gubFact[usFact] = !(Globals.gpSrcSoldier != null && (Globals.gpSrcSoldier.ubProfile == NPCID.DYNAMO));
                break;

            case FACT.PABLO_BRIBED:
                gubFact[usFact] = !CheckFact(FACT.PABLOS_BRIBED, ubProfileID);
                break;

            case FACT.VEHICLE_PRESENT:
                gubFact[usFact] = CheckFact(FACT.OK_USE_HUMMER, ubProfileID) && ((SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.PROF_HUMMER, true) != null) || (SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.PROF_ICECREAM, true) != null));
                break;

            case FACT.PLAYER_KILLED_BOXERS:
                gubFact[usFact] = !Boxing.BoxerExists();
                break;

            // chad: fix this
            //            case (FACT)245: // Can dimitri be recruited? should be true if already true, OR if Miguel has been recruited already
            //                gubFact[usFact] = (gubFact[usFact] || FindSoldierByProfileID(MIGUEL, true));
            /*
                    case FACT.:
                        gubFact[usFact] = ;
                        break;
            */

            default:
                break;
        }
        return (gubFact[usFact]);
    }

    public static void SetFactTrue(FACT usFact)
    {
        // This function is here just for control flow purposes (debug breakpoints)
        // and code is more readable that way

        // must intercept when Jake is first trigered to start selling fuel
        if ((usFact == FACT.ESTONI_REFUELLING_POSSIBLE) && (CheckFact(usFact, 0) == false))
        {
            // give him some gas...
            ArmsDealerInit.GuaranteeAtLeastXItemsOfIndex(ARMS_DEALER.JAKE, Items.GAS_CAN, (4 + Globals.Random.Next(3)));
        }

        gubFact[usFact] = true;
    }

    public static void SetFactFalse(FACT usFact)
    {
        gubFact[usFact] = false;
    }

}

public enum FACT
{
    // city liberations
    OMERTA_LIBERATED = 0,
    DRASSEN_LIBERATED,      //  	1
    SANMONA_LIBERATED,      //  	2
    CAMBRIA_LIBERATED,      //  	3
    ALMA_LIBERATED,         //  	4
    GRUMM_LIBERATED,        //  	5
    TIXA_LIBERATED,         //  	6
    CHITZENA_LIBERATED,     //		7
    ESTONI_LIBERATED,       //		8
    BALIME_LIBERATED,       //		9
    ORTA_LIBERATED,         //	    10
    MEDUNA_LIBERATED,       //		11

    // quest stuff
    MIGUEL_FOUND,           //		12
    LETTER_DELIVERED,       //		13
    FOOD_ROUTE_EXISTS,      //		14
    DIMITRI_DEAD,           //		15

    MIGUEL_READ_LETTER = 23,

    // rebels do not trust player
    REBELS_HATE_PLAYER = 25,

    PACOS_KILLED = 29,

    CURRENT_SECTOR_IS_SAFE = 31,
    BOBBYRAY_SHIPMENT_IN_TRANSIT,  //										32
    NEW_BOBBYRAY_SHIPMENT_WAITING, //										33
    REALLY_NEW_BOBBYRAY_SHIPMENT_WAITING,//							34
    LARGE_SIZED_OLD_SHIPMENT_WAITING,//									35
    PLAYER_FOUND_ITEMS_MISSING,//												36
    PABLO_PUNISHED_BY_PLAYER,//													37

    PABLO_RETURNED_GOODS = 39,

    PABLOS_BRIBED = 41,
    ESCORTING_SKYRIDER,//																42
    SKYRIDER_CLOSE_TO_CHOPPER,//													43

    SKYRIDER_USED_IN_MAPSCREEN = 45,
    NPC_OWED_MONEY,//																		46
    NPC_WOUNDED,//																				47
    NPC_WOUNDED_BY_PLAYER,//															48

    IRA_NOT_PRESENT = 50,
    IRA_TALKING,//																				51
    FOOD_QUEST_OVER,//																		52
    PABLOS_STOLE_FROM_LATEST_SHIPMENT,//									53
    LAST_SHIPMENT_CRASHED,//															54
    LAST_SHIPMENT_WENT_TO_WRONG_AIRPORT,//								55
    SHIPMENT_DELAYED_24_HOURS,//													56
    PACKAGE_DAMAGED,//																		57
    PACKAGE_LOST_PERMANENTLY,//													58
    NEXT_PACKAGE_CAN_BE_LOST,//													59
    NEXT_PACKAGE_CAN_BE_DELAYED,//												60
    MEDIUM_SIZED_SHIPMENT_WAITING,//											61
    LARGE_SIZED_SHIPMENT_WAITING,//											62
    DOREEN_HAD_CHANGE_OF_HEART,//												63

    IRA_UNHIRED_AND_ALIVE = 65,

    NPC_BLEEDING = 68,

    NPC_BLEEDING_BUT_OKAY = 70,
    PLAYER_HAS_HEAD_AND_CARMEN_IN_SAN_MONA,//						71
    PLAYER_HAS_HEAD_AND_CARMEN_IN_CAMBRIA,//							72
    PLAYER_HAS_HEAD_AND_CARMEN_IN_DRASSEN,//							73
    FATHER_DRUNK,//																			74
    WOUNDED_MERCS_NEARBY,//															75
    ONE_WOUNDED_MERC_NEARBY,//														76
    MULTIPLE_WOUNDED_MERCS_NEARBY,//											77
    BRENDA_IN_STORE_AND_ALIVE,//													78
    BRENDA_DEAD,//																				79

    NPC_IS_ENEMY = 81,
    PC_STRONG_AND_LESS_THAN_3_MALES_PRESENT,//						82
    PC_STRONG_AND_3_PLUS_MALES_PRESENT,//								83

    HANS_AT_SPOT = 85,
    TONY_NOT_AVAILABLE,//																86
    FEMALE_SPEAKING_TO_NPC,//														87
    PLAYER_USED_BROTHEL,//																88
    CARLA_AVAILABLE,//																		89
    CINDY_AVAILABLE,//																		90
    BAMBI_AVAILABLE,//																		91
    NO_GIRLS_AVAILABLE,//																92
    PLAYER_WAITED_FOR_GIRL,//														93
    PLAYER_PAID_RIGHT_AMOUNT,//													94
    PLAYER_PASSED_GOON,//																95
    MULTIPLE_MERCS_CLOSE,//															96,
    SOME_MERCS_CLOSE,//																	97

    DARREN_EXPECTING_MONEY = 99,
    PC_NEAR,//																						100
    CARMEN_IN_C5,//																			101
    CARMEN_EXPLAINED_DEAL,//															102
    KINGPIN_KNOWS_MONEY_GONE,//													103
    PLAYER_REPAID_KINGPIN,//															104
    FRANK_HAS_BEEN_BRIBED,//															105

    PAST_CLUB_CLOSING_AND_PLAYER_WARNED = 107,
    JOEY_ESCORTED,//																			108
    JOEY_IN_C5,//																				109
    JOEY_NEAR_MARTHA,//																	110
    JOEY_DEAD,//																					111
    MERC_NEAR_MARTHA,//																	112
    SPIKE_AT_DOOR,//																			113

    ANGEL_SOLD_VEST = 115,
    MARIA_ESCORTED,//																		116
    MARIA_ESCORTED_AT_LEATHER_SHOP,//										117
    PLAYER_WANTS_TO_BUY_LEATHER_VEST,//									118
    MARIA_ESCAPE_NOTICED,//															119
    ANGEL_LEFT_DEED,//																		120

    NPC_BANDAGED_TODAY = 122,

    PABLO_WONT_STEAL = 124,
    AGENTS_PREVENTED_SHIPMENT,//													125

    LARGE_AMOUNT_OF_MONEY = 127,
    SMALL_AMOUNT_OF_MONEY,//															128

    LOYALTY_OKAY = 135,
    LOYALTY_LOW,//																				136
    LOYALTY_HIGH,//																			137
    PLAYER_DOING_POORLY,//																138

    CURRENT_SECTOR_G9 = 140,
    CURRENT_SECTOR_C5,//																	141
    CURRENT_SECTOR_C13,//																142
    CARMEN_HAS_TEN_THOUSAND,//														143
    SLAY_HIRED_AND_WORKED_FOR_48_HOURS,//								144

    SLAY_IN_SECTOR = 146,

    VINCE_EXPLAINED_HAS_TO_CHARGE = 148,
    VINCE_EXPECTING_MONEY,//															149
    PLAYER_STOLE_MEDICAL_SUPPLIES,//											150
    PLAYER_STOLE_MEDICAL_SUPPLIES_AGAIN,//								151
    VINCE_RECRUITABLE,//																	152

    ALL_TERRORISTS_KILLED = 156,
    ELGIN_ALIVE,//																				157

    SHANK_IN_SQUAD_BUT_NOT_SPEAKING = 164,

    SHANK_NOT_IN_SECTOR = 167,
    BLOODCAT_QUEST_STARTED_TWO_DAYS_AGO,//								168

    QUEEN_DEAD = 170,

    SPEAKER_AIM_OR_AIM_NEARBY = 171,
    MINE_EMPTY,//																				172
    MINE_RUNNING_OUT,//																	173
    MINE_PRODUCING_BUT_LOYALTY_LOW,//										174
    CREATURES_IN_MINE,//																	175
    PLAYER_LOST_MINE,//																	176
    MINE_AT_FULL_PRODUCTION,//														177
    DYNAMO_SPEAKING_OR_NEARBY,//													178

    CHALICE_STOLEN = 184,
    JOHN_EPC,//																					185
    JOHN_AND_MARY_EPCS,//																186
    MARY_ALIVE,//																				187
    MARY_EPC,//																					188
    MARY_BLEEDING,//																			189
    JOHN_ALIVE,//																				190
    JOHN_BLEEDING,//																			191
    MARY_OR_JOHN_ARRIVED,//															192
    MARY_DEAD,//																					193
    MINERS_PLACED,//																			194
    KROTT_GOT_ANSWER_NO,//																195
    MADLAB_EXPECTING_FIREARM = 197,
    MADLAB_EXPECTING_VIDEO_CAMERA,//											198
    ITEM_POOR_CONDITION,//																199

    ROBOT_READY = 202,
    FIRST_ROBOT_DESTROYED,//															203
    MADLAB_HAS_GOOD_CAMERA,//														204
    ROBOT_READY_SECOND_TIME,//														205
    SECOND_ROBOT_DESTROYED,//														206

    DYNAMO_IN_J9 = 208,
    DYNAMO_ALIVE,//																			209
    ANOTHER_FIGHT_POSSIBLE,//														210
    RECEIVING_INCOME_FROM_DCAC,//												211
    PLAYER_BEEN_TO_K4,//																	212

    WARDEN_DEAD = 214,

    FIRST_BARTENDER = 216,
    SECOND_BARTENDER,//																	217
    THIRD_BARTENDER,//																		218
    FOURTH_BARTENDER,//																	219
    MANNY_IS_BARTENDER,//																220
    NOTHING_REPAIRED_YET,//															221,

    OK_USE_HUMMER = 224,

    DAVE_HAS_GAS = 226,
    VEHICLE_PRESENT,//																		227
    FIRST_BATTLE_WON,//																	228
    ROBOT_RECRUITED_AND_MOVED,//													229
    NO_CLUB_FIGHTING_ALLOWED,//													230
    PLAYER_FOUGHT_THREE_TIMES_TODAY,//										231
    PLAYER_SPOKE_TO_DRASSEN_MINER,//											232
    PLAYER_DOING_WELL,//																	233
    PLAYER_DOING_VERY_WELL,//														234
    FATHER_DRUNK_AND_SCIFI_OPTION_ON,//									235
    MICKY_DRUNK,//																				236
    PLAYER_FORCED_WAY_INTO_BROTHEL,//										237

    PLAYER_PAID_FOR_TWO_IN_BROTHEL = 239,

    PLAYER_OWNS_2_TOWNS_INCLUDING_OMERTA = 242,
    PLAYER_OWNS_3_TOWNS_INCLUDING_OMERTA,//							243
    PLAYER_OWNS_4_TOWNS_INCLUDING_OMERTA,//							244

    MALE_SPEAKING_FEMALE_PRESENT = 248,
    HICKS_MARRIED_PLAYER_MERC,//													249
    MUSEUM_OPEN,//																				250
    BROTHEL_OPEN,//																			251
    CLUB_OPEN,//																					252
    FIRST_BATTLE_FOUGHT,//																253
    FIRST_BATTLE_BEING_FOUGHT,//													254
    KINGPIN_INTRODUCED_SELF,//														255
    KINGPIN_NOT_IN_OFFICE,//															256
    DONT_OWE_KINGPIN_MONEY,//														257
    PC_MARRYING_DARYL_IS_FLO,//													258
    I16_BLOODCATS_KILLED,//															259

    NPC_COWERING = 261,

    TOP_AND_BOTTOM_LEVELS_CLEARED = 264,
    TOP_LEVEL_CLEARED,//																	265
    BOTTOM_LEVEL_CLEARED,//															266
    NEED_TO_SAY_SOMETHING,//															267
    ATTACHED_ITEM_BEFORE,//															268
    SKYRIDER_EVER_ESCORTED,//														269
    NPC_NOT_UNDER_FIRE,//																270
    WILLIS_HEARD_ABOUT_JOEY_RESCUE,//										271
    WILLIS_GIVES_DISCOUNT,//															272
    HILLBILLIES_KILLED,//																273
    KEITH_OUT_OF_BUSINESS, //														274												
    MIKE_AVAILABLE_TO_ARMY,//														275
    KINGPIN_CAN_SEND_ASSASSINS,//												276
    ESTONI_REFUELLING_POSSIBLE,//                        277
    MUSEUM_ALARM_WENT_OFF,//															278

    MADDOG_IS_SPEAKER = 280,

    ANGEL_MENTIONED_DEED = 282,
    IGGY_AVAILABLE_TO_ARMY,//														283
    PC_HAS_CONRADS_RECRUIT_OPINION,//										284

    NPC_HOSTILE_OR_PISSED_OFF = 289,

    TONY_IN_BUILDING = 291,
    SHANK_SPEAKING = 292,
    PABLO_ALIVE,//																				293
    DOREEN_ALIVE,//																			294
    WALDO_ALIVE,//																				295
    PERKO_ALIVE,//																				296
    TONY_ALIVE,//																				297

    VINCE_ALIVE = 299,
    JENNY_ALIVE,//																				300

    ARNOLD_ALIVE = 303,
    ROCKET_RIFLE_EXISTS,//																304,
    TWENTYFOUR_HOURS_SINCE_JOEY_RESCUED,//												305
    TWENTYFOUR_HOURS_SINCE_DOCTOR_TALKED_TO,                                   //	306
    OK_USE_ICECREAM,                                                                   //	307
    KINGPIN_DEAD,//																			308

    KIDS_ARE_FREE = 318,
    PLAYER_IN_SAME_ROOM,//																319

    PLAYER_IN_CONTROLLED_DRASSEN_MINE = 324,
    PLAYER_SPOKE_TO_CAMBRIA_MINER,//											325
    PLAYER_IN_CONTROLLED_CAMBRIA_MINE,//									326
    PLAYER_SPOKE_TO_CHITZENA_MINER,//										327
    PLAYER_IN_CONTROLLED_CHITZENA_MINE,//								328
    PLAYER_SPOKE_TO_ALMA_MINER,//												329
    PLAYER_IN_CONTROLLED_ALMA_MINE,//										330
    PLAYER_SPOKE_TO_GRUMM_MINER,//												331
    PLAYER_IN_CONTROLLED_GRUMM_MINE,//										332

    LARRY_CHANGED = 334,
    PLAYER_KNOWS_ABOUT_BLOODCAT_LAIR,//									335
    HOSPITAL_FREEBIE_DECISION_MADE,//										336
    ENOUGH_LOYALTY_TO_TRAIN_MILITIA,//										337
    WALKER_AT_BAR,//																			338

    JOEY_ALIVE = 340,
    UNPROPOSITIONED_FEMALE_SPEAKING_TO_NPC,//						341
    NUM_84_AND_85_true,//																		342

    KINGPIN_WILL_LEARN_OF_MONEY_GONE = 350,

    SKYRIDER_IN_B15 = 354,
    SKYRIDER_IN_C16,//																		355
    SKYRIDER_IN_E14,//																		356
    SKYRIDER_IN_D12,//																		357
    SKYRIDER_HINT_GIVEN,//																358
    KINGPIN_IS_ENEMY,//																	359
    BRENDA_PATIENCE_TIMER_EXPIRED,//											360

    DYNAMO_NOT_SPEAKER = 362,

    PABLO_BRIBED = 365,

    CONRAD_SHOULD_GO = 367,
    PLAYER_KILLED_BOXERS = 368,
}
