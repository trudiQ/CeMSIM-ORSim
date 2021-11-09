using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class SoiledPPE : MonoBehaviour
{
    public bool soiled { get; private set; } = false;

    public UnityEvent OnSoiled;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer.Equals("Ground"))
        {
            soiled = true;
            OnSoiled.Invoke();
        }
    }
}
