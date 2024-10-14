using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ClansCommon : MonoBehaviour
{
    public static ClansCommon instance;

    [SerializeField] private InClan inClanTab;
    [SerializeField] private NotInClan notInClanTab;
    [SerializeField] private NewClan newClanTab;
    [SerializeField] private InvitesToClan invToClanTab;

    void Start()
    {
        instance = this;

        UnityEngine.Debug.Log("commonclans start");
    }

    

    private void HideTabs()
    {
        inClanTab.gameObject.SetActive(false);
        notInClanTab.gameObject.SetActive(false);
        newClanTab.gameObject.SetActive(false);
    }

    public void GetUserClan()
    {
        //UnityEngine.Debug.Log("Get User Clan");
        HideTabs();

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetUserClan,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowUserClanInfo(ParameterDictionary parameters)
    {
        //UnityEngine.Debug.Log("Show user clan info " + parameters.Count);
        HideTabs();

        if (parameters.Count == 0)
        {
            ShowNotInClanTab();
        } else
        {
            ShowInClanTab(parameters);
        }
    }

    public void ShowInClanTab(ParameterDictionary parameters)
    {
        inClanTab.ShowInClan(parameters);       
    }

    public void ShowNotInClanTab()
    {
        notInClanTab.ShowNotInClanTab();
    }

    public void ShowClansForProposal(ParameterDictionary parameters)
    {
        notInClanTab.ShowClansForProposal(parameters);
    }

    public void ShowUserInvites(ParameterDictionary parameters)
    {
        notInClanTab.ShowUserInvites(parameters);
    }

    public void ShowClanPrice(ParameterDictionary parameters)
    {
        notInClanTab.ShowClanPrice(parameters);
    }

    public void ShowClanUsers(ParameterDictionary parameters)
    {
        inClanTab.ShowClanUsers(parameters);
    }

    public void GetInvitesTab()
    {
        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetInvitesTab,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowInvitesTab(ParameterDictionary parameters)
    {
        invToClanTab.ShowTab(parameters);
    }

    public void ShowClanUpgrades(ParameterDictionary parameters)
    {
        inClanTab.ShowClanUpgrades(parameters);
    }

}
