using ExitGames.Concurrency.Fibers;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;

namespace Mafia_Server
{
    public class RoleAuction
    {
        private int auctionTimeLimit = 15;
        public int remainAuctionTime;
        private int auctionTimerTick = 1000;
        private IDisposable auctionTimer;
        public List<AuctionSlot> auctionSlots;
        private Room room;
        public RoleAuction(Room room, Dictionary<byte, object> parameters=null)
        {
            this.room = room;
        }

        //public List<RoleType> availablesRoles;

        public void StartAuction()
        {
            List<RoleType> diamondBuyRoles = new List<RoleType>();
            List<RoleType> diamondAuctionRoles = new List<RoleType>();

            List<RoleType> coinRareRoles = new List<RoleType>();
            List<RoleType> coinBaseRoles = new List<RoleType>();

            var dice = new Random();

            int roleCycle = 0;

            foreach (var r in room.roomRoles.allRoles.Keys)
            {
                if (r == RoleType.Commissar ||
                    r == RoleType.Doctor ||
                    r == RoleType.Witness ||
                    r == RoleType.Guerilla) 
                {
                    coinBaseRoles.Add(r);
                    Logger.Log.Debug($"{r} to coinBaseRoles");
                }
                if (r == RoleType.MafiaBoss)
                {
                    coinRareRoles.Add(r);
                    Logger.Log.Debug($"{r} to coinRareRoles");
                    roleCycle++;
                }
                if (r == RoleType.Maniac ||
                    r == RoleType.Sinner ||
                    r == RoleType.Werewolf ||
                    r == RoleType.Saint) 
                {
                    switch(roleCycle)
                    {
                        case 0: Logger.Log.Debug($"{r} coinRareRoles"); coinRareRoles.Add(r);break;
                        case 1: Logger.Log.Debug($"{r} diamondBuyRoles"); diamondBuyRoles.Add(r); break;
                        case 2: Logger.Log.Debug($"{r} diamondAuctionRoles"); diamondAuctionRoles.Add(r); break;
                    }

                    roleCycle++;

                    if (roleCycle > 2) roleCycle = 0;
                }
            }           

            remainAuctionTime = auctionTimeLimit;

            auctionTimer = room. poolFiber.ScheduleOnInterval(() => AuctionTimer(), auctionTimerTick, auctionTimerTick);

            //создание списка ролей для аука и отправка и игрокам           

            //ивентовый бросок для все трех слотов

            //бросаем кость, чтобы понять из какого списка будем выбирать роль (80 или 20%)
            var roleDice = dice.Next(100);

            Logger.Log.Debug($"coinRareRoles {coinRareRoles.Count}");
            Logger.Log.Debug($"coinBaseRoles {coinBaseRoles.Count}");
            Logger.Log.Debug($"diamondBuyRoles {diamondBuyRoles.Count}");
            Logger.Log.Debug($"diamondAuctionRoles {diamondAuctionRoles.Count}");

            auctionSlots = new List<AuctionSlot>();

            var slotId = 0;

            RoleType coinAuctionRole;

            if (coinRareRoles.Count == 0)
            {
                var coinBaseDice = dice.Next(coinBaseRoles.Count);
                coinAuctionRole = coinBaseRoles[coinBaseDice];
            }
            else if (coinBaseRoles.Count == 0)
            {
                var coinRareDice = dice.Next(coinRareRoles.Count);
                coinAuctionRole = coinRareRoles[coinRareDice];
            }
            else
            if (roleDice < 20)
            {
                var coinRareDice = dice.Next(coinRareRoles.Count);
                coinAuctionRole = coinRareRoles[coinRareDice];
            }
            else
            {
                var coinBaseDice = dice.Next(coinBaseRoles.Count);
                coinAuctionRole = coinBaseRoles[coinBaseDice];
            }

            auctionSlots.Add(new AuctionSlot(slotId++, coinAuctionRole, false, 50, CurrencyType.Coins));

            Logger.Log.Debug($"dice coin auc role => {roleDice} => {coinAuctionRole}");

            //бросаем кубик для роли за алмазы для покупки
            if (diamondBuyRoles.Count > 0)
            {
                roleDice = dice.Next(diamondBuyRoles.Count);
                var diamondBuyRole = diamondBuyRoles[roleDice];

                auctionSlots.Add(new AuctionSlot(slotId++, diamondBuyRole, true, 1, CurrencyType.Diamond));
            }


            //бросаем кубик для роли за алмазы для аука
            if (diamondAuctionRoles.Count > 0)
            {
                roleDice = dice.Next(diamondAuctionRoles.Count);
                var diamomdAuctionRole = diamondAuctionRoles[roleDice];

                auctionSlots.Add(new AuctionSlot(slotId++, diamomdAuctionRole, false, 1, CurrencyType.Diamond));
            }

            EventData eventData = new EventData((byte)Events.StartAuction);
            eventData.Parameters = new Dictionary<byte, object> { };

            var slots = new Dictionary<byte, object>();

            for (byte i = 0; i < auctionSlots.Count; i++)
            {
                var slot = new Dictionary<byte, object>();
                slots.Add(i, slot);

                slot.Add((byte)Params.SlotId, auctionSlots[i].id);
                slot.Add((byte)Params.RoleId, auctionSlots[i].role);

                if (auctionSlots[i].isBuy == false) 
                    slot.Add((byte)Params.Bet, auctionSlots[i].currentBet);

                slot.Add((byte)Params.Cost, auctionSlots[i].bet);
                slot.Add((byte)Params.CurrencyType, auctionSlots[i].currency);
            }

            eventData.Parameters.Add((byte)Params.AuctionSlots, slots);
            eventData.Parameters.Add((byte)Params.Timer, auctionTimeLimit);

            eventData.SendTo(room.clients, Options.sendParameters);
        }

