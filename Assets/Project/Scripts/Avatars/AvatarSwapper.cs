using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.UI;
using UnityEditor;
using UnityEngine.Events;

[RequireComponent(typeof(UserHeightUtility))]
public class AvatarSwapper : MonoBehaviour
{
    public int activeAvatar = 0;
    public HVRManager manager;
    public HVRInputModule uiInputModule;
    public UserHeightUtility userHeightUtility;
    public AvatarNetworkedComponents avatarNetworkedComponents;
    public List<GameObject> avatars = new List<GameObject>();
    public GameObject spawnedAvatar { get; private set; }

    public UnityEvent<int> OnAvatarSwapped;

    void Awake()
    {
        if (!userHeightUtility)
            userHeightUtility = GetComponent<UserHeightUtility>();

        if (!avatarNetworkedComponents)
            avatarNetworkedComponents = GetComponent<AvatarNetworkedComponents>();

        SwapAvatar(activeAvatar);
    }

    // Create the chosen avatar and remove the current avatar
    public void SwapAvatar(int newIndex)
    {
        Destroy(spawnedAvatar);

        spawnedAvatar = Instantiate(original: avatars[newIndex], 
                                    parent: gameObject.transform, 
                                    position: transform.position,
                                    rotation: transform.rotation);
        activeAvatar = newIndex;

        AvatarComponents components = spawnedAvatar.GetComponent<AvatarComponents>();

        if (avatarNetworkedComponents)
        {
            avatarNetworkedComponents.DeepCopy(spawnedAvatar.GetComponent<AvatarNetworkedComponents>());
        }

        components.SetHVRComponents(manager, uiInputModule);

        if(components.calibration)
            components.calibration.userHeightUtility = userHeightUtility;

        userHeightUtility.floor = components.floor;
        userHeightUtility.camera = components.camera;

        OnAvatarSwapped.Invoke(activeAvatar);
    }
}

[CustomEditor(typeof(AvatarSwapper))]
public class AvatarSwapperEditor : Editor
{
    private int avatarIndex;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Application.isPlaying)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            avatarIndex = EditorGUILayout.IntField(label: "Avatar Index", value: avatarIndex);

            if (GUILayout.Button("Swap"))
                (target as AvatarSwapper).SwapAvatar(avatarIndex);
        }
            
    }
}