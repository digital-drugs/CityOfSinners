using Mafia_Server.Extras;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoleEffects
    {
        private Role role;
        public RoleEffects(Role role)
        {
            this.role = role;
        }

        #region скиллы примененные против этой роли

        private List<Skill> skillEffects = new List<Skill>();
        public void AddSkillEffect(Skill skill)
        {
            skillEffects.Add(skill);

           role. UpdateRoleStatus();
        }
        public void RemoveSkillEffect(Skill skill)
        {
            skillEffects.Remove(skill);
            //role.UpdateRoleStatus();
        }
        public Skill FindSkillEffect(SkillEffect effect)
        {
            return skillEffects.Find(x => x.effect == effect);
        }

        private List<SkillHandler> skillHandlers = new List<SkillHandler>();
        public void AddSkillHandler(SkillHandler skillHandler)
        {
            skillHandlers.Add(skillHandler);

            //SendAddExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"add handler for {extraHandler.extra.extraId}");
        }

        public void RemoveSkillHandler(SkillHandler skillHandler)
        {
            skillHandlers.Remove(skillHandler);

            //SendRemoveExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"remove handler for {extraHandler.extra.extraId}");
        }

        #endregion

        #region роли примененные против этой роли

        private List<Role> roleEffects = new List<Role>();
        public void AddRoleEffect(Role role)
        {
            roleEffects.Add(role);

            role.UpdateRoleStatus();
        }
        public void RemoveRoleEffect(Role role)
        {
            roleEffects.Remove(role);
        }
        public Role FindRoleEffect(RoleType effect)
        {
            return roleEffects.Find(x => x.roleType == effect);
        }

        private List<RoleHandler> roleHandlers = new List<RoleHandler>();
        public void AddRoleHandler(RoleHandler roleHandler)
        {
            roleHandlers.Add(roleHandler);

            //SendAddExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"add handler for {extraHandler.extra.extraId}");
        }

        public void RemoveRoleHandler(RoleHandler roleHandler)
        {
            roleHandlers.Remove(roleHandler);

            //SendRemoveExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"remove handler for {extraHandler.extra.extraId}");
        }



        #endregion

        #region экстры примененные против этой роли

        private List<Extra> extraEffects = new List<Extra>();
        public void AddExtraEffect(Extra extra)
        {
            extraEffects.Add(extra);

            role.UpdateRoleStatus();
        }
        public void RemoveExtraEffect(Extra extra)
        {
            extraEffects.Remove(extra);
        }
        public Extra FindExtraEffect(ExtraEffect effect)
        {
            return extraEffects.Find(x => x.effect == effect);        
        }

        private List<ExtraHandler> extraHandlers = new List<ExtraHandler>();
        public void AddExtraHandler(ExtraHandler extraHandler)
        {
            extraHandlers.Add(extraHandler);

            role.owner. SendAddExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"add handler for {extraHandler.extra.extraId}");
        }

        public void RemoveExtraHandler(ExtraHandler extraHandler)
        {
            extraHandlers.Remove(extraHandler);

            role.owner.SendRemoveExtraEffectToExtraOwner(extraHandler);

            //Logger.Log.Debug($"remove handler for {extraHandler.extra.extraId}");
        }

        private List<DelayedExtraHandler> delayedEextraHandlers = new List<DelayedExtraHandler>();
        public void AddDelayedExtraHandler(DelayedExtraHandler delayedExtraHandler)
        {
            delayedEextraHandlers.Add(delayedExtraHandler);

            role.owner.SendAddExtraEffectToExtraOwner(delayedExtraHandler);

            //Logger.Log.Debug($"add handler for {extraHandler.extra.extraId}");
        }

        public void RemoveDelayedExtraHandler(DelayedExtraHandler delayedExtraHandler)
        {
            delayedEextraHandlers.Remove(delayedExtraHandler);

            role.owner.SendRemoveExtraEffectToExtraOwner(delayedExtraHandler);

            //Logger.Log.Debug($"remove handler for {extraHandler.extra.extraId}");
        }

        private List<EventExtraHandler> eventExtraHandlers = new List<EventExtraHandler>();
        public void AddEventExtraHandler(EventExtraHandler eventExtraHandler)
        {
            eventExtraHandlers.Add(eventExtraHandler);

            role.owner.SendAddExtraEffectToExtraOwner(eventExtraHandler);
            //Logger.Log.Debug($"add handler for {extraHandler.extra.extraId}");
        }
        public void RemoveEventExtraHandler(EventExtraHandler eventExtraHandler)
        {
            eventExtraHandlers.Remove(eventExtraHandler);

            role.owner.SendRemoveExtraEffectToExtraOwner(eventExtraHandler);

            //Logger.Log.Debug($"remove handler for {extraHandler.extra.extraId}");
        }

        #endregion


        #region death timer

        public IDisposable chatTimer;
        private int timeLimit = Options.ChatSilenceKillLimit;
        private int chatSilenceKillWarning = 10;
        public void ChatTimer()
        {
            timeLimit--;

            if (timeLimit == chatSilenceKillWarning)
            {
                if (role.owner. isLive())
                {
                    role.owner.GetRoom().roomChat.PersonalMessage(role.owner, $"Не молчи! Тишина заберет тебя через {timeLimit} секунд!");
                }
            }

            if (timeLimit == 0)
            {
                if (role.owner.isLive())
                {
                    role.owner.GetRoom().roomChat.PublicMessage(
                        $"Молчание убило {role.owner.GetColoredName()} - " +
                        $"{role.owner.GetColoredRole()}");

                    role.owner.GetRoom().roomLogic.SendPlayerToMorgue(role.owner);
                }

                StopChatTimer();
            }
        }

        public void UpdateChatTimer()
        {
            timeLimit = Options.ChatSilenceKillLimit;
        }

        public void StopChatTimer()
        {
            if (chatTimer == null) { return; }
            chatTimer.Dispose();
        }

        #endregion

    }
}
