using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

    void Start()
    {
        // Subscribe to events when values change or buttons are pressed
        nameField.onValueChanged.AddListener(onNameChanged.Invoke);             // Name change
        roleDropdown.onValueChanged.AddListener(ChooseRole);                    // Role change
        previewButton.onClick.AddListener(ChangePreview);                       // Avatar preview
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
            }); // To swap avatars on selection

            avatarDropdowns.Add(dropdown);
        }

        roleDropdown.value = avatarSwapper.defaultRole;
        roleDropdown.RefreshShownValue();
        SetRoleDropdownsInteractable(false); // To prevent swapping avatars before height calibration

        ChooseRoleAndAvatar(avatarSwapper.defaultRole, avatarSwapper.defaultAvatar);
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
            ChooseAvatar(0);
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
        int selectedRole = avatarSwapper.activeRole;
        int selectedAvatar = avatarSwapper.activeAvatar;

        GameObject previewPrefab = avatarSwapper.avatarLists[selectedRole].avatars[selectedAvatar].previewPrefab;

        preview.Preview(previewPrefab);
    }
}
