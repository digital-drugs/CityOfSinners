using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public  class RoleHandler
    {
        private BasePlayer player;
        public Role role { get; private set; }
        private Room room;

        public int roleEffectId { get; private set; }

        public RoleHandler(BasePlayer player, Role role, Room room)
        {
            this.player = player;
            this.role = role;
            this.room = room;
        }

        private DurationType endPhase;
        public void Subscribe(DurationType endPhase)
        {
            this.endPhase = endPhase;

            switch (endPhase)
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
            switch (endPhase)
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
            if (room.roomIsStoped) return;

            if (player == null)
            {
                Logger.Log.Debug($"on RemoveRoleEffect player is null");
                return;
            }

            if (player.playerRole == null)
            {
                Logger.Log.Debug($"on RemoveRoleEffect player role is null");
                return;
            }

            //удаляем с цели эффект скилла
            player.playerRole.roleEffects.RemoveRoleEffect(role);

            //удаляем с цели контроллер эффекта скилла
            player.playerRole.roleEffects.RemoveRoleHandler(this);

            UnSubscribe();
        }

        public int GetRoleEffectId()
        {
            return roleEffectId;
        }

        public Role GetRole()
        {
            return role;
        }
    }
}
