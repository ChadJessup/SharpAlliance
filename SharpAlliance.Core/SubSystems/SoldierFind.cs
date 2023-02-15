using System;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems;

public class SoldierFind
{
    // This value is used to keep a small static array of uBID's which are stacked
    public const int MAX_STACKED_MERCS = 10;
    static int fCountdown = 0;

    private readonly ILogger<SoldierFind> logger;
    private readonly GameSettings gGameSettings;
    private readonly RenderWorld renderWorld;
    private readonly Overhead overhead;
    private readonly Globals globals;

    public SoldierFind(
        ILogger<SoldierFind> logger,
        GameSettings gGameSettings,
        RenderWorld renderWorld,
        Overhead overhead,
        Globals globals)
    {
        this.logger = logger;
        this.gGameSettings = gGameSettings;
        this.renderWorld = renderWorld;
        this.overhead = overhead;
        this.globals = globals;
    }

    public static int[] gScrollSlideInertiaDirection = new int[(int)WorldDirections.NUM_WORLD_DIRECTIONS]
    {
        3, 0, 0, 0, 0, 0, 3, 3,
    };

    SOLDIER_STACK_TYPE gSoldierStack;
    bool gfHandleStack = false;

    // extern bool gUIActionModeChangeDueToMouseOver;
    // extern int guiUITargetSoldierId;

    bool FindSoldierFromMouse(out int pusSoldierIndex, out int pMercFlags)
    {
        int sMapPos;

        pMercFlags = 0;

        if (GetMouseMapPos(sMapPos))
        {
            if (FindSoldier(sMapPos, out pusSoldierIndex, out pMercFlags, FINDSOLDIERSAMELEVEL(Interface.gsInterfaceLevel)))
            {
                return (true);
            }
        }

        pusSoldierIndex = -1;
        pMercFlags = -1;

        return (false);
    }

    public static FIND_SOLDIER FINDSOLDIERSAMELEVEL(int level)
        => (((FIND_SOLDIER.FULL | FIND_SOLDIER.SAMELEVEL) | (FIND_SOLDIER)(level << 16)));

    public static FIND_SOLDIER FINDSOLDIERSELECTIVESAMELEVEL(int level)
        => (((FIND_SOLDIER.SELECTIVE | FIND_SOLDIER.SAMELEVEL) | (FIND_SOLDIER)(level << 16)));

    bool SelectiveFindSoldierFromMouse(out int pusSoldierIndex, out int pMercFlags)
    {
        int sMapPos;

        pMercFlags = 0;

        if (GetMouseMapPos(sMapPos))
        {
            if (FindSoldier(sMapPos, out pusSoldierIndex, out pMercFlags, FINDSOLDIERSAMELEVEL(Interface.gsInterfaceLevel)))
            {
                return (true);
            }
        }

        pusSoldierIndex = -1;
        pMercFlags = -1;

        return (false);
    }


