using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers;

public class TextureManager : ITextureManager
{
    private readonly Dictionary<string, HVOBJECT> loadedTextures = new();
    private readonly ILogger<TextureManager> logger;
    private readonly ILibraryManager libraries;
    private readonly IFileManager files;
    private readonly Shading shading = new();

    public TextureManager(
        ILogger<TextureManager> logger,
        ILibraryManager libraryManager,
        IFileManager files,
        Shading shading)
    {
        this.shading = shading;
        this.logger = logger;
        this.libraries = libraryManager;
        this.files = files;
    }

    public ValueTask<bool> Initialize()
    {
        throw new NotImplementedException();
    }

    public HVOBJECT LoadTexture(string assetPath)
    {
        if (this.loadedTextures.TryGetValue(assetPath, out var vObject))
        {
            return vObject;
        }

        HVOBJECT hVObject = this.CreateVideoObject(assetPath);

        this.loadedTextures.Add(assetPath, hVObject);

        return hVObject;
    }

    private HVOBJECT CreateVideoObject(string assetPath)
    {
        HVOBJECT hVObject = new();
        hVObject.Name = assetPath;

        HIMAGE hImage;
        ETRLEData TempETRLEData = new();

        // Create himage object from file
        hImage = HIMAGE.CreateImage(assetPath, HIMAGECreateFlags.IMAGE_ALLIMAGEDATA, this.files);

        // Get TRLE data
        this.GetETRLEImageData(hImage, ref TempETRLEData);

        // Set values
        hVObject.usNumberOfObjects = TempETRLEData.usNumberOfObjects;
        hVObject.pETRLEObject = TempETRLEData.pETRLEObject;
        hVObject.pPixData = TempETRLEData.pPixData;
        hVObject.uiSizePixData = TempETRLEData.uiSizePixData;

        // Set palette from himage
        if (hImage.ubBitDepth == 8)
        {
            hVObject.pShade8 = this.shading.ubColorTables[Shading.DEFAULT_SHADE_LEVEL, 0];
            hVObject.pGlow8 = this.shading.ubColorTables[0, 0];

            this.SetVideoObjectPalette(hVObject, hImage, hImage.pPalette);
        }

        // Set values from himage
        hVObject.ubBitDepth = hImage.ubBitDepth;

        // All is well
        //  DbgMessage( TOPIC_VIDEOOBJECT, DBG_LEVEL_3, String("Success in Creating Video Object" ) );

        hVObject.hImage = hImage;
        hVObject.Images = new Image<Rgba32>[hImage.ParsedImages.Count];

        for (int i = 0; i < hImage.ParsedImages.Count; i++)
        {
            hVObject.Images[i] = hImage.ParsedImages[i];
            //new ImageSharpTexture(hImage.ParsedImages[i], mipmap: false)
            //.CreateDeviceTexture(GraphicDevice, GraphicDevice.ResourceFactory);

            //            hVObject.Textures[i].Name = $"{hImage.ImageFile}_{i}";
        }

        return hVObject;
    }

    private bool SetVideoObjectPalette(HVOBJECT hVObject, HIMAGE hImage, List<SGPPaletteEntry> pSrcPalette)
    {
        // Create palette object if not already done so
        hVObject.pPaletteEntry = pSrcPalette;

        // Create 16BPP Palette
        hVObject.Palette = hImage.Create16BPPPalette(pSrcPalette);
        hVObject.ShadeCurrentPixels = hVObject.Palette;

        if (hImage.fFlags.HasFlag(HIMAGECreateFlags.IMAGE_PALETTE))
        {
            hImage.ParsedImages = hImage.iFileLoader.ApplyPalette(ref hVObject, ref hImage);
        }

        // If you want to output all the images to disk, uncomment ..makes startup take a lot longer.
        // for (int i = 0; i < (hImage.ParsedImages?.Count ?? 0); i++)
        // {
        //     var fileName = Path.GetFileNameWithoutExtension(hImage.ImageFile) + $"_{i}.png";
        //     var directory = Path.Combine("C:\\", "assets", Path.GetDirectoryName(hImage.ImageFile)!);
        //     Directory.CreateDirectory(directory);
        //     hImage.ParsedImages![i].SaveAsPng(Path.Combine(directory, fileName));
        // }

        //  DbgMessage(TOPIC_VIDEOOBJECT, DBG_LEVEL_3, String("Video Object Palette change successfull" ));
        return true;
    }

