using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// The base class for a keyboard key, overrides needed for functionality
public abstract class KeyboardKey : MonoBehaviour
{
    public UnityEvent<KeyboardKey> OnKeyPressed;

    // Modifiers active
    protected bool shiftModifierActive = false;
    protected bool capsModifierActive = false;
    protected bool ctrlModifierActive = false;

    // Messages sent when the controller receives a modifier press
    public abstract void Shift(bool state);
    public abstract void Caps(bool state);
    public abstract void Ctrl(bool state);

    // General methods
    public abstract string GetText();
    public abstract void Pressed();
}
