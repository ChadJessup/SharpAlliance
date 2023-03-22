using System;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Points
{
    public static void DeductPoints(SOLDIERTYPE? pSoldier, int sAPCost, int sBPCost)
    {
        int  sNewAP = 0, sNewBP = 0;
        int bNewBreath;

        // in real time, there IS no AP cost, (only breath cost)
        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
            || !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            sAPCost = 0;
        }

        // Get New points
        sNewAP = pSoldier.bActionPoints - sAPCost;

        // If this is the first time with no action points, set UI flag
        if (sNewAP <= 0 && pSoldier.bActionPoints > 0)
        {
            pSoldier.fUIFirstTimeNOAP = true;
            fInterfacePanelDirty = 1;
        }

        // If we cannot deduct points, return FALSE
        if (sNewAP < 0)
        {
            sNewAP = 0;
        }

        pSoldier.bActionPoints = sNewAP;

        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Deduct Points (%d at %d) %d %d", pSoldier->ubID, pSoldier->sGridNo, sAPCost, sBPCost));

        if (AM_A_ROBOT(pSoldier))
        {
            // zap all breath costs for robot
            sBPCost = 0;
        }

        // is there a BREATH deduction/transaction to be made?  (REMEMBER: could be a GAIN!)
        if (sBPCost > 0)
        {
            // Adjust breath changes due to spending or regaining of energy
            sBPCost = AdjustBreathPts(pSoldier, sBPCost);
            sBPCost *= -1;

            pSoldier.sBreathRed -= sBPCost;

            // CJC: moved check for high breathred to below so that negative breath can be detected

            // cap breathred
            if (pSoldier->sBreathRed < 0)
            {
                pSoldier->sBreathRed = 0;
            }
            if (pSoldier->sBreathRed > 10000)
            {
                pSoldier->sBreathRed = 10000;
            }

            // Get new breath
            bNewBreath = (int)(pSoldier->bBreathMax - ((FLOAT)pSoldier->sBreathRed / (FLOAT)100));

            if (bNewBreath > 100)
            {
                bNewBreath = 100;
            }
            if (bNewBreath < 00)
            {
                // Take off 1 AP per 5 breath... rem adding a negative subtracts
                pSoldier->bActionPoints += (bNewBreath / 5);
                if (pSoldier->bActionPoints < 0)
                {
                    pSoldier->bActionPoints = 0;
                }

                bNewBreath = 0;
            }

            if (bNewBreath > pSoldier->bBreathMax)
            {
                bNewBreath = pSoldier->bBreathMax;
            }
            pSoldier->bBreath = bNewBreath;
        }

        // UPDATE BAR
        DirtyMercPanelInterface(pSoldier, DIRTYLEVEL1);

    }

    public static int AdjustBreathPts(SOLDIERTYPE? pSold, int sBPCost)
    {
        int sBreathFactor = 100;
        int ubBandaged;

        //NumMessage("BEFORE adjustments, BREATH PTS = ",breathPts);

        // in real time, there IS no AP cost, (only breath cost)
        /*
        if (!(gTacticalStatus.uiFlags & TURNBASED) || !(gTacticalStatus.uiFlags & INCOMBAT ) )
        {
            // ATE: ADJUST FOR RT - MAKE BREATH GO A LITTLE FASTER!
            sBPCost	*= TB_BREATH_DEDUCT_MODIFIER;
        }
        */


        // adjust breath factor for current breath deficiency
        sBreathFactor += (100 - pSold.bBreath);

        // adjust breath factor for current life deficiency (but add 1/2 bandaging)
        ubBandaged = pSold.bLifeMax - pSold.bLife - pSold.bBleeding;
        //sBreathFactor += (pSold->bLifeMax - (pSold->bLife + (ubBandaged / 2)));
        sBreathFactor += 100 * (pSold.bLifeMax - (pSold.bLife + (ubBandaged / 2))) / pSold.bLifeMax;

        if (pSold.bStrength > 80)
        {
            // give % reduction to breath costs for high strength mercs
            sBreathFactor -= (pSold.bStrength - 80) / 2;
        }

        /*	THIS IS OLD JAGGED ALLIANCE STUFF (left for possible future reference)

         // apply penalty due to high temperature, heat, and hot Metaviran sun
         // if INDOORS, in DEEP WATER, or possessing HEAT TOLERANCE trait
         if ((ptr->terrtype == FLOORTYPE) || (ptr->terr >= OCEAN21) ||
                               (ptr->trait == HEAT_TOLERANT))
           breathFactor += (Status.heatFactor / 5);	// 20% of normal heat penalty
         else
           breathFactor += Status.heatFactor;		// not used to this!
        */


        // if a non-swimmer type is thrashing around in deep water
        if ((pSold.ubProfile != NO_PROFILE) && (gMercProfiles[pSold.ubProfile].bPersonalityTrait == PersonalityTrait.NONSWIMMER))
        {
            if (pSold.usAnimState == AnimationStates.DEEP_WATER_TRED || pSold.usAnimState == AnimationStates.DEEP_WATER_SWIM)
            {
                sBreathFactor *= 5;     // lose breath 5 times faster in deep water!
            }
        }

        if (sBreathFactor == 0)
        {
            sBPCost = 0;
        }
        else if (sBPCost > 0)       // breath DECREASE
        {
            // increase breath COST by breathFactor
            sBPCost = ((sBPCost * sBreathFactor) / 100);
        }
        else                // breath INCREASE
        {
            // decrease breath GAIN by breathFactor
            sBPCost = ((sBPCost * 100) / sBreathFactor);
        }

        return (sBPCost);
    }

    public static int MinAPsToStartMovement(SOLDIERTYPE? pSoldier, AnimationStates usMovementMode)
    {
        int bAPs = 0;

        switch (usMovementMode)
        {
            case AnimationStates.RUNNING:
            case AnimationStates.WALKING:
                if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_PRONE)
                {
                    bAPs += AP.CROUCH + AP.PRONE;
                }
                else if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_CROUCH)
                {
                    bAPs += AP.CROUCH;
                }
                break;
            case AnimationStates.SWATTING:
                if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_PRONE)
                {
                    bAPs += AP.PRONE;
                }
                else if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
                {
                    bAPs += AP.CROUCH;
                }
                break;
            case AnimationStates.CRAWLING:
                if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND)
                {
                    bAPs += AP.CROUCH + AP.PRONE;
                }
                else if (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_CROUCH)
                {
                    bAPs += AP.CROUCH;
                }
                break;
            default:
                break;
        }

        if (usMovementMode == AnimationStates.RUNNING && pSoldier.usAnimState != AnimationStates.RUNNING)
        {
            bAPs += AP.START_RUN_COST;
        }
        return (bAPs);
    }

    public static int TerrainActionPoints(SOLDIERTYPE? pSoldier, int sGridno, int bDir, int bLevel)
    {
        int sAPCost = 0;
        int sSwitchValue;
        bool fHiddenStructVisible;               // Used for hidden struct visiblity

        if (pSoldier.bStealthMode)
        {
            sAPCost += AP.STEALTH_MODIFIER;
        }

        if (pSoldier.bReverse || gUIUseReverse)
        {
            sAPCost += AP.REVERSE_MODIFIER;
        }

        //if (GridCost[gridno] == NPCMINECOST)
        //   switchValue = BackupGridCost[gridno];
        //else

        sSwitchValue = gubWorldMovementCosts[sGridno,bDir,bLevel];

        // Check reality vs what the player knows....
        if (pSoldier.bTeam == gbPlayerNum)
        {
            // Is this obstcale a hidden tile that has not been revealed yet?
            if (DoesGridnoContainHiddenStruct(sGridno, fHiddenStructVisible))
            {
                // Are we not visible, if so use terrain costs!
                if (!fHiddenStructVisible)
                {
                    // Set cost of terrain!
                    sSwitchValue = gTileTypeMovementCost[gpWorldLevelData[sGridno].ubTerrainID];
                }
            }
        }
        if (sSwitchValue == TRAVELCOST.NOT_STANDING)
        {
            // use the cost of the terrain!
            sSwitchValue = gTileTypeMovementCost[gpWorldLevelData[sGridno].ubTerrainID];
        }
        else if (IS_TRAVELCOST_DOOR(sSwitchValue))
        {
            sSwitchValue = DoorTravelCost(pSoldier, sGridno, (int)sSwitchValue, (bool)(pSoldier.bTeam == gbPlayerNum), NULL);
        }

        if (sSwitchValue >= TRAVELCOST.BLOCKED && sSwitchValue != TRAVELCOST.DOOR)
        {
            return (100);   // Cost too much to be considered!
        }

        switch (sSwitchValue)
        {
            case TRAVELCOST.DIRTROAD:
            case TRAVELCOST.FLAT:
                sAPCost += AP.MOVEMENT_FLAT;
                break;
            //case TRAVELCOST.BUMPY		:		
            case TRAVELCOST.GRASS:
                sAPCost += AP.MOVEMENT_GRASS;
                break;
            case TRAVELCOST.THICK:
                sAPCost += AP.MOVEMENT_BUSH;
                break;
            case TRAVELCOST.DEBRIS:
                sAPCost += AP.MOVEMENT_RUBBLE;
                break;
            case TRAVELCOST.SHORE:
                sAPCost += AP.MOVEMENT_SHORE; // wading shallow water
                break;
            case TRAVELCOST.KNEEDEEP:
                sAPCost += AP.MOVEMENT_LAKE; // wading waist/chest deep - very slow
                break;

            case TRAVELCOST.DEEPWATER:
                sAPCost += AP.MOVEMENT_OCEAN; // can swim, so it's faster than wading
                break;
            /*
               case TRAVELCOST.VEINEND	:
               case TRAVELCOST.VEINMID	: sAPCost += AP.MOVEMENT_FLAT;
                                                                        break;
            */
            case TRAVELCOST.DOOR:
                sAPCost += AP.MOVEMENT_FLAT;
                break;

            // cost for jumping a fence REPLACES all other AP costs!
            case TRAVELCOST.FENCE:
                return (AP.JUMPFENCE);

            case TRAVELCOST.NONE:
                return (0);

            default:
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Calc AP: Unrecongnized MP type %d in %d, direction %d", sSwitchValue, sGridno, bDir));
                break;


        }

        if (bDir & 1)
        {
            sAPCost = (sAPCost * 14) / 10;
        }


        return (sAPCost);
    }

    public static bool EnoughAmmo(SOLDIERTYPE? pSoldier, bool fDisplay, InventorySlot bInvPos)
    {
        if (pSoldier.inv[bInvPos].usItem != Globals.NOTHING)
        {
            if (pSoldier.bWeaponMode == WM.ATTACHED)
            {
                return (true);
            }
            else
            {
                if (pSoldier.inv[bInvPos].usItem == Items.ROCKET_LAUNCHER)
                {
                    // hack... they turn empty afterwards anyways
                    return (true);
                }

                if (Globals.Item[pSoldier.inv[bInvPos].usItem].usItemClass == IC.LAUNCHER || pSoldier.inv[bInvPos].usItem == Items.TANK_CANNON)
                {
                    if (ItemSubSystem.FindAttachmentByClass((pSoldier.inv[bInvPos]), IC.GRENADE) != Globals.ITEM_NOT_FOUND)
                    {
                        return (true);
                    }

                    // ATE: Did an else if here...
                    if (ItemSubSystem.FindAttachmentByClass((pSoldier.inv[bInvPos]), IC.BOMB) != Globals.ITEM_NOT_FOUND)
                    {
                        return (true);
                    }

                    if (fDisplay)
                    {
                        DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.OUT_OF_AMMO);
                    }

                    return (false);
                }
                else if (Globals.Item[pSoldier.inv[bInvPos].usItem].usItemClass == IC.GUN)
                {
                    if (pSoldier.inv[bInvPos].ubGunShotsLeft == 0)
                    {
                        if (fDisplay)
                        {
                            DialogControl.TacticalCharacterDialogue(pSoldier, QUOTE.OUT_OF_AMMO);
                        }
                        return (false);
                    }
                }
            }

            return (true);
        }

        return (false);

    }


    public static void DeductAmmo(SOLDIERTYPE? pSoldier, InventorySlot bInvPos)
    {
        OBJECTTYPE? pObj;

        // tanks never run out of MG ammo!
        // unlimited cannon ammo is handled in AI
        if (TANK(pSoldier) && pSoldier.inv[bInvPos].usItem != Items.TANK_CANNON)
        {
            return;
        }

        pObj = (pSoldier.inv[bInvPos]);
        if (pObj.usItem != Globals.NOTHING)
        {
            if (pObj.usItem == Items.TANK_CANNON)
            {
            }
            else if (Globals.Item[pObj.usItem].usItemClass == IC.GUN && pObj.usItem != Items.TANK_CANNON)
            {
                if (pSoldier.usAttackingWeapon == pObj.usItem)
                {
                    // OK, let's see, don't overrun...
                    if (pObj.ubGunShotsLeft != 0)
                    {
                        pObj.ubGunShotsLeft--;
                    }
                }
                else
                {
                    // firing an attachment?
                }
            }
            else if (Globals.Item[pObj.usItem].usItemClass == IC.LAUNCHER || pObj.usItem == Items.TANK_CANNON)
            {
                Items? bAttachPos;

                bAttachPos = ItemSubSystem.FindAttachmentByClass(pObj, IC.GRENADE);
                if (bAttachPos is null)
                {
                    bAttachPos = ItemSubSystem.FindAttachmentByClass(pObj, IC.BOMB);
                }

                if (bAttachPos is not null)
                {
                    ItemSubSystem.RemoveAttachment(pObj, bAttachPos, null);
                }
            }

            // Dirty Bars
            DirtyMercPanelInterface(pSoldier, Globals.DIRTYLEVEL1);
        }
    }

    public int CalcTotalAPsToAttack(SOLDIERTYPE? pSoldier, int sGridNo, int ubAddTurningCost, int bAimTime)
    {
        int sAPCost = 0;
        Items usItemNum;
        int sActionGridNo;
        int ubDirection;
        int sAdjustedGridNo;
        IC uiItemClass;

        // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
        usItemNum = pSoldier.inv[InventorySlot.HANDPOS].usItem;
        uiItemClass = Globals.Item[usItemNum].usItemClass;

        if (uiItemClass == IC.GUN || uiItemClass == IC.LAUNCHER || uiItemClass == IC.TENTACLES || uiItemClass == IC.THROWING_KNIFE)
        {
            sAPCost = MinAPsToAttack(pSoldier, sGridNo, ubAddTurningCost);

            if (pSoldier.bDoBurst)
            {
                sAPCost += CalcAPsToBurst(CalcActionPoints(pSoldier), (pSoldier.inv[InventorySlot.HANDPOS]));
            }
            else
            {
                sAPCost += bAimTime;
            }
        }

        //ATE: HERE, need to calculate APs!
        if (uiItemClass.HasFlag(IC.EXPLOSV))
        {
            sAPCost = MinAPsToAttack(pSoldier, sGridNo, ubAddTurningCost);

            sAPCost = 5;
        }

        if (uiItemClass == IC.PUNCH || (uiItemClass == IC.BLADE && uiItemClass != IC.THROWING_KNIFE))
        {
            // IF we are at this gridno, calc min APs but if not, calc cost to goto this lication
            if (pSoldier.sGridNo != sGridNo)
            {
                // OK, in order to avoid path calculations here all the time... save and check if it's changed!
                if (pSoldier.sWalkToAttackGridNo == sGridNo)
                {
                    sAdjustedGridNo = sGridNo;
                    sAPCost += (pSoldier.sWalkToAttackWalkToCost);
                }
                else
                {
                    //int		cnt;
                    //int		sSpot;	
                    int ubGuyThere;
                    int sGotLocation = Globals.NOWHERE;
                    bool fGotAdjacent = false;
                    SOLDIERTYPE? pTarget;

                    ubGuyThere = WorldManager.WhoIsThere2(sGridNo, pSoldier.bLevel);

                    if (ubGuyThere != Globals.NOBODY)
                    {

                        pTarget = Globals.MercPtrs[ubGuyThere];

                        if (pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
                        {
                            sGotLocation = FindNextToAdjacentGridEx(pSoldier, sGridNo, out ubDirection, out sAdjustedGridNo, true, false);
                            if (sGotLocation == -1)
                            {
                                sGotLocation = Globals.NOWHERE;
                            }
                        }
                        else
                        {
                            sGotLocation = FindAdjacentPunchTarget(pSoldier, pTarget, out sAdjustedGridNo, out ubDirection);
                        }
                    }

                    if (sGotLocation == Globals.NOWHERE && pSoldier.ubBodyType != SoldierBodyTypes.BLOODCAT)
                    {
                        sActionGridNo = FindAdjacentGridEx(pSoldier, sGridNo, out ubDirection, out sAdjustedGridNo, true, false);

                        if (sActionGridNo == -1)
                        {
                            sGotLocation = Globals.NOWHERE;
                        }
                        else
                        {
                            sGotLocation = sActionGridNo;
                        }
                        fGotAdjacent = true;
                    }

                    if (sGotLocation != Globals.NOWHERE)
                    {
                        if (pSoldier.sGridNo == sGotLocation || !fGotAdjacent)
                        {
                            pSoldier.sWalkToAttackWalkToCost = 0;
                        }
                        else
                        {
                            // Save for next time...
                            pSoldier.sWalkToAttackWalkToCost = PathAI.PlotPath(
                                pSoldier, 
                                sGotLocation, 
                                PlotPathDefines.NO_COPYROUTE, 
                                Globals.NO_PLOT,
                                PlotPathDefines.TEMPORARY, 
                                pSoldier.usUIMovementMode,
                                PlotPathDefines.NOT_STEALTH,
                                PlotPathDefines.FORWARD, 
                                pSoldier.bActionPoints);

                            if (pSoldier.sWalkToAttackWalkToCost == 0)
                            {
                                return (99);
                            }
                        }
                    }
                    else
                    {
                        return (0);
                    }
                    sAPCost += pSoldier.sWalkToAttackWalkToCost;
                }

                // Save old location!
                pSoldier.sWalkToAttackGridNo = sGridNo;

                // Add points to attack
                sAPCost += MinAPsToAttack(pSoldier, sAdjustedGridNo, ubAddTurningCost);
            }
            else
            {
                // Add points to attack
                // Use our gridno
                sAPCost += MinAPsToAttack(pSoldier, sGridNo, ubAddTurningCost);
            }

            // Add aim time...
            sAPCost += bAimTime;

        }

        return sAPCost;
    }

    public int MinAPsToAttack(SOLDIERTYPE? pSoldier, int sGridno, int ubAddTurningCost)
    {
        int sAPCost = 0;
        IC uiItemClass;

        if (pSoldier.bWeaponMode == WM.ATTACHED)
        {
            InventorySlot bAttachSlot;
            // look for an attached grenade launcher

            bAttachSlot = FindAttachment((pSoldier.inv[InventorySlot.HANDPOS]), UNDER_GLAUNCHER);
            if (bAttachSlot == NO_SLOT)
            {
                // default to hand
                // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
                uiItemClass = Globals.Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass;
            }
            else
            {
                uiItemClass = Globals.Item[Items.UNDER_GLAUNCHER].usItemClass;
            }
        }
        else
        {
            // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
            uiItemClass = Globals.Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass;
        }

        if (uiItemClass == IC.BLADE || uiItemClass == IC.GUN || uiItemClass == IC.LAUNCHER || uiItemClass == IC.TENTACLES || uiItemClass == IC.THROWING_KNIFE)
        {
            sAPCost = MinAPsToShootOrStab(pSoldier, sGridno, ubAddTurningCost);
        }
        else if (uiItemClass.HasFlag(IC.GRENADE | IC.THROWN))
        {
            sAPCost = MinAPsToThrow(pSoldier, sGridno, ubAddTurningCost);
        }
        else if (uiItemClass == IC.PUNCH)
        {
            sAPCost = MinAPsToPunch(pSoldier, sGridno, ubAddTurningCost);
        }

        return (sAPCost);
    }
}

