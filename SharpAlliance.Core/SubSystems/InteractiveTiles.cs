﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Screens;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems;

public class InteractiveTiles
{
    public const int MAX_INTTILE_STACK = 10;
    public const int INTTILE_DOOR_TILE_ONE = 1;
    public const int INTTILE_DOOR_OPENSPEED = 70;
    public const int INTILE_CHECK_FULL = 1;
    public const int INTILE_CHECK_SELECTIVE = 2;

    private readonly ILogger<InteractiveTiles> logger;
    private readonly GameSettings gGameSettings;
    private readonly RenderWorld renderWorld;
    private readonly TileCache tileCache;
    private readonly Globals globals;
    private readonly IsometricUtils isometricUtils;
    private readonly Overhead overhead;
    private readonly WorldStructures worldStructures;

    public InteractiveTiles(
        ILogger<InteractiveTiles> logger,
        GameSettings gameSettings,
        RenderWorld renderWorld,
        Globals globals,
        TileCache tileCache,
        IsometricUtils isometricUtils,
        Overhead overhead,
        WorldStructures worldStructures)
    {
        this.logger = logger;
        this.gGameSettings = gameSettings;
        this.renderWorld = renderWorld;
        Globals = globals;
        this.tileCache = tileCache;
        this.isometricUtils = isometricUtils;
        this.overhead = overhead;
        this.worldStructures = worldStructures;
    }

    INTERACTIVE_TILE_STACK_TYPE? gCurIntTileStack;
    bool gfCycleIntTile = false;


    CUR_INTERACTIVE_TILE gCurIntTile;
    bool gfOverIntTile = false;

    // Values to determine if we should check or not
    int gsINTOldRenderCenterX = 0;
    int gsINTOldRenderCenterY = 0;
    int gusINTOldMousePosX = 0;
    int gusINTOldMousePosY = 0;

    bool InitInteractiveTileManagement()
    {
        return (true);
    }

    void ShutdownInteractiveTileManagement()
    {
    }

    bool AddInteractiveTile(int sGridNo, LEVELNODE? pLevelNode, int uiFlags, int usType)
    {
        return (true);
    }

    bool StartInteractiveObject(int sGridNo, int usStructureID, SOLDIERTYPE? pSoldier, int ubDirection)
    {
        STRUCTURE? pStructure;

        // ATE: Patch fix: Don't allow if alreay in animation
        if (pSoldier.usAnimState == AnimationStates.OPEN_STRUCT
            || pSoldier.usAnimState == AnimationStates.OPEN_STRUCT_CROUCHED
            || pSoldier.usAnimState == AnimationStates.BEGIN_OPENSTRUCT
            || pSoldier.usAnimState == AnimationStates.BEGIN_OPENSTRUCT_CROUCHED)
        {
            return (false);
        }

        pStructure = this.worldStructures.FindStructureByID(sGridNo, usStructureID);
        if (pStructure == null)
        {
            return (false);
        }
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
        {
            // Add soldier event for opening door....
            pSoldier.ubPendingAction = MERC.OPENDOOR;
            pSoldier.uiPendingActionData1 = usStructureID;
            pSoldier.sPendingActionData2 = sGridNo;
            pSoldier.bPendingActionData3 = ubDirection;
            pSoldier.ubPendingActionAnimCount = 0;


        }
        else
        {
            // Add soldier event for opening door....
            pSoldier.ubPendingAction = MERC.OPENSTRUCT;
            pSoldier.uiPendingActionData1 = usStructureID;
            pSoldier.sPendingActionData2 = sGridNo;
            pSoldier.bPendingActionData3 = ubDirection;
            pSoldier.ubPendingActionAnimCount = 0;

        }

        return (true);
    }


    bool CalcInteractiveObjectAPs(int sGridNo, STRUCTURE? pStructure, out int psAPCost, out int psBPCost)
    {
        psAPCost = 0;
        psBPCost = 0;

        if (pStructure == null)
        {
            return (false);
        }
        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
        {
            // For doors, if open, we can safely add APs for closing
            // If closed, we do not know what to do yet...
            //if ( pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN )
            //{
            psAPCost = AP.OPEN_DOOR;
            psBPCost = AP.OPEN_DOOR;
            //}
            //else
            //{
            //	*psAPCost = 0;
            //	*psBPCost = 0;
            //}
        }
        else
        {
            psAPCost = AP.OPEN_DOOR;
            psBPCost = AP.OPEN_DOOR;
        }

        return (true);
    }


