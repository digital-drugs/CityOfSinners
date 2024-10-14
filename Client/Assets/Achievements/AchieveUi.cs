using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchieveUi : MonoBehaviour
{
    [SerializeField] private Image achieveIco;
    [SerializeField] private TextMeshProUGUI achieveNameText;
    [SerializeField] private Button selectButton;
    private string achieveId;
    private Dictionary<byte, object> achieveData;
    public void Assign(string achieveId, Dictionary<byte, object> achieveData)
    {
        this.achieveId = achieveId;
        this.achieveData = achieveData;

        var achieveName = (string)achieveData[(byte)Params.AchieveName];

        achieveNameText.text = $"{achieveName}";      

        int userAcieveLevel = -1;

        if (achieveData.ContainsKey((byte)Params.AchieveCurrentLevel))
        {
            userAcieveLevel = (int)achieveData[(byte)Params.AchieveCurrentLevel];
        }

        var achieveLevels = (Dictionary<byte, object>)achieveData[(byte)Params.AchieveLevels];

        for (int i = 0; i < achieveLevels.Count; i++)
        {
            var levelOwned = userAcieveLevel > i;
            AddAchieveLevelMark(levelOwned);
        }

        selectButton.onClick.AddListener(() => SelectAchieve());
    }

    [SerializeField] private Transform skillLevelMarksContainer;
    private void AddAchieveLevelMark(bool levelOwned)
    {
        var newSkillLevelMark = Instantiate(SkillScreenUi.instance.skillLevelMarkPrefab);
        UiHelper.AssignObjectToContainer(newSkillLevelMark.gameObject, skillLevelMarksContainer);

        if (levelOwned) newSkillLevelMark.GetComponent<Image>().color = Color.green;
    }

    //public void UpdateSkillLevelMarkers(int userSkillLevel)
    //{
    //    if (skillData.ContainsKey((byte)Params.UserSkillLevel))
    //    {
    //        skillData[(byte)Params.UserSkillLevel] = userSkillLevel;
    //    }
    //    else
    //    {
    //        skillData.Add((byte)Params.UserSkillLevel, userSkillLevel);
    //    }

    //    var markers = skillLevelMarksContainer.GetComponentsInChildren<Image>();

    //    for (int i = 0; i < markers.Length; i++)
    //    {
    //        if (userSkillLevel > i)
    //        {
    //            markers[i].color = Color.green;
    //        }
    //    }
    //}

    private void SelectAchieve()
    {
        AchieveScreenUi.instance.SelectAchieveUi(achieveId, achieveData);
    }

}
