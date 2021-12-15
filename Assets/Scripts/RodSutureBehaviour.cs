using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class RodSutureBehaviour : MonoBehaviour
{
    private NeedleState currentState;
    private GameObject currentSuturing;

    public ExitTriggerRod ExitTrigger;

    public GameObject p_ColliderObject;

    enum NeedleState
    {
        Free,
        Suturing,
        Exited
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = NeedleState.Free;
    }

    //Enter trigger enter
    public void StartSuturing(Collider c)
    {
        currentSuturing = c.gameObject;
        currentState = NeedleState.Suturing;
        gameObject.layer = 8;
    }

    Vector3 enterPosition;

    //Exit trigger enter
    public void StopSuturing(Collider c)
    {
        if (currentState != NeedleState.Suturing) return;
        currentState = NeedleState.Exited;

        enterPosition = ExitTrigger.transform.position;
        Vector3 forwardPosition = Vector3.Lerp(enterPosition, transform.position, 0.25f);
        Quaternion rot = Quaternion.Euler(c.transform.rotation.eulerAngles + new Vector3(0, 0, -90));
        Instantiate(p_ColliderObject, forwardPosition, rot);
    }

    //Exit trigger exit
    public void CreateNewSutureLine(Collider c, Transform n)
    {
        if (currentState != NeedleState.Exited) return;
        currentState = NeedleState.Free;
        gameObject.layer = 0;

        //Vector3 middlePoint = Vector3.Lerp(enterPosition, ExitTrigger.transform.position, 0.35f);

        //Instantiate(p_ColliderObject, middlePoint, transform.rotation);

    }
}
