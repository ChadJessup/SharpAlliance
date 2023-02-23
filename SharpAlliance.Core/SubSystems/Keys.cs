using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace SharpAlliance.Core.SubSystems;

public class Keys
{
    //This is the link to see if a door exists at a gridno.  
    public static DOOR? FindDoorInfoAtGridNo(int iMapIndex)
    {
        for (int i = 0; i < Globals.gubNumDoors; i++)
        {
            if (Globals.DoorTable[i].sGridNo == iMapIndex)
            {
                return Globals.DoorTable[i];
            }
        }
        return null;
    }

}

public struct KEY
{

    public int usItem;                      // index in item table for key
    public int fFlags;                       // flags...
    public int usSectorFound;       // where and
    public int usDateFound;			// when the key was found
}

public enum DoorTrapTypes
{
    NO_TRAP = 0,
    EXPLOSION,
    ELECTRIC,
    SIREN,
    SILENT_ALARM,
    BROTHEL_SIREN,
    SUPER_ELECTRIC,
    NUM_DOOR_TRAPS
}

[Flags]
public enum DOOR_TRAP
{
    STOPS_ACTION = 0x01,
    RECURRING = 0x02,
    SILENT = 0x04,
}

public struct DOORTRAP
{
    public DOOR_TRAP fFlags;    // stops action?  recurring trap?
}

//The status of the door, either open or closed
[Flags]
public enum DOOR_STATUS_FLAGS
{
    OPEN = 0x01,
    PERCEIVED_OPEN = 0x02,
    PERCEIVED_NOTSET = 0x04,
    BUSY = 0x08,
    HAS_TIN_CAN = 0x10,
}

public struct DOOR_STATUS
{
    public int sGridNo;
    public DOOR_STATUS_FLAGS ubFlags;
}
