using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotInClan : MonoBehaviour
{
    [SerializeField] private Transform clansContent;

    [SerializeField] private Transform invitesContent;
    [SerializeField] private Transform proposalsContent;

    [SerializeField] private TMP_Text balanceText;

    [SerializeField] private Button createButton; 

    [SerializeField] private ClanUi clanUiPrefab;
    [SerializeField] private InviteUi inviteUiPrefab;

    public void ShowNotInClanTab()
    {
        this.gameObject.SetActive(true);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetClansForProposal,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetUserInvites,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetCoinsForClan,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowClansForProposal(ParameterDictionary parameters)
    {
        //UnityEngine.Debug.Log("Show clans for proposal");

        UiHelper.ClearContainer(clansContent);
        var clans = (Dictionary<int, object>)parameters[(byte)Params.clans];

        foreach(var el in clans)
        {
            var clanData = (Dictionary<byte, object>)el.Value;

            AddElementUi(clanData);
        }
    }

    private void AddElementUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(clanUiPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, clansContent);
        
        newEleemntUi.Assign(data);
    }

    public void ShowUserInvites(ParameterDictionary parameters)
    {
        //UnityEngine.Debug.Log("Show invites");

        UiHelper.ClearContainer(invitesContent);
        UiHelper.ClearContainer(proposalsContent);
        var invites = (Dictionary<int, object>)parameters[(byte)Params.invites];

        foreach (var el in invites)
        {
            var data = (Dictionary<byte, object>)el.Value;

            AddInvite(data);
        }
    }

    public void AddInvite(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(inviteUiPrefab);
        
        if ((string)data[(byte)Params.Invite] == InviteType.fromOwner.ToString())
        {
            UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, invitesContent);
        }

        if ((string)data[(byte)Params.Invite] == InviteType.fromUser.ToString())
        {
            UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, proposalsContent);
        }

        newEleemntUi.Assign(data);
    }

    public void ShowClanPrice(ParameterDictionary parameters)
    {
        int coins = (int)parameters[(byte)Params.Coins];
        int cost = (int)parameters[(byte)Params.Cost];

        UnityEngine.Debug.Log("Show price and coins " + coins );

        balanceText.text = cost.ToString() + " / " + coins.ToString();

        if(coins >= cost)
        {
            createButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            createButton.GetComponent<Button>().interactable = false;
        }   
    }
}
