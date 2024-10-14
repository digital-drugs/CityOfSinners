using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvitesToClan : MonoBehaviour
{
    [SerializeField] private Transform usersContent;

    [SerializeField] private Transform invitesContent;
    [SerializeField] private Transform proposalsContent;

    [SerializeField] private FreeUserUi freeUserUiPrefab;

    [SerializeField] private InvitePropToClanUi inviteUiPrefab;
    public void ShowTab(ParameterDictionary parameters)
    {
        //UnityEngine.Debug.Log("Sjow invites to clan tab");

        this.gameObject.SetActive(true);
        usersContent.gameObject.SetActive(false);
        invitesContent.gameObject.SetActive(false);
        proposalsContent.gameObject.SetActive(false);

        var rights = (Dictionary<int, object>)parameters[(byte)Params.rights];

        if (rights.ContainsValue((int)ClanRights.InvitePlayer))
        {
            UnityEngine.Debug.Log("Have invite");

            var players = (Dictionary<int, object>)parameters[(byte)Params.Players];

            ShowFreePlayers(players);
        }

        if (rights.ContainsValue((int)ClanRights.SeeProposals))
        {
            var proposals = (Dictionary<int, object>)parameters[(byte)Params.invites];

            ShowProposals(proposals);
        }
    }

    private void ShowProposals(Dictionary<int, object> proposals)
    {
        invitesContent.gameObject.SetActive(true);
        proposalsContent.gameObject.SetActive(true);

        UiHelper.ClearContainer(invitesContent);
        UiHelper.ClearContainer(proposalsContent);
        

        foreach (var el in proposals)
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

    private void ShowFreePlayers(Dictionary<int, object> players)
    {
        UnityEngine.Debug.Log(players.Count);

        usersContent.gameObject.SetActive(true);

        UiHelper.ClearContainer(usersContent);

        foreach (var el in players)
        {
            var playerData = (Dictionary<byte, object>)el.Value;

            AddFreePlayerUi(playerData);
        }
    }

    public void AddFreePlayerUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(freeUserUiPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, usersContent);

        newEleemntUi.Assign(data);
    }
}
