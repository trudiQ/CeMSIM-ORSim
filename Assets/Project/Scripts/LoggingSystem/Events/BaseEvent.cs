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
            protected string eventName;
            private bool disposed = false; ///> true: this object has been manually disposed


            public BaseEvent()
            {
                eventTime = DateTime.UtcNow - LogManager.instance.SystemStartTime;
                eventName = this.GetType().Name;
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
            /// Format the instance to a csv record. 
            /// </summary>
            /// <returns></returns>
            public virtual string ToCSV()
            {
                return "";
            }

            /// <summary>
            /// Serialize the event to a json element.
            /// </summary>
            /// <returns></returns>
            public virtual string ToJson()
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

            public void AddToJsonEventQueue()
            {
                LogManager.instance.jsonEventQueue.Enqueue(this);
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


            #region Json Related Functions
            public static string JsonParsePos(Vector3 pos, Vector3 rot)
            {
                string msg = $"{{\"Position\":[{pos.x},{pos.y},{pos.z}],\"Rotation\":[{rot.x},{rot.y},{rot.z}]}}";
                return msg;
            }
            public static string JsonAddElement(string fieldName, string value, bool isFirstElement=false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";

                return prefix + $"\"{fieldName}\":\"{value}\"";
            }
            public static string JsonAddElement(string fieldName, int value, bool isFirstElement = false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";
                return prefix + $"\"{fieldName}\":{value}";
            }
            public static string JsonAddElement(string fieldName, float value, bool isFirstElement = false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";
                return prefix + $"\"{fieldName}\":{value}";
            }
            public static string JsonAddElement(string fieldName, double value, bool isFirstElement = false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";
                return prefix + $"\"{fieldName}\":{value}";
            }
            public static string JsonAddElement(string fieldName, bool value, bool isFirstElement = false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";
                string boolmsg = ""; // c# uses "True/False" rather than "true/false"
                if (value)
                    boolmsg = "true";
                else
                    boolmsg = "false";
                return prefix + $"\"{fieldName}\":{boolmsg}";
            }
            public static string JsonAddSubElement(string fieldName, string value, bool isFirstElement = false)
            {
                string prefix = "";
                if (!isFirstElement)
                    prefix += ",";
                return prefix + $"\"{fieldName}\":{{{value}}}";
            }

            public string JsonPrefix()
            {
                return "{" + JsonAddElement("Time", eventTime.ToString(), true) + JsonAddElement("Type", eventName);
            }
            public string JsonSuffix()
            {
                return "}";
            }


            #endregion
            }
    }
}