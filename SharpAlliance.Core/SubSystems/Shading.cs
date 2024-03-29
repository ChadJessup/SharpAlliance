﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Shading
{
    // Defines for shade levels
    public const int DEFAULT_SHADE_LEVEL = 4;
    public const int MIN_SHADE_LEVEL = 4;
    public const int MAX_SHADE_LEVEL = 15;

    public byte[,] ubColorTables = new byte[VideoObjectManager.HVOBJECT_SHADE_TABLES + 3, 256];

    public ValueTask<bool> BuildShadeTable()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> BuildIntensityTable()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> DetermineRGBDistributionSettings()
    {
        return ValueTask.FromResult(true);
    }
}