    bool InteractWithInteractiveObject(SOLDIERTYPE? pSoldier, STRUCTURE? pStructure, int ubDirection)
    {
        bool fDoor = false;

        if (pStructure == null)
        {
            return (false);
        }

        if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
        {
            fDoor = true;
        }

        InteractWithOpenableStruct(pSoldier, pStructure, ubDirection, fDoor);

        return (true);
    }


    bool SoldierHandleInteractiveObject(SOLDIERTYPE? pSoldier)
    {
        STRUCTURE? pStructure;
        int usStructureID;
        int sGridNo;


        sGridNo = pSoldier.sPendingActionData2;
        usStructureID = (int)pSoldier.uiPendingActionData1;

        // HANDLE SOLDIER ACTIONS
        pStructure = this.worldStructures.FindStructureByID(sGridNo, usStructureID);
        if (pStructure == null)
        {
            //DEBUG MSG!
            return (false);
        }

        return (HandleOpenableStruct(pSoldier, sGridNo, pStructure));
    }

    void HandleStructChangeFromGridNo(SOLDIERTYPE? pSoldier, int sGridNo)
    {
        STRUCTURE? pStructure, pNewStructure;
        int sAPCost = 0, sBPCost = 0;
        ITEM_POOL? pItemPool;
        bool fDidMissingQuote = false;

        pStructure = this.worldStructures.FindStructure(sGridNo, STRUCTUREFLAGS.OPENABLE);

        if (pStructure == null)
        {
            //# ifdef JA2TESTVERSION
            //            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_TESTVERSION, L"ERROR: Told to handle struct that does not exist at %d.", sGridNo);
            //#endif
            return;
        }

        // Do sound...
        if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
        {
            // Play Opening sound...
            PlayJA2Sample(GetStructureOpenSound(pStructure, false), RATE_11025, SoundVolume(HIGHVOLUME, sGridNo), 1, SoundDir(sGridNo));
        }
        else
        {
            // Play Opening sound...
            PlayJA2Sample((GetStructureOpenSound(pStructure, true)), RATE_11025, SoundVolume(HIGHVOLUME, sGridNo), 1, SoundDir(sGridNo));
        }

        // ATE: Don't handle switches!
        if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH)))
        {
            if (pSoldier.bTeam == this.overhead.gbPlayerNum)
            {
                if (sGridNo == Quests.BOBBYR_SHIPPING_DEST_GRIDNO
                    && StrategicMap.gWorldSectorX == Quests.BOBBYR_SHIPPING_DEST_SECTOR_X
                    && StrategicMap.gWorldSectorY == Quests.BOBBYR_SHIPPING_DEST_SECTOR_Y
                    && StrategicMap.gbWorldSectorZ == Quests.BOBBYR_SHIPPING_DEST_SECTOR_Z
                    && CheckFact(FACT.PABLOS_STOLE_FROM_LATEST_SHIPMENT, 0)
                    && !(CheckFact(FACT.PLAYER_FOUND_ITEMS_MISSING, 0)))
                {
                    SayQuoteFromNearbyMercInSector(BOBBYR_SHIPPING_DEST_GRIDNO, 3, QUOTE_STUFF_MISSING_DRASSEN);
                    fDidMissingQuote = true;
                }
            }
            else if (pSoldier.bTeam == CIV_TEAM)
            {
                if (pSoldier.ubProfile != NO_PROFILE)
                {
                    TriggerNPCWithGivenApproach(pSoldier.ubProfile, APPROACH_DONE_OPEN_STRUCTURE, false);
                }
            }


            // LOOK for item pool here...
            if (GetItemPool(sGridNo, out pItemPool, pSoldier.bLevel))
            {
                // Update visiblity....
                if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
                {
                    bool fDoHumm = true;
                    bool fDoLocators = true;

                    if (pSoldier.bTeam != gbPlayerNum)
                    {
                        fDoHumm = false;
                        fDoLocators = false;
                    }

                    // Look for ownership here....
                    if (gWorldItems[pItemPool.iItemIndex].o.usItem == OWNERSHIP)
                    {
                        fDoHumm = false;
                        TacticalCharacterDialogueWithSpecialEvent(pSoldier, 0, DIALOGUE_SPECIAL_EVENT_DO_BATTLE_SND, BATTLE_SOUND_NOTHING, 500);
                    }

                    // If now open, set visible...
                    SetItemPoolVisibilityOn(pItemPool, ANY_VISIBILITY_VALUE, fDoLocators);

                    // Display quote!
                    //TacticalCharacterDialogue( pSoldier, (int)( QUOTE_SPOTTED_SOMETHING_ONE + Random( 2 ) ) );

                    // ATE: Check now many things in pool.....
                    if (!fDidMissingQuote)
                    {
                        if (pItemPool.pNext != null)
                        {
                            if (pItemPool.pNext.pNext != null)
                            {
                                fDoHumm = false;

                                TacticalCharacterDialogueWithSpecialEvent(pSoldier, 0, DIALOGUE_SPECIAL_EVENT_DO_BATTLE_SND, BATTLE_SOUND_COOL1, 500);

                            }
                        }

                        if (fDoHumm)
                        {
                            TacticalCharacterDialogueWithSpecialEvent(pSoldier, 0, DIALOGUE_SPECIAL_EVENT_DO_BATTLE_SND, BATTLE_SOUND_HUMM, 500);
                        }
                    }
                }
                else
                {
                    SetItemPoolVisibilityHidden(pItemPool);
                }
            }
            else
            {
                if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
                {
                    TacticalCharacterDialogueWithSpecialEvent(pSoldier, 0, DIALOGUE_SPECIAL_EVENT_DO_BATTLE_SND, BATTLE_SOUND_NOTHING, 500);
                }
            }
        }

        // Deduct points!
        // CalcInteractiveObjectAPs( sGridNo, pStructure, sAPCost, sBPCost );
        // DeductPoints( pSoldier, sAPCost, sBPCost );



        pNewStructure = SwapStructureForPartner(sGridNo, pStructure);
        if (pNewStructure != null)
        {
            RecompileLocalMovementCosts(sGridNo);
            SetRenderFlags(RENDER_FLAG_FULL);
            if (pNewStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
            {
                // just turned a switch on!
                ActivateSwitchInGridNo(pSoldier.ubID, sGridNo);
            }
        }

    }



    int GetInteractiveTileCursor(int uiOldCursor, bool fConfirm)
    {
        LEVELNODE? pIntNode;
        STRUCTURE? pStructure;
        int sGridNo;

        // OK, first see if we have an in tile...
        pIntNode = GetCurInteractiveTileGridNoAndStructure(out sGridNo, out pStructure);

        if (pIntNode != null && pStructure != null)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
            {
                SetDoorString(sGridNo);

                if (fConfirm)
                {
                    return (OKHANDCURSOR_UICURSOR);
                }
                else
                {
                    return (NORMALHANDCURSOR_UICURSOR);
                }

            }
            else
            {
                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
                {
                    //wcscpy(gzIntTileLocation, gzLateLocalizedString[25]);
                    gfUIIntTileLocation = true;
                }


                if (fConfirm)
                {
                    return (OKHANDCURSOR_UICURSOR);
                }
                else
                {
                    return (NORMALHANDCURSOR_UICURSOR);
                }
            }

        }

        return (uiOldCursor);
    }

    void SetActionModeDoorCursorText()
    {
        LEVELNODE? pIntNode;
        STRUCTURE? pStructure;
        int sGridNo;

        // If we are over a merc, don't
        if (gfUIFullTargetFound)
        {
            return;
        }

        // OK, first see if we have an in tile...
        pIntNode = GetCurInteractiveTileGridNoAndStructure(out sGridNo, out pStructure);

        if (pIntNode != null && pStructure != null)
        {
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
            {
                SetDoorString(sGridNo);
            }
        }
    }


    void GetLevelNodeScreenRect(LEVELNODE? pNode, out Rectangle pRect, int sXPos, int sYPos, int sGridNo)
    {
        int sScreenX, sScreenY;
        int sOffsetX, sOffsetY;
        int sTempX_S, sTempY_S;
        ETRLEObject? pTrav;
        int usHeight, usWidth;
        TILE_ELEMENT? TileElem;

        // Get 'true' merc position
        sOffsetX = sXPos - this.renderWorld.gsRenderCenterX;
        sOffsetY = sYPos - this.renderWorld.gsRenderCenterY;

        FromCellToScreenCoordinates(sOffsetX, sOffsetY, out sTempX_S, out sTempY_S);

        if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.CACHEDANITILE))
        {
            pTrav = (this.tileCache.gpTileCache[pNode.pAniTile.sCachedTileID].pImagery.vo.pETRLEObject[pNode.pAniTile.sCurrentFrame]);
        }
        else
        {
            TileElem = (TileDefine.gTileDatabase[pNode.usIndex]);

            //Adjust for current frames and animations....
            if (TileElem.uiFlags & ANIMATED_TILE)
            {
                Debug.Assert(TileElem.pAnimData != null);
                TileElem = TileDefine.gTileDatabase[TileElem.pAnimData.pusFrames[TileElem.pAnimData.bCurrentFrame]];
            }
            else if ((pNode.uiFlags & LEVELNODE_ANIMATION))
            {
                if (pNode.sCurrentFrame != -1)
                {
                    Debug.Assert(TileElem.pAnimData != null);
                    TileElem = TileDefine.gTileDatabase[TileElem.pAnimData.pusFrames[pNode.sCurrentFrame]];
                }
            }

            pTrav = (TileElem.hTileSurface.pETRLEObject[TileElem.usRegionIndex]);
        }

        sScreenX = ((this.renderWorld.gsVIEWPORT_END_X - this.renderWorld.gsVIEWPORT_START_X) / 2) + (int)sTempX_S;
        sScreenY = ((this.renderWorld.gsVIEWPORT_END_Y - this.renderWorld.gsVIEWPORT_START_Y) / 2) + (int)sTempY_S;

        // Adjust for offset position on screen
        sScreenX -= this.renderWorld.gsRenderWorldOffsetX;
        sScreenY -= this.renderWorld.gsRenderWorldOffsetY;
        sScreenY -= Globals.gpWorldLevelData[sGridNo].sHeight;

        // Adjust based on interface level
        if (Interface.gsInterfaceLevel > 0)
        {
            sScreenY += ROOF_LEVEL_HEIGHT;
        }

        // Adjust for render height
        sScreenY += this.renderWorld.gsRenderHeight;



        usHeight = (int)pTrav.usHeight;
        usWidth = (int)pTrav.usWidth;

        // Add to start position of dest buffer
        sScreenX += (pTrav.sOffsetX - (World.WORLD_TILE_X / 2));
        sScreenY += (pTrav.sOffsetY - (World.WORLD_TILE_Y / 2));

        // Adjust y offset!
        sScreenY += (World.WORLD_TILE_Y / 2);

        pRect = new(sScreenX, sScreenY, sScreenX + usWidth, sScreenY + usHeight);
    }

    void CompileInteractiveTiles()
    {

    }


    void LogMouseOverInteractiveTile(int sGridNo)
    {
        Rectangle aRect;
        int sXMapPos, sYMapPos, sScreenX, sScreenY;
        LEVELNODE? pNode;

        // OK, for now, don't allow any interactive tiles on higher interface level!
        if (Interface.gsInterfaceLevel > 0)
        {
            return;
        }

        // Also, don't allow for mercs who are on upper level...
        if (gusSelectedSoldier != NOBODY && MercPtrs[gusSelectedSoldier].bLevel == 1)
        {
            return;
        }

        // Get World XY From gridno
        ConvertGridNoToCellXY(sGridNo, out sXMapPos, out sYMapPos);

        // Set mouse stuff
        sScreenX = gusMouseXPos;
        sScreenY = gusMouseYPos;

        pNode = Globals.gpWorldLevelData[sGridNo].pStructHead;

        while (pNode != null)
        {
            {
                GetLevelNodeScreenRect(pNode, out aRect, sXMapPos, sYMapPos, sGridNo);

                // Make sure we are always on guy if we are on same gridno
                if (IsPointInScreenRect(sScreenX, sScreenY, aRect))
                {
                    // OK refine it!
                    if (RefinePointCollisionOnStruct(sGridNo, sScreenX, sScreenY, (int)aRect.Left, (int)aRect.Bottom, pNode))
                    {
                        // Do some additional checks here!
                        if (RefineLogicOnStruct(sGridNo, pNode))
                        {

                            gCurIntTile.fFound = true;

                            // Only if we are not currently cycling....
                            if (!gfCycleIntTile)
                            {
                                // Accumulate them!
                                gCurIntTileStack.bTiles[gCurIntTileStack.bNum].pFoundNode = pNode;
                                gCurIntTileStack.bTiles[gCurIntTileStack.bNum].sFoundGridNo = sGridNo;
                                gCurIntTileStack.bNum++;


                                // Determine if it's the best one
                                if (aRect.Bottom > gCurIntTile.sHeighestScreenY)
                                {
                                    gCurIntTile.sMaxScreenY = (int)aRect.Bottom;
                                    gCurIntTile.sHeighestScreenY = gCurIntTile.sMaxScreenY;

                                    // Set it!
                                    gCurIntTile.pFoundNode = pNode;
                                    gCurIntTile.sFoundGridNo = sGridNo;

                                    // Set stack current one...
                                    gCurIntTileStack.bCur = gCurIntTileStack.bNum - 1;
                                }
                            }
                        }
                    }
                }

                pNode = pNode.pNext;
            }
        }

    }


    LEVELNODE? InternalGetCurInteractiveTile(bool fRejectItemsOnTop)
    {
        LEVELNODE? pNode = null;
        STRUCTURE? pStructure = null;

        // OK, Look for our tile!

        // Check for shift down!
        if (_KeyDown(SHIFT))
        {
            return (null);
        }


        if (gfOverIntTile)
        {
            pNode = Globals.gpWorldLevelData[gCurIntTile.sGridNo].pStructHead;

            while (pNode != null)
            {
                if (pNode.usIndex == gCurIntTile.sTileIndex)
                {
                    if (fRejectItemsOnTop)
                    {
                        // get strucuture here...
                        if (gCurIntTile.fStructure)
                        {
                            pStructure = this.worldStructures.FindStructureByID(gCurIntTile.sGridNo, gCurIntTile.usStructureID);
                            if (pStructure != null)
                            {
                                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.HASITEMONTOP))
                                {
                                    return (null);
                                }
                            }
                            else
                            {
                                return (null);
                            }
                        }
                    }

                    return (pNode);
                }

                pNode = pNode.pNext;
            }
        }

        return (null);
    }



    LEVELNODE? GetCurInteractiveTile()
    {
        return (InternalGetCurInteractiveTile(true));
    }


    LEVELNODE? GetCurInteractiveTileGridNo(out int psGridNo)
    {
        LEVELNODE? pNode;

        pNode = GetCurInteractiveTile();

        if (pNode != null)
        {
            psGridNo = gCurIntTile.sGridNo;
        }
        else
        {
            psGridNo = IsometricUtils.NOWHERE;
        }

        return (pNode);
    }



    LEVELNODE? ConditionalGetCurInteractiveTileGridNoAndStructure(out int psGridNo, out STRUCTURE? ppStructure, bool fRejectOnTopItems)
    {
        LEVELNODE? pNode;
        STRUCTURE? pStructure;

        ppStructure = null;

        pNode = InternalGetCurInteractiveTile(fRejectOnTopItems);

        if (pNode != null)
        {
            psGridNo = gCurIntTile.sGridNo;
        }
        else
        {
            psGridNo = IsometricUtils.NOWHERE;
        }

        if (pNode != null)
        {
            if (gCurIntTile.fStructure)
            {
                pStructure = this.worldStructures.FindStructureByID(gCurIntTile.sGridNo, gCurIntTile.usStructureID);
                if (pStructure == null)
                {
                    ppStructure = null;
                    return (null);
                }
                else
                {
                    ppStructure = pStructure;
                }
            }
        }

        return (pNode);
    }


    LEVELNODE? GetCurInteractiveTileGridNoAndStructure(out int psGridNo, out STRUCTURE? ppStructure)
    {
        return (ConditionalGetCurInteractiveTileGridNoAndStructure(out psGridNo, out ppStructure, true));
    }


    void BeginCurInteractiveTileCheck(int bCheckFlags)
    {
        gfOverIntTile = false;

        // OK, release our stack, stuff could be different!
        gfCycleIntTile = false;

        // Reset some highest values
        gCurIntTile.sHeighestScreenY = 0;
        gCurIntTile.fFound = false;
        gCurIntTile.ubFlags = bCheckFlags;

        // Reset stack values
        gCurIntTileStack.bNum = 0;

    }

    void EndCurInteractiveTileCheck()
    {
        CUR_INTERACTIVE_TILE? pCurIntTile;

        if (gCurIntTile.fFound)
        {
            // Set our currently cycled guy.....
            if (gfCycleIntTile)
            {
                // OK, we're over this cycled node
                pCurIntTile = (gCurIntTileStack.bTiles[gCurIntTileStack.bCur]);
            }
            else
            {
                // OK, we're over this levelnode,
                pCurIntTile = gCurIntTile;
            }

            gCurIntTile.sGridNo = pCurIntTile.sFoundGridNo;
            gCurIntTile.sTileIndex = pCurIntTile.pFoundNode.usIndex;

            if (pCurIntTile.pFoundNode.pStructureData != null)
            {
                gCurIntTile.usStructureID = pCurIntTile.pFoundNode.pStructureData.usStructureID;
                gCurIntTile.fStructure = true;
            }
            else
            {
                gCurIntTile.fStructure = false;
            }


            gfOverIntTile = true;

        }
        else
        {
            // If we are in cycle mode, end it
            if (gfCycleIntTile)
            {
                gfCycleIntTile = false;
            }
        }
    }


    bool RefineLogicOnStruct(int sGridNo, LEVELNODE? pNode)
    {
        TILE_ELEMENT? TileElem;
        STRUCTURE? pStructure;


        if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.CACHEDANITILE))
        {
            return (false);
        }


        TileElem = (TileDefine.gTileDatabase[pNode.usIndex]);

        if (gCurIntTile.ubFlags == INTILE_CHECK_SELECTIVE)
        {
            // See if we are on an interactable tile!
            // Try and get struct data from levelnode pointer
            pStructure = pNode.pStructureData;

            // If no data, quit
            if (pStructure == null)
            {
                return (false);
            }

            if (!(pStructure.fFlags.HasFlag((STRUCTUREFLAGS.OPENABLE | STRUCTUREFLAGS.HASITEMONTOP))))
            {
                return (false);
            }

            if (gusSelectedSoldier != NOBODY && MercPtrs[gusSelectedSoldier].ubBodyType == ROBOTNOWEAPON)
            {
                return (false);
            }

            // If we are a door, we need a different definition of being visible than other structs
            if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR))
            {
                if (!IsDoorVisibleAtGridNo(sGridNo))
                {
                    return (false);
                }

                // OK, For a OPENED door, addition requirements are: need to be in 'HAND CURSOR' mode...
                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN))
                {
                    //Are we in hand cursor mode?
                    if (gCurrentUIMode != HANDCURSOR_MODE && gCurrentUIMode != ACTION_MODE)
                    {
                        return (false);
                    }
                }

                // If this option is on...
                if (!gGameSettings.fOptions[TOPTION.SNAP_CURSOR_TO_DOOR])
                {
                    if (gCurrentUIMode != HANDCURSOR_MODE)
                    {
                        return (false);
                    }
                }
            }
            else
            {
                // IF we are a switch, reject in another direction...
                if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
                {
                    // Find a new gridno based on switch's orientation...
                    int sNewGridNo = IsometricUtils.NOWHERE;

                    switch (pStructure.pDBStructureRef.pDBStructure.ubWallOrientation)
                    {
                        case WallOrientation.OUTSIDE_TOP_LEFT:
                        case WallOrientation.INSIDE_TOP_LEFT:

                            // Move south...
                            sNewGridNo = this.isometricUtils.NewGridNo(sGridNo, this.isometricUtils.DirectionInc(WorldDirections.SOUTH));
                            break;

                        case WallOrientation.OUTSIDE_TOP_RIGHT:
                        case WallOrientation.INSIDE_TOP_RIGHT:

                            // Move east...
                            sNewGridNo = this.isometricUtils.NewGridNo(sGridNo, this.isometricUtils.DirectionInc(WorldDirections.EAST));
                            break;

                    }

                    if (sNewGridNo != IsometricUtils.NOWHERE)
                    {
                        // If we are hidden by a roof, reject it!
                        if (!Environment.gfBasement && IsRoofVisible2(sNewGridNo) 
                            && !(Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ITEMS)))
                        {
                            return (false);
                        }
                    }
                }
                else
                {
                    // If we are hidden by a roof, reject it!
                    if (!Environment.gfBasement && IsRoofVisible(sGridNo) && !(Globals.gTacticalStatus.uiFlags & SHOW_ALL_ITEMS))
                    {
                        return (false);
                    }
                }
            }

            // Check if it's a hidden struct and we have not revealed anything!
            if (TileElem.uiFlags & HIDDEN_TILE)
            {
                if (!IsHiddenStructureVisible(sGridNo, pNode.usIndex))
                {
                    // Return false
                    return (false);
                }
            }
        }

        return (true);
    }


    bool RefinePointCollisionOnStruct(int sGridNo, int sTestX, int sTestY, int sSrcX, int sSrcY, LEVELNODE? pNode)
    {
        TILE_ELEMENT? TileElem;

        if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.CACHEDANITILE))
        {
            //Check it!
            return (CheckVideoObjectScreenCoordinateInData(this.tileCache.gpTileCache[pNode.pAniTile.sCachedTileID].pImagery.vo, pNode.pAniTile.sCurrentFrame, (int)(sTestX - sSrcX), (int)(-1 * (sTestY - sSrcY))));

        }
        else
        {
            TileElem = (TileDefine.gTileDatabase[pNode.usIndex]);

            //Adjust for current frames and animations....
            if (TileElem.uiFlags & ANIMATED_TILE)
            {
                Debug.Assert(TileElem.pAnimData != null);
                TileElem = TileDefine.gTileDatabase[TileElem.pAnimData.pusFrames[TileElem.pAnimData.bCurrentFrame]];
            }
            else if ((pNode.uiFlags & LEVELNODE_ANIMATION))
            {
                if (pNode.sCurrentFrame != -1)
                {
                    Debug.Assert(TileElem.pAnimData != null);
                    TileElem = TileDefine.gTileDatabase[TileElem.pAnimData.pusFrames[pNode.sCurrentFrame]];
                }
            }

            //Check it!
            return (CheckVideoObjectScreenCoordinateInData(TileElem.hTileSurface, TileElem.usRegionIndex, (int)(sTestX - sSrcX), (int)(-1 * (sTestY - sSrcY))));
        }
    }


    // This function will check the video object at SrcX and SrcY for the lack of transparency
    // will return true if data found, else false
    bool CheckVideoObjectScreenCoordinateInData(HVOBJECT hSrcVObject, int usIndex, int iTestX, int iTestY)
    {
        int uiOffset;
        int usHeight, usWidth;
        int SrcPtr;
        int LineSkip;
        ETRLEObject? pTrav;
        bool fDataFound = false;
        int iTestPos, iStartPos;

        // Assertions
        Debug.Assert(hSrcVObject != null);

        // Get Offsets from Index into structure
        pTrav = (hSrcVObject.pETRLEObject[usIndex]);
        usHeight = (int)pTrav.usHeight;
        usWidth = (int)pTrav.usWidth;
        uiOffset = pTrav.uiDataOffset;

        // Calculate test position we are looking for!
        // Calculate from 0, 0 at top left!
        iTestPos = ((usHeight - iTestY) * usWidth) + iTestX;
        iStartPos = 0;
        LineSkip = usWidth;

        // SrcPtr = hSrcVObject.pPixData + uiOffset;

        return false;

        //        __asm {
        //
        //            mov esi, SrcPtr
        //
        //
        //        mov edi, iStartPos
        //
        //
        //        xor eax, eax
        //
        //
        //        xor ebx, ebx
        //
        //
        //        xor ecx, ecx
        //
        //
        //BlitDispatch:
        //
        //            mov cl, [esi]
        //
        //
        //        inc esi
        //
        //
        //        or cl, cl
        //
        //
        //        js BlitTransparent
        //
        //
        //        jz BlitDoneLine
        //
        ////BlitNonTransLoop:
        //
        //        clc
        //        rcr     cl, 1
        //
        //
        //        jnc BlitNTL2
        //
        //
        //
        //        inc esi
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //
        //
        //BlitNTL2:
        //            clc
        //            rcr     cl, 1
        //
        //
        //        jnc BlitNTL3
        //
        //
        //
        //        add esi, 2
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //
        //
        //BlitNTL3:
        //
        //            or cl, cl
        //
        //
        //        jz BlitDispatch
        //
        //
        //
        //        xor ebx, ebx
        //
        //
        //BlitNTL4:
        //
        //            add esi, 4
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //        // Check
        //        cmp edi, iTestPos
        //
        //
        //        je BlitFound
        //
        //
        //        add edi, 1
        //
        //
        //
        //        dec cl
        //
        //
        //        jnz BlitNTL4
        //
        //
        //
        //        jmp BlitDispatch
        //
        //
        //BlitTransparent:
        //
        //            and ecx, 07fH
        //            //		shl		ecx, 1
        //            add     edi, ecx
        //            jmp     BlitDispatch
        //
        //
        //    BlitDoneLine:
        //				
        //// Here check if we have passed!
        //		cmp edi, iTestPos
        //
        //
        //        jge BlitDone
        //
        //
        //
        //        dec usHeight
        //
        //
        //        jz BlitDone
        //    //		add		edi, LineSkip
        //        jmp BlitDispatch
        //
        //
        //
        //BlitFound:
        //
        //            mov fDataFound, 1
        //
        //
        //BlitDone:
        //
        //}

        //      return (fDataFound);

    }


    bool ShouldCheckForMouseDetections()
    {
        bool fOK = false;

        if (gsINTOldRenderCenterX != this.renderWorld.gsRenderCenterX
            || gsINTOldRenderCenterY != this.renderWorld.gsRenderCenterY
            || gusINTOldMousePosX != gusMouseXPos
            || gusINTOldMousePosY != gusMouseYPos)
        {
            fOK = true;
        }

        // Set old values
        gsINTOldRenderCenterX = this.renderWorld.gsRenderCenterX;
        gsINTOldRenderCenterY = this.renderWorld.gsRenderCenterY;

        gusINTOldMousePosX = gusMouseXPos;
        gusINTOldMousePosY = gusMouseYPos;

        return (fOK);
    }


    void CycleIntTileFindStack(int usMapPos)
    {
        gfCycleIntTile = true;

        // Cycle around!
        gCurIntTileStack.bCur++;

        //PLot new movement
        gfPlotNewMovement = true;

        if (gCurIntTileStack.bCur == gCurIntTileStack.bNum)
        {
            gCurIntTileStack.bCur = 0;
        }
    }

    public struct CUR_INTERACTIVE_TILE
    {
        public int sGridNo;
        public int ubFlags;
        public int sTileIndex;
        public int sMaxScreenY;
        public int sHeighestScreenY;
        public bool fFound;
        public LEVELNODE? pFoundNode;
        public int sFoundGridNo;
        public int usStructureID;
        public bool fStructure;
    }

    public class INTERACTIVE_TILE_STACK_TYPE
    {
        public int bNum;
        public CUR_INTERACTIVE_TILE[] bTiles = new CUR_INTERACTIVE_TILE[InteractiveTiles.MAX_INTTILE_STACK];
        public int bCur;
    }
}