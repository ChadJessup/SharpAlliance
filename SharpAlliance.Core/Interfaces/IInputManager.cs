using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;
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
    bool DequeueEvent(out InputSnapshot? inputSnapshot);
    void KeyboardChangeEvent(KeyEvent keyEvent);
    void MouseChangeEvent(MouseEvent mouseEvent);
    void GetCursorPosition(out Point mousePos);
    bool DequeSpecificEvent(out InputSnapshot inputSnapshot);
    MouseEvents ConvertToMouseEvents(ref InputSnapshot inputSnapshot);
    void ProcessEvents();
    void DequeueAllKeyBoardEvents();
    void GetMousePos(out Point pPosition);
}
