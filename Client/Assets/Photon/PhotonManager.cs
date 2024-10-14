using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Threading;
using TMPro;

public class PhotonManager : MonoBehaviour, IPhotonPeerListener
{
    public static PhotonManager Inst;

    public PhotonPeer peer { get; private set; }

    [Header("Host")]
    public string Photon_Host;

    public string dev_Host;
    public string prod_Host;
    [SerializeField] private bool useProduction;
    //public string Test_Host;
    [Header("Port")]
    public string TCP_Port;
    public string UDP_Port;
    public string WEB_Port;
    public string WSS_Port;
    [Header("Mode")]
    public bool Use_Tcp;
    public bool Use_Udp;
    public bool Use_Web;
    public bool Use_Wss = true;

    [Header("Settings")]
    public string Photon_App_Name;


    public float ReConnectTime = 1f;

    public Image connectionIndicator;

    private void Start()
    {
        Setup();
    }

    //при старте приложения
    public string Setup()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
            return "";
        }

        DontDestroyOnLoad(gameObject);

        sendOptions = new SendOptions();

        //AsyncUpdatePhotonService();

        Connect();

        StartCoroutine(CheckServerAnswer());

        return "Manager Photon OK";
    }



    #region CONNECT_TO_SERVER 

    /// <summary>
    /// попытка одключения
    /// </summary>
    /// <param name="Next_Connect_Time"></param>
    /// <returns></returns>
    IEnumerator Try_Connect(float Next_Connect_Time)
    {
        Debug.Log("Try_Connect");
        //включаем индикатор сервера
        yield return new WaitForSeconds(Next_Connect_Time);
        Connect();
    }

    public bool CustomIni;
    public bool CustomToken;

    public void Connect()
    {
        Debug.Log("start Connect");

        if (peer == null)
        {
            if (Use_Tcp) peer = new PhotonPeer(this, ConnectionProtocol.Tcp);

            if (Use_Udp) peer = new PhotonPeer(this, ConnectionProtocol.Udp);

            if (Use_Web)
            {
                peer = new PhotonPeer(this, ConnectionProtocol.WebSocket);
                peer.SocketImplementationConfig.Add(ConnectionProtocol.WebSocket, typeof(SocketWebTcp));
            }

            if (Use_Wss)
            {
                peer = new PhotonPeer(this, ConnectionProtocol.WebSocketSecure);
                peer.SocketImplementationConfig.Add(ConnectionProtocol.WebSocketSecure, typeof(SocketWebTcp));
            }

        }

        var targetHost = "";

#if !UNITY_EDITOR

        targetHost = prod_Host;

#else
        if (useProduction)
        {
            targetHost = prod_Host;
        }
        else
        {
            targetHost = dev_Host;
        }

#endif

        if (Use_Tcp) Photon_Host = targetHost + ":" + TCP_Port;

        if (Use_Udp) Photon_Host = targetHost + ":" + UDP_Port;

        if (Use_Web) Photon_Host = $"ws://{targetHost}:{WEB_Port}";

        if (Use_Wss) Photon_Host = $"wss://{targetHost}:{WSS_Port}";

        if (peer != null)
        {
            peer.Connect(Photon_Host, Photon_App_Name);

            Debug.Log($"Photon_Host {Photon_Host} => ");
        }
    }
    #endregion

    private void Update()
    {
        UpdatePhotonService();
    }

    //async Task AsyncUpdatePhotonService()
    //{
    //    while (true)
    //    {
    //        UpdatePhotonService();

    //        await Task.Delay(20); 
    //    }
    //}

    private void UpdatePhotonService()
    {
        if (peer != null)
        {
            peer.Service();
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log($"dispose PhotonService");
        //UpdatePhotonService().Dispose();

        Debug.Log($"disconnect peer");
        //при закрытии клиент рвем соед с сервером
        if (peer != null)
        {
            peer.Disconnect();
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        //нужно для IPhotonPeerListener
    }

    #region Статус соединения с сервером

    [SerializeField] private TextMeshProUGUI serverState;
    [SerializeField] private TextMeshProUGUI serverAnswerTime;
    [SerializeField] private Image serverAnswerImage;

    private float lastAnswerTime = 0;
    private bool signalLost = false;
    IEnumerator CheckServerAnswer()
    {
        while (true)
        {
            var answerOffset = Time.time - lastAnswerTime;

            if (answerOffset > 2 && signalLost == false)
            {
                signalLost = true;

                ServerLogger.ins.AddLog($"id [{GameManager.instance.userId}] => signal FAIL => {DateTime.Now.TimeOfDay} => {answerOffset}");
                serverAnswerImage.color = Color.red;
            }

            if (answerOffset < 1 && signalLost)
            {
                signalLost = false;

                ServerLogger.ins.AddLog($"id [{GameManager.instance.userId}] => signal OK => {DateTime.Now.TimeOfDay} => {answerOffset}");

                serverAnswerImage.color = Color.green;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        serverState.text = $"state => {statusCode}";

        ServerLogger.ins.AddLog($"id [{GameManager.instance.userId}] => {DateTime.Now.TimeOfDay} => {statusCode}");

        switch (statusCode)
        {
            //при успешном подключении к серверу
            case StatusCode.Connect:
                {
                    AuthorizationUI.ShowAuthorization();
#if UNITY_EDITOR

#endif
                    connectionIndicator.color = Color.green;
                }
                break;

            case StatusCode.Disconnect:
                {
                    connectionIndicator.color = Color.red;

                    StartCoroutine(Try_Connect(1f));
                }
                break;

            case StatusCode.TimeoutDisconnect:
                {
                    AuthorizationUI.ShowAuthorization();
                }
                break;

            case StatusCode.DisconnectByServerUserLimit:
                AuthorizationUI.ShowAuthorization();
                break;

            case StatusCode.DisconnectByServerLogic:
                {
                    AuthorizationUI.ShowAuthorization();
                }
                break;

            case StatusCode.EncryptionEstablished:
                AuthorizationUI.ShowAuthorization();
                break;

            case StatusCode.EncryptionFailedToEstablish:
                AuthorizationUI.ShowAuthorization();
                break;

            default:
                {
                    AuthorizationUI.ShowAuthorization();
                }
                break;
        }
    }


    #endregion

    #region Сообщения от сервера

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        //Debug.Log($"get server resp");
        //Debug.Log(operationResponse.OperationCode);

        switch (operationResponse.OperationCode)
        {
            case (byte)Request.Login:
                {
                    //try
                    //{
                    var returnCode = (ReturnCode)operationResponse.ReturnCode;

                    switch (returnCode)
                    {
                        case ReturnCode.Succes:
                            {
                                AuthorizationUI.HideAuthorization();
                            }
                            break;
                        case ReturnCode.Fail:
                            {
                                AuthorizationUI.RequestName();
                            }
                            break;
                    }
                    GameManager.instance.SetUserId(operationResponse.Parameters);
                    StartScreenUi.instance.LoadStartScreen(operationResponse.Parameters);
                    //}
                    //catch(Exception e)
                    //{
                    //    var message = $"<color=#D45B5B><b>{e.Message}</b></color> \n\n {e.StackTrace} \n {e.InnerException}";

                    //    ErrorUi.instance.ShowError(message);
                    //}
                }
                break;

            case (byte)Request.PingFromServer:
                {
                    lastAnswerTime = Time.time;
                }
                break;

            case (byte)Request.LockSlot:
                {
                    AuctionUi.instance.LockSlot(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetLobbyList:
                {
                    Ui_Lobby.ins.ShowLobbyList(operationResponse.Parameters);
                }
                break;

            case (byte)Request.JoinLobby:
                {
                    Ui_Lobby.ins.JoinLobby(operationResponse.Parameters);
                }
                break;

            case (byte)Request.StartRoomBuilder:
                {
                    StartScreenUi.instance.ShowRoomBuilder();
                }
                break;

            case (byte)Request.AddLobbyPlayer:
                {
                    StartScreenUi.instance.roomBuilderUi.AddWaitPlayer(operationResponse.Parameters);
                }
                break;

            case (byte)Request.RemoveLobbyPlayer:
                {
                    StartScreenUi.instance.roomBuilderUi.RemoveWaitPlayer(operationResponse.Parameters); 
                }
                break;

            case (byte)Request.CloseLobby:
                {
                    StartScreenUi.instance.HideLobby();
                }
                break;


            case (byte)Request.Buy:
                {
                    //ServerData = operationResponse.Parameters;

                    //var confirmURL = (string)ServerData[(byte)Params.ConfirmURL];

                    //Debug.Log($"confirmURL => {confirmURL}");

                    //UMoneyPayment.OpenConfirmURL(confirmURL);
                }
                break;

            case (byte)Request.BuySecceeded:
                {
                    //ServerData = operationResponse.Parameters;

                    //var buyDescription = (string)ServerData[(byte)Params.Description];

                    //UiLog.AddLog(buyDescription);
                }
                break;

            case (byte)Request.ChangeUserName:
                {
                    var operationCode = (ReturnCode)operationResponse.ReturnCode;

                    var userName = (string)operationResponse.Parameters[(byte)Params.UserName];

                    Debug.Log($"set name result => {operationCode} name => ({userName})");

                    switch (operationCode)
                    {
                        case ReturnCode.Succes:
                            {
                                AuthorizationUI.HideRequestName();
                                AuthorizationUI.HideAuthorization();
                                GameManager.instance.SetUserName(operationResponse.Parameters);
                            }
                            break;
                        case ReturnCode.Fail: { AuthorizationUI.ShowNameError("bad name"); } break;
                    }


                }
                break;



            case (byte)Request.RoomPersonalSystemMessage:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomPersonalSystemMessage(operationResponse.Parameters);
                    //GameRoomUi.instance.systemRoomChatUi.RoomPublicSystemMessage(operationResponse.Parameters);
                }
                break;

            case (byte)Request.ShareMessage:
                {
                    GameRoomUi.instance.systemRoomChatUi.ShareSystemMessage(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadSkills:
                {
                    SkillAdmin.ins.ShowSkills(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadExtras:
                {
                    AdminExtra.instance.ShowExtras(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadGifts:
                {
                    AdminGifts.instance.ShowGifts(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadStocks:
                {
                    AdminStocks.instance.ShowStocks(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetStocksList:
                {
                    AdminStocks.instance.ShowStocksList(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetUserClan:
                {
                    ClansCommon.instance.ShowUserClanInfo(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetClanUpgrades:
                {
                    ClansCommon.instance.ShowClanUpgrades(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetClanUsers:
                {
                    ClansCommon.instance.ShowClanUsers(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetInvitesTab:
                {
                    ClansCommon.instance.ShowInvitesTab(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetCoinsForClan:
                {
                    ClansCommon.instance.ShowClanPrice(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetClansForProposal:
                {
                    ClansCommon.instance.ShowClansForProposal(operationResponse.Parameters);
                }
                break;

            case (byte)Request.RequestDuelsTab:
                {
                    DuelsCommon.inst.HandleRequestDuelsTab(operationResponse.Parameters);
                }
                break;

            case (byte)Request.RotateWheel:
                {
                    DuelsCommon.inst.BuildWheelPart(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetUserInvites:
                {
                    ClansCommon.instance.ShowUserInvites(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadStock:
                {
                    AdminStocks.instance.ShowStock(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadSet:
                {
                    AdminStocks.instance.ShowSet(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadItem:
                {
                    AdminStocks.instance.ShowItem(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LoadClans:
                {
                    Clans.instance.ShowClans(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LevelUpSkill:
                {
                    SkillScreenUi.instance.UpdateSkillLevel(operationResponse.Parameters);
                }
                break;

            case (byte)Request.BuyExtra:
                {
                    ExtraScreenUi.instance.UpdateExtraCount(operationResponse.Parameters);
                }
                break;

            case (byte)Request.CreateGameSlots:
                {
                    GameRoomUi.instance.CreateExtraSlots(operationResponse.Parameters);
                }
                break;

            case (byte)Request.ExtraGameCount:
                {
                    GameRoomUi.instance.UpdateExtraGameCount(operationResponse.Parameters);
                }
                break;

            case (byte)Request.EndGameResult:
                {
                    GameRoomUi.instance.ShowEndGameResult(operationResponse.Parameters);
                }
                break;

            case (byte)Request.ErrorMessage:
                {
                    ErrorUi.instance.ShowError(operationResponse.Parameters);
                }
                break;

            //case (byte)Request.InQueue:
            //    {
            //        StartScreenUi.instance.InQueue(operationResponse.Parameters);
            //    }
            //    break;

            case (byte)Request.SetPlayerRole:
                {
                    GameRoomUi.instance.SetPlayerRole(operationResponse.Parameters);
                }
                break;


            case (byte)Request.PlayerStatus:
                {
                    GameManager.instance.SetPlayerStatus(operationResponse.Parameters);
                }
                break;

            case (byte)Request.AddExtraEffectToPlayer:
                {
                    GameRoomUi.instance.AddExtraEffectToPlayer(operationResponse.Parameters);
                }
                break;

            case (byte)Request.RemoveExtraEffectFromPlayer:
                {
                    GameRoomUi.instance.RemoveExtraEffectFromPlayer(operationResponse.Parameters);
                }
                break;

            case (byte)Request.RequestExitGameRoom:
                {
                    GameRoomUi.instance.RequestExitGameRoom(operationResponse.Parameters);
                }
                break;

            case (byte)Request.EnableVote:
                {
                    GameRoomUi.instance.EnableVote(operationResponse.Parameters);
                }
                break;
            case (byte)Request.DisableVote:
                {
                    GameRoomUi.instance.DisableVote();
                }
                break;

            case (byte)Request.EnableVisit:
                {
                    GameRoomUi.instance.EnableVisit(operationResponse.Parameters);
                }
                break;
            case (byte)Request.DisableVisit:
                {
                    GameRoomUi.instance.DisableVisit();
                }
                break;

            case (byte)Request.UnlockRole_PlayerToPlayer:
                {
                    GameRoomUi.instance.UnlockRole_PlayerToPlayer(operationResponse.Parameters);
                }
                break;
            case (byte)Request.UnlockRole_GroupToPlayer:
                {
                    GameRoomUi.instance.UnlockRole_GroupToPlayer(operationResponse.Parameters);
                }
                break;

            case (byte)Request.PrivateChatMessage:
                {
                    GameRoomUi.instance.roomChatUi.AddMessageFromServer(operationResponse.Parameters);
                }
                break;

            case (byte)Request.JoinChat:
                {
                    GameRoomUi.instance.roomChatUi.JoinChat(operationResponse.Parameters);
                }
                break;

            case (byte)Request.LeaveChat:
                {
                    GameRoomUi.instance.roomChatUi.LeaveChat(operationResponse.Parameters);
                }
                break;

            case (byte)Request.SystemMessage_Extra:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Extra(operationResponse.Parameters);
                }
                break;

            case (byte)Request.SystemMessage_Skill:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Skill(operationResponse.Parameters);
                }
                break;

            case (byte)Request.SystemMessage_Role:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Role(operationResponse.Parameters);


                }
                break;


            case (byte)Request.ChangePlayerRole:
                {
                    GameRoomUi.instance.ChangePersonalPlayerRole(operationResponse.Parameters);
                }
                break;
            case (byte)Request.GetReitings:
                {
                    //GameRoomUi.instance.roomChatUi.AddMessageFromServer(operationResponse.Parameters);
                    UnityEngine.Debug.Log("GET REITINGS");
                    Tops.inst.BuildButtons(operationResponse.Parameters);
                }
                break;

            case (byte)Request.GetReiting:
                {
                    Tops.inst.BuildReiting(operationResponse.Parameters);
                }
                break;
        }
    }

    #endregion

    #region ивенты от сервера

    //обработка событий с сервера
    public void OnEvent(EventData eventData)
    {
        var ServerData = eventData.Parameters;

        switch (eventData.Code)
        {
            case (byte)Events.GeneralChatMessage:
                {
                    GeneralChat.instance.AddMessageFromServer(eventData.Parameters);
                }
                break;

            case (byte)Events.StartAuction:
                {
                    AuctionUi.instance.StartAuction(eventData.Parameters);
                }
                break;

            case (byte)Events.AuctionTimer:
                {
                    AuctionUi.instance.SetAuctionTimer(eventData.Parameters);
                }
                break;

            case (byte)Events.AuctionBet:
                {
                    AuctionUi.instance.UpdateAuctionBet(eventData.Parameters);
                }
                break;

            case (byte)Events.AuctionBuy:
                {
                    AuctionUi.instance.UpdateAuctionBuy(eventData.Parameters);
                }
                break;

            case (byte)Events.StartGame:
                {
                    StartScreenUi.instance.HideWiStartScreen();
                    StartScreenUi.instance.roomBuilderUi.HideRoomBuilder();
                    AuctionUi.instance.EndAuction();
                    GameRoomUi.instance.StartGame(eventData.Parameters);
                }
                break;

            case (byte)Events.GameTimer:
                {
                    GameRoomUi.instance.SetGameTimer(eventData.Parameters);
                }
                break;

            case (byte)Events.ChangeGamePhase:
                {
                    //Debug.Log($"set game phase");
                    GameRoomUi.instance.ChangeGamePhase(eventData.Parameters);
                }
                break;

            case (byte)Events.ChangeGoodTeam:
                {
                    GameRoomUi.instance.ChangeGoodTeamCompound(eventData.Parameters);
                }
                break;

            case (byte)Events.ChangeBadTeam:
                {
                    GameRoomUi.instance.ChangeBadTeamCompound(eventData.Parameters);
                }
                break;

            case (byte)Events.TeamCompound:
                {
                    GameRoomUi.instance.TeamCompound(eventData.Parameters);
                }
                break;

            case (byte)Events.PlayerToJail:
                {
                    GameRoomUi.instance.PlayerToJail(eventData.Parameters);
                }
                break;

            case (byte)Events.PlayerToMorgue:
                {
                    GameRoomUi.instance.PlayerToMorgue(eventData.Parameters);
                }
                break;

            case (byte)Events.ResurectPlayer:
                {
                    GameRoomUi.instance.ResurectPlayer(eventData.Parameters);
                }
                break;

            case (byte)Events.RoomChatMessage:
                {
                    GameRoomUi.instance.roomChatUi.AddMessageFromServer(eventData.Parameters);
                }
                break;


            case (byte)Events.RoomPublicSystemMessage:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomPublicSystemMessage(eventData.Parameters);
                }
                break;

            //case (byte)Events.CreateChat:
            //    {
            //        GameRoomUi.instance.roomChatUi.CreateChat(eventData.Parameters);
            //    }
            //    break;

            case (byte)Events.StartJudging:
                {
                    GameRoomUi.instance.StartJudging(eventData.Parameters);
                }
                break;

            case (byte)Events.SeeJudging:
                {
                    GameRoomUi.instance.SeeJudging(eventData.Parameters);
                }
                break;
            case (byte)Events.EndJudging:
                {
                    GameRoomUi.instance.EndJudging(eventData.Parameters);
                }
                break;

            case (byte)Events.UnlockRole_PlayerToRoom:
                {
                    GameRoomUi.instance.UnlockRole_PlayerToRoom(eventData.Parameters);
                }
                break;

            case (byte)Events.UnlockRole_PlayerToGroup:
                {
                    GameRoomUi.instance.UnlockRole_PlayerToGroup(eventData.Parameters);
                }
                break;

            case (byte)Events.RoomSystemMessage_Skill:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Skill(eventData.Parameters);
                }
                break;

            case (byte)Events.RoomSystemMessage_Extra:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Extra(eventData.Parameters);
                }
                break;

            case (byte)Events.Role_PublicMessage:
                {
                    GameRoomUi.instance.systemRoomChatUi.RoomSystemMessage_Role(eventData.Parameters);
                }
                break;

            case (byte)Events.SetPlayerVoteCount:
                {
                    GameRoomUi.instance.SetPlayerVoteCount(eventData.Parameters);
                }
                break;


            case (byte)Events.ChangePlayerRole:
                {
                    GameRoomUi.instance.ChangePlayerRole(eventData.Parameters);
                }
                break;

            case (byte)Events.ChangeTeam_Werewolf:
                {
                    GameRoomUi.instance.ChangeTeam_Werewolf(eventData.Parameters);
                }
                break;

            case (byte)Events.AddPlayerToLobby:
                {
                    Ui_Lobby.ins.AddPlayerToLobby(eventData.Parameters);
                }
                break;

            case (byte)Events.RemovePlayerFromLobby:
                {
                    Ui_Lobby.ins.RemovePlayerFromLobby(eventData.Parameters);
                }
                break;

            case (byte)Events.LobbyInfo_AddLobby:
                {
                    Ui_Lobby.ins.LobbyInfo_AddLobby(eventData.Parameters);
                }
                break;
            case (byte)Events.LobbyInfo_RemoveLobby:
                {
                    Ui_Lobby.ins.LobbyInfo_RemoveLobby(eventData.Parameters);
                }
                break;
            case (byte)Events.LobbyInfo_PlayersCount:
                {
                    Ui_Lobby.ins.LobbyInfo_PlayersCount(eventData.Parameters);
                }
                break;

            case (byte)Events.CloseLobby:
                {
                    Ui_Lobby.ins.CloseLobby();
                }
                break;

            default:
                Debug.Log($"unknown event: {eventData.Code}");
                break;
        }
    }
    #endregion

    public SendOptions sendOptions { get; private set; }

    /// <summary>
    /// запрос на авторизацию
    /// </summary>
    /// <param name="userLogin"></param>
    /// <param name="userPassword"></param>
    /// <param name="userType"></param>
    public void RequestLogin(string userId)
    {
        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.UserLogin, userId);

        peer.SendOperation((byte)Request.Login, parameters, sendOptions);
    }

    public void RequestGIF()
    {
        var parameters = new Dictionary<byte, object>();

        peer.SendOperation((byte)Request.Gif, parameters, sendOptions);
    }

}