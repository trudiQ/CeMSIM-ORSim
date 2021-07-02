﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;


namespace CEMSIM{

	public class ServerItemManager : MonoBehaviour
	{
		public List<GameObject> itemList = new List<GameObject>();	//This List contains all items to be instantiated. To use: drag gameobject into the list in Unity IDE
		public List<Vector3>spawnPositionList = new List<Vector3>();//This List contains all positions that item is to be allocated, To use: enter x y z value for vector3 in unity



	    // Start is called before the first frame update
	    void Start()
	    {
	    	
	    	
	    	InitializeItems();
	    	
	    }

	    // Update is called once per frame
	    void FixedUpdate()
	    {	
	    	SendItemStatus();
	    }


	    private void SendItemStatus(){
	    	foreach(GameObject item in itemList){

	    		//Brodcast item position via UDP
	    		ServerSend.BroadcastItemState(item);
	    		//Brodcast item owner via TCP
	    		//*****TO DO: Brodcast ownership information via TCP********
	    		
	    	}
	    }



	    /// <summary>
        /// Add all items under ItemManager into list
        /// </summary>
	    private void InitializeItems(){
	    	int id = 0;		// id of the item
	    	int owner = 0; // owner 0 is the server, because user id starts with 1

			if (itemList.Count != spawnPositionList.Count)
			{
				Debug.LogWarning("Warning: itemList and spawnPositionList do not have same size.");
				while (itemList.Count > spawnPositionList.Count)
				{
					spawnPositionList.Add(new Vector3(0, 1.5f, 0));
				}
			}

			for (int i = 0; i < itemList.Count; i++)
			{ 
				itemList[i] = Instantiate(itemList[i], spawnPositionList[i], Quaternion.identity);
				//itemList[i].transform.parent = transform;
				ItemController itemCon = itemList[i].GetComponent<ItemController>();
				itemCon.id = id;
				itemCon.ownerId = owner;
			    id++;
			}
	    }

		/// <summary>
		/// Update an item's position and rotation
		/// </summary>
		/// <param name="itemID"> The id of the item to be updated </param>
		/// <param name="position"> The vector3 position of the item </param>
		/// <param name="rotation"> The vector3 position of the item </param>
		public void UpdateItemPosition(int itemId, Vector3 position, Quaternion rotation)
		{
	    	itemList[itemId].transform.position = position;
			itemList[itemId].transform.rotation = rotation;
		}










	}
}