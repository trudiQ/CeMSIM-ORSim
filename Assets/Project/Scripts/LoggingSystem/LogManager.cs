using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CEMSIM.Network;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public class LogManager : MonoBehaviour
        {

            public static LogManager instance;
            [Tooltip("Period to store events to log files (seconds)")]
            public int StorageInterval = 1;
            public string LogDir = "Logs/";

            private string generalLogFile = "CEMSIM_LOG" + DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".log";
            private string playerCSVFile = "CEMSIM_Player" + DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".csv";

            public Queue<BaseEvent> generalEventQueue;
            public Queue<BaseEvent> playerEventQueue;

            // Start is called before the first frame update
            void Start()
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

                generalEventQueue = new Queue<BaseEvent>();
                playerEventQueue = new Queue<BaseEvent>();

                generalLogFile = Path.Combine(LogDir, generalLogFile);
                playerCSVFile = Path.Combine(LogDir, playerCSVFile);

                InitializeGeneralLog();
                InitializePlayerLog();

                // register general event trigger
                registerGeneralEventTrigger();


                StartCoroutine(StoreGeneralEvents());
                StartCoroutine(StorePlayerEvents());

            }


            private void InitializeGeneralLog()
            {
                using (StreamWriter sw = File.CreateText(generalLogFile))
                {
                    sw.WriteLine("---Begin Log---");
                }
            }

            private void InitializePlayerLog()
            {
                using (StreamWriter sw = File.CreateText(playerCSVFile))
                {
                    sw.WriteLine(PlayerEvent.GetHeader());
                }
            }

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


            private void registerGeneralEventTrigger()
            {
                // register event triggers
                ServerInstance.onServerStartTrigger += GeneralEvent.GenSeverStartEvent;
                ServerInstance.onServerStopTrigger += GeneralEvent.GenServerStopEvent;
            }



        }
    }
}