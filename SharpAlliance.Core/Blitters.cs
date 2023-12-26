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
    public static bool Blt16BPPBufferShadowRect(Image<Rgba32> pBuffer, int uiDestPitchBYTES, Rectangle area)
    {
        int width, height;
        uint LineSkip;
        Image<Rgba32> DestPtr;

        // Clipping
        if (area.X < ClippingRect.X)
        {
            area.X = ClippingRect.X;
        }

        if (area.Top < ClippingRect.Top)
        {
            area.Height = ClippingRect.Top;
        }

        if (area.Right >= ClippingRect.Right)
        {
            area.Width = ClippingRect.Width - 1;
        }

        if (area.Bottom >= ClippingRect.Bottom)
        {
            area.Y = ClippingRect.Y - 1;
        }

        //CHECKF(area.Left >= ClippingRect.Left );
        //CHECKF(area.Top >= ClippingRect.Top );
        //CHECKF(area.Right <= ClippingRect.Right );
        //CHECKF(area.Bottom <= ClippingRect.Bottom );

        //DestPtr = (pBuffer + (area.Top * (uiDestPitchBYTES / 2)) + area.Left);
        width = area.Right - area.Left + 1;
        height = area.Bottom - area.Top + 1;
        //LineSkip = (uiDestPitchBYTES - (width * 2));

        CHECKF(width >= 1);
        CHECKF(height >= 1);

        //        __asm {
        //            mov esi, OFFSET ShadeTable
        //            mov     edi, DestPtr
        //            xor     eax, eax
        //            mov     ebx, LineSkip
        //            mov     edx, height
        //
        //    BlitNewLine:
        //		mov ecx, width
        //    
        //BlitLine:
        //            mov ax, [edi]
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
        //    }

        return true;
    }

    /**********************************************************************************************
        Blt16BPPBufferShadowRect

            Darkens a rectangular area by 25%. This blitter is used by ShadowVideoObjectRect.

        pBuffer						Pointer to a 16BPP buffer
        uiDestPitchBytes	Pitch of the destination surface
        area							An SGPRect, the area to darken

    *********************************************************************************************/
    public static bool Blt16BPPBufferShadowRectAlternateTable(Image<Rgba32> pBuffer, int uiPitch, Rectangle area)
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
