using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IInputManager : ISharpAllianceManager
    {
        void KeyboardChangeEvent(KeyEvent keyEvent);
        void MouseChangeEvent(MouseEvent mouseEvent);
    }


    public readonly struct KeyEvent
    {

    }

    public readonly struct MouseEvent
    {

    }
}
