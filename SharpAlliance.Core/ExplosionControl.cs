using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.IsometricUtils;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int GASMASK_MIN_STATUS = 70;
}

public class ExplosionControl
{
    private static IFileManager files;
    public ExplosionControl(IFileManager fileManager) => files = fileManager;

    public static int GetFreeExplosion()
    {
        int uiCount;

        for (uiCount = 0; uiCount < guiNumExplosions; uiCount++)
        {
            if (gExplosionData[uiCount].fAllocated == false)
            {
                return (int)uiCount;
            }
        }

        if (guiNumExplosions < NUM_EXPLOSION_SLOTS)
        {
            return (int)guiNumExplosions++;
        }

        return -1;
    }

    void RecountExplosions()
    {
        int uiCount;

        for (uiCount = guiNumExplosions - 1; uiCount >= 0; uiCount--)
        {
            if (gExplosionData[uiCount].fAllocated)
            {
                guiNumExplosions = (int)(uiCount + 1);
                break;
            }
        }
    }

    // GENERATE EXPLOSION
    private static void InternalIgniteExplosion(int ubOwner, int sX, MAP_ROW sY, int sZ, int sGridNo, Items usItem, bool fLocate, int bLevel)
    {
        EXPLOSION_PARAMS ExpParams;


        // Double check that we are using an explosive!
        if (!Item[usItem].usItemClass.HasFlag(IC.EXPLOSV))
        {
            return;
        }

        // Increment attack counter...

        if (gubElementsOnExplosionQueue == 0)
        {
            // single explosion, disable sight until the end, and set flag
            // to check sight at end of attack

            gTacticalStatus.uiFlags |= TacticalEngineStatus.DISALLOW_SIGHT | TacticalEngineStatus.CHECK_SIGHT_AT_END_OF_ATTACK;
        }


        gTacticalStatus.ubAttackBusyCount++;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, string.Format("Incrementing Attack: Explosion gone off, COunt now %d", gTacticalStatus.ubAttackBusyCount));


        // OK, go on!
        ExpParams.uiFlags = EXPLOSION_FLAG.USEABSPOS;
        ExpParams.ubOwner = (byte)ubOwner;
        ExpParams.ubTypeID = Weapons.Explosive[Item[usItem].ubClassIndex].ubAnimationID;
        ExpParams.sX = (short)sX;
        ExpParams.sY = sY;
        ExpParams.sZ = (short)sZ;
        ExpParams.sGridNo = (short)sGridNo;
        ExpParams.usItem = usItem;
        ExpParams.fLocate = fLocate;
        ExpParams.bLevel = (byte)bLevel;

