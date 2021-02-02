using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class FontSubSystem : IDisposable
    {
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
            throw new NotImplementedException();
        }

        public void SetFontDestBuffer(Surfaces buttonDestBuffer, int y1, int y2, int width, int height, bool v)
        {
            throw new NotImplementedException();
        }

        internal int StringPixLength(string stringText, FontStyle usFont)
        {
            throw new NotImplementedException();
        }

        internal void SetFontShadow(FontColor sShadowColor)
        {
            throw new NotImplementedException();
        }

        internal void DisplayWrappedString(ushort xp, ushort yp, int sWrappedWidth, int v1, FontStyle usFont, byte sForeColor, string stringText, object fONT_MCOLOR_BLACK, bool v2, TextJustifies bJustified)
        {
            throw new NotImplementedException();
        }
    }

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
        FONT_MCOLOR_WHITE,
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
