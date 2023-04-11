using System;
using System.Diagnostics;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMines
{
    /* gradual monster infestation concept was ditched, now simply IN PRODUCTION or SHUT DOWN

    // percentage of workers working depends on level of mine infestation
    int gubMonsterMineInfestation[]={
        100,
        99,
        95,
        70,
        30,
        1,
        0,
    };
    */

    void InitializeMines()
    {
        MINE ubMineIndex;
        MINE_STATUS_TYPE pMineStatus;
        int ubMineProductionIncreases;
        MINE ubDepletedMineIndex;
        int ubMinDaysBeforeDepletion = 20;

        // set up initial mine status
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            Globals.gMineStatus[ubMineIndex] = new MINE_STATUS_TYPE
            {
                ubMineType = Globals.gubMineTypes[ubMineIndex],
                uiMaxRemovalRate = Globals.guiMinimumMineProduction[ubMineIndex],
                fEmpty = (Globals.guiMinimumMineProduction[ubMineIndex] == 0) ? true : false,
                fRunningOut = false,
                fWarnedOfRunningOut = false,
                //		pMineStatus.bMonsters = MINES_NO_MONSTERS;
                fShutDown = false,
                fPrevInvadedByMonsters = false,
                fSpokeToHeadMiner = false,
                fMineHasProducedForPlayer = false,
                fQueenRetookProducingMine = false,
                fShutDownIsPermanent = false, // this was gMineStatus - which feels like a bug.
            };
        }

        // randomize the exact size each mine.  The total production is always the same and depends on the game difficulty,
        // but some mines will produce more in one game than another, while others produce less

        // adjust for game difficulty
        switch (Globals.gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
            case DifficultyLevel.Medium:
                ubMineProductionIncreases = 25;
                break;
            case DifficultyLevel.Hard:
                ubMineProductionIncreases = 20;
                break;
            default:
                Debug.Assert(false);
                return;
        }

        while (ubMineProductionIncreases > 0)
        {
            // pick a producing mine at random and increase its production
            do
            {
                ubMineIndex = (MINE)Globals.Random.Next((int)MINE.MAX_NUMBER_OF_MINES);
            } while (Globals.gMineStatus[ubMineIndex].fEmpty);

            // increase mine production by 20% of the base (minimum) rate
            Globals.gMineStatus[ubMineIndex].uiMaxRemovalRate += (Globals.guiMinimumMineProduction[ubMineIndex] / 5);

            ubMineProductionIncreases--;
        }


        // choose which mine will run out of production.  This will never be the Alma mine or an empty mine (San Mona)...
        do
        {
            ubDepletedMineIndex = (MINE)Globals.Random.Next((int)MINE.MAX_NUMBER_OF_MINES);
            // Alma mine can't run out for quest-related reasons (see Ian)
        } while (Globals.gMineStatus[ubDepletedMineIndex].fEmpty || (ubDepletedMineIndex == MINE.ALMA));


        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            pMineStatus = (Globals.gMineStatus[ubMineIndex]);

            if (ubMineIndex == ubDepletedMineIndex)
            {
                if (ubDepletedMineIndex == MINE.DRASSEN)
                {
                    ubMinDaysBeforeDepletion = 20;
                }
                else
                {
                    ubMinDaysBeforeDepletion = 10;
                }

                // the mine that runs out has only enough ore for this many days of full production
                pMineStatus.uiRemainingOreSupply = ubMinDaysBeforeDepletion * (Globals.MINE_PRODUCTION_NUMBER_OF_PERIODS * pMineStatus.uiMaxRemovalRate);

                // ore starts running out when reserves drop to less than 25% of the initial supply
                pMineStatus.uiOreRunningOutPoint = pMineStatus.uiRemainingOreSupply / 4;
            }
            else
            if (!pMineStatus.fEmpty)
            {
                // never runs out...
                pMineStatus.uiRemainingOreSupply = 999999999;      // essentially unlimited
                pMineStatus.uiOreRunningOutPoint = 0;
            }
            else
            {
                // already empty
                pMineStatus.uiRemainingOreSupply = 0;
                pMineStatus.uiOreRunningOutPoint = 0;
            }
        }
    }


    void HourlyMinesUpdate()
    {
        MINE ubMineIndex;
        MINE_STATUS_TYPE pMineStatus;
        HEAD_MINER_STRATEGIC_QUOTE ubQuoteType;


        // check every non-empty mine
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            pMineStatus = (Globals.gMineStatus[ubMineIndex]);

            if (pMineStatus.fEmpty)
            {
                // nobody is working that mine, so who cares
                continue;
            }

            // check if the mine has any monster creatures in it
            if (MineClearOfMonsters(ubMineIndex))
            {
                // if it's shutdown, but not permanently
                if (IsMineShutDown(ubMineIndex) && !pMineStatus.fShutDownIsPermanent)
                {
                    // if we control production in it
                    if (PlayerControlsMine(ubMineIndex))
                    {
                        IssueHeadMinerQuote(ubMineIndex, HEAD_MINER_STRATEGIC_QUOTE.CREATURES_GONE);
                    }

                    //Force the creatures to avoid the mine for a period of time.  This gives the 
                    //player a chance to rest and decide how to deal with the problem.
                    ForceCreaturesToAvoidMineTemporarily(ubMineIndex);

                    // put mine back in service
                    RestartMineProduction(ubMineIndex);
                }
            }
            else    // mine is monster infested
            {
                // 'Der be monsters crawling around in there, lad!!!

                // if it's still producing
                if (!IsMineShutDown(ubMineIndex))
                {
                    // gotta put a stop to that!

                    // if we control production in it
                    if (PlayerControlsMine(ubMineIndex))
                    {
                        // 2 different quotes, depends whether or not it's the first time this has happened
                        if (pMineStatus.fPrevInvadedByMonsters)
                        {
                            ubQuoteType = HEAD_MINER_STRATEGIC_QUOTE.CREATURES_AGAIN;
                        }
                        else
                        {
                            ubQuoteType = HEAD_MINER_STRATEGIC_QUOTE.CREATURES_ATTACK;
                            pMineStatus.fPrevInvadedByMonsters = true;

                            if (Globals.gubQuest[QUEST.CREATURES] == Globals.QUESTNOTSTARTED)
                            {
                                // start it now!
                                Quests.StartQuest(QUEST.CREATURES, Globals.gMineLocation[ubMineIndex].sSectorX, Globals.gMineLocation[ubMineIndex].sSectorY);
                            }
                        }

                        // tell player the good news...
                        IssueHeadMinerQuote(ubMineIndex, ubQuoteType);
                    }

                    // and immediately halt all work at the mine (whether it's ours or the queen's).  This is a temporary shutdown
                    ShutOffMineProduction(ubMineIndex);
                }
            }
        }
    }

    public static int GetTotalLeftInMine(MINE bMineIndex)
    {
        // returns the value of the mine

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        return (Globals.gMineStatus[bMineIndex].uiRemainingOreSupply);
    }


    int GetMaxPeriodicRemovalFromMine(MINE bMineIndex)
    {
        // returns max amount that can be mined in a time period

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return (0);
        }

        return (Globals.gMineStatus[bMineIndex].uiMaxRemovalRate);
    }


    public static int GetMaxDailyRemovalFromMine(MINE bMineIndex)
    {
        int uiAmtExtracted;

        // returns max amount that can be mined in one day 

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return (0);
        }

        uiAmtExtracted = Globals.MINE_PRODUCTION_NUMBER_OF_PERIODS * Globals.gMineStatus[bMineIndex].uiMaxRemovalRate;

        // check if we will take more than there is
        if (uiAmtExtracted > Globals.gMineStatus[bMineIndex].uiRemainingOreSupply)
        {
            // yes, reduce to value of mine
            uiAmtExtracted = Globals.gMineStatus[bMineIndex].uiRemainingOreSupply;
        }

        return (uiAmtExtracted);
    }


    public static TOWNS GetTownAssociatedWithMine(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        return (Globals.gMineLocation[bMineIndex].bAssociatedTown);
    }


    // chad: Not called in origin code? Also seems like it's bugged.
    TOWNS GetMineAssociatedWithThisTown(TOWNS bTownId)
    {
        MINE bCounter = 0;

        // run through list of mines
        for (bCounter = 0; bCounter < MINE.MAX_NUMBER_OF_MINES; bCounter++)
        {
            if (Globals.gMineLocation[bCounter].bAssociatedTown == bTownId)
            {
                // town found, return the fact
                return (Globals.gMineLocation[bCounter].bAssociatedTown);
            }
        }

        // return that no town found..a 0
        return (0);

    }


    public static int ExtractOreFromMine(MINE bMineIndex, int uiAmount)
    {
        // will remove the ore from the mine and return the amount that was removed
        int uiAmountExtracted = 0;
        int uiOreRunningOutPoint = 0;
        int sSectorX;
        MAP_ROW sSectorY;

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return (0);
        }

        // if not capable of extracting anything, bail now
        if (uiAmount == 0)
        {
            return (0);
        }

        // will this exhaust the ore in this mine?
        if (uiAmount >= Globals.gMineStatus[bMineIndex].uiRemainingOreSupply)
        {
            // exhaust remaining ore
            var mineStatus = Globals.gMineStatus[bMineIndex];
            uiAmountExtracted = mineStatus.uiRemainingOreSupply;
            mineStatus.uiRemainingOreSupply = 0;
            mineStatus.uiMaxRemovalRate = 0;
            mineStatus.fEmpty = true;
            mineStatus.fRunningOut = false;

            // tell the strategic AI about this, that mine's and town's value is greatly reduced
            GetMineSector(bMineIndex, out sSectorX, out sSectorY);
            StrategicHandleMineThatRanOut(SECTORINFO.SECTOR(sSectorX, sSectorY));

            History.AddHistoryToPlayersLog(
                HISTORY.MINE_RAN_OUT,
                Globals.gMineLocation[bMineIndex].bAssociatedTown,
                GameClock.GetWorldTotalMin(),
                Globals.gMineLocation[bMineIndex].sSectorX,
                Globals.gMineLocation[bMineIndex].sSectorY);
        }
        else    // still some left after this extraction
        {
            // set amount used, and decrement ore remaining in mine	
            uiAmountExtracted = uiAmount;
            var mineStatus = Globals.gMineStatus[bMineIndex];

            mineStatus.uiRemainingOreSupply -= uiAmount;

            // one of the mines (randomly chosen) will start running out eventually, check if we're there yet
            if (Globals.gMineStatus[bMineIndex].uiRemainingOreSupply < Globals.gMineStatus[bMineIndex].uiOreRunningOutPoint)
            {
                mineStatus.fRunningOut = true;

                // round all fractions UP to the next REMOVAL_RATE_INCREMENT
                mineStatus.uiMaxRemovalRate = (int)(((float)Globals.gMineStatus[bMineIndex].uiRemainingOreSupply / 10) / REMOVAL_RATE_INCREMENT + 0.9999) * REMOVAL_RATE_INCREMENT;


                // if we control it
                if (PlayerControlsMine(bMineIndex))
                {
                    // and haven't yet been warned that it's running out
                    if (!Globals.gMineStatus[bMineIndex].fWarnedOfRunningOut)
                    {
                        mineStatus = Globals.gMineStatus[bMineIndex];

                        // that mine's head miner tells player that the mine is running out
                        IssueHeadMinerQuote(bMineIndex, HEAD_MINER_STRATEGIC_QUOTE.RUNNING_OUT);
                        mineStatus.fWarnedOfRunningOut = true;
                        AddHistoryToPlayersLog(HISTORY.MINE_RUNNING_OUT, Globals.gMineLocation[bMineIndex].bAssociatedTown, GameClock.GetWorldTotalMin(), Globals.gMineLocation[bMineIndex].sSectorX, Globals.gMineLocation[bMineIndex].sSectorY);
                    }
                }
            }
        }

        return (uiAmountExtracted);
    }

    private static int GetAvailableWorkForceForMineForPlayer(MINE bMineIndex)
    {
        // look for available workforce in the town associated with the mine
        int iWorkForceSize = 0;
        TOWNS bTownId = 0;

        // return the loyalty of the town associated with the mine

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return (0);
        }

        // until the player contacts the head miner, production in mine ceases if in player's control
        if (!Globals.gMineStatus[bMineIndex].fSpokeToHeadMiner)
        {
            return (0);
        }


        bTownId = Globals.gMineLocation[bMineIndex].bAssociatedTown;

        Debug.Assert(StrategicMap.GetTownSectorSize(bTownId) != 0);


        // get workforce size (is 0-100 based on local town's loyalty)
        iWorkForceSize = Globals.gTownLoyalty[bTownId].ubRating;

        /*	
            // adjust for monster infestation
            iWorkForceSize *= gubMonsterMineInfestation[ Globals.gMineStatus[ bMineIndex ].bMonsters ];
            iWorkForceSize /= 100;
        */

        // now adjust for town size.. the number of sectors you control
        iWorkForceSize *= StrategicTownLoyalty.GetTownSectorsUnderControl(bTownId);
        iWorkForceSize /= StrategicMap.GetTownSectorSize(bTownId);

        return (iWorkForceSize);
    }

    public static int GetAvailableWorkForceForMineForEnemy(MINE bMineIndex)
    {
        // look for available workforce in the town associated with the mine
        int iWorkForceSize = 0;
        TOWNS bTownId = 0;

        // return the loyalty of the town associated with the mine

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return (0);
        }

        bTownId = Globals.gMineLocation[bMineIndex].bAssociatedTown;

        if (StrategicMap.GetTownSectorSize(bTownId) == 0)
        {
            return 0;
        }

        // get workforce size (is 0-100 based on REVERSE of local town's loyalty)
        iWorkForceSize = 100 - Globals.gTownLoyalty[bTownId].ubRating;

        /*	
            // adjust for monster infestation
            iWorkForceSize *= gubMonsterMineInfestation[ Globals.gMineStatus[ bMineIndex ].bMonsters ];
            iWorkForceSize /= 100;
        */

        // now adjust for town size.. the number of sectors you control
        iWorkForceSize *= (StrategicMap.GetTownSectorSize(bTownId) - StrategicTownLoyalty.GetTownSectorsUnderControl(bTownId));
        iWorkForceSize /= StrategicMap.GetTownSectorSize(bTownId);

        return (iWorkForceSize);
    }

    private static int GetCurrentWorkRateOfMineForPlayer(MINE bMineIndex)
    {
        int iWorkRate = 0;

        // multiply maximum possible removal rate by the percentage of workforce currently working
        iWorkRate = (Globals.gMineStatus[bMineIndex].uiMaxRemovalRate * GetAvailableWorkForceForMineForPlayer(bMineIndex)) / 100;

        return (iWorkRate);
    }

    public static int GetCurrentWorkRateOfMineForEnemy(MINE bMineIndex)
    {
        int iWorkRate = 0;

        // multiply maximum possible removal rate by the percentage of workforce currently working
        iWorkRate = (Globals.gMineStatus[bMineIndex].uiMaxRemovalRate * GetAvailableWorkForceForMineForEnemy(bMineIndex)) / 100;

        return (iWorkRate);
    }

    public static int MineAMine(MINE bMineIndex)
    {
        // will extract ore based on available workforce, and increment players income based on amount
        MINE_TYPE bMineType = 0;
        int iAmtExtracted = 0;


        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // is mine is empty
        if (Globals.gMineStatus[bMineIndex].fEmpty)
        {
            return 0;
        }

        // if mine is shut down
        if (Globals.gMineStatus[bMineIndex].fShutDown)
        {
            return 0;
        }


        // who controls the PRODUCTION in the mine ?  (Queen receives production unless player has spoken to the head miner)
        if (PlayerControlsMine(bMineIndex))
        {
            // player controlled
            iAmtExtracted = ExtractOreFromMine(bMineIndex, GetCurrentWorkRateOfMineForPlayer(bMineIndex));

            // SHOW ME THE MONEY!!!!
            if (iAmtExtracted > 0)
            {
                // debug message
                //			Messages.ScreenMsg( MSG_FONT_RED, MSG_DEBUG, "%s - Mine income from %s = $%d", gswzWorldTimeStr, pTownNames[ GetTownAssociatedWithMine( bMineIndex ) ], iAmtExtracted );

                // check type of mine
                bMineType = Globals.gMineStatus[bMineIndex].ubMineType;

                // if this is the first time this mine has produced income for the player in the game
                if (!Globals.gMineStatus[bMineIndex].fMineHasProducedForPlayer)
                {
                    var mineStatus = Globals.gMineStatus[bMineIndex];
                    // remember that we've earned income from this mine during the game
                    mineStatus.fMineHasProducedForPlayer = true;
                    // and when we started to do so...
                    mineStatus.uiTimePlayerProductionStarted = GameClock.GetWorldTotalMin();
                }
            }
        }
        else    // queen controlled
        {
            // we didn't want mines to run out without player ever even going to them, so now the queen doesn't reduce the
            // amount remaining until the mine has produced for the player first (so she'd have to capture it).
            if (Globals.gMineStatus[bMineIndex].fMineHasProducedForPlayer)
            {
                // don't actually give her money, just take production away
                iAmtExtracted = ExtractOreFromMine(bMineIndex, GetCurrentWorkRateOfMineForEnemy(bMineIndex));
            }
        }


        return iAmtExtracted;
    }


    void PostEventsForMineProduction()
    {
        int ubShift;

        for (ubShift = 0; ubShift < Globals.MINE_PRODUCTION_NUMBER_OF_PERIODS; ubShift++)
        {
            GameEvents.AddStrategicEvent(EVENT.HANDLE_MINE_INCOME, (uint)(GameClock.GetWorldDayInMinutes() + Globals.MINE_PRODUCTION_START_TIME + (ubShift * Globals.MINE_PRODUCTION_PERIOD)), 0);
        }
    }


    public static void HandleIncomeFromMines()
    {
        int iIncome = 0;
        MINE bCounter = 0;

        // mine each mine, check if we own it and such
        for (bCounter = 0; bCounter < MINE.MAX_NUMBER_OF_MINES; bCounter++)
        {
            // mine this mine
            iIncome += MineAMine(bCounter);
        }
        if (iIncome > 0)
        {
            AddTransactionToPlayersBook(DEPOSIT_FROM_SILVER_MINE, 0, GameClock.GetWorldTotalMin(), iIncome);
        }
    }


    public static int PredictDailyIncomeFromAMine(MINE bMineIndex)
    {
        // predict income from this mine, estimate assumes mining situation will not change during next 4 income periods
        // (miner loyalty, % town controlled, monster infestation level, and current max removal rate may all in fact change)
        int uiAmtExtracted = 0;

        if (PlayerControlsMine(bMineIndex))
        {
            // get daily income for this mine (regardless of what time of day it currently is)
            uiAmtExtracted = Globals.MINE_PRODUCTION_NUMBER_OF_PERIODS * GetCurrentWorkRateOfMineForPlayer(bMineIndex);

            // check if we will take more than there is
            if (uiAmtExtracted > Globals.gMineStatus[bMineIndex].uiRemainingOreSupply)
            {
                // yes reduce to value of mine
                uiAmtExtracted = Globals.gMineStatus[bMineIndex].uiRemainingOreSupply;
            }
        }

        return (uiAmtExtracted);
    }


    public static int PredictIncomeFromPlayerMines()
    {
        int iTotal = 0;
        MINE bCounter = 0;

        for (bCounter = 0; bCounter < MINE.MAX_NUMBER_OF_MINES; bCounter++)
        {
            // add up the total
            iTotal += PredictDailyIncomeFromAMine(bCounter);
        }

        return (iTotal);
    }


    public static int CalcMaxPlayerIncomeFromMines()
    {
        int iTotal = 0;
        MINE bCounter = 0;

        // calculate how much player could make daily if he owned all mines with 100% control and 100% loyalty
        for (bCounter = 0; bCounter < MINE.MAX_NUMBER_OF_MINES; bCounter++)
        {
            // add up the total
            iTotal += (Globals.MINE_PRODUCTION_NUMBER_OF_PERIODS * Globals.gMineStatus[bCounter].uiMaxRemovalRate);
        }

        return (iTotal);
    }


    // get index of this mine, return -1 if no mine found
    static MINE GetMineIndexForSector(int sX, MAP_ROW sY)
    {
        MINE ubMineIndex = 0;


        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if ((Globals.gMineLocation[ubMineIndex].sSectorX == sX) && (Globals.gMineLocation[ubMineIndex].sSectorY == sY))
            {
                // yep mine here
                return (ubMineIndex);
            }
        }

        return (MINE)(-1);
    }

    public static void GetMineSector(MINE ubMineIndex, out int psX, out MAP_ROW psY)
    {
        Debug.Assert((ubMineIndex >= 0) && (ubMineIndex < MINE.MAX_NUMBER_OF_MINES));

        psX = Globals.gMineLocation[ubMineIndex].sSectorX;
        psY = Globals.gMineLocation[ubMineIndex].sSectorY;
    }


    // get the index of the mine associated with this town
    public static MINE GetMineIndexForTown(TOWNS bTownId)
    {
        MINE ubMineIndex = 0;

        // given town id, send sector value of mine, a 0 means no mine for this town
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if (Globals.gMineLocation[ubMineIndex].bAssociatedTown == bTownId)
            {
                return (ubMineIndex);
            }
        }

        return (MINE)(-1);
    }


    // get the sector value for the mine associated with this town
    public static int GetMineSectorForTown(TOWNS bTownId)
    {
        MINE ubMineIndex;
        int sMineSector = -1;

        // given town id, send sector value of mine, a 0 means no mine for this town
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if (Globals.gMineLocation[ubMineIndex].bAssociatedTown == bTownId)
            {
                sMineSector = Globals.gMineLocation[ubMineIndex].sSectorX + ((int)Globals.gMineLocation[ubMineIndex].sSectorY * Globals.MAP_WORLD_X);
                break;
            }
        }

        // -1 returned if the town doesn't have a mine
        return (sMineSector);
    }


    bool IsThereAMineInThisSector(int sX, MAP_ROW sY)
    {
        MINE ubMineIndex;

        // run through the list...if a mine here, great
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if ((Globals.gMineLocation[ubMineIndex].sSectorX == sX)
                && (Globals.gMineLocation[ubMineIndex].sSectorY == sY))
            {
                return (true);
            }
        }
        return (false);
    }


    public static bool PlayerControlsMine(MINE bMineIndex)
    {
        // a value of true is from the enemy's point of view
        if (Globals.strategicMap[(Globals.gMineLocation[bMineIndex].sSectorX) + (Globals.MAP_WORLD_X * ((int)Globals.gMineLocation[bMineIndex].sSectorY))].fEnemyControlled == true)
        {
            return (false);
        }
        else
        {
            // player only controls the actual mine after he has made arrangements to do so with the head miner there
            if (Globals.gMineStatus[bMineIndex].fSpokeToHeadMiner)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
    }


    bool SaveMineStatusToSaveGameFile()//Stream hFile)
    {
        int uiNumBytesWritten;

        //Save the MineStatus
        // FileWrite(hFile, Globals.gMineStatus, sizeof(MINE_STATUS_TYPE) * MAX_NUMBER_OF_MINES, out uiNumBytesWritten);
        //if (uiNumBytesWritten != sizeof(MINE_STATUS_TYPE) * MAX_NUMBER_OF_MINES)
        {
            return (false);
        }

        return (true);
    }


    bool LoadMineStatusFromSavedGameFile()//Stream hFile)
    {
        int uiNumBytesRead;

        //Load the MineStatus
        //FileRead(hFile, Globals.gMineStatus, sizeof(MINE_STATUS_TYPE) * MAX_NUMBER_OF_MINES, &uiNumBytesRead);
        //if (uiNumBytesRead != sizeof(MINE_STATUS_TYPE) * MAX_NUMBER_OF_MINES)
        {
            return (false);
        }

        return (true);
    }


    public static void ShutOffMineProduction(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        if (!Globals.gMineStatus[bMineIndex].fShutDown)
        {
            var mineStatus = Globals.gMineStatus[bMineIndex];
            mineStatus.fShutDown = true;
            History.AddHistoryToPlayersLog(HISTORY.MINE_SHUTDOWN, Globals.gMineLocation[bMineIndex].bAssociatedTown, GameClock.GetWorldTotalMin(), Globals.gMineLocation[bMineIndex].sSectorX, Globals.gMineLocation[bMineIndex].sSectorY);
        }
    }


    void RestartMineProduction(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        if (!Globals.gMineStatus[bMineIndex].fShutDownIsPermanent)
        {
            if (Globals.gMineStatus[bMineIndex].fShutDown)
            {
                var mineStatus = Globals.gMineStatus[bMineIndex];
                mineStatus.fShutDown = false;
                History.AddHistoryToPlayersLog(HISTORY.MINE_REOPENED, Globals.gMineLocation[bMineIndex].bAssociatedTown, GameClock.GetWorldTotalMin(), Globals.gMineLocation[bMineIndex].sSectorX, Globals.gMineLocation[bMineIndex].sSectorY);
            }
        }
    }


    void MineShutdownIsPermanent(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        var mineStatus = Globals.gMineStatus[bMineIndex];
        mineStatus.fShutDownIsPermanent = true;
    }


    public static bool IsMineShutDown(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        return (Globals.gMineStatus[bMineIndex].fShutDown);
    }


    public static int GetHeadMinerIndexForMine(MINE bMineIndex)
    {
        int ubMinerIndex = 0;
        NPCID usProfileId = 0;

        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        // loop through all head miners, checking which town they're associated with, looking for one that matches this mine
        for (ubMinerIndex = 0; ubMinerIndex < (int)MINER.NUM_HEAD_MINERS; ubMinerIndex++)
        {
            usProfileId = Globals.gHeadMinerData[ubMinerIndex].usProfileId;

            if (Globals.gMercProfiles[usProfileId].bTown == Globals.gMineLocation[bMineIndex].bAssociatedTown)
            {
                return (ubMinerIndex);
            }
        }

        // not found - yack!
        Debug.Assert(false);
        return (0);
    }


    NPCID GetHeadMinerProfileIdForMine(MINE bMineIndex)
    {
        return (Globals.gHeadMinerData[GetHeadMinerIndexForMine(bMineIndex)].usProfileId);
    }

    public static void IssueHeadMinerQuote(MINE bMineIndex, HEAD_MINER_STRATEGIC_QUOTE ubQuoteType)
    {
        int ubHeadMinerIndex = 0;
        NPCID usHeadMinerProfileId = 0;
        int bQuoteNum = 0;
        int ubFaceIndex = 0;
        bool fForceMapscreen = false;
        int sXPos, sYPos;


        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));
        Debug.Assert(ubQuoteType < HEAD_MINER_STRATEGIC_QUOTE.NUM_HEAD_MINER_STRATEGIC_QUOTES);
        //Debug.Assert(CheckFact(FACT.MINERS_PLACED, 0));

        ubHeadMinerIndex = GetHeadMinerIndexForMine(bMineIndex);
        usHeadMinerProfileId = Globals.gHeadMinerData[ubHeadMinerIndex].usProfileId;

        // make sure the miner ain't dead
        if (Globals.gMercProfiles[usHeadMinerProfileId].bLife < Globals.OKLIFE)
        {
            // debug message
            Messages.ScreenMsg(MSG_FONT_RED, Globals.MSG_DEBUG, "Head Miner #%s can't talk (quote #%d)", Globals.gMercProfiles[usHeadMinerProfileId].zNickname, ubQuoteType);
            return;
        }

        bQuoteNum = Globals.gHeadMinerData[ubHeadMinerIndex].bQuoteNum[(int)ubQuoteType];
        Debug.Assert(bQuoteNum != -1);