    int GetSoldierFindFlags(int ubID)
    {
        int MercFlags = 0;
        SOLDIERTYPE? pSoldier;

        // Get pSoldier!
        pSoldier = MercPtrs[ubID];

        // FInd out and set flags
        if (ubID == gusSelectedSoldier)
        {
            MercFlags |= SELECTED_MERC;
        }
        if (ubID >= this.overhead.gTacticalStatus.Team[this.overhead.gbPlayerNum].bFirstID && ubID <= this.overhead.gTacticalStatus.Team[this.overhead.gbPlayerNum].bLastID)
        {
            if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) && !GetNumberInVehicle(pSoldier.bVehicleID))
            {
                // Don't do anything!
            }
            else
            {
                // It's our own merc
                MercFlags |= OWNED_MERC;

                if (pSoldier.bAssignment < ON_DUTY)
                {
                    MercFlags |= ONDUTY_MERC;
                }
            }
        }
        else
        {
            // Check the side, etc
            if (!pSoldier.bNeutral && (pSoldier.bSide != this.overhead.gbPlayerNum))
            {
                // It's an enemy merc
                MercFlags |= ENEMY_MERC;
            }
            else
            {
                // It's not an enemy merc
                MercFlags |= NEUTRAL_MERC;
            }
        }

        // Check for a guy who does not have an iterrupt ( when applicable! )
        if (!OK_INTERRUPT_MERC(pSoldier))
        {
            MercFlags |= NOINTERRUPT_MERC;
        }

        if (pSoldier.bLife < OKLIFE)
        {
            MercFlags |= UNCONSCIOUS_MERC;
        }

        if (pSoldier.bLife == 0)
        {
            MercFlags |= DEAD_MERC;
        }

        if (pSoldier.bVisible != -1 || (this.overhead.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)))
        {
            MercFlags |= VISIBLE_MERC;
        }

        return (MercFlags);
    }

    // THIS FUNCTION IS CALLED FAIRLY REGULARLY
    public bool FindSoldier(int sGridNo, out int pusSoldierIndex, out int pMercFlags, FIND_SOLDIER uiFlags)
    {
        int cnt;
        SOLDIERTYPE? pSoldier;
        Rectangle aRect;
        bool fSoldierFound = false;
        int sXMapPos, sYMapPos, sScreenX, sScreenY;
        int sMaxScreenMercY, sHeighestMercScreenY = -32000;
        bool fDoFull;
        int ubBestMerc = OverheadTypes.NOBODY;
        int usAnimSurface;
        int iMercScreenX, iMercScreenY;
        bool fInScreenRect = false;
        bool fInGridNo = false;

        pusSoldierIndex = OverheadTypes.NOBODY;
        pMercFlags = 0;

        if (_KeyDown(SHIFT))
        {
            uiFlags = FIND_SOLDIER.GRIDNO;
        }

        // Set some values
        if (uiFlags.HasFlag(FIND_SOLDIER.BEGINSTACK))
        {
            gSoldierStack.bNum = 0;
            gSoldierStack.fUseGridNo = false;
        }


        // Loop through all mercs and make go
        for (cnt = 0; cnt < guiNumMercSlots; cnt++)
        {
            pSoldier = MercSlots[cnt];
            fInScreenRect = false;
            fInGridNo = false;

            if (pSoldier != null)
            {
                if (pSoldier.bActive && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
                    && (pSoldier.bVisible != -1 || (this.overhead.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS))))
                {
                    // OK, ignore if we are a passenger...
                    if (pSoldier.uiStatusFlags.HasFlag((SOLDIER.PASSENGER | SOLDIER.DRIVER)))
                    {
                        continue;
                    }

                    // If we want same level, skip if buggy's not on the same level!
                    if (uiFlags.HasFlag(FIND_SOLDIER.SAMELEVEL))
                    {
                        if (pSoldier.bLevel != ((int)uiFlags >> 16))
                        {
                            continue;
                        }
                    }


                    // If we are selective.... do our own guys FULL and other with gridno!
                    // First look for owned soldiers, by way of the full method
                    if (uiFlags.HasFlag(FIND_SOLDIER.GRIDNO))
                    {
                        fDoFull = false;
                    }
                    else if (uiFlags.HasFlag(FIND_SOLDIER.SELECTIVE))
                    {
                        if (pSoldier.ubID >= this.overhead.gTacticalStatus.Team[(int)this.overhead.gbPlayerNum].bFirstID && pSoldier.ubID <= this.overhead.gTacticalStatus.Team[(int)this.overhead.gbPlayerNum].bLastID)
                        {
                            fDoFull = true;
                        }
                        else
                        {
                            fDoFull = false;
                        }
                    }
                    else
                    {
                        fDoFull = true;
                    }

                    if (fDoFull)
                    {
                        // Get Rect contained in the soldier
                        GetSoldierScreenRect(pSoldier, out aRect);

                        // Get XY From gridno
                        ConvertGridNoToXY(sGridNo, sXMapPos, sYMapPos);

                        // Get screen XY pos from map XY
                        // Be carefull to convert to cell cords
                        //CellXYToScreenXY( (int)((sXMapPos*CELL_X_SIZE)), (int)((sYMapPos*CELL_Y_SIZE)), &sScreenX, &sScreenY);

                        // Set mouse stuff
                        sScreenX = gusMouseXPos;
                        sScreenY = gusMouseYPos;

                        if (IsPointInScreenRect(sScreenX, sScreenY, aRect))
                        {
                            fInScreenRect = true;
                        }

                        if (pSoldier.sGridNo == sGridNo)
                        {
                            fInGridNo = true;
                        }

                        // ATE: If we are an enemy....
                        if (!gGameSettings.fOptions[TOPTION.SMART_CURSOR])
                        {
                            if (pSoldier.ubID >= this.overhead.gTacticalStatus.Team[(int)this.overhead.gbPlayerNum].bFirstID && pSoldier.ubID <= this.overhead.gTacticalStatus.Team[this.overhead.gbPlayerNum].bLastID)
                            {
                                // ATE: NOT if we are in action or comfirm action mode
                                if (gCurrentUIMode != ACTION_MODE && gCurrentUIMode != CONFIRM_ACTION_MODE || gUIActionModeChangeDueToMouseOver)
                                {
                                    fInScreenRect = false;
                                }
                            }
                        }

                        // ATE: Refine this further....
                        // Check if this is the selected guy....
                        if (pSoldier.ubID == gusSelectedSoldier)
                        {
                            // Are we in action mode...
                            if (gCurrentUIMode == ACTION_MODE || gCurrentUIMode == CONFIRM_ACTION_MODE)
                            {
                                // Are we in medic mode?
                                if (GetActionModeCursor(pSoldier) != AIDCURS)
                                {
                                    fInScreenRect = false;
                                    fInGridNo = false;
                                }
                            }
                        }

                        // Make sure we are always on guy if we are on same gridno
                        if (fInScreenRect || fInGridNo)
                        {
                            // Check if we are a vehicle and refine if so....
                            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                            {
                                usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

                                if (usAnimSurface != INVALID_ANIMATION_SURFACE)
                                {
                                    iMercScreenX = (int)(sScreenX - aRect.Left);
                                    iMercScreenY = (int)(-1 * (sScreenY - aRect.Bottom));

                                    if (!CheckVideoObjectScreenCoordinateInData(gAnimSurfaceDatabase[usAnimSurface].hVideoObject, pSoldier.usAniFrame, iMercScreenX, iMercScreenY))
                                    {
                                        continue;
                                    }
                                }
                            }

                            // If thgis is from a gridno, use mouse pos!
                            if (pSoldier.sGridNo == sGridNo)
                            {

                            }

                            // Only break here if we're not creating a stack of these fellas
                            if (uiFlags.HasFlag(FIND_SOLDIER.BEGINSTACK))
                            {
                                gfHandleStack = true;

                                // Add this one!
                                gSoldierStack.ubIDs[gSoldierStack.bNum] = pSoldier.ubID;
                                gSoldierStack.bNum++;

                                // Determine if it's the current
                                if (aRect.Bottom > sHeighestMercScreenY)
                                {
                                    sMaxScreenMercY = (int)aRect.Bottom;
                                    sHeighestMercScreenY = sMaxScreenMercY;

                                    gSoldierStack.bCur = gSoldierStack.bNum - 1;
                                }
                            }
                            //Are we handling a stack right now?
                            else if (gfHandleStack)
                            {
                                // Are we the selected stack?
                                if (gSoldierStack.fUseGridNo)
                                {
                                    fSoldierFound = false;
                                    break;
                                }
                                else if (gSoldierStack.ubIDs[gSoldierStack.bCur] == pSoldier.ubID)
                                {
                                    // Set it!
                                    ubBestMerc = pSoldier.ubID;

                                    fSoldierFound = true;
                                    break;
                                }
                            }
                            else
                            {
                                // Determine if it's the best one
                                if (aRect.Bottom > sHeighestMercScreenY)
                                {
                                    sMaxScreenMercY = (int)aRect.Bottom;
                                    sHeighestMercScreenY = sMaxScreenMercY;

                                    // Set it!
                                    ubBestMerc = pSoldier.ubID;
                                }

                                fSoldierFound = true;
                                // Don't break here, find the rest!

                            }
                        }

                    }
                    else
                    {
                        //Otherwise, look for a bad guy by way of gridno]
                        // Selective means don't give out enemy mercs if they are not visible

                        ///&& !NewOKDestination( pSoldier, sGridNo, true, (int)gsInterfaceLevel )
                        if (pSoldier.sGridNo == sGridNo && !NewOKDestination(pSoldier, sGridNo, true, (int)Interface.gsInterfaceLevel))
                        {
                            // Set it!
                            ubBestMerc = pSoldier.ubID;

                            fSoldierFound = true;
                            break;
                        }
                    }
                }
            }
        }

        if (fSoldierFound && ubBestMerc != OverheadTypes.NOBODY)
        {
            pusSoldierIndex = (int)ubBestMerc;

            (pMercFlags) = GetSoldierFindFlags(ubBestMerc);

            return (true);

        }
        else
        {
            // If we were handling a stack, and we have not found anybody, end
            if (gfHandleStack && !(uiFlags.HasFlag(FIND_SOLDIER.BEGINSTACK | FIND_SOLDIER.SELECTIVE)))
            {
                if (gSoldierStack.fUseGridNo)
                {
                    if (gSoldierStack.sUseGridNoGridNo != sGridNo)
                    {
                        gfHandleStack = false;
                    }
                }
                else
                {
                    gfHandleStack = false;
                }
            }
        }
        return (false);
    }

    bool CycleSoldierFindStack(int usMapPos)
    {

        // Have we initalized for this yet?
        if (!gfHandleStack)
        {
            if (FindSoldier(usMapPos, out int usSoldierIndex, out int uiMercFlags, FINDSOLDIERSAMELEVEL(Interface.gsInterfaceLevel) | FIND_SOLDIER.BEGINSTACK))
            {
                gfHandleStack = true;
            }

        }

        if (gfHandleStack)
        {
            // we are cycling now?
            if (!gSoldierStack.fUseGridNo)
            {
                gSoldierStack.bCur++;
            }

            gfUIForceReExamineCursorData = true;

            if (gSoldierStack.bCur == gSoldierStack.bNum)
            {
                if (!gSoldierStack.fUseGridNo)
                {
                    gSoldierStack.fUseGridNo = true;
                    gUIActionModeChangeDueToMouseOver = false;
                    gSoldierStack.sUseGridNoGridNo = usMapPos;
                }
                else
                {
                    gSoldierStack.bCur = 0;
                    gSoldierStack.fUseGridNo = false;
                }
            }


            if (!gSoldierStack.fUseGridNo)
            {
                gusUIFullTargetID = gSoldierStack.ubIDs[gSoldierStack.bCur];
                guiUIFullTargetFlags = GetSoldierFindFlags(gusUIFullTargetID);
                guiUITargetSoldierId = gusUIFullTargetID;
                gfUIFullTargetFound = true;
            }
            else
            {
                gfUIFullTargetFound = false;
            }
        }

        // Return if we are in the cycle mode now...
        return (gfHandleStack);
    }

    SOLDIERTYPE? SimpleFindSoldier(int sGridNo, int bLevel)
    {
        int ubID;

        ubID = WhoIsThere2(sGridNo, bLevel);
        if (ubID == OverheadTypes.NOBODY)
        {
            return (null);
        }
        else
        {
            return (MercPtrs[ubID]);
        }
    }

    bool IsValidTargetMerc(int ubSoldierID)
    {
        SOLDIERTYPE? pSoldier = MercPtrs[ubSoldierID];


        // CHECK IF ACTIVE!
        if (!pSoldier.bActive)
        {
            return (false);
        }

        // CHECK IF DEAD
        if (pSoldier.bLife == 0)
        {
            //return( false );
        }

        // IF BAD GUY - CHECK VISIVILITY
        if (pSoldier.bTeam != this.overhead.gbPlayerNum)
        {
            if (pSoldier.bVisible == -1 && !(this.overhead.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)))
            {
                return (false);
            }
        }

        return (true);
    }


    bool IsGridNoInScreenRect(int sGridNo, Rectangle pRect)
    {
        int iXTrav, iYTrav;
        int sMapPos;

        // Start with top left corner
        iXTrav = pRect.Left;
        iYTrav = pRect.Top;

        do
        {
            do
            {
                GetScreenXYGridNo((int)iXTrav, (int)iYTrav, sMapPos);

                if (sMapPos == sGridNo)
                {
                    return (true);
                }

                iXTrav += World.WORLD_TILE_X;

            } while (iXTrav < pRect.Right);

            iYTrav += World.WORLD_TILE_Y;
            iXTrav = pRect.Left;

        } while (iYTrav < pRect.Bottom);

        return (false);
    }


    void GetSoldierScreenRect(SOLDIERTYPE? pSoldier, out Rectangle pRect)
    {
        int usAnimSurface;
        //		ETRLEObject *pTrav;
        //		int usHeight, usWidth;

        GetSoldierScreenPos(pSoldier, out int sMercScreenX, out int sMercScreenY);

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);
        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            pRect = new(sMercScreenX, sMercScreenY, sMercScreenX + 5, sMercScreenY + 5);

            return;
        }

        //pTrav = &(gAnimSurfaceDatabase[ usAnimSurface ].hVideoObject.pETRLEObject[ pSoldier.usAniFrame ] );
        //usHeight				= (int)pTrav.usHeight;
        //usWidth					= (int)pTrav.usWidth;

        pRect = new(
            sMercScreenX,
            sMercScreenY,
            sMercScreenX + pSoldier.sBoundingBoxWidth,
            sMercScreenY + pSoldier.sBoundingBoxHeight);
        ;
    }

    void GetSoldierAnimDims(SOLDIERTYPE? pSoldier, out int psHeight, out int psWidth)
    {
        int usAnimSurface;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            psHeight = (int)5;
            psWidth = (int)5;

            return;
        }

        // OK, noodle here on what we should do... If we take each frame, it will be different slightly
        // depending on the frame and the value returned here will vary thusly. However, for the
        // uses of this function, we should be able to use just the first frame...

        if (pSoldier.usAniFrame >= gAnimSurfaceDatabase[usAnimSurface].hVideoObject.usNumberOfObjects)
        {
            int i = 0;
        }

        psHeight = pSoldier.sBoundingBoxHeight;
        psWidth = pSoldier.sBoundingBoxWidth;
    }

    void GetSoldierAnimOffsets(SOLDIERTYPE? pSoldier, out int sOffsetX, out int sOffsetY)
    {
        int usAnimSurface;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            sOffsetX = (int)0;
            sOffsetY = (int)0;

            return;
        }

        sOffsetX = (int)pSoldier.sBoundingBoxOffsetX;
        sOffsetY = (int)pSoldier.sBoundingBoxOffsetY;
    }

    void GetSoldierScreenPos(SOLDIERTYPE? pSoldier, out int psScreenX, out int psScreenY)
    {
        int sMercScreenX, sMercScreenY;
        float dOffsetX, dOffsetY;
        float dTempX_S, dTempY_S;
        int usAnimSurface;
        //		ETRLEObject *pTrav;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            psScreenX = 0;
            psScreenY = 0;
            return;
        }

        // Get 'true' merc position
        dOffsetX = pSoldier.dXPos - this.renderWorld.gsRenderCenterX;
        dOffsetY = pSoldier.dYPos - this.renderWorld.gsRenderCenterY;

        FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, dTempX_S, dTempY_S);

        //pTrav = &(gAnimSurfaceDatabase[ usAnimSurface ].hVideoObject.pETRLEObject[ pSoldier.usAniFrame ] );

        sMercScreenX = ((this.renderWorld.gsVIEWPORT_END_X - this.renderWorld.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sMercScreenY = ((this.renderWorld.gsVIEWPORT_END_Y - this.renderWorld.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sMercScreenX -= this.renderWorld.gsRenderWorldOffsetX;
        sMercScreenY -= this.renderWorld.gsRenderWorldOffsetY;
        sMercScreenY -= this.globals.gpWorldLevelData[pSoldier.sGridNo].sHeight;

        // Adjust for render height
        sMercScreenY += this.renderWorld.gsRenderHeight;

        // Add to start position of dest buffer
        //sMercScreenX += pTrav.sOffsetX;
        //sMercScreenY += pTrav.sOffsetY;
        sMercScreenX += pSoldier.sBoundingBoxOffsetX;
        sMercScreenY += pSoldier.sBoundingBoxOffsetY;


        sMercScreenY -= pSoldier.sHeightAdjustment;

        psScreenX = sMercScreenX;
        psScreenY = sMercScreenY;
    }

    // THE true SCREN RECT DOES NOT TAKE THE OFFSETS OF BUDDY INTO ACCOUNT!
    void GetSoldiertrueScreenPos(SOLDIERTYPE? pSoldier, out int psScreenX, out int psScreenY)
    {
        int sMercScreenX, sMercScreenY;
        float dOffsetX, dOffsetY;
        float dTempX_S, dTempY_S;
        int usAnimSurface;

        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == INVALID_ANIMATION_SURFACE)
        {
            psScreenX = 0;
            psScreenY = 0;
            return;
        }

        // Get 'true' merc position
        dOffsetX = pSoldier.dXPos - this.renderWorld.gsRenderCenterX;
        dOffsetY = pSoldier.dYPos - this.renderWorld.gsRenderCenterY;

        FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, dTempX_S, dTempY_S);

        sMercScreenX = ((this.renderWorld.gsVIEWPORT_END_X - this.renderWorld.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sMercScreenY = ((this.renderWorld.gsVIEWPORT_END_Y - this.renderWorld.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sMercScreenX -= this.renderWorld.gsRenderWorldOffsetX;
        sMercScreenY -= this.renderWorld.gsRenderWorldOffsetY;

        // Adjust for render height
        sMercScreenY += this.renderWorld.gsRenderHeight;
        sMercScreenY -= this.globals.gpWorldLevelData[pSoldier.sGridNo].sHeight;

        sMercScreenY -= pSoldier.sHeightAdjustment;

        psScreenX = sMercScreenX;
        psScreenY = sMercScreenY;
    }

    bool GridNoOnScreen(int sGridNo)
    {
        int sNewCenterWorldX, sNewCenterWorldY;
        int sWorldX;
        int sWorldY;
        int sAllowance = 20;

        if (this.renderWorld.gsVIEWPORT_WINDOW_START_Y == 20)
        {
            sAllowance = 40;
        }

        ConvertGridNoToXY(sGridNo, sNewCenterWorldX, sNewCenterWorldY);

        // Get screen coordinates for current position of soldier
        GetWorldXYAbsoluteScreenXY((int)(sNewCenterWorldX), (int)(sNewCenterWorldY), sWorldX, sWorldY);

        // ATE: OK, here, adjust the top value so that it's a tile and a bit over, because of our mercs!
        if (sWorldX >= this.renderWorld.gsTopLeftWorldX
            && sWorldX <= this.renderWorld.gsBottomRightWorldX
            && sWorldY >= (this.renderWorld.gsTopLeftWorldY + sAllowance)
            && sWorldY <= (this.renderWorld.gsBottomRightWorldY + 20))
        {
            return (true);
        }

        return (false);
    }

    bool SoldierOnScreen(int usID)
    {
        SOLDIERTYPE? pSoldier;

        // Get pointer of soldier
        pSoldier = MercPtrs[usID];

        return (GridNoOnScreen(pSoldier.sGridNo));
    }


    bool SoldierOnVisibleWorldTile(SOLDIERTYPE? pSoldier)
    {
        return (GridNoOnVisibleWorldTile(pSoldier.sGridNo));
    }

    bool SoldierLocationRelativeToScreen(int sGridNo, int usReasonID, out int pbDirection, out int puiScrollFlags)
    {
        int sWorldX;
        int sWorldY;
        int sY, sX;
        int sScreenCenterX, sScreenCenterY;
        int sDistToCenterY, sDistToCenterX;

        puiScrollFlags = 0;

        sX = CenterX(sGridNo);
        sY = CenterY(sGridNo);

        // Get screen coordinates for current position of soldier
        GetWorldXYAbsoluteScreenXY((int)(sX / CELL_X_SIZE), (int)(sY / CELL_Y_SIZE), sWorldX, sWorldY);

        // Find the diustance from render center to true world center
        sDistToCenterX = this.renderWorld.gsRenderCenterX - this.renderWorld.gCenterWorldX;
        sDistToCenterY = this.renderWorld.gsRenderCenterY - this.renderWorld.gCenterWorldY;

        // From render center in world coords, convert to render center in "screen" coords
        FromCellToScreenCoordinates(sDistToCenterX, sDistToCenterY, sScreenCenterX, sScreenCenterY);

        // Subtract screen center
        sScreenCenterX += gsCX;
        sScreenCenterY += gsCY;

        // Adjust for offset origin!
        sScreenCenterX += 0;
        sScreenCenterY += 10;

        // Get direction
        //*pbDirection = atan8( sScreenCenterX, sScreenCenterY, sWorldX, sWorldY );
        pbDirection = atan8(this.renderWorld.gsRenderCenterX, this.renderWorld.gsRenderCenterY, (int)(sX), (int)(sY));

        // Check values!
        if (sWorldX > (sScreenCenterX + 20))
        {
            (puiScrollFlags) |= SCROLL_RIGHT;
        }
        if (sWorldX < (sScreenCenterX - 20))
        {
            (puiScrollFlags) |= SCROLL_LEFT;
        }
        if (sWorldY > (sScreenCenterY + 20))
        {
            (puiScrollFlags) |= SCROLL_DOWN;
        }
        if (sWorldY < (sScreenCenterY - 20))
        {
            (puiScrollFlags) |= SCROLL_UP;
        }


        // If we are on screen, stop
        if (sWorldX >= this.renderWorld.gsTopLeftWorldX
            && sWorldX <= this.renderWorld.gsBottomRightWorldX
            && sWorldY >= this.renderWorld.gsTopLeftWorldY
            && sWorldY <= (this.renderWorld.gsBottomRightWorldY + 20))
        {
            // CHECK IF WE ARE DONE...
            if (fCountdown > gScrollSlideInertiaDirection[pbDirection])
            {
                fCountdown = 0;
                return (false);
            }
            else
            {
                fCountdown++;
            }
        }

        return (true);
    }

    bool IsPointInSoldierBoundingBox(SOLDIERTYPE? pSoldier, int sX, int sY)
    {
        // Get Rect contained in the soldier
        GetSoldierScreenRect(pSoldier, out Rectangle aRect);

        if (IsPointInScreenRect(sX, sY, aRect))
        {
            return (true);
        }

        return (false);
    }


    bool FindRelativeSoldierPosition(SOLDIERTYPE? pSoldier, out int usFlags, int sX, int sY)
    {
        int sRelX, sRelY;
        float dRelPer;


        // Get Rect contained in the soldier
        GetSoldierScreenRect(pSoldier, out Rectangle aRect);

        if (IsPointInScreenRectWithRelative(sX, sY, aRect, sRelX, sRelY))
        {
            dRelPer = (float)sRelY / (aRect.Bottom - aRect.Top);

            // Determine relative positions
            switch (gAnimControl[pSoldier.usAnimState].ubHeight)
            {
                case ANIM_STAND:

                    if (dRelPer < .2)
                    {
                        (usFlags) = TILE_FLAG_HEAD;
                        return (true);
                    }
                    else if (dRelPer < .6)
                    {
                        (usFlags) = TILE_FLAG_MID;
                        return (true);
                    }
                    else
                    {
                        (usFlags) = TILE_FLAG_FEET;
                        return (true);
                    }

                case ANIM_CROUCH:

                    if (dRelPer < .2)
                    {
                        (usFlags) = TILE_FLAG_HEAD;
                        return (true);
                    }
                    else if (dRelPer < .7)
                    {
                        (usFlags) = TILE_FLAG_MID;
                        return (true);
                    }
                    else
                    {
                        (usFlags) = TILE_FLAG_FEET;
                        return (true);
                    }
            }
        }

        usFlags = 0;
        return (false);
    }

    // VERY quickly finds a soldier at gridno , ( that is visible )
    int QuickFindSoldier(int sGridNo)
    {
        int cnt;
        SOLDIERTYPE? pSoldier = null;

        // Loop through all mercs and make go
        for (cnt = 0; cnt < guiNumMercSlots; cnt++)
        {
            pSoldier = MercSlots[cnt];

            if (pSoldier != null)
            {
                if (pSoldier.sGridNo == sGridNo && pSoldier.bVisible != -1)
                {
                    return ((int)cnt);
                }
            }

        }

        return (OverheadTypes.NOBODY);
    }


    void GetGridNoScreenPos(int sGridNo, int ubLevel, out int psScreenX, out int psScreenY)
    {
        int sScreenX, sScreenY;
        float dOffsetX, dOffsetY;
        float dTempX_S, dTempY_S;

        // Get 'true' merc position
        dOffsetX = (float)(CenterX(sGridNo) - this.renderWorld.gsRenderCenterX);
        dOffsetY = (float)(CenterY(sGridNo) - this.renderWorld.gsRenderCenterY);

        // OK, DONT'T ASK... CONVERSION TO PROPER Y NEEDS THIS...
        dOffsetX -= CELL_Y_SIZE;

        FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, dTempX_S, dTempY_S);

        sScreenX = ((this.renderWorld.gsVIEWPORT_END_X - this.renderWorld.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sScreenY = ((this.renderWorld.gsVIEWPORT_END_Y - this.renderWorld.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sScreenX -= this.renderWorld.gsRenderWorldOffsetX;
        sScreenY -= this.renderWorld.gsRenderWorldOffsetY;

        sScreenY += this.renderWorld.gsRenderHeight;

        // Adjust for world height
        sScreenY -= this.globals.gpWorldLevelData[sGridNo].sHeight;

        // Adjust for level height
        if (ubLevel)
        {
            sScreenY -= ROOF_LEVEL_HEIGHT;
        }

        psScreenX = sScreenX;
        psScreenY = sScreenY;
    }
}

// Struct used for cycling through multiple mercs per mouse position
public struct SOLDIER_STACK_TYPE
{
    public int bNum;
    public int[] ubIDs; //[MAX_STACKED_MERCS];
    public int bCur;
    public bool fUseGridNo;
    public int sUseGridNoGridNo;
}

[Flags]
public enum FIND_SOLDIER
{
    FULL = 0x000000002,
    GRIDNO = 0x000000004,
    SAMELEVEL = 0x000000008,
    SELECTIVE = 0x000000020,
    BEGINSTACK = 0x000000040,
}
