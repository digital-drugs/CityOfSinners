using Photon.SocketServer.Annotations;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Scientist : Role
    {
        public Scientist(RoleType roleType) : base(roleType)
        {

        }

        public void Visit(BasePlayer targetPlayer)
        {
            var visitDice = owner.GetRoom().dice.Next(0, 100);

            if (visitDice < 34)
            {
                var diceString = ColorString.GetColoredString("кубик", ColorId.Gold);

                owner.GetRoom().roomChat.PersonalMessage
            (owner, $"вы подарили {targetPlayer.GetColoredName()} {diceString} и получили такой же");
            }

            if (visitDice > 33 && visitDice < 67)
            {
                owner.GetRoom().roomLogic.SendPlayerToMorgue(targetPlayer);

                owner.GetRoom().roomChat.PublicMessage
                    ($"{owner.GetColoredRole()} убил {targetPlayer.GetColoredName()}");
            }

            if (visitDice > 66)
            {
                RoleHelper.ApplyRoleEffectForced(owner.GetRoom(), owner, DurationType.Game, targetPlayer);

                owner.GetRoom().roomChat.PublicMessage
                    ($"{owner.GetColoredRole()} посадил в паутину {targetPlayer.GetColoredName()}");
            }
        }
    }
}
