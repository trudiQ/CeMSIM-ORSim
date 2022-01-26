using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Animations;

public class InstructionUI : MonoBehaviour
{
    public Text textDisplay;
    public InstructionText[] instructions;
    public Animator animator;

    private string currentRole = "[role]";
    private string currentProcedure = "[procedure]";
    private int currentIndex = 0;
    private bool isTransitioning = false;
    private Queue<IEnumerator> transitionQueue = new Queue<IEnumerator>();

    public void Start()
    {
        DisplayText(0);
    }

    public void UpdateRole(string role)
    {
        currentRole = role;
        UpdateText();
    }

    public void UpdateProcedure(string procedure)
    {
        currentProcedure = procedure;
        UpdateText();
    }

    public void TransitionToNextInstruction()
    {
        if (!isTransitioning)
            StartCoroutine(CoroutineTransitionToNextInstruction());
        else
            transitionQueue.Enqueue(CoroutineTransitionToNextInstruction());
    }

    public void TransitionToInstructionIndex(int index)
    {
        if (!isTransitioning)
            StartCoroutine(CoroutineTransitionToInstructionIndex(index));
        else
            transitionQueue.Enqueue(CoroutineTransitionToInstructionIndex(index));
    }

    public void TransitionToCustomText(int index, string role, string procedure)
    {
        if (!isTransitioning)
            StartCoroutine(CoroutineTransitionToCustomText(index, role, procedure));
        else
            transitionQueue.Enqueue(CoroutineTransitionToCustomText(index, role, procedure));
    }

    private IEnumerator CoroutineTransitionToNextInstruction()
    {
        isTransitioning = true;
        animator.SetTrigger("NewInstruction");

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);

        DisplayNextText();

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        isTransitioning = false;

        if (transitionQueue.Count > 0)
            StartCoroutine(transitionQueue.Dequeue());
    }

    private IEnumerator CoroutineTransitionToInstructionIndex(int index)
    {
        isTransitioning = true;
        animator.SetTrigger("NewInstruction");

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);

        DisplayText(index);

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        isTransitioning = false;

        if (transitionQueue.Count > 0)
            StartCoroutine(transitionQueue.Dequeue());
    }

    private IEnumerator CoroutineTransitionToCustomText(int index, string role, string procedure)
    {
        isTransitioning = true;
        animator.SetTrigger("NewInstruction");

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);

        DisplayText(index, role, procedure);

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        isTransitioning = false;

        if (transitionQueue.Count > 0)
            StartCoroutine(transitionQueue.Dequeue());
    }

    private void DisplayText(int index, string role, string procedure)
    {
        if (index >= 0 && index < instructions.Length)
        {
            currentIndex = index;
            instructions[index].ShowText(textDisplay, role, procedure);
        }
    }

    private void DisplayText(int index)
    {
        if (index >= 0 && index < instructions.Length)
        {
            currentIndex = index;
            instructions[index].ShowText(textDisplay, currentRole, currentProcedure);
        }
    }

    private void DisplayNextText()
    {
        currentIndex = ++currentIndex % instructions.Length;

        DisplayText(currentIndex);
    }

    private void UpdateText()
    {
        DisplayText(currentIndex);
    }
}

[CustomEditor(typeof(InstructionUI))]
public class InstructionUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
            if (GUILayout.Button("Next Text"))
            {
                InstructionUI ui = target as InstructionUI;
                ui.TransitionToNextInstruction();
            }
    }
}