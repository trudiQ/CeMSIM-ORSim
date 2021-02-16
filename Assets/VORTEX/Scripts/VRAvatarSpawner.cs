using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAvatarSpawner : MonoBehaviour
{
    public GameObject VRCamera;
    private Transform VRCameraTransform;

    public GameObject floorPositionObject { get; private set; }
    private Transform floorPositionObjectTransform;

    public GameObject[] avatarPrefabs;
    public int avatarSelection;
    public VRAvatar avatar { get; private set; }

    public GameObject VRLeftHand;
    public GameObject VRRightHand;

    void Start()
    {
        floorPositionObject = new GameObject(name: "VR Camera Floor Position");
        floorPositionObjectTransform = floorPositionObject.transform;

        try
        {
            VRCameraTransform = VRCamera.transform;
            floorPositionObjectTransform.SetPositionAndRotation(FindFloorPosition(), PlanarCameraRotation());
        }
        catch (System.NullReferenceException e)
        {
            VRCamera = Camera.main.gameObject;
            VRCameraTransform = VRCamera.transform;
            Debug.LogWarning("VR Camera not specified, using main camera." + e.ToString());
        }

        avatar = Instantiate(
            original: avatarPrefabs[avatarSelection], 
            parent: floorPositionObjectTransform, 
            position: floorPositionObjectTransform.position,
            rotation: floorPositionObjectTransform.rotation).GetComponentInChildren<VRAvatar>();

        avatar.SetFloorTargetAndCamera(target: floorPositionObjectTransform, camera: VRCameraTransform);
        avatar.SetIKTargets(_head: VRCameraTransform, _leftHand: VRLeftHand.transform, _rightHand: VRRightHand.transform);
    }

    void LateUpdate()
    {
        floorPositionObjectTransform.SetPositionAndRotation(FindFloorPosition(), PlanarCameraRotation());
    }

    private Vector3 FindFloorPosition()
    {
        RaycastHit hit;

        if(Physics.Raycast(
            origin: VRCameraTransform.position, 
            direction: Vector3.down, 
            maxDistance: int.MaxValue, 
            layerMask: LayerMask.GetMask("Ground"), 
            hitInfo: out hit))
        {
            return hit.point;
        }
        else
        {
            // Found no floor
            return Vector3.zero;
        }
    }

    private Quaternion PlanarCameraRotation()
    {
        Vector3 projected = Vector3.ProjectOnPlane(VRCameraTransform.forward, Vector3.up).normalized;
        Debug.DrawLine(floorPositionObjectTransform.position, floorPositionObjectTransform.position + projected * .5f, color: Color.red);

        return Quaternion.LookRotation(projected, Vector3.up);
    }
}
