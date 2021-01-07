using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class HelpScreen : IScreen
    {
        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
        }

        public ValueTask<IScreen> Handle()
        {
            return ValueTask.FromResult<IScreen>(this);
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }
    }
}
