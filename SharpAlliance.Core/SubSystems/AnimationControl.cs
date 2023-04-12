using System;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class AnimationControl
{
    public const int MAX_ANIMATIONS = 320;
    public const int MAX_FRAMES_PER_ANIM = 100;
    public const int MAX_RANDOM_ANIMS_PER_BODYTYPE = 7;

    public static AnimationSurfaceTypes LoadSoldierAnimationSurface(SOLDIERTYPE pSoldier, AnimationStates usAnimState)
    {
        AnimationSurfaceTypes usAnimSurface;

        usAnimSurface = DetermineSoldierAnimationSurface(pSoldier, usAnimState);

        if (usAnimSurface != INVALID_ANIMATION_SURFACE)
        {
            // Ensure that it's been loaded!
//            if (GetCachedAnimationSurface(pSoldier.ubID, (pSoldier.AnimCache), usAnimSurface, pSoldier.usAnimState) == false)
            {
                usAnimSurface = INVALID_ANIMATION_SURFACE;
            }

        }

        return (usAnimSurface);
    }

    public static bool SetSoldierAnimationSurface(SOLDIERTYPE pSoldier, AnimationStates usAnimState)
    {
        AnimationSurfaceTypes usAnimSurface;

        // Delete any structure info!
        if (pSoldier.pLevelNode != null)
        {
            WorldStructures.DeleteStructureFromWorld(pSoldier.pLevelNode.pStructureData);
            pSoldier.pLevelNode.pStructureData = null;
        }


        usAnimSurface = LoadSoldierAnimationSurface(pSoldier, usAnimState);

        // Add structure info!
        if (pSoldier.pLevelNode != null && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.PAUSEANIMOVE)))
        {
//            AddMercStructureInfoFromAnimSurface(pSoldier.sGridNo, pSoldier, usAnimSurface, usAnimState);
        }

        // Set
        pSoldier.usAnimSurface = usAnimSurface;

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            return (false);
        }

        return (true);
    }

    public static AnimationSurfaceTypes DetermineSoldierAnimationSurface(SOLDIERTYPE pSoldier, AnimationStates usAnimState)
    {
        AnimationSurfaceTypes usAnimSurface = 0;
        AnimationSurfaceTypes usAltAnimSurface = 0;
        SoldierBodyTypes ubBodyType;
        Items usItem;
        int ubWaterHandIndex = 1;
        int cnt;
        bool fAdjustedForItem = false;
        AnimationStates usNewAnimState = 0;

        ubBodyType = pSoldier.ubBodyType;

//        if (SubstituteBodyTypeAnimation(pSoldier, usAnimState, out usNewAnimState))
        {
            usAnimState = usNewAnimState;
        }

//        usAnimSurface = gubAnimSurfaceIndex[pSoldier.ubBodyType][usAnimState];

        // CHECK IF WE CAN DO THIS ANIMATION, IE WE HAVE IT AVAILIBLE
        if (usAnimSurface == AnimationSurfaceTypes.INVALID_ANIMATION)
        {
            // WE SHOULD NOT BE USING THIS ANIMATION
//            Messages.ScreenMsg(FontColor.FONT_MCOLOR_RED, MSG.BETAVERSION, "Invalid Animation File for Body %d, animation %S.", pSoldier.ubBodyType, gAnimControl[usAnimState].zAnimStr);
            // Set index to FOUND_INVALID_ANIMATION
//            gubAnimSurfaceIndex[pSoldier.ubBodyType][usAnimState] = AnimationStates.FOUND_INVALID_ANIMATION;
            return (AnimationSurfaceTypes.INVALID_ANIMATION_SURFACE);
        }

        
        if (usAnimSurface == AnimationSurfaceTypes.FOUND_INVALID_ANIMATION)
        {
            return (AnimationSurfaceTypes.INVALID_ANIMATION_SURFACE);
        }


        // OK - DO SOME MAGIC HERE TO SWITCH BODY TYPES IF WE WANT!


        // If we are a queen, pick the 'real' anim surface....
        if (usAnimSurface == AnimationSurfaceTypes.QUEENMONSTERSPIT_SW)
        {
            WorldDirections bDir;

            // Assume a target gridno is here.... get direction...
            // ATE: use +2 in gridno because here head is far from body
            bDir = SoldierControl.GetDirectionToGridNoFromGridNo((pSoldier.sGridNo + 2), pSoldier.sTargetGridNo);

            return 0;//(gusQueenMonsterSpitAnimPerDir[bDir]);
        }


        // IF we are not a merc, return
        if (pSoldier.ubBodyType > SoldierBodyTypes.REGFEMALE)
        {
            return (usAnimSurface);
        }

        // SWITCH TO DIFFERENT AIM ANIMATION FOR BIG GUY!
        if (usAnimSurface == AnimationSurfaceTypes.BGMSTANDAIM2)
        {
            if (pSoldier.uiAnimSubFlags.HasFlag(SUB_ANIM.BIGGUYSHOOT2))
            {
                usAnimSurface = AnimationSurfaceTypes.BGMSTANDAIM;
            }
        }

        // SWITCH TO DIFFERENT STAND ANIMATION FOR BIG GUY!
        if (usAnimSurface == AnimationSurfaceTypes.BGMSTANDING)
        {
            if (pSoldier.uiAnimSubFlags.HasFlag(SUB_ANIM.BIGGUYTHREATENSTANCE))
            {
                usAnimSurface = AnimationSurfaceTypes.BGMTHREATENSTAND;
            }
        }

        if (usAnimSurface == AnimationSurfaceTypes.BGMWALKING)
        {
            if (pSoldier.uiAnimSubFlags.HasFlag(SUB_ANIM.BIGGUYTHREATENSTANCE))
            {
                usAnimSurface = AnimationSurfaceTypes.BGMWALK2;
            }
        }

        if (usAnimSurface == AnimationSurfaceTypes.BGMRUNNING)
        {
            if (pSoldier.uiAnimSubFlags.HasFlag(SUB_ANIM.BIGGUYTHREATENSTANCE))
            {
                usAnimSurface = AnimationSurfaceTypes.BGMRUN2;
            }
        }

        if (usAnimSurface == AnimationSurfaceTypes.BGMRAISE)
        {
            if (pSoldier.uiAnimSubFlags.HasFlag(SUB_ANIM.BIGGUYTHREATENSTANCE))
            {
                usAnimSurface = AnimationSurfaceTypes.BGMRAISE2;
            }
        }


        // ADJUST ANIMATION SURFACE BASED ON TERRAIN

        // CHECK FOR WATER
        if (SoldierControl.MercInWater(pSoldier))
        {
            // ADJUST BASED ON ITEM IN HAND....
            usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

            // Default it to the 1 ( ie: no rifle )
            if (usItem != NOTHING)
            {
                if ((Item[usItem].usItemClass == IC.GUN || Item[usItem].usItemClass == IC.LAUNCHER) && usItem != Items.ROCKET_LAUNCHER)
                {
                    if ((Item[usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)))
                    {
                        ubWaterHandIndex = 0;
                    }
                }
            }

            // CHANGE BASED ON HIEGHT OF WATER
//            usAltAnimSurface = gubAnimSurfaceMidWaterSubIndex[pSoldier.ubBodyType][usAnimState][ubWaterHandIndex];

            if (usAltAnimSurface != AnimationSurfaceTypes.INVALID_ANIMATION)
            {
                usAnimSurface = usAltAnimSurface;
            }

        }
        else
        {
            // ADJUST BASED ON ITEM IN HAND....
            usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

            if (!(Item[usItem].usItemClass == IC.GUN) && !(Item[usItem].usItemClass == IC.LAUNCHER) || usItem == Items.ROCKET_LAUNCHER)
            {
                if (usAnimState == AnimationStates.STANDING)
                {
//                    usAnimSurface = gusNothingBreath[pSoldier.ubBodyType];
                    fAdjustedForItem = true;
                }
                else
                {
//                    usAltAnimSurface = gubAnimSurfaceItemSubIndex[pSoldier.ubBodyType][usAnimState];

                    if (usAltAnimSurface != AnimationSurfaceTypes.INVALID_ANIMATION)
                    {
                        usAnimSurface = usAltAnimSurface;
                        fAdjustedForItem = true;
                    }
                }
            }
            else
            {
                // CHECK FOR HANDGUN
                if ((Item[usItem].usItemClass == IC.GUN || Item[usItem].usItemClass == IC.LAUNCHER) && usItem != Items.ROCKET_LAUNCHER)
                {
                    if (!(Item[usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)))
                    {
//                        usAltAnimSurface = gubAnimSurfaceItemSubIndex[pSoldier.ubBodyType][usAnimState];
//                        if (usAltAnimSurface != AnimationSurfaceTypes.INVALID_ANIMATION)
                        {
                            usAnimSurface = usAltAnimSurface;
                            fAdjustedForItem = true;
                        }

                        // Look for good two pistols sub anim.....
//                        if (gDoubleHandledSub.usAnimState == usAnimState)
                        {
                            // Do we carry two pistols...
                            if (Item[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem].usItemClass == IC.GUN)
                            {
//                                usAnimSurface = gDoubleHandledSub.usAnimationSurfaces[pSoldier.ubBodyType];
                                fAdjustedForItem = true;
                            }
                        }

                    }
                }
                else
                {
//                    usAltAnimSurface = gubAnimSurfaceItemSubIndex[pSoldier.ubBodyType][usAnimState];

                    if (usAltAnimSurface != AnimationSurfaceTypes.INVALID_ANIMATION)
                    {
                        usAnimSurface = usAltAnimSurface;
                        fAdjustedForItem = true;
                    }
                }
            }

            // Based on if we have adjusted for item or not... check for injured status...
            if (fAdjustedForItem)
            {
                // If life below thresthold for being injured 
                if (pSoldier.bLife < INJURED_CHANGE_THREASHOLD)
                {
                    // ADJUST FOR INJURED....
//                    for (cnt = 0; cnt < NUM_INJURED_SUBS; cnt++)
                    {
//                        if (gNothingInjuredSub[cnt].usAnimState == usAnimState)
                        {
//                            usAnimSurface = gNothingInjuredSub[cnt].usAnimationSurfaces[pSoldier.ubBodyType];
                        }
                    }
                }
            }
            else
            {
                // If life below thresthold for being injured 
                if (pSoldier.bLife < INJURED_CHANGE_THREASHOLD)
                {
                    // ADJUST FOR INJURED....
//                    for (cnt = 0; cnt < NUM_INJURED_SUBS; cnt++)
                    {
//                        if (gRifleInjuredSub[cnt].usAnimState == usAnimState)
                        {
//                            usAnimSurface = gRifleInjuredSub[cnt].usAnimationSurfaces[pSoldier.ubBodyType];
                        }
                    }
                }
            }
        }

        return (usAnimSurface);
    }

    public static AnimationSurfaceTypes GetSoldierAnimationSurface(SOLDIERTYPE? pSoldier, AnimationStates usAnimState)
    {
        AnimationSurfaceTypes usAnimSurface = pSoldier.usAnimSurface;

        if (usAnimSurface != Globals.INVALID_ANIMATION_SURFACE)
        {
            // Ensure that it's loaded!
            if (gAnimSurfaceDatabase[usAnimSurface].hVideoObject == null)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_RED, MSG.BETAVERSION, "IAnimation Surface for Body %d, animation %S, surface %d not loaded.", pSoldier.ubBodyType.ToString(),
                    Globals.gAnimControl[usAnimState].zAnimStr, usAnimSurface.ToString());
                //AnimDebugMsg("Surface Database: PROBLEMS!!!!!!");
                usAnimSurface = Globals.INVALID_ANIMATION_SURFACE;
            }
        }

        return (usAnimSurface);
    }
}

