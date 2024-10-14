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

            if (comissar == null) return;

            comissar.SetActiveAtNight(comissar.targetPlayer != null);

            //проверяем, что цель жива на начало ночи
            if (comissar.targetPlayer != null && !comissar.targetPlayer.isLive()) comissar.targetPlayer = null;
        }

        public void Visit(bool skipCapeExtra = true)
        {
            //если комиссара нет
            if (comissar == null) return;

            //если у комиссара нет цели
            if (comissar.targetPlayer == null) return;

            //проверить у цели наличие накидки
            var capeExtra = comissar.targetPlayer.FindExtraInSlots(ExtraEffect.Cape);
            if (capeExtra != null && skipCapeExtra)
            {
                return;
            }

            //если комиссар не может сделать ход
            if (!comissar.playerRole.CanVisit()) return;

            //если цель защищена зеркалом

            //если цель защищена экстрами
            if (comissar.targetPlayer.playerRole.CheckResistExtras(comissar)) return;

            //роль комиссара
            var comissarRole = GetRole();

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
                //раскрываем роль цели
                var targetRoleString = ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType);

                var targetRoleId = comissar.targetPlayer.playerRole.roleType;

                var targetPlayer = comissar.targetPlayer;

                room.roomLogic.nightActionMessages.AddNightActionMessage
                   (
                   RoleType.Commissar,
                   NightActionId.Role,
                   () =>
                   {
                       room.roomChat.Role_PublicMessage(
                           RoleType.Commissar,
                           $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, {targetPlayer.GetColoredName()} " +
                           $"играет за - " +
                           $"{ColorString.GetColoredRole("Гражданина")}");

                       room.roomChat.Role_PersonalMessage(
                           comissar, RoleType.Commissar,
                           $"{targetPlayer.GetColoredName()} - играет за " +
                           $"{targetRoleString}");

                       //выводим для всех кроме кома и цели ложную роль
                       var fakeRoleGroup = new List<BasePlayer>();
                       foreach (var p in room.players.Values)
                       {
                           if (p == comissar || p == targetPlayer) continue;

                           fakeRoleGroup.Add(p);
                       }
                       RoleHelper.UnlockRole_PlayerToGroup(fakeRoleGroup, targetPlayer, RoleType.Citizen);

                       //раскрыть настоящую роль посещенного для комиссара
                       RoleHelper.UnlockRole_PlayerToPlayer(targetPlayer, comissar, targetRoleId);

                       comissarRole.Check_CommissarGift(targetPlayer);
                   },
                   comissar
                   );
            }

            //если комиссар пошел к злым ролям
            if (comissar.targetPlayer.team != room.roomRoles.goodTeam)
            {
                //если цель комиссара мафиози и его спас хамелеон
                if (CheckMafiaChameleon())
                {
                    comissar.targetPlayer = null;
                    return; 
                }

                //если у цели есть дубликат 
                if (CheckTargetDublicat(comissarRole))
                {
                    comissar.targetPlayer = null;
                    return; 
                }

                //раскрываем роль цели
                var targetRoleString = ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType);

                var targetRoleId = comissar.targetPlayer.playerRole.roleType;

                var targetPlayer = comissar.targetPlayer;

                room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Commissar,
                        NightActionId.Role,
                        () =>
                        {
                            RoleHelper.UnlockRole_PlayerToRoom(targetPlayer, targetRoleId);

                            room.roomChat.Role_PublicMessage(RoleType.Commissar,
                                $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                                $"{targetPlayer.GetColoredName()} " +
                                $"играет за - {targetRoleString}");

                            //проверяем удалось ли комиссару раскрыть коллег мафиози
                            comissarRole.Check_CommissarMafiaColleagues(targetPlayer);
                        },
                        comissar
                        );              
            }

            comissar.targetPlayer = null;
        }

        private bool CheckMafiaChameleon()
        {
            if (comissar.targetPlayer.playerRole.roleType == RoleType.Mafia)
            {
                var mafiaRole = (Mafia)comissar.targetPlayer.playerRole;

                //проверяем спас ли "хамелеон" мафиози
                if (mafiaRole.Check_MafiaChameleon())
                {
                    //показываем ложную роль всем, кроме команды мафии
                    var players = new List<BasePlayer>();
                    foreach (var p in room.players.Values)
                    {
                        //совпадение по группам

                        //совпадение по ролям

                        //совпадение по команде
                        if (p.team.teamType == TeamType.Bad || p == comissar.targetPlayer) continue;

                        players.Add(p);
                    }

                    var targetPlayer = comissar.targetPlayer;

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Commissar,
                    NightActionId.Role,
                    () =>
                    {                     
                        RoleHelper.UnlockRole_PlayerToGroup(players, targetPlayer, RoleType.Citizen);

                        room.roomChat.Role_PublicMessage(
                            RoleType.Commissar,
                            $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, {targetPlayer.GetColoredName()} " +
                            $"играет за - " +
                            $"{ColorString.GetColoredRole("Гражданина")}");
                    },
                    comissar
                    );

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Mafia,
                    NightActionId.Skill,
                    () =>
                    {
                        room.roomChat.Skill_PersonalMessage(targetPlayer, mafiaRole.skill_MafiaChameleon,
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
                var targetRoleString = ColorString.GetColoredFromRole(comissar.targetPlayer.playerRole.roleType);

                var targetRoleId = comissar.targetPlayer.playerRole.roleType;

                var targetPlayer = comissar.targetPlayer;

                //если комиссару удалось прорваться через дубликат
                if (comissarRole.Check_CommissarThroughLie())
                {
                    room.roomLogic.nightActionMessages.AddNightActionMessage
                            (
                            RoleType.Commissar,
                            NightActionId.Skill,
                            () =>
                            {
                                RoleHelper.UnlockRole_PlayerToRoom(targetPlayer, targetRoleId);
                                //owner.room.roomLogic.UnlockPlayerRoleToOtherPlayer(targetPlayer, owner, targetPlayer.playerRole.roleType);

                                room.roomChat.Role_PublicMessage(
                                    RoleType.Commissar,
                                    $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                                    $"{targetPlayer.GetColoredName()} " +
                                    $"играет за - {targetRoleString}");

                                room.roomChat.Skill_PersonalMessage(
                                    targetPlayer, comissarRole.skill_CommissarThroughLie,
                                    $"{ColorString.GetColoredRole("Комиссар")} игнорировал Ваш " +
                                    $"{ColorString.GetColoredExtra("Дубликат")} при помощи навыка " +
                                    $"{ColorString.GetColoredSkill("Сквозь обман")}");
                            },
                            comissar
                            );
                }
                else
                {
                    room.roomLogic.nightActionMessages.AddNightActionMessage
                      (
                      RoleType.Commissar,
                      NightActionId.Role,
                      () =>
                      {
                          //показываем ложную роль 
                          var players = new List<BasePlayer>();
                          foreach (var p in room.players.Values)
                          {
                              //если дубликат у мафии
                              if(targetPlayer.team.teamType == TeamType.Bad)
                              {
                                  if(p.team.teamType == TeamType.Bad)
                                  {
                                      continue;
                                  }
                              }

                              //если дубликат у пирата
                              if (targetPlayer.team.teamType == TeamType.Pirate)
                              {
                                  if (p.team.teamType == TeamType.Pirate)
                                  {
                                      continue;
                                  }
                              }

                              //если дубликат у нейтрала
                              if (targetPlayer.team.teamType == TeamType.Neutral)
                              {
                                  if (targetPlayer ==  p)
                                  {
                                      continue;
                                  }
                              }

                              players.Add(p);
                          }

                          RoleHelper.UnlockRole_PlayerToGroup(players, targetPlayer, RoleType.Citizen);

                          room.roomChat.Role_PublicMessage(
                              RoleType.Commissar,
                              $"{ColorString.GetColoredRole("Комиссар")} раскрыл роль, " +
                              $"{targetPlayer.GetColoredName()} " +
                              $"играет за - " +
                              $"{ColorString.GetColoredRole("Гражданина")}");
                      },
                      comissar
                      );

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        targetRoleId,
                        NightActionId.Extra,
                        () =>
                        {
                            room.roomChat.Extra_PersonalMessage(
                                targetPlayer, extraDublicat,
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

        private Commissar GetRole()
        {
            Commissar role = null;

            if (comissar.oldRole != null)
            {
                role = (Commissar)comissar.oldRole;
            }
            else
            {
                role = (Commissar)comissar.playerRole;
            }

            return role;
        }
    }
}
