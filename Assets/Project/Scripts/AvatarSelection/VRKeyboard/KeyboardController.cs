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

    public List<InputField> inputFields;

    private UnityEvent<KeyboardModifier.ModifierType> OnModifierToggled = new UnityEvent<KeyboardModifier.ModifierType>();
    private LinkedList<InputField> inputFieldNavigation;
    private InputField currentField;

    void Start()
    {
        if (!inputModule)
            inputModule = FindObjectOfType<HVRInputModule>();

        keys = new List<KeyboardKey>(GetComponentsInChildren<KeyboardKey>());

        // Bind the key press events to the controller and controller message events to each key
        foreach (KeyboardKey key in keys)
        {
            inputModule.UICanvases.Add(key.GetComponentInChildren<Canvas>());
            key.OnKeyPressed.AddListener(KeyPressed);

            OnModifierToggled.AddListener(key.ModifierToggled);
        }

        // Prevent the keys from colliding with the base
        Collider[] keyColliders = GetComponentsInChildren<Collider>();

        foreach (var collider in keyColliders)
        {
            Physics.IgnoreCollision(baseCollider, collider);
        }

        // Add each text field to the queue
        inputFieldNavigation = new LinkedList<InputField>();

        foreach (InputField inputField in inputFields)
            inputFieldNavigation.AddLast(inputField);

        currentField = inputFieldNavigation.First.Value;
        currentField.Select();
    }

    public void KeyPressed(KeyboardKey key)
    {
        string keyValue = key.GetText();

        if (currentField)
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
                case "Tab":
                    currentField.text += '\t';
                    break;
                case "Clear":
                    currentField.text = "";
                    break;
                case "Backspace":
                    if(currentField.text.Length > 0)
                        currentField.text = currentField.text.Remove(currentField.text.Length - 1, 1);
                    break;
                case "Paste":
                    currentField.text += GUIUtility.systemCopyBuffer;
                    break;
                case "Previous":
                    currentField.DeactivateInputField();
                    currentField = CycleNavigationBackward();
                    break;
                case "Next":
                    currentField.DeactivateInputField();
                    currentField = CycleNavigationForward();
                    break;
                default:
                    currentField.text += keyValue;
                    break;
            }

            StartCoroutine(ManuallyActivateInputField(currentField));
        }
    }

    private IEnumerator ManuallyActivateInputField(InputField field)
    {
        field.ActivateInputField();
        field.Select();

        yield return 0; // Wait until the start of next frame to update the caret position
                        // Updating its position in the same frame does not work

        field.MoveTextEnd(false);
    }

    public InputField CycleNavigationForward()
    {
        // Put the last value in the list to the front
        inputFieldNavigation.AddLast(inputFieldNavigation.First.Value);
        inputFieldNavigation.RemoveFirst();

        return inputFieldNavigation.First.Value;
    }

    public InputField CycleNavigationBackward()
    {
        // Put the first value in the list to the back
        inputFieldNavigation.AddFirst(inputFieldNavigation.Last.Value);
        inputFieldNavigation.RemoveLast();

        return inputFieldNavigation.First.Value;
    }

    // Change the current field to a specifiec one, reorder the list
    public void SetCurrentInputField(InputField newField)
    {
        if (inputFieldNavigation.Find(newField) != null)
        {
            while (inputFieldNavigation.First.Value != newField)
                CycleNavigationForward();

            currentField = inputFieldNavigation.First.Value;
            currentField.Select();
        }
        else
            Debug.LogWarning("Keyboard does not contain the selected InputField.");
    }

    private void OnDestroy()
    {
        foreach (KeyboardKey key in keys)
            inputModule.UICanvases.Remove(key.GetComponentInChildren<Canvas>());
    }
}
