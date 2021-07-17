using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardController : MonoBehaviour
{
    public List<KeyboardKey> keys { get; private set; }

    private Text activeTextfield;

    void Start()
    {
        keys = new List<KeyboardKey>(GetComponentsInChildren<KeyboardKey>());

        foreach (KeyboardKey key in keys)
            key.OnKeyPressed.AddListener(KeyPressed);
    }

    public void KeyPressed(KeyboardKey key)
    {
        KeyParse(key.GetText());
    }

    public void KeyParse(string keyValue)
    {
        if (activeTextfield)
        {
            switch (keyValue)
            {
                case "Shift":
                    break;
                case "Caps":
                    break;
                case "Ctrl":
                    break;
                case "Alt":
                    break;
                default:
                    break;
            }
        }
    }
}