public class AP
{
    public const int MINIMUM = 10;      // no merc can have less for his turn
    public const int MAXIMUM = 25;      // no merc can have more for his turn
    public const int MONSTER_MAXIMUM = 40;      // no monster can have more for his turn
    public const int VEHICLE_MAXIMUM = 50;      // no merc can have more for his turn
    public const int INCREASE = 10;      // optional across-the-board AP boost
    public const int MAX_AP_CARRIED = 5;      // APs carried from turn-to-turn
                                              // monster AP bonuses; expressed in 10ths (12 = 120% normal) 
    public const int YOUNG_MONST_FACTOR = 15;
    public const int ADULT_MONST_FACTOR = 12;
    public const int MONST_FRENZY_FACTOR = 13;
    // AP penalty for a phobia situation (again; in 10ths)
    public const int CLAUSTROPHOBE = 9;
    public const int AFRAID_OF_INSECTS = 8;
    public const int EXCHANGE_PLACES = 5;
    // Action Point values
    public const int REVERSE_MODIFIER = 1;
    public const int STEALTH_MODIFIER = 2;
    public const int STEAL_ITEM = 10;         // APs to steal item....
    public const int TAKE_BLOOD = 10;
    public const int TALK = 6;
    public const int MOVEMENT_FLAT = 3;               // div by 2 for run; +2; for crawl; -1 for swat
    public const int MOVEMENT_GRASS = 4;
    public const int MOVEMENT_BUSH = 5;
    public const int MOVEMENT_RUBBLE = 6;
    public const int MOVEMENT_SHORE = 7;   // shallow wade
    public const int MOVEMENT_LAKE = 9;   // deep wade . slowest
    public const int MOVEMENT_OCEAN = 8;   // swimming is faster than deep wade
    public const int CHANGE_FACING = 1;   // turning to face any other direction
    public const int CHANGE_TARGET = 1;   // aiming at a new target
    public const int CATCH_ITEM = 5;              // turn to catch item
    public const int TOSS_ITEM = 8;           // toss item from inv
    public const int REFUEL_VEHICLE = 10;
    /*
    MOVE_ITEM_FREE       0       // same place; pocket.pocket
    MOVE_ITEM_FAST       2       // hand; holster; ground only
    MOVE_ITEM_AVG        4       // everything else!
    MOVE_ITEM_SLOW       6       // vests; protective gear
    */
    public const int MOVE_ITEM_FAST = 4;     // hand; holster; ground only
    public const int MOVE_ITEM_SLOW = 6; // vests; protective gear
    public const int RADIO = 5;
    public const int CROUCH = 2;
    public const int PRONE = 2;
    public const int LOOK_STANDING = 1;
    public const int LOOK_CROUCHED = 2;
    public const int LOOK_PRONE = 2;
    public const int READY_KNIFE = 0;
    public const int READY_PISTOL = 1;
    public const int READY_RIFLE = 2;
    public const int READY_SAW = 0;
    // JA2Gold: reduced dual AP cost from 3 to 1
    //READY_DUAL           3
    public const int READY_DUAL = 1;
    public const int MIN_AIM_ATTACK = 0;       // minimum permitted extra aiming
    public const int MAX_AIM_ATTACK = 4;       // maximum permitted extra aiming
    public const int BURST = 5;
    public const int DROP_BOMB = 3;
    public const int RELOAD_GUN = 5;// loading new clip/magazine
    public const int START_FIRST_AID = 5;// get the stuff out of medic kit
    public const int PER_HP_FIRST_AID = 1;// for each point healed
    public const int STOP_FIRST_AID = 3;// put everything away again
    public const int START_REPAIR = 5;    // get the stuff out of repair kit
    public const int GET_HIT = 2;      // struck by bullet; knife; explosion
    public const int GET_WOUNDED_DIVISOR = 4;  // 1 AP lost for every 'divisor' dmg
    public const int FALL_DOWN = 4;  // falling down (explosion; exhaustion)
    public const int GET_THROWN = 2;  // get thrown back (by explosion)
    public const int GET_UP = 5;  // getting up again
    public const int ROLL_OVER = 2;  // flipping from back to stomach
    public const int OPEN_DOOR = 3;  // whether successful; or not (locked)
    public const int PICKLOCK = 10;     // should really be several turns
    public const int EXAMINE_DOOR = 5;    // time to examine door
    public const int BOOT_DOOR = 8;         // time to boot door
    public const int USE_CROWBAR = 10;      // time to crowbar door
    public const int UNLOCK_DOOR = 6;       // time to unlock door
    public const int LOCK_DOOR = 6;             // time to lock door
    public const int EXPLODE_DOOR = 10;       // time to set explode charge on door
    public const int UNTRAP_DOOR = 10;        // time to untrap door
    public const int USEWIRECUTTERS = 10;     // Time to use wirecutters
    public const int CLIMBROOF = 10;          // APs to climb roof
    public const int CLIMBOFFROOF = 6;        // APs to climb off roof
    public const int JUMPFENCE = 6;           // time to jump over a fence
    public const int OPEN_SAFE = 8;       // time to use combination
    public const int USE_REMOTE = 2;
    public const int PULL_TRIGGER = 2;       // operate nearby panic trigger
    public const int FORCE_LID_OPEN = 10;
    public const int SEARCH_CONTAINER = 5;       // boxes; crates; safe; etc.
    public const int READ_NOTE = 10;   // reading a note's contents in inv.
    public const int SNAKE_BATTLE = 10;   // when first attacked
    public const int KILL_SNAKE = 7;   // when snake battle's been won
    public const int USE_SURV_CAM = 5;
    public const int PICKUP_ITEM = 3;
    public const int GIVE_ITEM = 1;
    public const int BURY_MINE = 10;
    public const int DISARM_MINE = 10;
    public const int DRINK = 5;
    public const int CAMOFLAGE = 10;
    public const int TAKE_PHOTOGRAPH = 5;
    public const int MERGE = 8;
    public const int OTHER_COST = 99;
    public const int START_RUN_COST = 1;
    public const int ATTACH_CAN = 5;
    public const int JUMP_OVER = 6;
}

