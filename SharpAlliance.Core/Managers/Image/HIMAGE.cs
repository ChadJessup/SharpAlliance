using System;
using System.IO;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image
{
    // Image header structure
    public struct HIMAGE
    {
        public ushort gusAlphaMask;// = 0;
        public ushort gusRedMask;// = 0;
        public ushort gusGreenMask;// = 0;
        public ushort gusBlueMask;// = 0;
        public short gusRedShift;// = 0;
        public short gusBlueShift;// = 0;
        public short gusGreenShift;// = 0;

        public int usWidth;
        public int usHeight;
        public int ubBitDepth;
        public HIMAGECreateFlags fFlags;
        public string ImageFile;
        public IImageFileLoader iFileLoader;
        public SGPPaletteEntry[] pPalette;
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

        public Image<Rgba32>? ParsedImage { get; set; }
        public const ushort BLACK_SUBSTITUTE = 0x0001;

        public static async ValueTask<HIMAGE> CreateImage(string imageFilePath, HIMAGECreateFlags createFlags, IFileManager fileManager)
        {
            HIMAGE hImage;
            var ext = Path.GetExtension(imageFilePath);

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
            };

            hImage = await LoadImage(hImage, createFlags, fileManager);
            if (hImage.ParsedImage is null)
            {
                throw new InvalidProgramException($"{nameof(hImage.ParsedImage)} was null");
            }

            return hImage;
        }

        public Rgba32[] Create16BPPPalette(ref SGPPaletteEntry[] pPalette)
        {
            Rgba32[] palette;
            uint cnt;
            // ushort r16, g16, b16, usColor;
            // byte r, g, b;

            palette = new Rgba32[256];

            for (cnt = 0; cnt < 256; cnt++)
            {
                Bgr565 pixel = new Bgr565(
                    pPalette[cnt].peRed,
                    pPalette[cnt].peGreen,
                    pPalette[cnt].peBlue);

                // r = pPalette[cnt].peRed;
                // g = pPalette[cnt].peGreen;
                // b = pPalette[cnt].peBlue;

                //if (gusRedShift < 0)
                //{
                //    r16 = (ushort)(r >> Math.Abs(gusRedShift));
                //}
                //else
                //{
                //    r16 = (ushort)(r << gusRedShift);
                //}
                //
                //if (gusGreenShift < 0)
                //{
                //    g16 = (ushort)(g >> Math.Abs(gusGreenShift));
                //}
                //else
                //{
                //    g16 = (ushort)(g << gusGreenShift);
                //}
                //
                //if (gusBlueShift < 0)
                //{
                //    b16 = (ushort)(b >> Math.Abs(gusBlueShift));
                //}
                //else
                //{
                //    b16 = (ushort)(b << gusBlueShift);
                //}
                //
                //usColor = (ushort)((r16 & gusRedMask) | (g16 & gusGreenMask) | (b16 & gusBlueMask));
                //
                //if (usColor == 0)
                //{
                //    if ((r + g + b) != 0)
                //    {
                //        usColor = (ushort)(HIMAGE.BLACK_SUBSTITUTE | gusAlphaMask);
                //    }
                //}
                //else
                //{
                //    usColor |= gusAlphaMask;
                //}

                pixel.ToRgba32(ref palette[cnt]);
            }

            return palette;
        }

        private static async ValueTask<HIMAGE> LoadImage(HIMAGE hImage, HIMAGECreateFlags createFlags, IFileManager fileManager)
        {
            bool fReturnVal = await hImage.iFileLoader.LoadImage(ref hImage, createFlags, fileManager);

            // TODO: log
            if (!fReturnVal)
            {

            }

            hImage.ParsedImage.SaveAsPng($@"c:\assets\{Path.GetFileNameWithoutExtension(hImage.ImageFile)}.png");

            return hImage;
        }
    }
}