// Enumeration of animation states
public enum AnimationStates
{
    WALKING = 0,
    STANDING,
    KNEEL_DOWN,
    CROUCHING,
    KNEEL_UP,
    SWATTING,
    RUNNING,
    PRONE_DOWN,
    CRAWLING,
    PRONE_UP,
    PRONE,
    READY_RIFLE_STAND,
    AIM_RIFLE_STAND,
    SHOOT_RIFLE_STAND,
    END_RIFLE_STAND,
    START_SWAT,
    END_SWAT,
    FLYBACK_HIT,
    READY_RIFLE_PRONE,
    AIM_RIFLE_PRONE,
    SHOOT_RIFLE_PRONE,
    END_RIFLE_PRONE,
    FALLBACK_DEATHTWICH,
    GENERIC_HIT_STAND,
    FLYBACK_HIT_BLOOD_STAND,
    FLYBACK_HIT_DEATH,
    READY_RIFLE_CROUCH,
    AIM_RIFLE_CROUCH,
    SHOOT_RIFLE_CROUCH,
    END_RIFLE_CROUCH,
    FALLBACK_HIT_STAND,
    ROLLOVER,
    CLIMBUPROOF,
    FALLOFF,
    GETUP_FROM_ROLLOVER,
    CLIMBDOWNROOF,
    FALLFORWARD_ROOF,
    GENERIC_HIT_DEATHTWITCHNB,
    GENERIC_HIT_DEATHTWITCHB,
    FALLBACK_HIT_DEATHTWITCHNB,
    FALLBACK_HIT_DEATHTWITCHB,
    GENERIC_HIT_DEATH,
    FALLBACK_HIT_DEATH,
    GENERIC_HIT_CROUCH,
    STANDING_BURST,
    STANDING_BURST_HIT,
    FALLFORWARD_FROMHIT_STAND,
    FALLFORWARD_FROMHIT_CROUCH,
    ENDFALLFORWARD_FROMHIT_CROUCH,
    GENERIC_HIT_PRONE,
    PRONE_HIT_DEATH,
    PRONE_LAY_FROMHIT,
    PRONE_HIT_DEATHTWITCHNB,
    PRONE_HIT_DEATHTWITCHB,

