using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularStapler : MonoBehaviour
{
    public Animator animator;
    public string activeInputAxis = "Fire1";
    public string secondaryActiveInputAxis = "Fire2";
    public float speed = 1f;

    void Start()
    {
        animator.SetFloat("animationTime", 0f);
    }

    void Update()
    {
        if (Input.GetButton(activeInputAxis))
        {
            animator.SetFloat("animationTime1", Mathf.Clamp(animator.GetFloat("animationTime1") + speed * Time.deltaTime, 0f, 1f));
        }
        else
        {
            animator.SetFloat("animationTime1", Mathf.Clamp(animator.GetFloat("animationTime1") - speed * Time.deltaTime, 0f, 1f));
        }

        if (Input.GetButton(secondaryActiveInputAxis))
        {
            animator.SetFloat("animationTime2", Mathf.Clamp(animator.GetFloat("animationTime2") + speed * Time.deltaTime, 0f, 1f));
        }
        else
        {
            animator.SetFloat("animationTime2", Mathf.Clamp(animator.GetFloat("animationTime2") - speed * Time.deltaTime, 0f, 1f));
        }
    }
}
