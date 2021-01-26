using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.SubSystems
{
    public class ButtonSubSystem : IDisposable
    {
        private static class Constants
        {
            public const int MAX_GENERIC_PICS = 40;
            public const int MAX_BUTTON_ICONS = 40;
            public const int MAX_BUTTON_PICS = 256;
            public const int MAX_BUTTONS = 400;

            public const Surfaces BUTTON_USE_DEFAULT = Surfaces.Unknown;
            public static readonly int? BUTTON_NO_FILENAME = null;
            public static readonly int? BUTTON_NO_CALLBACK = null;
            public const int BUTTON_NO_IMAGE = -1;
            public const int BUTTON_NO_SLOT = -1;

            public const int BUTTON_INIT = 1;
            public const int BUTTON_WAS_CLICKED = 2;

            public const string DEFAULT_GENERIC_BUTTON_OFF = "GENBUTN.STI";
            public const string DEFAULT_GENERIC_BUTTON_ON = "GENBUTN2.STI";
            public const string DEFAULT_GENERIC_BUTTON_OFF_HI = "GENBUTN3.STI";
            public const string DEFAULT_GENERIC_BUTTON_ON_HI = "GENBUTN4.STI";
        }

        private readonly ILogger<ButtonSubSystem> logger;
        private readonly GameContext gameContext;
        private VeldridVideoManager video;

        public bool IsInitialized { get; private set; }

        private int gbDisabledButtonStyle;
        private GUI_BUTTON gpCurrentFastHelpButton;

        private bool gfRenderHilights = true;

        private int ButtonPicsLoaded;

        private Surfaces ButtonDestBuffer = Surfaces.BACKBUFFER;
        private uint ButtonDestPitch = 640 * 2;
        private uint ButtonDestBPP = 16;

        private List<GUI_BUTTON> ButtonList = new(Constants.MAX_BUTTONS);
        private HVOBJECT[] GenericButtonGrayed = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOffNormal = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOffHilite = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOnNormal = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonOnHilite = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private HVOBJECT[] GenericButtonBackground = new HVOBJECT[Constants.MAX_GENERIC_PICS];
        private ushort[] GenericButtonFillColors = new ushort[Constants.MAX_GENERIC_PICS];
        private ushort[] GenericButtonBackgroundIndex = new ushort[Constants.MAX_GENERIC_PICS];
        private short[] GenericButtonOffsetX = new short[Constants.MAX_GENERIC_PICS];
        private short[] GenericButtonOffsetY = new short[Constants.MAX_GENERIC_PICS];

        public const byte COMPRESS_TRANSPARENT = 0x80;
        public const byte COMPRESS_RUN_MASK = 0x7F;

        private HVOBJECT[] GenericButtonIcons = new HVOBJECT[Constants.MAX_BUTTON_ICONS];

        private int ButtonsInList = 0;

        public ButtonSubSystem(
            ILogger<ButtonSubSystem> logger,
            GameContext gameContext)
        {
            this.logger = logger;
            this.gameContext = gameContext;
        }

        public async ValueTask<bool> Initialize(GameContext gameContext)
        {
            this.video = (gameContext.VideoManager as VeldridVideoManager)!;

            this.IsInitialized = await this.InitializeButtonImageManager(
                Surfaces.Unknown,
                -1,
                -1);

            return this.IsInitialized;
        }

        public List<ButtonPics> ButtonPictures { get; } = new(Constants.MAX_BUTTON_PICS);

        private async ValueTask<bool> InitializeButtonImageManager(Surfaces DefaultBuffer, int DefaultPitch, int DefaultBPP)
        {
            VOBJECT_DESC vo_desc = new();
            byte Pix;
            int x;

            // Set up the default settings
            if (DefaultBuffer != Constants.BUTTON_USE_DEFAULT)
            {
                this.ButtonDestBuffer = DefaultBuffer;
            }
            else
            {
                this.ButtonDestBuffer = Surfaces.FRAME_BUFFER;
            }

            if (DefaultPitch != (int)Constants.BUTTON_USE_DEFAULT)
            {
                this.ButtonDestPitch = (uint)DefaultPitch;
            }
            else
            {
                this.ButtonDestPitch = 640 * 2;
            }

            if (DefaultBPP != (int)Constants.BUTTON_USE_DEFAULT)
            {
                this.ButtonDestBPP = (uint)DefaultBPP;
            }
            else
            {
                this.ButtonDestBPP = 16;
            }

            // Blank out all QuickButton images
            for (x = 0; x < Constants.MAX_BUTTON_PICS; x++)
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
            for (x = 0; x < Constants.MAX_GENERIC_PICS; x++)
            {
                //this.GenericButtonGrayed[x] = null;
                //this.GenericButtonOffNormal[x] = null;
                //this.GenericButtonOffHilite[x] = null;
                //this.GenericButtonOnNormal[x] = null;
                //this.GenericButtonOnHilite[x] = null;
                //this.GenericButtonBackground[x] = null;
                this.GenericButtonBackgroundIndex[x] = 0;
                this.GenericButtonFillColors[x] = 0;
                this.GenericButtonBackgroundIndex[x] = 0;
                this.GenericButtonOffsetX[x] = 0;
                this.GenericButtonOffsetY[x] = 0;
            }

            // Blank out all icon images
            for (x = 0; x < Constants.MAX_BUTTON_ICONS; x++)
            {
                // this.GenericButtonIcons[x] = null;
            }

            // Load the default generic button images
            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = Constants.DEFAULT_GENERIC_BUTTON_OFF;

            this.GenericButtonOffNormal[0] = await this.video.CreateVideoObject(vo_desc);
            if (this.GenericButtonOffNormal[0] == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_OFF);
                return false;
            }

            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = Constants.DEFAULT_GENERIC_BUTTON_ON;

            if ((this.GenericButtonOnNormal[0] = await this.video.CreateVideoObject(vo_desc)) == null)
            {
                //DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't create VOBJECT for "DEFAULT_GENERIC_BUTTON_ON);
                return false;
            }

            // Load up the off hilite and on hilite images. We won't check for errors because if the file
            // doesn't exists, the system simply ignores that file. These are only here as extra images, they
            // aren't required for operation (only OFF Normal and ON Normal are required).
            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = Constants.DEFAULT_GENERIC_BUTTON_OFF_HI;

            this.GenericButtonOffHilite[0] = await this.video.CreateVideoObject(vo_desc);

            vo_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            vo_desc.ImageFile = Constants.DEFAULT_GENERIC_BUTTON_ON_HI;

            this.GenericButtonOnHilite[0] = await this.video.CreateVideoObject(vo_desc);

            Pix = 0;
            if (!this.GetETRLEPixelValue(ref Pix, this.GenericButtonOffNormal[0], 8, 0, 0))
            {
                // DbgMessage(TOPIC_BUTTON_HANDLER, DBG_LEVEL_0, "Couldn't get generic button's background pixel value");
                return false;
            }

            this.GenericButtonFillColors[0] = this.GenericButtonOffNormal[0].Palette[Pix];

            return true;
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
            // CHECKF(hVObject != NULL);
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
                        offset += (byte)((usX - usLoopX) + 1);
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

        public void ReleaseAnchorMode()
        {
        }

        public void Dispose()
        {
        }
    }

    // GUI_BUTTON callback function type
    public delegate void GuiCallback(GUI_BUTTON button, int value);

    public struct GUI_BUTTON
    {
        public int IDNum;                        // ID Number, contains it's own button number
        public int ImageNum;                    // Image number to use (see DOCs for details)
        public MouseRegion Area;                          // Mouse System's mouse region to use for this button
        public GuiCallback ClickCallback;     // Button Callback when button is clicked
        public GuiCallback MoveCallback;          // Button Callback when mouse moved on this region
        public int Cursor;                       // Cursor to use for this button
        public int uiFlags;                 // Button state flags etc.( 32-bit )
        public int uiOldFlags;              // Old flags from previous render loop
        public int XLoc;                         // Coordinates where button is on the screen
        public int YLoc;
        public int[] UserData;//[4];          // Place holder for user data etc.
        public int Group;                        // Group this button belongs to (see DOCs)
        public int bDefaultStatus;
        //Button disabled style
        public int bDisabledStyle;
        //For buttons with text
        public string stringText;					//the string
        public int usFont;                      //font for text 
        public bool fMultiColor;            //font is a multi-color font
        public int sForeColor;               //text colors if there is text
        public int sShadowColor;
        public int sForeColorDown;       //text colors when button is down (optional)
        public int sShadowColorDown;
        public int sForeColorHilited;        //text colors when button is down (optional)
        public int sShadowColorHilited;
        public int bJustification;        // BUTTON_TEXT_LEFT, BUTTON_TEXT_CENTER, BUTTON_TEXT_RIGHT
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

    public struct ButtonPics
    {
        public HVOBJECT? vobj;                      // The Image itself
        public int Grayed;                   // Index to use for a "Grayed-out" button
        public int OffNormal;            // Index to use when button is OFF
        public int OffHilite;            // Index to use when button is OFF w/ hilite on it
        public int OnNormal;             // Index to use when button is ON
        public int OnHilite;             // Index to use when button is ON w/ hilite on it
        public int MaxWidth;                // Width of largest image in use
        public int MaxHeight;           // Height of largest image in use
        public int fFlags;                  // Special image flags
    }
}
