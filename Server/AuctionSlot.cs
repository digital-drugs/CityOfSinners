using Share;
using System;

namespace Mafia_Server
{
    [Serializable]
    public class AuctionSlot:BaseAuctionSlot
    {
        public BasePlayer player; //owner
        public bool isBuy;

        public AuctionSlot(int id, RoleType role, bool isBuy,int bet,CurrencyType currency) :base(id, role, bet, currency)
        {
            this.isBuy = isBuy;
        }

        public void SetBet(BasePlayer player)
        {
            if (isBuy)
            {
                this.player = player;
                isOpen = false;
            }
            else
            {
                this.player = player;
                currentBet += bet;
            }
        }
    }
}
