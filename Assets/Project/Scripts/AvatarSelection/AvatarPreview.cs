using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class AvatarPreview : MonoBehaviour
{
    public Transform spawnPosition;
    public Transform headTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private GameObject currentAvatar;
    private VRIK currentIK;

    public void Preview(GameObject prefab)
    {
        if (prefab)
        {
            if (currentAvatar)
                Destroy(currentAvatar);

            currentAvatar = Instantiate(prefab, spawnPosition);
            currentIK = currentAvatar.GetComponentInChildren<VRIK>();

            currentIK.solver.spine.headTarget = headTarget;
            currentIK.solver.leftArm.target = leftHandTarget;
            currentIK.solver.rightArm.target = rightHandTarget;
        }
    }
}
