using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class DoctorVisit : RoleVisit
    {
        public DoctorVisit(Room room) : base(room)
        {

        }

        BasePlayer doctor;
        public void Setup()
        {
            doctor = RoomHelper.FindPlayerByRole(RoleType.Doctor, room);
        }

        public void Visit_Heal()
        {
            //если доктора нет
            if (doctor == null) return;

            //если у доктора нет цели
            if (doctor.targetPlayer == null) return;

            var doctorRole = (Doctor)doctor.playerRole;

            if (doctorRole.isClizma) return;

            //если доктор не может сделать ход
            if (!doctor.playerRole.CanVisit()) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (doctor.targetPlayer.playerRole.CheckResistExtras(doctor)) return;

            RoleHelper.ApplyRoleEffectForced(room, doctor, DurationType.NightEnd, doctor.targetPlayer);

            if (doctorRole.Check_DoctorHealX2())
            {
                //ищем соседнего игрока
                var randomTarget = RoomHelper.FindNearPlayers(
                      room, doctor, doctor.targetPlayer, 1, false, true);

                if (randomTarget.Count > 0)
                {
                    RoleHelper.ApplyRoleEffectForced(room, doctor, DurationType.NightEnd, randomTarget[0]);

                    room.roomLogic. AddNightActionMessage
                    (
                    doctor,
                    NightActionId.Skill,
                    () =>
                    {
                    room.roomChat.Skill_PersonalMessage(doctor, doctorRole.skill_DoctorHealX2,
                        $"{ColorString.GetColoredRole("Доктор")} навестил {randomTarget[0].GetColoredName()}, " +
                        $"благодаря навыку {ColorString.GetColoredSkill("х2")}. " +
                        $"Ваш навык сработал");

                    var excludedPlayers = new BasePlayer[] { doctor };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{ColorString.GetColoredRole("Доктор")} навестил {randomTarget[0].GetColoredName()}, " +
                        $"благодаря навыку {ColorString.GetColoredSkill("х2")}",
                        room, doctorRole.skill_DoctorHealX2, excludedPlayers);
                    }
                    );
                }
            }
        }

        public void Visit_Report()
        {
            if (doctor == null) return;

            if (doctor.isLive() == false) return;

            if (doctor.targetPlayer == null) return;

            if (doctor.targetPlayer.isLive() == false) return;

            if (doctor.targetPlayer.playerRole.IsResurected() == true) return;

            var doctorEffect = doctor.targetPlayer.playerRole.roleEffects.FindRoleEffect(RoleType.Doctor);
            if (doctorEffect != null)
            {
                room.roomLogic. AddNightActionMessage
                (
                doctor,
                NightActionId.Role,
                () =>
                {
                room.roomChat.PublicMessage($"{ColorString.GetColoredRole("Доктор")} навестил " +
                    $"{doctor.targetPlayer.GetColoredName()}, но всё было спокойно");
                }
                );
            }
        }
    }
}
