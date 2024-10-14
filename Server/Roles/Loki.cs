using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Loki : Role
    {
        public Loki(RoleType roleType) : base(roleType)
        {

        }

        //public override void SetPlayer(BasePlayer player)
        //{
        //    base.SetPlayer(player);

        //    player.room.roomPhases.OnNightStart += OnNightStart;
        //}

        //private void OnNightStart(object sender, EventArgs e)
        //{
        //    CheckPirateTeam();
        //}

        public void CheckKill()
        {
            if (owner.team.players.Count > 1)
            {
                isKiller = false;
            }
            else
            {
                isKiller = true;
            }
        }

        public int Rob(BasePlayer targetPlayer)
        {
            var result = 0;

            result = owner.GetRoom().dice.Next(1, 5001);

            return result;
        }
    }
}
