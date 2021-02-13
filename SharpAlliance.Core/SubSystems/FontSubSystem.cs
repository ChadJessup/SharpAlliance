using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems
{
    public class FontSubSystem : ISharpAllianceManager
    {
        private const int PALETTE_SIZE = 768;
        private const int STRING_DELIMITER = 0;
        private const int ID_BLACK = 0;
        private const int MAX_FONTS = 25;
        public const int INVALIDATE_TEXT = 0x00000010;

        // Wont display the text.  Used if you just want to get how many lines will be displayed
        public const int DONT_DISPLAY_TEXT = 0x00000020;
        private readonly GameContext context;
        private IVideoManager video;
        private int gpLargeFontType1;
        private HVOBJECT gvoLargeFontType1;
        private int FontDefault = -1;

        private HVOBJECT[] FontObjs = new HVOBJECT[MAX_FONTS];
        private int gpSmallFontType1;
        private HVOBJECT gvoSmallFontType1;
        private int gpTinyFontType1;
        private HVOBJECT gvoTinyFontType1;
        private int gp12PointFont1;
        private HVOBJECT gvo12PointFont1;
        private int gpClockFont;
        private HVOBJECT gvoClockFont;
        private int gpCompFont;
        private HVOBJECT gvoCompFont;
        private int gpSmallCompFont;
        private HVOBJECT gvoSmallCompFont;
        private int gp10PointRoman;
        private HVOBJECT gvo10PointRoman;
        private int gp12PointRoman;
        private HVOBJECT gvo12PointRoman;
        private int gp14PointSansSerif;
        private HVOBJECT gvo14PointSansSerif;
        private int gp10PointArial;
        private HVOBJECT gvo10PointArial;
        private int gp14PointArial;
        private HVOBJECT gvo14PointArial;
        private int gp10PointArialBold;
        private HVOBJECT gvo10PointArialBold;
        private int gp12PointArial;
        private HVOBJECT gvo12PointArial;
        private int gpBlockyFont;
        private HVOBJECT gvoBlockyFont;
        private int gpBlockyFont2;
        private HVOBJECT gvoBlockyFont2;
        private int gp12PointArialFixedFont;
        private HVOBJECT gvo12PointArialFixedFont;
        private int gp16PointArial;
        private HVOBJECT gvo16PointArial;
        private int gpBlockFontNarrow;
        private HVOBJECT gvoBlockFontNarrow;
        private int gp14PointHumanist;
        private HVOBJECT gvo14PointHumanist;
        private int gpHugeFont;
        private HVOBJECT gvoHugeFont;

        public bool IsInitialized { get; }

        public FontSubSystem(GameContext gameContext)
        {
            this.context = gameContext;
        }

        public void SetFont(FontStyle fontStyle)
        {
        }

        public void SetFontBackground(FontColor fontColor)
        {
        }

        public void SetFontForeground(FontColor fontColor)
        {
        }

        public void SaveFontSettings()
        {
        }

        public void RestoreFontSettings()
        {
        }

        public int GetFontHeight(FontStyle usFont)
        {
            return 10;
        }

        public void SetFontDestBuffer(Surfaces buttonDestBuffer, int y1, int y2, int width, int height, bool v)
        {
        }

        public int StringPixLength(string stringText, FontStyle usFont)
        {
            return 1;
        }

        public void SetFontShadow(FontShadow sShadowColor)
        {
        }

        public int DisplayWrappedString(
            Point pos,
            int sWrappedWidth,
            int v1,
            FontStyle usFont,
            FontColor sForeColor,
            string stringText,
            FontColor sBackgroundColor,
            bool fDirty,
            TextJustifies bJustified)
        {
            WRAPPED_STRING pFirstWrappedString, pTempWrappedString;
            ushort uiCounter = 0;
            ushort usLineWidthIfWordIsWiderThenWidth = 0;
            ushort usHeight;

            //            usHeight = WFGetFontHeight(uiFont);
            //
            //            //If we are to a Single char for a word ( like in Taiwan )
            //            if (gfUseSingleCharWordsForWordWrap)
            //            {
            //                pFirstWrappedString = LineWrapForSingleCharWords(uiFont, usWidth, &usLineWidthIfWordIsWiderThenWidth, pString);
            //            }
            //            else
            //            {
            //                pFirstWrappedString = LineWrap(uiFont, usWidth, &usLineWidthIfWordIsWiderThenWidth, pString);
            //            }
            //
            //            //if an error occured and a word was bigger then the width passed in, reset the width
            //            if (usLineWidthIfWordIsWiderThenWidth != usWidth)
            //                usWidth = usLineWidthIfWordIsWiderThenWidth;
            //
            //            while (pFirstWrappedString != null)
            //            {
            //                DrawTextToScreen(pFirstWrappedString.sString, usPosX, usPosY, usWidth, uiFont, ubColor, ubBackGroundColor, fDirty, uiFlags);
            //
            //                pTempWrappedString = pFirstWrappedString;
            //                pFirstWrappedString = pTempWrappedString.pNextWrappedString;
            //                MemFree(pTempWrappedString.sString);
            //                pTempWrappedString.sString = null;
            //                MemFree(pTempWrappedString);
            //                pTempWrappedString = null;
            //
            //                uiCounter++;
            //
            //                usPosY += usHeight + ubGap;
            //            }
            //
            //            return (uiCounter * (WFGetFontHeight(uiFont) + ubGap);
            return 0;
        }

        public void DrawTextToScreen(string v1, Point pos, int v2, FontStyle oPT_MAIN_FONT, FontColor oPT_MAIN_COLOR, FontColor fONT_MCOLOR_BLACK, bool v3, TextJustifies lEFT_JUSTIFIED)
        {
        }

        public void Dispose()
        {
        }

        public ValueTask<bool> Initialize()
        {
            this.video = this.context.Services.GetRequiredService<IVideoManager>();

            foreach (var fs in Enum.GetValues<FONT_SHADE>())
            {

            }

            Color color;

            // Initialize fonts
            // gpLargeFontType1  = this.LoadFontFile( "FONTS\\lfont1.sti" );
            this.gpLargeFontType1 = this.LoadFontFile("FONTS\\LARGEFONT1.sti");
            this.gvoLargeFontType1 = this.GetFontObject(this.gpLargeFontType1);
            this.CreateFontPaletteTables(this.gvoLargeFontType1);

            // 
            // //	gpSmallFontType1  = LoadFontFile( "FONTS\\6b-font.sti" );
            this.gpSmallFontType1 = LoadFontFile("FONTS\\SMALLFONT1.sti");
            gvoSmallFontType1 = GetFontObject(gpSmallFontType1);
            CreateFontPaletteTables(gvoSmallFontType1);

            //	gpTinyFontType1  = LoadFontFile( "FONTS\\tfont1.sti" );
            gpTinyFontType1 = LoadFontFile("FONTS\\TINYFONT1.sti");
            gvoTinyFontType1 = GetFontObject(gpTinyFontType1);
            CreateFontPaletteTables(gvoTinyFontType1);

            //	gp12PointFont1	= LoadFontFile( "FONTS\\font-12.sti" );
            gp12PointFont1 = LoadFontFile("FONTS\\FONT12POINT1.sti");
            gvo12PointFont1 = GetFontObject(gp12PointFont1);
            CreateFontPaletteTables(gvo12PointFont1);


            //  gpClockFont  = LoadFontFile( "FONTS\\DIGI.sti" );
            gpClockFont = LoadFontFile("FONTS\\CLOCKFONT.sti");
            gvoClockFont = GetFontObject(gpClockFont);
            CreateFontPaletteTables(gvoClockFont);

            //  gpCompFont  = LoadFontFile( "FONTS\\compfont.sti" );
            gpCompFont = LoadFontFile("FONTS\\COMPFONT.sti");
            gvoCompFont = GetFontObject(gpCompFont);
            CreateFontPaletteTables(gvoCompFont);

            //  gpSmallCompFont  = LoadFontFile( "FONTS\\scfont.sti" );
            gpSmallCompFont = LoadFontFile("FONTS\\SMALLCOMPFONT.sti");
            gvoSmallCompFont = GetFontObject(gpSmallCompFont);
            CreateFontPaletteTables(gvoSmallCompFont);

            //  gp10PointRoman  = LoadFontFile( "FONTS\\Roman10.sti" );
            gp10PointRoman = LoadFontFile("FONTS\\FONT10ROMAN.sti");
            gvo10PointRoman = GetFontObject(gp10PointRoman);
            CreateFontPaletteTables(gvo10PointRoman);

            //  gp12PointRoman  = LoadFontFile( "FONTS\\Roman12.sti" );
            gp12PointRoman = LoadFontFile("FONTS\\FONT12ROMAN.sti");
            gvo12PointRoman = GetFontObject(gp12PointRoman);
            CreateFontPaletteTables(gvo12PointRoman);

            //  gp14PointSansSerif  = LoadFontFile( "FONTS\\SansSerif14.sti" );
            gp14PointSansSerif = LoadFontFile("FONTS\\FONT14SANSERIF.sti");
            gvo14PointSansSerif = GetFontObject(gp14PointSansSerif);
            CreateFontPaletteTables(gvo14PointSansSerif);

            //	DEF:	Removed.  Replaced with BLOCKFONT
            //  gpMilitaryFont1  = LoadFontFile( "FONTS\\milfont.sti" );
            //  gvoMilitaryFont1 = GetFontObject( gpMilitaryFont1);
            //   CreateFontPaletteTables( gvoMilitaryFont1) );


            //  gp10PointArial  = LoadFontFile( "FONTS\\Arial10.sti" );
            gp10PointArial = LoadFontFile("FONTS\\FONT10ARIAL.sti");
            gvo10PointArial = GetFontObject(gp10PointArial);
            CreateFontPaletteTables(gvo10PointArial);

            //  gp14PointArial  = LoadFontFile( "FONTS\\Arial14.sti" );
            gp14PointArial = LoadFontFile("FONTS\\FONT14ARIAL.sti");
            gvo14PointArial = GetFontObject(gp14PointArial);
            CreateFontPaletteTables(gvo14PointArial);

            //  gp10PointArialBold  = LoadFontFile( "FONTS\\Arial10Bold2.sti" );
            gp10PointArialBold = LoadFontFile("FONTS\\FONT10ARIALBOLD.sti");
            gvo10PointArialBold = GetFontObject(gp10PointArialBold);
            CreateFontPaletteTables(gvo10PointArialBold);

            //  gp12PointArial  = LoadFontFile( "FONTS\\Arial12.sti" );
            gp12PointArial = LoadFontFile("FONTS\\FONT12ARIAL.sti");
            gvo12PointArial = GetFontObject(gp12PointArial);
            CreateFontPaletteTables(gvo12PointArial);

            //	gpBlockyFont  = LoadFontFile( "FONTS\\FONT2.sti" );
            gpBlockyFont = LoadFontFile("FONTS\\BLOCKFONT.sti");
            gvoBlockyFont = GetFontObject(gpBlockyFont);
            CreateFontPaletteTables(gvoBlockyFont);

            //	gpBlockyFont2  = LoadFontFile( "FONTS\\interface_font.sti" );
            gpBlockyFont2 = LoadFontFile("FONTS\\BLOCKFONT2.sti");
            gvoBlockyFont2 = GetFontObject(gpBlockyFont2);
            CreateFontPaletteTables(gvoBlockyFont2);

            //	gp12PointArialFixedFont = LoadFontFile( "FONTS\\Arial12FixedWidth.sti" );
            gp12PointArialFixedFont = LoadFontFile("FONTS\\FONT12ARIALFIXEDWIDTH.sti");
            gvo12PointArialFixedFont = GetFontObject(gp12PointArialFixedFont);
            CreateFontPaletteTables(gvo12PointArialFixedFont);

            gp16PointArial = LoadFontFile("FONTS\\FONT16ARIAL.sti");
            gvo16PointArial = GetFontObject(gp16PointArial);
            CreateFontPaletteTables(gvo16PointArial);

            gpBlockFontNarrow = LoadFontFile("FONTS\\BLOCKFONTNARROW.sti");
            gvoBlockFontNarrow = GetFontObject(gpBlockFontNarrow);
            CreateFontPaletteTables(gvoBlockFontNarrow);

            gp14PointHumanist = LoadFontFile("FONTS\\FONT14HUMANIST.sti");
            gvo14PointHumanist = GetFontObject(gp14PointHumanist);
            CreateFontPaletteTables(gvo14PointHumanist);

            gpHugeFont = LoadFontFile("FONTS\\HUGEFONT.sti");
            gvoHugeFont = GetFontObject(gpHugeFont);
            CreateFontPaletteTables(gvoHugeFont);

            return ValueTask.FromResult(true);
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
            pObj.pShadeCurrent = pObj.pShades[(int)FONT_SHADE.NEUTRAL].Value;

            // check to make sure every table got a palette
            //for(count=0; (count < HVOBJECT_SHADE_TABLES) && (pObj.pShades[count]!=null); count++);

            // return the result of the check
            //return(count==HVOBJECT_SHADE_TABLES);
            return (true);
        }

        private HVOBJECT GetFontObject(int iFont)
        {
            return this.FontObjs[iFont];
        }

        //*****************************************************************************
        // LoadFontFile
        //
        //	Loads a font from an ETRLE file, and inserts it into one of the font slots.
        //  This function returns (-1) if it fails, and debug msgs for a reason.
        //  Otherwise the font number is returned.
        //*****************************************************************************
        private int LoadFontFile(string filename)
        {
            int LoadIndex;

            if ((LoadIndex = this.FindFreeFont()) == (-1))
            {
                //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Out of font slots (%s)", filename);
                return (-1);
            }

            if ((this.FontObjs[LoadIndex] = this.video.CreateVideoObject(filename)) == null)
            {
                //DbgMessage(TOPIC_FONT_HANDLER, DBG_LEVEL_0, String("Error creating VOBJECT (%s)", filename);

                return (-1);
            }

            if (this.FontDefault == (-1))
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
        private int FindFreeFont()
        {
            int count;

            for (count = 0; count < MAX_FONTS; count++)
            {
                if (this.FontObjs[count] == null)
                {
                    return (count);
                }
            }

            return (-1);

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
        FONT_WHITE = 208,	//lightest color
        FONT_GRAY1 = 133,
        FONT_GRAY2 = 134,	//light gray
        FONT_GRAY3 = 135,
        FONT_GRAY4 = 136,	//gray
        FONT_GRAY5 = 137,
        FONT_GRAY6 = 138,
        FONT_GRAY7 = 139,	//dark gray
        FONT_GRAY8 = 140,
        FONT_NEARBLACK = 141,
        FONT_BLACK = 0,	//darkest color
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
