using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class MafiaVisit : RoleVisit
    {
        public MafiaVisit(Room room) : base(room)
        {

        }

        List<BasePlayer> mafia;
        public void Setup()
        {
            mafia = new List<BasePlayer>();

            foreach (var p in room.players.Values)
            {
                if (p.playerRole.roleType == RoleType.Mafia && p.isLive())
                {
                    mafia.Add(p);
                }
            }

            foreach(var p in mafia)
            {
                p.SetActiveAtNight(p.targetPlayer != null);
            }

            ////проверяем, что цель жива на начало ночи
            //if (mafiaBoss.targetPlayer != null && !mafiaBoss.targetPlayer.isLive())
            //{
            //    mafiaBoss.targetPlayer = null;
            //    return;
            //}
        }

        public void Visit()
        {
            if (mafia.Count == 0) return;

            //var succesCount = 0;
            var targetSuccesCount = 0;

            if (mafia.Count > 2)
            {
                targetSuccesCount = mafia.Count - 1;
            }
            else
            {
                targetSuccesCount = mafia.Count;
            }

            Dictionary<BasePlayer, int> attemptTargets = new Dictionary<BasePlayer, int>();   

            BasePlayer mafiaAttemptTarget = null;
            var mafiaAttemptSuccess = false;

            for (int i = 0; i < mafia.Count; i++)
            {
                //если мафиози может ходить
                if (mafia[i].playerRole.CanVisit())
                {
                    //для текущего мафиози в цикле считаем покушение удавшимся
                    //succesCount++;

                    if (mafia[i].targetPlayer == null)
                    {
                        continue;
                    }

                    if (mafia[i].targetPlayer.isLive()==false)
                    {
                        continue;
                    }

                    //if (mafia[i].targetPlayer.playerRole.IsResurected())
                    //{
                    //    continue;
                    //}

                    //если эта цель была уже кем-то атакована, увеличиваем счетчик атакующих
                    if (attemptTargets.ContainsKey(mafia[i].targetPlayer))
                    {
                        attemptTargets[mafia[i].targetPlayer]++;
                    }
                    else
                    {
                        attemptTargets.Add(mafia[i].targetPlayer, 1);
                    }

                    //Logger.Log.Debug($"mafia target => {attemptTargets[mafia[i].targetPlayer]}");
                }
            }

            foreach(var at in attemptTargets)
            {
                if(at.Value >= targetSuccesCount)
                {
                    mafiaAttemptSuccess = true;
                    mafiaAttemptTarget = at.Key;
                }
            }

            if (mafiaAttemptSuccess == false && attemptTargets.Count > 1)
            {
                var mafiaString = $"{ColorString.GetColoredRole("Мафиози")}";

                room.roomLogic.nightActionMessages.AddNightActionMessage
                (
                RoleType.Mafia,
                NightActionId.Role,
                () =>
                {
                    room.roomChat.PublicMessage($"{mafiaString} не определились");
                }
                );

                return;
            }

            //проверяем защиту цели от покушения
            if (mafiaAttemptSuccess)
            {
                if (mafiaAttemptTarget.playerRole.CheckResistExtras(mafia[0]))
                {
                    mafiaAttemptSuccess = false;
                    return;
                }

                if (mafiaAttemptTarget.playerRole.CheckResistRoles(mafia[0]))
                {
                    mafiaAttemptSuccess = false;
                    return;
                }               

                //если цель защищена скиллами
                if (mafiaAttemptTarget.playerRole.CheckResistSkills(mafia[0]))
                {
                    mafiaAttemptSuccess = false;
                    return;
                }
            }           

            if (mafiaAttemptSuccess)
            {
                //проверяем навык х2 убийство
                var mafiaX2Kill = false;
                List<BasePlayer> х2killerList = new List<BasePlayer>();
                foreach (var m in mafia)
                {
                    var mafiaRole = GetRole(m);

                    if (mafiaRole.Check_MafiaKillX2())
                    {
                        mafiaX2Kill = true;
                        //mafiaX2Killer = m;
                        х2killerList.Add(m);
                    }
                }

                if (mafia.Count > 1)
                {
                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Mafia,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage
                        ($"{ColorString.GetColoredRole("Мафиози")} убили {mafiaAttemptTarget.GetColoredName()} " +
                        $"- {mafiaAttemptTarget.GetColoredRole()}");
                    }
                    );
                }
                else
                {
                    room.roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Mafia,
                    NightActionId.Role,
                    () =>
                    {
                        room.roomChat.PublicMessage
                        ($"{ColorString.GetColoredRole("Мафиози")} убил {mafiaAttemptTarget.GetColoredName()} " +
                        $"- {mafiaAttemptTarget.GetColoredRole()}");
                    }
                    );
                }

                room.roomLogic.SendPlayerToMorgue(mafiaAttemptTarget);
                mafiaAttemptTarget.SetKiller(mafia[0]);

                if (mafiaX2Kill)
                {
                    var randomTarget = RoomHelper.FindNearPlayers(
                            room, х2killerList[0], mafiaAttemptTarget, 1, true, true);

                    var mafiaRole = (Mafia)х2killerList[0].playerRole;

                    if (randomTarget.Count > 0)
                    {
                        room.roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Mafia,
                        NightActionId.Skill,
                        () =>
                        {
                        room.roomChat.Skill_PersonalMessage(х2killerList[0], mafiaRole.skill_MafiaKillX2,
                              $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                              $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}. Ваш навык сработал");

                        room.roomChat.Skill_PersonalMessage(randomTarget[0], mafiaRole.skill_MafiaKillX2,
                            $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                            $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}. Вы убиты, увы!");

                        var exludedPlayers = new BasePlayer[] { х2killerList[0], randomTarget[0] };

                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                            $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}",
                            room, mafiaRole.skill_MafiaKillX2, exludedPlayers);
                        }
                        );

                        room.roomLogic.SendPlayerToMorgue(randomTarget[0]);
                    }
                }
            }

           
        }

        public Mafia GetRole(BasePlayer player)
        {
            Mafia role;

            if (player.oldRole != null)
            {
                role = (Mafia)player.oldRole;
            }
            else
            {
                role = (Mafia)player.playerRole;
            }

            return role;
        }
    }
}
