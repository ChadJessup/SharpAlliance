using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullInputManager : IInputManager
    {
        public bool IsInitialized { get; } = true;

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        public void KeyboardChangeEvent(KeyEvent keyEvent)
        {
        }

        public void MouseChangeEvent(MouseEvent mouseEvent)
        {
        }
    }
}