        GenerateExplosion(out ExpParams);
    }

    public static void IgniteExplosion(int ubOwner, int sX, MAP_ROW sY, int sZ, int sGridNo, Items usItem, int bLevel)
    {
        InternalIgniteExplosion(ubOwner, sX, sY, sZ, sGridNo, usItem, true, bLevel);
    }

    public static void GenerateExplosion(out EXPLOSION_PARAMS pExpParams)
    {
        EXPLOSIONTYPE pExplosion;
        EXPLOSION_FLAG uiFlags;
        int ubOwner;
        EXPLOSION_TYPES ubTypeID;
        int sX;
        MAP_ROW sY;
        int sZ;
        int sGridNo;
        Items usItem;
        int iIndex;
        int bLevel;

        // Assign param values
        pExpParams = new();
        uiFlags = pExpParams.uiFlags;
        ubOwner = pExpParams.ubOwner;
        ubTypeID = pExpParams.ubTypeID;
        sX = pExpParams.sX;
        sY = pExpParams.sY;
        sZ = pExpParams.sZ;
        sGridNo = pExpParams.sGridNo;
        usItem = pExpParams.usItem;
        bLevel = pExpParams.bLevel;

        {
            // GET AND SETUP EXPLOSION INFO IN TABLE....
            iIndex = GetFreeExplosion();

            if (iIndex == -1)
            {
                return;
            }

            // OK, get pointer...
            pExplosion = gExplosionData[iIndex];

            //memset(pExplosion, 0, sizeof(EXPLOSIONTYPE));

            // Setup some data...
            //memcpy(&(pExplosion.Params), pExpParams, sizeof(EXPLOSION_PARAMS));
            pExplosion.fAllocated = true;
            pExplosion.iID = (WorldDirections)iIndex;

            GenerateExplosionFromExplosionPointer(pExplosion);
        }

        // ATE: Locate to explosion....
        if (pExpParams.fLocate)
        {
            Overhead.LocateGridNo(sGridNo);
        }
    }


    public static void GenerateExplosionFromExplosionPointer(EXPLOSIONTYPE pExplosion)
    {
        EXPLOSION_FLAG uiFlags;
        int ubOwner;
        EXPLOSION_TYPES ubTypeID;
        int sX;
        MAP_ROW sY;
        int sZ;
        int sGridNo;
        Items usItem;
        TerrainTypeDefines ubTerrainType;
        int bLevel;
        SoundDefine uiSoundID;

        ANITILE_PARAMS AniParams = new();

        // Assign param values
        uiFlags = pExplosion.Params.uiFlags;
        ubOwner = pExplosion.Params.ubOwner;
        ubTypeID = pExplosion.Params.ubTypeID;
        sX = pExplosion.Params.sX;
        sY = pExplosion.Params.sY;
        sZ = pExplosion.Params.sZ;
        sGridNo = pExplosion.Params.sGridNo;
        usItem = pExplosion.Params.usItem;
        bLevel = pExplosion.Params.bLevel;

        // If Z value given is 0 and bLevel > 0, make z heigher
        if (sZ == 0 && bLevel > 0)
        {
            sZ = ROOF_LEVEL_HEIGHT;
        }

        pExplosion.iLightID = -1;

        // OK, if we are over water.... use water explosion...
        ubTerrainType = WorldManager.GetTerrainType(sGridNo);

        // Setup explosion!
        //memset(&AniParams, 0, sizeof(ANITILE_PARAMS));

        AniParams.sGridNo = sGridNo;
        AniParams.ubLevelID = ANI.TOPMOST_LEVEL;
        AniParams.sDelay = sBlastSpeeds[ubTypeID];
        AniParams.sStartFrame = pExplosion.sCurrentFrame;
        AniParams.uiFlags = ANITILEFLAGS.CACHEDTILE | ANITILEFLAGS.FORWARD | ANITILEFLAGS.EXPLOSION;

        if (ubTerrainType == TerrainTypeDefines.LOW_WATER || ubTerrainType == TerrainTypeDefines.MED_WATER || ubTerrainType == TerrainTypeDefines.DEEP_WATER)
        {
            // Change type to water explosion...
            ubTypeID = EXPLOSION_TYPES.WATER_BLAST;
            AniParams.uiFlags |= ANITILEFLAGS.ALWAYS_TRANSLUCENT;
        }


        if (sZ < TileDefine.WALL_HEIGHT)
        {
            AniParams.uiFlags |= ANITILEFLAGS.NOZBLITTER;
        }

        if (uiFlags.HasFlag(EXPLOSION_FLAG.USEABSPOS))
        {
            AniParams.sX = sX;
            AniParams.sY = sY;
            AniParams.sZ = sZ;

            //AniParams.uiFlags							|= ANITILE_USEABSOLUTEPOS;
        }

        AniParams.ubKeyFrame1 = ubTransKeyFrame[ubTypeID];
        AniParams.uiKeyFrame1Code = ANI_KEYFRAME.BEGIN_TRANSLUCENCY;

        if (!uiFlags.HasFlag(EXPLOSION_FLAG.DISPLAYONLY))
        {
            AniParams.ubKeyFrame2 = ubDamageKeyFrame[ubTypeID];
            AniParams.uiKeyFrame2Code = ANI_KEYFRAME.BEGIN_DAMAGE;
        }
        AniParams.uiUserData = usItem;
        AniParams.ubUserData2 = ubOwner;
        AniParams.uiUserData3 = pExplosion.iID;


        AniParams.zCachedFile = zBlastFilenames[ubTypeID];

        TileAnimations.CreateAnimationTile(ref AniParams);

        //  set light source....
        if (pExplosion.iLightID == -1)
        {
            // DO ONLY IF WE'RE AT A GOOD LEVEL
            //            if (ubAmbientLightLevel >= MIN_AMB_LEVEL_FOR_MERC_LIGHTS)
            //            {
            //                if ((pExplosion.iLightID = LightSpriteCreate("L-R04.LHT", 0)) != (-1))
            //                {
            //                    LightSpritePower(pExplosion.iLightID, true);
            //
            //                    LightSpritePosition(pExplosion.iLightID, (int)(sX / CELL_X_SIZE), ((int)sY / CELL_Y_SIZE));
            //                }
            //            }
        }

        uiSoundID = uiExplosionSoundID[(int)ubTypeID];

        if (uiSoundID == SoundDefine.EXPLOSION_1)
        {
            // Randomize
            if (Globals.Random.Next(2) == 0)
            {
                uiSoundID = SoundDefine.EXPLOSION_ALT_BLAST_1;
            }
        }

        // PlayJA2Sample(uiSoundID, RATE_11025, SoundVolume(HIGHVOLUME, sGridNo), 1, SoundDir(sGridNo));

    }

    void UpdateExplosionFrame(int iIndex, int sCurrentFrame)
    {
        gExplosionData[iIndex].sCurrentFrame = (short)sCurrentFrame;
    }

    public static void RemoveExplosionData(int iIndex)
    {
        gExplosionData[iIndex].fAllocated = false;

        if (gExplosionData[iIndex].iLightID != -1)
        {
            //            LightSpriteDestroy(gExplosionData[iIndex].iLightID);
        }
    }

    void HandleFencePartnerCheck(int sStructGridNo)
    {
        STRUCTURE? pFenceStructure, pFenceBaseStructure;
        LEVELNODE? pFenceNode;
        int bFenceDestructionPartner = -1;

        pFenceStructure = WorldStructures.FindStructure(sStructGridNo, STRUCTUREFLAGS.FENCE);

        if (pFenceStructure is not null)
        {
            // How does our explosion partner look?
            if (pFenceStructure.pDBStructureRef.pDBStructure.bDestructionPartner < 0)
            {
                // Find level node.....
                pFenceBaseStructure = WorldStructures.FindBaseStructure(pFenceStructure);

                // Get LEVELNODE for struct and remove!
                pFenceNode = WorldStructures.FindLevelNodeBasedOnStructure(pFenceBaseStructure.sGridNo, pFenceBaseStructure);

                // Get type from index...
                TileDefine.GetTileType(pFenceNode.usIndex, out TileTypeDefines uiFenceType);

                bFenceDestructionPartner = -1 * pFenceBaseStructure.pDBStructureRef.pDBStructure.bDestructionPartner;

                // Get new index
                TileDefine.GetTileIndexFromTypeSubIndex(uiFenceType, (int)bFenceDestructionPartner, out TileIndexes usTileIndex);

                //Set a flag indicating that the following changes are to go the the maps, temp file
                SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                // Remove it!
                WorldManager.RemoveStructFromLevelNode(pFenceBaseStructure.sGridNo, pFenceNode);

                // Add it!
                WorldManager.AddStructToHead(pFenceBaseStructure.sGridNo, usTileIndex);

                SaveLoadMap.ApplyMapChangesToMapTempFile(false);
            }
        }
    }

    int ExplosiveDamageStructureAtGridNo(STRUCTURE? pCurrent, STRUCTURE? ppNextCurrent, int sGridNo, int sWoundAmt, int uiDist, out bool pfRecompileMovementCosts, bool fOnlyWalls, bool fSubSequentMultiTilesTransitionDamage, int ubOwner, int bLevel)
    {
        pfRecompileMovementCosts = false;
        int sX, sY;
        STRUCTURE? pBase, pWallStruct = null, pAttached, pAttachedBase;
        LEVELNODE? pNode = null, pNewNode = null, pAttachedNode;
        int sNewGridNo, sStructGridNo;
        TileIndexes sNewIndex;
        int sSubIndex;
        TileIndexes usObjectIndex, usTileIndex;
        int ubNumberOfTiles, ubLoop;
        List<DB_STRUCTURE_TILE> ppTile;
        int bDestructionPartner = -1;
        int bDamageReturnVal;
        int fContinue;
        TileTypeDefines uiTileType;
        int sBaseGridNo;
        bool fExplosive;

        // ATE: Check for O3 statue for special damage..
        // note we do this check every time explosion goes off in game, but it's
        // an effiecnent check...
        //        if (DoesO3SectorStatueExistHere(sGridNo) && uiDist <= 1)
        //        {
        //            ChangeO3SectorStatue(true);
        //            return (1);
        //        }

        // Get xy
        sX = IsometricUtils.CenterX(sGridNo);
        sY = IsometricUtils.CenterY(sGridNo);

        // ATE: Continue if we are only looking for walls
        if (fOnlyWalls && !pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF))
        {
            return 1;
        }

        if (bLevel > 0)
        {
            return 1;
        }

        // Is this a corpse?
        if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.CORPSE)
            && GameSettings.fOptions[TOPTION.BLOOD_N_GORE]
            && sWoundAmt > 10)
        {
            // Spray corpse in a fine mist....
            if (uiDist <= 1)
            {
                // Remove corpse...
                //                VaporizeCorpse(sGridNo, pCurrent.usStructureID);
            }
        }
        else if (!pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.PERSON))
        {
            // Damage structure!
            if ((bDamageReturnVal = StructureInternals.DamageStructure(pCurrent, (int)sWoundAmt, STRUCTURE_DAMAGE_EXPLOSION, sGridNo, sX, sY, NOBODY)) != 0)
            {
                fContinue = 0;

                pBase = WorldStructures.FindBaseStructure(pCurrent);

                sBaseGridNo = pBase.sGridNo;

                // if the structure is openable, destroy all items there
                if (pBase.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE)
                    && !pBase.fFlags.HasFlag(STRUCTUREFLAGS.DOOR))
                {
                    //                    RemoveAllUnburiedItems(pBase.sGridNo, bLevel);
                }

                fExplosive = (pCurrent.fFlags & STRUCTUREFLAGS.EXPLOSIVE) != 0;

                // Get LEVELNODE for struct and remove!
                pNode = WorldStructures.FindLevelNodeBasedOnStructure(pBase.sGridNo, pBase);

                // ATE: if we have completely destroyed a structure,
                // and this structure should have a in-between explosion partner,
                // make damage code 2 - which means only damaged - the normal explosion
                // spreading will cause it do use the proper peices..
                if (bDamageReturnVal == 1 && pBase.pDBStructureRef.pDBStructure.bDestructionPartner < 0)
                {
                    bDamageReturnVal = 2;
                }

                if (bDamageReturnVal == 1)
                {
                    fContinue = 1;
                }
                // Check for a damaged looking graphic...
                else if (bDamageReturnVal == 2)
                {
                    if (pBase.pDBStructureRef.pDBStructure.bDestructionPartner < 0)
                    {
                        // We swap to another graphic!
                        // It's -ve and 1-based, change to +ve, 1 based
                        bDestructionPartner = -1 * pBase.pDBStructureRef.pDBStructure.bDestructionPartner;

                        TileDefine.GetTileType(pNode.usIndex, out uiTileType);

                        fContinue = 2;
                    }
                }

                if (fContinue > 0)
                {
                    // Remove the beast!
                    while (ppNextCurrent != null && ppNextCurrent.usStructureID == pCurrent.usStructureID)
                    {
                        // the next structure will also be deleted so we had better
                        // skip past it!
                        ppNextCurrent = ppNextCurrent.pNext;
                    }

                    // Replace with explosion debris if there are any....
                    // ( and there already sin;t explosion debris there.... )
                    if (pBase.pDBStructureRef.pDBStructure.bDestructionPartner > 0)
                    {
                        // Alrighty add!

                        // Add to every gridno structure is in
                        ubNumberOfTiles = pBase.pDBStructureRef.pDBStructure.ubNumberOfTiles;
                        ppTile = pBase.pDBStructureRef.ppTile;

                        bDestructionPartner = pBase.pDBStructureRef.pDBStructure.bDestructionPartner;

                        // OK, destrcution index is , as default, the partner, until we go over the first set of explsion
                        // debris...
                        if (bDestructionPartner > 39)
                        {
                            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.SECONDEXPLDEBRIS, (int)(bDestructionPartner - 40), out usTileIndex);
                        }
                        else
                        {
                            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.FIRSTEXPLDEBRIS, bDestructionPartner, out usTileIndex);
                        }

                        // Free all the non-base tiles; the base tile is at pointer 0
                        for (ubLoop = BASE_TILE; ubLoop < ubNumberOfTiles; ubLoop++)
                        {
                            if (!ppTile[ubLoop].fFlags.HasFlag(TILE.ON_ROOF))
                            {
                                sStructGridNo = pBase.sGridNo + ppTile[ubLoop].sPosRelToBase;
                                // there might be two structures in this tile, one on each level, but we just want to
                                // delete one on each pass

                                if (!WorldManager.TypeRangeExistsInObjectLayer(sStructGridNo, TileTypeDefines.FIRSTEXPLDEBRIS, TileTypeDefines.SECONDEXPLDEBRIS, out usObjectIndex))
                                {
                                    //Set a flag indicating that the following changes are to go the the maps, temp file
                                    SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                    WorldManager.AddObjectToHead(sStructGridNo, usTileIndex + Globals.Random.Next(3));

                                    SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                }
                            }
                        }

                        // IF we are a wall, add debris for the other side
                        if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF))
                        {
                            switch (pCurrent.ubWallOrientation)
                            {
                                case WallOrientation.OUTSIDE_TOP_LEFT:
                                case WallOrientation.INSIDE_TOP_LEFT:

                                    sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTH));
                                    if (!WorldManager.TypeRangeExistsInObjectLayer(sStructGridNo, TileTypeDefines.FIRSTEXPLDEBRIS, TileTypeDefines.SECONDEXPLDEBRIS, out usObjectIndex))
                                    {
                                        //Set a flag indicating that the following changes are to go the the maps, temp file
                                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                        WorldManager.AddObjectToHead(sStructGridNo, usTileIndex + Globals.Random.Next(3));

                                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                    }
                                    break;

                                case WallOrientation.OUTSIDE_TOP_RIGHT:
                                case WallOrientation.INSIDE_TOP_RIGHT:

                                    sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.EAST));
                                    if (!WorldManager.TypeRangeExistsInObjectLayer(sStructGridNo, TileTypeDefines.FIRSTEXPLDEBRIS, TileTypeDefines.SECONDEXPLDEBRIS, out usObjectIndex))
                                    {
                                        //Set a flag indicating that the following changes are to go the the maps, temp file
                                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                        WorldManager.AddObjectToHead(sStructGridNo, usTileIndex + Globals.Random.Next(3));

                                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                    }
                                    break;
                            }
                        }
                    }
                    // Else look for fences, walk along them to change to destroyed peices...
                    else if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.FENCE))
                    {
                        // walk along based on orientation
                        switch (pCurrent.ubWallOrientation)
                        {
                            case WallOrientation.OUTSIDE_TOP_RIGHT:
                            case WallOrientation.INSIDE_TOP_RIGHT:

                                sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTH));
                                this.HandleFencePartnerCheck(sStructGridNo);
                                sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.NORTH));
                                this.HandleFencePartnerCheck(sStructGridNo);
                                break;

                            case WallOrientation.OUTSIDE_TOP_LEFT:
                            case WallOrientation.INSIDE_TOP_LEFT:

                                sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.EAST));
                                this.HandleFencePartnerCheck(sStructGridNo);
                                sStructGridNo = IsometricUtils.NewGridNo(pBase.sGridNo, IsometricUtils.DirectionInc(WorldDirections.WEST));
                                this.HandleFencePartnerCheck(sStructGridNo);
                                break;
                        }
                    }

                    // OK, Check if this is a wall, then search and change other walls based on this
                    if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF))
                    {
                        // ATE
                        // Remove any decals in tile....
                        // Use tile database for this as apposed to stuct data
                        WorldManager.RemoveAllStructsOfTypeRange(pBase.sGridNo, TileTypeDefines.FIRSTWALLDECAL, TileTypeDefines.FOURTHWALLDECAL);
                        WorldManager.RemoveAllStructsOfTypeRange(pBase.sGridNo, TileTypeDefines.FIFTHWALLDECAL, TileTypeDefines.EIGTHWALLDECAL);

                        // Alrighty, now do this
                        // Get orientation
                        // based on orientation, go either x or y dir
                        // check for wall in both _ve and -ve directions
                        // if found, replace!
                        switch (pCurrent.ubWallOrientation)
                        {
                            case WallOrientation.OUTSIDE_TOP_LEFT:
                            case WallOrientation.INSIDE_TOP_LEFT:

                                // Move WEST
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.WEST));

                                pNewNode = StructureWrap.GetWallLevelNodeAndStructOfSameOrientationAtGridno(sNewGridNo, pCurrent.ubWallOrientation, pWallStruct);

                                if (pNewNode != null)
                                {
                                    if (pWallStruct.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
                                    {
                                        if (pCurrent.ubWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                                        {
                                            sSubIndex = 48;
                                        }
                                        else
                                        {
                                            sSubIndex = 52;
                                        }

                                        // Replace!
                                        TileDefine.GetTileIndexFromTypeSubIndex(gTileDatabase[pNewNode.usIndex].fType, sSubIndex, out sNewIndex);

                                        //Set a flag indicating that the following changes are to go the the maps temp file
                                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                        WorldManager.RemoveStructFromLevelNode(sNewGridNo, pNewNode);
                                        WorldManager.AddWallToStructLayer(sNewGridNo, sNewIndex, true);

                                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);

                                    }
                                }

                                // Move in EAST
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.EAST));

                                pNewNode = StructureWrap.GetWallLevelNodeAndStructOfSameOrientationAtGridno(sNewGridNo, pCurrent.ubWallOrientation, pWallStruct);

                                if (pNewNode != null)
                                {
                                    if (pWallStruct.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
                                    {
                                        if (pCurrent.ubWallOrientation == WallOrientation.OUTSIDE_TOP_LEFT)
                                        {
                                            sSubIndex = 49;
                                        }
                                        else
                                        {
                                            sSubIndex = 53;
                                        }

                                        // Replace!
                                        TileDefine.GetTileIndexFromTypeSubIndex(gTileDatabase[pNewNode.usIndex].fType, sSubIndex, out sNewIndex);

                                        //Set a flag indicating that the following changes are to go the the maps, temp file
                                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                        WorldManager.RemoveStructFromLevelNode(sNewGridNo, pNewNode);
                                        WorldManager.AddWallToStructLayer(sNewGridNo, sNewIndex, true);

                                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                    }
                                }

                                // look for attached structures in same tile
                                sNewGridNo = pBase.sGridNo;
                                pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_LEFT_WALL);
                                while (pAttached is not null)
                                {
                                    pAttachedBase = WorldStructures.FindBaseStructure(pAttached);
                                    if (pAttachedBase is not null)
                                    {
                                        // Remove the beast!
                                        while (ppNextCurrent != null && ppNextCurrent.usStructureID == pAttachedBase.usStructureID)
                                        {
                                            // the next structure will also be deleted so we had better
                                            // skip past it!
                                            ppNextCurrent = ppNextCurrent.pNext;
                                        }

                                        pAttachedNode = WorldStructures.FindLevelNodeBasedOnStructure(pAttachedBase.sGridNo, pAttachedBase);
                                        if (pAttachedNode is not null)
                                        {
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                                            WorldManager.RemoveStructFromLevelNode(pAttachedBase.sGridNo, pAttachedNode);
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                        }
                                        else
                                        {
                                            // error!
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // error!
                                        break;
                                    }
                                    // search for another, from the start of the list
                                    pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_LEFT_WALL);
                                }

                                // Move in SOUTH, looking for attached structures to remove
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.SOUTH));
                                pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_LEFT_WALL);
                                while (pAttached is not null)
                                {
                                    pAttachedBase = WorldStructures.FindBaseStructure(pAttached);
                                    if (pAttachedBase is not null)
                                    {
                                        pAttachedNode = WorldStructures.FindLevelNodeBasedOnStructure(pAttachedBase.sGridNo, pAttachedBase);
                                        if (pAttachedNode is not null)
                                        {
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                                            WorldManager.RemoveStructFromLevelNode(pAttachedBase.sGridNo, pAttachedNode);
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                        }
                                        else
                                        {
                                            // error!
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // error!
                                        break;
                                    }
                                    // search for another, from the start of the list
                                    pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_LEFT_WALL);
                                }
                                break;

                            case WallOrientation.OUTSIDE_TOP_RIGHT:
                            case WallOrientation.INSIDE_TOP_RIGHT:

                                // Move in NORTH
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.NORTH));

                                pNewNode = StructureWrap.GetWallLevelNodeAndStructOfSameOrientationAtGridno(sNewGridNo, pCurrent.ubWallOrientation, pWallStruct);

                                if (pNewNode != null)
                                {
                                    if (pWallStruct.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
                                    {
                                        if (pCurrent.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
                                        {
                                            sSubIndex = 51;
                                        }
                                        else
                                        {
                                            sSubIndex = 55;
                                        }

                                        // Replace!
                                        TileDefine.GetTileIndexFromTypeSubIndex(gTileDatabase[pNewNode.usIndex].fType, sSubIndex, out sNewIndex);

                                        //Set a flag indicating that the following changes are to go the the maps, temp file
                                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                                        WorldManager.RemoveStructFromLevelNode(sNewGridNo, pNewNode);
                                        WorldManager.AddWallToStructLayer(sNewGridNo, sNewIndex, true);

                                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                    }
                                }

                                // Move in SOUTH
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.SOUTH));

                                pNewNode = StructureWrap.GetWallLevelNodeAndStructOfSameOrientationAtGridno(sNewGridNo, pCurrent.ubWallOrientation, pWallStruct);

                                if (pNewNode != null)
                                {
                                    if (pWallStruct.fFlags.HasFlag(STRUCTUREFLAGS.WALL))
                                    {
                                        if (pCurrent.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
                                        {
                                            sSubIndex = 50;
                                        }
                                        else
                                        {
                                            sSubIndex = 54;
                                        }

                                        // Replace!
                                        TileDefine.GetTileIndexFromTypeSubIndex(gTileDatabase[pNewNode.usIndex].fType, sSubIndex, out sNewIndex);

                                        //Set a flag indicating that the following changes are to go the the maps, temp file
                                        //ApplyMapChangesToMapTempFile(true);

                                        WorldManager.RemoveStructFromLevelNode(sNewGridNo, pNewNode);
                                        WorldManager.AddWallToStructLayer(sNewGridNo, sNewIndex, true);

                                        //ApplyMapChangesToMapTempFile(false);
                                    }
                                }

                                // looking for attached structures to remove in base tile
                                sNewGridNo = pBase.sGridNo;
                                pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_RIGHT_WALL);
                                while (pAttached is not null)
                                {
                                    pAttachedBase = WorldStructures.FindBaseStructure(pAttached);
                                    if (pAttachedBase is not null)
                                    {
                                        pAttachedNode = WorldStructures.FindLevelNodeBasedOnStructure(pAttachedBase.sGridNo, pAttachedBase);
                                        if (pAttachedNode is not null)
                                        {
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                                            WorldManager.RemoveStructFromLevelNode(pAttachedBase.sGridNo, pAttachedNode);
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                        }
                                        else
                                        {
                                            // error!
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // error!
                                        break;
                                    }
                                    // search for another, from the start of the list
                                    pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_RIGHT_WALL);
                                }

                                // Move in EAST, looking for attached structures to remove
                                sNewGridNo = NewGridNo(pBase.sGridNo, DirectionInc(WorldDirections.EAST));
                                pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_RIGHT_WALL);
                                while (pAttached is not null)
                                {
                                    pAttachedBase = WorldStructures.FindBaseStructure(pAttached);
                                    if (pAttachedBase is not null)
                                    {
                                        pAttachedNode = WorldStructures.FindLevelNodeBasedOnStructure(pAttachedBase.sGridNo, pAttachedBase);
                                        if (pAttachedNode is not null)
                                        {
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                                            WorldManager.RemoveStructFromLevelNode(pAttachedBase.sGridNo, pAttachedNode);
                                            SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                                        }
                                        else
                                        {
                                            // error!
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // error!
                                        break;
                                    }
                                    // search for another, from the start of the list
                                    pAttached = WorldStructures.FindStructure(sNewGridNo, STRUCTUREFLAGS.ON_RIGHT_WALL);
                                }

                                break;
                        }

                        // CJC, Sept 16: if we destroy any wall of the brothel, make Kingpin's men hostile!
                        if (gWorldSectorX == 5 && gWorldSectorY == MAP_ROW.C && gbWorldSectorZ == 0)
                        {
                            bool fInRoom;

                            fInRoom = RenderFun.InARoom(sGridNo, out int ubRoom);
                            if (!fInRoom)
                            {
                                // try to south
                                fInRoom = RenderFun.InARoom((int)(sGridNo + DirectionInc(WorldDirections.SOUTH)), out ubRoom);
                                if (!fInRoom)
                                {
                                    // try to east
                                    fInRoom = RenderFun.InARoom((int)(sGridNo + DirectionInc(WorldDirections.EAST)), out ubRoom);
                                }
                            }

                            if (fInRoom && IN_BROTHEL(ubRoom))
                            {
                                //                                CivilianGroupChangesSides(CIV_GROUP.KINGPIN_CIV_GROUP);
                            }
                        }

                    }

                    // OK, we need to remove the water from the fountain 
                    // Lots of HARD CODING HERE :(
                    // Get tile type
                    TileDefine.GetTileType(pNode.usIndex, out uiTileType);
                    // Check if we are a fountain!
                    if (gTilesets[giCurrentTilesetID].TileSurfaceFilenames[uiTileType].Equals("fount1.sti", StringComparison.OrdinalIgnoreCase))
                    {
                        // Yes we are!
                        // Remove water....
                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                        TileDefine.GetTileIndexFromTypeSubIndex(uiTileType, 1, out sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        TileDefine.GetTileIndexFromTypeSubIndex(uiTileType, 2, out sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        TileDefine.GetTileIndexFromTypeSubIndex(uiTileType, 3, out sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        WorldManager.RemoveStruct(sBaseGridNo, sNewIndex);
                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                    }


                    // Remove any interactive tiles we could be over!
                    InteractiveTiles.BeginCurInteractiveTileCheck(INTILE_CHECK_SELECTIVE);

                    if (pCurrent.fFlags.HasFlag(STRUCTUREFLAGS.WALLSTUFF))
                    {
                        //                        RecompileLocalMovementCostsForWall(pBase.sGridNo, pBase.ubWallOrientation);
                    }

                    // Remove!
                    //Set a flag indicating that the following changes are to go the the maps, temp file
                    SaveLoadMap.ApplyMapChangesToMapTempFile(true);
                    WorldManager.RemoveStructFromLevelNode(pBase.sGridNo, pNode);
                    SaveLoadMap.ApplyMapChangesToMapTempFile(false);

                    // OK, if we are to swap structures, do it now...
                    if (fContinue == 2)
                    {
                        // We have a levelnode...
                        // Get new index for new grpahic....
                        TileDefine.GetTileIndexFromTypeSubIndex(uiTileType, bDestructionPartner, out usTileIndex);

                        SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                        WorldManager.AddStructToHead(sBaseGridNo, usTileIndex);

                        SaveLoadMap.ApplyMapChangesToMapTempFile(false);


                    }

                    // Rerender world!
                    // Reevaluate world movement costs, reduncency!
                    gTacticalStatus.uiFlags |= TacticalEngineStatus.NOHIDE_REDUNDENCY;
                    // FOR THE NEXT RENDER LOOP, RE-EVALUATE REDUNDENT TILES
                    RenderWorld.InvalidateWorldRedundency();
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                    // Movement costs!
                    pfRecompileMovementCosts = true;

                    {
                        // Make secondary explosion if eplosive....
                        if (fExplosive)
                        {
                            InternalIgniteExplosion(ubOwner, CenterX(sBaseGridNo), (MAP_ROW)CenterY(sBaseGridNo), 0, sBaseGridNo, Items.STRUCTURE_EXPLOSION, false, bLevel);
                        }
                    }

                    if (fContinue == 2)
                    {
                        return 0;
                    }
                }

                // 2 is NO DAMAGE
                pfRecompileMovementCosts = false;
                return 2;
            }
        }

        pfRecompileMovementCosts = false;
        return 1;
    }

    void ExplosiveDamageGridNo(int sGridNo, int sWoundAmt, int uiDist, out bool pfRecompileMovementCosts, bool fOnlyWalls, int bMultiStructSpecialFlag, int fSubSequentMultiTilesTransitionDamage, int ubOwner, int bLevel)
    {
        STRUCTURE? pCurrent, pNextCurrent, pStructure;
        STRUCTURE? pBaseStructure;
        STRUCTURE_ON sDesiredLevel;
        DB_STRUCTURE_TILE[] ppTile;
        int ubLoop, ubLoop2;
        int sNewGridNo, sNewGridNo2, sBaseGridNo = 0;
        bool fToBreak = false;
        bool fMultiStructure = false;
        int ubNumberOfTiles = 0;
        int fMultiStructSpecialFlag = 0;
        int fExplodeDamageReturn = 0;

        // Based on distance away, damage any struct at this gridno
        // OK, loop through structures and damage!
        pCurrent = gpWorldLevelData[sGridNo].pStructureHead;
        sDesiredLevel = STRUCTURE_ON.GROUND;

        // This code gets a little hairy because 
        // (1) we might need to destroy the currently-examined structure
        while (pCurrent != null)
        {
            // ATE: These are for the chacks below for multi-structs....
            pBaseStructure = WorldStructures.FindBaseStructure(pCurrent);

            if (pBaseStructure is not null)
            {
                sBaseGridNo = pBaseStructure.sGridNo;
                ubNumberOfTiles = pBaseStructure.pDBStructureRef.pDBStructure.ubNumberOfTiles;
                fMultiStructure = pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.MULTI);
                ppTile = new DB_STRUCTURE_TILE[ubNumberOfTiles];
                //memcpy(ppTile, pBaseStructure.pDBStructureRef.ppTile, sizeof(DB_STRUCTURE_TILE) * ubNumberOfTiles);

                if (bMultiStructSpecialFlag == -1)
                {
                    // Set it!
                    bMultiStructSpecialFlag = pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.SPECIAL) ? 1 : 0;
                }

                if (pBaseStructure.fFlags.HasFlag(STRUCTUREFLAGS.EXPLOSIVE))
                {
                    // ATE: Set hit points to zero....
                    pBaseStructure.ubHitPoints = 0;
                }
            }
            else
            {
                fMultiStructure = false;
            }

            pNextCurrent = pCurrent.pNext;
            gStruct = pNextCurrent;

            // Check level!
            if (pCurrent.sCubeOffset == sDesiredLevel)
            {
                fExplodeDamageReturn = this.ExplosiveDamageStructureAtGridNo(pCurrent, pNextCurrent, sGridNo, sWoundAmt, uiDist, out pfRecompileMovementCosts, fOnlyWalls, false, ubOwner, bLevel);

                // Are we overwritting damage due to multi-tile...?
                if (fExplodeDamageReturn > 0)
                {
                    if (fSubSequentMultiTilesTransitionDamage == 2)
                    {
                        fExplodeDamageReturn = 2;
                    }
                    else
                    {
                        fExplodeDamageReturn = 1;
                    }
                }

                if (fExplodeDamageReturn == 0)
                {
                    fToBreak = true;
                }
            }

            // OK, for multi-structs...
            // AND we took damage...
            if (fMultiStructure && !fOnlyWalls && fExplodeDamageReturn == 0)
            {
                // ATE: Don't after first attack...
                if (uiDist > 1)
                {
                    if (pBaseStructure is not null)
                    {
                        ppTile = null;
                    }

                    pfRecompileMovementCosts = false;
                    return;
                }

                {

                    for (ubLoop = BASE_TILE; ubLoop < ubNumberOfTiles; ubLoop++)
                    {
                        sNewGridNo = 0;
                        //                        sNewGridNo = sBaseGridNo + ppTile[ubLoop].sPosRelToBase;

                        // look in adjacent tiles
                        for (ubLoop2 = 0; ubLoop2 < NUM_WORLD_DIRECTIONS; ubLoop2++)
                        {
                            sNewGridNo2 = NewGridNo(sNewGridNo, DirectionInc(ubLoop2));
                            if (sNewGridNo2 != sNewGridNo && sNewGridNo2 != sGridNo)
                            {
                                pStructure = WorldStructures.FindStructure(sNewGridNo2, STRUCTUREFLAGS.MULTI);
                                if (pStructure is not null)
                                {
                                    fMultiStructSpecialFlag = pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SPECIAL) ? 1 : 0;

                                    if (bMultiStructSpecialFlag == fMultiStructSpecialFlag)
                                    {
                                        // If we just damaged it, use same damage value....
                                        if (fMultiStructSpecialFlag > 0)
                                        {
                                            this.ExplosiveDamageGridNo(sNewGridNo2, sWoundAmt, uiDist, out pfRecompileMovementCosts, fOnlyWalls, bMultiStructSpecialFlag, 1, ubOwner, bLevel);
                                        }
                                        else
                                        {
                                            this.ExplosiveDamageGridNo(sNewGridNo2, sWoundAmt, uiDist, out pfRecompileMovementCosts, fOnlyWalls, bMultiStructSpecialFlag, 2, ubOwner, bLevel);
                                        }

                                        {
                                            //                                            InternalIgniteExplosion(ubOwner, CenterX(sNewGridNo2), CenterY(sNewGridNo2), 0, sNewGridNo2, RDX, false, bLevel);
                                        }

                                        fToBreak = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (fToBreak)
                {
                    break;
                }
            }

            if (pBaseStructure is not null)
            {
                ppTile = null;
            }
            pCurrent = pNextCurrent;
        }

        pfRecompileMovementCosts = false;
    }


    bool DamageSoldierFromBlast(int ubPerson, int ubOwner, int sBombGridNo, int sWoundAmt, int sBreathAmt, int uiDist, Items usItem, int sSubsequent)
    {
        SOLDIERTYPE? pSoldier;
        int sNewWoundAmt = 0;
        WorldDirections ubDirection;

        pSoldier = MercPtrs[ubPerson];   // someone is here, and they're gonna get hurt

        if (!pSoldier.bActive || !pSoldier.bInSector || pSoldier.bLife == 0)
        {
            return false;
        }

        if (pSoldier.ubMiscSoldierFlags.HasFlag(SOLDIER_MISC.HURT_BY_EXPLOSION))
        {
            // don't want to damage the guy twice
            return false;
        }

        // Direction to center of explosion
        ubDirection = SoldierControl.GetDirectionFromGridNo(sBombGridNo, pSoldier);

        // Increment attack counter...
        gTacticalStatus.ubAttackBusyCount++;
        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Incrementing Attack: Explosion dishing out damage, Count now %d", gTacticalStatus.ubAttackBusyCount));

        //        sNewWoundAmt = sWoundAmt - Math.Min(sWoundAmt, 35) * ArmourVersusExplosivesPercent(pSoldier) / 100;
        if (sNewWoundAmt < 0)
        {
            sNewWoundAmt = 0;
        }

        //        SoldierControl.EVENT_SoldierGotHit(pSoldier, usItem, sNewWoundAmt, sBreathAmt, ubDirection, (int)uiDist, ubOwner, 0, AnimationHeights.ANIM_CROUCH, sSubsequent, sBombGridNo);

        pSoldier.ubMiscSoldierFlags |= SOLDIER_MISC.HURT_BY_EXPLOSION;

        if (ubOwner != NOBODY && MercPtrs[ubOwner].bTeam == gbPlayerNum && pSoldier.bTeam != gbPlayerNum)
        {
            //            ProcessImplicationsOfPCAttack(MercPtrs[ubOwner], pSoldier, REASON_EXPLOSION);
        }
        return true;
    }

    bool DishOutGasDamage(SOLDIERTYPE? pSoldier, EXPLOSIVETYPE? pExplosive, int sSubsequent, bool fRecompileMovementCosts, int sWoundAmt, int sBreathAmt, int ubOwner)
    {
        InventorySlot bPosOfMask = NO_SLOT;

        if (!pSoldier.bActive || !pSoldier.bInSector || pSoldier.bLife == 0 || AM_A_ROBOT(pSoldier))
        {
            return fRecompileMovementCosts;
        }

        if (pExplosive.ubType == EXPLOSV.CREATUREGAS)
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER))
            {
                // unaffected by own gas effects
                return fRecompileMovementCosts;
            }
            if (sSubsequent > 0 && pSoldier.fHitByGasFlags.HasFlag(HIT_BY.CREATUREGAS))
            {
                // already affected by creature gas this turn
                return fRecompileMovementCosts;
            }
        }
        else // no gas mask help from creature attacks
             // ATE/CJC: gas stuff
        {
            if (pExplosive.ubType == EXPLOSV.TEARGAS)
            {
                if (AM_A_ROBOT(pSoldier))
                {
                    return fRecompileMovementCosts;
                }

                // ignore whether subsequent or not if hit this turn 
                if (pSoldier.fHitByGasFlags.HasFlag(HIT_BY.TEARGAS))
                {
                    // already affected by creature gas this turn
                    return fRecompileMovementCosts;
                }
            }
            else if (pExplosive.ubType == EXPLOSV.MUSTGAS)
            {
                if (AM_A_ROBOT(pSoldier))
                {
                    return fRecompileMovementCosts;
                }

                if (sSubsequent > 0 && pSoldier.fHitByGasFlags.HasFlag(HIT_BY.MUSTARDGAS))
                {
                    // already affected by creature gas this turn
                    return fRecompileMovementCosts;
                }

            }

            if (pSoldier.inv[InventorySlot.HEAD1POS].usItem == Items.GASMASK && pSoldier.inv[InventorySlot.HEAD1POS].bStatus[0] >= USABLE)
            {
                bPosOfMask = InventorySlot.HEAD1POS;
            }
            else if (pSoldier.inv[InventorySlot.HEAD2POS].usItem == Items.GASMASK && pSoldier.inv[InventorySlot.HEAD2POS].bStatus[0] >= USABLE)
            {
                bPosOfMask = InventorySlot.HEAD2POS;
            }

            if (bPosOfMask != NO_SLOT)
            {
                //                if (pSoldier.inv[bPosOfMask].bStatus[0] < GASMASK_MIN_STATUS)
                {
                    // GAS MASK reduces breath loss by its work% (it leaks if not at least 70%)
                    sBreathAmt = sBreathAmt * (100 - pSoldier.inv[bPosOfMask].bStatus[0]) / 100;
                    if (sBreathAmt > 500)
                    {
                        // if at least 500 of breath damage got through
                        // the soldier within the blast radius is gassed for at least one
                        // turn, possibly more if it's tear gas (which hangs around a while)
                        pSoldier.uiStatusFlags |= SOLDIER.GASSED;
                    }

                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC))
                    {

                        if (sWoundAmt > 1)
                        {
                            pSoldier.inv[bPosOfMask].bStatus[0] -= (int)Globals.Random.Next(4);
                            sWoundAmt = sWoundAmt * (100 - pSoldier.inv[bPosOfMask].bStatus[0]) / 100;
                        }
                        else if (sWoundAmt == 1)
                        {
                            pSoldier.inv[bPosOfMask].bStatus[0] -= (int)Globals.Random.Next(2);
                        }
                    }
                }
                //                else
                {
                    sBreathAmt = 0;
                    if (sWoundAmt > 0)
                    {
                        if (sWoundAmt == 1)
                        {
                            pSoldier.inv[bPosOfMask].bStatus[0] -= (int)Globals.Random.Next(2);
                        }
                        else
                        {
                            // use up gas mask
                            pSoldier.inv[bPosOfMask].bStatus[0] -= (int)Globals.Random.Next(4);
                        }
                    }
                    sWoundAmt = 0;
                }

            }
        }

        if (sWoundAmt != 0 || sBreathAmt != 0)
        {
            switch (pExplosive.ubType)
            {
                case EXPLOSV.CREATUREGAS:
                    pSoldier.fHitByGasFlags |= HIT_BY.CREATUREGAS;
                    break;
                case EXPLOSV.TEARGAS:
                    pSoldier.fHitByGasFlags |= HIT_BY.TEARGAS;
                    break;
                case EXPLOSV.MUSTGAS:
                    pSoldier.fHitByGasFlags |= HIT_BY.MUSTARDGAS;
                    break;
                default:
                    break;
            }
            // a gas effect, take damage directly...
            SoldierControl.SoldierTakeDamage(pSoldier, AnimationHeights.ANIM_STAND, sWoundAmt, sBreathAmt, TAKE_DAMAGE.GAS, NOBODY, NOWHERE, 0, true);
            if (pSoldier.bLife >= CONSCIOUSNESS)
            {
                //                DoMercBattleSound(pSoldier, (int)(BATTLE_SOUND.HIT1 + Globals.Random.Next(2)));
            }

            if (ubOwner != NOBODY && MercPtrs[ubOwner].bTeam == gbPlayerNum && pSoldier.bTeam != gbPlayerNum)
            {
                //                ProcessImplicationsOfPCAttack(MercPtrs[ubOwner], out pSoldier, REASON_EXPLOSION);
            }
        }
        return fRecompileMovementCosts;
    }

    bool ExpAffect(int sBombGridNo, int sGridNo, int uiDist, Items usItem, int ubOwner, int sSubsequent, out bool pfMercHit, int bLevel, int iSmokeEffectID)
    {
        int sWoundAmt = 0, sBreathAmt = 0, sNewWoundAmt = 0, sNewBreathAmt = 0, sStructDmgAmt;
        int ubPerson;
        SOLDIERTYPE? pSoldier;
        EXPLOSIVETYPE? pExplosive;
        int sX, sY;
        bool fRecompileMovementCosts = false;
        bool fSmokeEffect = false;
        bool fStunEffect = false;
        SmokeEffectType bSmokeEffectType = 0;
        bool fBlastEffect = true;
        int sNewGridNo;
        bool fBloodEffect = false;
        ITEM_POOL? pItemPoolNext;
        int uiRoll;

        //Init the variables
        sX = sY = -1;

        if (sSubsequent == BLOOD_SPREAD_EFFECT)
        {
            fSmokeEffect = false;
            fBlastEffect = false;
            fBloodEffect = true;
        }
        else
        {
            // Turn off blast effect if some types of items...
            switch (usItem)
            {
                case Items.MUSTARD_GRENADE:

                    fSmokeEffect = true;
                    bSmokeEffectType = SmokeEffectType.MUSTARDGAS_SMOKE_EFFECT;
                    fBlastEffect = false;
                    break;

                case Items.TEARGAS_GRENADE:
                case Items.GL_TEARGAS_GRENADE:
                case Items.BIG_TEAR_GAS:

                    fSmokeEffect = true;
                    bSmokeEffectType = SmokeEffectType.TEARGAS_SMOKE_EFFECT;
                    fBlastEffect = false;
                    break;

                case Items.SMOKE_GRENADE:
                case Items.GL_SMOKE_GRENADE:

                    fSmokeEffect = true;
                    bSmokeEffectType = SmokeEffectType.NORMAL_SMOKE_EFFECT;
                    fBlastEffect = false;
                    break;

                case Items.STUN_GRENADE:
                case Items.GL_STUN_GRENADE:
                    fStunEffect = true;
                    break;

                case Items.SMALL_CREATURE_GAS:
                case Items.LARGE_CREATURE_GAS:
                case Items.VERY_SMALL_CREATURE_GAS:

                    fSmokeEffect = true;
                    bSmokeEffectType = SmokeEffectType.CREATURE_SMOKE_EFFECT;
                    fBlastEffect = false;
                    break;
            }
        }


        // OK, here we:
        // Get explosive data from table
        pExplosive = Weapons.Explosive[Item[usItem].ubClassIndex];

        uiRoll = PreRandom(100);

        // Calculate wound amount
        sWoundAmt = pExplosive.ubDamage + (int)(pExplosive.ubDamage * uiRoll / 100);

        // Calculate breath amount ( if stun damage applicable )
        sBreathAmt = (pExplosive.ubStunDamage * 100) + (int)(pExplosive.ubStunDamage / 2 * 100 * uiRoll / 100);

        // ATE: Make sure guys get pissed at us!
        this.HandleBuldingDestruction(sGridNo, ubOwner);


        if (fBlastEffect)
        {
            // lower effects for distance away from center of explosion
            // If radius is 3, damage % is (100)/66/33/17
            // If radius is 5, damage % is (100)/80/60/40/20/10
            // If radius is 8, damage % is (100)/88/75/63/50/37/25/13/6

            if (pExplosive.ubRadius == 0)
            {
                // leave as is, has to be at range 0 here
            }
            else if (uiDist < pExplosive.ubRadius)
            {
                // if radius is 5, go down by 5ths ~ 20%
                sWoundAmt -= (int)(sWoundAmt * uiDist / pExplosive.ubRadius);
                sBreathAmt -= (int)(sBreathAmt * uiDist / pExplosive.ubRadius);
            }
            else
            {
                // at the edge of the explosion, do half the previous damage
                sWoundAmt = (int)(sWoundAmt / pExplosive.ubRadius / 2);
                sBreathAmt = (int)(sBreathAmt / pExplosive.ubRadius / 2);
            }

            if (sWoundAmt < 0)
            {
                sWoundAmt = 0;
            }

            if (sBreathAmt < 0)
            {
                sBreathAmt = 0;
            }

            // damage structures
            if (uiDist <= Math.Max(1, (int)(pExplosive.ubDamage / 30)))
            {
                if (Item[usItem].usItemClass.HasFlag(IC.GRENADE))
                {
                    sStructDmgAmt = sWoundAmt / 3;
                }
                else // most explosives
                {
                    sStructDmgAmt = sWoundAmt;
                }

                this.ExplosiveDamageGridNo(sGridNo, sStructDmgAmt, uiDist, out fRecompileMovementCosts, false, -1, 0, ubOwner, bLevel);

                // ATE: Look for damage to walls ONLY for next two gridnos
                sNewGridNo = NewGridNo(sGridNo, DirectionInc(WorldDirections.NORTH));

                if (GridNoOnVisibleWorldTile(sNewGridNo))
                {
                    this.ExplosiveDamageGridNo(sNewGridNo, sStructDmgAmt, uiDist, out fRecompileMovementCosts, true, -1, 0, ubOwner, bLevel);
                }

                // ATE: Look for damage to walls ONLY for next two gridnos
                sNewGridNo = NewGridNo(sGridNo, DirectionInc(WorldDirections.WEST));

                if (GridNoOnVisibleWorldTile(sNewGridNo))
                {
                    this.ExplosiveDamageGridNo(sNewGridNo, sStructDmgAmt, uiDist, out fRecompileMovementCosts, true, -1, 0, ubOwner, bLevel);
                }

            }

            // Add burn marks to ground randomly....
            if (Globals.Random.Next(50) < 15 && uiDist == 1)
            {
                //if ( !TypeRangeExistsInObjectLayer( sGridNo, FIRSTEXPLDEBRIS, SECONDEXPLDEBRIS, &usObjectIndex ) )
                //{
                //	GetTileIndexFromTypeSubIndex( SECONDEXPLDEBRIS, (int)( Globals.Random.Next( 10 ) + 1 ), &usTileIndex );
                //	AddObjectToHead( sGridNo, usTileIndex );

                //	RenderWorld.SetRenderFlags(RenderingFlags.FULL);

                //}
            }

            // NB radius can be 0 so cannot divide it by 2 here
            if (!fStunEffect && (uiDist * 2 <= pExplosive.ubRadius))
            {
                HandleItems.GetItemPool(sGridNo, out ITEM_POOL? pItemPool, bLevel);

                while (pItemPool is not null)
                {
                    pItemPoolNext = pItemPool.pNext;

                    if (ItemSubSystem.DamageItemOnGround(gWorldItems[pItemPool.iItemIndex].o, sGridNo, bLevel, (int)(sWoundAmt * 2), ubOwner))
                    {
                        // item was destroyed
                        //                        RemoveItemFromPool(sGridNo, pItemPool.iItemIndex, bLevel);
                    }
                    pItemPool = pItemPoolNext;
                }

                /*
                // Search for an explosive item in item pool
                while ( ( iWorldItem = GetItemOfClassTypeInPool( sGridNo, IC_EXPLOSV, bLevel ) ) != -1 )
                {
                    // Get usItem
                    usItem = gWorldItems[ iWorldItem ].o.usItem;

                    DamageItem

                    if ( CheckForChainReaction( usItem, gWorldItems[ iWorldItem ].o.bStatus[0], sWoundAmt, true ) )
                    {
                        RemoveItemFromPool( sGridNo, iWorldItem, bLevel );

                        // OK, Ignite this explosion!
                        IgniteExplosion( NOBODY, sX, sY, 0, sGridNo, usItem, bLevel );
                    }
                    else
                    {
                        RemoveItemFromPool( sGridNo, iWorldItem, bLevel );
                    }

                }

                // Remove any unburied items here!
                RemoveAllUnburiedItems( sGridNo, bLevel );
                */
            }
        }
        else if (fSmokeEffect)
        {
            // If tear gar, determine turns to spread.....
            if (sSubsequent == ERASE_SPREAD_EFFECT)
            {
                //                RemoveSmokeEffectFromTile(sGridNo, bLevel);
            }
            else if (sSubsequent != REDO_SPREAD_EFFECT)
            {
                //                AddSmokeEffectToTile(iSmokeEffectID, bSmokeEffectType, sGridNo, bLevel);
            }
        }
        else
        {
            // Drop blood ....
            // Get blood quantity....
            //            InternalDropBlood(sGridNo, 0, 0, (int)(Math.Max((MAXBLOODQUANTITY - (uiDist * 2)), 0)), 1);
        }

        if (sSubsequent != ERASE_SPREAD_EFFECT && sSubsequent != BLOOD_SPREAD_EFFECT)
        {
            // if an explosion effect....
            if (fBlastEffect)
            {
                // don't hurt anyone who is already dead & waiting to be removed
                if ((ubPerson = WorldManager.WhoIsThere2(sGridNo, bLevel)) != NOBODY)
                {
                    this.DamageSoldierFromBlast(ubPerson, ubOwner, sBombGridNo, sWoundAmt, sBreathAmt, uiDist, usItem, sSubsequent);
                }

                if (bLevel == 1)
                {
                    if ((ubPerson = WorldManager.WhoIsThere2(sGridNo, 0)) != NOBODY)
                    {
                        if ((sWoundAmt / 2) > 20)
                        {
                            // debris damage!
                            if ((sBreathAmt / 2) > 20)
                            {
                                this.DamageSoldierFromBlast(ubPerson, ubOwner, sBombGridNo, (int)Globals.Random.Next((sWoundAmt / 2) - 20), (int)Globals.Random.Next((sBreathAmt / 2) - 20), uiDist, usItem, sSubsequent);
                            }
                            else
                            {
                                this.DamageSoldierFromBlast(ubPerson, ubOwner, sBombGridNo, (int)Globals.Random.Next((sWoundAmt / 2) - 20), 1, uiDist, usItem, sSubsequent);
                            }

                        }

                    }
                }
            }
            else
            {
                if ((ubPerson = WorldManager.WhoIsThere2(sGridNo, bLevel)) >= NOBODY)
                {
                    pfMercHit = false;
                    return fRecompileMovementCosts;
                }

                pSoldier = MercPtrs[ubPerson];   // someone is here, and they're gonna get hurt

                fRecompileMovementCosts = this.DishOutGasDamage(pSoldier, pExplosive, sSubsequent, fRecompileMovementCosts, sWoundAmt, sBreathAmt, ubOwner);
                /*
                         if (!pSoldier.bActive || !pSoldier.bInSector || !pSoldier.bLife || AM_A_ROBOT( pSoldier ) )
                         {
                             return( fRecompileMovementCosts );
                         }

                         if ( pExplosive.ubType == EXPLOSV.CREATUREGAS )
                         {
                             if ( pSoldier.uiStatusFlags.HasFlag(SOLDIER.MONSTER ))
                             {
                                // unaffected by own gas effects
                                return( fRecompileMovementCosts );
                             }
                             if ( sSubsequent && pSoldier.fHitByGasFlags & HIT_BY.CREATUREGAS )
                             {
                                // already affected by creature gas this turn
                                return( fRecompileMovementCosts );				
                             }
                         }
                         else // no gas mask help from creature attacks
                            // ATE/CJC: gas stuff
                            {
                             int bPosOfMask = NO_SLOT;


                             if ( pExplosive.ubType == EXPLOSV.TEARGAS )
                             {
                                // ignore whether subsequent or not if hit this turn 
                                 if ( pSoldier.fHitByGasFlags & HIT_BY.TEARGAS )
                                 {
                                    // already affected by creature gas this turn
                                    return( fRecompileMovementCosts );				
                                 }
                             }
                             else if ( pExplosive.ubType == EXPLOSV.MUSTGAS )
                             {
                                 if ( sSubsequent && pSoldier.fHitByGasFlags & HIT_BY.MUSTARDGAS )
                                 {
                                    // already affected by creature gas this turn
                                    return( fRecompileMovementCosts );				
                                 }

                             }

                             if ( sSubsequent && pSoldier.fHitByGasFlags & HIT_BY.CREATUREGAS )
                             {
                                // already affected by creature gas this turn
                                return( fRecompileMovementCosts );				
                             }


                             if ( pSoldier.inv[ HEAD1POS ].usItem == GASMASK && pSoldier.inv[ HEAD1POS ].bStatus[0] >= USABLE )
                             {
                                    bPosOfMask = HEAD1POS;
                             }
                             else if ( pSoldier.inv[ HEAD2POS ].usItem == GASMASK && pSoldier.inv[ HEAD2POS ].bStatus[0] >= USABLE )
                             {
                                    bPosOfMask = HEAD2POS;
                             }

                             if ( bPosOfMask != NO_SLOT  )
                             {
                                 if ( pSoldier.inv[ bPosOfMask ].bStatus[0] < GASMASK_MIN_STATUS )
                                 {
                                     // GAS MASK reduces breath loss by its work% (it leaks if not at least 70%)
                                     sBreathAmt = ( sBreathAmt * ( 100 - pSoldier.inv[ bPosOfMask ].bStatus[0] ) ) / 100;
                                     if ( sBreathAmt > 500 )
                                     {
                                            // if at least 500 of breath damage got through
                                            // the soldier within the blast radius is gassed for at least one
                                            // turn, possibly more if it's tear gas (which hangs around a while)
                                            pSoldier.uiStatusFlags |= SOLDIER.GASSED;
                                     }

                                     if ( sWoundAmt > 1 )
                                     {
                                      pSoldier.inv[ bPosOfMask ].bStatus[0] -= (int) Globals.Random.Next( 4 );
                                        sWoundAmt = ( sWoundAmt * ( 100 -  pSoldier.inv[ bPosOfMask ].bStatus[0] ) ) / 100;
                                     }
                                     else if ( sWoundAmt == 1 )
                                     {
                                        pSoldier.inv[ bPosOfMask ].bStatus[0] -= (int) Globals.Random.Next( 2 );
                                     }
                                 }
                                 else
                                 {
                                    sBreathAmt = 0;
                                    if ( sWoundAmt > 0 )
                                    {
                                     if ( sWoundAmt == 1 )
                                     {
                                        pSoldier.inv[ bPosOfMask ].bStatus[0] -= (int) Globals.Random.Next( 2 );
                                     }
                                     else
                                     {
                                        // use up gas mask
                                        pSoldier.inv[ bPosOfMask ].bStatus[0] -= (int) Globals.Random.Next( 4 );
                                     }
                                    }
                                    sWoundAmt = 0;					
                                 }

                             }
                            }

                            if ( sWoundAmt != 0 || sBreathAmt != 0 )
                            {
                                switch( pExplosive.ubType )
                                {
                                    case EXPLOSV.CREATUREGAS:
                                        pSoldier.fHitByGasFlags |= HIT_BY.CREATUREGAS;
                                        break;
                                    case EXPLOSV.TEARGAS:
                                        pSoldier.fHitByGasFlags |= HIT_BY.TEARGAS;
                                        break;
                                    case EXPLOSV.MUSTGAS:
                                        pSoldier.fHitByGasFlags |= HIT_BY.MUSTARDGAS;
                                        break;
                                    default:
                                        break;
                                }
                                // a gas effect, take damage directly...
                                SoldierTakeDamage( pSoldier, ANIM_STAND, sWoundAmt, sBreathAmt, TAKE_DAMAGE.GAS, NOBODY, NOWHERE, 0, true );
                                if ( pSoldier.bLife >= CONSCIOUSNESS )
                                {
                                    DoMercBattleSound( pSoldier, (int)( BATTLE_SOUND.HIT1 + Globals.Random.Next( 2 ) ) );
                                }
                            }
                            */
            }

            pfMercHit = true;
        }

        pfMercHit = false;
        return fRecompileMovementCosts;
    }

    void GetRayStopInfo(int uiNewSpot, WorldDirections ubDir, int bLevel, bool fSmokeEffect, int uiCurRange, out int? piMaxRange, out int pubKeepGoing)
    {
        piMaxRange = -1;
        int ubMovementCost;
        BLOCKING Blocking, BlockingTemp;
        bool fTravelCostObs = false;
        int uiRangeReduce;
        int sNewGridNo;
        bool fBlowWindowSouth = false;
        bool fReduceRay = true;

        ubMovementCost = gubWorldMovementCosts[uiNewSpot, (int)ubDir, bLevel];

        if (TRAVELCOST.IS_TRAVELCOST_DOOR(ubMovementCost))
        {
            //            ubMovementCost = DoorTravelCost(null, uiNewSpot, ubMovementCost, false, null);
            // If we have hit a wall, STOP HERE
            if (ubMovementCost >= TRAVELCOST.BLOCKED)
            {
                fTravelCostObs = true;
            }
        }
        else
        {
            // If we have hit a wall, STOP HERE
            if (ubMovementCost == TRAVELCOST.WALL)
            {
                // We have an obstacle here..
                fTravelCostObs = true;
            }
        }


        Blocking = StructureInternals.GetBlockingStructureInfo((int)uiNewSpot, ubDir, 0, bLevel, out int bStructHeight, out STRUCTURE? pBlockingStructure, true);

        if (pBlockingStructure is not null)
        {
            if (pBlockingStructure.fFlags.HasFlag(STRUCTUREFLAGS.CAVEWALL))
            {
                // block completely!
                fTravelCostObs = true;
            }
            else if (pBlockingStructure.pDBStructureRef.pDBStructure.ubDensity <= 15)
            {
                // not stopped
                fTravelCostObs = false;
                fReduceRay = false;
            }
        }

        if (fTravelCostObs)
        {

            if (fSmokeEffect)
            {
                if (Blocking == BLOCKING.TOPRIGHT_OPEN_WINDOW || Blocking == BLOCKING.TOPLEFT_OPEN_WINDOW)
                {
                    // If open, fTravelCostObs set to false and reduce range....
                    fTravelCostObs = false;
                    // Range will be reduced below...
                }

                if (fTravelCostObs)
                {
                    // ATE: For windows, check to the west and north for a broken window, as movement costs
                    // will override there...
                    sNewGridNo = NewGridNo(uiNewSpot, DirectionInc(WorldDirections.WEST));

                    BlockingTemp = StructureInternals.GetBlockingStructureInfo(sNewGridNo, ubDir, 0, bLevel, out bStructHeight, out pBlockingStructure, true);
                    if (BlockingTemp == BLOCKING.TOPRIGHT_OPEN_WINDOW || BlockingTemp == BLOCKING.TOPLEFT_OPEN_WINDOW)
                    {
                        // If open, fTravelCostObs set to false and reduce range....
                        fTravelCostObs = false;
                        // Range will be reduced below...
                    }
                    if (pBlockingStructure is not null && pBlockingStructure.pDBStructureRef.pDBStructure.ubDensity <= 15)
                    {
                        fTravelCostObs = false;
                        fReduceRay = false;
                    }
                }

                if (fTravelCostObs)
                {
                    sNewGridNo = NewGridNo(uiNewSpot, DirectionInc(WorldDirections.NORTH));

                    BlockingTemp = StructureInternals.GetBlockingStructureInfo(sNewGridNo, ubDir, 0, bLevel, out bStructHeight, out pBlockingStructure, true);
                    if (BlockingTemp == BLOCKING.TOPRIGHT_OPEN_WINDOW || BlockingTemp == BLOCKING.TOPLEFT_OPEN_WINDOW)
                    {
                        // If open, fTravelCostObs set to false and reduce range....
                        fTravelCostObs = false;
                        // Range will be reduced below...
                    }
                    if (pBlockingStructure is not null && pBlockingStructure.pDBStructureRef.pDBStructure.ubDensity <= 15)
                    {
                        fTravelCostObs = false;
                        fReduceRay = false;
                    }
                }

            }
            else
            {
                // We are a blast effect....

                // ATE: explode windows!!!!
                if (Blocking == BLOCKING.TOPLEFT_WINDOW || Blocking == BLOCKING.TOPRIGHT_WINDOW)
                {
                    // Explode!
                    if (ubDir == WorldDirections.SOUTH || ubDir == WorldDirections.SOUTHEAST || ubDir == WorldDirections.SOUTHWEST)
                    {
                        fBlowWindowSouth = true;
                    }

                    if (pBlockingStructure != null)
                    {
                        Weapons.WindowHit(uiNewSpot, pBlockingStructure.usStructureID, fBlowWindowSouth, true);
                    }
                }

                // ATE: For windows, check to the west and north for a broken window, as movement costs
                // will override there...
                sNewGridNo = NewGridNo(uiNewSpot, DirectionInc(WorldDirections.WEST));

                BlockingTemp = StructureInternals.GetBlockingStructureInfo(sNewGridNo, ubDir, 0, bLevel, out bStructHeight, out pBlockingStructure, true);
                if (pBlockingStructure is not null && pBlockingStructure.pDBStructureRef.pDBStructure.ubDensity <= 15)
                {
                    fTravelCostObs = false;
                    fReduceRay = false;
                }

                if (BlockingTemp == BLOCKING.TOPRIGHT_WINDOW || BlockingTemp == BLOCKING.TOPLEFT_WINDOW)
                {
                    if (pBlockingStructure != null)
                    {
                        Weapons.WindowHit(sNewGridNo, pBlockingStructure.usStructureID, false, true);
                    }
                }

                sNewGridNo = NewGridNo((int)uiNewSpot, DirectionInc(WorldDirections.NORTH));
                BlockingTemp = StructureInternals.GetBlockingStructureInfo(sNewGridNo, ubDir, 0, bLevel, out bStructHeight, out pBlockingStructure, true);

                if (pBlockingStructure is not null && pBlockingStructure.pDBStructureRef.pDBStructure.ubDensity <= 15)
                {
                    fTravelCostObs = false;
                    fReduceRay = false;
                }
                if (BlockingTemp == BLOCKING.TOPRIGHT_WINDOW || BlockingTemp == BLOCKING.TOPLEFT_WINDOW)
                {
                    if (pBlockingStructure != null)
                    {
                        Weapons.WindowHit(sNewGridNo, pBlockingStructure.usStructureID, false, true);
                    }
                }
            }
        }

        // Have we hit things like furniture, etc?
        if (Blocking != BLOCKING.NOTHING_BLOCKING && !fTravelCostObs)
        {
            // ATE: Tall things should blaock all
            if (bStructHeight == 4)
            {
                pubKeepGoing = 0;
            }
            else
            {
                // If we are smoke, reduce range variably....
                if (fReduceRay)
                {
                    if (fSmokeEffect)
                    {
                        uiRangeReduce = bStructHeight switch
                        {
                            3 => 2,
                            2 => 1,
                            _ => 0,
                        };
                    }
                    else
                    {
                        uiRangeReduce = 2;
                    }

                  piMaxRange -= uiRangeReduce;
                }

                if (uiCurRange <= piMaxRange)
                {
                    pubKeepGoing = 1;
                }
                else
                {
                    pubKeepGoing = 0;
                }
            }
        }
        else
        {
            if (fTravelCostObs)
            {
                pubKeepGoing = 0;
            }
            else
            {
                pubKeepGoing = 1;
            }
        }
    }

    void SpreadEffect(int sGridNo, int ubRadius, Items usItem, int ubOwner, int fSubsequent, int bLevel, int iSmokeEffectID)
    {
        int uiNewSpot, uiTempSpot, uiBranchSpot, cnt, branchCnt;
        int? uiTempRange;
        int? ubBranchRange;
        WorldDirections ubDir, ubBranchDir;
        int ubKeepGoing;
        int sRange;
        bool fRecompileMovement = false;
        bool fSmokeEffect = false;

        switch (usItem)
        {
            case Items.MUSTARD_GRENADE:
            case Items.TEARGAS_GRENADE:
            case Items.GL_TEARGAS_GRENADE:
            case Items.BIG_TEAR_GAS:
            case Items.SMOKE_GRENADE:
            case Items.GL_SMOKE_GRENADE:
            case Items.SMALL_CREATURE_GAS:
            case Items.LARGE_CREATURE_GAS:
            case Items.VERY_SMALL_CREATURE_GAS:

                fSmokeEffect = true;
                break;
        }

        // Set values for recompile region to optimize area we need to recompile for MPs
        gsRecompileAreaTop = sGridNo / WORLD_COLS;
        gsRecompileAreaLeft = sGridNo % WORLD_COLS;
        gsRecompileAreaRight = gsRecompileAreaLeft;
        gsRecompileAreaBottom = gsRecompileAreaTop;

        // multiply range by 2 so we can correctly calculate approximately round explosion regions
        sRange = ubRadius * 2;

        // first, affect main spot
        if (this.ExpAffect(sGridNo, sGridNo, 0, usItem, ubOwner, fSubsequent, out bool fAnyMercHit, bLevel, iSmokeEffectID))
        {
            fRecompileMovement = true;
        }


        for (ubDir = WorldDirections.NORTH; ubDir <= WorldDirections.NORTHWEST; ubDir++)
        {
            uiTempSpot = sGridNo;

            uiTempRange = sRange;

            if (ubDir.HasFlag(WorldDirections.NORTHEAST))
            {
                cnt = 3;
            }
            else
            {
                cnt = 2;
            }
            while (cnt <= uiTempRange) // end of range loop
            {
                // move one tile in direction
                uiNewSpot = NewGridNo((int)uiTempSpot, DirectionInc(ubDir));

                // see if this was a different spot & if we should be able to reach
                // this spot
                if (uiNewSpot == uiTempSpot)
                {
                    ubKeepGoing = 0;
                }
                else
                {
                    // Check if struct is a tree, etc and reduce range...
                    this.GetRayStopInfo(uiNewSpot, ubDir, bLevel, fSmokeEffect, cnt, out uiTempRange, out ubKeepGoing);
                }

                if (ubKeepGoing > 0)
                {
                    uiTempSpot = uiNewSpot;

                    //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("Explosion affects %d", uiNewSpot) );
                    // ok, do what we do here...
                    if (this.ExpAffect(sGridNo, (int)uiNewSpot, cnt / 2, usItem, ubOwner, fSubsequent, out fAnyMercHit, bLevel, iSmokeEffectID))
                    {
                        fRecompileMovement = true;
                    }

                    // how far should we branch out here?
                    ubBranchRange = (int)(sRange - cnt);

                    if (ubBranchRange > 0)
                    {
                        // ok, there's a branch here. Mark where we start this branch.
                        uiBranchSpot = uiNewSpot;

                        // figure the branch direction - which is one dir clockwise
                        ubBranchDir = (WorldDirections)(int)(((int)ubDir + 1) % 8);

                        if (ubBranchDir.HasFlag(WorldDirections.NORTHEAST))
                        {
                            branchCnt = 3;
                        }
                        else
                        {
                            branchCnt = 2;
                        }

                        while (branchCnt <= ubBranchRange) // end of range loop
                        {
                            ubKeepGoing = 1;
                            uiNewSpot = NewGridNo((int)uiBranchSpot, DirectionInc(ubBranchDir));

                            if (uiNewSpot != uiBranchSpot)
                            {
                                // Check if struct is a tree, etc and reduce range...
                                this.GetRayStopInfo(uiNewSpot, ubBranchDir, bLevel, fSmokeEffect, branchCnt, out ubBranchRange, out ubKeepGoing);

                                if (ubKeepGoing > 0)
                                {
                                    // ok, do what we do here
                                    //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("Explosion affects %d", uiNewSpot) );
                                    if (this.ExpAffect(sGridNo, (int)uiNewSpot, (int)((cnt + branchCnt) / 2), usItem, ubOwner, fSubsequent, out fAnyMercHit, bLevel, iSmokeEffectID))
                                    {
                                        fRecompileMovement = true;
                                    }
                                    uiBranchSpot = uiNewSpot;
                                }
                                //else
                                {
                                    // check if it's ANY door, and if so, affect that spot so it's damaged
                                    //   if (RealDoorAt(uiNewSpot))
                                    //	 {
                                    //      ExpAffect(sGridNo,uiNewSpot,cnt,ubReason,fSubsequent);
                                    //	 }
                                    // blocked, break out of the the sub-branch loop
                                    //	 break;
                                }
                            }

                            if ((ubBranchDir & (WorldDirections)1) > 0)
                            {
                                branchCnt += 3;
                            }
                            else
                            {
                                branchCnt += 2;
                            }

                        }
                    } // end of if a branch to do

                }
                else        // at edge, or tile blocks further spread in that direction
                {
                    break;
                }

                if (ubDir.HasFlag(WorldDirections.NORTHEAST))
                {
                    cnt += 3;
                }
                else
                {
                    cnt += 2;
                }
            }

        }   // end of dir loop

        // Recompile movement costs...
        if (fRecompileMovement)
        {

            // DO wireframes as well
            ConvertGridNoToXY((int)sGridNo, out int sX, out int sY);
            // SetRecalculateWireFrameFlagRadius(sX, sY, ubRadius);
            // CalculateWorldWireFrameTiles(false);

            // RecompileLocalMovementCostsInAreaWithFlags();
            // RecompileLocalMovementCostsFromRadius(sGridNo, MAX_DISTANCE_EXPLOSIVE_CAN_DESTROY_STRUCTURES);

            // if anything has been done to change movement costs and this is a potential POW situation, check
            // paths for POWs
            if (gWorldSectorX == 13 && gWorldSectorY == MAP_ROW.I)
            {
                // DoPOWPathChecks();
            }
        }

        // do sight checks if something damaged or smoke stuff involved
        if (fRecompileMovement || fSmokeEffect)
        {
            if (gubElementsOnExplosionQueue > 0)
            {
                gfExplosionQueueMayHaveChangedSight = true;
            }
        }

        gsRecompileAreaTop = 0;
        gsRecompileAreaLeft = 0;
        gsRecompileAreaRight = 0;
        gsRecompileAreaBottom = 0;

        if (fAnyMercHit)
        {
            // reset explosion hit flag so we can damage mercs again
            for (cnt = 0; cnt < guiNumMercSlots; cnt++)
            {
                if (MercSlots[cnt] is not null)
                {
                    MercSlots[cnt].ubMiscSoldierFlags &= ~SOLDIER_MISC.HURT_BY_EXPLOSION;
                }
            }
        }

        if (fSubsequent != BLOOD_SPREAD_EFFECT)
        {
//            MakeNoise(NOBODY, sGridNo, bLevel, gpWorldLevelData[sGridNo].ubTerrainID, Explosive[Item[usItem].ubClassIndex].ubVolume, NOISE.EXPLOSION);
        }
    }

    void ToggleActionItemsByFrequency(int bFrequency)
    {
        int uiWorldBombIndex;
        OBJECTTYPE? pObj;

        // Go through all the bombs in the world, and look for remote ones
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (pObj.bDetonatorType == DetonatorType.BOMB_REMOTE)
                {
                    // Found a remote bomb, so check to see if it has the same frequency
                    if (pObj.bFrequency == bFrequency)
                    {
                        // toggle its active flag
                        if (pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                        {
                            pObj.fFlags &= ~OBJECT.DISABLED_BOMB;
                        }
                        else
                        {
                            pObj.fFlags |= OBJECT.DISABLED_BOMB;
                        }
                    }
                }
            }
        }
    }

    void TogglePressureActionItemsInGridNo(int sGridNo)
    {
        int uiWorldBombIndex;
        OBJECTTYPE? pObj;

        // Go through all the bombs in the world, and look for remote ones
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists && gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].sGridNo == sGridNo)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (pObj.bDetonatorType == DetonatorType.BOMB_PRESSURE)
                {
                    // Found a pressure item
                    // toggle its active flag
                    if (pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                    {
                        pObj.fFlags &= ~OBJECT.DISABLED_BOMB;
                    }
                    else
                    {
                        pObj.fFlags |= OBJECT.DISABLED_BOMB;
                    }
                }
            }
        }
    }


    void DelayedBillyTriggerToBlockOnExit()
    {
        if (WorldManager.WhoIsThere2(gsTempActionGridNo, 0) == NOBODY)
        {
            NPC.TriggerNPCRecord(NPCID.BILLY, 6);
        }
        else
        {
            // delay further!
//            SetCustomizableTimerCallbackAndDelay(1000, DelayedBillyTriggerToBlockOnExit, true);
        }
    }

    void BillyBlocksDoorCallback()
    {
        NPC.TriggerNPCRecord(NPCID.BILLY, 6);
    }

    bool HookerInRoom(int ubRoom)
    {
        int ubLoop;
        SOLDIERTYPE? pSoldier;

        for (ubLoop = gTacticalStatus.Team[TEAM.CIV_TEAM].bFirstID; ubLoop <= gTacticalStatus.Team[TEAM.CIV_TEAM].bLastID; ubLoop++)
        {
            pSoldier = MercPtrs[ubLoop];

            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.bLife >= OKLIFE && pSoldier.bNeutral > 0 && pSoldier.ubBodyType == SoldierBodyTypes.MINICIV)
            {
                if (RenderFun.InARoom(pSoldier.sGridNo, out int ubTempRoom) && ubTempRoom == ubRoom)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void PerformItemAction(int sGridNo, OBJECTTYPE pObj)
    {
        STRUCTURE? pStructure;

        switch (pObj.bActionValue)
        {
            case ACTION_ITEM.OPEN_DOOR:
                pStructure = StructureInternals.FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);
                if (pStructure is not null)
                {
                    if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                    {
                        // it's already open - this MIGHT be an error but probably not
                        // because we are basically just ensuring that the door is open
                    }
                    else
                    {
                        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
                        {
                            HandleDoors.HandleDoorChangeFromGridNo(null, sGridNo, false);
                        }
                        else
                        {
                            HandleDoors.HandleDoorChangeFromGridNo(null, pStructure.sBaseGridNo, false);
                        }

                        gfExplosionQueueMayHaveChangedSight = true;
                    }
                }
                else
                {
                    // error message here
                }
                break;
            case ACTION_ITEM.CLOSE_DOOR:
                pStructure = StructureInternals.FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);
                if (pStructure is not null)
                {
                    if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                    {
                        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
                        {
                            HandleDoors.HandleDoorChangeFromGridNo(null, sGridNo, false);
                        }
                        else
                        {
                            HandleDoors.HandleDoorChangeFromGridNo(null, pStructure.sBaseGridNo, false);
                        }
                        gfExplosionQueueMayHaveChangedSight = true;
                    }
                    else
                    {
                        // it's already closed - this MIGHT be an error but probably not
                        // because we are basically just ensuring that the door is closed
                    }
                }
                else
                {
                    // error message here
                }
                break;
            case ACTION_ITEM.TOGGLE_DOOR:
                pStructure = StructureInternals.FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);
                if (pStructure is not null)
                {
                    if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.BASE_TILE))
                    {
                        HandleDoors.HandleDoorChangeFromGridNo(null, sGridNo, false);
                    }
                    else
                    {
                        HandleDoors.HandleDoorChangeFromGridNo(null, pStructure.sBaseGridNo, false);
                    }
                    gfExplosionQueueMayHaveChangedSight = true;
                }
                else
                {
                    // error message here
                }
                break;
            case ACTION_ITEM.UNLOCK_DOOR:
                {
                    DOOR? pDoor;

                    pDoor = Keys.FindDoorInfoAtGridNo(sGridNo);
                    if (pDoor is not null)
                    {
                        pDoor.fLocked = false;
                    }
                }
                break;
            case ACTION_ITEM.TOGGLE_LOCK:
                {
                    DOOR? pDoor;

                    pDoor = Keys.FindDoorInfoAtGridNo(sGridNo);
                    if (pDoor is not null)
                    {
                        if (pDoor.fLocked)
                        {
                            pDoor.fLocked = false;
                        }
                        else
                        {
                            pDoor.fLocked = true;
                        }
                    }
                }
                break;
            case ACTION_ITEM.UNTRAP_DOOR:
                {
                    DOOR? pDoor;

                    pDoor = Keys.FindDoorInfoAtGridNo(sGridNo);
                    if (pDoor is not null)
                    {
                        pDoor.ubTrapLevel = 0;
                        pDoor.ubTrapID = DoorTrapTypes.NO_TRAP;
                    }
                }
                break;
            case ACTION_ITEM.SMALL_PIT:
