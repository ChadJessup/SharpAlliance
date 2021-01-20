using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class FontSubSystem : IDisposable
    {
        public void Dispose()
        {
        }

        internal void SetFont(object tINYFONT1)
        {
            throw new NotImplementedException();
        }

        internal void SetFontBackground(object fONT_MCOLOR_BLACK)
        {
            throw new NotImplementedException();
        }

        internal void SetFontForeground(object fONT_MCOLOR_WHITE)
        {
            throw new NotImplementedException();
        }
    }

    public enum FontStyle
    {
        TINYFONT1
    }

    public enum FontColor
    {
        FONT_MCOLOR_WHITE,
        FONT_MCOLOR_BLACK
    }
}
