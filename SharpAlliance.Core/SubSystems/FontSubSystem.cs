using System;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems
{
    public class FontSubSystem : IDisposable
    {
        public const int INVALIDATE_TEXT = 0x00000010;

        //Wont display the text.  Used if you just want to get how many lines will be displayed
        public const int DONT_DISPLAY_TEXT = 0x00000020;

        public void Dispose()
        {
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

        public void SetFontShadow(FontColor sShadowColor)
        {
        }

        public int DisplayWrappedString(
            Point pos,
            int sWrappedWidth,
            int v1,
            FontStyle usFont,
            FontColor sForeColor,
            string stringText,
            FontColor fONT_MCOLOR_BLACK,
            bool v2,
            TextJustifies bJustified)
        {
            return 2;
        }

        public void DrawTextToScreen(string v1, Point pos, int v2, FontStyle oPT_MAIN_FONT, FontColor oPT_MAIN_COLOR, FontColor fONT_MCOLOR_BLACK, bool v3, TextJustifies lEFT_JUSTIFIED)
        {
        }
    }

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
        FONT_MCOLOR_BLACK
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