//                Add3X3Pit(sGridNo);
//                SearchForOtherMembersWithinPitRadiusAndMakeThemFall(sGridNo, 1);
                break;
            case ACTION_ITEM.LARGE_PIT:
//                Add5X5Pit(sGridNo);
//                SearchForOtherMembersWithinPitRadiusAndMakeThemFall(sGridNo, 2);
                break;
            case ACTION_ITEM.TOGGLE_ACTION1:
                this.ToggleActionItemsByFrequency(FIRST_MAP_PLACED_FREQUENCY + 1);
                break;
            case ACTION_ITEM.TOGGLE_ACTION2:
                this.ToggleActionItemsByFrequency(FIRST_MAP_PLACED_FREQUENCY + 2);
                break;
            case ACTION_ITEM.TOGGLE_ACTION3:
                this.ToggleActionItemsByFrequency(FIRST_MAP_PLACED_FREQUENCY + 3);
                break;
            case ACTION_ITEM.TOGGLE_ACTION4:
                this.ToggleActionItemsByFrequency(FIRST_MAP_PLACED_FREQUENCY + 4);
                break;
            case ACTION_ITEM.TOGGLE_PRESSURE_ITEMS:
                this.TogglePressureActionItemsInGridNo(sGridNo);
                break;
            case ACTION_ITEM.ENTER_BROTHEL:
                // JA2Gold: Disable brothel tracking
                /*
                if ( ! (gTacticalStatus.uiFlags & INCOMBAT) )
                {
                    int		ubID;

                    ubID = WhoIsThere2( sGridNo, 0 );
                    if ( (ubID != NOBODY) && (MercPtrs[ ubID ].bTeam == gbPlayerNum) )
                    {
                        if ( MercPtrs[ ubID ].sOldGridNo == sGridNo + DirectionInc( SOUTH ) )
                        {
                            gMercProfiles[ MADAME ].bNPCData2++;

                            SetFactTrue( FACT_PLAYER_USED_BROTHEL );
                            SetFactTrue( FACT_PLAYER_PASSED_GOON );

                            // If we for any reason trigger Madame's record 34 then we don't bother to do
                            // anything else

                            // Billy always moves back on a timer so that the player has a chance to sneak
                            // someone else through

                            // Madame's quote about female mercs should therefore not be made on a timer

                            if ( gMercProfiles[ MADAME ].bNPCData2 > 2 )
                            {
                                // more than 2 entering brothel
                                TriggerNPCRecord( MADAME, 35 );
                                return;
                            }

                            if ( gMercProfiles[ MADAME ].bNPCData2 == gMercProfiles[ MADAME ].bNPCData )
                            {
                                // full # of mercs who paid have entered brothel
                                // have Billy block the way again
                                SetCustomizableTimerCallbackAndDelay( 2000, BillyBlocksDoorCallback, false );
                                //TriggerNPCRecord( BILLY, 6 );
                            }
                            else if ( gMercProfiles[ MADAME ].bNPCData2 > gMercProfiles[ MADAME ].bNPCData )
                            {
                                // more than full # of mercs who paid have entered brothel
                                // have Billy block the way again?
                                if ( CheckFact( FACT_PLAYER_FORCED_WAY_INTO_BROTHEL, 0 ) )
                                {
                                    // player already did this once!
                                    TriggerNPCRecord( MADAME, 35 );
                                    return;
                                }
                                else
                                {
                                    SetCustomizableTimerCallbackAndDelay( 2000, BillyBlocksDoorCallback, false );
                                    SetFactTrue( FACT_PLAYER_FORCED_WAY_INTO_BROTHEL );
                                    TriggerNPCRecord( MADAME, 34 );
                                }
                            }

                            if ( gMercProfiles[ MercPtrs[ ubID ].ubProfile ].bSex == FEMALE )
                            {
                                // woman walking into brothel
                                TriggerNPCRecordImmediately( MADAME, 33 );
                            }

                        }
                        else
                        {
                            // someone wants to leave the brothel
                            TriggerNPCRecord( BILLY, 5 );	
                        }

                    }

                }
                */
                break;
            case ACTION_ITEM.EXIT_BROTHEL:
                // JA2Gold: Disable brothel tracking
                /*
                if ( ! (gTacticalStatus.uiFlags & INCOMBAT) )
                {
                    int		ubID;

                    ubID = WhoIsThere2( sGridNo, 0 );
                    if ( (ubID != NOBODY) && (MercPtrs[ ubID ].bTeam == gbPlayerNum) && MercPtrs[ ubID ].sOldGridNo == sGridNo + DirectionInc( NORTH ) )
                    {
                        gMercProfiles[ MADAME ].bNPCData2--;
                        if ( gMercProfiles[ MADAME ].bNPCData2 == 0 )
                        {
                            // reset paid #
                            gMercProfiles[ MADAME ].bNPCData = 0;
                        }
                        // Billy should move back to block the door again
                        gsTempActionGridNo = sGridNo;
                        SetCustomizableTimerCallbackAndDelay( 1000, DelayedBillyTriggerToBlockOnExit, true );
                    }
                }
                */
                break;
            case ACTION_ITEM.KINGPIN_ALARM:
                //PlayJA2Sample(KLAXON_ALARM, RATE_11025, SoundVolume(MIDVOLUME, sGridNo), 5, SoundDir(sGridNo));
