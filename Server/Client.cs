using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using System.Collections.Generic;
using PhotonHostRuntimeInterfaces;
using Share;
using static System.Net.Mime.MediaTypeNames;
//using Newtonsoft.Json;
using System;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using MySqlX.XDevAPI.Common;
using ExitGames.Concurrency.Fibers;

namespace Mafia_Server
{
    public class Client : Peer
    {
        //айди юзера в базе данных
        public long userDbId = -1;
        public string userName = "";

        public Player player;

        //параметры отправки сообщений юзеру
        //public SendParameters sendParameters { get; private set; }

        PoolFiber timers;
        IDisposable pingToClient;
        public Client(InitRequest initRequest) : base(initRequest)
        {
            //sendParameters = new SendParameters();

            timers = new PoolFiber();
            timers.Start();

         

            //forced error
            //Logger.Log.Debug($"{player.playerName}");
        }

        private void PingToClient()
        {
            OperationResponse resp = new OperationResponse((byte)Request.PingFromServer);
            resp.Parameters = new Dictionary<byte, object>();
            SendOperationResponse(resp, Options.sendParameters);
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            //удаляем лобби
            //если при дисконнекте у игрока есть лобби и он является его владельцем
            if (player.GetLobby() != null && player.GetLobby().owner == player)
            {
                ManagerRooms.instance.DisposeLobby(player.GetLobby());
            }

            UnSubscribeLobby();

            ManagerUser.instance.RemoveOnlineUser(userDbId);

            timers.Stop();
            timers.Dispose();
            if (pingToClient != null) pingToClient.Dispose();

            //if(player!= null && player.room!=null)
            //{
            //    player.room.Dispose();               
            //}

            if (player != null && player.GetRoom() != null)
            {
                if(player.GetRoom().roomState == RoomState.WaitPlayers)
                {
                    CloseLobby();
                }
            }

           
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Info("client request code" + operationRequest.OperationCode);

            try
            {
                switch (operationRequest.OperationCode)
                {
                    case (byte)Request.Login: OnRequestLogin(operationRequest, sendParameters); break;

                    case (byte)Request.Buy: OnRequestBuy(operationRequest, sendParameters); break;

                    case (byte)Request.ChangeUserName: OnRequestChangeName(operationRequest, sendParameters); break;

                    case (byte)Request.SendChat: OnRequestSendChat(operationRequest, sendParameters); break;
                    case (byte)Request.ShareMessage: OnRequestShareMessage(operationRequest, sendParameters); break;

                    case (byte)Request.CreateGame: OnRequestCreateGame(operationRequest, sendParameters); break;
                    case (byte)Request.CreateLobby: OnRequestCreateLobby(operationRequest, sendParameters); break;
                    case (byte)Request.ForceStartGameFromLobby: OnRequestForceStartGameFromLobby(operationRequest, sendParameters); break;


                    case (byte)Request.JoinLobby: OnRequestJoinLobby(operationRequest, sendParameters); break;
                    case (byte)Request.LeaveLobby: OnRequestLeaveLobby(operationRequest, sendParameters); break;
                    //case (byte)Request.RemoveLobby: OnRequestRemoveLobby(operationRequest, sendParameters); break;


                    case (byte)Request.StartGameFromLobby: OnRequestStartGameFromLobby(operationRequest, sendParameters); break;
                    case (byte)Request.StartGame: OnRequestStartGame(operationRequest, sendParameters); break;
                    case (byte)Request.CloseLobby:OnRequestCloseLobby(operationRequest, sendParameters); break;

                    case (byte)Request.RequestExitGameRoom: OnRequestExitGameRoom(operationRequest, sendParameters); break;
                    case (byte)Request.ConfirmExitGameRoom: OnRequestConfirmExitGameRoom(operationRequest, sendParameters); break;

                    case (byte)Request.Bet: OnRequestBet(operationRequest, sendParameters); break;

                    case (byte)Request.SelectPlayer: OnRequestSelectPlayer(operationRequest, sendParameters); break;
                    case (byte)Request.Judging: OnRequestJudging(operationRequest, sendParameters); break;

                    case (byte)Request.SaveSkill: OnRequestSaveSkill(operationRequest, sendParameters); break;
                    case (byte)Request.LoadSkills: OnRequestLoadSkills(operationRequest, sendParameters); break;
                    case (byte)Request.LevelUpSkill: OnRequestLevelUpSkill(operationRequest, sendParameters); break;

                    case (byte)Request.SaveExtra: OnRequestSaveExtra(operationRequest, sendParameters); break;
                    case (byte)Request.LoadExtras: OnRequestLoadExtras(operationRequest, sendParameters); break;

                    case (byte)Request.BuyExtra: OnRequestBuyExtra(operationRequest, sendParameters); break;

                    case (byte)Request.SaveUserSlot: OnRequestSaveUserSlot(operationRequest, sendParameters); break;
                    case (byte)Request.ClearUserSlot: OnRequestClearUserSlot(operationRequest, sendParameters); break;

                    case (byte)Request.UseExtra: OnRequestUseExtra(operationRequest, sendParameters); break;

                    case (byte)Request.SaveAchieve: OnRequestSaveAchieve(operationRequest, sendParameters); break;

                    case (byte)Request.LoadGifts: OnRequestLoadGifts(operationRequest, sendParameters); break;

                    case (byte)Request.SaveGift: OnRequestSaveGift(operationRequest, sendParameters); break;

                    case (byte)Request.LoadClans: OnRequestLoadClans(operationRequest, sendParameters); break;

                    case (byte)Request.LoadStocks: OnRequestLoadStocks(operationRequest, sendParameters); break;

                    case (byte)Request.GetStocksList: OnRequestGetStocksList(operationRequest, sendParameters); break;

                    case (byte)Request.LoadStock: OnRequestLoadStock(operationRequest, sendParameters); break;

                    case (byte)Request.SaveStock: OnRequestSaveStock(operationRequest, sendParameters); break;

                    case (byte)Request.LoadSet: OnRequestLoadSet(operationRequest, sendParameters); break;

                    case (byte)Request.BuyUpgrade: OnRequestBuyUpgrade(operationRequest, sendParameters); break;

                    case (byte)Request.SaveSet: OnRequestSaveSet(operationRequest, sendParameters); break;

                    case (byte)Request.LoadItem: OnRequestLoadItem(operationRequest, sendParameters); break;

                    case (byte)Request.SaveItem: OnRequestSaveItem(operationRequest, sendParameters); break;

                    case (byte)Request.GetUserClan: OnRequestGetUserClan(operationRequest, sendParameters); break;

                    case (byte)Request.GetClansForProposal: OnRequestGetClansForProposal(operationRequest, sendParameters); break;

                    case (byte)Request.GetUserInvites: OnRequestGetUserInvites(operationRequest, sendParameters); break;

                    case (byte)Request.CreateProposal: OnRequestCreateProposal(operationRequest, sendParameters); break;

                    case (byte)Request.DeleteProposal: OnRequestDeleteProposal(operationRequest, sendParameters); break;

                    case (byte)Request.GetCoinsForClan: OnRequestGetCoinsForClan(operationRequest, sendParameters); break;

                    case (byte)Request.TryCreateClan: OnRequestTryCreateClan(operationRequest, sendParameters); break;

                    case (byte)Request.GetClanUsers : OnRequestGetClanUsers(operationRequest, sendParameters); break;

                    case (byte)Request.GetUserRights: OnRequestGetUserRights(operationRequest, sendParameters); break;

                    case (byte)Request.GetInvitesTab: OnRequestGetInvitesTab(operationRequest, sendParameters); break;

                    case (byte)Request.CreateInvite: OnRequestCreateInvite(operationRequest, sendParameters); break;

                    case (byte)Request.DeleteInvite: OnRequestDeleteInvite(operationRequest, sendParameters); break;

                    case (byte)Request.AcceptInvite: OnRequestAcceptInvite(operationRequest, sendParameters); break;

                    case (byte)Request.AcceptProposal: OnRequestAcceptProposal(operationRequest, sendParameters); break;

                    case (byte)Request.GetClanUpgrades: OnRequestGetClanUpgrades(operationRequest, sendParameters); break;

                    case (byte)Request.RequestDuelsTab: OnRequestDuelsTab(operationRequest, sendParameters); break;

                    case (byte)Request.RotateWheel: OnRequestRotateWheel(operationRequest, sendParameters); break;

                    case (byte)Request.GetReitings: OnRequestGetReitings(operationRequest, sendParameters); break;

                    case (byte)Request.GetReiting: OnRequestGetReiting(operationRequest, sendParameters); break;

                    case (byte)Request.RemoveSkills: OnRequestRemoveSkills(operationRequest, sendParameters); break;
                        
                    case (byte)Request.GetLobbyList: OnRequestGetLobbyList(); break;
                    case (byte)Request.UnSubscribeLobby: OnRequestUnSubscribeLobby(); break;

                    default:
                        Logger.Log.Info("Unknown request code" + operationRequest.OperationCode);
                        break;
                }
            }
            catch (Exception e)
            {              
                SendErrorToClient(e);
            }
        }

