using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
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
                    //UINT32 uiDestPitchBYTES, uiSrcPitchBYTES;
                    //UINT8	 *pDestBuf, *pSrcBuf;

                    if (this.messageBoxSubSystem.gfStartedFromGameScreen)
                    {
                        HandleTacticalUILoseCursorFromOtherScreen();
                    }
                    else
                    {
                        HandleMAPUILoseCursorFromOtherScreen();
                    }

                    this.messageBoxSubSystem.gfStartedFromGameScreen = false;
                    this.messageBoxSubSystem.gfStartedFromMapScreen = false;
                }

                this.messageBoxSubSystem.gfNewMessageBox = false;

                return ValueTask.FromResult(ScreenName.MSG_BOX_SCREEN);
            }

            UnmarkButtonsDirty();

            // Render the box!
            if (this.messageBoxSubSystem.gMsgBox.fRenderBox)
            {
                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[0]);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[1]);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[2]);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiButton[3]);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_OK)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_CANCEL)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNO)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_OKCONTRACT)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNOCONTRACT)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_GENERICCONTRACT)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_GENERIC)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_CONTINUESTOP)
                {
                    // Exit messagebox
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNOLIE)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiOKButton);
                }

                if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_OKSKIP)
                {
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiYESButton);
                    MarkAButtonDirty(this.messageBoxSubSystem.gMsgBox.uiNOButton);
                }


                RenderMercPopUpBoxFromIndex(this.messageBoxSubSystem.gMsgBox.iBoxId, this.messageBoxSubSystem.gMsgBox.sX, this.messageBoxSubSystem.gMsgBox.sY, FRAME_BUFFER);
                //this.messageBoxSubSystem.gMsgBox.fRenderBox = FALSE;
                // ATE: Render each frame...
            }

            // Render buttons
            RenderButtons();

            EndFrameBufferRender();

            // carter, need key shortcuts for clearing up message boxes
            // Check for esc 
            while (DequeueEvent(&InputEvent) == true)
            {
                if (InputEvent.usEvent == KEY_UP)
                {
                    if ((InputEvent.usParam == ESC) || (InputEvent.usParam == 'n'))
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNO)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_NO;
                        }
                    }

                    if (InputEvent.usParam == ENTER)
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNO)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_YES;
                        }
                        else if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_OK)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_OK;
                        }
                        else if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_CONTINUESTOP)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == 'o')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_OK)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_OK;
                        }
                    }
                    if (InputEvent.usParam == 'y')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_YESNO)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = MSG_BOX_RETURN_YES;
                        }
                    }
                    if (InputEvent.usParam == '1')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = 1;
                        }
                    }
                    if (InputEvent.usParam == '2')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = 1;
                        }
                    }
                    if (InputEvent.usParam == '3')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = 1;
                        }
                    }
                    if (InputEvent.usParam == '4')
                    {
                        if (this.messageBoxSubSystem.gMsgBox.usFlags & MSG_BOX_FLAG_FOUR_NUMBERED_BUTTONS)
                        {
                            // Exit messagebox
                            this.messageBoxSubSystem.gMsgBox.bHandled = 1;
                        }
                    }

                }
            }

            if (this.messageBoxSubSystem.gMsgBox.bHandled)
            {
                SetRenderFlags(RENDER_FLAG_FULL);
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
