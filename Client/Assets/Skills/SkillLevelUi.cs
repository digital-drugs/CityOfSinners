using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillLevelUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private TextMeshProUGUI skillCostText;
    [SerializeField] private TextMeshProUGUI skillValueText;

    public byte skillLevel { get; private set; }
    internal void Assign(KeyValuePair<byte, object> data, bool showValue, int userSkillLevel)
    {
        this.skillLevel = data.Key;
        skillLevelText.text = $"Уровень {skillLevel}";

        var skillLevelData = (Dictionary<byte, object>)data.Value;

        var skillCost = (int)skillLevelData[(byte)Params.SkillCost];
        skillCostText.text = $"Стоимость {skillCost}";

        if (showValue)
        {
            var skillValue = (int)skillLevelData[(byte)Params.SkillValue];
            skillValueText.text = $"Значение {skillValue}";
        }
        else
        {
            skillValueText.text = $"";
        }

        if (userSkillLevel >= this.skillLevel) 
        {
            SetOwned();
        }
    }

    [SerializeField] private Image skillOwnedImage;
    public void SetOwned()
    {
        skillOwnedImage.color = Color.green;
    }
}
