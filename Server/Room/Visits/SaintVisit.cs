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

            foreach (var p in saints)
            {
                p.SetActiveAtNight(p.targetPlayer != null);
            }

            ////проверяем, что цель жива на начало ночи
            //if (maniac.targetPlayer != null && !maniac.targetPlayer.isLive())
            //{
            //    maniac.targetPlayer = null;
            //    return;
            //}
        }

        public void Visit()
        {
            if (room.roomLogic.sinnerVisit.doubleKill) return;

            if (saints.Count == 0) return;

            //var succesCount = 0;
            var targetSuccesCount = 0;

            if (saints.Count > 2)
            {
                targetSuccesCount = saints.Count - 1;
            }
            else
            {
                targetSuccesCount = saints.Count;
            }

            Dictionary<BasePlayer, int> attemptTargets = new Dictionary<BasePlayer, int>();

            //проверяем, что вербовка началась
            BasePlayer saintsAttemptTarget = null;
            var saintsAttemptSuccess = false;
            for (int i = 0; i < saints.Count; i++)
            {
                //если мафиози может ходить
                if (saints[i].playerRole.CanVisit())
                {
                    //если цель святого отсутствует
                    if (saints[i].targetPlayer == null)
                    {
                        continue;
                    }

                    //если цель святого мертва
                    if (!saints[i].targetPlayer.isLive())
                    {
                        continue;
                    }
                    
                    //зеркало                    

                    //если эта цель была уже кем-то атакована, увеличиваем счетчик атакующих
                    if (attemptTargets.ContainsKey(saints[i].targetPlayer))
                    {
                        attemptTargets[saints[i].targetPlayer]++;
                    }
                    else
                    {
                        attemptTargets.Add(saints[i].targetPlayer, 1);
                    }
                }
            }

            foreach (var at in attemptTargets)
            {
                if (at.Value >= targetSuccesCount)
                {
                    saintsAttemptSuccess = true;
                    saintsAttemptTarget = at.Key;
                }
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
                        var saintRole = GetRole(s);

                        if (saintRole.Check_SaintBadMood())
                        {
                            saintBadMood = true;
                            badMoodList.Add(s);

                            badMoodSkill = saintRole.skill_SaintBadMood;

                            //персональное сообщение святому у которого сработал навык
                            room.roomLogic.nightActionMessages.AddNightActionMessage
                                (
                                RoleType.Saint,
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

                        room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Saint,
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
                        var saintRole = GetRole(s);

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
                        saintString = $"{ColorString.GetColoredRole("Святым")}";                        
                    }
                    else
                    {
                        saintString = $"{ColorString.GetColoredRole("Святому")}";
                    }

                    //запоминаем роль жервы до вербовки
                    var targetRole = saintsAttemptTarget.GetColoredRole();

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Saint,
                    NightActionId.Role,
                    () =>
                    {
                    room.roomChat.PublicMessage($"{saintString} удалось " +
                        $"забрать {saintsAttemptTarget.GetColoredName()} - {targetRole} на светлую сторону " +
                        $"и теперь он играет за {ColorString.GetColoredRole("Святого")}");

                        RoomHelper.AddPlayerToTeam(room, saintsAttemptTarget, TeamType.Good);

                        RoleHelper.ChangePlayerRole(saintsAttemptTarget, RoleType.Saint);

                        //открываем аватарку нового святого всем игрокам
                        RoleHelper.UnlockRole_PlayerToRoom(saintsAttemptTarget);

                        //открываем новому святому аватарки всех святых
                        RoleHelper.UnlockRole_GroupToPlayer(saints, saintsAttemptTarget);
                    }
                    );                   
                }
                else
                {
                    AddFailedMessage();
                }
            }
           
            if(!saintsAttemptSuccess && attemptTargets.Count>1)
            {
                var saintString = $"{ColorString.GetColoredRole("Святые")}";             

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Saint,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage($"{saintString} не определились");
                }
                );
            }
        }

        private void AddFailedMessage()
        {
            var saintString = "";

            if (saints.Count > 1)
            {
                saintString = $"{ColorString.GetColoredRole("Святым")}";
            }
            else
            {
                saintString = $"{ColorString.GetColoredRole("Святому")}";
            }

            room.roomLogic.nightActionMessages.AddNightActionMessage
            (
            RoleType.Saint,
            NightActionId.Role,
            () =>
            {
                room.roomChat.PublicMessage($"{saintString} не удалось " +
                    $"исправить игрока");
            }
            );
        }

        public List<BasePlayer> guardList { get; private set; } = new List<BasePlayer>();
        public Skill skillSaintGuard { get; private set; }
        public bool CheckSaintGuard(BasePlayer comissar)
        {
            //if (comissar == null) return;

            //if (comissar.isLive()) return;

            skillSaintGuard = null;

            var comissarGuarded = false;
            guardList = new List<BasePlayer>();
            
            foreach (var s in saints)
            {
                if (s.isLive() && s.targetPlayer == comissar)
                {
                    var saintRole = GetRole(s);

                    if (saintRole.Check_SaintGuard())
                    {
                        comissarGuarded = true;
                        guardList.Add(s);

                        skillSaintGuard = saintRole.skill_SaintGuard;                       
                    }
                }
            }

            //if (comissarGuarded)
            //{
            //    SkillHelper.AddSkillEffectToPlayer(comissar, skillSaintGuard, DurationType.NightEnd);               
            //}

            return comissarGuarded;
        }

        public Saint GetRole(BasePlayer player)
        {
            Saint role;

            if (player.oldRole != null)
            {
                role = (Saint)player.oldRole;
            }
            else
            {
                role = (Saint)player.playerRole;
            }

            return role;
        }
    }
}
