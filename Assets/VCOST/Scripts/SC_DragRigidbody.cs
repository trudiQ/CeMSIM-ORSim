using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

public class SC_DragRigidbody : MonoBehaviour
{
    public float forceAmount = 500;
    public float rotateMult = 4f;
    
    public RodBlueprintParticleIndividualizer rod;
    private float maxTension = 1.5f;

    Rigidbody selectedRigidbody;
    List<PositionConstraint> heldRigidbodies;
    Camera targetCamera;
    Vector3 originalScreenTargetPosition;
    Vector3 originalRigidbodyPos;
    float selectionDistance;

    // Start is called before the first frame update
    void Start()
    {
        targetCamera = GetComponent<Camera>();
        heldRigidbodies = new List<PositionConstraint>();
    }

    void Update()
    {
        if (!targetCamera)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            //Check if we are hovering over Rigidbody, if so, select it
            selectedRigidbody = GetRigidbodyFromMouseClick();

            if (selectedRigidbody != null)
            {
                NeedleBehavior n = selectedRigidbody.GetComponent<NeedleBehavior>();
                if (n != null) n.e_needleGrab.Invoke();
            }
        }
        if (Input.GetMouseButtonUp(0) && selectedRigidbody)
        {
            NeedleBehavior n = selectedRigidbody.GetComponent<NeedleBehavior>();
            if (n != null) n.e_needleRelease.Invoke();

            //Release selected Rigidbody if there any
            selectedRigidbody = null;
        }
    }

    //remap value, inMin, inMax, outMin, outMax
    float map(float s, float a1, float b1, float a2, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    void FixedUpdate()
    {
        if (selectedRigidbody != null)
        {
            Vector3 mousePositionOffset = targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, selectionDistance)) - originalScreenTargetPosition;
            selectedRigidbody.velocity = (originalRigidbodyPos + mousePositionOffset - originalCollider.transform.position) * forceAmount * Time.deltaTime;
            selectedRigidbody.angularVelocity = new Vector3(0, 0, Input.GetAxis("Horizontal")) * rotateMult;
        }
    }

    private Collider originalCollider;
    Rigidbody GetRigidbodyFromMouseClick()
    {
        RaycastHit hitInfo = new RaycastHit();
        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        bool hit = Physics.Raycast(ray, out hitInfo);
        if (hit)
        {
            Rigidbody r = hitInfo.collider.gameObject.GetComponentInParent<Rigidbody>();
            if (r)
            {
                selectionDistance = Vector3.Distance(ray.origin, hitInfo.point);
                originalScreenTargetPosition = targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, selectionDistance));
                originalRigidbodyPos = hitInfo.collider.transform.position;
                originalCollider = hitInfo.collider;
                return r;
            }
        }
        return null;
    }
}