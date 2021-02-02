using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace SharpAlliance.Core.SubSystems
{
    public enum ButtonTextJustifies
    {
        BUTTON_TEXT_LEFT = -1,
        BUTTON_TEXT_CENTER = 0,
        BUTTON_TEXT_RIGHT = 1,
    }

    [Flags]
    public enum GUI_BTN
    {
        NONE = 0,
        DUPLICATE_VOBJ = 1,
        EXTERNAL_VOBJ = 2,
    }

    public class ButtonSubSystem : ISharpAllianceManager
    {
        public const int MAX_GENERIC_PICS = 40;
        public const int MAX_BUTTON_ICONS = 40;
        public const int MAX_BUTTON_PICS = 256;
        public const int MAX_BUTTONS = 400;

        public const Surfaces BUTTON_USE_DEFAULT = Surfaces.Unknown;
        public static readonly int? BUTTON_NO_FILENAME = null;
        public static readonly GuiCallback BUTTON_NO_CALLBACK = (ref GUI_BUTTON o, MouseCallbackReasons r) => { };
        public const int BUTTON_NO_IMAGE = -1;
        public const int BUTTON_NO_SLOT = -1;

        public const int BUTTON_INIT = 1;
        public const int BUTTON_WAS_CLICKED = 2;
        public bool gfDelayButtonDeletion = false;
        public bool gfPendingButtonDeletion = false;

        public const string DEFAULT_GENERIC_BUTTON_OFF = "GENBUTN.STI";
        public const string DEFAULT_GENERIC_BUTTON_ON = "GENBUTN2.STI";
        public const string DEFAULT_GENERIC_BUTTON_OFF_HI = "GENBUTN3.STI";
        public const string DEFAULT_GENERIC_BUTTON_ON_HI = "GENBUTN4.STI";

        private readonly ILogger<ButtonSubSystem> logger;
        private readonly GameContext gameContext;
        private readonly FontSubSystem fonts;

        private IInputManager inputs;
        private MouseSubSystem mouse;
        private IVideoManager video;

        public bool IsInitialized { get; private set; }

        private DISABLED_STYLE gbDisabledButtonStyle;
        private GUI_BUTTON gpCurrentFastHelpButton;

        // flag to state we wish to render buttons on the one after the next pass through render buttons
        private bool fPausedMarkButtonsDirtyFlag = false;
        private bool fDisableHelpTextRestoreFlag = false;

        private bool gfRenderHilights = true;

        private int ButtonPicsLoaded;

        private Surfaces ButtonDestBuffer = Surfaces.BACKBUFFER;
        private uint ButtonDestPitch = 640 * 2;
        private uint ButtonDestBPP = 16;

        public GUI_BUTTON[] ButtonList = new GUI_BUTTON[MAX_BUTTONS];
        private HVOBJECT[] GenericButtonGrayed = new HVOBJECT[MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOffNormal = new HVOBJECT[MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOffHilite = new HVOBJECT[MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOnNormal = new HVOBJECT[MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOnHilite = new HVOBJECT[MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonBackground = new HVOBJECT[MAX_GENERIC_PICS];
        private Rgba32[] GenericButtonFillColors = new Rgba32[MAX_GENERIC_PICS];
        private ushort[] GenericButtonBackgroundIndex = new ushort[MAX_GENERIC_PICS];
        private short[] GenericButtonOffsetX = new short[MAX_GENERIC_PICS];
        private short[] GenericButtonOffsetY = new short[MAX_GENERIC_PICS];

        public const byte COMPRESS_TRANSPARENT = 0x80;
        public const byte COMPRESS_RUN_MASK = 0x7F;

        private HVOBJECT[] GenericButtonIcons = new HVOBJECT[MAX_BUTTON_ICONS];

        private int ButtonsInList = 0;

        public static GUI_BUTTON? gpAnchoredButton;
        public static GUI_BUTTON? gpPrevAnchoredButton;
        public static bool gfAnchoredState;

        public ButtonSubSystem(
            ILogger<ButtonSubSystem> logger,
            GameContext gameContext,
            FontSubSystem fontSubSystem)
        {
            this.logger = logger;
            this.gameContext = gameContext;
            this.fonts = fontSubSystem;
        }

        public async ValueTask<bool> Initialize(GameContext gameContext)
        {
            this.video = (gameContext.Services.GetRequiredService<IVideoManager>() as VeldridVideoManager)!;
            this.mouse = this.gameContext.Services.GetRequiredService<MouseSubSystem>();
            this.inputs = this.gameContext.Services.GetRequiredService<IInputManager>();

            this.IsInitialized = await this.InitializeButtonImageManager(
                Surfaces.Unknown,
                -1,
                -1);

            return this.IsInitialized;
        }

        public List<ButtonPics> ButtonPictures { get; } = new(MAX_BUTTON_PICS);

        private async ValueTask<bool> InitializeButtonImageManager(Surfaces DefaultBuffer, int DefaultPitch, int DefaultBPP)
        {
            VOBJECT_DESC vo_desc = new();
            byte Pix;
            int x;

            // Set up the default settings
            if (DefaultBuffer != BUTTON_USE_DEFAULT)
            {
                this.ButtonDestBuffer = DefaultBuffer;
            }
            else
            {
                this.ButtonDestBuffer = Surfaces.FRAME_BUFFER;
            }

            if (DefaultPitch != (int)BUTTON_USE_DEFAULT)
            {
                this.ButtonDestPitch = (uint)DefaultPitch;
            }
            else
            {
                this.ButtonDestPitch = 640 * 2;
            }

            if (DefaultBPP != (int)BUTTON_USE_DEFAULT)
            {
                this.ButtonDestBPP = (uint)DefaultBPP;
            }
            else
            {
                this.ButtonDestBPP = 16;
            }

            // Blank out all QuickButton images
            for (x = 0; x < MAX_BUTTON_PICS; x++)
            {
                var bp = new ButtonPics
                {
                    vobj = null,
                    Grayed = -1,
                    OffNormal = -1,
                    OffHilite = -1,
                    OnNormal = -1,
                    OnHilite = -1,
                };

                this.ButtonPictures.Add(bp);
            }

            this.ButtonPicsLoaded = 0;

            // Blank out all Generic button data
            for (x = 0; x < MAX_GENERIC_PICS; x++)
            {
                //this.GenericButtonGrayed[x] = null;
                //this.GenericButtonOffNormal[x] = null;
                //this.GenericButtonOffHilite[x] = null;
                //this.GenericButtonOnNormal[x] = null;
                //this.GenericButtonOnHilite[x] = null;
                //this.GenericButtonBackground[x] = null;
                this.GenericButtonBackgroundIndex[x] = 0;
                this.GenericButtonFillColors[x] = new Rgba32(0, 0, 0);
                this.GenericButtonBackgroundIndex[x] = 0;
                this.GenericButtonOffsetX[x] = 0;
                this.GenericButtonOffsetY[x] = 0;
            }

            // Blank out all icon images
            for (x = 0; x < MAX_BUTTON_ICONS; x++)
            {
                // this.GenericButtonIcons[x] = null;
            }

            // Load the default generic button images
            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = DEFAULT_GENERIC_BUTTON_OFF;

            this.GenericButtonOffNormal[0] = this.video.CreateVideoObject(ref vo_desc);
            if (this.GenericButtonOffNormal[0] == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_OFF);
                return false;
            }

            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = DEFAULT_GENERIC_BUTTON_ON;

            if ((this.GenericButtonOnNormal[0] = this.video.CreateVideoObject(ref vo_desc)) == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_ON);
                return false;
            }

            // Load up the off hilite and on hilite images. We won't check for errors because if the file
            // doesn't exists, the system simply ignores that file. These are only here as extra images, they
            // aren't required for operation (only OFF Normal and ON Normal are required).
            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = DEFAULT_GENERIC_BUTTON_OFF_HI;

            this.GenericButtonOffHilite[0] = this.video.CreateVideoObject(ref vo_desc);

            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = DEFAULT_GENERIC_BUTTON_ON_HI;

            this.GenericButtonOnHilite[0] = this.video.CreateVideoObject(ref vo_desc);

            Pix = 0;
            if (!this.GetETRLEPixelValue(ref Pix, this.GenericButtonOffNormal[0], 8, 0, 0))
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't get generic button's background pixel value");
                return false;
            }

            this.GenericButtonFillColors[0] = this.GenericButtonOffNormal[0].Palette[Pix];

            return true;
        }

        internal void PlayButtonSound(int iDNum, ButtonSounds bUTTON_SOUND_CLICKED_ON)
        {
            throw new NotImplementedException();
        }

        public void RenderButtons()
        {
            if (!this.ButtonList.Any())
            {
                return;
            }

            int iButtonID;
            bool fOldButtonDown, fOldEnabled;
            GUI_BUTTON b;

            this.fonts.SaveFontSettings();
            for (iButtonID = 0; iButtonID < MAX_BUTTONS; iButtonID++)
            {
                // If the button exists, and it's not owned by another object, draw it
                //Kris:  and make sure that the button isn't hidden.
                b = this.ButtonList[iButtonID];

                if (b is null)
                {
                    continue;
                }

                if (b.Area.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                {
                    // Check for buttonchanged status
                    fOldButtonDown = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON);

                    if (fOldButtonDown != b.uiOldFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                    {
                        //Something is different, set dirty!
                        b.uiFlags |= ButtonFlags.BUTTON_DIRTY;
                    }

                    // Check for button dirty flags
                    fOldEnabled = (bool)b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED);

                    if (fOldEnabled != b.uiOldFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
                    {
                        //Something is different, set dirty!
                        b.uiFlags |= ButtonFlags.BUTTON_DIRTY;
                    }

                    // If we ABSOLUTELY want to render every frame....
                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_SAVEBACKGROUND))
                    {
                        b.uiFlags |= ButtonFlags.BUTTON_DIRTY;
                    }

                    // Set old flags
                    b.uiOldFlags = b.uiFlags;

                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_FORCE_UNDIRTY))
                    {
                        b.uiFlags &= ~ButtonFlags.BUTTON_DIRTY;
                        b.uiFlags &= ~ButtonFlags.BUTTON_FORCE_UNDIRTY;
                    }

                    // Check if we need to update!
                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_DIRTY))
                    {
                        // Turn off dirty flag
                        b.uiFlags &= ~ButtonFlags.BUTTON_DIRTY;
                        this.DrawButtonFromPtr(ref b);

                        this.video.InvalidateRegion(b.Area.RegionTopLeftX, b.Area.RegionTopLeftY, b.Area.RegionBottomRightX, b.Area.RegionBottomRightY);
                    }
                }
            }

            // check if we want to render 1 frame later?
            if ((this.fPausedMarkButtonsDirtyFlag == true) && (this.fDisableHelpTextRestoreFlag == false))
            {
                this.fPausedMarkButtonsDirtyFlag = false;
                this.MarkButtonsDirty();
            }

            this.fonts.RestoreFontSettings();
        }

        private void DrawButtonFromPtr(ref GUI_BUTTON b)
        {
            // Draw the appropriate button according to button type
            this.gbDisabledButtonStyle = DISABLED_STYLE.NONE;
            switch (b.uiFlags & ButtonFlags.BUTTON_TYPES)
            {
                case ButtonFlags.BUTTON_QUICK:
                    this.DrawQuickButton(ref b);
                    break;
                case ButtonFlags.BUTTON_GENERIC:
                    this.DrawGenericButton(ref b);
                    break;
                case ButtonFlags.BUTTON_HOT_SPOT:
                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_NO_TOGGLE))
                    {
                        b.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                    }
                    return;  //hotspots don't have text, but if you want to, change this to a break!
                case ButtonFlags.BUTTON_CHECKBOX:
                    this.DrawCheckBoxButton(ref b);
                    break;
            }

            //If button has an icon, overlay it on current button.
            if (b.iIconID != -1)
            {
                this.DrawIconOnButton(ref b);
            }

            //If button has text, draw it now
            if (!string.IsNullOrWhiteSpace(b.stringText))
            {
                this.DrawTextOnButton(ref b);
            }

            //If the button is disabled, and a style has been calculated, then
            //draw the style last.
            switch (this.gbDisabledButtonStyle)
            {
                case DISABLED_STYLE.HATCHED:
                    this.DrawHatchOnButton(ref b);
                    break;
                case DISABLED_STYLE.SHADED:
                    this.DrawShadeOnButton(ref b);
                    break;
            }

            if (b.bDefaultStatus != DEFAULT_STATUS.NONE)
            {
                this.DrawDefaultOnButton(ref b);
            }
        }

        private void DrawDefaultOnButton(ref GUI_BUTTON b)
        {
            byte[] pDestBuf = this.video.LockVideoSurface(ButtonDestBuffer, out uint uiDestPitchBYTES);
            this.video.SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
            if (b.bDefaultStatus == DEFAULT_STATUS.DARKBORDER || b.bDefaultStatus == DEFAULT_STATUS.WINDOWS95)
            {
                //left (one thick)
                this.video.LineDraw(true, b.Area.RegionTopLeftX - 1, b.Area.RegionTopLeftY - 1, b.Area.RegionTopLeftX - 1, b.Area.RegionBottomRightY + 1, 0, pDestBuf);
                //top (one thick)
                this.video.LineDraw(true, b.Area.RegionTopLeftX - 1, b.Area.RegionTopLeftY - 1, b.Area.RegionBottomRightX + 1, b.Area.RegionTopLeftY - 1, 0, pDestBuf);
                //right (two thick)
                this.video.LineDraw(true, b.Area.RegionBottomRightX, b.Area.RegionTopLeftY - 1, b.Area.RegionBottomRightX, b.Area.RegionBottomRightY + 1, 0, pDestBuf);
                this.video.LineDraw(true, b.Area.RegionBottomRightX + 1, b.Area.RegionTopLeftY - 1, b.Area.RegionBottomRightX + 1, b.Area.RegionBottomRightY + 1, 0, pDestBuf);
                //bottom (two thick)
                this.video.LineDraw(true, b.Area.RegionTopLeftX - 1, b.Area.RegionBottomRightY, b.Area.RegionBottomRightX + 1, b.Area.RegionBottomRightY, 0, pDestBuf);
                this.video.LineDraw(true, b.Area.RegionTopLeftX - 1, b.Area.RegionBottomRightY + 1, b.Area.RegionBottomRightX + 1, b.Area.RegionBottomRightY + 1, 0, pDestBuf);

                this.video.InvalidateRegion(b.Area.RegionTopLeftX - 1, b.Area.RegionTopLeftY - 1, b.Area.RegionBottomRightX + 1, b.Area.RegionBottomRightY + 1);
            }

            if (b.bDefaultStatus == DEFAULT_STATUS.DOTTEDINTERIOR || b.bDefaultStatus == DEFAULT_STATUS.WINDOWS95)
            { //Draw an internal dotted rectangle.

            }

            this.video.UnLockVideoSurface(ButtonDestBuffer);
        }

        private void DrawShadeOnButton(ref GUI_BUTTON b)
        {
            byte[] pDestBuf;
            uint uiDestPitchBYTES;
            Rectangle ClipRect = new();
            ClipRect.Y = b.Area.RegionTopLeftX;
            ClipRect.Width = b.Area.RegionBottomRightX - 1;
            ClipRect.Y = b.Area.RegionTopLeftY;
            ClipRect.Height = b.Area.RegionBottomRightY - 1;
            pDestBuf = this.video.LockVideoSurface(ButtonDestBuffer, out uiDestPitchBYTES);
            this.video.Blt16BPPBufferShadowRect(ref pDestBuf, uiDestPitchBYTES, ref ClipRect);
            this.video.UnLockVideoSurface(ButtonDestBuffer);
        }

        private void DrawHatchOnButton(ref GUI_BUTTON b)
        {
            byte[] pDestBuf;
            uint uiDestPitchBYTES;
            Rectangle ClipRect = new();
            ClipRect.Y = b.Area.RegionTopLeftX;
            ClipRect.Width = b.Area.RegionBottomRightX - 1;
            ClipRect.Y = b.Area.RegionTopLeftY;
            ClipRect.Height = b.Area.RegionBottomRightY - 1;
            pDestBuf = this.video.LockVideoSurface(ButtonDestBuffer, out uiDestPitchBYTES);
            this.video.Blt16BPPBufferHatchRect(ref pDestBuf, uiDestPitchBYTES, ref ClipRect);
            this.video.UnLockVideoSurface(ButtonDestBuffer);
        }

        private void DrawTextOnButton(ref GUI_BUTTON b)
        {
            int xp, yp, width, height, TextX, TextY;
            Rectangle NewClip = new();
            Rectangle OldClip = new();
            FontColor sForeColor;

            // If this button actually has a string to print
            if (!string.IsNullOrWhiteSpace(b.stringText))
            {
                // Get the width and height of this button
                width = b.Area.RegionBottomRightX - b.Area.RegionTopLeftX;
                height = b.Area.RegionBottomRightY - b.Area.RegionTopLeftY;

                // Compute the viewable area on this button
                NewClip.X = b.XLoc + 3;
                NewClip.Width = b.XLoc + width - 3;
                NewClip.Y = b.YLoc + 2;
                NewClip.Height = b.YLoc + height - 2;

                // Get the starting coordinates to print
                TextX = NewClip.Left;
                TextY = NewClip.Top;

                // Get the current clipping area
                this.video.GetClippingRect(out OldClip);

                // Clip the button's viewable area to the screen
                if (NewClip.Left < OldClip.Left)
                {
                    NewClip.X = OldClip.Left;
                }

                // Are we off hte right side?
                if (NewClip.Y > OldClip.Width)
                {
                    return;
                }

                if (NewClip.Width > OldClip.Width)
                {
                    NewClip.Width = OldClip.Width;
                }

                // Are we off the left side?
                if (NewClip.Width < OldClip.Y)
                {
                    return;
                }

                if (NewClip.Y < OldClip.Y)
                {
                    NewClip.Y = OldClip.Y;
                }

                // Are we off the bottom of the screen?
                if (NewClip.Y > OldClip.Height)
                {
                    return;
                }

                if (NewClip.Height > OldClip.Height)
                {
                    NewClip.Height = OldClip.Height;
                }

                // Are we off the top?
                if (NewClip.Height < OldClip.Y)
                {
                    return;
                }

                // Did we clip the viewable area out of existance?
                if ((NewClip.Width <= NewClip.Y) || (NewClip.Height <= NewClip.Y))
                {
                    return;
                }

                // Set the font printing settings to the buttons viewable area
                this.fonts.SetFontDestBuffer(
                    ButtonDestBuffer,
                    NewClip.Y,
                    NewClip.Y,
                    NewClip.Width,
                    NewClip.Height,
                    false);

                // Compute the coordinates to center the text
                if (b.bTextYOffset == -1)
                {
                    yp = (((height) - this.fonts.GetFontHeight(b.usFont)) / 2) + TextY - 1;
                }
                else
                {
                    yp = b.Area.RegionTopLeftY + b.bTextYOffset;
                }

                if (b.bTextXOffset == -1)
                {
                    xp = b.bJustification switch
                    {
                        ButtonTextJustifies.BUTTON_TEXT_LEFT => TextX + 3,
                        ButtonTextJustifies.BUTTON_TEXT_RIGHT => NewClip.Width - this.fonts.StringPixLength(b.stringText, b.usFont) - 3,
                        _ => (((width - 6) - this.fonts.StringPixLength(b.stringText, b.usFont)) / 2) + TextX,
                    };
                }
                else
                {
                    xp = b.Area.RegionTopLeftX + b.bTextXOffset;
                }

                // Set the printing font to the button text font
                this.fonts.SetFont(b.usFont);

                // print the text
                this.fonts.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
                this.fonts.SetFontForeground(b.sForeColor);
                sForeColor = b.sForeColor;
                if (b.sShadowColor != FontColor.None)
                {
                    this.fonts.SetFontShadow(b.sShadowColor);
                }
                //Override the colors if necessary.
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA) && b.sForeColorHilited != FontColor.None)
                {
                    this.fonts.SetFontForeground(b.sForeColorHilited);
                    sForeColor = b.sForeColorHilited;
                }
                else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.sForeColorDown != FontColor.None)
                {
                    this.fonts.SetFontForeground(b.sForeColorDown);
                    sForeColor = b.sForeColorDown;
                }
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA) && b.sShadowColorHilited != FontColor.None)
                {
                    this.fonts.SetFontShadow(b.sShadowColorHilited);
                }
                else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.sShadowColorDown != FontColor.None)
                {
                    this.fonts.SetFontShadow(b.sShadowColorDown);
                }
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.fShiftText)
                {   // Was the button clicked on? if so, move the text slightly for the illusion
                    // that the text moved into the screen.
                    xp++;
                    yp++;
                }

                if (b.sWrappedWidth != -1)
                {
                    TextJustifies bJustified = 0;
                    switch (b.bJustification)
                    {
                        case ButtonTextJustifies.BUTTON_TEXT_LEFT:
                            bJustified = TextJustifies.LEFT_JUSTIFIED;
                            break;
                        case ButtonTextJustifies.BUTTON_TEXT_RIGHT:
                            bJustified = TextJustifies.RIGHT_JUSTIFIED;
                            break;
                        case ButtonTextJustifies.BUTTON_TEXT_CENTER:
                            bJustified = TextJustifies.CENTER_JUSTIFIED;
                            break;
                        default:
                            // Assert(0);
                            break;
                    }

                    if (b.bTextXOffset == -1)
                    {
                        //Kris:
                        //There needs to be recalculation of the start positions based on the
                        //justification and the width specified wrapped width.  I was drawing a
                        //double lined word on the right side of the button to find it drawing way
                        //over to the left.  I've added the necessary code for the right and center
                        //justification.
                        yp = b.Area.RegionTopLeftY + 2;

                        switch (b.bJustification)
                        {
                            case ButtonTextJustifies.BUTTON_TEXT_RIGHT:
                                xp = b.Area.RegionBottomRightX - 3 - b.sWrappedWidth;

                                if (b.fShiftText && b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                                {
                                    xp++;
                                    yp++;
                                }
                                break;
                            case ButtonTextJustifies.BUTTON_TEXT_CENTER:
                                xp = b.Area.RegionTopLeftX + 3 + b.sWrappedWidth / 2;

                                if (b.fShiftText && b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                                {
                                    xp++;
                                    yp++;
                                }
                                break;
                        }
                    }
                    yp += b.bTextYSubOffSet;
                    xp += b.bTextXSubOffSet;
                    this.fonts.DisplayWrappedString((ushort)xp, (ushort)yp, b.sWrappedWidth, 1, b.usFont, (byte)sForeColor, b.stringText, FontColor.FONT_MCOLOR_BLACK, false, bJustified);
                }
                else
                {
                    yp += b.bTextYSubOffSet;
                    xp += b.bTextXSubOffSet;
                    // mprintf(xp, yp, b.stringText);
                }
            }

        }

        private void DrawIconOnButton(ref GUI_BUTTON b)
        {
            int xp, yp, width, height, IconX, IconY;
            int IconW, IconH;
            Rectangle NewClip = new();
            Rectangle OldClip = new();
            ETRLEObject pTrav;
            HVOBJECT hvObject = new();

            // If there's an actual icon on this button, try to show it.
            if (b.iIconID >= 0)
            {
                // Get width and height of button area
                width = b.Area.RegionBottomRightX - b.Area.RegionTopLeftX;
                height = b.Area.RegionBottomRightY - b.Area.RegionTopLeftY;

                // Compute viewable area (inside borders)
                NewClip.Y = b.XLoc + 3;
                NewClip.Width = b.XLoc + width - 3;
                NewClip.Y = b.YLoc + 2;
                NewClip.Height = b.YLoc + height - 2;

                // Get Icon's blit start coordinates
                IconX = NewClip.Y;
                IconY = NewClip.Y;

                // Get current clip area
                this.video.GetClippingRect(out OldClip);

                // Clip button's viewable area coords to screen
                if (NewClip.Y < OldClip.Y)
                {
                    NewClip.Y = OldClip.Y;
                }

                // Is button right off the right side of the screen?
                if (NewClip.Y > OldClip.Width)
                {
                    return;
                }

                if (NewClip.Width > OldClip.Width)
                {
                    NewClip.Width = OldClip.Width;
                }

                // Is button completely off the left side of the screen?
                if (NewClip.Width < OldClip.Y)
                {
                    return;
                }

                if (NewClip.Y < OldClip.Y)
                {
                    NewClip.Y = OldClip.Y;
                }

                // Are we right off the bottom of the screen?
                if (NewClip.Y > OldClip.Height)
                {
                    return;
                }

                if (NewClip.Height > OldClip.Height)
                {
                    NewClip.Height = OldClip.Height;
                }

                // Are we off the top?
                if (NewClip.Height < OldClip.Y)
                {
                    return;
                }

                // Did we clip the viewable area out of existance?
                if ((NewClip.Width <= NewClip.Y) || (NewClip.Height <= NewClip.Y))
                {
                    return;
                }

                // Get the width and height of the icon itself
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_GENERIC))
                {
                    pTrav = (GenericButtonIcons[b.iIconID].pETRLEObject[b.usIconIndex]);
                }
                else
                {
                    this.video.GetVideoObject(b.iIconID.ToString(), out hvObject);
                    pTrav = (hvObject.pETRLEObject[b.usIconIndex]);
                }

                IconH = (pTrav.usHeight + pTrav.sOffsetY);
                IconW = (pTrav.usWidth + pTrav.sOffsetX);

                // Compute coordinates for centering the icon on the button or
                // use the offset system.
                if (b.bIconXOffset == -1)
                {
                    xp = (((width - 6) - IconW) / 2) + IconX;
                }
                else
                {
                    xp = b.Area.RegionTopLeftX + b.bIconXOffset;
                }

                if (b.bIconYOffset == -1)
                {
                    yp = (((height - 4) - IconH) / 2) + IconY;
                }
                else
                {
                    yp = b.Area.RegionTopLeftY + b.bIconYOffset;
                }

                // Was the button clicked on? if so, move the image slightly for the illusion
                // that the image moved into the screen.
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.fShiftImage)
                {
                    xp++;
                    yp++;
                }

                // Set the clipping rectangle to the viewable area of the button
                this.video.SetClippingRect(ref NewClip);
                // Blit the icon
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_GENERIC))
                {
                    this.video.BltVideoObject(GenericButtonIcons[b.iIconID], b.usIconIndex, (short)xp, (short)yp);
                }
                else
                {
                    this.video.BltVideoObject(hvObject, b.usIconIndex, (short)xp, (short)yp);
                }

                // Restore previous clip region
                this.video.SetClippingRect(ref OldClip);
            }

        }

        private void DrawCheckBoxButton(ref GUI_BUTTON b)
        {
            int UseImage;

            UseImage = 0;
            // Is button Enabled, or diabled but no "Grayed" image associated with this QuickButton?
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                // Is the button's state ON?
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    // Is the mouse over this area, and we have a hilite image?
                    if (b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA)
                        && gfRenderHilights
                        && this.inputs.gfLeftButtonState
                        && ButtonPictures[b.ImageNum].OnHilite != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OnHilite;            // Use On-Hilite image
                    }
                    else if (ButtonPictures[b.ImageNum].OnNormal != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OnNormal;            // Use On-Normal image
                    }
                }
                else
                {
                    // Is the mouse over the button, and do we have hilite image?
                    if (b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA) && gfRenderHilights &&
                          this.inputs.gfLeftButtonState &&
                            ButtonPictures[b.ImageNum].OffHilite != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OffHilite;           // Use Off-Hilite image
                    }
                    else if (ButtonPictures[b.ImageNum].OffNormal != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OffNormal;           // Use Off-Normal image
                    }
                }
            }
            else if (ButtonPictures[b.ImageNum].Grayed != -1)
            {   // Button is disabled so use the "Grayed-out" image
                UseImage = ButtonPictures[b.ImageNum].Grayed;
            }
            else //use the disabled style
            {
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    UseImage = ButtonPictures[b.ImageNum].OnHilite;
                }
                else
                {
                    UseImage = ButtonPictures[b.ImageNum].OffHilite;
                }

                switch (b.bDisabledStyle)
                {
                    case DISABLED_STYLE.DEFAULT:
                        gbDisabledButtonStyle = DISABLED_STYLE.HATCHED;
                        break;
                    case DISABLED_STYLE.HATCHED:
                    case DISABLED_STYLE.SHADED:
                        gbDisabledButtonStyle = b.bDisabledStyle;
                        break;
                }
            }

            // Display the button image
            this.video.BltVideoObject(ButtonPictures[b.ImageNum].vobj,
                                         (ushort)UseImage, b.XLoc, b.YLoc);

        }

        private void DrawGenericButton(ref GUI_BUTTON b)
        {
            int NumChunksWide, NumChunksHigh, cx, cy, width, height, hremain, wremain;
            int q, ImgNum, ox, oy;
            int iBorderHeight, iBorderWidth;
            HVOBJECT BPic;
            uint uiDestPitchBYTES;
            byte[] pDestBuf;
            Rectangle ClipRect = new();
            ETRLEObject? pTrav;

            // Select the graphics to use depending on the current state of the button
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                if (!(b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)) && (GenericButtonGrayed[b.ImageNum] == null))
                {
                    BPic = GenericButtonOffNormal[b.ImageNum];
                }
                else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    if ((b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA)) && (GenericButtonOnHilite[b.ImageNum] != null) && gfRenderHilights)
                    {
                        BPic = GenericButtonOnHilite[b.ImageNum];
                    }
                    else
                    {
                        BPic = GenericButtonOnNormal[b.ImageNum];
                    }
                }
                else
                {
                    if ((b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA)) && (GenericButtonOffHilite[b.ImageNum] != null) && gfRenderHilights)
                    {
                        BPic = GenericButtonOffHilite[b.ImageNum];
                    }
                    else
                    {
                        BPic = GenericButtonOffNormal[b.ImageNum];
                    }
                }
            }
            else if (GenericButtonGrayed[b.ImageNum] is not null)
            {
                BPic = GenericButtonGrayed[b.ImageNum];
            }
            else
            {
                BPic = GenericButtonOffNormal[b.ImageNum];
                switch (b.bDisabledStyle)
                {
                    case DISABLED_STYLE.DEFAULT:
                        gbDisabledButtonStyle = !string.IsNullOrWhiteSpace(b.stringText) ? DISABLED_STYLE.SHADED : DISABLED_STYLE.HATCHED;
                        break;
                    case DISABLED_STYLE.HATCHED:
                    case DISABLED_STYLE.SHADED:
                        gbDisabledButtonStyle = b.bDisabledStyle;
                        break;
                }
            }

            iBorderWidth = 3;
            iBorderHeight = 2;
            pTrav = null;

            // Compute the number of button "chunks" needed to be blitted
            width = b.Area.RegionBottomRightX - b.Area.RegionTopLeftX;
            height = b.Area.RegionBottomRightY - b.Area.RegionTopLeftY;
            NumChunksWide = width / iBorderWidth;
            NumChunksHigh = height / iBorderHeight;
            hremain = height % iBorderHeight;
            wremain = width % iBorderWidth;

            cx = (b.XLoc + ((NumChunksWide - 1) * iBorderWidth) + wremain);
            cy = (b.YLoc + ((NumChunksHigh - 1) * iBorderHeight) + hremain);

            // Fill the button's area with the button's background color
            this.video.ColorFillVideoSurfaceArea(
                ButtonDestBuffer,
                b.Area.RegionTopLeftX,
                b.Area.RegionTopLeftY,
                b.Area.RegionBottomRightX,
                b.Area.RegionBottomRightY,
                GenericButtonFillColors[b.ImageNum]);

            // If there is a background image, fill the button's area with it
            if (GenericButtonBackground[b.ImageNum] != null)
            {
                ox = oy = 0;
                // if the button was clicked on, adjust the background image so that we get
                // the illusion that it is sunk into the screen.
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    ox = oy = 1;
                }

                // Fill the area with the image, tilling it if need be.
                this.video.ImageFillVideoSurfaceArea(
                    ButtonDestBuffer,
                    b.Area.RegionTopLeftX + ox,
                    b.Area.RegionTopLeftY + oy,
                    b.Area.RegionBottomRightX,
                    b.Area.RegionBottomRightY,
                    GenericButtonBackground[b.ImageNum],
                    GenericButtonBackgroundIndex[b.ImageNum],
                    GenericButtonOffsetX[b.ImageNum],
                    GenericButtonOffsetY[b.ImageNum]);
            }

            // Lock the dest buffer
            pDestBuf = this.video.LockVideoSurface(ButtonDestBuffer, out uiDestPitchBYTES);

            this.video.GetClippingRect(out ClipRect);

            // Draw the button's borders and corners (horizontally)
            for (q = 0; q < NumChunksWide; q++)
            {
                if (q == 0)
                {
                    ImgNum = 0;
                }
                else
                {
                    ImgNum = 1;
                }

                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)(b.XLoc + (q * iBorderWidth)),
                                                    (int)b.YLoc,
                                                    (ushort)ImgNum, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)(b.XLoc + (q * iBorderWidth)),
                                                    (int)b.YLoc,
                                                    (ushort)ImgNum, ref ClipRect);
                }

                if (q == 0)
                {
                    ImgNum = 5;
                }
                else
                {
                    ImgNum = 6;
                }

                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)(b.XLoc + (q * iBorderWidth)),
                                                    cy, (ushort)ImgNum, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)(b.XLoc + (q * iBorderWidth)),
                                                    cy, (ushort)ImgNum, ref ClipRect);
                }

            }
            // Blit the right side corners
            if (this.video.gbPixelDepth == 16)
            {
                this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                uiDestPitchBYTES, BPic,
                                                cx, (int)b.YLoc,
                                                2, ref ClipRect);
            }
            else if (this.video.gbPixelDepth == 8)
            {
                this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                uiDestPitchBYTES, BPic,
                                                cx, (int)b.YLoc,
                                                2, ref ClipRect);
            }


            if (this.video.gbPixelDepth == 16)
            {
                this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                uiDestPitchBYTES, BPic,
                                                cx, cy, 7, ref ClipRect);
            }
            else if (this.video.gbPixelDepth == 8)
            {
                this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                uiDestPitchBYTES, BPic,
                                                cx, cy, 7, ref ClipRect);
            }
            // Draw the vertical members of the button's borders
            NumChunksHigh--;

            if (hremain != 0)
            {
                q = NumChunksHigh;
                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)b.XLoc,
                                                    (int)(b.YLoc + (q * iBorderHeight) - (iBorderHeight - hremain)),
                                                    3, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)b.XLoc,
                                                    (int)(b.YLoc + (q * iBorderHeight) - (iBorderHeight - hremain)),
                                                    3, ref ClipRect);
                }

                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    cx, (int)(b.YLoc + (q * iBorderHeight) - (iBorderHeight - hremain)),
                                                    4, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    cx, (int)(b.YLoc + (q * iBorderHeight) - (iBorderHeight - hremain)),
                                                    4, ref ClipRect);
                }
            }

            for (q = 1; q < NumChunksHigh; q++)
            {
                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)b.XLoc,
                                                    (int)(b.YLoc + (q * iBorderHeight)),
                                                    3, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    (int)b.XLoc,
                                                    (int)(b.YLoc + (q * iBorderHeight)),
                                                    3, ref ClipRect);
                }

                if (this.video.gbPixelDepth == 16)
                {
                    this.video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    cx, (int)(b.YLoc + (q * iBorderHeight)),
                                                    4, ref ClipRect);
                }
                else if (this.video.gbPixelDepth == 8)
                {
                    this.video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                                                    uiDestPitchBYTES, BPic,
                                                    cx, (int)(b.YLoc + (q * iBorderHeight)),
                                                    4, ref ClipRect);
                }
            }

            // Unlock buffer
            this.video.UnLockVideoSurface(ButtonDestBuffer);
        }

        private void DrawQuickButton(ref GUI_BUTTON b)
        {
            int UseImage;
            UseImage = 0;
            // Is button Enabled, or diabled but no "Grayed" image associated with this QuickButton?
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                // Is the button's state ON?
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    // Is the mouse over this area, and we have a hilite image?
                    if ((b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA) && gfRenderHilights &&
                            (ButtonPictures[b.ImageNum].OnHilite != -1)))
                    {
                        UseImage = ButtonPictures[b.ImageNum].OnHilite;            // Use On-Hilite image
                    }
                    else if (ButtonPictures[b.ImageNum].OnNormal != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OnNormal;            // Use On-Normal image
                    }
                }
                else
                {
                    // Is the mouse over the button, and do we have hilite image?
                    if ((b.Area.uiFlags.HasFlag(MouseRegionFlags.MOUSE_IN_AREA) && gfRenderHilights &&
                            (ButtonPictures[b.ImageNum].OffHilite != -1)))
                    {
                        UseImage = ButtonPictures[b.ImageNum].OffHilite;           // Use Off-Hilite image
                    }
                    else if (ButtonPictures[b.ImageNum].OffNormal != -1)
                    {
                        UseImage = ButtonPictures[b.ImageNum].OffNormal;           // Use Off-Normal image
                    }
                }
            }
            else if (ButtonPictures[b.ImageNum].Grayed != -1)
            {   // Button is diabled so use the "Grayed-out" image
                UseImage = ButtonPictures[b.ImageNum].Grayed;
            }
            else
            {
                UseImage = ButtonPictures[b.ImageNum].OffNormal;
                switch (b.bDisabledStyle)
                {
                    case DISABLED_STYLE.DEFAULT:
                        gbDisabledButtonStyle = !string.IsNullOrWhiteSpace(b.stringText) ? DISABLED_STYLE.SHADED : DISABLED_STYLE.HATCHED;
                        break;
                    case DISABLED_STYLE.HATCHED:
                    case DISABLED_STYLE.SHADED:
                        gbDisabledButtonStyle = b.bDisabledStyle;
                        break;
                }
            }

            // Display the button image
            this.video.BltVideoObject(ButtonPictures[b.ImageNum].vobj,
                                         (ushort)UseImage, b.XLoc, b.YLoc);
        }


        //=============================================================================
        //	MarkButtonsDirty
        //
        private void MarkButtonsDirty()
        {
            int x;
            for (x = 0; x < MAX_BUTTONS; x++)
            {
                // If the button exists, and it's not owned by another object, draw it
                if (this.ButtonList[x] is not null)
                {
                    // Turn on dirty flag
                    this.ButtonList[x].uiFlags |= ButtonFlags.BUTTON_DIRTY;
                }
            }
        }

        public void MarkAButtonDirty(int iButtonNum)
        {
            // surgical dirtying . marks a user specified button dirty, without dirty the whole lot of them

            // If the button exists, and it's not owned by another object, draw it
            var guiButton = this.ButtonList[iButtonNum];
            guiButton.uiFlags |= ButtonFlags.BUTTON_DIRTY;
        }

        public bool DisableButton(int iButtonID)
        {
            return false;
        }

        private bool GetETRLEPixelValue(
            ref byte pDest,
            HVOBJECT hVObject,
            ushort usETRLEIndex,
            ushort usX,
            ushort usY)
        {
            byte pCurrent = 0;
            ushort usLoopX = 0;
            ushort usLoopY = 0;
            ushort ubRunLength;
            ETRLEObject pETRLEObject;

            // Do a bunch of checks
            // CHECKF(hVObject != null);
            // CHECKF(usETRLEIndex < hVObject.usNumberOfObjects);

            pETRLEObject = hVObject.pETRLEObject[usETRLEIndex];

            //CHECKF(usX < pETRLEObject.usWidth);
            //CHECKF(usY < pETRLEObject.usHeight);

            // Assuming everything's okay, go ahead and look...
            int offset = 0;

            pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + offset];

            // Skip past all uninteresting scanlines
            while (usLoopY < usY)
            {
                while (pCurrent != 0)
                {
                    if ((pCurrent & COMPRESS_TRANSPARENT) != 0)
                    {
                        pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + ++offset];
                    }
                    else
                    {
                        offset += (byte)(pCurrent & COMPRESS_RUN_MASK);
                        pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + offset];
                    }
                }

                usLoopY++;
            }

            // Now look in this scanline for the appropriate byte
            do
            {
                ubRunLength = (ushort)(pCurrent & COMPRESS_RUN_MASK);

                if ((pCurrent & COMPRESS_TRANSPARENT) != 0)
                {
                    if (usLoopX + ubRunLength >= usX)
                    {
                        pDest = 0;
                        return true;
                    }
                    else
                    {
                        pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + ++offset];
                    }
                }
                else
                {
                    if (usLoopX + ubRunLength >= usX)
                    {
                        // skip to the correct byte; skip at least 1 to get past the byte defining the run
                        offset += (byte)(usX - usLoopX + 1);
                        pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + offset];

                        pDest = pCurrent;
                        return true;
                    }
                    else
                    {
                        offset += (byte)(ubRunLength + 1);
                        pCurrent = hVObject.pPixData[pETRLEObject.uiDataOffset + offset];
                    }
                }
                usLoopX += ubRunLength;
            }
            while (usLoopX < usX);

            // huh???
            return false;
        }

        public ushort GetWidthOfButtonPic(int usButtonPicID, int iSlot)
        {
            return this.ButtonPictures[usButtonPicID].vobj.pETRLEObject[iSlot].usWidth;
        }

        public int UseLoadedButtonImage(
            int LoadedImg, 
            int Grayed, 
            int OffNormal, 
            int OffHilite, 
            int OnNormal, 
            int OnHilite)
        {
            int UseSlot;
            ETRLEObject pTrav;
            int MaxHeight, MaxWidth, ThisHeight, ThisWidth;


            // Is button image index given valid?
            if (this.ButtonPictures[LoadedImg].vobj == null)
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Invalid button picture handle given for pre-loaded button image %d", LoadedImg));
                return -1;
            }

            // Is button image an external vobject?
            if (this.ButtonPictures[LoadedImg].fFlags.HasFlag(GUI_BTN.EXTERNAL_VOBJ))
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Invalid button picture handle given (%d), cannot use external images as duplicates.", LoadedImg));
                return -1;
            }

            // is there ANY file to open?
            if ((Grayed == BUTTON_NO_IMAGE)
                && (OffNormal == BUTTON_NO_IMAGE)
                && (OffHilite == BUTTON_NO_IMAGE)
                && (OnNormal == BUTTON_NO_IMAGE)
                && (OnHilite == BUTTON_NO_IMAGE))
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("No button pictures selected for pre-loaded button image %d", LoadedImg));
                return -1;
            }

            // Get a button image slot
            if ((UseSlot = this.FindFreeButtonSlot()) == BUTTON_NO_SLOT)
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Out of button image slots for pre-loaded button image %d", LoadedImg));
                return -1;
            }

            // Init the QuickButton image structure with indexes to use
            this.ButtonPictures[UseSlot].vobj = this.ButtonPictures[LoadedImg].vobj;
            this.ButtonPictures[UseSlot].Grayed = Grayed;
            this.ButtonPictures[UseSlot].OffNormal = OffNormal;
            this.ButtonPictures[UseSlot].OffHilite = OffHilite;
            this.ButtonPictures[UseSlot].OnNormal = OnNormal;
            this.ButtonPictures[UseSlot].OnHilite = OnHilite;
            this.ButtonPictures[UseSlot].fFlags = GUI_BTN.DUPLICATE_VOBJ;

            // Fit the button size to the largest image in the set
            MaxWidth = MaxHeight = 0;
            if (Grayed != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[Grayed];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OffNormal != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OffNormal];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OffHilite != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OffHilite];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OnNormal != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OnNormal];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OnHilite != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OnHilite];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            // Set the width and height for this image set
            this.ButtonPictures[UseSlot].MaxHeight = MaxHeight;
            this.ButtonPictures[UseSlot].MaxWidth = MaxWidth;

            // return the image slot number
            this.ButtonPicsLoaded++;
            return UseSlot;
        }

        public int LoadButtonImage(string filename, int Grayed, int OffNormal, int OffHilite, int OnNormal, int OnHilite)
        {
            VOBJECT_DESC vo_desc = new();
            int UseSlot;
            ETRLEObject pTrav;
            int MaxHeight, MaxWidth, ThisHeight, ThisWidth;

            // is there ANY file to open?
            if ((Grayed == BUTTON_NO_IMAGE)
                && (OffNormal == BUTTON_NO_IMAGE)
                && (OffHilite == BUTTON_NO_IMAGE)
                && (OnNormal == BUTTON_NO_IMAGE)
                && (OnHilite == BUTTON_NO_IMAGE))
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("No button pictures selected for %s", filename));
                return -1;
            }

            // Get a button image slot
            if ((UseSlot = this.FindFreeButtonSlot()) == BUTTON_NO_SLOT)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Out of button image slots for %s", filename));
                return -1;
            }

            // Load the image
            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = filename;

            if ((this.ButtonPictures[UseSlot].vobj = this.video.CreateVideoObject(ref vo_desc)) == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Couldn't create VOBJECT for %s", filename));
                return -1;
            }

            // Init the QuickButton image structure with indexes to use
            this.ButtonPictures[UseSlot].Grayed = Grayed;
            this.ButtonPictures[UseSlot].OffNormal = OffNormal;
            this.ButtonPictures[UseSlot].OffHilite = OffHilite;
            this.ButtonPictures[UseSlot].OnNormal = OnNormal;
            this.ButtonPictures[UseSlot].OnHilite = OnHilite;
            this.ButtonPictures[UseSlot].fFlags = GUI_BTN.NONE;

            // Fit the button size to the largest image in the set
            MaxWidth = MaxHeight = 0;
            if (Grayed != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[Grayed];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OffNormal != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OffNormal];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OffHilite != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OffHilite];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OnNormal != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OnNormal];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            if (OnHilite != BUTTON_NO_IMAGE)
            {
                pTrav = this.ButtonPictures[UseSlot].vobj.pETRLEObject[OnHilite];
                ThisHeight = pTrav.usHeight + pTrav.sOffsetY;
                ThisWidth = pTrav.usWidth + pTrav.sOffsetX;

                if (MaxWidth < ThisWidth)
                {
                    MaxWidth = ThisWidth;
                }

                if (MaxHeight < ThisHeight)
                {
                    MaxHeight = ThisHeight;
                }
            }

            // Set the width and height for this image set
            this.ButtonPictures[UseSlot].MaxHeight = MaxHeight;
            this.ButtonPictures[UseSlot].MaxWidth = MaxWidth;

            // return the image slot number
            this.ButtonPicsLoaded++;
            return UseSlot;

        }

        public void ReleaseAnchorMode()
        {
        }

        public int QuickCreateButton(
            int Image,
            short xloc,
            short yloc,
            ButtonFlags Type,
            MSYS_PRIORITY Priority,
            GuiCallback MoveCallback,
            GuiCallback ClickCallback)
        {
            GUI_BUTTON b = new();
            int ButtonNum;
            ButtonFlags BType;
            int x;

            if (xloc < 0 || yloc < 0)
            {
                // sprintf(str, "Attempting to QuickCreateButton with invalid position of %d,%d", xloc, yloc);
                // AssertMsg(0, str);
            }

            if (Image < 0 || Image >= MAX_BUTTON_PICS)
            {
                // sprintf(str, "Attempting to QuickCreateButton with out of range ImageID %d.", Image);
                // AssertMsg(0, str);
            }

            // Strip off any extraneous bits from button type
            BType = Type & (ButtonFlags.BUTTON_TYPE_MASK | ButtonFlags.BUTTON_NEWTOGGLE);

            // Is there a QuickButton image in the given image slot?
            if (this.ButtonPictures[Image].vobj == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "QuickCreateButton: Invalid button image number");
                return -1;
            }

            // Get a new button number
            if ((ButtonNum = this.GetNextButtonNumber()) == BUTTON_NO_SLOT)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "QuickCreateButton: No more button slots");
                return -1;
            }

            // Set the values for this buttn
            b.uiFlags = ButtonFlags.BUTTON_DIRTY;
            b.uiOldFlags = 0;

            // Set someflags if of s certain type....
            if (Type.HasFlag(ButtonFlags.BUTTON_NEWTOGGLE))
            {
                b.uiFlags |= ButtonFlags.BUTTON_NEWTOGGLE;
            }

            // shadow style
            b.bDefaultStatus = DEFAULT_STATUS.NONE;
            b.bDisabledStyle = DISABLED_STYLE.DEFAULT;

            b.Group = -1;
            //Init string
            b.stringText = null;
            b.usFont = 0;
            b.fMultiColor = false;
            b.sForeColor = 0;
            b.sWrappedWidth = -1;
            b.sShadowColor = FontColor.None;
            b.sForeColorDown = FontColor.None;
            b.sShadowColorDown = FontColor.None;
            b.sForeColorHilited = FontColor.None;
            b.sShadowColorHilited = FontColor.None;
            b.bJustification = ButtonTextJustifies.BUTTON_TEXT_CENTER;
            b.bTextXOffset = -1;
            b.bTextYOffset = -1;
            b.bTextXSubOffSet = 0;
            b.bTextYSubOffSet = 0;
            b.fShiftText = true;
            //Init icon
            b.iIconID = -1;
            b.usIconIndex = -1;
            b.bIconXOffset = -1;
            b.bIconYOffset = -1;
            b.fShiftImage = true;
            //Init quickbutton
            b.IDNum = ButtonNum;
            b.ImageNum = Image;
            for (x = 0; x < 4; x++)
            {
                b.UserData[x] = 0;
            }

            b.XLoc = xloc;
            b.YLoc = yloc;

            b.ubToggleButtonOldState = 0;
            b.ubToggleButtonActivated = 0;

            // Set the button click callback function (if any)
            if (ClickCallback != BUTTON_NO_CALLBACK)
            {
                b.ClickCallback = ClickCallback;
                BType |= ButtonFlags.BUTTON_CLICK_CALLBACK;
            }
            else
            {
                b.ClickCallback = BUTTON_NO_CALLBACK;
            }

            // Set the button's mouse movement callback function (if any)
            if (MoveCallback != BUTTON_NO_CALLBACK)
            {
                b.MoveCallback = MoveCallback;
                BType |= ButtonFlags.BUTTON_MOVE_CALLBACK;
            }
            else
            {
                b.MoveCallback = BUTTON_NO_CALLBACK;
            }

            b.Area = new();

            // Define a MOUSE_REGION for this QuickButton
            this.mouse.MSYS_DefineRegion(
                ref b.Area,
                (ushort)xloc,
                (ushort)yloc,
                (ushort)(xloc + (short)this.ButtonPictures[Image].MaxWidth),
                (ushort)(yloc + (short)this.ButtonPictures[Image].MaxHeight),
                Priority,
                Cursor.NORMAL,
                this.QuickButtonCallbackMMove,
                this.QuickButtonCallbackMButn);

            // Link the MOUSE_REGION with this QuickButton
            this.mouse.MSYS_SetRegionUserData(ref b.Area, 0, ButtonNum);

            // Set the flags for this button
            b.uiFlags |= ButtonFlags.BUTTON_ENABLED | BType | ButtonFlags.BUTTON_QUICK;

            // Add this QuickButton to the button list

            this.ButtonList[ButtonNum] = b;

            //SpecifyButtonSoundScheme(b.IDNum, BUTTON_SOUND_SCHEME_GENERIC);

            // return the button number (slot)
            return ButtonNum;
        }

        //=============================================================================
        //	QuickButtonCallbackMMove
        //
        //	Dispatches all button callbacks for mouse movement. This function gets
        //	called by the Mouse System. *DO NOT CALL DIRECTLY*
        //
        private void QuickButtonCallbackMMove(ref MouseRegion reg, MouseCallbackReasons reason)
        {
            GUI_BUTTON b;
            int iButtonID;

            iButtonID = this.mouse.MSYS_GetRegionUserData(ref reg, 0);

            // sprintf(str, "QuickButtonCallbackMMove: Mouse Region #%d (%d,%d to %d,%d) has invalid buttonID %d",
            //                     reg.IDNumber, reg.RegionTopLeftX, reg.RegionTopLeftY, reg.RegionBottomRightX, reg.RegionBottomRightY, iButtonID);

            // AssertMsg(iButtonID >= 0, str);
            // AssertMsg(iButtonID < MAX_BUTTONS, str);

            b = this.ButtonList[iButtonID];

            // AssertMsg(b != null, str);

            if (b is null)
            {
                return;  //This is getting called when Adding new regions...
            }


            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) &&
                  reason.HasFlag(MouseCallbackReasons.LOST_MOUSE | MouseCallbackReasons.GAIN_MOUSE))
            {
                b.uiFlags |= ButtonFlags.BUTTON_DIRTY;
            }

            // Mouse moved on the button, so reset it's timer to maximum.
            if (b.Area.uiFlags.HasFlag(MouseCallbackReasons.GAIN_MOUSE))
            {
                //check for sound playing stuff
                if (b.ubSoundSchemeID != 0)
                {
                    if (b.Area == this.mouse.MSYS_PrevRegion && gpAnchoredButton is null)
                    {
                        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
                        {
                            this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_MOVED_ONTO);
                        }
                        else
                        {
                            this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_DISABLED_MOVED_ONTO);
                        }
                    }
                }
            }
            else
            {
                //Check if we should play a sound
                if (b.ubSoundSchemeID != 0)
                {
                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
                    {
                        if (b.Area == this.mouse.MSYS_PrevRegion && gpAnchoredButton is null)
                        {
                            this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_MOVED_OFF_OF);
                        }
                    }
                    else
                    {
                        this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_DISABLED_MOVED_OFF_OF);
                    }
                }
            }

            // ATE: New stuff for toggle buttons that work with new Win95 paridigm
            if ((b.uiFlags.HasFlag(ButtonFlags.BUTTON_NEWTOGGLE)))
            {
                if (reason.HasFlag(MouseCallbackReasons.LOST_MOUSE))
                {
                    if (b.ubToggleButtonActivated != 0)
                    {
                        b.ubToggleButtonActivated = 0;

                        if (b.ubToggleButtonOldState == 0)
                        {
                            b.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                        }
                        else
                        {
                            b.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                        }
                    }
                }
            }

            // If this button is enabled and there is a callback function associated with it,
            // call the callback function.
            if ((b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)) && (b.uiFlags.HasFlag(ButtonFlags.BUTTON_MOVE_CALLBACK)))
            {
                b.MoveCallback(ref b, reason);
            }
        }



        //=============================================================================
        //	QuickButtonCallbackMButn
        //
        //	Dispatches all button callbacks for button presses. This function is
        //	called by the Mouse System. *DO NOT CALL DIRECTLY*
        //
        private void QuickButtonCallbackMButn(ref MouseRegion reg, MouseCallbackReasons reason)
        {
            GUI_BUTTON b;
            int iButtonID;
            bool MouseBtnDown;
            bool StateBefore, StateAfter = false;

            // Assert(reg != null);

            iButtonID = this.mouse.MSYS_GetRegionUserData(ref reg, index: 0);

            //      sprintf(str, "QuickButtonCallbackMButn: Mouse Region #%d (%d,%d to %d,%d) has invalid buttonID %d",
            //                          reg.IDNumber, reg.RegionTopLeftX, reg.RegionTopLeftY, reg.RegionBottomRightX, reg.RegionBottomRightY, iButtonID);

            b = this.ButtonList[iButtonID];

            if (b is null)
            {
                return;
            }


            if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN | MouseCallbackReasons.RBUTTON_DWN))
            {
                MouseBtnDown = true;
            }
            else
            {
                MouseBtnDown = false;
            }

            StateBefore = (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)) ? (true) : (false);

            // ATE: New stuff for toggle buttons that work with new Win95 paridigm
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_NEWTOGGLE) && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
                {
                    if (b.ubToggleButtonActivated == 0)
                    {
                        if (!(b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
                        {
                            b.ubToggleButtonOldState = 0;
                            b.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                        }
                        else
                        {
                            b.ubToggleButtonOldState = 1;
                            b.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                        }
                        b.ubToggleButtonActivated = 1;
                    }
                }
                else if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
                {
                    b.ubToggleButtonActivated = 0;
                }
            }


            //Kris:
            //Set the anchored button incase the user moves mouse off region while still holding
            //down the button, but only if the button is up.  In Win95, buttons that are already
            //down, and anchored never change state, unless you release the mouse in the button area.

            if (b.MoveCallback == default && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
                {
                    gpAnchoredButton = b;
                    gfAnchoredState = StateBefore;
                    b.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                }
                else if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP) && b.uiFlags.HasFlag(ButtonFlags.BUTTON_NO_TOGGLE))
                {
                    b.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
                }
            }
            else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CHECKBOX))
            {
                if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
                {   //the check box button gets anchored, though it doesn't actually use the anchoring move callback.
                    //The effect is different, we don't want to toggle the button state, but we do want to anchor this
                    //button so that we don't effect any other buttons while we move the mouse around in anchor mode.
                    gpAnchoredButton = b;
                    gfAnchoredState = StateBefore;

                    //Trick the before state of the button to be different so the sound will play properly as checkbox buttons 
                    //are processed differently.
                    StateBefore = (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)) ? false : true;
                    StateAfter = !StateBefore;
                }
                else if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
                {
                    b.uiFlags ^= ButtonFlags.BUTTON_CLICKED_ON; //toggle the checkbox state upon release inside button area.
                                                                //Trick the before state of the button to be different so the sound will play properly as checkbox buttons 
                                                                //are processed differently.
                    StateBefore = (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)) ? false : true;
                    StateAfter = !StateBefore;
                }
            }

            // Should we play a sound if clicked on while disabled?
            if (b.ubSoundSchemeID != 0 && !(b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)) && MouseBtnDown)
            {
                this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_DISABLED_CLICK);
            }

            // If this button is disabled, and no callbacks allowed when disabled
            // callback
            if (!(b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)) && !(b.uiFlags.HasFlag(ButtonFlags.BUTTON_ALLOW_DISABLED_CALLBACK)))
            {
                return;
            }

            // Button not enabled but allowed to use callback, then do that!
            if (!(b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)) && (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ALLOW_DISABLED_CALLBACK)))
            {
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICK_CALLBACK))
                {
                    //b.ClickCallback(b, reason | ButtonFlags.BUTTON_DISABLED_CALLBACK);
                }
                return;
            }

            // If there is a callback function with this button, call it
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICK_CALLBACK))
            {
                //Kris:  January 6, 1998
                //Added these checks to avoid a case where it was possible to process a leftbuttonup message when
                //the button wasn't anchored, and should have been.
                this.gfDelayButtonDeletion = true;
                if (!(reason.HasFlag(MouseCallbackReasons.LBUTTON_UP)) || b.MoveCallback != default ||
                        b.MoveCallback == default && gpPrevAnchoredButton == b)
                {
                    b.ClickCallback(ref b, reason);
                }

                this.gfDelayButtonDeletion = false;
            }
            else if ((reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN) && !(b.uiFlags.HasFlag(ButtonFlags.BUTTON_IGNORE_CLICKS))))
            {
                // Otherwise, do default action with this button.
                b.uiFlags ^= ButtonFlags.BUTTON_CLICKED_ON;
            }

            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CHECKBOX))
            {
                StateAfter = (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)) ? (true) : (false);
            }

            // Play sounds for this enabled button (disabled sounds have already been done)
            if (b.ubSoundSchemeID != 0 && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
            {
                if (reason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
                {
                    if (b.ubSoundSchemeID != 0 && StateBefore && !StateAfter)
                    {
                        this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_CLICKED_OFF);
                    }
                }
                else if (reason.HasFlag(MouseCallbackReasons.LBUTTON_DWN))
                {
                    if (b.ubSoundSchemeID != 0 && !StateBefore && StateAfter)
                    {
                        this.PlayButtonSound(iButtonID, ButtonSounds.BUTTON_SOUND_CLICKED_ON);
                    }
                }
            }

            if (StateBefore != StateAfter)
            {
                this.video.InvalidateRegion(b.Area.RegionTopLeftX, b.Area.RegionTopLeftY, b.Area.RegionBottomRightX, b.Area.RegionBottomRightY);
            }

            if (this.gfPendingButtonDeletion)
            {
                this.RemoveButtonsMarkedForDeletion();
            }
        }

        private void RemoveButtonsMarkedForDeletion()
        {
            throw new NotImplementedException();
        }

        private int GetNextButtonNumber()
        {
            for (int x = 0; x < MAX_BUTTONS; x++)
            {
                if (this.ButtonList[x] is null)
                {
                    return x;
                }
            }

            return BUTTON_NO_SLOT;
        }

        public void Dispose()
        {
        }

        public int FindFreeButtonSlot()
        {
            int slot;

            // Are there any slots available?
            if (this.ButtonPicsLoaded >= MAX_BUTTON_PICS)
            {
                return BUTTON_NO_SLOT;
            }

            // Search for a slot
            for (slot = 0; slot < MAX_BUTTON_PICS; slot++)
            {
                if (this.ButtonPictures[slot].vobj == null)
                {
                    return slot;
                }
            }

            return BUTTON_NO_SLOT;
        }

        public void RemoveButton(int v)
        {
        }

        public void UnloadButtonImage(int v)
        {
        }

        public ValueTask<bool> Initialize() => this.Initialize(this.gameContext);
    }

    // GUI_BUTTON callback function type
    public delegate void GuiCallback(ref GUI_BUTTON button, MouseCallbackReasons reason);

    public class GUI_BUTTON
    {
        public int IDNum;                        // ID Number, contains it's own button number
        public int ImageNum;                    // Image number to use (see DOCs for details)
        public MouseRegion Area = new();                          // Mouse System's mouse region to use for this button
        public GuiCallback ClickCallback;     // Button Callback when button is clicked
        public GuiCallback MoveCallback;          // Button Callback when mouse moved on this region
        public Cursor Cursor;                       // Cursor to use for this button
        public ButtonFlags uiFlags;                 // Button state flags etc.( 32-bit )
        public ButtonFlags uiOldFlags;              // Old flags from previous render loop
        public int XLoc;                         // Coordinates where button is on the screen
        public int YLoc;
        public int[] UserData = new int[4];          // Place holder for user data etc.
        public int Group;                        // Group this button belongs to (see DOCs)
        public DEFAULT_STATUS bDefaultStatus;
        //Button disabled style
        public DISABLED_STYLE bDisabledStyle;
        //For buttons with text
        public string? stringText;					//the string
        public FontStyle usFont;                      //font for text 
        public bool fMultiColor;            //font is a multi-color font
        public FontColor sForeColor;               //text colors if there is text
        public FontColor sShadowColor;
        public FontColor sForeColorDown;       //text colors when button is down (optional)
        public FontColor sShadowColorDown;
        public FontColor sForeColorHilited;        //text colors when button is down (optional)
        public FontColor sShadowColorHilited;
        public ButtonTextJustifies bJustification;        // BUTTON_TEXT_LEFT, BUTTON_TEXT_CENTER, BUTTON_TEXT_RIGHT
        public int bTextXOffset;
        public int bTextYOffset;
        public int bTextXSubOffSet;
        public int bTextYSubOffSet;
        public bool fShiftText;
        public int sWrappedWidth;
        //For buttons with icons (don't confuse this with quickbuttons which have up to 5 states )
        public int iIconID;
        public int usIconIndex;
        public int bIconXOffset; //-1 means horizontally centered
        public int bIconYOffset; //-1 means vertically centered
        public bool fShiftImage;  //if true, icon is shifted +1,+1 when button state is down.

        public int ubToggleButtonOldState;       // Varibles for new toggle buttons that work
        public int ubToggleButtonActivated;

        public int BackRect;                 // Handle to a Background Rectangle
        public int ubSoundSchemeID;
    }

    public class ButtonPics
    {
        public HVOBJECT vobj = new();                      // The Image itself
        public int Grayed;                   // Index to use for a "Grayed-out" button
        public int OffNormal;            // Index to use when button is OFF
        public int OffHilite;            // Index to use when button is OFF w/ hilite on it
        public int OnNormal;             // Index to use when button is ON
        public int OnHilite;             // Index to use when button is ON w/ hilite on it
        public int MaxWidth;                // Width of largest image in use
        public int MaxHeight;           // Height of largest image in use
        public GUI_BTN fFlags;                  // Special image flags
    }

    [Flags]
    public enum ButtonSounds : uint
    {
        BUTTON_SOUND_NONE = 0x00,
        BUTTON_SOUND_CLICKED_ON = 0x01,
        BUTTON_SOUND_CLICKED_OFF = 0x02,
        BUTTON_SOUND_MOVED_ONTO = 0x04,
        BUTTON_SOUND_MOVED_OFF_OF = 0x08,
        BUTTON_SOUND_DISABLED_CLICK = 0x10,
        BUTTON_SOUND_DISABLED_MOVED_ONTO = 0x20,
        BUTTON_SOUND_DISABLED_MOVED_OFF_OF = 0x40,
        BUTTON_SOUND_ALREADY_PLAYED = 0X80,
        BUTTON_SOUND_ALL_EVENTS = 0xff,
    }

    public enum DEFAULT_STATUS
    {
        NONE,
        DARKBORDER,          //shades the borders 2 pixels deep
        DOTTEDINTERIOR,  //draws the familiar dotted line in the interior portion of the button.
        WINDOWS95,               //both DARKBORDER and DOTTEDINTERIOR
    }

    public enum DISABLED_STYLE//for use with SpecifyDisabledButtonStyle
    {
        NONE,        //for dummy buttons, panels, etc.  Always displays normal state.
        DEFAULT, //if button has text then shade, else hatch
        HATCHED, //always hatches the disabled button
        SHADED       //always shades the disabled button 25% darker
    }

    [Flags]
    public enum ButtonFlags : uint
    {
        //button flags
        BUTTON_TOGGLE = 0x00000000,
        BUTTON_QUICK = 0x00000000,
        BUTTON_ENABLED = 0x00000001,
        BUTTON_CLICKED_ON = 0x00000002,
        BUTTON_NO_TOGGLE = 0x00000004,
        BUTTON_CLICK_CALLBACK = 0x00000008,
        BUTTON_MOVE_CALLBACK = 0x00000010,
        BUTTON_GENERIC = 0x00000020,
        BUTTON_HOT_SPOT = 0x00000040,
        BUTTON_SELFDELETE_IMAGE = 0x00000080,
        BUTTON_DELETION_PENDING = 0x00000100,
        BUTTON_ALLOW_DISABLED_CALLBACK = 0x00000200,
        BUTTON_DIRTY = 0x00000400,
        BUTTON_SAVEBACKGROUND = 0x00000800,
        BUTTON_CHECKBOX = 0x00001000,
        BUTTON_NEWTOGGLE = 0x00002000,
        BUTTON_FORCE_UNDIRTY = 0x00004000, // no matter what happens this buttons does NOT get marked dirty
        BUTTON_IGNORE_CLICKS = 0x00008000, // Ignore any clicks on this button
        BUTTON_DISABLED_CALLBACK = 0x80000000,

        //effects how the button is rendered.
        BUTTON_TYPES = ButtonFlags.BUTTON_QUICK | ButtonFlags.BUTTON_GENERIC | ButtonFlags.BUTTON_HOT_SPOT | ButtonFlags.BUTTON_CHECKBOX,
        //effects how the button is processed
        BUTTON_TYPE_MASK = ButtonFlags.BUTTON_NO_TOGGLE | ButtonFlags.BUTTON_ALLOW_DISABLED_CALLBACK | ButtonFlags.BUTTON_CHECKBOX | ButtonFlags.BUTTON_IGNORE_CLICKS,
    }
}
