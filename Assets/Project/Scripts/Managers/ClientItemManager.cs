using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.Network;
using CEMSIM.GameLogic;

namespace CEMSIM{
	public class ClientItemManager : MonoBehaviour
	{

		private Dictionary<(int,string),(GameObject,string)> itemDict = new Dictionary<(int,string),(GameObject,string)>();//This dictionary contains all items to be manage. To use: drag gameobject into itemManager
		private  List<string> itemStatusList = new List<string>();
	    // Start is called before the first frame update
	    void Start()
	    {
	    	CollectItems();
	    }

	    // Update is called once per frame
	    void Update()
	    {
	        
	    }

	    /// <summary>
        /// Parse and update ietm status from server's message
        /// </summary>
        /// <param name="msg"></param>
	    public void UpdateItemStatus(string msg){
	    	Debug.Log(msg);
	    	//TODO: If current list doesn't contain this item of it has diffrent type, update according to server
	    	
	    	//Get item status from sever's message
	    	(int id, string name, string owner, Vector3 pos, Quaternion rot) newStatus = 
	    	ParseItemFromString(msg);

	    	//construct key
	    	(int,string) key = (newStatus.id, newStatus.name);

	    	//get item reference from dictionary
	    	(GameObject item,string owner) val;
	    	if(!itemDict.TryGetValue(key, out val)){
	    		Debug.Log("WARNING: Unkown item status received from server, check itemManager list");
	    		return;
	    	}
	    	GameObject item = val.item;

	    	//Update an item's rotation and position
	    	item.transform.position = newStatus.pos;
	    	
	    	
	    	item.transform.rotation = newStatus.rot;
	    	
	    	
	    	

	    	//Update an item's owner
	    	if(val.owner != newStatus.owner){
	    		itemDict[key] = (item, newStatus.owner);
	    	}
	    	



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
				Debug.Log(name);
				owner = ""; 	
																										//TODO: Figure out about owner
			    itemDict.Add((id,name),(child.gameObject,owner));
			    id++;
			}
	    }

	    /// <summary>
        /// Parse Item information from string message
        /// </summary>
        /// <param name="msg"></param>
	    private  (int id, string name, string owner, Vector3 pos, Quaternion rot) ParseItemFromString(string msg){
	    	//Format of msg:
	    	//"id=X|name=X|owner=XXX|pos=(x,y,z)|rot=(x,y,z)"
	    	string[] splited = msg.Split('|');
	    	int id;
	    	string name;
	    	string owner;
	    	string vector;
	    	float x,y,z;

	    	//Parse Id
	    	if(!int.TryParse(splited[0].Split('=')[1], out id)){
	    		Debug.Log("Warning: Item id parsing error");
	    	}

	    	//Parse name
	    	name = splited[1].Split('=')[1];

	    	//Parse owner
	    	owner = splited[2].Split('=')[1];
	    	
	    	//Parse position
	    	vector = splited[3].Split('=')[1].Replace("(","").Replace(")","");
	    	string[] vector_splited = vector.Split(',');
	    	float.TryParse(vector_splited[0], out x);
	    	float.TryParse(vector_splited[1], out y);
	    	float.TryParse(vector_splited[2], out z);
	    	Vector3 pos = new Vector3(x,y,z);

	    	//Parse rotation
	    	vector = splited[4].Split('=')[1].Replace("(","").Replace(")","");
	    	vector_splited = vector.Split(',');

	    	
	    	float.TryParse(vector_splited[0], out x);
	    	float.TryParse(vector_splited[1], out y);
	    	float.TryParse(vector_splited[2], out z);
	    	Quaternion rot = new Quaternion(x,y,z,1);

	    	//return
	    	(int id, string name, string owner, Vector3 pos, Quaternion rot) itemStatus 
	    	= (id, name, owner, pos, rot);
	    	return itemStatus;

	    }





	}
}
