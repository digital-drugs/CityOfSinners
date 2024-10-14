using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillScreenUi : MonoBehaviour
{
    public static SkillScreenUi instance;

    private void Start()
    {
        if(instance==null)
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

    [SerializeField] private GameObject wi_Skills;
    [SerializeField] private SkillUi skillUiPrefab;  
    private Dictionary<string, Transform> skillContainers= new Dictionary<string, Transform>();
    private Dictionary<string, SkillUi> skillUis = new Dictionary<string, SkillUi>();
    public void SetupSkills(ParameterDictionary parameters)
    {
        var skillsData = (Dictionary<string, object>)parameters[(byte)Params.Skills];

        //Debug.Log($"skills => {skillsData.Count}");

        foreach (var sd in skillsData)
        {
            var skillId = sd.Key;
            var skillData = (Dictionary<byte, object>)sd.Value;

            var roleId = (string)skillData[(byte)Params.RoleId];

            Transform containerTransform = null;

            if (skillContainers.ContainsKey(roleId))
            {
                containerTransform = skillContainers[roleId];
            }
            else
            {
                containerTransform = AddRolePanel(roleId);

                AddRoleButton(roleId, containerTransform);                
            }

            var newSkillUi = Instantiate(skillUiPrefab, containerTransform);
            skillUis.Add(skillId, newSkillUi);

            newSkillUi.Assign(skillId, skillData);
        }

        AddTestRemoveRoleSkills();
    }

    [SerializeField] private Ui_ButtonRemoveSkill ui_ButtonRemoveSkill;
    private void AddTestRemoveRoleSkills()
    {
        foreach (var rb in roleButtons)
        {
            var containerTransform = skillContainers[rb.roleId];

            Action action = () =>
            {
                var skillData = new Dictionary<byte, object>();

                skillData.Add((byte)Params.RoleId, rb.roleId);

                PhotonManager.Inst.peer.SendOperation((byte)Request.RemoveSkills, skillData, PhotonManager.Inst.sendOptions);
            };

            var newButton = Instantiate(ui_ButtonRemoveSkill, containerTransform);

            newButton.Assign(action);
        }
    }

    public SkillUi FindSkillUi(SkillEffect skillId)
    {
        return FindSkillUi(skillId.ToString()) ;
    }

    public SkillUi FindSkillUi(string skillId)
    {
        if (skillUis.ContainsKey(skillId))
        {
            return skillUis[skillId];
        }

        return null;
    }

    [SerializeField] private Transform rolesContainer;
    [SerializeField] private Transform rolePanelPrefab;
    private Transform AddRolePanel(string roleId)
    {
        var result = Instantiate(rolePanelPrefab);
        UiHelper.AssignObjectToContainer(result.gameObject, rolesContainer);

        result.name = $"{roleId} skills";
        skillContainers.Add(roleId, result);

        result.gameObject.SetActive(false);

        return result;
    }

    [SerializeField] private Transform roleButtonsContainer;
    [SerializeField] private RoleButtonUi roleButtonUiPrefab;
    private List<RoleButtonUi> roleButtons = new List<RoleButtonUi>();
    private void AddRoleButton(string roleId ,Transform skillContainer)
    {
        var newRoleButton = Instantiate(roleButtonUiPrefab);
        roleButtons.Add(newRoleButton);
        UiHelper.AssignObjectToContainer(newRoleButton.gameObject, roleButtonsContainer);


        Action buttonAction = () => { SelectRole(skillContainer); };

        newRoleButton.Assign(roleId, buttonAction);       
    }

    private void SelectRole(Transform skillContainer)
    {
        Debug.Log($"skillContainers {skillContainers.Count}");

        foreach(var c in skillContainers.Values)
        {
            c.gameObject.SetActive(false);
        }

        skillContainer.gameObject.SetActive(true);
    }

    public void OpenSkillWindow()
    {
        wi_Skills.SetActive(true);
    }

    public void CloseSkillWindow()
    {
        wi_Skills.SetActive(false);
    }

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField]private TextMeshProUGUI skillDescription;
    [SerializeField] private SkillLevelUi skillLevelUiPrefab;
    [SerializeField] private Transform skillLevelUisContainer;

    private string currentSelectedSkillId;
    public void SelectSkillUi(string skillId, Dictionary<byte, object> skillData)
    {
        UiHelper.ClearContainer(skillLevelUisContainer);

        currentSelectedSkillId = skillId;

        skillName.text = (string)skillData[(byte)Params.SkillName];
        skillDescription.text = (string)skillData[(byte)Params.SkillDescription];

        int userSkillLevel = 0;
        if (skillData.ContainsKey((byte)Params.UserSkillLevel))
        {
            userSkillLevel = (int)skillData[(byte)Params.UserSkillLevel];
        }

        var levels = (Dictionary<byte,object>)skillData[(byte)Params.Levels];

        foreach(var l in levels)
        {
            var newSkillLevelUi = Instantiate(skillLevelUiPrefab);
            UiHelper.AssignObjectToContainer(newSkillLevelUi.gameObject, skillLevelUisContainer);

            var showValue = levels.Count > 1;

            newSkillLevelUi.Assign(l, showValue, userSkillLevel);          
        }
    }

    public void LevelUpSelectedSkill()
    {
        Debug.Log($"selected skill id = {currentSelectedSkillId}");

        var skillData = new Dictionary<byte, object>();

        skillData.Add((byte)Params.SkillId, currentSelectedSkillId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.LevelUpSkill, skillData, PhotonManager.Inst.sendOptions);
    }

    public void UpdateSkillLevel(ParameterDictionary parameters)
    {
        var skillId = (string)parameters[(byte)Params.SkillId];
        var userSkillLevel = (int)parameters[(byte)Params.SkillLevel];

        if(skillId == currentSelectedSkillId)
        {
            var skillLevelsUi = skillLevelUisContainer.GetComponentsInChildren<SkillLevelUi>();

            foreach(var slui in skillLevelsUi)
            {
                if(slui.skillLevel == userSkillLevel)
                {
                    slui.SetOwned();
                }
            }
        }

        var skillUi = skillUis[skillId];
        skillUi.UpdateSkillLevelMarkers(userSkillLevel);
    }

    public GameObject skillLevelMarkPrefab;
}
