using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public FADE_HOOK gFadeInDoneCallback { get; set; } = default!;
    public FADE_HOOK gFadeOutDoneCallback { get; set; } = default!;
    public FADE_FUNCTION gFadeFunction { get; set; } = default!;

    public bool gfFadeInitialized;
    public bool gfFadeIn;
    public bool gfFadeInVideo;
    public int gbFadeType;

    public void Draw(ITextureManager textureManager)
    {
    }

    public bool HandleBeginFadeIn(ScreenName gubGIOExitScreen)
    {
        return true;
    }

    public bool HandleFadeInCallback()
    {
        return true;
    }

    public bool HandleBeginFadeOut(ScreenName gubGIOExitScreen)
    {
        return true;
    }

    public bool HandleFadeOutCallback()
    {
        return true;
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
}
