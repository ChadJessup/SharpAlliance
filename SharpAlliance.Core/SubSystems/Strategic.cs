using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class Strategic
{
}

public struct ARMY_COMPOSITION
{
    public int iReadability;                 //contains the enumeration which is useless, but helps readability.
    public int bPriority;
    public int bElitePercentage;
    public int bTroopPercentage;
    public int bAdminPercentage;
    public int bDesiredPopulation;
    public int bStartPopulation;
    public int[] bPadding;// [10];
}

//Defines the patrol groups -- movement groups.
public struct PATROL_GROUP
{
    public int bSize;
    public int bPriority;
    public int[] ubSectorID;// [4];
    public int bFillPermittedAfterDayMod100;
    public int ubGroupID;
    public int bWeight;
    public int ubPendingGroupID;
    public int[] bPadding;// [10];
}

//Defines all stationary defence forces. 
public struct GARRISON_GROUP
{
    public int ubSectorID;
    public int ubComposition;
    public int bWeight;
    public int ubPendingGroupID;
    public int[] bPadding;// [10];
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
