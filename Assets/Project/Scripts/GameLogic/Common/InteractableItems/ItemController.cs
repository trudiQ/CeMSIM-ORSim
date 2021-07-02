using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;
using CEMSIM.GameLogic;

public class ItemController : MonoBehaviour
{
    public int category;
    public int id;
    public int ownerId;

    public override string ToString(){
        return string.Format("id={0}|owner={1}|pos={2}|rot={3}",id.ToString(),ownerId.ToString(),gameObject.transform.position,gameObject.transform.rotation);
    } 

    public void GainOwnership(){
        ClientItemManager.instance.GainOwnership(gameObject);
    	Debug.Log($"Grabbing item {id}");
    }

    public void DropOwnership(){
        ClientItemManager.instance.DropOwnership(gameObject);
        Debug.Log($"Release item {id}");
    }
}

