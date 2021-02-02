using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

namespace SharpAlliance.Core.Interfaces
{
    public interface IInputManager : ISharpAllianceManager
    {
        MouseSubSystem mouseSystem { get; init; }
        ButtonSubSystem buttonSystem { get; init; }

        bool gfLeftButtonState { get; set; }
        bool gfRightButtonState { get; set; }
        bool DequeueEvent(out InputAtom? inputAtom);
        void KeyboardChangeEvent(KeyEvent keyEvent);
        void MouseChangeEvent(MouseEvent mouseEvent);
        void GetCursorPosition(out Point mousePos);
        bool DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents mouseEvents);
        void ProcessEvents();
        void DequeueAllKeyBoardEvents();
    }

    //public readonly struct KeyEvent
    //{

    //}

    //public readonly struct MouseEvent
    //{
    //    public MouseEvents EventType { get; init; }
    //    public Point Position { get; init; }
    //}
}
