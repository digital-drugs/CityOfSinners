using ExitGames.Client.Photon.LoadBalancing;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomBots
    {
        private Room room;
        public RoomBots(Room room) 
        {
            this.room = room;
        }

        public void AddBots()
        {
            if (room.players.Count < room.config.playerLimit)
            {
                var botCount = room.config.playerLimit - room.players.Count;

                for (int i = 0; i < botCount; i++)
                {
                    var bot = new Bot();

                    bot.playerId = i;
                    bot.playerName = $"Bot {i}";
                    bot.playerType = PlayerType.Bot;
                    bot.SetRoom(room);

                    room.players.Add(bot.playerId, bot);

                    AddExtrasToBot(bot);
                }
            }
        }

        private void AddExtrasToBot(BasePlayer player)
        {
            var slots = new Dictionary<int, object>();
            var slotCount = 0;

            var extraData = new Dictionary<byte, object>();

            var extraEffect = ExtraEffect.Bit.ToString();

            extraData.Add((byte)Params.ExtraId, extraEffect);
            extraData.Add((byte)Params.ExtraName, extraEffect);
            extraData.Add((byte)Params.ExtraCount, 10);
            extraData.Add((byte)Params.ExtraGameCount, 2);
            extraData.Add((byte)Params.ExtraUseType, ExtraUseType.Auto);
            extraData.Add((byte)Params.ExtraType, ExtraType.Clan.ToString());
            extraData.Add((byte)Params.GamePhase, GamePhase.Day.ToString());

            slots.Add(slotCount++, extraData);

            ////

            //if (player.playerId % 6 == 0 && player.playerId != 0)
            //{
            //    extraData = new Dictionary<byte, object>();

            //    var extraEffect = ExtraEffect.Cocoon.ToString();

            //    extraData.Add((byte)Params.ExtraId, extraEffect);
            //    extraData.Add((byte)Params.ExtraName, extraEffect);
            //    extraData.Add((byte)Params.ExtraCount, 10);
            //    extraData.Add((byte)Params.ExtraGameCount, 10);
            //    extraData.Add((byte)Params.ExtraUseType, ExtraUseType.Auto);
            //    extraData.Add((byte)Params.ExtraType, ExtraType.Event.ToString());
            //    extraData.Add((byte)Params.GamePhase, GamePhase.Night.ToString());

            //    slots.Add(slotCount++, extraData);

            //    Logger.Log.Debug($"bot {player.playerId} get {extraEffect}");
            //}


            ////

            //extraData = new Dictionary<byte, object>();

            //extraData.Add((byte)Params.ExtraId, ExtraEffect.AirPlane.ToString());
            //extraData.Add((byte)Params.ExtraCount, 10);
            //extraData.Add((byte)Params.ExtraGameCount, 10);
            //extraData.Add((byte)Params.ExtraUseType, ExtraUseType.Auto);

            //slots.Add(slotCount++, extraData);

            ////

            //extraData = new Dictionary<byte, object>();

            //extraData.Add((byte)Params.ExtraId, ExtraEffect.Mine.ToString());
            //extraData.Add((byte)Params.ExtraCount, 10);
            //extraData.Add((byte)Params.ExtraGameCount, 10);
            //extraData.Add((byte)Params.ExtraUseType, ExtraUseType.Auto);

            //slots.Add(slotCount++, extraData);

            player.SetupGameExtraSlots(slots);
        }

        public void BotChatEmulation()
        {
            foreach (var p in room.GetLivePlayers().Values)
            {
                if (p.playerId == 5)
                {
                    //p.SendChatMessage($"я лотвкптолвкптол", 0, ChatType.RoomChat);
                }
            }
        }

        public bool botUseExtra = false;
        public void BotExtraEmulation(ExtraEffect extraEffect)
        {
            if (!botUseExtra) return;

            for (int i = room.GetLivePlayers().Values.Count - 1; i >= 0; i--)
            {
                if (room.GetLivePlayers().Values.ElementAt(i).playerType == PlayerType.Bot)
                {
                    var extraData = new Dictionary<byte, object>();
                    extraData.Add((byte)Params.UserId,(long)23);
                    room.GetLivePlayers().Values.ElementAt(i).UseExtra(0, extraData);
                }
            }
        }

        public bool botUseVote = false;
        public void BotVotesEmulation()
        {
            //if(!botUseVote) return;            

            //if (room.GetLivePlayers().Count == 0) return;

            //foreach (var p in room.GetLivePlayers().Values)
            //{
            //    if (p.playerType == PlayerType.Bot)
            //    {
            //        var randomPlayerDice = room.dice.Next(room.GetLivePlayers().Count);

            //        var playerId = room.GetLivePlayers().ElementAt(randomPlayerDice).Key;

            //        room.roomLogic.SelectPlayer(p, playerId);
            //    }
            //}
        }

        public bool botUseVisit = false;
        public void BotVisitsEmulation()
        {
            //if (!botUseVisit) return;

            //if (room.roomLogic.activePlayers.Count == 0) return;

            //foreach (var p in room.roomLogic.activePlayers.Values)
            //{
            //    //Боты граждане не ходят ночью
            //    if(p.playerType == PlayerType.Bot && p.playerRole.roleType == RoleType.Citizen)
            //    {
            //        continue;
            //    }

            //    if (p.playerType == PlayerType.Bot && p.playerRole.roleType == RoleType.Maniac)
            //    {
            //        var randomPlayerDice = room.dice.Next(room.roomLogic.activePlayers.Count);

            //        var playerId = room.roomLogic.activePlayers.ElementAt(randomPlayerDice).Key;

            //        room.roomLogic.SelectPlayer(p, playerId);

            //        continue;
            //    }

            //    //Поход мафии к одному человеку для проверки
            //    //if (p.playerType == PlayerType.Bot && p.playerRole.roleType == RoleType.Mafia)
            //    //{
            //    //    //Logger.Log.Debug("Current day: " + room.roomPhases.dayCount);
            //    //    //Сделать визит мафии только в первую игровую ночь
            //    //    if (true)
            //    //    {
            //    //        if(room.roomPhases.dayCount == 1)
            //    //        {
            //    //            room.roomLogic.SelectPlayer(p, 5);
            //    //        }
            //    //    }

            //    //    continue;
            //    //}

            //    if (p.playerType == PlayerType.Bot)
            //    {
            //        var randomPlayerDice = room.dice.Next(room.roomLogic.activePlayers.Count);

            //        var playerId = room.roomLogic.activePlayers.ElementAt(randomPlayerDice).Key;

            //        room.roomLogic.SelectPlayer(p, playerId);

            //        Logger.Log.Debug($"{p.playerName} go to {room.roomLogic.activePlayers.ElementAt(randomPlayerDice).Value.playerName}");
            //    }
            //}
        }

      
    }
}
