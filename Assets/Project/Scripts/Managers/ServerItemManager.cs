using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;


namespace CEMSIM{

	public class ServerItemManager : MonoBehaviour
	{
		private Dictionary<(int,string),(GameObject,string)> itemDict = new Dictionary<(int,string),(GameObject,string)>();//This dictionary contains all items to be manage. To use: drag gameobject into the list in Unity IDE
	    private List<string> itemStatusList = new List<string>(); 			//List of string that contains mesages of item information.


	    // Start is called before the first frame update
	    void Start()
	    {
	    	CollectItems();

	    	InvokeRepeating("GetItemStatus", 0.05f, 0.05f);  //1s delay, repeat every 1s
	    	InvokeRepeating("SendItemStatus", 0.05f, 0.05f);  //1s delay, repeat every 1s
	    	
	    }

	    // Update is called once per frame
	    void FixedUpdate()
	    {	

	    }

        /// <summary>
        /// Get item status (id(unique),type,rotation,position,owner) for each item in itemList.
        /// </summary>
	    private void GetItemStatus(){

	    	foreach(KeyValuePair<(int id,string name),(GameObject item,string owner)> kv in itemDict){
	    		GameObject item = kv.Value.item;
	    		int id = kv.Key.id;
	    		string name = kv.Key.name;
	    		string owner = kv.Value.owner;
	    		string pos = item.transform.position.ToString();
	    		string rot = item.transform.rotation.ToString();
	    		itemStatusList.Add(string.Format("id={0}|name={1}|owner={2}|pos={3}|rot={4}",id.ToString(),name,owner,pos,rot));

	    	}
	    }

	    private void SendItemStatus(){
	    	foreach(string msg in itemStatusList){
	    		//Brodcast messages via tcp
	    		Debug.Log("Sending item status");
	    		Debug.Log(msg);
	    		ServerSend.BrodcastItemStatus(msg);
	    	}
	    	itemStatusList.Clear();
	    }



	    /// <summary>
        /// Add all items under ItemManager into list
        /// </summary>
	    private void CollectItems(){
	    	int id = 0;
	    	string name = "";
	    	string owner = "";
			foreach (Transform child in gameObject.transform)
			{ 
				name = child.gameObject.name;
				//Debug.Log(name);
				owner = ""; 	
																										//TODO: Figure out about owner
			    itemDict.Add((id,name),(child.gameObject,owner));
			    id++;
			}
	    }

 

	}
}