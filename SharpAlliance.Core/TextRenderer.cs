using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using Veldrid;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Drawing.Processing;
using FontStyle = SixLabors.Fonts.FontStyle;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Interfaces;
using Veldrid.Sdl2;
using SixLabors.ImageSharp.Drawing;

namespace SharpAlliance.Core;

public class TextRenderer
{
    private Rgba32 White = Rgba32.ParseHex("FFFFFF");
    private readonly IVideoManager video;
    private readonly Font font;
    private readonly Image<Rgba32> _image;
    private readonly FontSubSystem fonts;

    public TextRenderer(FontSubSystem fontSubsystem, IVideoManager videoManager)
    {
        this.fonts = fontSubsystem;
        int width = 640;
        int height = 480;

        this.video = videoManager;
        this.font = this.LoadFont("Arial", 10, FontStyle.Bold);
    }

    public Font LoadFont(string fontFamily, int size, FontStyle style)
    {
        var allFonts = SystemFonts.Collection;
        var families = SystemFonts.Get(fontFamily);
        return families.CreateFont(size, style);
    }

    public unsafe void ClearText()
    {
        this._image.DangerousTryGetSinglePixelMemory(out var span);
        fixed (void* data = &MemoryMarshal.GetReference<Rgba32>(span.Span))
        {
            Unsafe.InitBlock(data, 0, (uint)(this._image.Width * this._image.Height * 4));
        }
    }

    public void DrawText(
        string text,
        PointF location,
        int width,
        TextAlignment alignment,
        Font font,
        Rgba32 foreground,
        Rgba32 background)
    {
        var buffer = this.video.Surfaces[FontSubSystem.FontDestBuffer];

        RichTextOptions options = new(font)
        {
            Origin = location,
            //HorizontalAlignment = width == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            TextAlignment = alignment,
            TextDirection = TextDirection.LeftToRight,
            VerticalAlignment = VerticalAlignment.Top,
            WrappingLength = width == 0 ? -1 : width,
            //TextJustification = TextJustification.InterWord,
            //LayoutMode = LayoutMode.HorizontalTopBottom,
        };

        var foreColor = new Color(foreground);
        var backColor = new Color(background);
        Brush brush = Brushes.Solid(foreColor);//, backColor);
        Pen pen = Pens.Solid(backColor, 0.1f);

        buffer.Mutate(ctx =>
        {
            ctx.DrawText(
                options,
                text,
                brush,
                pen);
        });
    }
}
