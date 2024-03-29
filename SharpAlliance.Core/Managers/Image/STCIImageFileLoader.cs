﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image;

public class STCIImageFileLoader : ImageDecoder, IImageFormatDetector, IImageFileLoader
{
    public const int STCI_HEADER_SIZE = 64;
    public const string STCI_ID_STRING = "STCI";
    public const int STCI_ID_LEN = 4;
    public const int STCI_SUBIMAGE_SIZE = 16;
    public const int STCI_PALETTE_ELEMENT_SIZE = 3;
    public const int STCI_8BIT_PALETTE_SIZE = 768;

    public bool LoadImage(ref HIMAGE hImage, HIMAGECreateFlags flags, IFileManager fileManager)
    {
        if (!fileManager.FileExists(hImage.ImageFile))
        {
            return false;
        }

        using var stream = fileManager.FileOpen(hImage.ImageFile, FileAccess.Read, fDeleteOnClose: false);
        var config = SixLabors.ImageSharp.Configuration.Default;
        config.Properties.Clear();

        config.Properties.Add(stream, hImage);
        config.Properties.Add(typeof(IFileManager), fileManager);
        config.Properties.Add(typeof(HIMAGECreateFlags), flags);
        config.ImageFormatsManager.AddImageFormat(new HIMAGE());

        config.ImageFormatsManager.SetDecoder(HIMAGE.Instance, STCIImageFileLoader.Instance);
        config.ImageFormatsManager.AddImageFormatDetector(STCIImageFileLoader.Instance);

        var dopt = new DecoderOptions()
        {
            Configuration = config,
        };

        var i = Image<Rgba32>.Load(dopt, stream);

        // parsing the image modifies the hImage, so reassign back.
        hImage = (HIMAGE)config.Properties[stream];
        hImage.ParsedImages.Add((Image<Rgba32>)i);
        hImage.usWidth = i.Width;
        hImage.usHeight = i.Height;

        return true;
    }

    protected override SixLabors.ImageSharp.Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
        => this.Decode<Rgba32>(options, stream, cancellationToken);

    protected override Image<TPixel> Decode<TPixel>(DecoderOptions configuration, Stream stream, CancellationToken token)
    {
        Span<byte> buffer = stackalloc byte[Marshal.SizeOf<STCIHeader>()];
        stream.Read(buffer);

        var header = MemoryMarshal.Read<STCIHeader>(buffer);
        Image<TPixel> image;

        if (header.fFlags.HasFlag(STCITypes.STCI_RGB))
        {
            image = this.DecodeRgba<TPixel>(header, configuration, stream);
        }
        else if (header.fFlags.HasFlag(STCITypes.STCI_INDEXED))
        {
            image = this.DecodeIndexed<TPixel>(header, configuration, stream);
        }
        else if (header.fFlags.HasFlag(STCITypes.STCI_ZLIB_COMPRESSED))
        {
            image = new Image<TPixel>(1, 1);// this.DecodeETRLECompressed<TPixel>(header, configuration, stream);
        }
        else
        {
            image = new Image<TPixel>(1, 1);
        }

        return image;
    }

