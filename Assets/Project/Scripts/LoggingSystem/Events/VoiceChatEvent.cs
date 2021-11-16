using System.Collections;
using System.Collections.Generic;
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
            private string speakerId;
            private string audioFilename;

            public VoiceChatEvent(string _speakerId, VoiceChatEventType _eventType,  string _audioFilename="")
            {
                eventType = (VoiceChatEventType)_eventType;
                speakerId = _speakerId;
                audioFilename = _audioFilename;
            }

            public override string ToString()
            {
                string msg = "";
                switch (eventType)
                {
                    case VoiceChatEventType.AudioFileRecording:
                        msg += $"{eventTime}: Player {speakerId} {eventType} > {audioFilename}";
                        break;
                    case VoiceChatEventType.SpeakingStarts:
                    case VoiceChatEventType.SpeakingEnds:
                        msg += $"{eventTime}: Player {speakerId} {eventType}";
                        break;
                }
            
                return msg;
            }

            #region Generate events

            public static void GenAudioFileCreateEvent(string _playerId, string _filename)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_playerId, VoiceChatEventType.AudioFileRecording, _filename))
                {
                    e.AddToGeneralEventQueue();
                }
            }

            public static void GenStartSpeakEvent(string _playerId)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_playerId, VoiceChatEventType.SpeakingStarts))
                {
                    e.AddToGeneralEventQueue();
                }
            }

            public static void GenStopSpeakEvent(string _playerId)
            {
                using (VoiceChatEvent e = new VoiceChatEvent(_playerId, VoiceChatEventType.SpeakingEnds))
                {
                    e.AddToGeneralEventQueue();
                }
            }
            #endregion

        }
    }
}