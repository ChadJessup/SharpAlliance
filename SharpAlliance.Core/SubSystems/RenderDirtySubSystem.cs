using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class RenderDirtySubSystem
{
    public RenderDirtySubSystem(GameContext context)
    {

    }

    public void RestoreBackgroundRects()
    {
    }

    public void ExecuteBaseDirtyRectQueue()
    {
    }

    public void RestoreExternBackgroundRect(ushort sLeft, short sTop, ushort sWidth, ushort sHeight)
    {
    }

    public static bool RestoreExternBackgroundRect(int sLeft, int sTop, int sWidth, int sHeight)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        int pDestBuf, pSrcBuf;

        Debug.Assert((sLeft >= 0) && (sTop >= 0) && (sLeft + sWidth <= 640) && (sTop + sHeight <= 480));

        pDestBuf = LockVideoSurface(Globals.guiRENDERBUFFER, out uiDestPitchBYTES);
        pSrcBuf = LockVideoSurface(Globals.guiSAVEBUFFER, out uiSrcPitchBYTES);

        if (Globals.gbPixelDepth == 16)
        {
            Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
                        pSrcBuf, uiSrcPitchBYTES,
                        sLeft, sTop,
                        sLeft, sTop,
                        sWidth, sHeight);
        }
        else if (Globals.gbPixelDepth == 8)
        {
            Blt8BPPTo8BPP(pDestBuf, uiDestPitchBYTES,
                        pSrcBuf, uiSrcPitchBYTES,
                        sLeft, sTop,
                        sLeft, sTop,
                        sWidth, sHeight);
        }

        UnLockVideoSurface(Globals.guiRENDERBUFFER);
        UnLockVideoSurface(Globals.guiSAVEBUFFER);

        // Add rect to frame buffer queue
        InvalidateRegionEx(sLeft, sTop, (sLeft + sWidth), (sTop + sHeight), 0);

        return (true);
    }
}
