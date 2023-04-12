using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class FontManager
{
    public int usDefaultPixelDepth { get; set; }
    public FontTranslationTable FontTranslationTable { get; set; } = new FontTranslationTable();
}

public class FontTranslationTable
{
    public int usNumberOfSymbols { get; set; }
    public int[] DynamicArrayOf16BitValues { get; set; }
}

public class FontSubSystem : ISharpAllianceManager
{
    private static FontManager FontManager { get; } = new FontManager();

    private const int PALETTE_SIZE = 768;
    private const int STRING_DELIMITER = 0;
    private const int ID_BLACK = 0;
    private const int MAX_FONTS = 25;
    public const int INVALIDATE_TEXT = 0x00000010;
    public Rectangle FontDestRegion = new(0, 0, 640, 480);

    // Wont display the text.  Used if you just want to get how many lines will be displayed
    public const int DONT_DISPLAY_TEXT = 0x00000020;
    private static FontStyle gpLargeFontType1;
    private static HVOBJECT gvoLargeFontType1;
    private static FontStyle FontDefault;
    //private int FontDestBuffer = BACKBUFFER;
    private static int FontDestPitch = 640 * 2;
    private static int FontDestBPP = 16;
    private static IVideoManager video;
    private static FontColor FontForeground16 = 0;
    private static FontColor FontBackground16 = 0;
    private static FontShadow FontShadow16 = FontShadow.DEFAULT_SHADOW;
    private static FontColor FontForeground8 = 0;
    private static FontColor FontBackground8 = 0;

    private static Dictionary<FontStyle, HVOBJECT> FontObjs = new();

    private static FontStyle gpSmallFontType1;
    private static HVOBJECT gvoSmallFontType1;
    private static FontStyle gpTinyFontType1;
    private static HVOBJECT gvoTinyFontType1;
    private static FontStyle gp12PointFont1;
    private static HVOBJECT gvo12PointFont1;
    private static FontStyle gpClockFont;
    private static HVOBJECT gvoClockFont;
    private static FontStyle gpCompFont;
    private static HVOBJECT gvoCompFont;
    private static FontStyle gpSmallCompFont;
    private static HVOBJECT gvoSmallCompFont;
    private static FontStyle gp10PointRoman;
    private static HVOBJECT gvo10PointRoman;
    private static FontStyle gp12PointRoman;
    private static HVOBJECT gvo12PointRoman;
    private static FontStyle gp14PointSansSerif;
    private static HVOBJECT gvo14PointSansSerif;
    private static FontStyle gp10PointArial;
    private static HVOBJECT gvo10PointArial;
    private static FontStyle gp14PointArial;
    private static HVOBJECT gvo14PointArial;
    private static FontStyle gp10PointArialBold;
    private static HVOBJECT gvo10PointArialBold;
    private static FontStyle gp12PointArial;
    private static HVOBJECT gvo12PointArial;
    private static FontStyle gpBlockyFont;
    private static HVOBJECT gvoBlockyFont;
    private static FontStyle gpBlockyFont2;
    private static HVOBJECT gvoBlockyFont2;
    private static FontStyle gp12PointArialFixedFont;
    private static HVOBJECT gvo12PointArialFixedFont;
    private static FontStyle gp16PointArial;
    private static HVOBJECT gvo16PointArial;
    private static FontStyle gpBlockFontNarrow;
    private static HVOBJECT gvoBlockFontNarrow;
    private static FontStyle gp14PointHumanist;
    private static HVOBJECT gvo14PointHumanist;
    private static FontStyle gpHugeFont;
    private static HVOBJECT gvoHugeFont;

    private static Dictionary<FontStyle, Font> fontLookup = new();
    private static Dictionary<FontColor, Rgba32> fontColorLookup = new();

    public bool IsInitialized { get; }
    public bool FontDestWrap { get; private set; }
    public TextJustifies IAN_WRAP_NO_SHADOW { get; } = (TextJustifies)32;

    public static TextRenderer TextRenderer { get; private set; }

    public FontSubSystem(IVideoManager videoManager) => video = videoManager;

    public static void SetFont(FontStyle fontStyle)
    {
        FontDefault = fontStyle;
    }

    public static void FindFontRightCoordinates(int sLeft, int sTop, int sWidth, int sHeight, string pStr, FontStyle iFontIndex, out int psNewX, out int psNewY)
    {
        int xp, yp;

        // Compute the coordinates to right justify the text
        xp = ((sWidth - StringPixLength(pStr, iFontIndex))) + sLeft;
        yp = ((sHeight - GetFontHeight(iFontIndex)) / 2) + sTop;

        psNewX = xp;
        psNewY = yp;
    }

    public static void SetFontBackground(FontColor fontColor)
    {
    }

    public static void SetFontForeground(FontColor ubForeground)
    {
        int uiRed, uiGreen, uiBlue;

        if ((FontDefault < 0) || (((int)FontDefault) > MAX_FONTS))
        {
            return;
        }

        FontForeground8 = ubForeground;

        uiRed = (int)FontObjs[FontDefault].pPaletteEntry[(int)ubForeground].peRed;
        uiGreen = (int)FontObjs[FontDefault].pPaletteEntry[(int)ubForeground].peGreen;
        uiBlue = (int)FontObjs[FontDefault].pPaletteEntry[(int)ubForeground].peBlue;

        FontForeground16 = FontColor.FONT_MCOLOR_LTBLUE; // Get16BPPColor(FROMRGB(uiRed, uiGreen, uiBlue));
    }

