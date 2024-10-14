using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleScreenUi : MonoBehaviour
{
    public static RoleScreenUi ins;

    private void Awake()
    {
        ins = this;
    }

    public Dictionary<string, RoleData> roleDatas { get; private set; } 
    public Dictionary<string, RoleImageUi> roleImages { get; private set; }

    [SerializeField] private RoleImageUi Prefab_RoleImage;
    [SerializeField] private Transform Container_RoleImages;

    public void SetupRoles(ParameterDictionary parameters)
    {
        roleDatas = new Dictionary<string, RoleData>();
        roleImages = new Dictionary<string, RoleImageUi>();

        var roles = (Dictionary<string, object>)parameters[(byte)Params.Roles];

        foreach(var r in roles)
        {
            var roleId = r.Key;

            //setup role data
            var role = (Dictionary<byte, object>)r.Value;
            var roleData = new RoleData(role);
            roleDatas.Add(roleId, roleData);

            //load role sprites
            var newRoleImage = Instantiate(Prefab_RoleImage, Container_RoleImages);
            roleImages.Add(roleId, newRoleImage);
            newRoleImage.StartLoadImages(roleData);            
        }
    }
}

public class RoleData
{
    public string roleId;
    public string name;
    public string description;
    public string maleUrl;
    public string femaleUrl;
    public string unicUrl;

    public Sprite maleSprite;

    public RoleData(Dictionary<byte, object> data)
    {
        roleId = (string)data[(byte)Params.RoleId];
        name = (string)data[(byte)Params.RoleName];
        description = (string)data[(byte)Params.RoleDescription];
        maleUrl = (string)data[(byte)Params.RoleMaleUrl];
        femaleUrl = (string)data[(byte)Params.RoleFemaleUrl];
        unicUrl = (string)data[(byte)Params.RoleUnicUrl];
    }
}
