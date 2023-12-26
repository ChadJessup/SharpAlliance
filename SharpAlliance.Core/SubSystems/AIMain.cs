using System;
using System.IO;
using System.Linq;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.IsometricUtils;

namespace SharpAlliance.Core.SubSystems;

public class AIMain
{
    //#define TESTAI

    int[] GameOption = { 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0 };

    public static void DebugAI(string szOutput)
    {
# if JA2BETAVERSION
        // Send regular debug msg AND AI debug message
        Stream DebugFile;

        DebugMsg(TOPIC_JA2, DBG_LEVEL_3, szOutput);
        if ((DebugFile = fopen("aidebug.txt", "a+t")) != null)
        {
            fputs(szOutput, DebugFile);
            fputs("\n", DebugFile);
            fclose(DebugFile);
        }
#endif
    }


    public bool InitAI()
    {
        Stream DebugFile;

# if _DEBUG
        if (gfDisplayCoverValues)
        {
            memset(gsCoverValue, 0x7F, sizeof(int) * WORLD_MAX);
        }
#endif

        //If we are not loading a saved game ( if we are, this has already been called )
        if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            //init the panic system
            PanicButtons.InitPanicSystem();
        }

        return true;
    }

    bool AimingGun(SOLDIERTYPE? pSoldier)
    {
        return false;
    }

    void HandleSoldierAI(SOLDIERTYPE? pSoldier)
    {
        uint uiCurrTime = GetJA2Clock();

        // ATE
        // Bail if we are engaged in a NPC conversation/ and/or sequence ... or we have a pause because 
        // we just saw someone... or if there are bombs on the bomb queue
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ENGAGEDINACTION)
            || gTacticalStatus.fEnemySightingOnTheirTurn
            || (gubElementsOnExplosionQueue != 0))
        {
            return;
        }

        if (gfExplosionQueueActive)
        {
            return;
        }

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
        {
            // if we're in autobandage, or the AI control flag is set and the player has a quote record to perform, or is a boxer,
            // let AI process this merc; otherwise abort
            if (!gTacticalStatus.fAutoBandageMode
                && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PCUNDERAICONTROL)
                && (pSoldier.ubQuoteRecord != 0
                || pSoldier.uiStatusFlags.HasFlag(SOLDIER.BOXER))))
            {
                // patch...
                if (pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_HANDLE_EVERY_FRAME))
                {
                    pSoldier.fAIFlags &= ~AIDEFINES.AI_HANDLE_EVERY_FRAME;
                }
                return;
            }

        }
        /*
        else
        {		
            // AI is run on all PCs except the one who is selected
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC )
            {
                // if this soldier is "selected" then only let user give orders!
                if ((pSoldier.ubID == gusSelectedSoldier) && !(gTacticalStatus.uiFlags.HasFlag(DEMOMODE))
                {
                    return;
                }
            }
        }
        */

        // determine what sort of AI to use
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            gfTurnBasedAI = true;
        }
        else
        {
            gfTurnBasedAI = false;
        }

        // If TURN BASED and NOT NPC's turn, or realtime and not our chance to think, bail...
        if (gfTurnBasedAI)
        {
            if ((pSoldier.bTeam != OUR_TEAM) && gTacticalStatus.ubCurrentTeam == gbPlayerNum)
            {
                return;
            }
            // why do we let the quote record thing be in here?  we're in turnbased the quote record doesn't matter,
            // we can't act out of turn!
            if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
            //if ( !(pSoldier.uiStatusFlags.HasFlag(SOLDIER_UNDERAICONTROL) && (pSoldier.ubQuoteRecord == 0))
            {
                return;
            }

            if (pSoldier.bTeam != gTacticalStatus.ubCurrentTeam)
            {
                pSoldier.uiStatusFlags &= ~SOLDIER.UNDERAICONTROL;
                return;
            }
            if (pSoldier.bMoved > 0)
            {
                // this guy doesn't get to act!
                EndAIGuysTurn(pSoldier);
                return;
            }

        }
        else if (!pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_HANDLE_EVERY_FRAME)) // if set to handle every frame, ignore delay!
        {
            //#ifndef AI_PROFILING
            //Time to handle guys in realtime (either combat or not )
            if (!TIMECOUNTERDONE(pSoldier.AICounter, pSoldier.uiAIDelay))
            {
                // CAMFIELD, LOOK HERE!
                return;
            }
            else
            {
                //Reset counter!
                RESETTIMECOUNTER(ref pSoldier.AICounter, pSoldier.uiAIDelay);
                //DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "%s waiting %d from %d", pSoldier.name, pSoldier.AICounter, uiCurrTime ) );
            }
            //#endif
        }

        if (pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_HANDLE_EVERY_FRAME)) // if set to handle every frame, ignore delay!
        {
            if (pSoldier.ubQuoteActionID != QUOTE_ACTION_ID.TURNTOWARDSPLAYER)
            {
                // turn off flag!
                pSoldier.fAIFlags &= ~AIDEFINES.AI_HANDLE_EVERY_FRAME;
            }
        }

        // if this NPC is getting hit, abort
        if (pSoldier.fGettingHit > 0)
        {
            return;
        }

        if (gTacticalStatus.bBoxingState == BoxingStates.PRE_BOXING
            || gTacticalStatus.bBoxingState == BoxingStates.BOXING
            || gTacticalStatus.bBoxingState == BoxingStates.WON_ROUND
            || gTacticalStatus.bBoxingState == BoxingStates.LOST_ROUND)
        {
            if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.BOXER))
            {
                // do nothing!
                EndAIGuysTurn(pSoldier);
                return;
            }
        }

        // if this NPC is dying, bail
        if (pSoldier.bLife < OKLIFE || !pSoldier.bActive)
        {
            if (pSoldier.bActive && pSoldier.fMuzzleFlash)
            {
                OppList.EndMuzzleFlash(pSoldier);
            }

            EndAIGuysTurn(pSoldier);
            return;
        }

        if (pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_ASLEEP))
        {
            if (gfTurnBasedAI && pSoldier.bVisible > 0)
            {
                // turn off sleep flag, guy's got to be able to do stuff in turnbased
                // if he's visible
                pSoldier.fAIFlags &= ~AIDEFINES.AI_ASLEEP;
            }
            else if (!pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_CHECK_SCHEDULE))
            {
                // don't do anything!
                EndAIGuysTurn(pSoldier);
                return;
            }
        }

        if (pSoldier.bInSector == false && !pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_CHECK_SCHEDULE))
        {
            // don't do anything!
            EndAIGuysTurn(pSoldier);
            return;
        }

        if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)
            && !TANK(pSoldier)) || AM_A_ROBOT(pSoldier))
        {
            // bail out!
            EndAIGuysTurn(pSoldier);
            return;
        }

        if (pSoldier.bCollapsed)
        {
            // being handled so turn off muzzle flash
            if (pSoldier.fMuzzleFlash)
            {
                OppList.EndMuzzleFlash(pSoldier);
            }

            // stunned/collapsed!
            CancelAIAction(pSoldier, FORCE);
            EndAIGuysTurn(pSoldier);
            return;
        }

        // in the unlikely situation (Sgt Krott et al) that we have a quote trigger going on
        // during turnbased, don't do any AI
        if (pSoldier.ubProfile != NO_PROFILE
            && (pSoldier.ubProfile == NPCID.SERGEANT || pSoldier.ubProfile == NPCID.MIKE || pSoldier.ubProfile == NPCID.JOE)
            && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && (gfInTalkPanel || gfWaitingForTriggerTimer || !DialogControl.DialogueQueueIsEmpty()))
        {
            return;
        }

        // ATE: Did some changes here 
        // DON'T rethink if we are determined to get somewhere....
        if (pSoldier.bNewSituation == IS_NEW_SITUATION)
        {
            bool fProcessNewSituation;

            // if this happens during an attack then do nothing... wait for the A.B.C.
            // to be reduced to 0 first -- CJC December 13th
            if (gTacticalStatus.ubAttackBusyCount > 0)
            {
                fProcessNewSituation = false;
                // HACK!!
                if (pSoldier.bAction == AI_ACTION.FIRE_GUN)
                {
                    if (guiNumBullets == 0)
                    {
                        // abort attack!
                        //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String(">>>>>> Attack busy count lobotomized due to new situation for %d", pSoldier.ubID ) );
                        //gTacticalStatus.ubAttackBusyCount = 0;
                        fProcessNewSituation = true;
                    }
                }
                else if (pSoldier.bAction == AI_ACTION.TOSS_PROJECTILE)
                {
                    if (guiNumObjectSlots == 0)
                    {
                        // abort attack!
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String(">>>>>> Attack busy count lobotomized due to new situation for %d", pSoldier.ubID));
                        gTacticalStatus.ubAttackBusyCount = 0;
                        fProcessNewSituation = true;
                    }
                }
            }
            else
            {
                fProcessNewSituation = true;
            }

            if (fProcessNewSituation)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL) && pSoldier.ubQuoteActionID >= QUOTE_ACTION_ID.TRAVERSE_EAST && pSoldier.ubQuoteActionID <= QUOTE_ACTION_ID.TRAVERSE_NORTH && !GridNoOnVisibleWorldTile(pSoldier.sGridNo))
                {
                    // traversing offmap, ignore new situations
                }
                else if (pSoldier.ubQuoteRecord == 0 && !gTacticalStatus.fAutoBandageMode)
                {
                    // don't force, don't want escorted mercs reacting to new opponents, etc.
                    // now we don't have AI controlled escorted mercs though - CJC
                    CancelAIAction(pSoldier, FORCE);
                    // zap any next action too
                    if (pSoldier.bAction != AI_ACTION.END_COWER_AND_MOVE)
                    {
                        pSoldier.bNextAction = AI_ACTION.NONE;
                    }

                    DecideAction.DecideAlertStatus(pSoldier);
                }
                else
                {
                    if (pSoldier.ubQuoteRecord > 0)
                    {
                        // make sure we're not using combat AI
                        pSoldier.bAlertStatus = STATUS.GREEN;
                    }
                    pSoldier.bNewSituation = WAS_NEW_SITUATION;
                }
            }
        }
        else
        {
            // might have been in 'was' state; no longer so...
            pSoldier.bNewSituation = NOT_NEW_SITUATION;
        }

        /*********
            Start of new overall AI system
            ********/

        if (gfTurnBasedAI)
        {
            if ((GetJA2Clock() - gTacticalStatus.uiTimeSinceMercAIStart) > DEADLOCK_DELAY && !gfUIInDeadlock)
            {
                // ATE: Display message that deadlock occured...
                // LiveMessage("Breaking Deadlock");

                this.EndAIDeadlock();
                if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
                {
                    return;
                }
            }
        }

        if (pSoldier.bAction == AI_ACTION.NONE)
        {
            // being handled so turn off muzzle flash
            if (pSoldier.fMuzzleFlash)
            {
                OppList.EndMuzzleFlash(pSoldier);
            }

            gubAICounter++;
            // figure out what to do!
            if (gfTurnBasedAI)
            {
                if (pSoldier.fNoAPToFinishMove)
                {
                    // well that move must have been cancelled because we're thinking now!
                    //pSoldier.fNoAPToFinishMove = false;
                }
                this.TurnBasedHandleNPCAI(pSoldier);
            }
            else
            {
//                RTHandleAI(pSoldier);
            }

        }
        else
        {

            // an old action was in progress; continue it
            if (pSoldier.bAction >= FIRST_MOVEMENT_ACTION && pSoldier.bAction <= LAST_MOVEMENT_ACTION && pSoldier.fDelayedMovement == 0)
            {
                if (pSoldier.usPathIndex == pSoldier.usPathDataSize)
                {
                    if (pSoldier.sAbsoluteFinalDestination != NOWHERE)
                    {
                        if (!ACTING_ON_SCHEDULE(pSoldier) && SpacesAway(pSoldier.sGridNo, pSoldier.sAbsoluteFinalDestination) < 4)
                        {
                            // This is close enough... reached final destination for NPC system move				
                            if (pSoldier.sAbsoluteFinalDestination != pSoldier.sGridNo)
                            {
                                // update NPC records to replace our final dest with this location
                                NPC.ReplaceLocationInNPCDataFromProfileID(pSoldier.ubProfile, pSoldier.sAbsoluteFinalDestination, pSoldier.sGridNo);
                            }
                            pSoldier.sAbsoluteFinalDestination = pSoldier.sGridNo;
                            // change action data so that we consider this our final destination below
                            pSoldier.usActionData = pSoldier.sGridNo;
                        }

                        if (pSoldier.sAbsoluteFinalDestination == pSoldier.sGridNo)
                        {
                            pSoldier.sAbsoluteFinalDestination = NOWHERE;

                            if (!ACTING_ON_SCHEDULE(pSoldier) && pSoldier.ubQuoteRecord > 0 && pSoldier.ubQuoteActionID == QUOTE_ACTION_ID.CHECKFORDEST)
                            {
                                NPC.NPCReachedDestination(pSoldier, false);
                                // wait just a little bit so the queue can be processed
                                pSoldier.bNextAction = AI_ACTION.WAIT;
                                pSoldier.usNextActionData = 500;

                            }
                            else if (pSoldier.ubQuoteActionID >= QUOTE_ACTION_ID.TRAVERSE_EAST
                                && pSoldier.ubQuoteActionID <= QUOTE_ACTION_ID.TRAVERSE_NORTH)
                            {
                                this.HandleAITacticalTraversal(pSoldier);
                                return;
                            }
                        }
                        else
                        {
                            // make sure this guy is handled next frame!
                            pSoldier.uiStatusFlags |= (SOLDIER)AIDEFINES.AI_HANDLE_EVERY_FRAME;
                        }
                    }
                    // for regular guys still have to check for leaving the map
                    else if (pSoldier.ubQuoteActionID >= QUOTE_ACTION_ID.TRAVERSE_EAST
                        && pSoldier.ubQuoteActionID <= QUOTE_ACTION_ID.TRAVERSE_NORTH)
                    {
                        this.HandleAITacticalTraversal(pSoldier);
                        return;
                    }

                    // reached destination
# if TESTAI
                    DebugMsg(TOPIC_JA2AI, DBG_LEVEL_0, String("OPPONENT %d REACHES DEST - ACTION DONE", pSoldier.ubID));
#endif

                    if (pSoldier.sGridNo == pSoldier.sFinalDestination)
                    {
                        if (pSoldier.bAction == AI_ACTION.MOVE_TO_CLIMB)
                        {
                            // successfully moved to roof!

                            // fake setting action to climb roof and see if we can afford this
                            pSoldier.bAction = AI_ACTION.CLIMB_ROOF;
                            if (AIUtils.IsActionAffordable(pSoldier))
                            {
                                // set action to none and next action to climb roof so we do that next
                                pSoldier.bAction = AI_ACTION.NONE;
                                pSoldier.bNextAction = AI_ACTION.CLIMB_ROOF;
                            }

                        }
                    }

                    ActionDone(pSoldier);
                }

                //*** TRICK- TAKE INTO ACCOUNT PAUSED FOR NO TIME ( FOR NOW )
                if (pSoldier.fNoAPToFinishMove)
                {
//                    SoldierTriesToContinueAlongPath(pSoldier);
                }
                // ATE: Let's also test if we are in any stationary animation...
                else if (gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY))
                {
                    // ATE: Put some ( MORE ) refinements on here....
                    // If we are trying to open door, or jump fence  don't continue until done...
                    if (!pSoldier.fContinueMoveAfterStanceChange && pSoldier.bEndDoorOpenCode == 0)
                    {
                        //ATE: just a few more.....
                        // If we have ANY pending aninmation that is movement.....
                        if (pSoldier.usPendingAnimation != NO_PENDING_ANIMATION && gAnimControl[pSoldier.usPendingAnimation].uiFlags.HasFlag(ANIM.MOVING))
                        {
                            // Don't do anything, we're waiting on a pending animation....
                        }
                        else
                        {
                            // OK, we have a move to finish...
# if TESTAI
                            DebugMsg(TOPIC_JA2AI, DBG_LEVEL_0, String("GONNA TRY TO CONTINUE PATH FOR %d", pSoldier.ubID));
#endif

//                            SoldierTriesToContinueAlongPath(pSoldier);
                        }
                    }
                }
            }

        }

        /*********
            End of new overall AI system
            ********/

    }

    public static void EndAIGuysTurn(SOLDIERTYPE? pSoldier)
    {
        int ubID;

        if (gfTurnBasedAI)
        {
            if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.PLAYER_TEAM_DEAD))
            {
                TeamTurns.EndAITurn();
                return;
            }

            // search for any player merc to say close call quote
            for (ubID = gTacticalStatus.Team[gbPlayerNum].bFirstID; ubID <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubID++)
            {
                if (OK_INSECTOR_MERC(MercPtrs[ubID]))
                {
                    if (MercPtrs[ubID].fCloseCall > 0)
                    {
                        if (!gTacticalStatus.fSomeoneHit && MercPtrs[ubID].bNumHitsThisTurn == 0 && !MercPtrs[ubID].usQuoteSaidExtFlags.HasFlag(SOLDIER_QUOTE.SAID_EXT_CLOSE_CALL) && Globals.Random.Next(3) == 0)
                        {
                            // say close call quote!
                            DialogControl.TacticalCharacterDialogue(MercPtrs[ubID], QUOTE.CLOSE_CALL);
                            MercPtrs[ubID].usQuoteSaidExtFlags |= SOLDIER_QUOTE.SAID_EXT_CLOSE_CALL;
                        }
                        MercPtrs[ubID].fCloseCall = 0;
                    }
                }
            }
            gTacticalStatus.fSomeoneHit = false;

            // if civ in civ group and hostile, try to change nearby guys to hostile
            if (pSoldier.ubCivilianGroup != CIV_GROUP.NON_CIV_GROUP && pSoldier.bNeutral == 0)
            {

                if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.BOXER)
                    || !(gTacticalStatus.bBoxingState == BoxingStates.PRE_BOXING
                    || gTacticalStatus.bBoxingState == BoxingStates.BOXING))
                {
                    NPCID ubFirstProfile;

//                    ubFirstProfile = CivilianGroupMembersChangeSidesWithinProximity(pSoldier);
//                    if (ubFirstProfile != NPCID.NO_PROFILE)
                    {
//                        TriggerFriendWithHostileQuote(ubFirstProfile);
                    }
                }
            }

            if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ROOFS)
                && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                gTacticalStatus.uiFlags &= ~TacticalEngineStatus.SHOW_ALL_ROOFS;
                RenderWorld.InvalidateWorldRedundency();
            }

            // End this NPC's control, move to next dude
