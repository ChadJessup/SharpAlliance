﻿using System;
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

namespace SharpAlliance.Core.SubSystems
{
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
        private FontManager FontManager { get; } = new FontManager();

        private const int PALETTE_SIZE = 768;
        private const int STRING_DELIMITER = 0;
        private const int ID_BLACK = 0;
        private const int MAX_FONTS = 25;
        public const int INVALIDATE_TEXT = 0x00000010;
        public Rectangle FontDestRegion = new(0, 0, 640, 480);

        // Wont display the text.  Used if you just want to get how many lines will be displayed
        public const int DONT_DISPLAY_TEXT = 0x00000020;
        private readonly GameContext context;
        private IVideoManager video;
        private FontStyle gpLargeFontType1;
        private HVOBJECT gvoLargeFontType1;
        private FontStyle FontDefault;
        //private int FontDestBuffer = BACKBUFFER;
        private int FontDestPitch = 640 * 2;
        private int FontDestBPP = 16;
        private FontColor FontForeground16 = 0;
        private FontColor FontBackground16 = 0;
        private FontShadow FontShadow16 = FontShadow.DEFAULT_SHADOW;
        private FontColor FontForeground8 = 0;
        private FontColor FontBackground8 = 0;

        private HVOBJECT[] FontObjs = new HVOBJECT[MAX_FONTS];
        private FontStyle gpSmallFontType1;
        private HVOBJECT gvoSmallFontType1;
        private FontStyle gpTinyFontType1;
        private HVOBJECT gvoTinyFontType1;
        private FontStyle gp12PointFont1;
        private HVOBJECT gvo12PointFont1;
        private FontStyle gpClockFont;
        private HVOBJECT gvoClockFont;
        private FontStyle gpCompFont;
        private HVOBJECT gvoCompFont;
        private FontStyle gpSmallCompFont;
        private HVOBJECT gvoSmallCompFont;
        private FontStyle gp10PointRoman;
        private HVOBJECT gvo10PointRoman;
        private FontStyle gp12PointRoman;
        private HVOBJECT gvo12PointRoman;
        private FontStyle gp14PointSansSerif;
        private HVOBJECT gvo14PointSansSerif;
        private FontStyle gp10PointArial;
        private HVOBJECT gvo10PointArial;
        private FontStyle gp14PointArial;
        private HVOBJECT gvo14PointArial;
        private FontStyle gp10PointArialBold;
        private HVOBJECT gvo10PointArialBold;
        private FontStyle gp12PointArial;
        private HVOBJECT gvo12PointArial;
        private FontStyle gpBlockyFont;
        private HVOBJECT gvoBlockyFont;
        private FontStyle gpBlockyFont2;
        private HVOBJECT gvoBlockyFont2;
        private FontStyle gp12PointArialFixedFont;
        private HVOBJECT gvo12PointArialFixedFont;
        private FontStyle gp16PointArial;
        private HVOBJECT gvo16PointArial;
        private FontStyle gpBlockFontNarrow;
        private HVOBJECT gvoBlockFontNarrow;
        private FontStyle gp14PointHumanist;
        private HVOBJECT gvo14PointHumanist;
        private FontStyle gpHugeFont;
        private HVOBJECT gvoHugeFont;

        private Dictionary<FontStyle, Font> fontLookup = new();
        private Dictionary<FontColor, Rgba32> fontColorLookup = new();

        public bool IsInitialized { get; }
        public bool FontDestWrap { get; private set; }
        public TextJustifies IAN_WRAP_NO_SHADOW { get; } = (TextJustifies)32;

        public TextRenderer TextRenderer { get; private set; }

        public FontSubSystem(GameContext gameContext)
        {
            this.context = gameContext;
        }

        public void SetFont(FontStyle fontStyle)
        {
            this.FontDefault = fontStyle;
        }

        public void SetFontBackground(FontColor fontColor)
        {
        }

        public void SetFontForeground(FontColor ubForeground)
        {
            int uiRed, uiGreen, uiBlue;

            if ((FontDefault < 0) || (((int)FontDefault) > MAX_FONTS))
            {
                return;
            }

            FontForeground8 = ubForeground;

            uiRed = (int)FontObjs[(int)FontDefault].pPaletteEntry[(int)ubForeground].peRed;
            uiGreen = (int)FontObjs[(int)FontDefault].pPaletteEntry[(int)ubForeground].peGreen;
            uiBlue = (int)FontObjs[(int)FontDefault].pPaletteEntry[(int)ubForeground].peBlue;

            FontForeground16 = FontColor.FONT_MCOLOR_LTBLUE; // Get16BPPColor(FROMRGB(uiRed, uiGreen, uiBlue));
        }

