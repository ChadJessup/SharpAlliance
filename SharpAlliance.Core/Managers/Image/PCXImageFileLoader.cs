using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SharpAlliance.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image;

public class PCXImageFileLoader : IImageFileLoader
{
    public const int PcxHeaderSize = 128;
    public const int PcxObjectSize = 784;

    public bool SetPcxPalette(ref HVOBJECT pCurrentPcxObject, ref HIMAGE hImage)
    {
        ushort Index;
        Rgba32[] pubPalette;

        pubPalette = pCurrentPcxObject.Palette;

        // Allocate memory for palette
        hImage.pPalette = new(256);//MemAlloc(sizeof(SGPPaletteEntry) * 256);

        if (hImage.pPalette == null)
        {
            return false;
        }

        // Initialize the proper palette entries
        for (Index = 0; Index < 256; Index++)
        {
            SGPPaletteEntry entry = new()
            {
                // peRed = (pubPalette[(Index * 3)]),
                // peGreen = (pubPalette[(Index * 3) + 1]),
                // peBlue = (pubPalette[(Index * 3) + 2]),
                peFlags = 0,
            };

            hImage.pPalette[Index] = entry;
        }

        return true;
    }

    public bool LoadImage(ref HIMAGE hImage, HIMAGECreateFlags fContents, IFileManager fileManager)
    {
        // First Load a PCX Image
        PcxObject? pPcxObject = this.LoadPcx(hImage.ImageFile, fileManager);

        if (pPcxObject == null)
        {
            return false;
        }

        // Set some header information
        hImage.usWidth = pPcxObject.usWidth;
        hImage.usHeight = pPcxObject.usHeight;
        hImage.ubBitDepth = 8;
        hImage.fFlags |= fContents;

        // Read and allocate bitmap block if requested
        if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA))
        {
            // Allocate memory for buffer
            hImage.p8BPPData = new byte[hImage.usWidth * hImage.usHeight];

            if (!this.BlitPcxToBuffer(pPcxObject, hImage.p8BPPData, hImage.usWidth, hImage.usHeight, 0, 0, false))
            {
                MemFree(hImage.p8BPPData);
                return false;
            }
        }

        if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_PALETTE))
        {
            this.SetPcxPalette(pPcxObject, hImage);

            // Create 16 BPP palette if flags and BPP justify 
            hImage.pui16BPPPalette = hImage.Create16BPPPalette(hImage.pPalette);

        }

        return true;
    }

    PcxObject? LoadPcx(string pFilename, IFileManager fileManager)
    {
        Stream hFileHandle;
        uint uiFileSize;
        byte[] pPcxBuffer;

        // Open and read in the file
        if ((hFileHandle = fileManager.FileOpen(pFilename, FileAccess.Read)) == Stream.Null)
        { // damn we failed to open the file
            return null;
        }

        uiFileSize = (uint)hFileHandle.Length;
        if (uiFileSize == 0)
        { // we failed to size up the file
            return null;
        }

        // Create enw pCX object
        PcxObject pCurrentPcxObject = new();
        pCurrentPcxObject.pPcxBuffer = new byte[(uiFileSize - (PCXImageFileLoader.PcxHeaderSize + 768))];

        if (pCurrentPcxObject.pPcxBuffer == null)
        {
            return null;
        }

        // Ok we now have a file handle, so let's read in the data
        Span<byte> headerBuffer = new byte[PCXImageFileLoader.PcxHeaderSize];
        fileManager.FileRead(hFileHandle, headerBuffer, out var _);

        PcxHeader Header = new()
        {
            ubManufacturer = headerBuffer[0],
            ubVersion = headerBuffer[1],
            ubEncoding = headerBuffer[2],
            ubBitsPerPixel = headerBuffer[3],
            usLeft = MemoryMarshal.Read<ushort>(headerBuffer[4..]),
            usTop = MemoryMarshal.Read<ushort>(headerBuffer[6..]),
            usRight = MemoryMarshal.Read<ushort>(headerBuffer[8..]),
            usBottom = MemoryMarshal.Read<ushort>(headerBuffer[10..]),
            usHorRez = MemoryMarshal.Read<ushort>(headerBuffer[12..]),
            usVerRez = MemoryMarshal.Read<ushort>(headerBuffer[14..]),
            ubEgaPalette = headerBuffer[16..65].ToArray(),
            ubReserved = headerBuffer[64],
            ubColorPlanes = headerBuffer[65],
            usBytesPerLine = MemoryMarshal.Read<ushort>(headerBuffer[66..]),
            usPaletteType = MemoryMarshal.Read<ushort>(headerBuffer[68..]),
            ubFiller = headerBuffer[72..128].ToArray(),
        };

        if ((Header.ubManufacturer != 10) || (Header.ubEncoding != 1))
        {
            // We have an invalid pcx format
            // Delete the object
            //MemFree(pCurrentPcxObject.pPcxBuffer);
            //MemFree(pCurrentPcxObject);
            return null;
        }

        if (Header.ubBitsPerPixel == 8)
        {
            pCurrentPcxObject.usPcxFlags = PCX.COLOR256;
        }
        else
        {
            pCurrentPcxObject.usPcxFlags = 0;
        }

        pCurrentPcxObject.usWidth = (ushort)(1 + (Header.usRight - Header.usLeft));
        pCurrentPcxObject.usHeight = (ushort)(1 + (Header.usBottom - Header.usTop));
        pCurrentPcxObject.uiBufferSize = uiFileSize - 768 - PcxHeaderSize;

        // We are ready to read in the pcx buffer data. Therefore we must lock the buffer
        //pPcxBuffer = pCurrentPcxObject.pPcxBuffer;

        Span<byte> buffer = new byte[pCurrentPcxObject.uiBufferSize];
        fileManager.FileRead(hFileHandle, buffer, out var _);
        pCurrentPcxObject.pPcxBuffer = buffer.ToArray();

        // Read in the palette
        Span<byte> paletteBuffer = new byte[768];
        fileManager.FileRead(hFileHandle, paletteBuffer, out var _);

        pCurrentPcxObject.ubPalette = paletteBuffer.ToArray();

        // Close file
        fileManager.FileClose(hFileHandle);
        return pCurrentPcxObject;
    }

    bool BlitPcxToBuffer(PcxObject pCurrentPcxObject, byte[] pBuffer, int usBufferWidth, int usBufferHeight, ushort usX, ushort usY, bool fTransp)
    {
        byte[] pPcxBuffer;
        byte ubRepCount;
        ushort usMaxX, usMaxY;
        uint uiImageSize;
        byte ubCurrentByte = 0;
        PCX ubMode;
        ushort usCurrentX, usCurrentY;
        uint uiOffset, uiIndex;
        uint uiNextLineOffset, uiStartOffset, uiCurrentOffset;

        pPcxBuffer = pCurrentPcxObject.pPcxBuffer;

        if (((pCurrentPcxObject.usWidth + usX) == usBufferWidth) && ((pCurrentPcxObject.usHeight + usY) == usBufferHeight))
        { // Pre-compute PCX blitting aspects.                                   
            uiImageSize = (uint)(usBufferWidth * usBufferHeight);
            ubMode = PCX.NORMAL;
            uiOffset = 0;
            ubRepCount = 0;

            // Blit Pcx object. Two main cases, one for transparency (0's are skipped and for without transparency.
            if (fTransp == true)
            {
                for (uiIndex = 0; uiIndex < uiImageSize; uiIndex++)
                {
                    if (ubMode == PCX.NORMAL)
                    {
                        ubCurrentByte = pPcxBuffer[uiOffset++];
                        if (ubCurrentByte > 0x0BF)
                        {
                            ubRepCount = (byte)(ubCurrentByte & 0x03F);
                            ubCurrentByte = pPcxBuffer[uiOffset++];
                            if (--ubRepCount > 0)
                            {
                                ubMode = PCX.RLE;
                            }
                        }
                    }
                    else
                    {
                        if (--ubRepCount == 0)
                        {
                            ubMode = PCX.NORMAL;
                        }
                    }
                    if (ubCurrentByte != 0)
                    {
                        pBuffer[uiIndex] = ubCurrentByte;
                    }
                }
            }
            else
            {
                for (uiIndex = 0; uiIndex < uiImageSize; uiIndex++)
                {
                    if (ubMode == PCX.NORMAL)
                    {
                        ubCurrentByte = pPcxBuffer[uiOffset++];
                        if (ubCurrentByte > 0x0BF)
                        {
                            ubRepCount = (byte)(ubCurrentByte & 0x03F);
                            ubCurrentByte = pPcxBuffer[uiOffset++];
                            if (--ubRepCount > 0)
                            {
                                ubMode = PCX.RLE;
                            }
                        }
                    }
                    else
                    {
                        if (--ubRepCount == 0)
                        {
                            ubMode = PCX.NORMAL;
                        }
                    }

                    pBuffer[uiIndex] = ubCurrentByte;
                }
            }
        }
        else
        {
            // Pre-compute PCX blitting aspects.
            if ((pCurrentPcxObject.usWidth + usX) >= usBufferWidth)
            {
                pCurrentPcxObject.usPcxFlags |= PCX.X_CLIPPING;
                usMaxX = (ushort)(usBufferWidth - 1);
            }
            else
            {
                usMaxX = (ushort)(pCurrentPcxObject.usWidth + usX);
            }

            if ((pCurrentPcxObject.usHeight + usY) >= usBufferHeight)
            {
                pCurrentPcxObject.usPcxFlags |= PCX.Y_CLIPPING;
                uiImageSize = (ushort)(pCurrentPcxObject.usWidth * (usBufferHeight - usY));
                usMaxY = (ushort)(usBufferHeight - 1);
            }
            else
            {
                uiImageSize = (ushort)(pCurrentPcxObject.usWidth * pCurrentPcxObject.usHeight);
                usMaxY = (ushort)(pCurrentPcxObject.usHeight + usY);
            }

            ubMode = PCX.NORMAL;
            uiOffset = 0;
            ubRepCount = 0;
            usCurrentX = usX;
            usCurrentY = usY;

            // Blit Pcx object. Two main cases, one for transparency (0's are skipped and for without transparency.
            if (fTransp == true)
            {
                for (uiIndex = 0; uiIndex < uiImageSize; uiIndex++)
                {
                    if (ubMode == PCX.NORMAL)
                    {
                        ubCurrentByte = pPcxBuffer[uiOffset++];
                        if (ubCurrentByte > 0x0BF)
                        {
                            ubRepCount = (byte)(ubCurrentByte & 0x03F);
                            ubCurrentByte = pPcxBuffer[uiOffset++];
                            if (--ubRepCount > 0)
                            {
                                ubMode = PCX.RLE;
                            }
                        }
                    }
                    else
                    {
                        if (--ubRepCount == 0)
                        {
                            ubMode = PCX.NORMAL;
                        }
                    }

                    if (ubCurrentByte != 0)
                    {
                        pBuffer[(usCurrentY * usBufferWidth) + usCurrentX] = ubCurrentByte;
                    }

                    usCurrentX++;
                    if (usCurrentX > usMaxX)
                    {
                        usCurrentX = usX;
                        usCurrentY++;
                    }
                }
            }
            else
            {
                uiStartOffset = (uint)(usCurrentY * usBufferWidth) + usCurrentX;
                uiNextLineOffset = (uint)(uiStartOffset + usBufferWidth);
                uiCurrentOffset = uiStartOffset;

                for (uiIndex = 0; uiIndex < uiImageSize; uiIndex++)
                {

                    if (ubMode == PCX.NORMAL)
                    {
                        ubCurrentByte = pPcxBuffer[uiOffset++];
                        if (ubCurrentByte > 0x0BF)
                        {
                            ubRepCount = (byte)(ubCurrentByte & 0x03F);
                            ubCurrentByte = pPcxBuffer[uiOffset++];
                            if (--ubRepCount > 0)
                            {
                                ubMode = PCX.RLE;
                            }
                        }
                    }
                    else
                    {
                        if (--ubRepCount == 0)
                        {
                            ubMode = PCX.NORMAL;
                        }
                    }

                    if (usCurrentX < usMaxX)
                    { // We are within the visible bounds so we write the byte to buffer
                        pBuffer[uiCurrentOffset] = ubCurrentByte;
                        uiCurrentOffset++;
                        usCurrentX++;
                    }
                    else
                    {
                        if ((uiCurrentOffset + 1) < uiNextLineOffset)
                        { // Increment the uiCurrentOffset
                            uiCurrentOffset++;
                        }
                        else
                        { // Go to next line
                            usCurrentX = usX;
                            usCurrentY++;
                            if (usCurrentY > usMaxY)
                            {
                                break;
                            }

                            uiStartOffset = (uint)(usCurrentY * usBufferWidth) + usCurrentX;
                            uiNextLineOffset = (uint)(uiStartOffset + usBufferWidth);
                            uiCurrentOffset = uiStartOffset;
                        }
                    }
                }
            }
        }
        return true;
    }

    private bool SetPcxPalette(PcxObject pCurrentPcxObject, HIMAGE hImage)
    {
        ushort Index;
        byte[] pubPalette;

        pubPalette = pCurrentPcxObject.ubPalette;

        // Allocate memory for palette
        hImage.pPalette = new(256);//MemAlloc(sizeof(SGPPaletteEntry) * 256);

        if (hImage.pPalette == null)
        {
            return false;
        }

        // Initialize the proper palette entries
        for (Index = 0; Index < 256; Index++)
        {
            hImage.pPalette.Add(new()
            {
                peRed = pubPalette[Index * 3],
                peGreen = pubPalette[(Index * 3) + 1],
                peBlue = pubPalette[(Index * 3) + 2],
                peFlags = 0,
            });
        }

        return true;
    }

    public List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage)
    {
        var img = this.CreateIndexedImages(ref hImage, hVObject);

        return hImage.ParsedImages;
    }

    public List<Image<Rgba32>> CreateIndexedImages(
        ref HIMAGE hImage,
        HVOBJECT hVObject)
    {
        var rgba32 = new Rgba32();
        Rgba32 color = default;

        //using var byteBuffer = configuration.MemoryAllocator.AllocateManagedByteBuffer(numOfPixels * image.PixelType.BitsPerPixel);
        ReadOnlySpan<byte> indexSpan = new(hVObject.pPixData);
        hImage.ParsedImages = new();

        var numOfPixels = hImage.usHeight * hImage.usWidth;
        var image = new Image<Rgba32>(hImage.usWidth, hImage.usHeight);
       // var imageSpan = indexSpan.Slice((int)hImage.uiDataOffset, (int)hImage.uiDataLength);

      //  var uncompressedData = imageSpan.ToArray();

        image.ProcessPixelRows(p =>
        {
            ReadOnlySpan<Rgba32> paletteSpan = new(hVObject.Palette);

            for (int y = 0; y < image.Height; y++)
            {
                Span<Rgba32> pixelRow = p.GetRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = IVideoManager.AlphaPixel;//paletteSpan[x];

                    // This is the alpha pixel after all the conversions...so replace with
                    // RGBA32 alpha pixel.
              //      if (uncompressedData[idx] == 0)
              //      {
              //          pixel = IVideoManager.AlphaPixel;
              //      }

                    color.FromRgba32(pixel);

                    pixelRow[x] = color;
                }
            }
        });

        hImage.ParsedImages.Add(image);

        return hImage.ParsedImages;
    }
}

