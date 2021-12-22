using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ColonQuadColliderManager : MonoBehaviour
{
    public GameObject colonQuadPrefab;
    public List<Transform> targetColons;
    [ShowInInspector]
    public List<List<List<Transform>>> targetColonSpheres;

    public List<List<ColonQuadColliderBehavior>> colonQuadColliders;

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
    public void CreateQuadColliders()
    {
        colonQuadColliders = new List<List<ColonQuadColliderBehavior>>();

        for (int c = 0; c < targetColonSpheres.Count; c++)
        {
            GameObject newColonQuads = new GameObject("Colon_" + c.ToString() + "_Quads");
            newColonQuads.transform.parent = transform;
            colonQuadColliders.Add(new List<ColonQuadColliderBehavior>());
            for (int l = 0; l < targetColonSpheres[0].Count - 1; l++)
            {
                for (int s = 0; s < targetColonSpheres[0][0].Count; s++)
                {
                    GameObject newQuad = Instantiate(colonQuadPrefab, newColonQuads.transform);
                    ColonQuadColliderBehavior newQuadBehavior = newQuad.GetComponent<ColonQuadColliderBehavior>();
                    newQuadBehavior.targetSpheres.Add(targetColonSpheres[c][l][s]);
                    int nextSphereIndex = s + 1;
                    if (s + 1 >= targetColonSpheres[0][0].Count)
                    {
                        nextSphereIndex = 0;
                    }
                    newQuadBehavior.targetSpheres.Add(targetColonSpheres[c][l][nextSphereIndex]);
                    newQuadBehavior.targetSpheres.Add(targetColonSpheres[c][l + 1][s]);
                    newQuadBehavior.targetSpheres.Add(targetColonSpheres[c][l + 1][nextSphereIndex]);
                    colonQuadColliders[c].Add(newQuadBehavior);
                }
            }
        }
    }
}
