﻿namespace SharpAlliance.Core.SubSystems;

public class Points
{
    public int CalcTotalAPsToAttack(SOLDIERTYPE? pSoldier, int sGridNo, int ubAddTurningCost, int bAimTime)
    {
        int sAPCost = 0;
        int usItemNum;
        int sActionGridNo;
        int ubDirection;
        int sAdjustedGridNo;
        IC uiItemClass;

        // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
        usItemNum = pSoldier.inv[(int)InventorySlot.HANDPOS].usItem;
        uiItemClass = Item[usItemNum].usItemClass;

        if (uiItemClass == IC.GUN || uiItemClass == IC.LAUNCHER || uiItemClass == IC.TENTACLES || uiItemClass == IC.THROWING_KNIFE)
        {
            sAPCost = MinAPsToAttack(pSoldier, sGridNo, ubAddTurningCost);

            if (pSoldier.bDoBurst)
            {
                sAPCost += CalcAPsToBurst(CalcActionPoints(pSoldier), &(pSoldier.inv[(int)InventorySlot.HANDPOS]));
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
                    //INT32		cnt;
                    //INT16		sSpot;	
                    int ubGuyThere;
                    int sGotLocation = IsometricUtils.NOWHERE;
                    bool fGotAdjacent = false;
                    SOLDIERTYPE? pTarget;

                    ubGuyThere = WhoIsThere2(sGridNo, pSoldier.bLevel);

                    if (ubGuyThere != OverheadTypes.NOBODY)
                    {

                        pTarget = MercPtrs[ubGuyThere];

                        if (pSoldier.ubBodyType == BLOODCAT)
                        {
                            sGotLocation = FindNextToAdjacentGridEx(pSoldier, sGridNo, ubDirection, sAdjustedGridNo, true, false);
                            if (sGotLocation == -1)
                            {
                                sGotLocation = IsometricUtils.NOWHERE;
                            }
                        }
                        else
                        {
                            sGotLocation = FindAdjacentPunchTarget(pSoldier, pTarget, sAdjustedGridNo, ubDirection);
                        }
                    }

                    if (sGotLocation == IsometricUtils.NOWHERE && pSoldier.ubBodyType != BLOODCAT)
                    {
                        sActionGridNo = FindAdjacentGridEx(pSoldier, sGridNo, ubDirection, sAdjustedGridNo, true, false);

                        if (sActionGridNo == -1)
                        {
                            sGotLocation = IsometricUtils.NOWHERE;
                        }
                        else
                        {
                            sGotLocation = sActionGridNo;
                        }
                        fGotAdjacent = true;
                    }

                    if (sGotLocation != IsometricUtils.NOWHERE)
                    {
                        if (pSoldier.sGridNo == sGotLocation || !fGotAdjacent)
                        {
                            pSoldier.sWalkToAttackWalkToCost = 0;
                        }
                        else
                        {
                            // Save for next time...
                            pSoldier.sWalkToAttackWalkToCost = PlotPath(pSoldier, sGotLocation, NO_COPYROUTE, NO.PLOT, TEMPORARY, pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

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
            int bAttachSlot;
            // look for an attached grenade launcher

            bAttachSlot = FindAttachment((pSoldier.inv[(int)InventorySlot.HANDPOS]), UNDER_GLAUNCHER);
            if (bAttachSlot == NO_SLOT)
            {
                // default to hand
                // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
                uiItemClass = Item[pSoldier.inv[(int)InventorySlot.HANDPOS].usItem].usItemClass;
            }
            else
            {
                uiItemClass = Item[UNDER_GLAUNCHER].usItemClass;
            }
        }
        else
        {
            // LOOK IN BUDDY'S HAND TO DETERMINE WHAT TO DO HERE
            uiItemClass = Item[pSoldier.inv[(int)InventorySlot.HANDPOS].usItem].usItemClass;
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

public enum BP
{
    // special Breath Point related constants
    RATIO_RED_PTS_TO_NORMAL = 100,
    RUN_ENERGYCOSTFACTOR = 3,// Richard thinks running is 3rd most strenous over time... tough, Mark didn't.  CJC increased it again
    WALK_ENERGYCOSTFACTOR = 1,// walking subtracts flat terrain breath value
    SWAT_ENERGYCOSTFACTOR = 2,// Richard thinks swatmove is 2nd most strenous over time... tough, Mark didn't
    CRAWL_ENERGYCOSTFACTOR = 4,// Richard thinks crawling is the MOST strenuous over time	
    RADIO = 0,// no breath cost
    USE_DETONATOR = 0, // no breath cost
    REVERSE_MODIFIER = 0,  // no change, a bit more challenging
    STEALTH_MODIFIER = -20,   // slow & cautious, not too strenuous
    MINING_MODIFIER = -30, // pretty relaxing, overall
                           // end-of-turn Breath Point gain/usage rates
    PER_AP_NO_EFFORT = -200,   // gain breath!
    PER_AP_MIN_EFFORT = -100,    // gain breath!
    PER_AP_LT_EFFORT = -50,     // gain breath!
    PER_AP_MOD_EFFORT = 25,
    PER_AP_HVY_EFFORT = 50,
    PER_AP_MAX_EFFORT = 100,
    // Breath Point values
    MOVEMENT_FLAT = 5,
    MOVEMENT_GRASS = 10,
    MOVEMENT_BUSH = 20,
    MOVEMENT_RUBBLE = 35,
    MOVEMENT_SHORE = 50,      // shallow wade
    MOVEMENT_LAKE = 75,      // deep wade
    MOVEMENT_OCEAN = 100,     // swimming
    CHANGE_FACING = 10,    // turning to face another direction
    CROUCH = 10,
    PRONE = 10,
    CLIMBROOF = 500,     // BPs to climb roof
    CLIMBOFFROOF = 250,  // BPs to climb off roof
    JUMPFENCE = 200,     // BPs to jump fence
    /*
    MOVE_ITEM_FREE       0       // same place, pocket.pocket
    MOVE_ITEM_FAST       0       // hand, holster, ground only
    MOVE_ITEM_AVG        0       // everything else!
    MOVE_ITEM_SLOW       20      // vests, protective gear
    */
    MOVE_ITEM_FAST = 0,// hand, holster, ground only
    MOVE_ITEM_SLOW = 20,// vests, protective gear
    READY_KNIFE = 0,// raise/lower knife
    READY_PISTOL = 10,// raise/lower pistol
    READY_RIFLE = 20,// raise/lower rifle
    READY_SAW = 0,// raise/lower saw
    STEAL_ITEM = 50,// BPs steal item
    PER_AP_AIMING = 5,// breath cost while aiming
    RELOAD_GUN = 20,// loading new clip/magazine
    THROW_ITEM = 50,// throw grenades, fire-bombs, etc.
    START_FIRST_AID = 0,// get the stuff out of medic kit
    PER_HP_FIRST_AID = -25,// gain breath for each point healed
    STOP_FIRST_AID = 0,// put everything away again
    GET_HIT = 200,// struck by bullet, knife, explosion
    GET_WOUNDED = 50,// per pt of GUNFIRE/EXPLOSION impact
    FALL_DOWN = 250,// falling down (explosion, exhaustion)
    GET_UP = 50,// getting up again
    ROLL_OVER = 20,// flipping from back to stomach
    OPEN_DOOR = 30,// whether successful, or not (locked)
    PICKLOCK = -250,// gain breath, not very tiring...
    EXAMINE_DOOR = -250,// gain breath, not very tiring...
    BOOT_DOOR = 200,     // BP to boot door
    USE_CROWBAR = 350,     // BP to crowbar door
    UNLOCK_DOOR = 50,    // BP to unlock door
    EXPLODE_DOOR = -250,     // BP to set explode charge on door
    UNTRAP_DOOR = 150,       // BP to untrap
    LOCK_DOOR = 50,          // BP to untrap
    USEWIRECUTTERS = 200,    // BP to use wirecutters
    PULL_TRIGGER = 0, // for operating panic triggers
    FORCE_LID_OPEN = 50, // per point of strength required
    SEARCH_CONTAINER = 0,// get some breath back (was -50)
    OPEN_SAFE = -50,
    READ_NOTE = -250,    // reading a note's contents in inv.
    SNAKE_BATTLE = 500,   // when first attacked
    KILL_SNAKE = 350,  // when snake battle's been won
    USE_SURV_CAM = -100,
    BURY_MINE = 250,   // involves digging & filling again
    DISARM_MINE = 0,  // 1/2 digging, 1/2 light effort
    FIRE_HANDGUN = 25, // preatty easy, little recoil
    FIRE_RIFLE = 50,// heavier, nasty recoil
    FIRE_SHOTGUN = 100, // quite tiring, must be pumped up
    STAB_KNIFE = 200,
    TAKE_PHOTOGRAPH = 0,
    MERGE = 50,
    FALLFROMROOF = 1000,
    JUMP_OVER = 250,
}