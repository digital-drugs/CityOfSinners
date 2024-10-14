using Share;
using System;
//using System.Buffers.Text;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// свидетель
    /// </summary>
    public class Witness : Role
    {
        public Witness(RoleType roleType) : base(roleType)
        {
            isKiller = false;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} хочу навестить {targetPlayer.GetColoredName()}");

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


        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.WitnessCarma: { skill_WitnessCarma = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WitnessFriends: { skill_WitnessFriends = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WitnessNoInterference: { skill_WitnessNoInterference = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WitnessOneMore: { skill_WitnessOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WitnessWillPower: { skill_WitnessWillPower = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_WitnessCarma { get; private set; }
        public bool Check_WitnessCarma()
        {
            if (!SkillSuccess(skill_WitnessCarma))
            {
                return false;
            }

         

            return true;
        }

        public Skill skill_WitnessFriends { get; private set; }
        public bool Check_WitnessFriends()
        {
            //if (targetPlayer.playerRole.roleType != RoleType.Commissar) return false;

            if (!SkillSuccess(skill_WitnessFriends))
            {
                return false;
            }

            return true;
        }

        private Skill skill_WitnessNoInterference;
        public bool Check_WitnessNoInterference()
        {
            return SkillExist(skill_WitnessNoInterference);
        }

        private Skill skill_WitnessOneMore;
        public bool Check_WitnessOneMore()
        {
            return SkillExist(skill_WitnessOneMore);
        }

        public Skill skill_WitnessWillPower { get; private set; }
        public bool Check_WitnessWillPower()
        {
            if (!SkillSuccess(skill_WitnessWillPower))
            {
                return false;
            }        

            return true;
        }
    }
}
