using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM.GameLogic;
using CEMSIM.Network;
using Newtonsoft.Json.Linq;

namespace CEMSIM
{
    namespace Tools
    {
        /// <summary>
        /// This is the base class for all tool interactions.
        /// </summary>
        public abstract class ToolBaseInteraction: MonoBehaviour
        {
            protected int itemId; // unique id of the tool
            protected ToolType toolType; // category of the tool
            public ToolBaseState toolState;

            public static event Action<ToolType, int, ToolBaseState, int> onToolStateUpdateTrigger;

            public ToolBaseInteraction(ToolType _toolType)
            {
                //itemId = _itemId; // itemId is assigned by system, so it will be initialized later via Initialization function
                toolType = _toolType;
            }

            public virtual void InitializeItem(int _itemId)
            {
                itemId = _itemId;
            }

            ///// <summary>
            ///// This function will set the initial state of the item based on the input json Jobject
            ///// Each tool should serialize the  
            ///// </summary>
            ///// <param name="jobject"></param>
            //public virtual void DigestJsonObject(JObject item)
            //{
            //    if (item.ContainsKey("State")){
            //        DigestToolStateFromJsonObject(item["State"]);
            //    }
            //}

            ///// <summary>
            ///// This function will use a json object which is serialized from a ToolState.
            ///// </summary>
            ///// <param name="jobject_state"></param>
            //public abstract void DigestToolStateFromJsonObject(JToken jtoken_state);

            /// <summary>
            /// Set the state of the binded gameobject by a given state.
            /// </summary>
            /// <typeparam name="ToolState"></typeparam>
            /// <param name="curState"></param>
            public virtual void SetState(ToolBaseState curState)
            {
                toolState = curState;
                UpdateState();
                StateUpdateEvent();
            }

            /// <summary>
            /// Update the model of the gameobject based on the current state.
            /// </summary>
            public abstract void UpdateState();

            /// <summary>
            /// Get the current state of the binded gameobject. This function may be called by HVR components to set the state of a tool
            /// </summary>
            /// <typeparam name="ToolState"></typeparam>
            /// <returns></returns>
            public virtual ToolBaseState GetState() { return toolState; }


            /// <summary>
            /// Return a byte array that abstract the current state of the attached item
            /// </summary>
            /// <returns></returns>
            public virtual byte[] GenStateBytes()
            {
                //if (toolState == null)
                //{
                //    Debug.Log($"{toolType} state is null");
                //}
                //else
                //{
                //    Debug.Log($"{toolType} state is good!!!!!");
                //}
                return toolState.ToPacketPayload();
            }

            


            /// <summary>
            /// Digest the payload of the _packet to extract additional information of the attached item
            /// </summary>
            /// <param name="_packet"></param>
            public virtual void DigestStateBytes(Packet _remainderPacket)
            {
                if (toolState.FromPacketPayload(_remainderPacket))
                    UpdateState();
            }

            



            /// <summary>
            /// Triggered when the tool state has been changed
            /// </summary>
            public void StateUpdateEvent()
            {
                if(GameManager.instance.isSinglePlayerMode)
                    ItemStateUpdateTrigger(toolType, itemId, toolState, GameConstants.SINGLE_PLAYER_CLIENTID);
                else
                    if (ClientItemManager.instance != null)
                        ItemStateUpdateTrigger(toolType, itemId, toolState, ClientInstance.instance.myId);
            }

            #region Event System
            public static void ItemStateUpdateTrigger(ToolType toolType, int _itemId, ToolBaseState _state, int _clientId)
            {
                if (onToolStateUpdateTrigger != null)
                    onToolStateUpdateTrigger(toolType, _itemId, _state, _clientId);
            }

            

            #endregion

        }
    }
}