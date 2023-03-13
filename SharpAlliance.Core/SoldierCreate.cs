using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;
namespace SharpAlliance.Core;

public class SoldierCreate
{
    internal void TacticalRemoveSoldier(int usSoldierIndex)
    {
        throw new NotImplementedException();
    }
}

//Kris: 
//This value is the total maximum number of slots in a map.  
//Players		20
//Enemies		32
//Creatures 32
//Rebels		32
//Civilians 32
//Total			148

//Kris:  SERIALIZING INFORMATION
//All maps must have:
//	-MAPCREATE_STRUCT
//		MAPCREATE_STRUCT.ubNumIndividuals determines how many BASIC_SOLDIERCREATE_STRUCTs there are
//  -The BASIC_SOLDIERCREATE_STRUCTS are saved contiguously, but if any of them
//		fDetailedPlacement set, then there is a SOLDIERCREATE_STRUCT saved immediately after.

//These are the placement slots used by the editor to define where characters are in a map, what 
//they are, what team they are on, personality traits, etc.  The Merc section of the editor is 
//what is used to define these values.
public class BASIC_SOLDIERCREATE_STRUCT
{
    public bool fDetailedPlacement;         //Specialized information.  Has a counterpart containing all info.
    public int usStartingGridNo;                //Where the placement position is.
    public int bTeam;                                         //The team this individual is part of.
    public int bRelativeAttributeLevel;
    public int bRelativeEquipmentLevel;
    public int bDirection;                                //1 of 8 values (always mandatory)
    public int bOrders;
    public int bAttitude;
    public int bBodyType;                                 //up to 128 body types, -1 means random
    public int[] sPatrolGrid = new int[MAXPATROLGRIDS]; //possible locations to visit, patrol, etc.
    public int bPatrolCnt;
    public bool fOnRoof;
    public int ubSoldierClass;                           //army, administrator, elite
    public int ubCivilianGroup;
    public bool fPriorityExistance;         //These slots are used first
    public bool fHasKeys;
    public int[] PADDINGSLOTS = new int[14];
} //50 bytes

public class SOLDIERCREATE_STRUCT
{
    //Bulletproofing so static detailed placements aren't used to tactically create soldiers.
    //Used by editor for validation purposes.
    public bool fStatic;

    //Profile information used for special NPCs and player mercs.
    public int ubProfile;
    public bool fPlayerMerc;
    public bool fPlayerPlan;
    public bool fCopyProfileItemsOver;

    //Location information
    public int sSectorX;
    public MAP_ROW sSectorY;
    public int bDirection;
    public int sInsertionGridNo;

    // Can force a team, but needs flag set
    public int bTeam;
    public int bBodyType;

    //Orders and attitude settings
    public int bAttitude;
    public int bOrders;

    //Attributes
    public int bLifeMax;
    public int bLife;
    public int bAgility;
    public int bDexterity;
    public int bExpLevel;
    public int bMarksmanship;
    public int bMedical;
    public int bMechanical;
    public int bExplosive;
    public int bLeadership;
    public int bStrength;
    public int bWisdom;
    public int bMorale;
    public int bAIMorale;

    //Inventory
    public OBJECTTYPE[] Inv = new OBJECTTYPE[(int)InventorySlot.NUM_INV_SLOTS];

    //Palette information for soldiers.
    public PaletteRepID HeadPal;
    public PaletteRepID PantsPal;
    public PaletteRepID VestPal;
    public PaletteRepID SkinPal;
    public PaletteRepID MiscPal;

    //Waypoint information for patrolling
    public int[] sPatrolGrid = new int[MAXPATROLGRIDS];
    public int bPatrolCnt;
    //Kris:  Additions November 16, 1997 (padding down to 129 from 150)
    public bool fVisible;
    public string name;
    public int ubSoldierClass;   //army, administrator, elite
    public bool fOnRoof;
    public int bSectorZ;
    public SOLDIERTYPE? pExistingSoldier;
    public bool fUseExistingSoldier;
    public int ubCivilianGroup;
    public bool fKillSlotIfOwnerDies;
    public int ubScheduleID;
    public bool fUseGivenVehicle;
    public int bUseGivenVehicleID;
    public bool fHasKeys;
    public int[] bPadding = new int[115];
}
