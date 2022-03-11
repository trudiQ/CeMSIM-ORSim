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
    public float staticAnchorEnableDistance; // How far the sphere of an anchor away from belonged column line for static anchors to be active
    public float staticAnchorDisableDistance; // How close the sphere of an anchor to belonged column line for static anchors to be inactive
    public float staplerTiltEnableDistance; // How much higher the stapler tip compare to stapler end for static anchors to be active
    public float staplerTiltDisableDistance; // How much less higher the stapler tip compare to stapler end for static anchors to be active
    public float anchorRigidBodyMass;
    public float anchorJointMassScale;
    public float staticAnchorConnectingForce;

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
            a => a.anchorForNeighborSpheres = HapticSurgTools.GetNeighborColonSphere(a.targetSphere.transform).Select(s => GetSphereAnchor(s)).ToList());

        // Set other values
        colonJointAnchors.ForEach(a => a.InitializeAnchor());
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

    public void EnableAnchor(ColonStaplerJointBehavior anchor, Transform staplerStart, Transform staplerEnd, Transform stapler, StaplerColonSphereTrigger staplerTrigger, Vector3 anchorInitialPos, float detachDist)
    {
        anchor.followedStaplerStart = staplerStart;
        anchor.followedStaplerEnd = staplerEnd;
        anchor.followedStapler = stapler;
        anchor.followedStaplerCollisionTrigger = staplerTrigger;
        anchor.transform.position = anchorInitialPos;
        anchor.detachDistance = detachDist;
        anchor.gameObject.SetActive(true);
        activeAnchors.Add(anchor);
        anchor.AttachColonSphere();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collidingStapler"></param>
    /// <returns></returns>
    public List<ColonStaplerJointBehavior> SwitchJointAnchorStatic(ColonStaplerJointBehavior leadAnchor, StaplerColonSphereTrigger collidingStapler, bool isStatic)
    {
        List<ColonStaplerJointBehavior> updatedSphereAnchors = new List<ColonStaplerJointBehavior>();

        if (isStatic)
        {
            updatedSphereAnchors = collidingStapler.touchingSpheres.Select(s => GetSphereAnchor(s)).ToList();

            List<ColonStaplerJointBehavior> activeAnchorsCopy = new List<ColonStaplerJointBehavior>(activeAnchors);
            // Detach other anchors to avoid physics glitch
            foreach (ColonStaplerJointBehavior a in activeAnchorsCopy)
            {
                if (a.targetSphereColon == leadAnchor.targetSphereColon && !updatedSphereAnchors.Contains(a))
                {
                    a.DetachColonSphere();
                }
            }

            foreach (ColonStaplerJointBehavior a in updatedSphereAnchors)
            {
                if (!a.gameObject.activeInHierarchy)
                {
                    //EnableAnchor(a, leadAnchor.followedStaplerStart, leadAnchor.followedStaplerEnd, leadAnchor.followedStapler, leadAnchor.followedStaplerCollisionTrigger, a.transform.position, leadAnchor.detachDistance);
                }
            }
            collidingStapler.staticAnchors = updatedSphereAnchors;
            updatedSphereAnchors.ForEach(a => a.SwitchStatic(isStatic));
        }
        else
        {
            updatedSphereAnchors = collidingStapler.staticAnchors;
            updatedSphereAnchors.ForEach(a => a.SwitchStatic(isStatic));
            // Detach other anchors to avoid physics glitch
            updatedSphereAnchors.ForEach(a => a.DetachColonSphere());

            collidingStapler.staticAnchors.Clear();

            // ### TEST
            print("switch static to false");
        }

        return updatedSphereAnchors;
    }

    public ColonStaplerJointBehavior GetSphereAnchor(Transform sphere)
    {
        return colonJointAnchors.Find(j => j.targetSphere.transform == sphere);
    }

    [ShowInInspector]
    public void AdjustAnchorPhysics()
    {
        foreach (ColonStaplerJointBehavior a in colonJointAnchors)
        {
            Rigidbody aR = a.GetComponent<Rigidbody>();
            FixedJoint aJ = a.GetComponent<FixedJoint>();
            aR.mass = anchorRigidBodyMass;
            aJ.massScale = anchorJointMassScale;
        }
    }
}
