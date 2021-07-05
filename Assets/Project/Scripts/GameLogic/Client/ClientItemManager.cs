using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;

namespace CEMSIM
{
	namespace GameLogic
	{
		public class ClientItemManager : MonoBehaviour
		{
			public static ClientItemManager instance;

			[Header("Library of all tool prefebs")]
			public List<GameObject> itemLibrary = new List<GameObject>(); // A library 

			[HideInInspector]
			private List<GameObject> itemList = new List<GameObject>();  //This List contains all items in the scene
			private List<GameObject> ownedItemList = new List<GameObject>();        //This List contains all items owned by this client


			private void Awake()
			{
				if (instance == null)
				{
					instance = this;
				}
				else if (instance != this)
				{
					// We only allow one instance of this class to exist.
					// Destroy current instance if different from the current one.
					Debug.Log("Another instance already exists. Destroy this one.");
					Destroy(this);
				}
			}

			// Start is called before the first frame update
			void Start()
			{
			}

			// Update is called once per frame
			void FixedUpdate()
			{
				SendOwnedItemStatus();
			}


			public void InitializeItem(int _listSize, int _itemId, int _itemTypeId, Vector3 _position, Quaternion _rotation, Packet _remainderPacket)
            {

				if(_itemTypeId >= itemLibrary.Count)
                {
					Debug.LogError($"Server asked to spawn an item{_itemTypeId} which is now in the library");
                }

				GameObject _item = Instantiate(itemLibrary[_itemTypeId], _position, _rotation);

				_item.GetComponent<ItemController>().DigestStateMessage(_remainderPacket);

				if (_itemId >= itemList.Count)
				{
					// Count == _itemId means the current packet indicates the next item that should be added to the list
					// if Count < _itemId, then there may be some gameobjects that are missed (or come in random order).
					// Create empty gameobjects as place holder.
					for (int i = itemList.Count; i < _itemId; i++)
					{
						itemList.Add(new GameObject()); // add empty gameObject
					}
					itemList.Add(_item);
				}
				else
				{
					itemList[_itemId] = _item;
				}

			}


			/// <summary>
			/// Update an item's position, rotation, and detailed state
			/// </summary>
			/// <param name="itemID"> The id of the item to be updated </param>
			/// <param name="position"> The vector3 position of the item </param>
			/// <param name="position"> The vector3 position of the item </param>
			/// 
			public void UpdateItemState(int _itemId, Vector3 _position, Quaternion _rotation, Packet _remainderPacket)
			{
				itemList[_itemId].transform.position = _position;
				itemList[_itemId].transform.rotation = _rotation;

				itemList[_itemId].GetComponent<ItemController>().DigestStateMessage(_remainderPacket);

			}



			/// <summary>
			/// Add all items under ItemManager into list
			/// </summary>
			private void CollectItems()
			{
				int id = 0;
				int owner = 0;
				for (int i = 0; i < itemList.Count; i++)
				{
					//Debug.Log(i);
					itemList[i] = Instantiate(itemList[i], new Vector3(0, 0, 0), Quaternion.identity);
					itemList[i].transform.parent = transform;
					GameObject item = itemList[i];
					ItemController itemCon = item.GetComponent<ItemController>();
					Rigidbody rb = item.GetComponent<Rigidbody>();
					//rb.isKinematic = true;					// prevent client from changing the item's position & rotation // why when isKinematic=true, HVR items cannot be picked up? It worked for xrinteractable.
					rb.useGravity = false;                      // prevent an item from falling even though it is grabbed by a player.	
					itemCon.id = id;
					itemCon.ownerId = owner;
					id++;
				}
			}


			/// <summary>
			/// Send Item status that is owned by this client
			/// </summary>
			private void SendOwnedItemStatus()
			{
				foreach (GameObject item in ownedItemList)
				{

					//Send position to Server via UDP
					ClientSend.SendItemPosition(item, true);
					//Get Item Controller
					//ItemController itemCon = item.GetComponent<ItemController>();
					//Debug.Log("Sending item status:");
					//Debug.Log(itemCon.ToString());

				}
			}

			public void GainOwnership(int _itemId)
			{
				GameObject _item = itemList[_itemId];
				_item.GetComponent<ItemController>().ownerId = ClientInstance.instance.myId;
				ownedItemList.Add(_item);
				//Send ownership request to user
				ClientSend.SendOnwershipChange(_item);
			}

			/// <summary>
			/// When the player actively drops the item, the function informs the server the ownership change.
			/// </summary>
			/// <param name="_itemId"></param>
			public void DropOwnership(int _itemId)
			{
				GameObject _item = itemList[_itemId];
				_item.GetComponent<ItemController>().ownerId = 0;
				
				ownedItemList.Remove(_item);
				//Send drop ownership notification to server
				ClientSend.SendOnwershipChange(_item);
			}

			public void TransferOwnership(GameObject item, int _toId)
            {
				ItemController itemCon = item.GetComponent<ItemController>();
				itemCon.ownerId = _toId;
				if (_toId != ClientInstance.instance.myId)
                {
					ownedItemList.Remove(item);
				}
            }












		}
	}
}
