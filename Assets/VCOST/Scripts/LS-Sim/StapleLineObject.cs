using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StapleLineObject : MonoBehaviour
{
    public Transform belongedObjet; // The transform of the mesh object it should attach to
    public Mesh belongedMesh;
    public int belongedTriangleIndex; // Use (belongedTriangleIndex * 3) and (belongedTriangleIndex * 3 + 1) and (belongedTriangleIndex * 3 + 2) to get the vertices

}