    private Image<TPixel>? DecodeIndexed<TPixel>(STCIHeader pHeader, DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    {
        IFileManager fileManager = (IFileManager)options.Configuration.Properties[typeof(IFileManager)];
        HIMAGE hImage = (HIMAGE)options.Configuration.Properties[stream];
        HIMAGECreateFlags fContents = (HIMAGECreateFlags)options.Configuration.Properties[typeof(HIMAGECreateFlags)];

        Image<TPixel> image = new(options.Configuration, pHeader.usWidth, pHeader.usHeight);

        int uiFileSectionSize;
        int uiBytesRead;
        byte[]? pSTCIPalette = null;

        if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_PALETTE))
        {
            // Allocate memory for reading in the palette
            if (pHeader.Indexed.uiNumberOfColours != 256)
            {
                //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Palettized image has bad palette size.");
                return null;
            }

            uiFileSectionSize = (int)pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
            pSTCIPalette = new byte[uiFileSectionSize];

            // ATE: Memset: Jan 16/99
            //memset(pSTCIPalette, 0, uiFileSectionSize);

            // Read in the palette
            if (!fileManager.FileRead(stream, ref pSTCIPalette, (int)uiFileSectionSize, out uiBytesRead) || uiBytesRead != uiFileSectionSize)
            {
                //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem loading palette!");
                //FileClose(hFile);
                //MemFree(pSTCIPalette);
                return null;
            }
            else if (!this.STCISetPalette(ref pSTCIPalette, ref hImage))
            {
                // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem setting hImage-format palette!");
                // FileClose(hFile);
                // MemFree(pSTCIPalette);
                return null;
            }

            hImage.fFlags |= HIMAGECreateFlags.IMAGE_PALETTE;
        }
        else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA | HIMAGECreateFlags.IMAGE_APPDATA))
        {
            // seek past the palette
            uiFileSectionSize = (int)pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
            if (fileManager.FileSeek(stream, uiFileSectionSize, SeekOrigin.Current) == false)
            {
                // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past palette!");
                // FileClose(hFile);
                return null;
            }
        }

        if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA))
        {
            if (pHeader.fFlags.HasFlag(STCITypes.STCI_ETRLE_COMPRESSED))
            {
                // load data for the subimage (object) structures
                hImage.usNumberOfObjects = pHeader.Indexed.usNumberOfSubImages;
                uiFileSectionSize = hImage.usNumberOfObjects * STCI_SUBIMAGE_SIZE;

                if (!fileManager.FileRead(stream, ref hImage.pETRLEObject, (int)uiFileSectionSize, out uiBytesRead)
                    || uiBytesRead != uiFileSectionSize)
                {
                    return null;
                }

                hImage.uiSizePixData = pHeader.uiStoredSize;
                hImage.fFlags |= HIMAGECreateFlags.IMAGE_TRLECOMPRESSED;
            }

            hImage.pImageData = new byte[pHeader.uiStoredSize];
            if (!fileManager.FileRead(stream, ref hImage.pImageData, (int)pHeader.uiStoredSize, out uiBytesRead) || uiBytesRead != pHeader.uiStoredSize)
            {
                return null;
            }

            hImage.fFlags |= HIMAGECreateFlags.IMAGE_BITMAPDATA;
        }
        else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_APPDATA)) // then there's a point in seeking ahead
        {
            if (fileManager.FileSeek(stream, (int)pHeader.uiStoredSize, SeekOrigin.Current) == false)
            {
                // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past image data!");
                // FileClose(hFile);
                return null;
            }
        }

        if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_APPDATA) && pHeader.uiAppDataSize > 0)
        {
            // load application-specific data
            hImage.pAppData = new byte[pHeader.uiAppDataSize];
            if (!fileManager.FileRead(stream, ref hImage.pAppData, (int)pHeader.uiAppDataSize, out uiBytesRead) || uiBytesRead != pHeader.uiAppDataSize)
            {
            
            }

            hImage.uiAppDataSize = pHeader.uiAppDataSize;

            hImage.fFlags |= HIMAGECreateFlags.IMAGE_APPDATA;
        }
        else
        {
            hImage.pAppData = null;
            hImage.uiAppDataSize = 0;
        }

        hImage.ubBitDepth = pHeader.ubDepth;
        options.Configuration.Properties[stream] = hImage;

        return image;
    }

    public List<Image<Rgba32>> CreateIndexedImages(
        ref HIMAGE hImage,
        HVOBJECT hVObject)
    {
        var rgba32 = new Rgba32();
        Rgba32 color = default;

        //using var byteBuffer = configuration.MemoryAllocator.AllocateManagedByteBuffer(numOfPixels * image.PixelType.BitsPerPixel);
        ReadOnlySpan<byte> indexSpan = new(hVObject.pPixData);
        hImage.ParsedImages = new(hImage.pETRLEObject.Length);

        foreach (var etrle in hImage.pETRLEObject)
        {
            int idx = 0;
            var numOfPixels = etrle.usHeight * etrle.usWidth;
            var image = new Image<Rgba32>(etrle.usWidth, etrle.usHeight);
            var imageSpan = indexSpan.Slice((int)etrle.uiDataOffset, (int)etrle.uiDataLength);

            var uncompressedData = this.DecompressETRLEBytes(imageSpan.ToArray());

            image.ProcessPixelRows(p =>
            {
                ReadOnlySpan<Rgba32> paletteSpan = new(hVObject.Palette);

                for (int y = 0; y < image.Height; y++)
                {
                    Span<Rgba32> pixelRow = p.GetRowSpan(y);
                    for (int x = 0; x < image.Width && idx < uncompressedData.Length; x++)
                    {
                        var pixel = paletteSpan[uncompressedData[idx]];

                        // This is the alpha pixel after all the conversions...so replace with
                        // RGBA32 alpha pixel.
                        if (uncompressedData[idx] == 0)
                        {
                            pixel = IVideoManager.AlphaPixel;
                        }

                        color.FromRgba32(pixel);

                        pixelRow[x] = color;
                        idx++;
                    }
                }
            });

            hImage.ParsedImages.Add(image);
        }

        return hImage.ParsedImages;
    }

    public const byte COMPRESSED_FLAG = 0x80;
    public const int MAX_COMPR_BYTES = 127;
    public const byte ALPHA_VALUE = 0;
    public const byte IS_COMPRESSED_BYTE_MASK = 0x80;
    public const byte NUMBER_OF_BYTES_MASK = 0x7F;

    public byte[] DecompressETRLEBytes(byte[] data)
    {
        var number_of_compressed_bytes = data.Length;
        var compressed_bytes = data;
        using MemoryStream extracted_buffer = new(number_of_compressed_bytes);
        var bytes_til_next_control_byte = 0;

        foreach (var current_byte in compressed_bytes)
        {
            if (bytes_til_next_control_byte == 0)
            {
                var is_compressed_alpha_byte = ((current_byte & IS_COMPRESSED_BYTE_MASK) >> 7) == 1;
                var length_of_subsequence = current_byte & NUMBER_OF_BYTES_MASK;
                if (is_compressed_alpha_byte)
                {
                    foreach (var s in Enumerable.Range(0, length_of_subsequence))
                    {
                        extracted_buffer.WriteByte(ALPHA_VALUE);
                    }
                }
                else
                {
                    bytes_til_next_control_byte = length_of_subsequence;
                }
            }
            else
            {
                extracted_buffer.WriteByte(current_byte);
                bytes_til_next_control_byte -= 1;
            }

            if (bytes_til_next_control_byte != 0)
            {
                //raise EtrleException('Not enough data to decompress')
            }
        }

        return extracted_buffer.ToArray();
    }

    private bool STCISetPalette(ref byte[] pSTCIPalette, ref HIMAGE hImage)
    {
        ushort usIndex;
        var paletteSpan = new ReadOnlySpan<byte>(pSTCIPalette);

        var pubPaletteIdx = 0;
        var pubPalette = MemoryMarshal.Cast<byte, STCIPaletteElement>(paletteSpan)
            .ToArray();
        //pubPalette = MemoryMarshal.Read<STCIPaletteElement>(paletteSpan);

        // Allocate memory for palette
        hImage.pPalette = new();

        // Initialize the proper palette entries
        for (usIndex = 0; usIndex < 256; usIndex++)
        {
            var paletteEntry = new SGPPaletteEntry
            {
                peRed = pubPalette[pubPaletteIdx].ubRed,
                peGreen = pubPalette[pubPaletteIdx].ubGreen,
                peBlue = pubPalette[pubPaletteIdx].ubBlue,
                peFlags = 0,
            };

            hImage.pPalette.Add(paletteEntry);
            pubPaletteIdx++;
        }

        return true;
    }

    private Image<TPixel> DecodeRgba<TPixel>(STCIHeader header, DecoderOptions configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    {
        var rgba32 = new Rgba32();
        TPixel color = default;

        var numOfPixels = header.usHeight * header.usWidth;

        using var byteBuffer = configuration.Configuration.MemoryAllocator.Allocate<byte>(numOfPixels * header.ubDepth);
        stream.Read(byteBuffer.Memory.Span);

        var image = new Image<TPixel>(configuration.Configuration, header.usWidth, header.usHeight);

        int idx = 0;

        image.ProcessPixelRows(p =>
        {
            Span<ushort> pixelSpan = MemoryMarshal.Cast<byte, ushort>(byteBuffer.Memory.Span);

            for (int y = 0; y < header.usHeight; y++)
            {

                Span<TPixel> pixelRow = p.GetRowSpan(y);
                for (int x = 0; x < header.usWidth; x++)
                {
                    var bgr565 = new Bgr565
                    {
                        PackedValue = pixelSpan[idx],
                    };

                    bgr565.ToRgba32(ref rgba32);
                    color.FromRgba32(rgba32);

                    pixelRow[x] = color;
                    idx++;
                }
            }
        });

        return image;
    }

    public Task<Image<TPixel>> DecodeAsync<TPixel>(DecoderOptions configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    {
        throw new NotImplementedException();
    }

    public async Task<SixLabors.ImageSharp.Image> DecodeAsync(DecoderOptions configuration, Stream stream, CancellationToken cancellationToken)
        => await this.DecodeAsync<Rgba32>(configuration, stream, cancellationToken);

    public List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage)
    {
        var img = this.CreateIndexedImages(ref hImage, hVObject);

        return hImage.ParsedImages;
    }

    public static STCIImageFileLoader Instance { get; } = new();

    public int HeaderSize { get; } = STCI_HEADER_SIZE;

    public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
    {
        if (header.Length >= STCI_HEADER_SIZE)
        {
            format = HIMAGE.Instance;
            var id = Encoding.ASCII.GetString(header.Slice(0, STCI_ID_LEN));

            return id.Equals(STCI_ID_STRING);
        }

        format = null;
        return false;
    }

    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
