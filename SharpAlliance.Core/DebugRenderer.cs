using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core
{
    public class DebugRenderer : SpriteRenderer
    {
        public DebugRenderer(GraphicsDevice gd)
            : base(gd)
        {
        }

        public override void Draw(GraphicsDevice gd, CommandList cl, bool clearCalls = true)
        {
            if (this.DrawCalls.Count > 0)
            {

            }

            base.Draw(gd, cl, clearCalls);
        }

        public void DrawRectangle(Rectangle regionRect, Color color)
        {
            var lineWidth = 2;
            regionRect.Inflate(lineWidth, lineWidth);

            var rectangle = new Image<Rgba32>(regionRect.Width, regionRect.Height);
            var newRect = new Rectangle(0, 0, regionRect.Width, regionRect.Height);

            rectangle.Mutate(ctx =>
            {
                ctx.Draw(color, lineWidth, newRect);
            });

            this.AddSprite(
                regionRect,
                texture: new ImageSharpTexture(rectangle).CreateDeviceTexture(this.gd, this.gd.ResourceFactory),
                regionRect.GetHashCode().ToString());
        }
    }
}
