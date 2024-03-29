﻿using System.Collections.Generic;
using System.Numerics;

namespace SharpAlliance.Core;

public static class InputTracker
{
    private static HashSet<Key> _currentlyPressedKeys = new();
    private static HashSet<Key> _newKeysThisFrame = new();

    private static HashSet<MouseButton> _currentlyPressedMouseButtons = new();
    private static HashSet<MouseButton> _newMouseButtonsThisFrame = new();

    public static Vector2 MousePosition;
    public static Vector2 MouseDelta;
    public static IInputSnapshot? FrameSnapshot { get; private set; }

    public static bool GetKey(Key key)
    {
        return _currentlyPressedKeys.Contains(key);
    }

    public static bool GetKeyDown(Key key)
    {
        return _newKeysThisFrame.Contains(key);
    }

    public static bool GetMouseButton(MouseButton button)
    {
        return _currentlyPressedMouseButtons.Contains(button);
    }

    public static bool GetMouseButtonDown(MouseButton button)
    {
        return _newMouseButtonsThisFrame.Contains(button);
    }

    public static void UpdateFrameInput(IInputSnapshot snapshot)
    {
        FrameSnapshot = snapshot;
        _newKeysThisFrame.Clear();
        _newMouseButtonsThisFrame.Clear();

        MousePosition = snapshot.MousePosition;
        //MouseDelta = window.MouseDelta;
        for (int i = 0; i < snapshot.KeyEvents.Count; i++)
        {
            KeyEvent ke = snapshot.KeyEvents[i];
            if (ke.Down)
            {
                KeyDown(ke.Key);
            }
            else
            {
                KeyUp(ke.Key);
            }
        }

        for (int i = 0; i < snapshot.MouseEvents.Count; i++)
        {
            MouseEvent me = snapshot.MouseEvents[i];
            if (me.Down)
            {
                MouseDown(me.MouseButton);
            }
            else
            {
                MouseUp(me.MouseButton);
            }
        }
    }

    private static void MouseUp(MouseButton mouseButton)
    {
        _currentlyPressedMouseButtons.Remove(mouseButton);
        _newMouseButtonsThisFrame.Remove(mouseButton);
    }

    private static void MouseDown(MouseButton mouseButton)
    {
        if (_currentlyPressedMouseButtons.Add(mouseButton))
        {
            _newMouseButtonsThisFrame.Add(mouseButton);
        }
    }

    private static void KeyUp(Key key)
    {
        _currentlyPressedKeys.Remove(key);
        _newKeysThisFrame.Remove(key);
    }

    private static void KeyDown(Key key)
    {
        if (_currentlyPressedKeys.Add(key))
        {
            _newKeysThisFrame.Add(key);
        }
    }
}
