using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Admin : MonoBehaviour
{
    public static Admin instance;

    private void Start()
    {
        instance = this;
    }

    [SerializeField] private GameObject adminMenu;
    public void CheckSystemRole(ParameterDictionary parameters)
    {
        var systemRole = (SystemRole)parameters[(byte)Params.UserSystemRole];

        switch(systemRole)
        {
            case SystemRole.admin:
                {
                    adminMenu.SetActive(true); 
                }
                break;

            case SystemRole.user:
                {
                    adminMenu.SetActive(false);
                }
                break;
        }
    }

    [SerializeField] private GameObject wi_AdminSkill;
    public void Show_WiAdminSkill()
    {
        wi_AdminSkill.SetActive(true);
    }

    public void Hide_WiAdminSkill()
    {
        wi_AdminSkill.SetActive(false);
    }

    [SerializeField] private GameObject wi_AdminExtra;

    public void Show_WiAdminExtra()
    {
        wi_AdminExtra.SetActive(true);
    }

    public void Hide_WiAdminExtra()
    {
        wi_AdminExtra.SetActive(false);
    }

    [SerializeField] private GameObject wi_AdminAchieve;

    public void Show_WiAdminAchieve()
    {
        wi_AdminAchieve.SetActive(true);
    }

    public void Hide_WiAdminAchieve()
    {
        wi_AdminAchieve.SetActive(false) ;
    }

}