//            EndRadioLocator(pSoldier.ubID);
            pSoldier.uiStatusFlags &= ~SOLDIER.UNDERAICONTROL;
            pSoldier.fTurnInProgress = false;
            pSoldier.bMoved = 1;
            pSoldier.bBypassToGreen = 0;

            // find the next AI guy
//            ubID = RemoveFirstAIListEntry();
            if (ubID != NOBODY)
            {
                StartNPCAI(MercPtrs[ubID]);
                return;
            }

            // We are at the end, return control to next team
            DebugAI("Ending AI turn\n");
            TeamTurns.EndAITurn();
        }
        else
        {
            // realtime
        }
    }



    void EndAIDeadlock()
    {
        int bFound = 0;

        // ESCAPE ENEMY'S TURN			

        // find enemy with problem and free him up...
        foreach (var pSoldier in Menptr)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL))
                {
                    CancelAIAction(pSoldier, FORCE);
# if TESTAICONTROL
                    if (gfTurnBasedAI)
                    {
                        DebugAI(String("Ending turn for %d because breaking deadlock", pSoldier.ubID));
                    }
#endif

                    // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Number of bullets in the air is %ld", guiNumBullets));

                    // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Setting attack busy count to 0 from deadlock break"));
                    gTacticalStatus.ubAttackBusyCount = 0;

                    EndAIGuysTurn(pSoldier);
                    bFound = 1;
                    break;
                }
            }
        }


        if (bFound == 0)
        {
//            StartPlayerTeamTurn(true, false);
        }
    }


    public static void StartNPCAI(SOLDIERTYPE? pSoldier)
    {

        bool fInValidSoldier = false;

        // Only the host should do this
# if NETWORKED
        if (!gfAmIHost)
            return;
#endif


        //pSoldier.uiStatusFlags |= SOLDIER_UNDERAICONTROL;
//        SetSoldierAsUnderAiControl(pSoldier);

        pSoldier.fTurnInProgress = true;

        pSoldier.sLastTwoLocations[0] = NOWHERE;
        pSoldier.sLastTwoLocations[1] = NOWHERE;

        RefreshAI(pSoldier);

# if TESTAICONTROL
        if (!(gTacticalStatus.uiFlags.HasFlag(DEMOMODE))
            DebugAI(String("Giving control to %d", pSoldier.ubID));
#endif

        gTacticalStatus.uiTimeSinceMercAIStart = GetJA2Clock();


        // important: if "fPausedAnimation" is true, then we have to turn it off else
        // HandleSoldierAI() will not be called!

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
//            if (GetNumberInVehicle(pSoldier.bVehicleID) == 0)
            {
                fInValidSoldier = true;
            }
        }

        // Locate to soldier
        // If we are not in an interrupt situation!
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && gubOutOfTurnPersons == 0)
        {
            if (((pSoldier.bVisible != -1 && pSoldier.IsAlive) || gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)) && (fInValidSoldier == false))
            {
                // If we are on a roof, set flag for rendering...
                if (pSoldier.bLevel != 0 && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                {
                    gTacticalStatus.uiFlags |= TacticalEngineStatus.SHOW_ALL_ROOFS;
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                    RenderWorld.InvalidateWorldRedundency();
                }


                //ATE: Changed to show locator

                // Skip locator for green friendly militia
                if (!(pSoldier.bTeam == MILITIA_TEAM
                    && pSoldier.bSide == 0
                    && pSoldier.bAlertStatus == STATUS.GREEN))
                {
                    Overhead.LocateSoldier(pSoldier.ubID, SETLOCATORFAST);
                }

                // try commenting this out altogether
                /*
				// so long as he's not a neutral civ or a militia friendly to the player
				if ( !(pSoldier.bNeutral || (pSoldier.bTeam == MILITIA_TEAM && pSoldier.bSide == 0) ) )
				{
					PauseAITemporarily();
				}
				*/
            }

//            UpdateEnemyUIBar();

        }

        // Remove deadlock message
