using System;

namespace SharpAlliance.Core.SubSystems;

public class OBJECTTYPE
{
    public Items usItem;
    public byte ubNumberOfObjects;
    public int bGunStatus;            // status % of gun
    public AMMO ubGunAmmoType;    // ammo type, as per weapons.h
    public byte ubGunShotsLeft;   // duh, amount of ammo left
    public Items usGunAmmoItem;   // the item # for the item table
    public sbyte bGunAmmoStatus; // only for "attached ammo" - grenades, mortar shells
    public byte[] ubGunUnused = new byte[Globals.MAX_OBJECTS_PER_SLOT - 6];
    public byte[] ubShotsLeft = new byte[Globals.MAX_OBJECTS_PER_SLOT];
    public int[] bStatus = new int[Globals.MAX_OBJECTS_PER_SLOT];
    public sbyte bMoneyStatus;
    public int uiMoneyAmount;
    public byte[] ubMoneyUnused = new byte[Globals.MAX_OBJECTS_PER_SLOT - 5];

    // this is used by placed bombs, switches, and the action item
    public sbyte bBombStatus;           // % status
    public sbyte bDetonatorType;        // timed, remote, or pressure-activated
    public ushort usBombItem;              // the usItem of the bomb.
    public sbyte bDelay;                // >=0 values used only
    public sbyte bFrequency;        // >=0 values used only
    public byte ubBombOwner; // side which placed the bomb
    public byte bActionValue;// this is used by the ACTION_ITEM fake item
    public byte ubTolerance; // tolerance value for panic triggers
    public byte ubLocationID; // location value for remote non-bomb (special!) triggers
    public sbyte[] bKeyStatus = new sbyte[6];
    public byte ubKeyID;
    public byte[] ubKeyUnused = new byte[1];
    public byte ubOwnerProfile;
    public byte ubOwnerCivGroup;
    public byte[] ubOwnershipUnused = new byte[6];
    // attached objects
    public Items[] usAttachItem = new Items[Globals.MAX_ATTACHMENTS];
    public Items[] bAttachStatus = new Items[Globals.MAX_ATTACHMENTS];

    public OBJECT fFlags;
    public byte ubMission;
    public sbyte bTrap;        // 1-10 exp_lvl to detect
    public byte ubImprintID;  // ID of merc that item is imprinted on
    public int ubWeight;
    public byte fUsed;                // flags for whether the item is used or not
}

[Flags]
public enum OBJECT
{
    UNDROPPABLE = 0x01,
    MODIFIED = 0x02,
    AI_UNUSABLE = 0x04,
    ARMED_BOMB = 0x08,
    KNOWN_TO_BE_TRAPPED = 0x10,
    DISABLED_BOMB = 0x20,
    ALARM_TRIGGER = 0x40,
    NO_OVERWRITE = 0x80,
}
