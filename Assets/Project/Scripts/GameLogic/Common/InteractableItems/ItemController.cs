using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;
using CEMSIM.GameLogic;
using CEMSIM.Network;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ItemController : MonoBehaviour
        {
            [Header("Item Information")]
            public ToolType toolType;
            public int id;

            [HideInInspector]
            public int ownerId;

            private ItemStateManager itemStateManager; // pointed to the specific controller

            private void Awake()
            {
                switch (toolType)
                {
                    case ToolType.scalpel:
                        itemStateManager = new ScalpelStateManager();
                        break;
                    case ToolType.decompressionNeedle:
                        itemStateManager = new CatheterStateManager();
                        break;
                    default:
                        itemStateManager = new ItemStateManager();
                        break;
                }
            }


            public void initialize(int _id)
            {
                id = _id;
                itemStateManager.initializeItem(_id);
            }

            public byte[] GetItemState()
            {
                return itemStateManager.GetItemState();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="_packet"></param>
            public void DigestStateMessage(Packet _remainderPacket)
            {
                if(_remainderPacket.UnreadLength() > 0)
                    itemStateManager.DigestStateMessage(_remainderPacket);
            }





            public override string ToString()
            {
                return string.Format("id={0}|owner={1}|pos={2}|rot={3}", id.ToString(), ownerId.ToString(), gameObject.transform.position, gameObject.transform.rotation);
            }

            public void GainOwnership()
            {
                ClientItemManager.instance.GainOwnership(id);
                Debug.Log($"Grabbing item {id}");
            }

            public void DropOwnership()
            {
                ClientItemManager.instance.DropOwnership(id);
                Debug.Log($"Release item {id}");
            }
        }

    }
}