using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class BaseEvent : IDisposable
        {
            protected TimeSpan eventTime;
            private bool disposed = false; ///> true: this object has been manually disposed

            public BaseEvent()
            {
                eventTime = DateTime.UtcNow - LogManager.instance.SystemStartTime;
            }

            /// <summary>
            /// Serialize the event to a string. Used to add to log file
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "";
            }

            /// <summary>
            /// Format the instance to a csv string. 
            /// </summary>
            /// <returns></returns>
            public virtual string ToCSV()
            {
                return "";
            }


            public void AddToGeneralEventQueue()
            {
                LogManager.instance.generalEventQueue.Enqueue(this);
            }

            public void AddToPlayerEventQueue()
            {
                LogManager.instance.playerEventQueue.Enqueue(this);
            }


            /// <summary>
            /// A distructor
            /// </summary>
            /// <param name="_disposing"></param>
            protected virtual void Dispose(bool _disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                }
            }

            public void Dispose()
            {
                // since we manually dispose the object, no need to call the system garbage collector.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}