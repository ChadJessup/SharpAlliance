using SharpAlliance.Core;
using SharpAlliance.Platform;
using Point = SixLabors.ImageSharp.Point;

using static SharpAlliance.Core.Globals;
namespace SharpAlliance.Core.Interfaces;

public interface IInputManager : ISharpAllianceManager
{
    MouseSubSystem Mouse { get; init; }
    ButtonSubSystem buttonSystem { get; init; }

    Point gusMousePos { get; set; }
    bool gfLeftButtonState { get; set; }
    bool gfRightButtonState { get; set; }
    bool DequeueEvent(out IInputSnapshot? inputSnapshot);
    void KeyboardChangeEvent(KeyEvent keyEvent);
    void MouseChangeEvent(MouseEvent mouseEvent);
    void GetCursorPosition(out Point mousePos);
    bool DequeSpecificEvent(out IInputSnapshot inputSnapshot);
    MouseEvents ConvertToMouseEvents(ref IInputSnapshot inputSnapshot);
    void ProcessEvents();
    void DequeueAllKeyBoardEvents();
    void GetMousePos(out Point pPosition);
    bool HandleTextInput(IInputSnapshot @event);
}
