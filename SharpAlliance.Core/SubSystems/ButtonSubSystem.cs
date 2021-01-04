using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class ButtonSubSystem : IDisposable
    {
        private static class Constants
        {
            public const int MAX_BUTTONS = 400;
            public const int MAX_BUTTON_PICS = 256;
        }

        private readonly ILogger<ButtonSubSystem> logger;
        private readonly MouseSubSystem mouseSystem;
        private int gbDisabledButtonStyle;
        private GUI_BUTTON gpCurrentFastHelpButton;

        private bool gfRenderHilights = true;

        private int ButtonPicsLoaded;

        private Surfaces ButtonDestBuffer = Surfaces.BACKBUFFER;
        private int ButtonDestPitch = 640 * 2;
        private int ButtonDestBPP = 16;

        private List<GUI_BUTTON> ButtonList = new(Constants.MAX_BUTTONS);

        private int ButtonsInList = 0;

        public ButtonSubSystem(ILogger<ButtonSubSystem> logger, MouseSubSystem mouseSubSystem)
        {
            this.logger = logger;
            this.mouseSystem = mouseSubSystem;
        }

        public List<ButtonPics> ButtonPictures { get; } = new(Constants.MAX_BUTTON_PICS);

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
        public HVOBJECT vobj;                      // The Image itself
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
