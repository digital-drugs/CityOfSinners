using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameRoleUi : MonoBehaviour
{
    private RoomBuilderUi roomBuilderUi;
    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private Button addButton;
    public RoleType roleType;
    public void Assign(RoleType roleType, RoomBuilderUi roomBuilderUi)
    {
        this.roleType = roleType;
        roleNameText.text = Helper.GetRoleNameById_Rus(roleType);// $"{roleType}";

        this.roomBuilderUi = roomBuilderUi;

        addButton.onClick.AddListener(() => RemoveRoleInGame());
    }

    private void RemoveRoleInGame()
    {
        roomBuilderUi.RemoveInGameRole(this);
    }
}
