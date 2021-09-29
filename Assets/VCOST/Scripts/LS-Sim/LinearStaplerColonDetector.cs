using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LinearStaplerColonDetector : MonoBehaviour
{
    public Transform belongedStapler;
    public int belongedStaplerInd;
    public LinearStaplerTool staplerController;

    public Transform belongedStaplerInitialParent;
    public Vector3 belongedStaplerInitialLocalPosition;
    public Quaternion belongedStaplerInitialLocalRotation;
    public int touchingColonSpheres;
    public bool stopped; // If this LS part is stopped because surgeon trying to insertion without using forceps
    public List<int> staplerTipLastTouchedColonLayers; // What's the layers of the colon sphere touched by this trigger
    public float touchingLayerAverage;

    // Start is called before the first frame update
    void Start()
    {
        if (belongedStapler != null)
        {
            belongedStaplerInitialParent = belongedStapler.parent;
            belongedStaplerInitialLocalPosition = belongedStapler.localPosition;
            belongedStaplerInitialLocalRotation = belongedStapler.localRotation;
        }

        staplerTipLastTouchedColonLayers = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if (belongedStapler == null && staplerTipLastTouchedColonLayers.Count > 0)
        {
            touchingLayerAverage = (float)staplerTipLastTouchedColonLayers.Average();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("sphere_"))
        {
            touchingColonSpheres++;

            staplerTipLastTouchedColonLayers.Add(int.Parse(other.name[9].ToString()));

            if (belongedStapler != null)
            {
                int colon = int.Parse(other.name[7].ToString());
                // If stapler touches colon when colon is not secured by forceps and the stapler is not already inserting
                if (!staplerController.colonSecuredByForceps[colon] &&
                    (globalOperators.m_bInsert[0] != belongedStaplerInd + 1 && globalOperators.m_bInsert[1] != belongedStaplerInd + 1))
                {
                    StopStapler();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("sphere_"))
        {
            touchingColonSpheres--;

            staplerTipLastTouchedColonLayers.Remove(int.Parse(other.name[9].ToString()));

            if (belongedStapler != null)
            {
                if (touchingColonSpheres <= 0)
                {
                    touchingColonSpheres = 0;
                    ReenableStapler();
                }
            }
        }
    }

    public void StopStapler()
    {
        belongedStapler.parent = null;
        stopped = true;
    }

    public void ReenableStapler()
    {
        belongedStapler.parent = belongedStaplerInitialParent;
        belongedStapler.localPosition = belongedStaplerInitialLocalPosition;
        belongedStapler.localRotation = belongedStaplerInitialLocalRotation;
        stopped = false;
    }
}