public class BP
{
    // special Breath Point related constants
    public const int RATIO_RED_PTS_TO_NORMAL = 100;
    public const int RUN_ENERGYCOSTFACTOR = 3;// Richard thinks running is 3rd most strenous over time... tough; Mark didn't.  CJC increased it again
    public const int WALK_ENERGYCOSTFACTOR = 1;// walking subtracts flat terrain breath value
    public const int SWAT_ENERGYCOSTFACTOR = 2;// Richard thinks swatmove is 2nd most strenous over time... tough; Mark didn't
    public const int CRAWL_ENERGYCOSTFACTOR = 4;// Richard thinks crawling is the MOST strenuous over time	
    public const int RADIO = 0;// no breath cost
    public const int USE_DETONATOR = 0; // no breath cost
    public const int REVERSE_MODIFIER = 0;  // no change; a bit more challenging
    public const int STEALTH_MODIFIER = -20;   // slow & cautious; not too strenuous
    public const int MINING_MODIFIER = -30; // pretty relaxing; overall
                           // end-of-turn Breath Point gain/usage rates
    public const int PER_AP_NO_EFFORT = -200;   // gain breath!
    public const int PER_AP_MIN_EFFORT = -100;    // gain breath!
    public const int PER_AP_LT_EFFORT = -50;     // gain breath!
    public const int PER_AP_MOD_EFFORT = 25;
    public const int PER_AP_HVY_EFFORT = 50;
    public const int PER_AP_MAX_EFFORT = 100;
    // Breath Point values
    public const int MOVEMENT_FLAT = 5;
    public const int MOVEMENT_GRASS = 10;
    public const int MOVEMENT_BUSH = 20;
    public const int MOVEMENT_RUBBLE = 35;
    public const int MOVEMENT_SHORE = 50;      // shallow wade
    public const int MOVEMENT_LAKE = 75;      // deep wade
    public const int MOVEMENT_OCEAN = 100;     // swimming
    public const int CHANGE_FACING = 10;    // turning to face another direction
    public const int CROUCH = 10;
    public const int PRONE = 10;
    public const int CLIMBROOF = 500;     // BPs to climb roof
    public const int CLIMBOFFROOF = 250;  // BPs to climb off roof
    public const int JUMPFENCE = 200;     // BPs to jump fence
    /*
    MOVE_ITEM_FREE       0       // same place; pocket.pocket
    MOVE_ITEM_FAST       0       // hand; holster; ground only
    MOVE_ITEM_AVG        0       // everything else!
    MOVE_ITEM_SLOW       20      // vests; protective gear
    */
    public const int MOVE_ITEM_FAST = 0;// hand; holster; ground only
    public const int MOVE_ITEM_SLOW = 20;// vests; protective gear
    public const int READY_KNIFE = 0;// raise/lower knife
    public const int READY_PISTOL = 10;// raise/lower pistol
    public const int READY_RIFLE = 20;// raise/lower rifle
    public const int READY_SAW = 0;// raise/lower saw
    public const int STEAL_ITEM = 50;// BPs steal item
    public const int PER_AP_AIMING = 5;// breath cost while aiming
    public const int RELOAD_GUN = 20;// loading new clip/magazine
    public const int THROW_ITEM = 50;// throw grenades; fire-bombs; etc.
    public const int START_FIRST_AID = 0;// get the stuff out of medic kit
    public const int PER_HP_FIRST_AID = -25;// gain breath for each point healed
    public const int STOP_FIRST_AID = 0;// put everything away again
    public const int GET_HIT = 200;// struck by bullet; knife; explosion
    public const int GET_WOUNDED = 50;// per pt of GUNFIRE/EXPLOSION impact
    public const int FALL_DOWN = 250;// falling down (explosion; exhaustion)
    public const int GET_UP = 50;// getting up again
    public const int ROLL_OVER = 20;// flipping from back to stomach
    public const int OPEN_DOOR = 30;// whether successful; or not (locked)
    public const int PICKLOCK = -250;// gain breath; not very tiring...
    public const int EXAMINE_DOOR = -250;// gain breath; not very tiring...
    public const int BOOT_DOOR = 200;     // BP to boot door
    public const int USE_CROWBAR = 350;     // BP to crowbar door
    public const int UNLOCK_DOOR = 50;    // BP to unlock door
    public const int EXPLODE_DOOR = -250;     // BP to set explode charge on door
    public const int UNTRAP_DOOR = 150;       // BP to untrap
    public const int LOCK_DOOR = 50;          // BP to untrap
    public const int USEWIRECUTTERS = 200;    // BP to use wirecutters
    public const int PULL_TRIGGER = 0; // for operating panic triggers
    public const int FORCE_LID_OPEN = 50; // per point of strength required
    public const int SEARCH_CONTAINER = 0;// get some breath back (was -50)
    public const int OPEN_SAFE = -50;
    public const int READ_NOTE = -250;    // reading a note's contents in inv.
    public const int SNAKE_BATTLE = 500;   // when first attacked
    public const int KILL_SNAKE = 350;  // when snake battle's been won
    public const int USE_SURV_CAM = -100;
    public const int BURY_MINE = 250;   // involves digging & filling again
    public const int DISARM_MINE = 0;  // 1/2 digging; 1/2 light effort
    public const int FIRE_HANDGUN = 25; // preatty easy; little recoil
    public const int FIRE_RIFLE = 50;// heavier; nasty recoil
    public const int FIRE_SHOTGUN = 100; // quite tiring; must be pumped up
    public const int STAB_KNIFE = 200;
    public const int TAKE_PHOTOGRAPH = 0;
    public const int MERGE = 50;
    public const int FALLFROMROOF = 1000;
    public const int JUMP_OVER = 250;
}
