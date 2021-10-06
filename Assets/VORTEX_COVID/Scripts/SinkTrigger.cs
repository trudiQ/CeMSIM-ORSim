using UnityEngine;
using UnityEngine.Events;

public class SinkTrigger : MonoBehaviour
{
    public UnityEvent triggerEnter;
    public UnityEvent triggerExit;
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
            triggerEnter.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            triggerExit.Invoke();
        }
    }
}
