using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public class ColonStaplerJointManager : MonoBehaviour
{
    public GameObject colonJointAnchorPrefab;
    public List<Transform> targetColons;
    [ShowInInspector]
    public List<List<List<Transform>>> targetColonSpheres;
    public globalOperators gOperator;
    public float insertionDepthDetatch;
    public float layerDisplacementDetatch;
    public float staticAnchorEnableDistance; // How far the sphere of an anchor away from main colon for static anchor to be active
    public float staticAnchorDisableDistance; // How close the sphere of an anchor to main colon for static anchor to be inactive

    public List<ColonStaplerJointBehavior> colonJointAnchors;
    public static ColonStaplerJointManager instance;
    public List<ColonStaplerJointBehavior> activeAnchors;
    public List<ColonStaplerJointBehavior> staticCollisionJoints; // Fixedjoints created between the stapler and the colon sphere that's been pushed further away to prevent stapler penetration

    private void Start()
    {
        instance = this;

        InitializeAnchorInfo();
    }

    public void DeactivateAllAnchors()
    {
        colonJointAnchors.ForEach(a => a.DetachColonSphere());
    }

    public bool IsNeighborAnchorActive(ColonStaplerJointBehavior centerAnchor)
    {
        return centerAnchor.anchorForNeighborSpheres.Find(t => t.gameObject.activeInHierarchy);
    }
    public bool IsNeighborAnchorActive(int centerAnchorIndex)
    {
        return colonJointAnchors[centerAnchorIndex].anchorForNeighborSpheres.Find(t => t.gameObject.activeInHierarchy);
    }

    [ShowInInspector]
    public void GetColonSpheres()
    {
        targetColonSpheres = new List<List<List<Transform>>>();
        for (int c = 0; c < targetColons.Count; c++)
        {
            targetColonSpheres.Add(new List<List<Transform>>());
            for (int l = 0; l < 20; l++)
            {
                targetColonSpheres[c].Add(new List<Transform>());
                Transform layer = targetColons[c].GetChild(l);
                for (int s = 0; s < 20; s++)
                {
                    targetColonSpheres[c][l].Add(layer.GetChild(s));
                }
            }
        }
    }

    [ShowInInspector]
    public void CreateAnchorObjects()
    {
        colonJointAnchors = new List<ColonStaplerJointBehavior>();

        for (int c = 0; c < targetColonSpheres.Count; c++)
        {
            GameObject newColonAnchor = new GameObject("Colon_" + c.ToString() + "_Anchors");
            newColonAnchor.transform.parent = transform;
            for (int l = 0; l < targetColonSpheres[0].Count; l++)
            {
                for (int s = 0; s < targetColonSpheres[0][0].Count; s++)
                {
                    GameObject newAnchor = Instantiate(colonJointAnchorPrefab, newColonAnchor.transform);
                    ColonStaplerJointBehavior newAnchorBehavior = newAnchor.GetComponent<ColonStaplerJointBehavior>();
                    newAnchorBehavior.targetSphere = targetColonSpheres[c][l][s].GetComponent<Rigidbody>();

                    colonJointAnchors.Add(newAnchorBehavior);
                }
            }
        }
    }

    //[ShowInInspector]
    public void InitializeAnchorInfo()
    {
        //globalOperators.instance = gOperator;
        //foreach (ColonStaplerJointBehavior anchor in colonJointAnchors)
        //{
        //    List<Transform> neighbor = HapticSurgTools.GetNeighborColonSphere(a.targetSphere.transform);
        //    anchor.anchorForNeighborSpheres = colonJointAnchors.FindAll(j => neighbor.Contains(j.targetSphere.transform)).Select(j => j.transform).ToList();
        //}

        // Get neighbor anchors
        colonJointAnchors.ForEach(
            a => a.anchorForNeighborSpheres = HapticSurgTools.GetNeighborColonSphere(a.targetSphere.transform).Select(s => colonJointAnchors.Find(j => j.targetSphere.transform == s)).ToList());

        // Get target sphere layer
        colonJointAnchors.ForEach(a => a.targetSphereLayer = globalOperators.GetSphereLayer(a.targetSphere.name));

        // Get target sphere colon
        colonJointAnchors.ForEach(a => a.targetSphereColon = int.Parse(a.targetSphere.name[7].ToString()));
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="targetSphere"></param>
    ///// <param name="followedStapler"></param>
    ///// <param name="relativeStaplerPosition"></param>
    ///// <returns></returns>
    //public ColonStaplerJointBehavior CreateStaticJointObject(Rigidbody targetSphere, Transform followedStapler, Vector3 relativeStaplerPosition)
    //{
    //    GameObject newAnchor = Instantiate(colonJointAnchorPrefab, transform);
    //    ColonStaplerJointBehavior newAnchorBehavior = newAnchor.GetComponent<ColonStaplerJointBehavior>();
    //    newAnchorBehavior.isStaticCollisionJoint = true;
    //    newAnchorBehavior.targetSphere = targetSphere;
    //    staticCollisionJoints.Add(newAnchorBehavior);

    //    return newAnchorBehavior;
    //}

    ///// <summary>
    ///// Update which colon anchor object should be active based on colon insertion depth and spacing
    ///// </summary>
    //public void UpdateActiveJointAnchor()
    //{

    //}

    //public void 
}
