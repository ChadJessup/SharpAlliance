using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core;

public static class Blitters
{
    public static Rectangle ClippingRect = new(0, 0, 640, 480);

    /**********************************************************************************************
	Blt16BPPBufferShadowRect

		Darkens a rectangular area by 25%. This blitter is used by ShadowVideoObjectRect.

	pBuffer						Pointer to a 16BPP buffer
	uiDestPitchBytes	Pitch of the destination surface
	area							An SGPRect, the area to darken

*********************************************************************************************/
    public static bool Blt16BPPBufferShadowRect(Image<Rgba32> pBuffer, Rectangle area)
    {
        // The area is given by its corners, and the right and bottom edges are part of it (as in the original).
        int left = Math.Max(area.Left, ClippingRect.Left);
        int top = Math.Max(area.Top, ClippingRect.Top);
        int right = Math.Min(area.Right, Math.Min(ClippingRect.Right, pBuffer.Width) - 1);
        int bottom = Math.Min(area.Bottom, Math.Min(ClippingRect.Bottom, pBuffer.Height) - 1);

        int width = right - left + 1;
        int height = bottom - top + 1;

        CHECKF(width >= 1);
        CHECKF(height >= 1);

        // darken every pixel by 25%
        pBuffer.ProcessPixelRows(accessor =>
        {
            for (int y = top; y <= bottom; y++)
            {
                foreach (ref Rgba32 pixel in accessor.GetRowSpan(y).Slice(left, width))
                {
                    pixel.R = (byte)(pixel.R * 3 / 4);
                    pixel.G = (byte)(pixel.G * 3 / 4);
                    pixel.B = (byte)(pixel.B * 3 / 4);
                }
            }
        });

        return true;
    }

    /**********************************************************************************************
        Blt16BPPBufferShadowRect

            Darkens a rectangular area by 25%. This blitter is used by ShadowVideoObjectRect.

        pBuffer						Pointer to a 16BPP buffer
        uiDestPitchBytes	Pitch of the destination surface
        area							An SGPRect, the area to darken

    *********************************************************************************************/
    public static bool Blt16BPPBufferShadowRectAlternateTable(Image<Rgba32> pBuffer, Rectangle area)
    {
        int width, height;
        int  LineSkip;
        Image<Rgba32> DestPtr;

        // Clipping
        //if (area.iLeft < ClippingRect.iLeft)
        //{
        //    area.iLeft = ClippingRect.iLeft;
        //}
        //
        //if (area.iTop < ClippingRect.iTop)
        //{
        //    area.iTop = ClippingRect.iTop;
        //}
        //
        //if (area.iRight >= ClippingRect.iRight)
        //{
        //    area.iRight = ClippingRect.iRight - 1;
        //}
        //
        //if (area.iBottom >= ClippingRect.iBottom)
        //{
        //    area.iBottom = ClippingRect.iBottom - 1;
        //}

        //CHECKF(area.iLeft >= ClippingRect.iLeft );
        //CHECKF(area.iTop >= ClippingRect.iTop );
        //CHECKF(area.iRight <= ClippingRect.iRight );
        //CHECKF(area.iBottom <= ClippingRect.iBottom );

        //DestPtr = (pBuffer + (area.iTop * (uiDestPitchBYTES / 2)) + area.iLeft);
        //width = area.iRight - area.iLeft + 1;
        //height = area.iBottom - area.iTop + 1;
        //LineSkip = (uiDestPitchBYTES - (width * 2));

        //CHECKF(width >= 1);
        //CHECKF(height >= 1);

        //            __asm {
        //                mov esi, OFFSET IntensityTable
        //                mov     edi, DestPtr
        //                xor     eax, eax
        //                mov     ebx, LineSkip
        //                mov     edx, height
        //
        //        BlitNewLine:
        //		mov ecx, width
        //
        //BlitLine:
        //        mov ax, [edi]
        //
        //        mov ax, [esi + eax * 2]
        //
        //        mov[edi], ax
        //        add     edi, 2
        //
        //        dec ecx
        //
        //        jnz BlitLine
        //
        //
        //        add edi, ebx
        //
        //        dec edx
        //
        //        jnz BlitNewLine
        //}

        return true;
    }
}
