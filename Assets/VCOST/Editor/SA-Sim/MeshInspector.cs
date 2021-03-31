using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using System.Linq;

[CustomEditor(typeof(colonMesh))]
public class MeshInspector : Editor
{
    private colonMesh mesh;
    private Transform handleTransform;
    private Quaternion handleRotation;
    string triangleIdx;

    private void OnSceneGUI()
    {
        mesh = target as colonMesh;
        //Debug.Log("Custom editor is running");

        EditMesh();
    }

    void EditMesh()
    {
        handleTransform = mesh.transform; //get mesh's transform in world space
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity; //get the current pivot rotation
        for (int i = 0; i < mesh.vertices.Length; i++) //draw mesh vertices with dots
        {
            ShowPoint(i);
        }
    }

    private void ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(mesh.vertices[index]); //convert vertex's local position into world space
        Vector3 normal = handleTransform.TransformPoint(mesh.getNormal(index)); // convert vertex's local normal into world space
        normal.Normalize();
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        if (mesh.moveVertexPoint)
        {
            if (!mesh.isBound)
            {
                //attach handles to mesh vertices
                Handles.color = Color.blue;

                //drag a single vertex
                point = Handles.FreeMoveHandle(point, handleRotation, mesh.handleSize,
                                                   Vector3.zero, Handles.CircleHandleCap); //Draw the dot using the Handles utility class
                if (GUI.changed) //check if any changes made to the dot
                {
                    mesh.DoAction(index, handleTransform.InverseTransformPoint(point)); //dragging a vertex by converting vertex's position back to local space
                }
            }
        }
        else // not move, just select
        {
            GameObject sphereJointObj = GameObject.Find(mesh.sphereJointObjName); //"sphereJointGameObj0"
            sphereJointModel physicsModel = null;
            if (sphereJointObj)
               physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
            // if duplication checked , not display duplicate vertices
            if (mesh.bDuplicate.Count > 0 && index < mesh.bDuplicate.Count)
            {
                if (mesh.bDuplicate[index])
                    return;
            }

            //select vertices
            if (!mesh.seleVertexList.indices.Contains(index))
            {
                Handles.color = Color.blue;
                if (Handles.Button(point, handleRotation, mesh.handleSize, mesh.pickSize, Handles.CircleHandleCap)) // draw mesh vertices as button to click and select
                {
                    mesh.seleVertexList.indices.Add(index);
                    mesh.seleVertexList.positions.Add(point);
                    // debug use: print normal
                    //Debug.Log("normal selected: " + normal.x.ToString() + "," +
                    //                                normal.y.ToString() + "," +
                    //                                normal.z.ToString());
                    //

                    // highlight its sphere neighbors
                    if (mesh.isBound && physicsModel)
                    {
                        if (mesh.neighborSpheres.Count > 0)
                            physicsModel.highlight(mesh.neighborSpheres[index]);
                    }
                }
            }
            else // unselect
            {
                Handles.color = Color.red;
                if (Handles.Button(point, handleRotation, mesh.handleSize, mesh.pickSize, Handles.CircleHandleCap))
                {
                    int i = mesh.seleVertexList.indices.IndexOf(index);
                    if (i >= 0)
                    {
                        mesh.seleVertexList.indices.RemoveAt(i);
                        mesh.seleVertexList.positions.RemoveAt(i);

                        // unhighlight its sphere neighbors
                        if (mesh.isBound && physicsModel)
                        {
                            if (mesh.neighborSpheres.Count > 0)
                                physicsModel.unhighlight(mesh.neighborSpheres[index]);
                        }
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        mesh = target as colonMesh;

        //draw reset button in the Inspector
        if (GUILayout.Button("Reset")) //draw button 
        {
            // reset mesh
            mesh.Reset();
            Debug.Log("Mesh is reset!");
            // clear all member variables
            mesh.ClearAllData();
            Debug.Log("Mesh member variables are cleared!");
            // clear duplicate tag
            if (mesh.bRemoveDuplicate)
            {
                Debug.Log("Mesh duplicate handling is reversed!");
                mesh.bRemoveDuplicate = false;
            }
            // clear neighbors (same to 'unbind sphereJointModel')
            if (mesh.isBound || mesh.neighborSpheres.Count > 0)
            {
                mesh.clearNeighbors();
                mesh.unbindSphereJointModel();
                Debug.Log("Mesh is unbind!");
            }
        }

        // For testing Reset function
        if (mesh.isCloned)
        {
            //if (GUILayout.Button("Test Edit"))
            //{
            //    mesh.EditMesh();
            //}
        }

        // clear selected vertices 
        if (GUILayout.Button("clear Selected Vertices"))
        {
            // unhighlight the neighbor spheres of selected mesh vertices
            if (mesh.isBound || mesh.neighborSpheres.Count > 0)
            {
                GameObject sphereJointObj = GameObject.Find(mesh.sphereJointObjName); //"sphereJointGameObj0"
                sphereJointModel physicsModel = null;
                if (sphereJointObj)
                    physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
                foreach (int idx in mesh.seleVertexList.indices)
                {
                    if (physicsModel)
                    {
                        if (mesh.neighborSpheres.Count > 0)
                            physicsModel.unhighlight(mesh.neighborSpheres[idx]);
                    }
                }
            }
            //mesh.ClearAllData();
            mesh.seleVertexList.indices = new List<int>();
            mesh.seleVertexList.positions = new List<Vector3>();
        }

        // examine and clear duplicated vertices
        if (GUILayout.Button("Handling vertex duplicates"))
        {
            if (mesh.bRemoveDuplicate || mesh.verDupMap.Count > 0)
            {
                Debug.Log("Duplicated already handled");
                return;
            }
            // check and handle vertex duplication  
            if (!mesh.checkDuplicateVertices())
            {
                mesh.removeDuplicateVertices();
                mesh.bRemoveDuplicate = true;
            }
        }

        // build inner/outer layer info for each triangle
        if (GUILayout.Button("Build inner/outer layer info"))
        {
            // duplication handling should be done first
            if (!mesh.bRemoveDuplicate || mesh.verDupMap.Count <= 0)
            {
                Debug.Log("Duplications not yet handled");
                return;
            }
            // binding should NOT be done before this
            if (mesh.isBound || mesh.neighborSpheres.Count > 0)
            {
                Debug.Log("Error: Cannot buid inner/outer layer info due to binding already done!");
                return;
            }
            mesh.evaluateTriIOLayerInfo();
            Debug.Log("Done build inner/outer layer info");
        }

        // show 4 neighboring spheres
        if (GUILayout.Button("Binding sphereJointModel"))
        {
            if (mesh.neighborSpheres.Count > 0)
            {
                Debug.Log("neighbors're already bound!");
                return;
            }
            GameObject sphereJointObj = GameObject.Find(mesh.sphereJointObjName); //"sphereJointGameObj0"
            if (!sphereJointObj)
            {
                Debug.Log("Error, No SphereJointGameObj0 found!");
                return;
            }

            sphereJointModel physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
            if (!physicsModel)
            {
                Debug.Log("Error, No physics model of SphereJointGameObj0 found!");
                return;
            }

            if (mesh.neighborSphereSearching1(physicsModel))
            {
                mesh.bindSphereJointModel();
                mesh.isBound = true;
            }
            Debug.Log("Binding sphereJointModel done!");
        }

        // clear neighbors
        if (GUILayout.Button("Unbind sphereJointModel"))
        {
            mesh.clearNeighbors();
            mesh.unbindSphereJointModel();
            Debug.Log("Unbind sphereJointModel done!");
        }

        // split
        if (GUILayout.Button("Split"))
        {
            // check duplication handling
            if (!mesh.bRemoveDuplicate || mesh.verDupMap.Count <= 0)
            {
                Debug.Log("Error, Duplicated not yet handled");
                return;
            }
            // check inner/outer layer info building
            if (mesh.triIOLayerInfo.Count <= 0)
            {
                Debug.Log("Error, inner/outer layer info not yet built");
                return;
            }

            // check binding to the sphereModel
            GameObject sphereJointObj = GameObject.Find(mesh.sphereJointObjName); //"sphereJointGameObj0"
            if (!sphereJointObj)
            {
                Debug.Log("Error, No SphereJointGameObj0 found!");
                return;
            }

            sphereJointModel physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
            if (!physicsModel)
            {
                Debug.Log("Error, No physics model of SphereJointGameObj0 found!");
                return;
            }

            if (!mesh.isBound)
            {
                Debug.Log("Error: Mesh is not bound yet!");
                return;
            }

            int[] layers2Split = { -1, -1 };
            int sphereIdx = -1;
            if (mesh.sphereJointObjIdx == 0)
            {
                layers2Split[0] = 0;
                layers2Split[1] = 14;
                sphereIdx = 4;
            }
            else if (mesh.sphereJointObjIdx == 1)
            {
                layers2Split[0] = 0;
                layers2Split[1] = 14;//10
                sphereIdx = 16;//15
            }
            //int[] layers2Split = { 0, 19 };
            //int sphereIdx = 4;
            if (layers2Split[0] < 0 || layers2Split[1] < 0 || sphereIdx < 0)
            {
                Debug.Log("Error: invalid layers2Split or sphereIdx");
                return;
            }

            if (Application.isPlaying)
                physicsModel.splitInlayerJoints_sigle(ref layers2Split, sphereIdx);

            if (mesh.split1(layers2Split, sphereIdx, physicsModel))
                Debug.Log("Split Mesh!");
        }

        // test
        if (GUILayout.Button("test"))
        {
            /*GameObject sphereJointObj = GameObject.Find(mesh.sphereJointObjName); //"sphereJointGameObj0"
            if (!sphereJointObj)
            {
                Debug.Log("Error, No SphereJointGameObj0 found!");
                return;
            }

            sphereJointModel physicsModel = sphereJointObj.GetComponent<sphereJointModel>();
            if (!physicsModel)
            {
                Debug.Log("Error, No physics model of SphereJointGameObj0 found!");
                return;
            }

            if (!mesh.isBound || mesh.neighborSpheres.Count <= 0)
            {
                Debug.Log("Error: Mesh is not bound yet!");
                return;
            }

            // check mesh connectivity
            int layerNum = physicsModel.m_numLayers;*/
            int layerNum = 0;
            if (mesh.examineMeshFaceConnectivity(layerNum))
                Debug.Log("Mesh has good face connecivity");
            else
                Debug.Log("Mesh has bad face connecivity");
        }
    }

}
