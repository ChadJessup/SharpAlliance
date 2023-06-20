using System;
using System.Diagnostics;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class RenderDirty
{
    private static IVideoManager video;
    private readonly SurfaceManager surfaces;

    public RenderDirty(IVideoManager videoManager, SurfaceManager surfaceManager)
    {
        video = videoManager;
        surfaces = surfaceManager;
    }

    public void RestoreBackgroundRects()
    {
    }

    public static bool UpdateVideoOverlay(VIDEO_OVERLAY_DESC? pTopmostDesc, int iBlitterIndex, bool fForceAll)
    {
        if (pTopmostDesc is null)
        {
            return false;
        }

        VOVERLAY_DESC uiFlags;
        int uiStringLength, uiStringHeight;

        if (iBlitterIndex != -1)
        {
            if (Globals.gVideoOverlays[iBlitterIndex].fAllocated == 0)
            {
                return (false);
            }

            uiFlags = pTopmostDesc.uiFlags;

            if (fForceAll)
            {
                Globals.gVideoOverlays[iBlitterIndex].uiFontID = pTopmostDesc.uiFontID;
                Globals.gVideoOverlays[iBlitterIndex].sX = pTopmostDesc.sX;
                Globals.gVideoOverlays[iBlitterIndex].sY = pTopmostDesc.sY;
                Globals.gVideoOverlays[iBlitterIndex].ubFontBack = pTopmostDesc.ubFontBack;
                Globals.gVideoOverlays[iBlitterIndex].ubFontFore = pTopmostDesc.ubFontFore;

                if (pTopmostDesc.pzText != null)
                {
                    Globals.gVideoOverlays[iBlitterIndex].zText = pTopmostDesc.pzText;
                }
            }
            else
            {
                if (uiFlags.HasFlag(VOVERLAY_DESC.TEXT))
                {
                    if (pTopmostDesc.pzText != null)
                    {
                        Globals.gVideoOverlays[iBlitterIndex].zText = pTopmostDesc.pzText;
                    }
                }


                if (uiFlags.HasFlag(VOVERLAY_DESC.DISABLED))
                {
                    Globals.gVideoOverlays[iBlitterIndex].fDisabled = pTopmostDesc.fDisabled;
                    DisableBackgroundRect(Globals.gVideoOverlays[iBlitterIndex].uiBackground, pTopmostDesc.fDisabled);
                }

                // If position has changed and flags are of type that use dirty rects, adjust
                if ((uiFlags.HasFlag(VOVERLAY_DESC.POSITION)))
                {

                    if (Globals.gVideoOverlays[iBlitterIndex].uiFlags.HasFlag(VOVERLAY.DIRTYBYTEXT))
                    {
                        // Get dims by supplied text
                        if (pTopmostDesc.pzText == null)
                        {
                            return (false);
                        }

                        uiStringLength = FontSubSystem.StringPixLength(Globals.gVideoOverlays[iBlitterIndex].zText, Globals.gVideoOverlays[iBlitterIndex].uiFontID);
                        uiStringHeight = FontSubSystem.GetFontHeight(Globals.gVideoOverlays[iBlitterIndex].uiFontID);

                        // Delete old rect
                        // Remove background
                        FreeBackgroundRectPending(Globals.gVideoOverlays[iBlitterIndex].uiBackground);

                        Globals.gVideoOverlays[iBlitterIndex].uiBackground = RegisterBackgroundRect(BGND_FLAG.PERMANENT, out var _, pTopmostDesc.sLeft, pTopmostDesc.sTop, (pTopmostDesc.sLeft + uiStringLength), (pTopmostDesc.sTop + uiStringHeight));
                        Globals.gVideoOverlays[iBlitterIndex].sX = pTopmostDesc.sX;
                        Globals.gVideoOverlays[iBlitterIndex].sY = pTopmostDesc.sY;

                    }

                }

            }

        }

        return (true);
    }

    public static bool RestoreExternBackgroundRectGivenID(int iBack)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        int sLeft, sTop, sWidth, sHeight;
        Image<Rgba32> pDestBuf, pSrcBuf;


        if (!gBackSaves[iBack].fAllocated)
        {
            return (false);
        }

        sLeft = gBackSaves[iBack].sLeft;
        sTop = gBackSaves[iBack].sTop;
        sWidth = gBackSaves[iBack].sWidth;
        sHeight = gBackSaves[iBack].sHeight;

        Debug.Assert((sLeft >= 0) && (sTop >= 0) && (sLeft + sWidth <= 640) && (sTop + sHeight <= 480));

        pDestBuf = video.LockVideoSurface(Surfaces.RENDER_BUFFER, out uiDestPitchBYTES);
        pSrcBuf = video.LockVideoSurface(Surfaces.SAVE_BUFFER, out uiSrcPitchBYTES);

        if (gbPixelDepth == 16)
        {
            video.Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
                        pSrcBuf, uiSrcPitchBYTES,
                        sLeft, sTop,
                        sLeft, sTop,
                        sWidth, sHeight);
        }
        else if (gbPixelDepth == 8)
        {
            video.Blt8BPPTo8BPP(pDestBuf, uiDestPitchBYTES,
                        pSrcBuf, uiSrcPitchBYTES,
                        sLeft, sTop,
                        sLeft, sTop,
                        sWidth, sHeight);
        }

        video.UnLockVideoSurface(Surfaces.RENDER_BUFFER);
        video.UnLockVideoSurface(Surfaces.SAVE_BUFFER);

        // Add rect to frame buffer queue
        video.InvalidateRegionEx(sLeft, sTop, (sLeft + sWidth), (sTop + sHeight), 0);

        return (true);
    }

    public static int RegisterBackgroundRect(BGND_FLAG uiFlags, out int? pSaveArea, int sLeft, int sTop, int sRight, int sBottom)
    {
        int uiBufSize;
        int iBackIndex;
        int ClipX1, ClipY1, ClipX2, ClipY2;
        int uiLeftSkip, uiRightSkip, uiTopSkip, uiBottomSkip;
        int usHeight, usWidth;
        int iTempX, iTempY;
        pSaveArea = -1;

        // Don't register if we are rendering and we are below the viewport
        //if ( sTop >= gsVIEWPORT_WINDOW_END_Y )
        //{
        //	return(-1 );
        //}

        ClipX1 = Globals.gDirtyClipRect.Left;
        ClipY1 = Globals.gDirtyClipRect.Top;
        ClipX2 = Globals.gDirtyClipRect.Right;
        ClipY2 = Globals.gDirtyClipRect.Bottom;

        usHeight = sBottom - sTop;
        usWidth = sRight - sLeft;

        //if((sClipLeft >= sClipRight) || (sClipTop >= sClipBottom))
        //	return(-1);
        iTempX = sLeft;
        iTempY = sTop;

        // Clip to rect
        uiLeftSkip = Math.Min(ClipX1 - Math.Min(ClipX1, iTempX), (int)usWidth);
        uiRightSkip = Math.Min(Math.Max(ClipX2, (iTempX + (int)usWidth)) - ClipX2, (int)usWidth);
        uiTopSkip = Math.Min(ClipY1 - Math.Min(ClipY1, iTempY), (int)usHeight);
        uiBottomSkip = Math.Min(Math.Max(ClipY2, (iTempY + (int)usHeight)) - ClipY2, (int)usHeight);

        // check if whole thing is clipped
        if ((uiLeftSkip >= (int)usWidth) || (uiRightSkip >= (int)usWidth))
        {
            return (-1);
        }

        // check if whole thing is clipped
        if ((uiTopSkip >= (int)usHeight) || (uiBottomSkip >= (int)usHeight))
        {
            return (-1);
        }

        // Set re-set values given based on clipping
        sLeft = sLeft + (int)uiLeftSkip;
        sRight = sRight - (int)uiRightSkip;
        sTop = sTop + (int)uiTopSkip;
        sBottom = sBottom - (int)uiBottomSkip;


        if (sLeft == 192 || sLeft == 188)
        {
            int i = 0;
        }

        if ((iBackIndex = GetFreeBackgroundBuffer()) == (-1))
        {
            return (-1);
        }

        Globals.gBackSaves[iBackIndex] = new BACKGROUND_SAVE();

        Globals.gBackSaves[iBackIndex].fZBuffer = false;

        if (pSaveArea == null)
        {
            uiBufSize = ((sRight - sLeft) * 2) * (sBottom - sTop);

            if (uiBufSize == 0)
            {
                return (-1);
            }

            if (uiFlags.HasFlag(BGND_FLAG.SAVERECT))
            {
                Globals.gBackSaves[iBackIndex].pSaveArea = new int[uiBufSize];
            }


            if (uiFlags.HasFlag(BGND_FLAG.SAVE_Z))
            {
                Globals.gBackSaves[iBackIndex].pZSaveArea = new int[uiBufSize];
                Globals.gBackSaves[iBackIndex].fZBuffer = true;
            }

            Globals.gBackSaves[iBackIndex].fFreeMemory = true;
        }
        //else
        //	gBackSaves[iBackIndex].pSaveArea=pSaveArea;

        Globals.gBackSaves[iBackIndex].fAllocated = true;
        Globals.gBackSaves[iBackIndex].uiFlags = uiFlags;
        Globals.gBackSaves[iBackIndex].sLeft = sLeft;
        Globals.gBackSaves[iBackIndex].sTop = sTop;
        Globals.gBackSaves[iBackIndex].sRight = sRight;
        Globals.gBackSaves[iBackIndex].sBottom = sBottom;
        Globals.gBackSaves[iBackIndex].sWidth = (sRight - sLeft);
        Globals.gBackSaves[iBackIndex].sHeight = (sBottom - sTop);

        Globals.gBackSaves[iBackIndex].fFilled = false;

        return (iBackIndex);
    }

    public static void RemoveVideoOverlay(int iVideoOverlay)
    {

        if (iVideoOverlay != -1 && Globals.gVideoOverlays[iVideoOverlay].fAllocated > 0)
        {
            // Check if we are actively scrolling
            if (Globals.gVideoOverlays[iVideoOverlay].fActivelySaving)
            {

                //		DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "Overlay Actively saving %d %S", iVideoOverlay, gVideoOverlays[ iVideoOverlay ].zText ) );

                Globals.gVideoOverlays[iVideoOverlay].fDeletionPending = true;
            }
            else
            {
                //RestoreExternBackgroundRectGivenID( gVideoOverlays[ iVideoOverlay ].uiBackground );

                // Remove background
                FreeBackgroundRect(Globals.gVideoOverlays[iVideoOverlay].uiBackground);

                //DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "Delete Overlay %d %S", iVideoOverlay, gVideoOverlays[ iVideoOverlay ].zText ) );


                // Remove save buffer if not done so
                if (Globals.gVideoOverlays[iVideoOverlay].pSaveArea != null)
                {
                    Globals.gVideoOverlays[iVideoOverlay].pSaveArea = null;
                }

                Globals.gVideoOverlays[iVideoOverlay].pSaveArea = null;


                // Set as not allocated
                Globals.gVideoOverlays[iVideoOverlay].fAllocated = 0;
            }
        }
    }

    public static bool FreeBackgroundRect(int iIndex)
    {
        if (iIndex != -1)
        {
            Globals.gBackSaves[iIndex].fAllocated = false;

            RecountBackgrounds();
        }

        return (true);
    }

    public static void RecountBackgrounds()
    {
        for (int uiCount = Globals.guiNumBackSaves - 1; (uiCount >= 0); uiCount--)
        {
            if ((Globals.gBackSaves[uiCount].fAllocated) || (Globals.gBackSaves[uiCount].fFilled))
            {
                Globals.guiNumBackSaves = (uiCount + 1);
                break;
            }
        }
    }

    public static bool FreeBackgroundRectPending(int iIndex)
    {
        Globals.gBackSaves[iIndex].fPendingDelete = true;

        return (true);
    }

    public static bool FreeBackgroundRectNow(int uiCount)
    {
        if (Globals.gBackSaves[uiCount].fFreeMemory == true)
        {
            //MemFree(gBackSaves[uiCount].pSaveArea);
            if (Globals.gBackSaves[uiCount].fZBuffer)
            {
                Globals.gBackSaves[uiCount].pZSaveArea = null;
            }
        }

        Globals.gBackSaves[uiCount].fZBuffer = false;
        Globals.gBackSaves[uiCount].fAllocated = false;
        Globals.gBackSaves[uiCount].fFreeMemory = false;
        Globals.gBackSaves[uiCount].fFilled = false;
        Globals.gBackSaves[uiCount].pSaveArea = null;

        RecountBackgrounds();
        return (true);
    }

    bool FreeBackgroundRectType(BGND_FLAG uiFlags)
    {
        int uiCount;

        for (uiCount = 0; uiCount < Globals.guiNumBackSaves; uiCount++)
        {
            if (Globals.gBackSaves[uiCount].uiFlags.HasFlag(uiFlags))
            {
                if (Globals.gBackSaves[uiCount].fFreeMemory == true)
                {
                    //MemFree(gBackSaves[uiCount].pSaveArea);
                    if (Globals.gBackSaves[uiCount].fZBuffer)
                    {
                        Globals.gBackSaves[uiCount].pZSaveArea = null;
                    }
                }

                Globals.gBackSaves[uiCount].fZBuffer = false;
                Globals.gBackSaves[uiCount].fAllocated = false;
                Globals.gBackSaves[uiCount].fFreeMemory = false;
                Globals.gBackSaves[uiCount].fFilled = false;
                Globals.gBackSaves[uiCount].pSaveArea = null;
            }
        }

        RecountBackgrounds();

        return (true);
    }

    public static int GetFreeBackgroundBuffer()
    {
        for (int uiCount = 0; uiCount < Globals.guiNumBackSaves; uiCount++)
        {
            if ((Globals.gBackSaves[uiCount].fAllocated == false) && (Globals.gBackSaves[uiCount].fFilled == false))
            {
                return uiCount;
            }
        }

        if (Globals.guiNumBackSaves < Globals.BACKGROUND_BUFFERS)
        {
            return Globals.guiNumBackSaves++;
        }

        return (-1);
    }

    public static void DisableBackgroundRect(int iIndex, bool fDisabled)
    {
        Globals.gBackSaves[iIndex].fDisabled = fDisabled;
    }


    public void ExecuteBaseDirtyRectQueue()
    {
    }

    public static bool RestoreExternBackgroundRect(int sLeft, int sTop, int sWidth, int sHeight)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf, pSrcBuf;

        Debug.Assert((sLeft >= 0) && (sTop >= 0) && (sLeft + sWidth <= 640) && (sTop + sHeight <= 480));
        
        pDestBuf = video.LockVideoSurface(Surfaces.RENDER_BUFFER, out uiDestPitchBYTES);
        pSrcBuf = video.LockVideoSurface(Surfaces.SAVE_BUFFER, out uiSrcPitchBYTES);

        if (Globals.gbPixelDepth == 16)
        {
            video.Blt16BPPTo16BPP(
                pDestBuf,
                uiDestPitchBYTES,
                pSrcBuf, uiSrcPitchBYTES,
                sLeft, sTop,
                sLeft, sTop,
                sWidth, sHeight);
        }
        else if (Globals.gbPixelDepth == 8)
        {
            video.Blt8BPPTo8BPP(pDestBuf, uiDestPitchBYTES,
                        pSrcBuf, uiSrcPitchBYTES,
                        sLeft, sTop,
                        sLeft, sTop,
                        sWidth, sHeight);
        }

        video.UnLockVideoSurface(Surfaces.RENDER_BUFFER);
        video.UnLockVideoSurface(Surfaces.SAVE_BUFFER);

        // Add rect to frame buffer queue
        video.InvalidateRegionEx(sLeft, sTop, (sLeft + sWidth), (sTop + sHeight), 0);

        return (true);
    }

    public static int RegisterVideoOverlay(VOVERLAY uiFlags, VIDEO_OVERLAY_DESC? pTopmostDesc)
    {
        int iBlitterIndex;
        int iBackIndex;
        int uiStringLength, uiStringHeight;

        if (uiFlags.HasFlag(VOVERLAY.DIRTYBYTEXT))
        {
            // Get dims by supplied text
            if (pTopmostDesc.pzText == null)
            {
                return (-1);
            }

            uiStringLength = FontSubSystem.StringPixLength(pTopmostDesc.pzText, pTopmostDesc.uiFontID);
            uiStringHeight = FontSubSystem.GetFontHeight(pTopmostDesc.uiFontID);

            iBackIndex = RegisterBackgroundRect(BGND_FLAG.PERMANENT, out var _, pTopmostDesc.sLeft, pTopmostDesc.sTop, (pTopmostDesc.sLeft + uiStringLength), (pTopmostDesc.sTop + uiStringHeight));


        }
        else
        {
            // Register background
            iBackIndex = RegisterBackgroundRect(BGND_FLAG.PERMANENT, out var _, pTopmostDesc.sLeft, pTopmostDesc.sTop, pTopmostDesc.sRight, pTopmostDesc.sBottom);
        }


        if (iBackIndex == -1)
        {
            return (-1);
        }


        // Get next free topmost blitter index
        if ((iBlitterIndex = GetFreeVideoOverlay()) == (-1))
        {
            return (-1);
        }

        // Init new blitter
        Globals.gVideoOverlays[iBlitterIndex] = new();

        Globals.gVideoOverlays[iBlitterIndex].uiFlags = uiFlags;
        Globals.gVideoOverlays[iBlitterIndex].fAllocated = 2;
        Globals.gVideoOverlays[iBlitterIndex].uiBackground = iBackIndex;
        Globals.gVideoOverlays[iBlitterIndex].pBackground = Globals.gBackSaves[iBackIndex];
        Globals.gVideoOverlays[iBlitterIndex].BltCallback = pTopmostDesc.BltCallback;

        // Update blitter info
        // Set update flags to zero since we are forcing all updates
        pTopmostDesc.uiFlags = 0;
        UpdateVideoOverlay(pTopmostDesc, iBlitterIndex, true);

        // Set disabled flag to true 
        if (uiFlags.HasFlag(VOVERLAY.STARTDISABLED))
        {
            Globals.gVideoOverlays[iBlitterIndex].fDisabled = true;
            DisableBackgroundRect(Globals.gVideoOverlays[iBlitterIndex].uiBackground, true);
        }

        Globals.gVideoOverlays[iBlitterIndex].uiDestBuff = Surfaces.FRAME_BUFFER;

        //DebugMsg( TOPIC_JA2, DBG_LEVEL_0, String( "Register Overlay %d %S", iBlitterIndex, gVideoOverlays[ iBlitterIndex ].zText ) );

        return (iBlitterIndex);

    }

    // OVERLAY STUFF
    public static int GetFreeVideoOverlay()
    {
        for (int uiCount = 0; uiCount < Globals.guiNumVideoOverlays; uiCount++)
        {
            if ((Globals.gVideoOverlays[uiCount].fAllocated == 0))
            {
                return (uiCount);
            }
        }

        if (Globals.guiNumVideoOverlays < Globals.BACKGROUND_BUFFERS)
        {
            return (Globals.guiNumVideoOverlays++);
        }

        return (-1);
    }
}

