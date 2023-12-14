using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class LOS
{
    public static bool SoldierToLocationLineOfSightTest(SOLDIERTYPE pStartSoldier, int sGridNo, int ubTileSightLimit, int bAware)
    {
        return (SoldierTo3DLocationLineOfSightTest(pStartSoldier, sGridNo, 0, 0, ubTileSightLimit, bAware));
    }

    public static bool SoldierTo3DLocationLineOfSightTest(SOLDIERTYPE pStartSoldier, int sGridNo, int bLevel, int bCubeLevel, int ubTileSightLimit, int bAware)
    {
        float dEndZPos;
        int ubTargetID;
        SOLDIERTYPE? pTarget;
        bool fOk;

        CHECKF(pStartSoldier);

        fOk = CalculateSoldierZPos(pStartSoldier, POS.LOS_POS, out float dStartZPos);
        CHECKF(fOk);

        if (bCubeLevel > 0)
        {
            dEndZPos = ((bCubeLevel + bLevel * PROFILE_Z_SIZE) - 0.5f) * HEIGHT_UNITS_PER_INDEX;
            dEndZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[sGridNo].sHeight);
        }
        else
        {
            ubTargetID = WorldManager.WhoIsThere2(sGridNo, bLevel);
            if (ubTargetID != NOBODY)
            {
                pTarget = MercPtrs[ubTargetID];
                // there's a merc there; do a soldier-to-soldier test
                return (SoldierToSoldierLineOfSightTest(pStartSoldier, pTarget, ubTileSightLimit, bAware));
            }
            // else... assume standing height
            dEndZPos = STANDING_LOS_POS + bLevel * HEIGHT_UNITS;
            // add in ground height
            dEndZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[sGridNo].sHeight);
        }

        IsometricUtils.ConvertGridNoToXY(sGridNo, out int sXPos, out int sYPos);
        sXPos = sXPos * CELL_X_SIZE + (CELL_X_SIZE / 2);
        sYPos = sYPos * CELL_Y_SIZE + (CELL_Y_SIZE / 2);

        //        return (LineOfSightTest(IsometricUtils.CenterX(pStartSoldier.sGridNo), IsometricUtils.CenterY(pStartSoldier.sGridNo), dStartZPos, sXPos, sYPos, dEndZPos, ubTileSightLimit, gubTreeSightReduction[AnimationHeights.ANIM_STAND], bAware, 0, false, null));
        return false;
    }

    public static bool CalculateSoldierZPos(SOLDIERTYPE? pSoldier, POS ubPosType, out float pdZPos)
    {
        pdZPos = 0.0f;
        AnimationHeights ubHeight;

        if (pSoldier.ubBodyType == SoldierBodyTypes.CROW)
        {
            // Crow always as prone...
            ubHeight = AnimationHeights.ANIM_PRONE;
        }
        else if (pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
        {
            // treat as prone
            ubHeight = AnimationHeights.ANIM_PRONE;
        }
        else if (pSoldier.bOverTerrainType == TerrainTypeDefines.LOW_WATER || pSoldier.bOverTerrainType == TerrainTypeDefines.MED_WATER)
        {
            // treat as crouched
            ubHeight = AnimationHeights.ANIM_CROUCH;
        }
        else
        {
            if (CREATURE_OR_BLOODCAT(pSoldier) || pSoldier.ubBodyType == SoldierBodyTypes.COW)
            {
                // this if statement is to avoid the 'creature weak spot' target 
                // spot for creatures
                if (ubPosType == POS.HEAD_TARGET_POS || ubPosType == POS.LEGS_TARGET_POS)
                {
                    // override!
                    ubPosType = POS.TORSO_TARGET_POS;
                }
            }
            else if (TANK(pSoldier))
            {
                // high up!
                ubPosType = POS.HEAD_TARGET_POS;
            }

            ubHeight = gAnimControl[pSoldier.usAnimState].ubEndHeight;
        }

        switch (ubPosType)
        {
            case POS.LOS_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_LOS_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_LOS_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_LOS_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.FIRING_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_FIRING_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_FIRING_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_FIRING_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.TARGET_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_TARGET_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.HEAD_TARGET_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_HEAD_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_HEAD_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_HEAD_TARGET_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.TORSO_TARGET_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_TORSO_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_TORSO_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_TORSO_TARGET_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.LEGS_TARGET_POS:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_LEGS_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_LEGS_TARGET_POS;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_LEGS_TARGET_POS;
                        break;
                    default:
                        return (false);
                }
                break;
            case POS.HEIGHT:
                switch (ubHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        pdZPos = STANDING_HEIGHT;
                        break;
                    case AnimationHeights.ANIM_CROUCH:
                        pdZPos = CROUCHED_HEIGHT;
                        break;
                    case AnimationHeights.ANIM_PRONE:
                        pdZPos = PRONE_HEIGHT;
                        break;
                    default:
                        return (false);
                }
                break;
        }
        if (pSoldier.ubBodyType == SoldierBodyTypes.HATKIDCIV || pSoldier.ubBodyType == SoldierBodyTypes.KIDCIV)
        {
            // reduce value for kids who are 2/3 the height of regular people
            pdZPos = (pdZPos * 2) / 3;
        }
        else if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON || pSoldier.ubBodyType == SoldierBodyTypes.LARVAE_MONSTER || pSoldier.ubBodyType == SoldierBodyTypes.INFANT_MONSTER || pSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
        {
            // robot is 1/3 the height of regular people
            pdZPos = pdZPos / 3;
        }
        else if (TANK(pSoldier))
        {
           pdZPos = (pdZPos * 4) / 3;
        }

        if (pSoldier.bLevel > 0)
        { // on a roof
            pdZPos += WALL_HEIGHT_UNITS;
        }

        // IF this is a plane, strafe!
        // ATE: Don;t panic - this is temp - to be changed to a status flag....
        if (pSoldier.ubID == MAX_NUM_SOLDIERS)
        {
            pdZPos = (WALL_HEIGHT_UNITS * 2) - 1;
        }

        pdZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[pSoldier.sGridNo].sHeight);
        return (true);
    }

    public static bool SoldierToSoldierLineOfSightTest(SOLDIERTYPE pStartSoldier, SOLDIERTYPE pEndSoldier, int ubTileSightLimit, int bAware)
    {
        float dEndZPos;
        bool fOk;
        bool fSmell;
        int bEffectiveCamo;
        int ubTreeReduction;

        // TO ADD: if target is camouflaged and in cover, reduce sight distance by 30%
        // TO ADD: if in tear gas, reduce sight limit to 2 tiles
        CHECKF(pStartSoldier);
        CHECKF(pEndSoldier);
        fOk = CalculateSoldierZPos(pStartSoldier, POS.LOS_POS, out float dStartZPos);
        CHECKF(fOk);

        if (gWorldSectorX == 5 && gWorldSectorY == MAP_ROW.N)
        {
            // in the bloodcat arena sector, skip sight between army & bloodcats
            if (pStartSoldier.bTeam == ENEMY_TEAM && pEndSoldier.bTeam == CREATURE_TEAM)
            {
                return (false);
            }
            if (pStartSoldier.bTeam == CREATURE_TEAM && pEndSoldier.bTeam == ENEMY_TEAM)
            {
                return (false);
            }
        }

        if (pStartSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
        {
            // monsters use smell instead of sight!
            dEndZPos = STANDING_LOS_POS; // should avoid low rocks etc
            if (pEndSoldier.bLevel > 0)
            { // on a roof
                dEndZPos += WALL_HEIGHT_UNITS;
            }

            dEndZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[pEndSoldier.sGridNo].sHeight);
            fSmell = true;
        }
        else
        {
            fOk = CalculateSoldierZPos(pEndSoldier, POS.LOS_POS, out dEndZPos);
            CHECKF(fOk);
            fSmell = false;
        }

        if (TANK(pStartSoldier))
        {
            int sDistance;

            sDistance = IsometricUtils.PythSpacesAway(pStartSoldier.sGridNo, pEndSoldier.sGridNo);

            if (sDistance <= 8)
            {
                // blind spot?
                if (dEndZPos <= PRONE_LOS_POS)
                {
                    return (false);
                }
                else if (sDistance <= 4 && dEndZPos <= CROUCHED_LOS_POS)
                {
                    return (false);
                }
            }
        }

        if (pEndSoldier.bCamo > 0 && bAware == 0)
        {
            int iTemp;

            // reduce effects of camo of 5% per tile moved last turn
            if (pEndSoldier.ubBodyType == SoldierBodyTypes.BLOODCAT)
            {
                bEffectiveCamo = 100 - pEndSoldier.bTilesMoved * 5;
            }
            else
            {
                bEffectiveCamo = pEndSoldier.bCamo * (100 - pEndSoldier.bTilesMoved * 5) / 100;
            }
            bEffectiveCamo = Math.Max(bEffectiveCamo, 0);

            if (gAnimControl[pEndSoldier.usAnimState].ubEndHeight < AnimationHeights.ANIM_STAND)
            {
                // reduce visibility by up to a third for camouflage!
                switch (pEndSoldier.bOverTerrainType)
                {
                    case TerrainTypeDefines.FLAT_GROUND:
                    case TerrainTypeDefines.LOW_GRASS:
                    case TerrainTypeDefines.HIGH_GRASS:
                        iTemp = ubTileSightLimit;
                        iTemp -= iTemp * (bEffectiveCamo / 3) / 100;
                        ubTileSightLimit = iTemp;
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            bEffectiveCamo = 0;
        }

        if (TANK(pEndSoldier))
        {
            ubTreeReduction = 0;
        }
        else
        {
            ubTreeReduction = gubTreeSightReduction[gAnimControl[pEndSoldier.usAnimState].ubEndHeight];
        }

//        return (LineOfSightTest(IsometricUtils.CenterX(pStartSoldier.sGridNo), IsometricUtils.CenterY(pStartSoldier.sGridNo), dStartZPos, IsometricUtils.CenterX(pEndSoldier.sGridNo), IsometricUtils.CenterY(pEndSoldier.sGridNo), dEndZPos, ubTileSightLimit, ubTreeReduction, bAware, bEffectiveCamo, fSmell, null));
        return false;
    }
}


public enum POS
{
    LOS_POS,
    FIRING_POS,
    TARGET_POS,
    HEAD_TARGET_POS,
    TORSO_TARGET_POS,
    LEGS_TARGET_POS,
    HEIGHT
};