//        ubFaceIndex = (int)uiExternalStaticNPCFaces[Globals.gHeadMinerData[ubHeadMinerIndex].ubExternalFace];

        // transition to mapscreen is not necessary for "creatures gone" quote - player is IN that mine, so he'll know
        if (ubQuoteType != HEAD_MINER_STRATEGIC_QUOTE.CREATURES_GONE)
        {
            fForceMapscreen = true;
        }


        // decide where the miner's face and text box should be positioned in order to not obscure the mine he's in as it flashes
        switch (bMineIndex)
        {
            case MINE.GRUMM:
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = Globals.DEFAULT_EXTERN_PANEL_Y_POS;
                break;
            case MINE.CAMBRIA:
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = Globals.DEFAULT_EXTERN_PANEL_Y_POS;
                break;
            case MINE.ALMA:
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = Globals.DEFAULT_EXTERN_PANEL_Y_POS;
                break;
            case MINE.DRASSEN:
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = 135;
                break;
            case MINE.CHITZENA:
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = 117;
                break;

            // there's no head miner in San Mona, this is an error!
            case MINE.SAN_MONA:
            default:
                Debug.Assert(false);
                sXPos = Globals.DEFAULT_EXTERN_PANEL_X_POS;
                sYPos = Globals.DEFAULT_EXTERN_PANEL_Y_POS;
                break;
        }

        SetExternMapscreenSpeechPanelXY(sXPos, sYPos);

        // cause this quote to come up for this profile id and an indicator to flash over the mine sector
        HandleMinerEvent(Globals.gHeadMinerData[ubHeadMinerIndex].ubExternalFace, Globals.gMineLocation[bMineIndex].sSectorX, Globals.gMineLocation[bMineIndex].sSectorY, (int)bQuoteNum, fForceMapscreen);

        // stop time compression with any miner quote - these are important events.
        GameClock.StopTimeCompression();
    }

    public static MINE GetHeadMinersMineIndex(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        // find which mine this guy represents
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if (Globals.gMineLocation[ubMineIndex].bAssociatedTown == Globals.gMercProfiles[ubMinerProfileId].bTown)
            {
                return (ubMineIndex);
            }
        }

        // not found!  Illegal profile id receieved or something is very wrong
        Debug.Assert(false);
        return (0);
    }


    public static void PlayerSpokeToHeadMiner(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        var mineStatus = Globals.gMineStatus[ubMineIndex];

        // if this is our first time set a history fact 
        if (mineStatus.fSpokeToHeadMiner == false)
        {
            History.AddHistoryToPlayersLog(HISTORY.TALKED_TO_MINER, gMineLocation[ubMineIndex].bAssociatedTown, GameClock.GetWorldTotalMin(), Globals.gMineLocation[ubMineIndex].sSectorX, Globals.gMineLocation[ubMineIndex].sSectorY);
            mineStatus.fSpokeToHeadMiner = true;
        }
    }


    public static bool IsHisMineRunningOut(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);
        return (Globals.gMineStatus[ubMineIndex].fRunningOut);
    }

    public static bool IsHisMineEmpty(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);
        return (Globals.gMineStatus[ubMineIndex].fEmpty);
    }

    public static bool IsHisMineDisloyal(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        if (Globals.gTownLoyalty[Globals.gMineLocation[ubMineIndex].bAssociatedTown].ubRating < Globals.LOW_MINE_LOYALTY_THRESHOLD)
        {
            // pretty disloyal
            return (true);
        }
        else
        {
            // pretty loyal
            return (false);
        }
    }

    public static bool IsHisMineInfested(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);
        return false;//chad (!MineClearOfMonsters(ubMineIndex));
    }

    public static bool IsHisMineLostAndRegained(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        if (PlayerControlsMine(ubMineIndex) && Globals.gMineStatus[ubMineIndex].fQueenRetookProducingMine)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    void ResetQueenRetookMine(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        Globals.gMineStatus[ubMineIndex].fQueenRetookProducingMine = false;
    }

    public static bool IsHisMineAtMaxProduction(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        if (GetAvailableWorkForceForMineForPlayer(ubMineIndex) == 100)
        {
            // loyalty is 100% and control is 100%
            return (true);
        }
        else
        {
            // something not quite perfect yet
            return (false);
        }
    }


    void QueenHasRegainedMineSector(MINE bMineIndex)
    {
        Debug.Assert((bMineIndex >= 0) && (bMineIndex < MINE.MAX_NUMBER_OF_MINES));

        if (Globals.gMineStatus[bMineIndex].fMineHasProducedForPlayer)
        {
            Globals.gMineStatus[bMineIndex].fQueenRetookProducingMine = true;
        }
    }


    public static bool HasAnyMineBeenAttackedByMonsters()
    {
        MINE ubMineIndex;

        // find which mine this guy represents
        for (ubMineIndex = 0; ubMineIndex < MINE.MAX_NUMBER_OF_MINES; ubMineIndex++)
        {
            if (!CreatureSpreading.MineClearOfMonsters(ubMineIndex) || Globals.gMineStatus[ubMineIndex].fPrevInvadedByMonsters)
            {
                return (true);
            }
        }

        return (false);
    }


    void PlayerAttackedHeadMiner(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;
        TOWNS bTownId;

        // get the index of his mine
        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);


        // if it's the first time he's been attacked
        if (Globals.gMineStatus[ubMineIndex].fAttackedHeadMiner == false)
        {
            // shut off production at his mine (Permanently!)
            ShutOffMineProduction(ubMineIndex);
            MineShutdownIsPermanent(ubMineIndex);

            // get the index of his town
            bTownId = GetTownAssociatedWithMine(ubMineIndex);
            // penalize associated town's loyalty
            DecrementTownLoyalty(bTownId, LOYALTY_PENALTY_HEAD_MINER_ATTACKED);

            // don't allow this more than once
            Globals.gMineStatus[ubMineIndex].fAttackedHeadMiner = true;
        }
    }


    public static bool HasHisMineBeenProducingForPlayerForSomeTime(NPCID ubMinerProfileId)
    {
        MINE ubMineIndex;

        ubMineIndex = GetHeadMinersMineIndex(ubMinerProfileId);

        if (Globals.gMineStatus[ubMineIndex].fMineHasProducedForPlayer &&
                ((GameClock.GetWorldTotalMin() - Globals.gMineStatus[ubMineIndex].uiTimePlayerProductionStarted) >= (24 * 60)))
        {
            return (true);
        }

        return (false);
    }

    // gte the id of the mine for this sector x,y,z...-1 is invalid
    public static MINE GetIdOfMineForSector(int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        MINE bMineIndex = (MINE)(-1);
        SEC sSectorValue;


        // are we even on the right level?
        if ((bSectorZ < 0) && (bSectorZ > 2))
        {
            // nope
            return (MINE)(-1);
        }

        // now get the sectorvalue
        sSectorValue = SECTORINFO.SECTOR(sSectorX, sSectorY);

        // support surface
        if (bSectorZ == 0)
        {
            bMineIndex = GetMineIndexForSector(sSectorX, sSectorY);
        }
        // handle for first level
        else if (bSectorZ == 1)
        {
            switch (sSectorValue)
            {
                // grumm
                case (SEC.H3):
                case (SEC.I3):
                    bMineIndex = MINE.GRUMM;
                    break;
                // cambria
                case (SEC.H8):
                case (SEC.H9):
                    bMineIndex = MINE.CAMBRIA;
                    break;
                // alma
                case (SEC.I14):
                case (SEC.J14):
                    bMineIndex = MINE.ALMA;
                    break;
                // drassen
                case (SEC.D13):
                case (SEC.E13):
                    bMineIndex = MINE.DRASSEN;
                    break;
                // chitzena
                case (SEC.B2):
                    bMineIndex = MINE.CHITZENA;
                    break;
                // san mona
                case (SEC.D4):
                case (SEC.D5):
                    bMineIndex = MINE.SAN_MONA;
                    break;
            }
        }
        else
        {
            // level 2
            switch (sSectorValue)
            {
                case (SEC.I3):
                case (SEC.H3):
                case (SEC.H4):
                    bMineIndex = MINE.GRUMM;
                    break;
            }
        }

        return (bMineIndex);
    }

    // use this for miner (civilian) quotes when *underground* in a mine
    bool PlayerForgotToTakeOverMine(MINE ubMineIndex)
    {
        MINE_STATUS_TYPE pMineStatus;

        Debug.Assert((ubMineIndex >= 0) && (ubMineIndex < MINE.MAX_NUMBER_OF_MINES));

        pMineStatus = (Globals.gMineStatus[ubMineIndex]);

        // mine surface sector is player controlled
        // mine not empty
        // player hasn't spoken to the head miner, but hasn't attacked him either
        // miner is alive
        if ((Globals.strategicMap[(Globals.gMineLocation[ubMineIndex].sSectorX) + (Globals.MAP_WORLD_X * ((int)Globals.gMineLocation[ubMineIndex].sSectorY))].fEnemyControlled == false) &&
             (!pMineStatus.fEmpty) &&
             (!pMineStatus.fSpokeToHeadMiner) &&
             (!pMineStatus.fAttackedHeadMiner) &&
             (Globals.gMercProfiles[GetHeadMinerProfileIdForMine(ubMineIndex)].IsAlive))
        {
            return (true);
        }

        return (false);
    }



    // use this to determine whether or not to place miners into a underground mine level
    bool AreThereMinersInsideThisMine(MINE ubMineIndex)
    {
        MINE_STATUS_TYPE pMineStatus;


        Debug.Assert((ubMineIndex >= 0) && (ubMineIndex < MINE.MAX_NUMBER_OF_MINES));

        pMineStatus = (Globals.gMineStatus[ubMineIndex]);

        // mine not empty
        // mine clear of any monsters
        // the "shutdown permanently" flag is only used for the player never receiving the income - miners will keep mining
        if ((!pMineStatus.fEmpty) && CreatureSpreading.MineClearOfMonsters(ubMineIndex))
        {
            return (true);
        }

        return (false);
    }

    // returns whether or not we've spoken to the head miner of a particular mine
    public static bool SpokenToHeadMiner(MINE ubMineIndex)
    {
        return (Globals.gMineStatus[ubMineIndex].fSpokeToHeadMiner);
    }

}

