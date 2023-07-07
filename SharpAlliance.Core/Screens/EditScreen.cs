using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class EditScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public static bool gfProfileDataLoaded = false;

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.EDIT_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void Draw(IVideoManager videoManager)
    {
        throw new NotImplementedException();
    }

    public ValueTask Deactivate()
    {
        throw new NotImplementedException();
    }
}
