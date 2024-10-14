using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ui_LobbyConstructor : MonoBehaviour
{
    public static Ui_LobbyConstructor ins;

    private void Awake()
    {
        ins = this;

        //кол-во игроков
        {
            Drop_PlayerCount.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var element in Enum.GetValues(typeof(RoomType)))
            {
                var elementString = Helper.RoomTypeToRus_Name((RoomType)element);

                options.Add(new TMP_Dropdown.OptionData(elementString));
            }

            Drop_PlayerCount.AddOptions(options);
        }

        //лига
        {
            Drop_League.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var element in Enum.GetValues(typeof(LeagueId)))
            {
                var elementString = Helper.LeagueIdToRusName((LeagueId)element);

                options.Add(new TMP_Dropdown.OptionData(elementString));
            }

            Drop_League.AddOptions(options);
        }

        //цена
        {
            Drop_Cost.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var element in Enum.GetValues(typeof(CostId)))
            {
                var elementString = Helper.CostIdToRusName((CostId)element);

                options.Add(new TMP_Dropdown.OptionData(elementString));
            }

            Drop_Cost.AddOptions(options);
        }
    }

    [SerializeField] private GameObject GameObject_WindowLobbyConstructor;
    public void Button_ShowLobbyConstructor()
    {
        ShowLobbyConstructor(true);
    }

    public void ShowLobbyConstructor(bool show)
    {
        GameObject_WindowLobbyConstructor.SetActive(show);
    }

    public void Button_CreateLobby()
    {
        CreateLobby();
    }

    [SerializeField] private TMP_Dropdown Drop_PlayerCount;
    [SerializeField] private TMP_Dropdown Drop_League;
    [SerializeField] private TMP_Dropdown Drop_Cost;
    [SerializeField] private Toggle Toggle_UseExtras;
    private void CreateLobby()
    {
        var parameters = new Dictionary<byte, object>();

        var roomType = (RoomType)Drop_PlayerCount.value;
        parameters.Add((byte)Params.RoomType, roomType);

        var league = (LeagueId)Drop_League.value;
        parameters.Add((byte)Params.LeagueId, league);

        //Debug.Log($"league => {(LeagueId)league}");

        var cost = (CostId)Drop_Cost.value;
        parameters.Add((byte)Params.Cost, cost);

        parameters.Add((byte)Params.UseExtra, Toggle_UseExtras.isOn);

        PhotonManager.Inst.peer.SendOperation((byte)Request.CreateLobby, parameters, PhotonManager.Inst.sendOptions);
    }
}

