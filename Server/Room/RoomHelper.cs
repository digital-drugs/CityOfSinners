using ExitGames.Client.Photon.LoadBalancing;
using MySqlX.XDevAPI.Common;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Mafia_Server
{
    public class RoomHelper
    {
        public static BasePlayer FindPlayerByRole(RoleType roleType, Room room)
        {
            BasePlayer result = null;

            foreach (var p in room.players.Values)
            {
                if(p.playerRole.roleType== roleType && p.isLive())
                {
                    return p;
                }
            }
            
            return result;
        }

        public static List< BasePlayer> FindPlayersByRole(RoleType roleType, Room room)
        {
            var result = new List<BasePlayer>();

            foreach (var p in room.players.Values)
            {
                if (p.playerRole.roleType == roleType && p.isLive())
                {
                    result.Add(p);
                }
            }

            return result;
        }

        public static void AddPlayerToTeam(Room room, BasePlayer player, TeamType teamId)
        {
            //сначала выгоняем игрока из старой команды
            RemovePlayerFromTeam(room, player);

            Team targetTeam = null;

            switch (teamId)
            {
                case TeamType.Good: 
                    { 
                        targetTeam = room.roomRoles.goodTeam;
                        //добавляем игрока в новую команду
                        targetTeam.AddPlayer(player);
                        //запоминаем за игроком его команду
                        player.SetTeam(targetTeam);
                    } break;

                case TeamType.Bad: 
                    { 
                        targetTeam = room.roomRoles.badTeam;
                        //если игрок присоединился к команде мафии, добавляем его в чат мафии
                        room.roomChat.chats[ChatId.BadTeam].AddPlayerToChat(player);
                        //добавляем игрока в новую команду
                        targetTeam.AddPlayer(player);
                        //запоминаем за игроком его команду
                        player.SetTeam(targetTeam);
                        //если игрок присоединился к команде мафии, обновляем состав команды в клиентах
                        targetTeam.UpdateCompound();
                    }
                    break;

                case TeamType.Neutral: 
                    { 
                        targetTeam = room.roomRoles.neutralTeam;
                        //добавляем игрока в новую команду
                        targetTeam.AddPlayer(player);
                        //запоминаем за игроком его команду
                        player.SetTeam(targetTeam);
                    } break;

                case TeamType.Pirate: 
                    { 
                        targetTeam = room.roomRoles.pirateTeam;
                        //если игрок присоединился к команде пиратов, добавляем его в чат пиратов
                        room.roomChat.chats[ChatId.PiratTeam].AddPlayerToChat(player);
                        //добавляем игрока в новую команду
                        targetTeam.AddPlayer(player);
                        //запоминаем за игроком его команду
                        player.SetTeam(targetTeam);
                        //если игрок присоединился к команде мафии, обновляем состав команды в клиентах
                        targetTeam.UpdateCompound();
                    }
                    break;
            }
        }

        /// <summary>
        /// удаляем игрока из его старой команды
        /// </summary>
        /// <param name="room"></param>
        /// <param name="player"></param>
        /// <param name="teamId"></param>
        public static void RemovePlayerFromTeam(Room room, BasePlayer player)
        {
            if (player.team == null) return;

            //определяем команду, в которой состоит игрок
            Team team = player.team;

            //удаляем игрока из его старой команды
            team.RemovePlayer(player);

            //обнуляем команду игрока
            player.SetTeam(null);

            //чаты          
            switch (team.teamType)
            {
                //если игрок покинул команду мафии, удаляем его из чата мафии
                case TeamType.Bad: { room.roomChat.chats[ChatId.BadTeam].RemovePlayerFromChat(player); } break;
                //если игрок покинул команду пиратов, удаляем его из чата пиратов
                case TeamType.Pirate: { room.roomChat.chats[ChatId.PiratTeam].RemovePlayerFromChat(player); } break;
            }
        }  

        public static Role GetRoleByType(RoleType roleType)
        {
            Role result = null;

            switch (roleType)
            {
                case RoleType.Citizen: { result = new Citizen(RoleType.Citizen); } break;
                case RoleType.Doctor: { result = new Doctor(RoleType.Doctor); } break;
                case RoleType.Guerilla: { result = new Guerilla(RoleType.Guerilla); } break;
                case RoleType.Saint: { result = new Saint(RoleType.Saint); } break;
                case RoleType.Witness: { result = new Witness(RoleType.Witness); } break;
                case RoleType.Commissar: { result = new Commissar(RoleType.Commissar); } break;
                case RoleType.Mafia: { result = new Mafia(RoleType.Mafia); } break;
                case RoleType.MafiaBoss: { result = new MafiaBoss(RoleType.MafiaBoss); } break;
                case RoleType.Maniac: { result = new Maniac(RoleType.Maniac); } break;
                case RoleType.Sinner: { result = new Sinner(RoleType.Sinner); } break;
                case RoleType.Werewolf: { result = new Werewolf(RoleType.Werewolf); } break;

                case RoleType.Jane: { result = new Jane(RoleType.Jane); } break;
                case RoleType.Garry: { result = new Garry(RoleType.Garry); } break;
                case RoleType.Loki: { result = new Loki(RoleType.Loki); } break;

                case RoleType.Alien: { result = new Alien(RoleType.Alien); } break;
                case RoleType.Astronaut: { result = new Astronaut(RoleType.Astronaut); } break;

                case RoleType.BadClown: { result = new BadClown(RoleType.BadClown); } break;
                case RoleType.GoodClown: { result = new GoodClown(RoleType.GoodClown); } break;

                case RoleType.Scientist: { result = new Scientist(RoleType.Scientist); } break;
            }

            return result;
        }      


        public static bool RoleIsBad(RoleType role)
        {
            switch (role)
            {
                case RoleType.Mafia:
                case RoleType.Maniac:
                case RoleType.Werewolf:
                case RoleType.MafiaBoss:
                case RoleType.Jane:
                case RoleType.Loki:
                case RoleType.Garry:
                case RoleType.Sinner: return true;
            }
            return false;
        }

        public static bool RoleIsGood(RoleType role)
        {
            switch (role)
            {
                case RoleType.Citizen:
                case RoleType.Doctor:
                case RoleType.Guerilla:
                case RoleType.Saint:
                case RoleType.Witness:
                case RoleType.Commissar: return true;
            }
            return false;
        }

      

        public static string GetRoleActionString(RoleType roleType)
        {
            var result = "";

            var roleRus = Helper.GetRoleNameById_Rus(roleType);

            switch (roleType)
            {
                case RoleType.Citizen: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Doctor: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Guerilla: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Saint: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Witness: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Commissar: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Mafia: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.MafiaBoss: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Maniac: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Sinner: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Werewolf: { result = $"{roleRus} сделал ночной ход"; } break;

                case RoleType.Jane: { result = $"{roleRus} сделала ночной ход"; } break;
                case RoleType.Garry: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Loki: { result = $"{roleRus} сделал ночной ход"; } break;

                case RoleType.Alien: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.Astronaut: { result = $"{roleRus} сделал ночной ход"; } break;

                case RoleType.BadClown: { result = $"{roleRus} сделал ночной ход"; } break;
                case RoleType.GoodClown: { result = $"{roleRus} сделал ночной ход"; } break;

                case RoleType.Scientist: { result = $"{roleRus} сделал ночной ход"; } break;
            }

             return result;
        }

        /// <summary>
        /// поиск соседних с целью игроков
        /// </summary>
        /// <param name="room">комната в которой ищем</param>
        /// <param name="player">владелец</param>
        /// <param name="targetPlayer">цель</param>
        /// <param name="count">кол-во соседних игроков</param>
        /// <param name="matchTeam">true исключает свою команду из списка</param>
        /// <param name="matchSelf">true исключает себя из списка</param>
        public static List<BasePlayer> FindNearPlayers(
            Room room, 
            BasePlayer player, BasePlayer targetPlayer, 
            int count, 
            bool matchTeam, bool matchSelf)
        {
            //получаем список активных игроков
            var activePlayers = new List<BasePlayer>();
            foreach(var p in room.players.Values)
            {
                if (p.isLive())
                {
                    activePlayers.Add(p);
                }
            }
          
            //
            List<BasePlayer> targetPlayerList = new List<BasePlayer>();          

            var ownerIndex = activePlayers.IndexOf(player);
            var targetIndex = activePlayers.IndexOf(targetPlayer);

            Logger.Log.Debug($"own index => {ownerIndex} target index => {targetIndex}");

            var upIndex = targetIndex;
            var downIndex = targetIndex;           

            for (int i = 0; i < activePlayers.Count; i++)
            {
                Logger.Log.Debug($" i % 2 => {i%2}");

                if (i % 2 == 0)
                {
                    upIndex--;

                    if (upIndex < 0) upIndex = activePlayers.Count - 1;

                    if (activePlayers[upIndex] == targetPlayer) continue;

                    if (matchSelf && activePlayers[upIndex] == player) continue;                    

                    if (matchTeam && activePlayers[upIndex].team == player.team) continue;

                    if(targetPlayerList.Count< count)
                    {
                        targetPlayerList.Add(activePlayers[upIndex]);
                        //Logger.Log.Debug($"{upIndex} => {}");
                    }

                    if (targetPlayerList.Count == count) break;
                }
                else
                {
                    downIndex++;

                    if (downIndex == activePlayers.Count) downIndex = 0;

                    if (activePlayers[downIndex] == targetPlayer) continue;

                    if (matchSelf && activePlayers[downIndex] == player) continue;                  

                    if (matchTeam && activePlayers[downIndex].team == player.team) continue;

                    if (targetPlayerList.Count < count)
                    {
                        targetPlayerList.Add(activePlayers[downIndex]);
                    }

                    if (targetPlayerList.Count == count) break;
                }

                //Logger.Log.Debug($"{upIndex} / {downIndex}");
            }

            return targetPlayerList;
        }
    }
}
