namespace SharpAlliance.Core
{

    //        public bool IsButtonDown(MouseButton button)
    //        {
    //            uint index = (uint)button;
    //            switch (index)
    //            {
    //                case 0:
    //                    return _mouseDown0;
    //                case 1:
    //                    return _mouseDown1;
    //                case 2:
    //                    return _mouseDown2;
    //                case 3:
    //                    return _mouseDown3;
    //                case 4:
    //                    return _mouseDown4;
    //                case 5:
    //                    return _mouseDown5;
    //                case 6:
    //                    return _mouseDown6;
    //                case 7:
    //                    return _mouseDown7;
    //                case 8:
    //                    return _mouseDown8;
    //                case 9:
    //                    return _mouseDown9;
    //                case 10:
    //                    return _mouseDown10;
    //                case 11:
    //                    return _mouseDown11;
    //                case 12:
    //                    return _mouseDown12;
    //            }
    //
    //            throw new ArgumentOutOfRangeException(nameof(button));
    //        }
    //    }

    public struct MouseWheelEventArgs
    {
        public MouseState State { get; }
        public float WheelDelta { get; }
        public MouseWheelEventArgs(MouseState mouseState, float wheelDelta)
        {
            State = mouseState;
            WheelDelta = wheelDelta;
        }
    }
}

