using Share;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// вор/стерва
    /// </summary>
    public class Guerilla : Role
    {
        public Guerilla(RoleType roleType) : base(roleType)
        {
            isKiller = false;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} пойду поболтаю с {targetPlayer.GetColoredName()}");

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
                    case SkillEffect.GuerillaIce: { skill_GuerillaIce = new Skill(skillId, s.Value); } break;
                    case SkillEffect.GuerillaLongEffect: { skill_GuerillaLongEffect = new Skill(skillId, s.Value); } break;
                    case SkillEffect.GuerillaOwn: { skill_GuerillaOwn = new Skill(skillId, s.Value); } break;
                    case SkillEffect.GuerillaReward: { skill_GuerillaReward = new Skill(skillId, s.Value); } break;
                    case SkillEffect.GuerillaThatsAll: { skill_GuerillaThatsAll = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_GuerillaIce { get; private set; }
        public bool Check_GuerillaIce()
        {
            if (!SkillSuccess(skill_GuerillaIce))
            {
                return false;
            }

            //do some action   


            return true;
        }

        public Skill skill_GuerillaLongEffect { get; private set; }
        public bool Check_GuerillaLongEffect()
        {
            if (!SkillSuccess(skill_GuerillaLongEffect))
            {
                return false;
            }

            return true;
        }

        public Skill skill_GuerillaOwn { get; private set; }
        public bool Check_GuerillaOwn()
        {
            if (!SkillSuccess(skill_GuerillaOwn))
            {
                return false;
            }

            return true;
        }

        public Skill skill_GuerillaReward { get; private set; }
        public bool Check_GuerillaReward()
        {
            return SkillExist(skill_GuerillaReward);
        }

        public Skill skill_GuerillaThatsAll { get; private set; }
        public bool Check_GuerillaThatsAll()
        {
            if (!SkillSuccess(skill_GuerillaThatsAll))
            {
                return false;
            }

            //do some action   

            //message

            return true;
        }
    }
}
