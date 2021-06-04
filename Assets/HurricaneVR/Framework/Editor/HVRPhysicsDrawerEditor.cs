using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.Shared;
using UnityEditor;
using UnityEngine;

namespace HurricaneVR.Editor
{
    [CustomEditor(typeof(HVRPhysicsDrawer))]
    public class HVRPhysicsDrawerEditor : UnityEditor.Editor
    {
        private SerializedProperty SP_StartPosition;
        private SerializedProperty SP_EndPosition;
        private SerializedProperty SP_Threshold;
        private SerializedProperty SP_UpThreshold;
        public HVRPhysicsDrawer component;
        private bool _setupExpanded;

        protected void OnEnable()
        {
            SP_StartPosition = serializedObject.FindProperty("StartPosition");
            SP_EndPosition = serializedObject.FindProperty("EndPosition");
            SP_Threshold = serializedObject.FindProperty("DownThreshold");
            SP_UpThreshold = serializedObject.FindProperty("ResetThreshold");
            component = target as HVRPhysicsDrawer;

        }

        public override void OnInspectorGUI()
        {
           

            var dir = SP_EndPosition.vector3Value - SP_StartPosition.vector3Value;
            dir.Normalize();

            _setupExpanded = EditorGUILayout.Foldout(_setupExpanded, "Setup Helpers");
            if (_setupExpanded)
            {
                EditorGUILayout.HelpBox("1. Choose the local axis the drawer will move on.\r\n" +
                                        "2. Save the start position of the drawer.\r\n" +
                                        "3. Save the end position of the drawer.\r\n"
                                        //"4. Save the down and reset positions.\r\n" +
                                        //"5. Return the transform to start by pressing the return button.\r\n" +
                                        //"6. If the Connected Body is left blank, the button will be jointed to the world and cannot be moved."
                    , MessageType.Info);



                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Start"))
                {
                    SP_StartPosition.vector3Value = component.transform.localPosition;
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("GoTo Start"))
                {
                    component.transform.localPosition = SP_StartPosition.vector3Value;
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save End"))
                {
                    SP_EndPosition.vector3Value = component.transform.localPosition;
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("GoTo End"))
                {
                    component.transform.localPosition = SP_EndPosition.vector3Value;
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}