        public void SaveFontSettings()
        {
        }

        public void RestoreFontSettings()
        {
        }

        public int GetFontHeight(FontStyle usFont)
        {
            return this.GetHeight(this.FontObjs[(int)usFont], 0);
        }

        private int GetHeight(HVOBJECT hSrcVObject, int ssIndex)
        {
            ETRLEObject pTrav;

            // Get Offsets from Index into structure
            pTrav = hSrcVObject.pETRLEObject[ssIndex];
            return pTrav.usHeight + pTrav.sOffsetY;
        }

        public void SetFontDestBuffer(Surfaces buttonDestBuffer, int y1, int y2, int width, int height, bool v)
        {
        }

        public int StringPixLength(string stringText, FontStyle UseFont)
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
                transletter = this.GetIndex(curletter[idx++]);
                Cur += this.GetWidth(this.FontObjs[(int)UseFont], transletter);
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
        private char GetIndex(char siChar)
        {
            char ssCount = (char)0;
            int usNumberOfSymbols = this.FontManager.FontTranslationTable.usNumberOfSymbols;

            // search the Translation Table and return the index for the font
            int idx = 0;
            int pTrav = this.FontManager.FontTranslationTable.DynamicArrayOf16BitValues[idx];

            while (ssCount < usNumberOfSymbols)
            {
                if (siChar == pTrav)
                {
                    return ssCount;
                }
                ssCount++;
                pTrav = this.FontManager.FontTranslationTable.DynamicArrayOf16BitValues[++idx];
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
        private int GetWidth(HVOBJECT hSrcVObject, char ssIndex)
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

        public void SetFontShadow(FontShadow sShadowColor)
        {
        }

        public int DisplayWrappedString(
            Point pos,
            int sWrappedWidth,
            int gap,
            FontStyle usFont,
            FontColor sForeColor,
            string stringText,
            FontColor sBackgroundColor,
            TextJustifies bJustified)
        {
            this.DrawTextToScreen(
                stringText,
                pos,
                sWrappedWidth,
                usFont,
                sForeColor,
                sBackgroundColor,
                bJustified);

            return 0;
        }

        public void DrawTextToScreen(
            string text,
            Point pos,
            int width,
            FontStyle font,
            FontColor foregroundColor,
            FontColor backgroundColor,
            TextJustifies justification)
            => this.DrawTextToScreen(
                text,
                pos.X,
                pos.Y,
                width,
                font,
                foregroundColor,
                backgroundColor,
                justification);

        public void DrawTextToScreen(
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
                this.TextRenderer.DrawText(
                    text,
                    x,
                    y,
                    width,
                    alignment,
                    this.fontLookup[font],
                    this.fontColorLookup[foregroundColor],
                    this.fontColorLookup[foregroundColor]);
            }
            catch(Exception e)
            {

            }
        }

        public void Dispose()
        {
        }

        public ValueTask<bool> Initialize()
        {
            this.video = this.context.Services.GetRequiredService<IVideoManager>();
            this.TextRenderer = new TextRenderer(this.video.GraphicDevice);

            var translationTable = this.CreateEnglishTransTable();
            this.InitializeFontManager(translationTable);

            foreach (var fs in Enum.GetValues<FONT_SHADE>())
            {

            }

            Color color;

            this.fontColorLookup.Add(FontColor.FONT_YELLOW, Color.Yellow);
            this.fontColorLookup.Add(FontColor.FONT_WHITE, Color.White);
            this.fontColorLookup.Add(FontColor.FONT_MCOLOR_WHITE, Color.White);

            // Initialize fonts
            // gpLargeFontType1  = this.LoadFontFile( "FONTS\\lfont1.sti" );
            this.gpLargeFontType1 = this.LoadFontFile("FONTS\\LARGEFONT1.sti");
            this.gvoLargeFontType1 = this.GetFontObject(this.gpLargeFontType1);
            this.CreateFontPaletteTables(this.gvoLargeFontType1);
            this.fontLookup.TryAdd(FontStyle.LARGEFONT1, this.TextRenderer.LoadFont("Arial", 15, SixLabors.Fonts.FontStyle.Regular));
            // 
            // //	gpSmallFontType1  = LoadFontFile( "FONTS\\6b-font.sti" );
            this.gpSmallFontType1 = this.LoadFontFile("FONTS\\SMALLFONT1.sti");
            this.gvoSmallFontType1 = this.GetFontObject(this.gpSmallFontType1);
            this.CreateFontPaletteTables(this.gvoSmallFontType1);
            this.fontLookup.TryAdd(FontStyle.SMALLFONT1, this.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Regular));

            //	gpTinyFontType1  = LoadFontFile( "FONTS\\tfont1.sti" );
            this.gpTinyFontType1 = this.LoadFontFile("FONTS\\TINYFONT1.sti");
            this.gvoTinyFontType1 = this.GetFontObject(this.gpTinyFontType1);
            this.CreateFontPaletteTables(this.gvoTinyFontType1);
            this.fontLookup.TryAdd(FontStyle.TINYFONT1, this.TextRenderer.LoadFont("Arial", 5, SixLabors.Fonts.FontStyle.Regular));

            //	gp12PointFont1	= LoadFontFile( "FONTS\\font-12.sti" );
            this.gp12PointFont1 = this.LoadFontFile("FONTS\\FONT12POINT1.sti");
            this.gvo12PointFont1 = this.GetFontObject(this.gp12PointFont1);
            this.CreateFontPaletteTables(this.gvo12PointFont1);
            this.fontLookup.TryAdd(FontStyle.FONT12POINT1, this.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Bold));

