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
        }

        public void Visit()
        {
            if (sinners.Count == 0) return;

            var succesCount = 0;
            var targetSuccesCount = 0;

            if (sinners.Count > 2)
            {
                targetSuccesCount = sinners.Count - 1;
            }
            else
            {
                targetSuccesCount = sinners.Count;
            }

            //проверяем что вербовка началась
            BasePlayer sinnersAttemptTarget = null;
            var sinnersAttemptSuccess = true;
            for (int i = 0; i < sinners.Count; i++)
            {
                //запоминаем цель первого грешника
                if (i == 0)
                {
                    sinnersAttemptTarget = sinners[i].targetPlayer;
                }

                //если цель грешника отсутствует
                if (sinners[i].targetPlayer == null)
                {
                    sinnersAttemptSuccess = false; break;
                }

                //если цель грешника мертва
                if (!sinners[i].targetPlayer.isLive())
                {
                    sinnersAttemptSuccess = false; break;
                }

                //если цели грешников не совпали
                if (sinners[i].targetPlayer != sinnersAttemptTarget)
                {
                    sinnersAttemptSuccess = false; break;
                }

                //если грешник наткнулся на команду мафии - ничего не делаем
                //зеркало
                if (sinners[i].targetPlayer.team.teamType == TeamType.Bad)
                {
                    return;
                }

                //если мафиози может ходить
                if (sinners[i].playerRole.CanVisit())
                {
                    //для текущего мафиози в цикле считаем покушение удавшимся
                    succesCount++;
                }
            }

            //если кол-во успешных покушений меньше минимально необходимого, то групповое покушение не удалось
            if (succesCount < targetSuccesCount)
            {
                sinnersAttemptSuccess = false;
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

                        room.roomLogic.AddNightActionMessage
                        (
                        sinnersAttemptTarget.targetPlayer,
                        NightActionId.Role,
                        () =>
                        {
                        room.roomChat.PublicMessage(
                            $"{sinnersAttemptTarget.GetColoredName()} - {sinnersAttemptTarget.GetColoredRole()} - " +
                            $"погиб от рук " +
                            $"{ColorString.GetColoredRole("Грешника")}");

                        room.roomChat.PublicMessage   (
                            $"{sinnersAttemptTarget.targetPlayer.GetColoredName()} - " +
                            $"{sinnersAttemptTarget.targetPlayer.GetColoredRole()} - " +
                            $"погиб от рук " +
                            $"{ColorString.GetColoredRole("Святого")}");
                        }
                        );

                        return;
                    }                   
                }

                //проверяем удалось ли свидетелю переманить грешника
                var witnessWillPowerSucces = false;
                Skill skillWitnessWillPower = null;
                if (sinnersAttemptTarget.playerRole.roleType == RoleType.Witness)
                {
                   var witnessRole = (Witness)sinnersAttemptTarget.playerRole;

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
                    var sinnerRole = (Sinner)s.playerRole;

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
                        var sinnerRole = (Sinner)s.playerRole;

                        var skill = sinnerRole.skill_SinnerStudy;

                        //Logger.Log.Debug($"learn skill {skill.effect}");

                        room.roomLogic.AddNightActionMessage
                        (
                        s,
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

                    room.roomLogic.AddNightActionMessage
                    (
                    sinners[0],
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage($"{sinnerString} " +
                                $"в команду Мафии {sinnersAttemptTarget.GetColoredName()} - {targetRole}, " +
                                $"теперь он играет за {ColorString.GetColoredRole("Грешника")}");
                    }
                    );

                    //добавляем жертву в команду мафии
                    room.roomRoles.badTeam.AddPlayer(sinnersAttemptTarget);
                    //меняем жертве роль на грешника
                    RoleHelper.ChangePlayerRole(sinnersAttemptTarget, RoleType.Sinner);

                    //открываем аватарку нового грешника всем игрокам
                    RoleHelper.UnlockRole_PlayerToRoom(sinnersAttemptTarget);

                    //открываем новому грешнику аватарки всех грешников
                    RoleHelper.UnlockRole_GroupToPlayer(sinners, sinnersAttemptTarget);
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

                    room.roomLogic.AddNightActionMessage
                    (
                    sinners[0],
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage($"{sinnerString} не удалось " +
                            $"завербовать выбранную цель");
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
                    room.roomRoles.goodTeam.AddPlayer(randomSinner);
                    //меняем грешнику роль на гражданина
                    RoleHelper.ChangePlayerRole(randomSinner, RoleType.Citizen);

                    room.roomLogic.AddNightActionMessage
                    (
                    sinnersAttemptTarget,
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
    }
}
