using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ui_AvailableRoom : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_RoomId;
    [SerializeField] private TextMeshProUGUI Text_RoomName;
    [SerializeField] private TextMeshProUGUI Text_RoomLeague;
    [SerializeField] private TextMeshProUGUI Text_RoomCost;
    [SerializeField] private TextMeshProUGUI Text_PlayerCount;
    [SerializeField] private TextMeshProUGUI Text_UseExtra;
    [SerializeField] private Button Button_Action;
    [SerializeField] private TextMeshProUGUI Text_ButtonAction;

    public int id { get; private set; }

    public void Assign(ParameterDictionary data)
    {
        //var roomData = (Dictionary<byte, object>)d.Value;

        var id = (int)data[(byte)Params.Id];
        this.id = id;
        Text_RoomId.text = $"{id}";

        var roomName = (string)data[(byte)Params.Name];
        Text_RoomName.text = $"{roomName}";

        var playerCount = (int)data[(byte)Params.Count];
        var maxPlayerCount = (int)data[(byte)Params.MaxCount];

        Text_PlayerCount.text = $"{playerCount}/{maxPlayerCount}";

        var leagueId = (LeagueId)data[(byte)Params.LeagueId];
        Text_RoomLeague.text = $"{Helper.LeagueIdToRusName(leagueId)}";

        var cost = (int)data[(byte)Params.Cost];
        Text_RoomCost.text = $"{cost}";

        var useExtras = (bool)data[(byte)Params.UseExtra];
        if (useExtras)
        {
            Text_UseExtra.text = $"<color=#1CD417>Да</color>";
        }
        else
        {
            Text_UseExtra.text = $"<color=#FF5151>Нет</color>";
        }

        SetAvailable();
    }

    private int maxPlayerCount;

    public void Assign(KeyValuePair<int, object> d)
    {
        var data = (Dictionary<byte, object>)d.Value;

        var id = (int)data[(byte)Params.Id]; 
        this.id = id;
        Text_RoomId.text = $"{id}";

        var roomName = (string)data[(byte)Params.Name];
        Text_RoomName.text = $"{roomName}";

        var playerCount = (int)data[(byte)Params.Count];
        var roomType = (RoomType)data[(byte)Params.MaxCount];

        switch (roomType)
        {
            case RoomType._8: maxPlayerCount = 8; break;
            case RoomType._12: maxPlayerCount = 12; break;
            case RoomType._16: maxPlayerCount = 16; break;
            case RoomType._20: maxPlayerCount = 20; break;
        }       

        Text_PlayerCount.text = $"{playerCount}/{maxPlayerCount}";

        var leagueId = (LeagueId)data[(byte)Params.LeagueId];
        Text_RoomLeague.text = $"{Helper.LeagueIdToRusName(leagueId)}";

        var cost = (CostId)data[(byte)Params.Cost];

        switch (cost)
        {
            case CostId._20: Text_RoomCost.text = $"20"; ; break;
            case CostId._100: Text_RoomCost.text = $"100"; ; break;
            case CostId._200: Text_RoomCost.text = $"200"; ; break;
            case CostId._5000: Text_RoomCost.text = $"5000"; ; break;
        }        

        var useExtras = (bool)data[(byte)Params.UseExtra];
        if (useExtras)
        {
            Text_UseExtra.text = $"<color=#1CD417>Да</color>";
        }
        else
        {
            Text_UseExtra.text = $"<color=#FF5151>Нет</color>"; 
        }

        

        SetAvailable();
    }   

    public void UpdatePlayersCount(ParameterDictionary data)
    {
        var playerCount = (int)data[(byte)Params.Count];

        Text_PlayerCount.text = $"{playerCount}/{maxPlayerCount}";
    }

    public void SetJoined()
    {
        Button_Action.image.color = Color.green;
        Text_ButtonAction.text = $"Отключиться";

        Button_Action.onClick.RemoveAllListeners();

        Button_Action.onClick.AddListener(() => LeaveLobby());
    }

  

    private void LeaveLobby()
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.Id, id);

        PhotonManager.Inst.peer.SendOperation((byte)Request.LeaveLobby, parameters, PhotonManager.Inst.sendOptions);

        Debug.Log($"leave Lobby {id}");
    }

    public void SetAvailable()
    {
        Text_ButtonAction.text = $"Подключиться";
        Button_Action.image.color = Color.white;

        Button_Action.onClick.RemoveAllListeners();

        Button_Action.onClick.AddListener(() => JoinLobby());
    }

    private void JoinLobby()
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.Id, id);

        PhotonManager.Inst.peer.SendOperation((byte)Request.JoinLobby, parameters, PhotonManager.Inst.sendOptions);

        Debug.Log($"join Lobby {id}");
    }  
}
