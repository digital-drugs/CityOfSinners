using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Doctor : Role
    {
        public Doctor(RoleType roleType) : base(roleType)
        {

        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} иду спасать {targetPlayer.GetColoredName()}");

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
                    case SkillEffect.DoctorClizma: { skill_DoctorClizma = new Skill(skillId, s.Value); } break;
                    case SkillEffect.DoctorCrazyDoc: { skill_DoctorCrazyDoc = new Skill(skillId, s.Value); } break;
                    case SkillEffect.DoctorEvilScalpel: { skill_DoctorGoldScalpel = new Skill(skillId, s.Value); } break;
                    case SkillEffect.DoctorHealX2: { skill_DoctorHealX2 = new Skill(skillId, s.Value); } break;
                    case SkillEffect.DoctorWhereAreYou: { skill_DoctorWhereAreYou = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public bool isClizma = false;
        public Skill skill_DoctorClizma { get; private set; }
        public bool Check_DoctorClizma()
        {
            if (!SkillSuccess(skill_DoctorClizma))
            {
                isClizma = false;
                return false;
            }

        

            return true;
        }

        public Skill skill_DoctorCrazyDoc { get; private set; }
        /// <summary>
        /// шанс нанести смертельный удар битой
        /// </summary>
        /// <param name="targetPlayer"></param>
        /// <returns></returns>
        public bool Check_DoctorCrazyDoc()
        {
            if (!SkillSuccess(skill_DoctorCrazyDoc))
            {
                return false;
            }         

            return true;
        }

        public Skill skill_DoctorGoldScalpel { get; private set; }
        public bool Check_DoctorEvilScalpel()
        {
            return SkillExist(skill_DoctorGoldScalpel);
        }

        public Skill skill_DoctorHealX2 { get; private set; }
        public bool Check_DoctorHealX2()
        {
            if (!SkillSuccess(skill_DoctorHealX2))
            {
                return false;
            }           

            return true;
        }

        public Skill skill_DoctorWhereAreYou { get; private set; }
        public bool Check_DoctorWhereAreYou()
        {
            if (!SkillSuccess(skill_DoctorWhereAreYou))
            {
                return false;
            }         

            return true;
        }
    }
}
