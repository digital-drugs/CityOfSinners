using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextContentUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI contentText;
    public void Assign(string text)
    {
        contentText.text = text;
    }   
}
