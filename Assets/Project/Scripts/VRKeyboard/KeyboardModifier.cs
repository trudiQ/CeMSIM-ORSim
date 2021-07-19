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
            default:
                break;
        }
    }

    protected override void Shift()
    {
        // Toggle the color of the key
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

    public override string GetText()
    {
        return modifierType.ToString();
    }

    public enum ModifierType { Shift, Caps, Tab, Clear, Backspace, Paste, Next, Previous } // Modifiers available limited to this list
}
