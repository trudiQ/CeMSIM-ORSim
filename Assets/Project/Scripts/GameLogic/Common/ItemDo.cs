using System;
using System.Collections.Generic;
using UnityEngine;
namespace CEMSIM
{
    namespace GameLogic
    {

        public class ItemDo{
            public GameObject gameObject;
            public int id;
            public int ownerId;

            public ItemDo(GameObject _gameObject, int _id, int _ownerId)
            {
                gameObject = _gameObject;
                id = _id;
                ownerId = _ownerId;
            }


            public override string ToString(){
                return string.Format("id={0}|owner={1}|pos={2}|rot={3}",id.ToString(),ownerId.ToString(),gameObject.transform.position,gameObject.transform.rotation);
            }   
        
        }


    }
}
