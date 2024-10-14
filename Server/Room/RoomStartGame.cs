using ExitGames.Client.Photon.LoadBalancing;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Mafia_Server
{
    public class RoomStartGame
    {
        private Room room;
        public RoomStartGame(Room room) 
        { 
            this.room = room;
        }

        private long gameTimerTick = 1000;
        public void StartGame()
        {
            //меняем состояние комнаты
            room.SetRoomState(RoomState.Game);

            //добавляем в комнату ботов на пустые позиции игроков
            Logger.Log.Debug($"room add bots");
            room.roomBots.AddBots();

            //сброс игроков к дефолтным настройкам и смена статуса игрока на "в игре"
            Logger.Log.Debug($"room reset players to default");
            ResetPlayersToDefaultSettings();

            //раздаем роли выигранные на ацукционе
            //все доступные для раздачи роли
            //var availableRoles = new List<RoleType>();
            AssignAuctionRolesToPlayers(/*availableRoles*/);

            //случайным рбразом раздаем оставшиеся роли
            AssignRandomAvailableRolesToPlayers(/*availableRoles*/);    

            //отправляем на клиенты инфо о игроках(айди и ники) и всех ролях в этой игре
            SendPlayersAndRoles();
            //сообщаем игрокам, кем они играют в этой игре
            SendPersonalRoleToPlayers();

            //добавляем игроков в соотв чаты
            Logger.Log.Debug($"assign players to chats");
            AssignPlayersToChats();

            Logger.Log.Debug($"room assign players to teams");
            //распределение игроков и ботов по командам 
            AssignPlayersToTeams();

            //загрузка скиллов для игроков в соотв их ролям
            Logger.Log.Debug($"room assign skills to players and bots");
            LoadAndAssignSkills();

            //после того, как назначены все роли и их скиллы, загрузка и назначение Экстр для игроков
            //некоторые скиллы влияют на экстры
            Logger.Log.Debug($"room load and assign extras to players to players");
            //tесли в комнате разрешены экстры
            if (room.useExtras)
            {
                LoadAndAssignExtras();
            }          

            //включаем таймер смерти, если игрок молчал в чате
            Logger.Log.Debug($"room enable death timer fo players");
            EnableDeathTimers();

            Logger.Log.Debug($"room set phase first night");
            room.roomPhases.SetGamePhase(GamePhase.FirstNight);

            Logger.Log.Debug($"room start game timer");
            room.roomLogic.gameTimer = room.poolFiber.ScheduleOnInterval(() => room.roomPhases.GameTimer(), gameTimerTick, gameTimerTick);

            Logger.Log.Debug($"room game started ===========================");
        }

       

        public void StartGame(Dictionary<byte,object> parameters)
        {
            Logger.Log.Debug($"room start game ===========================");

            //важно сохранять очередность вызова методов

            //меняем состояние комнаты
            room.SetRoomState(RoomState.Game);

            //добавляем в комнату ботов на пустые позиции игроков
            Logger.Log.Debug($"room add bots");
            room.roomBots.AddBots();

            //сброс игроков к дефолтным настройкам и смена статуса игрока на "в игре"
            Logger.Log.Debug($"room reset players to default");            
            ResetPlayersToDefaultSettings();

            //тестовое распределение ролей между игроками и ботами
            Logger.Log.Debug($"room assign players roles from room builder");           
            AssignRolesToPlayers(parameters);
            AssignRolesToBots(parameters);         

            //отправляем на клиенты инфо о игроках(айди и ники) и всех ролях в этой игре
            SendPlayersAndRoles();
            //сообщаем игрокам, кем они играют в этой игре
            SendPersonalRoleToPlayers();

            //добавляем игроков в соотв чаты
            Logger.Log.Debug($"assign players to chats");
            AssignPlayersToChats();

            Logger.Log.Debug($"room assign players to teams");
            //распределение игроков и ботов по командам 
            AssignPlayersToTeams();              

            //загрузка скиллов для игроков в соотв их ролям
            Logger.Log.Debug($"room assign skills to players and bots");            
            LoadAndAssignSkills();

            //после того, как назначены все роли и их скиллы, загрузка и назначение Экстр для игроков
            //некоторые скиллы влияют на экстры
            Logger.Log.Debug($"room load and assign extras to players to players");
            LoadAndAssignExtras();

            //включаем таймер смерти, если игрок молчал в чате
            Logger.Log.Debug($"room enable death timer fo players");          
            EnableDeathTimers();         

            Logger.Log.Debug($"room set phase first night");
            room.roomPhases.SetGamePhase(GamePhase.FirstNight);

            Logger.Log.Debug($"room start game timer");
            room.roomLogic.gameTimer = room.poolFiber.ScheduleOnInterval(() => room.roomPhases.GameTimer(), gameTimerTick, gameTimerTick);

            Logger.Log.Debug($"room game started ===========================");
        }

        private void AssignAuctionRolesToPlayers(/*List<RoleType> availableRoles*/)
        {
            Logger.Log.Debug($"room available roles {room.roomRoles.allRoles.Count}");

            //присваиваем роли по результату аукциона
            foreach (var s in room.roleAuction.auctionSlots)
            {
                if (s.player != null)
                {
                    var auctionRole = RoomHelper.GetRoleByType(s.role);
                    s.player.SetupRole(auctionRole);

                    if (room.roomRoles.allRoles[s.role] > 1)
                    {
                        room.roomRoles.allRoles[s.role]--;
                       
                    }
                    else //(room.roomRoles.allRoles[s.role] > 1)
                    {
                        room.roomRoles.allRoles.Remove(s.role);
                    }

                    Logger.Log.Debug($"assign player role from auc {s.role}");
                }
            }
        }

        private void AssignRandomAvailableRolesToPlayers(/*List<RoleType> availableRoles*/)
        {
            foreach (var p in room.players.Values)
            {
                if (p.playerRole == null)
                {
                    var dice = room.dice.Next(room.roomRoles.allRoles.Count);

                    var role = room.roomRoles.allRoles.ElementAt(dice).Key;

                    p.SetupRole(RoomHelper.GetRoleByType(role));

                    if (room.roomRoles.allRoles[role] > 1)
                    {
                        room.roomRoles.allRoles[role]--;
                    }
                    else// (room.roomRoles.allRoles[role] > 1)
                    {                      
                        room.roomRoles.allRoles.Remove(role);
                    }

                    Logger.Log.Debug($"assign player role free {role}");
                }
            }
        }

        private void SendPersonalRoleToPlayers()
        {
            foreach (var p in room. players.Values)
            {
                if (p.playerType != PlayerType.Player) continue;

                OperationResponse resp = new OperationResponse((byte)Request.SetPlayerRole);
                resp.Parameters = new Dictionary<byte, object>();

                resp.Parameters.Add((byte)Params.RoleId, p.playerRole.roleType);
                p.client.SendOperationResponse(resp, Options.sendParameters);

                room.roomChat.Role_PersonalMessage(
                    p, p.playerRole.roleType,
                    $"{p.GetColoredName()}, Ваша роль - {p.GetColoredRole()}!");
            }
        }

        private void SendPlayersAndRoles()
        {
            ///собираем общую информацию об игроках в комнате
            var playersData = new Dictionary<long, object>();
            var rolesData = new Dictionary<byte, object>();
            byte roleCounter = 0;
            //ники игроков
            foreach (var p in room.players.Values)
            {
                var player = new Dictionary<byte, object>();
                player.Add((byte)Params.UserName, p.playerName);
                playersData.Add(p.playerId, player);

                var roleData = DBManager.Inst.LoadRoleData(p.playerRole.roleType);

                if (roleData.Count == 0) Logger.Log.Debug($"no role data for {p.playerRole.roleType}");

                rolesData.Add(roleCounter++, roleData);
            }

            //передаем все клиентам перечень ников и айди игроков и список всех ролей в комнате
            EventData eventData = new EventData((byte)Events.StartGame);
            eventData.Parameters = new Dictionary<byte, object>();

            eventData.Parameters.Add((byte)Params.Roles, rolesData);
            eventData.Parameters.Add((byte)Params.Players, playersData);

            eventData.SendTo(room.clients, Options.sendParameters);
        }

        private void AssignPlayersToChats()
        {
            //чат мафии и пиратов добавляется при вступлении в соотв команду
            //RoomHelper.AddPlayerToTeam();

            foreach(var p in room.players.Values)
            {
                //добавляем всех игроков в общий чат
                room.roomChat.chats[ChatId.General].AddPlayerToChat(p);

                //добавляем игроков в чат по ролям
                if(p.playerRole.roleType == RoleType.Saint)
                {
                    room.roomChat.chats[ChatId.SaintRole].AddPlayerToChat(p);
                }
            }
        }

        private void AssignPlayersToTeams()
        {
            //распределение игроков по командам, в зависимости от их роли
            foreach (var p in room.players.Values)
            {
                room.roomRoles.AssignPlayerToTeam(p);
            }

            Logger.Log.Debug
             ($"good {room.roomRoles.goodTeam.players.Count} \n" +
             $" bad {room.roomRoles.badTeam.players.Count} \n" +
             $" neutral {room.roomRoles.neutralTeam.players.Count} \n" +
             $" pirates {room.roomRoles.pirateTeam.players.Count}");
        }

        private void AssignRolesToPlayers(Dictionary<byte, object> parameters)
        {
            var players = (Dictionary<long, int>)parameters[(byte)Params.Players];
            foreach (var p in players)
            {
                var playerId = p.Key;
                var roleId = (RoleType)p.Value;

                Logger.Log.Debug($"set role for player {playerId} => {roleId}");

                var roomPlayer = room.players[playerId];

                roomPlayer.SetupRole(RoomHelper.GetRoleByType(roleId));
            }
        }

        private void AssignRolesToBots(Dictionary<byte, object> parameters)
        {
            Logger.Log.Debug($"room assign bots roles from room builder");
            var botRoles = (Dictionary<int, int>)parameters[(byte)Params.Roles];

            foreach (var p in room.players.Values)
            {
                if (p.playerType == PlayerType.Bot)
                {
                    if (botRoles.Count > 0)
                    {
                        var element = botRoles.ElementAt(0);

                        p.SetupRole(RoomHelper.GetRoleByType((RoleType)element.Value));
                        botRoles.Remove(element.Key);
                    }
                    else
                    {
                        p.SetupRole(RoomHelper.GetRoleByType(RoleType.Citizen));
                    }
                }
            }
        }

        private void ResetPlayersToDefaultSettings()
        {
            foreach (var player in room.players.Values)
            {
                player.ResetPlayer_StartGame();
            }
        }

        private void EnableDeathTimers()
        {
            foreach (var p in room.players.Values)
            {
                if (p.playerType == PlayerType.Player)
                {
                    //проверка на скил "молчун"
                    if (p.playerRole.Check_SkillAntiTalk()) continue;

                    p.playerRole.roleEffects.chatTimer = room.poolFiber.ScheduleOnInterval(
                        () => p.playerRole.roleEffects.ChatTimer(), 1000, 1000);
                }
            }
        }

        private void LoadAndAssignSkills()
        {
            foreach (var p in room.players.Values)
            {
                if (p.playerType == PlayerType.Player)
                {
                    var playerSkills = DBManager.Inst.LoadUserSkillsByRole(p.client, p.playerRole.roleType);
                    p.playerRole.SetSkills(playerSkills);
                }

                if (p.playerType == PlayerType.Bot)
                {
                    var playerSkills = DBManager.Inst.LoadSkillsByRole(p.playerRole.roleType);
                    p.playerRole.SetSkills(playerSkills);
                }
            }
        }

        private void LoadAndAssignExtras()
        {
            foreach (var p in room.players.Values)
            {
                if (p.playerType == PlayerType.Player)
                {                    
                    //загружаем из бд слоты игрока
                    var userExtraSlots = DBManager.Inst.LoadUserExtraSlotsForGame(p.client);

                    p.SetupGameExtraSlots(userExtraSlots);
                }
            }
        }
    }
}
