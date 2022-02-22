using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectManipulator : MonoBehaviour
{
    private IEnumerator activeAction;

    public UnityEvent e_OnCoroutineFinish = new UnityEvent();

    private void CheckAndStartCoroutine(IEnumerator routine)
    {
        if (activeAction != null)
        {
            Debug.LogWarning("Manipulator is already busy, discarding new directions.");
        }
        else
        {
            activeAction = routine;
            StartCoroutine(activeAction);
        }
    }

    private void FinishCoroutine()
    {
        activeAction = null;
        e_OnCoroutineFinish.Invoke();
    }

    public void AlignOverTime(Vector3 position, Quaternion orientation, float seconds)
    {
        CheckAndStartCoroutine(C_AlignOverTime(position, orientation, seconds));
    }

    private IEnumerator C_AlignOverTime(Vector3 position, Quaternion orientation, float seconds)
    {
        Vector3 oldPosition = transform.position;
        Quaternion oldOrientation = transform.rotation;
        float elapsedTime = 0;
        while (true)
        {
            if (elapsedTime >= seconds)
            {
                transform.position = position;
                transform.rotation = orientation;
                break;
            }

            transform.position = Vector3.Lerp(oldPosition, position, elapsedTime / seconds);
            transform.rotation = Quaternion.Lerp(oldOrientation, orientation, elapsedTime / seconds);
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.fixedDeltaTime;
        }
        FinishCoroutine();
    }
}
