using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HurricaneVR.Framework.Core.UI;

public class KeyboardController : MonoBehaviour
{
    public HVRInputModule inputModule;
    public List<KeyboardKey> keys { get; private set; }
    public Collider baseCollider;

    public InputField activeTextfield;

    private UnityEvent<KeyboardModifier.ModifierType> OnModifierToggled = new UnityEvent<KeyboardModifier.ModifierType>();

    void Start()
    {
        if (!inputModule)
            inputModule = FindObjectOfType<HVRInputModule>();

        keys = new List<KeyboardKey>(GetComponentsInChildren<KeyboardKey>());

        foreach (KeyboardKey key in keys)
        {
            inputModule.UICanvases.Add(key.GetComponentInChildren<Canvas>());
            key.OnKeyPressed.AddListener(KeyPressed);

            OnModifierToggled.AddListener(key.ModifierToggled);
        }

        Collider[] keyColliders = GetComponentsInChildren<Collider>();

        foreach (var collider in keyColliders)
        {
            Physics.IgnoreCollision(baseCollider, collider);
        }
    }

    public void KeyPressed(KeyboardKey key)
    {
        string keyValue = key.GetText();

        if (activeTextfield)
        {
            switch (keyValue)
            {
                case "Shift":
                    KeyboardKey.ToggleShiftState();
                    OnModifierToggled.Invoke(KeyboardModifier.ModifierType.Shift);
                    break;
                case "Caps":
                    KeyboardKey.ToggleCapsState();
                    OnModifierToggled.Invoke(KeyboardModifier.ModifierType.Caps);
                    break;
                case "Ctrl":
                    KeyboardKey.ToggleCtrlState();
                    OnModifierToggled.Invoke(KeyboardModifier.ModifierType.Ctrl);
                    break;
                case "Alt":
                    KeyboardKey.ToggleAltState();
                    OnModifierToggled.Invoke(KeyboardModifier.ModifierType.Alt);
                    break;
                case "Tab":
                    // Insert a tab character
                    break;
                case "Enter":
                    // Insert a newline or confirm
                    break;
                case "Backspace":
                    activeTextfield.text = activeTextfield.text.Remove(activeTextfield.text.Length - 1, 1);
                    break;
                default:
                    activeTextfield.text += keyValue;
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyboardKey key in keys)
            inputModule.UICanvases.Remove(key.GetComponentInChildren<Canvas>());
    }
}
