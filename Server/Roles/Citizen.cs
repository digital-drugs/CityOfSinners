using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// гражданин
    /// </summary>
    public class Citizen:Role
    {
        public Citizen(RoleType roleType) : base(roleType)
        {
           
        }

        public override bool CheckResist(BasePlayer guest)
        {
            if (base.CheckResist(guest))
            {
                return true;
            }         

            return false;
        }
        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.CitizenManiac: { skill_CitizenManiac = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CitizenOneMore: { skill_CitizenOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CitizenSecret: { skill_CitizenSecret = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CitizenVitality: { skill_CitizenVitality = new Skill(skillId, s.Value); } break;
                    case SkillEffect.CitizenWerewolf: { skill_CitizenWerewolf = new Skill(skillId, s.Value); } break;
                }               
            }
        }

        public Skill skill_CitizenManiac { get; private set; }
        public bool Check_CitizenManiac()
        {            
            if (!SkillSuccess(skill_CitizenManiac))
            {
                return false;
            }

            return true;
        }

        private Skill skill_CitizenOneMore;
        public bool Check_CitizenOneMore()
        {
            return SkillExist(skill_CitizenOneMore);
        }

        public Skill skill_CitizenSecret { get; private set; }
        public bool Check_CitizenSecret()
        {
            if (!SkillSuccess(skill_CitizenSecret))
            {
                return false;
            }

            return true;
        }

        public Skill skill_CitizenVitality { get; private set; }
        public bool Check_CitizenVitality()
        {
            if (!SkillSuccess(skill_CitizenVitality))
            {
                return false;
            }          

            return true;
        }

        public Skill skill_CitizenWerewolf { get; private set; }
        public bool Check_CitizenWerewolf()
        { 
            if (!SkillSuccess(skill_CitizenWerewolf))
            {
                return false;
            }

                  

            return true;
        }
    }
}
