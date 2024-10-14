using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvailableRoleUi : MonoBehaviour
{
    private RoomBuilderUi roomBuilderUi;
    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private Button addButton;
    private RoleType roleType;
    public void Assign(RoleType roleType, RoomBuilderUi roomBuilderUi)
    {
        this.roleType = roleType;

        roleNameText.text = Helper.GetRoleNameById_Rus(roleType);// $"{roleType}";

        this.roomBuilderUi = roomBuilderUi;

        addButton.onClick.AddListener(() => AddRoleInGame());
    }

    private void AddRoleInGame()
    {
        roomBuilderUi.AddInGameRole(roleType);
    }
}
