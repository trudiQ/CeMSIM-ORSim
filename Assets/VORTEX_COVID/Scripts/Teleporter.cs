using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject rigToBeTeleported;
    public Transform destination;
    public MainManager.Room destinationRoom;
    public bool disableAfterArrival;

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
            if (disableAfterArrival)
            {
                this.gameObject.SetActive(false);
            }
            Collider[] colliders = rigToBeTeleported.GetComponentsInChildren<Collider>();
            switchColliders(colliders, false);
            rigToBeTeleported.transform.position = new Vector3(destination.position.x, other.transform.position.y, destination.position.z);
            FindObjectOfType<MainManager>().currentRoom = destinationRoom;
            switchColliders(colliders, true);
        }
    }

    private void switchColliders(Collider[] colliders, bool enabled)
    {
        foreach (Collider c in colliders)
        {
            c.enabled = enabled;
        }
    }
}
