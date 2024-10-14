using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class GoodClown : Role
    {
        public GoodClown(RoleType roleType) : base(roleType)
        {

        }

        //private List<long> visitedPlayers = new List<long>();
        //private int presentLimit = 3;
        public void PresentKeys(BasePlayer targetPlayer)
        {
            //if (visitedPlayers.Contains(targetPlayer.playerId))
            //{
            //    owner.room.roomChat.PersonalMessage(owner, $"увы, билетов нет");
            //    return;
            //}

            //if (presentLimit == 0)
            //{
            //    owner.room.roomChat.PersonalMessage(owner, $"увы, билетов нет");
            //    return;
            //}

            //visitedPlayers.Add(targetPlayer.playerId);

            //presentLimit--;

            var randomKeyQuality = owner.GetRoom().dice.Next(0, 100);

            if (randomKeyQuality < 20)
            {
                var keyString = ColorString.GetColoredString("золотой ключ", ColorId.Gold);

                owner.GetRoom().roomChat.PersonalMessage
               (owner, $"вы подарили {targetPlayer.GetColoredName()} {keyString} и получили такой же");
            }
            else
            {
                var keyString = ColorString.GetColoredString("обычный ключ", ColorId.Silver);

                owner.GetRoom().roomChat.PersonalMessage
               (owner, $"вы подарили {targetPlayer.GetColoredName()} {keyString} и получили такой же");
            }

            //var randomKeys = owner.room.dice.Next(1, 4);

           
        }
    }
}
