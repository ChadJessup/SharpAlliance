using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

namespace SharpAlliance.Core.Screens
{
    public class MessageBoxScreen : IScreen
    {
        private readonly ILogger<MessageBoxScreen> logger;
        private readonly MessageBoxSubSystem messageBoxSubSystem;

        public MessageBoxScreen(
            ILogger<MessageBoxScreen> logger,
            IClockManager clockManager,
            IInputManager inputs,
            MessageBoxSubSystem messageBoxSubSystem)
        {
            this.logger = logger;
            this.messageBoxSubSystem = messageBoxSubSystem;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {
            InputAtom InputEvent;

            if (this.messageBoxSubSystem.gfNewMessageBox)
            {
                // If in game screen....
                if ((this.messageBoxSubSystem.gfStartedFromGameScreen) || (this.messageBoxSubSystem.gfStartedFromMapScreen))
                {
                    //int uiDestPitchBYTES, uiSrcPitchBYTES;
                    //Ubyte	 *pDestBuf, *pSrcBuf;

                    if (this.messageBoxSubSystem.gfStartedFromGameScreen)
                    {
                        HandleUI.HandleTacticalUILoseCursorFromOtherScreen();
                    }
                    else
                    {
                        MapScreenInterfaceMap.HandleMAPUILoseCursorFromOtherScreen();
                    }

                    this.messageBoxSubSystem.gfStartedFromGameScreen = false;
                    this.messageBoxSubSystem.gfStartedFromMapScreen = false;
                }

                this.messageBoxSubSystem.gfNewMessageBox = false;

                return ValueTask.FromResult(ScreenName.MSG_BOX_SCREEN);
            }

            ButtonSubSystem.UnmarkButtonsDirty();

            // Render the box!
            if (this.messageBoxSubSystem.gMsgBox.fRenderBox)
            {
                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[0]);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[1]);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[2]);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[3]);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CANCEL))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKCONTRACT))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOCONTRACT))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERICCONTRACT))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_GENERIC))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CONTINUESTOP))
                {
                    // Exit messagebox
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNOLIE))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OKSKIP))
                {
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    ButtonSubSystem.MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                MercTextBox.RenderMercPopUpBoxFromIndex(this.messageBoxSubSystem.gMsgBox.iBoxId, this.messageBoxSubSystem.gMsgBox.sX, this.messageBoxSubSystem.gMsgBox.sY, Surfaces.FRAME_BUFFER);
                //this.messageBoxSubSystem.gMsgBox.fRenderBox = false;
                // ATE: Render each frame...
            }

            // Render buttons
            ButtonSubSystem.RenderButtons();

            VeldridVideoManager.EndFrameBufferRender();

            // carter, need key shortcuts for clearing up message boxes
            // Check for esc 
            while (DequeueEvent(out InputEvent) == true)
            {
                if (InputEvent.usEvent == KEY_UP)
                {
                    if ((InputEvent.usParam == ESC) || (InputEvent.usParam == 'n'))
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_NO;
                        }
                    }

                    if (InputEvent.usParam == ENTER)
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_YES;
                        }
                        else if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                        else if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_CONTINUESTOP))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == 'o')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_OK))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == 'y')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_YESNO))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_YES;
                        }
                    }
                    if (InputEvent.usParam == '1')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == '2')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == '3')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == '4')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags.HasFlag(MessageBoxFlags.MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS))
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MessageBoxReturnCode.MSG_BOX_RETURN_OK;
                        }
                    }

                }
            }

            if (this.messageBoxSubSystem.gMsgBox.bHandled != 0)
            {
                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                return (ExitMsgBox(this.messageBoxSubSystem.gMsgBox.bHandled));
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

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
        }
    }
}
