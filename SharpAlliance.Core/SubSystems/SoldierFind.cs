using System;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

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

    public SoldierFind(
        ILogger<SoldierFind> logger,
        GameSettings gGameSettings,
        RenderWorld renderWorld,
        Overhead overhead)
    {
        this.logger = logger;
        this.gGameSettings = gGameSettings;
        this.renderWorld = renderWorld;
        this.overhead = overhead;
    }

    public static int[] gScrollSlideInertiaDirection = new int[(int)WorldDirections.NUM_WORLD_DIRECTIONS]
    {
        3, 0, 0, 0, 0, 0, 3, 3,
    };

    // extern bool Globals.gUIActionModeChangeDueToMouseOver;
    // extern int guiUITargetSoldierId;

    bool FindSoldierFromMouse(out int pusSoldierIndex, out FIND_SOLDIER_RESPONSES pMercFlags)
    {

        pMercFlags = 0;

        if (IsometricUtils.GetMouseMapPos(out int sMapPos))
        {
            if (FindSoldier(sMapPos, out pusSoldierIndex, out pMercFlags, FINDSOLDIERSAMELEVEL(Globals.gsInterfaceLevel)))
            {
                return true;
            }
        }

        pusSoldierIndex = -1;
        pMercFlags = (FIND_SOLDIER_RESPONSES)(-1);

        return false;
    }

    public static FIND_SOLDIER FINDSOLDIERSAMELEVEL(int level)
        => FIND_SOLDIER.FULL | FIND_SOLDIER.SAMELEVEL | (FIND_SOLDIER)(level << 16);

    public static FIND_SOLDIER FINDSOLDIERSELECTIVESAMELEVEL(int level)
        => FIND_SOLDIER.SELECTIVE | FIND_SOLDIER.SAMELEVEL | (FIND_SOLDIER)(level << 16);

    bool SelectiveFindSoldierFromMouse(out int pusSoldierIndex, out FIND_SOLDIER_RESPONSES pMercFlags)
    {

        pMercFlags = 0;

        if (IsometricUtils.GetMouseMapPos(out int sMapPos))
        {
            if (FindSoldier(sMapPos, out pusSoldierIndex, out pMercFlags, FINDSOLDIERSAMELEVEL(Globals.gsInterfaceLevel)))
            {
                return true;
            }
        }

        pusSoldierIndex = -1;
        pMercFlags = (FIND_SOLDIER_RESPONSES)(-1);

        return false;
    }

    public static FIND_SOLDIER_RESPONSES GetSoldierFindFlags(int ubID)
    {
        FIND_SOLDIER_RESPONSES MercFlags = 0;
        SOLDIERTYPE? pSoldier;

        // Get pSoldier!
        pSoldier = Globals.MercPtrs[ubID];

        // FInd out and set flags
        if (ubID == Globals.gusSelectedSoldier)
        {
            MercFlags |= FIND_SOLDIER_RESPONSES.SELECTED_MERC;
        }
        if (ubID >= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID && ubID <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID)
        {
//            if ((pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)) && !GetNumberInVehicle(pSoldier.bVehicleID))
//            {
//                // Don't do anything!
//            }
//            else
//            {
//                // It's our own merc
//                MercFlags |= FIND_SOLDIER_RESPONSES.OWNED_MERC;
//
//                if (pSoldier.bAssignment < Assignments.ON_DUTY)
//                {
//                    MercFlags |= FIND_SOLDIER_RESPONSES.ONDUTY_MERC;
//                }
//            }
        }
        else
        {
            // Check the side, etc
            if (pSoldier.bNeutral == 0 && (pSoldier.bSide != Globals.gbPlayerNum))
            {
                // It's an enemy merc
                MercFlags |= FIND_SOLDIER_RESPONSES.ENEMY_MERC;
            }
            else
            {
                // It's not an enemy merc
                MercFlags |= FIND_SOLDIER_RESPONSES.NEUTRAL_MERC;
            }
        }

        // Check for a guy who does not have an iterrupt ( when applicable! )
        if (!OK_INTERRUPT_MERC(pSoldier))
        {
            MercFlags |= FIND_SOLDIER_RESPONSES.NOINTERRUPT_MERC;
        }

        if (pSoldier.bLife < Globals.OKLIFE)
        {
            MercFlags |= FIND_SOLDIER_RESPONSES.UNCONSCIOUS_MERC;
        }

        if (pSoldier.bLife == 0)
        {
            MercFlags |= FIND_SOLDIER_RESPONSES.DEAD_MERC;
        }

        if (pSoldier.bVisible != -1 || Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS))
        {
            MercFlags |= FIND_SOLDIER_RESPONSES.VISIBLE_MERC;
        }

        return MercFlags;
    }

    // THIS FUNCTION IS CALLED FAIRLY REGULARLY
    public static bool FindSoldier(int sGridNo, out int pusSoldierIndex, out FIND_SOLDIER_RESPONSES pMercFlags, FIND_SOLDIER uiFlags)
    {
        int cnt;
        SOLDIERTYPE? pSoldier;
        bool fSoldierFound = false;
        int sScreenX, sScreenY;
        int sMaxScreenMercY, sHeighestMercScreenY = -32000;
        bool fDoFull;
        int ubBestMerc = Globals.NOBODY;
        AnimationSurfaceTypes usAnimSurface = 0;
        int iMercScreenX, iMercScreenY;
        bool fInScreenRect = false;
        bool fInGridNo = false;

        pusSoldierIndex = Globals.NOBODY;
        pMercFlags = 0;

        if (_KeyDown(Key.LShift | Key.RShift))
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
        for (cnt = 0; cnt < Globals.guiNumMercSlots; cnt++)
        {
            pSoldier = Globals.MercSlots[cnt];
            fInScreenRect = false;
            fInGridNo = false;

            if (pSoldier != null)
            {
                if (pSoldier.bActive && !pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD)
                    && (pSoldier.bVisible != -1 || Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS)))
                {
                    // OK, ignore if we are a passenger...
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.PASSENGER | SOLDIER.DRIVER))
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
                        if (pSoldier.ubID >= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID
                            && pSoldier.ubID <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID)
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
                        GetSoldierScreenRect(pSoldier, out Rectangle aRect);

                        // Get XY From gridno
                        IsometricUtils.ConvertGridNoToXY(sGridNo, out int sXMapPos, out int sYMapPos);

                        // Get screen XY pos from map XY
                        // Be carefull to convert to cell cords
                        //CellXYToScreenXY( (int)((sXMapPos*CELL_X_SIZE)), (int)((sYMapPos*CELL_Y_SIZE)), &sScreenX, &sScreenY);

                        // Set mouse stuff
                        sScreenX = Globals.gusMouseXPos;
                        sScreenY = Globals.gusMouseYPos;

