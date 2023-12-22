namespace SharpAlliance.Core;

public partial struct MouseState
{
    public readonly int X;
    public readonly int Y;

    private bool _mouseDown0;
    private bool _mouseDown1;
    private bool _mouseDown2;
    private bool _mouseDown3;
    private bool _mouseDown4;
    private bool _mouseDown5;
    private bool _mouseDown6;
    private bool _mouseDown7;
    private bool _mouseDown8;
    private bool _mouseDown9;
    private bool _mouseDown10;
    private bool _mouseDown11;
    private bool _mouseDown12;

    public MouseState(
        int x, int y,
        bool mouse0, bool mouse1, bool mouse2, bool mouse3, bool mouse4, bool mouse5, bool mouse6,
        bool mouse7, bool mouse8, bool mouse9, bool mouse10, bool mouse11, bool mouse12)
    {
        this.X = x;
        this.Y = y;
        this._mouseDown0 = mouse0;
        this._mouseDown1 = mouse1;
        this._mouseDown2 = mouse2;
        this._mouseDown3 = mouse3;
        this._mouseDown4 = mouse4;
        this._mouseDown5 = mouse5;
        this._mouseDown6 = mouse6;
        this._mouseDown7 = mouse7;
        this._mouseDown8 = mouse8;
        this._mouseDown9 = mouse9;
        this._mouseDown10 = mouse10;
        this._mouseDown11 = mouse11;
        this._mouseDown12 = mouse12;
    }
}
