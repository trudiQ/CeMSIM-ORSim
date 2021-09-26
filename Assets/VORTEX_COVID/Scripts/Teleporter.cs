using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    public UnityEvent onTriggered;
    public bool disableAfterTrigger;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (disableAfterTrigger)
            {
                this.gameObject.SetActive(false);
            }
            onTriggered.Invoke();
        }
    }

}