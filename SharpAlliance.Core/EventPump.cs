using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class EventPump
{
    internal static void DequeAllGameEvents(bool fExecute)
    {
    }
}

// Enumerate all events for JA2
public enum eJA2Events
{
    E_PLAYSOUND,
    S_CHANGEDEST,
    //	S_GETNEWPATH,
    S_BEGINTURN,
    S_CHANGESTANCE,
    S_SETDESIREDDIRECTION,
    S_BEGINFIREWEAPON,
    S_FIREWEAPON,
    S_WEAPONHIT,
    S_STRUCTUREHIT,
    S_WINDOWHIT,
    S_MISS,
    S_NOISE,
    S_STOP_MERC,

    EVENTS_LOCAL_AND_NETWORK,               // Events above here are sent locally and over network

    S_GETNEWPATH,
    S_SETPOSITION,
    S_CHANGESTATE,
    S_SETDIRECTION,
    EVENTS_ONLY_USED_LOCALLY,           // Events above are only used locally 

    S_SENDPATHTONETWORK,
    S_UPDATENETWORKSOLDIER,
    EVENTS_ONLY_SENT_OVER_NETWORK,  // Events above are only sent to the network

    NUM_EVENTS,
}

// Enumerate all structures for events
public struct EV_E_PLAYSOUND
{
    public int usIndex;
    public int usRate;
    public int ubVolume;
    public int ubLoops;
    public int uiPan;
}

public struct EV_S_CHANGESTATE
{
    public int usSoldierID;
    public int uiUniqueId;
    public AnimationStates usNewState;
    public int sXPos;
    public int sYPos;
    public int usStartingAniCode;
    public bool fForce;
}

public struct EV_S_CHANGEDEST
{
    public int usSoldierID;
    public int uiUniqueId;
    public int usNewDestination;
}

public struct EV_S_SETPOSITION
{
    public int usSoldierID;
    public int uiUniqueId;
    public float dNewXPos;
    public float dNewYPos;
}

public struct EV_S_GETNEWPATH
{
    public int usSoldierID;
    public int uiUniqueId;
    public int sDestGridNo;
    public AnimationStates usMovementAnim;
}

public struct EV_S_BEGINTURN
{
    public int usSoldierID;
    public int uiUniqueId;
}

public struct EV_S_CHANGESTANCE
{
    public int usSoldierID;
    public int uiUniqueId;
    public int ubNewStance;
    public int sXPos;
    public int sYPos;
}

public struct EV_S_SETDIRECTION
{
    public int usSoldierID;
    public int uiUniqueId;
    public int usNewDirection;
}

public struct EV_S_SETDESIREDDIRECTION
{
    public int usSoldierID;
    public int uiUniqueId;
    public WorldDirections usDesiredDirection;
}

public struct EV_S_BEGINFIREWEAPON
{
    public int usSoldierID;
    public int uiUniqueId;
    public int sTargetGridNo;
    public int bTargetLevel;
    public int bTargetCubeLevel;
}


public struct EV_S_FIREWEAPON
{
    public int usSoldierID;
    public int uiUniqueId;
    public int sTargetGridNo;
    public int bTargetLevel;
    public int bTargetCubeLevel;
}

public struct EV_S_WEAPONHIT
{
    public int usSoldierID;
    public int uiUniqueId;
    public int usWeaponIndex;
    public int sDamage;
    public int sBreathLoss;
    public int usDirection;
    public int sXPos;
    public int sYPos;
    public int sZPos;
    public int sRange;
    public int ubAttackerID;
    public bool fHit;
    public int ubSpecial;
    public int ubLocation;
}

public struct EV_S_STRUCTUREHIT
{
    public int sXPos;
    public int sYPos;
    public int sZPos;
    public int usWeaponIndex;
    public int bWeaponStatus;
    public int ubAttackerID;
    public int usStructureID;
    public int iImpact;
    public int iBullet;
}

public struct EV_S_WINDOWHIT
{
    public int sGridNo;
    public int usStructureID;
    public bool fBlowWindowSouth;
    public bool fLargeForce;
}

public struct EV_S_MISS
{
    public int ubAttackerID;
}

public struct EV_S_NOISE
{
    public int ubNoiseMaker;
    public int sGridNo;
    public int bLevel;
    public int ubTerrType;
    public int ubVolume;
    public int ubNoiseType;
}

public struct EV_S_STOP_MERC
{
    public int usSoldierID;
    public int uiUniqueId;
    public int bDirection;
    public int sGridNo;
    public int sXPos;
    public int sYPos;
}

public struct EV_S_SENDPATHTONETWORK
{
    public int usSoldierID;
    public int uiUniqueId;
    public int usPathDataSize;           // Size of Path
    public int sAtGridNo;                    // Owner merc is at this tile when sending packet
    public int usCurrentPathIndex;   // Index the owner of the merc is at when sending packet
    public int[] usPathData;// = new int[NETWORK_PATH_DATA_SIZE];       // make define  // Next X tile to go to
    public int ubNewState;           // new movment Anim
    //	public int		bActionPoints;
    //	public int		bBreath;			// current breath value
    //	public int		bDesiredDirection;

    // maybe send current action & breath points
}

public struct EV_S_UPDATENETWORKSOLDIER
{
    public int usSoldierID;
    public int uiUniqueId;
    public int sAtGridNo;                    // Owner merc is at this tile when sending packet
    public int bActionPoints;         // current A.P. value
    public int bBreath;						// current breath value
}
