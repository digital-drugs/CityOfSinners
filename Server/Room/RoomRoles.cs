using ExitGames.Client.Photon.LoadBalancing;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventData = Photon.SocketServer.EventData;

namespace Mafia_Server
{
    public class RoomRoles
    {
        private Room room;

        Random dice = new Random();

        //public RoomRoles(Room room, Dictionary<byte, object> parameters = null)
        //{
        //    this.room = room;

        //    CreateTeams();

        //    //CreateGroups();

        //    if (parameters != null)
        //    {
        //        //var rolesType = parameters[(byte)Params.Roles];

        //        //Logger.Log.Debug($"roles type {rolesType.GetType()}");

        //        var roles = (Dictionary<int, int>)parameters[(byte)Params.Roles];

        //        foreach (var role in roles.Values)
        //        {
        //            var roleType = (RoleType)role;

        //            if (RoomHelper.RoleIsBad(roleType))
        //            {
        //                listAvailableBadRoles.Add(roleType);
        //            }

        //            if (RoomHelper.RoleIsGood(roleType))
        //            {
        //                listAvailableGoodRoles.Add(roleType);
        //            }
        //        }

        //        var offset = room.roomConfig.playerLimit - roles.Count;

        //        for (int i = 0; i < offset; i++)
        //        {
        //            listAvailableGoodRoles.Add(RoleType.Citizen);
        //        }

        //        Logger.Log.Debug($"goo => {listAvailableGoodRoles.Count} bad => {listAvailableBadRoles.Count}");

        //        return;
        //    }

        //    //определяем кол-во злых ролей в комнате
        //    var badLimit = 0;

        //    var ratioDice = dice.Next(100);

        //    if (ratioDice < 50)
        //    {
        //        badLimit = 3;
        //    }
        //    else if (ratioDice < 70)
        //    {
        //        badLimit = 4;
        //    }
        //    else if (ratioDice < 90)
        //    {
        //        badLimit = 2;
        //    }
        //    else
        //    {
        //        badLimit = 1;
        //    }

        //    Logger.Log.Debug($"dice {ratioDice} bad limit {badLimit}");

        //    if (GameRoomOptions.testMode)
        //    {

        //        //listAvailableGoodRoles.Add(RoleType.Commissar);

        //        listAvailableBadRoles.Add(RoleType.Maniac);

        //        for (int i = 0; i < 7; i++)
        //        {
        //            listAvailableGoodRoles.Add(RoleType.Citizen);
        //        }

        //        //for (int i = 0; i < 7; i++)
        //        //{
        //        //    listAvailableBadRoles.Add(RoleType.Sinner);
        //        //}
        //        //listAvailableGoodRoles.Add(RoleType.Commissar);


        //        //for (int i = 0; i < 7; i++)
        //        //{
        //        //    listAvailableBadRoles.Add(RoleType.Mafia);
        //        //}               
        //    }
        //    else
        //    {
        //        CalculateBadRoles(badLimit);
        //        CalculateGoodRoles(badLimit);
        //    }
        //}

        public RoomRoles(Room room)
        {
            this.room = room;

            CreateTeams();

            //создаем доступные добрые роли
            //комиссар будет добавлен принудительно при распределнии ролей
            //availableGoodRoles.Add(RoleType.Commissar, 1);
            availableGoodRoles.Add(RoleType.Doctor, 1);
            availableGoodRoles.Add(RoleType.Citizen, room.config.playerLimit);
            availableGoodRoles.Add(RoleType.Guerilla, 1);
            availableGoodRoles.Add(RoleType.Witness, 1);
            availableGoodRoles.Add(RoleType.Saint, 1);

            //если идет соотв ивент
            {

            }

            //создаем доступные злые роли
            availableBadRoles.Add(RoleType.Maniac, 1);
            availableBadRoles.Add(RoleType.Werewolf, 1);
            availableBadRoles.Add(RoleType.MafiaBoss, 1);
            availableBadRoles.Add(RoleType.Mafia, room.config.mafiaLimit);
            availableBadRoles.Add(RoleType.Sinner, 1);

            //если идет соотв ивент
            {

            }           

            //определяем кол-во злых ролей в комнате
            var badLimit = 0;            

            var ratioDice = dice.Next(100);

            if (ratioDice >= 0 && ratioDice < 10)
            {
                switch (room.config.roomType)
                {
                    case RoomType._8: badLimit = 1; break;
                    case RoomType._12: badLimit = 2; break;
                    case RoomType._16: badLimit = 2; break;
                    case RoomType._20: badLimit = 3; break;
                }                
            }

            if (ratioDice >= 10 && ratioDice < 30)
            {
                switch (room.config.roomType)
                {
                    case RoomType._8: badLimit = 2; break;
                    case RoomType._12: badLimit = 3; break;
                    case RoomType._16: badLimit = 4; break;
                    case RoomType._20: badLimit = 5; break;
                }
            }
          
            if (ratioDice >= 50 && ratioDice < 100)
            {
                switch (room.config.roomType)
                {
                    case RoomType._8: badLimit = 3; break;
                    case RoomType._12: badLimit = 5; break;
                    case RoomType._16: badLimit = 6; break;
                    case RoomType._20: badLimit = 8; break;
                }
            }

            if (ratioDice >= 30 && ratioDice < 50)
            {
                switch (room.config.roomType)
                {
                    case RoomType._8: badLimit = 4; break;
                    case RoomType._12: badLimit = 6; break;
                    case RoomType._16: badLimit = 8; break;
                    case RoomType._20: badLimit = 10; break;
                }
            }

            Logger.Log.Debug($"dice {ratioDice} bad limit {badLimit}");

            CalculateBadRoles(badLimit);
            CalculateGoodRoles(badLimit);

            Logger.Log.Debug($"room all roles ******************");
            foreach (var r in allRoles)
            {
                Logger.Log.Debug($"role => {r.Key} => {r.Value}");
            }
            Logger.Log.Debug($"******************");
        }

