using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class OmniToolsAnimations : MonoBehaviour
{
    public Transform forcepsA;
    public Transform forcepsB;
    public Transform scissorsA;
    public Transform scissorsB;
    public float forcepsOpenAngle;
    public float scissorsOpenAngle;
    public float forcepsOpenDuration;
    public float forcepsCloseDuration;
    public float scissorsAnimationDuration;
    public Vector3 forcepsRotationAxis; // A towards this * 1, B towards this * -1
    public Vector3 scissorsRotationAxis;

    public Quaternion forcepsAopenRot;
    public Quaternion forcepsAcloseRot;
    public Quaternion forcepsBopenRot;
    public Quaternion forcepsBcloseRot;
    public Quaternion scissorsAopenRot;
    public Quaternion scissorsAcloseRot;
    public Quaternion scissorsBopenRot;
    public Quaternion scissorsBcloseRot;
    public bool isPlayingAnimation;

    private void Start()
    {
        forcepsAopenRot = Quaternion.Euler(forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsAcloseRot = Quaternion.identity;
        forcepsBopenRot = Quaternion.Euler(-forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsBcloseRot = Quaternion.identity;
        scissorsAopenRot = Quaternion.Euler(scissorsRotationAxis * scissorsOpenAngle * 0.5f);
        scissorsAcloseRot = Quaternion.identity;
        scissorsBopenRot = Quaternion.Euler(-scissorsRotationAxis * scissorsOpenAngle * 0.5f);
        scissorsBcloseRot = Quaternion.identity;
    }

    [ShowInInspector]
    public void OpenForceps()
    {
        if (isPlayingAnimation)
        {
            return;
        }

        isPlayingAnimation = true;
        StartCoroutine(RotateObject(forcepsA, new Quaternion[] { forcepsAcloseRot }, new Quaternion[] { forcepsAopenRot }, new float[] { forcepsOpenDuration }));
        StartCoroutine(RotateObject(forcepsB, new Quaternion[] { forcepsBcloseRot }, new Quaternion[] { forcepsBopenRot }, new float[] { forcepsOpenDuration }));
    }
    [ShowInInspector]
    public void CloseForceps()
    {
        if (isPlayingAnimation)
        {
            return;
        }

        isPlayingAnimation = true;
        StartCoroutine(RotateObject(forcepsA, new Quaternion[] { forcepsAopenRot }, new Quaternion[] { forcepsAcloseRot }, new float[] { forcepsCloseDuration }));
        StartCoroutine(RotateObject(forcepsB, new Quaternion[] { forcepsBopenRot }, new Quaternion[] { forcepsBcloseRot }, new float[] { forcepsCloseDuration }));
    }
    [ShowInInspector]
    public void CloseOpenScissors()
    {
        if (isPlayingAnimation)
        {
            return;
        }

        isPlayingAnimation = true;
        StartCoroutine(RotateObject(scissorsA, new Quaternion[] { scissorsAopenRot, scissorsAcloseRot }, new Quaternion[] { scissorsAcloseRot, scissorsAopenRot }, new float[] { scissorsAnimationDuration * 0.5f, scissorsAnimationDuration * 0.5f }));
        StartCoroutine(RotateObject(scissorsB, new Quaternion[] { scissorsBopenRot, scissorsBcloseRot }, new Quaternion[] { scissorsBcloseRot, scissorsBopenRot }, new float[] { scissorsAnimationDuration * 0.5f, scissorsAnimationDuration * 0.5f }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="startLocalRot"></param>
    /// <param name="endLocalRot"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IEnumerator RotateObject(Transform target, Quaternion[] startLocalRot, Quaternion[] endLocalRot, float[] duration)
    {
        for (int i = 0; i < duration.Length; i++)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / duration[i])
            {
                target.localRotation = Quaternion.Lerp(startLocalRot[i], endLocalRot[i], t);
                yield return null;
            }
            target.localRotation = endLocalRot[i];
        }

        isPlayingAnimation = false;
    }
}
