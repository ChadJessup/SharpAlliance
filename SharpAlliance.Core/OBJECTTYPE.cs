using System;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

/// <summary>
/// A window onto part of the 8-byte union at the heart of OBJECTTYPE (bStatus[], ubShotsLeft[],
/// bKeyStatus[] ...). Reads and writes go straight to the shared storage, like the C arrays did.
/// </summary>
public sealed class ByteView
{
    private readonly byte[] store;
    private readonly int offset;
    private readonly bool signed;

    public ByteView(byte[] store, int offset, int length, bool signed)
    {
        this.store = store;
        this.offset = offset;
        this.Length = length;
        this.signed = signed;
    }

    public int Length { get; }

    public int this[int index]
    {
        get
        {
            if ((uint)index >= (uint)this.Length)
            {
                throw new IndexOutOfRangeException();
            }

            byte b = this.store[this.offset + index];
            return this.signed ? (sbyte)b : b;
        }
        set
        {
            if ((uint)index >= (uint)this.Length)
            {
                throw new IndexOutOfRangeException();
            }

            this.store[this.offset + index] = unchecked((byte)value);
        }
    }

    /// <summary>Lets object initializers keep writing <c>bStatus = new[] { 100 }</c>.</summary>
    public static implicit operator ByteView(int[] values)
    {
        var v = new ByteView(new byte[values.Length], 0, values.Length, signed: true);
        for (int i = 0; i < values.Length; i++)
        {
            v[i] = values[i];
        }

        return v;
    }

    public void CopyFrom(ByteView other)
    {
        for (int i = 0; i < Math.Min(this.Length, other.Length); i++)
        {
            this[i] = other[i];
        }
    }
}

/// <summary>
/// Port of the C OBJECTTYPE. In C the first 8 bytes after ubNumberOfObjects are a union: gun, ammo stack,
/// money, bomb, key and ownership views all overlay the same storage (so bGunStatus IS bStatus[0],
/// ubGunShotsLeft IS bStatus[2], and so on). That is reproduced here with one byte array and
/// properties, so code written against any view sees the same data, as in the original.
/// Layout assumes the 1-byte packing the original relies on (sizeof(OBJECTTYPE) == 36).
/// </summary>
public class OBJECTTYPE
{
    private readonly byte[] u = new byte[Globals.MAX_OBJECTS_PER_SLOT];
    private readonly ByteView gunUnused;
    private readonly ByteView shotsLeft;
    private readonly ByteView status;
    private readonly ByteView moneyUnused;
    private readonly ByteView keyStatus;
    private readonly ByteView keyUnused;
    private readonly ByteView ownershipUnused;

    public OBJECTTYPE()
    {
        this.status = new ByteView(this.u, 0, Globals.MAX_OBJECTS_PER_SLOT, signed: true);
        this.shotsLeft = new ByteView(this.u, 0, Globals.MAX_OBJECTS_PER_SLOT, signed: false);
        this.gunUnused = new ByteView(this.u, 6, Globals.MAX_OBJECTS_PER_SLOT - 6, signed: false);
        this.moneyUnused = new ByteView(this.u, 5, Globals.MAX_OBJECTS_PER_SLOT - 5, signed: false);
        this.keyStatus = new ByteView(this.u, 0, 6, signed: true);
        this.keyUnused = new ByteView(this.u, 7, 1, signed: false);
        this.ownershipUnused = new ByteView(this.u, 2, 6, signed: false);
    }

    public Items usItem;
    public int ubNumberOfObjects;

    // ---- the union ----------------------------------------------------------------------------

    // gun
    public int bGunStatus { get => (sbyte)this.u[0]; set => this.u[0] = unchecked((byte)value); }                // status % of gun
    public AMMO ubGunAmmoType { get => (AMMO)this.u[1]; set => this.u[1] = unchecked((byte)(int)value); }      // ammo type, as per weapons.h
    public int ubGunShotsLeft { get => this.u[2]; set => this.u[2] = unchecked((byte)value); }                  // amount of ammo left
    public Items usGunAmmoItem                                                                                  // the item # for the item table
    {
        get => (Items)(short)(this.u[3] | (this.u[4] << 8));
        set { this.u[3] = unchecked((byte)(short)value); this.u[4] = unchecked((byte)((short)value >> 8)); }
    }
    public sbyte bGunAmmoStatus { get => (sbyte)this.u[5]; set => this.u[5] = unchecked((byte)value); }        // only for "attached ammo" - grenades, mortar shells
    public ByteView ubGunUnused { get => this.gunUnused; set => this.gunUnused.CopyFrom(value); }

    // ammo stack / generic stack of items
    public ByteView ubShotsLeft { get => this.shotsLeft; set => this.shotsLeft.CopyFrom(value); }
    public ByteView bStatus { get => this.status; set => this.status.CopyFrom(value); }

