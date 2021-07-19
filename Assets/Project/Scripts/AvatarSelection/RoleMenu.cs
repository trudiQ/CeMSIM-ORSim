using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleMenu : MonoBehaviour
{
    public AvatarSwapper avatarSwapper;
    public Dropdown roleDropdown;
    public GameObject dropdownPrefab;
    public Transform spawnParent;
    public UnityEngine.UI.Button swapButton;

    private List<Dropdown> avatarDropdowns = new List<Dropdown>();
    private GameObject activeAvatarDropdownObject;
    private Dropdown activeAvatarDropdown;

    void Start()
    {
        roleDropdown.onValueChanged.AddListener(ChooseRole);
        swapButton.onClick.AddListener(avatarSwapper.SwapToSelectedAvatar);

        foreach(RoleAvatarList list in avatarSwapper.avatarLists)
        {
            roleDropdown.options.Add(new Dropdown.OptionData(list.role));
            roleDropdown.RefreshShownValue();

            Dropdown dropdown = Instantiate(original: dropdownPrefab,
                                              parent: spawnParent).GetComponent<Dropdown>();

            foreach(RoleAvatar avatar in list.avatars)
                dropdown.options.Add(new Dropdown.OptionData(avatar.avatarName));

            dropdown.RefreshShownValue();

            dropdown.gameObject.SetActive(false);
            dropdown.onValueChanged.AddListener(ChooseAvatar);
            avatarDropdowns.Add(dropdown);
        }

        roleDropdown.value = avatarSwapper.defaultRole;
        roleDropdown.RefreshShownValue();
        ChooseRole(avatarSwapper.defaultRole);
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

            activeAvatarDropdown.value = 0;
            activeAvatarDropdown.RefreshShownValue();
            ChooseAvatar(0);
        }
    }

    public void ChooseAvatar(int index)
    {
        if(index >= 0 && index < activeAvatarDropdown.options.Count)
            avatarSwapper.ChooseAvatar(index);
    }

    public void SwapAvatar()
    {
        avatarSwapper.SwapToSelectedAvatar();
    }
}
