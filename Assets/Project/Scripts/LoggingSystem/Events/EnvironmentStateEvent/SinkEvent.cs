using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class SinkEvent : EnvironmentStateEvent
        {
            private bool sinkState; // true: on, false: off

            public SinkEvent(bool _sinkState) : base(EnvironmentId.sink)
            {
                sinkState = _sinkState;
            }

            public override string ToString()
            {
                string msg = $"{eventTime}: {eventType} - {sinkState}";
                return msg;
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("EventType", eventType.ToString());
                msg += JsonAddElement("State", sinkState);
                msg += JsonSuffix();
                return msg;
            }

            public static void GenSinkEvent(bool _sinkState)
            {
                using (SinkEvent e = new SinkEvent(_sinkState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}