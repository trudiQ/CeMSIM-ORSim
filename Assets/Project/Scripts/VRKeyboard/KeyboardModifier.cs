using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardModifier : KeyboardKey
{
    public ModifierType modifierType;
    public Text label;
    public Color activeColor = Color.cyan;

    private bool active = false;
    private Color baseColor;

    void Start()
    {
        baseColor = label.color;
    }

    public override void ModifierToggled(ModifierType type)
    {
        switch (type)
        {
            case ModifierType.Shift:
                Shift();
                break;
            case ModifierType.Caps:
                Caps();
                break;
            case ModifierType.Ctrl:
                Ctrl();
                break;
            case ModifierType.Alt:
                Alt();
                break;
            default:
                break;
        }
    }

    protected override void Shift()
    {
        if (modifierType == ModifierType.Shift)
        {
            if (active = shiftModifierActive)
                label.color = activeColor;
            else
                label.color = baseColor;
        }
    }

    protected override void Caps()
    {
        // Only one caps button
    }

    protected override void Ctrl()
    {
        if (modifierType == ModifierType.Ctrl)
        {
            if (active = ctrlModifierActive)
                label.color = activeColor;
            else
                label.color = baseColor;
        }
    }

    protected override void Alt()
    {
        if (modifierType == ModifierType.Alt)
        {
            if (active = altModifierActive)
                label.color = activeColor;
            else
                label.color = baseColor;
        }
    }

    public override string GetText()
    {
        return modifierType.ToString();
    }

    public enum ModifierType { Shift, Caps, Ctrl, Alt, Tab, Enter, Backspace } // Modifiers available limited to this list
}
