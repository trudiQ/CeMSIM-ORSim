using UnityEngine;
using CEMSIM.GameLogic;
using CEMSIM.Network;
using System;
using CEMSIM.Tools;

namespace CEMSIM
{
    namespace GameLogic
    {
        public class ItemController : MonoBehaviour
        {
            [Header("Item Information")]
            public ToolType toolType;
            public int itemId;

            [HideInInspector]
            public int ownerId;


            private ToolBaseInteraction itemInteractionManager;


            // event system
            public static event Action<ToolType, int, int> onItemPickupTrigger;
            public static event Action<ToolType, int, int> onItemDropoffTrigger;


            private void Awake()
            {

                itemInteractionManager = gameObject.GetComponent<ToolBaseInteraction>();
                if (itemInteractionManager == null)
                {
                    Debug.LogError("Could not find an attached ToolBaseInteraction script");
                }
            }


            public void initialize(int _id, int _ownerId=0)
            {
                itemId = _id;
                ownerId = _ownerId;
                itemInteractionManager.InitializeItem(_id);
            }

            public byte[] GetItemState()
            {
                return itemInteractionManager.GenStateBytes();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="_packet"></param>
            public void DigestStateMessage(Packet _remainderPacket)
            {
                if(_remainderPacket.UnreadLength() > 0)
                    itemInteractionManager.DigestStateBytes(_remainderPacket);
            }


            public override string ToString()
            {
                return string.Format("id={0}|owner={1}|pos={2}|rot={3}", itemId.ToString(), ownerId.ToString(), gameObject.transform.position, gameObject.transform.rotation);
            }

            public void GainOwnership()
            {
                if (ClientItemManager.instance != null)
                {
                    ClientItemManager.instance.GainOwnership(itemId);
                    ItemPickupTrigger(toolType, itemId, GameConstants.SINGLE_PLAYER_CLIENTID);
                }
                else
                {
                    ItemPickupTrigger(toolType, itemId, ownerId);
                }
            }

            public void DropOwnership()
            {
                if (ClientItemManager.instance != null)
                {
                    ClientItemManager.instance.DropOwnership(itemId, true);
                    ItemDropoffTrigger(toolType, itemId, ClientInstance.instance.myId);
                }
                else
                {
                    ItemDropoffTrigger(toolType, itemId, ownerId);
                }
            }



            #region
            public static void ItemPickupTrigger(ToolType _toolType, int _itemId, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onItemPickupTrigger != null)
                    onItemPickupTrigger(_toolType, _itemId, _clientId);
            }

            public static void ItemDropoffTrigger(ToolType _toolType, int _itemId, int _clientId)
            {
                //Debug.LogError($"lalalalala,onPlayerEnterTrigger {onPlayerEnterTrigger}");
                if (onItemDropoffTrigger != null)
                    onItemDropoffTrigger(_toolType, _itemId, _clientId);
            }
            #endregion
        }

    }
}