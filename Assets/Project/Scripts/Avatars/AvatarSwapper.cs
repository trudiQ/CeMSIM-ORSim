using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using UnityEditor;

public class AvatarSwapper : MonoBehaviour
{
    public int activeAvatar = 0;
    public HVRManager manager;
    public List<GameObject> avatars = new List<GameObject>();
    public GameObject spawnedAvatar { get; private set; }

    void Start()
    {
        SpawnAvatar(activeAvatar);
    }

    // Create the chosen avatar and remove the current avatar
    public void SpawnAvatar(int newIndex)
    {
        GameObject temp = Instantiate(original: avatars[newIndex], parent: gameObject.transform);
        GameObject.Destroy(spawnedAvatar);

        try
        {
            AvatarComponents components = spawnedAvatar.GetComponent<AvatarComponents>();
            components.SetManagerComponents(manager);
        }
        catch
        {
            Debug.LogWarning("AvatarComponents script not available on the spawned avatar.");
        }
        
        spawnedAvatar = temp;
        activeAvatar = newIndex;
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