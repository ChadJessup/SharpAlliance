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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public enum ButtonTextJustifies
{
    BUTTON_TEXT_LEFT = -1,
    BUTTON_TEXT_CENTER = 0,
    BUTTON_TEXT_RIGHT = 1,
    TEXT_LJUSTIFIED = BUTTON_TEXT_LEFT,
    TEXT_CJUSTIFIED = BUTTON_TEXT_CENTER,
    TEXT_RJUSTIFIED = BUTTON_TEXT_RIGHT,
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
    private readonly ILogger<ButtonSubSystem> logger;
    private readonly GameContext gameContext;
    private static FontSubSystem fonts;

    private static IInputManager inputs;
    private static MouseSubSystem mouse;
    private static IVideoManager video;

    public bool IsInitialized { get; private set; }

    // flag to state we wish to render buttons on the one after the next pass through render buttons
    private bool fPausedMarkButtonsDirtyFlag = false;
    private bool fDisableHelpTextRestoreFlag = false;

    private static int ButtonPicsLoaded;

    private static Surfaces ButtonDestBuffer = Surfaces.BACKBUFFER;
    private uint ButtonDestPitch = 640 * 2;
    private uint ButtonDestBPP = 16;

    public static GUI_BUTTON[] ButtonList = new GUI_BUTTON[Globals.MAX_BUTTONS];
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonGrayed = new ();
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonOffNormal = new();
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonOffHilite = new();
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonOnNormal = new();
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonOnHilite = new();
    private static Dictionary<ButtonPic, HVOBJECT> GenericButtonBackground = new();

    private static Dictionary<ButtonPic, Rgba32> GenericButtonFillColors = new();
    private static Dictionary<ButtonPic, ushort> GenericButtonBackgroundindex = new();
    private static Dictionary<ButtonPic, short> GenericButtonOffsetX = new();
    private static Dictionary<ButtonPic, short> GenericButtonOffsetY = new();

    public const byte COMPRESS_TRANSPARENT = 0x80;
    public const byte COMPRESS_RUN_MASK = 0x7F;

    private static HVOBJECT[] GenericButtonIcons = new HVOBJECT[Globals.MAX_BUTTON_ICONS];

    private int ButtonsInList = 0;

    public ButtonSubSystem(
        ILogger<ButtonSubSystem> logger,
        GameContext gameContext,
        FontSubSystem fontSubSystem)
    {
        logger = logger;
        gameContext = gameContext;
        fonts = fontSubSystem;
    }

    public async ValueTask<bool> Initialize(GameContext gameContext)
    {
        //video = (gameContext.Services.GetRequiredService<IVideoManager>() as VeldridVideoManager)!;
        mouse = gameContext.Services.GetRequiredService<MouseSubSystem>();
        inputs = gameContext.Services.GetRequiredService<IInputManager>();

        IsInitialized = await InitializeButtonImageManager(
            Surfaces.Unknown,
        -1,
            -1);

        return IsInitialized;
    }

    public static bool EnableButton(GUI_BUTTON iButtonID)
    {
        GUI_BUTTON? b = iButtonID;
        bool OldState;

        // If button exists, set the ENABLED flag
        if (b is not null)
        {
            OldState = b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED);
            b.uiFlags |= (ButtonFlags.BUTTON_ENABLED | ButtonFlags.BUTTON_DIRTY);
        }
        else
        {
            OldState = false;
        }


        // Return previous ENABLED state of this button
        return OldState;
    }

    public static void SpecifyButtonSoundScheme(GUI_BUTTON buttonId, BUTTON_SOUND_SCHEME soundScheme)
    {
    }

    // public List<ButtonPic> ButtonPictures { get; } = new(MAX_BUTTON_PICS);

    private async ValueTask<bool> InitializeButtonImageManager(Surfaces DefaultBuffer, int DefaultPitch, int DefaultBPP)
    {
        byte Pix;
        int x;

        // Set up the default settings
        if (DefaultBuffer != Globals.BUTTON_USE_DEFAULT)
        {
            ButtonDestBuffer = DefaultBuffer;
        }
        else
        {
            ButtonDestBuffer = Surfaces.FRAME_BUFFER;
        }

        if (DefaultPitch != (int)Globals.BUTTON_USE_DEFAULT)
        {
            ButtonDestPitch = (uint)DefaultPitch;
        }
        else
        {
            ButtonDestPitch = 640 * 2;
        }

        if (DefaultBPP != (int)Globals.BUTTON_USE_DEFAULT)
        {
            ButtonDestBPP = (uint)DefaultBPP;
        }
        else
        {
            ButtonDestBPP = 16;
        }

        // Blank out all QuickButton images
        //for (x = 0; x < MAX_BUTTON_PICS; x++)
        //{
        
        var bp = new ButtonPic
        {
            vobj = null,
            Grayed = -1,
            OffNormal = -1,
            OffHilite = -1,
            OnNormal = -1,
            OnHilite = -1,
        };

        //    ButtonPictures.Add(bp);
        //}

        ButtonPicsLoaded = 0;

        // Blank out all Generic button data
        // for (x = 0; x < MAX_GENERIC_PICS; x++)
        // {
        //     //GenericButtonGrayed[x] = null;
        //     //GenericButtonOffNormal[x] = null;
        //     //GenericButtonOffHilite[x] = null;
        //     //GenericButtonOnNormal[x] = null;
        //     //GenericButtonOnHilite[x] = null;
        //     //GenericButtonBackground[x] = null;
        //     GenericButtonBackgroundindex[x] = 0;
        //     GenericButtonFillColors[x] = new Rgba32(0, 0, 0);
        //     GenericButtonBackgroundindex[x] = 0;
        //     GenericButtonOffsetX[x] = 0;
        //     GenericButtonOffsetY[x] = 0;
        // }

        // Blank out all icon images
        // for (x = 0; x < MAX_BUTTON_ICONS; x++)
        // {
        //     // GenericButtonIcons[x] = null;
        // }

        // Load the default generic button images
        GenericButtonOffNormal.Add(bp, VeldridVideoManager.CreateVideoObject(Globals.DEFAULT_GENERIC_BUTTON_OFF));
        if (GenericButtonOffNormal[bp] == null)
        {
            //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_OFF);
            return false;
        }

        if ((GenericButtonOnNormal[bp] = VeldridVideoManager.CreateVideoObject(Globals.DEFAULT_GENERIC_BUTTON_ON)) == null)
        {
            //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_ON);
            return false;
        }

        // Load up the off hilite and on hilite images. We won't check for errors because if the file
        // doesn't exists, the system simply ignores that file. These are only here as extra images, they
        // aren't required for operation (only OFF Normal and ON Normal are required).
        GenericButtonOffHilite[bp] = VeldridVideoManager.CreateVideoObject(Globals.DEFAULT_GENERIC_BUTTON_OFF_HI);

        GenericButtonOnHilite[bp] = VeldridVideoManager.CreateVideoObject(Globals.DEFAULT_GENERIC_BUTTON_ON_HI);

        Pix = 0;
        if (!GetETRLEPixelValue(ref Pix, GenericButtonOffNormal[bp], 8, 0, 0))
        {
            // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't get generic button's background pixel value");
            return false;
        }

        GenericButtonFillColors[bp] = GenericButtonOffNormal[bp].Palette[Pix];

        return true;
    }

    public static void UnMarkButtonDirty(GUI_BUTTON button)
    {
        if (button is not null)
        {
            button.IsDirty = false;
        }
    }

    public static void PlayButtonSound(GUI_BUTTON iDNum, ButtonSounds bUTTON_SOUND_CLICKED_ON)
    {
    }


    public static void RenderButtons()
        => RenderButtons(ButtonList);

    public static void RenderButtons(IEnumerable<GUI_BUTTON> buttons)
    {
        if (!ButtonList.Any())
        {
            return;
        }

        int iButtonID;
        bool fOldButtonDown, fOldEnabled;

        fonts.SaveFontSettings();
        foreach(var b in buttons)
        {
            // If the button exists, and it's not owned by another object, draw it
            //Kris:  and make sure that the button isn't hidden.

            if (b is null)
            {
                continue;
            }

            if (b.MouseRegion.IsEnabled)
            {
                // Check for buttonchanged status
                fOldButtonDown = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON);

                if (fOldButtonDown != b.uiOldFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                {
                    //Something is different, set dirty!
                    b.IsDirty = true;
                }

                // Check for button dirty flags
                fOldEnabled = b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED);

                if (fOldEnabled != b.uiOldFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
                {
                    //Something is different, set dirty!
                    b.IsDirty = true;
                }

                // If we ABSOLUTELY want to render every frame....
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_SAVEBACKGROUND))
                {
                    b.IsDirty = true;
                }

                // Set old flags
                b.uiOldFlags = b.uiFlags;

                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_FORCE_UNDIRTY))
                {
                    b.IsDirty = false;
                    b.uiFlags &= ~ButtonFlags.BUTTON_FORCE_UNDIRTY;
                }

                // Check if we need to update!
                if (b.IsDirty)
                {
                    b.IsDirty = false;
                    DrawButtonFromPtr(b);

                    VeldridVideoManager.InvalidateRegion(b.MouseRegion.Bounds);
                }
            }
        }

        // check if we want to render 1 frame later?
        if ((Globals.fPausedMarkButtonsDirtyFlag == true) && (Globals.fDisableHelpTextRestoreFlag == false))
        {
            Globals.fPausedMarkButtonsDirtyFlag = false;
            MarkButtonsDirty(buttons);
        }

        fonts.RestoreFontSettings();
    }

    internal static bool SetButtonCursor(GUI_BUTTON? b, CURSOR usCursor)
    {
        if (b is null)
        {
            return false;
        }

        b.Cursor = usCursor;

        return true;
    }

    private static void DrawButtonFromPtr(GUI_BUTTON b, int id = 0)
    {
        // Draw the appropriate button according to button type
        Globals.gbDisabledButtonStyle = DISABLED_STYLE.NONE;
        switch (b.uiFlags & ButtonFlags.BUTTON_TYPES)
        {
            case ButtonFlags.BUTTON_QUICK:
                ButtonSubSystem.DrawQuickButton(ref b);
                break;
            case ButtonFlags.BUTTON_GENERIC:
                ButtonSubSystem.DrawGenericButton(ref b, id);
                break;
            case ButtonFlags.BUTTON_HOT_SPOT:
                if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_NO_TOGGLE))
                {
                    b.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                }
                return;  //hotspots don't have text, but if you want to, change this to a break!
            case ButtonFlags.BUTTON_CHECKBOX:
                ButtonSubSystem.DrawCheckBoxButton(ref b, id);
                break;
        }

        //If button has an icon, overlay it on current button.
        if (b.iIconID != -1)
        {
            ButtonSubSystem.DrawIconOnButton(ref b, id);
        }

        //If button has text, draw it now
        if (!string.IsNullOrWhiteSpace(b.stringText))
        {
            ButtonSubSystem.DrawTextOnButton(ref b, id);
        }

        //If the button is disabled, and a style has been calculated, then
        //draw the style last.
        switch (Globals.gbDisabledButtonStyle)
        {
            case DISABLED_STYLE.HATCHED:
                ButtonSubSystem.DrawHatchOnButton(ref b, id);
                break;
            case DISABLED_STYLE.SHADED:
                ButtonSubSystem.DrawShadeOnButton(ref b, id);
                break;
        }

        if (b.bDefaultStatus != DEFAULT_STATUS.NONE)
        {
            DrawDefaultOnButton(ref b, id);
        }
    }

    public static void ForceButtonUnDirty(GUI_BUTTON button)
    {
        button.uiFlags &= ~ButtonFlags.BUTTON_DIRTY;
        button.uiFlags |= ButtonFlags.BUTTON_FORCE_UNDIRTY;
    }

    public static GUI_BUTTON CreateIconAndTextButton(
        ButtonPic image,
        string text,
        FontStyle uiFont,
        FontColor sForeColor,
        FontShadow sShadowColor,
        FontColor sForeColorDown,
        FontShadow sShadowColorDown,
        ButtonTextJustifies bJustification,
        Point loc,
        ButtonFlags Type,
        MSYS_PRIORITY Priority,
        GuiCallback? MoveCallback,
        GuiCallback ClickCallback)
    {
        GUI_BUTTON b;
        ButtonFlags BType;
        int x;

        loc.Y = 480 - loc.Y;

        if (loc.X < 0 || loc.Y < 0)
        {
            throw new InvalidOperationException($"Attempting to CreateIconAndTextButton with invalid position of {loc}");
        }

        // Strip off any extraneous bits from button type
        BType = Type & (ButtonFlags.BUTTON_TYPE_MASK | ButtonFlags.BUTTON_NEWTOGGLE);

        b = new();
        // Set the values for this button
        b.IsDirty = true;
        b.uiOldFlags = 0;
        // b.IdNum = iButtonID;
        b.Loc = loc;
        b.ButtonPicture = image;
        for (x = 0; x < 4; x++)
        {
            b.UserData[x] = 0;
        }

        b.Group = -1;
        b.bDefaultStatus = DEFAULT_STATUS.NONE;
        b.bDisabledStyle = DISABLED_STYLE.DEFAULT;

        // Allocate memory for the button's text string...
        b.stringText = null;
        if (!string.IsNullOrWhiteSpace(text))
        {
            b.stringText = text;
        }

        b.bJustification = bJustification;
        b.usFont = uiFont;
        b.fMultiColor = false;
        b.sForeColor = sForeColor;
        b.sWrappedWidth = -1;
        b.sShadowColor = sShadowColor;
        b.sForeColorDown = sForeColorDown;
        b.sShadowColorDown = sShadowColorDown;
        b.sForeColorHilited = FontColor.None;
        b.sShadowColorHilited = FontShadow.NO_SHADOW;
        b.bTextOffset = new(-1, -1);
        b.bTextSubOffSet = new(0, 0);
        b.fShiftText = true;

        b.iIconID = -1;
        b.usIconindex = 0;

        // Set the button click callback function (if any)
        if (ClickCallback != Globals.BUTTON_NO_CALLBACK)
        {
            b.ClickCallback = ClickCallback;
            BType |= ButtonFlags.BUTTON_CLICK_CALLBACK;
        }
        else
        {
            b.ClickCallback = Globals.BUTTON_NO_CALLBACK;
        }

        // Set the button's mouse movement callback function (if any)
        if (MoveCallback != Globals.BUTTON_NO_CALLBACK)
        {
            b.MoveCallback = MoveCallback;
            BType |= ButtonFlags.BUTTON_MOVE_CALLBACK;
        }
        else
        {
            b.MoveCallback = Globals.BUTTON_NO_CALLBACK;
        }

        // Define a MOUSE_REGION for this QuickButton
        MouseSubSystem.MSYS_DefineRegion(
            b.MouseRegion,
            new(loc.X,
                loc.Y,
                b.ButtonPicture.MaxWidth,
                b.ButtonPicture.MaxHeight),
            Priority,
            CURSOR.NORMAL,
            QuickButtonCallbackMouseMove,
            QuickButtonCallbackMButn);

        // Link the MOUSE_REGION with this QuickButton
        MouseSubSystem.SetRegionUserData(b.MouseRegion, 0, b);

        // Set the flags for this button
        b.uiFlags |= ButtonFlags.BUTTON_ENABLED | BType | ButtonFlags.BUTTON_QUICK;

        //SpecifyButtonSoundScheme(b.IDNum, BUTTON_SOUND_SCHEME_GENERIC);

        // return the button number (slot)
        return b;
    }

    public static void SpecifyDisabledButtonStyle(GUI_BUTTON iButtonID, DISABLED_STYLE bStyle)
    {
        iButtonID.bDisabledStyle = bStyle;
    }

    private static void DrawDefaultOnButton(ref GUI_BUTTON b, int id = 0)
    {
        // video.SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
        var image = new Image<Rgba32>(640, 480);
        if (b.bDefaultStatus == DEFAULT_STATUS.DARKBORDER || b.bDefaultStatus == DEFAULT_STATUS.WINDOWS95)
        {
            var color = Color.Black;
            //left (one thick)
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.X - 1, b.MouseRegion.Bounds.Y - 1, b.MouseRegion.Bounds.X - 1, b.MouseRegion.Bounds.Height + 1, color, image);
            //top (one thick)
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.X - 1, b.MouseRegion.Bounds.Y - 1, b.MouseRegion.Bounds.Width + 1, b.MouseRegion.Bounds.Y - 1, color, image);
            //right (two thick)
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.Width, b.MouseRegion.Bounds.Y - 1, b.MouseRegion.Bounds.Width, b.MouseRegion.Bounds.Height + 1, color, image);
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.Width + 1, b.MouseRegion.Bounds.Y - 1, b.MouseRegion.Bounds.Width + 1, b.MouseRegion.Bounds.Height + 1, color, image);
            //bottom (two thick)
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.X - 1, b.MouseRegion.Bounds.Height, b.MouseRegion.Bounds.Width + 1, b.MouseRegion.Bounds.Height, color, image);
            VeldridVideoManager.LineDraw(b.MouseRegion.Bounds.X - 1, b.MouseRegion.Bounds.Height + 1, b.MouseRegion.Bounds.Width + 1, b.MouseRegion.Bounds.Height + 1, color, image);

            VeldridVideoManager.InvalidateRegion(new Rectangle(
                b.MouseRegion.Bounds.X - 1,
                b.MouseRegion.Bounds.Y - 1,
                b.MouseRegion.Bounds.Width + 1,
                b.MouseRegion.Bounds.Height + 1));
        }

        if (b.bDefaultStatus == DEFAULT_STATUS.DOTTEDINTERIOR || b.bDefaultStatus == DEFAULT_STATUS.WINDOWS95)
        { //Draw an internal dotted rectangle.

        }
    }

    private static void DrawShadeOnButton(ref GUI_BUTTON b, int id = 0)
    {
        byte[] pDestBuf;
        uint uiDestPitchBYTES;
        Rectangle ClipRect = new();
        ClipRect.Y = b.MouseRegion.Bounds.X;
        ClipRect.Width = b.MouseRegion.Bounds.Width - 1;
        ClipRect.Y = b.MouseRegion.Bounds.Y;
        ClipRect.Height = b.MouseRegion.Bounds.Height - 1;
    }

    public static void SetButtonUserData(GUI_BUTTON btn, int index, object userData)
    {
        if (index < 0 || index > 3)
        {
            return;
        }

        btn.UserData[index] = userData;
    }

    public static GUI_BUTTON CreateCheckBoxButton(
        Point loc,
        string filename,
        MSYS_PRIORITY Priority,
        GuiCallback ClickCallback)
    {
        ButtonPic ButPic;

        GUI_BUTTON iButtonID;

        if ((ButPic = LoadButtonImage(filename, -1, 0, 1, 2, 3)) == null)
        {
            //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "CreateCheckBoxButton: Can't load button image");
            throw new InvalidOperationException();
        }

        iButtonID = QuickCreateButton(
            ButPic,
            loc,
            ButtonFlags.BUTTON_CHECKBOX,
            Priority,
            null,
            ClickCallback);

        if (iButtonID is null)
        {
            //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "CreateCheckBoxButton: Can't create button");
            throw new InvalidOperationException();
        }

        //change the flags so that it isn't a quick button anymore
        var b = iButtonID;
        b.uiFlags &= ~ButtonFlags.BUTTON_QUICK;
        b.uiFlags |= ButtonFlags.BUTTON_CHECKBOX | ButtonFlags.BUTTON_SELFDELETE_IMAGE;

        return b;
    }

    private static void DrawHatchOnButton(ref GUI_BUTTON b, int id = 0)
    {
        byte[] pDestBuf;
        uint uiDestPitchBYTES;
        Rectangle ClipRect = new();
        ClipRect.Y = b.MouseRegion.Bounds.X;
        ClipRect.Width = b.MouseRegion.Bounds.Width - 1;
        ClipRect.Y = b.MouseRegion.Bounds.Y;
        ClipRect.Height = b.MouseRegion.Bounds.Height - 1;
    }

    private static void DrawTextOnButton(ref GUI_BUTTON b, int id = 0)
    {
        int height, TextX, TextY;
        Rectangle NewClip = new();
        Rectangle OldClip = new();
        FontColor sForeColor;

        // If this button actually has a string to print
        if (!string.IsNullOrWhiteSpace(b.stringText))
        {
            // Get the width and height of this button
            //width = b.MouseRegion.Bounds.Width - b.MouseRegion.Bounds.X;
            height = b.MouseRegion.Bounds.Height - b.MouseRegion.Bounds.Y;

            // Compute the coordinates to center the text
            //yp = b.MouseRegion.Bounds.Y + b.bTextOffset.Y;
            //xp = b.MouseRegion.Bounds.X + b.bTextOffset.X;

            // Set the printing font to the button text font
            FontSubSystem.SetFont(b.usFont);

            // print the text
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontForeground(b.sForeColor);
            sForeColor = b.sForeColor;
            if (b.sShadowColor != FontShadow.NO_SHADOW)
            {
                FontSubSystem.SetFontShadow(b.sShadowColor);
            }

            //Override the colors if necessary.
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)
                && b.MouseRegion.HasMouse
                && b.sForeColorHilited != FontColor.None)
            {
                FontSubSystem.SetFontForeground(b.sForeColorHilited);
                sForeColor = b.sForeColorHilited;
            }
            else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.sForeColorDown != FontColor.None)
            {
                FontSubSystem.SetFontForeground(b.sForeColorDown);
                sForeColor = b.sForeColorDown;
            }

            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)
                && b.MouseRegion.HasMouse
                && b.sShadowColorHilited != FontShadow.NO_SHADOW)
            {
                FontSubSystem.SetFontShadow(b.sShadowColorHilited);
            }
            else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.sShadowColorDown != FontShadow.NO_SHADOW)
            {
                FontSubSystem.SetFontShadow(b.sShadowColorDown);
            }

            int x = b.MouseRegion.Bounds.X;
            int y = 480 - (b.MouseRegion.Bounds.Height + b.MouseRegion.Bounds.Y - (b.MouseRegion.Bounds.Height / 4));

            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.fShiftText)
            {   // Was the button clicked on? if so, move the text slightly for the illusion
                // that the text moved into the screen.
                x++;
                y++;
            }

            // yp += b.bTextSubOffSet.Y;
            // xp += b.bTextSubOffSet.X;

            FontSubSystem.DrawTextToScreen(
                b.stringText,
                x,
                y,
                b.MouseRegion.Bounds.Width + 8,
                b.usFont,
                sForeColor,
                FontColor.FONT_MCOLOR_BLACK,
                TextJustifies.CENTER_JUSTIFIED);
        }
    }

    public static void SetButtonFastHelpText(GUI_BUTTON btn, string text)
    {
        MouseSubSystem.SetRegionFastHelpText(btn.MouseRegion, text);
    }

    private static void DrawIconOnButton(ref GUI_BUTTON b, int id = 0)
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
            width = b.MouseRegion.Bounds.Width - b.MouseRegion.Bounds.X;
            height = b.MouseRegion.Bounds.Height - b.MouseRegion.Bounds.Y;

            // Compute viewable area (inside borders)
            NewClip.Y = b.Loc.X + 3;
            NewClip.Width = b.Loc.X + width - 3;
            NewClip.Y = b.Loc.Y + 2;
            NewClip.Height = b.Loc.Y + height - 2;

            // Get Icon's blit start coordinates
            IconX = NewClip.Y;
            IconY = NewClip.Y;

            // Get current clip area
            VeldridVideoManager.GetClippingRect(out OldClip);

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
                pTrav = GenericButtonIcons[b.iIconID].pETRLEObject[b.usIconindex];
            }
            else
            {
                hvObject = VeldridVideoManager.GetVideoObject(b.iIconID.ToString());
                pTrav = hvObject.pETRLEObject[b.usIconindex];
            }

            IconH = pTrav.usHeight + pTrav.sOffsetY;
            IconW = pTrav.usWidth + pTrav.sOffsetX;

            // Compute coordinates for centering the icon on the button or
            // use the offset system.
            if (b.bIconOffset.X == -1)
            {
                xp = ((width - 6 - IconW) / 2) + IconX;
            }
            else
            {
                xp = b.MouseRegion.Bounds.X + b.bIconOffset.X;
            }

            if (b.bIconOffset.Y == -1)
            {
                yp = ((height - 4 - IconH) / 2) + IconY;
            }
            else
            {
                yp = b.MouseRegion.Bounds.Y + b.bIconOffset.Y;
            }

            // Was the button clicked on? if so, move the image slightly for the illusion
            // that the image moved into the screen.
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) && b.fShiftImage)
            {
                xp++;
                yp++;
            }

            // Set the clipping rectangle to the viewable area of the button
            VeldridVideoManager.SetClippingRect(ref NewClip);
            // Blit the icon
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_GENERIC))
            {
                VeldridVideoManager.BltVideoObject(GenericButtonIcons[b.iIconID], b.usIconindex, (short)xp, (short)yp, b.iIconID);
            }
            else
            {
                VeldridVideoManager.BltVideoObject(hvObject, b.usIconindex, (short)xp, (short)yp, b.usIconindex);
            }

            // Restore previous clip region
            VeldridVideoManager.SetClippingRect(ref OldClip);
        }
    }

    public static object MSYS_GetBtnUserData(GUI_BUTTON btn, int index) => btn.UserData[index];

    private static void DrawCheckBoxButton(ref GUI_BUTTON b, int id = 0)
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
                if (b.MouseRegion.HasMouse
                    && Globals.gfRenderHilights
                    && inputs.gfLeftButtonState
                    && b.ButtonPicture.OnHilite != -1)
                {
                    UseImage = b.ButtonPicture.OnHilite;            // Use On-Hilite image
                }
                else if (b.ButtonPicture.OnNormal != -1)
                {
                    UseImage = b.ButtonPicture.OnNormal;            // Use On-Normal image
                }
            }
            else
            {
                // Is the mouse over the button, and do we have hilite image?
                if (b.MouseRegion.HasMouse
                    && Globals.gfRenderHilights
                    && inputs.gfLeftButtonState
                    && b.ButtonPicture.OffHilite != -1)
                {
                    UseImage = b.ButtonPicture.OffHilite;           // Use Off-Hilite image
                }
                else if (b.ButtonPicture.OffNormal != -1)
                {
                    UseImage = b.ButtonPicture.OffNormal;           // Use Off-Normal image
                }
            }
        }
        else if (b.ButtonPicture.Grayed != -1)
        {
            // Button is disabled so use the "Grayed-out" image
            UseImage = b.ButtonPicture.Grayed;
        }
        else //use the disabled style
        {
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                UseImage = b.ButtonPicture.OnHilite;
            }
            else
            {
                UseImage = b.ButtonPicture.OffHilite;
            }

            switch (b.bDisabledStyle)
            {
                case DISABLED_STYLE.DEFAULT:
                    Globals.gbDisabledButtonStyle = DISABLED_STYLE.HATCHED;
                    break;
                case DISABLED_STYLE.HATCHED:
                case DISABLED_STYLE.SHADED:
                    Globals.gbDisabledButtonStyle = b.bDisabledStyle;
                    break;
            }
        }

        // Display the button image
        VeldridVideoManager.BltVideoObject(
            b.ButtonPicture.vobj,
            (ushort)UseImage,
            b.Loc.X, b.Loc.Y,
            UseImage);
    }

    public static void DrawCheckBoxButtonOff(GUI_BUTTON btn)
    {
        bool fLeftButtonState = inputs.gfLeftButtonState;

        inputs.gfLeftButtonState = false;
        btn.MouseRegion.HasMouse = true;

        DrawButtonFromPtr(btn);

        inputs.gfLeftButtonState = fLeftButtonState;
    }

    public static void DrawCheckBoxButtonOn(GUI_BUTTON btn)
    {
    }

    private static void DrawGenericButton(ref GUI_BUTTON b, int id = 0)
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
            if (!b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && (GenericButtonGrayed[b.ButtonPicture] == null))
            {
                BPic = GenericButtonOffNormal[b.ButtonPicture];
            }
            else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                if (b.MouseRegion.HasMouse && (GenericButtonOnHilite[b.ButtonPicture] != null) && Globals.gfRenderHilights)
                {
                    BPic = GenericButtonOnHilite[b.ButtonPicture];
                }
                else
                {
                    BPic = GenericButtonOnNormal[b.ButtonPicture];
                }
            }
            else
            {
                if (b.MouseRegion.HasMouse
                    && (GenericButtonOffHilite[b.ButtonPicture] != null)
                    && Globals.gfRenderHilights)
                {
                    BPic = GenericButtonOffHilite[b.ButtonPicture];
                }
                else
                {
                    BPic = GenericButtonOffNormal[b.ButtonPicture];
                }
            }
        }
        else if (GenericButtonGrayed[b.ButtonPicture] is not null)
        {
            BPic = GenericButtonGrayed[b.ButtonPicture];
        }
        else
        {
            BPic = GenericButtonOffNormal[b.ButtonPicture];
            switch (b.bDisabledStyle)
            {
                case DISABLED_STYLE.DEFAULT:
                    Globals.gbDisabledButtonStyle = !string.IsNullOrWhiteSpace(b.stringText) ? DISABLED_STYLE.SHADED : DISABLED_STYLE.HATCHED;
                    break;
                case DISABLED_STYLE.HATCHED:
                case DISABLED_STYLE.SHADED:
                    Globals.gbDisabledButtonStyle = b.bDisabledStyle;
                    break;
            }
        }

        iBorderWidth = 3;
        iBorderHeight = 2;
        pTrav = null;

        // Compute the number of button "chunks" needed to be blitted
        width = b.MouseRegion.Bounds.Width - b.MouseRegion.Bounds.X;
        height = b.MouseRegion.Bounds.Height - b.MouseRegion.Bounds.Y;
        NumChunksWide = width / iBorderWidth;
        NumChunksHigh = height / iBorderHeight;
        hremain = height % iBorderHeight;
        wremain = width % iBorderWidth;

        cx = b.Loc.X + ((NumChunksWide - 1) * iBorderWidth) + wremain;
        cy = b.Loc.Y + ((NumChunksHigh - 1) * iBorderHeight) + hremain;

        // Fill the button's area with the button's background color
        VeldridVideoManager.ColorFillVideoSurfaceArea(
            ButtonDestBuffer,
            b.MouseRegion.Bounds,
            GenericButtonFillColors[b.ButtonPicture]);

        // If there is a background image, fill the button's area with it
        if (GenericButtonBackground[b.ButtonPicture] != null)
        {
            ox = oy = 0;
            // if the button was clicked on, adjust the background image so that we get
            // the illusion that it is sunk into the screen.
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                ox = oy = 1;
            }

            // Fill the area with the image, tilling it if need be.
            VeldridVideoManager.ImageFillVideoSurfaceArea(
                // ButtonDestBuffer,
                new Rectangle(
                    b.MouseRegion.Bounds.X + ox,
                    b.MouseRegion.Bounds.Y + oy,
                    b.MouseRegion.Bounds.Width,
                    b.MouseRegion.Bounds.Height),
                GenericButtonBackground[b.ButtonPicture],
                GenericButtonBackgroundindex[b.ButtonPicture],
                GenericButtonOffsetX[b.ButtonPicture],
                GenericButtonOffsetY[b.ButtonPicture]);
        }

        VeldridVideoManager.GetClippingRect(out ClipRect);

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

            if (Globals.gbPixelDepth == 16)
            {
                // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                //    uiDestPitchBYTES, BPic,
                //    (int)(b.Loc.X + (q * iBorderWidth)),
                //    (int)b.Loc.Y,
                //    (ushort)ImgNum, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                // video.Blt8BPPDataTo8BPPBufferTransparentClip(
                //     ref pDestBuf,
                //     uiDestPitchBYTES, BPic,
                //     (int)(b.Loc.X + (q * iBorderWidth)),
                //     (int)b.Loc.Y,
                //     (ushort)ImgNum, ref ClipRect);
            }

            if (q == 0)
            {
                ImgNum = 5;
            }
            else
            {
                ImgNum = 6;
            }

            if (Globals.gbPixelDepth == 16)
            {
                // video.Blt8BPPDataTo16BPPBufferTransparentClip(
                //     ref pDestBuf,
                //     uiDestPitchBYTES, BPic,
                //     (int)(b.Loc.X + (q * iBorderWidth)),
                //     cy, (ushort)ImgNum, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                // video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 (int)(b.Loc.X + (q * iBorderWidth)),
                //                                 cy, (ushort)ImgNum, ref ClipRect);
            }

        }
        // Blit the right side corners
        if (Globals.gbPixelDepth == 16)
        {
            // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
            //                                 uiDestPitchBYTES, BPic,
            //                                 cx, (int)b.Loc.Y,
            //                                 2, ref ClipRect);
        }
        else if (Globals.gbPixelDepth == 8)
        {
            // video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
            //                                 uiDestPitchBYTES, BPic,
            //                                 cx, (int)b.Loc.Y,
            //                                 2, ref ClipRect);
        }


        if (Globals.gbPixelDepth == 16)
        {
            // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
            //                                uiDestPitchBYTES, BPic,
            //                                cx, cy, 7, ref ClipRect);
        }
        else if (Globals.gbPixelDepth == 8)
        {
            // video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
            //                                 uiDestPitchBYTES, BPic,
            //                                 cx, cy, 7, ref ClipRect);
        }
        // Draw the vertical members of the button's borders
        NumChunksHigh--;

        if (hremain != 0)
        {
            q = NumChunksHigh;
            if (Globals.gbPixelDepth == 16)
            {
                // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 (int)b.Loc.X,
                //                                 (int)(b.Loc.Y + (q * iBorderHeight) - (iBorderHeight - hremain)),
                //                                 3, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                //video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                //                                uiDestPitchBYTES, BPic,
                //                                (int)b.Loc.X,
                //                                (int)(b.Loc.Y + (q * iBorderHeight) - (iBorderHeight - hremain)),
                //                                3, ref ClipRect);
            }

            if (Globals.gbPixelDepth == 16)
            {
                // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 cx, (int)(b.Loc.Y + (q * iBorderHeight) - (iBorderHeight - hremain)),
                //                                 4, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                //video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                //                                uiDestPitchBYTES, BPic,
                //                                cx, (int)(b.Loc.Y + (q * iBorderHeight) - (iBorderHeight - hremain)),
                //                                4, ref ClipRect);
            }
        }

        for (q = 1; q < NumChunksHigh; q++)
        {
            if (Globals.gbPixelDepth == 16)
            {
                //video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                //                                uiDestPitchBYTES, BPic,
                //                                (int)b.Loc.X,
                //                                (int)(b.Loc.Y + (q * iBorderHeight)),
                //                                3, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                // video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 (int)b.Loc.X,
                //                                 (int)(b.Loc.Y + (q * iBorderHeight)),
                //                                 3, ref ClipRect);
            }

            if (Globals.gbPixelDepth == 16)
            {
                // video.Blt8BPPDataTo16BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 cx, (int)(b.Loc.Y + (q * iBorderHeight)),
                //                                 4, ref ClipRect);
            }
            else if (Globals.gbPixelDepth == 8)
            {
                // video.Blt8BPPDataTo8BPPBufferTransparentClip(ref pDestBuf,
                //                                 uiDestPitchBYTES, BPic,
                //                                 cx, (int)(b.Loc.Y + (q * iBorderHeight)),
                //                                 4, ref ClipRect);
            }
        }
    }

    public static int GetButtonnUserData(GUI_BUTTON btn, int v)
    {
        throw new NotImplementedException();
    }

    private static void DrawQuickButton(ref GUI_BUTTON b)
    {
        int UseImage = 0;

        // Is button Enabled, or diabled but no "Grayed" image associated with this QuickButton?
        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            // Is the button's state ON?
            if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                // Is the mouse over this area, and we have a hilite image?
                if (b.MouseRegion.HasMouse
                    && Globals.gfRenderHilights
                    && b.ButtonPicture.OnHilite != -1)
                {
                    UseImage = b.ButtonPicture.OnHilite;            // Use On-Hilite image
                }
                else if (b.ButtonPicture.OnNormal != -1)
                {
                    UseImage = b.ButtonPicture.OnNormal;            // Use On-Normal image
                }
            }
            else
            {
                // Is the mouse over the button, and do we have hilite image?
                if (b.MouseRegion.HasMouse
                    && Globals.gfRenderHilights
                    && b.ButtonPicture.OffHilite != -1)
                {
                    UseImage = b.ButtonPicture.OffHilite;           // Use Off-Hilite image
                }
                else if (b.ButtonPicture.OffNormal != -1)
                {
                    UseImage = b.ButtonPicture.OffNormal;           // Use Off-Normal image
                }
            }
        }
        else if (b.ButtonPicture.Grayed != -1)
        {   // Button is diabled so use the "Grayed-out" image
            UseImage = b.ButtonPicture.Grayed;
        }
        else
        {
            UseImage = b.ButtonPicture.OffNormal;
            switch (b.bDisabledStyle)
            {
                case DISABLED_STYLE.DEFAULT:
                    Globals.gbDisabledButtonStyle = !string.IsNullOrWhiteSpace(b.stringText) ? DISABLED_STYLE.SHADED : DISABLED_STYLE.HATCHED;
                    break;
                case DISABLED_STYLE.HATCHED:
                case DISABLED_STYLE.SHADED:
                    Globals.gbDisabledButtonStyle = b.bDisabledStyle;
                    break;
            }
        }

        // Display the button image
        if (b.ButtonPicture.vobj is not null)
        {
            VeldridVideoManager.BltVideoObject(
                b.ButtonPicture.vobj,
                (ushort)UseImage,
                b.Loc.X,
                b.Loc.Y,
                UseImage);
        }
    }


    //=============================================================================
    //	MarkButtonsDirty
    //
    public static void MarkButtonsDirty(IEnumerable<GUI_BUTTON> buttons)
    {
        foreach (var b in buttons)
        {
            MarkAButtonDirty(b);
        }
    }

    public static void MarkButtonsDirty()
    {
        foreach (var b in ButtonList)
        {
            // If the button exists, and it's not owned by another object, draw it
            if (b is not null)
            {
                // Turn on dirty flag
                b.uiFlags |= ButtonFlags.BUTTON_DIRTY;
            }
        }
    }

    public static void UnmarkButtonsDirty()
    {
        for (int x = 0; x < Globals.MAX_BUTTONS; x++)
        {
            // If the button exists, and it's not owned by another object, draw it
            if (ButtonList[x] is not null)
            {
                UnMarkButtonDirty(ButtonList[x]);
            }
        }
    }

    public static void MarkAButtonDirty(GUI_BUTTON button)
    {
        // surgical dirtying . marks a user specified button dirty, without dirty the whole lot of them

        // If the button exists, and it's not owned by another object, draw it
        button.IsDirty = true;
    }

    public static bool DisableButton(GUI_BUTTON button)
    {
        return false;
    }

    private bool GetETRLEPixelValue(
        ref byte pDest,
        HVOBJECT hVObject,
        ushort usETRLEindex,
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
        // CHECKF(usETRLEindex < hVObject.usNumberOfObjects);

        pETRLEObject = hVObject.pETRLEObject[usETRLEindex];

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

    public static int GetWidthOfButtonPic(ButtonPic usButtonPicID, int iSlot)
    {
        return usButtonPicID.vobj.pETRLEObject[iSlot].usWidth;
    }

    public static ButtonPic UseLoadedButtonImage(
        ButtonPic LoadedImg,
        int Grayed,
        int OffNormal,
        int OffHilite,
        int OnNormal,
        int OnHilite)
    {
        int UseSlot;
        ETRLEObject pTrav;
        int MaxHeight, MaxWidth, ThisHeight, ThisWidth;
        ButtonPic buttonPic = new();

        // Is button image index given valid?
        // if (LoadedImg.vobj == null)
        // {
        //     // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Invalid button picture handle given for pre-loaded button image %d", LoadedImg));
        //     return ;
        // }
        // 
        // // Is button image an external vobject?
        // if (LoadedImg.fFlags.HasFlag(GUI_BTN.EXTERNAL_VOBJ))
        // {
        //     // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Invalid button picture handle given (%d), cannot use external images as duplicates.", LoadedImg));
        //     return -1;
        // }

        // is there ANY file to open?
        // if ((Grayed == BUTTON_NO_IMAGE)
        //     && (OffNormal == BUTTON_NO_IMAGE)
        //     && (OffHilite == BUTTON_NO_IMAGE)
        //     && (OnNormal == BUTTON_NO_IMAGE)
        //     && (OnHilite == BUTTON_NO_IMAGE))
        // {
        //     // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("No button pictures selected for pre-loaded button image %d", LoadedImg));
        //     return -1;
        // }

        // Init the QuickButton image structure with indexes to use
        buttonPic.vobj = LoadedImg.vobj;
        buttonPic.Grayed = Grayed;
        buttonPic.OffNormal = OffNormal;
        buttonPic.OffHilite = OffHilite;
        buttonPic.OnNormal = OnNormal;
        buttonPic.OnHilite = OnHilite;
        buttonPic.fFlags = GUI_BTN.DUPLICATE_VOBJ;

        // Fit the button size to the largest image in the set
        MaxWidth = MaxHeight = 0;
        if (Grayed != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[Grayed];
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

        if (OffNormal != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj!.pETRLEObject[OffNormal];
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

        if (OffHilite != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj!.pETRLEObject[OffHilite];
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

        if (OnNormal != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj!.pETRLEObject[OnNormal];
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

        if (OnHilite != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj!.pETRLEObject[OnHilite];
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
        buttonPic.MaxHeight = MaxHeight;
        buttonPic.MaxWidth = MaxWidth;

        // return the image slot number
        ButtonPicsLoaded++;
        return buttonPic;
    }

    public static ButtonPic LoadButtonImage(string filename, int Grayed, int OffNormal, int OffHilite, int OnNormal, int OnHilite)
    {
        int UseSlot;
        ETRLEObject pTrav;
        int MaxHeight, MaxWidth, ThisHeight, ThisWidth;
        ButtonPic buttonPic = new();

        //// is there ANY file to open?
        //if ((Grayed == BUTTON_NO_IMAGE)
        //    && (OffNormal == BUTTON_NO_IMAGE)
        //    && (OffHilite == BUTTON_NO_IMAGE)
        //    && (OnNormal == BUTTON_NO_IMAGE)
        //    && (OnHilite == BUTTON_NO_IMAGE))
        //{
        //    //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("No button pictures selected for %s", filename));
        //    return -1;
        //}

        // Get a button image slot
        // if ((UseSlot = FindFreeButtonSlot()) == BUTTON_NO_SLOT)
        // {
        //     //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Out of button image slots for %s", filename));
        //     return -1;
        // }

        // Load the image
        if ((buttonPic.vobj = VeldridVideoManager.CreateVideoObject(filename)) == null)
        {
            //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, String("Couldn't create VOBJECT for %s", filename));
            return null;
        }

        // Init the QuickButton image structure with indexes to use
        buttonPic.Grayed = Grayed;
        buttonPic.OffNormal = OffNormal;
        buttonPic.OffHilite = OffHilite;
        buttonPic.OnNormal = OnNormal;
        buttonPic.OnHilite = OnHilite;
        buttonPic.fFlags = GUI_BTN.NONE;

        // Fit the button size to the largest image in the set
        MaxWidth = MaxHeight = 0;
        if (Grayed != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[Grayed];
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

        if (OffNormal != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[OffNormal];
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

        if (OffHilite != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[OffHilite];
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

        if (OnNormal != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[OnNormal];
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

        if (OnHilite != Globals.BUTTON_NO_IMAGE)
        {
            pTrav = buttonPic.vobj.pETRLEObject[OnHilite];
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
        buttonPic.MaxHeight = MaxHeight;
        buttonPic.MaxWidth = MaxWidth;

        // return the image slot number
        ButtonPicsLoaded++;

        return buttonPic;
    }

    public static void ReleaseAnchorMode(Point coords)
    {
        if (Globals.gpAnchoredButton is null)
        {
            return;
        }

        if (Globals.gpAnchoredButton.MouseRegion.Bounds.Contains(coords))
        {
            //released outside button area, so restore previous button state.
            if (Globals.gfAnchoredState)
            {
                Globals.gpAnchoredButton.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else
            {
                Globals.gpAnchoredButton.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            }

            VeldridVideoManager.InvalidateRegion(Globals.gpAnchoredButton.MouseRegion.Bounds);
        }

        Globals.gpPrevAnchoredButton = Globals.gpAnchoredButton;
        Globals.gpAnchoredButton = null;
    }

    public static GUI_BUTTON QuickCreateButton(
        ButtonPic Image,
        Point loc,
        ButtonFlags Type,
        MSYS_PRIORITY Priority,
        GuiCallback? MoveCallback,
        GuiCallback ClickCallback)
    {
        GUI_BUTTON b = new();
        int ButtonNum;
        ButtonFlags BType;

        loc.Y = 480 - loc.Y;

        // Strip off any extraneous bits from button type
        BType = Type & (ButtonFlags.BUTTON_TYPE_MASK | ButtonFlags.BUTTON_NEWTOGGLE);

        // Set the values for this buttn
        b.IsDirty = true;
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
        b.sShadowColor = FontShadow.NO_SHADOW;
        b.sForeColorDown = FontColor.None;
        b.sShadowColorDown = FontShadow.NO_SHADOW;
        b.sForeColorHilited = FontColor.None;
        b.sShadowColorHilited = FontShadow.NO_SHADOW;
        b.bJustification = ButtonTextJustifies.BUTTON_TEXT_CENTER;
        b.bTextOffset = new(-1, -1);
        b.bTextSubOffSet = new(0, 0);
        b.fShiftText = true;
        //Init icon
        b.iIconID = -1;
        b.usIconindex = -1;
        b.bIconOffset = new(-1, -1);
        b.fShiftImage = true;
        //Init quickbutton
        // b.IdNum = ButtonNum;
        b.ButtonPicture = Image;
        b.Loc = loc;

        b.ubToggleButtonOldState = 0;
        b.ubToggleButtonActivated = 0;

        // Set the button click callback function (if any)
        if (ClickCallback != Globals.BUTTON_NO_CALLBACK)
        {
            b.ClickCallback = ClickCallback;
            BType |= ButtonFlags.BUTTON_CLICK_CALLBACK;
        }
        else
        {
            b.ClickCallback = Globals.BUTTON_NO_CALLBACK;
        }

        // Set the button's mouse movement callback function (if any)
        if (MoveCallback != Globals.BUTTON_NO_CALLBACK)
        {
            b.MoveCallback = MoveCallback;
            BType |= ButtonFlags.BUTTON_MOVE_CALLBACK;
        }
        else
        {
            b.MoveCallback = Globals.BUTTON_NO_CALLBACK;
        }

        b.MouseRegion = new(nameof(QuickCreateButton));

        // Define a MOUSE_REGION for this QuickButton

        var regionRect = new Rectangle(
            loc.X,
            loc.Y,
            Image.MaxWidth,
            Image.MaxHeight);

        MouseSubSystem.MSYS_DefineRegion(
            ref b.MouseRegion,
            regionRect,
            Priority,
            CURSOR.NORMAL,
            QuickButtonCallbackMouseMove,
            QuickButtonCallbackMButn);

        IVideoManager.DebugRenderer.DrawRectangle(regionRect, Color.Green);
        IVideoManager.DebugRenderer.DrawRectangle(b.MouseRegion.Bounds, Color.Red);

        // Link the MOUSE_REGION with this QuickButton
        MouseSubSystem.SetRegionUserData(ref b.MouseRegion, 0, b);

        // Set the flags for this button
        b.uiFlags |= ButtonFlags.BUTTON_ENABLED | BType | ButtonFlags.BUTTON_QUICK;

        //SpecifyButtonSoundScheme(b.IDNum, BUTTON_SOUND_SCHEME_GENERIC);

        // return the button number (slot)
        return b;
    }

    //=============================================================================
    //	QuickButtonCallbackMMove
    //
    //	Dispatches all button callbacks for mouse movement. This function gets
    //	called by the Mouse System. *DO NOT CALL DIRECTLY*
    //
    private static void QuickButtonCallbackMouseMove(ref MOUSE_REGION reg, MSYS_CALLBACK_REASON reason)
    {
        GUI_BUTTON b = (GUI_BUTTON)MouseSubSystem.GetRegionUserData(ref reg, 0);

        // sprintf(str, "QuickButtonCallbackMMove: Mouse Region #%d (%d,%d to %d,%d) has invalid buttonID %d",
        //                     reg.IDNumber, reg.Bounds.X, reg.Bounds.Y, reg.Bounds.Width, reg.Bounds.Height, iButtonID);

        if (b is null)
        {
            return;  //This is getting called when Adding new regions...
        }


        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) &&
              (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) || reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE)))
        {
            b.IsDirty = true;
        }

        // Mouse moved on the button, so reset it's timer to maximum.
        if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            //check for sound playing stuff
            if (b.ubSoundSchemeID != 0)
            {
                if (b.MouseRegion == MouseSubSystem.PreviousRegion && Globals.gpAnchoredButton is null)
                {
                    if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
                    {
                        PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_MOVED_ONTO);
                    }
                    else
                    {
                        PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_DISABLED_MOVED_ONTO);
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
                    if (b.MouseRegion == MouseSubSystem.PreviousRegion && Globals.gpAnchoredButton is null)
                    {
                        PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_MOVED_OFF_OF);
                    }
                }
                else
                {
                    PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_DISABLED_MOVED_OFF_OF);
                }
            }
        }

        // ATE: New stuff for toggle buttons that work with new Win95 paridigm
        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_NEWTOGGLE))
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
            {
                if (b.ubToggleButtonActivated != 0)
                {
                    b.ubToggleButtonActivated = 0;

                    if (b.ubToggleButtonOldState == 0)
                    {
                        b.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
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
        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)
            & b.uiFlags.HasFlag(ButtonFlags.BUTTON_MOVE_CALLBACK))
        {
            b.MoveCallback?.Invoke(ref b, reason);
        }
    }



    //=============================================================================
    //	QuickButtonCallbackMButn
    //
    //	Dispatches all button callbacks for button presses. This function is
    //	called by the Mouse System. *DO NOT CALL DIRECTLY*
    //
    private static void QuickButtonCallbackMButn(ref MOUSE_REGION reg, MSYS_CALLBACK_REASON reason)
    {
        GUI_BUTTON b;
        int iButtonID;
        bool MouseBtnDown;
        bool StateBefore, StateAfter = false;

        // Assert(reg != null);

        b = (GUI_BUTTON)MouseSubSystem.GetRegionUserData(ref reg, index: 0);

        //      sprintf(str, "QuickButtonCallbackMButn: Mouse Region #%d (%d,%d to %d,%d) has invalid buttonID %d",
        //                          reg.IDNumber, reg.Bounds.X, reg.Bounds.Y, reg.Bounds.Width, reg.Bounds.Height, iButtonID);

        if (b is null)
        {
            return;
        }

        MouseBtnDown = reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN)
            || reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN);

        StateBefore = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) ? true : false;

        // ATE: New stuff for toggle buttons that work with new Win95 paridigm
        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_NEWTOGGLE)
            && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                if (b.ubToggleButtonActivated == 0)
                {
                    if (!b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
                    {
                        b.ubToggleButtonOldState = 0;
                        b.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                    else
                    {
                        b.ubToggleButtonOldState = 1;
                        b.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    }
                    b.ubToggleButtonActivated = 1;
                }
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                b.ubToggleButtonActivated = 0;
            }
        }


        //Kris:
        //Set the anchored button incase the user moves mouse off region while still holding
        //down the button, but only if the button is up.  In Win95, buttons that are already
        //down, and anchored never change state, unless you release the mouse in the button area.

        if (b.MoveCallback == MouseSubSystem.DefaultMoveCallback
            && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                Globals.gpAnchoredButton = b;
                Globals.gfAnchoredState = StateBefore;
                b.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && b.uiFlags.HasFlag(ButtonFlags.BUTTON_NO_TOGGLE))
            {
                b.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            }
        }
        else if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CHECKBOX))
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {   //the check box button gets anchored, though it doesn't actually use the anchoring move callback.
                //The effect is different, we don't want to toggle the button state, but we do want to anchor this
                //button so that we don't effect any other buttons while we move the mouse around in anchor mode.
                Globals.gpAnchoredButton = b;
                Globals.gfAnchoredState = StateBefore;

                //Trick the before state of the button to be different so the sound will play properly as checkbox buttons 
                //are processed differently.
                StateBefore = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) ? false : true;
                StateAfter = !StateBefore;
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                b.uiFlags ^= ButtonFlags.BUTTON_CLICKED_ON; //toggle the checkbox state upon release inside button area.
                                                            //Trick the before state of the button to be different so the sound will play properly as checkbox buttons 
                                                            //are processed differently.
                StateBefore = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) ? false : true;
                StateAfter = !StateBefore;
            }
        }

        // Should we play a sound if clicked on while disabled?
        if (b.ubSoundSchemeID != 0 && !b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && MouseBtnDown)
        {
            PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_DISABLED_CLICK);
        }

        // If this button is disabled, and no callbacks allowed when disabled
        // callback
        if (!b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && !b.uiFlags.HasFlag(ButtonFlags.BUTTON_ALLOW_DISABLED_CALLBACK))
        {
            return;
        }

        // Button not enabled but allowed to use callback, then do that!
        if (!b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ALLOW_DISABLED_CALLBACK))
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
            Globals.gfDelayButtonDeletion = true;
            if ((reason & MSYS_CALLBACK_REASON.LBUTTON_UP) == 0
                || b.MoveCallback is not null
                && Globals.gpPrevAnchoredButton == b)
            {
                b.ClickCallback?.Invoke(ref b, reason);
            }

            Globals.gfDelayButtonDeletion = false;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) && !b.uiFlags.HasFlag(ButtonFlags.BUTTON_IGNORE_CLICKS))
        {
            // Otherwise, do default action with this button.
            b.uiFlags ^= ButtonFlags.BUTTON_CLICKED_ON;
        }

        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_CHECKBOX))
        {
            StateAfter = b.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) ? true : false;
        }

        // Play sounds for this enabled button (disabled sounds have already been done)
        if (b.ubSoundSchemeID != 0 && b.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
            {
                if (b.ubSoundSchemeID != 0 && StateBefore && !StateAfter)
                {
                    PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_CLICKED_OFF);
                }
            }
            else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                if (b.ubSoundSchemeID != 0 && !StateBefore && StateAfter)
                {
                    PlayButtonSound(b, ButtonSounds.BUTTON_SOUND_CLICKED_ON);
                }
            }
        }

        if (StateBefore != StateAfter)
        {
            VeldridVideoManager.InvalidateRegion(b.MouseRegion.Bounds);
        }

        if (Globals.gfPendingButtonDeletion)
        {
            RemoveButtonsMarkedForDeletion();
        }
    }

    private static void RemoveButtonsMarkedForDeletion()
    {
        throw new NotImplementedException();
    }

    private int GetNextButtonNumber()
    {
        for (int x = 0; x < Globals.MAX_BUTTONS; x++)
        {
            if (ButtonSubSystem.ButtonList[x] is null)
            {
                return x;
            }
        }

        return Globals.BUTTON_NO_SLOT;
    }

    public void Dispose()
    {
    }

    public static void RemoveButton(GUI_BUTTON iButtonID)
    {
        GUI_BUTTON b = iButtonID;

        // If button exists...
        if (b is null)
        {
            throw new InvalidOperationException("Attempting to remove a button that has already been deleted.");
        }

        //If we happen to be in the middle of a callback, and attempt to delete a button,
        //like deleting a node during list processing, then we delay it till after the callback
        //is completed.
        if (Globals.gfDelayButtonDeletion)
        {
            b.uiFlags |= ButtonFlags.BUTTON_DELETION_PENDING;
            Globals.gfPendingButtonDeletion = true;
            return;
        }

        //Kris:
        if (b.uiFlags.HasFlag(ButtonFlags.BUTTON_SELFDELETE_IMAGE))
        { //checkboxes and simple create buttons have their own graphics associated with them,
          //and it is handled internally.  We delete it here.  This provides the advantage of less
          //micromanagement, but with the disadvantage of wasting more memory if you have lots of
          //buttons using the same graphics.
            UnloadButtonImage(b.ButtonPicture);
        }

        // ...kill it!!!
        MouseSubSystem.MSYS_RemoveRegion(b.MouseRegion);

        if (b == Globals.gpAnchoredButton)
        {
            Globals.gpAnchoredButton = null;
        }

        if (b == Globals.gpPrevAnchoredButton)
        {
            Globals.gpPrevAnchoredButton = null;
        }

        b = null;
        iButtonID = null;
    }

    public static void UnloadButtonImage(ButtonPic buttonPic)
    {
        int x;
        bool fDone;

        if (buttonPic is null)
        {
            throw new InvalidOperationException("Attempting to UnloadButtonImage that has a null vobj (already deleted).");
        }

        // If this is a duplicated button image, then don't trash the vobject
        if (buttonPic.fFlags.HasFlag(GUI_BTN.DUPLICATE_VOBJ)
            || buttonPic.fFlags.HasFlag(GUI_BTN.EXTERNAL_VOBJ))
        {
            buttonPic.vobj = null;
            ButtonPicsLoaded--;
        }
        else
        {
            // Deleting a non-duplicate, so see if any dups present. if so, then
            // convert one of them to an original!

            fDone = false;
            for (x = 0; x < Globals.MAX_BUTTON_PICS && !fDone; x++)
            {
                if (buttonPic.fFlags.HasFlag(GUI_BTN.DUPLICATE_VOBJ))
                {
                    // If we got here, then we got a duplicate object of the one we
                    // want to delete, so convert it to an original!
                    // ButtonPictures[x].fFlags &= ~GUI_BTN.DUPLICATE_VOBJ;

                    // Now remove this button, but not it's vobject
                    buttonPic.vobj = null;

                    fDone = true;
                    ButtonPicsLoaded--;
                }
            }
        }

        // If image slot isn't empty, delete the image
        if (buttonPic.vobj is not null)
        {
            VeldridVideoManager.DeleteVideoObject(buttonPic.vobj);
            buttonPic.vobj = null;
            ButtonPicsLoaded--;
        }
    }

    public ValueTask<bool> Initialize() => Initialize(gameContext);
}

