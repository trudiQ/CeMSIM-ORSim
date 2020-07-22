using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class deals with the panel for network tests.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    // controls
    public GameObject networkPanel;
    public Button connectButton;
    public Button sendViaTCPButton;
    public Button sendViaUDPButton;
    public InputField clientMsgField;   // for player to type message for the server
    public InputField serverMsgField;   // for display server responses.

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

        //// initialization to controls
        //serverMsgField.interactable = false; // this control is for display only
    }

    /// <summary>
    /// Callback function for button "Connect"
    /// </summary>
    public void ConnectOnClick()
    {
        // connect to the server via TCP
        serverMsgField.text = "Connecting to Server";
        Client.instance.ConnectedToServer();
        
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
        networkPanel.SetActive(false);

        // we use Player + id to temporarily represent the player username
        string _username = "Player" + Client.instance.myId.ToString(); 

        ClientSend.SendSpawnRequest(_username);

    }

}
