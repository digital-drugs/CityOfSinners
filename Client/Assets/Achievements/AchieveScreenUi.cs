using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchieveScreenUi : MonoBehaviour
{
    public static AchieveScreenUi instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] private GameObject wi_Achieves;
    [SerializeField] private AchieveUi achieveUiPrefab;
    private Dictionary<string, Transform> achieveContainers = new Dictionary<string, Transform>();
    private Dictionary<string, AchieveUi> achieveUis = new Dictionary<string, AchieveUi>();
    public void SetupAchieves(ParameterDictionary parameters)
    {
        var achievesData = (Dictionary<string, object>)parameters[(byte)Params.Achieves];

        //Debug.Log($"achieves => {achievesData.Count}");

        foreach (var ad in achievesData)
        {
            var achieveId = ad.Key;
            var achieveData = (Dictionary<byte, object>)ad.Value;

            var achieveType = (string)achieveData[(byte)Params.AchieveType];

            Transform containerTransform = null;

            if (achieveContainers.ContainsKey(achieveType))
            {
                containerTransform = achieveContainers[achieveType];
            }
            else
            {
                containerTransform = AddAchievePanel(achieveType);

                AddAchiveButton(achieveType, containerTransform);
            }

            var newAchiveUi = Instantiate(achieveUiPrefab, containerTransform);
            achieveUis.Add(achieveId, newAchiveUi);

            newAchiveUi.Assign(achieveId, achieveData);
        }
    }

    [SerializeField] private Transform achieveContainer;
    [SerializeField] private Transform achievePanelPrefab;
    private Transform AddAchievePanel(string achieveType)
    {
        var result = Instantiate(achievePanelPrefab);
        UiHelper.AssignObjectToContainer(result.gameObject, achieveContainer);

        result.name = $"{achieveType} achieves";
        achieveContainers.Add(achieveType, result);

        result.gameObject.SetActive(false);

        return result;
    }

    [SerializeField] private Transform achieveButtonsContainer;
    [SerializeField] private AchiveButtonUi achieveButtonUiPrefab;
    private void AddAchiveButton(string achieveType, Transform achieveContainer)
    {
        var newAchieveButton = Instantiate(achieveButtonUiPrefab);
        UiHelper.AssignObjectToContainer(newAchieveButton.gameObject, achieveButtonsContainer);


        Action buttonAction = () => { SelectAchive(achieveContainer); };

        newAchieveButton.Assign(achieveType, buttonAction);
    }

    private void SelectAchive(Transform achieveContainer)
    {
        //Debug.Log($"skillContainers {skillContainers.Count}");

        foreach (var c in achieveContainers.Values)
        {
            c.gameObject.SetActive(false);
        }

        achieveContainer.gameObject.SetActive(true);
    }

    public void OpenAchiveWindow()
    {
        wi_Achieves.SetActive(true);
    }

    public void CloseAchiveWindow()
    {
        wi_Achieves.SetActive(false);
    }

    [SerializeField] private TextMeshProUGUI achieveName;
    [SerializeField] private TextMeshProUGUI achieveDescription;
    [SerializeField] private AchiveLevelUi achiveLevelUi;
    [SerializeField] private Transform achieveLevelUisContainer;

    private string currentSelectedAchiveId;
    public void SelectAchieveUi(string achieveId, Dictionary<byte, object> achieveData)
    {
        UiHelper.ClearContainer(achieveLevelUisContainer);

        achieveName.text = (string)achieveData[(byte)Params.AchieveName];
        achieveDescription.text = (string)achieveData[(byte)Params.AchieveDescription];

        currentSelectedAchiveId = achieveId;

        achiveLevelUi.Assign(achieveData);
    }

    //public void LevelUpSelectedSkill()
    //{
    //    Debug.Log($"selected skill id = {currentSelectedSkillId}");

    //    var skillData = new Dictionary<byte, object>();

    //    skillData.Add((byte)Params.SkillId, currentSelectedSkillId);

    //    PhotonManager.Inst.peer.SendOperation((byte)Request.LevelUpSkill, skillData, PhotonManager.Inst.sendOptions);
    //}

    //public void UpdateSkillLevel(ParameterDictionary parameters)
    //{
    //    var skillId = (string)parameters[(byte)Params.SkillId];
    //    var userSkillLevel = (int)parameters[(byte)Params.SkillLevel];

    //    if (skillId == currentSelectedSkillId)
    //    {
    //        var skillLevelsUi = skillLevelUisContainer.GetComponentsInChildren<SkillLevelUi>();

    //        foreach (var slui in skillLevelsUi)
    //        {
    //            if (slui.skillLevel == userSkillLevel)
    //            {
    //                slui.SetOwned();
    //            }
    //        }
    //    }

    //    var skillUi = skillUis[skillId];
    //    skillUi.UpdateSkillLevelMarkers(userSkillLevel);
    //}

    public GameObject levelMarkPrefab;
}