    // money
    public sbyte bMoneyStatus { get => (sbyte)this.u[0]; set => this.u[0] = unchecked((byte)value); }
    public int uiMoneyAmount
    {
        get => this.u[1] | (this.u[2] << 8) | (this.u[3] << 16) | (this.u[4] << 24);
        set { this.u[1] = (byte)value; this.u[2] = (byte)(value >> 8); this.u[3] = (byte)(value >> 16); this.u[4] = (byte)(value >> 24); }
    }
    public ByteView ubMoneyUnused { get => this.moneyUnused; set => this.moneyUnused.CopyFrom(value); }

    // this is used by placed bombs, switches, and the action item
    public sbyte bBombStatus { get => (sbyte)this.u[0]; set => this.u[0] = unchecked((byte)value); }           // % status
    public DetonatorType bDetonatorType { get => (DetonatorType)(sbyte)this.u[1]; set => this.u[1] = unchecked((byte)(int)value); }  // timed, remote, or pressure-activated
    public Items usBombItem                                                                                     // the usItem of the bomb.
    {
        get => (Items)(short)(this.u[2] | (this.u[3] << 8));
        set { this.u[2] = unchecked((byte)(short)value); this.u[3] = unchecked((byte)((short)value >> 8)); }
    }
    public sbyte bDelay { get => (sbyte)this.u[4]; set => this.u[4] = unchecked((byte)value); }                // >=0 values used only
    public int bFrequency { get => (sbyte)this.u[4]; set => this.u[4] = unchecked((byte)value); }              // >=0 values used only (shares the byte with bDelay)
    public byte ubBombOwner { get => this.u[5]; set => this.u[5] = value; }                                     // side which placed the bomb
    public ACTION_ITEM bActionValue { get => (ACTION_ITEM)this.u[6]; set => this.u[6] = unchecked((byte)(int)value); }  // this is used by the ACTION_ITEM fake item
    public byte ubTolerance { get => this.u[7]; set => this.u[7] = value; }                                     // tolerance value for panic triggers
    public byte ubLocationID { get => this.u[7]; set => this.u[7] = value; }                                    // location value for remote non-bomb (special!) triggers

    // keys
    public ByteView bKeyStatus { get => this.keyStatus; set => this.keyStatus.CopyFrom(value); }
    public byte ubKeyID { get => this.u[6]; set => this.u[6] = value; }
    public ByteView ubKeyUnused { get => this.keyUnused; set => this.keyUnused.CopyFrom(value); }

    // ownership
    public byte ubOwnerProfile { get => this.u[0]; set => this.u[0] = value; }
    public byte ubOwnerCivGroup { get => this.u[1]; set => this.u[1] = value; }
    public ByteView ubOwnershipUnused { get => this.ownershipUnused; set => this.ownershipUnused.CopyFrom(value); }

    // ---- not part of the union ----------------------------------------------------------------

    // attached objects
    public Items[] usAttachItem = new Items[Globals.MAX_ATTACHMENTS];
    public int[] bAttachStatus = new int[Globals.MAX_ATTACHMENTS];

    public OBJECT fFlags;
    public byte ubMission;
    public sbyte bTrap;        // 1-10 exp_lvl to detect
    public NPCID ubImprintID;  // ID of merc that item is imprinted on
    public int ubWeight;
    public byte fUsed;                // flags for whether the item is used or not

    // ---- C struct semantics ---------------------------------------------------------------------

    /// <summary>memcpy(&amp;dest, &amp;src, sizeof(OBJECTTYPE))</summary>
    public void CopyFrom(OBJECTTYPE src)
    {
        if (ReferenceEquals(this, src))
        {
            return;
        }

        this.usItem = src.usItem;
        this.ubNumberOfObjects = src.ubNumberOfObjects;
        Array.Copy(src.u, this.u, this.u.Length);
        Array.Copy(src.usAttachItem, this.usAttachItem, this.usAttachItem.Length);
        Array.Copy(src.bAttachStatus, this.bAttachStatus, this.bAttachStatus.Length);
        this.fFlags = src.fFlags;
        this.ubMission = src.ubMission;
        this.bTrap = src.bTrap;
        this.ubImprintID = src.ubImprintID;
        this.ubWeight = src.ubWeight;
        this.fUsed = src.fUsed;
    }

    /// <summary>memset(&amp;obj, 0, sizeof(OBJECTTYPE))</summary>
    public void Clear() => this.CopyFrom(new OBJECTTYPE());

    public OBJECTTYPE Clone()
    {
        var o = new OBJECTTYPE();
        o.CopyFrom(this);
        return o;
    }
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
