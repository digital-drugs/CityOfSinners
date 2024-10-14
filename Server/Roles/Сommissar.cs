using ExitGames.Client.Photon.LoadBalancing;
using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// комиссар
    /// </summary>
    public class Commissar:Role
    {
        public Commissar(RoleType roleType) : base(roleType)
        {
            isKiller = false;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} иду проверять {targetPlayer.GetColoredName()}");

            var playersGroup = new List<BasePlayer>();

            foreach (var p in owner.GetRoom().GetLivePlayers())
            {
                if (p.Value.client != null && p.Value != owner)
                {
                    playersGroup.Add(p.Value);
                }
            }

            var roleAction = RoomHelper.GetRoleActionString(owner.playerRole.roleType);
            owner.GetRoom().roomChat.PublicMessageToCustomPlayersGroup(playersGroup, $"{roleAction}");
        }

        private Dictionary<long, BasePlayer> visitedPlayers = new Dictionary<long, BasePlayer>();
        public void AddVisitedPlayer(BasePlayer player)
        {
            if (!visitedPlayers.ContainsKey(player.playerId))
            {
                visitedPlayers.Add(player.playerId, player);
            }
        }

        public bool PlayerIsVisited(BasePlayer player)
        {
            return visitedPlayers.ContainsKey(player.playerId);
        }

        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.CommissarGift: { skill_CommissarGift = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CommissarMafiaColleagues: { skill_CommissarMafiaColleagues = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CommissarOneMore: { skill_CommissarOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CommissarRage: { skill_CommissarRage = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CommissarThroughLie: { skill_CommissarThroughLie = new Skill(skillId, s.Value); } break;
                }
            }
        }

        private Skill skill_CommissarGift;
        private List<BasePlayer> unlockedSinners = new List<BasePlayer>();
        public bool Check_CommissarGift(BasePlayer targetPlayer)
        {
            if(targetPlayer.playerRole.roleType != RoleType.Saint) return false;

            if (!SkillSuccess(skill_CommissarGift))
            {
                return false;
            }

            //do some action
           var sinners = RoomHelper.FindPlayersByRole(RoleType.Sinner, owner.GetRoom());

            foreach(var s in unlockedSinners)
            {
                sinners.Remove(s);
            }

            if (sinners.Count == 0) return false;

            var randomSinnerIndex = owner.GetRoom().dice.Next(0, sinners.Count);
            var randomSinner = sinners[randomSinnerIndex];

            unlockedSinners.Add(randomSinner);

            //Logger.Log.Debug($"sin {sinners.Count} {randomSinner.playerName}");

            owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Commissar,
                    NightActionId.Skill,
                    () =>
                    {
                        owner.GetRoom().roomChat.Skill_PersonalMessage(owner, skill_CommissarGift,
                            $"{randomSinner.GetColoredName()} играет за {ColorString.GetColoredRole("Грешника")}! " +
                            $"Помог навык {ColorString.GetColoredSkill("Дар")}. " +
                            $"Ваш навык сработал");

                        var excludedPlayers = new BasePlayer[] { owner };

                        owner.GetRoom().roomChat.Skill_PublicMessageExcludePlayers(
                            $"{randomSinner.GetColoredName()} играет за {ColorString.GetColoredRole("Грешника")}! " +
                            $"Помог навык {ColorString.GetColoredSkill("Дар")}",
                            owner.GetRoom(), skill_CommissarGift, excludedPlayers);

                        RoleHelper.UnlockRole_PlayerToRoom(randomSinner);
                    },
                    owner
                    );


            return true;
        }

        private Skill skill_CommissarMafiaColleagues;
        private List<BasePlayer> unlockedMafia = new List<BasePlayer>();
        public bool Check_CommissarMafiaColleagues(BasePlayer targetPlayer)
        {
            if (targetPlayer.playerRole.roleType != RoleType.Mafia) return false;

            if (!SkillSuccess(skill_CommissarMafiaColleagues))
            {
                return false;
            }

            //do some action
            var mafia = RoomHelper.FindPlayersByRole(RoleType.Mafia, owner.GetRoom());

            mafia.Remove(targetPlayer);

            var mafiaBoss = RoomHelper.FindPlayerByRole(RoleType.MafiaBoss, owner.GetRoom());

            if (mafiaBoss != null)
            {
                mafia.Add(mafiaBoss);
            }

            foreach (var m in unlockedMafia)
            {
                mafia.Remove(m);
            }

            if (mafia.Count==0) return false;

            var randomMafiaIndex = owner.GetRoom().dice.Next(0, mafia.Count);
            var randomMafia = mafia[randomMafiaIndex];

            owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                     (
                     RoleType.Commissar,
                     NightActionId.Skill,
                     () =>
                     {
                         RoleHelper.UnlockRole_PlayerToRoom(randomMafia, randomMafia.playerRole.roleType);

                         owner.GetRoom().roomChat.Skill_PersonalMessage(owner, skill_CommissarMafiaColleagues,
                            $"{randomMafia.GetColoredName()} играет за " +
                            $"{randomMafia.GetColoredRole()}, его сдал коллега. " +
                            $"Ваш навык сработал");

                         var excludedPlayers = new BasePlayer[] { owner };
                         owner.GetRoom().roomChat.Skill_PublicMessageExcludePlayers(
                             $"{randomMafia.GetColoredName()} играет за " +
                             $"{randomMafia.GetColoredRole()}, его сдал коллега",
                             owner.GetRoom(), skill_CommissarMafiaColleagues, excludedPlayers);
                     },
                     owner
                     );

            return true;
        }


        private Skill skill_CommissarOneMore;
        public bool Check_CommissarOneMore()
        {
            return SkillExist(skill_CommissarOneMore);
        }

        public Skill skill_CommissarRage { get; private set; }
        public bool Check_CommissarRage()
        {
            if (!SkillSuccess(skill_CommissarRage))
            {
                return false;
            }

            return true;
        }

        public Skill skill_CommissarThroughLie { get; private set; }
        public bool Check_CommissarThroughLie()
        {          
            if (!SkillSuccess(skill_CommissarThroughLie))
            {
                return false;
            }

            return true;
        }
    }
}
