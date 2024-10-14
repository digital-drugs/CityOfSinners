using ExitGames.Client.Photon.LoadBalancing;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Group_: IChat
    {
        public Dictionary<long, BasePlayer> players { get; private set; } = new Dictionary<long, BasePlayer>();
        public RoleType roleType;

        //public Group()
        //{
            
        //}

        //public Group(RoleType roleType) 
        //{
        //    this.roleType = roleType;
        //}

        public List<Client> clients { get; private set; } = new List<Client>();
        public void AddPlayer(BasePlayer player)
        {
            if (players.ContainsKey(player.playerId))
            {
                Logger.Log.Debug($" player {player.playerName} {player.playerId} already in this group {roleType}");
                return;
            }

            //if (player.group != null)
            //{
            //    player.group.RemovePlayer(player);
            //}

            players.Add(player.playerId, player);
            //player.SetGroup(this);

            if (player.playerType == PlayerType.Player)
            {
                clients.Add(player.client);
            }
        }

        //public ChatId chatId { get; private set; }
        //public string chatName { get; private set; }
        //public void CreateGroupChat(ChatId id, Room room, string chatName)
        //{
        //    chatId = id;// room.GetChatId();

        //    room.roomChat.AddChat(chatId, this);

        //    EventData eventData = new EventData((byte)Events.CreateChat);

        //    eventData.Parameters = new Dictionary<byte, object> { };

        //    eventData.Parameters.Add((byte)Params.ChatType, ChatType.GroupChat);
        //    eventData.Parameters.Add((byte)Params.ChatId, chatId);

        //    this.chatName = chatName;

        //    eventData.Parameters.Add((byte)Params.ChatName, chatName);

        //    eventData.SendTo(clients, Options.sendParameters);

        //    Logger.Log.Debug($"chat clients count {clients.Count}");
        //}

        public void RemovePlayer(BasePlayer player)
        {
            if (!players.ContainsKey(player.playerId))
            {
                Logger.Log.Debug($"cant remove player {player.playerName} {player.playerId} . no this player in {roleType}");
                return;
            }

            players.Remove(player.playerId);
            //player.SetGroup(null);

            if (player.playerType == PlayerType.Player)
            {
                clients.Remove(player.client);
            }
        }

        public List<Client> GetClients()
        {
            return clients;
        }

        public bool IsContainPlayer(BasePlayer player)
        {
            if(players.ContainsKey(player.playerId))
            {
                return true;
            }
            
            return false;
        }
    }
}