    public void SaveFontSettings()
    {
    }

    public void RestoreFontSettings()
    {
    }

    public static int GetFontHeight(FontStyle usFont)
    {
        if (!FontObjs.TryGetValue(usFont, out var hVobject))
        {
            FontSubSystem.InitializeFonts();
        }

        return GetHeight(FontObjs[usFont], 0);
    }

    private static int GetHeight(HVOBJECT hSrcVObject, int ssIndex)
    {
        ETRLEObject pTrav = new();

        // Get Offsets from Index into structure
        pTrav = hSrcVObject.pETRLEObject[ssIndex];
        return pTrav.usHeight + pTrav.sOffsetY;
    }

    public static void SetFontDestBuffer(Surfaces buttonDestBuffer, int y1, int y2, int width, int height, bool v)
    {
    }

    public static int StringPixLength(string stringText, FontStyle UseFont)
    {
        if (string.IsNullOrWhiteSpace(stringText))
        {
            return 0;
        }

        var Cur = 0;
        var curletter = stringText.AsSpan();
        char transletter;

        var idx = 0;
        while (idx < curletter.Length)
        {
            transletter = GetIndex(curletter[idx++]);
            Cur += GetWidth(FontObjs[UseFont], transletter);
        }

        return Cur;
    }

    //*****************************************************************************
    // GetIndex
    //
    //		Given a word-sized character, this function returns the index of the
    //	cell in the font to print to the screen. The conversion table is built by
    //	CreateEnglishTransTable()
    //
    //*****************************************************************************
    private static char GetIndex(char siChar)
    {
        char ssCount = (char)0;
        int usNumberOfSymbols = FontManager.FontTranslationTable.usNumberOfSymbols;

        // search the Translation Table and return the index for the font
        int idx = 0;
        int pTrav = FontManager.FontTranslationTable.DynamicArrayOf16BitValues[idx];

        while (ssCount < usNumberOfSymbols)
        {
            if (siChar == pTrav)
            {
                return ssCount;
            }
            ssCount++;
            pTrav = FontManager.FontTranslationTable.DynamicArrayOf16BitValues[++idx];
        }

        // If here, present warning and give the first index
        // DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Error: Invalid character given %d", siChar));

        // Return 0 here, NOT -1 - we should see A's here now...
        return (char)0;
    }

    //*****************************************************************************
    // GetWidth
    //
    //	Returns the width of a given character in the font.
    //
    //*****************************************************************************
    private static int GetWidth(HVOBJECT hSrcVObject, char ssIndex)
    {
        ETRLEObject pTrav;

        // Assertions
        //Assert(hSrcVObject != null);

        if (ssIndex < 0 || ssIndex > 92)
        {
            int i = 0;
        }

        // Get Offsets from Index into structure
        pTrav = hSrcVObject.pETRLEObject[ssIndex];
        return pTrav.usWidth + pTrav.sOffsetX;
    }

    public static void SetFontShadow(FontShadow sShadowColor)
    {
    }

    public static int DisplayWrappedString(
        Point pos,
        int sWrappedWidth,
        int gap,
        FontStyle usFont,
        FontColor sForeColor,
        string stringText,
        FontColor sBackgroundColor,
        TextJustifies bJustified)
    {
        DrawTextToScreen(
            stringText,
            pos,
            sWrappedWidth,
            usFont,
            sForeColor,
            sBackgroundColor,
            bJustified);

        return 0;
    }

    public static void DrawTextToScreen(
        string text,
        Point pos,
        int width,
        FontStyle font,
        FontColor foregroundColor,
        FontColor backgroundColor,
        TextJustifies justification)
        => DrawTextToScreen(
            text,
            pos.X,
            pos.Y,
            width,
            font,
            foregroundColor,
            backgroundColor,
            justification);

    public static void DrawTextToScreen(
        string text,
        int x,
        int y,
        int width,
        FontStyle font,
        FontColor foregroundColor,
        FontColor backgroundColor,
        TextJustifies justification)
    {
        if (justification.HasFlag(TextJustifies.DONT_DISPLAY_TEXT))
        {
            return;
        }

        var alignment = justification switch
        {
            TextJustifies.CENTER_JUSTIFIED => HorizontalAlignment.Center,
            TextJustifies.RIGHT_JUSTIFIED => HorizontalAlignment.Right,
            TextJustifies.LEFT_JUSTIFIED => HorizontalAlignment.Left,
            _ => HorizontalAlignment.Left,
        };

        try
        {
            FontSubSystem.TextRenderer.DrawText(
                text,
                x,
                y,
                width,
                alignment,
                fontLookup[font],
                fontColorLookup[foregroundColor],
                fontColorLookup[foregroundColor]);
        }
        catch (Exception e)
        {

        }
    }

    public void Dispose()
    {
    }

