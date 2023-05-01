using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.Image;

// Image header structure
public class HIMAGE : IImageFormat
{
    public int usWidth;
    public int usHeight;
    public int ubBitDepth;
    public HIMAGECreateFlags fFlags;
    public string ImageFile;
    public IImageFileLoader iFileLoader;
    public List<SGPPaletteEntry> pPalette = new();
    public uint pui16BPPPalette;
    public byte[]? pAppData;
    public uint uiAppDataSize;
    // This union is used to describe each data type and is flexible to include the
    // data strucutre of the compresssed format, once developed.
    public byte[] pImageData;
    public byte[] pCompressedImageData;
    public int p8BPPData;
    public byte[] p16BPPData;
    public byte[] pPixData8;
    public uint uiSizePixData;
    public ETRLEObject[] pETRLEObject;
    public ushort usNumberOfObjects;

    public static HIMAGE Instance { get; } = new();

    public List<Image<Rgba32>> ParsedImages { get; set; }
    public string Name { get; } = "STI";
    public string DefaultMimeType { get; }
    public IEnumerable<string> MimeTypes { get; } = new List<string>() { "BLAH" };
    public IEnumerable<string> FileExtensions { get; } = new List<string> { "sti" };

    public const ushort BLACK_SUBSTITUTE = 0x0001;

    public static HIMAGE CreateImage(string imageFilePath, HIMAGECreateFlags createFlags, IFileManager fileManager)
    {
        HIMAGE hImage;
        var ext = System.IO.Path.GetExtension(imageFilePath);

        IImageFileLoader iFileLoader = ext.ToUpper() switch
        {
            ".STI" => new STCIImageFileLoader(),
            ".TGA" => new TGAImageFileLoader(),
            ".PCX" => new PCXImageFileLoader(),
            _ => new PCXImageFileLoader(),
        };

        if (!fileManager.FileExists(imageFilePath))
        {
            throw new FileNotFoundException($"Unable to find asset, on disk or in library: {imageFilePath}");
        }

        hImage = new HIMAGE
        {
            iFileLoader = iFileLoader,
            ImageFile = imageFilePath,
            ParsedImages = new(),
        };

        hImage = LoadImage(hImage, createFlags, fileManager);
        if (hImage.ParsedImages is null)
        {
            throw new InvalidProgramException($"{nameof(hImage.ParsedImages)} was null");
        }

        return hImage;
    }

    public Rgba32[] Create16BPPPalette(List<SGPPaletteEntry> pPalette)
    {
        Rgba32[] palette;
        int cnt;
        ushort r16, g16, b16, usColor;
        byte r, g, b; // byte

        gusRedShift = 8;//-8;
        gusGreenShift = 3;//8;
        gusBlueShift = -3;//0;
        gusRedMask = 63488;//0;
        gusGreenMask = 2016;//65280;
        gusBlueMask = 31;//255;

        palette = new Rgba32[256];

        for (cnt = 0; cnt < 256; cnt++)
        {
            r = pPalette[cnt].peRed;
            g = pPalette[cnt].peGreen;
            b = pPalette[cnt].peBlue;

            var p = new Bgr24(r, g, b);
            p.ToRgba32(ref palette[cnt]);

            if (gusRedShift < 0)
            {
                r16 = (ushort)(r >> Math.Abs(gusRedShift));
            }
            else
            {
                r16 = (ushort)(r << gusRedShift);
            }

            if (gusGreenShift < 0)
            {
                g16 = (ushort)(g >> Math.Abs(gusGreenShift));
            }
            else
            {
                g16 = (ushort)(g << gusGreenShift);
            }

            if (gusBlueShift < 0)
            {
                b16 = (ushort)(b >> Math.Abs(gusBlueShift));
            }
            else
            {
                b16 = (ushort)(b << gusBlueShift);
            }

            usColor = (ushort)((r16 & gusRedMask) | (g16 & gusGreenMask) | (b16 & gusBlueMask));

            if (usColor == 0)
            {
                if ((r + g + b) != 0)
                {
                    usColor = (ushort)(HIMAGE.BLACK_SUBSTITUTE | gusAlphaMask);
                }
            }
            else
            {
                usColor |= gusAlphaMask;
            }


            Bgr565 pixel = new()
            {
                PackedValue = usColor,
            };

            pixel.ToRgba32(ref palette[cnt]);
        }

        return palette;
    }

    private static HIMAGE LoadImage(HIMAGE hImage, HIMAGECreateFlags createFlags, IFileManager fileManager)
    {
        bool fReturnVal = hImage.iFileLoader.LoadImage(ref hImage, createFlags, fileManager);

        // TODO: log
        if (!fReturnVal)
        {

        }

        return hImage;
    }

    public override string ToString() => this.ImageFile;
}
