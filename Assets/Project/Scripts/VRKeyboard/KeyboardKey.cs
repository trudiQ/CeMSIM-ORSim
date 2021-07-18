﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// The base class for a keyboard key, overrides needed for functionality
public abstract class KeyboardKey : MonoBehaviour
{
    public UnityEvent<KeyboardKey> OnKeyPressed;

    // Modifiers active (these will be the same across all keys)
    protected static bool shiftModifierActive = false;
    protected static bool capsModifierActive = false;
    protected static bool ctrlModifierActive = false;
    protected static bool altModifierActive = false;

    // Messages sent when the controller receives a modifier press
    public abstract void ModifierToggled(KeyboardModifier.ModifierType type);
    protected abstract void Shift();
    protected abstract void Caps();
    protected abstract void Ctrl();
    protected abstract void Alt();

    public static void ToggleShiftState() { shiftModifierActive = !shiftModifierActive; }
    public static void ToggleCapsState() { capsModifierActive = !capsModifierActive; }
    public static void ToggleCtrlState() { ctrlModifierActive = !ctrlModifierActive; }
    public static void ToggleAltState() { altModifierActive = !altModifierActive; }
    
    // General methods
    public abstract string GetText();
    public void Pressed() { OnKeyPressed.Invoke(this); }
}
