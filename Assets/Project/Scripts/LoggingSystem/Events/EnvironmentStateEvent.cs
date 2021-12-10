using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        // Since different environment state can contain a high diversity of state variable(s),
        // we requires the each event to have its own class.

        public class EnvironmentStateEvent : BaseEvent
        {
            // since we defined environment id in GameLogic/Common/Environment/EncironmentId.cs, no need to re-define
            protected EnvironmentId eventType;

            public EnvironmentStateEvent(EnvironmentId _eventType)
            {
                eventType = _eventType;
            }

            public override string ToString()
            {
                string msg = $"{eventTime}: {eventType}";
                return msg;
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("EventType", eventType.ToString());
                msg += JsonSuffix();
                return msg;
            }
        }

        public class RoomLightEvent : EnvironmentStateEvent
        {
            private bool lightState;

            public RoomLightEvent(bool _lightState) : base(EnvironmentId.roomLight)
            {
                lightState = _lightState;
            }

            public override string ToString()
            {
                string msg = $"{eventTime}: {eventType} - {lightState}";
                return msg;
            }

            public override string ToJson()
            {
                string msg = JsonPrefix();
                msg += JsonAddElement("EventType", eventType.ToString());
                msg += JsonAddElement("State", lightState);
                msg += JsonSuffix();
                return msg;
            }

            public static void GenRoomLightEvent(bool _btnState)
            {
                using (RoomLightEvent e = new RoomLightEvent(_btnState))
                {
                    e.AddToGeneralEventQueue();
                    e.AddToJsonEventQueue();
                }
            }
        }
    }
}