using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SWR_Disc : MonoBehaviour
{
    public float jointRadius = 0.2f;
    Mesh skinMesh;
    public List<GameObject> objects;
    public GameObject skinObject;
    Vector3[] originalVertices;
    Vector3[] verticesTempRot;

    public bool directionIsPositiveX = false;
    public bool useMarkers = true;
    public GameObject p_Marker;

    private Bone[] bones;
    private VertexBoneAttachment[] vertexBoneAttachments;

    bool computeWeight3(Vector3 boneLocation, Vector3 vertexPoint, ref float weight)
    {
        float distance = Vector3.Distance(boneLocation, vertexPoint);
        weight = Mathf.Exp(-20.0f * (distance * distance));
        return true;
    }
    public VertexBoneAttachment[] getVertexAttachments()
    {

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

    void TrimObjects()
    {
        float desiredX;
        if (directionIsPositiveX)
        {
            desiredX = float.MinValue;
            foreach (GameObject g in objects)
            {
                if (g.transform.position.x > desiredX) desiredX = g.transform.position.x;
            }
        }
        else
        {
            desiredX = float.MaxValue;
            foreach (GameObject g in objects)
            {
                if (g.transform.position.x < desiredX) desiredX = g.transform.position.x;
            }
        }

        List<GameObject> newObjects = new List<GameObject>();
        float fuzzDistance = 0.05f;
        int skipAmount = 4;
        int currentSkip = 0;
        foreach(GameObject g in objects)
        {
            float xpos = g.transform.position.x;
            if (Mathf.Abs(desiredX - xpos) > fuzzDistance) continue;
            if (currentSkip != 0)
            {
                if (currentSkip == skipAmount)
                {
                    currentSkip = 0;
                }
                else
                {
                    currentSkip++;
                }
            }
            else
            {
                newObjects.Add(g);
            }
        }

        objects = newObjects;
    }

    void PlaceMarkers()
    {
        foreach(GameObject g in objects)
        {
            GameObject marker = Instantiate(p_Marker, g.transform.position, Quaternion.identity, g.transform);
        }
    }

    // Use this for initialization
    public void Start()
    {
        objects = new List<GameObject>();
        GetChildrenRecursively(transform);
        TrimObjects();
        if (useMarkers) PlaceMarkers();
        skinMesh = skinObject.GetComponent<MeshFilter>().mesh;
        skinMesh.MarkDynamic();

        initSkeleton(skinMesh, skinObject.transform);
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
        animation(skinMesh);
    }
}
