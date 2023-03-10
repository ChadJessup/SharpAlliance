﻿using System;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems;

public class InterfaceCursors
{
    private readonly ILogger<InterfaceCursors> logger;
    private readonly GameSettings gGameSettings;
    private readonly IInputManager inputs;
    private readonly IClockManager clock;
    private readonly WorldManager world;
    private readonly Overhead overhead;

    public InterfaceCursors(
        ILogger<InterfaceCursors> logger,
        IClockManager clock,
        GameSettings gameSettings,
        WorldManager worldManager,
        Overhead overhead,
        IInputManager inputManager)
    {
        this.logger = logger;
        this.clock = clock;
        this.gGameSettings = gameSettings;
        this.world = worldManager;
        this.overhead = overhead;
        this.inputs = inputManager;
    }

    public const int DISPLAY_AP_INDEX = (int)TileDefines.MOCKFLOOR1;

    public const int SNAPCURSOR_AP_X_STARTVAL = 18;
    public const int SNAPCURSOR_AP_Y_STARTVAL = 9;

    public const int LOOSE_CURSOR_DELAY = 300;
    static bool gfLooseCursorOn = false;
    static short gsLooseCursorGridNo = Globals.NOWHERE;
    static int guiLooseCursorID = 0;
    static uint guiLooseCursorTimeOfLastUpdate = 0;

    public static bool SetUICursor(UICursorDefines uiNewCursor)
    {
        Globals.guiOldUICursor = Globals.guiCurUICursor;
        Globals.guiCurUICursor = uiNewCursor;

        return true;
    }

