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
        return (1000 + 50 * wString.Length);
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
                sLeft = (640 - gusUIMessageWidth) / 2,
                sTop = 150,
                sRight = (640 - gusUIMessageWidth) / 2 + gusUIMessageWidth,
                sBottom = 150 + gusUIMessageHeight,
                sX = (640 - gusUIMessageWidth) / 2,
                sY = 150,
                BltCallback = this.RenderUIMessage,
            };

            giUIMessageOverlay = this.renderDirty.RegisterVideoOverlay(0, VideoOverlayDesc);
        }

        gfUseSkullIconMessage = fUseSkullIcon;
    }

    private void RenderUIMessage(VIDEO_OVERLAY pBlitter)
    {
        // Shade area first...
        VideoSurfaceManager.ShadowVideoSurfaceRect(pBlitter.uiDestBuff, pBlitter.sX, pBlitter.sY, pBlitter.sX + gusUIMessageWidth - 2, pBlitter.sY + gusUIMessageHeight - 2);

        this.mercTextBox.RenderMercPopUpBoxFromIndex(iUIMessageBox, pBlitter.sX, pBlitter.sY, pBlitter.uiDestBuff);

        video.InvalidateRegion(pBlitter.sX, pBlitter.sY, pBlitter.sX + gusUIMessageWidth, pBlitter.sY + gusUIMessageHeight);
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
