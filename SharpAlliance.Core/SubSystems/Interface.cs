using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems;

public class Interface
{
    private static Dictionary<ICON_IMAGES, ButtonPic> iIconImages = [];

    private static int iOverlayMessageBox = -1;
    private static int iUIMessageBox = -1;
    private static HVOBJECT guiCLOSE;
    private static HVOBJECT guiDEAD;
    private static HVOBJECT guiHATCH;
    private static HVOBJECT guiINTEXT;
    private static HVOBJECT guiGUNSM;
    private static HVOBJECT guiP1ITEMS;
    private static HVOBJECT guiP2ITEMS;
    private static HVOBJECT guiP3ITEMS;
    private static HVOBJECT guiBUTTONBORDER;
    private static HVOBJECT guiAIMCUBES;
    private static HVOBJECT guiAIMBARS;
    private static HVOBJECT guiVEHINV;
    private static HVOBJECT guiBURSTACCUM;
    private static HVOBJECT guiPORTRAITICONS;
    private static HVOBJECT guiRADIO;
    private static HVOBJECT guiRADIO2;
    private static HVOBJECT guiCOMPANEL;
    private static HVOBJECT guiITEMPOINTERHATCHES;
    private static HVOBJECT guiCOMPANELB;
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

        foreach (var iconImage in Enum.GetValues<ICON_IMAGES>())
        {
            iIconImages[iconImage] = new();
        }
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

        InterfaceCursors.HideUICursor();

        Interface.ResetPhysicsTrajectoryUI();

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

    private static void ResetPhysicsTrajectoryUI()
    {
        throw new NotImplementedException();
    }

    internal static bool InitializeTacticalInterface()
    {
        VSURFACE_DESC vs_desc;
        VOBJECT_DESC VObjectDesc;

        // Load button Interfaces
        iIconImages[ICON_IMAGES.WALK_IMAGES] = ButtonSubSystem.LoadButtonImage("INTERFACE\\newicons3.sti", -1, 3, 4, 5, -1);
        iIconImages[ICON_IMAGES.SNEAK_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 6, 7, 8, -1);
        iIconImages[ICON_IMAGES.RUN_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 0, 1, 2, -1);
        iIconImages[ICON_IMAGES.CRAWL_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 9, 10, 11, -1);
        iIconImages[ICON_IMAGES.LOOK_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 12, 13, 14, -1);
        iIconImages[ICON_IMAGES.TALK_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 21, 22, 23, -1);
        iIconImages[ICON_IMAGES.HAND_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 18, 19, 20, -1);
        iIconImages[ICON_IMAGES.CANCEL_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 15, 16, 17, -1);

        iIconImages[ICON_IMAGES.TARGETACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 24, 25, 26, -1);
        iIconImages[ICON_IMAGES.KNIFEACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 27, 28, 29, -1);
        iIconImages[ICON_IMAGES.AIDACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 30, 31, 32, -1);
        iIconImages[ICON_IMAGES.PUNCHACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 33, 34, 35, -1);
        iIconImages[ICON_IMAGES.BOMBACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 36, 37, 38, -1);
        iIconImages[ICON_IMAGES.TOOLKITACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 39, 40, 41, -1);
        iIconImages[ICON_IMAGES.WIRECUTACTIONC_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.WALK_IMAGES], -1, 42, 43, 44, -1);

        iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES] = ButtonSubSystem.LoadButtonImage("INTERFACE\\door_op2.sti", -1, 9, 10, 11, -1);
        iIconImages[ICON_IMAGES.EXAMINE_DOOR_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 12, 13, 14, -1);
        iIconImages[ICON_IMAGES.LOCKPICK_DOOR_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 21, 22, 23, -1);
        iIconImages[ICON_IMAGES.BOOT_DOOR_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 25, 26, 27, -1);
        iIconImages[ICON_IMAGES.CROWBAR_DOOR_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 0, 1, 2, -1);
        iIconImages[ICON_IMAGES.USE_KEY_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 3, 4, 5, -1);
        iIconImages[ICON_IMAGES.USE_KEYRING_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 6, 7, 8, -1);
        iIconImages[ICON_IMAGES.EXPLOSIVE_DOOR_IMAGES] = ButtonSubSystem.UseLoadedButtonImage(iIconImages[ICON_IMAGES.OPEN_DOOR_IMAGES], -1, 15, 16, 17, -1);

        // Load interface panels
        
        // failing the CHECKF after this will cause you to lose your mouse
        var path  ="INTERFACE\\IN_TEXT.STI";

        guiINTEXT = video.GetVideoObject(path);
        video.SetVideoSurfaceTransparency(guiINTEXT, FROMRGB(255, 0, 0));


        // LOAD CLOSE ANIM
        guiCLOSE = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\p_close.sti"));

        // LOAD DEAD ANIM
        guiDEAD = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\p_dead.sti"));

        // LOAD HATCH
        guiHATCH = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\hatch.sti"));

        // LOAD INTERFACE GUN PICTURES
        guiGUNSM = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\mdguns.sti"));

        // LOAD INTERFACE ITEM PICTURES
        guiP1ITEMS = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\mdp1items.sti"));

        // LOAD INTERFACE ITEM PICTURES
        guiP2ITEMS = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\mdp2items.sti"));

        // LOAD INTERFACE ITEM PICTURES
        guiP3ITEMS = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\mdp3items.sti"));

        // LOAD INTERFACE BUTTON BORDER
        guiBUTTONBORDER = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\button_frame.sti"));

        // LOAD AIM CUBES
        guiAIMCUBES = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\aimcubes.sti"));
        // LOAD AIM BARS
        guiAIMBARS = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\aimbars.sti"));
        guiVEHINV = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\inventor.sti"));
        guiBURSTACCUM = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\burst1.sti"));
        guiPORTRAITICONS = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\portraiticons.sti"));

        // LOAD RADIO
        guiRADIO = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\radio.sti"));

        // LOAD RADIO2
        guiRADIO2 = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\radio2.sti"));

        // LOAD com panel 2
        guiCOMPANEL = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\communicationpopup.sti"));

        // LOAD ITEM GRIDS....
        guiITEMPOINTERHATCHES = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\itemgrid.sti"));
        guiCOMPANELB = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\communicationpopup_2.sti"));

        // Alocate message surfaces
//        vs_desc.usWidth = 640;
//        vs_desc.usHeight = 20;
//        vs_desc.ubBitDepth = 16;
//        CHECKF(AddVideoSurface(&vs_desc, &(gTopMessage.uiSurface)));

        InterfaceItems.InitItemInterface();

        RadarScreen.InitRadarScreen();

        InterfacePanel.InitTEAMSlots();

        // Init popup box images
        //	CHECKF( LoadTextMercPopupImages( BASIC_MERC_POPUP_BACKGROUND, BASIC_MERC_POPUP_BORDER ) );

        return (true);
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

public enum ICON_IMAGES
{
    WALK_IMAGES = 0,
    SNEAK_IMAGES,
    RUN_IMAGES,
    CRAWL_IMAGES,
    LOOK_IMAGES,
    TALK_IMAGES,
    HAND_IMAGES,
    CANCEL_IMAGES,

    TARGETACTIONC_IMAGES,
    KNIFEACTIONC_IMAGES,
    AIDACTIONC_IMAGES,
    PUNCHACTIONC_IMAGES,
    BOMBACTIONC_IMAGES,

    OPEN_DOOR_IMAGES,
    EXAMINE_DOOR_IMAGES,
    LOCKPICK_DOOR_IMAGES,
    BOOT_DOOR_IMAGES,
    CROWBAR_DOOR_IMAGES,
    USE_KEY_IMAGES,
    USE_KEYRING_IMAGES,
    EXPLOSIVE_DOOR_IMAGES,

    TOOLKITACTIONC_IMAGES,
    WIRECUTACTIONC_IMAGES,

    NUM_ICON_IMAGES
};
