using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is for bone declarations. They are moved to the definitions C#
/// </summary>
/// 
public class VertexBoneAttachment
{
    public List<int> attachedBones = new List<int>();
    public List<float> attachedWeights = new List<float>();
    public float totalWeights = 0.0f;

}
public class Bone
{
    public Vector3 initialPos = new Vector3();
    public Quaternion initialRot = new Quaternion();
    public int nbrVertexAttachments = 0;
    //public Transform initialTransform = new Transform();
    //public List<int> vertexIndex = new List<int>();//in case needed we will have attachment to the vertices.
    //public List<float> weights = new List<float>();//in case needed we will have attachment to the weights of the vertices.
}
public class skeleton_withRotation : MonoBehaviour
{
    public int doCustom = 0;
    public float jointRadius = 0.2f;
    Mesh skinMesh;
    public List<GameObject> objects;
    public GameObject skinObject;
    Vector3[] originalVertices;
    Vector3[] verticesTempRot;

    public bool drawGizmos = false;
    public bool drawinEditorMode = false;

    private Bone[] bones;
    private VertexBoneAttachment[] vertexBoneAttachments;

    bool computeWeight3(Vector3 boneLocation, Vector3 vertexPoint, ref float weight)
    {
        if (doCustom == 0 || doCustom == 1)
        {
            float distance;
            distance = Vector3.Distance(boneLocation, vertexPoint);
            if (distance <= jointRadius)
            {
                weight = 1.0f;

                return true;
            }
            else if (distance <= jointRadius * 2.0)
            {

                weight = Mathf.Exp(-20.0f * (distance * distance));
                weight = 1f;
                return true;
            }
            else
                return false;
        }
        else if (doCustom==2)
        {
            float distance;
            distance = Vector3.Distance(boneLocation, vertexPoint);
            if (distance <= jointRadius * 2.0)
            {

                weight = Mathf.Exp(-(distance * distance) / (jointRadius * jointRadius * jointRadius * jointRadius));
                return true;
            }
            else
                return false;
        }
        else if(doCustom==3)
        {
            float distance;
            distance = Vector3.Distance(boneLocation, vertexPoint);
            if (distance <= jointRadius * 2.0)
            {

                weight = distance;
                if (weight >= jointRadius)
                    weight = Mathf.Exp(-(distance * distance) / (jointRadius * jointRadius * jointRadius * jointRadius));
                else
                    weight = 1.0f;
                return true;
            }
            else
                return false;
        }
        else
        {
            float distance = Vector3.Distance(boneLocation, vertexPoint);
            weight = Mathf.Exp(-20.0f * (distance * distance));
            return true;
        }
    }
    public    VertexBoneAttachment[] getVertexAttachments() {

        return vertexBoneAttachments;
    }
    public Bone[] getBones()
    {

        return bones;
    }
    void initSkeleton(Mesh mesh, Transform trans)
    {
        float weight = 0.0f;


        originalVertices = new Vector3[mesh.vertexCount];
        verticesTempRot = new Vector3[mesh.vertexCount];

        bones = new Bone[objects.Count];
        vertexBoneAttachments = new VertexBoneAttachment[mesh.vertexCount];


        for (int i = 0; i < mesh.vertexCount; i++)
        {

            vertexBoneAttachments[i] = new VertexBoneAttachment();
            originalVertices[i] = (trans.position + trans.rotation * Vector3.Scale(mesh.vertices[i], trans.localScale));


        }
        for (int j = 0; j < objects.Count; j++)
        {
            bones[j] = new Bone();
            bones[j].initialPos = objects[j].transform.position;
            bones[j].initialRot = objects[j].transform.rotation;
            bones[j].nbrVertexAttachments = 0;

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (computeWeight3(objects[j].transform.position, originalVertices[i], ref weight))
                {


                    vertexBoneAttachments[i].attachedBones.Add(j);
                    vertexBoneAttachments[i].attachedWeights.Add(weight);
                    vertexBoneAttachments[i].totalWeights += weight;
                    bones[j].nbrVertexAttachments++;
                }

            }




        }

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            for (int j = 0; j < vertexBoneAttachments[i].attachedWeights.Count; j++)
            {
                vertexBoneAttachments[i].attachedWeights[j] = vertexBoneAttachments[i].attachedWeights[j] / vertexBoneAttachments[i].totalWeights;
            }

