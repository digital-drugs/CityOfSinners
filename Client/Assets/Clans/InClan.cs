using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InClan : MonoBehaviour
{
    [SerializeField] private TMP_Text clanName;
    [SerializeField] private TMP_Text roleName;

    [SerializeField] private TMP_Text uPpointsTxt;

    [SerializeField] private Transform usersContent;
    [SerializeField] private Transform upgradesContent;

    [SerializeField] private Button manageButton;

    [SerializeField] private ClanUserUi userUiPrefab;
    [SerializeField] private UpgradeUi upgradeUiPrefab;

    public void ShowInClan(ParameterDictionary parameters)
    {
        this.gameObject.SetActive(true);

        //foreach(var el in parameters)
        //{
        //    UnityEngine.Debug.Log((Params)el.Key + " " + el.Value);
        //}

        var roletxt = (string)parameters[(byte)Params.RoleId];

        clanName.text = (string)parameters[(byte)Params.Name];
        roleName.text = roletxt;

        if(roletxt != "")
        {
            manageButton.gameObject.SetActive(true);
        }
        else
        {
            manageButton.gameObject.SetActive(false);
        }

        UnityEngine.Debug.Log("upPoints " + (int)parameters[(byte)Params.upPoints]);
        uPpointsTxt.text = ((int)parameters[(byte)Params.upPoints]).ToString();

        //UnityEngine.Debug.Log("clan Id: " + (int)parameters[(byte)Params.Id]);

        var parametrs = new Dictionary<byte, object>();

        parametrs.Add((byte)Params.Id, (int)parameters[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetClanUsers,
            parametrs,
            PhotonManager.Inst.sendOptions);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetClanUpgrades,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }

    public void ShowClanUsers(ParameterDictionary parameters)
    {
        var users = (Dictionary<int, object>)parameters[(byte)Params.users];

        UnityEngine.Debug.Log("Show clan users " + users.Count);

        UiHelper.ClearContainer(usersContent);
        foreach (var usr in users)
        {
            var usrData = (Dictionary<byte, object>)usr.Value;

            AddElement(usrData);
        }
    }

    public void AddElement(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(userUiPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, usersContent);

        newEleemntUi.Assign(data);
    }

    public void ShowClanUpgrades(ParameterDictionary parameters)
    {
        //UnityEngine.Debug.Log("Show clan upgrades Inclan");

        bool right = false;

        if (parameters.ContainsKey((byte)Params.rights))
        {
            right = true;
        }
        UnityEngine.Debug.Log("USER RIGHT " + right);

        var upgrades = (Dictionary<int, object>)parameters[(byte)Params.upgrades];

        UnityEngine.Debug.Log("upgrades count " + upgrades.Count);

        UiHelper.ClearContainer(upgradesContent);
        foreach (var el in upgrades)
        {
            var rowData = (Dictionary<byte, object>)el.Value;

            AddUpgradeElement(rowData, right);
        }
    }


    public void AddUpgradeElement(Dictionary<byte, object> data, bool right)
    {
        UnityEngine.Debug.Log(data[(byte)Params.Id]);
        UnityEngine.Debug.Log((string)data[(byte)Params.Description]);

        var newEleemntUi = Instantiate(upgradeUiPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, upgradesContent);

        newEleemntUi.Assign(data, right);
    }
}
