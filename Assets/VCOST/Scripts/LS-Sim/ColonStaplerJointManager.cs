using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ColonStaplerJointManager : MonoBehaviour
{
    public GameObject colonJointAnchorPrefab;
    public List<Transform> targetColons;
    [ShowInInspector]
    public List<List<List<Transform>>> targetColonSpheres;
    public globalOperators gOperator;

    public List<List<ColonStaplerJointBehavior>> colonJointAnchors;
    public static ColonStaplerJointManager instance;

    private void Start()
    {
        instance = this;
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
        colonJointAnchors = new List<List<ColonStaplerJointBehavior>>();

        for (int c = 0; c < targetColonSpheres.Count; c++)
        {
            GameObject newColonQuads = new GameObject("Colon_" + c.ToString() + "_Anchors");
            newColonQuads.transform.parent = transform;
            colonJointAnchors.Add(new List<ColonStaplerJointBehavior>());
            for (int l = 0; l < targetColonSpheres[0].Count; l++)
            {
                for (int s = 0; s < targetColonSpheres[0][0].Count; s++)
                {
                    GameObject newAnchor = Instantiate(colonJointAnchorPrefab, newColonQuads.transform);
                    ColonStaplerJointBehavior newAnchorBehavior = newAnchor.GetComponent<ColonStaplerJointBehavior>();
                    newAnchorBehavior.targetSphere = targetColonSpheres[c][l][s].GetComponent<Rigidbody>();

                    colonJointAnchors[c].Add(newAnchorBehavior);
                }
            }
        }
    }

    ///// <summary>
    ///// Update which colon anchor object should be active based on colon insertion depth and spacing
    ///// </summary>
    //public void UpdateActiveJointAnchor()
    //{

    //}

    //public void 
}
