using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CreateAssetMenu(fileName = "Instruction Text", menuName = "CeMSIM/Instruction Text", order = 1)]
public class InstructionText : ScriptableObject
{
    public string text;

    public void ShowText(Text textField, string role, string procedure)
    {
        textField.text = string.Format(text, role, procedure);
    }
}

[CustomEditor(typeof(InstructionText))]
public class InstructionTextEditor : Editor
{
    InstructionText text;

    private void OnEnable()
    {
        text = target as InstructionText;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Text:", GUILayout.Width(30));
        EditorStyles.textArea.wordWrap = true;
        text.text = EditorGUILayout.TextArea(text.text, EditorStyles.textArea);
        EditorGUILayout.EndHorizontal();
    }
}