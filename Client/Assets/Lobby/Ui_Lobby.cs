using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ui_Lobby : MonoBehaviour
{
    public static Ui_Lobby ins;
    private void Awake()
    {
        ins = this;
    }

    [SerializeField] private Ui_AvailableRoom ui_AvailableRoom;
    [SerializeField] private Transform availableRoomsContainer;
    [SerializeField] private GameObject Window_AvailableRooms;
    Dictionary<int, Ui_AvailableRoom> lobbyUis = new Dictionary<int, Ui_AvailableRoom>();
    public void ShowLobbyList(ParameterDictionary parameters)
    {
        lobbyUis.Clear();

        UiHelper.ClearContainer(availableRoomsContainer);

        var data = (Dictionary<int, object>)parameters[(byte)Params.Data];

        foreach (var d in data)
        {
            var newRoomUi = Instantiate(ui_AvailableRoom, availableRoomsContainer);
            newRoomUi.Assign(d);

            lobbyUis.Add(d.Key, newRoomUi);
        }

        Window_AvailableRooms.SetActive(true);
    }

    public void HideLobbyList()
    {
        var parameters = new Dictionary<byte, object>();

        PhotonManager.Inst.peer.SendOperation((byte)Request.UnSubscribeLobby, parameters, PhotonManager.Inst.sendOptions);

        Window_AvailableRooms.SetActive(false);
    }

    public void LobbyInfo_AddLobby(ParameterDictionary data)
    {
        var id = (int)data[(byte)Params.Id];

        var newRoomUi = Instantiate(ui_AvailableRoom, availableRoomsContainer);
        newRoomUi.Assign(data);

        lobbyUis.Add(id, newRoomUi);
    }

    public void LobbyInfo_RemoveLobby(ParameterDictionary data)
    {
        var id = (int)data[(byte)Params.Id];

        if (!lobbyUis.ContainsKey(id)) return;

        var lobbyUi = lobbyUis[id];

        lobbyUis.Remove(id);

        UiHelper.MoveUiObjectToTrash(lobbyUi.gameObject);
    }

    public void LobbyInfo_PlayersCount(ParameterDictionary data)
    {
        var id = (int)data[(byte)Params.Id];

        if (!lobbyUis.ContainsKey(id)) return;

        var lobbyUi = lobbyUis[id];

        lobbyUi.UpdatePlayersCount(data);
    }    

    public void Button_RequestLeaveLobby()
    {
        var parameters = new Dictionary<byte, object>();
        PhotonManager.Inst.peer.SendOperation((byte)Request.LeaveLobby, parameters, PhotonManager.Inst.sendOptions);
       
        window.SetActive(false);

        StartScreenUi.instance.roomBuilderUi.HideRoomBuilder();
    }

    [SerializeField] private GameObject window;
    public void JoinLobby(ParameterDictionary parameters)
    {
        ui_lobbyPlayers = new Dictionary<long, Ui_LobbyPlayer>();

        UiHelper.ClearContainer(Container_ui_LobbyPlayer);

        foreach (var p in parameters)
        {
            var playerData = (Dictionary<byte, object>)p.Value;

            AddPlayerToLobby(playerData);
        }

        window.SetActive(true);

        Ui_LobbyConstructor.ins.ShowLobbyConstructor(false);
    }

    [SerializeField] private Ui_LobbyPlayer ui_LobbyPlayer;
    [SerializeField] private Transform Container_ui_LobbyPlayer;
    private Dictionary<long, Ui_LobbyPlayer> ui_lobbyPlayers = new Dictionary<long, Ui_LobbyPlayer>();
    public void AddPlayerToLobby(ParameterDictionary data)
    {
        var playerId = (long)data[(byte)Params.Id];
        var playerName = (string)data[(byte)Params.Name];

        var newLobbyPlayer = Instantiate(ui_LobbyPlayer, Container_ui_LobbyPlayer);

        ui_lobbyPlayers.Add(playerId, newLobbyPlayer);

        newLobbyPlayer.Assign(playerName);
    }

    private void AddPlayerToLobby(Dictionary<byte,object> data)
    {
        var playerId = (long)data[(byte)Params.Id];
        var playerName = (string)data[(byte)Params.Name];

        var newLobbyPlayer = Instantiate(ui_LobbyPlayer, Container_ui_LobbyPlayer);
        
        ui_lobbyPlayers.Add(playerId, newLobbyPlayer);

        newLobbyPlayer.Assign(playerName);
    }

    public void RemovePlayerFromLobby(ParameterDictionary data)
    {
        var id = (long)data[(byte)Params.Id];

        if (ui_lobbyPlayers.ContainsKey(id))
        {
            var ui = ui_lobbyPlayers[id];

            UiHelper.MoveUiObjectToTrash(ui.gameObject);

            ui_lobbyPlayers.Remove(id);
        }
    }

    public void CloseLobby()
    {
        StartScreenUi.instance.roomBuilderUi.HideRoomBuilder();

        window.SetActive(false);
    }

    public void ForceGameStart()
    {
        var parameters = new Dictionary<byte, object>();

        PhotonManager.Inst.peer.SendOperation((byte)Request.ForceStartGameFromLobby, parameters, PhotonManager.Inst.sendOptions);
    }
}
