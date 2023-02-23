﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class HandleDoors
{
    public static void SetDoorString(int sGridNo)
    {
        DOOR? pDoor;
        DOOR_STATUS? pDoorStatus;
        STRUCTURE? pStructure;

        bool fTrapped = false;

        // Try and get a door if one exists here
        pDoor = FindDoorInfoAtGridNo(sGridNo);

        if (Globals.gfUIIntTileLocation == false)
        {
            if (pDoor == null)
            {
                //wcscpy(Globals.gzIntTileLocation, Globals.TacticalStr[(int)STR.DOOR_DOOR_MOUSE_DESCRIPTION]);
                Globals.gfUIIntTileLocation = true;
            }
            else
            {
                //wcscpy(Globals.gzIntTileLocation, Globals.TacticalStr[(int)STR.DOOR_DOOR_MOUSE_DESCRIPTION]);
                Globals.gfUIIntTileLocation = true;

                // CHECK PERCEIVED VALUE
                switch (pDoor?.bPerceivedTrapped)
                {
                    case DOOR_PERCEIVED.TRAPPED:

                        //Globals.gzIntTileLocation2, Globals.TacticalStr[(int)STR.DOOR_TRAPPED_MOUSE_DESCRIPTION]);
                        Globals.gfUIIntTileLocation2 = true;
                        fTrapped = true;
                        break;
                }

                if (!fTrapped)
                {
                    // CHECK PERCEIVED VALUE
                    switch (pDoor?.bPerceivedLocked)
                    {
                        case DOOR_PERCEIVED.UNKNOWN:

                            break;

                        case DOOR_PERCEIVED.LOCKED:

                            // Globals.gzIntTileLocation2 = Globals.TacticalStr[(int)STR.DOOR_LOCKED_MOUSE_DESCRIPTION];
                            Globals.gfUIIntTileLocation2 = true;
                            break;

                        case DOOR_PERCEIVED.UNLOCKED:

                            //Globals.gzIntTileLocation2 = Globals.TacticalStr[(int)STR.DOOR_UNLOCKED_MOUSE_DESCRIPTION];
                            Globals.gfUIIntTileLocation2 = true;
                            break;

                        case DOOR_PERCEIVED.BROKEN:

                            //wcscpy(Globals.gzIntTileLocation2, Globals.TacticalStr[(int)STR.DOOR_BROKEN_MOUSE_DESCRIPTION]);
                            Globals.gfUIIntTileLocation2 = true;
                            break;

                    }
                }

            }
        }

        // ATE: If here, we try to say, opened or closed...
        if (Globals.gfUIIntTileLocation2 == false)
        {
            // Try to get doors status here...
            pDoorStatus = GetDoorStatus(sGridNo);
            if (pDoorStatus == null || (pDoorStatus?.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_NOTSET) ?? false))
            {
                // OK, get status based on graphic.....
                pStructure = FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);
                if (pStructure is not null)
                {
                    if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                    {
                        // Door is opened....
                        //wcscpy(Globals.gzIntTileLocation2, EnglishText.pMessageStrings[MSG.OPENED]);
                        Globals.gfUIIntTileLocation2 = true;
                    }
                    else
                    {
                        // Door is closed
                        //wcscpy(Globals.gzIntTileLocation2, EnglishText.pMessageStrings[MSG.CLOSED]);
                        Globals.gfUIIntTileLocation2 = true;
                    }
                }
            }
            else
            {
                // Use percived value
                if (pDoorStatus?.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_OPEN) ?? false)
                {
                    // Door is opened....
                    //wcscpy(Globals.gzIntTileLocation2, EnglishText.pMessageStrings[MSG.OPENED]);
                    Globals.gfUIIntTileLocation2 = true;
                }
                else
                {
                    // Door is closed
                    //wcscpy(Globals.gzIntTileLocation2, EnglishText.pMessageStrings[MSG.CLOSED]);
                    Globals.gfUIIntTileLocation2 = true;
                }
            }
        }
    }
}

// Defines below for the perceived value of the door
public enum DOOR_PERCEIVED
{
    UNKNOWN = 0,
    LOCKED = 1,
    UNLOCKED = 2,
    BROKEN = 3,
    TRAPPED = 1,
    UNTRAPPED = 2,
}

public struct DOOR
{
    public int sGridNo;
    public bool fLocked;                            // is the door locked
    public int ubTrapLevel;                  // difficulty of finding the trap, 0-10
    public int ubTrapID;                         // the trap type (0 is no trap)
    public int ubLockID;                         // the lock (0 is no lock)
    public DOOR_PERCEIVED bPerceivedLocked;          // The perceived lock value can be different than the fLocked.
                                          // Values for this include the fact that we don't know the status of
                                          // the door, etc
    public DOOR_PERCEIVED bPerceivedTrapped;     // See above, but with respect to traps rather than locked status
    public int bLockDamage;                   // Damage to the lock
    public int[] bPadding;// [4];					// extra bytes
}

public struct LOCK
{
    public int[] ubEditorName;// [Globals.MAXLOCKDESCLENGTH];  // name to display in editor
    public int usKeyItem;                                                   // key for this door uses which graphic (item #)?
    public int ubLockType;                                                   // regular, padlock, electronic, etc
    public int ubPickDifficulty;                                     // difficulty to pick such a lock
    public int ubSmashDifficulty;                                        // difficulty to smash such a lock
    public int ubFiller;
}