//        EndDeadlockMsg();
        DecideAction.DecideAlertStatus(pSoldier);

    }



    bool DestNotSpokenFor(SOLDIERTYPE? pSoldier, int sGridno)
    {
        int cnt;

        cnt = gTacticalStatus.Team[pSoldier.bTeam].bFirstID;

        // make a list of all of our team's mercs
        foreach (var pOurTeam in MercPtrs.TakeWhile(_ => cnt <= gTacticalStatus.Team[pSoldier.bTeam].bLastID))
        {
            cnt++;

            if (pOurTeam.bActive)
            {
                if (pOurTeam.sGridNo == sGridno || sGridno == (int)pOurTeam.usActionData)
                {
                    return false;
                }
            }
        }

        return true;  // dest is free to go to...
    }


    int FindAdjacentSpotBeside(SOLDIERTYPE pSoldier, int sGridno)
    {
        int cnt;
        int[] mods = { -1, -MAPWIDTH, 1, MAPWIDTH };
        int sTempGridno, sCheapestCost = 500, sMovementCost, sCheapestDest = NOWHERE;


        for (cnt = 0; cnt < 4; cnt++)
        {
            sTempGridno = sGridno + mods[cnt];
            if (!OutOfBounds(sGridno, sTempGridno))
            {
//                if (Overhead.NewOKDestination(pSoldier, sTempGridno, PEOPLETOO, pSoldier.bLevel) && DestNotSpokenFor(pSoldier, sTempGridno))
                {
                    sMovementCost = PathAI.PlotPath(
                        pSoldier,
                        sTempGridno,
                        null,
                        false,
                        null,
                        AnimationStates.WALKING,
                        null,
                        null,
                        0);

                    if (sMovementCost < sCheapestCost)
                    {
                        sCheapestCost = sMovementCost;
                        sCheapestDest = sTempGridno;
                    }

                }
            }

        }

        return sCheapestDest;
    }

    int GetMostThreateningOpponent(SOLDIERTYPE? pSoldier)
    {
        int uiLoop;
        int iThreatVal = 0, iMinThreat = 30000;
        SOLDIERTYPE? pTargetSoldier;
        int ubTargetSoldier = NO_SOLDIER;

        // Loop through all mercs 

        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pTargetSoldier = MercSlots[uiLoop];

            if (pTargetSoldier is null)
            {
                continue;
            }

            // if this soldier is on same team as me, skip him
            if (pTargetSoldier.bTeam == pSoldier.bTeam || pTargetSoldier.bSide == pSoldier.bSide)
            {
                continue;
            }

            // if potential opponent is dead, skip him
            if (pTargetSoldier.bLife == 0)
            {
                continue;
            }

            if (pSoldier.bOppList[pTargetSoldier.ubID] != SEEN_CURRENTLY)
            {
                continue;
            }

            // Special stuff for Carmen the bounty hunter
            if (pSoldier.bAttitude == Attitudes.ATTACKSLAYONLY && pTargetSoldier.ubProfile != (NPCID)64)
            {
                continue;  // next opponent
            }

//            iThreatVal = CalcManThreatValue(pTargetSoldier, pSoldier.sGridNo, true, pSoldier);
            if (iThreatVal < iMinThreat)
            {
                iMinThreat = iThreatVal;
                ubTargetSoldier = pTargetSoldier.ubID;
            }

        }

        return ubTargetSoldier;
    }



    void FreeUpNPCFromPendingAction(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier is not null)
        {
            if (pSoldier.bAction == AI_ACTION.PENDING_ACTION
                || pSoldier.bAction == AI_ACTION.OPEN_OR_CLOSE_DOOR
                || pSoldier.bAction == AI_ACTION.CREATURE_CALL
                || pSoldier.bAction == AI_ACTION.YELLOW_ALERT
                || pSoldier.bAction == AI_ACTION.RED_ALERT
                || pSoldier.bAction == AI_ACTION.UNLOCK_DOOR
                || pSoldier.bAction == AI_ACTION.PULL_TRIGGER
                || pSoldier.bAction == AI_ACTION.LOCK_DOOR)
            {
                if (pSoldier.ubProfile != NO_PROFILE)
                {
                    if (pSoldier.ubQuoteRecord == NPC_ACTION.KYLE_GETS_MONEY)
                    {
                        // Kyle after getting money
                        pSoldier.ubQuoteRecord = 0;
                        NPC.TriggerNPCRecord(NPCID.KYLE, 11);
                    }
                    else if (pSoldier.usAnimState == AnimationStates.END_OPENSTRUCT)
                    {
//                        TriggerNPCWithGivenApproach(pSoldier.ubProfile, APPROACH.DONE_OPEN_STRUCTURE, true);
                        //TriggerNPCWithGivenApproach( pSoldier.ubProfile, APPROACH_DONE_OPEN_STRUCTURE, false );
                    }
                    else if (pSoldier.usAnimState == AnimationStates.PICKUP_ITEM || pSoldier.usAnimState == AnimationStates.ADJACENT_GET_ITEM)
                    {
//                        TriggerNPCWithGivenApproach(pSoldier.ubProfile, APPROACH.DONE_GET_ITEM, true);
                    }
                }
                ActionDone(pSoldier);
            }
        }
    }

    public static void FreeUpNPCFromAttacking(int ubID)
    {
        SOLDIERTYPE? pSoldier;

        pSoldier = MercPtrs[ubID];
        ActionDone(pSoldier);
        pSoldier.bNeedToLook = 1;

        /*
            if (pSoldier.bActionInProgress)
            { 
        #if TESTAI
                DebugMsg( TOPIC_JA2AI, DBG_LEVEL_0, String( "FreeUpNPCFromAttacking for %d", pSoldier.ubID ) );
        #endif
                if (pSoldier.bAction == FIRE_GUN)
                {
                    if (pSoldier.bDoBurst)
                    {
                        if (pSoldier.bBulletsLeft == 0)
                        {
                            // now find the target and have them say "close call" quote if
                            // applicable
                            pTarget = SimpleFindSoldier( pSoldier.sTargetGridNo, pSoldier.bTargetLevel );
                            if (pTarget && pTarget.bTeam == OUR_TEAM && pTarget.fCloseCall && pTarget.bShock == 0)
                            {
                                // say close call quote!
                                TacticalCharacterDialogue( pTarget, QUOTE_CLOSE_CALL );
                                pTarget.fCloseCall = false;
                            }
                            ActionDone(pSoldier);
                            pSoldier.bDoBurst = false;
                        }		
                    }
                    else
                    {
                        pTarget = SimpleFindSoldier( pSoldier.sTargetGridNo, pSoldier.bTargetLevel );
                        if (pTarget && pTarget.bTeam == OUR_TEAM && pTarget.fCloseCall && pTarget.bShock == 0)
                        {
                            // say close call quote!
                            TacticalCharacterDialogue( pTarget, QUOTE_CLOSE_CALL );
                            pTarget.fCloseCall = false;
                        }
                        ActionDone(pSoldier);	
                    }
                }
                else if ((pSoldier.bAction == TOSS_PROJECTILE) || (pSoldier.bAction == KNIFE_STAB))
                {
                    ActionDone(pSoldier);
                }
            }

            // DO WE NEED THIS???
            //pSoldier.sTarget = NOWHERE;

            // make him look in case he turns to face a new direction
            pSoldier.bNeedToLook = true;

            // This is here to speed up resolution of interrupts that have already been
            // delayed while AttackingPerson was still set (causing ChangeControl to
            // bail).  Without it, an interrupt would have to wait until next ani frame!
            //if (SwitchTo > -1)
            //  ChangeControl();
            */
    }

    void FreeUpNPCFromLoweringGun(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier is not null && pSoldier.bAction == AI_ACTION.LOWER_GUN)
        {
            ActionDone(pSoldier);
        }
    }

    void FreeUpNPCFromTurning(SOLDIERTYPE? pSoldier, int bLook)
    {

        // if NPC is in the process of changing facing, mark him as being done!
        if ((pSoldier.bAction == AI_ACTION.CHANGE_FACING)
            && pSoldier.bActionInProgress > 0)
        {
# if TESTAI
            DebugMsg(TOPIC_JA2AI, DBG_LEVEL_3,
                            String("FREEUPNPCFROMTURNING: our action %d, desdir %d dir %d", pSoldier.bAction, pSoldier.bDesiredDirection, pSoldier.bDirection));
#endif


            ActionDone(pSoldier);

            if (bLook > 0)
            {
                //HandleSight(pSoldier,SIGHT_LOOK | SIGHT_RADIO); // no interrupt possible
            }

        }
    }


    public static void FreeUpNPCFromStanceChange(SOLDIERTYPE? pSoldier)
    {
        // are we/were we doing something?
        if (pSoldier.bActionInProgress > 0)
        {
            // check and see if we were changing stance
            if (pSoldier.bAction == AI_ACTION.CHANGE_STANCE
                || pSoldier.bAction == AI_ACTION.COWER
                || pSoldier.bAction == AI_ACTION.STOP_COWERING)
            {
                // yes we were - are we finished?
                if (gAnimControl[pSoldier.usAnimState].ubHeight == (AnimationHeights)pSoldier.usActionData)
                {
                    // yes! Free us up to do other fun things
                    ActionDone(pSoldier);
                }
            }
        }
    }

    void FreeUpNPCFromRoofClimb(SOLDIERTYPE? pSoldier)
    {
        // are we/were we doing something?
        if (pSoldier.bActionInProgress > 0)
        {
            // check and see if we were climbing
            if (pSoldier.bAction == AI_ACTION.CLIMB_ROOF)
            {
                // yes! Free us up to do other fun things
                ActionDone(pSoldier);
            }
        }
    }




    public static void ActionDone(SOLDIERTYPE? pSoldier)
    {
        // if an action is currently selected
        if (pSoldier.bAction != AI_ACTION.NONE)
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
            {
# if TESTAI
                DebugMsg(TOPIC_JA2AI, DBG_LEVEL_3,
                            String("Cancelling actiondone: our action %d, desdir %d dir %d", pSoldier.bAction, pSoldier.bDesiredDirection, pSoldier.bDirection));
#endif
            }

            // If doing an attack, reset attack busy count and # of bullets
            //if ( gTacticalStatus.ubAttackBusyCount )
            //{
            //	gTacticalStatus.ubAttackBusyCount = 0;
            //	DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String( "Setting attack busy count to 0 due to Action Done" ) );
            //	pSoldier.bBulletsLeft = 0;
            //}

            // cancel any turning & movement by making current settings desired ones
            pSoldier.sFinalDestination = pSoldier.sGridNo;

            if (!pSoldier.fNoAPToFinishMove)
            {
                SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);
//                AdjustNoAPToFinishMove(pSoldier, false);
            }

            // cancel current action
            pSoldier.bLastAction = pSoldier.bAction;
            pSoldier.bAction = AI_ACTION.NONE;
            pSoldier.usActionData = NOWHERE;
            pSoldier.bActionInProgress = 0;
            pSoldier.fDelayedMovement = 0;

            /*
                    if ( pSoldier.bLastAction == CHANGE_STANCE || pSoldier.bLastAction == COWER || pSoldier.bLastAction == STOP_COWERING )
                    {
                        SoldierGotoStationaryStance( pSoldier );
                    }
                    */


            // make sure pathStored is not left true by accident.
            // This is possible if we decide on an action that we have no points for
            // (but which set pathStored).  The action is retained until next turn,
            // although NewDest isn't called.  A newSit. could cancel it before then!
            pSoldier.bPathStored = false;
        }
    }


    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////

    //	O L D    D G    A I    C O D E

    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////
    // ////////////////////////////////////////////////////////////////////////////////////


    // GLOBALS:

    // threat percentage is based on the certainty of opponent knowledge:
    // opplist value:        -4  -3  -2  -1 SEEN  1    2   3   4   5
    public static int[] ThreatPercent = { 20, 40, 60, 80, 25, 100, 90, 75, 60, 45 };

    public static void NPCDoesAct(SOLDIERTYPE pSoldier)
    {
        // if the action is visible and we're in a hidden turnbased mode, go to turnbased
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && (pSoldier.bAction == AI_ACTION.FIRE_GUN || pSoldier.bAction == AI_ACTION.TOSS_PROJECTILE || pSoldier.bAction == AI_ACTION.KNIFE_MOVE || pSoldier.bAction == AI_ACTION.KNIFE_STAB || pSoldier.bAction == AI_ACTION.THROW_KNIFE)))
        {
//            DisplayHiddenTurnbased(pSoldier);
        }

        if (gfHiddenInterrupt)
        {
//            DisplayHiddenInterrupt(pSoldier);
        }
        //StartInterruptVisually(pSoldier.ubID);
        // *** IAN deleted lots of interrupt related code here to simplify JA2	development

        // CJC Feb 18 99: make sure that soldier is not in the middle of a turn due to visual crap to make enemies
        // face and point their guns at us
        if (pSoldier.bDesiredDirection != pSoldier.bDirection)
        {
            pSoldier.bDesiredDirection = pSoldier.bDirection;
        }
    }



    void NPCDoesNothing(SOLDIERTYPE? pSoldier)
    {
        // NPC, for whatever reason, did/could not start an action, so end his turn
        //pSoldier.moved = true;

# if TESTAICONTROL
        if (gfTurnBasedAI)
        {
            DebugAI(String("Ending turn for %d because doing no-action", pSoldier.ubID));
        }
#endif

        EndAIGuysTurn(pSoldier);

        // *** IAN deleted lots of interrupt related code here to simplify JA2	development
    }




    public static void CancelAIAction(SOLDIERTYPE? pSoldier, int ubForce)
    {
        // re-enable cover checking, something is new or something strange happened
        SkipCoverCheck = 0;

        // turn off new situation flag to stop this from repeating all the time!
        if (pSoldier.bNewSituation == IS_NEW_SITUATION)
        {
            pSoldier.bNewSituation = WAS_NEW_SITUATION;
        }

        // NPCs getting escorted do NOT react to new situations, unless forced!
        if (pSoldier.bUnderEscort > 0 && ubForce == 0)
        {
            return;
        }

        // turn off RED/YELLOW status "bypass to Green", to re-check all actions
        pSoldier.bBypassToGreen = 0;

        ActionDone(pSoldier);
    }




    /*
    void ActionTimeoutExceeded(SOLDIERTYPE *pSoldier, UCHAR alreadyFreedUp)
    {
     int cnt;
     UCHAR attackAction = false;


    #if BETAVERSION
     if (ConvertedMultiSave)
      {
       // re-start real-time NPC action timer
       EnemyTimedOut = false;
       EnemyTimerCnt = ENEMYWAITTOLERANCE;
       return;
      }
    #endif


     // check if it's a problem with a offensive combat action
     if ((pSoldier.bAction == FIRE_GUN) ||
         (pSoldier.bAction == TOSS_PROJECTILE) ||
         (pSoldier.bAction == KNIFE_STAB))
      {
       // THESE ARE LESS SERIOUS, SINCE THEY LIKELY WON'T REPEAT THEMSELVES
       attackAction = true;
      }
       // OTHERS ARE VERY SERIOUS, SINCE THEY ARE LIKELY TO REPEAT THEMSELVES


    #if BETAVERSION
     sprintf(tempstr,"ActionInProgress - ERROR: %s's timeout limit exceeded.  Action #%d (%d)",
            pSoldier.name,pSoldier.bAction,pSoldier.usActionData);

    #if RECORDNET
     fprintf(NetDebugFile,"\n%s\n\n",tempstr);
    #endif

     PopMessage(tempstr);
     SaveGame(ERROR_SAVE);
    #endif

    #if TESTVERSION
     PopMessage("FULL SOLDIER INFORMATION DUMP COMING UP, BRACE THYSELF!");
     DumpSoldierInfo(pSoldier);
    #endif


     // re-start real-time NPC action timer
     EnemyTimedOut = false;
     EnemyTimerCnt = ENEMYWAITTOLERANCE;

     if (attackAction)
      {
    #if BETAVERSION
       NameMessage(pSoldier,"will now be freed up from attacking...",2000);
    #endif


       // free up ONLY players from whom we haven't received an DONE yet
       // we can all agree the action is DONE and we can continue...
       // (otherwise they'll be calling FreeUp... twice and get REAL screwed up)
       NetSend.msgType = NET_FREE_UP_ATTACK;
       NetSend.ubID  = pSoldier.ubID;

       for (cnt = 0; cnt < MAXPLAYERS; cnt++)
        {
         if ((cnt != Net.pnum) && Net.player[cnt].playerActive &&
         (Net.player[cnt].actionDone != pSoldier.ubID))
           SendNetData(cnt);
        }

       if (!alreadyFreedUp)
         FreeUpManFromAttacking(pSoldier.ubID,COMMUNICATE);
      }
     else if (pSoldier.bAction == CHANGE_FACING)
      {
    #if BETAVERSION
       NameMessage(pSoldier,"will now be freed up from turning...",2000);
    #endif

       // force him to face in the right direction (as long as it's legal)
       if ((pSoldier.bDesiredDirection >= 1) && (pSoldier.bDesiredDirection <= 8))
         pSoldier.bDirection = pSoldier.bDesiredDirection;
       else
         pSoldier.bDesiredDirection = pSoldier.bDirection;

       // free up ONLY players from whom we haven't received an DONE yet
       // we can all agree the action is DONE and we can continue...
       // (otherwise they'll be calling FreeUp... twice and get REAL screwed up)
       NetSend.msgType    = NET_FREE_UP_TURN;
       NetSend.ubID     = pSoldier.ubID;
       NetSend.misc_UCHAR = pSoldier.bDirection;
       NetSend.answer     = pSoldier.bDesiredDirection;

       for (cnt = 0; cnt < MAXPLAYERS; cnt++)
        {
         if ((cnt != Net.pnum) && Net.player[cnt].playerActive &&
         (Net.player[cnt].actionDone != pSoldier.ubID))
           SendNetData(cnt);
        }

       if (!alreadyFreedUp)
         // this calls FreeUpManFromTurning()
         NowFacingRightWay(pSoldier,COMMUNICATE);
      }
     else
      {
    #if BETAVERSION
       NameMessage(pSoldier,"is having the remainder of his turn canceled...",1000);
    #endif

       // cancel the remainder of the offender's turn as a penalty!
       pSoldier.bActionPoints = 0;
       NPCDoesNothing(pSoldier);
      }


     // cancel whatever the current action is, force this even for escorted NPCs
     CancelAIAction(pSoldier,FORCE);


     // reset the timeout counter for next time
     pSoldier.bActionTimeout = 0;
    }
    */




    int ActionInProgress(SOLDIERTYPE? pSoldier)
    {
        // if NPC has a desired destination, but isn't currently going there
        if ((pSoldier.sFinalDestination != NOWHERE) && (pSoldier.sDestination != pSoldier.sFinalDestination))
        {
            // return success (true) if we successfully resume the movement
            return 0;//(TryToResumeMovement(pSoldier, pSoldier.sFinalDestination));
        }


        // this here should never happen, but it seems to (turns sometimes hang!)
        if ((pSoldier.bAction == AI_ACTION.CHANGE_FACING) && (pSoldier.bDesiredDirection != (WorldDirections)pSoldier.usActionData))
        {
            // don't try to pay any more APs for this, it was paid for once already!
            pSoldier.bDesiredDirection = (WorldDirections)pSoldier.usActionData;   // turn to face direction in actionData
            return 1;
        }


        // needs more time to complete action
        return 1;
    }





    /*
    void RestoreMarkedMines()
    {
     int gridno;

     // all tiles marked with the special NPC mine cost value must be restored
     for (gridno = 0; gridno < GRIDSIZE; gridno++)
      {
       if (GridCost[gridno] == NPCMINECOST)
        {
         GridCost[gridno] = BackupGridCost[gridno];

    #if TESTMINEMARKING
         fprintf(NetDebugFile,"\tRestoring marked mine at gridno %d back to gridCost %d\n",gridno,BackupGridCost[gridno]);
    #endif
        }
      }

     MarkedNPCMines = false;
    }



    void MarkDetectableMines(SOLDIERTYPE *pSoldier)
    {
     int gridno,detectLevel;
     GRIDINFO *gpSoldier;


     // this should happen, means we missed a clean-up cycle last time!
     if (MarkedNPCMines)
      {
    #if BETAVERSION
       sprintf(tempstr,"MarkDetectableMines: ERROR - mines still marked!  Guynum %d",pSoldier.ubID);

    #if RECORDNET
       fprintf(NetDebugFile,"\n\t%s\n\n",tempstr);
    #endif

       PopMessage(tempstr);
    #endif

       RestoreMarkedMines();
      }


     // make a backup of the current gridcosts
     memcpy(BackupGridCost,GridCost,sizeof(GridCost));

     // calculate what "level" of mines we are able to detect
     detectLevel = CalcMineDetectLevel(pSoldier);


     // check every tile, looking for BURIED mines only
     for (gridno = 0,gpSoldier = &Grid[0]; gridno < GRIDSIZE; gridno++,gpSoldier++)
      {
       // if there's a valid object there, and it is still "buried"
       if ((gpSoldier.object < 255) &&
           (ObjList[gpSoldier.object].visible == BURIED) &&
           (ObjList[gpSoldier.object].item == MINE))
        {
         // are we bright enough to detect it (should we get there) ?
         if (detectLevel >= ObjList[gpSoldier.object].trap)
          {
           // bingo!  Mark it as "unpassable" for the purposes of the path AI
           GridCost[gridno] = NPCMINECOST;
           MarkedNPCMines = true;

    #if TESTMINEMARKING
           fprintf(NetDebugFile,"\tNPC %d, dtctLvl %d, marking mine at gridno %d, gridCost was %d\n",pSoldier.ubID,detectLevel,gridno,BackupGridCost[gridno]);
    #endif
          }
        }
      }
    }

    */




    void TurnBasedHandleNPCAI(SOLDIERTYPE? pSoldier)
    {


        /*
         if (Status.gamePaused)
          {
        #if DEBUGBUSY
           DebugAI("HandleManAI - Skipping %d, the game is paused\n",pSoldier.ubID);
        #endif

           return;
          }
        //

         // If man is inactive/at base/dead/unconscious
         if (!pSoldier.bActive || !pSoldier.bInSector || (pSoldier.bLife < OKLIFE))
          {
        #if DEBUGDECISIONS
           AINumMessage("HandleManAI - Unavailable man, skipping guy#",pSoldier.ubID);
        #endif

           NPCDoesNothing(pSoldier);
           return;
          }

         if (PTR_CIVILIAN && pSoldier.service &&
             (pSoldier.bNeutral || MedicsMissionIsEscort(pSoldier)))
          {
        #if DEBUGDECISIONS
           AINumMessage("HandleManAI - Civilian is being serviced, skipping guy#",pSoldier.ubID);
        #endif

           NPCDoesNothing(pSoldier);
           return;
          }
        */



        /*
        anim = pSoldier.anitype[pSoldier.anim];

        // If man is down on the ground
        if (anim < BREATHING)
         {
          // if he lacks the breath, or APs to get up this turn (life checked above)
          // OR... (new June 13/96 Ian) he's getting first aid...
          if ((pSoldier.bBreath < OKBREATH) || (pSoldier.bActionPoints < (AP_GET_UP + AP.ROLL_OVER))
              || pSoldier.service)
           {
       #if RECORDNET
            fprintf(NetDebugFile,"\tAI: %d can't get up (breath %d, AP %d), ending his turn\n",
               pSoldier.ubID,pSoldier.bBreath,pSoldier.bActionPoints);
       #endif
       #if DEBUGDECISIONS
            AINumMessage("HandleManAI - CAN'T GET UP, skipping guy #",pSoldier.ubID);
       #endif

            NPCDoesNothing(pSoldier);
            return;
           }
          else
           {
            // wait until he gets up first, only then worry about deciding his AI

       #if RECORDNET
            fprintf(NetDebugFile,"\tAI: waiting for %d to GET UP (breath %d, AP %d)\n",
               pSoldier.ubID,pSoldier.bBreath,pSoldier.bActionPoints);
       #endif

       #if DEBUGBUSY
            AINumMessage("HandleManAI - About to get up, skipping guy#",pSoldier.ubID);
       #endif

            return;
           }
         }


        // if NPC's has been forced to stop by an opponent's interrupt or similar
        if (pSoldier.forcedToStop)
         {
       #if DEBUGBUSY
          AINumMessage("HandleManAI - Forced to stop, skipping guy #",pSoldier.ubID);
       #endif

          return;
         }

        // if we are still in the midst in an uninterruptable animation
        if (!AnimControl[anim].interruptable)
         {
       #if DEBUGBUSY
          AINumMessage("HandleManAI - uninterruptable animation, skipping guy #",pSoldier.ubID);
       #endif

          return;      // wait a while, let the animation finish first
         }

       */

        // yikes, this shouldn't occur! we should be trying to finish our move!
        // pSoldier.fNoAPToFinishMove = false;

        // unless in mid-move, get an up-to-date alert status for this guy
        if (pSoldier.bStopped > 0)
        {
            // if active team is waiting for oppChanceToDecide, that means we have NOT
            // had a chance to go through NewSelectedNPC(), so do the refresh here
            /* 
            ???
            if (gTacticalStatus.team[Net.turnActive].allowOppChanceToDecide)
            {
                // if mines are still marked (this could happen if we also control the
                // active team that's potentially BEING interrupted), unmark them
                //RestoreMarkedMines();

                RefreshAI(pSoldier);
            }
            else
            {
                DecideAlertStatus(pSoldier);
            }
            */
        }

        /*
            // move this clause outside of the function...
            if (pSoldier.bNewSituation)
                // don't force, don't want escorted mercs reacting to new opponents, etc.
                CancelAIAction(pSoldier,DONTFORCE);

        */


        /*
        if (!pSoldier.stopped)
         {
       #if DEBUGBUSY
          AINumMessage("HandleManAI - Moving, skipping guy#",pSoldier.ubID);
       #endif

          return;
         }
       */



        if ((pSoldier.bAction != AI_ACTION.NONE) && pSoldier.bActionInProgress > 0)
        {
            /*
                        if (pSoldier.bAction == RANDOM_PATROL)
                        {
                            if (pSoldier.usPathIndex == pSoldier.usPathDataSize)
                            //if (pSoldier.usActionData == pSoldier.sGridNo )
                            //(IC?) if (pSoldier.bAction == RANDOM_PATROL && ( pSoldier.usPathIndex == pSoldier.usPathDataSize ) )
                            //(old?) if (pSoldier.bAction == RANDOM_PATROL && ( pSoldier.usActionData == pSoldier.sGridNo ) )
                            {
                #if TESTAI
                                DebugMsg( TOPIC_JA2AI, DBG_LEVEL_0, String("OPPONENT %d REACHES DEST - ACTION DONE",pSoldier.ubID ) );
                #endif
                                ActionDone(pSoldier);
                            }

                            //*** TRICK- TAKE INTO ACCOUNT PAUSED FOR NO TIME ( FOR NOW )
                            if (pSoldier.fNoAPToFinishMove)
                            //if (pSoldier.bAction == RANDOM_PATROL && pSoldier.fNoAPToFinishMove)
                            {
                                // OK, we have a move to finish...

                #if TESTAI
                                DebugMsg( TOPIC_JA2AI, DBG_LEVEL_0, String("GONNA TRY TO CONTINUE PATH FOR %d", pSoldier.ubID ) );
                #endif

                                SoldierTriesToContinueAlongPath(pSoldier);

                                // since we just gave up on our action due to running out of points, better end our turn
                                //EndAIGuysTurn(pSoldier);
                            }
                        }
            */

            // if action should remain in progress
            if (this.ActionInProgress(pSoldier) > 0)
            {
                // let it continue
                return;
            }
        }

        // if man has nothing to do
        if (pSoldier.bAction == AI_ACTION.NONE)
        {
            // make sure this flag is turned off (it already should be!)
            pSoldier.bActionInProgress = 0;

            // Since we're NEVER going to "continue" along an old path at this point,
            // then it would be nice place to reinitialize "pathStored" flag for
            // insurance purposes.
            //
            // The "pathStored" variable controls whether it's necessary to call
            // findNewPath() after you've called NewDest(). Since the AI calls
            // findNewPath() itself, a speed gain can be obtained by avoiding
            // redundancy.
            //
            // The "normal" way for pathStored to be reset is inside
            // SetNewCourse() [which gets called after NewDest()].
            //
            // The only reason we would NEED to reinitialize it here is if I've
            // incorrectly set pathStored to true in a process that doesn't end up
            // calling NewDest()
            pSoldier.bPathStored = false;

            // decide on the next action
            if (pSoldier.bNextAction != AI_ACTION.NONE)
            {
                // do the next thing we have to do...
                if (pSoldier.bNextAction == AI_ACTION.END_COWER_AND_MOVE)
                {
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                    {
                        pSoldier.bAction = AI_ACTION.STOP_COWERING;
                        pSoldier.usActionData = (int)AnimationHeights.ANIM_STAND;
                    }
                    else if (gAnimControl[pSoldier.usAnimState].ubEndHeight < AnimationHeights.ANIM_STAND)
                    {
                        // stand up!
                        pSoldier.bAction = AI_ACTION.CHANGE_STANCE;
                        pSoldier.usActionData = (int)AnimationHeights.ANIM_STAND;
                    }
                    else
                    {
                        pSoldier.bAction = AI_ACTION.NONE;
                    }
                    if (pSoldier.sGridNo == pSoldier.usNextActionData)
                    {
                        // no need to walk after this
                        pSoldier.bNextAction = AI_ACTION.NONE;
                        pSoldier.usNextActionData = NOWHERE;
                    }
                    else
                    {
                        pSoldier.bNextAction = AI_ACTION.WALK;
                        // leave next-action-data as is since that's where we want to go
                    }
                }
                else
                {
                    pSoldier.bAction = pSoldier.bNextAction;
                    pSoldier.usActionData = pSoldier.usNextActionData;
                    pSoldier.bTargetLevel = pSoldier.bNextTargetLevel;
                    pSoldier.bNextAction = AI_ACTION.NONE;
                    pSoldier.usNextActionData = 0;
                    pSoldier.bNextTargetLevel = 0;
                }
                if (pSoldier.bAction == AI_ACTION.PICKUP_ITEM)
                {
                    // the item pool index was stored in the special data field
                    pSoldier.uiPendingActionData1 = pSoldier.iNextActionSpecialData;
                }
            }
            else if (pSoldier.sAbsoluteFinalDestination != NOWHERE)
            {
                if (ACTING_ON_SCHEDULE(pSoldier))
                {
                    pSoldier.bAction = AI_ACTION.SCHEDULE_MOVE;
                }
                else
                {
                    pSoldier.bAction = AI_ACTION.WALK;
                }
                pSoldier.usActionData = pSoldier.sAbsoluteFinalDestination;
            }
            else
            {
                if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.ENGAGED_IN_CONV))
                {
                    if (CREATURE_OR_BLOODCAT(pSoldier))
                    {
//                        pSoldier.bAction = CreatureDecisions(pSoldier);
                    }
                    else if (pSoldier.ubBodyType == SoldierBodyTypes.CROW)
                    {
                        pSoldier.bAction = CreatureDecisions.CrowDecideAction(pSoldier);
                    }
                    else
                    {
//                        pSoldier.bAction = DecideAction(pSoldier);
                    }
                }
            }

            if (pSoldier.bAction == AI_ACTION.ABSOLUTELY_NONE)
            {
                pSoldier.bAction = AI_ACTION.NONE;
            }

            // if he chose to continue doing nothing
            if (pSoldier.bAction == AI_ACTION.NONE)
            {
                this.NPCDoesNothing(pSoldier);  // sets pSoldier.moved to true
                return;
            }



            /*
            // if we somehow just caused an uninterruptable animation to occur
            // This is mainly to finish a weapon_AWAY anim that preceeds a TOSS attack
            if (!AnimControl[ pSoldier.anitype[pSoldier.anim] ].interruptable)
             {
           #if DEBUGBUSY
              DebugAI( String( "Uninterruptable animation %d, skipping guy %d",pSoldier.anitype[pSoldier.anim],pSoldier.ubID ) );
           #endif

              return;      // wait a while, let the animation finish first
             }
               */

            // to get here, we MUST have an action selected, but not in progress...

            // see if we can afford to do this action
            if (AIUtils.IsActionAffordable(pSoldier))
            {
                NPCDoesAct(pSoldier);

                // perform the chosen action
                pSoldier.bActionInProgress = ExecuteAction(pSoldier); // if started, mark us as busy

                if (pSoldier.bActionInProgress == 0 && pSoldier.sAbsoluteFinalDestination != NOWHERE)
                {
                    // turn based... abort this guy's turn
                    EndAIGuysTurn(pSoldier);
                }
            }
            else
            {
                Movement.HaltMoveForSoldierOutOfPoints(pSoldier);
                return;
            }
        }
    }


    public static void RefreshAI(SOLDIERTYPE? pSoldier)
    {
        // produce our own private "mine map" so we can avoid the ones we can detect
        // MarkDetectableMines(pSoldier);

        // whether last attack hit or not doesn't matter once control has been lost
        pSoldier.bLastAttackHit = 0;

        // get an up-to-date alert status for this guy
        DecideAction.DecideAlertStatus(pSoldier);

        if (pSoldier.bAlertStatus == STATUS.YELLOW)
        {
            SkipCoverCheck = 0;
        }

        // if he's in battle or knows opponents are here
        if (gfTurnBasedAI)
        {
            if ((pSoldier.bAlertStatus == STATUS.BLACK)
                || (pSoldier.bAlertStatus == STATUS.RED))
            {
                // always freshly rethink things at start of his turn
                pSoldier.bNewSituation = IS_NEW_SITUATION;
            }
            else
            {
                // make sure any paths stored during out last AI decision but not reacted
                // to (probably due to lack of APs) get re-tested by the ExecuteAction()
                // function in AI, since the .sDestination may no longer be legal now!
                pSoldier.bPathStored = false;

                // if not currently engaged, or even alerted
                // take a quick look around to see if any friends seem to be in trouble
                ManChecksOnFriends(pSoldier);

                // allow stationary GREEN Civilians to turn again at least 1/turn!
            }
            pSoldier.bLastAction = AI_ACTION.NONE;

        }
    }


    public static void AIDecideRadioAnimation(SOLDIERTYPE? pSoldier)
    {
        if (pSoldier.ubBodyType != SoldierBodyTypes.REGMALE && pSoldier.ubBodyType != SoldierBodyTypes.BIGMALE)
        {
            // no animation available
            ActionDone(pSoldier);
            return;
        }

        if (PTR_CIVILIAN(pSoldier) && pSoldier.ubCivilianGroup != CIV_GROUP.KINGPIN_CIV_GROUP)
        {
            // don't play anim
            ActionDone(pSoldier);
            return;
        }

        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                SoldierControl.EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.AI_RADIO, 0, false);
                break;

            case AnimationHeights.ANIM_CROUCH:

                SoldierControl.EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.AI_CR_RADIO, 0, false);
                break;

            case AnimationHeights.ANIM_PRONE:

                ActionDone(pSoldier);
                break;
        }
    }


    public static int ExecuteAction(SOLDIERTYPE? pSoldier)
    {
        ITEM_HANDLE iRetCode = 0;
        //NumMessage("ExecuteAction - Guy#",pSoldier.ubID);

        // in most cases, merc will change location, or may cause damage to opponents,
        // so a new cover check will be necessary.  Exceptions handled individually.
        SkipCoverCheck = 0;

        // reset this field, too
        pSoldier.bLastAttackHit = 0;

# if TESTAICONTROL
        if (gfTurnBasedAI || gTacticalStatus.fAutoBandageMode)
        {
            DebugAI(String("%d does %s (a.d. %d) in %d with %d APs left", pSoldier.ubID, gzActionStr[pSoldier.bAction], pSoldier.usActionData, pSoldier.sGridNo, pSoldier.bActionPoints));
        }
#endif

//        DebugAI(string.Format("%d does %s (a.d. %d) at time %ld", pSoldier.ubID, gzActionStr[pSoldier.bAction], pSoldier.usActionData, GetJA2Clock()));

        switch (pSoldier.bAction)
        {
            case AI_ACTION.NONE:                  // maintain current position & facing
                                                  // do nothing
                break;

            case AI_ACTION.WAIT:                                     // hold NONE for a specified time
                if (gfTurnBasedAI)
                {
                    // probably an action set as a next-action in the realtime prior to combat
                    // do nothing
                }
                else
                {
                    RESETTIMECOUNTER(ref pSoldier.AICounter, (uint)pSoldier.usActionData);
                    if (pSoldier.ubProfile != NO_PROFILE)
                    {
                        //DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "%s waiting %d from %d", pSoldier.name, pSoldier.AICounter, GetJA2Clock() ) );
                    }
                }
                ActionDone(pSoldier);
                break;

            case AI_ACTION.CHANGE_FACING:         // turn this way & that to look
                                                  // as long as we don't see anyone new, cover won't have changed
                                                  // if we see someone new, it will cause a new situation & remove this
                SkipCoverCheck = 1;

# if DEBUGDECISIONS
                DebugAI(String("ExecuteAction: SkipCoverCheck ON\n"));
#endif

                //			pSoldier.bDesiredDirection = (int) ;   // turn to face direction in actionData
//                SendSoldierSetDesiredDirectionEvent(pSoldier, pSoldier.usActionData);
                // now we'll have to wait for the turning to finish; no need to call TurnSoldier here
                //TurnSoldier( pSoldier );
                /*
                            if (!StartTurn(pSoldier,pSoldier.usActionData,FASTTURN))
                            {
                #if BETAVERSION
                                sprintf(tempstr,"ERROR: %s tried TURN to direction %d, StartTurn failed, action %d CANCELED",
                                        pSoldier.name,pSoldier.usActionData,pSoldier.bAction);
                                PopMessage(tempstr);
                #endif

                                // ZAP NPC's remaining action points so this isn't likely to repeat
                                pSoldier.bActionPoints = 0;

                                CancelAIAction(pSoldier,FORCE);
                                return(false);         // nothing is in progress
                            }
                            else
                            {
                #if RECORDNET
                                fprintf(NetDebugFile,"\tAI decides to turn guynum %d to dir %d\n",pSoldier.ubID,pSoldier.usActionData);
                #endif
                                NetLookTowardsDir(pSoldier,pSoldier.usActionData);
                            }
                            */
                break;

            case AI_ACTION.PICKUP_ITEM:                  // grab something!
//                SoldierPickupItem(pSoldier, pSoldier.uiPendingActionData1, pSoldier.usActionData, 0);
                break;

            case AI_ACTION.DROP_ITEM:                    // drop item in hand
//                SoldierDropItem(pSoldier, (pSoldier.inv[InventorySlot.HANDPOS]));
                ItemSubSystem.DeleteObj(pSoldier.inv[InventorySlot.HANDPOS]);
                pSoldier.bAction = AI_ACTION.PENDING_ACTION;
                break;

            case AI_ACTION.MOVE_TO_CLIMB:
                if ((int)pSoldier.usActionData == pSoldier.sGridNo)
                {
                    // change action to climb now and try that.
                    pSoldier.bAction = AI_ACTION.CLIMB_ROOF;
                    if (AIUtils.IsActionAffordable(pSoldier))
                    {
                        return ExecuteAction(pSoldier);
                    }
                    else
                    {
                        // no action started
                        return 0;
                    }
                }
                break;
            // fall through			
            case AI_ACTION.RANDOM_PATROL:         // move towards a particular location
            case AI_ACTION.SEEK_FRIEND:           // move towards friend in trouble
            case AI_ACTION.SEEK_OPPONENT:         // move towards a reported opponent
            case AI_ACTION.TAKE_COVER:            // run for nearest cover from threat
            case AI_ACTION.GET_CLOSER:            // move closer to a strategic location

            case AI_ACTION.POINT_PATROL:          // move towards next patrol point
            case AI_ACTION.LEAVE_WATER_GAS:       // seek nearest spot of ungassed land
            case AI_ACTION.SEEK_NOISE:            // seek most important noise heard
            case AI_ACTION.RUN_AWAY:              // run away from nearby opponent(s)

            case AI_ACTION.APPROACH_MERC:                // walk up to someone to talk
            case AI_ACTION.TRACK:                                // track by ground scent
            case AI_ACTION.EAT:                                  // monster approaching corpse
            case AI_ACTION.SCHEDULE_MOVE:
            case AI_ACTION.WALK:
            case AI_ACTION.RUN:

                if (gfTurnBasedAI && pSoldier.bAlertStatus < STATUS.BLACK)
                {
                    if (pSoldier.sLastTwoLocations[0] == NOWHERE)
                    {
                        pSoldier.sLastTwoLocations[0] = pSoldier.sGridNo;
                    }
                    else if (pSoldier.sLastTwoLocations[1] == NOWHERE)
                    {
                        pSoldier.sLastTwoLocations[1] = pSoldier.sGridNo;
                    }
                    // check for loop
                    else if ((int)pSoldier.usActionData == pSoldier.sLastTwoLocations[1] && pSoldier.sGridNo == pSoldier.sLastTwoLocations[0])
                    {
                        DebugAI(string.Format("%d in movement loop, aborting turn", pSoldier.ubID));

                        // loop found!
                        ActionDone(pSoldier);
                        EndAIGuysTurn(pSoldier);
                    }
                    else
                    {
                        pSoldier.sLastTwoLocations[0] = pSoldier.sLastTwoLocations[1];
                        pSoldier.sLastTwoLocations[1] = pSoldier.sGridNo;
                    }
                }

                // Randomly do growl...
                if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
                {
                    if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        if (Globals.Random.Next(2) == 0)
                        {
                            // PlaySoldierJA2Sample(pSoldier.ubID, (BLOODCAT_GROWL_1 + Globals.Random.Next(4)), RATE_11025, SoundVolume(HIGHVOLUME, pSoldier.sGridNo), 1, SoundDir(pSoldier.sGridNo), true);
                        }
                    }
                }

                // on YELLOW/GREEN status, NPCs keep the actions from turn to turn
                // (newSituation is intentionally NOT set in NewSelectedNPC()), so the
                // possibility exists that NOW the actionData is no longer a valid
                // NPC .sDestination (path got blocked, someone is now standing at that
                // gridno, etc.)  So we gotta check again that the .sDestination's legal!

                // optimization - Ian (if up-to-date path is known, do not check again)
                if (!pSoldier.bPathStored)
                {
                    if ((pSoldier.sAbsoluteFinalDestination != NOWHERE || gTacticalStatus.fAutoBandageMode) && !gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        // NPC system move, allow path through
                        if (Movement.LegalNPCDestination(pSoldier, (int)pSoldier.usActionData, ENSURE_PATH, WATEROK, PATH.THROUGH_PEOPLE) > 0)
                        {
                            // optimization - Ian: prevent another path call in SetNewCourse()
                            pSoldier.bPathStored = true;
                        }
                    }
                    else
                    {
                        if (Movement.LegalNPCDestination(pSoldier, (int)pSoldier.usActionData, ENSURE_PATH, WATEROK, 0) > 0)
                        {
                            // optimization - Ian: prevent another path call in SetNewCourse()
                            pSoldier.bPathStored = true;
                        }
                    }

                    // if we STILL don't have a path
                    if (!pSoldier.bPathStored)
                    {
                        // Check if we were told to move by NPC stuff
                        if (pSoldier.sAbsoluteFinalDestination != NOWHERE && !gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                        {
                            //ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG.ERROR, "AI %s failed to get path for dialogue-related move!", pSoldier.name );

                            // Are we close enough?
                            if (!ACTING_ON_SCHEDULE(pSoldier) && SpacesAway(pSoldier.sGridNo, pSoldier.sAbsoluteFinalDestination) < 4)
                            {
                                // This is close enough...
                                NPC.ReplaceLocationInNPCDataFromProfileID(pSoldier.ubProfile, pSoldier.sAbsoluteFinalDestination, pSoldier.sGridNo);
//                                NPCGotoGridNo(pSoldier.ubProfile, pSoldier.sGridNo, (int)(pSoldier.ubQuoteRecord - 1));
                            }
                            else
                            {
                                // This is important, so try taking a path through people (and bumping them aside)
//                                if (Movement.LegalNPCDestination(pSoldier, (int)pSoldier.usActionData, ENSURE_PATH, WATEROK, PATH.THROUGH_PEOPLE))
                                {
                                    // optimization - Ian: prevent another path call in SetNewCourse()
                                    pSoldier.bPathStored = true;
                                }
//                                else
                                {
                                    // Have buddy wait a while...
                                    pSoldier.bNextAction = AI_ACTION.WAIT;
                                    pSoldier.usNextActionData = (int)REALTIME_AI_DELAY;
                                }
                            }

                            if (!pSoldier.bPathStored)
                            {
                                CancelAIAction(pSoldier, FORCE);
                                return 0;         // nothing is in progress
                            }
                        }
                        else
                        {
                            CancelAIAction(pSoldier, FORCE);
                            return 0;         // nothing is in progress
                        }
                    }
                }

                // add on anything necessary to traverse off map edge
                switch (pSoldier.ubQuoteActionID)
                {
                    case QUOTE_ACTION_ID.TRAVERSE_EAST:
                        pSoldier.sOffWorldGridNo = (int)pSoldier.usActionData;
//                        AdjustSoldierPathToGoOffEdge(pSoldier, pSoldier.usActionData, WorldDirections.EAST);
                        break;
                    case QUOTE_ACTION_ID.TRAVERSE_SOUTH:
                        pSoldier.sOffWorldGridNo = (int)pSoldier.usActionData;
//                        AdjustSoldierPathToGoOffEdge(pSoldier, pSoldier.usActionData, WorldDirections.SOUTH);
                        break;
                    case QUOTE_ACTION_ID.TRAVERSE_WEST:
                        pSoldier.sOffWorldGridNo = (int)pSoldier.usActionData;
//                        AdjustSoldierPathToGoOffEdge(pSoldier, pSoldier.usActionData, WorldDirections.WEST);
                        break;
                    case QUOTE_ACTION_ID.TRAVERSE_NORTH:
                        pSoldier.sOffWorldGridNo = (int)pSoldier.usActionData;
//                        AdjustSoldierPathToGoOffEdge(pSoldier, pSoldier.usActionData, WorldDirections.NORTH);
                        break;
                    default:
                        break;
                }

                AIUtils.NewDest(pSoldier, (int)pSoldier.usActionData);    // set new .sDestination to actionData

                // make sure it worked (check that pSoldier.sDestination == pSoldier.usActionData)
                if (pSoldier.sFinalDestination != (int)pSoldier.usActionData)
                {
                    // temporarily black list this gridno to stop enemy from going there
                    pSoldier.sBlackList = (int)pSoldier.usActionData;

                    DebugAI(string.Format("Setting blacklist for %d to %d", pSoldier.ubID, pSoldier.sBlackList));

                    CancelAIAction(pSoldier, FORCE);
                    return 0;         // nothing is in progress
                }

                // cancel any old black-listed gridno, got a valid new .sDestination
                pSoldier.sBlackList = NOWHERE;
                break;

            case AI_ACTION.ESCORTED_MOVE:         // go where told to by escortPlayer
                                                  // since this is a delayed move, gotta make sure that it hasn't become
                                                  // illegal since escort orders were issued (.sDestination/route blocked).
                                                  // So treat it like a CONTINUE movement, and handle errors that way
//                if (!TryToResumeMovement(pSoldier, pSoldier.usActionData))
                {
                    // don't black-list anything here, and action already got canceled
                    return 0;         // nothing is in progress
                }

                // cancel any old black-listed gridno, got a valid new .sDestination
                pSoldier.sBlackList = NOWHERE;
                break;

            case AI_ACTION.TOSS_PROJECTILE:       // throw grenade at/near opponent(s)
//                LoadWeaponIfNeeded(pSoldier);
                                                  // drop through here...
                break;
            case AI_ACTION.KNIFE_MOVE:            // preparing to stab opponent
                if (pSoldier.bAction == AI_ACTION.KNIFE_MOVE) // if statement because toss falls through
                {
//                    pSoldier.usUIMovementMode = DetermineMovementMode(pSoldier, AI_ACTION.KNIFE_MOVE);
                }
                break;
            // fall through
            case AI_ACTION.FIRE_GUN:              // shoot at nearby opponent
            case AI_ACTION.THROW_KNIFE:                     // throw knife at nearby opponent
                                                            // randomly decide whether to say civ quote
                if (pSoldier.bVisible != -1 && pSoldier.bTeam != MILITIA_TEAM)
                {
                    // ATE: Make sure it's a person :)
                    if (IS_MERC_BODY_TYPE(pSoldier) && pSoldier.ubProfile == NO_PROFILE)
                    {
                        // CC, ATE here - I put in some TEMP randomness...
                        if (Globals.Random.Next(50) == 0)
                        {
//                            StartCivQuote(pSoldier);
                        }
                    }
                }

//                iRetCode = HandleItem(pSoldier, pSoldier.usActionData, pSoldier.bTargetLevel, pSoldier.inv[InventorySlot.HANDPOS].usItem, false);
                if (iRetCode != ITEM_HANDLE.OK)
                {
                    if (iRetCode != ITEM_HANDLE.BROKEN) // if the item broke, this is 'legal' and doesn't need reporting
                    {
                        DebugAI(string.Format("AI %d got error code %ld from HandleItem, doing action %d, has %d APs... aborting deadlock!", pSoldier.ubID, iRetCode, pSoldier.bAction, pSoldier.bActionPoints));
//                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.BETAVERSION, "AI %d got error code %ld from HandleItem, doing action %d... aborting deadlock!", pSoldier.ubID, iRetCode, pSoldier.bAction);
                    }

                    CancelAIAction(pSoldier, FORCE);
                    EndAIGuysTurn(pSoldier);
                }
                break;

            case AI_ACTION.PULL_TRIGGER:          // activate an adjacent panic trigger

                // turn to face trigger first
                if (StructureInternals.FindStructure(pSoldier.sGridNo + DirectionInc(WorldDirections.NORTH), STRUCTUREFLAGS.SWITCH) is not null)
                {
                    SoldierControl.SendSoldierSetDesiredDirectionEvent(pSoldier, WorldDirections.NORTH);
                }
                else
                {
                    SoldierControl.SendSoldierSetDesiredDirectionEvent(pSoldier, WorldDirections.WEST);
                }

                SoldierControl.EVENT_InitNewSoldierAnim(pSoldier, AnimationStates.AI_PULL_SWITCH, 0, false);

                Points.DeductPoints(pSoldier, AP.PULL_TRIGGER, 0);

                //gTacticalStatus.fPanicFlags					= 0; // turn all flags off
                gTacticalStatus.ubTheChosenOne = NOBODY;
                break;

            case AI_ACTION.USE_DETONATOR:
                //gTacticalStatus.fPanicFlags					= 0; // turn all flags off
                gTacticalStatus.ubTheChosenOne = NOBODY;
                //gTacticalStatus.sPanicTriggerGridno	= NOWHERE;

                // grab detonator and set off bomb(s)
                Points.DeductPoints(pSoldier, AP.USE_REMOTE, BP.USE_DETONATOR);// pay for it!
                                                                        //SetOffPanicBombs(1000,COMMUNICATE);    // BOOOOOOOOOOOOOOOOOOOOM!!!!!
//                SetOffPanicBombs(pSoldier.ubID, 0);

                // action completed immediately, cancel it right away
                pSoldier.usActionData = NOWHERE;
                pSoldier.bLastAction = pSoldier.bAction;
                pSoldier.bAction = AI_ACTION.NONE;
                return 0;           // no longer in progress
            case AI_ACTION.RED_ALERT:             // tell friends opponent(s) seen
                                                  // if a computer merc, and up to now they didn't know you're here
                if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC)
                    && ((gTacticalStatus.Team[pSoldier.bTeam].bAwareOfOpposition == 0)
                    || (gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE)
                    && gTacticalStatus.ubTheChosenOne == NOBODY)))
                {
                    HandleInitialRedAlert(pSoldier.bTeam, 1);
                }
                //ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG.BETAVERSION, "Debug: AI radios your position!" );
                // DROP THROUGH HERE!
                break;
            case AI_ACTION.YELLOW_ALERT:          // tell friends opponent(s) heard
                                                  //ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG.BETAVERSION, "Debug: AI radios about a noise!" );
                /*
                            NetSend.msgType = NET_RADIO_SIGHTINGS;
                            NetSend.ubID  = pSoldier.ubID;

                            SendNetData(ALL_NODES);
                */
                Points.DeductPoints(pSoldier, AP.RADIO, BP.RADIO);// pay for it!
               // RadioSightings(pSoldier, EVERYBODY, pSoldier.bTeam);      // about everybody
                                                                          // action completed immediately, cancel it right away

                // ATE: Change to an animation!
                AIDecideRadioAnimation(pSoldier);
                //return(false);           // no longer in progress
                break;

            case AI_ACTION.CREATURE_CALL:                                   // creature calling to others
                Points.DeductPoints(pSoldier, AP.RADIO, BP.RADIO);// pay for it!
                //CreatureCall(pSoldier);
                //return( false ); // no longer in progress
                break;

            case AI_ACTION.CHANGE_STANCE:                // crouch
                if (gAnimControl[pSoldier.usAnimState].ubHeight == (AnimationHeights)pSoldier.usActionData)
                {
                    // abort!
                    ActionDone(pSoldier);
                    return 0;
                }

                SkipCoverCheck = 1;

