using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role Avatar List", menuName = "CeMSIM/Roles")]
public class RoleAvatarList : ScriptableObject
{
    //public CEMSIM.GameLogic.Roles role;

    public string role; //Remove this line and uncomment other lines when merged into networking-avatar
    public RoleAvatar[] avatars;

    /*
    public string GetRole()
    {
        return role.ToString();
    }
    */
}
