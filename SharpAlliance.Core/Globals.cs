﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{
    public class Globals
    {
        public const int SHOW_MIN_FPS = 0;
        public const int SHOW_FULL_FPS = 1;

        public string? gubErrorText;
        public bool gfAniEditMode;
        public bool gfEditMode;
        public bool fFirstTimeInGameScreen;
        public bool fDirtyRectangleMode;
        public string? gDebugStr;
        public string? gSystemDebugStr;

        public bool gfMode;
        public int gsCurrentActionPoints;
        public int gbFPSDisplay;
        public bool gfResetInputCheck;
        public bool gfGlobalError;

        public int guiGameCycleCounter;

        // VIDEO OVERLAYS 
        public int giFPSOverlay;
        public int giCounterPeriodOverlay;
    }
}
