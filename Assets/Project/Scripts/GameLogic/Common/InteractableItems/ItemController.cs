﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;
using CEMSIM.GameLogic;
using CEMSIM.Network;
using System;

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

            private ItemStateManager itemStateManager; // pointed to the specific controller

            // event system
            public static event Action<ToolType, int, int> onItemPickupTrigger;
            public static event Action<ToolType, int, int> onItemDropoffTrigger;


            private void Awake()
            {
                switch (toolType)
                {
                    case ToolType.scalpel:
                        itemStateManager = new ScalpelStateManager();
                        break;
                    case ToolType.catheter:
                        itemStateManager = new CatheterStateManager();
                        break;
                    case ToolType.N95Mask:
                        itemStateManager = new N95MaskStateManager();
                        break;
                    case ToolType.boufant:
                        itemStateManager = new BoufantStateManager();
                        break;
                    case ToolType.visor:
                        itemStateManager = new VisorStateManager();
                        break;
                    case ToolType.shoeCover:
                        itemStateManager = new ShoeCoverStateManager();
                        break;
                    case ToolType.gown:
                        itemStateManager = new GownStateManager();
                        break;
                    case ToolType.glove:
                        itemStateManager = new GloveStateManager();
                        break;
                    default:
                        itemStateManager = new ItemStateManager();
                        break;
                }
            }


            public void initialize(int _id, int _ownerId=0)
            {
                itemId = _id;
                ownerId = _ownerId;
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