    ADULTMONSTER_BREATHING,
    ADULTMONSTER_WALKING,
    ADULTMONSTER_ATTACKING,

    FLYBACK_HITDEATH_STOP,
    FALLFORWARD_HITDEATH_STOP,
    FALLBACK_HITDEATH_STOP,
    PRONE_HITDEATH_STOP,

    FLYBACKHIT_STOP,
    FALLBACKHIT_STOP,
    FALLOFF_STOP,
    FALLOFF_FORWARD_STOP,
    STAND_FALLFORWARD_STOP,
    PRONE_LAYFROMHIT_STOP,

    HOPFENCE,

    ADULTMONSTER_HIT,
    ADULTMONSTER_DYING,
    ADULTMONSTER_DYING_STOP,

    PUNCH_BREATH,
    PUNCH,
    NOTHING_STAND,

    JFK_HITDEATH,
    JFK_HITDEATH_STOP,
    JFK_HITDEATH_TWITCHB,

    FIRE_STAND_BURST_SPREAD,

    FALLOFF_DEATH,
    FALLOFF_DEATH_STOP,
    FALLOFF_TWITCHB,
    FALLOFF_TWITCHNB,

    FALLOFF_FORWARD_DEATH,
    FALLOFF_FORWARD_DEATH_STOP,
    FALLOFF_FORWARD_TWITCHB,
    FALLOFF_FORWARD_TWITCHNB,

