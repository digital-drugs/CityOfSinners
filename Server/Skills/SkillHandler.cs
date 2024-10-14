using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class SkillHandler
    {
        private BasePlayer player;
        public Skill skill { get; private set; }
        private Room room;

        public int skillEffectId { get; private set; }

        public SkillHandler(BasePlayer player, Skill skill, Room room)
        {
            this.player = player;
            this.skill = skill;
            this.room = room;

            skillEffectId = room.GetSkillEffectId();

            //Logger.Log.Debug($"{extra.extraId} dur type {extra.durationType} ");

            //Subscribe();
        }

        private DurationType endPhase;
        private int dayCount = 0;
         public void Subscribe(DurationType  endPhase, int dayCount=0)
        {
            this.endPhase = endPhase;
            this.dayCount = dayCount;   

            switch (endPhase)
            {
                case DurationType.DayStart: { room.roomPhases.OnDayStart += OnAction; } break;
                case DurationType.NightStart: { room.roomPhases.OnNightStart += OnAction; } break;

                //case DurationType.NightEnd: { room.roomPhases.OnNightEnd += OnNightEnd; } break;
                //case DurationType.DayEnd: { room.roomPhases.OnDayEnd += OnDayEnd; } break;
                //case DurationType.EndPhase: { room.roomPhases.OnNightEnd += OnNightEnd;
                //        room.roomPhases.OnDayEnd += OnDayEnd; } break;

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
            switch (endPhase)
            {
                case DurationType.DayStart: { room.roomPhases.OnDayStart -= OnAction; } break;
                case DurationType.NightStart: { room.roomPhases.OnNightStart -= OnAction; } break;

                //case DurationType.NightEnd: { room.roomPhases.OnNightEnd -= OnNightEnd; } break;
                //case DurationType.DayEnd: { room.roomPhases.OnDayEnd -= OnDayEnd; } break;
                //case DurationType.EndPhase: { room.roomPhases.OnNightEnd -= OnNightEnd;
                //        room.roomPhases.OnDayEnd -= OnDayEnd; } break;

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
            if (dayCount != 0)
            {
                if (room.roomPhases.dayCount != dayCount)
                {
                    Logger.Log.Debug($"try remove skill effect {room.roomPhases.dayCount} {dayCount}");
                    return;
                }
            }

            //Logger.Log.Debug($"skill duration end NIGHT");

            //удаляем с цели эффект скилла
            player.playerRole.roleEffects.RemoveSkillEffect(skill);

            //удаляем с цели контроллер эффекта скилла
            player.playerRole.roleEffects.RemoveSkillHandler(this);

            UnSubscribe();

														 
        }

        public int GetSkillEffectId()
        {
            return skillEffectId;
        }

        public Skill GetSkill()
        {
            return skill;
        }
    }
}
