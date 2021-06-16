using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;


public class ItemController : MonoBehaviour
{
    public int id;
    public int ownerId;

    public override string ToString(){
        return string.Format("id={0}|owner={1}|pos={2}|rot={3}",id.ToString(),ownerId.ToString(),gameObject.transform.position,gameObject.transform.rotation);
    } 

    public void GainOwnership(){
    	GameObject itemManager = GameObject.Find("ItemManager");
    	Debug.Log($"Grabbing item {id}");
    	ClientItemManager CIM = (ClientItemManager)itemManager.GetComponent<ClientItemManager>();
        if (CIM)
        {
            CIM.GainOwnership(gameObject);
        }
    }

    public void DropOwnership(){
    	GameObject itemManager = GameObject.Find("ItemManager");
    	Debug.Log($"Release item {id}");
    	ClientItemManager CIM = (ClientItemManager)itemManager.GetComponent<ClientItemManager>();
        if (CIM)
        {
            CIM.DropOwnership(gameObject);
        }
    }
}

