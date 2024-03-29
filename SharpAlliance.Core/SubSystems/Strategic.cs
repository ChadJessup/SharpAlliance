﻿using System.Collections.Generic;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Strategic
{

}

public class STRATEGIC_STATUS
{
    public int uiFlags;
    public int ubNumCapturedForRescue;
    public int ubHighestProgress;            // the highest level of progress player has attained thus far in the game (0-100)
    public Items[] ubStandardArmyGunIndex = new Items[ARMY_GUN_LEVELS];      // type of gun in each group that Queen's army is using this game
    public Dictionary<Items, bool> fWeaponDroppedAlready = new();             // flag that tracks whether this weapon type has been dropped before
    public int ubMercDeaths;                     // how many soldiers have bit it while in the player's employ (0-100)
    public int uiManDaysPlayed;             // once per day, # living mercs on player's team is added to this running total
    public int ubBadReputation;              // how bad a reputation player has earned through his actions, performance, etc. (0-100)
    public ENRICO_EMAIL usEnricoEmailFlags;      // bit flags that control progress-related E-mails from Enrico
    public int ubInsuranceInvestigationsCnt;     // how many times merc has been investigated for possible insurance fraud
    public int ubUnhiredMercDeaths;      // how many mercs have died while NOT working for the player
    public int usPlayerKills;                   // kills achieved by all mercs controlled by player together.  *Excludes* militia kills!
    public int[,] usEnemiesKilled = new int[(int)ENEMY_KILLED.NUM_WAYS_ENEMIES_KILLED, (int)ENEMY_RANK.NUM_ENEMY_RANKS];	// admin/troop/elite.  Includes kills by militia, too
    public uint usLastDayOfPlayerActivity;
    public int ubNumNewSectorsVisitedToday;
    public int ubNumberOfDaysOfInactivity;
    public int[] bPadding;// [70];
}

// enemy ranks
public enum ENEMY_RANK
{
    ADMIN,
    TROOP,
    ELITE,
    NUM_ENEMY_RANKS
}

// ways enemies can be killed
public enum ENEMY_KILLED
{
    IN_TACTICAL,
    IN_AUTO_RESOLVE,
    TOTAL,
    NUM_WAYS_ENEMIES_KILLED,
}


public record ARMY_COMPOSITION
{
    public ARMY_COMPOSITION(
        Garrisons iReadability,
        int bPriority,
        int bElitePercentage,
        int bTroopPercentage,
        int bAdminPercentage,
        int bDesiredPopulation,
        int bStartPopulation)
    {
        this.iReadability = iReadability;
        this.bPriority = bPriority;
        this.bElitePercentage = bElitePercentage;
        this.bTroopPercentage = bTroopPercentage;
        this.bAdminPercentage = bAdminPercentage;
        this.bDesiredPopulation = bDesiredPopulation;
        this.bStartPopulation = bStartPopulation;
    }

    public Garrisons iReadability;                 //contains the enumeration which is useless, but helps readability;
    public int bPriority;
    public int bElitePercentage;
    public int bTroopPercentage;
    public int bAdminPercentage;
    public int bDesiredPopulation;
    public int bStartPopulation;
}

//Defines the patrol groups -- movement groups.
public class PATROL_GROUP
{
    public PATROL_GROUP(
        int bSize,
        int bPriority,
        SEC[] ubSectorID,
        int bFillPermittedAfterDayMod100,
        int ubGroupID,
        int bWeight,
        int ubPendingGroupID)
    {
        this.bSize = bSize;
        this.bPriority = bPriority;
        this.ubSectorID = ubSectorID;
        this.bFillPermittedAfterDayMod100 = bFillPermittedAfterDayMod100;
        this.ubGroupID = ubGroupID;
        this.bWeight = bWeight;
        this.ubPendingGroupID = ubPendingGroupID;
    }

    public int bSize;
    public int bPriority;
    public SEC[] ubSectorID = new SEC[4];
    public int bFillPermittedAfterDayMod100;
    public int ubGroupID;
    public int bWeight;
    public int ubPendingGroupID;
}

//Defines all stationary defence forces. 
public class GARRISON_GROUP
{
    public GARRISON_GROUP(SEC a, Garrisons b, int c, int d)
    {
        this.ubSectorID = a;
        this.ubComposition = b;
        this.bWeight = c;
        this.ubPendingGroupID = d;
    }

    public SEC ubSectorID;
    public Garrisons ubComposition;
    public int bWeight;
    public int ubPendingGroupID;// [10];
}

public enum INSERTION_CODE
{
    NORTH,
    SOUTH,
    EAST,
    WEST,
    GRIDNO,
    ARRIVING_GAME,
    CHOPPER,
    PRIMARY_EDGEINDEX,
    SECONDARY_EDGEINDEX,
    CENTER,
};
