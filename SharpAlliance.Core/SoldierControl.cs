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
    public enum SoldierClass
    {
        SOLDIER_CLASS_NONE,
        SOLDIER_CLASS_ADMINISTRATOR,
        SOLDIER_CLASS_ELITE,
        SOLDIER_CLASS_ARMY,
        SOLDIER_CLASS_GREEN_MILITIA,
        SOLDIER_CLASS_REG_MILITIA,
        SOLDIER_CLASS_ELITE_MILITIA,
        SOLDIER_CLASS_CREATURE,
        SOLDIER_CLASS_MINER,
    };
}
