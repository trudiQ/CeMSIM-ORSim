using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnKey : MonoBehaviour
{
    public KeyCode key;

    public UnityEvent e_KeyPressed;
    public UnityEvent e_KeyReleased;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            e_KeyPressed.Invoke();
        }

        if (Input.GetKeyUp(key))
        {
            e_KeyReleased.Invoke();
        }
    }
}