// GUI_BUTTON callback function type
public delegate void GuiCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason);

public class GUI_BUTTON
{
    // public int IdNum;                        // ID Number, contains it's own button number
    public ButtonPic ButtonPicture;                    // Image number to use (see DOCs for details)
    public MOUSE_REGION MouseRegion = new(nameof(GUI_BUTTON.MouseRegion)); // Mouse System's mouse region to use for this button
    public GuiCallback? ClickCallback;     // Button Callback when button is clicked
    public GuiCallback? MoveCallback;          // Button Callback when mouse moved on this region
    public CURSOR Cursor;                       // Cursor to use for this button
    public ButtonFlags uiFlags;                 // Button state flags etc.( 32-bit )
    public ButtonFlags uiOldFlags;              // Old flags from previous render loop
    public Point Loc;
    // public int Loc.X;                         // Coordinates where button is on the screen
    // public int Loc.Y;
    public object[] UserData = new object[4];          // Place holder for user data etc.
    public int Group;                        // Group this button belongs to (see DOCs)
    public DEFAULT_STATUS bDefaultStatus;
    //Button disabled style
    public DISABLED_STYLE bDisabledStyle;
    //For buttons with text
    public string? stringText;                      //the string
    public FontStyle usFont;                        //font for text 
    public bool fMultiColor;                        //font is a multi-color font
    public FontColor sForeColor;                    //text colors if there is text
    public FontShadow sShadowColor;
    public FontColor sForeColorDown;                //text colors when button is down (optional)
    public FontShadow sShadowColorDown;
    public FontColor sForeColorHilited;             //text colors when button is down (optional)
    public FontShadow sShadowColorHilited;
    public ButtonTextJustifies bJustification;      // BUTTON_TEXT_LEFT, BUTTON_TEXT_CENTER, BUTTON_TEXT_RIGHT
    public Point bTextOffset = new();
    // public int bTextXOffset;
    // public int bTextYOffset;

