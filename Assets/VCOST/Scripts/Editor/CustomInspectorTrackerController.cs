using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackerController))]
public class CustomInspectorTrackerController : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20f); //2

        TrackerController tracker = (TrackerController)target;
        if (GUILayout.Button("Zero orientation"))
        {
            tracker.ZeroOrientation();
        }
    }
}
