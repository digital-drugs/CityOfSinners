using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminAchieve : MonoBehaviour
{
    public static AdminAchieve ins;


    [SerializeField] private TMP_Dropdown achieveId_TmpDrop;
    [SerializeField] private TMP_Dropdown achieveType_TmpDrop;
    private void Start()
    {
        ins = this;

        UiHelper.SetupTMPDropMenu(achieveId_TmpDrop, typeof(AchieveId));
        UiHelper.SetupTMPDropMenu(achieveType_TmpDrop, typeof(AchieveType));
    }

    /// <summary>
    /// загрузка всех скиллов из БД
    /// </summary>
    //public void ButtonLoadSkills()
    //{
    //    PhotonManager.Inst.peer.SendOperation(
    //        (byte)Request.LoadSkills,
    //        new Dictionary<byte, object>(),
    //        PhotonManager.Inst.sendOptions);
    //}

    //public void ShowSkills(ParameterDictionary data)
    //{
    //    UiHelper.ClearContainer(adminSkillsUiContainer);

    //    var skillsData = (Dictionary<string, object>)data[(byte)Params.Skills];

    //    Debug.Log(skillsData.Count);

    //    foreach (var sd in skillsData)
    //    {
    //        AddAdminSkillUi(sd);
    //    }
    //}

    //[SerializeField] private AdminSkillUi adminSkillUiPrefab;
    //[SerializeField] private Transform adminSkillsUiContainer;
    //private void AddAdminSkillUi(KeyValuePair<string, object> data)
    //{
    //    var newAdminSkillUi = Instantiate(adminSkillUiPrefab);
    //    UiHelper.AssignObjectToContainer(newAdminSkillUi.gameObject, adminSkillsUiContainer);

    //    newAdminSkillUi.Assign(data);
    //}



    [SerializeField] private TMP_InputField achieveName;
    [SerializeField] private TMP_InputField achieveDescription;
    /// <summary>
    /// сохранение скилла в БД
    /// </summary>
    public void ButtonSaveAchieve()
    {
        var achieveData = new Dictionary<byte, object>();
       

        achieveData.Add((byte)Params.AchieveId, achieveId_TmpDrop.captionText.text);
        achieveData.Add((byte)Params.AchieveType, achieveType_TmpDrop.captionText.text);

        achieveData.Add((byte)Params.AchieveName, achieveName.text);
        achieveData.Add((byte)Params.AchieveDescription, achieveDescription.text);

        var achieveLevelsData = new Dictionary<byte, object>();
        achieveData.Add((byte)Params.AchieveLevels, achieveLevelsData);

        var achieveLevelUis = achieveLevelUisContainer.GetComponentsInChildren<AdminAchieveLevelUi>();

        foreach (var al in achieveLevelUis)
        {
            var achieveLevelData = new Dictionary<byte, object>();

            achieveLevelData.Add((byte)Params.AchieveLevel, al.achieveLevel);

            var achieveLevelExp = int.Parse(al.achieveLevelExp.text);
            achieveLevelData.Add((byte)Params.AchieveLevelExp, achieveLevelExp);

            var achieveLevelReward = int.Parse(al.achieveLevelReward.text);
            achieveLevelData.Add((byte)Params.AchieveLevelReward, achieveLevelReward);

            achieveLevelsData.Add((byte)al.achieveLevel, achieveLevelData);
        }

        PhotonManager.Inst.peer.SendOperation((byte)Request.SaveAchieve, achieveData, PhotonManager.Inst.sendOptions);
    }

    //public void AddSkillLevels(Dictionary<byte, object> skillLevels)
    //{
    //    UiHelper.ClearContainer(skillLevelsUiContainer);

    //    skillLevel = 0;

    //    foreach (var sl in skillLevels)
    //    {
    //        AddSkillLevel(sl);
    //        skillLevel++;
    //    }
    //}

    [SerializeField] private AdminAchieveLevelUi adminAchieveLevelUi;
    [SerializeField] private Transform achieveLevelUisContainer;
    private byte achieveLevel = 0;
    /// <summary>
    /// добавить в форму скилла уровень
    /// </summary>
    public void AddAchieveLevel()
    {
        var newAchieveLevelUi = Instantiate(adminAchieveLevelUi);

        UiHelper.AssignObjectToContainer(newAchieveLevelUi.gameObject, achieveLevelUisContainer);

        achieveLevel++;

        newAchieveLevelUi.Assign(achieveLevel);
    }

    //public void AddSkillLevel(KeyValuePair<byte, object> data)
    //{
    //    var newSkillLevelUi = Instantiate(adminSkillLevelUiPrefab);

    //    UiHelper.AssignObjectToContainer(newSkillLevelUi.gameObject, skillLevelsUiContainer);

    //    skillLevel++;

    //    newSkillLevelUi.Assign(data);
    //}

    //public void RemoveSkillLevel(AdminSkillLevelUi skillLevelUi)
    //{
    //    UiHelper.MoveUiObjectToTrash(skillLevelUi.gameObject);

    //    var skillLevelsUi = skillLevelsUiContainer.GetComponentsInChildren<AdminSkillLevelUi>();
    //    skillLevel = 0;

    //    foreach (var sl in skillLevelsUi)
    //    {
    //        skillLevel++;
    //        sl.SetSkillLevel(skillLevel);
    //    }
    //}

    //public void AssignSkillToForm(KeyValuePair<string, object> data)
    //{
    //    var skillId = data.Key;

    //    var optionIndex = this.skillId.options.FindIndex(x => x.text == skillId);
    //    this.skillId.value = optionIndex;

    //    var skillData = (Dictionary<byte, object>)data.Value;

    //    var roleId = (string)skillData[(byte)Params.RoleId];
    //    optionIndex = this.roleId.options.FindIndex(x => x.text == roleId);
    //    this.roleId.value = optionIndex;

    //    var skillName = (string)skillData[(byte)Params.SkillName];
    //    this.skillName.text = skillName;

    //    var skillDescription = (string)skillData[(byte)Params.SkillDescription];
    //    this.skillDescription.text = skillDescription;

    //    var skillLevels = (Dictionary<byte, object>)skillData[(byte)Params.Levels];

    //    AddSkillLevels(skillLevels);
    //}
}
