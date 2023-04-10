using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StrategicStatus
{
    void InitStrategicStatus()
    {
        gStrategicStatus = new STRATEGIC_STATUS();
        //Add special non-zero start conditions here...

        InitArmyGunTypes();
    }

    bool SaveStrategicStatusToSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten = 10;

        //Save the Strategic Status structure to the saved game file
        //FileWrite(hFile, &gStrategicStatus, sizeof(STRATEGIC_STATUS), out uiNumBytesWritten);
        if (uiNumBytesWritten != 10)//sizeof(STRATEGIC_STATUS))
        {
            return (false);
        }

        return (true);
    }



    bool LoadStrategicStatusFromSaveGameFile(Stream hFile)
    {
        int uiNumBytesRead = 10;

        //Load the Strategic Status structure from the saved game file
        //FileRead(hFile, &gStrategicStatus, sizeof(STRATEGIC_STATUS), &uiNumBytesRead);
        if (uiNumBytesRead != 10)//sizeof(STRATEGIC_STATUS))
        {
            return (false);
        }

        return (true);
    }


    int CalcDeathRate()
    {
        int uiDeathRate = 0;

        // give the player a grace period of 1 day
        if (gStrategicStatus.uiManDaysPlayed > 0)
        {
            // calculates the player's current death rate
            uiDeathRate = (int)((gStrategicStatus.ubMercDeaths * DEATH_RATE_SEVERITY * 100) / gStrategicStatus.uiManDaysPlayed);
        }

        return ((int)uiDeathRate);
    }


    public static void ModifyPlayerReputation(REPUTATION bRepChange)
    {
        int iNewBadRep;

        // subtract, so that a negative reputation change results in an increase in bad reputation
        iNewBadRep = gStrategicStatus.ubBadReputation - (int)bRepChange;

        // keep within a 0-100 range (0 = Saint, 100 = Satan)
        iNewBadRep = Math.Max(0, iNewBadRep);
        iNewBadRep = Math.Min(100, iNewBadRep);

        gStrategicStatus.ubBadReputation = (int)iNewBadRep;
    }


    bool MercThinksDeathRateTooHigh(NPCID ubProfileID)
    {
        int bDeathRateTolerance;

        bDeathRateTolerance = gMercProfiles[ubProfileID].bDeathRate;

        // if he couldn't care less what it is
        if (bDeathRateTolerance == 101)
        {
            // then obviously it CAN'T be too high...
            return (false);
        }

        if (CalcDeathRate() > bDeathRateTolerance)
        {
            // too high - sorry
            return (true);
        }
        else
        {
            // within tolerance
            return (false);
        }
    }


    bool MercThinksBadReputationTooHigh(NPCID ubProfileID)
    {
        int bRepTolerance;

        bRepTolerance = gMercProfiles[ubProfileID].bReputationTolerance;

        // if he couldn't care less what it is
        if (bRepTolerance == 101)
        {
            // then obviously it CAN'T be too high...
            return (false);
        }

        if (gStrategicStatus.ubBadReputation > bRepTolerance)
        {
            // too high - sorry
            return (true);
        }
        else
        {
            // within tolerance
            return (false);
        }
    }


    // only meaningful for already hired mercs
    bool MercThinksHisMoraleIsTooLow(SOLDIERTYPE? pSoldier)
    {
        int bRepTolerance;
        int bMoraleTolerance;

        bRepTolerance = gMercProfiles[pSoldier.ubProfile].bReputationTolerance;

        // if he couldn't care less what it is
        if (bRepTolerance == 101)
        {
            // that obviously it CAN'T be too low...
            return (false);
        }


        // morale tolerance is based directly upon reputation tolerance
        // above 50, morale is GOOD, never below tolerance then
        bMoraleTolerance = (100 - bRepTolerance) / 2;

        if (pSoldier.bMorale < bMoraleTolerance)
        {
            // too low - sorry
            return (true);
        }
        else
        {
            // within tolerance
            return (false);
        }
    }

    public static void UpdateLastDayOfPlayerActivity(uint usDay)
    {
        if (usDay > gStrategicStatus.usLastDayOfPlayerActivity)
        {
            gStrategicStatus.usLastDayOfPlayerActivity = usDay;
            gStrategicStatus.ubNumberOfDaysOfInactivity = 0;
        }
    }

    int LackOfProgressTolerance()
    {
        if (gGameOptions.ubDifficultyLevel >= DifficultyLevel.Hard)
        {
            // give an EXTRA day over normal
            return (7 - (int)DifficultyLevel.Medium + gStrategicStatus.ubHighestProgress / 42);
        }
        else
        {
            return (6 - (int)gGameOptions.ubDifficultyLevel + gStrategicStatus.ubHighestProgress / 42);
        }

    }


    // called once per day in the morning, decides whether Enrico should send any new E-mails to the player
    void HandleEnricoEmail()
    {
        int ubCurrentProgress = Campaign.CurrentPlayerProgressPercentage();
        int ubHighestProgress = Campaign.HighestPlayerProgressPercentage();

        // if creatures have attacked a mine (doesn't care if they're still there or not at the moment)
        if (StrategicMines.HasAnyMineBeenAttackedByMonsters() && !(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_CREATURES)))
        {
            Emails.AddEmail(ENRICO_CREATURES, ENRICO_CREATURES_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_CREATURES;
            return; // avoid any other E-mail at the same time
        }


        if ((ubCurrentProgress >= SOME_PROGRESS_THRESHOLD) && !(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_SOME_PROGRESS)))
        {
            Emails.AddEmail(ENRICO_PROG_20, ENRICO_PROG_20_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_SOME_PROGRESS;
            return; // avoid any setback E-mail at the same time
        }

        if ((ubCurrentProgress >= ABOUT_HALFWAY_THRESHOLD) && !(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_ABOUT_HALFWAY)))
        {
            Emails.AddEmail(ENRICO_PROG_55, ENRICO_PROG_55_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_ABOUT_HALFWAY;
            return; // avoid any setback E-mail at the same time
        }

        if ((ubCurrentProgress >= NEARLY_DONE_THRESHOLD) && !(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_NEARLY_DONE)))
        {
            Emails.AddEmail(ENRICO_PROG_80, ENRICO_PROG_80_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_NEARLY_DONE;
            return; // avoid any setback E-mail at the same time
        }

        // test for a major setback OR a second minor setback
        if ((((ubHighestProgress - ubCurrentProgress) >= MAJOR_SETBACK_THRESHOLD) ||
            (((ubHighestProgress - ubCurrentProgress) >= MINOR_SETBACK_THRESHOLD) && (gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.FLAG_SETBACK_OVER)))) &&
                !(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_MAJOR_SETBACK)))
        {
            Emails.AddEmail(ENRICO_SETBACK, ENRICO_SETBACK_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_MAJOR_SETBACK;
        }
        else
        // test for a first minor setback
        if (((ubHighestProgress - ubCurrentProgress) >= MINOR_SETBACK_THRESHOLD) &&
              !(gStrategicStatus.usEnricoEmailFlags.HasFlag((ENRICO_EMAIL.SENT_MINOR_SETBACK | ENRICO_EMAIL.SENT_MAJOR_SETBACK))))
        {
            Emails.AddEmail(ENRICO_SETBACK_2, ENRICO_SETBACK_2_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_MINOR_SETBACK;
        }
        else
        // if player is back at his maximum progress after having suffered a minor setback
        if ((ubHighestProgress == ubCurrentProgress) && (gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_MINOR_SETBACK)))
        {
            // remember that the original setback has been overcome, so another one can generate another E-mail
            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.FLAG_SETBACK_OVER;
        }
        else if (GameClock.GetWorldDay() > (int)(gStrategicStatus.usLastDayOfPlayerActivity))
        {
            int bComplaint = 0;
            int ubTolerance;

            gStrategicStatus.ubNumberOfDaysOfInactivity++;
            ubTolerance = LackOfProgressTolerance();

            if (gStrategicStatus.ubNumberOfDaysOfInactivity >= ubTolerance)
            {
                if (gStrategicStatus.ubNumberOfDaysOfInactivity == ubTolerance)
                {
                    // send email
                    if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS1)))
                    {
                        bComplaint = 1;
                    }
                    else if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS2)))
                    {
                        bComplaint = 2;
                    }
                    else if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS3)))
                    {
                        bComplaint = 3;
                    }
                }
                else if (gStrategicStatus.ubNumberOfDaysOfInactivity == (int)ubTolerance * 2)
                {
                    // six days? send 2nd or 3rd message possibly
                    if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS2)))
                    {
                        bComplaint = 2;
                    }
                    else if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS3)))
                    {
                        bComplaint = 3;
                    }

                }
                else if (gStrategicStatus.ubNumberOfDaysOfInactivity == ubTolerance * 3)
                {
                    // nine days??? send 3rd message possibly
                    if (!(gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS3)))
                    {
                        bComplaint = 3;
                    }
                }

                if (bComplaint != 0)
                {
                    switch (bComplaint)
                    {
                        case 3:
                            Emails.AddEmail(LACK_PLAYER_PROGRESS_3, LACK_PLAYER_PROGRESS_3_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
                            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_LACK_PROGRESS3;
                            break;
                        case 2:
                            Emails.AddEmail(LACK_PLAYER_PROGRESS_2, LACK_PLAYER_PROGRESS_2_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
                            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_LACK_PROGRESS2;
                            break;
                        default:
                            Emails.AddEmail(LACK_PLAYER_PROGRESS_1, LACK_PLAYER_PROGRESS_1_LENGTH, EmailAddresses.MAIL_ENRICO, GameClock.GetWorldTotalMin());
                            gStrategicStatus.usEnricoEmailFlags |= ENRICO_EMAIL.SENT_LACK_PROGRESS1;
                            break;

                    }

                    History.AddHistoryToPlayersLog(HISTORY.ENRICO_COMPLAINED, 0, GameClock.GetWorldTotalMin(), -1, (MAP_ROW)(-1));
                }

                // penalize loyalty!
                if (gStrategicStatus.usEnricoEmailFlags.HasFlag(ENRICO_EMAIL.SENT_LACK_PROGRESS2))
                {
                    DecrementTownLoyaltyEverywhere(LOYALTY_PENALTY_INACTIVE * (gStrategicStatus.ubNumberOfDaysOfInactivity - LackOfProgressTolerance() + 1));
                }
                else
                {
                    // on first complaint, give a day's grace...
                    DecrementTownLoyaltyEverywhere(LOYALTY_PENALTY_INACTIVE * (gStrategicStatus.ubNumberOfDaysOfInactivity - LackOfProgressTolerance()));
                }
            }
        }

        // reset # of new sectors visited 'today' 
        // grant some leeway for the next day, could have started moving
        // at night...
        gStrategicStatus.ubNumNewSectorsVisitedToday = Math.Min(gStrategicStatus.ubNumNewSectorsVisitedToday, NEW_SECTORS_EQUAL_TO_ACTIVITY) / 3;
    }


    void TrackEnemiesKilled(int ubKilledHow, SOLDIER_CLASS ubSoldierClass)
    {
        int bRankIndex;

        bRankIndex = SoldierClassToRankIndex(ubSoldierClass);

        // if it's not a standard enemy-class soldier
        if (bRankIndex == -1)
        {
            // don't count him as an enemy
            return;
        }

        gStrategicStatus.usEnemiesKilled[ubKilledHow, bRankIndex]++;

        if (ubKilledHow != ENEMY_KILLED_TOTAL)
        {
            gStrategicStatus.usEnemiesKilled[ENEMY_KILLED_TOTAL, bRankIndex]++;
        }
    }


    int SoldierClassToRankIndex(SOLDIER_CLASS ubSoldierClass)
    {
        int bRankIndex = -1;

        // the soldier class defines are not in natural ascending order, elite comes before army!
        switch (ubSoldierClass)
        {
            case SOLDIER_CLASS.ADMINISTRATOR:
                bRankIndex = 0;
                break;
            case SOLDIER_CLASS.ELITE:
                bRankIndex = 2;
                break;
            case SOLDIER_CLASS.ARMY:
                bRankIndex = 1;
                break;

            default:
                // this happens when an NPC joins the enemy team (e.g. Conrad, Iggy, Mike)
                break;
        }

        return (bRankIndex);
    }



    SOLDIER_CLASS RankIndexToSoldierClass(int ubRankIndex)
    {
        SOLDIER_CLASS ubSoldierClass = 0;

        Debug.Assert(ubRankIndex < NUM_ENEMY_RANKS);

        switch (ubRankIndex)
        {
            case 0:
                ubSoldierClass = SOLDIER_CLASS.ADMINISTRATOR;
                break;
            case 1:
                ubSoldierClass = SOLDIER_CLASS.ARMY;
                break;
            case 2:
                ubSoldierClass = SOLDIER_CLASS.ELITE;
                break;
        }

        return (ubSoldierClass);
    }
}

