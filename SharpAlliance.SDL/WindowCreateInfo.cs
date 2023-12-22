namespace SharpAlliance.Core.Managers;

// Following is from the Veldrid project.
public struct WindowCreateInfo
{
    public int X;
    public int Y;
    public int WindowWidth;
    public int WindowHeight;
    public WindowState WindowInitialState;
    public string WindowTitle;

    public WindowCreateInfo(
        int x,
        int y,
        int windowWidth,
        int windowHeight,
        WindowState windowInitialState,
        string windowTitle)
    {
        this.X = x;
        this.Y = y;
        this.WindowWidth = windowWidth;
        this.WindowHeight = windowHeight;
        this.WindowInitialState = windowInitialState;
        this.WindowTitle = windowTitle;
    }
}
