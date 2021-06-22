using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Finds the height of the avatar's prefab for height calibration
public class AvatarPrefabHeightUtility : MonoBehaviour
{
    public Transform avatarRoot;
    public Transform avatarHead;

    public float height;

    public float CalculateHeight()
    {
        height = avatarHead.position.y - avatarRoot.position.y;
        return height;
    }
}

[CustomEditor(typeof(AvatarPrefabHeightUtility))]
public class AvatarPrefabHeightUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AvatarPrefabHeightUtility heightUtility = (target as AvatarPrefabHeightUtility);

        if (!Application.isPlaying && heightUtility.avatarRoot && heightUtility.avatarHead && GUILayout.Button("Calculate Height"))
        {
            Undo.RecordObject(heightUtility, "Changed avatar height");
            heightUtility.CalculateHeight();
            PrefabUtility.RecordPrefabInstancePropertyModifications(heightUtility);
        }
    }
}