//                CallAvailableKingpinMenTo(sGridNo);

                gTacticalStatus.fCivGroupHostile[CIV_GROUP.KINGPIN_CIV_GROUP] = CIV_GROUP_HOSTILE;

                {
                    int ubID, ubID2;
                    bool fEnterCombat = false;

                    for (ubID = gTacticalStatus.Team[CIV_TEAM].bFirstID; ubID <= gTacticalStatus.Team[CIV_TEAM].bLastID; ubID++)
                    {
                        if (MercPtrs[ubID].bActive && MercPtrs[ubID].bInSector && MercPtrs[ubID].ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP)
                        {
                            for (ubID2 = gTacticalStatus.Team[gbPlayerNum].bFirstID; ubID2 <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubID2++)
                            {
                                if (MercPtrs[ubID].bOppList[ubID2] == SEEN_CURRENTLY)
                                {
//                                    MakeCivHostile(MercPtrs[ubID], 2);
                                    fEnterCombat = true;
                                }
                            }
                        }
                    }

                    if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
//                        EnterCombatMode(CIV_TEAM);
                    }
                }

                // now zap this object so it won't activate again
                pObj.fFlags &= ~OBJECT.DISABLED_BOMB;
                break;
            case ACTION_ITEM.SEX:
                // JA2Gold: Disable brothel sex
                /*
                if ( ! (gTacticalStatus.uiFlags & INCOMBAT) )
                {
                    int		ubID;
                    OBJECTTYPE DoorCloser;
                    int		sTeleportSpot;
                    int		sDoorSpot;
                    int		ubDirection;
                    int		ubRoom, ubOldRoom;

                    ubID = WhoIsThere2( sGridNo, 0 );
                    if ( (ubID != NOBODY) && (MercPtrs[ ubID ].bTeam == gbPlayerNum) )
                    {
                        if ( InARoom( sGridNo, &ubRoom ) && InARoom( MercPtrs[ ubID ].sOldGridNo, &ubOldRoom ) && ubOldRoom != ubRoom )
                        {
                            // also require there to be a miniskirt civ in the room
                            if ( HookerInRoom( ubRoom ) )
                            {

                                // stop the merc...
                                EVENT_StopMerc( MercPtrs[ ubID ], MercPtrs[ ubID ].sGridNo, MercPtrs[ ubID ].bDirection );

                                switch( sGridNo )
                                {
                                    case 13414:
                                        sDoorSpot = 13413;
                                        sTeleportSpot = 13413;
                                        break;
                                    case 11174:
                                        sDoorSpot = 11173;
                                        sTeleportSpot = 11173;
                                        break;
                                    case 12290:
                                        sDoorSpot = 12290;
                                        sTeleportSpot = 12291;
                                        break;

                                    default:

                                        sDoorSpot = NOWHERE;
                                        sTeleportSpot = NOWHERE;


                                }

                                if ( sDoorSpot != NOWHERE && sTeleportSpot != NOWHERE )
                                {
                                    // close the door... 
                                    DoorCloser.bActionValue = ACTION_ITEM.CLOSE_DOOR;
                                    PerformItemAction( sDoorSpot, &DoorCloser );

                                    // have sex
                                    HandleNPCDoAction( 0, NPC_ACTION.SEX, 0 );	

                                    // move the merc outside of the room again
                                    sTeleportSpot = FindGridNoFromSweetSpotWithStructData( MercPtrs[ ubID ], STANDING, sTeleportSpot, 2, &ubDirection, false );
                                    ChangeSoldierState( MercPtrs[ ubID ], STANDING, 0, true );
                                    TeleportSoldier( MercPtrs[ ubID ], sTeleportSpot, false );

                                    HandleMoraleEvent( MercPtrs[ ubID ], MORALE_SEX, gWorldSectorX, gWorldSectorY, gbWorldSectorZ );
                                    FatigueCharacter( MercPtrs[ ubID ] );
                                    FatigueCharacter( MercPtrs[ ubID ] );
                                    FatigueCharacter( MercPtrs[ ubID ] );
                                    FatigueCharacter( MercPtrs[ ubID ] );
                                    DirtyMercPanelInterface( MercPtrs[ ubID ], DIRTYLEVEL1 ); 
                                }
                            }

                        }
                        break;

                    }
                }
                */
                break;
            case ACTION_ITEM.REVEAL_ROOM:
                {
                    int ubRoom;
//                    if (InAHiddenRoom(sGridNo, out ubRoom))
//                    {
//                        RemoveRoomRoof(sGridNo, ubRoom, null);
//                    }
                }
                break;
            case ACTION_ITEM.LOCAL_ALARM:
