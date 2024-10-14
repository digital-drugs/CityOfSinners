using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class ManiacVisit : RoleVisit
    {
        public ManiacVisit(Room room) : base(room)
        {

        }

        BasePlayer maniac;
        public void Setup()
        {
            maniac = RoomHelper.FindPlayerByRole(RoleType.Maniac, room);
        }

        public void Visit()
        {
            //если маньяка нет
            if (maniac == null) return;

            //если у маньяка нет цели
            if (maniac.targetPlayer == null) return;

            //если маньяк не может сделать ход
            if (maniac.playerRole.CanVisit() == false) return;

            //если цель защищена зеркалом

            //если цель защищена ролями
            if (maniac.targetPlayer.playerRole.CheckResistRoles(maniac)) return;

            //если цель защищена экстрами
            if (maniac.targetPlayer.playerRole.CheckResistExtras(maniac)) return;

            var maniacRole = (Maniac)maniac.playerRole;

            if (maniacRole.idealUsed)
            {
                var targetRole = maniac.targetPlayer.GetColoredRole();

                room.roomLogic.AddNightActionMessage
                (
                maniac,
                NightActionId.Skill,
                () =>
                {
                    room.roomChat.Skill_PersonalMessage(
                        maniac, maniacRole.skill_ManiacIdeal,
                        $"{maniac.targetPlayer.GetColoredName()} - {targetRole} " +
                        $"убит, сработал навык {ColorString.GetColoredSkill("Я идеальный")} " +
                        $"от {ColorString.GetColoredRole("Маньяка")}. " +
                        $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(
                        maniac.targetPlayer, maniacRole.skill_ManiacIdeal,
                        $"{maniac.targetPlayer.GetColoredName()} - {targetRole} " +
                        $"убит, сработал навык {ColorString.GetColoredSkill("Я идеальный")} " +
                        $"от {ColorString.GetColoredRole("Маньяка")}. " +
                        $"Вы убиты, увы!");

                    var excludedPlayers = new BasePlayer[] { maniac, maniac.targetPlayer };

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{maniac.targetPlayer.GetColoredName()} - {targetRole} " +
                        $"убит, сработал навык {ColorString.GetColoredSkill("Я идеальный")} " +
                        $"от {ColorString.GetColoredRole("Маньяка")}",
                        room, maniacRole.skill_ManiacIdeal, excludedPlayers);
                }
                );
            }
            else
            {
                var targetRole = maniac.targetPlayer.GetColoredRole();

                room.roomLogic. AddNightActionMessage
                (
                maniac,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage(
                        $"{maniac.GetColoredRole()} убил {maniac.targetPlayer.GetColoredName()} " +
                        $"- {targetRole}");
                }
                );
            }

            room.roomLogic.SendPlayerToMorgue(maniac.targetPlayer);

            maniac.targetPlayer.SetKiller(maniac);

            CheckMafiaManiac();
        }

        private void CheckMafiaManiac()
        {
            if (maniac.targetPlayer.playerRole.roleType != RoleType.Mafia) return;

            var mafiaRole = (Mafia)maniac.targetPlayer.playerRole;

            if (mafiaRole.Check_MafiaManiac())
            {
                room.roomLogic.SendPlayerToMorgue(maniac);

                var maniacRole = maniac.GetColoredRole();

                room.roomLogic.AddNightActionMessage
                (
                maniac.targetPlayer,
                NightActionId.Skill,
                () =>
                {
                    room.roomChat.Skill_PersonalMessage(
                        maniac.targetPlayer, mafiaRole.skill_MafiaManiac,
                        $"{maniac.GetColoredName()} - {maniacRole} " +
                        $"был забран в могилу навыком " +
                        $"{ColorString.GetColoredSkill("Месть маньяку")}. Вы отомстили");

                    room.roomChat.Skill_PersonalMessage(
                        maniac, mafiaRole.skill_MafiaManiac,
                        $"{maniac.GetColoredName()} - {maniacRole} " +
                        $"был забран в могилу навыком {ColorString.GetColoredSkill("Месть маньяку")}. Вы убиты, увы!");

                    var excludedPlayers = new BasePlayer[] { maniac, maniac.targetPlayer };

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{maniac.GetColoredName()} - {maniacRole} " +
                        $"был забран в могилу навыком {ColorString.GetColoredSkill("Месть маньяку")}",
                        room, mafiaRole.skill_MafiaManiac, excludedPlayers);
                }
                );
            }
        }
    }
}
