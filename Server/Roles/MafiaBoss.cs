using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// Босс мафии
    /// </summary>
    public class MafiaBoss : Role
    {
        public MafiaBoss(RoleType roleType) : base(roleType)
        {

        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            //base.SendVisitMessage();
            var team = owner.GetRoom().roomRoles.badTeam;
            owner.GetRoom().roomChat.TeamMessage(team, $"{owner.GetColoredName()} решил заморозить {targetPlayer.GetColoredName()}");

            var playersGroup = new List<BasePlayer>();

            foreach (var p in owner.GetRoom().players)
            {
                if (p.Value.team.teamType != TeamType.Bad && p.Value.client != null && p.Value != owner)
                {
                    playersGroup.Add(p.Value);
                }
            }

            var roleAction = RoomHelper.GetRoleActionString(owner.playerRole.roleType);
            owner.GetRoom().roomChat.PublicMessageToCustomPlayersGroup(playersGroup, $"{roleAction}");
        }

        public void CheckKill()
        {
            var mafia = RoomHelper.FindPlayersByRole(RoleType.Mafia, owner.GetRoom());

            if (mafia.Count > 0)
            {
                isKiller = false;
            }
            else
            {
                isKiller = true;
            }
        }

        public override Role GetRole()
        {
            return this;
        }

        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.MafiaBossBodyBuilder: { skill_MafiaBossBodyBuilder = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaBossExperienced: { skill_MafiaBossExperienced = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaBossInterception: { skill_MafiaBossInterception = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaBossOneMore: { skill_MafiaBossOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaBossUps: { skill_MafiaBossUps = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_MafiaBossBodyBuilder { get; private set; }
        public bool Check_MafiaBossBodyBuilder()
        {
            if (!SkillSuccess(skill_MafiaBossBodyBuilder))
            {
                return false;
            }           

            return true;
        }

        public Skill skill_MafiaBossExperienced { get; private set; }
        public bool Check_MafiaBossExperienced()
        {
            return SkillExist(skill_MafiaBossExperienced);
        }

        public Skill skill_MafiaBossInterception { get; private set; }
        public bool Check_MafiaBossInterception()
        {
            if (!SkillSuccess(skill_MafiaBossInterception))
            {
                return false;
            }

            return true;
        }

        public Skill skill_MafiaBossOneMore { get; private set; }
        public bool Check_MafiaBossOneMore()
        {
            return SkillExist(skill_MafiaBossOneMore);
        }

        public bool isUps = false;
        public Skill skill_MafiaBossUps { get; private set; }
        public bool Check_MafiaBossUps()
        {
            if (!SkillSuccess(skill_MafiaBossUps))
            {
                isUps = false;
                return false;
            }

            //do some action        
            //owner.room.roomLogic.SendPlayerToMorgue(targetPlayer);
            isUps=true;

            //message
            //var name = ColorString.GetColoredString(targetPlayer.playerName, ColorId.Player);

            //var bossRusString = Helper.GetRoleNameById_Rus(roleType);
            //var bossString = ColorString.GetColoredString(bossRusString, ColorId.Role);
            //owner.room.roomChat.PublicMessage($"{bossString} насмерть заморозил игрока {name}");

            return true;
        }
    }
}
