using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminSkillUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Button selectButton;
    private KeyValuePair<string, object> rawData;

    public void Assign(KeyValuePair<string, object> data)
    {
        rawData = data;

        //var skillId = data.Key;
        var skillData = (Dictionary<byte, object>)data.Value;

        var skillName = (string)skillData[(byte)Params.SkillName];
        skillNameText.text= skillName;

        selectButton.onClick.AddListener(() => SelectSkill());
    }

    private void SelectSkill()
    {
        SkillAdmin.ins.AssignSkillToForm(rawData);
    }


}
