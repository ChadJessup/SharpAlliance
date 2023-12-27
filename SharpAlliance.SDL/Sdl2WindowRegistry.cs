using System.Runtime.CompilerServices;
using SDL2;

namespace SharpAlliance.Core;

internal static class Sdl2WindowRegistry
{
    public static readonly object Lock = new object();
    private static readonly Dictionary<uint, Sdl2Window> _eventsByWindowID
        = new Dictionary<uint, Sdl2Window>();
    private static bool _firstInit;

    public static void RegisterWindow(Sdl2Window window)
    {
        lock (Lock)
        {
            _eventsByWindowID.Add(window.WindowID, window);
            if (!_firstInit)
            {
                _firstInit = true;
                Sdl2Events.Subscribe(ProcessWindowEvent);
            }
        }
    }

    public static void RemoveWindow(Sdl2Window window)
    {
        lock (Lock)
        {
            _eventsByWindowID.Remove(window.WindowID);
        }
    }

    private static unsafe void ProcessWindowEvent(ref SDL.SDL_Event ev)
    {
        bool handled = false;
        uint windowID = 0;
        switch (ev.type)
        {
            case SDL.SDL_EventType.SDL_KEYDOWN:
                windowID = ev.window.windowID;
                handled = true;
                break;
            case SDL.SDL_EventType.SDL_QUIT:
            case SDL.SDL_EventType.SDL_APP_TERMINATING:
            case SDL.SDL_EventType.SDL_WINDOWEVENT:
            case SDL.SDL_EventType.SDL_KEYUP:
            case SDL.SDL_EventType.SDL_TEXTEDITING:
            case SDL.SDL_EventType.SDL_TEXTINPUT:
            case SDL.SDL_EventType.SDL_KEYMAPCHANGED:
            case SDL.SDL_EventType.SDL_MOUSEMOTION:
            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
            case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                windowID = ev.window.windowID;
                handled = true;
                break;
            case SDL.SDL_EventType.SDL_DROPBEGIN:
            case SDL.SDL_EventType.SDL_DROPCOMPLETE:
            case SDL.SDL_EventType.SDL_DROPFILE:
            case SDL.SDL_EventType.SDL_DROPTEXT:
                SDL.SDL_DropEvent dropEvent = Unsafe.As<SDL.SDL_Event, SDL.SDL_DropEvent>(ref ev);
                windowID = dropEvent.windowID;
                handled = true;
                break;
            default:
                handled = false;
                break;
        }

        if (handled && _eventsByWindowID.TryGetValue(windowID, out Sdl2Window window))
        {
            window.AddEvent(ev);
        }
    }
}
