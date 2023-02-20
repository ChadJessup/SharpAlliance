﻿using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using Veldrid;

namespace SharpAlliance.Core.Screens;

public class MapScreen : IScreen
{
    private readonly IVideoManager video;
    private readonly MapScreenInterfaceMap mapScreenInterface;
    private readonly MessageSubSystem messages;

    public MapScreen(
        IVideoManager videoManager,
        MapScreenInterfaceMap mapScreenInterfaceMap,
        MessageSubSystem messageSubSystem)
    {
        this.video = videoManager;
        this.mapScreenInterface = mapScreenInterfaceMap;
        this.messages = messageSubSystem;
    }

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }
    public bool fMapPanelDirty { get; set; }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.MAP_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        this.mapScreenInterface.SetUpBadSectorsList();

        // setup message box system
        this.messages.InitGlobalMessageList();

        // init palettes for big map
        this.mapScreenInterface.InitializePalettesForMap();

        // set up mapscreen fast help text
        this.mapScreenInterface.SetUpMapScreenFastHelpText();

        // set up leave list arrays for dismissed mercs
        this.mapScreenInterface.InitLeaveList();

        this.video.AddVideoObject("INTERFACE\\group_confirm.sti", out var idx1);
        this.mapScreenInterface.guiUpdatePanel = idx1;

        this.video.AddVideoObject("INTERFACE\\group_confirm_tactical.sti", out var idx2);
        this.mapScreenInterface.guiUpdatePanelTactical = idx2;

        return ValueTask.FromResult(true);
    }

    public void HandlePreloadOfMapGraphics()
    {
    }

    public void Dispose()
    {
    }

    public static void RenderMapRegionBackground()
    {
        // renders to save buffer when dirty flag set

        if (Globals.fMapPanelDirty == false)
        {
            Globals.gfMapPanelWasRedrawn = false;

            // not dirty, leave
            return;
        }

        // don't bother if showing sector inventory instead of the map!!!
        if (!Globals.fShowMapInventoryPool)
        {
            // draw map
            MapScreenInterfaceMap.DrawMap();
        }


        // blit in border
        RenderMapBorder();

        if (Globals.ghAttributeBox != -1)
        {
            PopUpBox.ForceUpDateOfBox(Globals.ghAttributeBox);
        }

        if (Globals.ghTownMineBox != -1)
        {
            // force update of town mine info boxes
            PopUpBox.ForceUpDateOfBox(Globals.ghTownMineBox);
        }

        MapscreenMarkButtonsDirty();

        RestoreExternBackgroundRect(261, 0, 640 - 261, 359);

        // don't bother if showing sector inventory instead of the map!!!
        if (!Globals.fShowMapInventoryPool)
        {
            // if Skyrider can and wants to talk to us
            if (MapScreenHelicopter.IsHelicopterPilotAvailable())
            {
                // see if Skyrider has anything new to tell us
                CheckAndHandleSkyriderMonologues();
            }
        }

        // reset dirty flag
        Globals.fMapPanelDirty = false;

        Globals.gfMapPanelWasRedrawn = true;

        return;
    }

    public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask Deactivate()
    {
        throw new System.NotImplementedException();
    }
}

public enum TOWNS
{
    BLANK_SECTOR = 0,
    OMERTA,
    DRASSEN,
    ALMA,
    GRUMM,
    TIXA,
    CAMBRIA,
    SAN_MONA,
    ESTONI,
    ORTA,
    BALIME,
    MEDUNA,
    CHITZENA,
    NUM_TOWNS
}
