using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvailableRole100Ui : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roleNameText;

    public RoleType roleType;
    public void Assign(RoleType roleType)
    {
        this.roleType = roleType;

        roleNameText.text = Helper.GetRoleNameById_Rus(roleType);
    }

    public bool isOn { get; private set; }
    [SerializeField] private Toggle toggle;
    public void Check()
    {
        isOn = toggle.isOn;
    }
}
