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
        lowercaseText.text = lowercaseCharacter.ToString(); // will not do anything if var letter is already lowercase
        uppercaseText.text = uppercaseCharacter.ToString(); // will not do anything if var letter is already uppercase

        lowercaseText.gameObject.SetActive(true);
        uppercaseText.gameObject.SetActive(false);
    }

    public override void Shift(bool state)
    {
        shiftModifierActive = state;

        lowercaseText.gameObject.SetActive(!state);
        uppercaseText.gameObject.SetActive(state);
    }

    public override void Caps(bool state)
    {
        capsModifierActive = state;

        lowercaseText.gameObject.SetActive(!state);
        uppercaseText.gameObject.SetActive(state);
    }

    public override void Ctrl(bool state)
    {
        ctrlModifierActive = state;
    }

    public override string GetText()
    {
        return (shiftModifierActive ^ capsModifierActive ? uppercaseCharacter : lowercaseCharacter).ToString();
    }

    public override void Pressed()
    {
        OnKeyPressed.Invoke(this);
    }
}
