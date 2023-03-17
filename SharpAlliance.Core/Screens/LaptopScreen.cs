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

public class LaptopScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.LAPTOP_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
    {
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }
}
