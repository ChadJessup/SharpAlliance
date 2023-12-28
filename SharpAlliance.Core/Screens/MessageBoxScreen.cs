﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Screens;

public class MessageBoxScreen : IScreen
{
    private readonly IInputManager inputs;
    private readonly ILogger<MessageBoxScreen> logger;
    private readonly MessageBoxSubSystem messageBoxSubSystem;
    private static IVideoManager video;
    private readonly MercTextBox mercTextBox;
    private readonly List<GUI_BUTTON> buttonList = [];

    public MessageBoxScreen(
        ILogger<MessageBoxScreen> logger,
        IClockManager clockManager,
        IInputManager inputs,
        IInputManager inputManager,
        IVideoManager videoManager,
        MessageBoxSubSystem messageBoxSubSystem,
        MercTextBox mercTextBox)
    {
        this.inputs = inputManager;
        this.logger = logger;
        this.messageBoxSubSystem = messageBoxSubSystem;
        video = videoManager;
        this.mercTextBox = mercTextBox;
    }

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        if (gfNewMessageBox)
        {
            this.buttonList.Add(gMsgBox.uiOKButton);
            this.buttonList.Add(gMsgBox.uiYESButton);
            this.buttonList.Add(gMsgBox.uiNOButton);
            this.buttonList.Add(gMsgBox.uiUnusedButton);
            this.buttonList.AddRange(gMsgBox.uiButton);

            // If in game screen....
            if (gfStartedFromGameScreen || gfStartedFromMapScreen)
            {
                if (gfStartedFromGameScreen)
                {
                    HandleUI.HandleTacticalUILoseCursorFromOtherScreen();
                }
                else
                {
                    MapScreenInterfaceMap.HandleMAPUILoseCursorFromOtherScreen();
                }

                gfStartedFromGameScreen = false;
                gfStartedFromMapScreen = false;
            }

            gfNewMessageBox = false;

            return ValueTask.FromResult(ScreenName.MSG_BOX_SCREEN);
        }

        ButtonSubSystem.UnmarkButtonsDirty(this.buttonList);

