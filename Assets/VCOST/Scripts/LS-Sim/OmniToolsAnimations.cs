using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class OmniToolsAnimations : MonoBehaviour
{
    public Transform forcepsA;
    public Transform forcepsB;
    public Transform forcepsA1;
    public Transform forcepsB1;
    public Transform forcepsA2;
    public Transform forcepsB2;
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
    public Quaternion forcepsAopenRot1;
    public Quaternion forcepsAcloseRot1;
    public Quaternion forcepsBopenRot1;
    public Quaternion forcepsBcloseRot1;
    public Quaternion forcepsAopenRot2;
    public Quaternion forcepsAcloseRot2;
    public Quaternion forcepsBopenRot2;
    public Quaternion forcepsBcloseRot2;
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
        forcepsAopenRot1 = Quaternion.Euler(forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsAcloseRot1 = Quaternion.identity;
        forcepsBopenRot1 = Quaternion.Euler(-forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsBcloseRot1 = Quaternion.identity;
        forcepsAopenRot2 = Quaternion.Euler(forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsAcloseRot2 = Quaternion.identity;
        forcepsBopenRot2 = Quaternion.Euler(-forcepsRotationAxis * forcepsOpenAngle * 0.5f);
        forcepsBcloseRot2 = Quaternion.identity;
        scissorsAopenRot = Quaternion.Euler(scissorsRotationAxis * scissorsOpenAngle * 0.5f);
        scissorsAcloseRot = Quaternion.identity;
        scissorsBopenRot = Quaternion.Euler(-scissorsRotationAxis * scissorsOpenAngle * 0.5f);
        scissorsBcloseRot = Quaternion.identity;
    }

    [ShowInInspector]
    public void OpenForceps(string forcepsName)
    {
        if (isPlayingAnimation)
        {
            return;
        }

        isPlayingAnimation = true;
        if (forcepsName == "Forceps")
        {
            StartCoroutine(RotateObject(forcepsA, new Quaternion[] { forcepsAcloseRot }, new Quaternion[] { forcepsAopenRot }, new float[] { forcepsOpenDuration }));
            StartCoroutine(RotateObject(forcepsB, new Quaternion[] { forcepsBcloseRot }, new Quaternion[] { forcepsBopenRot }, new float[] { forcepsOpenDuration }));
        }
        else if (forcepsName == "Forceps1")
        {
            StartCoroutine(RotateObject(forcepsA1, new Quaternion[] { forcepsAcloseRot1 }, new Quaternion[] { forcepsAopenRot1 }, new float[] { forcepsOpenDuration }));
            StartCoroutine(RotateObject(forcepsB1, new Quaternion[] { forcepsBcloseRot1 }, new Quaternion[] { forcepsBopenRot1 }, new float[] { forcepsOpenDuration }));
        }
        else if (forcepsName == "Forceps2")
        {
            StartCoroutine(RotateObject(forcepsA2, new Quaternion[] { forcepsAcloseRot2 }, new Quaternion[] { forcepsAopenRot2 }, new float[] { forcepsOpenDuration }));
            StartCoroutine(RotateObject(forcepsB2, new Quaternion[] { forcepsBcloseRot2 }, new Quaternion[] { forcepsBopenRot2 }, new float[] { forcepsOpenDuration }));
        }
    }
    [ShowInInspector]
    public void CloseForceps(string forcepsName)
    {
        if (isPlayingAnimation)
        {
            return;
        }

        isPlayingAnimation = true;
        if (forcepsName == "Forceps")
        {
            StartCoroutine(RotateObject(forcepsA, new Quaternion[] { forcepsAopenRot }, new Quaternion[] { forcepsAcloseRot }, new float[] { forcepsCloseDuration }));
            StartCoroutine(RotateObject(forcepsB, new Quaternion[] { forcepsBopenRot }, new Quaternion[] { forcepsBcloseRot }, new float[] { forcepsCloseDuration }));
        }
        else if (forcepsName == "Forceps1")
        {
            StartCoroutine(RotateObject(forcepsA1, new Quaternion[] { forcepsAopenRot1 }, new Quaternion[] { forcepsAcloseRot1 }, new float[] { forcepsCloseDuration }));
            StartCoroutine(RotateObject(forcepsB1, new Quaternion[] { forcepsBopenRot1 }, new Quaternion[] { forcepsBcloseRot1 }, new float[] { forcepsCloseDuration }));
        }
        else if (forcepsName == "Forceps2")
        {
            StartCoroutine(RotateObject(forcepsA2, new Quaternion[] { forcepsAopenRot2 }, new Quaternion[] { forcepsAcloseRot2 }, new float[] { forcepsCloseDuration }));
            StartCoroutine(RotateObject(forcepsB2, new Quaternion[] { forcepsBopenRot2 }, new Quaternion[] { forcepsBcloseRot2 }, new float[] { forcepsCloseDuration }));
        }
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
