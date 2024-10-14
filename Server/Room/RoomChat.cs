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
    public class RoomChat
    {
        private Room room;

        public RoomChat(Room room)
        {
            this.room = room;

            //создаем общий чат
            CreateChat(ChatId.General);

            //создаем чат мафии
            CreateChat(ChatId.BadTeam);

            //создаем чат пиратов
            CreateChat(ChatId.PiratTeam);

            //создаем чат святых
            CreateChat(ChatId.SaintRole);

            //создаем личные чаты
        }

        public Dictionary<ChatId, Chat> chats { get; private set; } = new Dictionary<ChatId, Chat>();

        public void CreateChat(ChatId chatId)
        {
            var chat = new Chat(chatId);

            chats.Add(chatId, chat);
        }
        public Chat GetChatById(ChatId chatId)
        {
            return chats[chatId];
        }

        private void SendResponse(BasePlayer player, Dictionary<byte, object> parameters, Request request)
        {
            var mistEffect = player.playerRole.roleEffects.FindSkillEffect(SkillEffect.SinnerMist);
            if (mistEffect != null) return;

            OperationResponse resp = new OperationResponse((byte)request);
            resp.Parameters = parameters;
            player.client.SendOperationResponse(resp, Options.sendParameters);
        }

        private void SendEvent(List< BasePlayer> players, Dictionary<byte, object> parameters, Events request)
        {
            var clientList = new List<Client>();

            foreach (var p in players)
            {
                if (p.client == null) continue;

                var mistEffect = p.playerRole.roleEffects.FindSkillEffect(SkillEffect.SinnerMist);
                if (mistEffect == null)
                {
                    clientList.Add(p.client);
                }
            }

            EventData eventData = new EventData((byte)request);
            eventData.Parameters = parameters;
            eventData.SendTo(clientList, Options.sendParameters);
        }

        private void SendEvent(Dictionary<long, BasePlayer> players, Dictionary<byte, object> parameters, Events request)
        {
            var clientList = new List<Client>();

            foreach(var p in players.Values)
            {
                if (p.client == null) continue;

                var mistEffect = p.playerRole.roleEffects.FindSkillEffect(SkillEffect.SinnerMist);
                if (mistEffect == null)
                {
                    clientList.Add(p.client);
                }
            }        

            EventData eventData = new EventData((byte)request);
            eventData.Parameters = parameters;
            eventData.SendTo(clientList, Options.sendParameters);
        }

        #region Extra

        public void Extra_PersonalMessage(BasePlayer player, Extra extra, string message, ExtraEffect effect= ExtraEffect.none)
        {
            if (player.client == null) return;

																							  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);

            if(effect == ExtraEffect.none)
            {
                parameters.Add((byte)Params.ExtraId, extra.effect);
            }
            else
            {
                parameters.Add((byte)Params.ExtraId, effect);
            }

          

            SendResponse(player, parameters, Request.SystemMessage_Extra);
        }

        public void Extra_PublicMessage(Extra extra, string message)
        {
																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.ExtraId, extra.effect);

            SendEvent(room.players, parameters, Events.RoomSystemMessage_Extra);
        }

        public void Extra_PublicMessageExcludePlayers(string message, Room room, Extra extra, BasePlayer[] players, ExtraEffect effect = ExtraEffect.none)
        {
            var playersGroup = new List<BasePlayer>();
            foreach (var p in room.players.Values)
            {
                if (p.client != null && !players.Contains(p))
                {
                    playersGroup.Add(p);
                }
            }

																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);

            if (effect == ExtraEffect.none)
            {
                parameters.Add((byte)Params.ExtraId, extra.effect);
            }
            else
            {
                parameters.Add((byte)Params.ExtraId, effect);
            }

            SendEvent(playersGroup, parameters, Events.RoomSystemMessage_Extra);
        }

        public void Extra_TeamMessage(Team team, Extra extra, string message)
        {
            EventData eventData = new EventData((byte)Events.RoomSystemMessage_Extra);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.ChatMessage, message);
            eventData.Parameters.Add((byte)Params.ExtraId, extra.effect);
            eventData.SendTo(team.clients, Options.sendParameters);
        }

        public void Extra_PublicMessageClientList(List<Client> list, Extra extra, string message)
        {
            EventData eventData = new EventData((byte)Events.RoomPublicSystemMessage);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.ChatMessage, message);
            eventData.Parameters.Add((byte)Params.ExtraId, extra.effect);
            eventData.SendTo(list, Options.sendParameters);
        }

        public void Extra_SharePersonalMessage(BasePlayer player,Extra extra, int id, string message)
        {
            if (player.playerType != PlayerType.Player) return;
           
             var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.messageId, id);
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.ExtraId, extra.effect);

            SendResponse(player, parameters, Request.ShareMessage);		  
        }

        #endregion

        #region Skill

        public void Skill_PersonalMessage(BasePlayer player, Skill skill, string message)
        {
            if (player.client == null) return;          
																							  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.SkillId, skill.effect);

            SendResponse(player, parameters, Request.SystemMessage_Skill);
        }

        public void Skill_PublicMessage(Skill skill, string message)
        {
																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.SkillId, skill.effect);

            SendEvent(room.players, parameters, Events.RoomSystemMessage_Skill);
        }

        public void Skill_GroupMessage(List<BasePlayer> playersGroup, Skill skill, string message)
        {
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.SkillId, skill.effect);

            SendEvent(playersGroup, parameters, Events.RoomSystemMessage_Skill);
        }

        public void Skill_PublicMessageExcludePlayers(string message, Room room, Skill skill, BasePlayer[] players)
        {
            var playersGroup = new List<BasePlayer>();
            foreach (var p in room.players)
            {
                if (p.Value.client != null && !players.Contains(p.Value))
                {
                    playersGroup.Add(p.Value);
                }
            }
																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.SkillId, skill.effect);

            SendEvent(playersGroup, parameters, Events.RoomSystemMessage_Skill);
        }

        #endregion

        public void PersonalMessage(BasePlayer player, string message)
        {
            if (player.playerType != PlayerType.Player) return;

																									
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);            

            SendResponse(player, parameters, Request.RoomPersonalSystemMessage);
        }

        //public void GroupMessage(Group group, string message)
        //{
        //    EventData eventData = new EventData((byte)Events.RoomPublicSystemMessage);
        //    eventData.Parameters = new Dictionary<byte, object>();
        //    eventData.Parameters.Add((byte)Params.ChatMessage, message);
        //    eventData.SendTo(group.clients, Options.sendParameters);
        //}

        public void TeamMessage(Team team, string message)
        {
            EventData eventData = new EventData((byte)Events.RoomPublicSystemMessage);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.ChatMessage, message);
            eventData.SendTo(team.clients, Options.sendParameters);
        }

        public void PublicMessageToCustomPlayersGroup(List<BasePlayer> playersGroup, string message)
        {            																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);	

            SendEvent(playersGroup, parameters, Events.RoomPublicSystemMessage);
        }     
        

        public void PublicMessage(string message)
        {																					  
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);

            SendEvent(room.players, parameters, Events.RoomPublicSystemMessage);
        }

        public void PublicMessageExcludePlayers(string message, Room room, BasePlayer[] players)
        {
            var playersGroup = new List<BasePlayer>();
            foreach (var p in room.GetLivePlayers())
            {
                if (p.Value.client != null && !players.Contains(p.Value))
                {
                    playersGroup.Add(p.Value);
                }
            }

            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);

            SendEvent(playersGroup, parameters, Events.RoomPublicSystemMessage);
        }

        public void Role_PersonalMessage(BasePlayer player, RoleType roleId, string message)
        {
            if (player.playerType != PlayerType.Player) return;

            OperationResponse resp = new OperationResponse((byte)Request.SystemMessage_Role);
            resp.Parameters = new Dictionary<byte, object>();

            resp.Parameters.Add((byte)Params.ChatMessage, message);
            resp.Parameters.Add((byte)Params.RoleId, roleId);

            player.client.SendOperationResponse(resp, Options.sendParameters);
        }

        public void Role_PublicMessage(RoleType roleId, string message)
        {
            var parameters = new Dictionary<byte, object>();
            parameters.Add((byte)Params.ChatMessage, message);
            parameters.Add((byte)Params.RoleId, roleId);

            SendEvent(room.players, parameters, Events.Role_PublicMessage);
        }

     
    }

    public interface IChat
    {
        List<Client> GetClients();

        bool IsContainPlayer(BasePlayer player);
    }

    public class Chat
    {
        private ChatId id;
        public Chat(ChatId id)
        {
            this.id = id;
        }

        public Dictionary<long, BasePlayer> players { get; private set; } = new Dictionary<long, BasePlayer>();
        public List<Client> clients { get; private set; } = new List<Client>();

        public void AddPlayerToChat(BasePlayer player)
        {
            //если этот игрок уже находится в этом чате, то ничего не делаем
            //например грешник заменил босса мафии. оба были в чате мафии
            if (players.ContainsKey(player.playerId)) return;

            players.Add(player.playerId, player);

            if (player.client != null)
            {
                clients.Add(player.client);

                OperationResponse resp = new OperationResponse((byte)Request.JoinChat);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.ChatId, id);
                player.client.SendOperationResponse(resp, Options.sendParameters);
            }
        }

        public void RemovePlayerFromChat(BasePlayer player)
        {
            //если пытаемся удалить из чата игрока, которого там не было 
            //???
            if (players.ContainsKey(player.playerId) == false) return;

            players.Remove(player.playerId);

            if (player.client != null)
            {
                clients.Remove(player.client);

                OperationResponse resp = new OperationResponse((byte)Request.LeaveChat);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.ChatId, id);
                player.client.SendOperationResponse(resp, Options.sendParameters);
            }
        }
    }
}


