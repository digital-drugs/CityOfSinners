using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    /// <summary>
    /// мафия
    /// </summary>
    public class Mafia : Role
    {
        public Mafia(RoleType roleType) : base(roleType)
        {
            isKiller = true;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            //base.SendVisitMessage();

            owner.GetRoom().roomChat.TeamMessage(owner.GetRoom().roomRoles.badTeam, $"{owner.GetColoredName()} решил убить {targetPlayer.GetColoredName()}");

            var playersGroup = new List<BasePlayer>();

            foreach(var p in owner.GetRoom().players)
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
                    case SkillEffect.MafiaBold: { skill_MafiaBold = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaChameleon: { skill_MafiaChameleon = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaKillX2: { skill_MafiaKillX2 = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaManiac: { skill_MafiaManiac = new Skill(skillId, s.Value); } break;
                    case SkillEffect.MafiaToFreedom: { skill_MafiaToFreedom = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_MafiaBold { get; private set; }
        public bool Check_MafiaBold()
        {
            var mafia = RoomHelper.FindPlayersByRole(RoleType.Mafia, owner.GetRoom());
            
            if(mafia.Count>1) return false;

            if (!SkillSuccess(skill_MafiaBold))
            {
                return false;
            }

            //do some action 
           

            //message
        

            return true;
        }

        public Skill skill_MafiaChameleon { get; private set; }
        /// <summary>
        /// шанс обмануть комиссара, притворившись гражданином, если нет дубликата
        /// </summary>
        /// <returns></returns>
        public bool Check_MafiaChameleon()
        {
            if (!SkillSuccess(skill_MafiaChameleon))
            {
                return false;
            }

            //do some action 

            //message
            //owner.room.roomChat.PublicMessage($"маньяк идеальный");

            return true;
        }

        public Skill skill_MafiaKillX2 { get; private set; }
        /// <summary>
        /// шанс убить случайного соседнего игрока
        /// </summary>
        /// <param name="targetPlayer"></param>
        /// <returns></returns>
        public bool Check_MafiaKillX2(/*BasePlayer targetPlayer*/)
        {
            if (!SkillSuccess(skill_MafiaKillX2))
            {
                return false;
            }

            return true;
        }

        public Skill skill_MafiaManiac { get; private set; }
        public bool Check_MafiaManiac()
        {
            if (!SkillSuccess(skill_MafiaManiac))
            {
                return false;
            }            

            return true;
        }

        public Skill skill_MafiaToFreedom { get; private set; }
        public bool Check_MafiaToFreedom()
        {
            if (!SkillSuccess(skill_MafiaToFreedom))
            {
                return false;
            }

          

            return true;
        }

    }
}
