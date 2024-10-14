using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class WitnessVisit:RoleVisit
    {
        public WitnessVisit(Room room) : base(room)
        {

        }

        BasePlayer witness;
        public void Setup()
        {
            witness = RoomHelper.FindPlayerByRole(RoleType.Witness, room);

            if (witness == null) return;

            witness.SetActiveAtNight(witness.targetPlayer != null);

            //проверяем, что цель жива на начало ночи
            if (witness.targetPlayer != null && !witness.targetPlayer.isLive())
            {
                witness.targetPlayer = null;
                return;
            }
        }

        public void Visit()
        {
            //если свидетеля нет
            if (witness == null) return;

            //если у свидетеля нет цели
            if (witness.targetPlayer == null) return;

            //если свидетель не может сделать ход
            if (!witness.playerRole.CanVisit()) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (witness.targetPlayer.playerRole.CheckResistExtras(witness)) return;

            if ((witness.targetPlayer.isLive() && witness.targetPlayer.killer == null)
                               ||
                               (!witness.targetPlayer.isLive() && witness.targetPlayer.killer == null))
            {
                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Witness,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage(
                        $"{ColorString.GetColoredRole("Свидетель")} был не там, где надо");
                }
                );
            }

            if ((witness.targetPlayer.isLive() && witness.targetPlayer.killer != null)
                ||
                (!witness.targetPlayer.isLive() && witness.targetPlayer.killer != null))
            {

                var killerRole = witness.targetPlayer.killer.GetColoredRole();

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Witness,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.Role_PublicMessage(
                        RoleType.Witness,
                        $"{ColorString.GetColoredRole("Свидетель")} застал " +
                        $"{witness.targetPlayer.killer.GetColoredName()} - " +
                        $"{killerRole} на месте преступления ");
                }
                );

                RoleHelper.UnlockRole_PlayerToRoom(witness.targetPlayer.killer);

                var witnessRole = GetRole();

                if (witnessRole.Check_WitnessCarma())
                {
                    var killer = witness.targetPlayer.killer;

                    var targetKillerRole = killer.GetColoredRole();

                    room.roomLogic.SendPlayerToMorgue(killer);

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Witness,
                    NightActionId.Skill,
                    () =>
                    {
                        room.roomChat.Skill_PersonalMessage(witness, witnessRole.skill_WitnessCarma,
                            $"{killer.GetColoredName()} - {targetKillerRole} " +
                            $"умер от {ColorString.GetColoredSkill("Кармы")} " +
                            $"{ColorString.GetColoredRole("Свидетеля")}. " +
                            $"Ваш навык сработал");

                        room.roomChat.Skill_PersonalMessage(killer, witnessRole.skill_WitnessCarma,
                            $"{killer.GetColoredName()} - {targetKillerRole} " +
                            $"умер от {ColorString.GetColoredSkill("Кармы")} " +
                            $"{ColorString.GetColoredRole("Свидетеля")}. " +
                            $"Вы убиты, увы!");

                        var excludedPlayers = new BasePlayer[] { witness, killer };

                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{killer.GetColoredName()} - {targetKillerRole} " +
                            $"умер от {ColorString.GetColoredSkill("Кармы")} " +
                            $"{ColorString.GetColoredRole("Свидетеля")}",
                            room, witnessRole.skill_WitnessCarma, excludedPlayers);
                    }
                    );
                }
            }
        }

        public void CheckWitnessFriends(BasePlayer comissar)
        {
            if (witness == null) return;

            if (comissar == null) return;

            if (comissar.isLive()) return;

            if (witness.targetPlayer != comissar) return;

            var witnessRole = GetRole();

            if (witnessRole.Check_WitnessFriends())
            {
                room.roomLogic.ResurectPlayer(comissar);

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Witness,
                NightActionId.Skill,
                () =>
                {
                    room.roomChat.Skill_PersonalMessage(witness, witnessRole.skill_WitnessFriends,
                        $"{comissar.GetColoredName()} - {comissar.GetColoredRole()} " +
                        $"воскрес, благодаря навыку {ColorString.GetColoredSkill("Друзья")}. " +
                        $"Вы спасли жизнь {ColorString.GetColoredRole("Комиссару")}");

                    room.roomChat.Skill_PersonalMessage(comissar, witnessRole.skill_WitnessFriends,
                        $"{comissar.GetColoredName()} - {comissar.GetColoredRole()} " +
                        $"воскрес, благодаря навыку {ColorString.GetColoredSkill("Друзья")}. " +
                        $"{ColorString.GetColoredRole("Свидетель")} спас Вам жизнь");

                    var excludedPlayers = new BasePlayer[] { witness, comissar };

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{comissar.GetColoredName()} - {comissar.GetColoredRole()} " +
                        $"воскрес, благодаря навыку {ColorString.GetColoredSkill("Друзья")}",
                        room, witnessRole.skill_WitnessFriends, excludedPlayers);
                }
                );
            }
        }

        public Witness GetRole()
        {
            Witness role = null;

            if (witness.oldRole != null)
            {
                role = (Witness)witness.oldRole;
            }
            else
            {
                role = (Witness)witness.playerRole;
            }

            return role;
        }

        public Witness GetRole(BasePlayer player)
        {
            Witness role = null;

            if (player.oldRole != null)
            {
                role = (Witness)player.oldRole;
            }
            else
            {
                role = (Witness)player.playerRole;
            }

            return role;
        }
    }
}
