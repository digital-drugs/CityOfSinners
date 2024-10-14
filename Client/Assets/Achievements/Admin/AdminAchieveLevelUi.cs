using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminAchieveLevelUi : MonoBehaviour
{
    public int achieveLevel=0;

    public void Assign(byte achieveLevel)
    {
        SetAchieveLevel(achieveLevel);

        //deleteButton.onClick.AddListener(() => RemoveLevel());
    }

    public TextMeshProUGUI achieveLevelText;
    public TMP_InputField achieveLevelExp;
    public TMP_InputField achieveLevelReward;
    [SerializeField] private Button deleteButton;
    public void SetAchieveLevel(byte achieveLevel)
    {
        this.achieveLevel = achieveLevel;
        achieveLevelText.text = $"{achieveLevel}";
    }

    //public void Assign(KeyValuePair<byte, object> data)
    //{
    //    var skillLevel = data.Key;
    //    SetAchieveLevel(skillLevel);

    //    var skillLevelData = (Dictionary<byte, object>)data.Value;

    //    var skillCost = (int)skillLevelData[(byte)Params.SkillCost];
    //    this.skillCost.text = $"{skillCost}";

    //    var skillChance = (int)skillLevelData[(byte)Params.SkillValue];
    //    this.skillChance.text = $"{skillChance}";

    //    deleteButton.onClick.AddListener(() => RemoveLevel());
    //}

    //private void RemoveLevel()
    //{
    //    AdminAchieve.ins.RemoveSkillLevel(this);
    //}
}
