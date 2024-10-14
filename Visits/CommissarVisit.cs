using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class CommissarVisit:RoleVisit
    {
        public CommissarVisit(Room room) : base(room)
        {

        }

        public BasePlayer comissar { get; private set; }
        public void Setup()
        {
            comissar = RoomHelper.FindPlayerByRole(RoleType.Commissar, room);           
        }

        public void Visit()
        {
            //если комиссара нет
            if (comissar == null) return;

            //если у комиссара нет цели
            if (comissar.targetPlayer == null) return;

            //если комиссар не может сделать ход
            if (!comissar.playerRole.CanVisit()) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (comissar.targetPlayer.playerRole.CheckResistExtras(comissar)) return;

            //роль комиссара
            var comissarRole = (Commissar)comissar.playerRole;

            //применяем к цели комиссара защиту до кона ночи
            if (!comissarRole.PlayerIsVisited(comissar.targetPlayer))
            {
                //применяем к цели защиту комиссара до конца ночи
                RoleHelper.ApplyRoleEffectForced(room, comissar, DurationType.NightEnd, comissar.targetPlayer);

                //запоминаем посещенного игрока
                comissarRole.AddVisitedPlayer(comissar.targetPlayer);
            }          

            //если комиссар пошел к мирной роли
            if (comissar.targetPlayer.team == room.roomRoles.goodTeam)
            {
                room.roomLogic.AddNightActionMessage
                   (
                   comissar,
                   NightActionId.Role,
                   () =>
                   {
                       room.roomChat.PublicMessage(
                            $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, {comissar.targetPlayer.GetColoredName()} " +
                            $"играет за - " +
                            $"{ColorString.GetColoredRole("Гражданина")}");

                       room.roomChat.PersonalMessage(
                           comissar, $"{comissar.targetPlayer.GetColoredName()} - играет за " +
                           $"{ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType)}");
                   }
                   );

                //выводим для всех кроме кома и цели ложную роль
                var fakeRoleGroup = new List<BasePlayer>();
                foreach (var p in room.players.Values)
                {
                    if (p == comissar || p == comissar. targetPlayer) continue;

                    fakeRoleGroup.Add(p);
                }
                RoleHelper.UnlockRole_PlayerToGroup(fakeRoleGroup, comissar.targetPlayer, RoleType.Citizen);

                //раскрыть настоящую роль посещенного для комиссара
                RoleHelper.UnlockRole_PlayerToPlayer(comissar.targetPlayer, comissar, comissar.targetPlayer.playerRole.roleType);

                comissarRole.Check_CommissarGift(comissar.targetPlayer);
            }

            //если комиссар пошел к злым ролям
            if (comissar.targetPlayer.team != room.roomRoles.goodTeam)
            {
                //если цель комиссара мафиози и его спас хамелеон
                if (CheckMafiaChameleon()) return;

                //если у цели есть дубликат 
                if (CheckTargetDublicat(comissarRole)) return;

                //раскрываем роль цели
                var targetRole = ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType);

                RoleHelper.UnlockRole_PlayerToRoom(comissar.targetPlayer, comissar.targetPlayer.playerRole.roleType);

                room.roomLogic.AddNightActionMessage
                        (
                        comissar,
                        NightActionId.Role,
                        () =>
                        {
                            room.roomChat.PublicMessage
                              ($"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                              $"{comissar.targetPlayer.GetColoredName()} " +
                              $"играет за - {targetRole}");
                        }
                        );

                //проверяем удалось ли комиссару раскрыть коллег мафиози
                comissarRole.Check_CommissarMafiaColleagues(comissar.targetPlayer); 
            }
        }

        private bool CheckMafiaChameleon()
        {
            if (comissar.targetPlayer.playerRole.roleType == RoleType.Mafia)
            {
                var mafiaRole = (Mafia)comissar.targetPlayer.playerRole;

                //проверяем спас ли "хамелеон" мафиози
                if (mafiaRole.Check_MafiaChameleon())
                {
                    RoleHelper.UnlockRole_PlayerToRoom(comissar.targetPlayer, RoleType.Citizen);
                    //owner.room.roomLogic.UnlockPlayerRoleToOtherPlayer(targetPlayer, owner, RoleType.Citizen);

                    room.roomLogic.AddNightActionMessage
                    (
                    comissar,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage(
                            $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, {comissar.targetPlayer.GetColoredName()} " +
                            $"играет за - " +
                            $"{ColorString.GetColoredRole("Гражданина")}");
                    }
                    );

                    room.roomLogic.AddNightActionMessage
                    (
                    comissar.targetPlayer,
                    NightActionId.Skill,
                    () =>
                    {
                        room.roomChat.Skill_PersonalMessage(comissar.targetPlayer, mafiaRole.skill_MafiaChameleon,
                            $"{ColorString.GetColoredSkill("Хамелеон")} " +
                            $"помог Вам скрыть истинную роль");
                    }
                    );

                    return true;
                }
            }

            return false;
        }

        private bool CheckTargetDublicat(Commissar comissarRole)
        {
            //проверяем у цели наличие дубликата
            var extraDublicat = comissar.targetPlayer.FindExtraInSlots(ExtraEffect.Dublicat);

            //если дубликат есть
            if (extraDublicat != null)
            {
                //если комиссару удалось прорваться через дубликат
                if (comissarRole.Check_CommissarThroughLie())
                {
                    RoleHelper.UnlockRole_PlayerToRoom(comissar.targetPlayer, comissar.targetPlayer.playerRole.roleType);
                    //owner.room.roomLogic.UnlockPlayerRoleToOtherPlayer(targetPlayer, owner, targetPlayer.playerRole.roleType);

                    var targetRole = ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType);

                    room.roomLogic.AddNightActionMessage
                            (
                            comissar,
                            NightActionId.Skill,
                            () =>
                            {
                                room.roomChat.PublicMessage
                                  ($"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                                  $"{comissar.targetPlayer.GetColoredName()} " +
                                  $"играет за - {targetRole}");

                                room.roomChat.Skill_PersonalMessage(
                                    comissar.targetPlayer, comissarRole.skill_CommissarThroughLie,
                                    $"{ColorString.GetColoredRole("Комиссар")} игнорировал Ваш " +
                                    $"{ColorString.GetColoredExtra("Дубликат")} при помощи навыка " +
                                    $"{ColorString.GetColoredSkill("Сквозь обман")}");
                            }
                            );
                }
                else
                {
                    RoleHelper.UnlockRole_PlayerToRoom(comissar.targetPlayer, RoleType.Citizen);
                    //owner.room.roomLogic.UnlockPlayerRoleToOtherPlayer(targetPlayer, owner, RoleType.Citizen);

                    room.roomLogic.AddNightActionMessage
                      (
                      comissar,
                      NightActionId.Role,
                      () =>
                      {
                          room.roomChat.PublicMessage
                              ($"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                              $"{comissar.targetPlayer.GetColoredName()} " +
                              $"играет за - " +
                              $"{ColorString.GetColoredRole("Гражданина")}");
                      }
                      );

                    room.roomLogic.AddNightActionMessage
                        (
                        comissar.targetPlayer,
                        NightActionId.Extra,
                        () =>
                        {
                            room.roomChat.Extra_PersonalMessage(
                                comissar.targetPlayer, extraDublicat,
                                $"{ColorString.GetColoredExtra("Дубликат")} " +
                                $"помог Вам скрыть истинную роль");
                        }
                        );
                }

                extraDublicat.DecreaseCount();

                return true;
            }

            return false;
        }
    }
}
