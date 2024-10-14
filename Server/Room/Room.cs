using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Concurrency.Fibers;
using Mafia_Server.Extras;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;
using EventData = Photon.SocketServer.EventData;

///TODO
///добавить счетчик состава команд по колву игроков
///передавать на клиент изменение счетчика

///добавить enum сообщений для чата
///реализовать диспос комнаты

///уточнить какую информацию получают роли за свои действия по результату проверки

//сделать лиги. сделать проверку лиг и наград при завершении игры
//обавить в мафию убийство игрока если тот ничего не пишет более 90 секунд. ". ник игрока. убит своим молчанием"
// суицид за 1000 коинов, досрочный выход из игры.
//если игрок делает суицид, у него отнимается рейтинг после окончания этой игры как за проигрыш этой роли, даже если его команда победила.

//добавить джейн в обработку экстры кипяток

///сделать смену ролей. (свидетель сила воли)
///добавить эфект скилла гражданина на маньяка(оглушение)
///добавить эффект тумана на игрока
///переделать работу рации при ночном ходе
///поправить уровни скиллов

namespace Mafia_Server
{
    public class Room
    {
        public BasePlayer roomOwner;

        public RoleAuction roleAuction;

        public RoomChat roomChat { get; private set; }

        public RoomRoles roomRoles { get; private set; }

        public RoomStartGame roomStartGame { get; private set; }

        public RoomPhases roomPhases { get; private set; }
        public RoomLogic roomLogic { get; private set; }

        public RoomBots roomBots { get; private set; }

        public RoomJudging roomJudging { get; private set; }

        public RoomEndGame roomEndGame { get; private set; }

        public ExtraHelper extraHelper { get; private set; }    

      
     
        public RoomConfig config;

        public Random dice = new Random();

        public int bet=100;

        private int chatId;
        public int GetChatId()
        {
            return chatId++;
        }

        private int shareMessageId;
        public int GetShareMessageId()
        {
            return shareMessageId++;
        }

        private int extraEffectId;
        public int GetExtraEffectId()
        {
            return extraEffectId++;
        }

        private int skillEffectId;
        public int GetSkillEffectId()
        {
            return skillEffectId++;
        }

        public int id { get; private set; }

        public Room(int id, Player player, RoomConfig roomConfig)
        {
            this.id = id;

            poolFiber.Start();

            this.config = roomConfig;

            roleAuction = new RoleAuction(this);

            roomChat = new RoomChat(this);

            roomRoles=new RoomRoles(this);

            roomStartGame = new RoomStartGame(this);

            roomPhases=new RoomPhases(this);

            roomLogic = new RoomLogic(this);

            roomJudging = new RoomJudging(this);

            roomBots = new RoomBots(this);

            roomEndGame = new RoomEndGame(this);

            extraHelper = new ExtraHelper(this);

            roomOwner =player;          

            //AddPlayer(player);          

            SetRoomState(RoomState.WaitPlayers);
            //SetRoomState(RoomState.Auction);
        }

        private Dictionary<byte, object> parameters;
        public Room(int id,Player player, Dictionary<byte, object> parameters)
        {
            this.id = id;

            this.parameters = parameters;

            poolFiber.Start();

            //var playerCount = (int)parameters[(byte)Params.PlayerCount];

            //config = new RoomConfig(playerCount, 0, 0);

            //roleAuction = new RoleAuction(this, parameters);

            roomChat = new RoomChat(this);

            //roomRoles = new RoomRoles(this);

            roomStartGame = new RoomStartGame(this);

            roomPhases = new RoomPhases(this);

            roomLogic = new RoomLogic(this);

            roomJudging = new RoomJudging(this);

            roomBots = new RoomBots(this);

            roomEndGame = new RoomEndGame(this);

            extraHelper = new ExtraHelper(this);

            roomOwner = player;

            //AddPlayer(player);

            //waitPlayersTime = (int)parameters[(byte)Params.Timer];

            SetRoomState(RoomState.WaitPlayers);
        }

        private bool useLobby=false;
        public Room(int id)
        {         
            this.id = id;

            useLobby = true;

            poolFiber.Start();

            roomChat = new RoomChat(this);

            roomRoles = new RoomRoles(this);

            roomStartGame = new RoomStartGame(this);

            roomPhases = new RoomPhases(this);

            roomLogic = new RoomLogic(this);

            roomJudging = new RoomJudging(this);

            roomBots = new RoomBots(this);

            roomEndGame = new RoomEndGame(this);

            extraHelper = new ExtraHelper(this);           

            SetRoomState(RoomState.WaitPlayers);

            useLobby = true;
        }

        public void SetOwner(Player player)
        {
            roomOwner = player;

            //AddPlayer(player);
        }

