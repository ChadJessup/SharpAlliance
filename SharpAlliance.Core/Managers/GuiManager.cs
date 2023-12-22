using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class GuiManager : ISharpAllianceManager
{
    public SliderSubSystem Sliders { get; private set; }

    public bool IsInitialized { get; }

    public GuiManager(SliderSubSystem sliders)
    {
        Sliders = sliders;
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void RenderButtonsFastHelp()
    {
    }

    public void RenderSliderBars()
        => Sliders.RenderSliderBars();

    public void RenderButtons(IEnumerable<GUI_BUTTON> buttons)
        => ButtonSubSystem.RenderButtons(buttons);

    public void Dispose()
    {
    }
}