    public ValueTask<bool> Initialize()
    {
        //VeldridVideoManager = this.context.Services.GetRequiredService<IVideoManager>();
        FontSubSystem.TextRenderer = new TextRenderer(VeldridVideoManager.GraphicDevice);

        var translationTable = this.CreateEnglishTransTable();
        this.InitializeFontManager(translationTable);

        foreach (var fs in Enum.GetValues<FONT_SHADE>())
        {

        }

        Color color;

        fontColorLookup.Add(FontColor.FONT_YELLOW, Color.Yellow);
        fontColorLookup.Add(FontColor.FONT_WHITE, Color.White);
        fontColorLookup.Add(FontColor.FONT_MCOLOR_WHITE, Color.White);

        InitializeFonts();

        return ValueTask.FromResult(true);
    }

    public static void InitializeFonts()
    {
        // Initialize fonts
        gpLargeFontType1 = LoadFontFile(FontStyle.LARGEFONT1, "FONTS\\LARGEFONT1.sti");
        gvoLargeFontType1 = GetFontObject(gpLargeFontType1);
        CreateFontPaletteTables(gvoLargeFontType1);
        fontLookup.TryAdd(FontStyle.LARGEFONT1, FontSubSystem.TextRenderer.LoadFont("Arial", 15, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.LARGEFONT1] = gvoLargeFontType1;
        // 
        gpSmallFontType1 = LoadFontFile(FontStyle.SMALLFONT1, "FONTS\\SMALLFONT1.sti");
        gvoSmallFontType1 = GetFontObject(gpSmallFontType1);
        CreateFontPaletteTables(gvoSmallFontType1);
        fontLookup.TryAdd(FontStyle.SMALLFONT1, FontSubSystem.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.SMALLFONT1] = gvoSmallFontType1;

        gpTinyFontType1 = LoadFontFile(FontStyle.TINYFONT1, "FONTS\\TINYFONT1.sti");
        gvoTinyFontType1 = GetFontObject(gpTinyFontType1);
        CreateFontPaletteTables(gvoTinyFontType1);
        fontLookup.TryAdd(FontStyle.TINYFONT1, FontSubSystem.TextRenderer.LoadFont("Arial", 5, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.TINYFONT1] = gvoTinyFontType1;

        gp12PointFont1 = LoadFontFile(FontStyle.FONT12POINT1, "FONTS\\FONT12POINT1.sti");
        gvo12PointFont1 = GetFontObject(gp12PointFont1);
        CreateFontPaletteTables(gvo12PointFont1);
        fontLookup.TryAdd(FontStyle.FONT12POINT1, FontSubSystem.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Bold));
        FontObjs[FontStyle.FONT12POINT1] = gvo12PointFont1;

        gpClockFont = LoadFontFile(FontStyle.CLOCKFONT, "FONTS\\CLOCKFONT.sti");
        gvoClockFont = GetFontObject(gpClockFont);
        CreateFontPaletteTables(gvoClockFont);
        FontObjs[FontStyle.CLOCKFONT] = gvoClockFont;

        gpCompFont = LoadFontFile(FontStyle.COMPFONT, "FONTS\\COMPFONT.sti");
        gvoCompFont = GetFontObject(gpCompFont);
        CreateFontPaletteTables(gvoCompFont);
        FontObjs[FontStyle.COMPFONT] = gvoCompFont;

        gpSmallCompFont = LoadFontFile(FontStyle.SMALLCOMPFONT, "FONTS\\SMALLCOMPFONT.sti");
        gvoSmallCompFont = GetFontObject(gpSmallCompFont);
        CreateFontPaletteTables(gvoSmallCompFont);
        FontObjs[FontStyle.SMALLCOMPFONT] = gvoSmallCompFont;

        gp10PointRoman = LoadFontFile(FontStyle.FONT10ROMAN, "FONTS\\FONT10ROMAN.sti");
        gvo10PointRoman = GetFontObject(gp10PointRoman);
        CreateFontPaletteTables(gvo10PointRoman);
        fontLookup.TryAdd(FontStyle.FONT10ROMAN, FontSubSystem.TextRenderer.LoadFont("Times New Roman", 10, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT10ROMAN] = gvo10PointRoman;

        gp12PointRoman = LoadFontFile(FontStyle.FONT12ROMAN, "FONTS\\FONT12ROMAN.sti");
        gvo12PointRoman = GetFontObject(gp12PointRoman);
        CreateFontPaletteTables(gvo12PointRoman);
        fontLookup.TryAdd(FontStyle.FONT12ROMAN, FontSubSystem.TextRenderer.LoadFont("Times New Roman", 12, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT12ROMAN] = gvo12PointRoman;

        gp14PointSansSerif = LoadFontFile(FontStyle.FONT14SANSERIF, "FONTS\\FONT14SANSERIF.sti");
        gvo14PointSansSerif = GetFontObject(gp14PointSansSerif);
        CreateFontPaletteTables(gvo14PointSansSerif);
        fontLookup.TryAdd(FontStyle.FONT14SANSERIF, FontSubSystem.TextRenderer.LoadFont("Times New Roman", 14, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT14SANSERIF] = gvo14PointSansSerif;

        //	DEF:	Removed.  Replaced with BLOCKFONT
        //  gpMilitaryFont1  = LoadFontFile( "FONTS\\milfont.sti" );
        //  gvoMilitaryFont1 = GetFontObject( gpMilitaryFont1);
        //   CreateFontPaletteTables( gvoMilitaryFont1) );

        gp10PointArial = LoadFontFile(FontStyle.FONT10ARIAL, "FONTS\\FONT10ARIAL.sti");
        gvo10PointArial = GetFontObject(gp10PointArial);
        CreateFontPaletteTables(gvo10PointArial);
        fontLookup.TryAdd(FontStyle.FONT10ARIAL, FontSubSystem.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT10ARIAL] = gvo10PointArial;

        gp14PointArial = LoadFontFile(FontStyle.FONT14ARIAL, "FONTS\\FONT14ARIAL.sti");
        gvo14PointArial = GetFontObject(gp14PointArial);
        CreateFontPaletteTables(gvo14PointArial);
        fontLookup.TryAdd(FontStyle.FONT14ARIAL, FontSubSystem.TextRenderer.LoadFont("Arial", 14, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT14ARIAL] = gvo14PointArial;

        gp10PointArialBold = LoadFontFile(FontStyle.FONT10ARIALBOLD, "FONTS\\FONT10ARIALBOLD.sti");
        gvo10PointArialBold = GetFontObject(gp10PointArialBold);
        CreateFontPaletteTables(gvo10PointArialBold);
        fontLookup.TryAdd(FontStyle.FONT10ARIALBOLD, FontSubSystem.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Bold));
        FontObjs[FontStyle.FONT10ARIALBOLD] = gvo10PointArialBold;

        gp12PointArial = LoadFontFile(FontStyle.FONT12ARIAL, "FONTS\\FONT12ARIAL.sti");
        gvo12PointArial = GetFontObject(gp12PointArial);
        CreateFontPaletteTables(gvo12PointArial);
        fontLookup.TryAdd(FontStyle.FONT12ARIAL, FontSubSystem.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT12ARIAL] = gvo12PointArial;

        gpBlockyFont = LoadFontFile(FontStyle.BLOCKFONT, "FONTS\\BLOCKFONT.sti");
        gvoBlockyFont = GetFontObject(gpBlockyFont);
        CreateFontPaletteTables(gvoBlockyFont);
        FontObjs[FontStyle.BLOCKFONT] = gvoBlockyFont;

        gpBlockyFont2 = LoadFontFile(FontStyle.BLOCKFONT2, "FONTS\\BLOCKFONT2.sti");
        gvoBlockyFont2 = GetFontObject(gpBlockyFont2);
        CreateFontPaletteTables(gvoBlockyFont2);
        FontObjs[FontStyle.BLOCKFONT2] = gvoBlockyFont2;

        gp12PointArialFixedFont = LoadFontFile(FontStyle.FONT12ARIALFIXEDWIDTH, "FONTS\\FONT12ARIALFIXEDWIDTH.sti");
        gvo12PointArialFixedFont = GetFontObject(gp12PointArialFixedFont);
        CreateFontPaletteTables(gvo12PointArialFixedFont);
        fontLookup.TryAdd(FontStyle.FONT12ARIALFIXEDWIDTH, FontSubSystem.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT12ARIALFIXEDWIDTH] = gvo12PointArialFixedFont;

        gp16PointArial = LoadFontFile(FontStyle.FONT16ARIAL, "FONTS\\FONT16ARIAL.sti");
        gvo16PointArial = GetFontObject(gp16PointArial);
        CreateFontPaletteTables(gvo16PointArial);
        fontLookup.TryAdd(FontStyle.FONT16ARIAL, FontSubSystem.TextRenderer.LoadFont("Arial", 16, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT16ARIAL] = gvo16PointArial;

        gpBlockFontNarrow = LoadFontFile(FontStyle.BLOCKFONTNARROW, "FONTS\\BLOCKFONTNARROW.sti");
        gvoBlockFontNarrow = GetFontObject(gpBlockFontNarrow);
        CreateFontPaletteTables(gvoBlockFontNarrow);
        FontObjs[FontStyle.BLOCKFONTNARROW] = gvoBlockFontNarrow;

        gp14PointHumanist = LoadFontFile(FontStyle.FONT14HUMANIST, "FONTS\\FONT14HUMANIST.sti");
        gvo14PointHumanist = GetFontObject(gp14PointHumanist);
        CreateFontPaletteTables(gvo14PointHumanist);
        fontLookup.TryAdd(FontStyle.FONT14HUMANIST, FontSubSystem.TextRenderer.LoadFont("Arial", 14, SixLabors.Fonts.FontStyle.Regular));
        FontObjs[FontStyle.FONT14HUMANIST] = gvo14PointHumanist;

        gpHugeFont = LoadFontFile(FontStyle.HUGEFONT, "FONTS\\HUGEFONT.sti");
        gvoHugeFont = GetFontObject(gpHugeFont);
        CreateFontPaletteTables(gvoHugeFont);
        FontObjs[FontStyle.HUGEFONT] = gvoHugeFont;
    }

    //*****************************************************************************
    // SetFontColors
    //
    //	Sets both the foreground and the background colors of the current font. The
    // top byte of the parameter word is the background color, and the bottom byte
    // is the foreground.
    //
    //*****************************************************************************
    public static void SetFontColors(FontColor usColors)
    {
        FontColor ubForeground, ubBackground;

        ubForeground = (FontColor)((int)usColors & 0xff);
        ubBackground = (FontColor)(((int)usColors & 0xff00) >> 8);

        SetFontForeground(ubForeground);
        SetFontBackground(ubBackground);
    }


    private void InitializeFontManager(FontTranslationTable translationTable)
    {
        // VeldridVideoManager = this.context.Services.GetRequiredService<IVideoManager>();

        int count;
        int uiRight, uiBottom;
        int uiPixelDepth = 16;

        FontDefault = FontStyle.None;
        //FontDestBuffer = Font.BACKBUFFER;
        //FontDestPitch = 0;

        //	FontDestBPP=0;

        VeldridVideoManager.GetCurrentVideoSettings(out uiRight, out uiBottom, out uiPixelDepth);
        this.FontDestRegion.X = 0;
        this.FontDestRegion.Y = 0;
        this.FontDestRegion.Width = uiRight;
        this.FontDestRegion.Height = uiBottom;
        // FontDestBPP = uiPixelDepth;

        this.FontDestWrap = false;
        FontManager.FontTranslationTable = translationTable;
        FontManager.usDefaultPixelDepth = uiPixelDepth;
    }

    private static bool CreateFontPaletteTables(HVOBJECT pObj)
    {
        SGPPaletteEntry[] Pal = new SGPPaletteEntry[256];

        for (int count = 0; count < 16; count++)
        {
            if ((count == (int)FONT_SHADE.NEUTRAL) && (pObj.p16BPPPalette == pObj.pShades[count]))
            {
                pObj.pShades[count] = null;
            }
            else if (pObj.pShades[count] is not null)
            {
                pObj.pShades[count] = null;
            }
        }

        // Build white palette
        for (int count = 0; count < 256; count++)
        {
            Pal[count].peRed = (byte)255;
            Pal[count].peGreen = (byte)255;
            Pal[count].peBlue = (byte)255;
        }

        pObj.pShades[(int)FONT_SHADE.RED] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 0, 0, true);
        pObj.pShades[(int)FONT_SHADE.BLUE] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 0, 255, true);
        pObj.pShades[(int)FONT_SHADE.GREEN] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 255, 0, true);
        pObj.pShades[(int)FONT_SHADE.YELLOW] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 0, true);
        pObj.pShades[(int)FONT_SHADE.NEUTRAL] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 255, false);

        pObj.pShades[(int)FONT_SHADE.WHITE] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 255, true);

