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

public class HelpScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Deactivate()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public void Draw(ITextureManager textureManager)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.HelpScreen);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }
}
