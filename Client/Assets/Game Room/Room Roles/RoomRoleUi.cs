using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Share;
using System;
using TMPro;
using SimpleGif.Data;
using UnityEngine.Networking;

public class RoomRoleUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roleNameText;
    public Image roleImage;

    public RoleType roleType;
    public int roleCount;

    public string roleUrl { get; private set; }

    public Dictionary<byte, object> roleData;

    public void Assign(Dictionary<byte, object> roleData)
    {
        this.roleData = roleData;

        var roleId = (string)roleData[(byte)Params.RoleId];
        roleType = (RoleType)Helper.GetEnumElement<RoleType>(roleId);

        var roleName = (string)roleData[(byte)Params.RoleName];

        roleUrl = (string)roleData[(byte)Params.RoleMaleUrl];
        GameRoomUi.instance.StartLoadTexture(roleImage, roleUrl); 

        roleNameText.text = $"{roleName}";

        IncreaseCount();
    }

    [SerializeField] private GameObject counter_GO;
    [SerializeField] private TextMeshProUGUI countText;
    public void IncreaseCount()
    {
        roleCount ++;

        if (roleCount > 1)
        {
            counter_GO.SetActive(true);
            countText.text = $"{roleCount}";
        }
        else
        {
            counter_GO.SetActive(false);
            countText.text = "";
        }
        
    }

    public void DecreaseCount()
    {
        roleCount--;

        countText.text = $"{roleCount}";

        if (roleCount == 0)
        {
            counter_GO.SetActive(false);
            gameObject.SetActive(false);
        }

        if (roleCount == 1)
        {
            counter_GO.SetActive(false);            
        }
    }

    [SerializeField] private Image bgImage;
    public void SetActive()
    {
        bgImage.color = Color.green;
    }

    [SerializeField] private GameObject deadBg;
    [SerializeField] private GameObject liveBg;
    public void KillRole()
    {
        deadBg.SetActive(true );
        liveBg.SetActive(false);
        roleImage.color = Color.gray;
    }   
}
