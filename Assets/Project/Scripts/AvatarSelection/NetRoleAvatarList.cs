using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Net Role Avatar List", menuName = "CeMSIM/NetRoles")]
public class NetRoleAvatarList : ScriptableObject
{
    //public CEMSIM.GameLogic.Roles role;

    public string role; //Remove this line and uncomment other lines when merged into networking-avatar
    public NetRoleAvatar[] avatars;

    /*
    public string GetRole()
    {
        return role.ToString();
    }
    */
}
