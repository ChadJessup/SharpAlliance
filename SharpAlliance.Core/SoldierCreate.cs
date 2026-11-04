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
    private static int guiCurrentUniqueSoldierId = 1;

    /// <summary>
    /// Creates a soldier in the first free slot of its team (TacticalCreateSoldier in Soldier Create.c).
    /// TODO(port): the parts that need the tactical engine are left out: CreateSoldierCommon (animation, palettes, light,
    /// world placement), vehicles, creature/bloodcat inventories, face loading, the autoresolve and saved-game paths.
    /// </summary>
    internal static bool TacticalCreateSoldier(SOLDIERCREATE_STRUCT pCreateStruct, out int ubId)
    {
        ubId = NOBODY;

        SOLDIERTYPE Soldier = new();
        Soldier.uiUniqueSoldierIdValue = guiCurrentUniqueSoldierId++;

        // OK, CHECK IF WE HAVE A VALID PROFILE ID!
        if (pCreateStruct.ubProfile != NO_PROFILE)
        {
            TacticalCopySoldierFromProfile(Soldier, pCreateStruct);
        }
        else
        {
            TacticalCopySoldierFromCreateStruct(Soldier, pCreateStruct);
        }

        // First off, force player team if they are a player guy!
        if (pCreateStruct.fPlayerMerc)
        {
            Soldier.uiStatusFlags |= SOLDIER.PC;
            Soldier.bTeam = gbPlayerNum;
            Soldier.bVisible = 1;
        }
        else if (pCreateStruct.fPlayerPlan)
        {
            Soldier.uiStatusFlags |= SOLDIER.PC;
            Soldier.bVisible = 1;
        }
        else
        {
            Soldier.uiStatusFlags |= SOLDIER.ENEMY;
        }

        // Check for auto team
        if ((int)pCreateStruct.bTeam == SOLDIER_CREATE_AUTO_TEAM)
        {
            if (pCreateStruct.fPlayerMerc)
            {
                Soldier.bTeam = OUR_TEAM;
                Soldier.bNormalSmell = NORMAL_HUMAN_SMELL_STRENGTH;
            }
            else if (pCreateStruct.fPlayerPlan)
            {
                Soldier.bTeam = TEAM.PLAYER_PLAN;
            }
            else
            {
                Soldier.bTeam = TeamForBodyType(pCreateStruct.bBodyType);
            }
        }
        else
        {
            Soldier.bTeam = pCreateStruct.bTeam;
            if (Soldier.bTeam == MILITIA_TEAM)
            {
                Soldier.bVisible = 1;
            }
        }

        // Given team, get an ID for this guy!
        int cnt = gTacticalStatus.Team[Soldier.bTeam].bFirstID;
        int bLastTeamID = gTacticalStatus.Team[Soldier.bTeam].bLastID;
        bool fGuyAvail = false;

        // look for all mercs on the same team,
        for (; cnt <= bLastTeamID; cnt++)
        {
            if (!MercPtrs[cnt].bActive)
            {
                fGuyAvail = true;
                break;
            }
        }

        // Check if there was space!
        if (!fGuyAvail)
        {
            //No space, so can't create the soldier.
            return false;
        }

        Soldier.ubID = cnt;
        ubId = cnt;

        // Copy the items over for thew soldier, only if we have a valid profile id!
        if (pCreateStruct.ubProfile != NO_PROFILE)
        {
            CopyProfileItems(Soldier, pCreateStruct);
        }

        // TODO(port): InitSoldierFace for our team

        Soldier.bActionPoints = SoldierControl.CalcActionPoints(Soldier);
        Soldier.bInitialActionPoints = Soldier.bActionPoints;
        Soldier.bSide = gTacticalStatus.Team[Soldier.bTeam].bSide;
        Soldier.bActive = true;
        Soldier.sSectorX = pCreateStruct.sSectorX;
        Soldier.sSectorY = pCreateStruct.sSectorY;
        Soldier.bSectorZ = pCreateStruct.bSectorZ;
        Soldier.ubInsertionDirection = pCreateStruct.bDirection;
        Soldier.bDesiredDirection = pCreateStruct.bDirection;
        Soldier.bDominantDir = pCreateStruct.bDirection;
        Soldier.bDirection = pCreateStruct.bDirection;
        Soldier.sInsertionGridNo = pCreateStruct.sInsertionGridNo;
        Soldier.bOldLife = Soldier.bLifeMax;

        if (Soldier.bTeam == CIV_TEAM)
        {
            Soldier.bNeutral = 1;
        }

        if (Soldier.ubProfile == NO_PROFILE)
        {
            Soldier.ubBattleSoundID = Globals.Random.Next(3);
        }

        Soldier.bNormalSmell = NORMAL_HUMAN_SMELL_STRENGTH;

        // Copy into the merc slot
        Menptr[cnt] = Soldier;
        MercPtrs[cnt] = Soldier;

        // Alrighty then, we are set to create the merc, stuff after here can fail!
        if (!CreateSoldierCommon(Soldier.ubBodyType, Soldier, Soldier.ubID, AnimationStates.STANDING))
        {
            Soldier.bActive = false;
            ubId = NOBODY;
            return false;
        }

        // Increment men in sector number!
        gTacticalStatus.Team[Soldier.bTeam].bMenInSector++;

        return true;
    }

    /// <summary>Gives a soldier created from a profile the items listed in that profile (CopyProfileItems).</summary>
    private static void CopyProfileItems(SOLDIERTYPE pSoldier, SOLDIERCREATE_STRUCT pCreateStruct)
    {
        var pProfile = gMercProfiles[pCreateStruct.ubProfile];

        // Copy over inv if we want to
        if (!pCreateStruct.fCopyProfileItemsOver && pSoldier.bTeam == OUR_TEAM)
        {
            return;
        }

        if (pCreateStruct.fPlayerMerc)
        {
            // do some special coding to put stuff in the profile in better-looking spots
            for (InventorySlot slot = 0; slot < InventorySlot.NUM_INV_SLOTS; slot++)
            {
                pSoldier.inv[slot] = new OBJECTTYPE();
            }

            for (InventorySlot cnt = 0; cnt < InventorySlot.NUM_INV_SLOTS; cnt++)
            {
                if (!pProfile.inv.TryGetValue(cnt, out Items item) || item == Items.NONE)
                {
                    continue;
                }

                OBJECTTYPE Obj = new();
                ItemSubSystem.CreateItems(item, pProfile.bInvStatus[(int)cnt], pProfile.bInvNumber[(int)cnt], Obj);

                if (Item[Obj.usItem].fFlags.HasFlag(ItemAttributes.ITEM_ATTACHMENT))
                {
                    // try to find the appropriate item to attach to!
                    InventorySlot cnt2;
                    for (cnt2 = 0; cnt2 < InventorySlot.NUM_INV_SLOTS; cnt2++)
                    {
                        if (pSoldier.inv[cnt2].usItem != Items.NONE && ItemSubSystem.ValidAttachment(Obj.usItem, pSoldier.inv[cnt2].usItem))
                        {
                            ItemSubSystem.AttachObject(null, pSoldier.inv[cnt2], Obj);
                            break;
                        }
                    }

                    if (cnt2 == InventorySlot.NUM_INV_SLOTS)
                    {
                        // oh well, couldn't find anything to attach to!
                        ItemSubSystem.AutoPlaceObject(pSoldier, Obj, false);
                    }
                }
                else
                {
                    ItemSubSystem.AutoPlaceObject(pSoldier, Obj, false);
                }
            }

            pProfile.usOptionalGearCost = 0;
        }
        else
        {
            for (InventorySlot cnt = 0; cnt < InventorySlot.NUM_INV_SLOTS; cnt++)
            {
                if (pProfile.inv.TryGetValue(cnt, out Items item) && item != Items.NONE)
                {
                    if (Item[item].usItemClass == IC.KEY)
                    {
                        // TODO(port): the hardcoded keys of Brewster, Skipper and Doreen (CreateKeyObject); no key otherwise
                        pSoldier.inv[cnt] = new OBJECTTYPE();
                    }
                    else
                    {
                        ItemSubSystem.CreateItems(item, pProfile.bInvStatus[(int)cnt], pProfile.bInvNumber[(int)cnt], pSoldier.inv[cnt]);
                    }

                    if (item == Items.ROCKET_RIFLE || item == Items.AUTO_ROCKET_RIFLE)
                    {
                        pSoldier.inv[cnt].ubImprintID = pSoldier.ubProfile;
                    }

                    // TODO(port): OBJECT_UNDROPPABLE from gubItemDroppableFlag / ubInvUndroppable
                }
                else
                {
                    pSoldier.inv[cnt] = new OBJECTTYPE();
                }
            }

            if (pProfile.uiMoney > 0)
            {
                int uiMoneyLeft = pProfile.uiMoney;
                InventorySlot bSlot = ItemSubSystem.FindEmptySlotWithin(pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.SMALLPOCK8POS);

                // add in increments of
                while (bSlot != (InventorySlot)NO_SLOT)
                {
                    int uiMoneyLimitInSlot = MAX_MONEY_PER_SLOT;
                    if (bSlot >= InventorySlot.SMALLPOCK1POS)
                    {
                        uiMoneyLimitInSlot /= 2;
                    }

                    ItemSubSystem.CreateItem(Items.MONEY, 100, pSoldier.inv[bSlot]);
                    if (uiMoneyLeft > uiMoneyLimitInSlot)
                    {
                        // fill pocket with money
                        pSoldier.inv[bSlot].uiMoneyAmount = uiMoneyLimitInSlot;
                        uiMoneyLeft -= uiMoneyLimitInSlot;
                    }
                    else
                    {
                        pSoldier.inv[bSlot].uiMoneyAmount = uiMoneyLeft;

                        // done!
                        break;
                    }

                    bSlot = ItemSubSystem.FindEmptySlotWithin(pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.SMALLPOCK8POS);
                }
            }
        }
    }

    /// <summary>
    /// The set-up every new soldier gets (CreateSoldierCommon in Soldier Control.c). The animation and palette set-up
    /// need the animation database and a loaded world, so they only run once a world is loaded; until then the soldier
    /// just remembers the state it should start in. TODO(port): InitAnimationCache.
    /// </summary>
    internal static bool CreateSoldierCommon(SoldierBodyTypes ubBodyType, SOLDIERTYPE pSoldier, int usSoldierID, AnimationStates usState)
    {
        //if we are loading a saved game, we DO NOT want to reset the opplist, look for enemies, or say a dying commnet
        if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            // Set initial values for opplist!
            InitSoldierOppList(pSoldier);

            if (gfWorldLoaded)
            {
                OppList.HandleSight(pSoldier, SIGHT.LOOK);
            }

            // Set some quote flags
            pSoldier.fDyingComment = pSoldier.bLife < OKLIFE;
        }

        // ATE: Reset some timer flags...
        pSoldier.uiTimeSameBattleSndDone = 0;

        // ATE: Reset every time.....
        pSoldier.fSoldierWasMoving = true;
        pSoldier.iTuringSoundID = NO_SAMPLE;
        pSoldier.uiTimeSinceLastBleedGrunt = 0;

        if (usSoldierID <= gTacticalStatus.Team[OUR_TEAM].bLastID)
        {
            pSoldier.pKeyRing = new KEY_ON_RING[Keys.NUM_KEYS];
            for (int iCounter = 0; iCounter < Keys.NUM_KEYS; iCounter++)
            {
                pSoldier.pKeyRing[iCounter] = new KEY_ON_RING { ubKeyID = 255 /* INVALID_KEY_NUMBER */ };
            }
        }

        if (gfWorldLoaded)
        {
            // OFFSET FIRST ANIMATION FRAME FOR NEW MERCS
            int startingCode = usState != AnimationStates.STANDING ? 0 : Globals.Random.Next(10);
            SoldierControl.EVENT_InitNewSoldierAnim(pSoldier, usState, startingCode, true);

            // Init palettes
            if (!SoldierControl.CreateSoldierPalettes(pSoldier))
            {
                pSoldier.bActive = false;
                return false;
            }
        }
        else
        {
            // no world yet: the animation is set up when the soldier is put into a loaded sector
            pSoldier.usAnimState = usState;
        }

        return true;
    }

    private static void InitSoldierOppList(SOLDIERTYPE pSoldier)
    {
        Array.Fill(pSoldier.bOppList, NOT_HEARD_OR_SEEN);
        pSoldier.bOppCnt = 0;

        // TODO(port): ResetLastKnownLocs
        gbSeenOpponents[pSoldier.ubID] = new List<int>(new int[MAXMERCS]);
    }

    private static TEAM TeamForBodyType(SoldierBodyTypes bodyType)
    {
        switch (bodyType)
        {
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:
            case SoldierBodyTypes.LARVAE_MONSTER:
            case SoldierBodyTypes.INFANT_MONSTER:
            case SoldierBodyTypes.QUEENMONSTER:
                return CREATURE_TEAM;

            case SoldierBodyTypes.FATCIV:
            case SoldierBodyTypes.MANCIV:
            case SoldierBodyTypes.MINICIV:
            case SoldierBodyTypes.DRESSCIV:
            case SoldierBodyTypes.HATKIDCIV:
            case SoldierBodyTypes.KIDCIV:
            case SoldierBodyTypes.COW:
            case SoldierBodyTypes.CROW:
            case SoldierBodyTypes.ROBOTNOWEAPON:
                return CIV_TEAM;

            default:
                return ENEMY_TEAM;
        }
    }

    private static bool TacticalCopySoldierFromProfile(SOLDIERTYPE pSoldier, SOLDIERCREATE_STRUCT pCreateStruct)
    {
        NPCID ubProfileIndex = pCreateStruct.ubProfile;
        var pProfile = gMercProfiles[ubProfileIndex];

        pSoldier.HeadPal = pProfile.HAIR;
        pSoldier.VestPal = pProfile.VEST;
        pSoldier.SkinPal = pProfile.SKIN;
        pSoldier.PantsPal = pProfile.PANTS;

        // Set profile index!
        pSoldier.ubProfile = ubProfileIndex;
        pSoldier.ubScheduleID = pCreateStruct.ubScheduleID;
        pSoldier.bHasKeys = pCreateStruct.fHasKeys;

        pSoldier.name = pProfile.zNickname;

        pSoldier.bLife = pProfile.bLife;
        pSoldier.bLifeMax = pProfile.bLifeMax;
        pSoldier.bAgility = pProfile.bAgility;
        pSoldier.bLeadership = pProfile.bLeadership;
        pSoldier.bDexterity = pProfile.bDexterity;
        pSoldier.bStrength = pProfile.bStrength;
        pSoldier.bWisdom = pProfile.bWisdom;
        pSoldier.bExpLevel = pProfile.bExpLevel;
        pSoldier.bMarksmanship = pProfile.bMarksmanship;
        pSoldier.bMedical = pProfile.bMedical;
        pSoldier.bMechanical = pProfile.bMechanical;
        pSoldier.bExplosive = pProfile.bExplosive;
        pSoldier.bScientific = pProfile.bScientific;

        pSoldier.bVocalVolume = 64; // MIDVOLUME
        pSoldier.uiAnimSubFlags = (SUB_ANIM)pProfile.uiBodyTypeSubFlags;
        pSoldier.ubBodyType = pProfile.ubBodyType;
        pSoldier.ubCivilianGroup = (CIV_GROUP)pProfile.ubCivilianGroup;

        pSoldier.bOldAssignment = Assignment.NO_ASSIGNMENT;
        pSoldier.ubSkillTrait1 = pProfile.bSkillTrait;
        pSoldier.ubSkillTrait2 = pProfile.bSkillTrait2;

        pSoldier.bOrders = pCreateStruct.bOrders;
        pSoldier.bAttitude = pCreateStruct.bAttitude;
        pSoldier.bDirection = pCreateStruct.bDirection;
        pSoldier.bPatrolCnt = pCreateStruct.bPatrolCnt;
        Array.Copy(pCreateStruct.sPatrolGrid, pSoldier.usPatrolGrid, Math.Min(pCreateStruct.sPatrolGrid.Length, pSoldier.usPatrolGrid.Length));

        if (pSoldier.ubSkillTrait1 == SkillTrait.CAMOUFLAGED || pSoldier.ubSkillTrait2 == SkillTrait.CAMOUFLAGED)
        {
            // set camouflaged to 100 automatically
            pSoldier.bCamo = 100;
        }

        return true;
    }

    private static bool TacticalCopySoldierFromCreateStruct(SOLDIERTYPE pSoldier, SOLDIERCREATE_STRUCT pCreateStruct)
    {
        pSoldier.ubProfile = NO_PROFILE;

        pSoldier.HeadPal = pCreateStruct.HeadPal;
        pSoldier.VestPal = pCreateStruct.VestPal;
        pSoldier.SkinPal = pCreateStruct.SkinPal;
        pSoldier.PantsPal = pCreateStruct.PantsPal;

        pSoldier.name = pCreateStruct.name ?? string.Empty;
        pSoldier.ubScheduleID = pCreateStruct.ubScheduleID;
        pSoldier.bHasKeys = pCreateStruct.fHasKeys;

        pSoldier.bLife = pCreateStruct.bLife;
        pSoldier.bLifeMax = pCreateStruct.bLifeMax;
        pSoldier.bAgility = pCreateStruct.bAgility;
        pSoldier.bLeadership = pCreateStruct.bLeadership;
        pSoldier.bDexterity = pCreateStruct.bDexterity;
        pSoldier.bStrength = pCreateStruct.bStrength;
        pSoldier.bWisdom = pCreateStruct.bWisdom;
        pSoldier.bExpLevel = pCreateStruct.bExpLevel;
        pSoldier.bMarksmanship = pCreateStruct.bMarksmanship;
        pSoldier.bMedical = pCreateStruct.bMedical;
        pSoldier.bMechanical = pCreateStruct.bMechanical;
        pSoldier.bExplosive = pCreateStruct.bExplosive;

        pSoldier.ubBodyType = pCreateStruct.bBodyType;
        pSoldier.ubSoldierClass = pCreateStruct.ubSoldierClass;
        pSoldier.ubCivilianGroup = pCreateStruct.ubCivilianGroup;
        pSoldier.bOrders = pCreateStruct.bOrders;
        pSoldier.bAttitude = pCreateStruct.bAttitude;
        pSoldier.bDirection = pCreateStruct.bDirection;
        pSoldier.bPatrolCnt = pCreateStruct.bPatrolCnt;
        Array.Copy(pCreateStruct.sPatrolGrid, pSoldier.usPatrolGrid, Math.Min(pCreateStruct.sPatrolGrid.Length, pSoldier.usPatrolGrid.Length));

        // TODO(port): the rest of TacticalCopySoldierFromCreateStruct (inventory, palettes chosen by class, morale, ...)
        return true;
    }

    internal bool TacticalRemoveSoldier(int usSoldierIndex)
        => InternalTacticalRemoveSoldier(usSoldierIndex, true);

    internal static bool InternalTacticalRemoveSoldier(int usSoldierIndex, bool fRemoveVehicle)
    {
        // Check range of index given
        if (usSoldierIndex < 0 || usSoldierIndex > TOTAL_SOLDIERS - 1)
        {
            return false;
        }

        // ATE: If this guy is our global selected dude, take selection off...
        if (gfUIFullTargetFound && gusUIFullTargetID == usSoldierIndex)
        {
            gfUIFullTargetFound = false;
        }

        //This one is for a single-gridno guy.....
        if (gfUISelectiveTargetFound && gusUISelectiveTargetID == usSoldierIndex)
        {
            gfUISelectiveTargetFound = false;
        }

        return TacticalRemoveSoldierPointer(MercPtrs[usSoldierIndex], fRemoveVehicle);
    }

    internal static bool TacticalRemoveSoldierPointer(SOLDIERTYPE? pSoldier, bool fRemoveVehicle)
    {
        // slots that were never filled have nothing to remove
        if (pSoldier is null || !pSoldier.bActive)
        {
            return false;
        }

        // TODO(port): DeleteSchedule, RemoveVehicleFromList, HandleCrowLeave, the AUTORESOLVE_SCREEN branch,
        // RemovePlayerFromTeamSlotGivenMercID, DeleteAniTile and DeleteSoldier are not ported yet.
        Squads.RemoveCharacterFromSquads(pSoldier);
        SoldierControl.RemoveSoldierFromGridNo(pSoldier);

        if (!pSoldier.uiStatusFlags.HasFlag(SOLDIER.OFF_MAP))
        {
            // Decrement men in sector number! (RemoveManFromTeam)
            gTacticalStatus.Team[pSoldier.bTeam].bMenInSector--;
        }

        pSoldier.bActive = false;

        return true;
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
    public int fOnRoof;
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
    public NPCID ubProfile;
    public bool fPlayerMerc;
    public bool fPlayerPlan;
    public bool fCopyProfileItemsOver;

    //Location information
    public int sSectorX;
    public MAP_ROW sSectorY;
    public WorldDirections bDirection;
    public int sInsertionGridNo;

    // Can force a team, but needs flag set
    public TEAM bTeam;
    public SoldierBodyTypes bBodyType;

    //Orders and attitude settings
    public Attitudes bAttitude;
    public Orders bOrders;

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
    public MORALE bAIMorale;

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
    public SOLDIER_CLASS ubSoldierClass;   //army, administrator, elite
    public int fOnRoof;
    public int bSectorZ;
    public SOLDIERTYPE? pExistingSoldier;
    public bool fUseExistingSoldier;
    public CIV_GROUP ubCivilianGroup;
    public bool fKillSlotIfOwnerDies;
    public int ubScheduleID;
    public bool fUseGivenVehicle;
    public int bUseGivenVehicleID;
    public int fHasKeys;
    public int[] bPadding = new int[115];
}
