using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;

namespace CEMSIM{
	public class ClientItemManager : MonoBehaviour
	{

		public List<GameObject> itemList = new List<GameObject>();	//This List contains all items to be instantiated. To use: drag gameobject into the list in Unity IDE
		public List<Item> itemManageList = new List<Item>();		//This List contains all items to be managed.
		private List<Item> ownedItemList = new List<Item>();		//This List contains all items owned by this client

		public bool testOwnAllItems = false;

		private Dictionary<(int,string),(GameObject,string)> itemDict = new Dictionary<(int,string),(GameObject,string)>();//This dictionary contains all items to be manage. To use: drag gameobject into itemManager
		private  List<string> itemStatusList = new List<string>();
	    // Start is called before the first frame update
	    void Start()
	    {
	    	CollectItems();
	    }

	    // Update is called once per frame
	    void FixedUpdate()
	    {
	    	if(testOwnAllItems){
	    		foreach(Item item in itemManageList){
	    			if(item.ownerId != ClientInstance.instance.myId){
	    				GainOwnership(item);
	    				Debug.Log("Client requested to gain ownership of an item!");
	    			}
	    		}
	    	}else{
	    		foreach(Item item in ownedItemList){
	    			DropOwnership(item);
	    			Debug.Log("Client gave up on ownership of an item!");

	    		}
	    	}


	    	SendOwnedItemStatus();


	    }



	    /// <summary>
        /// Update an item's position
        /// </summary>
        /// <param name="itemID"> The id of the item to be updated </param>
        /// <param name="position"> The vector3 position of the item </param>
	    public void UpdateItemPosition(int itemId, Vector3 position){
	    	itemManageList[itemId].gameObject.transform.position = position;
	    }

	    /// <summary>
        /// Update an item's rotation
        /// </summary>
        /// <param name="itemID"> The id of the item to be updated </param>
        /// <param name="position"> The vector3 position of the item </param>
	    public void UpdateItemRotation(int itemId, Quaternion rotation){
	    	itemManageList[itemId].gameObject.transform.rotation = rotation;
	    }








	    /// <summary>
        /// Add all items under ItemManager into list
        /// </summary>
	    private void CollectItems(){
	    	int id = 0;
	    	int owner = 0;
			foreach (GameObject itemPrefab in itemList)
			{ 
				GameObject item = Instantiate(itemPrefab, new Vector3(0,0,0), Quaternion.identity);
				Rigidbody rb = item.GetComponent<Rigidbody>();
				rb.isKinematic = true;					//Prevent client from changing the item's position & rotation
				rb.useGravity = false;

				itemManageList.Add( new Item(item, id, owner ) );
			    id++;
			}
	    }


	    /// <summary>
        /// Send Item status that is owned by this client
        /// </summary>
	    private void SendOwnedItemStatus(){
	    	foreach(Item item in ownedItemList){

	    		//Send position to Server via UDP
	    		ClientSend.SendItemPosition(item);
	    		//Send rotation to Server via UDP
	    		ClientSend.SendItemRotation(item);

	    		Debug.Log("Sending item status:");
	    		Debug.Log(item.ToString());
	    		
	    	}
	    }

	    public void GainOwnership(Item item){
	    	//Update item's ownerId
	    	item.ownerId = ClientInstance.instance.myId;
	    	//Add item to owned list
	    	ownedItemList.Add(item);
	    	//Send ownership request to user
	    	ClientSend.SendOnwershipChange(item);
	    }

	    public void DropOwnership(Item item){
	    	//Update item's ownerID to 0 (not owned)
	    	item.ownerId = 0;
	    	//Drop item from ownedItemList
	    	ownedItemList.Remove(item);
	    	//Send drop ownership notification to server
	    	ClientSend.SendOnwershipChange(item);
	    }














	}
}
