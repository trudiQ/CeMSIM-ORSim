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

    void Start()
    {
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

    public override string GetText()
    {
        return (shiftModifierActive ^ capsModifierActive ? uppercaseCharacter : lowercaseCharacter).ToString();
    }
}