# if DEBUGDECISIONS
                DebugAI(String("ExecuteAction: SkipCoverCheck ON\n"));
#endif
                SoldierControl.SendChangeSoldierStanceEvent(pSoldier, (AnimationHeights)pSoldier.usActionData);
                break;

            case AI_ACTION.COWER:
                // make sure action data is set right
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                {
                    // nothing to do!
                    ActionDone(pSoldier);
                    return 0;
                }
                else
                {
                    pSoldier.usActionData = (int)AnimationHeights.ANIM_CROUCH;
                    SoldierControl.SetSoldierCowerState(pSoldier, true);
                }
                break;

            case AI_ACTION.STOP_COWERING:
                // make sure action data is set right
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                {
                    pSoldier.usActionData = (int)AnimationHeights.ANIM_STAND;
                    SoldierControl.SetSoldierCowerState(pSoldier, false);
                }
                else
                {
                    // nothing to do!
                    ActionDone(pSoldier);
                    return 0;
                }
                break;

            case AI_ACTION.GIVE_AID:              // help injured/dying friend
                                                  //pSoldier.usUIMovementMode = RUNNING;
//                iRetCode = HandleItem(pSoldier, pSoldier.usActionData, 0, pSoldier.inv[InventorySlot.HANDPOS].usItem, false);
                if (iRetCode != ITEM_HANDLE.OK)
                {
                    CancelAIAction(pSoldier, FORCE);
                    EndAIGuysTurn(pSoldier);
                }
                break;

            case AI_ACTION.OPEN_OR_CLOSE_DOOR:
            case AI_ACTION.UNLOCK_DOOR:
            case AI_ACTION.LOCK_DOOR:
                {
                    STRUCTURE? pStructure;
                    WorldDirections bDirection = 0;
                    int sDoorGridNo;

//                    bDirection = GetDirectionFromGridNo(pSoldier.usActionData, pSoldier);
                    if (bDirection == WorldDirections.EAST || bDirection == WorldDirections.SOUTH)
                    {
                        sDoorGridNo = pSoldier.sGridNo;
                    }
                    else
                    {
                        sDoorGridNo = pSoldier.sGridNo + DirectionInc(bDirection);
                    }

                    pStructure = StructureInternals.FindStructure(sDoorGridNo, STRUCTUREFLAGS.ANYDOOR);
                    if (pStructure == null)
                    {
                        CancelAIAction(pSoldier, FORCE);
                        EndAIGuysTurn(pSoldier);
                    }

//                    StartInteractiveObject(sDoorGridNo, pStructure.usStructureID, pSoldier, bDirection);
//                    InteractWithInteractiveObject(pSoldier, pStructure, bDirection);
                }
                break;

            case AI_ACTION.LOWER_GUN:
                // for now, just do "action done"
                ActionDone(pSoldier);
                break;

            case AI_ACTION.CLIMB_ROOF:
                if (pSoldier.bLevel == 0)
                {
//                    BeginSoldierClimbUpRoof(pSoldier);
                }
                else
                {
//                    BeginSoldierClimbDownRoof(pSoldier);
                }
                break;

            case AI_ACTION.END_TURN:
                ActionDone(pSoldier);
                if (gfTurnBasedAI)
                {
                    EndAIGuysTurn(pSoldier);
                }
                return 0;         // nothing is in progress

            case AI_ACTION.TRAVERSE_DOWN:
                if (gfTurnBasedAI)
                {
                    EndAIGuysTurn(pSoldier);
                }

                if (pSoldier.ubProfile != NO_PROFILE)
                {
                    gMercProfiles[pSoldier.ubProfile].bSectorZ++;
                    gMercProfiles[pSoldier.ubProfile].fUseProfileInsertionInfo = 0;
                }
                
