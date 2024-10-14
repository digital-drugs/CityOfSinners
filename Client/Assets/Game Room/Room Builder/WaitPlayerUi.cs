using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitPlayerUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    public TMP_Dropdown roleDrop;

    public long id;
    public void Assign(ParameterDictionary parameters)
    {
        id = (long)parameters[(byte)Params.UserId];

        roleDrop.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var element in Enum.GetValues(typeof(RoleType)))
        {
            var rusName = Helper.GetRoleNameById_Rus((RoleType)element);
            
            options.Add(new TMP_Dropdown.OptionData(rusName));
        }

        roleDrop.AddOptions(options);

        var name = (string)parameters[(byte)Params.UserName];

        nameText.text = name;        
    }

    public void OnValueChange()
    {
        //Debug.Log($"selected role => {roleDrop.value} => {(RoleType)roleDrop.value}");
    }
}
