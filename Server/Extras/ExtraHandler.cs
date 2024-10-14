using ExitGames.Client.Photon.LoadBalancing;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server.Extras
{
    public class ExtraHandler: IExtraHandler
    {
        private BasePlayer player;
        public Extra extra { get; private set; }
        private Room room;

        public int extraEffectId { get; private set; }

        DurationType handlerDuration = DurationType.Null;

        public ExtraHandler(BasePlayer player, Extra extra, Room room, DurationType duration = DurationType.Null) 
        {
            this.player = player;
            this.extra = extra;
            this.room = room;

            extraEffectId = room.GetExtraEffectId();

            //Logger.Log.Debug($"{extra.extraId} dur type {extra.durationType} ");

            if (duration == DurationType.Null)
            {
                handlerDuration = extra.durationType;
            }
            else
            {
                handlerDuration = duration;
            }

            Subscribe();
        }

        private void Subscribe()
        {
            switch (handlerDuration)
            {
                case DurationType.DayStart: { room.roomPhases.OnDayStart += OnAction; } break;
                case DurationType.NightStart: { room.roomPhases.OnNightStart += OnAction; } break;
                case DurationType.NightEnd: { room.roomPhases.OnNightEnd += OnAction; } break;
                case DurationType.DayEnd: { room.roomPhases.OnDayEnd += OnAction; } break;
                case DurationType.EndPhase:
                    {
                        room.roomPhases.OnNightEnd += OnAction;
                        room.roomPhases.OnDayEnd += OnAction;
                    }
                    break;
            }
        }

        private void UnSubscribe()
        {
            switch (handlerDuration)
            {
                case DurationType.DayStart: { room.roomPhases.OnDayStart -= OnAction; } break;
                case DurationType.NightStart: { room.roomPhases.OnNightStart -= OnAction; } break;
                case DurationType.NightEnd: { room.roomPhases.OnNightEnd -= OnAction; } break;
                case DurationType.DayEnd: { room.roomPhases.OnDayEnd -= OnAction; } break;
                case DurationType.EndPhase:
                    {
                        room.roomPhases.OnNightEnd -= OnAction;
                        room.roomPhases.OnDayEnd -= OnAction;
                    }
                    break;
            }
        }

        private void OnAction(object sender, EventArgs e)
        {
            player.playerRole.roleEffects.RemoveExtraEffect(extra);

            player.playerRole.roleEffects.RemoveExtraHandler(this);

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
