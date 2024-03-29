﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Rectangle = SixLabors.ImageSharp.Rectangle;

using static SharpAlliance.Core.Globals;
using Point = SixLabors.ImageSharp.Point;

namespace SharpAlliance.Core.Managers;

public class VideoObjectManager
{
    public const int DEFAULT_VIDEO_OBJECT_LIST_SIZE = 10;

    public const int COMPRESS_TRANSPARENT = 0x80;
    public const int COMPRESS_RUN_MASK = 0x7F;

    public const int HVOBJECT_SHADE_TABLES = 48;

    public const int HVOBJECT_GLOW_GREEN = 0;
    public const int HVOBJECT_GLOW_BLUE = 1;
    public const int HVOBJECT_GLOW_YELLOW = 2;
    public const int HVOBJECT_GLOW_RED = 3;

    private readonly ILogger<VideoObjectManager> logger;
    public VideoObjectManager(ILogger<VideoObjectManager> logger)
    {
        this.logger = logger;

        Globals.gpVObjectTail = Globals.gpVObjectHead;
        Globals.gfVideoObjectsInit = true;

        this.IsInitialized = true;
    }

    public bool IsInitialized { get; }

    public ValueTask<bool> Initialize()
    {
        this.logger.LogDebug(LoggingEventId.VIDEOOBJECT, "Video Object Manager");
        Globals.gpVObjectHead = Globals.gpVObjectTail = null;
        Globals.gfVideoObjectsInit = true;
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    public static bool BltVideoObject(
        SurfaceType uiDestVSurface,
        HVOBJECT hSrcVObject,
        ushort usRegionIndex,
        int iDestX,
        int iDestY,
        VO_BLT fBltFlags,
        blt_fx? pBltFx)
    {
        uint uiPitch;

        // Now we have the video object and surface, call the VO blitter function
        if (!BltVideoObjectToBuffer(
            out Image<Rgba32> pBuffer,
            16,
            hSrcVObject,
            usRegionIndex,
            iDestX,
            iDestY,
            fBltFlags,
            pBltFx))
        {
            // UnLockVideoSurface(uiDestVSurface);
            // VO Blitter will set debug messages for error conditions
            return false;
        }

        // UnLockVideoSurface(uiDestVSurface);

        return true;
    }

    private static Rectangle? ClippingRect = new(0, 0, 640, 480);

    public static bool BltVideoObjectToBuffer(
        out Image<Rgba32> pBuffer,
        uint uiDestPitchBYTES,
        HVOBJECT hSrcVObject,
        ushort usIndex,
        int iDestX,
        int iDestY,
        VO_BLT fBltFlags,
        blt_fx? pBltFx)
    {
        pBuffer = new(1, 1);

        if (hSrcVObject == null)
        {
            int i = 0;
        }

        // Check For Flags and bit depths
        switch (hSrcVObject.ubBitDepth)
        {
            case 16:
                break;

            case 8:

                // Switch based on flags given
                if (16 == 16)
                {

                    if ((fBltFlags & VO_BLT.SRCTRANSPARENCY) == 0)
                    {
                        if (BltIsClipped(hSrcVObject, iDestX, iDestY, usIndex, ref ClippingRect))
                        {
                            Blt8BPPDataTo16BPPBufferTransparentClip(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex, ref ClippingRect);
                        }
                        else
                        {
                            Blt8BPPDataTo16BPPBufferTransparent(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex);
                        }

                        break;
                    }
                    else if ((fBltFlags & VO_BLT.SHADOW) == 0)
                    {
                        if (BltIsClipped(hSrcVObject, iDestX, iDestY, usIndex, ref ClippingRect))
                        {
                            Blt8BPPDataTo16BPPBufferShadowClip(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex, ref ClippingRect);
                        }
                        else
                        {
                            Blt8BPPDataTo16BPPBufferShadow(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex);
                        }

                        break;
                    }

                }
                // else if (gbPixelDepth == 8)
                // {
                //     if (fBltFlags & VO_BLT.SRCTRANSPARENCY)
                //     {
                //         if (BltIsClipped(hSrcVObject, iDestX, iDestY, usIndex, &ClippingRect))
                //         {
                //             Blt8BPPDataTo8BPPBufferTransparentClip(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex, &ClippingRect);
                //         }
                //         else
                //         {
                //             Blt8BPPDataTo8BPPBufferTransparent(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex);
                //         }
                // 
                //         break;
                //     }
                //     else if (fBltFlags & VO_BLT.SHADOW)
                //     {
                //         if (BltIsClipped(hSrcVObject, iDestX, iDestY, usIndex, &ClippingRect))
                //         {
                //             Blt8BPPDataTo8BPPBufferShadowClip(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex, &ClippingRect);
                //         }
                //         else
                //         {
                //             Blt8BPPDataTo8BPPBufferShadow(pBuffer, uiDestPitchBYTES, hSrcVObject, iDestX, iDestY, usIndex);
                //         }
                // 
                //         break;
                //     }
                // }
                // Use default blitter here
                //Blt8BPPDataTo16BPPBuffer( hDestVObject, hSrcVObject, (int)iDestX, (int)iDestY, (SGPRect*)&SrcRect );

                break;
        }

        return true;
    }

    private static void Blt8BPPDataTo16BPPBufferShadow(Image<Rgba32> pBuffer, uint uiDestPitchBYTES, HVOBJECT hSrcVObject, int iDestX, int iDestY, ushort usIndex)
    {
    }

    private static bool Blt8BPPDataTo16BPPBufferShadowClip(
        Image<Rgba32> pBuffer,
        uint uiDestPitchBYTES,
        HVOBJECT hSrcVObject,
        int iX,
        int iY,
        ushort usIndex,
        ref Rectangle? clipregion)
    {
        ushort p16BPPPalette;
        uint uiOffset;
        uint Unblitted;
        Size us;
        ushort SrcPtr, DestPtr;
        uint LineSkip;
        ETRLEObject pTrav;
        Point iTemp = new();
        int LeftSkip, RightSkip, TopSkip, BottomSkip, BlitLength, BlitHeight;
        int ClipX1, ClipY1, ClipX2, ClipY2;

        // Get Offsets from Index into structure
        pTrav = hSrcVObject.pETRLEObject[usIndex];
        us = new(pTrav.usWidth, pTrav.usHeight);
        uiOffset = pTrav.uiDataOffset;

        // Add to start position of dest buffer
        iTemp.X = iX + pTrav.sOffsetX;
        iTemp.Y = iY + pTrav.sOffsetY;

        if (clipregion == null)
        {
            ClipX1 = ClippingRect.Value.Left;
            ClipY1 = ClippingRect.Value.Top;
            ClipX2 = ClippingRect.Value.Right;
            ClipY2 = ClippingRect.Value.Bottom;
        }
        else
        {
            ClipX1 = clipregion.Value.Left;
            ClipY1 = clipregion.Value.Top;
            ClipX2 = clipregion.Value.Right;
            ClipY2 = clipregion.Value.Bottom;
        }

        // Calculate rows hanging off each side of the screen
        LeftSkip = Math.Min(ClipX1 - Math.Min(ClipX1, iTemp.X), (int)us.Width);
        RightSkip = Math.Min(Math.Max(ClipX2, iTemp.X + (int)us.Width) - ClipX2, (int)us.Width);
        TopSkip = Math.Min(ClipY1 - Math.Min(ClipY1, iTemp.Y), (int)us.Height);
        BottomSkip = Math.Min(Math.Max(ClipY2, iTemp.Y + (int)us.Height) - ClipY2, (int)us.Height);

        // calculate the remaining rows and columns to blit
        BlitLength = (int)us.Width - LeftSkip - RightSkip;
        BlitHeight = (int)us.Height - TopSkip - BottomSkip;

        // whole thing is clipped
        if ((LeftSkip >= (int)us.Width) || (RightSkip >= (int)us.Width))
        {
            return true;
        }

        // whole thing is clipped
        if ((TopSkip >= (int)us.Height) || (BottomSkip >= (int)us.Height))
        {
            return true;
        }


        // SrcPtr = (ushort)hSrcVObject.pPixData + uiOffset;
        // DestPtr = (ushort)pBuffer + (uiDestPitchBYTES * (iTempY + TopSkip)) + ((iTempX + LeftSkip) * 2);
        // p16BPPPalette = hSrcVObject.pShadeCurrent;
        // LineSkip = uiDestPitchBYTES - (BlitLength * 2);

        //             __asm {
        // 
        //                 mov esi, SrcPtr
        //         
        //         mov edi, DestPtr
        //         
        //         mov edx, OFFSET ShadeTable
        //     xor     eax, eax
        //     mov     ebx, TopSkip
        //     xor     ecx, ecx
        // 
        //     or      ebx, ebx                            // check for nothing clipped on top
        //     jz      LeftSkipSetup
        // 
        // TopSkipLoop:										// Skips the number of lines clipped at the top
        // 
        // 		mov cl, [esi]
        //         
        //         inc esi
        //         
        //         or cl, cl
        //         
        //         js TopSkipLoop
        //         
        //         jz TSEndLine
        //         
        // 
        //         add esi, ecx
        //         
        //         jmp TopSkipLoop
        //         
        // TSEndLine:
        //                 dec ebx
        //         
        //         jnz TopSkipLoop
        //         
        // 
        // 
        // 
        // LeftSkipSetup:
        // 
        //                 mov Unblitted, 0
        //         
        //         mov ebx, LeftSkip                   // check for nothing clipped on the left
        // 
        //         or ebx, ebx
        //         
        //         jz BlitLineSetup
        //         
        // LeftSkipLoop:
        // 
        //                 mov cl, [esi]
        //         
        //         inc esi
        //         
        // 
        //         or cl, cl
        //         
        //         js LSTrans
        //         
        // 
        //         cmp ecx, ebx
        //         
        //         je LSSkip2                              // if equal, skip whole, and start blit with new run
        // 
        //         jb LSSkip1                              // if less, skip whole thing
        // 
        // 
        //         add esi, ebx                            // skip partial run, jump into normal loop for rest
        // 
        //         sub ecx, ebx
        //         
        //         mov ebx, BlitLength
        //         
        //         mov Unblitted, 0
        //         
        //         jmp BlitNonTransLoop
        //         
        // LSSkip2:
        //                 add esi, ecx                            // skip whole run, and start blit with new run
        // 
        //         jmp BlitLineSetup
        //         
        // 
        // LSSkip1:
        //                 add esi, ecx                            // skip whole run, continue skipping
        // 
        //         sub ebx, ecx
        //         
        //         jmp LeftSkipLoop
        //         
        // 
        // LSTrans:
        //                 and ecx, 07fH
        //             cmp     ecx, ebx
        //             je      BlitLineSetup                   // if equal, skip whole, and start blit with new run
        //             jb      LSTrans1                            // if less, skip whole thing
        // 
        //             sub     ecx, ebx                            // skip partial run, jump into normal loop for rest
        //             mov     ebx, BlitLength
        //             jmp     BlitTransparent
        // 
        // 
        //     LSTrans1:
        // 		sub ebx, ecx                            // skip whole run, continue skipping
        // 
        //         jmp LeftSkipLoop
        //         
        // 
        // 
        // 
        // BlitLineSetup:                                  // Does any actual blitting (trans/non) for the line
        //                 mov ebx, BlitLength
        //         
        //         mov Unblitted, 0
        //         
        // BlitDispatch:
        // 
        //                 or ebx, ebx                         // Check to see if we're done blitting
        // 
        //         jz RightSkipLoop
        //         
        // 
        //         mov cl, [esi]
        //         
        //         inc esi
        //         
        //         or cl, cl
        //         
        //         js BlitTransparent
        //         
        // BlitNonTransLoop:
        // 
        //                 cmp ecx, ebx
        //         
        //         jbe BNTrans1
        //         
        // 
        //         sub ecx, ebx
        //         
        //         mov Unblitted, ecx
        //         
        //         mov ecx, ebx
        //         
        // BNTrans1:
        //                 sub ebx, ecx
        //         
        // 
        //         clc
        //         rcr     cl, 1
        //         
        //         jnc BlitNTL2
        //         
        // 
        //         mov ax, [edi]
        //         
        //         mov ax, [edx + eax * 2]
        //         
        //         mov[edi], ax
        // 
        //    inc     esi
        //    add     edi, 2
        //         
        // BlitNTL2:
        //                 clc
        //                 rcr     cl, 1
        //         
        //         jnc BlitNTL3
        //         
        // 
        //         mov ax, [edi]
        //         
        //         mov ax, [edx + eax * 2]
        //         
        //         mov[edi], ax
        // 
        //    mov     ax, [edi+2]
        // 		mov ax, [edx + eax * 2]
        //         
        //         mov[edi + 2], ax
        // 
        //      add     esi, 2
        //         
        //         add edi, 4
        //         
        // BlitNTL3:
        // 
        //                 or cl, cl
        //         
        //         jz BlitLineEnd
        //         
        // BlitNTL4:
        // 
        //                 mov ax, [edi]
        //         
        //         mov ax, [edx + eax * 2]
        //         
        //         mov[edi], ax
        // 
        //    mov     ax, [edi+2]
        // 		mov ax, [edx + eax * 2]
        //         
        //         mov[edi + 2], ax
        // 
        //      mov     ax, [edi+4]
        // 		mov ax, [edx + eax * 2]
        //         
        //         mov[edi + 4], ax
        // 
        //      mov     ax, [edi+6]
        // 		mov ax, [edx + eax * 2]
        //         
        //         mov[edi + 6], ax
        // 
        //      add     esi, 4
        //         
        //         add edi, 8
        //         
        //         dec cl
        //         
        //         jnz BlitNTL4
        //         
        // BlitLineEnd:
        //                 add esi, Unblitted
        //         
        //         jmp BlitDispatch
        //         
        // BlitTransparent:
        // 
        //                 and ecx, 07fH
        //             cmp     ecx, ebx
        //             jbe     BTrans1
        // 
        //             mov     ecx, ebx
        // 
        //     BTrans1:
        // 
        // 		sub ebx, ecx
        //         //		shl		ecx, 1
        //                 add ecx, ecx
        //         
        //         add edi, ecx
        //         
        //         jmp BlitDispatch
        //         
        // 
        // RightSkipLoop:
        // 
        // 
        //             RSLoop1:
        //                 mov al, [esi]
        //         
        //         inc esi
        //         
        //         or al, al
        //         
        //         jnz RSLoop1
        //         
        // 
        //         dec BlitHeight
        //         
        //         jz BlitDone
        //         
        //         add edi, LineSkip
        //         
        // 
        //         jmp LeftSkipSetup
        //         
        // 
        // BlitDone:
        // 
        //     }

        return true;
    }

    private static void Blt8BPPDataTo16BPPBufferTransparent(Image<Rgba32> pBuffer, uint uiDestPitchBYTES, HVOBJECT hSrcVObject, int iDestX, int iDestY, ushort usIndex)
    {
    }

    private static void Blt8BPPDataTo16BPPBufferTransparentClip(Image<Rgba32> pBuffer, uint uiDestPitchBYTES, HVOBJECT hSrcVObject, int iDestX, int iDestY, ushort usIndex, ref Rectangle? clipRegion)
    {

    }


    /**********************************************************************************************
     BltIsClipped

        DeterMath.Mines whether a given blit will need clipping or not. Returns true/false.

    **********************************************************************************************/
    private static bool BltIsClipped(HVOBJECT hSrcVObject, int iX, int iY, ushort usIndex, ref Rectangle? clipregion)
    {
        uint usHeight, usWidth;
        ETRLEObject pTrav;
        int iTempX, iTempY;
        int ClipX1, ClipY1, ClipX2, ClipY2;

        // Get Offsets from Index into structure
        pTrav = hSrcVObject.pETRLEObject[usIndex];
        usHeight = (uint)pTrav.usHeight;
        usWidth = (uint)pTrav.usWidth;

        // Add to start position of dest buffer
        iTempX = iX + pTrav.sOffsetX;
        iTempY = iY + pTrav.sOffsetY;

        if (clipregion == null)
        {
            ClipX1 = ClippingRect!.Value.Left;
            ClipY1 = ClippingRect!.Value.Top;
            ClipX2 = ClippingRect!.Value.Right;
            ClipY2 = ClippingRect!.Value.Bottom;
        }
        else
        {
            ClipX1 = clipregion.Value.Left;
            ClipY1 = clipregion.Value.Top;
            ClipX2 = clipregion.Value.Right;
            ClipY2 = clipregion.Value.Bottom;
        }

        // Calculate rows hanging off each side of the screen
        if (Math.Max(ClipX1 - Math.Min(ClipX1, iTempX), (int)usWidth) != 0)
        {
            return true;
        }

        if (Math.Max(Math.Max(ClipX2, iTempX + (int)usWidth) - ClipX2, (int)usWidth) != 0)
        {
            return true;
        }

        if (Math.Max(ClipY1 - Math.Max(ClipY1, iTempY), (int)usHeight) != 0)
        {
            return true;
        }

        if (Math.Max(Math.Max(ClipY2, iTempY + (int)usHeight) - ClipY2, (int)usHeight) != 0)
        {
            return true;
        }

        return false;
    }

    public static bool DeleteVideoObjectFromIndex(int uiLogoID)
    {
        return true;
    }

    public int CountVideoObjectNodes()
    {
        VOBJECT_NODE? curr = Globals.gpVObjectHead;
        int i = 0;

        while (curr is not null)
        {
            i++;
            curr = curr.next;
        }

        return i;
    }
}

public class VOBJECT_NODE
{
    public HVOBJECT hVObject;
    public int uiIndex;
    public VOBJECT_NODE? next;
    public VOBJECT_NODE? prev;

    public int? pName;
    public int? pCode;
}

// Effects structure for specialized blitting
public struct blt_fx
{
    public int uiShadowLevel;
    public Rectangle ClipRect;
}


// Z-buffer info structure for properly assigning Z values
public struct ZStripInfo
{
    public int bInitialZChange;       // difference in Z value between the leftmost and base strips
    public int ubFirstZStripWidth;   // # of pixels in the leftmost strip
    public int ubNumberOfZChanges;   // number of strips (after the first)
    public List<int> pbZChange;            // change to the Z value in each strip (after the first)
}

public struct SixteenBPPObjectInfo
{
    public int p16BPPData;
    public int usRegionIndex;
    public int ubShadeLevel;
    public int usWidth;
    public int usHeight;
    public int sOffsetX;
    public int sOffsetY;
}
