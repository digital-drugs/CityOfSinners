using ExitGames.Client.Photon.LoadBalancing;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Jane:Role
    {
        public Jane(RoleType roleType) : base(roleType)
        {

        }

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
    }
}