        // the rest are darkening tables, right down to all-black.
        pObj.pShades[0] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 165, 165, 165, false);
        pObj.pShades[7] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 135, 135, 135, false);
        pObj.pShades[8] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 105, 105, 105, false);
        pObj.pShades[9] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 75, 75, 75, false);
        pObj.pShades[10] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 45, 45, 45, false);
        pObj.pShades[11] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 36, 36, 36, false);
        pObj.pShades[12] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 27, 27, 27, false);
        pObj.pShades[13] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 18, 18, 18, false);
        pObj.pShades[14] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 9, 9, 9, false);
        pObj.pShades[15] = VeldridVideoManager.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 0, 0, false);

        // Set current shade table to neutral color
        pObj.pShadeCurrent = pObj.pShades[(int)FONT_SHADE.NEUTRAL]!.Value;

        // check to make sure every table got a palette
        //for(count=0; (count < HVOBJECT_SHADE_TABLES) && (pObj.pShades[count]!=null); count++);

        // return the result of the check
        //return(count==HVOBJECT_SHADE_TABLES);
        return true;
    }

    //*****************************************************************************
    // CreateEnglishTransTable
    //
    // Creates the English text.font map table.
    //*****************************************************************************
    private FontTranslationTable CreateEnglishTransTable()
    {
        FontTranslationTable pTable = new();

        // ha ha, we have more than Wizardry now (again)
        pTable.usNumberOfSymbols = 172;
        pTable.DynamicArrayOf16BitValues = new int[pTable.usNumberOfSymbols * 2];
        var tempList = pTable.DynamicArrayOf16BitValues;
        var temp = 0;
        tempList[temp] = 'A';
        temp++;
        tempList[temp] = 'B';
        temp++;
        tempList[temp] = 'C';
        temp++;
        tempList[temp] = 'D';
        temp++;
        tempList[temp] = 'E';
        temp++;
        tempList[temp] = 'F';
        temp++;
        tempList[temp] = 'G';
        temp++;
        tempList[temp] = 'H';
        temp++;
        tempList[temp] = 'I';
        temp++;
        tempList[temp] = 'J';
        temp++;
        tempList[temp] = 'K';
        temp++;
        tempList[temp] = 'L';
        temp++;
        tempList[temp] = 'M';
        temp++;
        tempList[temp] = 'N';
        temp++;
        tempList[temp] = 'O';
        temp++;
        tempList[temp] = 'P';
        temp++;
        tempList[temp] = 'Q';
        temp++;
        tempList[temp] = 'R';
        temp++;
        tempList[temp] = 'S';
        temp++;
        tempList[temp] = 'T';
        temp++;
        tempList[temp] = 'U';
        temp++;
        tempList[temp] = 'V';
        temp++;
        tempList[temp] = 'W';
        temp++;
        tempList[temp] = 'X';
        temp++;
        tempList[temp] = 'Y';
        temp++;
        tempList[temp] = 'Z';
        temp++;
        tempList[temp] = 'a';
        temp++;
        tempList[temp] = 'b';
        temp++;
        tempList[temp] = 'c';
        temp++;
        tempList[temp] = 'd';
        temp++;
        tempList[temp] = 'e';
        temp++;
        tempList[temp] = 'f';
        temp++;
        tempList[temp] = 'g';
        temp++;
        tempList[temp] = 'h';
        temp++;
        tempList[temp] = 'i';
        temp++;
        tempList[temp] = 'j';
        temp++;
        tempList[temp] = 'k';
        temp++;
        tempList[temp] = 'l';
        temp++;
        tempList[temp] = 'm';
        temp++;
        tempList[temp] = 'n';
        temp++;
        tempList[temp] = 'o';
        temp++;
        tempList[temp] = 'p';
        temp++;
        tempList[temp] = 'q';
        temp++;
        tempList[temp] = 'r';
        temp++;
        tempList[temp] = 's';
        temp++;
        tempList[temp] = 't';
        temp++;
        tempList[temp] = 'u';
        temp++;
        tempList[temp] = 'v';
        temp++;
        tempList[temp] = 'w';
        temp++;
        tempList[temp] = 'x';
        temp++;
        tempList[temp] = 'y';
        temp++;
        tempList[temp] = 'z';
        temp++;
        tempList[temp] = '0';
        temp++;
        tempList[temp] = '1';
        temp++;
        tempList[temp] = '2';
        temp++;
        tempList[temp] = '3';
        temp++;
        tempList[temp] = '4';
        temp++;
        tempList[temp] = '5';
        temp++;
        tempList[temp] = '6';
        temp++;
        tempList[temp] = '7';
        temp++;
        tempList[temp] = '8';
        temp++;
        tempList[temp] = '9';
        temp++;
        tempList[temp] = '!';
        temp++;
        tempList[temp] = '@';
        temp++;
        tempList[temp] = '#';
        temp++;
        tempList[temp] = '$';
        temp++;
        tempList[temp] = '%';
        temp++;
        tempList[temp] = '^';
        temp++;
        tempList[temp] = '&';
        temp++;
        tempList[temp] = '*';
        temp++;
        tempList[temp] = '(';
        temp++;
        tempList[temp] = ')';
        temp++;
        tempList[temp] = '-';
        temp++;
        tempList[temp] = '_';
        temp++;
        tempList[temp] = '+';
        temp++;
        tempList[temp] = '=';
        temp++;
        tempList[temp] = '|';
        temp++;
        tempList[temp] = '\\';
        temp++;
        tempList[temp] = '{';
        temp++;
        tempList[temp] = '}';// 80
        temp++;
        tempList[temp] = '[';
        temp++;
        tempList[temp] = ']';
        temp++;
        tempList[temp] = ':';
        temp++;
        tempList[temp] = ';';
        temp++;
        tempList[temp] = '"';
        temp++;
        tempList[temp] = '\'';
        temp++;
        tempList[temp] = '<';
        temp++;
        tempList[temp] = '>';
        temp++;
        tempList[temp] = ',';
        temp++;
        tempList[temp] = '.';
        temp++;
        tempList[temp] = '?';
        temp++;
        tempList[temp] = '/';
        temp++;
        tempList[temp] = ' '; //93
        temp++;

        tempList[temp] = 196; // "A" umlaut
        temp++;
        tempList[temp] = 214; // "O" umlaut
        temp++;
        tempList[temp] = 220; // "U" umlaut
        temp++;
        tempList[temp] = 228; // "a" umlaut
        temp++;
        tempList[temp] = 246; // "o" umlaut
        temp++;
        tempList[temp] = 252; // "u" umlaut
        temp++;
        tempList[temp] = 223; // double-s that looks like a beta/B  // 100
        temp++;
        // START OF FUNKY RUSSIAN STUFF
        tempList[temp] = 1101;
        temp++;
        tempList[temp] = 1102;
        temp++;
        tempList[temp] = 1103;
        temp++;
        tempList[temp] = 1104;
        temp++;
        tempList[temp] = 1105;
        temp++;
        tempList[temp] = 1106;
        temp++;
        tempList[temp] = 1107;
        temp++;
        tempList[temp] = 1108;
        temp++;
        tempList[temp] = 1109;
        temp++;
        tempList[temp] = 1110;
        temp++;
        tempList[temp] = 1111;
        temp++;
        tempList[temp] = 1112;
        temp++;
        tempList[temp] = 1113;
        temp++;
        tempList[temp] = 1114;
        temp++;
        tempList[temp] = 1115;
        temp++;
        tempList[temp] = 1116;
        temp++;
        tempList[temp] = 1117;
        temp++;
        tempList[temp] = 1118;
        temp++;
        tempList[temp] = 1119;
        temp++;
        tempList[temp] = 1120;
        temp++;
        tempList[temp] = 1121;
        temp++;
        tempList[temp] = 1122;
        temp++;
        tempList[temp] = 1123;
        temp++;
        tempList[temp] = 1124;
        temp++;
        tempList[temp] = 1125;
        temp++;
        tempList[temp] = 1126;
        temp++;
        tempList[temp] = 1127;
        temp++;
        tempList[temp] = 1128;
        temp++;
        tempList[temp] = 1129;
        temp++;
        tempList[temp] = 1130; // 130
        temp++;
        tempList[temp] = 1131;
        temp++;
        tempList[temp] = 1132;
        temp++;
        // END OF FUNKY RUSSIAN STUFF
        tempList[temp] = 196; // Ä 
        temp++;
        tempList[temp] = 192; // À 
        temp++;
        tempList[temp] = 193; // Á 
        temp++;
        tempList[temp] = 194; // Â
        temp++;
        tempList[temp] = 199; // Ç
        temp++;
        tempList[temp] = 203; // Ë
        temp++;
        tempList[temp] = 200; // È
        temp++;
        tempList[temp] = 201; // É				140
        temp++;
        tempList[temp] = 202; // Ê
        temp++;
        tempList[temp] = 207; // Ï
        temp++;
        tempList[temp] = 214; // Ö
        temp++;
        tempList[temp] = 210; // Ò
        temp++;
        tempList[temp] = 211; // Ó
        temp++;
        tempList[temp] = 212; // Ô
        temp++;
        tempList[temp] = 220; // Ü
        temp++;
        tempList[temp] = 217; // Ù
        temp++;
        tempList[temp] = 218; // Ú
        temp++;
        tempList[temp] = 219; // Û				150
        temp++;

        tempList[temp] = 228; // ä
        temp++;
        tempList[temp] = 224; // à
        temp++;
        tempList[temp] = 225; // á
        temp++;
        tempList[temp] = 226; // â
        temp++;
        tempList[temp] = 231; // ç
        temp++;
        tempList[temp] = 235; // ë
        temp++;
        tempList[temp] = 232; // è
        temp++;
        tempList[temp] = 233; // é
        temp++;
        tempList[temp] = 234; // ê
        temp++;
        tempList[temp] = 239; // ï				160
        temp++;
        tempList[temp] = 246; // ö
        temp++;
        tempList[temp] = 242; // ò
        temp++;
        tempList[temp] = 243; // ó
        temp++;
        tempList[temp] = 244; // ô
        temp++;
        tempList[temp] = 252; // ü
        temp++;
        tempList[temp] = 249; // ù
        temp++;
        tempList[temp] = 250; // ú
        temp++;
        tempList[temp] = 251; // û
        temp++;
        tempList[temp] = 204; // Ì
        temp++;
        tempList[temp] = 206; // Î				170
        temp++;
        tempList[temp] = 236; // ì
        temp++;
        tempList[temp] = 238; // î

        return pTable;
    }

    private static HVOBJECT GetFontObject(FontStyle iFont)
    {
        return FontObjs[iFont];
    }

    //*****************************************************************************
    // LoadFontFile
    //
    //	Loads a font from an ETRLE file, and inserts it into one of the font slots.
    //  This function returns (-1) if it fails, and debug msgs for a reason.
    //  Otherwise the font number is returned.
    //*****************************************************************************
    private static FontStyle LoadFontFile(FontStyle LoadIndex, string filename)
    {
        if (FontObjs.ContainsKey(LoadIndex))
        {
            //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Out of font slots (%s)", filename);
            return LoadIndex;
        }

        if ((FontObjs[LoadIndex] = video.CreateVideoObject(filename)) == null)
        {
            //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Error creating VOBJECT (%s)", filename);

            return FontStyle.None;
        }

        if (FontDefault == FontStyle.None)
        {
            FontDefault = LoadIndex;
        }

        return (LoadIndex);
    }

    //*****************************************************************************
    // FindFreeFont
    //
    //	Locates an empty slot in the font table.
    //
    //*****************************************************************************
    private static FontStyle FindFreeFont()
    {
        FontStyle count;

        for (count = 0; count < (FontStyle)MAX_FONTS; count++)
        {
            if (FontObjs[count] == null)
            {
                return count;
            }
        }

        return FontStyle.None;
    }

    public int WFGetFontHeight(FontStyle font)
    {
        return GetFontHeight(font);
    }

    public void VarFindFontCenterCoordinates(int sLeft, int sTop, int sWidth, int sHeight, FontStyle iFontIndex, out int psNewX, out int psNewY, string pFontString)
    {
        psNewX = 0;
        psNewY = 0;
        //wchar_t string[512];
        //va_list argptr;
        //
        //va_start(argptr, pFontString);          // Set up variable argument pointer
        //vwprintf(string, pFontString, argptr);  // process gprintf string (get output str)
        //va_end(argptr);
        //
        FindFontCenterCoordinates(sLeft, sTop, sWidth, sHeight, pFontString, iFontIndex, out psNewX, out psNewY);
    }

    public static void FindFontCenterCoordinates(int sLeft, int sTop, int sWidth, int sHeight, string pStr, FontStyle iFontIndex, out int psNewX, out int psNewY)
    {
        int xp, yp;

        // Compute the coordinates to center the text
        xp = ((sWidth - StringPixLength(pStr, iFontIndex) + 1) / 2) + sLeft;
        yp = ((sHeight - GetFontHeight(iFontIndex)) / 2) + sTop;

        psNewX = xp;
        psNewY = yp;
    }

    internal void ShadowText(string text, FontStyle ulFont, int v1, int v2)
    {
        throw new NotImplementedException();
    }

    public static void SetFontShade(FontStyle font, FONT_SHADE shade)
    {
        throw new NotImplementedException();
    }
}