//                TacticalRemoveSoldier(pSoldier.ubID);
//                CheckForEndOfBattle(true);

                return 0;         // nothing is in progress

            case AI_ACTION.OFFER_SURRENDER:
                // start the offer of surrender!
//                StartCivQuote(pSoldier);
                break;

            default:
                return 0;
        }

        // return status indicating execution of action was properly started
        return 1;
    }

    public static void CheckForChangingOrders(SOLDIERTYPE pSoldier)
    {
        switch (pSoldier.bAlertStatus)
        {
            case STATUS.GREEN:
                if (!CREATURE_OR_BLOODCAT(pSoldier))
                {
                    if (pSoldier.bTeam == TEAM.CIV_TEAM
                        && pSoldier.ubProfile != NO_PROFILE
                        & pSoldier.bNeutral > 0
                        && gMercProfiles[pSoldier.ubProfile].sPreCombatGridNo != NOWHERE
                        && pSoldier.ubCivilianGroup != CIV_GROUP.QUEENS_CIV_GROUP)
                    {
                        // must make them uncower first, then return to start location
                        pSoldier.bNextAction = AI_ACTION.END_COWER_AND_MOVE;
                        pSoldier.usNextActionData = gMercProfiles[pSoldier.ubProfile].sPreCombatGridNo;
                        gMercProfiles[pSoldier.ubProfile].sPreCombatGridNo = NOWHERE;
                    }
                    else if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.COWERING))
                    {
                        pSoldier.bNextAction = AI_ACTION.STOP_COWERING;
                        pSoldier.usNextActionData = (int)AnimationHeights.ANIM_STAND;
                    }
                    else
                    {
                        pSoldier.bNextAction = AI_ACTION.CHANGE_STANCE;
                        pSoldier.usNextActionData = (int)AnimationHeights.ANIM_STAND;
                    }
                }
                break;
            case STATUS.YELLOW:
                break;
            default:
                if ((pSoldier.bOrders == Orders.ONGUARD) || (pSoldier.bOrders == Orders.CLOSEPATROL))
                {
                    // crank up ONGUARD to CLOSEPATROL, and CLOSEPATROL to FARPATROL
                    pSoldier.bOrders++;       // increase roaming range by 1 category
                }
                else if (pSoldier.bTeam == TEAM.MILITIA_TEAM)
                {
                    // go on alert!
                    pSoldier.bOrders = Orders.SEEKENEMY;
                }
                else if (CREATURE_OR_BLOODCAT(pSoldier))
                {
                    if (pSoldier.bOrders != Orders.STATIONARY && pSoldier.bOrders != Orders.ONCALL)
                    {
                        pSoldier.bOrders = Orders.SEEKENEMY;
                    }
                }

                if (pSoldier.ubProfile == NPCID.WARDEN)
                {
                    // Tixa
//                    MakeClosestEnemyChosenOne();
                }
                break;
        }
    }

    void InitAttackType(out ATTACKTYPE pAttack)
    {
        // initialize the given bestAttack structure fields to their default values
        pAttack = new ATTACKTYPE
        {
            ubPossible = 0,
            ubOpponent = NOBODY,
            ubAimTime = 0,
            ubChanceToReallyHit = 0,
            sTarget = NOWHERE,
            iAttackValue = 0,
            ubAPCost = 0
        };
    }

    public static void HandleInitialRedAlert(TEAM bTeam, int ubCommunicate)
    {
        /*
         if (ubCommunicate)
          {
           NetSend.msgType = NET_RED_ALERT;
           SendNetData(ALL_NODES);
          }*/

        if (gTacticalStatus.Team[bTeam].bAwareOfOpposition == 0)
        {
        }

        // if there is a stealth mission in progress here, and a panic trigger exists
        if (bTeam == ENEMY_TEAM && gTacticalStatus.fPanicFlags.HasFlag(PANIC.TRIGGERS_HERE))
        {
            // they're going to be aware of us now!
//            MakeClosestEnemyChosenOne();
        }

        if (bTeam == ENEMY_TEAM && gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.P && gbWorldSectorZ == 0)
        {
            // alert Queen and Joe if they are around
            SOLDIERTYPE? pSoldier;

            pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.QUEEN, false);
            if (pSoldier is not null)
            {
                pSoldier.bAlertStatus = STATUS.RED;
            }

            pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(NPCID.JOE, false);
            if (pSoldier is not null)
            {
                pSoldier.bAlertStatus = STATUS.RED;
            }
        }

        // open and close certain doors when this happens
        //AffectDoors(OPENDOORS, MapExt[Status.cur_sector].opendoors);
        //AffectDoors(CLOSEDOORS,MapExt[Status.cur_sector].closedoors);

        // remember enemies are alerted, prevent another red alert from happening
        gTacticalStatus.Team[bTeam].bAwareOfOpposition = 1;

    }

    public static void ManChecksOnFriends(SOLDIERTYPE? pSoldier)
    {
        int uiLoop;
        SOLDIERTYPE? pFriend;
        int sDistVisible;

        // THIS ROUTINE SHOULD ONLY BE CALLED FOR SOLDIERS ON STATUS GREEN or YELLOW

        // go through each soldier, looking for "friends" (soldiers on same side)
        for (uiLoop = 0; uiLoop < guiNumMercSlots; uiLoop++)
        {
            pFriend = MercSlots[uiLoop];

            if (pFriend is null)
            {
                continue;
            }

            // if this man is neutral / NOT on my side, he's not my friend
            if (pFriend.bNeutral > 0 || (pSoldier.bSide != pFriend.bSide))
            {
                continue;  // next merc
            }

            // if this merc is actually ME
            if (pFriend.ubID == pSoldier.ubID)
            {
                continue;  // next merc
            }

            sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, pFriend.sGridNo, pFriend.bLevel);
            // if we can see far enough to see this friend
            if (PythSpacesAway(pSoldier.sGridNo, pFriend.sGridNo) <= sDistVisible)
            {
                // and can trace a line of sight to his x,y coordinates
                //if (1) //*** SoldierToSoldierLineOfSightTest(pSoldier,pFriend,STRAIGHT,true))
//                if (SoldierToSoldierLineOfSightTest(pSoldier, pFriend, (int)sDistVisible, true))
                {
                    // if my friend is in battle or something is clearly happening there
                    if ((pFriend.bAlertStatus >= STATUS.RED) || pFriend.bUnderFire > 0 || (pFriend.bLife < OKLIFE))
                    {
# if DEBUGDECISIONS
                        sprintf(tempstr, "%s sees %s on alert, goes to RED ALERT!", pSoldier.name, ExtMen[pFriend.ubID].name);
                        AIPopMessage(tempstr);
#endif

                        pSoldier.bAlertStatus = STATUS.RED;
                        CheckForChangingOrders(pSoldier);
                        SetNewSituation(pSoldier);
                        break;         // don't bother checking on any other friends
                    }
                    else
                    {
                        // if he seems suspicious or acts like he thought he heard something
                        // and I'm still on status GREEN
                        if ((pFriend.bAlertStatus == STATUS.YELLOW) &&
                            (pSoldier.bAlertStatus < STATUS.YELLOW))
                        {
# if TESTVERSION
                            sprintf(tempstr, "TEST MSG: %s sees %s listening, goes to YELLOW ALERT!", pSoldier.name, ExtMen[pFriend.ubID].name);
                            PopMessage(tempstr);
#endif
                            pSoldier.bAlertStatus = STATUS.YELLOW;    // also get suspicious
                            SetNewSituation(pSoldier);
                            pSoldier.sNoiseGridno = pFriend.sGridNo;  // pretend FRIEND made noise
                            pSoldier.ubNoiseVolume = 3;                // remember this for 3 turns
                                                                       // keep check other friends, too, in case any are already on RED
                        }
                    }
                }
            }
        }
    }


    public static void SetNewSituation(SOLDIERTYPE pSoldier)
    {
        if (pSoldier.bTeam != gbPlayerNum)
        {
            if (pSoldier.ubQuoteRecord == 0 && !gTacticalStatus.fAutoBandageMode && !(pSoldier.bNeutral > 0 && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.ENGAGED_IN_CONV)))
            {
                // allow new situation to be set
                pSoldier.bNewSituation = IS_NEW_SITUATION;

                if (gTacticalStatus.ubAttackBusyCount != 0)
                {
                    //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("BBBBBB bNewSituation is set for %d when ABC !=0.", pSoldier.ubID));
                }

                if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT) || gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME))
                {
                    // reset delay if necessary!
                    RESETTIMECOUNTER(ref pSoldier.AICounter, (uint)Globals.Random.Next(1000));
                }
            }
        }
    }


    void HandleAITacticalTraversal(SOLDIERTYPE? pSoldier)
    {
//        HandleNPCChangesForTacticalTraversal(pSoldier);

//        if (pSoldier.ubProfile != NO_PROFILE && NPCHasUnusedRecordWithGivenApproach(pSoldier.ubProfile, APPROACH_DONE_TRAVERSAL))
        {
            gMercProfiles[pSoldier.ubProfile].ubMiscFlags3 |= PROFILE_MISC_FLAG3.HANDLE_DONE_TRAVERSAL;
        }
//        else
        {
            pSoldier.ubQuoteActionID = 0;
        }

# if TESTAICONTROL
        if (gfTurnBasedAI)
        {
            DebugAI(String("Ending turn for %d because traversing out", pSoldier.ubID));
        }
#endif

        EndAIGuysTurn(pSoldier);
//        RemoveManAsTarget(pSoldier);
        if (pSoldier.bTeam == TEAM.CIV_TEAM && pSoldier.fAIFlags.HasFlag(AIDEFINES.AI_CHECK_SCHEDULE))
        {
//            MoveSoldierFromMercToAwaySlot(pSoldier);
            pSoldier.bInSector = false;
        }
        else
        {
//            ProcessQueenCmdImplicationsOfDeath(pSoldier);
//            TacticalRemoveSoldier(pSoldier.ubID);
        }

//        CheckForEndOfBattle(true);
    }
}