            //  gpClockFont  = LoadFontFile( "FONTS\\DIGI.sti" );
            this.gpClockFont = this.LoadFontFile("FONTS\\CLOCKFONT.sti");
            this.gvoClockFont = this.GetFontObject(this.gpClockFont);
            this.CreateFontPaletteTables(this.gvoClockFont);

            //  gpCompFont  = LoadFontFile( "FONTS\\compfont.sti" );
            this.gpCompFont = this.LoadFontFile("FONTS\\COMPFONT.sti");
            this.gvoCompFont = this.GetFontObject(this.gpCompFont);
            this.CreateFontPaletteTables(this.gvoCompFont);

            //  gpSmallCompFont  = LoadFontFile( "FONTS\\scfont.sti" );
            this.gpSmallCompFont = this.LoadFontFile("FONTS\\SMALLCOMPFONT.sti");
            this.gvoSmallCompFont = this.GetFontObject(this.gpSmallCompFont);
            this.CreateFontPaletteTables(this.gvoSmallCompFont);

            //  gp10PointRoman  = LoadFontFile( "FONTS\\Roman10.sti" );
            this.gp10PointRoman = this.LoadFontFile("FONTS\\FONT10ROMAN.sti");
            this.gvo10PointRoman = this.GetFontObject(this.gp10PointRoman);
            this.CreateFontPaletteTables(this.gvo10PointRoman);
            this.fontLookup.TryAdd(FontStyle.FONT10ROMAN, this.TextRenderer.LoadFont("Times New Roman", 10, SixLabors.Fonts.FontStyle.Regular));


            //  gp12PointRoman  = LoadFontFile( "FONTS\\Roman12.sti" );
            this.gp12PointRoman = this.LoadFontFile("FONTS\\FONT12ROMAN.sti");
            this.gvo12PointRoman = this.GetFontObject(this.gp12PointRoman);
            this.CreateFontPaletteTables(this.gvo12PointRoman);
            this.fontLookup.TryAdd(FontStyle.FONT12ROMAN, this.TextRenderer.LoadFont("Times New Roman", 12, SixLabors.Fonts.FontStyle.Regular));

            //  gp14PointSansSerif  = LoadFontFile( "FONTS\\SansSerif14.sti" );
            this.gp14PointSansSerif = this.LoadFontFile("FONTS\\FONT14SANSERIF.sti");
            this.gvo14PointSansSerif = this.GetFontObject(this.gp14PointSansSerif);
            this.CreateFontPaletteTables(this.gvo14PointSansSerif);
            this.fontLookup.TryAdd(FontStyle.FONT14SANSERIF, this.TextRenderer.LoadFont("Times New Roman", 14, SixLabors.Fonts.FontStyle.Regular));

            //	DEF:	Removed.  Replaced with BLOCKFONT
            //  gpMilitaryFont1  = LoadFontFile( "FONTS\\milfont.sti" );
            //  gvoMilitaryFont1 = GetFontObject( gpMilitaryFont1);
            //   CreateFontPaletteTables( gvoMilitaryFont1) );


