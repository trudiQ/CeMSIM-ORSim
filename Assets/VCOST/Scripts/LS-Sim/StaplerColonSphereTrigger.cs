using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaplerColonSphereTrigger : MonoBehaviour
{

    public List<Transform> touchingSpheres;
    public List<ColonStaplerJointBehavior> staticAnchors;
    public Transform belongedStapler;
    public bool isConnectingColon; // Does the inserted colon have static anchor
    public bool connectedByTilt; // Is the colon anchors become static because of stapler tilt or sphere position

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }

    /// <summary>
    /// Rotate the trigger towards a position, but keep its alignment with the stapler
    /// </summary>
    /// <param name="target"></param>
    public void RotateToward(Vector3 target)
    {
        Ray staplerRay = new Ray(belongedStapler.position, belongedStapler.right);
        Vector3 targetProjectionOnStapler = MathUtil.ProjectionPointOnLine(staplerRay, target);
        Vector3 lookPos = target - targetProjectionOnStapler + transform.position;
        transform.LookAt(lookPos, -belongedStapler.right);
    }
}