//                        if (IsPointInScreenRect(sScreenX, sScreenY, aRect))
//                        {
//                            fInScreenRect = true;
//                        }

                        if (pSoldier.sGridNo == sGridNo)
                        {
                            fInGridNo = true;
                        }

                        // ATE: If we are an enemy....
                        if (!GameSettings.fOptions[TOPTION.SMART_CURSOR])
                        {
                            if (pSoldier.ubID >= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID && pSoldier.ubID <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID)
                            {
                                // ATE: NOT if we are in action or comfirm action mode
                                if (Globals.gCurrentUIMode != UI_MODE.ACTION_MODE
                                    && Globals.gCurrentUIMode != UI_MODE.CONFIRM_ACTION_MODE
                                    || Globals.gUIActionModeChangeDueToMouseOver)
                                {
                                    fInScreenRect = false;
                                }
                            }
                        }

                        // ATE: Refine this further....
                        // Check if this is the selected guy....
                        if (pSoldier.ubID == Globals.gusSelectedSoldier)
                        {
                            // Are we in action mode...
                            if (Globals.gCurrentUIMode == UI_MODE.ACTION_MODE
                                || Globals.gCurrentUIMode == UI_MODE.CONFIRM_ACTION_MODE)
                            {
                                // Are we in medic mode?
//                                if (GetActionModeCursor(pSoldier) != CURSOR.AIDCURS)
//                                {
//                                    fInScreenRect = false;
//                                    fInGridNo = false;
//                                }
                            }
                        }

                        // Make sure we are always on guy if we are on same gridno
                        if (fInScreenRect || fInGridNo)
                        {
                            // Check if we are a vehicle and refine if so....
                            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                            {
//                                usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

                                if (usAnimSurface != Globals.INVALID_ANIMATION_SURFACE)
                                {
                                    iMercScreenX = (int)(sScreenX - aRect.Left);
                                    iMercScreenY = (int)(-1 * (sScreenY - aRect.Bottom));

//                                    if (!CheckVideoObjectScreenCoordinateInData(Globals.gAnimSurfaceDatabase[usAnimSurface].hVideoObject, pSoldier.usAniFrame, iMercScreenX, iMercScreenY))
//                                    {
//                                        continue;
//                                    }
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

                        //&& !NewOKDestination( pSoldier, sGridNo, true, (int)gsInterfaceLevel )
                        if (pSoldier.sGridNo == sGridNo && !Overhead.NewOKDestination(pSoldier, sGridNo, true, (int)Globals.gsInterfaceLevel))
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

        if (fSoldierFound && ubBestMerc != Globals.NOBODY)
        {
            pusSoldierIndex = (int)ubBestMerc;

            pMercFlags = GetSoldierFindFlags(ubBestMerc);

            return true;

        }
        else
        {
            // If we were handling a stack, and we have not found anybody, end
            if (gfHandleStack && !uiFlags.HasFlag(FIND_SOLDIER.BEGINSTACK | FIND_SOLDIER.SELECTIVE))
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
        return false;
    }

    bool CycleSoldierFindStack(int usMapPos)
    {

        // Have we initalized for this yet?
        if (!gfHandleStack)
        {
            if (FindSoldier(usMapPos, out int usSoldierIndex, out FIND_SOLDIER_RESPONSES uiMercFlags, FINDSOLDIERSAMELEVEL(Globals.gsInterfaceLevel) | FIND_SOLDIER.BEGINSTACK))
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

            Globals.gfUIForceReExamineCursorData = true;

            if (gSoldierStack.bCur == gSoldierStack.bNum)
            {
                if (!gSoldierStack.fUseGridNo)
                {
                    gSoldierStack.fUseGridNo = true;
                    Globals.gUIActionModeChangeDueToMouseOver = false;
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
                Globals.gusUIFullTargetID = gSoldierStack.ubIDs[gSoldierStack.bCur];
                Globals.guiUIFullTargetFlags = GetSoldierFindFlags(Globals.gusUIFullTargetID);
                Globals.guiUITargetSoldierId = Globals.gusUIFullTargetID;
                Globals.gfUIFullTargetFound = true;
            }
            else
            {
                Globals.gfUIFullTargetFound = false;
            }
        }

        // Return if we are in the cycle mode now...
        return gfHandleStack;
    }

    SOLDIERTYPE? SimpleFindSoldier(int sGridNo, int bLevel)
    {
        int ubID;

        ubID = WorldManager.WhoIsThere2(sGridNo, bLevel);
        if (ubID == Globals.NOBODY)
        {
            return null;
        }
        else
        {
            return Globals.MercPtrs[ubID];
        }
    }

    bool IsValidTargetMerc(int ubSoldierID)
    {
        SOLDIERTYPE? pSoldier = Globals.MercPtrs[ubSoldierID];


        // CHECK IF ACTIVE!
        if (!pSoldier.bActive)
        {
            return false;
        }

        // CHECK IF DEAD
        if (pSoldier.bLife == 0)
        {
            //return( false );
        }

        // IF BAD GUY - CHECK VISIVILITY
        if (pSoldier.bTeam != Globals.gbPlayerNum)
        {
            if (pSoldier.bVisible == -1 && !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS))
            {
                return false;
            }
        }

        return true;
    }


    bool IsGridNoInScreenRect(int sGridNo, Rectangle pRect)
    {
        int iXTrav, iYTrav;
        int sMapPos = 0;

        // Start with top left corner
        iXTrav = pRect.Left;
        iYTrav = pRect.Top;

        do
        {
            do
            {
//                GetScreenXYGridNo((int)iXTrav, (int)iYTrav, out sMapPos);

                if (sMapPos == sGridNo)
                {
                    return true;
                }

                iXTrav += WORLD_TILE_X;

            } while (iXTrav < pRect.Right);

            iYTrav += WORLD_TILE_Y;
            iXTrav = pRect.Left;

        } while (iYTrav < pRect.Bottom);

        return false;
    }

    public static void GetSoldierScreenRect(SOLDIERTYPE? pSoldier, out Rectangle pRect)
    {
        int usAnimSurface;
        //		ETRLEObject *pTrav;
        //		int usHeight, usWidth;

        GetSoldierScreenPos(pSoldier, out int sMercScreenX, out int sMercScreenY);

//        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);
//        if (usAnimSurface == Globals.INVALID_ANIMATION_SURFACE)
//        {
//            pRect = new(sMercScreenX, sMercScreenY, sMercScreenX + 5, sMercScreenY + 5);
//
//            return;
//        }

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
        AnimationSurfaceTypes usAnimSurface = AnimationControl.GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == Globals.INVALID_ANIMATION_SURFACE)
        {
            psHeight = (int)5;
            psWidth = (int)5;

            return;
        }

        // OK, noodle here on what we should do... If we take each frame, it will be different slightly
        // depending on the frame and the value returned here will vary thusly. However, for the
        // uses of this function, we should be able to use just the first frame...

        if (pSoldier.usAniFrame >= Globals.gAnimSurfaceDatabase[usAnimSurface].hVideoObject.usNumberOfObjects)
        {
            int i = 0;
        }

        psHeight = pSoldier.sBoundingBoxHeight;
        psWidth = pSoldier.sBoundingBoxWidth;
    }

    void GetSoldierAnimOffsets(SOLDIERTYPE? pSoldier, out int sOffsetX, out int sOffsetY)
    {
        AnimationSurfaceTypes usAnimSurface = AnimationControl.GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == Globals.INVALID_ANIMATION_SURFACE)
        {
            sOffsetX = 0;
            sOffsetY = 0;

            return;
        }

        sOffsetX = pSoldier.sBoundingBoxOffsetX;
        sOffsetY = pSoldier.sBoundingBoxOffsetY;
    }

    public static void GetSoldierScreenPos(SOLDIERTYPE? pSoldier, out int psScreenX, out int psScreenY)
    {
        int sMercScreenX, sMercScreenY;
        float dOffsetX, dOffsetY;
        AnimationSurfaceTypes usAnimSurface = 0;
        //		ETRLEObject *pTrav;

        //        usAnimSurface = GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == Globals.INVALID_ANIMATION_SURFACE)
        {
            psScreenX = 0;
            psScreenY = 0;
            return;
        }

        // Get 'true' merc position
        dOffsetX = pSoldier.dXPos - Globals.gsRenderCenterX;
        dOffsetY = pSoldier.dYPos - Globals.gsRenderCenterY;

        IsometricUtils.FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, out float dTempX_S, out float dTempY_S);

        //pTrav = &(gAnimSurfaceDatabase[ usAnimSurface ].hVideoObject.pETRLEObject[ pSoldier.usAniFrame ] );

        sMercScreenX = ((Globals.gsVIEWPORT_END_X - Globals.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sMercScreenY = ((Globals.gsVIEWPORT_END_Y - Globals.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sMercScreenX -= Globals.gsRenderWorldOffsetX;
        sMercScreenY -= Globals.gsRenderWorldOffsetY;
        sMercScreenY -= Globals.gpWorldLevelData[pSoldier.sGridNo].sHeight;

        // Adjust for render height
        sMercScreenY += Globals.gsRenderHeight;

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
        AnimationSurfaceTypes usAnimSurface;

        usAnimSurface = AnimationControl.GetSoldierAnimationSurface(pSoldier, pSoldier.usAnimState);

        if (usAnimSurface == Globals.INVALID_ANIMATION_SURFACE)
        {
            psScreenX = 0;
            psScreenY = 0;
            return;
        }

        // Get 'true' merc position
        dOffsetX = pSoldier.dXPos - Globals.gsRenderCenterX;
        dOffsetY = pSoldier.dYPos - Globals.gsRenderCenterY;

        IsometricUtils.FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, out float dTempX_S, out float dTempY_S);

        sMercScreenX = ((Globals.gsVIEWPORT_END_X - Globals.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sMercScreenY = ((Globals.gsVIEWPORT_END_Y - Globals.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sMercScreenX -= Globals.gsRenderWorldOffsetX;
        sMercScreenY -= Globals.gsRenderWorldOffsetY;

        // Adjust for render height
        sMercScreenY += Globals.gsRenderHeight;
        sMercScreenY -= Globals.gpWorldLevelData[pSoldier.sGridNo].sHeight;

        sMercScreenY -= pSoldier.sHeightAdjustment;

        psScreenX = sMercScreenX;
        psScreenY = sMercScreenY;
    }

    public static bool GridNoOnScreen(int sGridNo)
    {
        int sAllowance = 20;

        if (Globals.gsVIEWPORT_WINDOW_START_Y == 20)
        {
            sAllowance = 40;
        }

        IsometricUtils.ConvertGridNoToXY(sGridNo, out int sNewCenterWorldX, out int sNewCenterWorldY);

        // Get screen coordinates for current position of soldier
        IsometricUtils.GetWorldXYAbsoluteScreenXY((int)sNewCenterWorldX, (int)sNewCenterWorldY, out int sWorldX, out int sWorldY);

        // ATE: OK, here, adjust the top value so that it's a tile and a bit over, because of our mercs!
        if (sWorldX >= Globals.gsTopLeftWorldX
            && sWorldX <= Globals.gsBottomRightWorldX
            && sWorldY >= (Globals.gsTopLeftWorldY + sAllowance)
            && sWorldY <= (Globals.gsBottomRightWorldY + 20))
        {
            return true;
        }

        return false;
    }

    public static bool SoldierOnScreen(int usID)
    {
        SOLDIERTYPE? pSoldier;

        // Get pointer of soldier
        pSoldier = Globals.MercPtrs[usID];

        return GridNoOnScreen(pSoldier.sGridNo);
    }


    bool SoldierOnVisibleWorldTile(SOLDIERTYPE? pSoldier)
    {
        return IsometricUtils.GridNoOnVisibleWorldTile(pSoldier.sGridNo);
    }

    bool SoldierLocationRelativeToScreen(int sGridNo, int usReasonID, out WorldDirections pbDirection, out ScrollDirection puiScrollFlags)
    {
        int sY, sX;
        int sDistToCenterY, sDistToCenterX;

        puiScrollFlags = 0;

        sX = IsometricUtils.CenterX(sGridNo);
        sY = IsometricUtils.CenterY(sGridNo);

        // Get screen coordinates for current position of soldier
        IsometricUtils.GetWorldXYAbsoluteScreenXY((int)(sX / Globals.CELL_X_SIZE), (int)(sY / Globals.CELL_Y_SIZE), out int sWorldX, out int sWorldY);

        // Find the diustance from render center to true world center
        sDistToCenterX = Globals.gsRenderCenterX - Globals.gCenterWorldX;
        sDistToCenterY = Globals.gsRenderCenterY - Globals.gCenterWorldY;

        // From render center in world coords, convert to render center in "screen" coords
        IsometricUtils.FromCellToScreenCoordinates(sDistToCenterX, sDistToCenterY, out int sScreenCenterX, out int sScreenCenterY);

        // Subtract screen center
        sScreenCenterX += Globals.gsCX;
        sScreenCenterY += Globals.gsCY;

        // Adjust for offset origin!
        sScreenCenterX += 0;
        sScreenCenterY += 10;

        // Get direction
        //*pbDirection = atan8( sScreenCenterX, sScreenCenterY, sWorldX, sWorldY );
        pbDirection = SoldierControl.atan8(Globals.gsRenderCenterX, Globals.gsRenderCenterY, (int)sX, (int)sY);

        // Check values!
        if (sWorldX > (sScreenCenterX + 20))
        {
            puiScrollFlags |= ScrollDirection.SCROLL_RIGHT;
        }
        if (sWorldX < (sScreenCenterX - 20))
        {
            puiScrollFlags |= ScrollDirection.SCROLL_LEFT;
        }
        if (sWorldY > (sScreenCenterY + 20))
        {
            puiScrollFlags |= ScrollDirection.SCROLL_DOWN;
        }
        if (sWorldY < (sScreenCenterY - 20))
        {
            puiScrollFlags |= ScrollDirection.SCROLL_UP;
        }

        // If we are on screen, stop
        if (sWorldX >= Globals.gsTopLeftWorldX
            && sWorldX <= Globals.gsBottomRightWorldX
            && sWorldY >= Globals.gsTopLeftWorldY
            && sWorldY <= (Globals.gsBottomRightWorldY + 20))
        {
            // CHECK IF WE ARE DONE...
            if (fCountdown > gScrollSlideInertiaDirection[(int)pbDirection])
            {
                fCountdown = 0;
                return false;
            }
            else
            {
                fCountdown++;
            }
        }

        return true;
    }

    bool IsPointInSoldierBoundingBox(SOLDIERTYPE? pSoldier, int sX, int sY)
    {
        // Get Rect contained in the soldier
        GetSoldierScreenRect(pSoldier, out Rectangle aRect);

        if (IsometricUtils.IsPointInScreenRect(sX, sY, aRect))
        {
            return true;
        }

        return false;
    }


    bool FindRelativeSoldierPosition(SOLDIERTYPE? pSoldier, out TILE_FLAG usFlags, int sX, int sY)
    {
        float dRelPer;


        // Get Rect contained in the soldier
        GetSoldierScreenRect(pSoldier, out Rectangle aRect);

        if (IsometricUtils.IsPointInScreenRectWithRelative(sX, sY, aRect, out int sRelX, out int sRelY))
        {
            dRelPer = (float)sRelY / (aRect.Bottom - aRect.Top);

            // Determine relative positions
            switch (Globals.gAnimControl[pSoldier.usAnimState].ubHeight)
            {
                case AnimationHeights.ANIM_STAND:

                    if (dRelPer < .2)
                    {
                        usFlags = TILE_FLAG.HEAD;
                        return true;
                    }
                    else if (dRelPer < .6)
                    {
                        usFlags = TILE_FLAG.MID;
                        return true;
                    }
                    else
                    {
                        usFlags = TILE_FLAG.FEET;
                        return true;
                    }

                case AnimationHeights.ANIM_CROUCH:

                    if (dRelPer < .2)
                    {
                        usFlags = TILE_FLAG.HEAD;
                        return true;
                    }
                    else if (dRelPer < .7)
                    {
                        usFlags = TILE_FLAG.MID;
                        return true;
                    }
                    else
                    {
                        usFlags = TILE_FLAG.FEET;
                        return true;
                    }
            }
        }

        usFlags = 0;
        return false;
    }

    // VERY quickly finds a soldier at gridno , ( that is visible )
    int QuickFindSoldier(int sGridNo)
    {
        int cnt;
        SOLDIERTYPE? pSoldier = null;

        // Loop through all mercs and make go
        for (cnt = 0; cnt < Globals.guiNumMercSlots; cnt++)
        {
            pSoldier = Globals.MercSlots[cnt];

            if (pSoldier != null)
            {
                if (pSoldier.sGridNo == sGridNo && pSoldier.bVisible != -1)
                {
                    return (int)cnt;
                }
            }

        }

        return Globals.NOBODY;
    }


    void GetGridNoScreenPos(int sGridNo, int ubLevel, out int psScreenX, out int psScreenY)
    {
        int sScreenX, sScreenY;
        float dOffsetX, dOffsetY;

        // Get 'true' merc position
        dOffsetX = IsometricUtils.CenterX(sGridNo) - Globals.gsRenderCenterX;
        dOffsetY = IsometricUtils.CenterY(sGridNo) - Globals.gsRenderCenterY;

        // OK, DONT'T ASK... CONVERSION TO PROPER Y NEEDS THIS...
        dOffsetX -= Globals.CELL_Y_SIZE;

        IsometricUtils.FloatFromCellToScreenCoordinates(dOffsetX, dOffsetY, out float dTempX_S, out float dTempY_S);

        sScreenX = ((Globals.gsVIEWPORT_END_X - Globals.gsVIEWPORT_START_X) / 2) + (int)dTempX_S;
        sScreenY = ((Globals.gsVIEWPORT_END_Y - Globals.gsVIEWPORT_START_Y) / 2) + (int)dTempY_S;

        // Adjust starting screen coordinates
        sScreenX -= Globals.gsRenderWorldOffsetX;
        sScreenY -= Globals.gsRenderWorldOffsetY;

        sScreenY += Globals.gsRenderHeight;

        // Adjust for world height
        sScreenY -= Globals.gpWorldLevelData[sGridNo].sHeight;

        // Adjust for level height
        if (ubLevel > 0)
        {
            sScreenY -= Globals.ROOF_LEVEL_HEIGHT;
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

// RETURN FLAGS FOR FINDSOLDIER
public enum FIND_SOLDIER_RESPONSES
{
    SELECTED_MERC = 0x000000002,
    OWNED_MERC = 0x000000004,
    ENEMY_MERC = 0x000000008,
    UNCONSCIOUS_MERC = 0x000000020,
    DEAD_MERC = 0x000000040,
    VISIBLE_MERC = 0x000000080,
    ONDUTY_MERC = 0x000000100,
    NOINTERRUPT_MERC = 0x000000200,
    NEUTRAL_MERC = 0x000000400,

    NONE = -1,
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
