using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleButtonUi : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    public string roleId;

    public void Assign(string roleId, Action action)
    {
        this.roleId = roleId;

        buttonText.text = Helper.GetRoleNameById_Rus( roleId);

        selectButton.GetComponent<Button>().onClick.AddListener(() => action());
    }
}
