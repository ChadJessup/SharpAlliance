using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class InitScreen : IScreen
    {
        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> Handle()
        {
            return ValueTask.FromResult(0);
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

    }
}
