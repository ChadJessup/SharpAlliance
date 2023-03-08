using System;
using System.Diagnostics;
using System.IO;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.EnglishText;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core;

public class GameClock
{
    // is the clock pause region created currently?
    bool fClockMouseRegionCreated = false;

    bool fTimeCompressHasOccured = false;

    void InitNewGameClock()
    {
        guiGameClock = STARTING_TIME;
        guiPreviousGameClock = STARTING_TIME;
        guiDay = (guiGameClock / NUM_SEC_IN_DAY);
        guiHour = (guiGameClock - (guiDay * NUM_SEC_IN_DAY)) / NUM_SEC_IN_HOUR;
        guiMin = (guiGameClock - ((guiDay * NUM_SEC_IN_DAY) + (guiHour * NUM_SEC_IN_HOUR))) / NUM_SEC_IN_MIN;
        wprintf(WORLDTIMESTR, "%s %d, %02d:%02d", pDayStrings[0], guiDay, guiHour, guiMin);
        guiTimeCurrentSectorWasLastLoaded = 0;
        guiGameSecondsPerRealSecond = 0;
        gubClockResolution = 1;
        memset(gubUnusedTimePadding, 0, TIME_PADDINGBYTES);
    }

    public static uint GetWorldTotalMin()
    {
        return (guiGameClock / NUM_SEC_IN_MIN);
    }

    public static uint GetWorldTotalSeconds()
    {
        return (guiGameClock);
    }


    public static uint GetWorldHour()
    {
        return (guiHour);
    }

    public static uint GetWorldMinutesInDay()
    {
        return ((guiHour * 60) + guiMin);
    }

    public static uint GetWorldDay()
    {
        return (guiDay);
    }

    public static uint GetWorldDayInSeconds()
    {
        return (guiDay * NUM_SEC_IN_DAY);
    }

    public static uint GetWorldDayInMinutes()
    {
        return ((guiDay * NUM_SEC_IN_DAY) / NUM_SEC_IN_MIN);
    }

    public static uint GetFutureDayInMinutes(uint uiDay)
    {
        return ((uiDay * NUM_SEC_IN_DAY) / NUM_SEC_IN_MIN);
    }

    //this function returns the amount of minutes there has been from start of game to midnight of the uiDay.  
    public static uint GetMidnightOfFutureDayInMinutes(uint uiDay)
    {
        return (GetWorldTotalMin() + (uiDay * 1440) - GetWorldMinutesInDay());
    }

    // Not to be used too often by things other than internally
    private static void WarpGameTime(uint uiAdjustment, WARPTIME ubWarpCode)
    {
        uint uiSaveTimeRate;
        uiSaveTimeRate = guiGameSecondsPerRealSecond;
        guiGameSecondsPerRealSecond = uiAdjustment;
        AdvanceClock(ubWarpCode);
        guiGameSecondsPerRealSecond = uiSaveTimeRate;
    }


