using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchiveLevelUi : MonoBehaviour
{
    [SerializeField] private Image achiveLevelPinUi;
    [SerializeField] private AchieveLevelSliderUi achiveLevelSliderUi;
    [SerializeField] private TextMeshProUGUI achieveLevelRewardText;
    [SerializeField] private Transform achieveLevelUisContainer;
    public void Assign(Dictionary<byte,object> achieveData)
    {
        //var achieveType = (string)levelsData[(byte)Params.AchieveType];

        var currentLevel = 0;
        var currentExp = 0;

        if (achieveData.ContainsKey((byte)Params.AchieveCurrentLevel))
        {
            currentLevel = (int)achieveData[(byte)Params.AchieveCurrentLevel];
            currentExp = (int)achieveData[(byte)Params.AchieveCurrentExp];
        }

        var levelsData = (Dictionary<byte, object>)achieveData[(byte)Params.AchieveLevels];

        foreach (var l in levelsData)
        {
            var level = l.Key;

            var levelData = (Dictionary<byte, object>)l.Value;

            var levelExp = (int)levelData[(byte)Params.AchieveLevelExp];
            var levelReward =(int)levelData[(byte)Params.AchieveLevelReward];

            var slider = Instantiate(achiveLevelSliderUi);
            UiHelper.AssignObjectToContainer(slider.gameObject, achieveLevelUisContainer);

            var paginator = Instantiate(achiveLevelPinUi);
            UiHelper.AssignObjectToContainer(paginator.gameObject, achieveLevelUisContainer);

            if (level == currentLevel+1)
            {
                achieveLevelRewardText.text = $"Награда {levelReward} респ.";

                //slider progress;
                var progress = (float)currentExp / (float)levelExp;

                Debug.Log($"{currentExp} {levelExp} {progress}");

                slider.SetSliderValue(progress);

                //color grey;
                paginator.color = Color.grey;              
            }

            if( currentLevel >= level)
            {
                //slider 100%
                slider.SetSliderValue( 1f);

                //color green
                paginator.color = Color.green;               
            }

            if(currentLevel+1 < level)
            {
                //slider 0%
                slider.SetSliderValue(0f);

                //color grey
                paginator.color = Color.grey;
            }
        }
    }
}
