using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class GuerillaVisit : RoleVisit
    {
        public GuerillaVisit(Room room) : base(room)
        {

        }

        BasePlayer guerilla;
        public void Setup()
        {
            guerilla = RoomHelper.FindPlayerByRole(RoleType.Guerilla, room);

            if (guerilla == null) return;

            guerilla.SetActiveAtNight(guerilla.targetPlayer != null);

            //проверяем, что цель жива на начало ночи
            if (guerilla.targetPlayer != null && !guerilla.targetPlayer.isLive()) guerilla.targetPlayer = null;
        }

        public void Visit()
        {
            //если партизана нет
            if (guerilla == null) return;

            //если у партизана нет цели
            if (guerilla.targetPlayer == null) return;

            //если партизана не может сделать ход
            if (!guerilla.playerRole.CanVisit()) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (guerilla.targetPlayer.playerRole.CheckResistExtras(guerilla)) return;

            RoleHelper.ApplyRoleEffectForced(room, guerilla, DurationType.NightEnd, guerilla.targetPlayer);

            var guerillaRole = GetRole();

            if (guerilla.targetPlayer.targetPlayer != null)
            {
                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Guerilla,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage(
                        $"{ColorString.GetColoredRole("Партизан")} этой ночью болтал с игроком и не дал ему походить");

                    room.roomChat.PersonalMessage(
                        guerilla.targetPlayer,
                        $"{ColorString.GetColoredRole("Партизан")} не дал Вам походить");
                }
                );
            }          

            if (guerillaRole.Check_GuerillaLongEffect())
            {
                var dayCount = room.roomPhases.dayCount + 2;
                SkillHelper.AddSkillEffectToPlayer(
                    guerilla.targetPlayer, guerillaRole.skill_GuerillaLongEffect, DurationType.NightEnd, dayCount);

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Guerilla,
                NightActionId.Skill,
                () =>
                {
                    room.roomChat.Skill_PersonalMessage(
                        guerilla, guerillaRole.skill_GuerillaLongEffect,
                        $"{ColorString.GetColoredRole("Партизан")} заболтал игрока на 3 ночи вперед, " +
                        $"при помощи навыка {ColorString.GetColoredSkill("Долгий эффект")}. " +
                        $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(
                        guerilla.targetPlayer, guerillaRole.skill_GuerillaLongEffect,
                        $"{ColorString.GetColoredRole("Партизан")} заболтал игрока на 3 ночи вперед, " +
                        $"при помощи навыка {ColorString.GetColoredSkill("Долгий эффект")}. " +
                        $"Вы не сможете ходить 3 ночи подряд");

                    var excludedPlayers = new BasePlayer[] { guerilla, guerilla.targetPlayer };

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{ColorString.GetColoredRole("Партизан")} заболтал игрока на 3 ночи вперед, " +
                        $"при помощи навыка {ColorString.GetColoredSkill("Долгий эффект")}",
                        room, guerillaRole.skill_GuerillaLongEffect, excludedPlayers);
                }
                );
            }
           
        }

        private Guerilla GetRole()
        {
            Guerilla role = null;

            if (guerilla.oldRole != null)
            {
                role = (Guerilla)guerilla.oldRole;
            }
            else
            {
                role = (Guerilla)guerilla.playerRole;
            }

            return role;
        }
    }
}
