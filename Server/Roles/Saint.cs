using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Saint:Role
    {
        public Saint(RoleType roleType) : base(roleType)
        {
            isKiller = false;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            var saints = RoomHelper.FindPlayersByRole(RoleType.Saint, owner.GetRoom());

            owner.GetRoom().roomChat.PublicMessageToCustomPlayersGroup(saints, 
                $"{owner.GetColoredName()} идёт вербовать {targetPlayer.GetColoredName()}");

            var excludedPlayers = saints.ToArray();

            var roleAction = RoomHelper.GetRoleActionString(owner.playerRole.roleType);

            owner.GetRoom().roomChat.PublicMessageExcludePlayers(
                 $"{roleAction}",
                 owner.GetRoom(), excludedPlayers);
        }

        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.SaintBadMood: { skill_SaintBadMood = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SaintGuard: { skill_SaintGuard = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SaintLucky: { skill_SaintLucky = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SaintPower: { skill_SaintPower = new Skill(skillId, s.Value); } break;
                    case SkillEffect.SaintResistant: { skill_SaintResistant = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_SaintBadMood { get; private set; }
        /// <summary>
        /// проверка вконце ночи. шанс убить игрока, вместо вербовки
        /// </summary>
        /// <param name="targetPlayer"></param>
        /// <returns></returns>
        public bool Check_SaintBadMood()
        {
            if (!SkillSuccess(skill_SaintBadMood))
            {
                return false;
            }         

            return true;
        }

        public Skill skill_SaintGuard { get; private set; }
        public bool Check_SaintGuard()
        {
            if (!SkillSuccess(skill_SaintGuard))
            {
                return false;
            }           

            return true;
        }

        public Skill skill_SaintLucky { get; private set; }
        public bool Check_SaintLucky()
        {
            return SkillExist(skill_SaintLucky);
        }

        public Skill skill_SaintPower { get; private set; }
        public bool Check_SaintPower()
        {
            if (!SkillSuccess(skill_SaintPower))
            {
                return false;
            }          

            return true;
        }

        private Skill skill_SaintResistant;
        public bool Check_SaintResistant()
        {
           return SkillExist(skill_SaintResistant);
        }


    }
}
