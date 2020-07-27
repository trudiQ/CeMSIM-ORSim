using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonV2 : MonoBehaviour
{
    public float MinLocalY = 0.25f;
    public float MaxLocalY = 0.55f;
    public float ClickTolerance = 0.01f;

    List<XRDirectInteractor> XRDirectInteractors;
    SpringJoint joint;

    bool clickingDown = false;
    public AudioClip ButtonClick;
    public AudioClip ButtonClickUp;

    public UnityEvent onButtonDown;
    public UnityEvent onButtonUp;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        XRDirectInteractors = new List<XRDirectInteractor>();
        joint = GetComponent<SpringJoint>();

        // Start with button up top / popped up
        transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

        audioSource = GetComponent<AudioSource>();
    }

    // These have been hard coded for hand speed
    float ButtonSpeed = 15f;
    float SpringForce = 1500f;

    // Update is called once per frame
    void Update()
    {
        Vector3 buttonDownPosition = new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
        Vector3 buttonUpPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
        bool XRDirectInteractorInButton = false;

        // Find a valid grabber to push down
        foreach (var XRDI in XRDirectInteractors)
        {
            if (!XRDI.GetComponent<GrabStatus>().isCurrentGrab)
            {
                XRDirectInteractorInButton = true;
                break;
            }
        }

        if (XRDirectInteractorInButton)
        {
            float speed = ButtonSpeed; //;framesInGrabber < 3 ? 5f : ButtonSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, buttonDownPosition, speed * Time.deltaTime);
            joint.spring = 0;
        }
        else
        {
            joint.spring = SpringForce;
        }

        // Cap values
        if (transform.localPosition.y < MinLocalY)
        {
            transform.localPosition = buttonDownPosition;
        }
        else if (transform.localPosition.y > MaxLocalY)
        {
            transform.localPosition = buttonUpPosition;
        }

        // Click Down?
        float buttonDownDistance = transform.localPosition.y - buttonDownPosition.y;
        if (buttonDownDistance <= ClickTolerance && !clickingDown)
        {
            clickingDown = true;
            OnButtonDown();
        }
        // Click Up?
        float buttonUpDistance = buttonUpPosition.y - transform.localPosition.y;
        if (buttonUpDistance <= ClickTolerance && clickingDown)
        {
            clickingDown = false;
            OnButtonUp();
        }
    }

    // Callback for ButtonDown
    public virtual void OnButtonDown()
    {

        // Play sound
        if (audioSource && ButtonClick)
        {
            audioSource.clip = ButtonClick;
            audioSource.Play();
        }

        // Call event
        if (onButtonDown != null)
        {
            onButtonDown.Invoke();
        }
    }

    // Callback for ButtonDown
    public virtual void OnButtonUp()
    {
        // Play sound
        if (audioSource && ButtonClickUp)
        {
            audioSource.clip = ButtonClickUp;
            audioSource.Play();
        }

        // Call event
        if (onButtonUp != null)
        {
            onButtonUp.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {

        XRDirectInteractor XRDirectInteract = other.GetComponent<XRDirectInteractor>();
        if (XRDirectInteract != null)
        {
            if (XRDirectInteractors == null)
            {
                XRDirectInteractors = new List<XRDirectInteractor>();
            }

            if (!XRDirectInteractors.Contains(XRDirectInteract))
            {
                XRDirectInteractors.Add(XRDirectInteract);
                Debug.Log("Trigger Entered");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        XRDirectInteractor XRDirectInteract = other.GetComponent<XRDirectInteractor>();
        if (XRDirectInteract != null)
        {
            if (XRDirectInteractors.Contains(XRDirectInteract))
            {
                XRDirectInteractors.Remove(XRDirectInteract);
                Debug.Log("Trigger Exited");
            }
        }
    }
}

