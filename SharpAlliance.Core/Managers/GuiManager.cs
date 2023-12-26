using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpAlliance.Core.Managers;

public class GuiManager : ISharpAllianceManager
{
    public SliderSubSystem Sliders { get; private set; }

    public bool IsInitialized { get; }

    public GuiManager(SliderSubSystem sliders)
    {
        this.Sliders = sliders;
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public static void RenderButtonsFastHelp()
    {
    }

    public void RenderAllSliderBars()
        => this.Sliders.RenderAllSliderBars();

    public void RenderButtons(IEnumerable<GUI_BUTTON> buttons)
        => ButtonSubSystem.RenderButtons(buttons);

    public void Dispose()
    {
    }
}
