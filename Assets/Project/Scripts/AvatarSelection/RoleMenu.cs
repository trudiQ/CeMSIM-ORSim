using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CEMSIM.GameLogic;
using CEMSIM.Network;

public class RoleMenu : MonoBehaviour
{
    public AvatarSwapper avatarSwapper;

    [Header("Dynamic Dropdown")]
    public Dropdown roleDropdown;
    public GameObject dropdownPrefab;
    public Transform dropdownParent;

    [Header("Event Components")]
    public InputField nameField;
    public UnityEngine.UI.Button previewButton;
    public AvatarPreview preview;
    public InputField ipHostnameField;
    public InputField portField;
    public UnityEngine.UI.Button connectButton;
    

    private List<Dropdown> avatarDropdowns = new List<Dropdown>();
    private GameObject activeAvatarDropdownObject;
    private Dropdown activeAvatarDropdown;

    [Header("Events")]
    public UnityEvent<string> onNameChanged;
    public UnityEvent<string> onIpHostnameChanged;
    public UnityEvent<string> onPortChanged;
    public UnityEvent onConnect;

    private bool connectionCheck = false;
    private bool isPlayerSpawned = false;

    void Start()
    {
        // Set initial values to each field 
        nameField.text = ClientInstance.instance.myUsername;
        ipHostnameField.text = ClientInstance.instance.ip;
        portField.text = ClientInstance.instance.port.ToString();


        // Subscribe to events when values change or buttons are pressed
        nameField.onValueChanged.AddListener(onNameChanged.Invoke);             // Name change

        roleDropdown.onValueChanged.AddListener((value) => {
            ChooseRoleAndAvatar(value, 0);
            SwapAvatar();
            ChangePreview();
        });                                                                     // Role change

        ipHostnameField.onValueChanged.AddListener(onIpHostnameChanged.Invoke); // IP / Hostname change
        portField.onValueChanged.AddListener(onPortChanged.Invoke);             // Port change
        connectButton.onClick.AddListener(onConnect.Invoke);                    // Connect pressed

        // Initialize each role dropdown
        foreach(RoleAvatarList list in avatarSwapper.avatarLists)
        {
            roleDropdown.options.Add(new Dropdown.OptionData(list.role));
            roleDropdown.RefreshShownValue();

            Dropdown dropdown = Instantiate(original: dropdownPrefab,
                                              parent: dropdownParent).GetComponent<Dropdown>();

            foreach(RoleAvatar avatar in list.avatars)
                dropdown.options.Add(new Dropdown.OptionData(avatar.avatarName));

            dropdown.RefreshShownValue();

            dropdown.gameObject.SetActive(false);

            dropdown.onValueChanged.AddListener( (value) => 
            { 
                ChooseAvatar(value);
                SwapAvatar();
                ChangePreview();
            }); // To swap avatars on selection

            avatarDropdowns.Add(dropdown);
        }

        roleDropdown.value = avatarSwapper.defaultRole;
        roleDropdown.RefreshShownValue();
        SetRoleDropdownsInteractable(false); // To prevent swapping avatars before height calibration

        ChooseRoleAndAvatar(avatarSwapper.defaultRole, avatarSwapper.defaultAvatar);
    }

    void Update()
    {
        if (connectionCheck)
        {
            // the connect button has been pressed
            if(ClientInstance.instance.CheckConnection())
            {
                if (!isPlayerSpawned)
                {
                    // use to guarantee that player will only be spawned once.
                    isPlayerSpawned = true;
                    connectButton.GetComponent<Selectable>().interactable = false;
                    connectButton.GetComponentInChildren<Text>().text = "Entering";
                    StartCoroutine(ClientInstance.instance.DelaySpawnRequest(1f));
                    StartCoroutine(DelayDestroy());
                    
                }
            }
            else
            {
                connectButton.GetComponentInChildren<Text>().text = "Reconnect";
                connectButton.GetComponent<Selectable>().interactable = true;
                Debug.Log($"TCP: {ClientInstance.instance.tcp.isTCPConnected} UDP: {ClientInstance.instance.udp.isUDPConnected}");
            }
        }
    }

    public void SetRoleDropdownsInteractable(bool state)
    {
        roleDropdown.interactable = state;

        foreach (Dropdown dropdown in avatarDropdowns)
            dropdown.interactable = state;

        previewButton.interactable = state;
    }

    public void ChooseRole(int index)
    {
        if(index >= 0 && index < roleDropdown.options.Count)
        {
            if(activeAvatarDropdownObject)
                activeAvatarDropdownObject.SetActive(false); // Hide old

            activeAvatarDropdown = avatarDropdowns[index];
            activeAvatarDropdownObject = avatarDropdowns[index].gameObject;

            activeAvatarDropdownObject.SetActive(true); // Show new

            avatarSwapper.ChooseRole(index);
            //avatarSwapper.ChooseRole((CEMSIM.GameLogic.Roles)index);

            activeAvatarDropdown.value = 0; // Make sure new dropdown has the same value as the selected role
            activeAvatarDropdown.RefreshShownValue();

            ClientInstance.instance.role = (Roles)index;
        }
    }

    public void ChooseAvatar(int index)
    {
        if(index >= 0 && index < activeAvatarDropdown.options.Count)
            avatarSwapper.ChooseAvatar(index);
    }

    public void ChooseRoleAndAvatar(int roleIndex, int avatarIndex)
    {
        ChooseRole(roleIndex);
        ChooseAvatar(avatarIndex);
    }

    public void SwapAvatar()
    {
        avatarSwapper.SwapToSelectedAvatar();
    }

    public void ChangePreview()
    {
        int selectedRole = avatarSwapper.selectedRole;
        int selectedAvatar = avatarSwapper.selectedAvatar;

        GameObject previewPrefab = avatarSwapper.avatarLists[selectedRole].avatars[selectedAvatar].previewPrefab;

        preview.Preview(previewPrefab);
    }

    public void OnConnectClick()
    {

        connectButton.GetComponentInChildren<Text>().text = "Connecting";
        //connectButton.GetComponent<Selectable>().interactable = !ClientInstance.instance.isConnected;
        connectButton.GetComponent<Selectable>().interactable = false;
        string _ip = ipHostnameField.text;
        int _port = int.Parse(portField.text);


        ClientInstance.instance.SetUsername(nameField.text);
        ClientInstance.instance.ConnectToServer(_ip, _port);

        connectionCheck = true;
    }



    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(3f);

        this.gameObject.SetActive(false);

        //Debug.Log($"Connection {ClientInstance.instance.CheckConnection()}");
        //this.gameObject.SetActive(!ClientInstance.instance.isConnected); //conceal the menu if connected
        //connectButton.GetComponent<Selectable>().interactable = !ClientInstance.instance.isConnected;
        //if (ClientInstance.instance.CheckConnection())
        //{
        //    Debug.Log($"user id {ClientInstance.instance.myId}");
        //    StartCoroutine(ClientInstance.instance.DelaySpawnRequest(1f));
        //    GameManager.instance.localPlayerVR.GetComponent<PlayerManager>().InitializePlayerManager(
        //            ClientInstance.instance.myId,
        //            nameField.text,
        //            ClientInstance.instance.role,
        //            true,   // at the client side?
        //            true    // VR player?
        //            );
        //}
        //else
        //{
        //    connectButton.GetComponentInChildren<Text>().text = "Reconnection";
        //}
    }
}
