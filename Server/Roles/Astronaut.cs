using Photon.SocketServer.Annotations;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Astronaut : Role
    {
        public Astronaut(RoleType roleType) : base(roleType)
        {

        }

        private List<long> visitedPlayers = new List<long>();
        private int presentLimit=3;
        public void PresentTickets(BasePlayer targetPlayer)
        {
            if (visitedPlayers.Contains(targetPlayer.playerId))
            {
                owner.GetRoom().roomChat.PersonalMessage(owner ,$"увы, билетов нет");
                return;
            }

            if(presentLimit==0)
            {
                owner.GetRoom().roomChat.PersonalMessage(owner, $"увы, билетов нет");
                return;
            }

            visitedPlayers.Add(targetPlayer.playerId);

            presentLimit--;

            var randomTicket = owner.GetRoom().dice.Next(1, 4);

            owner.GetRoom().roomChat.PersonalMessage
                (owner, $"вы подарили {targetPlayer.GetColoredName()} {randomTicket} билетов");
        }
    }
}