// player reputation modifiers
public enum REPUTATION
{
    LOW_DEATHRATE = +5,
    HIGH_DEATHRATE = -5,
    GREAT_MORALE = +3,
    POOR_MORALE = -3,
    BATTLE_WON = +2,
    BATTLE_LOST = -2,
    TOWN_WON = +5,
    TOWN_LOST = -5,
    SOLDIER_DIED = -2,			// per exp. level
    SOLDIER_CAPTURED = -1,
    KILLED_CIVILIAN = -5,
    EARLY_FIRING = -3,
    KILLED_MONSTER_QUEEN = +15,
    KILLED_DEIDRANNA = +25,
}

// flags to remember whether a certain E-mail has already been sent out
[Flags]
public enum ENRICO_EMAIL
{
    SENT_SOME_PROGRESS = 0x0001,
    SENT_ABOUT_HALFWAY = 0x0002,
    SENT_NEARLY_DONE = 0x0004,
    SENT_MINOR_SETBACK = 0x0008,
    SENT_MAJOR_SETBACK = 0x0010,
    SENT_CREATURES = 0x0020,
    FLAG_SETBACK_OVER = 0x0040,
    SENT_LACK_PROGRESS1 = 0x0080,
    SENT_LACK_PROGRESS2 = 0x0100,
    SENT_LACK_PROGRESS3 = 0x0200,
}

public enum STRATEGIC_PLAYER_CAPTURED_FOR
{
    RESCUE = 0x00000001,
    ESCAPE = 0x00000002,
}

// progress threshold that control Enrico E-mail timing
public enum EMAIL_PROGRESS_THRESHOLD
{
    SOME_PROGRESS_THRESHOLD = 20,
    ABOUT_HALFWAY_THRESHOLD = 55,
    NEARLY_DONE_THRESHOLD = 80,
    MINOR_SETBACK_THRESHOLD = 5,
    MAJOR_SETBACK_THRESHOLD = 15,
}
