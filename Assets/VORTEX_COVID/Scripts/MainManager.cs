using UnityEngine;

public class MainManager : MonoBehaviour
{
    public GameObject anteroomTeleportationPad;
    public GameObject operationRoomTeleportationPad;
    public Room currentRoom { get; set; } = Room.ANTEROOM;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onAllPPEsEquipped()
    {
        if (currentRoom == Room.ANTEROOM)
        {
            anteroomTeleportationPad.SetActive(true);
        }
    }

    public void onPPEUnequipped()
    {
        if (currentRoom == Room.ANTEROOM)
        {
            anteroomTeleportationPad.SetActive(false);
        }
    }

    public void onRSICompleted()
    {
        operationRoomTeleportationPad.SetActive(true);
    }

    public enum Room
    {
        ANTEROOM,
        OPERATIONROOM
    }
}
