using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullInputManager : IInputManager
    {
        public bool IsInitialized { get; } = true;
        public bool gfLeftButtonState { get; set; }
        public bool gfRightButtonState { get; set; }

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

        public void GetCursorPosition(out Point mousePos)
        {
            throw new NotImplementedException();
        }

        public bool DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents mouseEvents)
        {
            throw new NotImplementedException();
        }
    }
}
