using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using UnityEditor;

[RequireComponent(typeof(UserHeightUtility))]
public class AvatarSwapper : MonoBehaviour
{
    public int activeAvatar = 0;
    public HVRManager manager;
    public UserHeightUtility userHeightUtility;
    public AvatarNetworkedComponents avatarNetworkedComponents;
    public List<GameObject> avatars = new List<GameObject>();
    public GameObject spawnedAvatar { get; private set; }

    void Awake()
    {
        if (!userHeightUtility)
            userHeightUtility = GetComponent<UserHeightUtility>();

        if (!avatarNetworkedComponents)
            avatarNetworkedComponents = GetComponent<AvatarNetworkedComponents>();

        SpawnAvatar(activeAvatar);
    }

    // Create the chosen avatar and remove the current avatar
    public void SpawnAvatar(int newIndex)
    {
        Destroy(spawnedAvatar);
        spawnedAvatar = Instantiate(original: avatars[newIndex], parent: gameObject.transform);
        activeAvatar = newIndex;

        AvatarComponents components = spawnedAvatar.GetComponent<AvatarComponents>();

        if (avatarNetworkedComponents)
        {
            avatarNetworkedComponents.DeepCopy(spawnedAvatar.GetComponent<AvatarNetworkedComponents>());
        }

        components.SetManagerComponents(manager);
        components.PrepareAvatar(userHeightUtility);

        userHeightUtility.floor = components.floor;
        userHeightUtility.camera = components.camera;
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
                (target as AvatarSwapper).SpawnAvatar(avatarIndex);
        }
            
    }
}