            //  gp10PointArial  = LoadFontFile( "FONTS\\Arial10.sti" );
            this.gp10PointArial = this.LoadFontFile("FONTS\\FONT10ARIAL.sti");
            this.gvo10PointArial = this.GetFontObject(this.gp10PointArial);
            this.CreateFontPaletteTables(this.gvo10PointArial);
            this.fontLookup.TryAdd(FontStyle.FONT10ARIAL, this.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Regular));

            //  gp14PointArial  = LoadFontFile( "FONTS\\Arial14.sti" );
            this.gp14PointArial = this.LoadFontFile("FONTS\\FONT14ARIAL.sti");
            this.gvo14PointArial = this.GetFontObject(this.gp14PointArial);
            this.CreateFontPaletteTables(this.gvo14PointArial);
            this.fontLookup.TryAdd(FontStyle.FONT14ARIAL, this.TextRenderer.LoadFont("Arial", 14, SixLabors.Fonts.FontStyle.Regular));

            //  gp10PointArialBold  = LoadFontFile( "FONTS\\Arial10Bold2.sti" );
            this.gp10PointArialBold = this.LoadFontFile("FONTS\\FONT10ARIALBOLD.sti");
            this.gvo10PointArialBold = this.GetFontObject(this.gp10PointArialBold);
            this.CreateFontPaletteTables(this.gvo10PointArialBold);
            this.fontLookup.TryAdd(FontStyle.FONT10ARIALBOLD, this.TextRenderer.LoadFont("Arial", 10, SixLabors.Fonts.FontStyle.Bold));

            //  gp12PointArial  = LoadFontFile( "FONTS\\Arial12.sti" );
            this.gp12PointArial = this.LoadFontFile("FONTS\\FONT12ARIAL.sti");
            this.gvo12PointArial = this.GetFontObject(this.gp12PointArial);
            this.CreateFontPaletteTables(this.gvo12PointArial);
            this.fontLookup.TryAdd(FontStyle.FONT12ARIAL, this.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Regular));

            //	gpBlockyFont  = LoadFontFile( "FONTS\\FONT2.sti" );
            this.gpBlockyFont = this.LoadFontFile("FONTS\\BLOCKFONT.sti");
            this.gvoBlockyFont = this.GetFontObject(this.gpBlockyFont);
            this.CreateFontPaletteTables(this.gvoBlockyFont);

            //	gpBlockyFont2  = LoadFontFile( "FONTS\\interface_font.sti" );
            this.gpBlockyFont2 = this.LoadFontFile("FONTS\\BLOCKFONT2.sti");
            this.gvoBlockyFont2 = this.GetFontObject(this.gpBlockyFont2);
            this.CreateFontPaletteTables(this.gvoBlockyFont2);

            //	gp12PointArialFixedFont = LoadFontFile( "FONTS\\Arial12FixedWidth.sti" );
            this.gp12PointArialFixedFont = this.LoadFontFile("FONTS\\FONT12ARIALFIXEDWIDTH.sti");
            this.gvo12PointArialFixedFont = this.GetFontObject(this.gp12PointArialFixedFont);
            this.CreateFontPaletteTables(this.gvo12PointArialFixedFont);
            this.fontLookup.TryAdd(FontStyle.FONT12ARIALFIXEDWIDTH, this.TextRenderer.LoadFont("Arial", 12, SixLabors.Fonts.FontStyle.Regular));

            this.gp16PointArial = this.LoadFontFile("FONTS\\FONT16ARIAL.sti");
            this.gvo16PointArial = this.GetFontObject(this.gp16PointArial);
            this.CreateFontPaletteTables(this.gvo16PointArial);
            this.fontLookup.TryAdd(FontStyle.FONT16ARIAL, this.TextRenderer.LoadFont("Arial", 16, SixLabors.Fonts.FontStyle.Regular));

            this.gpBlockFontNarrow = this.LoadFontFile("FONTS\\BLOCKFONTNARROW.sti");
            this.gvoBlockFontNarrow = this.GetFontObject(this.gpBlockFontNarrow);
            this.CreateFontPaletteTables(this.gvoBlockFontNarrow);

            this.gp14PointHumanist = this.LoadFontFile("FONTS\\FONT14HUMANIST.sti");
            this.gvo14PointHumanist = this.GetFontObject(this.gp14PointHumanist);
            this.CreateFontPaletteTables(this.gvo14PointHumanist);
            this.fontLookup.TryAdd(FontStyle.FONT14HUMANIST, this.TextRenderer.LoadFont("Arial", 14, SixLabors.Fonts.FontStyle.Regular));

            this.gpHugeFont = this.LoadFontFile("FONTS\\HUGEFONT.sti");
            this.gvoHugeFont = this.GetFontObject(this.gpHugeFont);
            this.CreateFontPaletteTables(this.gvoHugeFont);

            return ValueTask.FromResult(true);
        }

        private void InitializeFontManager(FontTranslationTable translationTable)
        {
            this.video = this.context.Services.GetRequiredService<IVideoManager>();

            int count;
            int uiRight, uiBottom;
            int uiPixelDepth = 16;

            this.FontDefault = FontStyle.BLOCKFONT;
            //FontDestBuffer = Font.BACKBUFFER;
            //FontDestPitch = 0;

            //	FontDestBPP=0;

            this.video.GetCurrentVideoSettings(out uiRight, out uiBottom, out uiPixelDepth);
            this.FontDestRegion.X = 0;
            this.FontDestRegion.Y = 0;
            this.FontDestRegion.Width = uiRight;
            this.FontDestRegion.Height = uiBottom;
            // FontDestBPP = uiPixelDepth;

            this.FontDestWrap = false;
            this.FontManager.FontTranslationTable = translationTable;
            this.FontManager.usDefaultPixelDepth = uiPixelDepth;
        }

        private bool CreateFontPaletteTables(HVOBJECT pObj)
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

            pObj.pShades[(int)FONT_SHADE.RED] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 0, 0, true);
            pObj.pShades[(int)FONT_SHADE.BLUE] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 0, 255, true);
            pObj.pShades[(int)FONT_SHADE.GREEN] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 255, 0, true);
            pObj.pShades[(int)FONT_SHADE.YELLOW] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 0, true);
            pObj.pShades[(int)FONT_SHADE.NEUTRAL] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 255, false);

            pObj.pShades[(int)FONT_SHADE.WHITE] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 255, 255, 255, true);

            // the rest are darkening tables, right down to all-black.
            pObj.pShades[0] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 165, 165, 165, false);
            pObj.pShades[7] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 135, 135, 135, false);
            pObj.pShades[8] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 105, 105, 105, false);
            pObj.pShades[9] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 75, 75, 75, false);
            pObj.pShades[10] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 45, 45, 45, false);
            pObj.pShades[11] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 36, 36, 36, false);
            pObj.pShades[12] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 27, 27, 27, false);
            pObj.pShades[13] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 18, 18, 18, false);
            pObj.pShades[14] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 9, 9, 9, false);
            pObj.pShades[15] = this.video.Create16BPPPaletteShaded(ref pObj.pPaletteEntry, 0, 0, 0, false);

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
        // Creates the English text->font map table.
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

        private HVOBJECT GetFontObject(FontStyle iFont)
        {
            return this.FontObjs[(int)iFont];
        }

        //*****************************************************************************
        // LoadFontFile
        //
        //	Loads a font from an ETRLE file, and inserts it into one of the font slots.
        //  This function returns (-1) if it fails, and debug msgs for a reason.
        //  Otherwise the font number is returned.
        //*****************************************************************************
        private FontStyle LoadFontFile(string filename)
        {
            FontStyle LoadIndex;

            if ((LoadIndex = this.FindFreeFont()) == FontStyle.None)
            {
                //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Out of font slots (%s)", filename);
                return FontStyle.None;
            }

            if ((this.FontObjs[(int)LoadIndex] = this.video.CreateVideoObject(filename)) == null)
            {
                //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Error creating VOBJECT (%s)", filename);

                return FontStyle.None;
            }

            if (this.FontDefault == FontStyle.None)
            {
                this.FontDefault = LoadIndex;
            }

            return (LoadIndex);
        }

        //*****************************************************************************
        // FindFreeFont
        //
        //	Locates an empty slot in the font table.
        //
        //*****************************************************************************
        private FontStyle FindFreeFont()
        {
            int count;

            for (count = 0; count < MAX_FONTS; count++)
            {
                if (this.FontObjs[count] == null)
                {
                    return (FontStyle)count;
                }
            }

            return FontStyle.None;

        }

        public int WFGetFontHeight(FontStyle font)
        {
            return this.GetFontHeight(font);
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

        private void FindFontCenterCoordinates(int sLeft, int sTop, int sWidth, int sHeight, string pStr, FontStyle iFontIndex, out int psNewX, out int psNewY)
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
}
