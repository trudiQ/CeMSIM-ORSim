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
    public bool openAtStart = true;
    public bool closeAfterDelay = false;
    public float closeDelay = 5f;
    public bool isOpen { get; private set; }
    public bool stayOpen { get; private set; }
    public bool stayClosed { get; private set; }

    private string currentRole = "[role]";
    private string currentProcedure = "[procedure]";
    private int currentIndex = 0;
    private bool isTransitioning = false;
    private Queue<IEnumerator> transitionQueue = new Queue<IEnumerator>();

    public void Awake()
    {
        DisplayText(0);

        animator.SetBool("StayOpen", stayOpen);
        animator.SetBool("StayClosed", stayClosed);

        if (openAtStart)
            Open();
    }

    public void Open()
    {
        if (!isOpen && !stayOpen && !stayClosed)
        {
            if (closeAfterDelay)
            {
                if (!isTransitioning)
                    StartCoroutine(CoroutineOpenAndDelayClose());
                else
                    transitionQueue.Enqueue(CoroutineOpenAndDelayClose());
            }
            else
            {
                if (!isTransitioning)
                    StartCoroutine(CoroutineOpen());
                else
                    transitionQueue.Enqueue(CoroutineOpen());
            }
        }
    }

    public void Close()
    {
        if (isOpen && !stayOpen && !stayClosed)
        {
            isOpen = false;
            animator.SetBool("IsOpen", isOpen);
        }
    }

    public void SetStayOpen(bool state)
    {
        stayOpen = state;
        animator.SetBool("StayOpen", state);
    }

    public void SetStayClosed(bool state)
    {
        stayClosed = state;
        animator.SetBool("StayClosed", state);
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

    // Start the transition or add it to the queue if there's already a transition
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

    // Open function but queueable
    private IEnumerator CoroutineOpen()
    {
        isTransitioning = true;
        isOpen = true;

        animator.SetBool("IsOpen", isOpen);

        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);

        isTransitioning = false;
    }

    private IEnumerator CoroutineOpenAndDelayClose()
    {
        yield return new WaitUntil(() => isTransitioning == false); // Extra protection for race conditions

        isTransitioning = true;
        isOpen = true;

        animator.SetBool("IsOpen", isOpen);

        // Wait for the transition to finish before starting delay
        yield return null; // Wait until animator updates transition state
        yield return null;
        yield return new WaitUntil(() => animator.IsInTransition(0) == false);

        yield return new WaitForSeconds(closeDelay); // Wait for delay

        Close();

        isTransitioning = false;
    }

    private IEnumerator CoroutineTransitionToNextInstruction()
    {
        yield return new WaitUntil(() => isTransitioning == false); // Extra protection for race conditions

        isTransitioning = true;

        if (isOpen)
            animator.SetTrigger("NewInstruction");

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

        DisplayNextText();

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

        isTransitioning = false;

        if (transitionQueue.Count > 0)
            StartCoroutine(transitionQueue.Dequeue());
    }

    private IEnumerator CoroutineTransitionToInstructionIndex(int index)
    {
        yield return new WaitUntil(() => isTransitioning == false); // Extra protection for race conditions

        isTransitioning = true;
        animator.SetTrigger("NewInstruction");

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

        DisplayText(index);

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

        isTransitioning = false;

        if (transitionQueue.Count > 0)
            StartCoroutine(transitionQueue.Dequeue());
    }

    private IEnumerator CoroutineTransitionToCustomText(int index, string role, string procedure)
    {
        yield return new WaitUntil(() => isTransitioning == false); // Extra protection for race conditions

        isTransitioning = true;
        animator.SetTrigger("NewInstruction");

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

        DisplayText(index, role, procedure);

        if (!stayOpen && !stayClosed)
        {
            yield return null; // Wait until animator updates transition state
            yield return null;
            yield return new WaitUntil(() => animator.IsInTransition(0) == false);
        }

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