using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    class WerewolfVisit : RoleVisit
    {
        public WerewolfVisit(Room room) : base(room)
        {

        }

        BasePlayer werewolf;
        public void Setup()
        {
            werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);

            if (werewolf == null) return;

            werewolf.SetActiveAtNight(werewolf.targetPlayer != null);

            //проверяем, что цель жива на начало ночи
            if (werewolf.targetPlayer != null && !werewolf.targetPlayer.isLive())
            {
                werewolf.targetPlayer = null;
                return;
            }
        }

        public void Visit()
        {
            //если оборотня нет
            if (werewolf == null) return;

            //если у оборотня нет цели
            if (werewolf.targetPlayer == null) return;

            //если цель оборотня мертва
            if (werewolf.targetPlayer.isLive() == false) return;

            //если оборотень не может сделать ход
            if (werewolf.playerRole.CanVisit() == false) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (werewolf.targetPlayer.playerRole.CheckResistExtras(werewolf)) return;

            //если цель защищена ролями
            if (werewolf.targetPlayer.playerRole.CheckResistRoles(werewolf)) return;          

            //если цель защищена скиллами
            if (werewolf.targetPlayer.playerRole.CheckResistSkills(werewolf)) return;

            var werewolfRole = GetRole();

            if (werewolfRole.Check_WerewolfRage())
            {
                //сначала ищем цели рядом с жертвой
                var skillTargets = RoomHelper.FindNearPlayers(room, werewolf, werewolf.targetPlayer, 2, true, true);

                //отправляем основную жертву в морг
                room.roomLogic. SendPlayerToMorgue(werewolf.targetPlayer);
                werewolf.targetPlayer.SetKiller(werewolf);

                var message = $"{werewolf.targetPlayer.GetColoredName()} - {werewolf.targetPlayer.GetColoredRole()}\n";

                foreach (var t in skillTargets)
                {
                    message += $"{t.GetColoredName()} - {t.GetColoredRole()}\n";
                    room.roomLogic.SendPlayerToMorgue(t);
                    t.SetKiller(werewolf);
                }

                message += $"были убиты навыком {ColorString.GetColoredSkill("Ярость")} " +
                    $"{ColorString.GetColoredRole("Оборотня")}";

                var werewolfMessage = message + $". Ваш навык сработал";

                var targetMessage = message + $". Вы убиты, увы!";

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Werewolf,
                NightActionId.Skill,
                () =>
                {
                    room.roomChat.Skill_PersonalMessage(
                        werewolf, werewolfRole.skill_WerewolfRage, werewolfMessage);

                    //основная цель
                    room.roomChat.Skill_PersonalMessage(
                        werewolf.targetPlayer, werewolfRole.skill_WerewolfRage, targetMessage);

                    //1 доп цель
                    if (skillTargets.Count > 0)
                    {
                        room.roomChat.Skill_PersonalMessage(
                            skillTargets[0], werewolfRole.skill_WerewolfRage, targetMessage);
                    }

                    //2 доп цель
                    if (skillTargets.Count > 1)
                    {
                        room.roomChat.Skill_PersonalMessage(
                            skillTargets[1], werewolfRole.skill_WerewolfRage, targetMessage);
                    }

                    var excludedPlayersList = new List<BasePlayer>();
                    excludedPlayersList.AddRange(skillTargets);
                    excludedPlayersList.Add(werewolf);
                    excludedPlayersList.Add(werewolf.targetPlayer);

                    //остальные
                    var excludedPlayers = excludedPlayersList.ToArray();

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        message, room, werewolfRole.skill_WerewolfRage, excludedPlayers);
                }
                );
            }
            else
            {
                var targetRole = werewolf.targetPlayer.GetColoredRole();

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Werewolf,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage(
                        $"{ColorString.GetColoredRole("Оборотень")} " +
                        $"распотрошил {werewolf.targetPlayer.GetColoredName()} " +
                        $"- {targetRole}");
                }
                );

                //отправляем цель оборотня в морг
                room.roomLogic.SendPlayerToMorgue(werewolf.targetPlayer);
                werewolf.targetPlayer.SetKiller(werewolf);
            }
        }

        private Werewolf GetRole()
        {
            Werewolf role;

            if (werewolf.oldRole != null)
            {
                role = (Werewolf)werewolf.oldRole;
            }
            else
            {
                role = (Werewolf)werewolf.playerRole;
            }

            return role;
        }
    }
}