        public int testFNightDuration;
        public int testDayDuration;
        public int testNightDuration;
        public int testJudgeDuration;
        public List<RoleType> testSkill100 = new List<RoleType>();
        public void StartGameFromRoomBuilder(Dictionary<byte, object> parameters)
        {
            var playerCount = (int)parameters[(byte)Params.PlayerCount];

            config = new RoomConfig(RoomType._8, playerCount, 0, 0, 0);

            testFNightDuration = (int)parameters[(byte)Params.FNight];
            testDayDuration = (int)parameters[(byte)Params.Day];
            testNightDuration = (int)parameters[(byte)Params.Night];
            testJudgeDuration = (int)parameters[(byte)Params.Judge];

            Logger.Log.Debug($"fn {testFNightDuration} d {testDayDuration} n {testNightDuration}");

            roomBots.botUseExtra = (bool)parameters[(byte)Params.BotUseExtra];
            roomBots.botUseVote = (bool)parameters[(byte)Params.BotUseVote];
            roomBots.botUseVisit = (bool)parameters[(byte)Params.BotUseVisit];

            Logger.Log.Debug($"e {roomBots.botUseExtra} vo {roomBots.botUseVote} vi {roomBots.botUseVisit}");

            var skills100 = (Dictionary<byte, object>)parameters[(byte)Params.Skill100];

            foreach (var s in skills100)
            {
                var roleType = (RoleType)s.Value;
                testSkill100.Add(roleType);

                Logger.Log.Debug($"100% skill {roleType}");
            } 

            roomStartGame.StartGame(parameters); 
        }

        public bool useExtras { get; private set; } = false;
        public void StartGameFromLobby(Lobby lobby)
        {
            //получаем конфиг комнаты из кол-ва игроков
            config = ManagerRooms.instance.GetRoomConfigById(lobby.roomType);

            Logger.Log.Debug($"lobby.roomType {lobby.roomType}");

            if(config == null)
            {
                Logger.Log.Debug($"config not found");
            }

            //Logger.Log.Debug($"found room config {config.availablesRoles.Count}");
            Logger.Log.Debug($"config player limit {config.playerLimit}");
            Logger.Log.Debug($"config bad limit {config.maximumBadLimit}");
            Logger.Log.Debug($"config mafia limit {config.mafiaLimit}");

            testFNightDuration = Options.roomSettings.FirstNightDuration;
            testDayDuration = Options.roomSettings.DayDuration;
            testNightDuration = Options.roomSettings.NightDuration;
            testJudgeDuration = Options.roomSettings.JudgeDuration;

            Logger.Log.Debug($"fn {testFNightDuration} d {testDayDuration} n {testNightDuration} j {testJudgeDuration}");

            roomBots.botUseExtra = false;
            roomBots.botUseVote = false;
            roomBots.botUseVisit = false;

            //Logger.Log.Debug($"e {roomBots.botUseExtra} vo {roomBots.botUseVote} vi {roomBots.botUseVisit}");

            useExtras = lobby.useExtras;

            //настраиваем в комнате роли
            roomRoles = new RoomRoles(this);

            //запускаем аукцион
            roleAuction = new RoleAuction(this, parameters);
            roleAuction.StartAuction();

           
        }

        public Dictionary<long, BasePlayer> GetLivePlayers()
        {
            var result = new Dictionary<long, BasePlayer>();

            foreach(var p in players)
            {
                if (p.Value.isLive())
                {
                    result.Add(p.Key, p.Value);
                }
            }

            return result;
        }

        public Dictionary<long, BasePlayer> players { get; private set; } = new Dictionary<long, BasePlayer>();
        public List<Client> clients { get; private set; } = new List<Client>();

        public void AddPlayersFromLobby(Lobby lobby)
        {
            players = lobby.players;
            clients = lobby.clients;

            foreach(var p in players.Values)
            {
                //добавляем игроку эту комнату
                p.SetRoom(this);
            }
        }

        public void RemovePlayerFromGame(Player player)
        {
            players.Remove(player.client.userDbId);

            player.SetRoom( null);

            clients.Remove(player.client);

            var haveRealPlayer = false;

            foreach(var p in GetLivePlayers().Values)
            {
                if (p.playerType == PlayerType.Player)
                {
                    haveRealPlayer = true;
                }
            }

            if (!haveRealPlayer)
            {
                Dispose();
            }
        }

    

        public RoomState roomState { get; private set; }
        public PoolFiber poolFiber= new PoolFiber();
        public void SetRoomState(RoomState roomState)
        {
            this.roomState = roomState;

            switch(roomState)
            {
                case RoomState.WaitPlayers:
                    {
                        //StartWaitPlayers();
                    }
                    break;

                case RoomState.Auction:
                    {
                        roleAuction. StartAuction();
                    }
                    break;

                case RoomState.Game:
                    {
                        //roomStartGame.StartGame();
                    }
                    break;
            }
        }

        private IDisposable waitPlayersTimer;
        private void StartWaitPlayers()
        {
            waitPlayersTimer = poolFiber.ScheduleOnInterval(() => WaitPlayers(), 1000, 1000);

            Logger.Log.Debug($"waitPlayersTime {waitPlayersTime}");
        }

        private int waitPlayersTime = 10;
        private void WaitPlayers()
        {
            waitPlayersTime--;

            if (waitPlayersTime == 0)
            {
                waitPlayersTimer.Dispose();

                roleAuction.StartAuction();
            }
        }       

        public void Dispose()
        {
            poolFiber.Stop();
            poolFiber.Dispose();

            if (roomLogic.gameTimer != null) roomLogic.gameTimer.Dispose();           

            Logger.Log.Debug($"room disposed");
        }

        public bool roomIsStoped = false;
        //public void StopRoom()
        //{
        //    roomIsStoped = true;
            
        //}
    }   
}