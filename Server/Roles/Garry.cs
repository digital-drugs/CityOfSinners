using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Garry : Role
    {
        public Garry(RoleType roleType) : base(roleType)
        {

        }

        public override void SetPlayer(BasePlayer player)
        {
            base.SetPlayer(player);

            //player.room.roomPhases.OnNightStart += OnNightStart;
        }

        private void OnNightStart(object sender, EventArgs e)
        {
            CheckPirateTeam();
        }
        
        public void CheckPirateTeam()
        {
           
        }
    }
}
