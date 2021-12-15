using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColonQuadColliderBehavior : MonoBehaviour
{
    public List<int> quadTris;

    public List<Transform> targetSpheres; // Sphere order: 0: smaller layer smaller sphere 1: small lay big sph, 2: big l smal sph, 3: big l big sp
    public bool followSphere; // Should this quad follow sphere position, or vise versa
    public MeshFilter quadMesh;
    public MeshCollider quadCollider;
    public Rigidbody quadRigid;

    // Start is called before the first frame update
    void Start()
    {
        followSphere = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (followSphere)
        {
            UpdateQuad();
        }
        else
        {
            UpdateSphere();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if collision from inserting LS
        followSphere = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        // if collision from inserting LS
        followSphere = true;
    }

    public void UpdateQuad()
    {
        // Reset physics
        quadRigid.velocity = Vector3.zero;
        quadRigid.angularVelocity = Vector3.zero;

        // Center transform
        transform.position = (targetSpheres[0].position + targetSpheres[1].position + targetSpheres[2].position + targetSpheres[3].position) / 4;

        // Update quad mesh 
        quadMesh.mesh.Clear();
        quadMesh.mesh.vertices = targetSpheres.Select(t => transform.InverseTransformPoint(t.position)).ToArray();
        quadMesh.mesh.triangles = quadTris.ToArray();

        // Update collider mesh (maybe no need)
    }

    /// <summary>
    /// Make the spheres follow its position
    /// </summary>
    public void UpdateSphere()
    {
        for (int i = 0; i < targetSpheres.Count; i++)
        {
            targetSpheres[i].position = transform.TransformPoint(quadMesh.mesh.vertices[i]);
        }
    }
}