//                MakeNoise(NOBODY, sGridNo, 0, gpWorldLevelData[sGridNo].ubTerrainID, 30, NOISE.SILENT_ALARM);
                break;
            case ACTION_ITEM.GLOBAL_ALARM:
//                CallAvailableEnemiesTo(sGridNo);
                break;
            case ACTION_ITEM.BLOODCAT_ALARM:
//                CallAvailableTeamEnemiesTo(sGridNo, CREATURE_TEAM);
                break;
            case ACTION_ITEM.KLAXON:
                //PlayJA2Sample(KLAXON_ALARM, RATE_11025, SoundVolume(MIDVOLUME, sGridNo), 5, SoundDir(sGridNo));
                break;
            case ACTION_ITEM.MUSEUM_ALARM:
                //PlayJA2Sample(KLAXON_ALARM, RATE_11025, SoundVolume(MIDVOLUME, sGridNo), 5, SoundDir(sGridNo));
//                CallEldinTo(sGridNo);
                break;
            default:
                // error message here
                break;
        }
    }

    void AddBombToQueue(int uiWorldBombIndex, uint uiTimeStamp)
    {
        if (gubElementsOnExplosionQueue == MAX_BOMB_QUEUE)
        {
            return;
        }

        gExplosionQueue[gubElementsOnExplosionQueue].uiWorldBombIndex = uiWorldBombIndex;
        gExplosionQueue[gubElementsOnExplosionQueue].uiTimeStamp = uiTimeStamp;
        gExplosionQueue[gubElementsOnExplosionQueue].fExists = 1;
        if (!gfExplosionQueueActive)
        {
            // lock UI
            guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_BEGINUILOCK;
            // disable sight
            gTacticalStatus.uiFlags |= TacticalEngineStatus.DISALLOW_SIGHT;
        }
        gubElementsOnExplosionQueue++;
        gfExplosionQueueActive = true;
    }

    void HandleExplosionQueue()
    {
        int uiIndex;
        int uiWorldBombIndex;
        uint uiCurrentTime;
        int sGridNo;
        OBJECTTYPE? pObj;
        int ubLevel;

        if (!gfExplosionQueueActive)
        {
            return;
        }

        uiCurrentTime = GetJA2Clock();
        for (uiIndex = 0; uiIndex < gubElementsOnExplosionQueue; uiIndex++)
        {
            if (gExplosionQueue[uiIndex].fExists > 0 && uiCurrentTime >= gExplosionQueue[uiIndex].uiTimeStamp)
            {
                // Set off this bomb now!

                // Preliminary assignments:
                uiWorldBombIndex = gExplosionQueue[uiIndex].uiWorldBombIndex;
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                sGridNo = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].sGridNo;
                ubLevel = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].ubLevel;

                if (pObj.usItem == Items.ACTION_ITEM && pObj.bActionValue != ACTION_ITEM.BLOW_UP)
                {
                    this.PerformItemAction(sGridNo, pObj);
                }
                else if (pObj.usBombItem == Items.TRIP_KLAXON)
                {
                    //PlayJA2Sample(KLAXON_ALARM, RATE_11025, SoundVolume(MIDVOLUME, sGridNo), 5, SoundDir(sGridNo));
                    //                    CallAvailableEnemiesTo(sGridNo);
                    //RemoveItemFromPool( sGridNo, gWorldBombs[ uiWorldBombIndex ].iItemIndex, 0 );
                }
                else if (pObj.usBombItem == Items.TRIP_FLARE)
                {
                    //                    NewLightEffect(sGridNo, LIGHT_FLARE_MARK_1);
                    //                    RemoveItemFromPool(sGridNo, gWorldBombs[uiWorldBombIndex].iItemIndex, ubLevel);
                }
                else
                {
                    gfExplosionQueueMayHaveChangedSight = true;

                    // We have to remove the item first to prevent the explosion from detonating it
                    // a second time :-)
                    //                    RemoveItemFromPool(sGridNo, gWorldBombs[uiWorldBombIndex].iItemIndex, ubLevel);

                    // make sure no one thinks there is a bomb here any more!
                    if (gpWorldLevelData[sGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.PLAYER_MINE_PRESENT))
                    {
                        //                        RemoveBlueFlag(sGridNo, ubLevel);
                    }
                    gpWorldLevelData[sGridNo].uiFlags &= ~MAPELEMENTFLAGS.ENEMY_MINE_PRESENT;

                    // BOOM!

                    // bomb objects only store the SIDE who placed the bomb! :-(
                    if (pObj.ubBombOwner > 1)
                    {
                        IgniteExplosion(pObj.ubBombOwner - 2, CenterX(sGridNo), (MAP_ROW)CenterY(sGridNo), 0, sGridNo, pObj.usBombItem, ubLevel);
                    }
                    else
                    {
                        // pre-placed
                        IgniteExplosion(NOBODY, CenterX(sGridNo), (MAP_ROW)CenterY(sGridNo), 0, sGridNo, pObj.usBombItem, ubLevel);
                    }
                }

                // Bye bye bomb
                gExplosionQueue[uiIndex].fExists = 0;
            }
        }

        // See if we can reduce the # of elements on the queue that we have recorded
        // Easier to do it at this time rather than in the loop above
        while (gubElementsOnExplosionQueue > 0 && gExplosionQueue[gubElementsOnExplosionQueue - 1].fExists == 0)
        {
            gubElementsOnExplosionQueue--;
        }

        if (gubElementsOnExplosionQueue == 0 && (gubPersonToSetOffExplosions == NOBODY || gTacticalStatus.ubAttackBusyCount == 0))
        {
            // turn off explosion queue 

            // re-enable sight
            gTacticalStatus.uiFlags &= ~TacticalEngineStatus.DISALLOW_SIGHT;

            if (gubPersonToSetOffExplosions != NOBODY && !MercPtrs[gubPersonToSetOffExplosions].uiStatusFlags.HasFlag(SOLDIER.PC))
            {
                //                FreeUpNPCFromPendingAction(MercPtrs[gubPersonToSetOffExplosions]);
            }

            if (gfExplosionQueueMayHaveChangedSight)
            {
                int ubLoop;
//                SOLDIERTYPE? pTeamSoldier;

                // set variable so we may at least have someone to resolve interrupts vs
                gubInterruptProvoker = gubPersonToSetOffExplosions;
                //                AllTeamsLookForAll(true);

                // call fov code
                ubLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID;
//                for (pTeamSoldier = MercPtrs[ubLoop]; ubLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; ubLoop++, pTeamSoldier++)
                foreach (var pTeamSoldier in MercPtrs)
                {
                    if (pTeamSoldier.bActive && pTeamSoldier.bInSector)
                    {
//                        RevealRoofsAndItems(pTeamSoldier, true, false, pTeamSoldier.bLevel, false);
                    }
                }

                gfExplosionQueueMayHaveChangedSight = false;
                gubPersonToSetOffExplosions = NOBODY;
            }

            // unlock UI
            //UnSetUIBusy( (int)gusSelectedSoldier );
            if (!gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT) || gTacticalStatus.ubCurrentTeam == gbPlayerNum)
            {
                // don't end UI lock when it's a computer turn
                guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_ENDUILOCK;
            }

            gfExplosionQueueActive = false;
        }
    }

    void DecayBombTimers()
    {
        int uiWorldBombIndex;
        uint uiTimeStamp;
        OBJECTTYPE? pObj;

        uiTimeStamp = GetJA2Clock();

        // Go through all the bombs in the world, and look for timed ones
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (pObj.bDetonatorType == DetonatorType.BOMB_TIMED && !pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                {
                    // Found a timed bomb, so decay its delay value and see if it goes off
                    pObj.bDelay--;
                    if (pObj.bDelay == 0)
                    {
                        // put this bomb on the queue
                        this.AddBombToQueue(uiWorldBombIndex, uiTimeStamp);
                        // ATE: CC black magic....
                        if (pObj.ubBombOwner > 1)
                        {
                            gubPersonToSetOffExplosions = (int)(pObj.ubBombOwner - 2);
                        }
                        else
                        {
                            gubPersonToSetOffExplosions = NOBODY;
                        }

                        if (pObj.usItem != Items.ACTION_ITEM || pObj.bActionValue == ACTION_ITEM.BLOW_UP)
                        {
                            uiTimeStamp += BOMB_QUEUE_DELAY;
                        }
                    }
                }
            }
        }
    }

    void SetOffBombsByFrequency(int ubID, int bFrequency)
    {
        int uiWorldBombIndex;
        uint uiTimeStamp;
        OBJECTTYPE? pObj;

        uiTimeStamp = GetJA2Clock();

        // Go through all the bombs in the world, and look for remote ones
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (pObj.bDetonatorType == DetonatorType.BOMB_REMOTE && !pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                {
                    // Found a remote bomb, so check to see if it has the same frequency
                    if (pObj.bFrequency == bFrequency)
                    {

                        gubPersonToSetOffExplosions = ubID;

                        // put this bomb on the queue
                        this.AddBombToQueue(uiWorldBombIndex, uiTimeStamp);
                        if (pObj.usItem != Items.ACTION_ITEM || pObj.bActionValue == ACTION_ITEM.BLOW_UP)
                        {
                            uiTimeStamp += BOMB_QUEUE_DELAY;
                        }
                    }
                }
            }
        }
    }

    void SetOffPanicBombs(int ubID, int bPanicTrigger)
    {
        // need to turn off gridnos & flags in gTacticalStatus
        gTacticalStatus.sPanicTriggerGridNo[bPanicTrigger] = NOWHERE;
        if ((gTacticalStatus.sPanicTriggerGridNo[0] == NOWHERE) &&
                    (gTacticalStatus.sPanicTriggerGridNo[1] == NOWHERE) &&
                    (gTacticalStatus.sPanicTriggerGridNo[2] == NOWHERE))
        {
            gTacticalStatus.fPanicFlags &= ~PANIC.TRIGGERS_HERE;
        }

        switch (bPanicTrigger)
        {
            case 0:
                this.SetOffBombsByFrequency(ubID, PANIC_FREQUENCY);
                gTacticalStatus.fPanicFlags &= ~PANIC.BOMBS_HERE;
                break;

            case 1:
                this.SetOffBombsByFrequency(ubID, PANIC_FREQUENCY_2);
                break;

            case 2:
                this.SetOffBombsByFrequency(ubID, PANIC_FREQUENCY_3);
                break;

            default:
                break;

        }

        if (gTacticalStatus.fPanicFlags > 0)
        {
            // find a new "closest one"
//            MakeClosestEnemyChosenOne();
        }
    }

    bool SetOffBombsInGridNo(int ubID, int sGridNo, bool fAllBombs, int bLevel)
    {
        int uiWorldBombIndex;
        uint uiTimeStamp;
        OBJECTTYPE? pObj;
        bool fFoundMine = false;

        uiTimeStamp = GetJA2Clock();

        // Go through all the bombs in the world, and look for mines at this location
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists && gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].sGridNo == sGridNo && gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].ubLevel == bLevel)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (!pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                {
                    if (fAllBombs || pObj.bDetonatorType == DetonatorType.BOMB_PRESSURE)
                    {
                        if (!fAllBombs && MercPtrs[ubID].bTeam != gbPlayerNum)
                        {
                            // ignore this unless it is a mine, etc which would have to have been placed by the
                            // player, seeing as how the others are all marked as known to the AI.
                            if (!(pObj.usItem == Items.MINE || pObj.usItem == Items.TRIP_FLARE || pObj.usItem == Items.TRIP_KLAXON))
                            {
                                continue;
                            }
                        }

                        // player and militia ignore bombs set by player
                        if (pObj.ubBombOwner > 1 && (MercPtrs[ubID].bTeam == gbPlayerNum || MercPtrs[ubID].bTeam == MILITIA_TEAM))
                        {
                            continue;
                        }

                        if (pObj.usItem == Items.SWITCH)
                        {
                            // send out a signal to detonate other bombs, rather than this which
                            // isn't a bomb but a trigger
                            this.SetOffBombsByFrequency(ubID, pObj.bFrequency);
                        }
                        else
                        {
                            gubPersonToSetOffExplosions = ubID;

                            // put this bomb on the queue
                            this.AddBombToQueue(uiWorldBombIndex, uiTimeStamp);
                            if (pObj.usItem != Items.ACTION_ITEM || pObj.bActionValue == ACTION_ITEM.BLOW_UP)
                            {
                                uiTimeStamp += BOMB_QUEUE_DELAY;
                            }

                            if (pObj.usBombItem != NOTHING && Item[pObj.usBombItem].usItemClass.HasFlag(IC.EXPLOSV))
                            {
                                fFoundMine = true;
                            }

                        }
                    }
                }
            }
        }

        return fFoundMine;
    }

    void ActivateSwitchInGridNo(int ubID, int sGridNo)
    {
        int uiWorldBombIndex;
        OBJECTTYPE? pObj;

        // Go through all the bombs in the world, and look for mines at this location
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists && gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].sGridNo == sGridNo)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;

                if (pObj.usItem == Items.SWITCH && (!pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB)) && pObj.bDetonatorType == DetonatorType.BOMB_SWITCH)
                {
                    // send out a signal to detonate other bombs, rather than this which
                    // isn't a bomb but a trigger

                    // first set attack busy count to 0 in case of a lingering a.b.c. problem...
                    gTacticalStatus.ubAttackBusyCount = 0;

                    this.SetOffBombsByFrequency(ubID, pObj.bFrequency);
                }
            }
        }
    }

    unsafe bool SaveExplosionTableToSaveGameFile(Stream hFile)
    {
        int uiExplosionCount = 0;
        int uiCnt;

        //
        //	Explosion queue Info
        //

        //Write the number of explosion queues
        files.FileWrite(hFile, gubElementsOnExplosionQueue, sizeof(int), out int uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            files.FileClose(hFile);
            return false;
        }

        //loop through and add all the explosions
        for (uiCnt = 0; uiCnt < MAX_BOMB_QUEUE; uiCnt++)
        {
            files.FileWrite(hFile, gExplosionQueue[uiCnt], sizeof(ExplosionQueueElement), out uiNumBytesWritten);
            if (uiNumBytesWritten != sizeof(ExplosionQueueElement))
            {
                files.FileClose(hFile);
                return false;
            }
        }


        //
        //	Explosion Data
        //

        //loop through and count all the active explosions
        uiExplosionCount = 0;
        for (uiCnt = 0; uiCnt < NUM_EXPLOSION_SLOTS; uiCnt++)
        {
            if (gExplosionData[uiCnt].fAllocated)
            {
                uiExplosionCount++;
            }
        }

        //Save the number of explosions
        files.FileWrite(hFile, uiExplosionCount, sizeof(int), out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            files.FileClose(hFile);
            return false;
        }



        //loop through and count all the active explosions
        for (uiCnt = 0; uiCnt < NUM_EXPLOSION_SLOTS; uiCnt++)
        {
            if (gExplosionData[uiCnt].fAllocated)
            {
                files.FileWrite(hFile, gExplosionData[uiCnt], sizeof(EXPLOSIONTYPE), out uiNumBytesWritten);
                if (uiNumBytesWritten != sizeof(EXPLOSIONTYPE))
                {
                    files.FileClose(hFile);
                    return false;
                }
            }
        }
        return true;
    }

    unsafe bool LoadExplosionTableFromSavedGameFile(Stream hFile)
    {
        int uiExplosionCount = 0;
        int uiCnt;

        //
        //	Explosion Queue
        //

        //Clear the Explosion queue
        //memset(gExplosionQueue, 0, sizeof(ExplosionQueueElement) * MAX_BOMB_QUEUE);

        //Read the number of explosions queue's
        files.FileRead(hFile, ref gubElementsOnExplosionQueue, sizeof(int), out int uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return false;
        }

        //loop through read all the active explosions fro the file
        for (uiCnt = 0; uiCnt < MAX_BOMB_QUEUE; uiCnt++)
        {
            files.FileRead(hFile, ref gExplosionQueue[uiCnt], sizeof(ExplosionQueueElement), out uiNumBytesRead);
            if (uiNumBytesRead != sizeof(ExplosionQueueElement))
            {
                return false;
            }
        }

        //
        //	Explosion Data
        //

        //Load the number of explosions
        files.FileRead(hFile, ref guiNumExplosions, sizeof(int), out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return false;
        }


        //loop through and load all the active explosions
        for (uiCnt = 0; uiCnt < guiNumExplosions; uiCnt++)
        {
            files.FileRead(hFile, ref gExplosionData[uiCnt], sizeof(EXPLOSIONTYPE), out uiNumBytesRead);
            if (uiNumBytesRead != sizeof(EXPLOSIONTYPE))
            {
                return false;
            }
            gExplosionData[uiCnt].iID = (WorldDirections)uiCnt;
            gExplosionData[uiCnt].iLightID = -1;

            GenerateExplosionFromExplosionPointer(gExplosionData[uiCnt]);
        }

        return true;
    }

    bool DoesSAMExistHere(int sSectorX, MAP_ROW sSectorY, int sSectorZ, int sGridNo)
    {
        int cnt;
        SEC sSectorNo;

        // ATE: If we are belwo, return right away...
        if (sSectorZ != 0)
        {
            return false;
        }

        sSectorNo = SECTORINFO.SECTOR(sSectorX, sSectorY);

        for (cnt = 0; cnt < NUMBER_OF_SAMS; cnt++)
        {
            // Are we i nthe same sector...
            if (pSamList[cnt] == sSectorNo)
            {
                // Are we in the same gridno?
                if (pSamGridNoAList[cnt] == sGridNo || pSamGridNoBList[cnt] == sGridNo)
                {
                    return true;
                }
            }
        }

        return false;
    }


    void UpdateAndDamageSAMIfFound(int sSectorX, MAP_ROW sSectorY, int sSectorZ, int sGridNo, int ubDamage)
    {
        int sSectorNo;

        // OK, First check if SAM exists, and if not, return
        if (!this.DoesSAMExistHere(sSectorX, sSectorY, sSectorZ, sGridNo))
        {
            return;
        }

        // Damage.....
        sSectorNo = CALCULATE_STRATEGIC_INDEX(sSectorX, sSectorY);

        if (strategicMap[sSectorNo].bSAMCondition >= ubDamage)
        {
            strategicMap[sSectorNo].bSAMCondition -= ubDamage;
        }
        else
        {
            strategicMap[sSectorNo].bSAMCondition = 0;
        }

        // SAM site may have been put out of commission...
//        UpdateAirspaceControl();

        // ATE: GRAPHICS UPDATE WILL GET DONE VIA NORMAL EXPLOSION CODE.....
    }


    void UpdateSAMDoneRepair(int sSectorX, MAP_ROW sSectorY, int sSectorZ)
    {
        int cnt;
        SEC sSectorNo;
        bool fInSector = false;
        TileIndexes usDamagedGraphic;

        // ATE: If we are below, return right away...
        if (sSectorZ != 0)
        {
            return;
        }

        if (sSectorX == gWorldSectorX && sSectorY == gWorldSectorY && sSectorZ == gbWorldSectorZ)
        {
            fInSector = true;
        }


        sSectorNo = SECTORINFO.SECTOR(sSectorX, sSectorY);

        for (cnt = 0; cnt < NUMBER_OF_SAMS; cnt++)
        {
            // Are we i nthe same sector...
            if (pSamList[cnt] == sSectorNo)
            {
                // get graphic.......
                TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.EIGHTISTRUCT, gbSAMGraphicList[cnt], out TileIndexes usGoodGraphic);

                // Damaged one ( current ) is 2 less...
                usDamagedGraphic = usGoodGraphic - 2;

                // First gridno listed is base gridno....

                // if this is loaded....
                if (fInSector)
                {
                    // Update graphic.....
                    // Remove old!
                    SaveLoadMap.ApplyMapChangesToMapTempFile(true);

                    WorldManager.RemoveStruct(pSamGridNoAList[cnt], usDamagedGraphic);
                    WorldManager.AddStructToHead(pSamGridNoAList[cnt], usGoodGraphic);

                    SaveLoadMap.ApplyMapChangesToMapTempFile(false);
                }
                else
                {
                    // We add temp changes to map not loaded....
                    // Remove old
//                    RemoveStructFromUnLoadedMapTempFile(pSamGridNoAList[cnt], usDamagedGraphic, sSectorX, sSectorY, (int)sSectorZ);
                    // Add new
//                    AddStructToUnLoadedMapTempFile(pSamGridNoAList[cnt], usGoodGraphic, sSectorX, sSectorY, (int)sSectorZ);
                }
            }
        }

        // SAM site may have been put back into working order...
