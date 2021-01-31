using System;
using System.Collections.Generic;
using Veldrid;
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
            mousePos = new Point();
        }

        public bool DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents mouseEvents)
        {
            inputAtom = new InputAtom();
            return true;
        }

        public void ProcessEvents()
        {
        }

        public void DequeueAllKeyBoardEvents()
        {
            throw new NotImplementedException();
        }

        public bool DequeueEvent(out InputAtom? inputAtom)
        {
            throw new NotImplementedException();
        }
    }
}