            vertexBoneAttachments[i].totalWeights = 1.0f;
        }
        skinObject.transform.position = new Vector3(0, 0, 0);
        skinObject.transform.rotation = Quaternion.identity;
        skinObject.transform.localScale = new Vector3(1, 1, 1);
        mesh.vertices = originalVertices;
    }

    void GetChildrenRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child == null) continue;
            objects.Add(child.gameObject);
            GetChildrenRecursively(child);
        }
    }

    // Use this for initialization
    bool on = false;
    public void Go()
    {
        objects = new List<GameObject>();
        GetChildrenRecursively(transform);
        skinMesh = skinObject.GetComponent<MeshFilter>().mesh;
        skinMesh.MarkDynamic();

        initSkeleton(skinMesh, skinObject.transform);

        on = true;
    }



    void animation(Mesh mesh)
    {

       
        int numberOfVertices = 0;
        int numberOfBones;
        float timeBeginning;

        float timeEnd = 0.0f;
        float timeDifference = 0.0f;
        timeBeginning = Time.realtimeSinceStartup;
      


        numberOfVertices = mesh.vertices.Length;


        for (int i = 0; i < numberOfVertices; i++)
        {
            //verticesTemp[i].Set(0, 0, 0);
            verticesTempRot[i].Set(0, 0, 0);
            numberOfBones = vertexBoneAttachments[i].attachedBones.Count;
            Vector3 temp;
            if (numberOfBones > 0)
            {
                for (int j = 0; j < numberOfBones; j++)
                {
                    //relative = Quaternion.Inverse(bones[vertexBoneAttachments[i].attachedBones[j]].initialRot) * objects[vertexBoneAttachments[i].attachedBones[j]].transform.rotation;
                    //relative = Quaternion.Lerp(Quaternion.identity, relative, vertexBoneAttachments[i].attachedWeights[j] / vertexBoneAttachments[i].totalWeights);
                    //verticesTemp[i]+=(objects[vertexBoneAttachments[i].attachedBones[j]].transform.position- bones[vertexBoneAttachments[i].attachedBones[j]].initialPos )* vertexBoneAttachments[i].attachedWeights[j]/ vertexBoneAttachments[i].totalWeights;
                    temp = (Quaternion.Inverse(bones[vertexBoneAttachments[i].attachedBones[j]].initialRot)) * (originalVertices[i] - bones[vertexBoneAttachments[i].attachedBones[j]].initialPos);
                    temp = (vertexBoneAttachments[i].attachedWeights[j] / vertexBoneAttachments[i].totalWeights) * (objects[vertexBoneAttachments[i].attachedBones[j]].transform.position + objects[vertexBoneAttachments[i].attachedBones[j]].transform.rotation * temp);
                    verticesTempRot[i] += temp;
                    //Debug.Log(" " + vertexBoneAttachments[i].attachedWeights[j]+" i:"+i);

                }
            }
            else
              verticesTempRot[i] = originalVertices[i];
            //verticesTempRot[i] = verticesTempRot[i] / vertexBoneAttachments[i].totalWeights;
            //verticesTemp[i] = verticesTemp[i] + verticesTempRot[i] + originalVertices[i];
            //verticesTemp[i] = verticesTempRot[i];
        }
        timeEnd = Time.realtimeSinceStartup;
        timeDifference = timeEnd - timeBeginning;
        //Debug.Log("Time Dif:" + timeDifference);
        mesh.vertices = verticesTempRot;
        //	Debug.Log("Vertices:"+mesh.vertices[0]);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (!on) return;
        animation(skinMesh);
    }


    void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                //Gizmos.DrawSpheres(objects[i].transform.position, jointRadius);
            }
        }

    }
}
