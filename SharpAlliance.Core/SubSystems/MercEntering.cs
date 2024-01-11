using SharpAlliance.Core.Managers;

namespace SharpAlliance.Core.SubSystems;

public static class MercEntering
{
    private const int MAX_MERC_IN_HELI = 20;
    private const int MAX_HELI_SCRIPT = 30;
    private const int ME_SCRIPT_DELAY = 100;
    private const int NUM_PER_HELI_RUN = 6;

    public enum HeliStateEnums
    {
        HELI_APPROACH,
        HELI_MOVETO,
        HELI_BEGINDROP,
        HELI_DROP,
        HELI_ENDDROP,
        HELI_MOVEAWAY,
        HELI_EXIT,
        NUM_HELI_STATES

    }

    public enum HeliCodes
    {
        HELI_REST,
        HELI_MOVE_DOWN,
        HELI_MOVE_UP,
        HELI_MOVESMALL_DOWN,
        HELI_MOVESMALL_UP,
        HELI_MOVEY,
        HELI_MOVELARGERY,
        HELI_HANDLE_DROP,
        HELI_SHOW_HELI,

        HELI_GOTO_BEGINDROP,
        HELI_GOTO_DROP,
        HELI_GOTO_EXIT,
        HELI_GOTO_MOVETO,
        HELI_GOTO_MOVEAWAY,
        HELI_DONE,
    }


    private static HeliCodes[,] ubHeliScripts =
{
// HELI_APPROACH
{
            HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_GOTO_MOVETO,
},{
// MOVE TO
	HeliCodes.HELI_SHOW_HELI,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
},{
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
},{
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
},{
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
},{
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
},{
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_MOVEY,
HeliCodes.HELI_GOTO_BEGINDROP,
},{
// HELI_BEGIN_DROP
	HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
},{
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
},{
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
},{
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
},{
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
},{
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_MOVE_DOWN,
HeliCodes.HELI_GOTO_DROP,
},{
// Heli Begin Drop
	HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
},{
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
},{
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
},{
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
},{
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
HeliCodes.HELI_MOVESMALL_UP,
},{
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_MOVESMALL_DOWN,
HeliCodes.HELI_GOTO_DROP,
},{
// HELI END DROP
	HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
},{
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
},{
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
},{
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
},{
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
},{
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_MOVE_UP,
HeliCodes.HELI_GOTO_MOVEAWAY,
},{
// MOVE AWAY
	HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
},{
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
},{
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
},{
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
},{
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
},{
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_MOVELARGERY,
HeliCodes.HELI_GOTO_EXIT,
},{
	// HELI EXIT
	HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
},{
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_REST,
HeliCodes.HELI_DONE,
}
};

    private static bool gfHandleHeli = false;
    private static int[] gusHeliSeats = new int[MAX_MERC_IN_HELI];
    private static int gbNumHeliSeatsOccupied = 0;

    private static bool gfFirstGuyDown = false;

    private static int uiSoundSample;
    private static int gsGridNoSweetSpot;
    private static int gsHeliXPos;
    private static MAP_ROW gsHeliYPos;
    private static double gdHeliZPos;
    private static int gsHeliScript;
    private static HeliStateEnums gubHeliState;
    private static uint guiHeliLastUpdate;
    private static int gbCurDrop;
    private static int gbExitCount;
    private static int gbHeliRound;

    private static bool fFadingHeliIn = false;
    private static bool fFadingHeliOut = false;

    private static bool gfIngagedInDrop = false;

    private static ANITILE? gpHeli;
    private static bool gfFirstHeliRun;


    public static void ResetHeliSeats()
    {
        gbNumHeliSeatsOccupied = 0;
    }

    private static void AddMercToHeli(int ubID)
    {
        int cnt;

        if (gbNumHeliSeatsOccupied < MAX_MERC_IN_HELI)
        {
            // Check if it already exists!
            for (cnt = 0; cnt < gbNumHeliSeatsOccupied; cnt++)
            {
                if (gusHeliSeats[cnt] == ubID)
                {
                    return;
                }
            }

            gusHeliSeats[gbNumHeliSeatsOccupied] = ubID;
            gbNumHeliSeatsOccupied++;
        }
    }


