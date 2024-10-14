using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share
{
    [Serializable]
    public class BaseAuctionSlot
    {
        public int id;

        public RoleType role;
        //public RoleType roleType;

        public int bet;
        public int currentBet;

        public CurrencyType currency;

        public bool isOpen;

        public BaseAuctionSlot(int id, RoleType role, int bet, CurrencyType currency)
        {
            this.id = id;
            this.role = role;
            this.bet = bet;
            this.currency = currency;
            isOpen = true;
        }
    }
}