    private static void AdvanceClock(WARPTIME ubWarpCode)
    {
        uint uiGameSecondsPerRealSecond = guiGameSecondsPerRealSecond;


        // Set value, to different things if we are in combat...
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)))
            {
                uiGameSecondsPerRealSecond = SECONDS_PER_COMPRESSION_IN_TBCOMBAT;
            }
            else
            {
                uiGameSecondsPerRealSecond = SECONDS_PER_COMPRESSION_IN_RTCOMBAT;
            }
        }

        if (ubWarpCode != WARPTIME.NO_PROCESSING_OF_EVENTS)
        {
            guiTimeOfLastEventQuery = guiGameClock;
            //First of all, events are posted for movements, pending attacks, equipment arrivals, etc.  This time 
            //adjustment using time compression can possibly pass one or more events in a single pass.  So, this list
            //is looked at and processed in sequential order, until the uiAdjustment is fully applied.
            if (GameEvents.GameEventsPending(guiGameSecondsPerRealSecond))
            {
                //If a special event, justifying the cancellation of time compression is reached, the adjustment
                //will be shortened to the time of that event, and will stop processing events, otherwise, all
                //of the events in the time slice will be processed.  The time is adjusted internally as events
                //are processed.
                GameEvents.ProcessPendingGameEvents(guiGameSecondsPerRealSecond, ubWarpCode);
            }
            else
            {
                //Adjust the game clock now.
                guiGameClock += guiGameSecondsPerRealSecond;
            }
        }
        else
        {
            guiGameClock += guiGameSecondsPerRealSecond;
        }


        if (guiGameClock < guiPreviousGameClock)
        {
            //AssertMsg(false, string.Format("AdvanceClock: TIME FLOWING BACKWARDS!!! guiPreviousGameClock %d, now %d", guiPreviousGameClock, guiGameClock));

            // fix it if assertions are disabled
            guiGameClock = guiPreviousGameClock;
        }

        // store previous game clock value (for error-checking purposes only)
        guiPreviousGameClock = guiGameClock;


        //Calculate the day, hour, and minutes.
        guiDay = (guiGameClock / NUM_SEC_IN_DAY);
        guiHour = (guiGameClock - (guiDay * NUM_SEC_IN_DAY)) / NUM_SEC_IN_HOUR;
        guiMin = (guiGameClock - ((guiDay * NUM_SEC_IN_DAY) + (guiHour * NUM_SEC_IN_HOUR))) / NUM_SEC_IN_MIN;

        wprintf(WORLDTIMESTR, "%s %d, %02d:%02d", gpGameClockString[(int)STR_GAMECLOCK.DAY_NAME], guiDay, guiHour, guiMin);

        if (gfResetAllPlayerKnowsEnemiesFlags && !gTacticalStatus.fEnemyInSector)
        {
            ClearAnySectorsFlashingNumberOfEnemies();

            gfResetAllPlayerKnowsEnemiesFlags = false;
        }

        ForecastDayEvents();
    }


    void AdvanceToNextDay()
    {
        uint uiDiff;
        uint uiTomorrowTimeInSec;

        uiTomorrowTimeInSec = (guiDay + 1) * NUM_SEC_IN_DAY + 8 * NUM_SEC_IN_HOUR + 15 * NUM_SEC_IN_MIN;
        uiDiff = uiTomorrowTimeInSec - guiGameClock;
        WarpGameTime(uiDiff, WARPTIME.PROCESS_EVENTS_NORMALLY);

        ForecastDayEvents();
    }



    // set the flag that time compress has occured
    void SetFactTimeCompressHasOccured()
    {
        fTimeCompressHasOccured = true;
        return;
    }

    //reset fact the time compress has occured
    void ResetTimeCompressHasOccured()
    {
        fTimeCompressHasOccured = false;
        return;
    }

    // has time compress occured?
    bool HasTimeCompressOccured()
    {
        return (fTimeCompressHasOccured);
    }



    void RenderClock(int sX, int sY)
    {
        FontSubSystem.SetFont(CLOCK_FONT);
        FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);

        // Are we in combat?
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            FontSubSystem.SetFontForeground(FontColor.FONT_FCOLOR_NICERED);
        }
        else
        {
            FontSubSystem.SetFontForeground(FontColor.FONT_LTGREEN);
        }

        // Erase first!
        RenderDirty.RestoreExternBackgroundRect(sX, sY, CLOCK_STRING_WIDTH, CLOCK_STRING_HEIGHT);

        if ((gfPauseDueToPlayerGamePause == false))
        {
            mprintf(sX + (CLOCK_STRING_WIDTH - StringPixLength(WORLDTIMESTR, CLOCK_FONT)) / 2, sY, WORLDTIMESTR);
        }
        else
        {
            mprintf(sX + (CLOCK_STRING_WIDTH - StringPixLength(pPausedGameText[0], CLOCK_FONT)) / 2, sY, pPausedGameText[0]);
        }

    }

    private static TIME_COMPRESS uiOldTimeCompressMode = 0;
    void ToggleSuperCompression()
    {
        // Display message
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            //ScreenMsg( MSG_FONT_YELLOW, MSG_INTERFACE, "Cannot toggle compression in Combat Mode."  );
            return;
        }

        fSuperCompression = (bool)(!fSuperCompression);

        if (fSuperCompression)
        {
            uiOldTimeCompressMode = giTimeCompressMode;
            giTimeCompressMode = TIME_COMPRESS.TIME_SUPER_COMPRESS;
            guiGameSecondsPerRealSecond = giTimeCompressSpeeds[giTimeCompressMode] * SECONDS_PER_COMPRESSION;

            //ScreenMsg( MSG_FONT_YELLOW, MSG_INTERFACE, "Time compression ON."  );
        }
        else
        {
            giTimeCompressMode = uiOldTimeCompressMode;
            guiGameSecondsPerRealSecond = giTimeCompressSpeeds[giTimeCompressMode] * SECONDS_PER_COMPRESSION;

            //ScreenMsg( MSG_FONT_YELLOW, MSG_INTERFACE, "Time compression OFF."  );
        }
    }


    bool DidGameJustStart()
    {
        if (gTacticalStatus.fDidGameJustStart)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }
    public static void StopTimeCompression()
    {
        if (gfTimeCompressionOn)
        {
            // change the clock resolution to no time passage, but don't actually change the compress mode (remember it)
            SetClockResolutionToCompressMode(TIME_COMPRESS.TIME_COMPRESS_X0);
        }
    }


    void StartTimeCompression()
    {
        if (!gfTimeCompressionOn)
        {
            if (GamePaused())
            {
                // first have to be allowed to unpause the game
                UnPauseGame();

                // if we couldn't, ignore this request
                if (GamePaused())
                {
                    return;
                }
            }


            // check that we can start compressing
            if (!MapScreenInterfaceBottom.AllowedToTimeCompress())
            {
                // not allowed to compress time
                TellPlayerWhyHeCantCompressTime();
                return;
            }


            // if no compression mode is set, increase it first
            if (giTimeCompressMode <= TIME_COMPRESS.TIME_COMPRESS_X1)
            {
                IncreaseGameTimeCompressionRate();
            }

            // change clock resolution to the current compression mode
            SetClockResolutionToCompressMode(giTimeCompressMode);

            // if it's the first time we're doing this since entering map screen (which reset the flag)
            if (!HasTimeCompressOccured())
            {
                // set fact that we have compressed time during this map screen session
                SetFactTimeCompressHasOccured();

                ClearTacticalStuffDueToTimeCompression();
            }
        }
    }


    // returns false if time isn't currently being compressed for ANY reason (various pauses, etc.)
    bool IsTimeBeingCompressed()
    {
        if (!gfTimeCompressionOn || (giTimeCompressMode == TIME_COMPRESS.TIME_COMPRESS_X0) || gfGamePaused)
        {
            return (false);
        }
        else
        {
            return (true);
        }
    }


    // returns true if the player currently doesn't want time to be compressing
    bool IsTimeCompressionOn()
    {
        return (gfTimeCompressionOn);
    }


    void IncreaseGameTimeCompressionRate()
    {
        // if not already at maximum time compression rate
        if (giTimeCompressMode < TIME_COMPRESS.TIME_COMPRESS_60MINS)
        {
            // check that we can
            if (!MapScreenInterfaceBottom.AllowedToTimeCompress())
            {
                // not allowed to compress time
                TellPlayerWhyHeCantCompressTime();
                return;
            }


            giTimeCompressMode++;

            // in map screen, we wanna have to skip over x1 compression and go straight to 5x
            if ((guiCurrentScreen == ScreenName.MAP_SCREEN) && (giTimeCompressMode == TIME_COMPRESS.TIME_COMPRESS_X1))
            {
                giTimeCompressMode++;
            }

            SetClockResolutionToCompressMode(giTimeCompressMode);
        }
    }


    void DecreaseGameTimeCompressionRate()
    {
        // if not already at minimum time compression rate
        if (giTimeCompressMode > TIME_COMPRESS.TIME_COMPRESS_X0)
        {
            // check that we can
            if (!MapScreenInterfaceBottom.AllowedToTimeCompress())
            {
                // not allowed to compress time
                TellPlayerWhyHeCantCompressTime();
                return;
            }

            giTimeCompressMode--;

            // in map screen, we wanna have to skip over x1 compression and go straight to 5x
            if ((guiCurrentScreen == ScreenName.MAP_SCREEN) && (giTimeCompressMode == TIME_COMPRESS.TIME_COMPRESS_X1))
            {
                giTimeCompressMode--;
            }

            SetClockResolutionToCompressMode(giTimeCompressMode);
        }
    }


    void SetGameTimeCompressionLevel(TIME_COMPRESS uiCompressionRate)
    {
        Debug.Assert(uiCompressionRate < TIME_COMPRESS.NUM_TIME_COMPRESS_SPEEDS);

        if (guiCurrentScreen == ScreenName.GAME_SCREEN)
        {
            if (uiCompressionRate != TIME_COMPRESS.TIME_COMPRESS_X1)
            {
                uiCompressionRate = TIME_COMPRESS.TIME_COMPRESS_X1;
            }
        }

        if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            if (uiCompressionRate == TIME_COMPRESS.TIME_COMPRESS_X1)
            {
                uiCompressionRate = TIME_COMPRESS.TIME_COMPRESS_X0;
            }
        }

        // if we're attempting time compression
        if (uiCompressionRate >= TIME_COMPRESS.TIME_COMPRESS_5MINS)
        {
            // check that we can
            if (!MapScreenInterfaceBottom.AllowedToTimeCompress())
            {
                // not allowed to compress time
                TellPlayerWhyHeCantCompressTime();
                return;
            }
        }

        giTimeCompressMode = uiCompressionRate;
        SetClockResolutionToCompressMode(giTimeCompressMode);
    }
    public static void SetClockResolutionToCompressMode(TIME_COMPRESS iCompressMode)
    {
        guiGameSecondsPerRealSecond = giTimeCompressSpeeds[iCompressMode] * SECONDS_PER_COMPRESSION;

        // ok this is a bit confusing, but for time compression (e.g. 30x60) we want updates
        // 30x per second, but for standard unpaused time, like in tactical, we want 1x per second
        if (guiGameSecondsPerRealSecond == 0)
        {
            SetClockResolutionPerSecond(0);
        }
        else
        {
            SetClockResolutionPerSecond((uint)Math.Max(1, (int)(guiGameSecondsPerRealSecond / 60)));
        }

        // if the compress mode is X0 or X1
        if (iCompressMode <= TIME_COMPRESS.TIME_COMPRESS_X1)
        {
            gfTimeCompressionOn = false;
        }
        else
        {
            gfTimeCompressionOn = true;

            // handle the player just starting a game
            HandleTimeCompressWithTeamJackedInAndGearedToGo();
        }

        fMapScreenBottomDirty = true;
    }



    void SetGameHoursPerSecond(uint uiGameHoursPerSecond)
    {
        giTimeCompressMode = TIME_COMPRESS.NOT_USING_TIME_COMPRESSION;
        guiGameSecondsPerRealSecond = uiGameHoursPerSecond * 3600;
        if (uiGameHoursPerSecond == 1)
        {
            SetClockResolutionPerSecond(60);
        }
        else
        {
            SetClockResolutionPerSecond(59);
        }
    }

    void SetGameMinutesPerSecond(uint uiGameMinutesPerSecond)
    {
        giTimeCompressMode = TIME_COMPRESS.NOT_USING_TIME_COMPRESSION;
        guiGameSecondsPerRealSecond = uiGameMinutesPerSecond * 60;
        SetClockResolutionPerSecond(uiGameMinutesPerSecond);
    }

    void SetGameSecondsPerSecond(uint uiGameSecondsPerSecond)
    {
        giTimeCompressMode = TIME_COMPRESS.NOT_USING_TIME_COMPRESSION;
        guiGameSecondsPerRealSecond = uiGameSecondsPerSecond;
        //	SetClockResolutionPerSecond( (int)(guiGameSecondsPerRealSecond / 60) );
        if (guiGameSecondsPerRealSecond == 0)
        {
            SetClockResolutionPerSecond(0);
        }
        else
        {
            SetClockResolutionPerSecond((uint)Math.Max(1, (int)(guiGameSecondsPerRealSecond / 60)));
        }

    }


    // call this to prevent player from changing the time compression state via the interface

    public static void LockPauseState(int uiUniqueReasonId)
    {
        gfLockPauseState = true;

        // if adding a new call, please choose a new uiUniqueReasonId, this helps track down the cause when it's left locked
        // Highest # used was 21 on Feb 15 '99.
        guiLockPauseStateLastReasonId = uiUniqueReasonId;
    }

    // call this to allow player to change the time compression state via the interface once again
    public static void UnLockPauseState()
    {
        gfLockPauseState = false;
    }

    // tells you whether the player is currently locked out from messing with the time compression state
    public static bool PauseStateLocked()
    {
        return gfLockPauseState;
    }


    public static void PauseGame()
    {
        // always allow pausing, even if "locked".  Locking applies only to trying to compress time, not to pausing it
        if (!gfGamePaused)
        {
            gfGamePaused = true;
            fMapScreenBottomDirty = true;
        }
    }

    public static void UnPauseGame()
    {
        // if we're paused
        if (gfGamePaused)
        {
            // ignore request if locked
            if (gfLockPauseState)
            {
                Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG_TESTVERSION, "Call to UnPauseGame() while Pause State is LOCKED! AM-4");
                return;
            }

            gfGamePaused = false;
            fMapScreenBottomDirty = true;
        }
    }


    void TogglePause()
    {
        if (gfGamePaused)
        {
            UnPauseGame();
        }
        else
        {
            PauseGame();
        }
    }


    bool GamePaused()
    {
        return gfGamePaused;
    }


    //ONLY APPLICABLE INSIDE EVENT CALLBACKS!
    void InterruptTime()
    {
        gfTimeInterrupt = true;
    }

    void PauseTimeForInterupt()
    {
        gfTimeInterruptPause = true;
    }

    //USING CLOCK RESOLUTION
    //Note, that changing the clock resolution doesn't effect the amount of game time that passes per
    //real second, but how many times per second the clock is updated.  This rate will break up the actual
    //time slices per second into smaller chunks.  This is useful for animating strategic movement under
    //fast time compression, so objects don't warp around.
    void SetClockResolutionToDefault()
    {
        gubClockResolution = 1;
    }

    //Valid range is 0 - 60 times per second.
    public static void SetClockResolutionPerSecond(uint ubNumTimesPerSecond)
    {
        ubNumTimesPerSecond = Math.Max(0, Math.Min(60, ubNumTimesPerSecond));
        gubClockResolution = ubNumTimesPerSecond;
    }

    //Function for accessing the current rate
    uint ClockResolution()
    {
        return gubClockResolution;
    }


    //There are two factors that influence the flow of time in the game.
    //-Speed:  The speed is the amount of game time passes per real second of time.  The higher this
    //         value, the faster the game time flows.
    //-Resolution:  The higher the resolution, the more often per second the clock is actually updated.
    //				 This value doesn't affect how much game time passes per real second, but allows for
    //				 a more accurate representation of faster time flows.
    static uint ubLastResolution = 1;
    static uint uiLastSecondTime = 0;
    static uint uiLastTimeProcessed = 0;

    void UpdateClock()
    {
        uint uiNewTime;
        uint uiThousandthsOfThisSecondProcessed;
        uint uiTimeSlice;
        uint uiNewTimeProcessed;
        uint uiAmountToAdvanceTime;
#if DEBUG_GAME_CLOCK
        int uiOrigNewTime;
        int uiOrigLastSecondTime;
        int uiOrigThousandthsOfThisSecondProcessed;
        int ubOrigClockResolution;
        int uiOrigTimesThisSecondProcessed;
        int ubOrigLastResolution;
#endif
        // check game state for pause screen masks
        CreateDestroyScreenMaskForPauseGame();

#if JA2BETAVERSION
        if (guiCurrentScreen != GAME_SCREEN && guiCurrentScreen != MAP_SCREEN && guiCurrentScreen != AIVIEWER_SCREEN && guiCurrentScreen != GAME_SCREEN)
        {
#else
        if (guiCurrentScreen != ScreenName.GAME_SCREEN && guiCurrentScreen != ScreenName.MAP_SCREEN && guiCurrentScreen != ScreenName.GAME_SCREEN)
#endif
        {
            uiLastSecondTime = GetJA2Clock();
            gfTimeInterruptPause = false;
            return;
        }

        if (gfGamePaused || gfTimeInterruptPause || (gubClockResolution == 0) || guiGameSecondsPerRealSecond == 0 || ARE_IN_FADE_IN() || gfFadeOut)
        {
            uiLastSecondTime = GetJA2Clock();
            gfTimeInterruptPause = false;
            return;
        }

        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            return; //time is currently stopped!
        }

        uiNewTime = GetJA2Clock();

#if DEBUG_GAME_CLOCK
        uiOrigNewTime = uiNewTime;
        uiOrigLastSecondTime = uiLastSecondTime;
        uiOrigThousandthsOfThisSecondProcessed = uiThousandthsOfThisSecondProcessed;
        ubOrigClockResolution = gubClockResolution;
        uiOrigTimesThisSecondProcessed = guiTimesThisSecondProcessed;
        ubOrigLastResolution = ubLastResolution;
#endif

        //Because we debug so much, breakpoints tend to break the game, and cause unnecessary headaches.
        //This line ensures that no more than 1 real-second passes between frames.  This otherwise has
        //no effect on anything else.
        uiLastSecondTime = Math.Max(uiNewTime - 1000, uiLastSecondTime);

        //1000's of a second difference since last second.
        uiThousandthsOfThisSecondProcessed = uiNewTime - uiLastSecondTime;

        if (uiThousandthsOfThisSecondProcessed >= 1000 && gubClockResolution == 1)
        {
            uiLastSecondTime = uiNewTime;
            guiTimesThisSecondProcessed = uiLastTimeProcessed = 0;
            AdvanceClock(WARPTIME.PROCESS_EVENTS_NORMALLY);
        }
        else if (gubClockResolution > 1)
        {
            if (gubClockResolution != ubLastResolution)
            {
                //guiTimesThisSecondProcessed = guiTimesThisSecondProcessed * ubLastResolution / gubClockResolution % gubClockResolution;
                guiTimesThisSecondProcessed = guiTimesThisSecondProcessed * gubClockResolution / ubLastResolution;
                uiLastTimeProcessed = uiLastTimeProcessed * gubClockResolution / ubLastResolution;
                ubLastResolution = gubClockResolution;
            }

            uiTimeSlice = 1000000 / gubClockResolution;
            if (uiThousandthsOfThisSecondProcessed >= uiTimeSlice * (guiTimesThisSecondProcessed + 1) / 1000)
            {
                guiTimesThisSecondProcessed = uiThousandthsOfThisSecondProcessed * 1000 / uiTimeSlice;
                uiNewTimeProcessed = guiGameSecondsPerRealSecond * guiTimesThisSecondProcessed / gubClockResolution;

                uiNewTimeProcessed = Math.Max(uiNewTimeProcessed, uiLastTimeProcessed);

                uiAmountToAdvanceTime = uiNewTimeProcessed - uiLastTimeProcessed;

#if DEBUG_GAME_CLOCK
                if (uiAmountToAdvanceTime > 0x80000000 || guiGameClock + uiAmountToAdvanceTime < guiPreviousGameClock)
                {
                    uiNewTimeProcessed = uiNewTimeProcessed;
                }
#endif

                WarpGameTime(uiNewTimeProcessed - uiLastTimeProcessed, WARPTIME.PROCESS_EVENTS_NORMALLY);
                if (uiNewTimeProcessed < guiGameSecondsPerRealSecond)
                { //Processed the same real second
                    uiLastTimeProcessed = uiNewTimeProcessed;
                }
                else
                { //We have moved into a new real second.
                    uiLastTimeProcessed = uiNewTimeProcessed % guiGameSecondsPerRealSecond;
                    if (gubClockResolution > 0)
                    {
                        guiTimesThisSecondProcessed %= gubClockResolution;
                    }
                    else
                    {
                        // this branch occurs whenever an event during WarpGameTime stops time compression!
                        guiTimesThisSecondProcessed = 0;
                    }
                    uiLastSecondTime = uiNewTime;
                }
            }
        }
    }



    bool SaveGameClock(Stream hFile, bool fGamePaused, bool fLockPauseState)
    {
        int uiNumBytesWritten = 0;

        //FileWrite(hFile, giTimeCompressMode, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, gubClockResolution, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, fGamePaused, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, gfTimeInterrupt, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, fSuperCompression, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, guiGameClock, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, guiGameSecondsPerRealSecond, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, ubAmbientLightLevel, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, guiEnvTime, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, guiEnvDay, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, gubEnvLightValue, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, guiTimeOfLastEventQuery, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, fLockPauseState, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, gfPauseDueToPlayerGamePause, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, gfResetAllPlayerKnowsEnemiesFlags, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, gfTimeCompressionOn, sizeof(bool), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(bool))
        {
            return (false);
        }

        //FileWrite(hFile, guiPreviousGameClock, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, guiLockPauseStateLastReasonId, sizeof(int), uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, gubUnusedTimePadding, TIME_PADDINGBYTES, uiNumBytesWritten);
        if (uiNumBytesWritten != TIME_PADDINGBYTES)
        {
            return (false);
        }

        return (true);
    }


    bool LoadGameClock(Stream hFile)
    {
        int uiNumBytesRead = sizeof(int);

        //FileRead(hFile, giTimeCompressMode, sizeof(int), out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, gubClockResolution, sizeof(int), out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, gfGamePaused, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, gfTimeInterrupt, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, fSuperCompression, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, guiGameClock, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, guiGameSecondsPerRealSecond, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, ubAmbientLightLevel, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, guiEnvTime, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, guiEnvDay, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, gubEnvLightValue, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, guiTimeOfLastEventQuery, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, gfLockPauseState, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, gfPauseDueToPlayerGamePause, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, gfResetAllPlayerKnowsEnemiesFlags, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, gfTimeCompressionOn, sizeof(bool), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(bool))
        {
            return (false);
        }

        //FileRead(hFile, guiPreviousGameClock, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, guiLockPauseStateLastReasonId, sizeof(int), uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //FileRead(hFile, gubUnusedTimePadding, TIME_PADDINGBYTES, uiNumBytesRead);
        if (uiNumBytesRead != TIME_PADDINGBYTES)
        {
            return (false);
        }


        //Update the game clock
        guiDay = (guiGameClock / NUM_SEC_IN_DAY);
        guiHour = (guiGameClock - (guiDay * NUM_SEC_IN_DAY)) / NUM_SEC_IN_HOUR;
        guiMin = (guiGameClock - ((guiDay * NUM_SEC_IN_DAY) + (guiHour * NUM_SEC_IN_HOUR))) / NUM_SEC_IN_MIN;

        //wprintf(WORLDTIMESTR, "%s %d, %02d:%02d", pDayStrings[0], guiDay, guiHour, guiMin);

        if (!gfBasement && !gfCaves)
        {
            gfDoLighting = true;
        }

        return (true);
    }


    void CreateMouseRegionForPauseOfClock(int sX, int sY)
    {
        if (fClockMouseRegionCreated == false)
        {
            // create a mouse region for pausing of game clock
            MouseSubSystem.MSYS_DefineRegion(gClockMouseRegion, new((sX), (int)(sY), (int)(sX + CLOCK_REGION_WIDTH), (int)(sY + CLOCK_REGION_HEIGHT)), MSYS_PRIORITY.HIGHEST,
                                 CURSOR.MSYS_NO_CURSOR, MSYS_NO_CALLBACK, PauseOfClockBtnCallback);

            fClockMouseRegionCreated = true;

            if (gfGamePaused == false)
            {
                MouseSubSystem.SetRegionFastHelpText(gClockMouseRegion, pPausedGameText[2]);
            }
            else
            {
                MouseSubSystem.SetRegionFastHelpText(gClockMouseRegion, pPausedGameText[1]);
            }
        }
    }


    void RemoveMouseRegionForPauseOfClock()
    {
        // remove pause region
        if (fClockMouseRegionCreated == true)
        {
            MouseSubSystem.MSYS_RemoveRegion(gClockMouseRegion);
            fClockMouseRegionCreated = false;

        }
    }


    void PauseOfClockBtnCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            HandlePlayerPauseUnPauseOfGame();
        }
    }


    void HandlePlayerPauseUnPauseOfGame()
    {
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.ENGAGED_IN_CONV))
        {
            return;
        }

        // check if the game is paused BY THE PLAYER or not and reverse
        if (gfGamePaused && gfPauseDueToPlayerGamePause)
        {
            // If in game screen...
            if (guiCurrentScreen == ScreenName.GAME_SCREEN)
            {
                if (giTimeCompressMode == TIME_COMPRESS.TIME_COMPRESS_X0)
                {
                    giTimeCompressMode++;
                }

                // ATE: re-render
                SetRenderFlags(RENDER_FLAG_FULL);
            }

            UnPauseGame();
            TimerControl.PauseTime(false);
            gfIgnoreScrolling = false;
            gfPauseDueToPlayerGamePause = false;
        }
        else
        {
            // pause game
            PauseGame();
            TimerControl.PauseTime(true);
            gfIgnoreScrolling = true;
            gfPauseDueToPlayerGamePause = true;
        }

        return;
    }


    static bool fCreated = false;
    void CreateDestroyScreenMaskForPauseGame()
    {
        int sX = 0, sY = 0;

        if (((fClockMouseRegionCreated == false) || (gfGamePaused == false) || (gfPauseDueToPlayerGamePause == false)) && (fCreated == true))
        {
            fCreated = false;
            MouseSubSystem.MSYS_RemoveRegion(gClockScreenMaskMouseRegion);
            RemoveMercPopupBoxFromIndex(iPausedPopUpBox);
            iPausedPopUpBox = -1;
            SetRenderFlags(RENDER_FLAG_FULL);
            fTeamPanelDirty = true;
            fMapPanelDirty = true;
            fMapScreenBottomDirty = true;
            gfJustFinishedAPause = true;
            MarkButtonsDirty();
            SetRenderFlags(RENDER_FLAG_FULL);
        }
        else if ((gfPauseDueToPlayerGamePause == true) && (fCreated == false))
        {
            // create a mouse region for pausing of game clock
            MouseSubSystem.MSYS_DefineRegion(gClockScreenMaskMouseRegion, new(0, 0, 640, 480), MSYS_PRIORITY.HIGHEST,
                                 0, MSYS_NO_CALLBACK, ScreenMaskForGamePauseBtnCallBack);
            fCreated = true;

            // get region x and y values
            sX = (gClockMouseRegion).Bounds.Left;
            sY = (gClockMouseRegion).Bounds.Top;

            //re create region on top of this
            RemoveMouseRegionForPauseOfClock();
            CreateMouseRegionForPauseOfClock(sX, sY);

            SetRegionFastHelpText(gClockMouseRegion, pPausedGameText[1]);

            fMapScreenBottomDirty = true;

            //UnMarkButtonsDirty( );

            // now create the pop up box to say the game is paused
            iPausedPopUpBox = PrepareMercPopupBox(iPausedPopUpBox, BASIC_MERC_POPUP_BACKGROUND, BASIC_MERC_POPUP_BORDER, pPausedGameText[0], 300, 0, 0, 0, usPausedActualWidth, usPausedActualHeight);
        }
    }


    void ScreenMaskForGamePauseBtnCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // unpause the game
            HandlePlayerPauseUnPauseOfGame();
        }
    }

    void RenderPausedGameBox()
    {
        if ((gfPauseDueToPlayerGamePause == true) && (gfGamePaused == true) && (iPausedPopUpBox != -1))
        {
            MercTextBox.RenderMercPopUpBoxFromIndex(iPausedPopUpBox, (int)(320 - usPausedActualWidth / 2), (int)(200 - usPausedActualHeight / 2), Surfaces.FRAME_BUFFER);
            VeldridVideoManager.InvalidateRegion(new(
                (320 - usPausedActualWidth / 2),
                (200 - usPausedActualHeight / 2),
                (320 - usPausedActualWidth / 2 + usPausedActualWidth),
                (200 - usPausedActualHeight / 2 + usPausedActualHeight)));
        }

        // reset we've just finished a pause by the player
        gfJustFinishedAPause = false;
    }

    bool DayTime()
    { //between 7AM and 9PM
        return (guiHour >= 7 && guiHour < 21);
    }

    bool NightTime()
    {  //before 7AM or after 9PM
        return (guiHour < 7 || guiHour >= 21);
    }



    void ClearTacticalStuffDueToTimeCompression()
    {
        // is this test the right thing?  ARM
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN))
        {
            // clear tactical event queue
            ClearEventQueue();

            // clear tactical message queue
            ClearTacticalMessageQueue();

            if (gfWorldLoaded)
            {
                // clear tactical actions
                CencelAllActionsForTimeCompression();
            }
        }
    }
}

public enum STR_GAMECLOCK
{
    DAY_NAME,
};
