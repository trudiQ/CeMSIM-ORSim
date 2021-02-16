using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;

public class VRAvatarSpawner : MonoBehaviour
{
    // Variables that hold the main VR camera
    public GameObject VRCamera;
    private Transform VRCameraTransform;

    // Variables that hold the object that gives the position on the floor under the camera
    public GameObject floorPositionObject { get; private set; }
    private Transform floorPositionObjectTransform;

    // User avatar prefabs and selection index
    public GameObject[] avatarPrefabs;
    public int avatarSelection;
    public VRAvatar avatar { get; private set; }

    void Start()
    {
        // Get which objects are being used for the VR hands
        XRManager manager = FindObjectOfType<XRManager>();
        VRHands hands = manager.GetVRHands();

        if (hands == null)
        {
            // Subscribe to the event that returns the VRHands object when they are found
            manager.onHeadsetDetected += OnHeadsetDetected;
        }
        else
        {
            // Set the hands if they are already specified
            OnHeadsetDetected(hands);
        }

        // Create a GameObject that is always located at the floor under the camera
        floorPositionObject = new GameObject(name: "VR Camera Floor Position");
        floorPositionObjectTransform = floorPositionObject.transform;

        // If there is no camera specified, use the default main camera
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

        // Spawn the selected prefab and set the floor/camera object
        avatar = Instantiate(
            original: avatarPrefabs[avatarSelection], 
            parent: floorPositionObjectTransform, 
            position: floorPositionObjectTransform.position,
            rotation: floorPositionObjectTransform.rotation).GetComponentInChildren<VRAvatar>();

        avatar.SetFloorTargetAndCamera(target: floorPositionObjectTransform, camera: VRCameraTransform);
    }

    void LateUpdate()
    {
        // Move the floor object to the floor point directly under the camera and rotate it based on camera rotation
        floorPositionObjectTransform.SetPositionAndRotation(FindFloorPosition(), PlanarCameraRotation());
    }

    // Find the point where the floor is based on camera rotaiton
    private Vector3 FindFloorPosition()
    {
        if (Physics.Raycast(
            origin: VRCameraTransform.position,
            direction: Vector3.down,
            maxDistance: int.MaxValue,
            layerMask: LayerMask.GetMask("Ground"),
            hitInfo: out RaycastHit hit))
        {
            return hit.point;
        }
        else
        {
            // Found no floor
            return Vector3.zero;
        }
    }

    // Function that sets the IK targets of the avatar
    private void OnHeadsetDetected(VRHands hands)
    {
        avatar.SetIKTargets(_head: VRCameraTransform, _leftHand: hands.leftHand.transform, _rightHand: hands.rightHand.transform);
    }

    // Project the camera's rotation on a plane parallel to a flat floor
    private Quaternion PlanarCameraRotation()
    {
        Vector3 projected = Vector3.ProjectOnPlane(VRCameraTransform.forward, Vector3.up).normalized;
        Debug.DrawLine(floorPositionObjectTransform.position, floorPositionObjectTransform.position + projected * .5f, color: Color.red);

        return Quaternion.LookRotation(projected, Vector3.up);
    }
}
