using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server.Extras
{
    public class EventExtraHandler: IExtraHandler
    {
        private BasePlayer player;
        public Extra extra { get; private set; }
        private Room room;

        private PlayerEventType playerEventType;
        private int targetEventCount;

        public int extraEffectId { get; private set; }
        public EventExtraHandler(BasePlayer player, Extra extra, Room room, PlayerEventType playerEventType, int targetEventCount)
        {
            this.player = player;
            this.extra = extra;
            this.room = room;

            extraEffectId = room.GetExtraEffectId();

            this.playerEventType = playerEventType;
            this.targetEventCount = targetEventCount;

            Subscribe();
        }

        private void Subscribe()
        {
            Logger.Log.Debug($" event type = {playerEventType} ");

            switch (playerEventType)
            {
                case PlayerEventType.Action: { player.OnAction += OnAction; } break;
                case PlayerEventType.ChatMessage: { player.OnChatMessage += OnChatMessage; } break;
            }
        }
        private void UnSubscribe()
        {
            switch (playerEventType)
            {
                case PlayerEventType.Action: { player.OnAction -= OnAction; } break;
                case PlayerEventType.ChatMessage: { player.OnChatMessage -= OnChatMessage; } break;
            }
        }

        int currentEventCount = 0;
        private void OnAction(object sender, EventArgs e)
        {
            currentEventCount++;

            Logger.Log.Debug($"player {player.playerName} do action {playerEventType}");
            Logger.Log.Debug($"player {player.playerName} current Event Count => {currentEventCount}");

            if (currentEventCount < targetEventCount) return;
                
            room.extraHelper.UseEventExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveEventExtraHandler(this);

            UnSubscribe();
        }

        private void OnChatMessage(object sender, EventArgs e)
        {
            currentEventCount++;

            Logger.Log.Debug($"player {player.playerName} do action {playerEventType}");
            Logger.Log.Debug($"player {player.playerName} current Event Count => {currentEventCount}");

            if (currentEventCount < targetEventCount) return;

            room.extraHelper.UseEventExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveEventExtraHandler(this);

            UnSubscribe();
        }

        public int GetEffectId()
        {
            return extraEffectId;
        }

        public Extra GetExtra()
        {
            return extra;
        }
    }
}