public class PcxHeader
{
    public byte ubManufacturer;
    public byte ubVersion;
    public byte ubEncoding;
    public byte ubBitsPerPixel;
    public ushort usLeft;
    public ushort usTop;
    public ushort usRight;
    public ushort usBottom;
    public ushort usHorRez;
    public ushort usVerRez;
    public byte[] ubEgaPalette = new byte[48];
    public byte ubReserved;
    public byte ubColorPlanes;
    public ushort usBytesPerLine;
    public ushort usPaletteType;
    public byte[] ubFiller = new byte[58];
}

public class PcxObject
{
    public byte[] pPcxBuffer = Array.Empty<byte>();
    public byte[] ubPalette = new byte[768];
    public ushort usWidth;
    public ushort usHeight;
    public uint uiBufferSize;
    public PCX usPcxFlags;
}

[Flags]
public enum PCX : ushort
{
    NORMAL = 1,
    RLE = 2,
    COLOR256 = 4,
    TRANSPARENT = 8,
    CLIPPED = 16,
    REALIZEPALETTE = 32,
    X_CLIPPING = 64,
    Y_CLIPPING = 128,
    NOTLOADED = 256,
}

[Flags]
public enum PCX_ERRORS
{
    ERROROPENING = 1,
    INVALIDFORMAT = 2,
    INVALIDLEN = 4,
    OUTOFMEMORY = 8,
}
