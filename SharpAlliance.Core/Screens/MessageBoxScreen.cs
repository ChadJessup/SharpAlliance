using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Screens;

public class MessageBoxScreen : IScreen
{
    private readonly IInputManager inputs;
    private readonly ILogger<MessageBoxScreen> logger;
    private static MessageBoxSubSystem messageBoxSubSystem;
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
        messageBoxSubSystem = messageBoxSubSystem;
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
            pSrcBuf =  video.Surfaces[gMsgBox.uiSaveBuffer];
            pDestBuf = video.Surfaces[SurfaceType.FRAME_BUFFER];
            
            video.Blt16BPPTo16BPP(
                pDestBuf,
                pSrcBuf,
                gMsgBox.Location,
                new(0, 0),
                gMsgBox.Size);

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

    internal static void DoScreenIndependantMessageBox(string zString, MSG_BOX_FLAG usFlags, MSGBOX_CALLBACK returnCallback)
    {
        Rectangle CenteringRect = new(0, 0, 640, INV_INTERFACE_START_Y);
        DoScreenIndependantMessageBoxWithRect(zString, usFlags, returnCallback, CenteringRect);
    }

    private static void DoScreenIndependantMessageBoxWithRect(string zString, MSG_BOX_FLAG usFlags, MSGBOX_CALLBACK? ReturnCallback, Rectangle? pCenteringRect)
    {
        /// which screen are we in?
        // Map Screen (excluding AI Viewer)
            if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
            {

                // auto resolve is a special case
                if (guiCurrentScreen == ScreenName.AUTORESOLVE_SCREEN)
                {
                    messageBoxSubSystem.DoMessageBox(MessageBoxStyle.MSG_BOX_BASIC_STYLE, zString, ScreenName.AUTORESOLVE_SCREEN, usFlags, ReturnCallback, ref pCenteringRect);
                }
                else
                {
                    // set up for mapscreen
                    MapScreenInterface.DoMapMessageBoxWithRect(MessageBoxStyle.MSG_BOX_BASIC_STYLE, zString, ScreenName.MAP_SCREEN, usFlags, ReturnCallback, pCenteringRect);
                }
            }

            //Laptop
            else if (guiCurrentScreen == ScreenName.LAPTOP_SCREEN)
            {
            // set up for laptop
            SubSystems.LaptopSubSystem.Laptop.DoLapTopSystemMessageBoxWithRect(MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT, zString, ScreenName.LAPTOP_SCREEN, usFlags, ReturnCallback, pCenteringRect);
            }

            //Save Load Screen
            else if (guiCurrentScreen == ScreenName.SAVE_LOAD_SCREEN)
            {
                SaveLoadScreen.DoSaveLoadMessageBoxWithRect(MessageBoxStyle.MSG_BOX_BASIC_STYLE, zString, ScreenName.SAVE_LOAD_SCREEN, usFlags, ReturnCallback, pCenteringRect);
            }

            //Options Screen
            else if (guiCurrentScreen == ScreenName.OPTIONS_SCREEN)
            {
            OptionsScreen.DoOptionsMessageBoxWithRect(MessageBoxStyle.MSG_BOX_BASIC_STYLE, zString, ScreenName.OPTIONS_SCREEN, usFlags, ReturnCallback, pCenteringRect);
            }

            // Tactical
            else if (guiCurrentScreen == ScreenName.GAME_SCREEN)
            {
               messageBoxSubSystem.DoMessageBox(MessageBoxStyle.MSG_BOX_BASIC_STYLE, zString, guiCurrentScreen, usFlags, ReturnCallback, ref pCenteringRect);
            }
    }
}