        // Render the box!
        if (gMsgBox.fRenderBox)
        {
            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiButton[0]);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiButton[1]);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiButton[2]);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiButton[3]);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OK))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.CANCEL))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKCONTRACT))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOCONTRACT))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERICCONTRACT))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERIC))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.CONTINUESTOP))
            {
                // Exit messagebox
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOLIE))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKSKIP))
            {
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiYESButton);
                ButtonSubSystem.MarkAButtonDirty(gMsgBox.uiNOButton);
            }

            this.mercTextBox.RenderMercPopUpBoxFromIndex(
                gMsgBox.iBoxId,
                gMsgBox.Location,
                SurfaceType.FRAME_BUFFER);
            //gMsgBox.fRenderBox = false;
            // ATE: Render each frame...
        }

        // Render buttons
        ButtonSubSystem.RenderButtons(this.buttonList);

        video.EndFrameBufferRender();

        // carter, need key shortcuts for clearing up message boxes
        // Check for esc 
        while (this.inputs.DequeueEvent(out var InputEvent) == true)
        {
            if (InputEvent.IsMouseDown(MouseButton.Left))
            {

            }

            foreach (var keyevent in InputEvent.KeyEvents)
            {
                if (keyevent.Key == Key.Up)
                {
                    if ((keyevent.Key == Key.Escape) || (keyevent.Key == Key.N))
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_NO;
                        }
                    }

                    if (keyevent.Key == Key.Enter)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_YES;
                        }
                        else if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OK))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                        else if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.CONTINUESTOP))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (keyevent.Key == Key.O)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OK))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (keyevent.Key == Key.Y)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_YES;
                        }
                    }
                    if (keyevent.Key == Key.Number1)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (keyevent.Key == Key.Number2)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (keyevent.Key == Key.Number3)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (keyevent.Key == Key.Number4)
                    {
                        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                }
            }
        }

        if (gMsgBox.bHandled != 0)
        {
            RenderWorld.SetRenderFlags(RenderingFlags.FULL);
            return ValueTask.FromResult(this.ExitMsgBox(gMsgBox.bHandled));
        }

        return ValueTask.FromResult(ScreenName.MSG_BOX_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }

    public ScreenName ExitMsgBox(MessageBoxReturnCode ubExitCode)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf, pSrcBuf;

        // Delete popup!
        MercTextBox.RemoveMercPopupBoxFromIndex(gMsgBox.iBoxId);
        gMsgBox.iBoxId = -1;

        //Delete buttons!
        if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.FOUR_NUMBERED_BUTTONS))
        {
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[0]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[1]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[2]);
            ButtonSubSystem.RemoveButton(gMsgBox.uiButton[3]);
        }
        else
        {
            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OK))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNO))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERICCONTRACT))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.GENERIC))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.YESNOLIE))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiOKButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.CONTINUESTOP))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }

            if (gMsgBox.usFlags.HasFlag(MSG_BOX_FLAG.OKSKIP))
            {
                ButtonSubSystem.RemoveButton(gMsgBox.uiYESButton);
                ButtonSubSystem.RemoveButton(gMsgBox.uiNOButton);
            }
        }

        // Delete button images
        ButtonSubSystem.UnloadButtonImage(gMsgBox.iButtonImages);

        // Unpause game....
        GameClock.UnLockPauseState();
        GameClock.UnPauseGame();
        // UnPause timers as well....
        TimerControl.PauseTime(false);

        // Restore mouse restriction region...
        MouseSubSystem.RestrictMouseCursor(gOldCursorLimitRectangle);


        gfInMsgBox = false;

        // Call done callback!
        if (gMsgBox.ExitCallback != null)
        {
            gMsgBox.ExitCallback(ubExitCode);
        }


        //if ur in a non gamescreen and DONT want the msg box to use the save buffer, unset gfDontOverRideSaveBuffer in ur callback
        if (((gMsgBox.uiExitScreen != ScreenName.GAME_SCREEN) || (fRestoreBackgroundForMessageBox == true)) && gfDontOverRideSaveBuffer)
        {
            // restore what we have under here...
            //            pSrcBuf =  video.LockVideoSurface(gMsgBox.uiSaveBuffer, out uiSrcPitchBYTES);
            //            pDestBuf = video.LockVideoSurface(Surfaces.FRAME_BUFFER, out uiDestPitchBYTES);
            //
            //            video.Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
            //                        pSrcBuf, uiSrcPitchBYTES,
            //                        gMsgBox.sX, gMsgBox.sY,
            //                        0, 0,
            //                        gMsgBox.usWidth, gMsgBox.usHeight);

            //            video.UnLockVideoSurface(gMsgBox.uiSaveBuffer);
            //            video.UnLockVideoSurface(Surfaces.FRAME_BUFFER);

            video.InvalidateRegion(new(gMsgBox.Location, gMsgBox.Size));
        }

        fRestoreBackgroundForMessageBox = false;
        gfDontOverRideSaveBuffer = true;

        if (MessageBoxSubSystem.fCursorLockedToArea == true)
        {
            this.inputs.GetMousePos(out Point pPosition);

            if ((pPosition.X > MessageBoxRestrictedCursorRegion.Right)
                || (pPosition.X > MessageBoxRestrictedCursorRegion.Left)
                && (pPosition.Y < MessageBoxRestrictedCursorRegion.Top)
                && (pPosition.Y > MessageBoxRestrictedCursorRegion.Bottom))
            {
                MouseSubSystem.SimulateMouseMovement(pOldMousePosition);
            }

            MessageBoxSubSystem.fCursorLockedToArea = false;
            MouseSubSystem.RestrictMouseCursor(MessageBoxRestrictedCursorRegion);
        }

        // Remove region
        MouseSubSystem.MSYS_RemoveRegion(gMsgBox.BackRegion);

        // Remove save buffer!
        video.DeleteVideoSurfaceFromIndex(gMsgBox.uiSaveBuffer);

        switch (gMsgBox.uiExitScreen)
        {
            case ScreenName.GAME_SCREEN:

                if (Overhead.InOverheadMap())
                {
                    gfOverheadMapDirty = true;
                }
                else
                {
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                }
                break;
            case ScreenName.MAP_SCREEN:
                fMapPanelDirty = true;
                break;
        }

        if (gfFadeInitialized)
        {
            // SetPendingNewScreen(ScreenName.FADE_SCREEN);
            return ScreenName.FADE_SCREEN;
        }

        return gMsgBox.uiExitScreen;
    }
}
