using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchieveLevelSliderUi : MonoBehaviour
{
    [SerializeField] private Image sliderImage;
    public void SetSliderValue(float        value)
    {
        sliderImage.fillAmount= value;
    }
}
