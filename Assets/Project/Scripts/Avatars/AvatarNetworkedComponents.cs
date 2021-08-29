using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

// Included on the avatar prefab and avatar swapper if the simulation is multiplayer and avatars are synchronized
[Serializable]
public class AvatarNetworkedComponents : MonoBehaviour
{
    public Transform leftController;
    public Transform rightController;
    public Transform hmd;
    public AvatarHeightCalibration heightCalibration;
    public UnityEvent<float> onAvatarHeightChanged;

    private UnityEvent<float> currentSubscribedEvent;

    public void DeepCopy(AvatarNetworkedComponents components)
    {
        // Unlink the event listener from the previous avatar and add to the new event
        currentSubscribedEvent?.RemoveListener(OnAvatarHeightChanged);
        currentSubscribedEvent = components.onAvatarHeightChanged;
        currentSubscribedEvent?.AddListener(OnAvatarHeightChanged);

        // Assign all necessary components
        leftController = components.leftController;
        rightController = components.rightController;
        hmd = components.hmd;
        heightCalibration = components.heightCalibration;
    }

    public void OnAvatarHeightChanged(float scale)
    {
        onAvatarHeightChanged.Invoke(scale);
    }
}
