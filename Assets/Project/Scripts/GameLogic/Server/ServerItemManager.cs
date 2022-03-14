using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CEMSIM
{

	public class ServerItemManager : MonoBehaviour
	{
		public static ServerItemManager instance;

		[Tooltip("The order of prefabs should match the declaraction of Enum ToolType in GameLogic:GameConstants::ToolType")]
		public List<GameObject> itemLibrary = new List<GameObject>(); // A library
		public string SceneItemInitialStateJson;

		public List<GameObject> itemList = new List<GameObject>();  //This List contains all items to be instantiated. To use: drag gameobject into the list in Unity IDE
		//public List<Vector3> spawnPositionList = new List<Vector3>();//This List contains all positions that item is to be allocated, To use: enter x y z value for vector3 in unity

		// event system
		public static event Action onItemInitializeTrigger;

		// Start is called before the first frame update
		void Start()
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

			//InitializeItems_from_inspector();
			InitializeItems_from_json(SceneItemInitialStateJson);


		}

		// Update is called once per frame
		void FixedUpdate()
		{
			SendItemStatus();
		}


		private void SendItemStatus(bool isUDP = true)
		{
			foreach (GameObject item in itemList)
			{

				//Brodcast item position via UDP
				ServerSend.BroadcastItemState(item, isUDP);
				//Brodcast item owner via TCP
				//*****TO DO: Brodcast ownership information via TCP********

			}
		}


		public int GetItemNum()
		{
			return itemList.Count;

		}

		/// <summary>
		/// Add all items under ItemManager into list (deprecated)
		/// </summary>
		//private void InitializeItems_from_inspector()
		//{
		//	int id = 0;     // id of the item
		//	int owner = 0; // owner 0 is the server, because user id starts with 1

		//	if (itemList.Count != spawnPositionList.Count)
		//	{
		//		Debug.LogWarning("Warning: itemList and spawnPositionList do not have same size.");
		//		while (itemList.Count > spawnPositionList.Count)
		//		{
		//			spawnPositionList.Add(new Vector3(0, 1.5f, 0));
		//		}
		//	}

		//	for (int i = 0; i < itemList.Count; i++)
		//	{
		//		itemList[i] = Instantiate(itemList[i], spawnPositionList[i], Quaternion.identity);
		//		//itemList[i].transform.parent = transform;
		//		ItemController itemCon = itemList[i].GetComponent<ItemController>();
		//		itemCon.itemId = id;
		//		itemCon.ownerId = owner;
		//		id++;
		//	}

		//	ItemInitializeTrigger();

		//}

		private void InitializeItems_from_json(string jsonFileName)
		{
			using (StreamReader r = new StreamReader(jsonFileName))
			{
				ToolType toolType;
				bool ignoreCase = true;
				int itemId = 0;

				Vector3 itemPos = new Vector3(0,0,0);
				Vector3 itemRot_eular = new Vector3(0, 0, 0);
				Quaternion itemRot = new Quaternion(0,0,0,1);

				string json = r.ReadToEnd();
				List<JObject> array = JsonConvert.DeserializeObject<List<JObject>>(json);

				Debug.Log($"Loading {array.Count} items from file");

				foreach (JObject item in array)
				{
					if (item.ContainsKey("ToolType") && item.ContainsKey("Position") && item.ContainsKey("Rotation"))
					{
						if (Enum.TryParse((string)item["ToolType"], ignoreCase, out toolType))
						{
							Debug.Log($"Creating item {itemId} of type {toolType}");

							// converting position
							itemPos.x = float.Parse((string)item["Position"]["X"]);
							itemPos.y = float.Parse((string)item["Position"]["Y"]);
							itemPos.z = float.Parse((string)item["Position"]["Z"]);

							// converting rotation
							itemRot_eular.x = float.Parse((string)item["Rotation"]["X"]);
							itemRot_eular.y = float.Parse((string)item["Position"]["Y"]);
							itemRot_eular.z = float.Parse((string)item["Position"]["Z"]);

							GameObject _item = Instantiate(itemLibrary[(int)toolType], itemPos, itemRot); // create item

							_item.GetComponent<ItemController>().initialize(itemId);
							_item.GetComponent<ItemController>().DigestJsonObject(item);

							itemId++;
						}

					}
					else
					{
						Debug.Log($"key \"ToolType\" is required. Please check. {item}");
					}
				}
			}
		}

		/// <summary>
		/// Update an item's position and rotation
		/// </summary>
		/// <param name="itemID"> The id of the item to be updated </param>
		/// <param name="position"> The vector3 position of the item </param>
		/// <param name="rotation"> The vector3 position of the item </param>
		public void UpdateItemState(int itemId, Vector3 position, Quaternion rotation, Packet _remainderPacket)
		{
			itemList[itemId].transform.position = position;
			itemList[itemId].transform.rotation = rotation;
			itemList[itemId].GetComponent<ItemController>().DigestStateMessage(_remainderPacket);
		}


		/// <summary>
		/// Send the current state of all items in the list to a newly added user.
		/// </summary>
		/// <param name="_toClient"></param>
		public static void SendCurrentItemList(int _toClient)
		{
			foreach (GameObject _item in instance.itemList)
			{
				ServerSend.SendInitialItemState(_toClient, _item);
			}
		}


		#region event system
		public void ItemInitializeTrigger()
		{
			Debug.Log("initialize item list");
			if (onItemInitializeTrigger != null)
				onItemInitializeTrigger();

		}
		#endregion

	}
}