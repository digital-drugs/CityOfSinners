using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class MafiaBossVisit : RoleVisit
    {
        public MafiaBossVisit(Room room) : base(room)
        {

        }

        BasePlayer mafiaBoss;
        public void Setup()
        {
            mafiaBoss = RoomHelper.FindPlayerByRole(RoleType.MafiaBoss, room);

            if (mafiaBoss == null) return;

            mafiaBoss.SetActiveAtNight(mafiaBoss.targetPlayer != null);

            //проверяем, что цель жива на начало ночи
            if (mafiaBoss.targetPlayer != null && !mafiaBoss.targetPlayer.isLive())
            {
                mafiaBoss.targetPlayer = null;
                return;
            }

            var mafiaBossRole = GetRole();

            mafiaBossRole.CheckKill();
            mafiaBossRole.Check_MafiaBossUps();
        }

        public void Visit_Freeze()
        {
            //если босса нет
            if (mafiaBoss == null) return;

            //если у босса нет цели
            if (mafiaBoss.targetPlayer == null) return;

            //если босс не может сделать ход
            if (!mafiaBoss.playerRole.CanVisit()) return;

            var mafiaBossRole = GetRole();

            if (mafiaBossRole.IsKiller() == true) return;

            if (mafiaBossRole.isUps == true) return;

            //если цель защищена зеркалом

            //если цель защищена ролями
            if (mafiaBoss.targetPlayer.playerRole.CheckResistRoles(mafiaBoss)) return;

            //если цель защищена экстрами
            if (mafiaBoss.targetPlayer.playerRole.CheckResistExtras(mafiaBoss)) return;

            RoleHelper.ApplyRoleEffectForced(room, mafiaBoss, DurationType.NightStart, mafiaBoss.targetPlayer);

            room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                    RoleType.MafiaBoss,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage(
                            $"{mafiaBoss.GetColoredRole()} заморозил жертву");

                        room.roomChat.Role_PersonalMessage(
                            mafiaBoss.targetPlayer,
                            RoleType.MafiaBoss,
                            $"Вас заморозили!");
                    
                    }
               );
        }

        public void Visit_Kill()
        {
            //если босса нет
            if (mafiaBoss == null) return;

            //если у босса нет цели
            if (mafiaBoss.targetPlayer == null) return;

            //если босс не может сделать ход
            if (!mafiaBoss.playerRole.CanVisit()) return;

            var mafiaBossRole = GetRole();

            if (mafiaBossRole.IsKiller() == false && mafiaBossRole.isUps == false) return;

            //если цель защищена зеркалом

            //если цель защищена ролями
            if (mafiaBoss.targetPlayer.playerRole.CheckResistRoles(mafiaBoss)) return;

            //если цель защищена экстрами
            if (mafiaBoss.targetPlayer.playerRole.CheckResistExtras(mafiaBoss)) return;

            //если цель защищена скиллами
            if (mafiaBoss.targetPlayer.playerRole.CheckResistSkills(mafiaBoss)) return;            

            var stopVisit = false;

            var targetRole = mafiaBoss.targetPlayer.GetColoredRole();

            if (mafiaBossRole.isUps)
            {
                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                    RoleType.MafiaBoss,
                    NightActionId.Skill,
                    () =>
                        {
                            room.roomChat.Skill_PersonalMessage(
                                mafiaBoss, mafiaBossRole.skill_MafiaBossUps,
                                $"{mafiaBoss.targetPlayer.GetColoredName()} - {targetRole} " +
                                $"заморожен насмерть, {ColorString.GetColoredSkill("Упс")}! " +
                                $"Ваш навык сработал");

                            room.roomChat.Skill_PersonalMessage(
                                mafiaBoss.targetPlayer, mafiaBossRole.skill_MafiaBossUps,
                                $"{mafiaBoss.targetPlayer.GetColoredName()} - {targetRole} " +
                                $"заморожен насмерть, {ColorString.GetColoredSkill("Упс")}! " +
                                $"Вы убиты, увы!");

                            var excludedPlayers = new BasePlayer[] { mafiaBoss, mafiaBoss.targetPlayer };

                            room.roomChat.Skill_PublicMessageExcludePlayers(
                                $"{mafiaBoss.targetPlayer.GetColoredName()} - {targetRole} " +
                                $"заморожен насмерть, {ColorString.GetColoredSkill("Упс")}!",
                                room, mafiaBossRole.skill_MafiaBossUps, excludedPlayers);
                        }
                    );

                stopVisit = true;
            }

            if (!stopVisit && mafiaBossRole.IsKiller())
            {

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                    RoleType.MafiaBoss,
                    NightActionId.Role,
                    () =>
                        {
                            room.roomChat.PublicMessage(
                                $"{ColorString.GetColoredRole(RoleType.MafiaBoss)} насмерть заморозил игрока " +
                                $"{mafiaBoss.targetPlayer.GetColoredName()} - " +
                                $"{targetRole}");
                        }
                );
            }

            //отправляем цель босса мафии в морг
            room.roomLogic.SendPlayerToMorgue(mafiaBoss.targetPlayer);

            mafiaBoss.targetPlayer.SetKiller(mafiaBoss);
        }

        private MafiaBoss GetRole()
        {
            MafiaBoss role = null;

            if (mafiaBoss.oldRole != null)
            {
                role = (MafiaBoss)mafiaBoss.oldRole;
            }
            else
            {
                role = (MafiaBoss)mafiaBoss.playerRole;
            }

            return role;
        }
    }
}
