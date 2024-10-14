using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Team
    {      
        public TeamType teamType;
        private Room room;
        public Team(Room room, TeamType teamType) 
        {
            this.room = room;
            this.teamType = teamType;
        }

        public Dictionary<long, BasePlayer> players { get; private set; } = new Dictionary<long, BasePlayer>();
        public List<Client> clients { get; private set; } = new List<Client>();
        public void AddPlayer(BasePlayer player)
        {
            players.Add(player.playerId, player);

            if (player.client != null)
            {
                clients.Add(player.client);
            }   
        }
        public void RemovePlayer(BasePlayer player)
        {
            players.Remove(player.playerId);

            if (player.client != null)
            {
                clients.Remove(player.client);
            }
        }

        public void UpdateCompound()
        {
            var teamData = new Dictionary<long, object>();

            foreach (var p in players.Values)
            {
                var teamPlayer = new Dictionary<byte, object>();

                teamPlayer.Add((byte)Params.RoleId, p.playerRole.roleType);

                teamData.Add(p.playerId, teamPlayer);
            }

            EventData eventData = new EventData((byte)Events.TeamCompound);

            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.Roles, teamData);

            eventData.SendTo(clients, Options.sendParameters);           
        }

        public List<BasePlayer> GetLivePlayers()
        {
            var result = new List<BasePlayer>();

            foreach (var p in players.Values)
            {
                if (p.isLive()) result.Add(p);
            }

            return result;
        }



        public List<BasePlayer> GetPlayers()
        {
            var result = new List<BasePlayer>();

            foreach (var p in players.Values)
            {
                 result.Add(p);
            }

            return result;
        }

    }
}