public enum MINE
{
    SAN_MONA = 0,
    DRASSEN,
    ALMA,
    CAMBRIA,
    CHITZENA,
    GRUMM,

    MAX_NUMBER_OF_MINES,
}

public enum MINER
{
    FRED = 0,
    MATT,
    OSWALD,
    CALVIN,
    CARL,
    NUM_HEAD_MINERS,
}

// the strategic mine structures
public struct MINE_LOCATION_TYPE
{
    public MINE_LOCATION_TYPE(int sectorX, MAP_ROW sectorY, TOWNS town)
    {
        this.sSectorX = sectorX;
        this.sSectorY = sectorY;
        this.bAssociatedTown = town;
    }

    public int sSectorX;                     // x value of sector mine is in
    public MAP_ROW sSectorY;                     // y value of sector mine is in
    public TOWNS bAssociatedTown;			// associated town of this mine
}

// different types of mines
public enum MINE_TYPE
{
    SILVER_MINE = 0,
    GOLD_MINE,
    NUM_MINE_TYPES,
}

public class MINE_STATUS_TYPE
{
    public MINE_TYPE ubMineType;                               // type of mine (silver or gold)
    byte[] filler1;// [3];
    public int uiMaxRemovalRate;                    // fastest rate we can move ore from this mine in period

