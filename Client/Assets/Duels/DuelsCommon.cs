using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DuelsCommon : MonoBehaviour
{
    [SerializeField] private InDuels inDuelsTab;
    [SerializeField] private NotInDuel notInDuelTab;


    public static DuelsCommon inst;
    void Start()
    {
        inst = this;

        UnityEngine.Debug.Log("commonclans start");
    }
    /// <summary>
    /// Запросить данные для вкладки дуэлей
    /// </summary>
    public void RequestDuelsTab()
    {
        UnityEngine.Debug.Log("request duels tab");

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.RequestDuelsTab,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void HandleRequestDuelsTab(ParameterDictionary parameters)
    {
        HideAllTabs();

        UnityEngine.Debug.Log(parameters.Count);

        if(!parameters.ContainsKey((byte)Params.clanId))
        {
            ShowNotInDuelTab("Вы не состоите в клане.");
            return;
        }

        int dueldayId = (int)parameters[(byte)Params.Day];
        string start = (string)parameters[(byte)Params.Start];
        string end = (string)parameters[(byte)Params.End];

        if (dueldayId == 0)
        {
            ShowNotInDuelTab("Дуэли начнутся " + start);
            return;
        }

        if(!parameters.ContainsKey((byte)Params.clans))
        {
            ShowNotInDuelTab("Для вашего клана не нашлось соперника. Дуэль закончится " + end);
            return;
        }

        ShowDuelsTab(parameters);
    }

    public void ShowDuelsTab(ParameterDictionary parameters)
    {
        inDuelsTab.ShowTab(parameters);
    }

    public void BuildWheelPart(ParameterDictionary parameters)
    {
        inDuelsTab.BuildWheelPart(parameters);
    }

    public void ShowNotInDuelTab(string message)
    {
        notInDuelTab.ShowTab(message);
    }

    public void HideAllTabs()
    {
        notInDuelTab.gameObject.SetActive(false);
    }
}
