using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server.Extras
{
    public class DelayedExtraHandler: IExtraHandler
    {
        private BasePlayer player;
        public Extra extra { get; private set; }
        private Room room;

        DurationType durationType = DurationType.Null;

        public int extraEffectId { get; private set; }

        public DelayedExtraHandler(BasePlayer player, Extra extra, Room room, DurationType durationType = DurationType.Null)
        {
            this.player = player;
            this.extra = extra;
            this.room = room;

            extraEffectId = room.GetExtraEffectId();

            Logger.Log.Debug($"{extra.extraId} dur type {extra.durationType} ");

            this.durationType = durationType;

            Subscribe();
        }

        private void Subscribe()
        {
            DurationType type;

            if(durationType!= DurationType.Null)
            {
                type= durationType;
            }
            else
            {
                type = extra.durationType;
            }

            Logger.Log.Debug($" dur type = {durationType} ext type = {extra.durationType} type = {type} ");

            switch (type)
            {
                case DurationType.NightStart: { room.roomPhases.OnNightStart += OnNightStart; } break;
                case DurationType.NightEnd: { room.roomPhases.OnNightEnd += OnNightEnd; } break;

                case DurationType.DayStart: { room.roomPhases.OnDayStart += OnDayStart; } break;
                case DurationType.DayEnd: { room.roomPhases.OnDayEnd += OnDayEnd; } break;
            }
        }
        private void UnSubscribe()
        {
            DurationType type;

            if (durationType != DurationType.Null)
            {
                type = durationType;
            }
            else
            {
                type = extra.durationType;
            }

            Logger.Log.Debug($" dur type = {durationType} ext type = {extra.durationType} type = {type} ");

            switch (type)
            {
                case DurationType.NightStart: { room.roomPhases.OnNightStart -= OnNightStart; } break;
                case DurationType.NightEnd: { room.roomPhases.OnNightEnd -= OnNightEnd; } break;

                case DurationType.DayStart: { room.roomPhases.OnDayStart -= OnDayStart; } break;
                case DurationType.DayEnd: { room.roomPhases.OnDayEnd -= OnDayEnd; } break;
            }
        }

        private void OnNightStart(object sender, EventArgs e)
        {
            Logger.Log.Debug($"use delay extra {extra.extraId} on OnNightStart");

            room.extraHelper.UseDelayedExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveDelayedExtraHandler(this);

            UnSubscribe();
        }

        private void OnNightEnd(object sender, EventArgs e)
        {
            Logger.Log.Debug($"use delay extra {extra.extraId} on OnNightEnd");

            room.extraHelper.UseDelayedExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveDelayedExtraHandler(this);

            UnSubscribe();
        }

        private void OnDayStart(object sender, EventArgs e)
        {
            Logger.Log.Debug($"use delay extra {extra.extraId} on OnDayStart");

            room.extraHelper.UseDelayedExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveDelayedExtraHandler(this);

            UnSubscribe();
        }

        private void OnDayEnd(object sender, EventArgs e)
        {
            Logger.Log.Debug($"use delay extra {extra.extraId} on OnDayEnd");

            room.extraHelper.UseDelayedExtra(player, extra);

            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveDelayedExtraHandler(this);

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
