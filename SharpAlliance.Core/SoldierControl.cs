using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{
    public class SoldierControl
    {
        //Kris:  November 10, 1997
        //Please don't change this value from 10.  It will invalidate all of the maps and soldiers.
        public const int MAXPATROLGRIDS = 10;  // *** THIS IS A DUPLICATION - MUST BE MOVED !

        public const int NO_PROFILE = 200;
        public const int MAX_FULLTILE_DIRECTIONS = 3;
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
}
