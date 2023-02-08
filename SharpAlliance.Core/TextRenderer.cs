using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using Veldrid;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SharpAlliance.Core
{
    public class TextRenderer
    {
        private Rgba32 White = Rgba32.ParseHex("FFFFFF");
        private readonly GraphicsDevice _gd;
        private readonly Texture _texture;

        public TextureView TextureView { get; }

        private readonly Font _font;
        private readonly Image<Rgba32> _image;

        public TextRenderer(GraphicsDevice gd)
        {
            this._gd = gd;
            int width = 640;
            int height = 480;
            this._texture = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));

            this.TextureView = gd.ResourceFactory.CreateTextureView(this._texture);

            this._font = this.LoadFont("Arial", 10, FontStyle.Bold);

            this._image = new Image<Rgba32>(width, height);
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

        public void DrawText(string text, int x, int y, int width, HorizontalAlignment alignment, Font font, Rgba32 foreground, Rgba32 background)
        {
            if (text == "Save Game")
            {

            }

            this._image.Mutate(ctx =>
            {
                ctx.DrawText(
                    text,
                    font,
                    foreground,
                    new PointF(x, y));
            });
        }

        public unsafe void RenderAllText()
        {
            this._image.DangerousTryGetSinglePixelMemory(out var span2);
            fixed (void* data = &MemoryMarshal.GetReference(span2.Span))
            {
                uint size = (uint)(this._image.Width * this._image.Height * 4);

                try
                {
                    this._gd.UpdateTexture(
                        this._texture,
                        (IntPtr)data,
                        size,
                        x: 0,
                        y: 0,
                        z: 0,
                        this._texture.Width,
                        this._texture.Height,
                        depth: 1,
                        mipLevel: 0,
                        arrayLayer: 0);
                }
                catch (VeldridException e)
                {

                }
            }

        }
    }
}
