using System.Collections;
using System.Collections.Generic;
using CEMSIM.GameLogic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
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