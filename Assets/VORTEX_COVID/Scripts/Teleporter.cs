using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform destination;
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
        if (other.tag == "Teleportable")
        {
            other.transform.position = destination.position;
            if (disableAfterArrival)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
