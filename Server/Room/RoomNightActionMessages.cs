using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomNightActionMessages
    {
        private Room room;

        public RoomNightActionMessages(Room room)
        {
            this.room = room;
        }

        public void CheckNightActionMessages()
        {
            //скиллы
            {
                //Logger.Log.Debug($"skill messages");

                ///оборотень
                var werewolfSkills = FindNightActionMessage(RoleType.Werewolf, NightActionId.Skill);
                PrintNightMessageAction(werewolfSkills);

                ///маньяк
                var maniacSkills = FindNightActionMessage(RoleType.Maniac, NightActionId.Skill);
                PrintNightMessageAction(maniacSkills);

                ///мафиози
                var mafiaSkills = FindNightActionMessage(RoleType.Mafia, NightActionId.Skill);
                PrintNightMessageAction(mafiaSkills);

                ///босс мафии
                var mafiaBossSkills = FindNightActionMessage(RoleType.MafiaBoss, NightActionId.Skill);
                PrintNightMessageAction(mafiaBossSkills);

                ///комиссар
                var comissarSkills = FindNightActionMessage(RoleType.Commissar, NightActionId.Skill);
                PrintNightMessageAction(comissarSkills, true);

                ///доктор
                var doctorSkills = FindNightActionMessage(RoleType.Doctor, NightActionId.Skill);
                PrintNightMessageAction(doctorSkills);

                ///свидетель
                var witnessSkills = FindNightActionMessage(RoleType.Witness, NightActionId.Skill);
                PrintNightMessageAction(witnessSkills);

                ///грешник
                var sinnerSkills = FindNightActionMessage(RoleType.Sinner, NightActionId.Skill);
                PrintNightMessageAction(sinnerSkills);

                ///святой
                var saintSkills = FindNightActionMessage(RoleType.Saint, NightActionId.Skill);
                PrintNightMessageAction(saintSkills);

                ///партизан
                var guerillaSkills = FindNightActionMessage(RoleType.Guerilla, NightActionId.Skill);
                PrintNightMessageAction(guerillaSkills);

                ///гражданин
                var citizenSkills = FindNightActionMessage(RoleType.Citizen, NightActionId.Skill);
                PrintNightMessageAction(citizenSkills);
            }

            //экстры
            {
                //Logger.Log.Debug($"extra messages");

                var extra_Messages = FindNightActionMessage_Extras();
                PrintNightMessageAction(extra_Messages);
            }

            //роли
            {
                //Logger.Log.Debug($"role messages");
                ///оборотень
                var werewolfRole = FindNightActionMessage(RoleType.Werewolf, NightActionId.Role);
                PrintNightMessageAction(werewolfRole);

                ///маньяк
                var maniacRole = FindNightActionMessage(RoleType.Maniac, NightActionId.Role);
                PrintNightMessageAction(maniacRole);

                ///мафиози
                var mafiaRole = FindNightActionMessage(RoleType.Mafia, NightActionId.Role);
                PrintNightMessageAction(mafiaRole);

                ///босс мафии
                var mafiaBossRole = FindNightActionMessage(RoleType.MafiaBoss, NightActionId.Role);
                PrintNightMessageAction(mafiaBossRole);

                ///комиссар
                var comissarRole = FindNightActionMessage(RoleType.Commissar, NightActionId.Role);
                PrintNightMessageAction(comissarRole,true);

                ///доктор
                var doctorRole = FindNightActionMessage(RoleType.Doctor, NightActionId.Role);
                PrintNightMessageAction(doctorRole);

                ///свидетель
                var witnessRole = FindNightActionMessage(RoleType.Witness, NightActionId.Role);
                PrintNightMessageAction(witnessRole);

                ///грешник
                var sinnerRole = FindNightActionMessage(RoleType.Sinner, NightActionId.Role);
                PrintNightMessageAction(sinnerRole);

                ///святой
                var saintRole = FindNightActionMessage(RoleType.Saint, NightActionId.Role);
                PrintNightMessageAction(saintRole);

                ///партизан
                var guerillaRole = FindNightActionMessage(RoleType.Guerilla, NightActionId.Role);
                PrintNightMessageAction(guerillaRole);
            }
        }

        private void PrintNightMessageAction(List<NightActionMessage> messages, bool checkLive = false)
        {
            if (messages != null)
            {
                foreach (var m in messages)
                {
                    if (checkLive)
                    {
                        if (m.owner.isLive())
                        {
                            m.DoAction();
                        }
                    }
                    else
                    {
                        m.DoAction();
                    }
                }
            }
        }

        private List<NightActionMessage> nightActionMessages = new List<NightActionMessage>();
        public void AddNightActionMessage(RoleType roleId, NightActionId id, Action action, BasePlayer owner=null)
        {
            var newMessage = new NightActionMessage(roleId, id, action, owner);

            nightActionMessages.Add(newMessage);
        }

        private List<NightActionMessage> FindNightActionMessage(RoleType roleId, NightActionId actionId)
        {
            var result = nightActionMessages.FindAll(
                x => x.roleId == roleId && x.actionId == actionId);

            //if (result == null) result = new List<NightActionMessage>();

            return result;
        }

        private List<NightActionMessage> FindNightActionMessage_Extras()
        {
            return nightActionMessages.FindAll(x => x.actionId == NightActionId.Extra);
        }

        public void Clear()
        {
            nightActionMessages.Clear();
        }
    }
}
