using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using CEMSIM.GameLogic;

namespace CEMSIM
{
    namespace Network
    {
        public class ClientPCConnetMenu : Menu<ClientPCConnetMenu>
        {
            [Header("References")]
            public GameObject enterButton;      // For enabling once connected to the network
            public InputField IPField;          // For player to enter the IP address for connecting to.
            public InputField portField;        // For player to enter the port address
            public InputField usernameField;    // For player to enter the intended username for the player.
            public InputField clientMsgField;   // For player to type message for the server (Debug).
            public InputField serverMsgField;   // For displaying server responses (Debug).


            private void Start()
            {
                IPField.text = ClientInstance.instance.ip;
                portField.text = ClientInstance.instance.port + "";
                usernameField.text = ClientInstance.instance.myUsername;

            }

            //To Do: Create UI for identifying if user is entering using VR or desktop
            //public bool vREnabled;

            private void Update()
            {
                ClientInstance.instance.CheckConnection();
                enterButton.GetComponent<Selectable>().interactable = ClientInstance.instance.isConnected;
                if (!ClientInstance.instance.isConnected)
                {
                    //Debug.Log($"UDP:{ClientInstance.instance.udp.isUDPConnected} TCP: {ClientInstance.instance.tcp.isTCPConnected}");
                }
            }

            /// <summary>
            /// Callback function for button "Connect"
            /// </summary>
            public void ConnectOnClick()
            {
                // connect to the server via TCP and UDP
                string _ip = IPField.text;
                int _port = Int32.Parse(portField.text);
                string _username = usernameField.text;
                UpdateServerMessage("Connecting to Server");
                ClientInstance.instance.SetUsername(_username);
                ClientInstance.instance.ConnectToServer(_ip, _port);
            }

            /// <summary>
            /// Callback function for button "Send Via TCP"
            /// </summary>
            public void SendViaTCPOnClick()
            {
                string _msg = clientMsgField.text;
                ClientSend.SendTCPPing(_msg);
            }

            public void SendViaUDPOnClick()
            {
                string _msg = clientMsgField.text;
                ClientSend.SendUDPPing(_msg);
            }

            public void EnterOROnClick()
            {
                // disable the manu and request to enter the OR
                this.gameObject.SetActive(false);

                // we use Player + id to temporarily represent the player username
                //string _username = "Player" + ClientInstance.instance.myId.ToString();
                string _username = usernameField.text;

                ClientSend.SendSpawnRequest(_username, GameManager.instance.localPlayerVR.activeInHierarchy);
            }

            public void UpdateServerMessage(string serverMsg)
            {
                serverMsgField.text = serverMsg;
            }
        }
    }
}
