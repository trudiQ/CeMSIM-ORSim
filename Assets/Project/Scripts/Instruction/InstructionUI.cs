using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class InstructionUI : MonoBehaviour
{
    public Text textDisplay;
    public InstructionText[] instructions;

    private string currentRole = "[role]";
    private string currentProcedure = "[procedure]";
    private int currentIndex = 0;

    public void Start()
    {
        DisplayText(0);
    }

    public void UpdateRole(string role)
    {
        currentRole = role;
    }

    public void UpdateProcedure(string procedure)
    {
        currentProcedure = procedure;
    }

    public void DisplayText(int index, string role, string procedure)
    {
        if (index >= 0 && index < instructions.Length)
        {
            currentIndex = index;
            instructions[index].ShowText(textDisplay, role, procedure);
        }
    }

    public void DisplayText(int index)
    {
        if (index >= 0 && index < instructions.Length)
        {
            currentIndex = index;
            instructions[index].ShowText(textDisplay, currentRole, currentProcedure);
        }
    }

    public void DisplayNextText()
    {
        currentIndex = ++currentIndex % instructions.Length;

        DisplayText(currentIndex);
    }
}

[CustomEditor(typeof(InstructionUI))]
public class InstructionUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(Application.isPlaying)
        if (GUILayout.Button("Next Text"))
        {
            InstructionUI ui = target as InstructionUI;
            ui.DisplayNextText();
        }
    }
}