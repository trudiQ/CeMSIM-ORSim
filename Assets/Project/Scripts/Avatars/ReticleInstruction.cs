using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleInstruction : MonoBehaviour
{
    public GameObject reticle;
    public Text instructionText;
    public Text timerText;
    public Text confirmationText;
    public Color reticleUnfocusedColor = Color.red;
    public Color reticleFocusedColor = Color.green;
    public Color reticleConfirmationColor = Color.cyan;

    private Material reticleMaterial;
    private bool focused = false;

    void Start()
    {
        // Use this material to change the color of the reticle
        reticleMaterial = reticle.GetComponentInChildren<Renderer>().material;

        // Set initial visibility
        instructionText.gameObject.SetActive(!focused);
        timerText.gameObject.SetActive(focused);
        confirmationText.gameObject.SetActive(false);
    }

    // Toggle between the instruction text and the timer text
    public void ToggleFocus()
    {
        focused = !focused;
        reticleMaterial.SetColor("_UnlitColor", focused ? reticleFocusedColor : reticleUnfocusedColor);

        instructionText.gameObject.SetActive(!focused);
        timerText.gameObject.SetActive(focused);
    }

    public void SetTimerNumber(int number)
    {
        timerText.text = number.ToString();
    }

    // Hide the instruction text and the timer text and show the confirmation
    public void ShowConfirmation()
    {
        reticleMaterial.SetColor("_UnlitColor", reticleConfirmationColor);

        instructionText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        confirmationText.gameObject.SetActive(true);
    }
}
