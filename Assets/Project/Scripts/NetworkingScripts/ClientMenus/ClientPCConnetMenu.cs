using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CEMSIM
{
    namespace Network
    {
        namespace Client
        {
            public class ClientPCConnetMenu : Menu<ClientPCConnetMenu>
            {
                [Header("References")]
                public GameObject enterButton;      // For enabling once connected to the network
                public InputField IPField;          // For player to enter the IP address for connecting to.
                public InputField clientMsgField;   // For player to type message for the server (Debug).
                public InputField serverMsgField;   // For displaying server responses (Debug).

                //To Do: Create UI for identifying if user is entering using VR or desktop
                //public bool vREnabled;

                private void Update()
                {
                    enterButton.GetComponent<Selectable>().interactable = Client.instance.isConnected;
                }

                /// <summary>
                /// Callback function for button "Connect"
                /// </summary>
                public void ConnectOnClick()
                {
                    // connect to the server via TCP
                    UpdateServerMessage("Connecting to Server");
                    Client.instance.ConnectToServer(IPField.text);
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
                    string _username = "Player" + Client.instance.myId.ToString();

                    //TO DO: Allow user to set 
                    ClientSend.SendSpawnRequest(_username, GameManager.instance.localPlayerVR.activeInHierarchy);
                }

                public void UpdateServerMessage(string serverMsg)
                {
                    serverMsgField.text = serverMsg;
                }
            }
        }
    }
}