public enum CALL
{
    NONE = 0,
    SINGLE_PREY,
    MULTIPLE_PREY,
    ATTACKED,
    CRIPPLED,
    NUM_CREATURE_CALLS
}

public enum AI_ACTION // ActionType
{
    NONE = 0,                     // maintain current position & facing

    // actions that involve a move to another tile
    RANDOM_PATROL,            // move towards a random destination
    SEEK_FRIEND,              // move towards friend in trouble
    SEEK_OPPONENT,            // move towards a reported opponent
    TAKE_COVER,                   // run for nearest cover from threat
    GET_CLOSER,                   // move closer to a strategic location

    POINT_PATROL,             // move towards next patrol point
    LEAVE_WATER_GAS,      // seek nearest spot of ungassed land
    SEEK_NOISE,                   // seek most important noise heard
    ESCORTED_MOVE,            // go where told to by escortPlayer
    RUN_AWAY,                     // run away from nearby opponent(s)

    KNIFE_MOVE,                   // preparing to stab an opponent
    APPROACH_MERC,            // move up to a merc in order to talk with them; RT
    TRACK,                            // track a scent
    EAT,                              // monster eats corpse
    PICKUP_ITEM,              // grab things lying on the ground

    SCHEDULE_MOVE,            // move according to schedule
    WALK,                             // walk somewhere (NPC stuff etc)
    RUN,                              // run somewhere (NPC stuff etc)
    MOVE_TO_CLIMB,            // move to edge of roof/building
                              // miscellaneous movement actions
    CHANGE_FACING,            // turn to face a different direction