    OPEN_DOOR,
    OPEN_STRUCT,

    PICKUP_ITEM,
    DROP_ITEM,

    SLICE,
    STAB,
    CROUCH_STAB,


    START_AID,
    GIVING_AID,
    END_AID,

    DODGE_ONE,

    FATCIV_ASS_SCRATCH,

    READY_DUAL_STAND,
    AIM_DUAL_STAND,
    SHOOT_DUAL_STAND,
    END_DUAL_STAND,

    RAISE_RIFLE,
    LOWER_RIFLE,

    BODYEXPLODING,
    THROW_ITEM,
    LOB_ITEM,
    QUEEN_MONSTER_BREATHING,
    CROUCHED_BURST,
    PRONE_BURST,
    NOTUSEDANIM1,
    BIGBUY_FLEX,
    BIGBUY_STRECH,
    BIGBUY_SHOEDUST,
    BIGBUY_HEADTURN,
    MINIGIRL_STOCKING,
    GIVE_ITEM,
    CLIMB_CLIFF,
    COW_EATING,
    COW_HIT,
    COW_DYING,
    COW_DYING_STOP,
    WATER_HIT,
    WATER_DIE,
    WATER_DIE_STOP,
    CROW_WALK,
    CROW_TAKEOFF,
    CROW_LAND,
    CROW_FLY,
    CROW_EAT,
    HELIDROP,
    FEM_CLEAN,
    FEM_KICKSN,
    FEM_LOOK,
    FEM_WIPE,
    REG_SQUISH,
    REG_PULL,
    REG_SPIT,
    HATKID_YOYO,
    KID_ARMPIT,
    MONSTER_CLOSE_ATTACK,
    MONSTER_SPIT_ATTACK,
    MONSTER_BEGIN_EATTING_FLESH,
    MONSTER_EATTING_FLESH,
    MONSTER_END_EATTING_FLESH,
    BLOODCAT_RUN,
    BLOODCAT_STARTREADY,
    BLOODCAT_READY,
    BLOODCAT_ENDREADY,
    BLOODCAT_HIT,
    BLOODCAT_DYING,
    BLOODCAT_DYING_STOP,
    BLOODCAT_SWIPE,
    NINJA_GOTOBREATH,
    NINJA_BREATH,
    NINJA_LOWKICK,
    NINJA_PUNCH,
    NINJA_SPINKICK,
    END_OPEN_DOOR,
    END_OPEN_LOCKED_DOOR,
    KICK_DOOR,
    CLOSE_DOOR,
    RIFLE_STAND_HIT,
    DEEP_WATER_TRED,
    DEEP_WATER_SWIM,
    DEEP_WATER_HIT,
    DEEP_WATER_DIE,
    DEEP_WATER_DIE_STOPPING,
    DEEP_WATER_DIE_STOP,
    LOW_TO_DEEP_WATER,
    DEEP_TO_LOW_WATER,
    GOTO_SLEEP,
    SLEEPING,
    WKAEUP_FROM_SLEEP,
    FIRE_LOW_STAND,
    FIRE_BURST_LOW_STAND,
    LARVAE_BREATH,
    LARVAE_HIT,
    LARVAE_DIE,
    LARVAE_DIE_STOP,
    LARVAE_WALK,
    INFANT_HIT,
    INFANT_DIE,
    INFANT_DIE_STOP,
    INFANT_ATTACK,
    INFANT_BEGIN_EATTING_FLESH,
    INFANT_EATTING_FLESH,
    INFANT_END_EATTING_FLESH,
    MONSTER_UP,
    MONSTER_JUMP,
    STANDING_SHOOT_UNJAM,
    CROUCH_SHOOT_UNJAM,
    PRONE_SHOOT_UNJAM,
    STANDING_SHOOT_DWEL_UNJAM,
    STANDING_SHOOT_LOW_UNJAM,
    READY_DUAL_CROUCH,
    AIM_DUAL_CROUCH,
    SHOOT_DUAL_CROUCH,
    END_DUAL_CROUCH,
    CROUCH_SHOOT_DWEL_UNJAM,
    ADJACENT_GET_ITEM,
    CUTTING_FENCE,
    CRIPPLE_BEG,
    CRIPPLE_HIT,
    CRIPPLE_DIE,
    CRIPPLE_DIE_STOP,
    CRIPPLE_DIE_FLYBACK,
    CRIPPLE_DIE_FLYBACK_STOP,
    CRIPPLE_KICKOUT,
    FROM_INJURED_TRANSITION,
    THROW_KNIFE,
    KNIFE_BREATH,
    KNIFE_GOTOBREATH,
    KNIFE_ENDBREATH,
    ROBOTNW_HIT,
    ROBOTNW_DIE,
    ROBOTNW_DIE_STOP,
    CATCH_STANDING,
    CATCH_CROUCHED,
    PLANT_BOMB,
    USE_REMOTE,
    START_COWER,
    COWERING,
    END_COWER,
    STEAL_ITEM,
    SHOOT_ROCKET,
    CIV_DIE2,
    SHOOT_MORTAR,
    CROW_DIE,
    SIDE_STEP,
    WALK_BACKWARDS,
    BEGIN_OPENSTRUCT,
    END_OPENSTRUCT,
    END_OPENSTRUCT_LOCKED,
    PUNCH_LOW,
    PISTOL_SHOOT_LOW,
    DECAPITATE,
    BLOODCAT_BITE_ANIM,
    BIGMERC_IDLE_NECK,
    BIGMERC_CROUCH_TRANS_INTO,
    BIGMERC_CROUCH_TRANS_OUTOF,
    GOTO_PATIENT,
    BEING_PATIENT,
    GOTO_DOCTOR,
    BEING_DOCTOR,
    END_DOCTOR,
    GOTO_REPAIRMAN,
    BEING_REPAIRMAN,
    END_REPAIRMAN,
    FALL_INTO_PIT,
    ROBOT_WALK,
    ROBOT_SHOOT,
    QUEEN_HIT,
    QUEEN_DIE,
    QUEEN_DIE_STOP,
    QUEEN_INTO_READY,
    QUEEN_READY,
    QUEEN_END_READY,
    QUEEN_CALL,
    QUEEN_SPIT,
    QUEEN_SWIPE,
    RELOAD_ROBOT,
    END_CATCH,
    END_CROUCH_CATCH,
    AI_RADIO,
    AI_CR_RADIO,
    TANK_SHOOT,
    TANK_BURST,
    QUEEN_SLAP,
    SLAP_HIT,
    TAKE_BLOOD_FROM_CORPSE,
    VEHICLE_DIE,
    QUEEN_FRUSTRATED_SLAP,
    CHARIOTS_OF_FIRE,
    AI_PULL_SWITCH,
    MONSTER_MELT,
    MERC_HURT_IDLE_ANIM,
    END_HURT_WALKING,
    PASS_OBJECT,
    DROP_ADJACENT_OBJECT,
    READY_DUAL_PRONE,
    AIM_DUAL_PRONE,
    SHOOT_DUAL_PRONE,
    END_DUAL_PRONE,
    PRONE_SHOOT_DWEL_UNJAM,
    PICK_LOCK,
    OPEN_DOOR_CROUCHED,
    BEGIN_OPENSTRUCT_CROUCHED,
    CLOSE_DOOR_CROUCHED,
    OPEN_STRUCT_CROUCHED,
    END_OPEN_DOOR_CROUCHED,
    END_OPENSTRUCT_CROUCHED,
    END_OPEN_LOCKED_DOOR_CROUCHED,
    END_OPENSTRUCT_LOCKED_CROUCHED,
    DRUNK_IDLE,
    CROWBAR_ATTACK,
    CIV_COWER_HIT,
    BLOODCAT_WALK_BACKWARDS,
    MONSTER_WALK_BACKWARDS,
    KID_SKIPPING,
    ROBOT_BURST_SHOOT,
    ATTACH_CAN_TO_STRING,
    SWAT_BACKWARDS,
    JUMP_OVER_BLOCKING_PERSON,
    REFUEL_VEHICLE,
    ROBOT_CAMERA_NOT_MOVING,
    CRIPPLE_OPEN_DOOR,
    CRIPPLE_CLOSE_DOOR,
    CRIPPLE_END_OPEN_DOOR,
    CRIPPLE_END_OPEN_LOCKED_DOOR,
    LOCKPICK_CROUCHED,
    NUMANIMATIONSTATES,