    private static void StartHelicopterRun(int sGridNoSweetSpot)
    {
        int sX;
        MAP_ROW sY;

        gsGridNoSweetSpot = sGridNoSweetSpot;

        if (gbNumHeliSeatsOccupied == 0)
        {
            return;
        }

        GameClock.InterruptTime();
        GameClock.PauseGame();
        GameClock.LockPauseState(20);

        IsometricUtils.ConvertGridNoToCenterCellXY(sGridNoSweetSpot, out sX, out sY);

        gsHeliXPos = sX - (2 * CELL_X_SIZE);
        gsHeliYPos = sY - (10 * CELL_Y_SIZE);
        //gsHeliXPos					= sX - ( 3 * CELL_X_SIZE );
        //gsHeliYPos					= sY + ( 4 * CELL_Y_SIZE );
        gdHeliZPos = 0;
        gsHeliScript = 0;
        gbCurDrop = 0;
        gbExitCount = 0;
        gbHeliRound = 1;

        gubHeliState = HeliStateEnums.HELI_APPROACH;
        guiHeliLastUpdate = GetJA2Clock();

        // Start sound
        //uiSoundSample = PlayJA2Sample(HELI_1, RATE_11025, 0, 10000, MIDDLEPAN);
        fFadingHeliIn = true;

        gfHandleHeli = true;

        gfFirstGuyDown = true;

        guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_BEGINUILOCK;
    }


