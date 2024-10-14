using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillAdmin : MonoBehaviour
{
    public static SkillAdmin ins;

    private void Start()
    {
        ins = this;

        UiHelper.SetupTMPDropMenu(skillId, typeof(SkillEffect));

        UiHelper.SetupTMPDropMenu(roleId, typeof(RoleType));
    }

    /// <summary>
    /// загрузка всех скиллов из БД
    /// </summary>
    public void ButtonLoadSkills()
    {
        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadSkills,
            new Dictionary<byte, object>(), 
            PhotonManager.Inst.sendOptions);
    }
   
    public void ShowSkills(ParameterDictionary data)
    {
        UiHelper.ClearContainer(adminSkillsUiContainer);

        var skillsData = (Dictionary<string, object>)data[(byte)Params.Skills];

        Debug.Log(skillsData.Count);

        foreach (var sd in skillsData)
        {
            AddAdminSkillUi(sd);
        }
    }

    [SerializeField] private AdminSkillUi adminSkillUiPrefab;
    [SerializeField] private Transform adminSkillsUiContainer;
    private void AddAdminSkillUi(KeyValuePair<string, object> data)
    {
        var newAdminSkillUi = Instantiate(adminSkillUiPrefab);
        UiHelper.AssignObjectToContainer(newAdminSkillUi.gameObject, adminSkillsUiContainer);

        newAdminSkillUi.Assign(data);        
    }

    [SerializeField] private TMP_Dropdown roleId;
    [SerializeField] private TMP_Dropdown skillId;
    [SerializeField] private TMP_InputField skillName;
    [SerializeField] private TMP_InputField skillDescription;
    [SerializeField] private TMP_InputField skillUrl;
    /// <summary>
    /// сохранение скилла в БД
    /// </summary>
    public void ButtonSaveSkill()
    {
        var skillData = new Dictionary<byte,object>();
        var skillLevelsData = new Dictionary<byte,object>();

        skillData.Add((byte)Params.RoleId, roleId.captionText.text);
        skillData.Add((byte)Params.SkillId, skillId.captionText.text);
        skillData.Add((byte)Params.SkillName, skillName.text);
        skillData.Add((byte)Params.SkillDescription, skillDescription.text);
        skillData.Add((byte)Params.SkillUrl, skillUrl.text);

        var skillLevelsUi = skillLevelsUiContainer.GetComponentsInChildren<AdminSkillLevelUi>();

        foreach(var sl in skillLevelsUi)
        {
            var skillLevelData = new Dictionary<byte,object>();

            skillLevelData.Add((byte)Params.SkillLevel,sl.skillLevel);

            var skillCost = int.Parse(sl.skillCost.text);
            skillLevelData.Add((byte)Params.SkillCost, skillCost);

            var skillChance = byte.Parse(sl.skillChance.text);
            skillLevelData.Add((byte)Params.SkillValue, skillChance);

            skillLevelsData.Add(sl.skillLevel, skillLevelData);
        }

        skillData.Add((byte)Params.Level, skillLevelsData);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SaveSkill, skillData, PhotonManager.Inst.sendOptions);

    }

    public void AddSkillLevels(Dictionary<byte, object> skillLevels)
    {
        UiHelper.ClearContainer(skillLevelsUiContainer);

        skillLevel = 0;

        foreach (var sl in skillLevels)
        {
            AddSkillLevel(sl);
            skillLevel++;
        }
    }

    [SerializeField] private AdminSkillLevelUi adminSkillLevelUiPrefab;
    [SerializeField] private Transform skillLevelsUiContainer;
    private byte skillLevel = 0;
    /// <summary>
    /// добавить в форму скилла уровень
    /// </summary>
    public void AddSkillLevel()
    {
        var newSkillLevelUi = Instantiate(adminSkillLevelUiPrefab);

        UiHelper.AssignObjectToContainer(newSkillLevelUi.gameObject, skillLevelsUiContainer);

        skillLevel++;

        newSkillLevelUi.Assign(skillLevel);
    }

    public void AddSkillLevel(KeyValuePair<byte, object> data)
    {
        var newSkillLevelUi = Instantiate(adminSkillLevelUiPrefab);

        UiHelper.AssignObjectToContainer(newSkillLevelUi.gameObject, skillLevelsUiContainer);
        
        skillLevel++;

        newSkillLevelUi.Assign(data);
    }

    public void RemoveSkillLevel(AdminSkillLevelUi skillLevelUi)
    {
        UiHelper.MoveUiObjectToTrash(skillLevelUi.gameObject);

        var skillLevelsUi = skillLevelsUiContainer.GetComponentsInChildren<AdminSkillLevelUi>();
        skillLevel = 0;

        foreach(var sl in skillLevelsUi)
        {
            skillLevel++;
            sl.SetSkillLevel(skillLevel);
        }
    }

    public void AssignSkillToForm(KeyValuePair<string, object> data)
    {
        var skillId = data.Key;
        
        var optionIndex = this.skillId.options.FindIndex(x => x.text == skillId);
        this.skillId.value = optionIndex;

        var skillData = (Dictionary<byte, object>)data.Value;

        var roleId = (string)skillData[(byte)Params.RoleId];
        optionIndex = this.roleId.options.FindIndex(x => x.text == roleId);
        this.roleId.value = optionIndex;

        var skillName = (string)skillData[(byte)Params.SkillName];
        this.skillName.text = skillName;

        var skillDescription = (string)skillData[(byte)Params.SkillDescription];
        this.skillDescription.text = skillDescription;

        var skillLevels = (Dictionary<byte, object>)skillData[(byte)Params.Levels];

		 var skillUrl = (string)skillData[(byte)Params.SkillUrl];
        this.skillUrl.text = skillUrl;				
        
        AddSkillLevels(skillLevels);    
    }
}
