using System.Collections;
using System.Collections.Generic;
using Dissonance;
using UnityEngine;
using CEMSIM.GameLogic;
using CEMSIM.Network;


namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMVoicePlayer : MonoBehaviour, IDissonancePlayer
        {
            public bool isMine = false;
            public bool isServerDummyPlayer = false; // true: a dummy player running at server
            private DissonanceComms comm;
            private Coroutine _startCo;

            public bool IsTracking { get; private set; }
            public string clientuuid { get; private set; } // the dissonance uuid 
            public string _PlayerId; // for display and debug use only. 

            [Tooltip("The gameobject whose transform will be used to represent the player's position. By default, it's the attached gameobject.")]
            public GameObject trackingPoint = null;

            public NetworkPlayerType Type
            {
                get { return isMine ? NetworkPlayerType.Local : NetworkPlayerType.Remote; }
            }

            public Vector3 Position
            {
                get { return trackingPoint.transform.position; }
            }

            public Quaternion Rotation
            {
                get { return trackingPoint.transform.rotation; }
            }

            public void initialization(bool isLocalPlayer=false, bool _isServerDummyPlayer = false)
            {
                comm = FindObjectOfType<DissonanceComms>();
                if (comm == null)
                {
                    Debug.LogError("cannot find DissonanceComms component in scene");
                    return;
                }

                isMine = isLocalPlayer;
                isServerDummyPlayer = _isServerDummyPlayer;

                if (isMine)
                {
                    Debug.Log("Create a local voice player");

                    if (comm.LocalPlayerName != null)
                        SetPlayerName(comm.LocalPlayerName);

                    //Subscribe to future name changes (this is critical because we may not have run the initial set name yet and this will trigger that initial call)
                    comm.LocalPlayerNameChanged += SetPlayerName;
                }
                else
                {
                    Debug.Log("Create a remote voice player");
                }
                if (trackingPoint == null)
                {
                    trackingPoint = gameObject; // itself
                }

            }



            private void SetPlayerName(string _clientuuid)
            {
                //We need to stop and restart tracking to handle the name change
                if (IsTracking)
                    StopTracking();

                //Perform the actual work
                clientuuid = _clientuuid;
                _PlayerId = _clientuuid; // Debug: for display only
                StartTracking();

                // multicast the playerId

                if (isMine)
                    if (isServerDummyPlayer)
                    {
                        ServerSend.SendVoiceChatPlayerId(0, clientuuid, true);
                        ServerInstance.dissonancePlayerId = clientuuid;

                        Debug.Log($"Server chooses dissonance player id {clientuuid}");

                    }
                    else
                        ClientSend.SendVoiceChatPlayerId(clientuuid);

            }
            
            public void ChangePlayerName(string _clientuuid)
            {
                SetPlayerName(_clientuuid);
                gameObject.GetComponent<CEMSIMSpeakIndicator>().UpdateState();
            }


            public void OnDestroy()
            {
                if (comm != null)
                    comm.LocalPlayerNameChanged -= SetPlayerName;
            }

            public void OnEnable()
            {
                if (!IsTracking)
                    StartTracking();
            }

            public void OnDisable()
            {
                if (IsTracking)
                    StopTracking();
            }

            private void StartTracking()
            {
                if (IsTracking)
                    Debug.LogWarning("Attempting to start player tracking, but tracking is already started, 0663D808-ACCC-4D13-8913-03F9BA0C8578");

                _startCo = StartCoroutine(StartTrackingCo());
            }

            private IEnumerator StartTrackingCo()
            {
                // Wait until Dissonance comms object is initialised
                while (comm == null)
                {
                    comm = FindObjectOfType<DissonanceComms>();
                    yield return null;
                }

                // Can't track someone with a null name! Loop until name is valid
                while (clientuuid == null)
                    yield return null;

                // Now start tracking
                comm.TrackPlayerPosition(this);
                IsTracking = true;

                _startCo = null;
            }

            private void StopTracking()
            {
                if (!IsTracking)
                    Debug.LogWarning("Attempting to stop player tracking, but tracking is not started, 48802E32-C840-4C4B-BC58-4DC741464B9A");

                // Stop startup coroutine if it is running
                if (_startCo != null)
                    StopCoroutine(_startCo);
                _startCo = null;

                if (comm != null)
                {
                    comm.StopTracking(this);
                    IsTracking = false;
                }
            }
        }
    }
}