    static bool fHideCursor = false;
    bool DrawUICursor()
    {
        int usMapPos;
        LEVELNODE? pNode;
        TileDefines usTileCursor; // might be ushort, but testing this out.

        //RaiseMouseToLevel( (byte)gsInterfaceLevel );

        HandleLooseCursorDraw();


        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (Globals.gpItemPointer != null)
        {
            MouseSubSystem.MSYS_ChangeRegionCursor(Globals.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);

            // Check if we are in the viewport region...
            if (Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                DrawItemTileCursor();
            }
            else
            {
                DrawItemFreeCursor();
            }
            return true;
        }

        if (IsometricUtils.GetMouseMapPos(out usMapPos))
        {
            Globals.gusCurMousePos = usMapPos;

            if (Globals.guiCurUICursor == UICursorDefines.NO_UICURSOR)
            {
                MouseSubSystem.MSYS_ChangeRegionCursor(Globals.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);
                return true;
            }

            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILE))
            {
                if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = this.world.AddTopmostToTail(Globals.gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS3));
                }
                else
                {
                    pNode = this.world.AddTopmostToTail(Globals.gusCurMousePos, GetSnapCursorIndex(Globals.gUICursors[Globals.guiCurUICursor].usAdditionalData));
                }
                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    this.world.AddOnRoofToHead(Globals.gusCurMousePos, GetSnapCursorIndex(Globals.gUICursors[Globals.guiCurUICursor].usAdditionalData));
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }

            Globals.gfTargetDropPos = false;

            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING) && !(Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
            {
                Globals.gfTargetDropPos = true;
                Globals.gusTargetDropPos = Globals.gusCurMousePos;

                if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // If we are over a target, jump to that....
                    if (Globals.gfUIFullTargetFound)
                    {
                        Globals.gusTargetDropPos = Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo;
                    }

                    // Put tile on the floor
                    this.world.AddTopmostToTail(Globals.gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    Globals.gpWorldLevelData[Globals.gusTargetDropPos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    Globals.gpWorldLevelData[Globals.gusTargetDropPos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
            }

            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILEAPDEPENDENT))
            {
                // Add depending on AP status
                usTileCursor = Globals.gUICursors[Globals.guiCurUICursor].usAdditionalData;

                // ATE; Is the current guy in steath mode?
                if (Globals.gusSelectedSoldier != Globals.NOBODY)
                {
                    if (Globals.MercPtrs[Globals.gusSelectedSoldier].bStealthMode)
                    {
                        usTileCursor = TileDefines.FIRSTPOINTERS9;
                    }
                }

                if (Globals.gfUIDisplayActionPointsInvalid || Globals.gsCurrentActionPoints == 0)
                {
                    usTileCursor = TileDefines.FIRSTPOINTERS6;

                    // ATE; Is the current guy in steath mode?
                    if (Globals.gusSelectedSoldier != Globals.NOBODY)
                    {
                        if (Globals.MercPtrs[Globals.gusSelectedSoldier].bStealthMode)
                        {
                            usTileCursor = TileDefines.FIRSTPOINTERS10;
                        }
                    }
                }

                if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    pNode = this.world.AddTopmostToTail(Globals.gusCurMousePos, GetSnapCursorIndex(TileDefines.FIRSTPOINTERS14));
                }
                else
                {
                    pNode = this.world.AddTopmostToTail(Globals.gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                }

                pNode.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                pNode.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
                {
                    // Put one on the roof as well
                    this.world.AddOnRoofToHead(Globals.gusCurMousePos, GetSnapCursorIndex(usTileCursor));
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                }
            }


            // If snapping - remove from main viewport
            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.SNAPPING))
            {
                // Hide mouse region cursor
                MouseSubSystem.MSYS_ChangeRegionCursor(Globals.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);

                // Set Snapping Cursor
                DrawSnappingCursor();
            }


            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING))
            {
                switch (Globals.guiCurUICursor)
                {
                    case UICursorDefines.MOVE_VEHICLE_UICURSOR:

                        // Set position for APS
                        Globals.gfUIDisplayActionPointsCenter = false;
                        Globals.gUIDisplayActionPointsOffX = 16;
                        Globals.gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_WALK_UICURSOR:
                    case UICursorDefines.MOVE_RUN_UICURSOR:

                        // Set position for APS
                        Globals.gfUIDisplayActionPointsCenter = false;
                        Globals.gUIDisplayActionPointsOffX = 16;
                        Globals.gUIDisplayActionPointsOffY = 14;
                        break;

                    case UICursorDefines.MOVE_SWAT_UICURSOR:

                        // Set position for APS
                        Globals.gfUIDisplayActionPointsCenter = false;
                        Globals.gUIDisplayActionPointsOffX = 16;
                        Globals.gUIDisplayActionPointsOffY = 10;
                        break;

                    case UICursorDefines.MOVE_PRONE_UICURSOR:

                        // Set position for APS
                        Globals.gfUIDisplayActionPointsCenter = false;
                        Globals.gUIDisplayActionPointsOffX = 16;
                        Globals.gUIDisplayActionPointsOffY = 9;
                        break;
                }

                fHideCursor = false;

                if (!fHideCursor)
                {
                    MouseSubSystem.MSYS_ChangeRegionCursor(Globals.gViewportRegion, Globals.gUICursors[Globals.guiCurUICursor].usFreeCursorName);

                }
                else
                {
                    // Hide
                    MouseSubSystem.MSYS_ChangeRegionCursor(Globals.gViewportRegion, CURSOR.VIDEO_NO_CURSOR);
                }

            }

            if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.CENTERAPS))
            {
                Globals.gfUIDisplayActionPointsCenter = true;
            }
        }
        return true;
    }

    bool HideUICursor()
    {
        HandleLooseCursorHide();

        // OK, WE OVERRIDE HERE CURSOR DRAWING FOR THINGS LIKE
        if (Globals.gpItemPointer != null)
        {
            // Check if we are in the viewport region...
            if (Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                HideItemTileCursor();
                return true;
            }
        }

        if (Globals.guiCurUICursor == UICursorDefines.NO_UICURSOR)
        {
            //Do nothing here
            return true;
        }

        if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.SHOWTILE | UICURSOR.SHOWTILEAPDEPENDENT))
        {
            WorldManager.RemoveAllTopmostsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
            WorldManager.RemoveAllOnRoofsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.FIRSTPOINTERS);
        }


        if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING)
            && !(Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.DONTSHOW2NDLEVEL)))
        {
            if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
            {
                WorldManager.RemoveTopmost(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                WorldManager.RemoveTopmost(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS9);

                if (Globals.gfTargetDropPos)
                {
                    WorldManager.RemoveTopmost(Globals.gusTargetDropPos, TileDefines.FIRSTPOINTERS14);
                    WorldManager.RemoveTopmost(Globals.gusTargetDropPos, TileDefines.FIRSTPOINTERS9);
                }
            }

        }


        // If snapping - remove from main viewport
        if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.SNAPPING))
        {
            // hide Snapping Cursor
            EraseSnappingCursor();
        }

        if (Globals.gUICursors[Globals.guiCurUICursor].uiFlags.HasFlag(UICURSOR.FREEFLOWING))
        {
            // Nothing special here...
        }

        return true;
    }

    static bool fShowAP = true;
    void DrawSnappingCursor()
    {
        SOLDIERTYPE? pSoldier;

        if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
        {
            this.overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier);

        }

        LEVELNODE newUIElem;

        // If we are in draw item mode, do nothing here but call the fuctiuon
        switch (Globals.guiCurUICursor)
        {
            case UICursorDefines.NO_UICURSOR:
                break;

            case UICursorDefines.NORMAL_SNAPUICURSOR:

                this.world.AddTopmostToHead(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS1);
                Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_RUN_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR:
                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODRUN1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODRUN1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_WALK_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR:
                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODWALK1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODWALK1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_SWAT_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR:
                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODSWAT1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODSWAT1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_PRONE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR:
                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODPRONE1, 0, -TileDefine.WALL_HEIGHT - 8 - 6, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.GOODPRONE1, 0, -6, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR:
            case UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR:
                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                }
                else
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.VEHICLEMOVE1, 0, 0, out newUIElem);
                }

                newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                break;

            case UICursorDefines.MOVE_REALTIME_UICURSOR:
                break;

            case UICursorDefines.CANNOT_MOVE_UICURSOR:

                if (Globals.gsInterfaceLevel > 0)
                {
                    this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.BADMARKER1, 0, -TileDefine.WALL_HEIGHT - 8, out newUIElem);
                    newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (GameSettings.fOptions[TOPTION.CURSOR_3D])
                    {
                        this.world.AddTopmostToHead(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }

                    this.world.AddOnRoofToHead(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS14);
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pOnRoofHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                }
                else
                {
                    this.world.AddTopmostToHead(Globals.gusCurMousePos, TileDefines.BADMARKER1);
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

                    if (GameSettings.fOptions[TOPTION.CURSOR_3D])
                    {
                        this.world.AddTopmostToHead(Globals.gusCurMousePos, TileDefines.FIRSTPOINTERS13);
                        Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                        Globals.gpWorldLevelData[Globals.gusCurMousePos].pTopmostHead.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
                    }
                }

                break;
        }

        // Add action points
        if (Globals.gfUIDisplayActionPoints)
        {
            if (Globals.gfUIDisplayActionPointsInvalid)
            {
                if (COUNTERDONE(CURSORFLASH))
                {
                    RESETCOUNTER(CURSORFLASH);

                    fShowAP = !fShowAP;
                }
            }
            else
            {
                fShowAP = true;
            }

            if (Globals.gsInterfaceLevel > 0)
            {
                this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL - TileDefine.WALL_HEIGHT - 10, out newUIElem);
            }
            else
            {
                this.world.AddUIElem(Globals.gusCurMousePos, TileDefines.DISPLAY_AP_INDEX, SNAPCURSOR_AP_X_STARTVAL, SNAPCURSOR_AP_Y_STARTVAL, out newUIElem);
            }

            newUIElem.uiFlags |= LEVELNODEFLAGS.DISPLAY_AP;
            newUIElem.uiAPCost = Globals.gsCurrentActionPoints;
            newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;

            if (!fShowAP)
            {
                Globals.gfUIDisplayActionPointsBlack = true;
            }
        }
    }

    void EraseSnappingCursor()
    {
        WorldManager.RemoveAllTopmostsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
        WorldManager.RemoveAllTopmostsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        WorldManager.RemoveAllObjectsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        WorldManager.RemoveAllOnRoofsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.FIRSTPOINTERS, TileTypeDefines.LASTPOINTERS);
        WorldManager.RemoveAllOnRoofsOfTypeRange(Globals.gusCurMousePos, TileTypeDefines.MOCKFLOOR, TileTypeDefines.MOCKFLOOR);
    }

    void StartLooseCursor(short sGridNo, int uiCursorID)
    {
        gfLooseCursorOn = true;
        guiLooseCursorID = uiCursorID;
        guiLooseCursorTimeOfLastUpdate = Globals.GetJA2Clock();
        gsLooseCursorGridNo = sGridNo;
    }

    void HandleLooseCursorDraw()
    {
        if ((Globals.GetJA2Clock() - guiLooseCursorTimeOfLastUpdate) > LOOSE_CURSOR_DELAY)
        {
            gfLooseCursorOn = false;
        }

        if (gfLooseCursorOn)
        {
            this.world.AddUIElem(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4, 0, 0, out LEVELNODE newUIElem);
            newUIElem.ubShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
            newUIElem.ubNaturalShadeLevel = Shading.DEFAULT_SHADE_LEVEL;
        }
    }

    void HandleLooseCursorHide()
    {
        if (gfLooseCursorOn)
        {
            WorldManager.RemoveTopmost(gsLooseCursorGridNo, TileDefines.FIRSTPOINTERS4);
        }
    }

    TileDefines GetSnapCursorIndex(TileDefines usAdditionalData)
    {
        // OK, this function will get the 'true' index for drawing the cursor....
        if (GameSettings.fOptions[TOPTION.CURSOR_3D])
        {
            return usAdditionalData switch
            {
                TileDefines.FIRSTPOINTERS2 => TileDefines.FIRSTPOINTERS13,
                TileDefines.FIRSTPOINTERS3 => TileDefines.FIRSTPOINTERS14,
                TileDefines.FIRSTPOINTERS4 => TileDefines.FIRSTPOINTERS15,
                TileDefines.FIRSTPOINTERS5 => TileDefines.FIRSTPOINTERS16,
                TileDefines.FIRSTPOINTERS6 => TileDefines.FIRSTPOINTERS17,
                TileDefines.FIRSTPOINTERS7 => TileDefines.FIRSTPOINTERS18,
                TileDefines.FIRSTPOINTERS9 => TileDefines.FIRSTPOINTERS19,
                TileDefines.FIRSTPOINTERS10 => TileDefines.FIRSTPOINTERS20,
                _ => usAdditionalData,
            };
        }
        else
        {
            return usAdditionalData;
        }
    }
}


public record UICursor(
    UICursorDefines uiCursorID,
    UICURSOR uiFlags,
    CURSOR usFreeCursorName,
    TileDefines usAdditionalData)
{
    public UICursor(
        UICursorDefines uiCursorID,
        UICURSOR uiFlags,
        CURSOR usFreeCursorName,
        int usAdditionalData)
        : this(uiCursorID, uiFlags, usFreeCursorName, (TileDefines)usAdditionalData)
    { }
}


[Flags]
public enum UICURSOR
{
    FREEFLOWING = 0x00000002,
    SNAPPING = 0x00000004,
    SHOWTILE = 0x00000008,
    FLASHING = 0x00000020,
    CENTERAPS = 0x00000040,
    SHOWTILEAPDEPENDENT = 0x00000080,
    DONTSHOW2NDLEVEL = 0x00000100,
}
