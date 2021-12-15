using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ColonQuadColliderManager : MonoBehaviour
{
    public GameObject colonQuadPrefab;
    [ShowInInspector]
    public List<List<List<Transform>>> targetColon;

    public List<List<ColonQuadColliderBehavior>> colonQuadColliders;

    [ShowInInspector]
    public void CreateQuadColliders()
    {
        colonQuadColliders = new List<List<ColonQuadColliderBehavior>>();

        for (int c = 0; c < targetColon.Count; c++)
        {
            GameObject newColonQuads = new GameObject("Colon_" + c.ToString() + "_Quads");
            colonQuadColliders.Add(new List<ColonQuadColliderBehavior>());
            for (int l = 0; l < targetColon[0].Count - 1; l++)
            {
                for (int s = 0; s < targetColon[0][0].Count; s++)
                {
                    GameObject newQuad = Instantiate(colonQuadPrefab, newColonQuads.transform);
                    ColonQuadColliderBehavior newQuadBehavior = newQuad.GetComponent<ColonQuadColliderBehavior>();
                    newQuadBehavior.targetSpheres.Add(targetColon[c][l][s]);
                    int nextSphereIndex = s + 1;
                    if (s + 1 >= targetColon[0][0].Count)
                    {
                        nextSphereIndex = 0;
                    }
                    newQuadBehavior.targetSpheres.Add(targetColon[c][l][nextSphereIndex]);
                    newQuadBehavior.targetSpheres.Add(targetColon[c][l + 1][s]);
                    newQuadBehavior.targetSpheres.Add(targetColon[c][l + 1][nextSphereIndex]);
                    colonQuadColliders[c].Add(newQuadBehavior);
                }
            }
        }
    }
}
