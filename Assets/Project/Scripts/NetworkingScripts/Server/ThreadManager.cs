using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Network
    {
        namespace Server
        {
            public class ThreadManager : MonoBehaviour
            {
                private static readonly List<Action> executeOnMainThread = new List<Action>();      // a buffer queue that accept actions
                private static readonly List<Action> executeCopiedOnMainTread = new List<Action>(); // a queue of actions(no-input function handlers) to run on main thread.
                private static bool actionToExecuteOnMainThread = false;                            // whether executeOnMainThread is empty

                private void FixedUpdate()
                {
                    UpdateMain();
                }

                public static void ExecuteOnMainThread(Action _action)
                {
                    if (_action == null)
                    {
                        Debug.Log("No action to execute on main thread");
                        NetworkOverlayMenu.Instance.Log("No action to execute on main thread");
                        return;
                    }

                    // add action to a list of actions to run
                    // lock is released once the insertion is completed
                    lock (executeOnMainThread)
                    {
                        executeOnMainThread.Add(_action);
                        actionToExecuteOnMainThread = true;
                    }
                }

                /// <summary>
                /// call all actions piled up in the main thread. This is a not a good idea for real application, but make sense here.
                /// This function should be called only from the main thread.
                /// </summary>
                public static void UpdateMain()
                {
                    if (actionToExecuteOnMainThread)
                    {
                        executeCopiedOnMainTread.Clear();
                        lock (executeOnMainThread)
                        {
                            executeCopiedOnMainTread.AddRange(executeOnMainThread);
                            executeOnMainThread.Clear();
                            actionToExecuteOnMainThread = false;
                        }

                        // run actions
                        for (int i = 0; i < executeCopiedOnMainTread.Count; i++)
                        {
                            executeCopiedOnMainTread[i]();
                        }
                    }
                }
            }
        }
    }
}