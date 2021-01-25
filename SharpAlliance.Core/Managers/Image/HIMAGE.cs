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
        public SGPPaletteEntry pPalette;
        public int pui16BPPPalette;
        public int pAppData;
        public int uiAppDataSize;
        // This union is used to describe each data type and is flexible to include the
        // data strucutre of the compresssed format, once developed.
        public byte[] pImageData;
        public byte[] pCompressedImageData;
        public int p8BPPData;
        public byte[] p16BPPData;
        public int pPixData8;
        public int uiSizePixData;
        public ETRLEObject pETRLEObject;
        public int usNumberOfObjects;

        public Image<Rgba32>? ParsedImage { get; set; }

        public static async ValueTask<HIMAGE?> CreateImage(string imageFilePath, HIMAGECreateFlags createFlags, IFileManager fileManager)
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
                return null;
            }

            return hImage;
        }

        private static async ValueTask<HIMAGE> LoadImage(HIMAGE hImage, HIMAGECreateFlags createFlags, IFileManager fileManager)
        {
            bool fReturnVal = await hImage.iFileLoader.LoadImage(ref hImage, createFlags, fileManager);

            // TODO: log

            return hImage;
        }
    }
}