        #region lobby

        public void OnRequestGetLobbyList()
        {
            OperationResponse resp = new OperationResponse((byte)Request.GetLobbyList);
            resp.Parameters = new Dictionary<byte, object>();

            //получаем список текущих доступных лобби
            var lobbys = ManagerRooms.instance.GetLobbyDatas();
            resp.Parameters.Add((byte)Params.Data, lobbys);

            //подписываем игрока на обновление лобби
            SubscribeLobby();

            SendOperationResponse(resp, Options.sendParameters);
        }

        public void SubscribeLobby()
        {
            ManagerRooms.instance.AddLobbySubscriber(this);
        }

        public void OnRequestUnSubscribeLobby()
        {
            UnSubscribeLobby();
        }

        public void UnSubscribeLobby()
        {
            ManagerRooms.instance.RemoveLobbySubscriber(this);
        }

        #endregion

        private void OnRequestRemoveSkills(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var roleIdString = (string)operationRequest.Parameters[(byte)Params.RoleId];

            var roleId = (RoleType)Helper.GetEnumElement<RoleType>(roleIdString);

            DBManager.Inst.RemovePlayerSkillsByRole(player.playerId, roleId);
        }        

        private void OnRequestGetReiting(OperationRequest operationRequest, SendParameters sendParameters)
        {
            OperationResponse resp = new OperationResponse((byte)Request.GetReiting);
            var period = ReitingInterval.all;
            Logger.Log.Debug("reit ID " + operationRequest.Parameters[(byte)Params.Id]);

            if (operationRequest.Parameters.ContainsKey((byte)Params.periods))
            {
                Logger.Log.Debug("period " + operationRequest.Parameters[(byte)Params.periods]);
                period = (ReitingInterval)operationRequest.Parameters[(byte)Params.periods];
            }

            var result = ManagerReitings.inst.GetReiting(period, (ReitingType)operationRequest.Parameters[(byte)Params.Id]);
            
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetReitings(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Logger.Log.Debug("GET REITINGS");
            OperationResponse resp = new OperationResponse((byte)Request.GetReitings);
            var reitings = ManagerReitings.inst.GetReitings();
            var result = new Dictionary<byte, object>();
            result.Add((byte)Params.reitings, reitings);
            result.Add((byte)Params.periods, ManagerReitings.inst.periods);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestRotateWheel(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var result = new Dictionary<byte, object>();

            if (!ManagerDuels.inst.CheckDuelTime())
                return;

            if (!DBManager.Inst.TryPayWheelPoints(player.playerId, Options.wheelCost))
            {
                Logger.Log.Debug("Not enought wheel points");
            }

            var prize = ManagerDuels.inst.PlayDuelWheel(player.playerId);
            result.Add((byte)Params.prize, prize);
            
            var wheelPoins = DBManager.Inst.GetUserWheelPoinst(player.playerId);

            result.Add((byte)Params.wheelPoints, wheelPoins);
            result.Add((byte)Params.wheelCost, Options.wheelCost);

            OperationResponse resp = new OperationResponse((byte)Request.RotateWheel);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestDuelsTab(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var result = new Dictionary<byte, object>();

            if (!player.playerClan.ContainsKey((byte)Params.Id))
            {
                Logger.Log.Debug("Player not in Clan");
            }
            else
            {
                //Logger.Log.Debug("Player Clan id " + player.playerClan[(byte)Params.Id]);
                result.Add((byte)Params.clanId, player.playerClan[(byte)Params.Id]);
            }

            if(ManagerDuels.inst.currentDuelDayId != 0)
            {
                int clanPairId = 0;
                var clans = DBManager.Inst.GetDuelClansForUser(ManagerDuels.inst.currentDuelDayId, (int)player.playerClan[(byte)Params.Id], ref clanPairId);

                if(clanPairId != 0)
                {
                    result.Add((byte)Params.clans, clans);

                    var users = DBManager.Inst.GetDuelPersonalTopWithNames(clanPairId);
                    result.Add((byte)Params.users, users);

                    var missions = DBManager.Inst.GetDuelMissions(clanPairId);
                    result.Add((byte)Params.missions, missions);

                    var wheelPoins = DBManager.Inst.GetUserWheelPoinst(player.playerId);
                    var clanDuelWins = DBManager.Inst.GetClanWinsInDuel(ManagerDuels.inst.currentDuelDayId, (int)player.playerClan[(byte)Params.Id]);
                    
                    
                    var levels = ManagerDuels.inst.AwardsLevels;

                    result.Add((byte)Params.wheelPoints, wheelPoins);
                    result.Add((byte)Params.wheelCost, Options.wheelCost);
                    result.Add((byte)Params.Levels, levels);
                    result.Add((byte)Params.clanDuelWins, clanDuelWins);
                }
            }

            result.Add((byte)Params.Day, ManagerDuels.inst.currentDuelDayId);
            result.Add((byte)Params.Start, ManagerDuels.inst.start.ToString());
            result.Add((byte)Params.End, ManagerDuels.inst.end.ToString());



            OperationResponse resp = new OperationResponse((byte)Request.RequestDuelsTab);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestBuyUpgrade(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if(!this.player.clanRights.ContainsValue((int)ClanRights.BuyClanUpgrade)) return;

            //Logger.Log.Debug("Buy upgrade " + operationRequest.Parameters[(byte)Params.Id]);

            //Logger.Log.Debug("CLAN ID " + (int)player.playerClan[(byte)Params.Id]);
            //Logger.Log.Debug("UPGRADE ID " + (ClansUpgardesId)operationRequest.Parameters[(byte)Params.Id]);

            DBManager.Inst.TryBuyClanUpgrade((int)player.playerClan[(byte)Params.Id], (ClansUpgardesId)operationRequest.Parameters[(byte)Params.Id]);

            OnRequestGetUserClan(operationRequest, sendParameters);
        }

        /// <summary>
        /// Запрос для сектора, показывающего улучшения клана
        /// </summary>
        private void OnRequestGetClanUpgrades(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug("GET CLANS UPGRADES");

            var result = new Dictionary<byte, object>();

            if (this.player.clanRights.ContainsValue((int)ClanRights.BuyClanUpgrade))
            {
                var rights = new Dictionary<byte, object>();
                result.Add((byte)Params.rights, rights);
            }

            var ClanUpgrades = DBManager.Inst.GetClanUpgarades((int)player.playerClan[(byte)Params.Id]);
            result.Add((byte)Params.upgrades, ClanUpgrades);

            OperationResponse resp = new OperationResponse((byte)Request.GetClanUpgrades);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        /// <summary>
        /// Подтвердить запрос от пользователя вступить в клан
        /// </summary>
        private void OnRequestAcceptProposal(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Logger.Log.Debug("Accept invite");
            //Logger.Log.Debug("Check duel time " + ManagerDuels.inst.CheckDuelTime());

            //Если идёт дуэльное время, то не исполнять запрос
            if (ManagerDuels.inst.CheckDuelTime())
                return;

            if (!this.player.clanRights.ContainsValue((int)ClanRights.AcceptProposal))
            {
                return;
            }

            var userId = (long)operationRequest.Parameters[(byte)Params.Id];
            var clanId = (int)this.player.playerClan[(byte)Params.Id];

            //Logger.Log.Debug("userId " + userId);
            //Logger.Log.Debug("clanId " + clanId);

            if (!DBManager.Inst.DeleteInvPropClan(clanId: clanId, userId: userId, InviteType.fromUser))
            {
                Logger.Log.Debug("Check false");
                return;
            }

            Logger.Log.Debug("Check true");
            DBManager.Inst.AddUserToClan(clanId: clanId, userId: userId);

            OnRequestGetInvitesTab(operationRequest, sendParameters);
            OnRequestGetUserClan(operationRequest, sendParameters);
        }

        /// <summary>
        /// Принять приглашение
        /// </summary>
        private void OnRequestAcceptInvite(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug("Accept invite");
            Logger.Log.Debug("Check duel time " + ManagerDuels.inst.CheckDuelTime());
            
            //Если идёт дуэльное время, то не исполнять запрос
            if (ManagerDuels.inst.CheckDuelTime())
                return;

            var userId = this.userDbId;
            var clanId = (int)operationRequest.Parameters[(byte)Params.Id];

            //Logger.Log.Debug("userId " + userId);
            //Logger.Log.Debug("clanId " + clanId);

            if(!DBManager.Inst.DeleteInvPropClan(clanId:clanId, userId:userId, InviteType.fromOwner))
            {
                Logger.Log.Debug("Check false");
                return;
            }

            Logger.Log.Debug("Check true");
            DBManager.Inst.AddUserToClan(clanId: clanId, userId: userId);

            OnRequestGetUserClan(operationRequest, sendParameters);
        }

        /// <summary>
        /// Удаление приглашения
        /// </summary>
        private void OnRequestDeleteInvite(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var clanId = (int)this.player.playerClan[(byte)Params.Id];
            var userId = (long)operationRequest.Parameters[(byte)Params.Id];

            //Logger.Log.Debug("userId " + userId);
            //Logger.Log.Debug("clanId " + clanId);

            DBManager.Inst.DeleteInviteProposal(clanId, userId);

            OnRequestGetInvitesTab(operationRequest, sendParameters);
        }

        private void OnRequestCreateInvite(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var clanId = (int)this.player.playerClan[(byte)Params.Id];
            var userId = (long)operationRequest.Parameters[(byte)Params.Id];

            //Logger.Log.Debug("userId " + userId);
            //Logger.Log.Debug("clanId " + clanId);

            DBManager.Inst.CreateInviteProposal(clanId, userId, InviteType.fromOwner);

            OnRequestGetInvitesTab (operationRequest, sendParameters);
        }

        private void OnRequestGetInvitesTab(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Logger.Log.Debug("GetInvitesTab");
            //Logger.Log.Debug(this.player.playerClan[(byte)Params.Id]);

            var result = new Dictionary<byte, object>();

            result.Add((byte)Params.rights, this.player.clanRights);

            if (this.player.clanRights.ContainsValue((int)ClanRights.InvitePlayer))
            {
                var freePlayers = DBManager.Inst.GetFreePlayers();
                result.Add((byte)Params.Players, freePlayers);
            }

            if (this.player.clanRights.ContainsValue((int)ClanRights.SeeProposals))
            {
                var clanInvites = DBManager.Inst.GetClanInvites((int)this.player.playerClan[(byte)Params.Id]);
                result.Add((byte)Params.invites, clanInvites);
            }

            OperationResponse resp = new OperationResponse((byte)Request.GetInvitesTab);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetUserRights(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;

            var result = DBManager.Inst.GetUserClanRights(userId);

            OperationResponse resp = new OperationResponse((byte)Request.GetUserRights);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.rights, result);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetClanUsers(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Logger.Log.Debug(operationRequest.Parameters.Keys.Count);

            var clanId = (int)operationRequest.Parameters[(byte)Params.Id];

            //Logger.Log.Debug(clanId);
            var result = DBManager.Inst.GetClanUsers(clanId);

            OperationResponse resp = new OperationResponse((byte)Request.GetClanUsers);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.users , result);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestTryCreateClan(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;

            DBManager.Inst.TryCreateClan(operationRequest.Parameters, userId);

            OperationResponse resp = new OperationResponse((byte)Request.GetUserClan);
            resp.Parameters = DBManager.Inst.GetUserClan(userId);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetCoinsForClan(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            var result = DBManager.Inst.GetCoinsForClan(userId);

            OperationResponse resp = new OperationResponse((byte)Request.GetCoinsForClan);
            resp.Parameters = result;
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestDeleteProposal(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            var clanId = (int)operationRequest.Parameters[(byte)Params.Id];

            DBManager.Inst.DeleteInviteProposal(clanId, userId);

            OperationResponse resp = new OperationResponse((byte)Request.GetUserInvites);
            var invites = DBManager.Inst.GetUserInvites(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.invites, invites);
            SendOperationResponse(resp, sendParameters);

            resp = new OperationResponse((byte)Request.GetClansForProposal);
            var clans = DBManager.Inst.GetClansForProposal(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.clans, clans);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestCreateProposal(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            var clanId = (int)operationRequest.Parameters[(byte)Params.Id];

            DBManager.Inst.CreateInviteProposal(clanId, userId, InviteType.fromUser);
            
            OperationResponse resp = new OperationResponse((byte)Request.GetUserInvites);
            var invites = DBManager.Inst.GetUserInvites(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.invites, invites);
            SendOperationResponse(resp, sendParameters);

            resp = new OperationResponse((byte)Request.GetClansForProposal);
            var clans = DBManager.Inst.GetClansForProposal(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.clans, clans);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetUserInvites(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            OperationResponse resp = new OperationResponse((byte)Request.GetUserInvites);
            var invites = DBManager.Inst.GetUserInvites(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.invites, invites);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetClansForProposal(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            OperationResponse resp = new OperationResponse((byte)Request.GetClansForProposal);
            var clans = DBManager.Inst.GetClansForProposal(userId);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.clans, clans);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetUserClan(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userId = this.userDbId;
            OperationResponse resp = new OperationResponse((byte)Request.GetUserClan);
            resp.Parameters = DBManager.Inst.GetUserClan(userId);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestSaveItem(OperationRequest operationRequest, SendParameters sendParameters)
        {
            DBManager.Inst.SaveItem(operationRequest.Parameters);

            var setId = (int)operationRequest.Parameters[(byte)Params.SetId];
            OperationResponse resp = new OperationResponse((byte)Request.LoadSet);
            resp.Parameters = DBManager.Inst.GetSet(setId);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestLoadItem(OperationRequest operationRequest, SendParameters sendParameters)
        {
            int id = (int)operationRequest.Parameters[(byte)Params.Id];

            OperationResponse resp = new OperationResponse((byte)Request.LoadItem);
            resp.Parameters = DBManager.Inst.GetItem(id);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestLoadSet(OperationRequest operationRequest, SendParameters sendParameters)
        {
            int id = (int)operationRequest.Parameters[(byte)Params.Id];

            OperationResponse resp = new OperationResponse((byte)Request.LoadSet);
            resp.Parameters = DBManager.Inst.GetSet(id);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestLoadStock(OperationRequest operationRequest, SendParameters sendParameters)
        {
            int id = (int)operationRequest.Parameters[(byte)Params.Id];

            OperationResponse resp = new OperationResponse((byte)Request.LoadStock);
            resp.Parameters = DBManager.Inst.GetStock(id);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestSaveStock(OperationRequest operationRequest, SendParameters sendParameters)
        {
            int id = (int)operationRequest.Parameters[(byte)Params.Id];

            OperationResponse resp = new OperationResponse((byte)Request.LoadStock);
            resp.Parameters = DBManager.Inst.GetStock(DBManager.Inst.SaveStock(operationRequest.Parameters));
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestSaveSet(OperationRequest operationRequest, SendParameters sendParameters)
        {
            int id = (int)operationRequest.Parameters[(byte)Params.Id];

            OperationResponse resp = new OperationResponse((byte)Request.LoadSet);
            resp.Parameters = DBManager.Inst.GetSet(DBManager.Inst.SaveSet(operationRequest.Parameters));
            SendOperationResponse(resp, sendParameters);

            int stockId = (int)operationRequest.Parameters[(byte)Params.StockId];

            resp = new OperationResponse((byte)Request.LoadStock);
            resp.Parameters = DBManager.Inst.GetStock(stockId);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGetStocksList(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var stocksData = DBManager.Inst.GetStocksList();

            OperationResponse resp = new OperationResponse((byte)Request.GetStocksList);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.stocks, stocksData);
            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestReload(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Logger.Log.Debug("Reload");

            //Server.inst.Reload();
        }

        private void OnRequestLoadStocks(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug("load stocks request");

            var stocksData = DBManager.Inst.GetStocks();

            OperationResponse resp = new OperationResponse((byte)Request.LoadStocks);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.stocks, stocksData);
            SendOperationResponse(resp, sendParameters);

            Logger.Log.Debug($"send loaded stocks");
        }

        private void OnRequestLoadClans(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var clansData = DBManager.Inst.GetClans();

            OperationResponse resp = new OperationResponse((byte)Request.LoadClans);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.clans, clansData);
            SendOperationResponse(resp, sendParameters);

            Logger.Log.Debug($"send loaded clans");
        }

        private void OnRequestSaveGift(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (this.systemRole != SystemRole.admin)
            {
                Logger.Log.Debug($"not enought permission to save extra");
                return;
            }

            Logger.Log.Debug("create gift");

            var id = (int)(operationRequest.Parameters)[(byte)Params.giftId];

            var name = (string)(operationRequest.Parameters)[(byte)Params.giftName];
            var description = (string)(operationRequest.Parameters)[(byte)Params.giftDescription];
            var cost = (string)(operationRequest.Parameters)[(byte)Params.giftCost];
            var image = (string)(operationRequest.Parameters)[(byte)Params.giftImage];

            var giftEventType = (string)(operationRequest.Parameters)[(byte)Params.giftEventType];
            var giftType = (string)(operationRequest.Parameters)[(byte)Params.giftType];
            var costType = (string)(operationRequest.Parameters)[(byte)Params.giftCostType];
            var extraEffect = (string)(operationRequest.Parameters)[(byte)Params.giftExtraEffect];


            DBManager.Inst.CreateGift(id, name, description, cost, costType, image, giftEventType, giftType, extraEffect);
        }

        private void OnRequestLoadGifts(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug("Load gifts");

            var giftsData = DBManager.Inst.GetGifts();

            OperationResponse resp = new OperationResponse((byte)Request.LoadGifts);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.gifts, giftsData);
            SendOperationResponse(resp, sendParameters);

            Logger.Log.Debug($"send loaded Gifts");
        }


        /// <summary>
        /// запрос на авторизацию
        /// </summary>
        private void OnRequestLogin(OperationRequest operationRequest, SendParameters sendParameters)
        {
            ManagerUser.instance.UserAuthorization(this, operationRequest, sendParameters);

            pingToClient = timers.ScheduleOnInterval(() => PingToClient(), 0, 500);
        }

        private void OnRequestBet(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.GetRoom().roleAuction.SetBet(player, operationRequest.Parameters);
        }

        private void OnRequestSelectPlayer(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.SelectPlayer(operationRequest.Parameters);
        }

        private void OnRequestJudging(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.GetRoom().roomJudging.Judging(player, operationRequest.Parameters);
        }
        
        private void OnRequestBuyExtra(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.BuyExtra(operationRequest.Parameters);
        }

        private void OnRequestSaveUserSlot(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.SaveUserExtraSlot(operationRequest.Parameters);           
        }

        private void OnRequestClearUserSlot(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.ClearUserExtraSlot(operationRequest.Parameters);
        }

        private void OnRequestUseExtra(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.UseExtra(operationRequest.Parameters);          
        }

       
        

        private void OnRequestLevelUpSkill(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.LevelUpskill(operationRequest.Parameters);
        }
        
        private void OnRequestLoadSkills(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug($"start load skills");
            var skillsData =  DBManager.Inst.LoadSkills();
            Logger.Log.Debug($"end load skills");

            OperationResponse resp = new OperationResponse((byte)Request.LoadSkills);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.Skills, skillsData);
            SendOperationResponse(resp, sendParameters);

            Logger.Log.Debug($"send loaded skills");
        }

        private void OnRequestLoadExtras(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug($"start load extras");
            var extrasData = DBManager.Inst.LoadExtras();
            Logger.Log.Debug($"end load extras");

            OperationResponse resp = new OperationResponse((byte)Request.LoadExtras);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.Extras, extrasData);
            SendOperationResponse(resp, sendParameters);

            Logger.Log.Debug($"send loaded extras");
        }
        
        private void OnRequestSendChat(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.SendChatMessage(operationRequest.Parameters);
        }

        private void OnRequestShareMessage(OperationRequest operationRequest, SendParameters sendParameters)
        {
            player.UseShareAction(operationRequest.Parameters);
        }        

        private void OnRequestCreateGame(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //если при создании нового лобби у игрока уже есть лобби и он является его владельцем
            if (player.GetLobby() != null && player.GetLobby().owner == player)
            {
                ManagerRooms.instance.DisposeLobby(player.GetLobby());
            }

            ManagerRooms.instance.CreateLobby(player, null, true);
        }

        private void OnRequestCreateLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //если при создании нового лобби у игрока уже есть лобби и он является его владельцем
            if (player.GetLobby() != null && player.GetLobby().owner == player)
            {
                ManagerRooms.instance.DisposeLobby(player.GetLobby());
            }

            ManagerRooms.instance.CreateLobby(player, operationRequest.Parameters);
        }

        private void OnRequestForceStartGameFromLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            ManagerRooms.instance.StartGameFromLobby(player, operationRequest.Parameters);
        }        

        private void OnRequestStartGameFromLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            ManagerRooms.instance.StartGameFromRoomBuilder(player, operationRequest.Parameters);
        }

        private void OnRequestStartGame(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug($"find room");
            //ManagerRooms.instance.FindRoom(player, operationRequest.Parameters);
        }

        private void OnRequestJoinLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug($"join Lobby");
            ManagerRooms.instance.JoinLobby(player, operationRequest.Parameters);
        }

        private void OnRequestLeaveLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Logger.Log.Debug($"leave Lobby");
            ManagerRooms.instance.LeaveLobby(player);
        }

        //private void OnRequestRemoveLobby(OperationRequest operationRequest, SendParameters sendParameters)
        //{
        //    Logger.Log.Debug($"remove Lobby");
        //    ManagerRooms.instance.RemoveLobby(player, operationRequest.Parameters);
        //}       

        private void OnRequestBuy(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var shopItemId = (int)operationRequest.Parameters[(byte)Params.ShopItemId];

            Logger.Log.Info($"shopItemId {shopItemId}");

            var confirmURL = UMoney.CreateRequest(shopItemId, this);

            OperationResponse resp = new OperationResponse((byte)Request.Buy);

            resp.Parameters = new Dictionary<byte, object>();

            resp.Parameters.Add((byte)Params.ConfirmURL, confirmURL);

            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestChangeName(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var userName = (string)operationRequest.Parameters[(byte)Params.UserName];

            var beforeCensuredUserName = CensureFilter.Beatify(userName, true, true, true);

            //проверяем имя Проверяя на исключения:
            var afterCensuredUserName = CensureFilter.Process(beforeCensuredUserName, true, false);

            OperationResponse resp = new OperationResponse((byte)Request.ChangeUserName);
            resp.Parameters = new Dictionary<byte, object>();

            if (afterCensuredUserName == beforeCensuredUserName)
            {
                //ok
                Logger.Log.Debug($"name is good");

                this.userName = userName;

                DBManager.Inst.SetUserName(userDbId, userName);

                resp.ReturnCode = (short)ReturnCode.Succes;
            }
            else
            {
                //!ok
                Logger.Log.Debug($"name is bad");

                resp.ReturnCode = (short)ReturnCode.Fail;
            }

            resp.Parameters.Add((byte)Params.UserName, userName);

            SendOperationResponse(resp, sendParameters);
        }

        private void OnRequestGif(OperationRequest operationRequest, SendParameters sendParameters)
        {       
            OperationResponse resp = new OperationResponse((byte)Request.Gif);

            resp.Parameters = new Dictionary<byte, object>();

            var gifData = GifConverter.GetGifData();

            //resp.Parameters.Add((byte)Params.Gif, gifData);

            SendOperationResponse(resp, sendParameters);
        }

        public void OnRequestCloseLobby(OperationRequest operationRequest, SendParameters sendParameters)
        {
            CloseLobby();

            OperationResponse resp = new OperationResponse((byte)Request.CloseLobby);
            resp.Parameters = new Dictionary<byte, object>();
            SendOperationResponse(resp, sendParameters);
        }

        public void OnRequestExitGameRoom(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (player.GetRoom() != null)
            {
                if (player.GetRoom().roomState == RoomState.Game)
                {
                    OperationResponse resp = new OperationResponse((byte)Request.RequestExitGameRoom);
                    resp.Parameters = new Dictionary<byte, object>();

                    resp.Parameters.Add((byte)Params.caption, "Выход из комнаты");

                    if (player.isLive())
                    {
                        resp.Parameters.Add((byte)Params.message,
                            $"Выход из комнаты до конца игры стоит 1000 монет. " +
                            $"Ваш персонаж умрет и Вы не получите награу, " +
                            $"даже если Ваша команда победит.");                     
                    }
                    else
                    {
                        resp.Parameters.Add((byte)Params.message,
                            $"Вы действительно хотите выйти?");
                    }                          
                   
                    SendOperationResponse(resp, sendParameters);

                    Logger.Log.Debug($"player {player.playerName} {player.playerId} request exit from room");
                }
            }
            else
            {
                OperationResponse resp = new OperationResponse((byte)Request.RequestExitGameRoom);
                resp.Parameters = new Dictionary<byte, object>();

                resp.Parameters.Add((byte)Params.caption, "Выход из комнаты");
                resp.Parameters.Add((byte)Params.message,
                           $"Вы действительно хотите выйти?");

                SendOperationResponse(resp, sendParameters);
            }
        }

        public void OnRequestConfirmExitGameRoom(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (player.GetRoom() != null)
            {
                if (player.GetRoom().roomState == RoomState.WaitPlayers)
                {

                }

                if (player.GetRoom().roomState == RoomState.Game)
                {
                    //если игрок жив, отправляем его в морг и проверяем окончание игры
                    if (player.isLive())
                    {
                        player.GetRoom().roomLogic.SendPlayerToMorgue(player, true);

                        player.GetRoom().roomEndGame.CheckEndGame2();
                    }          

                    if (player.GetRoom() != null)
                    {
                        //пишем всем в комнате, что игрок покинул игру
                        player.GetRoom().roomChat.PublicMessage($"{player.playerName} покинул игру");

                        player.GetRoom().RemovePlayerFromGame(player);
                    }

                    player.targetPlayer = null;

                    //player.SetupRole(null);
                    //player.SetOldRole(null);

                    //меняем статус игрока на "ожидает"
                    player.SetPlayerQueueStatus(PlayerQueueStatus.Idle);

                    Logger.Log.Debug($"player {player.playerName} {player.playerId} exit from room");
                }
            }
        }

        private void CloseLobby()
        {
            foreach (var p in player.GetRoom().players.Values)
            {
                p.SetPlayerQueueStatus(PlayerQueueStatus.Idle);
            }

            player.GetRoom().Dispose();
        }

        public void PaymentSucceeded(string description)
        {
            OperationResponse resp = new OperationResponse((byte)Request.BuySecceeded);

            resp.Parameters = new Dictionary<byte, object>();

            //resp.Parameters.Add((byte)Params.Description, description);

            SendOperationResponse(resp, Options.sendParameters);
        }

        public SystemRole systemRole;
        public void SetSystemRole(string role)
        {
            this.systemRole = (SystemRole)Helper.GetEnumElement<SystemRole>(role);
        }

        //admin
        private void OnRequestSaveAchieve(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (this.systemRole != SystemRole.admin)
            {
                Logger.Log.Debug($"not enought permission to save achieve");
                return;
            }
            DBManager.Inst.SaveAchieve(operationRequest.Parameters);
        }
        private void OnRequestSaveSkill(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (this.systemRole != SystemRole.admin)
            {
                Logger.Log.Debug($"not enought permission to save skill");
                return;
            }
            DBManager.Inst.SaveSkill(operationRequest.Parameters);
        }

        private void OnRequestSaveExtra(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (this.systemRole != SystemRole.admin)
            {
                Logger.Log.Debug($"not enought permission to save extra");
                return;
            }
            DBManager.Inst.SaveExtra(operationRequest.Parameters);
        }

        #region Reusable Methods

        private void SendErrorToClient(Exception e)
        {
            var now = DateTime.Now;

            var message = $"{now.ToString("HH:mm:ss")} - <color=#D45B5B><b>{e.Message}</b></color> \n\n {e.StackTrace} \n {e.InnerException}";

            Logger.Log.Error(message);

            OperationResponse resp = new OperationResponse((byte)Request.ErrorMessage);

            resp.Parameters = new Dictionary<byte, object>();

            resp.Parameters.Add((byte)Params.Error, message);

            SendOperationResponse(resp, Options.sendParameters);
        }

        #endregion
    }

}
