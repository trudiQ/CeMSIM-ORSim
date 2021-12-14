using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CEMSIM.Network;
using CEMSIM.VoiceChat;
using Dissonance.Audio.Playback;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class LogManager : MonoBehaviour
        {

            public static LogManager instance;
            [Tooltip("Period to store events to log files (seconds)")]
            public bool isClientSide = false;
            public int StorageInterval = 1;
            public string LogDir = "Logs/";
            public DateTime SystemStartTime = DateTime.UtcNow;

            private string generalLogFile = "CEMSIM_LOG" + DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".log";
            private string playerCSVFile = "CEMSIM_Player" + DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".csv";
            private string jsonLogFile = "CEMSIM_LOG" + DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".json";


            public Queue<BaseEvent> generalEventQueue = new Queue<BaseEvent>();
            public Queue<BaseEvent> playerEventQueue = new Queue<BaseEvent>();
            public Queue<BaseEvent> jsonEventQueue = new Queue<BaseEvent>();

            // Start is called before the first frame update
            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
                }
                else if (instance != this)
                {
                    // We only allow one instance of this class to exist.
                    // Destroy current instance if different from the current one.
                    Debug.Log("Another instance already exists. Destroy this one.");
                    Destroy(this);
                }

                generalLogFile = Path.Combine(LogDir, generalLogFile);
                playerCSVFile = Path.Combine(LogDir, playerCSVFile);
                jsonLogFile = Path.Combine(LogDir, jsonLogFile);

                // register event 
                registerPublicEvent();
                registerPlayerEvent();
                registerVoiceChatEvent();
                registerEnvironmentStateEvent();

                InitializeGeneralLog();
                InitializePlayerLog();
                InitializeJsonLog();

                StartCoroutine(StoreGeneralEvents());
                StartCoroutine(StorePlayerEvents());
                StartCoroutine(StoreJsonEvents());
            }

            private void OnApplicationQuit()
            {
                using (StreamWriter sw = File.AppendText(generalLogFile))
                {
                    while (generalEventQueue.Count > 0)
                    {
                        sw.WriteLine(generalEventQueue.Dequeue().ToString());
                    }
                }

                using (StreamWriter sw = File.AppendText(playerCSVFile))
                {
                    while (playerEventQueue.Count > 0)
                    {
                        sw.WriteLine(playerEventQueue.Dequeue().ToCSV());
                    }
                }

                using (StreamWriter sw = File.AppendText(jsonLogFile))
                {
                    while (jsonEventQueue.Count > 0)
                    {
                        sw.WriteLine(jsonEventQueue.Dequeue().ToJson() + ",");
                    }
                    sw.WriteLine("]");
                }
            }


            private void InitializeGeneralLog()
            {
                using (StreamWriter sw = File.CreateText(generalLogFile))
                {
                    sw.WriteLine($"---Begin Log {DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss")}---");
                }
            }

            private void InitializePlayerLog()
            {
                using (StreamWriter sw = File.CreateText(playerCSVFile))
                {
                    sw.WriteLine(PlayerEvent.GetHeader());
                }
            }

            private void InitializeJsonLog()
            {
                using (StreamWriter sw = File.CreateText(jsonLogFile))
                {
                    sw.WriteLine("[");
                }
            }


            #region store log files
            private IEnumerator StoreGeneralEvents()
            {
                yield return new WaitForSeconds(StorageInterval);
                ServerThreadManager.ExecuteOnMainThread(() =>
                {
                    using (StreamWriter sw = File.AppendText(generalLogFile))
                    {
                        while(generalEventQueue.Count > 0)
                        {
                            sw.WriteLine(generalEventQueue.Dequeue().ToString());
                        }
                    }

                });

                StartCoroutine(StoreGeneralEvents());
            }

            private IEnumerator StorePlayerEvents()
            {
                yield return new WaitForSeconds(StorageInterval);

                ServerThreadManager.ExecuteOnMainThread(() =>
                {
                    using (StreamWriter sw = File.AppendText(playerCSVFile))
                    {
                        while (playerEventQueue.Count > 0)
                        {
                            sw.WriteLine(playerEventQueue.Dequeue().ToCSV());
                        }
                    }

                });

                StartCoroutine(StorePlayerEvents());
            }

            private IEnumerator StoreJsonEvents()
            {
                yield return new WaitForSeconds(StorageInterval);

                ServerThreadManager.ExecuteOnMainThread(() =>
                {
                    using (StreamWriter sw = File.AppendText(jsonLogFile))
                    {
                        while (jsonEventQueue.Count > 0)
                        {
                            sw.WriteLine(jsonEventQueue.Dequeue().ToJson()+",");
                        }
                    }

                });

                StartCoroutine(StoreJsonEvents());
            }


            #endregion

            #region register triggers

            private void registerPublicEvent()
            {
                if (isClientSide)
                {

                }
                else
                {
                    ServerInstance.onServerStartTrigger += GeneralEvent.GenSeverStartEvent;
                    ServerInstance.onServerStopTrigger += GeneralEvent.GenServerStopEvent;
                    ServerItemManager.onItemInitializeTrigger += GeneralEvent.GenInitializeItemsEvent;
                }
            }

            private void registerPlayerEvent()
            {
                if (isClientSide)
                {

                }
                else
                {
                    ServerHandle.onPlayerEnterTrigger += PlayerEvent.GenPlayerEnterEvent;
                    ServerHandle.onPlayerMoveTrigger += PlayerEvent.GenPlayerMoveEvent;
                    ServerHandle.onPlayerItemPickupTrigger += PlayerEvent.GenPlayerItemPickupEvent;
                    ServerHandle.onPlayerItemDropoffTrigger += PlayerEvent.GenPlayerItemDropoffEvent;
                    ServerHandle.onPlayerItemMoveTrigger += PlayerEvent.GenPlayerItemMoveEvent;
                    ServerSend.onPlayerExitTrigger += PlayerEvent.GenPlayerExitEvent;
                }
            }

            private void registerVoiceChatEvent()
            {
                if (isClientSide)
                {
                    // Actually, client alone won't be able to run the voice chat by itself. 
                }
                else
                {
                    SamplePlaybackComponent.onPlayerSpeechFileCreating += VoiceChatEvent.GenAudioFileCreateEvent;
                    CEMSIMSpeakIndicator.onPlayerStartSpeaking += VoiceChatEvent.GenStartSpeakEvent;
                    CEMSIMSpeakIndicator.onPlayerStopSpeaking += VoiceChatEvent.GenStopSpeakEvent;
                }
            }

            private void registerEnvironmentStateEvent()
            {
                if (isClientSide)
                {

                }
                else
                {
                    ServerNetworkManager.onRoomLightBtnTrigger += RoomLightEvent.GenRoomLightEvent;
                }
            }

            #endregion
        }
    }
}