    public Point bTextSubOffSet = new();
    // public int bTextXSubOffSet;
    // public int bTextYSubOffSet;
    public bool fShiftText;
    public int sWrappedWidth;
    // For buttons with icons (don't confuse this with quickbuttons which have up to 5 states )
    public int iIconID;
    public int usIconindex;

    public Point bIconOffset = new();
    // public int bIconXOffset; //-1 means horizontally centered
    // public int bIconYOffset; //-1 means vertically centered
    public bool fShiftImage;  //if true, icon is shifted +1,+1 when button state is down.

    public int ubToggleButtonOldState;       // Varibles for new toggle buttons that work
    public int ubToggleButtonActivated;

    public int BackRect;                 // Handle to a Background Rectangle
    public int ubSoundSchemeID;

    public bool IsDirty { get; internal set; }
}

public class ButtonPic
{
    public HVOBJECT? vobj = new();                      // The Image itself
    public Texture? Texture { get; set; }
    public int Grayed;                   // index to use for a "Grayed-out" button
    public int OffNormal;            // index to use when button is OFF
    public int OffHilite;            // index to use when button is OFF w/ hilite on it
    public int OnNormal;             // index to use when button is ON
    public int OnHilite;             // index to use when button is ON w/ hilite on it
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

public enum BUTTON_SOUND_SCHEME
{
    NONE,
    GENERIC,
    VERYSMALLSWITCH1,
    VERYSMALLSWITCH2,
    SMALLSWITCH1,
    SMALLSWITCH2,
    SMALLSWITCH3,
    BIGSWITCH3,
    COMPUTERBEEP2,
    COMPUTERSWITCH1,
};