//        UpdateAirspaceControl();
    }


    // loop through civ team and find
    // anybody who is an NPC and
    // see if they get angry
    void HandleBuldingDestruction(int sGridNo, int ubOwner)
    {
        if (ubOwner == NOBODY)
        {
            return;
        }

        if (MercPtrs[ubOwner].bTeam != gbPlayerNum)
        {
            return;
        }

        int cnt = gTacticalStatus.Team[CIV_TEAM].bFirstID;
        foreach (var pSoldier in MercPtrs.Skip(cnt))
        {
            if (pSoldier.bActive && pSoldier.bInSector && pSoldier.IsAlive && pSoldier.bNeutral > 0)
            {
                if (pSoldier.ubProfile != NO_PROFILE)
                {
                    // ignore if the player is fighting the enemy here and this is a good guy
                    if (gTacticalStatus.Team[ENEMY_TEAM].bMenInSector > 0
                        && gMercProfiles[pSoldier.ubProfile].ubMiscFlags3.HasFlag(PROFILE_MISC_FLAG3.GOODGUY))
                    {
                        continue;
                    }

//                    if (DoesNPCOwnBuilding(pSoldier, sGridNo))
//                    {
//                        MakeNPCGrumpyForMinorOffense(pSoldier, MercPtrs[ubOwner]);
//                    }
                }
            }
        }
    }

    int FindActiveTimedBomb()
    {
        int uiWorldBombIndex;
        uint uiTimeStamp;
        OBJECTTYPE? pObj;

        uiTimeStamp = GetJA2Clock();

        // Go through all the bombs in the world, and look for timed ones
        for (uiWorldBombIndex = 0; uiWorldBombIndex < guiNumWorldBombs; uiWorldBombIndex++)
        {
            if (gWorldBombs[uiWorldBombIndex].fExists)
            {
                pObj = gWorldItems[gWorldBombs[uiWorldBombIndex].iItemIndex].o;
                if (pObj.bDetonatorType == DetonatorType.BOMB_TIMED && !pObj.fFlags.HasFlag(OBJECT.DISABLED_BOMB))
                {
                    return gWorldBombs[uiWorldBombIndex].iItemIndex;
                }
            }
        }
        return -1;
    }

    bool ActiveTimedBombExists()
    {
        if (gfWorldLoaded)
        {
            return this.FindActiveTimedBomb() != -1;
        }
        else
        {
            return false;
        }
    }

    void RemoveAllActiveTimedBombs()
    {
        int iItemIndex;

        do
        {
            iItemIndex = this.FindActiveTimedBomb();
            if (iItemIndex != -1)
            {
                WorldItems.RemoveItemFromWorld(iItemIndex);
            }
        } while (iItemIndex != -1);

    }

    internal static void IgniteExplosion(int ubOwner, int v1, int v2, int v3, int sGridNo, Items usItem, int bLevel)
    {
        throw new NotImplementedException();
    }

    public static Dictionary<EXPLOSION_TYPES, int> ubTransKeyFrame = new()
    {
        { EXPLOSION_TYPES.NO_BLAST,      0 },
        { EXPLOSION_TYPES.BLAST_1,       17 },
        { EXPLOSION_TYPES.BLAST_2,       28 },
        { EXPLOSION_TYPES.BLAST_3,       24 },
        { EXPLOSION_TYPES.STUN_BLAST,    1 },
        { EXPLOSION_TYPES.WATER_BLAST,   1 },
        { EXPLOSION_TYPES.TARGAS_EXP,    1 },
        { EXPLOSION_TYPES.SMOKE_EXP,     1 },
        { EXPLOSION_TYPES.MUSTARD_EXP,   1 },
    };

    public static Dictionary<EXPLOSION_TYPES, int> ubDamageKeyFrame = new()
    {
        { EXPLOSION_TYPES.NO_BLAST,        0 },
        { EXPLOSION_TYPES.BLAST_1,         3 },
        { EXPLOSION_TYPES.BLAST_2,         5 },
        { EXPLOSION_TYPES.BLAST_3,         5 },
        { EXPLOSION_TYPES.STUN_BLAST,      5 },
        { EXPLOSION_TYPES.WATER_BLAST,    18 },
        { EXPLOSION_TYPES.TARGAS_EXP,     18 },
        { EXPLOSION_TYPES.SMOKE_EXP,      18 },
        { EXPLOSION_TYPES.MUSTARD_EXP,    18 },
    };


    public static SoundDefine[] uiExplosionSoundID =
    {
        SoundDefine.EXPLOSION_1,
        SoundDefine.EXPLOSION_1,
        SoundDefine.EXPLOSION_BLAST_2,  //LARGE
    	SoundDefine.EXPLOSION_BLAST_2,
        SoundDefine.EXPLOSION_1,
        SoundDefine.AIR_ESCAPING_1,
        SoundDefine.AIR_ESCAPING_1,
        SoundDefine.AIR_ESCAPING_1,
        SoundDefine.AIR_ESCAPING_1,
    };


    public static Dictionary<EXPLOSION_TYPES, string> zBlastFilenames = new()
    {
        { EXPLOSION_TYPES.NO_BLAST,   "" },
        { EXPLOSION_TYPES.BLAST_1,    "TILECACHE\\ZGRAV_D.STI" },
        { EXPLOSION_TYPES.BLAST_2,    "TILECACHE\\ZGRAV_C.STI" },
        { EXPLOSION_TYPES.BLAST_3,    "TILECACHE\\ZGRAV_B.STI" },
        { EXPLOSION_TYPES.STUN_BLAST, "TILECACHE\\shckwave.STI" },
        { EXPLOSION_TYPES.WATER_BLAST,"TILECACHE\\WAT_EXP.STI" },
        { EXPLOSION_TYPES.TARGAS_EXP, "TILECACHE\\TEAR_EXP.STI" },
        { EXPLOSION_TYPES.SMOKE_EXP,  "TILECACHE\\TEAR_EXP.STI" },
        { EXPLOSION_TYPES.MUSTARD_EXP,"TILECACHE\\MUST_EXP.STI" },
    };

    public static Dictionary<EXPLOSION_TYPES, int> sBlastSpeeds = new()
    {
        { EXPLOSION_TYPES.NO_BLAST,      0 },
        { EXPLOSION_TYPES.BLAST_1,      80 },
        { EXPLOSION_TYPES.BLAST_2,      80 },
        { EXPLOSION_TYPES.BLAST_3,      80 },
        { EXPLOSION_TYPES.STUN_BLAST,   20 },
        { EXPLOSION_TYPES.WATER_BLAST,  80 },
        { EXPLOSION_TYPES.TARGAS_EXP,   80 },
        { EXPLOSION_TYPES.SMOKE_EXP,    80 },
        { EXPLOSION_TYPES.MUSTARD_EXP,  80 },
    };
}

