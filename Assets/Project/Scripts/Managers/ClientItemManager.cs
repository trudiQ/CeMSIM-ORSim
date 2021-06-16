using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;

namespace CEMSIM{
	public class ClientItemManager : MonoBehaviour
	{

		public List<GameObject> itemList = new List<GameObject>();	//This List contains all items to be instantiated. To use: drag gameobject into the list in Unity IDE
		private List<GameObject> ownedItemList = new List<GameObject>();		//This List contains all items owned by this client

	    // Start is called before the first frame update
	    void Start()
	    {
	    	CollectItems();
	    }

	    // Update is called once per frame
	    void FixedUpdate()
	    {


	    	SendOwnedItemStatus();


	    }



		/// <summary>
		/// Update an item's position and rotation
		/// </summary>
		/// <param name="itemID"> The id of the item to be updated </param>
		/// <param name="position"> The vector3 position of the item </param>
		/// <param name="position"> The vector3 position of the item </param>
		/// 
		public void UpdateItemPosition(int itemId, Vector3 position, Quaternion rotation)
		{
	    	itemList[itemId].transform.position = position;
			itemList[itemId].transform.rotation = rotation;

		}



	    /// <summary>
        /// Add all items under ItemManager into list
        /// </summary>
	    private void CollectItems(){
	    	int id = 0;
	    	int owner = 0;
			for (int i = 0; i < itemList.Count; i++)
			{ 
				Debug.Log(i);
				itemList[i] = Instantiate(itemList[i], new Vector3(0,0,0), Quaternion.identity);
				itemList[i].transform.parent = transform;
				GameObject item = itemList[i];
				ItemController itemCon = item.GetComponent<ItemController>();
				Rigidbody rb = item.GetComponent<Rigidbody>();
				// The following two lines are no longer needed for the new HVR system
				//rb.isKinematic = true;					//Prevent client from changing the item's position & rotation
				//rb.useGravity = false;
				itemCon.id = id;
				itemCon.ownerId = owner;
			    id++;
			}
	    }


	    /// <summary>
        /// Send Item status that is owned by this client
        /// </summary>
	    private void SendOwnedItemStatus(){
	    	foreach(GameObject item in ownedItemList){

	    		//Send position to Server via UDP
	    		ClientSend.SendItemPosition(item);
	    		//Get Item Controller
	    		ItemController itemCon = item.GetComponent<ItemController>();
	    		//Debug.Log("Sending item status:");
	    		//Debug.Log(itemCon.ToString());
	    		
	    	}
	    }

	    public void GainOwnership(GameObject item){
	    	ItemController itemCon = item.GetComponent<ItemController>();
	    	//Update item's ownerId
	    	itemCon.ownerId = ClientInstance.instance.myId;
	    	//Add item to owned list
	    	ownedItemList.Add(item);
	    	//Send ownership request to user
	    	ClientSend.SendOnwershipChange(item);
	    }

	    public void DropOwnership(GameObject item){
	    	ItemController itemCon = item.GetComponent<ItemController>();
	    	//Update item's ownerID to 0 (not owned)
	    	itemCon.ownerId = 0;
	    	//Drop item from ownedItemList
	    	ownedItemList.Remove(item);
	    	//Send drop ownership notification to server
	    	ClientSend.SendOnwershipChange(item);
	    }














	}
}
