using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ui_ButtonRemoveSkill : MonoBehaviour
{
    [SerializeField] private Button button;

    public void Assign(Action action)
    {
        button.onClick.AddListener(() => action());
    }
}
