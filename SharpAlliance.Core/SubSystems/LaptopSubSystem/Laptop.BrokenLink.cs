using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of BrokenLink.c: the "Error 404" page shown for links that go nowhere.
public partial class Laptop
{
    private const FontStyle BROKEN_LINK__FONT = FontStyle.FONT12ARIAL;
    private const FontColor BROKEN_LINK__COLOR = FontColor.FONT_MCOLOR_BLACK;

    private const int BROKEN_LINK__MESSAGE_X = LAPTOP_SCREEN_UL_X + 20;
    private const int BROKEN_LINK__MESSAGE_Y = LAPTOP_SCREEN_UL_Y + 50;
    private const int BROKEN_LINK__MESSAGE_WIDTH = LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X;

    private const int BROKEN_LINK__SITE_NOT_FOUND_Y = LAPTOP_SCREEN_UL_Y + 65;

    private static bool EnterBrokenLink()
    {
        //  RenderBrokenLink();
        return true;
    }

    private static void ExitBrokenLink()
    {
    }

    private static void HandleBrokenLink()
    {
    }

    private static void RenderBrokenLink()
    {
        //Color fill the laptop white
        DrawBrokenLinkWhiteBackground();

        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        //Put up a message saying the link is dead
        FontSubSystem.DisplayWrappedString(
            new(BROKEN_LINK__MESSAGE_X, BROKEN_LINK__MESSAGE_Y),
            BROKEN_LINK__MESSAGE_WIDTH,
            2,
            BROKEN_LINK__FONT,
            BROKEN_LINK__COLOR,
            BrokenLinkText[(int)BROKEN_LINK_TXT.ERROR_404],
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        //Put up a message saying the site was not found
        FontSubSystem.DisplayWrappedString(
            new(BROKEN_LINK__MESSAGE_X, BROKEN_LINK__SITE_NOT_FOUND_Y),
            BROKEN_LINK__MESSAGE_WIDTH,
            2,
            BROKEN_LINK__FONT,
            BROKEN_LINK__COLOR,
            BrokenLinkText[(int)BROKEN_LINK_TXT.SITE_NOT_FOUND],
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        video.InvalidateRegion(new(0, 0, 640, 480));
    }

    private static void DrawBrokenLinkWhiteBackground()
    {
        video.ColorFillVideoSurfaceArea(
            SurfaceType.FRAME_BUFFER,
            new Rectangle(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_LR_Y - LAPTOP_SCREEN_WEB_UL_Y),
            Color.White.ToPixel<Rgba32>());
    }
}

public enum BROKEN_LINK_TXT
{
    ERROR_404,
    SITE_NOT_FOUND,
}
