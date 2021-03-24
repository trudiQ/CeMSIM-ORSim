using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class testEditor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // create game object to the scene
    [MenuItem("GameObject/Create Other/Floor")]
    static void addFloor()
    {
        GameObject Floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Floor.name = "Floor";
        Floor.transform.position = new Vector3(0, 0, 0);
        Floor.transform.rotation = Quaternion.identity;
        Floor.transform.localScale = new Vector3(5, 1, 5);
        // material
        Material mat = Resources.Load("Materials/floor_diffuse", typeof(Material)) as Material;
        Renderer rend = Floor.GetComponent<Renderer>();
        rend.material = mat;
        //EditorUtility.DisplayDialog("Yay!", "We're all done!", "Awesome!");
    }

    [MenuItem("GameObject/Create Other/SphereSkeleton")]
    static void addSphereSkeleton()
    {
        /// create layer: spheres, joints
        GameObject sphereJointGO = new GameObject("sphereJointGameObj0");
        sphereJointModel sphereJointModel = sphereJointGO.AddComponent<sphereJointModel>();
        sphereJointModel.initialize(0, 0.0f, ref sphereJointGO);

        // operations
        GameObject globalOperators = new GameObject("globalOperators");
        globalOperators.AddComponent<globalOperators>();
        globalOperators.GetComponent<globalOperators>().m_numSphereModels = 1;
    }

    // Add 2 spheresSkeletons 
    [MenuItem("GameObject/Create Other/SphereSkeleton2")]
    static void addSphereSkeleton2()
    {
        /// create layer: spheres, joints
        GameObject sphereJointGO0 = new GameObject("sphereJointGameObj0");
        sphereJointModel sphereJointModel0 = sphereJointGO0.AddComponent<sphereJointModel>();
        sphereJointModel0.initialize(0, 0.0f, ref sphereJointGO0);
        GameObject sphereJointGO1 = new GameObject("sphereJointGameObj1");
        sphereJointModel sphereJointModel1 = sphereJointGO1.AddComponent<sphereJointModel>();
        sphereJointModel1.initialize(1, 3.5f, ref sphereJointGO1);

        // operations
        GameObject globalOperators = new GameObject("globalOperators");
        globalOperators.AddComponent<globalOperators>();
        globalOperators.GetComponent<globalOperators>().m_numSphereModels = 2;
    }

    [MenuItem("GameObject/Create Other/Other Stuff")]
    static void OtherStuff()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0.5f, 0);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 1.5f, 0);

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.position = new Vector3(2, 1, 0);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = new Vector3(-2, 1, 0);
        EditorUtility.DisplayDialog("Other Stuff", "Doing other stuff", "Mmmmkay");
    }
}
