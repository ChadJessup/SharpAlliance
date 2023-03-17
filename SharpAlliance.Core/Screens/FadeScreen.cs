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

public class FadeScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }
    public bool gfFadeInitialized;
    public bool gfFadeIn;
    public bool gfFadeInVideo;
    public int gbFadeType;
    public Action gFadeFunction;

    public ValueTask Activate()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask<ScreenName> Handle()
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> Initialize()
    {
        throw new NotImplementedException();
    }

    public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
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

    public ValueTask Deactivate()
    {
        throw new NotImplementedException();
    }
}