    private static void HandleHeliDrop()
    {
        HeliCodes ubScriptCode;
        uint uiClock;
        //int sWorldX, sWorldY;
        int iVol = 0;
        int cnt;
        ANITILE_PARAMS AniParams;


        if (gfHandleHeli)
        {
            if (gCurrentUIMode != UI_MODE.LOCKUI_MODE)
            {
                guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_BEGINUILOCK;
            }

            if (_KeyDown(Key.Escape))
            {
                // Loop through all mercs not yet placed 
                for (cnt = gbCurDrop; cnt < gbNumHeliSeatsOccupied; cnt++)
                {
                    // Add merc to sector				
                    MercPtrs[gusHeliSeats[cnt]].ubStrategicInsertionCode = INSERTION_CODE.NORTH;
                    StrategicMap.UpdateMercInSector(MercPtrs[gusHeliSeats[cnt]], 9, 1, 0);

                    // Check for merc arrives quotes...
                    MercHiring.HandleMercArrivesQuotes(MercPtrs[gusHeliSeats[cnt]]);

                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, TacticalStr[(int)STR.MERC_HAS_ARRIVED_STR], MercPtrs[gusHeliSeats[cnt]].name);

                }

                // Remove heli
                TileAnimations.DeleteAniTile(gpHeli);

                Squads.RebuildCurrentSquad();

                // Remove sound
                if (uiSoundSample != NO_SAMPLE)
                {
                    //SoundStop(uiSoundSample);
                }

                gfHandleHeli = false;
                gfIgnoreScrolling = false;
                gbNumHeliSeatsOccupied = 0;
                GameClock.UnLockPauseState();
                GameClock.UnPauseGame();


                // Select our first guy
                Overhead.SelectSoldier(gusHeliSeats[0], false, true);

                //guiCurrentEvent = LU_ENDUILOCK;
                //gCurrentUIMode  = LOCKUI_MODE;
                guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_ENDUILOCK;
                //UIHandleLUIEndLock( NULL );

                HandleFirstHeliDropOfGame();
                return;

            }

            gfIgnoreScrolling = true;

            uiClock = GetJA2Clock();

            if ((uiClock - guiHeliLastUpdate) > ME_SCRIPT_DELAY)
            {
                guiHeliLastUpdate = uiClock;

                if (fFadingHeliIn)
                {
                    if (uiSoundSample != NO_SAMPLE)
                    {
                        //iVol = SoundGetVolume(uiSoundSample);
                        //iVol = __min(HIGHVOLUME, iVol + 5);
                        //SoundSetVolume(uiSoundSample, iVol);
                        //if (iVol == HIGHVOLUME)
                        //    fFadingHeliIn = false;
                    }
                    else
                    {
                        fFadingHeliIn = false;
                    }
                }
                else if (fFadingHeliOut)
                {
                    if (uiSoundSample != NO_SAMPLE)
                    {
                        //iVol = SoundGetVolume(uiSoundSample);

                        //iVol = __max(0, iVol - 5);

                        //SoundSetVolume(uiSoundSample, iVol);
                        if (iVol == 0)
                        {
                            // Stop sound
                          //  SoundStop(uiSoundSample);
                            fFadingHeliOut = false;
                            gfHandleHeli = false;
                            gfIgnoreScrolling = false;
                            gbNumHeliSeatsOccupied = 0;
                            guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_ENDUILOCK;
                            GameClock.UnLockPauseState();
                            GameClock.UnPauseGame();

                            Squads.RebuildCurrentSquad();

                            HandleFirstHeliDropOfGame();
                        }
                    }
                    else
                    {
                        fFadingHeliOut = false;
                        gfHandleHeli = false;
                        gfIgnoreScrolling = false;
                        gbNumHeliSeatsOccupied = 0;
                        guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_ENDUILOCK;
                        GameClock.UnLockPauseState();
                        GameClock.UnPauseGame();

                        Squads.RebuildCurrentSquad();

                        HandleFirstHeliDropOfGame();
                    }
                }

                if (gsHeliScript == MAX_HELI_SCRIPT)
                {
                    return;
                }

                ubScriptCode = ubHeliScripts[(int)gubHeliState,gsHeliScript];

                // Switch on mode...
                if (gubHeliState == HeliStateEnums.HELI_DROP)
                {
                    if (!gfIngagedInDrop)
                    {
                        int bEndVal;

                        bEndVal = (gbHeliRound * NUM_PER_HELI_RUN);

                        if (bEndVal > gbNumHeliSeatsOccupied)
                        {
                            bEndVal = gbNumHeliSeatsOccupied;
                        }

                        // OK, Check if we have anybody left to send!
                        if (gbCurDrop < bEndVal)
                        {
                            //sWorldX = CenterX( gsGridNoSweetSpot );
                            //sWorldY = CenterY( gsGridNoSweetSpot );
                            SoldierControl.EVENT_InitNewSoldierAnim(MercPtrs[gusHeliSeats[gbCurDrop]], AnimationStates.HELIDROP, 0, false);

                            // Change insertion code
                            MercPtrs[gusHeliSeats[gbCurDrop]].ubStrategicInsertionCode = INSERTION_CODE.NORTH;

                            StrategicMap.UpdateMercInSector(MercPtrs[gusHeliSeats[gbCurDrop]], 9, 1, 0);
                            //EVENT_SetSoldierPosition( MercPtrs[ gusHeliSeats[ gbCurDrop ] ], sWorldX, sWorldY );

                            // IF the first guy down, set squad!
                            if (gfFirstGuyDown)
                            {
                                gfFirstGuyDown = false;
                                Squads.SetCurrentSquad((SquadEnum)MercPtrs[gusHeliSeats[gbCurDrop]].bAssignment, true);
                            }
                            
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, TacticalStr[(int)STR.MERC_HAS_ARRIVED_STR], MercPtrs[gusHeliSeats[gbCurDrop]].name);

                            gbCurDrop++;

                            gfIngagedInDrop = true;
                        }
                        else
                        {
                            if (gbExitCount == 0)
                            {
                                gbExitCount = 2;
                            }
                            else
                            {
                                gbExitCount--;

                                if (gbExitCount == 1)
                                {
                                    // Goto leave 
                                    gsHeliScript = -1;
                                    gubHeliState = HeliStateEnums.HELI_ENDDROP;

                                }
                            }
                        }
                    }
                }

                switch (ubScriptCode)
                {
                    case HeliCodes.HELI_REST:

                        break;

                    case HeliCodes.HELI_MOVE_DOWN:

                        gdHeliZPos -= 1;
                        gpHeli.pLevelNode.sRelativeZ = (int)gdHeliZPos;
                        break;

                    case HeliCodes.HELI_MOVE_UP:

                        gdHeliZPos += 1;
                        gpHeli.pLevelNode.sRelativeZ = (int)gdHeliZPos;
                        break;

                    case HeliCodes.HELI_MOVESMALL_DOWN:

                        gdHeliZPos -= 0.25;
                        gpHeli.pLevelNode.sRelativeZ = (int)gdHeliZPos;
                        break;

                    case HeliCodes.HELI_MOVESMALL_UP:

                        gdHeliZPos += 0.25;
                        gpHeli.pLevelNode.sRelativeZ = (int)gdHeliZPos;
                        break;


                    case HeliCodes.HELI_MOVEY:

                        gpHeli.sRelativeY += 4;
                        break;

                    case HeliCodes.HELI_MOVELARGERY:

                        gpHeli.sRelativeY += 6;
                        break;

                    case HeliCodes.HELI_GOTO_BEGINDROP:

                        gsHeliScript = -1;
                        gubHeliState = HeliStateEnums.HELI_BEGINDROP;
                        break;

                    case HeliCodes.HELI_SHOW_HELI:

                        // Start animation
                        AniParams = new()
                        {
                            sGridNo = gsGridNoSweetSpot,
                            ubLevelID = ANI.SHADOW_LEVEL,
                            sDelay = 90,
                            sStartFrame = 0,
                            uiFlags = ANITILEFLAGS.CACHEDTILE | ANITILEFLAGS.FORWARD | ANITILEFLAGS.LOOPING,
                            sX = gsHeliXPos,
                            sY = gsHeliYPos,
                            sZ = (int)gdHeliZPos,
                            zCachedFile = "TILECACHE\\HELI_SH.STI",
                        };

                        gpHeli = TileAnimations.CreateAnimationTile(ref AniParams);
                        break;

                    case HeliCodes.HELI_GOTO_DROP:

                        // Goto drop animation
                        gdHeliZPos -= 0.25;
                        gpHeli.pLevelNode.sRelativeZ = (int)gdHeliZPos;
                        gsHeliScript = -1;
                        gubHeliState = HeliStateEnums.HELI_DROP;
                        break;

                    case HeliCodes.HELI_GOTO_MOVETO:

                        // Goto drop animation
                        gsHeliScript = -1;
                        gubHeliState = HeliStateEnums.HELI_MOVETO;
                        break;

                    case HeliCodes.HELI_GOTO_MOVEAWAY:

                        // Goto drop animation
                        gsHeliScript = -1;
                        gubHeliState = HeliStateEnums.HELI_MOVEAWAY;
                        break;

                    case HeliCodes.HELI_GOTO_EXIT:

                        if (gbCurDrop < gbNumHeliSeatsOccupied)
                        {
                            // Start another run......
                            int sX;
                            MAP_ROW sY;

                            IsometricUtils.ConvertGridNoToCenterCellXY(gsGridNoSweetSpot, out sX, out sY);

                            gsHeliXPos = sX - (2 * CELL_X_SIZE);
                            gsHeliYPos = (MAP_ROW)(sY - (10 * CELL_Y_SIZE));
                            gdHeliZPos = 0;
                            gsHeliScript = 0;
                            gbExitCount = 0;
                            gubHeliState = HeliStateEnums.HELI_APPROACH;
                            gbHeliRound++;

                            // Ahh, but still delete the heli!
                            TileAnimations.DeleteAniTile(gpHeli);
                            gpHeli = null;
                        }
                        else
                        {
                            // Goto drop animation
                            gsHeliScript = -1;
                            gubHeliState = HeliStateEnums.HELI_EXIT;

                            // Delete helicopter image!
                            TileAnimations.DeleteAniTile(gpHeli);
                            gpHeli = null;
                            gfIgnoreScrolling = false;

                            // Select our first guy
                            Overhead.SelectSoldier(gusHeliSeats[0], false, true);
                        }
                        break;

                    case HeliCodes.HELI_DONE:

                        // End
                        fFadingHeliOut = true;
                        break;
                }

                gsHeliScript++;

            }
        }
    }


    private static void BeginMercEntering(SOLDIERTYPE pSoldier, int sGridNo)
    {
        ResetHeliSeats();

        AddMercToHeli(pSoldier.ubID);


        StartHelicopterRun(sGridNo);

        // Make sure AI does nothing.....
        Overhead.PauseAIUntilManuallyUnpaused();
    }


    private static void HandleFirstHeliDropOfGame()
    {
        // Are we in the first heli drop?
        if (gfFirstHeliRun)
        {
            StrategicTurns.SyncStrategicTurnTimes();

            // Call people to area
            Knowledge.CallAvailableEnemiesTo(gsGridNoSweetSpot);

            // Say quote.....
            DialogControl.SayQuoteFromAnyBodyInSector(QUOTE.ENEMY_PRESENCE);

            // Start music
            //SetMusicMode(MUSIC_TACTICAL_ENEMYPRESENT);

            gfFirstHeliRun = false;

        }

        // Send message to turn on ai again....
        DialogControl.CharacterDialogueWithSpecialEvent(0, 0, 0, DIALOGUE_TACTICAL_UI, false, false, DIALOGUE_SPECIAL_EVENT.ENABLE_AI, 0, 0);
    }
}
