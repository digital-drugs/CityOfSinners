using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Alien : Role
    {
        public Alien(RoleType roleType) : base(roleType)
        {

        }


        //private List<long> visitedPlayers = new List<long>();
        private int robLimit = 3;
        public void Rob(BasePlayer targetPlayer)
        {
            //if (visitedPlayers.Contains(targetPlayer.playerId))
            //{
            //    owner.room.roomChat.PersonalMessage(owner, $"увы, билетов нет");
            //    return;
            //}

            if (robLimit == 0)
            {
                //owner.room.roomChat.PersonalMessage(owner, $"увы, билетов нет");

                owner.GetRoom().roomLogic.SendPlayerToMorgue(targetPlayer);

                owner.GetRoom().roomChat.PublicMessage
                    ($"{owner.GetColoredRole()} убил {targetPlayer.GetColoredName()}");

                var killRandomTicket = owner.GetRoom().dice.Next(1, 4);

                owner.GetRoom().roomChat.PersonalMessage
               (owner, $"вы получили {killRandomTicket} билетов за убийство {targetPlayer.GetColoredName()}");             

                return;
            }

            //visitedPlayers.Add(targetPlayer.playerId);

            robLimit--;

            owner.GetRoom().roomChat.PersonalMessage
            (owner, $"вы украли {1} билет у {targetPlayer.GetColoredName()}");
        }
    }
}