        private Dictionary<BasePlayer, AuctionSlot> playerSlots = new Dictionary<BasePlayer, AuctionSlot>();
        public void SetBet(BasePlayer player, Dictionary<byte, object> parameters)
        {
            //узнаем на какой слот ставит игрок
            var slotId = (int)parameters[(byte)Params.SlotId];

            //слот в котором игрок хочет сделать ставку
            var slot = auctionSlots[slotId];

            //сверяем слоты игрока
            //если игрок уже где-то делал ставку/покупку
            if (playerSlots.ContainsKey(player))
            {
                //слот в котором игрок уже делал ставку
                var playerSlot = playerSlots[player];

                //если игрок пытается сделать ставку в другом слоте
                if (playerSlot != slot) return;
            }

            //если слот закрыт для ставки/покупки
            if (!slot.isOpen)
            {
                Logger.Log.Debug($"slot is closed. some one buy it");
                return;
            }

            //проверяем есть ли у игрока средства на ставку
            var currentBet = slot.currentBet;

            //добавлянем ставку в слот
            slot.SetBet(player);          

            //если у игрока не был выбран слот до этой ставки
            if (playerSlots.ContainsKey(player) == false) 
            {
                //запоминаем слот в котором игрок может делать ставки
                playerSlots.Add(player, slot);

                //блокируем для игрока другие слоты
                OperationResponse resp = new OperationResponse((byte)Request.LockSlot);
                resp.Parameters = new Dictionary<byte, object>();
                
                byte slotCounter = 0;

                foreach(var s in auctionSlots)
                {
                    if (s == slot) continue;

                    var slotData = new Dictionary<byte, object>();
                    slotData.Add((byte)Params.SlotId, s.id);

                    resp.Parameters.Add(slotCounter++, slotData);
                }

                player.client.SendOperationResponse(resp, Options.sendParameters);
            }

            //если слот выкупили
            if(slot.isOpen == false)
            {
                EventData eventData = new EventData((byte)Events.AuctionBuy);
                eventData.Parameters = new Dictionary<byte, object> { };
                eventData.Parameters.Add((byte)Params.SlotId, slot.id);
                eventData.Parameters.Add((byte)Params.OwnerId, slot.player.playerId);
                eventData.Parameters.Add((byte)Params.Bet, slot.currentBet);
                eventData.SendTo(room.clients, Options.sendParameters);
            }
            //если на слот сделали ставку
            else
            {
                EventData eventData = new EventData((byte)Events.AuctionBet);
                eventData.Parameters = new Dictionary<byte, object> { };
                eventData.Parameters.Add((byte)Params.SlotId, slot.id);
                //eventData.Parameters.Add((byte)Params.SlotOpen, slot.isOpen);
                eventData.Parameters.Add((byte)Params.Bet, slot.currentBet);
                eventData.SendTo(room.clients, Options.sendParameters);
            }                 
        }

        private void AuctionTimer()
        {
            remainAuctionTime--;

            SendAuctionTickToPlayers();

            if (remainAuctionTime == 0)
            {
                room.roomStartGame.StartGame();

                auctionTimer.Dispose();
            }
        }

        private void SendAuctionTickToPlayers()
        {
            EventData eventData = new EventData((byte)Events.AuctionTimer);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.Timer, remainAuctionTime);
            eventData.SendTo(room.clients, Options.sendParameters);
        }
    }
}
