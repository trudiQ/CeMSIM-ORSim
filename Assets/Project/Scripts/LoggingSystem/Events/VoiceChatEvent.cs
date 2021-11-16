using System.Collections;
using System.Collections.Generic;
using CEMSIM.Network;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public enum VoiceChatEventType
        {
            AudioFileRecording = 1, // 
            SpeakingStarts,         // referred in ServerInstance.start
            SpeakingEnds,           // referred in ServerInstance.stop
        }

        public class VoiceChatEvent : BaseEvent
        {
            private VoiceChatEventType eventType;
            private int clientid;
            private string clientuuid;
            private string audioFilename;

            public VoiceChatEvent(string _clientuuid, VoiceChatEventType _eventType,  string _audioFilename="")
            {
                eventType = (VoiceChatEventType)_eventType;
                clientuuid = _clientuuid;
                clientid = ServerInstance.clientuuid2IdDict[_clientuuid];
                audioFilename = _audioFilename;
            }

            public VoiceChatEvent(int _clientid, VoiceChatEventType _eventType, string _audioFilename = "")
            {
                eventType = (VoiceChatEventType)_eventType;
                clientid = _clientid;
                clientuuid = ServerInstance.clientId2uuidDict[_clientid];
                audioFilename = _audioFilename;
            }


            public override string ToString()
            {
                string msg = "";
                switch (eventType)
                {
                    case VoiceChatEventType.AudioFileRecording:
                        msg += $"{eventTime}: Player {clientid} {eventType} > {audioFilename}";
                        break;
                    case VoiceChatEventType.SpeakingStarts:
                    case VoiceChatEventType.SpeakingEnds:
                        msg += $"{eventTime}: Player {clientid} {eventType}";
                        break;
                }
            
                return msg;
            }

            #region Generate events

            public static void GenAudioFileCreateEvent(string _clientuuid, string _filename)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_clientuuid, VoiceChatEventType.AudioFileRecording, _filename))
                {
                    e.AddToGeneralEventQueue();
                }
            }

            public static void GenStartSpeakEvent(string _clientuuid)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_clientuuid, VoiceChatEventType.SpeakingStarts))
                {
                    e.AddToGeneralEventQueue();
                }
            }

            public static void GenStopSpeakEvent(string _clientuuid)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_clientuuid, VoiceChatEventType.SpeakingEnds))
                {
                    e.AddToGeneralEventQueue();
                }
            }
            #endregion

        }
    }
}