using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Obi;


public class HapticColonGrabbersBehavior : MonoBehaviour
{
    public GameObject p_grabber;
    public HapticPlugin hapticPlugin;
    public BlueprintParticleIndividualizer colon;
    public Transform grabberParent;

    public float maxGrabberDistance = 0.1f;
    
    public List<ForcepBehavior> attachedForceps = new List<ForcepBehavior>();
    public ForcepBehavior activeForceps;
    public bool canGrabObi = true;
    private bool canSpawnForceps = false;

    public UnityEvent _E_PlacedGrabber = new UnityEvent();

    private void Start()
    {
        activeForceps = grabberParent.GetChild(0).GetComponentInChildren<ForcepBehavior>();
        activeForceps.obiObject = colon;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddNewGrabber();
        }
    }

    public void SendOpenSignal(bool signal)
    {
        activeForceps.SignalOpen(signal);
    }

    public void SendCloseSignal(bool signal)
    {
        activeForceps.SignalClose(signal);
    }

    public void SetForcepSpawning(bool canSpawn) { }
    
    public void SwitchGrabberObject(ForcepBehavior grabber)
    {
        if(activeForceps != null)
        {
            Destroy(activeForceps.gameObject);
            activeForceps = null;
        }
        p_grabber = grabber.gameObject;
        AddNewGrabber();
    }

    public void SwitchToObject(GameObject obj)
    {
        Destroy(activeForceps.gameObject);
        hapticPlugin.hapticManipulator = obj;
        gameObject.transform.SetParent(grabberParent);
    }

    public void AddNewGrabber()
    {
        //if (currentGrabberAttachment == null) return;

        //Freeze old grabber
        if (activeForceps != null)
        {
            attachedForceps.Add(activeForceps);
            FreezeGrabber(activeForceps.transform);
        }

        //Add new grabber to control
        activeForceps = Instantiate(
            p_grabber, 
            Vector3.up*-100f, 
            Quaternion.identity, 
            grabberParent).
            GetComponentInChildren<ForcepBehavior>();
        activeForceps.obiObject = colon;
        hapticPlugin.hapticManipulator = activeForceps.gameObject;
        activeForceps.doObi = canGrabObi;
    }

    private bool FreezeGrabber(Transform grabber)
    {
        Rigidbody r = grabber.GetComponent<Rigidbody>();
        if (r == null) return false;

        r.constraints = RigidbodyConstraints.FreezeAll;
        return true;
    }
}