public enum FONT_SHADE
{
    BLUE = 1,
    GREEN = 2,
    YELLOW = 3,
    NEUTRAL = 4,
    WHITE = 5,
    RED = 6,
};

public enum FontShadow
{
    DEFAULT_SHADOW = 2,
    MILITARY_SHADOW = 67,
    NO_SHADOW = 0,
};

public enum FontStyle
{
    None = -1,
    LARGEFONT1,
    SMALLFONT1,
    TINYFONT1,
    FONT12POINT1,
    CLOCKFONT,
    COMPFONT,
    SMALLCOMPFONT,
    FONT10ROMAN,
    FONT12ROMAN,
    FONT14SANSERIF,
    MILITARYFONT1,
    FONT10ARIAL,
    FONT14ARIAL,
    FONT12ARIAL,
    FONT10ARIALBOLD,
    BLOCKFONT,
    BLOCKFONT2,
    FONT12ARIALFIXEDWIDTH,
    FONT16ARIAL,
    BLOCKFONTNARROW,
    FONT14HUMANIST,
    HUGEFONT,
}

public enum FontColor
{
    None = -1,
    FONT_MCOLOR_WHITE = 73,
    FONT_MCOLOR_BLACK = 0,
    FONT_MCOLOR_TWHITE = 208,
    FONT_MCOLOR_DKWHITE = 134,
    FONT_MCOLOR_DKWHITE2 = 134,
    FONT_MCOLOR_LTGRAY = 134,
    FONT_MCOLOR_LTGRAY2 = 134,
    FONT_MCOLOR_DKGRAY = 136,
    FONT_MCOLOR_LTBLUE = 203,
    FONT_MCOLOR_LTRED = 162,
    FONT_MCOLOR_RED = 163,
    FONT_MCOLOR_DKRED = 164,
    FONT_MCOLOR_LTGREEN = 184,
    FONT_MCOLOR_LTYELLOW = 144,

