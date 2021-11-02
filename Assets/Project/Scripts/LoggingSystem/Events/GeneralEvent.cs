using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public enum GeneralEventType
        {
            ServerStart=1,
            ServerStop,
        }
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


            public static void GenSeverStartEvent()
            {
                using (GeneralEvent e = new GeneralEvent(GeneralEventType.ServerStart))
                {
                    e.AddToGeneralEventQueue();
                }

            }

            public static void GenServerStopEvent()
            {
                using (GeneralEvent e = new GeneralEvent(GeneralEventType.ServerStop))
                {
                    e.AddToGeneralEventQueue();
                }
            }
        }
    }
}