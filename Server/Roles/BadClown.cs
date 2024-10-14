using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class BadClown : Role
    {
        public BadClown(RoleType roleType) : base(roleType)
        {

        }

        public void GetKey(BasePlayer targetPlayer)
        {
            var getKeyRandom = owner.GetRoom().dice.Next(0, 2);

            if (getKeyRandom == 0) return;

            var randomKeyQuality = owner.GetRoom().dice.Next(0, 100);

            if (randomKeyQuality < 50)
            {
                var keyString = ColorString.GetColoredString("золотой ключ", ColorId.Gold);

                owner.GetRoom().roomChat.PersonalMessage
               (owner, $"вы получили {keyString} за убийство {targetPlayer.GetColoredName()}");
            }
            else
            {
                var keyString = ColorString.GetColoredString("обычный ключ", ColorId.Silver);

                owner.GetRoom().roomChat.PersonalMessage
              (owner, $"вы получили {keyString} за убийство {targetPlayer.GetColoredName()}");
            }
        }
    }
}