    private bool GetETRLEImageData(HIMAGE? hImage, ref ETRLEData pBuffer)
    {
        if (hImage is null)
        {
            return false;
        }

        // Create memory for data
        pBuffer.usNumberOfObjects = hImage.usNumberOfObjects;

        // Create buffer for objects
        pBuffer.pETRLEObject = new ETRLEObject[pBuffer.usNumberOfObjects];
        //CHECKF(pBuffer.pETRLEObject != null);

        // Copy into buffer
        pBuffer.pETRLEObject = hImage.pETRLEObject;

        // Allocate memory for pixel data
        pBuffer.pPixData = new byte[hImage.uiSizePixData];
        //CHECKF(pBuffer.pPixData != null);

        pBuffer.uiSizePixData = hImage.uiSizePixData;

        // Copy into buffer
        pBuffer.pPixData = hImage.pImageData;

        return true;
    }

    public void Dispose()
    {
    }

    public bool TryGetTexture(string key, out HVOBJECT hPixHandle)
        => this.loadedTextures.TryGetValue(key, out hPixHandle);

    HVOBJECT ITextureManager.LoadImage(string assetPath, bool debug = false)
    {
        if (this.loadedTextures.TryGetValue(assetPath, out var existingObj))
        {
            return existingObj;
        }

        HVOBJECT hVObject = new()
        {
            Name = assetPath
        };

        HIMAGE hImage;
        ETRLEData TempETRLEData = new();

        // Create himage object from file
        hImage = HIMAGE.CreateImage(assetPath, HIMAGECreateFlags.IMAGE_ALLIMAGEDATA, this.files);

        // Get TRLE data
        this.GetETRLEImageData(hImage, ref TempETRLEData);

        // Set values
        hVObject.usNumberOfObjects = TempETRLEData.usNumberOfObjects;
        hVObject.pETRLEObject = TempETRLEData.pETRLEObject;
        hVObject.pPixData = TempETRLEData.pPixData;
        hVObject.uiSizePixData = TempETRLEData.uiSizePixData;

        // Set palette from himage
        if (hImage.ubBitDepth == 8)
        {
            hVObject.pShade8 = this.shading.ubColorTables[Shading.DEFAULT_SHADE_LEVEL, 0];
            hVObject.pGlow8 = this.shading.ubColorTables[0, 0];

            this.SetVideoObjectPalette(hVObject, hImage, hImage.pPalette);
        }

        // Set values from himage
        hVObject.ubBitDepth = hImage.ubBitDepth;

        // All is well
        //  DbgMessage( TOPIC_VIDEOOBJECT, DBG_LEVEL_3, String("Success in Creating Video Object" ) );

        hVObject.hImage = hImage;
        hVObject.Images = new Image<Rgba32>[hImage.ParsedImages.Count];

        for (int i = 0; i < hImage.ParsedImages.Count; i++)
        {
            hVObject.Images[i] = hImage.ParsedImages[i];
        }

        if (debug)
        {
            for (int i = 0; i < hVObject.Images.Length; i++)
            {
                Directory.CreateDirectory($@"C:\temp\{assetPath}");
                hVObject.Images[i].SaveAsPng($@"C:\temp\{assetPath}\{i}.png");
            }
        }

        this.loadedTextures.Add(assetPath, hVObject);

        return hVObject;
    }
}
