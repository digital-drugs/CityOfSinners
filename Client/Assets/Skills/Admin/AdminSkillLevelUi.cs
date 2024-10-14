using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminSkillLevelUi : MonoBehaviour
{
    public byte skillLevel = 0;
    public TextMeshProUGUI skillLevelText;
    public TMP_InputField skillCost;
    public TMP_InputField skillChance;
    [SerializeField] private Button deleteButton;   

    public void Assign(byte skillLevel)
    {
        SetSkillLevel(skillLevel);

        deleteButton.onClick.AddListener(() => RemoveLevel());
    }

    public void SetSkillLevel(byte skillLevel)
    {
        this.skillLevel = skillLevel;
        skillLevelText.text = $"{skillLevel}";
    }

    public void Assign(KeyValuePair<byte, object> data)
    {
        var skillLevel = data.Key;
        SetSkillLevel(skillLevel);

         var skillLevelData = (Dictionary<byte, object>)data.Value;

        var skillCost = (int)skillLevelData[(byte)Params.SkillCost];
        this.skillCost.text = $"{skillCost}";

        var skillChance = (int)skillLevelData[(byte)Params.SkillValue];
        this.skillChance.text = $"{skillChance}";

        deleteButton.onClick.AddListener(() => RemoveLevel());
    }

    private void RemoveLevel()
    {
        SkillAdmin.ins.RemoveSkillLevel(this);
    }
}
