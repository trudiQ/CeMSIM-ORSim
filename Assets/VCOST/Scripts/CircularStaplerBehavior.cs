using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CircularStaplerBehavior : MonoBehaviour
{
    public Transform _moveToTransform;
    public float _movementMultiplier = 2f;
    public bool _debugMoving = true;
    private bool _isMoving = false;
    private IEnumerator _IEMoving;

    public UnityEvent _E_OnMoveFinish;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartMoving();
        }
    }

    public void StartMoving()
    {
        if (_isMoving) return;
        _IEMoving = StartMovingToTransform();
        StartCoroutine(_IEMoving);
        _isMoving = true;
    }

    IEnumerator StartMovingToTransform()
    {
        Vector3 path = Vector3.zero;
        while (Vector3.Distance(transform.position, _moveToTransform.position) > 0.05f)
        {
            path = (_moveToTransform.position - transform.position).normalized;
            path *= Time.deltaTime * _movementMultiplier;
            transform.position += path;
            yield return new WaitForFixedUpdate();
        }
        _E_OnMoveFinish.Invoke();
    }
}
