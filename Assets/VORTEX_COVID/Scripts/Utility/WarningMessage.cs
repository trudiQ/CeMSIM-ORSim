using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningMessage : MonoBehaviour
{
    public Text displayTextField;
    public Color defaultColor = Color.red;
    public float fadeOutStartTime = 3f;
    public float fadeOutTime = 1f;

    private Coroutine currentFade;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void DisplayWarning(string message)
    {

        if (currentFade == null)
        {
            gameObject.SetActive(true);
            currentFade = StartCoroutine(WarningFade(message, defaultColor));
        }
            
        else
        {
            StopCoroutine(currentFade);
            currentFade = StartCoroutine(WarningFade(message, defaultColor));
        }
    }
    
    public void DisplayWarning(string message, Color color)
    {
        if (currentFade == null)
        {
            gameObject.SetActive(true);
            currentFade = StartCoroutine(WarningFade(message, color));
        }
        else
        {
            StopCoroutine(currentFade);
            currentFade = StartCoroutine(WarningFade(message, color));
        }
    }

    private IEnumerator WarningFade(string message, Color color)
    {
        displayTextField.text = message;
        displayTextField.color = color;

        float timeElapsed = 0;

        while (timeElapsed < fadeOutStartTime)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        timeElapsed = 0;
        Color newColor = color;

        while (displayTextField.color.a > 0)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
            
            newColor.a = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
            displayTextField.color = newColor;
        }

        currentFade = null;
        gameObject.SetActive(false);
    }
}
