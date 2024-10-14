using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class GuerillaVisit : RoleVisit
    {
        public GuerillaVisit(Room room) : base(room)
        {

        }

        BasePlayer guerilla;
        public void Setup()
        {
            guerilla = RoomHelper.FindPlayerByRole(RoleType.Guerilla, room);
        }

        public void Visit()
        {

        }
    }
}
