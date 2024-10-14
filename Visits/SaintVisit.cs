using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class SaintVisit : RoleVisit
    {
        public SaintVisit(Room room) : base(room)
        {

        }

        List<BasePlayer> saints;
        public void Setup()
        {
            saints = new List<BasePlayer>();

            foreach (var p in room.players.Values)
            {
                if (p.playerRole.roleType == RoleType.Saint && p.isLive())
                {
                    saints.Add(p);
                }
            }
        }

        public void Visit()
        {
            if (saints.Count == 0) return;

            var succesCount = 0;
            var targetSuccesCount = 0;

            if (saints.Count > 2)
            {
                targetSuccesCount = saints.Count - 1;
            }
            else
            {
                targetSuccesCount = saints.Count;
            }

            //проверяем, что вербовка началась
            BasePlayer saintsAttemptTarget = null;
            var saintsAttemptSuccess = true;
            for (int i = 0; i < saints.Count; i++)
            {
                //запоминаем цель первого святого
                if (i == 0)
                {
                    saintsAttemptTarget = saints[i].targetPlayer;
                }

                //если цель святого отсутствует
                if (saints[i].targetPlayer == null)
                {
                    saintsAttemptSuccess = false; break;
                }

                //если цель святого мертва
                if (!saints[i].targetPlayer.isLive())
                {
                    saintsAttemptSuccess = false; break;
                }               

                //если цели святых не совпали
                if (i != 0 && saints[i].targetPlayer != saintsAttemptTarget)
                {
                    saintsAttemptSuccess = false; break;
                }

                //если мафиози может ходить
                if (saints[i].playerRole.CanVisit())
                {
                    //для текущего мафиози в цикле считаем покушение удавшимся
                    succesCount++;
                }
            }

            //если кол-во успешных покушений меньше минимально необходимого, то групповое покушение не удалось
            if (succesCount < targetSuccesCount)
            {
                saintsAttemptSuccess = false;
            }

            //проверяем защиту цели от покушения
            if (saintsAttemptSuccess)
            {
                if (saintsAttemptTarget.playerRole.CheckResistExtras(saints[0]))
                {
                    saintsAttemptSuccess = false;
                }
            }

            //проверяем, что вербовка удалась
            if (saintsAttemptSuccess )
            {
                //проверяем срабатывание навыка "не в духе"
                //если цель вербовки не комиссар
                if(saintsAttemptTarget.playerRole.roleType != RoleType.Commissar)
                {
                    var saintBadMood = false;
                    List<BasePlayer> badMoodList = new List<BasePlayer>();
                    Skill badMoodSkill = null;
                    foreach (var s in saints)
                    {
                        var saintRole = (Saint)s.playerRole;

                        if (saintRole.Check_SaintBadMood())
                        {
                            saintBadMood = true;
                            badMoodList.Add(s);

                            badMoodSkill = saintRole.skill_SaintBadMood;

                            //персональное сообщение святому у которого сработал навык
                            room.roomLogic.AddNightActionMessage
                                (
                                s,
                                NightActionId.Skill,
                                () =>
                                {
                                room.roomChat.Skill_PersonalMessage(s, saintRole.skill_SaintBadMood,
                                    $"{saintsAttemptTarget.GetColoredName()} - {saintsAttemptTarget.GetColoredRole()} " +
                                    $"убит {ColorString.GetColoredRole("Святым")}, он сегодня " +
                                    $"был {ColorString.GetColoredSkill("Не в духе")}. " +
                                    $"Ваш навык сработал!");                           
                                }
                                );
                        }
                    }

                    //если сработал навык "не в духе"
                    if (saintBadMood)
                    {
                        room.roomLogic.SendPlayerToMorgue(saintsAttemptTarget);

                        room.roomLogic.AddNightActionMessage
                        (
                        badMoodList[0],
                        NightActionId.Skill,
                        () =>
                        {        
                            //персональное сообщение жертве навыка
                            room.roomChat.Skill_PersonalMessage(saintsAttemptTarget, badMoodSkill,
                                $"{saintsAttemptTarget.GetColoredName()} - {saintsAttemptTarget.GetColoredRole()} " +
                                $"убит {ColorString.GetColoredRole("Святым")}, он сегодня " +
                                $"был {ColorString.GetColoredSkill("Не в духе")}. " +
                                $"Вас убили, увы!");

                            var excludedPlayers = new BasePlayer[badMoodList.Count+1];

                            for (int i = 0; i < badMoodList.Count; i++)
                            {
                                excludedPlayers[i] = badMoodList[i];
                            }

                            excludedPlayers[excludedPlayers.Length - 1] = saintsAttemptTarget;

                            //общее сообщение всем, кроме святых, у которых сработал навык и жертвы
                            room.roomChat.Skill_PublicMessageExcludePlayers(
                                $"{saintsAttemptTarget.GetColoredName()} - {saintsAttemptTarget.GetColoredRole()} " +
                                $"убит {ColorString.GetColoredRole("Святым")}, он сегодня " +
                                $"был {ColorString.GetColoredSkill("Не в духе")}",
                                room, badMoodSkill, excludedPlayers);
                        }
                        );

                        return;
                    }
                } 

                //результат вербовки святыми
                var saintRecruitSucces = false;
                //участники вербовки
                List<BasePlayer> recruiterList = new List<BasePlayer>();

                if(saintsAttemptTarget.team.teamType != TeamType.Good) 
                {
                    foreach (var s in saints)
                    {
                        var saintRole = (Saint)s.playerRole;

                        var recruitChance = room.dice.Next(0, 100);

                        if (recruitChance < 50)
                        {
                            saintRecruitSucces = true;
                            recruiterList.Add(s);
                        }
                    }
                }               

                //если вербовка прошла успешно
                if (saintRecruitSucces)
                {
                    var saintString = "";

                    if (saints.Count > 1)
                    {
                        saintString = $"{ColorString.GetColoredRole("Святому")}";
                    }
                    else
                    {
                        saintString = $"{ColorString.GetColoredRole("Святым")}";
                    }

                    //запоминаем роль жервы до вербовки
                    var targetRole = saintsAttemptTarget.GetColoredRole();

                    room.roomLogic.  AddNightActionMessage
                    (
                    saints[0],
                    NightActionId.Role,
                    () =>
                    {
                    room.roomChat.PublicMessage($"{saintString} удалось " +
                        $"забрать {saintsAttemptTarget.GetColoredName()} - {targetRole} на светлую сторону " +
                        $"и теперь он играет за {ColorString.GetColoredRole("Святого")}");
                    }
                    );

                    room.roomRoles.goodTeam.AddPlayer(saintsAttemptTarget);
                    RoleHelper.ChangePlayerRole(saintsAttemptTarget, RoleType.Saint);

                    //открываем аватарку нового святого всем игрокам
                    RoleHelper.UnlockRole_PlayerToRoom(saintsAttemptTarget);

                    //открываем новому святому аватарки всех святых
                    RoleHelper.UnlockRole_GroupToPlayer(saints, saintsAttemptTarget);
                }
                else
                {
                    AddFailedMessage();
                }
            }
            else
            {
                AddFailedMessage();
            }
        }

        private void AddFailedMessage()
        {
            var saintString = "";

            if (saints.Count > 1)
            {
                saintString = $"{ColorString.GetColoredRole("Святому")}";
            }
            else
            {
                saintString = $"{ColorString.GetColoredRole("Святым")}";
            }

            room.roomLogic.AddNightActionMessage
            (
            saints[0],
            NightActionId.Role,
            () =>
            {
                room.roomChat.PublicMessage($"{saintString} не удалось " +
                    $"исправить игрока");
            }
            );
        }

        public void CheckSaintGuard(BasePlayer comissar)
        {
            if (comissar == null) return;

            if (comissar.isLive()) return;

            var comissarResurected = false;
            List<BasePlayer> resurecterList = new List<BasePlayer>();
            Skill skillSaintGuard = null;
            foreach (var s in saints)
            {
                if (s.isLive() && s.targetPlayer == comissar)
                {
                    var saintRole = (Saint)s.playerRole;

                    if (saintRole.Check_SaintGuard())
                    {
                        comissarResurected = true;
                        resurecterList.Add(s);

                        skillSaintGuard = saintRole.skill_SaintGuard;

                        //персональное сообщение каждому святому, у которого сработал "оберег"
                        room.roomLogic.AddNightActionMessage
                        (
                        s,
                        NightActionId.Skill,
                        () =>
                        {
                            room.roomChat.Skill_PersonalMessage(s, saintRole.skill_SaintGuard,
                                    $"{ColorString.GetColoredRole("Комиссар")} был спасен " +
                                    $"{ColorString.GetColoredRole("Святым")}, благодаря навыку " +
                                    $"{ColorString.GetColoredSkill("Оберег")}. Ваш навык сработал");
                        }
                        );
                    }
                }
            }

            if (comissarResurected)
            {
                room.roomLogic.ResurectPlayer(comissar);

                room.roomLogic.AddNightActionMessage
                (
                saints[0],
                NightActionId.Skill,
                () =>
                {
                    var excludedPlayers = resurecterList.ToArray();

                    room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{ColorString.GetColoredRole("Комиссар")} был спасен " +
                            $"{ColorString.GetColoredRole("Святым")}, благодаря навыку " +
                            $"{ColorString.GetColoredSkill("Оберег")}",
                            room, skillSaintGuard, excludedPlayers);
                }
                );
            }
        }
    }
}