    //Grayscale font colors
    FONT_WHITE = 208,   //lightest color
    FONT_GRAY1 = 133,
    FONT_GRAY2 = 134,   //light gray
    FONT_GRAY3 = 135,
    FONT_GRAY4 = 136,   //gray
    FONT_GRAY5 = 137,
    FONT_GRAY6 = 138,
    FONT_GRAY7 = 139,   //dark gray
    FONT_GRAY8 = 140,
    FONT_NEARBLACK = 141,
    FONT_BLACK = 0, //darkest color
                    //Color font colors
    FONT_LTRED = 162,
    FONT_RED = 163,
    FONT_DKRED = 218,
    FONT_ORANGE = 76,
    FONT_YELLOW = 145,
    FONT_DKYELLOW = 80,
    FONT_LTGREEN = 184,
    FONT_GREEN = 185,
    FONT_DKGREEN = 186,
    FONT_LTBLUE = 71,
    FONT_BLUE = 203,
    FONT_DKBLUE = 205,

    FONT_BEIGE = 130,
    FONT_METALGRAY = 94,
    FONT_BURGUNDY = 172,
    FONT_LTKHAKI = 88,
    FONT_KHAKI = 198,
    FONT_DKKHAKI = 201,

    // these are bogus! No palette is set yet!
    // font foreground color symbols
    FONT_FCOLOR_WHITE = 208,
    FONT_FCOLOR_RED = 162,
    FONT_FCOLOR_NICERED = 164,
    FONT_FCOLOR_BLUE = 203,
    FONT_FCOLOR_GREEN = 184,
    FONT_FCOLOR_YELLOW = 144,
    FONT_FCOLOR_BROWN = 184,
    FONT_FCOLOR_ORANGE = 76,
    FONT_FCOLOR_PURPLE = 160,

    // font background color symbols
    FONT_BCOLOR_WHITE = 208,
    FONT_BCOLOR_RED = 162,
    FONT_BCOLOR_BLUE = 203,
    FONT_BCOLOR_GREEN = 184,
    FONT_BCOLOR_YELLOW = 144,
    FONT_BCOLOR_BROWN = 80,
    FONT_BCOLOR_ORANGE = 76,
    FONT_BCOLOR_PURPLE = 160,
}

[Flags]
public enum TextJustifies
{
    LEFT_JUSTIFIED = 0x00000001,
    CENTER_JUSTIFIED = 0x00000002,
    RIGHT_JUSTIFIED = 0x00000004,
    TEXT_SHADOWED = 0x00000008,
    INVALIDATE_TEXT = 0x00000010,

    //Wont display the text.  Used if you just want to get how many lines will be displayed
    DONT_DISPLAY_TEXT = 0x00000020,
}
