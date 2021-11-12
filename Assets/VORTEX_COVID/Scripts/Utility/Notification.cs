using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public Text warningTextField;
    public GameObject notificationTextPrefab;
    public Transform notificationPanel;
    public Color warningColor = Color.red;
    public Color notificationColor = Color.blue;
    public float fadeOutStartTime = 3f;
    public float fadeOutTime = 1f;

    private List<Coroutine> currentWarningFadeCoroutines = new List<Coroutine>();
    private GameObject lingeringNotification;

    void Start()
    {
        warningTextField.gameObject.SetActive(false);
    }

    public void DisplayWarning(string message)
    {
        if (currentWarningFadeCoroutines.Count == 0)
            currentWarningFadeCoroutines.Add(StartCoroutine(WarningFade(message)));
        else
        {
            foreach (Coroutine coroutine in currentWarningFadeCoroutines)
                StopCoroutine(coroutine);

            currentWarningFadeCoroutines.Clear();
            currentWarningFadeCoroutines.Add(StartCoroutine(WarningFade(message)));
        }
    }

    private IEnumerator WarningFade(string message)
    {
        warningTextField.gameObject.SetActive(true);

        Coroutine coroutine = StartCoroutine(TextFieldFade(warningTextField, message, warningColor));
        currentWarningFadeCoroutines.Add(coroutine);
        yield return coroutine;

        currentWarningFadeCoroutines.Clear();
        warningTextField.gameObject.SetActive(false);
    }

    public void DisplayLingeringNotification(string message)
    {
        RemoveLingeringNotification();

        lingeringNotification = Instantiate(notificationTextPrefab, notificationPanel);
        lingeringNotification.transform.SetSiblingIndex(0);

        Text field = lingeringNotification.GetComponent<Text>();
        field.text = message;
        field.color = notificationColor;
    }

    public void RemoveLingeringNotification()
    {
        if (lingeringNotification)
            Destroy(lingeringNotification);
    }

    public void DisplayFadingNotification(string message)
    {
        StartCoroutine(NotificationFade(message));
    }

    private IEnumerator NotificationFade(string message)
    {
        GameObject textObject = Instantiate(notificationTextPrefab, notificationPanel);
        Text field = textObject.GetComponent<Text>();

        yield return StartCoroutine(TextFieldFade(field, message, notificationColor));

        Destroy(textObject);
    }

    private IEnumerator TextFieldFade(Text field, string message, Color color)
    {
        field.text = message;
        field.color = color;

        float timeElapsed = 0;

        while (timeElapsed < fadeOutStartTime)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        timeElapsed = 0;
        Color newColor = color;

        while (field.color.a > 0)
        {
            yield return null;
            timeElapsed += Time.deltaTime;
            
            newColor.a = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
            field.color = newColor;
        }
    }
}
