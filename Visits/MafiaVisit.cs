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
        }

        public void Visit()
        {
            if (mafia.Count == 0) return;

            var succesCount = 0;
            var targetSuccesCount = 0;

            if (mafia.Count > 2)
            {
                targetSuccesCount = mafia.Count - 1;
            }
            else
            {
                targetSuccesCount = mafia.Count;
            }

            BasePlayer mafiaAttemptTarget = null;
            var mafiaAttemptSuccess = true;

            for (int i = 0; i < mafia.Count; i++)
            {
                if (i == 0)
                {
                    mafiaAttemptTarget = mafia[i].targetPlayer;
                }              

                if (mafia[i].targetPlayer == null)
                {
                    mafiaAttemptSuccess = false; break;
                }

                if (mafia[i].targetPlayer.playerRole.IsResurected())
                {
                    mafiaAttemptSuccess = false; break;
                }

                if (!mafia[i].targetPlayer.isLive())
                {
                    mafiaAttemptSuccess = false; break;
                }

                if (mafia[i].targetPlayer != mafiaAttemptTarget)
                {
                    mafiaAttemptSuccess = false; break;
                }

                //если мафиози может ходить
                if (mafia[i].playerRole.CanVisit())
                {
                    //для текущего мафиози в цикле считаем покушение удавшимся
                    succesCount++;
                }              
            }

            //если кол-во успешных покушений меньше минимально необходимого, то групповое покушение не удалось
            if(succesCount < targetSuccesCount)
            {
                mafiaAttemptSuccess = false;
            }

            //проверяем защиту цели от покушения
            if(mafiaAttemptSuccess)
            {
                if (mafiaAttemptTarget.playerRole.CheckResistRoles(mafia[0]))
                {
                    mafiaAttemptSuccess = false;
                }

                if (mafiaAttemptTarget.playerRole.CheckResistExtras(mafia[0]))
                {
                    mafiaAttemptSuccess = false;
                }
            }           

            if (mafiaAttemptSuccess)
            {
                //проверяем навык х2 убийство
                var mafiaX2Kill = false;
                List<BasePlayer> х2killerList = new List<BasePlayer>();
                foreach (var m in mafia)
                {
                    var mafiaRole = (Mafia)m.playerRole;

                    if (mafiaRole.Check_MafiaKillX2())
                    {
                        mafiaX2Kill = true;
                        //mafiaX2Killer = m;
                        х2killerList.Add(m);
                    }
                }

                if (mafia.Count > 1)
                {
                    room.roomLogic.AddNightActionMessage
                    (
                    mafia[0],
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
                    room.roomLogic.AddNightActionMessage
                    (
                    mafia[0],
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
                        room.roomLogic.AddNightActionMessage
                        (
                        х2killerList[0],
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
    }
}