        public Team goodTeam { get; private set; }
        public Team badTeam { get; private set; }
        public Team neutralTeam { get; private set; } 
        public Team pirateTeam { get; private set; } 
        private void CreateTeams()
        {
            goodTeam = new Team(room, TeamType.Good);

            badTeam = new Team(room, TeamType.Bad);

            neutralTeam = new Team(room, TeamType.Neutral);

            pirateTeam = new Team(room, TeamType.Pirate);
        }

        public Dictionary<RoleType, int> allRoles { get; private set; } = new Dictionary<RoleType, int>();

        private Dictionary<RoleType, int> availableBadRoles = new Dictionary<RoleType, int>();
        private void CalculateBadRoles(int badLimit)
        {
            if (badLimit == 1)
            {
                //roomBadRoles.Add(RoleType.Maniac, 1);
                availableBadRoles.Remove(RoleType.Maniac);
                allRoles.Add(RoleType.Maniac, 1);

                return;
            }

            int roleDice = 0;

            if (badLimit > 1)
            {
                for (int i = 0; i < badLimit; i++)
                {
                    roleDice = dice.Next(availableBadRoles.Count);

                    var randomBadRole = availableBadRoles.ElementAt(roleDice).Key;

                    if (allRoles.ContainsKey(randomBadRole))
                    {
                        allRoles[randomBadRole]++;
                    }
                    else
                    {
                        allRoles.Add(randomBadRole, 1);
                    }

                    if (availableBadRoles[randomBadRole] > 1)
                    {
                        availableBadRoles[randomBadRole]--;
                    }
                    else
                    {
                        availableBadRoles.Remove(randomBadRole);
                    }                  
                }             
            }

            Logger.Log.Debug($"room bad roles ******************");
            foreach (var r in allRoles)
            {
                Logger.Log.Debug($"room bad role => {r.Key} => {r.Value}");
            }
            Logger.Log.Debug($"******************");
        }

        private Dictionary<RoleType, int> availableGoodRoles = new Dictionary<RoleType, int>();
        private void CalculateGoodRoles(int badLimit)
        {
            //в комнате ВСЕГДА должен быть комиссар
            allRoles.Add(RoleType.Commissar, 1);

            //если активен соотв ивент
            {
                //обязательно добавить космонавта
                {
                    //AddForcedRole(RoleType.Astronaut);
                    //goodLimit--;
                }
            }

            //заполняем оставшиеся слоты в комнате            
            //доступное кол-во добрых слотов = общее кол-во игроков в комнате - кол-во выданных злых ролей - 1 комиссар

            var allRolesCount = 0;

            foreach(var r in allRoles)
            {
                allRolesCount += r.Value;
            }

            var goodLimit = room.config.playerLimit - allRolesCount;

            Logger.Log.Debug($"gl {goodLimit} pl {room.config.playerLimit} brc {allRolesCount} ");

            //количество добавленных в комнату святых
            //var saintCount = 0;

            int roleDice = 0;

            for (int i = 0; i < goodLimit; i++)
            {
                RoleType randomGoodRole = RoleType.Citizen;

                if (availableGoodRoles.Count > 0)
                {
                    roleDice = dice.Next(availableGoodRoles.Count);

                    randomGoodRole = availableGoodRoles.ElementAt(roleDice).Key;
                }

                if (allRoles.ContainsKey(randomGoodRole))
                {
                    allRoles[randomGoodRole]++;
                }
                else
                {
                    allRoles.Add(randomGoodRole, 1);
                }

                if (availableGoodRoles[randomGoodRole] > 1)
                {
                    availableGoodRoles[randomGoodRole]--;
                }
                else
                {
                    availableGoodRoles.Remove(randomGoodRole);
                }
            }
        }

        public void AssignPlayerToTeam(BasePlayer player)
        {
            switch (player.playerRole.roleType)
            {
                case RoleType.Citizen:
                case RoleType.Doctor:
                case RoleType.Guerilla:
                case RoleType.Saint:
                case RoleType.Witness:
                case RoleType.Commissar:
                case RoleType.GoodClown:
                case RoleType.Astronaut:
                    {
                        RoomHelper.AddPlayerToTeam(room, player, TeamType.Good);
                    } break;

                case RoleType.Mafia:
                case RoleType.MafiaBoss:
                case RoleType.Sinner:
                    {
                        RoomHelper.AddPlayerToTeam(room, player, TeamType.Bad);
                    } break;

                case RoleType.Maniac:
                case RoleType.Werewolf:
                case RoleType.BadClown:
                case RoleType.Alien:
                case RoleType.Scientist:
                    {
                        RoomHelper.AddPlayerToTeam(room, player, TeamType.Neutral);
                    }
                    break;

                case RoleType.Jane:
                case RoleType.Garry:
                case RoleType.Loki:
                    {
                        RoomHelper.AddPlayerToTeam(room, player, TeamType.Pirate);
                    } break;
            }

           
        }
    }

   
}
