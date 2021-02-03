using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAvatarAnimator : MonoBehaviour
{
    public Animator animator;
    public float transitionTimeMultiplier = 10f;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            animator.SetFloat("WalkSpeed", 0, 1f, Time.deltaTime * transitionTimeMultiplier);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            animator.SetFloat("WalkSpeed", 1, 1f, Time.deltaTime * transitionTimeMultiplier);
        }
    }
}