// Struct for init topmost blitter
public class VIDEO_OVERLAY_DESC
{
    public VOVERLAY_DESC uiFlags;
    public bool fDisabled;
    public int sLeft;
    public int sTop;
    public int sRight;
    public int sBottom;
    public FontStyle uiFontID;
    public int sX;
    public int sY;
    public FontColor ubFontBack;
    public FontColor ubFontFore;
    public string pzText;// [200];
    public OVERLAY_CALLBACK? BltCallback;
}

// Struct for topmost blitters
public class VIDEO_OVERLAY
{
    public VOVERLAY uiFlags;
    public int fAllocated;
    public bool fDisabled;
    public bool fActivelySaving;
    public bool fDeletionPending;
    public int uiBackground;
    public BACKGROUND_SAVE? pBackground;
    public int? pSaveArea;
    public int[] uiUserData = new int[5];
    public FontStyle uiFontID;
    public int sX;
    public int sY;
    public FontColor ubFontBack;
    public FontColor ubFontFore;
    public string zText;// [200];
    public Surfaces uiDestBuff;
    public OVERLAY_CALLBACK? BltCallback;
}

public delegate void OVERLAY_CALLBACK(VIDEO_OVERLAY video_overlay);

// Struct for backgrounds
public struct BACKGROUND_SAVE
{
    public bool fAllocated;
    public bool fFilled;
    public bool fFreeMemory;
    public bool fZBuffer;
    public bool fPendingDelete;
    public bool fDisabled;
    public int[]? pSaveArea;
    public int[]? pZSaveArea;
    public BGND_FLAG uiFlags;
    public int sLeft;
    public int sTop;
    public int sRight;
    public int sBottom;
    public int sWidth;
    public int sHeight;
}

[Flags]
public enum BGND_FLAG : uint
{
    PERMANENT = 0x80000000,
    SINGLE = 0x40000000,
    SAVE_Z = 0x20000000,
    MERC = 0x10000000,
    SAVERECT = 0x08000000,
    TOPMOST = 0x04000000,
    ANIMATED = 0x00000001,
}

[Flags]
public enum VOVERLAY
{
    DIRTYBYTEXT = 0x00000001,
    STARTDISABLED = 0x00000002,
}

[Flags]
public enum VOVERLAY_DESC
{
    TEXT = 0x00001000,
    DISABLED = 0x00002000,
    POSITION = 0x00004000,
}
