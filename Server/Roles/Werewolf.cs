using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Werewolf : Role
    {
        public Werewolf(RoleType roleType) : base(roleType)
        {
            isKiller = true;
        }

        public override void SendVisitMessage(BasePlayer targetPlayer)
        {
            if(owner.team.teamType == TeamType.Bad)
            {
                var team = owner.GetRoom().roomRoles.badTeam;
                owner.GetRoom().roomChat.TeamMessage(team, $"{owner.GetColoredName()} иду убивать {targetPlayer.GetColoredName()}");

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
            else
            {
                owner.GetRoom().roomChat.PersonalMessage(owner, $"{owner.GetColoredName()} иду убивать {targetPlayer.GetColoredName()}");

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

        }
        public override void SetSkills(Dictionary<string, int> playerSkills)
        {
            base.SetSkills(playerSkills);

            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.WerewolfChange: { skill_WerewolfChange = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WerewolfOneMore: { skill_WerewolfOneMore = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WerewolfRage: { skill_WerewolfRage = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WerewolfThunder: { skill_WerewolfThunder = new Skill(skillId, s.Value); } break;
                    case SkillEffect.WerewolfUnlimit: { skill_WerewolfUnlimit = new Skill(skillId, s.Value); } break;
                }
            }
        }

        public Skill skill_WerewolfChange { get; private set; }
        public bool Check_WerewolfChange()
        {
            if (!SkillSuccess(skill_WerewolfChange))
            {
                return false;
            }

            //do some action
            //ChangeTeamToRandom();

            return true;
        }

        public Team ChangeTeamToRandom()
        {
            List<Team> availableTeams = new List<Team>();

            if(owner.GetRoom().roomRoles.badTeam.GetLivePlayers().Count>0)
            {
                availableTeams.Add(owner.GetRoom().roomRoles.badTeam);
            }

            if (owner.GetRoom().roomRoles.goodTeam.GetLivePlayers().Count > 0)
            {
                availableTeams.Add(owner.GetRoom().roomRoles.goodTeam);
            }

            availableTeams.Add(owner.GetRoom().roomRoles.neutralTeam);

            availableTeams.Remove(owner.team);           

            Team resultTeam = null;

            if (availableTeams.Count > 0)
            {
                var dice = owner.GetRoom().dice.Next(availableTeams.Count);
                resultTeam = availableTeams[dice];
            }
            else
            {
                resultTeam = owner.team;
            }

            Logger.Log.Debug($"werewolf change team to {owner.team.teamType}");

            return resultTeam;
        }

        private Skill skill_WerewolfOneMore;
        public bool Check_WerewolfOneMore()
        {
            return SkillExist(skill_WerewolfOneMore);
        }

        public Skill skill_WerewolfRage { get; private set; }
        public bool Check_WerewolfRage()
        {
            //если оборотень не играет за себя, то скилл не срабатывает
            if (owner.team.teamType != TeamType.Neutral) return false;

            if (!SkillSuccess(skill_WerewolfRage))
            {
                return false;
            }

            //do some action
            //получить 2 соседние цели от targetPlayer
            //повесить ээфект роли оборотня на 2 соседние цели (учитывать зеркало?!)

            //message
            //owner.room.roomChat.PublicMessage($"{ColorString.GetColoredRole(roleType)} впал в ярость!");

            return true;
        }

        public Skill skill_WerewolfThunder { get; private set; }
        public bool Check_WerewolfThunder()
        {
            if (!SkillSuccess(skill_WerewolfThunder))
            {
                return false;
            }

            //do some action
            //message
            //var roleRusString = Helper.GetRoleNameById_Rus(roleType);
            //var roleString = ColorString.GetColoredString(roleRusString, ColorId.Role);
            //owner.room.roomChat.PublicMessage($"{roleString} выбрал сражаться за себя");
            //owner.room.roomChat.PersonalMessage(owner, $"гром на улице. беги убивать");

            BlockVisit(true);

            return true;
        }
       
        public Skill skill_WerewolfUnlimit { get; private set; }
        public bool Check_WerewolfUnlimit()
        {
            if (!SkillSuccess(skill_WerewolfUnlimit))
            {
                return false;
            }

          

            return true;
        }

        //ивент смены команды оборотнем
        public void Event_chnageTeam()
        {
            EventData eventData = new EventData((byte)Events.ChangeTeam_Werewolf);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.TeamType, owner.team.teamType);
            eventData.SendTo(owner.GetRoom().clients, Options.sendParameters);
        }
    }
}
