using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomConfig
    {
        public RoomType roomType { get; private set; }

        public int playerLimit;

        //public int goodLimit;
        public int minimumBadLimit;
        public int maximumBadLimit;

        public int mafiaLimit;

        //public List<RoleType> availablesRoles { get;  set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerLimit"></param>
        /// <param name="badLimit"></param>
        /// <param name="mafiaLimit"> максимальное колво ролей типа Mafia </param>
        public RoomConfig(RoomType roomType,int playerLimit, int minBadLim, int maxBadLim, int mafiaLimit) 
        {
            this.roomType = roomType;
            this.playerLimit = playerLimit;
            this.minimumBadLimit = minBadLim;
            this.maximumBadLimit = maxBadLim;
            this.mafiaLimit= mafiaLimit;

            //goodLimit = playerLimit - badLimit;
        }
    }
}
