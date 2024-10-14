using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public static class RoleHelper
    {

        private static Dictionary<RoleType, Dictionary<byte, object>> roleDatas;

        public static void LoadRoles()
        {
            roleDatas = new Dictionary<RoleType, Dictionary<byte, object>>();

            foreach (var element in Enum.GetValues(typeof( RoleType)))
            {             
                var roleType = (RoleType)element;

                if (roleType == RoleType.NULL) continue;
                    
                var roleData = DBManager.Inst.LoadRoleData(roleType);

                roleDatas.Add(roleType, roleData);
            }          
        }


        public static void ChangePlayerRole(BasePlayer player, RoleType newRoleType)
        {
            //запоминаем старую роль (может пригодиться на подведении итогов ночи)
            if (player.oldRole == null)
            {
                player.SetOldRole(player.playerRole);
            }

            var oldRoleType = player.playerRole.roleType;           

            var newRole = RoomHelper.GetRoleByType(newRoleType);

            newRole.SetRoleEffects(player.playerRole.roleEffects);          

            player.SetupRole(newRole);

            Logger.Log.Debug($"{player.playerName} set new role {newRoleType}");

            if (player.playerType == PlayerType.Player)
            {
                var playerSkills = DBManager.Inst.LoadUserSkillsByRole(player.client, newRoleType);

                newRole.SetSkills(playerSkills);
            }

            var newRoleData = roleDatas[newRoleType];

            //правка портрета роли в "столбиках ролей" составах команды
            EventData eventData = new EventData((byte)Events.ChangePlayerRole);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.OldRole , oldRoleType);
            eventData.Parameters.Add((byte)Params.NewRole, newRoleData);
            eventData.SendTo(player.GetRoom().clients, Options.sendParameters);

            //правка портрета роли в фишке игрока
            if (player.client != null)
            {
                OperationResponse resp = new OperationResponse((byte)Request.ChangePlayerRole);
                resp.Parameters = new Dictionary<byte, object>();

                resp.Parameters.Add((byte)Params.UserId, player.playerId);
                resp.Parameters.Add((byte)Params.NewRole, newRoleData);

                player.client.SendOperationResponse(resp, Options.sendParameters);
            }         

            //отключение чатов
            //если игрок был святым
            if(oldRoleType == RoleType.Saint)
            {
                //убираем игрока из чата святых
                player.GetRoom().roomChat.chats[ChatId.SaintRole].RemovePlayerFromChat(player);
            }

            //включение чатов
            if (newRoleType == RoleType.Saint)
            {
                //убираем игрока из чата святых
                player.GetRoom().roomChat.chats[ChatId.SaintRole].AddPlayerToChat(player);
            }

            //правка портрета роли в фишках команды игрока
        }

        public static void ApplyRoleEffectForced(Room room, BasePlayer player, DurationType durationType, BasePlayer targetPlayer)
        {
            targetPlayer.playerRole.roleEffects.AddRoleEffect(player.playerRole);

            //Logger.Log.Debug($"do night action {player.playerRole.GetType()}");

            var newRoleHandler = new RoleHandler(targetPlayer, player.playerRole, room);

            targetPlayer.playerRole.roleEffects.AddRoleHandler(newRoleHandler);

            newRoleHandler.Subscribe(durationType);
        }

        public static void UnlockRole_PlayerToPlayer(
            BasePlayer fromPlayer, 
            BasePlayer toPlayer, 
            RoleType roleId = RoleType.NULL)
        {
            if (toPlayer.client == null) return;

            if (roleId == RoleType.NULL)
            {
                roleId = fromPlayer.playerRole.roleType;
            }

            OperationResponse resp = new OperationResponse((byte)Request.UnlockRole_PlayerToPlayer);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.UserId, fromPlayer.playerId);
            resp.Parameters.Add((byte)Params.RoleId, roleId);
            toPlayer.client.SendOperationResponse(resp, Options.sendParameters);
        }

        public static void UnlockRole_GroupToPlayer(List<BasePlayer> playersGroup, BasePlayer player)
        {
            if (player.client == null) return;

            var roles = new Dictionary<long, RoleType>();

            foreach(var p in playersGroup)
            {
                roles.Add(p.playerId, p.playerRole.roleType);
            }

            OperationResponse resp = new OperationResponse((byte)Request.UnlockRole_GroupToPlayer);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.Roles, roles);
            player.client.SendOperationResponse(resp, Options.sendParameters);
        }       

        public static void UnlockRole_PlayerToGroup(List<BasePlayer> playersGroup, BasePlayer player, RoleType forcedRole = RoleType.NULL)
        {
            if (forcedRole == RoleType.NULL)
            {
                forcedRole = player.playerRole.roleType;
            }

            var clients = new List<Client>();

            foreach(var p in playersGroup)
            {
                if (p.client != null)
                {
                    clients.Add(p.client);
                }
            }

            //отправляем ивент о раскрытии роли игрока
            EventData eventData = new EventData((byte)Events.UnlockRole_PlayerToGroup);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, player.playerId);
            eventData.Parameters.Add((byte)Params.RoleId, forcedRole);
            eventData.SendTo(clients, Options.sendParameters);
        }

        public static void UnlockRole_PlayerToRoom(BasePlayer player, RoleType forcedRole = RoleType.NULL)
        {
            if (forcedRole == RoleType.NULL)
            {
                forcedRole = player.playerRole.roleType;
            }

            //отправляем ивент о раскрытии роли игрока
            EventData eventData = new EventData((byte)Events.UnlockRole_PlayerToRoom);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, player.playerId);
            eventData.Parameters.Add((byte)Params.RoleId, forcedRole);
            eventData.SendTo(player.GetRoom().clients, Options.sendParameters);
        }

    }
}
