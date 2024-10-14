using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraButtonUi : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void Assign(string extraType, Action action)
    {
        buttonText.text = extraType;

        selectButton.GetComponent<Button>().onClick.AddListener(() => action());
    }
}
