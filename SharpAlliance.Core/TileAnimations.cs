using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class TileAnimations
{
    ANITILE? pAniTileHead = null;


    ANITILE? CreateAnimationTile(ANITILE_PARAMS pAniParams)
    {
        ANITILE? pAniNode;
        ANITILE pNewAniNode = new();
        LEVELNODE? pNode;
        int iCachedTile = -1;
        int sGridNo;
        ANI ubLevel;
        int usTileType;
        int usTileIndex;
        int sDelay;
        int sStartFrame = -1;
        ANITILEFLAGS uiFlags;
        LEVELNODE? pGivenNode;
        int sX, sZ;
        MAP_ROW sY;
        int ubTempDir;

        // Get some parameters from structure sent in...
        sGridNo = pAniParams.sGridNo;
        ubLevel = pAniParams.ubLevelID;
        usTileType = pAniParams.usTileType;
        usTileIndex = pAniParams.usTileIndex;
        sDelay = pAniParams.sDelay;
        sStartFrame = pAniParams.sStartFrame;
        uiFlags = pAniParams.uiFlags;
        pGivenNode = pAniParams.pGivenLevelNode;
        sX = pAniParams.sX;
        sY = pAniParams.sY;
        sZ = pAniParams.sZ;


        pAniNode = pAniTileHead;

        // Allocate head

        if ((uiFlags.HasFlag(ANITILEFLAGS.EXISTINGTILE)))
        {
            pNewAniNode.pLevelNode = pGivenNode;
            pNewAniNode.pLevelNode.pAniTile = pNewAniNode;
        }
        else
        {
            if ((uiFlags.HasFlag(ANITILEFLAGS.CACHEDTILE)))
            {
                iCachedTile = GetCachedTile(pAniParams.zCachedFile);

                if (iCachedTile == -1)
                {
                    return (null);
                }

                usTileIndex = iCachedTile + TILE_CACHE_START_INDEX;
            }

            // ALLOCATE NEW TILE
            switch (ubLevel)
            {
                case ANI.STRUCT_LEVEL:

                    pNode = ForceStructToTail(sGridNo, usTileIndex);
                    break;

                case ANI.SHADOW_LEVEL:

                    AddShadowToHead(sGridNo, usTileIndex);
                    pNode = gpWorldLevelData[sGridNo].pShadowHead;
                    break;

                case ANI.OBJECT_LEVEL:

                    AddObjectToHead(sGridNo, usTileIndex);
                    pNode = gpWorldLevelData[sGridNo].pObjectHead;
                    break;

                case ANI.ROOF_LEVEL:

                    AddRoofToHead(sGridNo, usTileIndex);
                    pNode = gpWorldLevelData[sGridNo].pRoofHead;
                    break;

                case ANI.ONROOF_LEVEL:

                    AddOnRoofToHead(sGridNo, usTileIndex);
                    pNode = gpWorldLevelData[sGridNo].pOnRoofHead;
                    break;

                case ANI.TOPMOST_LEVEL:

                    AddTopmostToHead(sGridNo, usTileIndex);
                    pNode = gpWorldLevelData[sGridNo].pTopmostHead;
                    break;

                default:

                    return (null);
            }

            // SET NEW TILE VALUES
            pNode.ubShadeLevel = DEFAULT_SHADE_LEVEL;
            pNode.ubNaturalShadeLevel = DEFAULT_SHADE_LEVEL;

            pNewAniNode.pLevelNode = pNode;

            if ((uiFlags.HasFlag(ANITILEFLAGS.CACHEDTILE)))
            {
                pNewAniNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.CACHEDANITILE);
                pNewAniNode.sCachedTileID = (int)iCachedTile;
                pNewAniNode.usCachedTileSubIndex = usTileType;
                pNewAniNode.pLevelNode.pAniTile = pNewAniNode;
                pNewAniNode.sRelativeX = sX;
                pNewAniNode.sRelativeY = sY;
                pNewAniNode.pLevelNode.sRelativeZ = sZ;

            }
            // Can't set relative X,Y,Z IF FLAGS ANITILE_CACHEDTILE set!
            else if ((uiFlags.HasFlag(ANITILEFLAGS.USEABSOLUTEPOS)))
            {
                pNewAniNode.pLevelNode.sRelativeX = sX;
                pNewAniNode.pLevelNode.sRelativeY = sY;
                pNewAniNode.pLevelNode.sRelativeZ = sZ;
                pNewAniNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.USEABSOLUTEPOS);
            }

        }


        switch (ubLevel)
        {
            case ANI.STRUCT_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.STRUCTURES);
                break;

            case ANI.SHADOW_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.SHADOWS);
                break;

            case ANI.OBJECT_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.OBJECTS);
                break;

            case ANI.ROOF_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ROOF);
                break;

            case ANI.ONROOF_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.ONROOF);
                break;

            case ANI.TOPMOST_LEVEL:

                RenderWorld.ResetSpecificLayerOptimizing(TILES_DYNAMIC.TOPMOST);
                break;

        }

        // SET FLAGS FOR LEVELNODE
        pNewAniNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.ANIMATION | LEVELNODEFLAGS.USEZ | LEVELNODEFLAGS.DYNAMIC);

        if ((uiFlags.HasFlag(ANITILEFLAGS.NOZBLITTER)))
        {
            pNewAniNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.NOZBLITTER;
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.ALWAYS_TRANSLUCENT)))
        {
            pNewAniNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.REVEAL;
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.USEBEST_TRANSLUCENT)))
        {
            pNewAniNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.USEBESTTRANSTYPE;
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.ANIMATE_Z)))
        {
            pNewAniNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.DYNAMICZ;
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.PAUSED)))
        {
            pNewAniNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.LASTDYNAMIC | LEVELNODEFLAGS.UPDATESAVEBUFFERONCE);
            pNewAniNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.DYNAMIC);
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.OPTIMIZEFORSMOKEEFFECT)))
        {
            pNewAniNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.NOWRITEZ;
        }


        // SET ANITILE VALUES
        pNewAniNode.ubLevelID = ubLevel;
        pNewAniNode.usTileIndex = usTileIndex;

        if ((uiFlags.HasFlag(ANITILEFLAGS.CACHEDTILE)))
        {
            pNewAniNode.usNumFrames = gpTileCache[iCachedTile].ubNumFrames;
        }
        else
        {
            Debug.Assert(gTileDatabase[usTileIndex].pAnimData != null);
            pNewAniNode.usNumFrames = gTileDatabase[usTileIndex].pAnimData.ubNumFrames;
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME)))
        {
            // Our start frame is actually a direction indicator
            ubTempDir = gOneCDirection[pAniParams.uiUserData3];
            sStartFrame = (int)sStartFrame + (pNewAniNode.usNumFrames * ubTempDir);
        }

        if ((uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME)))
        {
            // Our start frame is actually a direction indicator
            ubTempDir = gb4DirectionsFrom8[pAniParams.uiUserData3];
            sStartFrame = (int)sStartFrame + (pNewAniNode.usNumFrames * ubTempDir);
        }

        pNewAniNode.usTileType = usTileType;
        pNewAniNode.pNext = pAniNode;
        pNewAniNode.uiFlags = uiFlags;
        pNewAniNode.sDelay = sDelay;
        pNewAniNode.sCurrentFrame = sStartFrame;
        pNewAniNode.uiTimeLastUpdate = GetJA2Clock();
        pNewAniNode.sGridNo = sGridNo;

        pNewAniNode.sStartFrame = sStartFrame;

        pNewAniNode.ubKeyFrame1 = pAniParams.ubKeyFrame1;
        pNewAniNode.uiKeyFrame1Code = pAniParams.uiKeyFrame1Code;
        pNewAniNode.ubKeyFrame2 = pAniParams.ubKeyFrame2;
        pNewAniNode.uiKeyFrame2Code = pAniParams.uiKeyFrame2Code;
        pNewAniNode.uiUserData = pAniParams.uiUserData;
        pNewAniNode.ubUserData2 = pAniParams.ubUserData2;
        pNewAniNode.uiUserData3 = pAniParams.uiUserData3;


        //Set head
        pAniTileHead = pNewAniNode;

        // Set some special stuff 
        return (pNewAniNode);
    }

    // Loop throug all ani tiles and remove...
    void DeleteAniTiles()
    {
        ANITILE? pAniNode = null;
        ANITILE? pNode = null;

        // LOOP THROUGH EACH NODE
        // And call delete function...
        pAniNode = pAniTileHead;

        while (pAniNode != null)
        {
            pNode = pAniNode;
            pAniNode = pAniNode.pNext;

            DeleteAniTile(pNode);
        }
    }


    void DeleteAniTile(ANITILE? pAniTile)
    {
        ANITILE? pAniNode = null;
        ANITILE? pOldAniNode = null;
        TILE_ELEMENT? TileElem;

        pAniNode = pAniTileHead;

        while (pAniNode != null)
        {
            if (pAniNode == pAniTile)
            {
                // OK, set links
                // Check for head or tail
                if (pOldAniNode == null)
                {
                    // It's the head
                    pAniTileHead = pAniTile.pNext;
                }
                else
                {
                    pOldAniNode.pNext = pAniNode.pNext;
                }

                if (!(pAniNode.uiFlags.HasFlag(ANITILEFLAGS.EXISTINGTILE)))
                {

                    // Delete memory assosiated with item
                    switch (pAniNode.ubLevelID)
                    {
                        case ANI.STRUCT_LEVEL:

                            RemoveStructFromLevelNode(pAniNode.sGridNo, pAniNode.pLevelNode);
                            break;

                        case ANI.SHADOW_LEVEL:

                            RemoveShadowFromLevelNode(pAniNode.sGridNo, pAniNode.pLevelNode);
                            break;

                        case ANI.OBJECT_LEVEL:

                            RemoveObject(pAniNode.sGridNo, pAniNode.usTileIndex);
                            break;

                        case ANI.ROOF_LEVEL:

                            RemoveRoof(pAniNode.sGridNo, pAniNode.usTileIndex);
                            break;

                        case ANI.ONROOF_LEVEL:

                            RemoveOnRoof(pAniNode.sGridNo, pAniNode.usTileIndex);
                            break;

                        case ANI.TOPMOST_LEVEL:

                            RemoveTopmostFromLevelNode(pAniNode.sGridNo, pAniNode.pLevelNode);
                            break;

                    }

                    if ((pAniNode.uiFlags.HasFlag(ANITILEFLAGS.CACHEDTILE)))
                    {
                        RemoveCachedTile(pAniNode.sCachedTileID);
                    }

                    if (pAniNode.uiFlags.HasFlag(ANITILEFLAGS.EXPLOSION))
                    {
                        // Talk to the explosion data...
                        RemoveExplosionData(pAniNode.uiUserData3);

                        if (!gfExplosionQueueActive)
                        {
                            // turn on sighting again
                            // the explosion queue handles all this at the end of the queue
                            gTacticalStatus.uiFlags &= (~TacticalEngineStatus.DISALLOW_SIGHT);
                        }

                        // Freeup attacker from explosion
                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Reducing attacker busy count..., EXPLOSION effect gone off"));
                        ReduceAttackBusyCount((int)pAniNode.ubUserData2, false);
                    }


                    if (pAniNode.uiFlags.HasFlag(ANITILEFLAGS.RELEASE_ATTACKER_WHEN_DONE))
                    {
                        // First delete the bullet!
                        RemoveBullet(pAniNode.uiUserData3);

                        //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("@@@@@@@ Freeing up attacker - miss finished animation"));
                        FreeUpAttacker((int)pAniNode.ubAttackerMissed);
                    }

                }
                else
                {
                    TileElem = (gTileDatabase[pAniNode.usTileIndex]);

                    // OK, update existing tile usIndex....
                    Debug.Assert(TileElem.pAnimData != null);
                    pAniNode.pLevelNode.usIndex = TileElem.pAnimData.pusFrames[pAniNode.pLevelNode.sCurrentFrame];

                    // OK, set our frame data back to zero....
                    pAniNode.pLevelNode.sCurrentFrame = 0;

                    // Set some flags to write to Z / update save buffer
                    // pAniNode.pLevelNode.uiFlags |=( LEVELNODE_LASTDYNAMIC | LEVELNODE_UPDATESAVEBUFFERONCE );
                    pAniNode.pLevelNode.uiFlags &= ~(LEVELNODEFLAGS.DYNAMIC | LEVELNODEFLAGS.USEZ | LEVELNODEFLAGS.ANIMATION);

                    if (pAniNode.uiFlags.HasFlag(ANITILEFLAGS.DOOR))
                    {
                        // unset door busy!
                        DOOR_STATUS? pDoorStatus;

                        pDoorStatus = GetDoorStatus(pAniNode.sGridNo);
                        if (pDoorStatus is not null)
                        {
                            pDoorStatus.ubFlags &= ~(DOOR_BUSY);
                        }

                        if (GridNoOnScreen(pAniNode.sGridNo))
                        {
                            RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                        }

                    }
                }

                MemFree(pAniNode);
                return;
            }

            pOldAniNode = pAniNode;
            pAniNode = pAniNode.pNext;

        }


    }



    void UpdateAniTiles()
    {
        ANITILE? pAniNode = null;
        ANITILE? pNode = null;
        uint uiClock = GetJA2Clock();
        int usMaxFrames, usMinFrames;
        int ubTempDir;

        // LOOP THROUGH EACH NODE
        pAniNode = pAniTileHead;

        while (pAniNode != null)
        {
            pNode = pAniNode;
            pAniNode = pAniNode.pNext;

            if ((uiClock - pNode.uiTimeLastUpdate) > (int)pNode.sDelay && !(pNode.uiFlags.HasFlag(ANITILEFLAGS.PAUSED)))
            {
                pNode.uiTimeLastUpdate = GetJA2Clock();

                if (pNode.uiFlags.HasFlag((ANITILEFLAGS.OPTIMIZEFORSLOWMOVING)))
                {
                    pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.DYNAMIC);
                    pNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.LASTDYNAMIC);
                }
                else if (pNode.uiFlags.HasFlag((ANITILEFLAGS.OPTIMIZEFORSMOKEEFFECT)))
                {
                    //	pNode.pLevelNode.uiFlags |= LEVELNODE_DYNAMICZ;
                    ResetSpecificLayerOptimizing(TILES_DYNAMIC_STRUCTURES);
                    pNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.LASTDYNAMIC);
                    pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.DYNAMIC);
                }

                if (pNode.uiFlags.HasFlag(ANITILEFLAGS.FORWARD))
                {
                    usMaxFrames = pNode.usNumFrames;

                    if (pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME))
                    {
                        ubTempDir = gOneCDirection[pNode.uiUserData3];
                        usMaxFrames = (int)usMaxFrames + (pNode.usNumFrames * ubTempDir);
                    }

                    if (pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME))
                    {
                        ubTempDir = gb4DirectionsFrom8[pNode.uiUserData3];
                        usMaxFrames = (int)usMaxFrames + (pNode.usNumFrames * ubTempDir);
                    }

                    if ((pNode.sCurrentFrame + 1) < usMaxFrames)
                    {
                        pNode.sCurrentFrame++;
                        pNode.pLevelNode.sCurrentFrame = pNode.sCurrentFrame;

                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.EXPLOSION))
                        {
                            // Talk to the explosion data...
                            UpdateExplosionFrame(pNode.uiUserData3, pNode.sCurrentFrame);
                        }

                        // CHECK IF WE SHOULD BE DISPLAYING TRANSLUCENTLY!
                        if (pNode.sCurrentFrame == pNode.ubKeyFrame1)
                        {
                            switch (pNode.uiKeyFrame1Code)
                            {
                                case KeyFrameEnums.ANI_KEYFRAME_BEGIN_TRANSLUCENCY:

                                    pNode.pLevelNode.uiFlags |= LEVELNODEFLAGS.REVEAL;
                                    break;

                                case KeyFrameEnums.ANI_KEYFRAME_CHAIN_WATER_EXPLOSION:

                                    IgniteExplosion(pNode.ubUserData2, pNode.pLevelNode.sRelativeX, pNode.pLevelNode.sRelativeY, 0, pNode.sGridNo, (int)(pNode.uiUserData), 0);
                                    break;

                                case KeyFrameEnums.ANI_KEYFRAME_DO_SOUND:

                                    // PlayJA2Sample(pNode.uiUserData, RATE_11025, SoundVolume(MIDVOLUME, (int)pNode.uiUserData3), 1, SoundDir((int)pNode.uiUserData3));
                                    break;
                            }

                        }

                        // CHECK IF WE SHOULD BE DISPLAYING TRANSLUCENTLY!
                        if (pNode.sCurrentFrame == pNode.ubKeyFrame2)
                        {
                            EXPLOSV ubExpType;

                            switch (pNode.uiKeyFrame2Code)
                            {
                                case KeyFrameEnums.ANI_KEYFRAME_BEGIN_DAMAGE:

                                    ubExpType = Explosive[Item[(Items)pNode.uiUserData].ubClassIndex].ubType;

                                    if (ubExpType == EXPLOSV.TEARGAS || ubExpType == EXPLOSV.MUSTGAS ||
                                         ubExpType == EXPLOSV.SMOKE)
                                    {
                                        // Do sound....
                                        // PlayJA2Sample( AIR_ESCAPING_1, RATE_11025, SoundVolume( HIGHVOLUME, pNode.sGridNo ), 1, SoundDir( pNode.sGridNo ) );			
                                        NewSmokeEffect(pNode.sGridNo, (int)pNode.uiUserData, gExplosionData[pNode.uiUserData3].Params.bLevel, (int)pNode.ubUserData2);
                                    }
                                    else
                                    {
                                        SpreadEffect(pNode.sGridNo, Explosive[Item[(Items)pNode.uiUserData].ubClassIndex].ubRadius, (int)pNode.uiUserData, (int)pNode.ubUserData2, false, gExplosionData[pNode.uiUserData3].Params.bLevel, -1);
                                    }
                                    // Forfait any other animations this frame....
                                    return;
                            }

                        }

                    }
                    else
                    {
                        // We are done!
                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.LOOPING))
                        {
                            pNode.sCurrentFrame = pNode.sStartFrame;

                            if ((pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME)))
                            {
                                // Our start frame is actually a direction indicator
                                ubTempDir = gOneCDirection[pNode.uiUserData3];
                                pNode.sCurrentFrame = (int)(pNode.usNumFrames * ubTempDir);
                            }

                            if ((pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME)))
                            {
                                // Our start frame is actually a direction indicator
                                ubTempDir = gb4DirectionsFrom8[pNode.uiUserData3];
                                pNode.sCurrentFrame = (int)(pNode.usNumFrames * ubTempDir);
                            }

                        }
                        else if (pNode.uiFlags.HasFlag(ANITILEFLAGS.REVERSE_LOOPING))
                        {
                            // Turn off backwards flag
                            pNode.uiFlags &= (~ANITILEFLAGS.FORWARD);

                            // Turn onn forwards flag
                            pNode.uiFlags |= ANITILEFLAGS.BACKWARD;
                        }
                        else
                        {
                            // Delete from world!
                            DeleteAniTile(pNode);

                            // Turn back on redunency checks!
                            gTacticalStatus.uiFlags &= (~NOHIDE_REDUNDENCY);

                            return;
                        }
                    }
                }

                if (pNode.uiFlags.HasFlag(ANITILEFLAGS.BACKWARD))
                {
                    if (pNode.uiFlags.HasFlag(ANITILEFLAGS.ERASEITEMFROMSAVEBUFFFER))
                    {
                        // ATE: Check if bounding box is on the screen...
                        if (pNode.bFrameCountAfterStart == 0)
                        {
                            pNode.bFrameCountAfterStart = 1;
                            pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.DYNAMIC);

                            // Dangerous here, since we may not even be on the screen...
                            RenderWorld.SetRenderFlags(RenderingFlags.FULL);

                            continue;
                        }
                    }

                    usMinFrames = 0;

                    if (pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME))
                    {
                        ubTempDir = gOneCDirection[pNode.uiUserData3];
                        usMinFrames = (pNode.usNumFrames * ubTempDir);
                    }

                    if (pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME))
                    {
                        ubTempDir = gb4DirectionsFrom8[pNode.uiUserData3];
                        usMinFrames = (pNode.usNumFrames * ubTempDir);
                    }

                    if ((pNode.sCurrentFrame - 1) >= usMinFrames)
                    {
                        pNode.sCurrentFrame--;
                        pNode.pLevelNode.sCurrentFrame = pNode.sCurrentFrame;

                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.EXPLOSION))
                        {
                            // Talk to the explosion data...
                            UpdateExplosionFrame(pNode.uiUserData3, pNode.sCurrentFrame);
                        }

                    }
                    else
                    {
                        // We are done!
                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.PAUSE_AFTER_LOOP))
                        {
                            // Turn off backwards flag
                            pNode.uiFlags &= (~ANITILEFLAGS.BACKWARD);

                            // Pause
                            pNode.uiFlags |= ANITILEFLAGS.PAUSED;

                        }
                        else if (pNode.uiFlags.HasFlag(ANITILEFLAGS.LOOPING))
                        {
                            pNode.sCurrentFrame = pNode.sStartFrame;

                            if ((pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME)))
                            {
                                // Our start frame is actually a direction indicator
                                ubTempDir = gOneCDirection[pNode.uiUserData3];
                                pNode.sCurrentFrame = (int)(pNode.usNumFrames * ubTempDir);
                            }
                            if ((pNode.uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME)))
                            {
                                // Our start frame is actually a direction indicator
                                ubTempDir = gb4DirectionsFrom8[pNode.uiUserData3];
                                pNode.sCurrentFrame = (int)(pNode.usNumFrames * ubTempDir);
                            }

                        }
                        else if (pNode.uiFlags.HasFlag(ANITILEFLAGS.REVERSE_LOOPING))
                        {
                            // Turn off backwards flag
                            pNode.uiFlags &= (~ANITILEFLAGS.BACKWARD);

                            // Turn onn forwards flag
                            pNode.uiFlags |= ANITILEFLAGS.FORWARD;
                        }
                        else
                        {
                            // Delete from world!
                            DeleteAniTile(pNode);

                            return;
                        }

                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.ERASEITEMFROMSAVEBUFFFER))
                        {
                            // ATE: Check if bounding box is on the screen...
                            pNode.bFrameCountAfterStart = 0;
                            //pNode.pLevelNode.uiFlags |= LEVELNODE_UPDATESAVEBUFFERONCE;

                            // Dangerous here, since we may not even be on the screen...
                            RenderWorld.SetRenderFlags(RenderingFlags.FULL);

                        }

                    }

                }

            }
            else
            {
                if (pNode.uiFlags.HasFlag(ANITILEFLAGS.OPTIMIZEFORSLOWMOVING))
                {
                    // ONLY TURN OFF IF PAUSED...
                    if ((pNode.uiFlags.HasFlag(ANITILEFLAGS.ERASEITEMFROMSAVEBUFFFER)))
                    {
                        if (pNode.uiFlags.HasFlag(ANITILEFLAGS.PAUSED))
                        {
                            if (pNode.pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.DYNAMIC))
                            {
                                pNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.DYNAMIC);
                                pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.LASTDYNAMIC);
                                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                            }
                        }
                    }
                    else
                    {
                        pNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.DYNAMIC);
                        pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.LASTDYNAMIC);
                    }
                }
                else if (pNode.uiFlags.HasFlag(ANITILEFLAGS.OPTIMIZEFORSMOKEEFFECT))
                {
                    pNode.pLevelNode.uiFlags |= (LEVELNODEFLAGS.LASTDYNAMIC);
                    pNode.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.DYNAMIC);
                }

            }

        }

    }

    void SetAniTileFrame(ANITILE? pAniTile, int sFrame)
    {
        int ubTempDir;
        int sStartFrame = 0;

        if ((pAniTile.uiFlags.HasFlag(ANITILEFLAGS.USE_DIRECTION_FOR_START_FRAME)))
        {
            // Our start frame is actually a direction indicator
            ubTempDir = gOneCDirection[pAniTile.uiUserData3];
            sStartFrame = (int)sFrame + (pAniTile.usNumFrames * ubTempDir);
        }

        if ((pAniTile.uiFlags.HasFlag(ANITILEFLAGS.USE_4DIRECTION_FOR_START_FRAME)))
        {
            // Our start frame is actually a direction indicator
            ubTempDir = gb4DirectionsFrom8[pAniTile.uiUserData3];
            sStartFrame = (int)sFrame + (pAniTile.usNumFrames * ubTempDir);
        }

        pAniTile.sCurrentFrame = sStartFrame;

    }


    ANITILE? GetCachedAniTileOfType(int sGridNo, ANI ubLevelID, ANITILEFLAGS uiFlags)
    {
        LEVELNODE? pNode = null;

        switch (ubLevelID)
        {
            case ANI.STRUCT_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pStructHead;
                break;

            case ANI.SHADOW_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pShadowHead;
                break;

            case ANI.OBJECT_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pObjectHead;
                break;

            case ANI.ROOF_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pRoofHead;
                break;

            case ANI.ONROOF_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pOnRoofHead;
                break;

            case ANI.TOPMOST_LEVEL:

                pNode = gpWorldLevelData[sGridNo].pTopmostHead;
                break;

            default:

                return (null);
        }

        while (pNode != null)
        {
            if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.CACHEDANITILE))
            {
                if (pNode.pAniTile.uiFlags.HasFlag(uiFlags))
                {
                    return (pNode.pAniTile);
                }

            }

            pNode = pNode.pNext;
        }

        return (null);
    }


    void HideAniTile(ANITILE? pAniTile, bool fHide)
    {
        if (fHide)
        {
            pAniTile.pLevelNode.uiFlags |= LEVELNODEFLAGS.HIDDEN;
        }
        else
        {
            pAniTile.pLevelNode.uiFlags &= (~LEVELNODEFLAGS.HIDDEN);
        }
    }

    void PauseAniTile(ANITILE? pAniTile, bool fPause)
    {
        if (fPause)
        {
            pAniTile.uiFlags |= ANITILEFLAGS.PAUSED;
        }
        else
        {
            pAniTile.uiFlags &= (~ANITILEFLAGS.PAUSED);
        }
    }


    void PauseAllAniTilesOfType(ANITILEFLAGS uiType, bool fPause)
    {
        ANITILE? pAniNode = null;
        ANITILE? pNode = null;

        // LOOP THROUGH EACH NODE
        pAniNode = pAniTileHead;

        while (pAniNode != null)
        {
            pNode = pAniNode;
            pAniNode = pAniNode.pNext;

            if (pNode.uiFlags.HasFlag(uiType))
            {
                PauseAniTile(pNode, fPause);
            }

        }
    }
}

public enum KeyFrameEnums
{
    ANI_KEYFRAME_NO_CODE,
    ANI_KEYFRAME_BEGIN_TRANSLUCENCY,
    ANI_KEYFRAME_BEGIN_DAMAGE,
    ANI_KEYFRAME_CHAIN_WATER_EXPLOSION,
    ANI_KEYFRAME_DO_SOUND,
}
