using ExitGames.Client.Photon.LoadBalancing;
using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Maniac : Role
    {
        public Maniac(RoleType roleType) : base(roleType)
        {
            isKiller = true;
        }

        public override Role GetRole()
        {
            return this;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} хочу убить {targetPlayer.GetColoredName()}");

            var playersGroup = new List<BasePlayer>();

            foreach (var p in owner.GetRoom().players)
            {
                if (p.Value.client != null && p.Value != owner)
                {
                    playersGroup.Add(p.Value);
                }
            }

            var roleAction = RoomHelper.GetRoleActionString(owner.playerRole.roleType);
            owner.GetRoom().roomChat.PublicMessageToCustomPlayersGroup(playersGroup, $"{roleAction}");
        }

        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.ManiacIdeal: { skill_ManiacIdeal = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ManiacOneMore: { skill_ManiacOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ManiacSong: { skill_ManiacSong = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ManiacToFreedom: { skill_ManiacToFreedom = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ManiacWorse: { skill_ManiacWorse = new Skill(skillId, s.Value); } break;
                }
            }

            Logger.Log.Debug($"set skills for maniac");
        }

       

        public bool maniacIsIdeal = false;
        public bool idealUsed = false;
        public Skill skill_ManiacIdeal { get; private set; }
        public bool Check_ManiacIdeal()
        {
            idealUsed = false;

            if (!SkillSuccess(skill_ManiacIdeal))
            {
                maniacIsIdeal = false;

                return false;
            }

            //do some action 
            maniacIsIdeal = true;

            //message
            //owner.room.roomChat.PublicMessage($"маньяк идеальный");

            return true;
        }

        private Skill skill_ManiacOneMore;
        public bool Check_ManiacOneMore()
        {
            return SkillExist(skill_ManiacOneMore);
        }

        public Skill skill_ManiacSong { get; private set; }
        public bool Check_ManiacSong()
        {
            if (!SkillSuccess(skill_ManiacSong))
            {
                return false;
            }

            //do some action 
            foreach(var p in owner.GetRoom().GetLivePlayers().Values)
            {
                if(p  != owner)
                {
                    SkillHelper.AddSkillEffectToPlayer(p, skill_ManiacSong, DurationType.NightEnd);
                }
            }

            owner.GetRoom().roomChat.Skill_PersonalMessage(owner, skill_ManiacSong,
                $"{ColorString.GetColoredRole("Маньяк")} лишил всех ночного хода, " +
                $"при помощи навыка {ColorString.GetColoredSkill("Песнь Маньяка")}. " +
                $"Ваш навык сработал");

            var excludedPlayers = new BasePlayer[] { owner};

            owner.GetRoom().roomChat.Skill_PublicMessageExcludePlayers(
                $"{ColorString.GetColoredRole("Маньяк")} лишил всех ночного хода, " +
                $"при помощи навыка {ColorString.GetColoredSkill("Песнь Маньяка")}",
                owner.GetRoom(), skill_ManiacSong,excludedPlayers);

            return true;
        }

        public Skill skill_ManiacToFreedom { get; private set; }
        public bool Check_ManiacToFreedom()
        {
            if (!SkillSuccess(skill_ManiacToFreedom))
            {
                return false;
            }        

            return true;
        }

        //public bool unlimit { get; private set; } = false;
        public Skill skill_ManiacWorse { get; private set; }
        public bool Check_ManiacWorse()
        {
            if (!SkillSuccess(skill_ManiacWorse))
            {
                return false;
            }

           

            return true;
        }
    }


}