using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public delegate void FADE_HOOK();
public delegate void FADE_FUNCTION();

public class FadeScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public FADE_HOOK? gFadeInDoneCallback { get; set; } = default!;
    public FADE_HOOK? gFadeOutDoneCallback { get; set; } = default!;
    public FADE_FUNCTION gFadeFunction { get; set; } = default!;

    public bool gfFadeInitialized;
    public bool gfFadeIn;
    public bool gfFadeInVideo;
    public FADE gbFadeType;

    public void Draw(IVideoManager videoManager)
    {
    }

    public bool HandleBeginFadeIn(ScreenName gubGIOExitScreen)
    {
        return true;
    }

    public bool HandleFadeInCallback()
    {
        if (gfFadeInDone)
        {
            gfFadeInDone = false;

            if (gFadeInDoneCallback != null)
            {
                gFadeInDoneCallback();
            }

            gFadeInDoneCallback = null;

            return (true);
        }

        return (false);

    }

    public bool HandleBeginFadeOut(ScreenName uiScreenExit)
    {
        if (gfFadeOut)
        {
            BeginFade(uiScreenExit, 35, FADE.OUT_REALFADE, 5);

            gfFadeOut = false;

            gfFadeOutDone = true;

            return (true);
        }

        return (false);
    }

    public bool HandleFadeOutCallback()
    {
        if (gfFadeOutDone)
        {
            gfFadeOutDone = false;

            if (gFadeOutDoneCallback != null)
            {
                gFadeOutDoneCallback();

                gFadeOutDoneCallback = null;

                return (true);
            }
        }

        return (false);

    }

    public void FadeOutNextFrame()
    {
        gfFadeOut = true;
        gfFadeOutDone = false;
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private void BeginFade(ScreenName uiExitScreen, byte bFadeValue, FADE bType, int uiDelay)
    {
        //Init some paramters
        guiExitScreen = uiExitScreen;
//        gbFadeValue = bFadeValue;
//        guiFadeDelay = uiDelay;
//        gfFadeIn = false;
//        gfFadeInVideo = true;
//
//        uiOldMusicMode = uiMusicHandle;
//
//
//        // Calculate step;
//        switch (bType)
//        {
//            case FADE.IN_REALFADE:
//
//                gsFadeRealCount = -1;
//                gsFadeLimit = 8;
//                gFadeFunction = (FADE_FUNCTION)FadeInFrameBufferRealFade;
//                gfFadeInVideo = false;
//
//                // Copy backbuffer to savebuffer
//                UpdateSaveBufferWithBackbuffer();
//
//                // Clear framebuffer
//                ColorFillVideoSurfaceArea(FRAME_BUFFER, 0, 0, 640, 480, Get16BPPColor(FROMRGB(0, 0, 0)));
//                break;
//
//            case FADE.OUT_REALFADE:
//
//                gsFadeRealCount = -1;
//                gsFadeLimit = 10;
//                gFadeFunction = (FADE_FUNCTION)FadeFrameBufferRealFade;
//                gfFadeInVideo = false;
//
//                // Clear framebuffer
//                //ColorFillVideoSurfaceArea( FRAME_BUFFER, 0, 0, 640, 480, Get16BPPColor( FROMRGB( 0, 0, 0 ) ) );
//                break;
//
//            case FADE.OUT_VERSION_ONE:
//                //gsFadeLimit = 255 / bFadeValue;
//                //gFadeFunction = (FADE_FUNCTION)FadeFrameBufferVersionOne;
//                //SetMusicFadeSpeed( 25 );
//                //SetMusicMode( MUSIC_NONE );
//                break;
//
//            case FADE.OUT_SQUARE:
//                gsFadeLimit = (640 / (SQUARE_STEP * 2));
//                giX1 = 0;
//                giX2 = 640;
//                giY1 = 0;
//                giY2 = 480;
//                gFadeFunction = (FADE_FUNCTION)FadeFrameBufferSquare;
//
//                // Zero frame buffer
//                ColorFillVideoSurfaceArea(FRAME_BUFFER, 0, 0, 640, 480, Get16BPPColor(FROMRGB(0, 0, 0)));
//                //ColorFillVideoSurfaceArea( guiSAVEBUFFER, 0, 0, 640,	480, Get16BPPColor( FROMRGB( 0, 0, 0 ) ) );
//
//                //	SetMusicFadeSpeed( 25 );
//                //SetMusicMode( MUSIC_NONE );
//                break;
//
//            case FADE.IN_VERSION_ONE:
//                gsFadeLimit = 255 / bFadeValue;
//                gFadeFunction = (FADE_FUNCTION)FadeInBackBufferVersionOne;
//                break;
//
//            case FADE.IN_SQUARE:
//                gFadeFunction = (FADE_FUNCTION)FadeInBackBufferSquare;
//                giX1 = 320;
//                giX2 = 320;
//                giY1 = 240;
//                giY2 = 240;
//                gsFadeLimit = (640 / (SQUARE_STEP * 2));
//                gfFadeIn = true;
//                break;
//
//            case FADE.OUT_VERSION_FASTER:
//                gsFadeLimit = (255 / bFadeValue) * 2;
//                gFadeFunction = (FADE_FUNCTION)FadeFrameBufferVersionFaster;
//
//                //SetMusicFadeSpeed( 25 );
//                //SetMusicMode( MUSIC_NONE );
//                break;
//
//            case FADE.OUT_VERSION_SIDE:
//                // Copy frame buffer to save buffer
//                gsFadeLimit = (640 / 8);
//                gFadeFunction = (FADE_FUNCTION)FadeFrameBufferSide;
//
//                //SetMusicFadeSpeed( 25 );
//                //SetMusicMode( MUSIC_NONE );
//                break;
//
//        }
//
//        gfFadeInitialized = true;
//        gfFirstTimeInFade = true;
//        gsFadeCount = 0;
//        gbFadeType = bType;
//
//        SetPendingNewScreen(ScreenName.FADE_SCREEN);
//
    }
}

public enum FADE
{
    OUT_VERSION_ONE = 1,
    OUT_VERSION_FASTER = 2,
    OUT_VERSION_SIDE = 3,
    OUT_SQUARE = 4,
    OUT_REALFADE = 5,

    IN_VERSION_ONE = 10,
    IN_SQUARE = 11,
    IN_REALFADE = 12,
}
