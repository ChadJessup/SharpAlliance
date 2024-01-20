using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Interface
{
    private static int iOverlayMessageBox = -1;
    private static int iUIMessageBox = -1;

    private static IVideoManager video;
    private readonly MercTextBox mercTextBox;
    private readonly RenderDirty renderDirty;

    public Interface(
        ILogger<Interface> logger,
        RenderDirty renderDirty,
        IVideoManager videoManager,
        MercTextBox mercTextBox)
    {
        video = videoManager;
        this.mercTextBox = mercTextBox;
        this.renderDirty = renderDirty;
    }

    public void BeginUIMessage(params string[] pFontString)
    {
        this.InternalBeginUIMessage(false, pFontString);
    }

    private static int CalcUIMessageDuration(string wString)
    {
        // base + X per letter
        return 1000 + 50 * wString.Length;
    }

    private void InternalBeginUIMessage(bool fUseSkullIcon, params string[] MsgString)
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc;
        guiUIMessageTime = ClockManager.GetJA2Clock();
        guiUIMessageTimeDelay = CalcUIMessageDuration(string.Format(MsgString.First(), MsgString[1..]));

        // Override it!
        this.mercTextBox.OverrideMercPopupBox(gpUIMessageOverrideMercBox);

        //SetPrepareMercPopupFlags( MERC_POPUP_PREPARE_FLAGS_TRANS_BACK | MERC_POPUP_PREPARE_FLAGS_MARGINS );

        if (fUseSkullIcon)
        {
            MercTextBox.SetPrepareMercPopupFlags(MERC_POPUP_PREPARE_FLAGS.MARGINS | MERC_POPUP_PREPARE_FLAGS.SKULLICON);
        }
        else
        {
            MercTextBox.SetPrepareMercPopupFlags(MERC_POPUP_PREPARE_FLAGS.MARGINS | MERC_POPUP_PREPARE_FLAGS.STOPICON);
        }

        // Prepare text box
        iUIMessageBox = this.mercTextBox.PrepareMercPopupBox(iUIMessageBox, MercTextBoxBackground.BASIC_MERC_POPUP_BACKGROUND, MercTextBoxBorder.BASIC_MERC_POPUP_BORDER, string.Format(MsgString.First(), MsgString[1..]), 200, 10, 0, 0, out gusUIMessageWidth, out gusUIMessageHeight);

        // Set it back!
        this.mercTextBox.ResetOverrideMercPopupBox();

        if (giUIMessageOverlay != -1)
        {
            RenderDirty.RemoveVideoOverlay(giUIMessageOverlay);

            giUIMessageOverlay = -1;
        }

        if (giUIMessageOverlay == -1)
        {
            //memset(&VideoOverlayDesc, 0, sizeof(VideoOverlayDesc));

            // Set Overlay
            VideoOverlayDesc = new()
            {
                Rectangle = new()
                {
                    X = (640 - gusUIMessageWidth) / 2,
                    Y = 150,
                    Width = (640 - gusUIMessageWidth) / 2 + gusUIMessageWidth,
                    Height = 150 + gusUIMessageHeight,
                },
                Location = new((640 - gusUIMessageWidth) / 2, 150),
                BltCallback = this.RenderUIMessage,
            };

            giUIMessageOverlay = this.renderDirty.RegisterVideoOverlay(0, VideoOverlayDesc);
        }

        gfUseSkullIconMessage = fUseSkullIcon;
    }

    private void RenderUIMessage(VIDEO_OVERLAY pBlitter)
    {
        // Shade area first...
        VideoSurfaceManager.ShadowVideoSurfaceRect(
            pBlitter.uiDestBuff, new(pBlitter.Location, new(pBlitter.Location.X + gusUIMessageWidth - 2, pBlitter.Location.Y + gusUIMessageHeight - 2)));

        this.mercTextBox.RenderMercPopUpBoxFromIndex(iUIMessageBox, pBlitter.Location, pBlitter.uiDestBuff);

        video.InvalidateRegion(new(pBlitter.Location, new(pBlitter.Location.X + gusUIMessageWidth, pBlitter.Location.Y + gusUIMessageHeight)));
    }

    public static void DirtyMercPanelInterface(SOLDIERTYPE pSoldier, int ubDirtyLevel)
    {
        if (pSoldier.bTeam == gbPlayerNum)
        {
            // ONly set to a higher level!
            if (fInterfacePanelDirty < ubDirtyLevel)
            {
                fInterfacePanelDirty = ubDirtyLevel;
            }
        }

    }

    internal static void EndUIMessage()
    {
        uint uiClock = GetJA2Clock();

        if (giUIMessageOverlay != -1)
        {
            if (gfUseSkullIconMessage)
            {
                if ((uiClock - guiUIMessageTime) < 300)
                {
                    return;
                }
            }

            //		DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "Removing Overlay message") );

            RenderDirty.RemoveVideoOverlay(giUIMessageOverlay);

            // Remove popup as well....
            if (iUIMessageBox != -1)
            {
                MercTextBox.RemoveMercPopupBoxFromIndex(iUIMessageBox);
                iUIMessageBox = -1;
            }

            giUIMessageOverlay = -1;

        }
        //iUIMessageBox = -1;
    }

    internal static void ResetInterface()
    {
        LEVELNODE? pNode;

        if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
        {
            return;
        }

        // find out if we need to show any menus
        Assignments.DetermineWhichAssignmentMenusCanBeShown();
        Assignments.CreateDestroyAssignmentPopUpBoxes();

        HideUICursor();

        ResetPhysicsTrajectoryUI();

        if (gfUIHandleSelection != 0)
        {
            if (gsSelectedLevel > 0)
            {
                WorldManager.RemoveRoof(gsSelectedGridNo, TileIndexes.GOODRING1);
                WorldManager.RemoveRoof(gsSelectedGridNo, TileIndexes.FIRSTPOINTERS2);
            }
            else
            {
                WorldManager.RemoveObject(gsSelectedGridNo, TileIndexes.FIRSTPOINTERS2);
                WorldManager.RemoveObject(gsSelectedGridNo, TileIndexes.GOODRING1);

            }
        }

        if (gfUIHandleShowMoveGrid != 0)
        {
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS4);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS9);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS2);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS13);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS15);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS19);
            WorldManager.RemoveTopmost(gsUIHandleShowMoveGridLocation, TileIndexes.FIRSTPOINTERS20);
        }

        if (fInterfacePanelDirty != 0)
        {
            fInterfacePanelDirty = 0;
        }


        // Reset int tile cursor stuff
        if (gfUIShowCurIntTile)
        {
//            if (gsUICurIntTileEffectGridNo != NOWHERE)
//            {
//                //Find our tile!
//                pNode = gpWorldLevelData[gsUICurIntTileEffectGridNo].pStructHead;
//
//                while (pNode != NULL)
//                {
//                    if (pNode->usIndex == gusUICurIntTileEffectIndex)
//                    {
//                        pNode->ubShadeLevel = gsUICurIntTileOldShade;
//                        pNode->uiFlags &= (~LEVELNODE_DYNAMIC);
//                        break;
//                    }
//
//                    pNode = pNode->pNext;
//                }
//            }
        }
    }
}

