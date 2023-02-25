using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class TeamTurns
{
    int InterruptOnlyGuynum = Globals.NOBODY;
    bool InterruptsAllowed = true;

    void ClearIntList()
    {
        memset(&gubOutOfTurnOrder, 0, MAXMERCS);
        gubOutOfTurnOrder[0] = END_OF_INTERRUPTS;
        gubOutOfTurnPersons = 0;
    }

    public static int LATEST_INTERRUPT_GUY() => (Globals.gubOutOfTurnOrder[Globals.gubOutOfTurnPersons]);
    public static int REMOVE_LATEST_INTERRUPT_GUY() => (DeleteFromIntList((int)(Globals.gubOutOfTurnPersons), true));
    public static bool INTERRUPTS_OVER() => (Globals.gubOutOfTurnPersons == 1);

    bool BloodcatsPresent()
    {
        int iLoop;
        SOLDIERTYPE? pSoldier;

        if (Globals.gTacticalStatus.Team[CREATURE_TEAM].bTeamActive == false)
        {
            return (false);
        }

        for (iLoop = Globals.gTacticalStatus.Team[CREATURE_TEAM].bFirstID; iLoop <= Globals.gTacticalStatus.Team[CREATURE_TEAM].bLastID; iLoop++)
        {
            pSoldier = Globals.MercPtrs[iLoop];

            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.bLife > 0 && pSoldier.ubBodyType == BLOODCAT)
            {
                return (true);
            }
        }

        return (false);
    }

    void StartPlayerTeamTurn(bool fDoBattleSnd, bool fEnteringCombatMode)
    {
        int cnt;
        //	SOLDIERTYPE		*pSoldier;
        //	EV_S_BEGINTURN	SBeginTurn;

        // Start the turn of player charactors

        // 
        // PATCH 1.06:
        //
        // make sure set properly in Globals.gTacticalStatus:
        Globals.gTacticalStatus.ubCurrentTeam = OUR_TEAM;

        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;

        InitPlayerUIBar(false);

        if (Globals.gTacticalStatus.uiFlags & TURNBASED)
        {
            // Are we in combat already?
            if (Globals.gTacticalStatus.uiFlags & INCOMBAT)
            {
                PlayJA2Sample(ENDTURN_1, RATE_11025, MIDVOLUME, 1, MIDDLEPAN);
            }

            // Remove deadlock message
            EndDeadlockMsg();

            // Check for victory conditions

            // ATE: Commented out - looks like this message is called earlier for our team
            // look for all mercs on the same team, 
            //for ( pSoldier = Globals.MercPtrs[ cnt ]; cnt <= Globals.gTacticalStatus.Team[ Globals.gbPlayerNum ].bLastID; cnt++,pSoldier++)
            //{	
            //	if ( pSoldier.bActive && pSoldier.bLife > 0 )
            //	{
            //		SBeginTurn.usSoldierID		= (UINT16)cnt;
            //		AddGameEvent( S_BEGINTURN, 0, &SBeginTurn );
            //	}
            //}

            // Are we in combat already?
            if (Globals.gTacticalStatus.uiFlags & INCOMBAT)
            {
                if (gusSelectedSoldier != NO_SOLDIER)
                {
                    // Check if this guy is able to be selected....
                    if (Globals.MercPtrs[gusSelectedSoldier].bLife < OKLIFE)
                    {
                        SelectNextAvailSoldier(Globals.MercPtrs[gusSelectedSoldier]);
                    }

                    // Slide to selected guy...
                    if (gusSelectedSoldier != NO_SOLDIER)
                    {
                        SlideTo(Globals.NOWHERE, gusSelectedSoldier, Globals.NOBODY, SETLOCATOR);

                        if (fDoBattleSnd)
                        {
                            // Say ATTENTION SOUND...
                            DoMercBattleSound(Globals.MercPtrs[gusSelectedSoldier], BATTLE_SOUND_ATTN1);
                        }

                        if (gsInterfaceLevel == 1)
                        {
                            Globals.gTacticalStatus.uiFlags |= SHOW_ALL_ROOFS;
                            InvalidateWorldRedundency();
                            SetRenderFlags(RENDER_FLAG_FULL);
                            ErasePath(false);
                        }
                    }
                }
            }

            // Dirty panel interface!
            fInterfacePanelDirty = DIRTYLEVEL2;

            // Adjust time now!
            UpdateClock();

            if (!fEnteringCombatMode)
            {
                CheckForEndOfCombatMode(true);
            }

        }
        // Signal UI done enemy's turn
        guiPendingOverrideEvent = LU_ENDUILOCK;

        // ATE: Reset killed on attack variable.. this is because sometimes timing is such
        /// that a baddie can die and still maintain it's attacker ID
        Globals.gTacticalStatus.fKilledEnemyOnAttack = false;

        HandleTacticalUI();
    }

    void FreezeInterfaceForEnemyTurn()
    {
        // Reset flags
        gfPlotNewMovement = true;

        // Erase path
        ErasePath(true);

        // Setup locked UI
        guiPendingOverrideEvent = LU_BEGINUILOCK;

        // Remove any UI messages!
        if (giUIMessageOverlay != -1)
        {
            EndUIMessage();
        }
    }


    void EndTurn(int ubNextTeam)
    {
        SOLDIERTYPE? pSoldier;
        int cnt;

        //Check for enemy pooling (add enemies if there happens to be more than the max in the
        //current battle.  If one or more slots have freed up, we can add them now.

        EndDeadlockMsg();

        /*
            if ( CheckForEndOfCombatMode( false ) )
            {
                return;
            }
            */

        if (INTERRUPT_QUEUED)
        {
            EndInterrupt(false);
        }
        else
        {
            AddPossiblePendingEnemiesToBattle();

            //		InitEnemyUIBar( );

            FreezeInterfaceForEnemyTurn();

            // Loop through all mercs and set to moved
            cnt = Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bFirstID;
            for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; cnt++, pSoldier++)
            {
                if (pSoldier.bActive)
                {
                    pSoldier.bMoved = 1;
                }
            }

            Globals.gTacticalStatus.ubCurrentTeam = ubNextTeam;

            BeginTeamTurn(Globals.gTacticalStatus.ubCurrentTeam);

            BetweenTurnsVisibilityAdjustments();
        }
    }

    void EndAITurn()
    {
        SOLDIERTYPE? pSoldier;
        int cnt;

        // Remove any deadlock message
        EndDeadlockMsg();
        if (INTERRUPT_QUEUED)
        {
            EndInterrupt(false);
        }
        else
        {
            cnt = Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bFirstID;
            for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; cnt++, pSoldier++)
            {
                if (pSoldier.bActive)
                {
                    pSoldier.bMoved = 1;
                    // record old life value... for creature AI; the human AI might
                    // want to use this too at some point
                    pSoldier.bOldLife = pSoldier.bLife;
                }
            }

            Globals.gTacticalStatus.ubCurrentTeam++;
            BeginTeamTurn(Globals.gTacticalStatus.ubCurrentTeam);
        }
    }

    void EndAllAITurns()
    {
        // warp turn to the player's turn
        SOLDIERTYPE? pSoldier;
        int cnt;

        // Remove any deadlock message
        EndDeadlockMsg();
        if (INTERRUPT_QUEUED)
        {
            EndInterrupt(false);
        }

        if (Globals.gTacticalStatus.ubCurrentTeam != Globals.gbPlayerNum)
        {
            cnt = Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bFirstID;
            for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; cnt++, pSoldier++)
            {
                if (pSoldier.bActive)
                {
                    pSoldier.bMoved = true;
                    pSoldier.uiStatusFlags &= (~SOLDIER_UNDERAICONTROL);
                    // record old life value... for creature AI; the human AI might
                    // want to use this too at some point
                    pSoldier.bOldLife = pSoldier.bLife;
                }
            }

            Globals.gTacticalStatus.ubCurrentTeam = Globals.gbPlayerNum;
            //BeginTeamTurn( Globals.gTacticalStatus.ubCurrentTeam );
        }
    }

    void EndTurnEvents()
    {
        // HANDLE END OF TURN EVENTS
        // handle team services like healing
        HandleTeamServices(Globals.gbPlayerNum);
        // handle smell and blood decay
        DecaySmells();
        // decay bomb timers and maybe set some off!
        DecayBombTimers();

        DecaySmokeEffects(GetWorldTotalSeconds());
        DecayLightEffects(GetWorldTotalSeconds());

        // decay AI warning values from corpses
        DecayRottingCorpseAIWarnings();
    }

    void BeginTeamTurn(TEAM ubTeam)
    {
        int cnt;
        int ubID;
        SOLDIERTYPE? pSoldier;

        while (true)
        {
            if (ubTeam > LAST_TEAM)
            {
                if (HandleAirRaidEndTurn(ubTeam))
                {
                    // End turn!!
                    ubTeam = Globals.gbPlayerNum;
                    Globals.gTacticalStatus.ubCurrentTeam = Globals.gbPlayerNum;
                    EndTurnEvents();
                }
                else
                {
                    break;
                }
            }
            else if ((Globals.gTacticalStatus.Team[ubTeam].bTeamActive) > 0)
            {
                // inactive team, skip to the next one
                ubTeam++;
                Globals.gTacticalStatus.ubCurrentTeam++;
                // skip back to the top, as we are processing another team now.
                continue;
            }

            if (Globals.gTacticalStatus.uiFlags & TURNBASED)
            {
                BeginLoggingForBleedMeToos(true);

                // decay team's public opplist
                DecayPublicOpplist(ubTeam);

                cnt = Globals.gTacticalStatus.Team[ubTeam].bFirstID;
                for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[ubTeam].bLastID; cnt++) ;//, pSoldier++)
                {
                    if (pSoldier.bActive && pSoldier.bLife > 0)
                    {
                        // decay personal opplist, and refresh APs and BPs
                        EVENT_BeginMercTurn(pSoldier, false, 0);
                    }
                }

                if (Globals.gTacticalStatus.bBoxingState == LOST_ROUND || Globals.gTacticalStatus.bBoxingState == WON_ROUND || Globals.gTacticalStatus.bBoxingState == DISQUALIFIED)
                {
                    // we have no business being in here any more!
                    return;
                }

                BeginLoggingForBleedMeToos(false);

            }

            if (ubTeam == Globals.gbPlayerNum)
            {
                // ATE: Check if we are still in a valid battle...
                // ( they could have blead to death above )
                if ((Globals.gTacticalStatus.uiFlags & INCOMBAT))
                {
                    StartPlayerTeamTurn(true, false);
                }
                break;
            }
            else
            {
                // Set First enemy merc to AI control	
                if (BuildAIListForTeam(ubTeam))
                {

                    ubID = RemoveFirstAIListEntry();
                    if (ubID != Globals.NOBODY)
                    {
                        // Dirty panel interface!
                        Globals.fInterfacePanelDirty = DIRTYLEVEL2;
                        if (ubTeam == TEAM.CREATURE_TEAM && BloodcatsPresent())
                        {
                            AddTopMessage(COMPUTER_TURN_MESSAGE, Message[STR_BLOODCATS_TURN]);
                        }
                        else
                        {
                            AddTopMessage(COMPUTER_TURN_MESSAGE, TeamTurnString[(int)ubTeam]);
                        }
                        StartNPCAI(Globals.MercPtrs[ubID]);
                        return;
                    }
                }

                // This team is dead/inactive/being skipped in boxing
                // skip back to the top to process the next team
                ubTeam++;
                Globals.gTacticalStatus.ubCurrentTeam++;
            }
        }
    }

    void DisplayHiddenInterrupt(SOLDIERTYPE? pSoldier)
    {
        // If the AI got an interrupt but this has been hidden from the player until this point,
        // this code will display the interrupt

        if (!Globals.gfHiddenInterrupt)
        {
            return;
        }
        EndDeadlockMsg();

        if (pSoldier.bVisible != -1)
        {
            SlideTo(Globals.NOWHERE, pSoldier.ubID, Globals.NOBODY, SETLOCATOR);
        }

        Globals.guiPendingOverrideEvent = LU_BEGINUILOCK;

        // Dirty panel interface!
        Globals.fInterfacePanelDirty = DIRTYLEVEL2;

        // Erase path!
        ErasePath(true);

        // Reset flags
        Globals.gfPlotNewMovement = true;

        // Stop our guy....
        AdjustNoAPToFinishMove(Globals.MercPtrs[LATEST_INTERRUPT_GUY()], true);
        // Stop him from going to prone position if doing a turn while prone
        Globals.MercPtrs[LATEST_INTERRUPT_GUY()].fTurningFromPronePosition = false;

        // get rid of any old overlay message
        if (pSoldier.bTeam == TEAM.MILITIA_TEAM)
        {
            AddTopMessage(MILITIA_INTERRUPT_MESSAGE, Message[STR_INTERRUPT]);
        }
        else
        {
            AddTopMessage(COMPUTER_INTERRUPT_MESSAGE, Message[STR_INTERRUPT]);
        }

        Globals.gfHiddenInterrupt = false;
    }

    void DisplayHiddenTurnbased(SOLDIERTYPE? pActingSoldier)
    {
        // This code should put the game in turn-based and give control to the AI-controlled soldier
        // whose pointer has been passed in as an argument (we were in non-combat and the AI is doing
        // something visible, i.e. making an attack)

        if (AreInMeanwhile())
        {
            return;
        }

        if (Globals.gTacticalStatus.uiFlags & REALTIME || Globals.gTacticalStatus.uiFlags & INCOMBAT)
        {
            // pointless call here; do nothing
            return;
        }

        // Enter combat mode starting with this side's turn
        Globals.gTacticalStatus.ubCurrentTeam = pActingSoldier.bTeam;

        CommonEnterCombatModeCode();

        //JA2Gold: use function to make sure flags turned off everywhere else
        //pActingSoldier.uiStatusFlags |= SOLDIER_UNDERAICONTROL;
        SetSoldierAsUnderAiControl(pActingSoldier);
        DebugAI(string.Format("Giving AI control to %d", pActingSoldier.ubID));
        pActingSoldier.fTurnInProgress = true;
        Globals.gTacticalStatus.uiTimeSinceMercAIStart = Globals.GetJA2Clock();

        if (Globals.gTacticalStatus.ubTopMessageType != COMPUTER_TURN_MESSAGE)
        {
            // Dirty panel interface!
            Globals.fInterfacePanelDirty = DIRTYLEVEL2;
            if (Globals.gTacticalStatus.ubCurrentTeam == TEAM.CREATURE_TEAM && BloodcatsPresent())
            {
                AddTopMessage(COMPUTER_TURN_MESSAGE, Message[STR_BLOODCATS_TURN]);
            }
            else
            {
                AddTopMessage(COMPUTER_TURN_MESSAGE, TeamTurnString[Globals.gTacticalStatus.ubCurrentTeam]);
            }

        }


        // freeze the user's interface
        FreezeInterfaceForEnemyTurn();
    }

    bool EveryoneInInterruptListOnSameTeam()
    {
        int ubLoop;
        TEAM ubTeam = (TEAM)255;

        for (ubLoop = 1; ubLoop <= Globals.gubOutOfTurnPersons; ubLoop++)
        {
            if (ubTeam == (TEAM)255)
            {
                ubTeam = Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubLoop]].bTeam;
            }
            else
            {
                if (Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubLoop]].bTeam != ubTeam)
                {
                    return (false);
                }
            }
        }
        return (true);
    }

    void StartInterrupt()
    {
        int ubFirstInterrupter;
        TEAM bTeam;
        SOLDIERTYPE? pSoldier;
        SOLDIERTYPE? pTempSoldier;
        int ubInterrupter;
        int cnt;

        ubFirstInterrupter = LATEST_INTERRUPT_GUY();
        pSoldier = Globals.MercPtrs[ubFirstInterrupter];
        bTeam = pSoldier.bTeam;
        ubInterrupter = ubFirstInterrupter;

        // display everyone on int queue!
        for (cnt = Globals.gubOutOfTurnPersons; cnt > 0; cnt--)
        {
            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("STARTINT:  Q position %d: %d", cnt, gubOutOfTurnOrder[cnt]));
        }

        //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: %d is now on top of the interrupt queue", ubFirstInterrupter ) );

        Globals.gTacticalStatus.fInterruptOccurred = true;

        cnt = 0;
        for (pTempSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.MAX_NUM_SOLDIERS; cnt++)//, pTempSoldier++)
        {
            if (pTempSoldier.bActive)
            {
                pTempSoldier.bMovedPriorToInterrupt = pTempSoldier.bMoved;
                pTempSoldier.bMoved = 1;
            }
        }

        if (pSoldier.bTeam == TEAM.OUR_TEAM)
        {
            // start interrupts for everyone on our side at once
            string sTemp;
            int ubInterrupters = 0;
            int iSquad, iCounter;

            // build string for display of who gets interrupt
            while (true)
            {
                Globals.MercPtrs[ubInterrupter].bMoved = 0;
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: popping %d off of the interrupt queue", ubInterrupter));

                REMOVE_LATEST_INTERRUPT_GUY();
                // now LATEST_INTERRUPT_GUY is the guy before the previous
                ubInterrupter = LATEST_INTERRUPT_GUY();

                if (ubInterrupter == Globals.NOBODY) // previously emptied slot!
                {
                    continue;
                }
                else if (Globals.MercPtrs[ubInterrupter].bTeam != bTeam)
                {
                    break;
                }
            }

            wcscpy(sTemp, Message[STR_INTERRUPT_FOR]);

            // build string in separate loop here, want to linearly process squads...
            for (iSquad = 0; iSquad < Globals.NUMBER_OF_SQUADS; iSquad++)
            {
                for (iCounter = 0; iCounter < Globals.NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
                {
                    pTempSoldier = Squad[iSquad,iCounter];
                    if (pTempSoldier is not null && pTempSoldier.bActive && pTempSoldier.bInSector && pTempSoldier.bMoved == 0)
                    {
                        // then this guy got an interrupt...
                        ubInterrupters++;
                        if (ubInterrupters > 6)
                        {
                            // flush... display string, then clear it (we could have 20 names!)
                            // add comma to end, we know we have another person after this...
                            // wcscat(sTemp, ", ");
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, Globals.MSG_INTERFACE, sTemp);
                            // wcscpy(sTemp, "");
                            ubInterrupters = 1;
                        }

                        if (ubInterrupters > 1)
                        {
                            wcscat(sTemp, ", ");
                        }
                        wcscat(sTemp, pTempSoldier.name);
                    }
                }
            }

            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, Globals.MSG_INTERFACE, sTemp);

            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: starting interrupt for %d", ubFirstInterrupter));
            // gusSelectedSoldier should become the topmost guy on the interrupt list
            //gusSelectedSoldier = ubFirstInterrupter;

            // Remove deadlock message
            EndDeadlockMsg();

            // Select guy....
            SelectSoldier(ubFirstInterrupter, true, true);

            // ATE; Slide to guy who got interrupted!
            SlideTo(Globals.NOWHERE, gubLastInterruptedGuy, Globals.NOBODY, SETLOCATOR);

            // Dirty panel interface!
            fInterfacePanelDirty = DIRTYLEVEL2;
            Globals.gTacticalStatus.ubCurrentTeam = pSoldier.bTeam;

            // Signal UI done enemy's turn
            guiPendingOverrideEvent = LU_ENDUILOCK;
            HandleTacticalUI();

            InitPlayerUIBar(true);
            //AddTopMessage( PLAYER_INTERRUPT_MESSAGE, Message[STR_INTERRUPT] );

            PlayJA2Sample(ENDTURN_1, RATE_11025, MIDVOLUME, 1, MIDDLEPAN);

            // report any close call quotes for us here
            for (iCounter = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID; iCounter <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; iCounter++)
            {
                if (OK_INSECTOR_MERC(Globals.MercPtrs[iCounter]))
                {
                    if (Globals.MercPtrs[iCounter].fCloseCall)
                    {
                        if (Globals.MercPtrs[iCounter].bNumHitsThisTurn == 0 && !(Globals.MercPtrs[iCounter].usQuoteSaidExtFlags & SOLDIER_QUOTE_SAID_EXT_CLOSE_CALL) && Random(3) == 0)
                        {
                            // say close call quote!
                            TacticalCharacterDialogue(Globals.MercPtrs[iCounter], QUOTE_CLOSE_CALL);
                            Globals.MercPtrs[iCounter].usQuoteSaidExtFlags |= SOLDIER_QUOTE_SAID_EXT_CLOSE_CALL;
                        }
                        Globals.MercPtrs[iCounter].fCloseCall = false;
                    }
                }
            }

        }
        else
        {
            // start interrupts for everyone on that side at once... and start AI with the lowest # guy

            // what we do is set everyone to moved except for people with interrupts at the moment
            /*
            cnt = Globals.gTacticalStatus.Team[ pSoldier.bTeam ].bFirstID;
            for ( pTempSoldier = Globals.MercPtrs[ cnt ]; cnt <= Globals.gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++,pTempSoldier++)
            {
                if ( pTempSoldier.bActive )
                {
                    pTempSoldier.bMovedPriorToInterrupt = pTempSoldier.bMoved;
                    pTempSoldier.bMoved = true;
                }
            }
            */

            while (1)
            {

                Globals.MercPtrs[ubInterrupter].bMoved = false;

                DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: popping %d off of the interrupt queue", ubInterrupter));

                REMOVE_LATEST_INTERRUPT_GUY();
                // now LATEST_INTERRUPT_GUY is the guy before the previous
                ubInterrupter = LATEST_INTERRUPT_GUY;
                if (ubInterrupter == Globals.NOBODY) // previously emptied slot!
                {
                    continue;
                }
                else if (Globals.MercPtrs[ubInterrupter].bTeam != bTeam)
                {
                    break;
                }
                else if (ubInterrupter < ubFirstInterrupter)
                {
                    ubFirstInterrupter = ubInterrupter;
                }
            }


            // here we have to rebuilt the AI list!
            BuildAIListForTeam(bTeam);

            // set to the new first interrupter
            cnt = RemoveFirstAIListEntry();

            pSoldier = Globals.MercPtrs[cnt];
            //		pSoldier = Globals.MercPtrs[ubFirstInterrupter];

            //if ( Globals.gTacticalStatus.ubCurrentTeam == OUR_TEAM)
            if (pSoldier.bTeam != OUR_TEAM)
            {
                // we're being interrupted by the computer!
                // we delay displaying any interrupt message until the computer
                // does something...
                gfHiddenInterrupt = true;
                Globals.gTacticalStatus.fUnLockUIAfterHiddenInterrupt = false;
            }
            // otherwise it's the AI interrupting another AI team

            Globals.gTacticalStatus.ubCurrentTeam = pSoldier.bTeam;

            StartNPCAI(pSoldier);
        }

        if (!gfHiddenInterrupt)
        {
            // Stop this guy....
            AdjustNoAPToFinishMove(Globals.MercPtrs[LATEST_INTERRUPT_GUY], true);
            Globals.MercPtrs[LATEST_INTERRUPT_GUY].fTurningFromPronePosition = false;
        }
    }

    void EndInterrupt(bool fMarkInterruptOccurred)
    {
        int ubInterruptedSoldier;
        SOLDIERTYPE? pSoldier;
        SOLDIERTYPE? pTempSoldier;
        int cnt;
        bool fFound;
        int ubMinAPsToAttack;

        for (cnt = gubOutOfTurnPersons; cnt > 0; cnt--)
        {
            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("ENDINT:  Q position %d: %d", cnt, gubOutOfTurnOrder[cnt]));
        }

        // ATE: OK, now if this all happended on one frame, we may not have to stop
        // guy from walking... so set this flag to false if so...
        if (fMarkInterruptOccurred)
        {
            // flag as true if an int occurs which ends an interrupt (int loop)
            Globals.gTacticalStatus.fInterruptOccurred = true;
        }
        else
        {
            Globals.gTacticalStatus.fInterruptOccurred = false;
        }

        // Loop through all mercs and see if any passed on this interrupt
        cnt = Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bFirstID;
        for (pTempSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; cnt++, pTempSoldier++)
        {
            if (pTempSoldier.bActive && pTempSoldier.bInSector && !pTempSoldier.bMoved && (pTempSoldier.bActionPoints == pTempSoldier.bIntStartAPs))
            {
                ubMinAPsToAttack = MinAPsToAttack(pTempSoldier, pTempSoldier.sLastTarget, false);
                if ((ubMinAPsToAttack <= pTempSoldier.bActionPoints) && (ubMinAPsToAttack > 0))
                {
                    pTempSoldier.bPassedLastInterrupt = true;
                }
            }
        }

        if (!EveryoneInInterruptListOnSameTeam())
        {
            gfHiddenInterrupt = false;

            // resume interrupted interrupt
            StartInterrupt();
        }
        else
        {
            ubInterruptedSoldier = LATEST_INTERRUPT_GUY;

            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: interrupt over, %d's team regains control", ubInterruptedSoldier));

            pSoldier = Globals.MercPtrs[ubInterruptedSoldier];

            cnt = 0;
            for (pTempSoldier = Globals.MercPtrs[cnt]; cnt <= MAX_NUM_SOLDIERS; cnt++, pTempSoldier++)
            {
                if (pTempSoldier.bActive)
                {
                    // AI guys only here...
                    if (pTempSoldier.bActionPoints == 0)
                    {
                        pTempSoldier.bMoved = true;
                    }
                    else if (pTempSoldier.bTeam != Globals.gbPlayerNum && pTempSoldier.bNewSituation == IS_NEW_SITUATION)
                    {
                        pTempSoldier.bMoved = false;
                    }
                    else
                    {
                        pTempSoldier.bMoved = pTempSoldier.bMovedPriorToInterrupt;
                    }
                }
            }


            // change team
            Globals.gTacticalStatus.ubCurrentTeam = pSoldier.bTeam;
            // switch appropriate messages & flags
            if (pSoldier.bTeam == OUR_TEAM)
            {
                // set everyone on the team to however they were set moved before the interrupt
                // must do this before selecting soldier...
                /*
                cnt = Globals.gTacticalStatus.Team[ Globals.gTacticalStatus.ubCurrentTeam ].bFirstID;
                for ( pTempSoldier = Globals.MercPtrs[ cnt ]; cnt <= Globals.gTacticalStatus.Team[ Globals.gTacticalStatus.ubCurrentTeam ].bLastID; cnt++,pTempSoldier++)
                {
                    if ( pTempSoldier.bActive )
                    {
                        pTempSoldier.bMoved = pTempSoldier.bMovedPriorToInterrupt;
                    }
                }
                */

                ClearIntList();

                // Select soldier....
                if (Globals.MercPtrs[ubInterruptedSoldier].bLife < OKLIFE)
                {
                    SelectNextAvailSoldier(Globals.MercPtrs[ubInterruptedSoldier]);
                }
                else
                {
                    SelectSoldier(ubInterruptedSoldier, false, false);
                }

                if (gfHiddenInterrupt)
                {
                    // Try to make things look like nothing happened at all.
                    gfHiddenInterrupt = false;

                    // If we can continue a move, do so!
                    if (Globals.MercPtrs[gusSelectedSoldier].fNoAPToFinishMove && pSoldier.ubReasonCantFinishMove != REASON_STOPPED_SIGHT)
                    {
                        // Continue
                        AdjustNoAPToFinishMove(Globals.MercPtrs[gusSelectedSoldier], false);

                        if (Globals.MercPtrs[gusSelectedSoldier].sGridNo != Globals.MercPtrs[gusSelectedSoldier].sFinalDestination)
                        {
                            EVENT_GetNewSoldierPath(Globals.MercPtrs[gusSelectedSoldier], Globals.MercPtrs[gusSelectedSoldier].sFinalDestination, Globals.MercPtrs[gusSelectedSoldier].usUIMovementMode);
                        }
                        else
                        {
                            UnSetUIBusy(pSoldier.ubID);
                        }
                    }
                    else
                    {
                        UnSetUIBusy(pSoldier.ubID);
                    }

                    if (Globals.gTacticalStatus.fUnLockUIAfterHiddenInterrupt)
                    {
                        Globals.gTacticalStatus.fUnLockUIAfterHiddenInterrupt = false;
                        UnSetUIBusy(pSoldier.ubID);
                    }
                }
                else
                {
                    // Signal UI done enemy's turn
                    /// ATE: This used to be ablow so it would get done for
                    // both hidden interrupts as well - NOT good because
                    // hidden interrupts should leave it locked if it was already...
                    guiPendingOverrideEvent = LU_ENDUILOCK;
                    HandleTacticalUI();

                    if (gusSelectedSoldier != NO_SOLDIER)
                    {
                        SlideTo(Globals.NOWHERE, gusSelectedSoldier, Globals.NOBODY, SETLOCATOR);

                        // Say ATTENTION SOUND...
                        DoMercBattleSound(Globals.MercPtrs[gusSelectedSoldier], BATTLE_SOUND_ATTN1);

                        if (gsInterfaceLevel == 1)
                        {
                            Globals.gTacticalStatus.uiFlags |= SHOW_ALL_ROOFS;
                            InvalidateWorldRedundency();
                            SetRenderFlags(RENDER_FLAG_FULL);
                            ErasePath(false);
                        }
                    }
                    // 2 indicates that we're ending an interrupt and going back to
                    // normal player's turn without readjusting time left in turn (for
                    // timed turns)
                    InitPlayerUIBar(2);
                }

            }
            else
            {
                // this could be set to true for AI-vs-AI interrupts
                gfHiddenInterrupt = false;

                // Dirty panel interface!
                fInterfacePanelDirty = DIRTYLEVEL2;

                // Erase path!
                ErasePath(true);

                // Reset flags
                gfPlotNewMovement = true;

                // restart AI with first available soldier
                fFound = false;

                // rebuild list for this team if anyone on the team is still available
                cnt = Globals.gTacticalStatus.Team[ENEMY_TEAM].bFirstID;
                for (pTempSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; cnt++, pTempSoldier++)
                {
                    if (pTempSoldier.bActive && pTempSoldier.bInSector && pTempSoldier.bLife >= OKLIFE)
                    {
                        fFound = true;
                        break;
                    }
                }

                if (fFound)
                {
                    // reset found flag because we are rebuilding the AI list
                    fFound = false;

                    if (BuildAIListForTeam(Globals.gTacticalStatus.ubCurrentTeam))
                    {
                        // now bubble up everyone left in the interrupt queue, starting
                        // at the front of the array
                        for (cnt = 1; cnt <= gubOutOfTurnPersons; cnt++)
                        {
                            MoveToFrontOfAIList(gubOutOfTurnOrder[cnt]);
                        }

                        cnt = RemoveFirstAIListEntry();
                        if (cnt != Globals.NOBODY)
                        {
                            fFound = true;
                            StartNPCAI(Globals.MercPtrs[cnt]);
                        }
                    }

                }

                if (fFound)
                {
                    // back to the computer!
                    if (Globals.gTacticalStatus.ubCurrentTeam == CREATURE_TEAM && BloodcatsPresent())
                    {
                        AddTopMessage(COMPUTER_TURN_MESSAGE, Message[STR_BLOODCATS_TURN]);
                    }
                    else
                    {
                        AddTopMessage(COMPUTER_TURN_MESSAGE, TeamTurnString[Globals.gTacticalStatus.ubCurrentTeam]);
                    }

                    // Signal UI done enemy's turn
                    guiPendingOverrideEvent = LU_BEGINUILOCK;

                    ClearIntList();
                }
                else
                {
                    // back to the computer!
                    if (Globals.gTacticalStatus.ubCurrentTeam == CREATURE_TEAM && BloodcatsPresent())
                    {
                        AddTopMessage(COMPUTER_TURN_MESSAGE, Message[STR_BLOODCATS_TURN]);
                    }
                    else
                    {
                        AddTopMessage(COMPUTER_TURN_MESSAGE, TeamTurnString[Globals.gTacticalStatus.ubCurrentTeam]);
                    }

                    // Signal UI done enemy's turn
                    guiPendingOverrideEvent = LU_BEGINUILOCK;

                    // must clear int list before ending turn
                    ClearIntList();
                    EndAITurn();
                }
            }

            // Reset our interface!
            fInterfacePanelDirty = DIRTYLEVEL2;

        }
    }


    bool StandardInterruptConditionsMet(SOLDIERTYPE? pSoldier, int ubOpponentID, int bOldOppList)
    {
        //	int ubAniType;
        int ubMinPtsNeeded;
        int bDir;
        SOLDIERTYPE? pOpponent;

        if ((Globals.gTacticalStatus.uiFlags & TURNBASED) && (Globals.gTacticalStatus.uiFlags & INCOMBAT) && !(gubSightFlags & SIGHT_INTERRUPT))
        {
            return (false);
        }

        if (Globals.gTacticalStatus.ubAttackBusyCount > 0)
        {
            return (false);
        }

        if (ubOpponentID < Globals.NOBODY)
        {
            /*
            // only the OPPONENT'S controller's decision matters
            if (Menptr[ubOpponentID].controller != Net.pnum)
            {
                return(false);
            }
            */

            // ALEX
            // if interrupts are restricted to a particular opponent only & he's not it
            if ((InterruptOnlyGuynum != Globals.NOBODY) && (ubOpponentID != InterruptOnlyGuynum))
            {
                return (false);
            }

            pOpponent = Globals.MercPtrs[ubOpponentID];
        }
        else    // no opponent, so controller of 'ptr' makes the call instead
        {
            // ALEX
            if (gsWhoThrewRock >= Globals.NOBODY)
            {
                return (false);
            }

            // the machine that controls the guy who threw the rock makes the decision
            /*
            if (Menptr[WhoThrewRock].controller != Net.pnum)
                return(false);
            */
            pOpponent = null;
        }

        // if interrupts have been disabled for any reason
        if (!InterruptsAllowed)
        {
            return (false);
        }

        // in non-combat allow interrupt points to be calculated freely (everyone's in control!)
        // also allow calculation for storing in AllTeamsLookForAll
        if ((Globals.gTacticalStatus.uiFlags & INCOMBAT) && (gubBestToMakeSightingSize != BEST_SIGHTING_ARRAY_SIZE_ALL_TEAMS_LOOK_FOR_ALL))
        {
            // if his team's already in control
            if (pSoldier.bTeam == Globals.gTacticalStatus.ubCurrentTeam)
            {
                // if this is a player's a merc or civilian
                if ((pSoldier.uiStatusFlags & SOLDIER.PC) || PTR_CIVILIAN)
                {
                    // then they are not allowed to interrupt their own team
                    return (false);
                }
                else
                {
                    // enemies, MAY interrupt each other, but NOT themselves!
                    //if ( pSoldier.uiStatusFlags & SOLDIER_UNDERAICONTROL )
                    //{
                    return (false);
                    //}
                }

                // CJC, July 9 1998
                // NO ONE EVER interrupts his own team
                //return( false );
            }
            else if (Globals.gTacticalStatus.bBoxingState != NOT_BOXING)
            {
                // while anything to do with boxing is going on, skip interrupts!
                return (false);
            }

        }

        if (!(pSoldier.bActive) || !(pSoldier.bInSector))
        {
            return (false);
        }

        // soldiers at less than OKLIFE can't perform any actions
        if (pSoldier.bLife < Globals.OKLIFE)
        {
            return (false);
        }

        // soldiers out of breath are about to fall over, no interrupt
        if (pSoldier.bBreath < Globals.OKBREATH || pSoldier.bCollapsed)
        {
            return (false);
        }

        // if soldier doesn't have enough APs
        if (pSoldier.bActionPoints < Globals.MIN_APS_TO_INTERRUPT)
        {
            return (false);
        }

        // soldiers gagging on gas are too busy about holding their cookies down...
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.GASSED))
        {
            return (false);
        }

        // a soldier already engaged in a life & death battle is too busy doing his
        // best to survive to worry about "getting the jump" on additional threats
        if (pSoldier.bUnderFire > 0)
        {
            return (false);
        }

        if (pSoldier.bCollapsed)
        {
            return (false);
        }

        // don't allow neutral folks to get interrupts
        if (pSoldier.bNeutral > 0)
        {
            return (false);
        }

        // no EPCs allowed to get interrupts
        if (AM_AN_EPC(pSoldier) && !AM_A_ROBOT(pSoldier))
        {
            return (false);
        }


        // don't let mercs on assignment get interrupts
        if (pSoldier.bTeam == Globals.gbPlayerNum && pSoldier.bAssignment >= Assignments.ON_DUTY)
        {
            return (false);
        }


        // the bare minimum default is enough APs left to TURN
        ubMinPtsNeeded = AP.CHANGE_FACING;

        // if the opponent is SOMEBODY
        if (ubOpponentID < Globals.NOBODY)
        {
            // if the soldiers are on the same side
            if (pSoldier.bSide == pOpponent.bSide)
            {
                // human/civilians on same side can't interrupt each other
                if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)) || PTR_CIVILIAN)
                {
                    return (false);
                }
                else    // enemy
                {
                    // enemies can interrupt EACH OTHER, but enemies and civilians on the
                    // same side (but different teams) can't interrupt each other.
                    if (pSoldier.bTeam != pOpponent.bTeam)
                    {
                        return (false);
                    }
                }
            }

            // if the interrupted opponent is not the selected character, then the only
            // people eligible to win an interrupt are those on the SAME SIDE AS
            // the selected character, ie. his friends...
            if (pOpponent.bTeam == Globals.gbPlayerNum)
            {
                if ((ubOpponentID != Globals.gusSelectedSoldier) && (pSoldier.bSide != Globals.Menptr[Globals.gusSelectedSoldier].bSide))
                {
                    return (false);
                }
            }
            else
            {
                if (!(pOpponent.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
                    && (pSoldier.bSide != pOpponent.bSide))
                {
                    return (false);
                }
            }
            /* old DG code for same:

            if ((ubOpponentID != gusSelectedSoldier) && (pSoldier.bSide != Menptr[gusSelectedSoldier].bSide))
            {
                return(false);
            }
            */

            // an non-active soldier can't interrupt a soldier who is also non-active!
            if ((pOpponent.bTeam != Globals.gTacticalStatus.ubCurrentTeam)
                && (pSoldier.bTeam != Globals.gTacticalStatus.ubCurrentTeam))
            {
                return (false);
            }


            // if this is a "SEEING" interrupt
            if (pSoldier.bOppList[ubOpponentID] == SEEN_CURRENTLY)
            {
                // if pSoldier already saw the opponent last "look" or at least this turn
                if ((bOldOppList == SEEN_CURRENTLY) || (bOldOppList == SEEN_THIS_TURN))
                {
                    return (false);     // no interrupt is possible
                }

                // if the soldier is behind him and not very close, forget it
                bDir = atan8(pSoldier.sX, pSoldier.sY, pOpponent.sX, pOpponent.sY);
                if (gOppositeDirection[pSoldier.bDesiredDirection] == bDir)
                {
                    // directly behind; allow interrupts only within # of tiles equal to level
                    if (PythSpacesAway(pSoldier.sGridNo, pOpponent.sGridNo) > EffectiveExpLevel(pSoldier))
                    {
                        return (false);
                    }
                }

                // if the soldier isn't currently crouching
                if (!PTR_CROUCHED)
                {
                    ubMinPtsNeeded = AP.CROUCH;
                }
                else
                {
                    ubMinPtsNeeded = MinPtsToMove(pSoldier);
                }
            }
            else   // this is a "HEARING" interrupt
            {
                // if the opponent can't see the "interrupter" either, OR
                // if the "interrupter" already has any opponents already in sight, OR
                // if the "interrupter" already heard the active soldier this turn
                if ((pOpponent.bOppList[pSoldier.ubID] != SEEN_CURRENTLY) || (pSoldier.bOppCnt > 0) || (bOldOppList == HEARD_THIS_TURN))
                {
                    return (false);     // no interrupt is possible
                }
            }
        }


        // soldiers without sufficient APs to do something productive can't interrupt
        if (pSoldier.bActionPoints < ubMinPtsNeeded)
        {
            return (false);
        }

        // soldier passed on the chance to react during previous interrupt this turn
        if (pSoldier.bPassedLastInterrupt)
        {
            return (false);
        }

        return (true);
    }


    int CalcInterruptDuelPts(SOLDIERTYPE? pSoldier, int ubOpponentID, bool fUseWatchSpots)
    {
        int bPoints;
        int bLightLevel;
        int ubDistance;

        // extra check to make sure neutral folks never get interrupts
        if (pSoldier.bNeutral)
        {
            return (NO_INTERRUPT);
        }

        // BASE is one point for each experience level.

        // Robot has interrupt points based on the controller...
        // Controller's interrupt points are reduced by 2 for being distracted...
        if (pSoldier.uiStatusFlags & SOLDIER_ROBOT && CanRobotBeControlled(pSoldier))
        {
            bPoints = EffectiveExpLevel(Globals.MercPtrs[pSoldier.ubRobotRemoteHolderID]) - 2;
        }
        else
        {
            bPoints = EffectiveExpLevel(pSoldier);
            /*
            if ( pSoldier.bTeam == ENEMY_TEAM )
            {
                // modify by the difficulty level setting
                bPoints += gbDiff[ DIFF_ENEMY_INTERRUPT_MOD ][ SoldierDifficultyLevel( pSoldier ) ];
                bPoints = __max( bPoints, 9 );
            }
            */

            if (ControllingRobot(pSoldier))
            {
                bPoints -= 2;
            }
        }

        if (fUseWatchSpots)
        {
            // if this is a previously noted spot of enemies, give bonus points!
            bPoints += GetWatchedLocPoints(pSoldier.ubID, Globals.MercPtrs[ubOpponentID].sGridNo, Globals.MercPtrs[ubOpponentID].bLevel);
        }

        // LOSE one point for each 2 additional opponents he currently sees, above 2
        if (pSoldier.bOppCnt > 2)
        {
            // subtract 1 here so there is a penalty of 1 for seeing 3 enemies
            bPoints -= (pSoldier.bOppCnt - 1) / 2;
        }

        // LOSE one point if he's trying to interrupt only by hearing
        if (pSoldier.bOppList[ubOpponentID] == HEARD_THIS_TURN)
        {
            bPoints--;
        }

        // if soldier is still in shock from recent injuries, that penalizes him
        bPoints -= pSoldier.bShock;

        ubDistance = (int)PythSpacesAway(pSoldier.sGridNo, Globals.MercPtrs[ubOpponentID].sGridNo);

        // if we are in combat mode - thus doing an interrupt rather than determine who gets first turn - 
        // then give bonus 
        if ((Globals.gTacticalStatus.uiFlags & INCOMBAT) && (pSoldier.bTeam != Globals.gTacticalStatus.ubCurrentTeam))
        {
            // passive player gets penalty due to range
            bPoints -= (ubDistance / 10);
        }
        else
        {
            // either non-combat or the player with the current turn... i.e. active...
            // unfortunately we can't use opplist here to record whether or not we saw this guy before, because at this point
            // the opplist has been updated to seen.  But we can use gbSeenOpponents ...

            // this soldier is moving, so give them a bonus for crawling or swatting at long distances
            if (!gbSeenOpponents[ubOpponentID][pSoldier.ubID])
            {
                if (pSoldier.usAnimState == SWATTING && ubDistance > (MaxDistanceVisible() / 2)) // more than 1/2 sight distance
                {
                    bPoints++;
                }
                else if (pSoldier.usAnimState == CRAWLING && ubDistance > (MaxDistanceVisible() / 4)) // more than 1/4 sight distance
                {
                    bPoints += ubDistance / STRAIGHT;
                }
            }
        }

        // whether active or not, penalize people who are running
        if (pSoldier.usAnimState == RUNNING && !gbSeenOpponents[pSoldier.ubID][ubOpponentID])
        {
            bPoints -= 2;
        }

        if (pSoldier.ubServicePartner != Globals.NOBODY)
        {
            // distracted by being bandaged/doing bandaging
            bPoints -= 2;
        }

        if (HAS_SKILL_TRAIT(pSoldier, NIGHTOPS))
        {
            bLightLevel = LightTrueLevel(pSoldier.sGridNo, pSoldier.bLevel);
            if (bLightLevel > NORMAL_LIGHTLEVEL_DAY + 3)
            {
                // it's dark, give a bonus for interrupts
                bPoints += 1 * NUM_SKILL_TRAITS(pSoldier, NIGHTOPS);
            }
        }

        // if he's a computer soldier

        // CJC note: this will affect friendly AI as well...

        if (pSoldier.uiStatusFlags & SOLDIER_PC)
        {
            if (pSoldier.bAssignment >= ON_DUTY)
            {
                // make sure don't get interrupts!
                bPoints = -10;
            }

            // GAIN one point if he's previously seen the opponent
            // check for true because -1 means we JUST saw him (always so here)
            if (gbSeenOpponents[pSoldier.ubID][ubOpponentID] == true)
            {
                bPoints++;  // seen him before, easier to react to him
            }
        }
        else if (pSoldier.bTeam == ENEMY_TEAM)
        {
            // GAIN one point if he's previously seen the opponent
            // check for true because -1 means we JUST saw him (always so here)
            if (gbSeenOpponents[pSoldier.ubID][ubOpponentID] == true)
            {
                bPoints++;  // seen him before, easier to react to him
            }
            else if (gbPublicOpplist[pSoldier.bTeam][ubOpponentID] != NOT_HEARD_OR_SEEN)
            {
                // GAIN one point if opponent has been recently radioed in by his team
                bPoints++;
            }
        }

        if (TANK(pSoldier))
        {
            // reduce interrupt possibilities for tanks!
            bPoints /= 2;
        }

        if (bPoints >= AUTOMATIC_INTERRUPT)
        {
            bPoints = AUTOMATIC_INTERRUPT - 1;  // hack it to one less than max so its legal
        }

        return (bPoints);
    }

    bool InterruptDuel(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pOpponent)
    {
        bool fResult = false;

        // if opponent can't currently see us and we can see them
        if (pSoldier.bOppList[pOpponent.ubID] == SEEN_CURRENTLY && pOpponent.bOppList[pSoldier.ubID] != SEEN_CURRENTLY)
        {
            fResult = true;       // we automatically interrupt
                                  // fix up our interrupt duel pts if necessary
            if (pSoldier.bInterruptDuelPts < pOpponent.bInterruptDuelPts)
            {
                pSoldier.bInterruptDuelPts = pOpponent.bInterruptDuelPts;
            }
        }
        else
        {
            // If our total points is HIGHER, then we interrupt him anyway
            if (pSoldier.bInterruptDuelPts > pOpponent.bInterruptDuelPts)
            {
                fResult = true;
            }
        }
        //	Messages.ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"Interrupt duel %d (%d pts) vs %d (%d pts)", pSoldier.ubID, pSoldier.bInterruptDuelPts, pOpponent.ubID, pOpponent.bInterruptDuelPts );
        return (fResult);
    }


    public static void DeleteFromIntList(int ubIndex, bool fCommunicate)
    {
        int ubLoop;
        int ubID;

        if (ubIndex > Globals.gubOutOfTurnPersons)
        {
            return;
        }

        // remember who we're getting rid of
        ubID = Globals.gubOutOfTurnOrder[ubIndex];

        //	Messages.ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"%d removed from int list", ubID );
        // if we're NOT deleting the LAST entry in the int list
        if (ubIndex < Globals.gubOutOfTurnPersons)
        {
            // not the last entry, must move all those behind it over to fill the gap
            for (ubLoop = ubIndex; ubLoop < Globals.gubOutOfTurnPersons; ubLoop++)
            {
                Globals.gubOutOfTurnOrder[ubLoop] = Globals.gubOutOfTurnOrder[ubLoop + 1];
            }
        }

        // either way, whack the last entry to Globals.NOBODY and decrement the list size
        Globals.gubOutOfTurnOrder[Globals.gubOutOfTurnPersons] = Globals.NOBODY;
        Globals.gubOutOfTurnPersons--;

        // once the last interrupted guy gets deleted from the list, he's no longer
        // the last interrupted guy!
        /*
        if (Status.lastInterruptedWas == ubID)
        {
            Status.lastInterruptedWas = Globals.NOBODY;
        }
        */
    }

    public static void AddToIntList(int ubID, bool fGainControl, bool fCommunicate)
    {
        int ubLoop;

        //	Messages.ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"%d added to int list", ubID );
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INTERRUPT: adding ID %d who %s", ubID, fGainControl ? "gains control" : "loses control"));

        // check whether 'who' is already anywhere on the queue after the first index
        // which we want to preserve so we can restore turn order
        for (ubLoop = 2; ubLoop <= Globals.gubOutOfTurnPersons; ubLoop++)
        {
            if (Globals.gubOutOfTurnOrder[ubLoop] == ubID)
            {
                if (!fGainControl)
                {
                    // he's LOSING control; that's it, we're done, DON'T add him to the queue again
                    gubLastInterruptedGuy = ubID;
                    return;
                }
                else
                {
                    // GAINING control, so delete him from this slot (because later he'll
                    // get added to the end and we don't want him listed more than once!)
                    DeleteFromIntList(ubLoop, false);
                }
            }
        }

        // increment total (making index valid) and add him to list
        gubOutOfTurnPersons++;
        gubOutOfTurnOrder[gubOutOfTurnPersons] = ubID;

        /*
            // the guy being interrupted HAS to be the currently selected character
            if (Status.lastInterruptedWas != CharacterSelected)
            {
                // if we don't already do so, remember who that was
                Status.lastInterruptedWas = CharacterSelected;
            }
        */

        // if the guy is gaining control
        if (fGainControl)
        {
            // record his initial APs at the start of his interrupt at this time
            // this is not the ideal place for this, but it's the best I could do...
            Menptr[ubID].bIntStartAPs = Menptr[ubID].bActionPoints;
        }
        else
        {
            gubLastInterruptedGuy = ubID;
            // turn off AI control flag if they lost control
            if (Menptr[ubID].uiStatusFlags & SOLDIER_UNDERAICONTROL)
            {
                DebugAI(String("Taking away AI control from %d", ubID));
                Menptr[ubID].uiStatusFlags &= (~SOLDIER_UNDERAICONTROL);
            }
        }
    }

    void VerifyOutOfTurnOrderArray()
    {
        int[] ubTeamHighest = new int[Globals.MAXTEAMS];
        int ubTeamsInList;
        int ubNextInArrayOnTeam, ubNextIndex;
        TEAM ubTeam;
        int ubLoop;
        TEAM ubLoop2;
        bool fFoundLoop = false;

        for (ubLoop = 1; ubLoop <= Globals.gubOutOfTurnPersons; ubLoop++)
        {
            ubTeam = Globals.Menptr[Globals.gubOutOfTurnOrder[ubLoop]].bTeam;
            if (ubTeamHighest[(int)ubTeam] > 0)
            {
                // check the other teams to see if any of them are between our last team's mention in
                // the array and this
                for (ubLoop2 = 0; ubLoop2 < Globals.MAXTEAMS; ubLoop2++)
                {
                    if (ubLoop2 == ubTeam)
                    {
                        continue;
                    }
                    else
                    {
                        if (ubTeamHighest[(int)ubLoop2] > ubTeamHighest[(int)ubTeam])
                        {
                            // there's a loop!! delete it!
                            ubNextInArrayOnTeam = Globals.gubOutOfTurnOrder[ubLoop];
                            ubNextIndex = ubTeamHighest[(int)ubTeam] + 1;

                            while (Globals.gubOutOfTurnOrder[ubNextIndex] != ubNextInArrayOnTeam)
                            {
                                // Pause them...
                                AdjustNoAPToFinishMove(Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubNextIndex]], true);

                                // If they were turning from prone, stop them
                                Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubNextIndex]].fTurningFromPronePosition = false;

                                DeleteFromIntList(ubNextIndex, false);
                            }

                            fFoundLoop = true;
                            break;
                        }
                    }
                }

                if (fFoundLoop)
                {
                    // at this point we should restart our outside loop (ugh)
                    fFoundLoop = false;
                    for (ubLoop2 = 0; ubLoop2 < (TEAM)Globals.MAXTEAMS; ubLoop2++)
                    {
                        ubTeamHighest[(int)ubLoop2] = 0;
                    }
                    ubLoop = 0;
                    continue;

                }

            }

            ubTeamHighest[(int)ubTeam] = ubLoop;
        }

        // Another potential problem: the player is interrupted by the enemy who is interrupted by 
        // the militia.  In this situation the enemy should just lose their interrupt.
        // (Or, the militia is interrupted by the enemy who is interrupted by the player.)

        // Check for 3+ teams in the interrupt queue.  If three exist then abort all interrupts (return
        // control to the first team)
        ubTeamsInList = 0;
        for (ubLoop = 0; ubLoop < Globals.MAXTEAMS; ubLoop++)
        {
            if (ubTeamHighest[ubLoop] > 0)
            {
                ubTeamsInList++;
            }
        }
        if (ubTeamsInList >= 3)
        {
            // This is bad.  Loop through everyone but the first person in the INT list and remove 'em
            for (ubLoop = 2; ubLoop <= Globals.gubOutOfTurnPersons;)
            {
                if (Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubLoop]].bTeam != Globals.MercPtrs[Globals.gubOutOfTurnOrder[1]].bTeam)
                {
                    // remove!

                    // Pause them...
                    AdjustNoAPToFinishMove(Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubLoop]], true);

                    // If they were turning from prone, stop them
                    Globals.MercPtrs[Globals.gubOutOfTurnOrder[ubLoop]].fTurningFromPronePosition = false;

                    DeleteFromIntList(ubLoop, false);

                    // since we deleted someone from the list, we want to check the same index in the
                    // array again, hence we DON'T increment.
                }
                else
                {
                    ubLoop++;
                }
            }
        }

    }

    void DoneAddingToIntList(SOLDIERTYPE? pSoldier, bool fChange, int ubInterruptType)
    {
        if (fChange)
        {
            VerifyOutOfTurnOrderArray();
            if (EveryoneInInterruptListOnSameTeam())
            {
                EndInterrupt(true);
            }
            else
            {
                StartInterrupt();
            }
        }
    }

    void ResolveInterruptsVs(SOLDIERTYPE? pSoldier, int ubInterruptType)
    {
        TEAM ubTeam;
        int ubOpp;
        int ubIntCnt;
        int[] ubIntList = new int[Globals.MAXMERCS];
        int[] ubIntDiff = new int[Globals.MAXMERCS];
        int ubSmallestDiff;
        int ubSlot, ubSmallestSlot;
        int ubLoop;
        bool fIntOccurs;
        SOLDIERTYPE? pOpponent;
        bool fControlChanged = false;

        if ((Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
            && (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            ubIntCnt = 0;

            for (ubTeam = 0; ubTeam < (TEAM)Globals.MAXTEAMS; ubTeam++)
            {
                if (Globals.gTacticalStatus.Team[ubTeam].bTeamActive > 0
                    && (Globals.gTacticalStatus.Team[ubTeam].bSide != pSoldier.bSide)
                    && ubTeam != TEAM.CIV_TEAM)
                {
                    for (ubOpp = Globals.gTacticalStatus.Team[ubTeam].bFirstID; ubOpp <= Globals.gTacticalStatus.Team[ubTeam].bLastID; ubOpp++)
                    {
                        pOpponent = Globals.MercPtrs[ubOpp];
                        if (pOpponent.bActive && pOpponent.bInSector && (pOpponent.bLife >= Globals.OKLIFE) && !(pOpponent.bCollapsed))
                        {
                            if (ubInterruptType == NOISEINTERRUPT)
                            {
                                // don't grant noise interrupts at greater than max. visible distance 
                                if (PythSpacesAway(pSoldier.sGridNo, pOpponent.sGridNo) > MaxDistanceVisible())
                                {
                                    pOpponent.bInterruptDuelPts = NO_INTERRUPT;
                                    continue;
                                }
                            }
                            else if (pOpponent.bOppList[pSoldier.ubID] != SEEN_CURRENTLY)
                            {
                                pOpponent.bInterruptDuelPts = NO_INTERRUPT;

                                continue;
                            }

                            switch (pOpponent.bInterruptDuelPts)
                            {
                                case NO_INTERRUPT:      // no interrupt possible, no duel necessary
                                    fIntOccurs = false;
                                    break;

                                case AUTOMATIC_INTERRUPT:   // interrupts occurs automatically
                                    pSoldier.bInterruptDuelPts = 0;    // just to have a valid intDiff later
                                    fIntOccurs = true;
                                    break;

                                default:        // interrupt is possible, run a duel
                                    DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Calculating int duel pts for onlooker in ResolveInterruptsVs");
                                    pSoldier.bInterruptDuelPts = CalcInterruptDuelPts(pSoldier, pOpponent.ubID, true);
                                    fIntOccurs = InterruptDuel(pOpponent, pSoldier);

                                    break;
                            }

                            if (fIntOccurs)
                            {
                                // remember that this opponent's scheduled to interrupt us
                                ubIntList[ubIntCnt] = pOpponent.ubID;

                                // and by how much he beat us in the duel
                                ubIntDiff[ubIntCnt] = pOpponent.bInterruptDuelPts - pSoldier.bInterruptDuelPts;

                                // increment counter of interrupts lost
                                ubIntCnt++;
                            }
                            else
                            {
                                /*
                                    if (pOpponent.bInterruptDuelPts != NO_INTERRUPT)
                                    {
                                        Messages.ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"%d fails to interrupt %d (%d vs %d pts)", pOpponent.ubID, pSoldier.ubID, pOpponent.bInterruptDuelPts, pSoldier.bInterruptDuelPts);
                                    }
                                    */
                            }

                            // either way, clear out both sides' bInterruptDuelPts field to prepare next one
                            pSoldier.bInterruptDuelPts = NO_INTERRUPT;
                            pOpponent.bInterruptDuelPts = NO_INTERRUPT;

                        }

                    }
                }
            }

            // if any interrupts are scheduled to occur (ie. I lost at least once)
            if (ubIntCnt)
            {
                // First add currently active character to the interrupt queue.  This is
                // USUALLY pSoldier.guynum, but NOT always, because one enemy can
                // "interrupt" on another enemy's turn if he hears another team's wound
                // victim's screaming...  the guy screaming is pSoldier here, it's not his turn!
                //AddToIntList( (int) gusSelectedSoldier, false, true);

                if ((Globals.gTacticalStatus.ubCurrentTeam != pSoldier.bTeam) && !(Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bHuman))
                {
                    // if anyone on this team is under AI control, remove 
                    // their AI control flag and put them on the queue instead of this guy
                    for (ubLoop = Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bFirstID; ubLoop <= Globals.gTacticalStatus.Team[Globals.gTacticalStatus.ubCurrentTeam].bLastID; ubLoop++)
                    {
                        if ((Globals.MercPtrs[ubLoop].uiStatusFlags & SOLDIER_UNDERAICONTROL))
                        {
                            // this guy lost control
                            Globals.MercPtrs[ubLoop].uiStatusFlags &= (~SOLDIER_UNDERAICONTROL);
                            AddToIntList(ubLoop, false, true);
                            break;
                        }
                    }

                }
                else
                {
                    // this guy lost control
                    AddToIntList(pSoldier.ubID, false, true);
                }

                // loop once for each opponent who interrupted
                for (ubLoop = 0; ubLoop < ubIntCnt; ubLoop++)
                {
                    // find the smallest intDiff still remaining in the list
                    ubSmallestDiff = NO_INTERRUPT;
                    ubSmallestSlot = Globals.NOBODY;

                    for (ubSlot = 0; ubSlot < ubIntCnt; ubSlot++)
                    {
                        if (ubIntDiff[ubSlot] < ubSmallestDiff)
                        {
                            ubSmallestDiff = ubIntDiff[ubSlot];
                            ubSmallestSlot = ubSlot;
                        }
                    }

                    if (ubSmallestSlot < Globals.NOBODY)
                    {
                        // add this guy to everyone's interrupt queue
                        AddToIntList(ubIntList[ubSmallestSlot], true, true);
                        if (INTERRUPTS_OVER)
                        {
                            // a loop was created which removed all the people in the interrupt queue!
                            EndInterrupt(true);
                            return;
                        }

                        ubIntDiff[ubSmallestSlot] = NO_INTERRUPT;      // mark slot as been handled
                    }
                }

                fControlChanged = true;
            }

            // sends off an end-of-list msg telling everyone whether to switch control,
            // unless it's a MOVEMENT interrupt, in which case that is delayed til later
            DoneAddingToIntList(pSoldier, fControlChanged, ubInterruptType);
        }
    }


    bool SaveTeamTurnsToTheSaveGameFile(Stream hFile)
    {
        UINT32 uiNumBytesWritten;
        TEAM_TURN_SAVE_STRUCT TeamTurnStruct;

        //Save the gubTurn Order Array
        FileWrite(hFile, gubOutOfTurnOrder, sizeof(int) * MAXMERCS, &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int) * MAXMERCS)
        {
            return (false);
        }


        TeamTurnStruct.ubOutOfTurnPersons = gubOutOfTurnPersons;

        TeamTurnStruct.InterruptOnlyGuynum = InterruptOnlyGuynum;
        TeamTurnStruct.sWhoThrewRock = gsWhoThrewRock;
        TeamTurnStruct.InterruptsAllowed = InterruptsAllowed;
        TeamTurnStruct.fHiddenInterrupt = gfHiddenInterrupt;
        TeamTurnStruct.ubLastInterruptedGuy = gubLastInterruptedGuy;


        //Save the Team turn save structure
        FileWrite(hFile, &TeamTurnStruct, sizeof(TEAM_TURN_SAVE_STRUCT), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(TEAM_TURN_SAVE_STRUCT))
        {
            return (false);
        }

        return (true);
    }

    bool LoadTeamTurnsFromTheSavedGameFile(Stream hFile)
    {
        UINT32 uiNumBytesRead;
        TEAM_TURN_SAVE_STRUCT TeamTurnStruct;

        //Load the gubTurn Order Array
        FileRead(hFile, gubOutOfTurnOrder, sizeof(int) * MAXMERCS, &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int) * MAXMERCS)
        {
            return (false);
        }


        //Load the Team turn save structure
        FileRead(hFile, &TeamTurnStruct, sizeof(TEAM_TURN_SAVE_STRUCT), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(TEAM_TURN_SAVE_STRUCT))
        {
            return (false);
        }

        gubOutOfTurnPersons = TeamTurnStruct.ubOutOfTurnPersons;

        InterruptOnlyGuynum = TeamTurnStruct.InterruptOnlyGuynum;
        gsWhoThrewRock = TeamTurnStruct.sWhoThrewRock;
        InterruptsAllowed = TeamTurnStruct.InterruptsAllowed;
        gfHiddenInterrupt = TeamTurnStruct.fHiddenInterrupt;
        gubLastInterruptedGuy = TeamTurnStruct.ubLastInterruptedGuy;


        return (true);
    }

    bool NPCFirstDraw(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pTargetSoldier)
    {
        // if attacking an NPC check to see who draws first!

        if (pTargetSoldier.ubProfile != NO_PROFILE && pTargetSoldier.ubProfile != SLAY && pTargetSoldier.bNeutral && pTargetSoldier.bOppList[pSoldier.ubID] == SEEN_CURRENTLY && (FindAIUsableObjClass(pTargetSoldier, IC_WEAPON) != NO_SLOT))
        {
            int ubLargerHalf, ubSmallerHalf, ubTargetLargerHalf, ubTargetSmallerHalf;

            // roll the dice!
            // e.g. if level 5, roll Random( 3 + 1 ) + 2 for result from 2 to 5
            // if level 4, roll Random( 2 + 1 ) + 2 for result from 2 to 4
            ubSmallerHalf = EffectiveExpLevel(pSoldier) / 2;
            ubLargerHalf = EffectiveExpLevel(pSoldier) - ubSmallerHalf;

            ubTargetSmallerHalf = EffectiveExpLevel(pTargetSoldier) / 2;
            ubTargetLargerHalf = EffectiveExpLevel(pTargetSoldier) - ubTargetSmallerHalf;
            if (gMercProfiles[pTargetSoldier.ubProfile].bApproached & gbFirstApproachFlags[APPROACH_THREATEN - 1])
            {
                // gains 1 to 2 points
                ubTargetSmallerHalf += 1;
                ubTargetLargerHalf += 1;
            }
            if (Random(ubTargetSmallerHalf + 1) + ubTargetLargerHalf > Random(ubSmallerHalf + 1) + ubLargerHalf)
            {
                return (true);
            }
        }
        return (false);
    }
}

public class TEAM_TURN_SAVE_STRUCT
{

    public int ubOutOfTurnPersons;

    public int InterruptOnlyGuynum;
    public int sWhoThrewRock;
    public bool InterruptsAllowed;
    public bool fHiddenInterrupt;
    public int ubLastInterruptedGuy;
    public int[] ubFiller = new int[16];
}


// noise type constants
public enum NOISE
{
    UNKNOWN = 0,
    MOVEMENT,
    CREAKING,
    SPLASHING,
    BULLET_IMPACT,
    GUNFIRE,
    EXPLOSION,
    SCREAM,
    ROCK_IMPACT,
    GRENADE_IMPACT,
    WINDOW_SMASHING,
    DOOR_SMASHING,
    SILENT_ALARM, // only heard by enemies
    MAX_NOISES
};

public enum EXPECTED
{
    NOSEND,    // other nodes expecting noise & have all info
    SEND,      // other nodes expecting noise, but need info
    UNEXPECTED              // other nodes are NOT expecting this noise
};
