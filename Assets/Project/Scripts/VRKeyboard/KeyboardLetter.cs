using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class KeyboardLetter : KeyboardKey
{
    public char lowercaseCharacter;
    public char uppercaseCharacter;

    public Text lowercaseText;
    public Text uppercaseText;

    public bool isCtrlHotkey;
    public bool isAltHotkey;
    public bool isComboHotkey;
    public Color ctrlColor = Color.green;
    public Color altColor = Color.red;
    public Color comboColor = Color.magenta;
    private Color baseColor;

    void Start()
    {
        baseColor = lowercaseText.color;

        lowercaseText.text = lowercaseCharacter.ToString();
        uppercaseText.text = uppercaseCharacter.ToString();

        lowercaseText.gameObject.SetActive(true);
        uppercaseText.gameObject.SetActive(false);
    }

    public override void ModifierToggled(KeyboardModifier.ModifierType type)
    {
        switch (type)
        {
            case KeyboardModifier.ModifierType.Shift:
                Shift();
                break;
            case KeyboardModifier.ModifierType.Caps:
                Caps();
                break;
            case KeyboardModifier.ModifierType.Ctrl:
                Ctrl();
                break;
            case KeyboardModifier.ModifierType.Alt:
                Alt();
                break;
            default:
                break;
        }
    }

    protected override void Shift()
    {
        lowercaseText.gameObject.SetActive(!(shiftModifierActive ^ capsModifierActive));
        uppercaseText.gameObject.SetActive(shiftModifierActive ^ capsModifierActive);
    }

    protected override void Caps()
    {
        lowercaseText.gameObject.SetActive(!(shiftModifierActive ^ capsModifierActive));
        uppercaseText.gameObject.SetActive(shiftModifierActive ^ capsModifierActive);
    }

    protected override void Ctrl()
    {
        bool useAltColor = isAltHotkey && altModifierActive && !ctrlModifierActive;
        bool useCtrlColor = isCtrlHotkey && ctrlModifierActive && !altModifierActive;
        bool useComboColor = isComboHotkey && altModifierActive && ctrlModifierActive;
        
        Color newColor = useComboColor ? comboColor :          // If it is a combo hotkey, use combo color
                        (useCtrlColor ? ctrlColor :            // If it is a ctrl hotkey and alt isnt active, use ctrl color
                        (useAltColor ? altColor : baseColor)); // If it is an alt hotkey and ctrl isnt active, use combo color, otherwise use base

        if (capsModifierActive)
        {
            lowercaseText.color = newColor;
            uppercaseText.color = newColor;
        }
        else
        {
            lowercaseText.color = newColor;
            uppercaseText.color = newColor;
        }
    }

    protected override void Alt()
    {
        bool useAltColor = isAltHotkey && altModifierActive && !ctrlModifierActive;
        bool useCtrlColor = isCtrlHotkey && ctrlModifierActive && !altModifierActive;
        bool useComboColor = isComboHotkey && altModifierActive && ctrlModifierActive;

        Color newColor = useComboColor ? comboColor :          // If it is a combo hotkey, use combo color
                        (useCtrlColor ? ctrlColor :            // If it is a ctrl hotkey and alt isnt active, use ctrl color
                        (useAltColor ? altColor : baseColor)); // If it is an alt hotkey and ctrl isnt active, use combo color, otherwise use base

        if (altModifierActive)
        {
            lowercaseText.color = newColor;
            uppercaseText.color = newColor;
        }
        else
        {
            lowercaseText.color = newColor;
            uppercaseText.color = newColor;
        }
    }

    public override string GetText()
    {
        return (shiftModifierActive ^ capsModifierActive ? uppercaseCharacter : lowercaseCharacter).ToString();
    }
}
