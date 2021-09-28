using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearStaplerColonDetector : MonoBehaviour
{
    public Transform belongedStapler;
    public LinearStaplerTool staplerController;

    public Transform belongedStaplerInitialParent;
    public Vector3 belongedStaplerInitialLocalPosition;
    public Quaternion belongedStaplerInitialLocalRotation;
    public int touchingColonSpheres;
    public bool stopped; // If this LS part is stopped because surgeon trying to insertion without using forceps

    // Start is called before the first frame update
    void Start()
    {
        belongedStaplerInitialParent = belongedStapler.parent;
        belongedStaplerInitialLocalPosition = belongedStapler.localPosition;
        belongedStaplerInitialLocalRotation = belongedStapler.localRotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("sphere_"))
        {
            touchingColonSpheres++;

            if (!staplerController.colonSecuredByForceps[int.Parse(other.name[7].ToString())])
            {
                StopStapler();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("sphere_"))
        {
            touchingColonSpheres--;

            if (touchingColonSpheres <= 0)
            {
                touchingColonSpheres = 0;
                ReenableStapler();
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
