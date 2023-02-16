﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class GuiManager : ISharpAllianceManager
    {
        public ButtonSubSystem Buttons { get; private set; }
        public SliderSubSystem Sliders { get; private set; }

        public bool IsInitialized { get; }

        public GuiManager(
            ButtonSubSystem buttons,
            SliderSubSystem sliders)
        {
            this.Buttons = buttons;
            this.Sliders = sliders;
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void RenderButtonsFastHelp()
        {
        }

        public void RenderSliderBars() => this.Sliders.RenderSliderBars();

        public void RenderButtons(IEnumerable<GUI_BUTTON> buttons) => this.Buttons.RenderButtons(buttons);

        public void Dispose()
        {
        }
    }
}