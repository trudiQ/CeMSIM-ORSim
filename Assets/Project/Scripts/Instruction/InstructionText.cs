using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Instruction Text", menuName = "CeMSIM/Instruction Text", order = 1)]
public class InstructionText : ScriptableObject
{
    [TextArea(3, 5)] public string text;

    public void ShowText(Text textField, string role, string procedure)
    {
        textField.text = string.Format(text, role, procedure).Replace("\\n", "\n");
    }
}