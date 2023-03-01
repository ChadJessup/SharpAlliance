﻿using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class SoldierControl
{

}

public enum InventorySlot
{
    HELMETPOS = 0,
    VESTPOS,
    LEGPOS,
    HEAD1POS,
    HEAD2POS,
    HANDPOS,
    SECONDHANDPOS,
    BIGPOCK1POS,
    BIGPOCK2POS,
    BIGPOCK3POS,
    BIGPOCK4POS,
    SMALLPOCK1POS,
    SMALLPOCK2POS,
    SMALLPOCK3POS,
    SMALLPOCK4POS,
    SMALLPOCK5POS,
    SMALLPOCK6POS,
    SMALLPOCK7POS,
    SMALLPOCK8POS, // = 18, so 19 pockets needed

    NUM_INV_SLOTS,
};

//used for color codes, but also shows the enemy type for debugging purposes
public enum SOLDIER_CLASS
{
    NONE,
    ADMINISTRATOR,
    ELITE,
    ARMY,
    GREEN_MILITIA,
    REG_MILITIA,
    ELITE_MILITIA,
    CREATURE,
    MINER,
};


// Soldier status flags
[Flags]
public enum SOLDIER : uint
{
    IS_TACTICALLY_VALID = 0x00000001,
    SHOULD_BE_TACTICALLY_VALID = 0x00000002,
    MULTI_SELECTED = 0x00000004,
    PC = 0x00000008,
    ATTACK_NOTICED = 0x00000010,
    PCUNDERAICONTROL = 0x00000020,
    UNDERAICONTROL = 0x00000040,
    DEAD = 0x00000080,
    GREEN_RAY = 0x00000100,
    LOOKFOR_ITEMS = 0x00000200,
    ENEMY = 0x00000400,
    ENGAGEDINACTION = 0x00000800,
    ROBOT = 0x00001000,
    MONSTER = 0x00002000,
    ANIMAL = 0x00004000,
    VEHICLE = 0x00008000,
    MULTITILE_NZ = 0x00010000,
    MULTITILE_Z = 0x00020000,
    MULTITILE = (MULTITILE_Z | MULTITILE_NZ),
    RECHECKLIGHT = 0x00040000,
    TURNINGFROMHIT = 0x00080000,
    BOXER = 0x00100000,
    LOCKPENDINGACTIONCOUNTER = 0x00200000,
    COWERING = 0x00400000,
    MUTE = 0x00800000,
    GASSED = 0x01000000,
    OFF_MAP = 0x02000000,
    PAUSEANIMOVE = 0x04000000,
    DRIVER = 0x08000000,
    PASSENGER = 0x10000000,
    NPC_DOING_PUNCH = 0x20000000,
    NPC_SHOOTING = 0x40000000,
    LOOK_NEXT_TURNSOLDIER = 0x80000000,
}

public enum WM
{
    NORMAL = 0,
    BURST,
    ATTACHED,
    NUM_WEAPON_MODES
}

public enum MERC
{
    OPENDOOR,
    OPENSTRUCT,
    PICKUPITEM,
    PUNCH,
    KNIFEATTACK,
    GIVEAID,
    GIVEITEM,
    WAITFOROTHERSTOTRIGGER,
    CUTFFENCE,
    DROPBOMB,
    STEAL,
    TALK,
    ENTER_VEHICLE,
    REPAIR,
    RELOADROBOT,
    TAKEBLOOD,
    ATTACH_CAN,
    FUEL_VEHICLE,

    NO_PENDING_ACTION = 255,
}

[Flags]
public enum SOLDIER_MISC
{
    HEARD_GUNSHOT = 0x01,
    // make sure soldiers (esp tanks) are not hurt multiple times by explosions
    HURT_BY_EXPLOSION = 0x02,
    // should be revealed due to xrays
    XRAYED = 0x04,
}

// reasons for being unable to continue movement
public enum REASON_STOPPED
{
    NO_APS,
    SIGHT,
};


[Flags]
public enum HIT_BY
{
    HIT_BY_TEARGAS = 0x01,
    HIT_BY_MUSTARDGAS = 0x02,
    HIT_BY_CREATUREGAS = 0x04,
}

public enum SOLDIER_QUOTE
{
    SAID_IN_SHIT = 0x0001,
    SAID_LOW_BREATH = 0x0002,
    SAID_BEING_PUMMELED = 0x0004,
    SAID_NEED_SLEEP = 0x0008,
    SAID_LOW_MORAL = 0x0010,
    SAID_MULTIPLE_CREATURES = 0x0020,
    SAID_ANNOYING_MERC = 0x0040,
    SAID_LIKESGUN = 0x0080,
    SAID_DROWNING = 0x0100,
    SAID_ROTTINGCORPSE = 0x0200,
    SAID_SPOTTING_CREATURE_ATTACK = 0x0400,
    SAID_SMELLED_CREATURE = 0x0800,
    SAID_ANTICIPATING_DANGER = 0x1000,
    SAID_WORRIED_ABOUT_CREATURES = 0x2000,
    SAID_PERSONALITY = 0x4000,
    SAID_FOUND_SOMETHING_NICE = 0x8000,
    SAID_EXT_HEARD_SOMETHING = 0x0001,
    SAID_EXT_SEEN_CREATURE_ATTACK = 0x0002,
    SAID_EXT_USED_BATTLESOUND_HIT = 0x0004,
    SAID_EXT_CLOSE_CALL = 0x0008,
    SAID_EXT_MIKE = 0x0010,
    SAID_DONE_ASSIGNMENT = 0x0020,
    SAID_BUDDY_1_WITNESSED = 0x0040,
    SAID_BUDDY_2_WITNESSED = 0x0080,
    SAID_BUDDY_3_WITNESSED = 0x0100,
}
