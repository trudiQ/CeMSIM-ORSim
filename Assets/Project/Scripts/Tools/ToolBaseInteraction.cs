using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Tools
    {
        /// <summary>
        /// This is the base class for all tool interactions.
        /// </summary>
        public abstract class ToolBaseInteraction<State> : MonoBehaviour
        {
            /// <summary>
            /// Set the state of the binded gameobject.  This function may be called by the player's HVR components or the network components.
            /// </summary>
            /// <typeparam name="State"></typeparam>
            /// <param name="curState"></param>
            public abstract void SetState(State curState);

            /// <summary>
            /// Get the current state of the binded gameobject. This function is called when requiring to synchronizing the object status or logging the state.
            /// </summary>
            /// <typeparam name="State"></typeparam>
            /// <returns></returns>
            public abstract State GetState();

        }
    }
}