using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchiveButtonUi : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    public void Assign(string roleId, Action action)
    {
        buttonText.text = roleId;

        selectButton.GetComponent<Button>().onClick.AddListener(() => action());
    }
}