public enum INTERFACE
{
    MAPSCREEN = 0x00000001,
    NORENDERBUTTONS = 0x00000002,
    LOCKEDLEVEL1 = 0x00000004,
    SHOPKEEP_INTERFACE = 0x00000008,
}

public enum MOVEMENT
{
    MENU_LOOK = 1,
    MENU_ACTIONC = 2,
    MENU_HAND = 3,
    MENU_TALK = 4,
    MENU_RUN = 5,
    MENU_WALK = 6,
    MENU_SWAT = 7,
    MENU_PRONE = 8,
}

public enum InterfacePanelDefines
{
    SM_PANEL,
    TEAM_PANEL,
    NUM_UI_PANELS
}

// Interface level enums
public class InterfaceLevel
{
    public const int I_GROUND_LEVEL = 0;
    public const int I_ROOF_LEVEL = 1;
    public const int I_NUMLEVELS = 2;
};

// GLOBAL DEFINES FOR SOME UI FLAGS
[Flags]
public enum ARROWS
{
    HIDE_UP = 0x00000002,
    HIDE_DOWN = 0x00000004,
    SHOW_UP_BESIDE = 0x00000008,
    SHOW_DOWN_BESIDE = 0x00000020,
    SHOW_UP_ABOVE_Y = 0x00000040,
    SHOW_DOWN_BELOW_Y = 0x00000080,
    SHOW_DOWN_BELOW_G = 0x00000200,
    SHOW_DOWN_BELOW_YG = 0x00000400,
    SHOW_DOWN_BELOW_GG = 0x00000800,
    SHOW_UP_ABOVE_G = 0x00002000,
    SHOW_UP_ABOVE_YG = 0x00004000,
    SHOW_UP_ABOVE_GG = 0x00008000,
    SHOW_UP_ABOVE_YY = 0x00020000,
    SHOW_DOWN_BELOW_YY = 0x00040000,
    SHOW_UP_ABOVE_CLIMB = 0x00080000,
    SHOW_UP_ABOVE_CLIMB2 = 0x00400000,
    SHOW_UP_ABOVE_CLIMB3 = 0x00800000,
    SHOW_DOWN_CLIMB = 0x02000000,
}