    CHANGE_STANCE,            // stand, crouch, or go prone
                              // actions related to items and attacks
    YELLOW_ALERT,             // tell friends opponent(s) heard
    RED_ALERT,                    // tell friends opponent(s) seen
    CREATURE_CALL,            // creature communication
    PULL_TRIGGER,             // go off to activate a panic trigger

    USE_DETONATOR,            // grab detonator and set off bomb(s)
    FIRE_GUN,                     // shoot at nearby opponent
    TOSS_PROJECTILE,      // throw grenade at/near opponent(s)
    KNIFE_STAB,                   // during the actual knifing attack
    THROW_KNIFE,              // throw a knife

    GIVE_AID,                     // help injured/dying friend
    WAIT,                             // RT: don't do anything for a certain length of time
    PENDING_ACTION,           // RT: wait for pending action (pickup, door open, etc) to finish
    DROP_ITEM,                    // duh
    COWER,                            // for civilians:  cower in fear and stay there!

    STOP_COWERING,            // stop cowering
    OPEN_OR_CLOSE_DOOR,   // schedule-provoked; open or close door
    UNLOCK_DOOR,              // schedule-provoked; unlock door (don't open)
    LOCK_DOOR,                    // schedule-provoked; lock door (close if necessary)
    LOWER_GUN,                    // lower gun prior to throwing knife

    ABSOLUTELY_NONE,      // like "none" but can't be converted to a wait by realtime
    CLIMB_ROOF,                   // climb up or down roof
    END_TURN,                     // end turn (after final stance change)
    END_COWER_AND_MOVE,   // sort of dummy value, special for civilians who are to go somewhere at end of battle
    TRAVERSE_DOWN,            // move down a level
    OFFER_SURRENDER,		// offer surrender to the player
}

public struct THREATTYPE
{
    public SOLDIERTYPE? pOpponent;
    public int sGridNo;
    public int iValue;
    public int iAPs;
    public int iCertainty;
    public int iOrigRange;
}
