using System.Drawing;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IInputManager : ISharpAllianceManager
    {
        bool gfLeftButtonState { get; set; }
        bool gfRightButtonState { get; set; }

        void KeyboardChangeEvent(KeyEvent keyEvent);
        void MouseChangeEvent(MouseEvent mouseEvent);
        void GetCursorPosition(out Point mousePos);
        bool DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents mouseEvents);
    }

    public readonly struct KeyEvent
    {

    }

    public readonly struct MouseEvent
    {
        public MouseEvents EventType { get; init; }
        public Point Position { get; init; }
    }
}