    INVALID_ANIMATION = 0xFFF0,
    FOUND_INVALID_ANIMATION = 0xFFF1,
}

public enum AnimationHeights
{
    ANIM_STAND = 6,
    ANIM_CROUCH = 3,
    ANIM_PRONE = 1,
}


public enum ANIM : uint
{
    IGNORE_AUTOSTANCE = 0x00000001,
    OK_CHARGE_AP_FOR_TURN = 0x00000002,
    STATIONARY = 0x00000004,
    MOVING = 0x00000008,
    TURNING = 0x00000010,
    FASTTURN = 0x00000020,
    FIREREADY = 0x00000040,
    NONINTERRUPT = 0x00000080,
    HITFINISH = 0x00000100,
    HITSTART = 0x00000200,
    HITWHENDOWN = 0x00000400,
    NOSHOW_MARKER = 0x00000800,
    NOMOVE_MARKER = 0x00001000,
    NORESTART = 0x00002000,
    HITSTOP = 0x00004000,
    SPECIALMOVE = 0x00008000,
    MERCIDLE = 0x00010000,
    STANCECHANGEANIM = 0x00020000,
    LOWER_WEAPON = 0x00040000,
    RAISE_WEAPON = 0x00080000,
    NOCHANGE_WEAPON = 0x00100000,
    NOCHANGE_PENDINGCOUNT = 0x00200000,
    NO_EFFORT = 0x00400000,
    MIN_EFFORT = 0x00800000,
    LIGHT_EFFORT = 0x01000000,
    MODERATE_EFFORT = 0x02000000,
    RT_NONINTERRUPT = 0x04000000,
    VARIABLE_EFFORT = 0x08000000,
    UPDATEMOVEMENTMODE = 0x10000000,
    FIRE = 0x20000000,
    BREATH = 0x40000000,
    IGNOREHITFINISH = 0x80000000,
}


public struct ANI_SPEED_DEF
{
    public int sSpeed;
    public float dMovementChange;
}

public struct ANIMCONTROLTYPE
{
    public string zAnimStr;
    public int sAP;
    public uint sSpeed;
    public float dMovementChange;
    public ANIM uiFlags;
    public AnimationHeights ubHeight;
    public AnimationHeights ubEndHeight;
    public int bProfile;
}

// DEFINES FOR BODY TYPE SUBSTITUTIONS
[Flags]
public enum SUB_ANIM
{
    BIGGUYSHOOT2 = 0x00000001,
    BIGGUYTHREATENSTANCE = 0x00000002,
}
