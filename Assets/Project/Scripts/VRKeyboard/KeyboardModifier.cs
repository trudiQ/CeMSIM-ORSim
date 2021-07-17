using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardModifier : KeyboardKey
{
    public ModifierType modifierType;
    public Text label;

    private bool active = false;

    void Start()
    {
        
    }

    public override void Shift(bool state)
    {
        shiftModifierActive = state;
    }

    public override void Caps(bool state)
    {
        capsModifierActive = state;
    }

    public override void Ctrl(bool state)
    {
        ctrlModifierActive = state;
    }

    public override string GetText()
    {
        return modifierType.ToString();
    }

    public override void Pressed()
    {
        active = !active;
        OnKeyPressed.Invoke(this);
    }

    public enum ModifierType { Shift, Caps, Ctrl, Alt, Tab, Enter } // Modifiers available limited to this
}