    public int uiRemainingOreSupply;            // the total value left to this mine (-1 means unlimited)
    public int uiOreRunningOutPoint;            // when supply drop below this, workers tell player the mine is running out of ore

    public bool fEmpty;                                     // whether no longer minable
    public bool fRunningOut;                                // whether mine is beginning to run out
    public bool fWarnedOfRunningOut;                // whether mine foreman has already told player the mine's running out
    public bool fShutDownIsPermanent;           // means will never produce again in the game (head miner was attacked & died/quit)
    public bool fShutDown;                                  // true means mine production has been shut off
    public bool fPrevInvadedByMonsters;     // whether or not mine has been previously invaded by monsters
    public bool fSpokeToHeadMiner;                  // player doesn't receive income from mine without speaking to the head miner first
    public bool fMineHasProducedForPlayer;  // player has earned income from this mine at least once

    public bool fQueenRetookProducingMine;  // whether or not queen ever retook a mine after a player had produced from it
    public bool fAttackedHeadMiner;             // player has attacked the head miner, shutting down mine & decreasing loyalty
    public int usValidDayCreaturesCanInfest; //Creatures will be permitted to spread if the game day is greater than this value.
    public uint uiTimePlayerProductionStarted;       // time in minutes when 'fMineHasProducedForPlayer' was first set

    byte[] filler;// [11];					// reserved for expansion
}

public record HEAD_MINER_TYPE(
    NPCID usProfileId,
    int[] bQuoteNum,// = new int[(int)HEAD_MINER_STRATEGIC_QUOTE.NUM_HEAD_MINER_STRATEGIC_QUOTES];
    ExternalFaces ubExternalFace);

// head miner quote types
public enum HEAD_MINER_STRATEGIC_QUOTE
{
    RUNNING_OUT = 0,
    CREATURES_ATTACK,
    CREATURES_GONE,
    CREATURES_AGAIN,

    NUM_HEAD_MINER_STRATEGIC_QUOTES
};