public struct ExplosionQueueElement
{
    public int uiWorldBombIndex;
    public uint uiTimeStamp;
    public int fExists;
}

// Explosion Data
[StructLayout(LayoutKind.Explicit)]
public unsafe struct EXPLOSION_PARAMS
{
    [FieldOffset(00)] public EXPLOSION_FLAG uiFlags;
    [FieldOffset(04)] public byte ubOwner;
    [FieldOffset(05)] public EXPLOSION_TYPES ubTypeID;
    [FieldOffset(06)] public Items usItem;
    [FieldOffset(08)] public short sX;                                       // World X ( optional )
    [FieldOffset(10)] public MAP_ROW sY;                                       // World Y ( optional )
    [FieldOffset(12)] public short sZ;                                       // World Z ( optional )
    [FieldOffset(14)] public short sGridNo;                          // World GridNo
    [FieldOffset(16)] public bool fLocate;
    [FieldOffset(17)] public byte bLevel;                                // World level
    [FieldOffset(18)] public fixed byte ubUnsed[1];
} // chad: fix sizes


[StructLayout(LayoutKind.Explicit, Size = 32)]
public unsafe struct EXPLOSIONTYPE
{
    [FieldOffset(00)] public EXPLOSION_PARAMS Params;
    [FieldOffset(18)] public bool fAllocated;
    [FieldOffset(19)] public short sCurrentFrame;
    [FieldOffset(21)] public WorldDirections iID;
    [FieldOffset(29)] public int iLightID;
    [FieldOffset(00)] public fixed sbyte ubUnsed[2];
} // chad: fix sizes


public enum EXPLOSION_TYPES : byte
{
    NO_BLAST,
    BLAST_1,
    BLAST_2,
    BLAST_3,
    STUN_BLAST,
    WATER_BLAST,
    TARGAS_EXP,
    SMOKE_EXP,
    MUSTARD_EXP,

    NUM_EXP_TYPES,
}

[Flags]
public enum EXPLOSION_FLAG : uint
{
    USEABSPOS = 0x00000001,
    DISPLAYONLY = 0x00000002,
}
