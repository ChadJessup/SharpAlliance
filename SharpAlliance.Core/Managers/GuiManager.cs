using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class GuiManager : ISharpAllianceManager
    {
        public static SliderSubSystem Sliders { get; private set; }

        public bool IsInitialized { get; }

        public GuiManager(SliderSubSystem sliders)
        {
            Sliders = sliders;
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public static void RenderButtonsFastHelp()
        {
        }

        public static void RenderSliderBars()
            => Sliders.RenderSliderBars();

        public static void RenderButtons(IEnumerable<GUI_BUTTON> buttons)
            => ButtonSubSystem.RenderButtons(buttons);

        public void Dispose()
        {
        }
    }
}
