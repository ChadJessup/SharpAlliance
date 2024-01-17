﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static bool fFlashAssignDone = false;
    public static bool fCharacterInfoPanelDirty = true;
    public static bool gfLoadPending = false;
    public static bool fReDrawFace = false;
    public static bool fFirstTimeInMapScreen = true;
    public static bool fShowInventoryFlag = false;
    public static bool fMapInventoryItem = false;
    public static bool fShowDescriptionFlag = false;

    public static Path? pTempCharacterPath;
    public static Path? pTempHelicopterPath;
}
