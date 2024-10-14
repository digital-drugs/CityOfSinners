using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class SinnerVisit : RoleVisit
    {
        public SinnerVisit(Room room) : base(room)
        {

        }

        List<BasePlayer> sinners;
        public void Setup()
        {
            sinners = new List<BasePlayer>();

            foreach (var p in room.players.Values)
            {
                if (p.playerRole.roleType == RoleType.Sinner && p.isLive())
                {
                    sinners.Add(p);
                }
            }

            foreach (var p in sinners)
            {
                p.SetActiveAtNight(p.targetPlayer != null);
            }

            //флаг, что святой и грешник убили друг друга
            doubleKill = false;

            ////проверяем, что цель жива на начало ночи
            //if (maniac.targetPlayer != null && !maniac.targetPlayer.isLive())
            //{
            //    maniac.targetPlayer = null;
            //    return;
            //}
        }

        //флаг, что святой и грешник убили друг друга
        public bool doubleKill { get; private set; } = false;

        public void Visit()
        {
            if (sinners.Count == 0) return;

            //var succesCount = 0;
            var targetSuccesCount = 0;

            if (sinners.Count > 2)
            {
                targetSuccesCount = sinners.Count - 1;
            }
            else
            {
                targetSuccesCount = sinners.Count;
            }

            Dictionary<BasePlayer, int> attemptTargets = new Dictionary<BasePlayer, int>();

            //проверяем что вербовка началась
            BasePlayer sinnersAttemptTarget = null;
            var sinnersAttemptSuccess = false;
            for (int i = 0; i < sinners.Count; i++)
            {
                //если грешник может ходить
                if (sinners[i].playerRole.CanVisit())
                {
                    //если цель грешника отсутствует
                    if (sinners[i].targetPlayer == null)
                    {
                        continue;
                    }

                    //если цель грешника мертва
                    if (!sinners[i].targetPlayer.isLive())
                    {
                        continue;
                    }

                    //если грешник наткнулся на команду мафии - ничего не делаем
                    //зеркало
                    if (sinners[i].targetPlayer.team.teamType == TeamType.Bad)
                    {
                        continue;
                    }

                    //если эта цель была уже кем-то атакована, увеличиваем счетчик атакующих
                    if (attemptTargets.ContainsKey(sinners[i].targetPlayer))
                    {
                        attemptTargets[sinners[i].targetPlayer]++;
                    }
                    else
                    {
                        attemptTargets.Add(sinners[i].targetPlayer, 1);
                    }
                }                
            }

            foreach (var at in attemptTargets)
            {
                if (at.Value >= targetSuccesCount)
                {
                    sinnersAttemptSuccess = true;
                    sinnersAttemptTarget = at.Key;
                }
            }

            //проверяем защиту цели от покушения
            if (sinnersAttemptSuccess)
            {
                if (sinnersAttemptTarget.playerRole.CheckResistExtras(sinners[0]))
                {
                    sinnersAttemptSuccess = false;
                }
            }

            //проверяем, что вербовка удалась
            if (sinnersAttemptSuccess)
            {
                //проверяем не убились ли грешник и святой друг об друга
                //если грешники наткнулись на святого
                if(sinnersAttemptTarget.playerRole.roleType == RoleType.Saint)
                {
                    //проверяем кто был целью святого

                    //если у святого была цель
                    if(sinnersAttemptTarget.targetPlayer != null 
                        //и цель святого жива
                        && sinnersAttemptTarget.targetPlayer.isLive()
                        //и цель святого является грешником
                        && sinnersAttemptTarget.targetPlayer.playerRole.roleType == RoleType.Sinner)
                    {
                        //значит святой и грешник сходили друг к другу
                        //отправялем обоих в морг
                        room.roomLogic.SendPlayerToMorgue(sinnersAttemptTarget);
                        room.roomLogic.SendPlayerToMorgue(sinnersAttemptTarget.targetPlayer);

                        room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Sinner,
                        NightActionId.Role,
                        () =>
                        {
                        room.roomChat.PublicMessage(
                            $"{sinnersAttemptTarget.GetColoredName()} - {ColorString.GetColoredRole("Святой")} - " +
                            $"погиб от рук " +
                            $"{ColorString.GetColoredRole("Грешника")}");

                        room.roomChat.PublicMessage   (
                            $"{sinnersAttemptTarget.targetPlayer.GetColoredName()} - " +
                            $"{ColorString.GetColoredRole("Грешник")} - " +
                            $"погиб от рук " +
                            $"{ColorString.GetColoredRole("Святого")}");
                        }
                        );

                        //флаг, что святой и грешник убили друг друга
                        doubleKill = true;

                        return;
                    }                   
                }

                //проверяем удалось ли свидетелю переманить грешника
                var witnessWillPowerSucces = false;
                Skill skillWitnessWillPower = null;
                if (sinnersAttemptTarget.playerRole.roleType == RoleType.Witness)
                {
                    var witnessRole = room.roomLogic.witnessVisit.GetRole(sinnersAttemptTarget);

                    if (witnessRole.Check_WitnessWillPower())
                    {
                        witnessWillPowerSucces = true;

                        skillWitnessWillPower = witnessRole.skill_WitnessWillPower;
                    }                   
                }

                //результат вербовки грешниками
                var sinnerRecruitSucces = false;               
                //участники вербовки
                List<BasePlayer> recruiterList = new List<BasePlayer>();
                List<BasePlayer> studyList = new List<BasePlayer>();
                foreach(var s in sinners)
                {
                    //получаем шанс вербовки для грешника с учетом его скилла "учусь"
                    var sinnerRole = GetRole(s);

                    //узнаем бонусный % вербовки из скилла "учусь"
                    var recruitBonus = sinnerRole.Check_SinnerStudy();

                    //кидаем кубик
                    var recruitChance = room.dice.Next(0, 100);

                    //проверяем результат с четом "учусь"
                    if (recruitChance < 50 + recruitBonus)
                    {
                        sinnerRecruitSucces = true;
                        recruiterList.Add(s);
                        //sinnerRecruiter = s;
                    }

                    if (recruitBonus > 0)
                    {
                        studyList.Add(s);                        
                    }
                }

                //если вербовка прошла успешно
                if (sinnerRecruitSucces)
                {              
                    foreach(var s in studyList)
                    {
                        var sinnerRole = GetRole(s);

                        var skill = sinnerRole.skill_SinnerStudy;

                        //Logger.Log.Debug($"learn skill {skill.effect}");

                        room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Sinner,
                        NightActionId.Skill,
                        () =>
                        {
                            room.roomChat.Skill_PersonalMessage(s, skill,
                                   $"Навык {ColorString.GetColoredSkill("Учусь")} помог " +
                                   $"завербовать цель!");
                        }
                        );
                    }


                    var sinnerString = "";

                    if (sinners.Count > 1)
                    {
                        sinnerString = $"{ColorString.GetColoredRole("Грешники")} завербовали";
                    }
                    else
                    {
                        sinnerString = $"{ColorString.GetColoredRole("Грешник")} завербовал";
                    }

                    //запоминаем роль жервы до вербовки
                    var targetRole = sinnersAttemptTarget.GetColoredRole();

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Sinner,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage($"{sinnerString} " +
                                $"в команду Мафии {sinnersAttemptTarget.GetColoredName()} - {targetRole}, " +
                                $"теперь он играет за {ColorString.GetColoredRole("Грешника")}");

                        //добавляем жертву в команду мафии
                        RoomHelper.AddPlayerToTeam(room, sinnersAttemptTarget, TeamType.Bad);

                        //меняем жертве роль на грешника
                        RoleHelper.ChangePlayerRole(sinnersAttemptTarget, RoleType.Sinner);

                        //открываем аватарку нового грешника всем игрокам
                        RoleHelper.UnlockRole_PlayerToRoom(sinnersAttemptTarget);

                        //открываем новому грешнику аватарки всех грешников
                        RoleHelper.UnlockRole_GroupToPlayer(sinners, sinnersAttemptTarget);
                    }
                    );

                   
                }
                else
                {
                    var sinnerString = "";

                    if (sinners.Count > 1)
                    {
                        sinnerString = $"{ColorString.GetColoredRole("Грешникам")}";
                    }
                    else
                    {
                        sinnerString = $"{ColorString.GetColoredRole("Грешнику")}";
                    }

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                   RoleType.Sinner,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage($"{sinnerString} не удалось " +
                            $"завербовать выбранную цель");
                    }
                );
                }

                if (!sinnersAttemptSuccess && attemptTargets.Count > 1)
                {
                    var sinnerString = $"{ColorString.GetColoredRole("Грешники")}";

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Sinner,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage($"{sinnerString} не определились");
                    }
                    );
                }

                //если свидетелю удалсь переманить грешника
                if (witnessWillPowerSucces)
                {
                    //выбираем случайного грешника
                    var randomIdex = room.dice.Next(0, sinners.Count);
                    var randomSinner = sinners[randomIdex];

                    //переносим грешника в команду мирных
                    RoomHelper.AddPlayerToTeam(room, randomSinner, TeamType.Good);
                    //меняем грешнику роль на гражданина
                    RoleHelper.ChangePlayerRole(randomSinner, RoleType.Citizen);

                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Witness,
                    NightActionId.Skill,
                    () =>
                    {
                        room.roomChat.Skill_PersonalMessage(sinnersAttemptTarget, skillWitnessWillPower,
                            $"{randomSinner.GetColoredName()} - {ColorString.GetColoredRole("Грешник")} " +
                            $"был перетянут на сторону Мирных навыком " +
                            $"{ColorString.GetColoredSkill("Сила Воли")}. " +
                            $"Ваш навык сработал");

                        room.roomChat.Skill_PersonalMessage(randomSinner, skillWitnessWillPower,
                            $"{randomSinner.GetColoredName()} - {ColorString.GetColoredRole("Грешник")} " +
                            $"был перетянут на сторону Мирных навыком " +
                            $"{ColorString.GetColoredSkill("Сила Воли")}. " +
                            $"Теперь вы играете за {ColorString.GetColoredRole("Гражданина")}");

                        var excludedPlayers = new BasePlayer[] { sinnersAttemptTarget, randomSinner };

                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{randomSinner.GetColoredName()} - {ColorString.GetColoredRole("Грешник")} " +
                            $"был перетянут на сторону Мирных навыком " +
                            $"{ColorString.GetColoredSkill("Сила Воли")}",
                            room, skillWitnessWillPower, excludedPlayers);
                    }
                    );
                }
            }         
        }
        public Sinner GetRole(BasePlayer player)
        {
            Sinner role;

            if (player.oldRole != null)
            {
                role = (Sinner)player.oldRole;
            }
            else
            {
                role = (Sinner)player.playerRole;
            }

            return role;
        }
    }
}
