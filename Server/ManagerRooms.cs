using ExitGames.Client.Photon.LoadBalancing;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Mafia_Server
{
    public class ManagerRooms
    {
        public static ManagerRooms instance;
        public ManagerRooms()
        {
            instance = this;

            CreateConfigs();
        }

        private Dictionary<RoomType, RoomConfig> roomConfigs;
        private void CreateConfigs()
        {
            roomConfigs = new Dictionary<RoomType, RoomConfig>();

            //List<RoleType> roles = new List<RoleType>
            //{RoleType.Citizen,RoleType.Doctor,RoleType.Guerilla,RoleType.Saint,RoleType.Witness,RoleType.Commissar,
            //RoleType.Mafia,RoleType.MafiaBoss,RoleType.Maniac,RoleType.Sinner,RoleType.Werewolf};

            //8
            {
                var roomConfig = new RoomConfig(RoomType._8 ,8, 1, 4, 2);

                //roomConfig.availablesRoles = roles;

                roomConfigs.Add(RoomType._8, roomConfig);
            }

            //12
            {
                var roomConfig = new RoomConfig(RoomType._12,12, 2,  6, 3);

                //roomConfig.availablesRoles = roles;

                roomConfigs.Add(RoomType._12, roomConfig);
            }

            //16
            {
                var roomConfig = new RoomConfig(RoomType._16, 16, 2, 8, 4);

                //roomConfig.availablesRoles = roles;

                roomConfigs.Add(RoomType._16, roomConfig);
            }

            //20
            {
                var roomConfig = new RoomConfig(RoomType._20, 20, 3, 10, 6);

                //roomConfig.availablesRoles = roles;

                roomConfigs.Add(RoomType._20, roomConfig);
            }   

            foreach(var c in roomConfigs)
            {
                Logger.Log.Debug($"create config {c.Key}");
            }
        }

        public RoomConfig GetRoomConfigById(RoomType id)
        {
            if (roomConfigs.ContainsKey(id))
            {
                return roomConfigs[id];
            }

            return null;
        }

        #region Lobbys

        private int lobbyIdCounter = 0;
        private int GetNewLobbyId { get => lobbyIdCounter++; }

        public Dictionary<int, Lobby> lobbys { get; private set; } = new Dictionary<int, Lobby>();

        public void CreateLobby(BasePlayer player, Dictionary<byte,object> parameters =null, bool useRoomBuilder=false)
        {
            var lobbyId = GetNewLobbyId;

            var lobby = new Lobby(lobbyId, player, parameters, useRoomBuilder);

            lobbys.Add(lobbyId, lobby);

            Update_LobbyInfo_AddLobby(lobby);
        }

        public void JoinLobby(Player player, Dictionary<byte, object> parameters)
        {
            //узнаем к какой комнате хочет подключиться игрок
            var lobbyId = (int)parameters[(byte)Params.Id];

            //ищем комнату
            Lobby lobby = null;
            if (lobbys.ContainsKey(lobbyId))
            {
                lobby = lobbys[lobbyId];
            }
            else
            {
                Logger.Log.Debug($"try join lobby not exist");
                //отправить ошибку на клиент, что такая комната не найдена
                return;
            }

            //проверяем состояние комнаты
            //if (room.roomState != RoomState.WaitPlayers)
            //{
            //    Logger.Log.Debug($"join room is not WaitPlayers ");
            //    //отправить ошибку на клиент, что комната не доступна для подключения
            //    return;
            //}

            //проверяем есть ли в комнате место
            //if (room.roomConfig.playerLimit >= room.players.Count)
            //{
            //    Logger.Log.Debug($"try join room if full");
            //    //отправить ошибку на клиент, что в комнате нет места для новых игроков
            //    return;
            //}

            //проверить наличие черных списков

            lobby.AddPlayer(player);

            Update_LobbyInfo_PlayerCount(lobby);
        }

        public void LeaveLobby(BasePlayer player)
        {
            var lobby = player.GetLobby();

            //если вышел владелец лобби
            if (lobby.owner == player)
            {
                DisposeLobby(lobby);               
                return;
            }

            lobby.RemovePlayer(player);

            Update_LobbyInfo_PlayerCount(lobby);
        }

        public void DisposeLobby(Lobby lobby)
        {
            //удаляем игроков из лобби
            foreach(var p in lobby.players.Values)
            {
                p.SetLobby(null);               

                p.SetPlayerQueueStatus(PlayerQueueStatus.Idle);   
            }

            //закрыть на клиентах окно лобби
            EventData eventData = new EventData((byte)Events.CloseLobby);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.SendTo(lobby.clients, Options.sendParameters);

            //забываем лобби
            lobbys.Remove(lobby.id);

            foreach (var p in lobby.players.Values)
            {
                if (p.client != null) p.client.OnRequestGetLobbyList();
            }

            Update_LobbyInfo_RemoveLobby(lobby);
        }

        public void StartGameFromRoomBuilder(Player player, Dictionary<byte, object> parameters)
        {
            //создаем новую комнату
            var room = CreateNewRoom(player, parameters);

            //переносим в нее игроков из лобби
            room.AddPlayersFromLobby(player.GetLobby());

            //запускаем игру
            room.StartGameFromRoomBuilder(parameters);

            //удаляем лобби
            DisposeLobby(player.GetLobby());
        }

        public void StartGameFromLobby(Player player, Dictionary<byte, object> parameters)
        {
            //создаем новую комнату
            var room = CreateNewRoom(player, parameters);

            //переносим в нее игроков из лобби
            room.AddPlayersFromLobby(player.GetLobby());

            //запускаем игру
            room.StartGameFromLobby(player.GetLobby());

            //удаляем лобби
            DisposeLobby(player.GetLobby());
        }

        private List<Client> subscribers = new List<Client>();
        public void AddLobbySubscriber(Client client)
        {
            if (subscribers.Contains(client)) return;

            subscribers.Add(client);
        }

        public void RemoveLobbySubscriber(Client client)
        {
            if (subscribers.Contains(client) == false) return;

            subscribers.Remove(client);
        }

        //изменение кол-ва игроков в лобби
        private void Update_LobbyInfo_PlayerCount(Lobby lobby)
        {
            EventData eventData = new EventData((byte)Events.LobbyInfo_PlayersCount);
            eventData.Parameters = lobby.GetLobbyData();
            eventData.SendTo(subscribers, Options.sendParameters);
        }

        //добавление лобби
        private void Update_LobbyInfo_AddLobby(Lobby lobby)
        {
            EventData eventData = new EventData((byte)Events.LobbyInfo_AddLobby);
            eventData.Parameters = lobby.GetLobbyData();
            eventData.SendTo(subscribers, Options.sendParameters);
        }

        //удаление лобби
        private void Update_LobbyInfo_RemoveLobby(Lobby lobby)
        {
            EventData eventData = new EventData((byte)Events.LobbyInfo_RemoveLobby);
            eventData.Parameters = lobby.GetLobbyData();
            eventData.SendTo(subscribers, Options.sendParameters);
        }

        #endregion

        #region Rooms

        int roomIdCounter = 0;
      

        public Dictionary<int,Room> rooms= new Dictionary<int, Room>();
        public Room CreateNewRoom(Player player, Dictionary<byte,object> parameters)
        {
            var roomId = roomIdCounter++;

            //testmode
            var newRoom = new Room(roomId, player, parameters);

            rooms.Add(roomId, newRoom);

            Logger.Log.Debug($"room count => {rooms.Count}");

            return newRoom;
        }

        public void DisposeRoom(Room room)
        {
            //флаг об окончании игры
            room.roomIsStoped = true;

            //меняем статус комнаты
            room. SetRoomState(RoomState.EndGame);

            //сбрасываем игроков
            foreach (var p in room.players.Values)
            {
                p.SetRoom(null);

                p.targetPlayer = null;

                if(p.playerRole != null)
                {
                    p.playerRole.roleEffects.StopChatTimer();
                }
               
                //p.SetOldRole(null);
                //p.SetupRole(null);

                p.SetPlayerQueueStatus(PlayerQueueStatus.Idle);
            }

            //удаляем в комнате таймеры
            room.Dispose();

            //забываем комнату
            rooms.Remove(room.id);

            Logger.Log.Debug($"room count => {rooms.Count}");
        }

        public Dictionary<int, object> GetLobbyDatas()
        {
            var result = new Dictionary<int, object>();

            foreach (var l in lobbys.Values)
            {
                var lobbyData = l.GetLobbyData();

                result.Add(l.id, lobbyData);
            }

            return result;
        }

        #endregion
    }

    public class Lobby
    {
        public int id { get; private set; }
        public BasePlayer owner { get; private set; }

        public RoomType roomType { get; private set; } = RoomType._8;
        public LeagueId leagueId { get; private set; } = LeagueId.League_1;
        public CostId cost { get; private set; } = CostId._20;
        public bool useExtras { get; private set; } = false;

        public bool useRoomBuilder { get; private set; }
        public Lobby(int id, BasePlayer owner, Dictionary<byte, object> parameters, bool useRoomBuilder=false)
        {
            this.id = id;
            this. owner = owner;

            this.useRoomBuilder = useRoomBuilder;

            roomType = (RoomType)parameters[(byte)Params.RoomType];

            leagueId = (LeagueId)parameters[(byte)Params.LeagueId];

            cost = (CostId)parameters[(byte)Params.Cost];

            useExtras = (bool)parameters[(byte)Params.UseExtra];

            //тестовый конструктор комнаты
            if (useRoomBuilder)
            {
                OperationResponse resp = new OperationResponse((byte)Request.StartRoomBuilder);
                resp.Parameters = new Dictionary<byte, object>();
                owner.client.SendOperationResponse(resp, Options.sendParameters);
            }          

            AddPlayer(owner);           
        }

        public Dictionary<byte, object> GetLobbyData()
        {
            var result = new Dictionary<byte, object>();

            result.Add((byte)Params.Id, id);

            result.Add((byte)Params.Name, owner.playerName);

            result.Add((byte)Params.Count, players.Count);

            result.Add((byte)Params.MaxCount, roomType);

            result.Add((byte)Params.LeagueId, leagueId);

            result.Add((byte)Params.Cost, cost);

            result.Add((byte)Params.UseExtra, useExtras);

            return result;
        }

        public Dictionary<long, BasePlayer> players { get; private set; } = new Dictionary<long, BasePlayer>();
        public List<Client> clients { get; private set; } = new List<Client>();
        public void AddPlayer(BasePlayer player)
        {
            if (players.ContainsKey(player.playerId))
            {
                return;
            }

            //запоминаем для нового игрока это лобби
            player.SetLobby(this);

            //отправить старым игрокам в этом лобби нового игрока
            EventData eventData = new EventData((byte)Events.AddPlayerToLobby);
            eventData.Parameters = GetPlayerData(player);
            eventData.SendTo(clients, Options.sendParameters);          



            //запоминаем новго игрока
            players.Add(player.playerId, player);

            //запоминаем клиент нового игрока
            clients.Add(player.client);

            //меняем статус новому игроку
            player.SetPlayerQueueStatus(PlayerQueueStatus.InLobby);

            //отправить новому игроку старых игроков
            OperationResponse resp = new OperationResponse((byte)Request.JoinLobby);
            resp.Parameters = GetLobbyPlayersData();
            player.client.SendOperationResponse(resp, Options.sendParameters);

            //отправляем владельцу в тестовый конструктор комнат нового игрока
            if (useRoomBuilder)
            {
                resp = new OperationResponse((byte)Request.AddLobbyPlayer);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.UserId, player.playerId);
                resp.Parameters.Add((byte)Params.UserName, player.playerName);
                owner.client.SendOperationResponse(resp, Options.sendParameters);
            }          
        }

        public void RemovePlayer(BasePlayer player)
        {
            player.SetLobby(null);

            players.Remove(player.playerId);

            clients.Remove(player.client);

            player.SetPlayerQueueStatus(PlayerQueueStatus.Idle);

            //удалить у оставшихся игроков, вышедшего игрока
            EventData eventData = new EventData((byte)Events.RemovePlayerFromLobby);
            eventData.Parameters = GetPlayerData(player);
            eventData.SendTo(clients, Options.sendParameters);

            //тестовый конструктор комнат
            //отправляем владельцу лобби инфо о ушедшем игроке
            if (useRoomBuilder)
            {
                OperationResponse resp = new OperationResponse((byte)Request.RemoveLobbyPlayer);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.UserId, player.playerId);
                owner.client.SendOperationResponse(resp, Options.sendParameters);
            }          
        }

        private Dictionary<byte, object> GetLobbyPlayersData()
        {
            var result = new Dictionary<byte, object>();
            byte playerCounter = 0;
            foreach (var p in players.Values)
            {
                var playerData = GetPlayerData(p);

                result.Add(playerCounter++, playerData);
            }

            return result;
        }

        private Dictionary<byte, object> GetPlayerData(BasePlayer player)
        {
            var result = new Dictionary<byte, object>();

            result.Add((byte)Params.Id, player.playerId);
            result.Add((byte)Params.Name, player.playerName);

            return result;
        }       
    }
}
