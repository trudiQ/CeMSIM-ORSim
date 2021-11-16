using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public enum GeneralEventType
        {
            ServerStart=1,      // referred in ServerInstance.start
            ServerStop,         // referred in ServerInstance.stop
            InitializeItems,    // referred in ServerItemManager.InitializeItems
        }

        /// <summary>
        /// GeneralEvent follows the eventSystem design pattern. The place where we add the "event action" is marked in the definiation of enumerator.
        /// </summary>
        public class GeneralEvent : BaseEvent
        {
            private GeneralEventType eventType;

            public GeneralEvent(GeneralEventType _eventType)
            {
                eventType = (GeneralEventType)_eventType;
            }
            public override string ToString()
            {
                string msg = string.Format("{0}: {1}",
                    eventTime,
                    eventType);
                return msg;
            }

            #region generate events

            private static void GenServerEvent(GeneralEventType _eventType)
            {
                using (GeneralEvent e = new GeneralEvent(_eventType))
                {
                    e.AddToGeneralEventQueue();
                }
            }

            public static void GenSeverStartEvent()
            {
                GenServerEvent(GeneralEventType.ServerStart);
            }

            public static void GenServerStopEvent()
            {
                GenServerEvent(GeneralEventType.ServerStop);

            }

            public static void GenInitializeItemsEvent()
            {
                GenServerEvent(GeneralEventType.InitializeItems);
            }

            #endregion
        }
    }
}