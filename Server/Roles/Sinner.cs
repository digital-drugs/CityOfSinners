using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Sinner : Role
    {
        public Sinner(RoleType roleType) : base(roleType)
        {
            isKiller = false;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            //base.SendVisitMessage();
            var team = owner.GetRoom().roomRoles.badTeam;
            owner.GetRoom().roomChat.TeamMessage(team, $"{owner.GetColoredName()} пошёл вербовать {targetPlayer.GetColoredName()}");

            var playersGroup = new List<BasePlayer>();

            foreach (var p in owner.GetRoom().GetLivePlayers())
            {
                if (p.Value.team.teamType != TeamType.Bad && p.Value.client != null && p.Value != owner)
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
                    case SkillEffect.SinnerChange: { skill_SinnerChange = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SinnerMist: { skill_SinnerMist = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SinnerNotGiveUp: { skill_SinnerNotGiveUp = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SinnerOneMore: { skill_SinnerOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SinnerStudy: { skill_SinnerStudy = new Skill(skillId, s.Value); } break;
                }
            }
        }

        //BasePlayer targetPlayer
        //var skillName = ColorString.GetColoredString("резкий дерзкий", ColorId.Skill);

        public Skill skill_SinnerChange { get; private set; }
        public bool Check_SinnerChange()
        {
            if (!SkillSuccess(skill_SinnerChange))
            {
                return false;
            }          

            return true;
        }

        public Skill skill_SinnerMist { get; private set; }
        public bool Check_SinnerMist()
        {
            if (!SkillSuccess(skill_SinnerMist))
            {
                return false;
            }

           

            return true;
        }

        public Skill skill_SinnerNotGiveUp { get; private set; }
        public bool Check_SinnerNotGiveUp()
        {
            if (!SkillSuccess(skill_SinnerNotGiveUp))
            {
                return false;
            }

            //do some action            

            //message         

            return true;
        }

        private Skill skill_SinnerOneMore;
        public bool Check_SinnerOneMore()
        {
           return SkillExist(skill_SinnerOneMore);
        }

        public Skill skill_SinnerStudy { get; private set; }
        public int Check_SinnerStudy()
        {
            if(skill_SinnerStudy == null)
            {
                return 0;
            }
            else
            {
                return skill_SinnerStudy.skillValue;
            }
        }

    }
}
