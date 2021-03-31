using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(sphereJointModel))]
public class SphereJointInspector : Editor
{
    private sphereJointModel physicsModel;
    private bool bShowSphereJointModel; // used for playmode control

    private void OnSceneGUI()
    {
        physicsModel = target as sphereJointModel;
        //Debug.Log("Custom editor is running");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        physicsModel = target as sphereJointModel;

        // add a toggle to hide/unhide sphereJointModel
        bShowSphereJointModel = EditorGUILayout.Toggle("Hide/Unhide sphereJointModel", bShowSphereJointModel);
        if (bShowSphereJointModel)
            physicsModel.m_bShow = true;
        else
            physicsModel.m_bShow = false;

        //draw reset button in the Inspector
        if (GUILayout.Button("Reset")) //draw button 
        {
            physicsModel.Reset(); //reset mesh
        }

        // clear selected vertices 
        if (GUILayout.Button("clear Selected Vertices"))
        {
            //mesh.ClearAllData();
        }

        // split in-layer joints
        if (GUILayout.Button("Split in-layer joints"))
        {
            int[] layers2Split = { 0, 19 };
            int sphereIdx = 18;
            physicsModel.splitInlayerJoints_sigle(ref layers2Split, sphereIdx);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
