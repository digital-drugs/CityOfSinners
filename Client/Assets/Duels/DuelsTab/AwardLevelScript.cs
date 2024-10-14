using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AwardLevelScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI number;


    public void Assign(string points, int number)
    {
        this.points.text = points;
        this.number.text = number.ToString();
    }
}
