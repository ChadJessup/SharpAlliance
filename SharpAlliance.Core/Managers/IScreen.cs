using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Screens;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public interface IScreen : IDisposable
{
    ValueTask<bool> Initialize();
    bool IsInitialized { get; set; }

    // Activation is to let the screen know it will be used soon.
    // This is different than Initialization, in the sense that this might be called multiple times.
    ValueTask Activate();

    // Deactivate is called when a screen is about to be changed. This allows a screen to stop drawing, etc.
    ValueTask Deactivate();

    ValueTask<ScreenName> Handle();
    void Draw(IVideoManager videoManager);
    ScreenState State { get; set; }
}
