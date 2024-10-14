using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUi : MonoBehaviour
{
    public Image skillIco;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Button selectButton;
    private string skillId;
    private Dictionary<byte, object> skillData;
    public string imageUrl;
    public void Assign(string skillId, Dictionary<byte,object> skillData)
    {
        this.skillId = skillId;
        this.skillData = skillData;

        var skillName = (string)skillData[(byte)Params.SkillName];

        imageUrl = (string)skillData[(byte)Params.SkillUrl];
        ImageManager.instance.StartLoadTexture(skillIco, imageUrl);
        //Debug.Log($"{imageUrl}");

        skillNameText.text = $"{skillName}";

        var skillLevels = (Dictionary<byte, object>)skillData[(byte)Params.Levels];

        int userSkillLevel = -1;

        if (skillData.ContainsKey((byte)Params.UserSkillLevel))
        {
            userSkillLevel = (int)skillData[(byte)Params.UserSkillLevel];
        }      

        for (int i = 0; i < skillLevels.Count; i++)
        {
            var levelOwned = userSkillLevel > i;
            AddSkillLevelMark(levelOwned);
        }

        selectButton.onClick.AddListener(() => SelectSkill());
    }

    [SerializeField] private Transform skillLevelMarksContainer;
    private void AddSkillLevelMark(bool levelOwned)
    {
        var newSkillLevelMark = Instantiate(SkillScreenUi.instance.skillLevelMarkPrefab);
        UiHelper.AssignObjectToContainer(newSkillLevelMark.gameObject, skillLevelMarksContainer);

        if (levelOwned) newSkillLevelMark.GetComponent<Image>().color = Color.green;
    }

    public void UpdateSkillLevelMarkers(int userSkillLevel)
    {
        if (skillData.ContainsKey((byte)Params.UserSkillLevel))
        {
            skillData[(byte)Params.UserSkillLevel] = userSkillLevel;
        }
        else
        {
            skillData.Add((byte)Params.UserSkillLevel, userSkillLevel);
        }      

        var markers = skillLevelMarksContainer.GetComponentsInChildren<Image>();

        for (int i = 0; i < markers.Length; i++)
        {
            if (userSkillLevel > i)
            {
                markers[i].color = Color.green;
            }
        }            
    }

    private void SelectSkill()
    {
        SkillScreenUi.instance.SelectSkillUi(skillId,skillData);
    }
}
