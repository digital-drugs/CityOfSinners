using ExitGames.Client.Photon.LoadBalancing;
using Mafia_Server.Extras;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;

namespace Mafia_Server
{
    [Serializable]
    public class Role
    {
        private protected bool isKiller = false;
        public bool IsKiller()
        {
            return isKiller;
        }

        public RoleType roleType { get; private set; }
        public Role(RoleType roleType)
        {
            this.roleType = roleType;

            roleEffects = new RoleEffects(this);
        }

        public virtual Role GetRole()
        {
            return this;
        }

        public BasePlayer owner { get; private set; }
        public virtual void SetPlayer(BasePlayer player)
        {
            owner = player;
        }     

        public virtual void SetSkills(Dictionary<string, int> playerSkills)
        {
            foreach (var s in playerSkills)
            {
                var skillId = (SkillEffect)Enum.Parse(typeof(SkillEffect), s.Key);

                switch (skillId)
                {
                    case SkillEffect.ShareSkillFree: { skill_Free = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ShareSkillAntiTalk: { skill_AntiTalk = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ShareSkillNiceAndLong: { skill_NiceAndLong = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ShareSkillRating: { skill_Rating = new Skill(skillId, s.Value); } break;
                    case SkillEffect.ShareSkillVoteX2: { skill_VoteX2 = new Skill(skillId, s.Value); } break;
                }
            }
        }

        //private protected bool isResurected=false;
        //public void SetResurection(bool value)
        //{
        //    isResurected = value;
        //}
        //public bool IsResurected()
        //{
        //    return isResurected;
        //}

        public Skill skill_NiceAndLong { get; private set; }
        public bool Check_SkillNiceAndLong()
        {
            return SkillExist(skill_NiceAndLong);
        }

        public Skill skill_Free { get; private set; }
        public bool Check_SkillFree()
        {
            return SkillExist(skill_Free);
        }

        public Extra GetMirror()
        {
            return roleEffects.FindExtraEffect(ExtraEffect.Mirror);             
        }

        public Skill skill_AntiTalk { get; private set; }
        public bool Check_SkillAntiTalk()
        {
            return SkillExist(skill_AntiTalk);
        }

        public Skill skill_Rating { get; private set; }
        public bool Check_SkillRating()
        {
            return SkillExist(skill_Rating);
        }

        public Skill skill_VoteX2 { get; private set; }
        public bool Check_SkillVoteX2()
        {
            if (!SkillSuccess(skill_VoteX2))
            {
                return false;
            }

            return true;
        }

        private protected bool SkillSuccess(Skill skill)
        {
            if (skill == null) return false;

            if(skill.effect == SkillEffect.ShareSkillVoteX2 &&
               owner.GetRoom().testSkill100.Contains(RoleType.NULL))
            {
                return true;
            }

            if (owner.GetRoom().testSkill100.Contains(owner.playerRole.roleType))
            {
                //Logger.Log.Debug($"skill 100% {skill.effect}");
                return true; 
            }

            var skillDice = owner.GetRoom().dice.Next(100);
           
            //Logger.Log.Debug($"skill id {skill.effect} roll {skillDice} / {skill.skillValue} ");
            
            if (skillDice <= skill.skillValue)
            {            
                //owner.room.roomChat.PublicMessage_UseSkill(skill, $"{owner.GetColoredRole()} использовал навык {skill.GetColoredName()}");

                return true;
            }

            return false;
        }

        private protected bool SkillExist(Skill skill)
        {
            if (skill == null) return false;

            return true;
        }

        public bool CanVisit()
        {
            if (visitBlocked) return false;

            //if (roleType == RoleType.Citizen) return false;

            //экстры
            var sleepElexirExtra = owner.playerRole.roleEffects.FindExtraEffect(ExtraEffect.SleepElexir);
            if (sleepElexirExtra != null)
            {
                //Logger.Log.Debug($"player cant move because extra {sleepElexirExtra.effect}");
                return false;
            }
            //паутина
            var webExtra = owner.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Web);
            if (webExtra != null)
            {
                //Logger.Log.Debug($"player cant move because extra {sleepElexirExtra.effect}");
                return false;
            }
            //бита 20%
            var bitExtra = owner.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Bit);
            if (bitExtra != null)
            {
                //Logger.Log.Debug($"player cant move because extra {sleepElexirExtra.effect}");
                return false;
            }

            //умения
            var guerillaLongEffect = roleEffects. FindSkillEffect(SkillEffect.GuerillaLongEffect);
            if(guerillaLongEffect != null) { return false; }

            var maniacSong = roleEffects. FindSkillEffect(SkillEffect.ManiacSong);
            if (maniacSong != null) { return false; }

            var citizenManiac = roleEffects.FindSkillEffect(SkillEffect.CitizenManiac);
            if (citizenManiac != null) { return false; }

            //роли
            var mafiaBoss = roleEffects. FindRoleEffect(RoleType.MafiaBoss);
            if(mafiaBoss != null) { return false; }

            var jane = roleEffects.FindRoleEffect(RoleType.Jane);
            if(jane != null) { return false; }

            var guerilla = roleEffects.FindRoleEffect(RoleType.Guerilla);
            if (guerilla != null) { return false; }

            return true;
        }

        public bool CanVote()
        {
            //экстры
            var bitExtra = roleEffects.FindExtraEffect(ExtraEffect.Bit);
            if (bitExtra != null)
            {
                //Logger.Log.Debug($"player {player.playerName} cant vote because extra {bitExtra.effect}");
                return false;
            }

            var colaExtra = roleEffects.FindExtraEffect(ExtraEffect.Cola);
            if (colaExtra != null)
            {
                //Logger.Log.Debug($"player {player.playerName} cant vote because extra {colaExtra.effect}");
                return false;
            }

            var fizzExtra = roleEffects.FindExtraEffect(ExtraEffect.Fizz);
            if (fizzExtra != null)
            {
                //Logger.Log.Debug($"player {player.playerName} cant vote because extra {fizzExtra.effect}");
                return false;
            }

            var magicCubeExtra = roleEffects.FindExtraEffect(ExtraEffect.MagicCube);
            if (magicCubeExtra != null)
            {
                //Logger.Log.Debug($"player {player.playerName} cant vote because extra {magicCubeExtra.effect}");
                return false;
            }

            //умения
            var citizenManiac = roleEffects.FindSkillEffect(SkillEffect.CitizenManiac);
            if (citizenManiac != null)
            {
                return false;
            }

            //роли
            var mafiaBossEffect = roleEffects.FindRoleEffect(RoleType.MafiaBoss);
            if(mafiaBossEffect != null)
            {
                return false;
            }            

            return true;
        }

        public bool CanUseExtra()
        {


            return true;
        }

        public bool visitBlocked { get; private set; } = false;
        public void BlockVisit(bool value)
        {
            visitBlocked = value;
        }

        public RoleEffects roleEffects { get; private set; } 
        public void SetRoleEffects(RoleEffects roleEffects)
        {
            this.roleEffects = roleEffects;
        }

        public virtual bool CheckResist(BasePlayer guest)
        {
            //проверка на защитные экстры
            if(CheckResistExtras(guest))
            {
                return true; 
            }

            //проверка на спасающие роли    
            if (CheckResistRoles(guest))
            {
                return true;
            }         

            return false;
        }

        public bool CheckResistExtras(BasePlayer guest)
        {
            var maniacIsIdeal = false;
            Maniac maniacRole = null;
            if (guest.playerRole.roleType == RoleType.Maniac)
            {
                maniacRole = (Maniac)guest.playerRole;
                maniacIsIdeal = maniacRole.maniacIsIdeal;
            }

            //кипяток против партизана
            if (guest.playerRole.roleType == RoleType.Guerilla)
            {
                var guerillaRole = (Guerilla)guest.playerRole;

                if (owner.team.teamType == TeamType.Good ) 
                {
                    if (guerillaRole.Check_GuerillaOwn())
                    {
                        owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Guerilla,
                        NightActionId.Role,
                        () =>
                        {
                            owner.GetRoom().roomChat.Skill_PersonalMessage(guest, guerillaRole.skill_GuerillaOwn,
                                $"Сработал навык {ColorString.GetColoredSkill("Я свой")}, " +
                                $"Вы не лишили {owner.GetColoredName()} хода");
                        }
                        );                      

                        return true;
                    }
                }

                var hotWaterExtra = owner.FindExtraInSlots(ExtraEffect.HotWater);

                if (hotWaterExtra != null)
                {
                    if (guerillaRole.Check_GuerillaIce())
                    {
                        //message

                        owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Guerilla,
                        NightActionId.Skill,
                        () =>
                        {
                            owner.GetRoom().roomChat.Skill_PersonalMessage(owner, guerillaRole.skill_GuerillaIce,
                                $"{ColorString.GetColoredRole("Партизан")} игнорировал Ваш " +
                                $"{ColorString.GetColoredExtra("Кипяток")}, благодаря навыку " +
                                $"{ColorString.GetColoredSkill("Лёд")}");

                            owner.GetRoom().roomChat.Skill_PersonalMessage(guest, guerillaRole.skill_GuerillaIce,
                                $"Навык {ColorString.GetColoredSkill("Лёд")} помог игнорировать " +
                                $"{ColorString.GetColoredExtra("Кипяток")} у {owner.GetColoredName()}");
                        }
                        );                      

                        return false;
                    }

                    else
                    {

                        owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
(
RoleType.Guerilla,
NightActionId.Extra,
() =>
{
    //владелец кипятка
    owner.GetRoom().roomChat.Extra_PersonalMessage(owner, hotWaterExtra,
        $"Ваш {ColorString.GetColoredExtra("Кипяток")} " +
        $"отпугнул {ColorString.GetColoredRole("Партизана")}");

    owner.GetRoom().roomChat.Extra_PersonalMessage(guest, hotWaterExtra,
        $"{owner.GetColoredName()} облил Вас " +
        $"{ColorString.GetColoredExtra("Кипятком")}, " +
        $"поговорить не получилось");

    var excludedPlayers = new BasePlayer[0];// { owner, guest };
    owner.GetRoom().roomChat.Extra_PublicMessageExcludePlayers(
        $"Игрок облил {ColorString.GetColoredRole("Партизана")} " +
        $"{ColorString.GetColoredExtra("Кипятком")} и избежал болтовни",
        owner.GetRoom(), hotWaterExtra, excludedPlayers);
}
);

                       

                        hotWaterExtra.DecreaseCount();

                        return true;
                    }
                }
            }

            //кипяток против босса мафии
            if (guest.playerRole.roleType == RoleType.MafiaBoss)
            {
                var mafiaBossRole = (MafiaBoss)guest.playerRole;

                var hotWaterExtra = owner.FindExtraInSlots(ExtraEffect.HotWater);

                if (hotWaterExtra != null && !mafiaBossRole.isKiller)
                {
                    //владелец кипятка

                    owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
(
RoleType.MafiaBoss,
NightActionId.Extra,
() =>
{

    owner.GetRoom().roomChat.Extra_PersonalMessage(owner, hotWaterExtra,
        $"Ваш {ColorString.GetColoredExtra("Кипяток")} " +
        $"отпугнул {ColorString.GetColoredRole("Босса мафии")}");

    owner.GetRoom().roomChat.Extra_PersonalMessage(guest, hotWaterExtra,
        $"{owner.GetColoredName()} облил Вас " +
        $"{ColorString.GetColoredExtra("Кипятком")}, " +
        $"заморозить не получилось");

    var excludedPlayers = new BasePlayer[] { owner, guest };
    owner.GetRoom().roomChat.Extra_PublicMessageExcludePlayers(
        $"Игрок облил {ColorString.GetColoredRole("Босса мафии")} " +
        $"{ColorString.GetColoredExtra("Кипятком")} и избежал заморозки",
        owner.GetRoom(), hotWaterExtra, excludedPlayers);
}
);


                    hotWaterExtra.DecreaseCount();

                    return true;
                }
            }

            //защита кокона
            var cocoonExtra = roleEffects.FindExtraEffect(ExtraEffect.Cocoon);
            if (cocoonExtra != null)
            {
                if (!maniacIsIdeal)
                {
                    owner.GetRoom().roomChat.Extra_PublicMessage(cocoonExtra,
                        $" {owner.GetColoredName()} спасен коконом");
                    return true;
                }
                else
                {
                    maniacRole.idealUsed = true;
                }
            }

            //защита накидки
            if (
                (guest.playerRole.IsKiller() || guest.playerRole.roleType == RoleType.Commissar)
                && 
                ( owner.GetRoom().roomPhases.gamePhase != GamePhase.FirstNight 
                || owner.GetRoom().roomPhases.gamePhase != GamePhase.EndFirstNight))
            {

                Logger.Log.Debug($"resist by cape {owner.GetRoom().roomPhases.gamePhase}");

                var capeExtra = owner.FindExtraInSlots(ExtraEffect.Cape);
                if (capeExtra != null )
                {
                    capeExtra.DecreaseCount();

                    if( guest.playerRole.roleType == RoleType.Commissar)
                    {
                        owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        owner.playerRole.roleType,
                        NightActionId.Extra,
                        () =>
                        {
                            owner.GetRoom().roomChat.Extra_PersonalMessage(
                                owner, capeExtra, $"{ColorString.GetColoredExtra("Накидка")} " +
                                $"спасла Вас от проверки {ColorString.GetColoredRole("Комиссара")}");

                            var excludedPlayers = new BasePlayer[] { owner };

                            var message = $"{owner.GetColoredName()} скрылся от проверки " +
                                $"{ColorString.GetColoredRole("Комиссара")}, " +
                                $"благодаря {ColorString.GetColoredExtra("Накидке")}";

                            owner.GetRoom().roomChat.Extra_PublicMessageExcludePlayers(
                                message, owner.GetRoom(), capeExtra, excludedPlayers);
                        }
                        );                      
                    }
                    else
                    {
                        if(!maniacIsIdeal)
                        {
                            var fromRoleString = ColorString.GetColoredFromRole(guest.playerRole.roleType);

                            owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                            (
                            owner.playerRole.roleType,
                            NightActionId.Extra,
                            () =>
                            {
                                owner.GetRoom().roomChat.Extra_PersonalMessage(owner, capeExtra,
                                    $"{ColorString.GetColoredExtra("Накидка")} " +
                                    $"спасла Вас от покушения {fromRoleString}");

                                var excludedPlayers = new BasePlayer[] { owner };

                                var message = $"{owner.GetColoredName()} избежал покушения " +
                                    $"{fromRoleString}, " +
                                    $"благодаря {ColorString.GetColoredExtra("Накидке")}";
                                    owner.GetRoom().roomChat.Extra_PublicMessageExcludePlayers(
                                        message, owner.GetRoom(), capeExtra, excludedPlayers);
                            }
                            );
                        }                                 
                    }

                    if (!maniacIsIdeal)
                    {
                        return true;
                    }
                    else
                    {
                        maniacRole.idealUsed = true;
                    }

                    if(guest.playerRole.roleType != RoleType.Commissar
                        &&
                         owner.GetRoom().roomLogic.commissarVisit.comissar!=null
                         &&
                          owner.GetRoom().roomLogic.commissarVisit.comissar.targetPlayer == owner)
                    {
                        if (capeExtra.count == 0 && owner.GetRoom().roomLogic.noMoreKillers ==false )
                        {
                            //вызов на ход комиссара
                            owner.GetRoom().roomLogic.commissarVisit.Visit();
                        }

                        if (owner.GetRoom().roomLogic.noMoreKillers)
                        {
                            owner.GetRoom().roomLogic.commissarVisit.Visit(false);
                        }
                       
                    }                 
                }
            }         

            //защита маски от маньяка
            if (guest.playerRole.roleType == RoleType.Maniac)
            {
                var maskExtra = owner.FindExtraInSlots(ExtraEffect.Mask);
                if (maskExtra != null)
                {
                    maskExtra.DecreaseCount();

                    if (!maniacIsIdeal)
                    {
                        return true;
                    }
                    else
                    {
                        maniacRole.idealUsed = true;
                    }
                }
            }       

            //защита ловушки
            var trapExtra = owner.FindExtraInSlots(ExtraEffect.Trap);
            if (trapExtra != null)
            {
                trapExtra.DecreaseCount();

                var roleString = guest.GetColoredRole();

                if (!maniacIsIdeal)
                {
                    owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
(
owner.playerRole.roleType,
NightActionId.Extra,
() =>
{

    owner.GetRoom().roomChat.PublicMessage($"{roleString} погиб в ловушке");

}
);

                  
                    owner.GetRoom().roomLogic.SendPlayerToMorgue(guest);
                    return true;
                }
                else
                {
                    maniacRole.idealUsed = true;
                }
            }

            return false;
        }

        public bool CheckResistRoles(BasePlayer guest)
        {
            var resist = false;

            //защита комиссаром
            var comissarEffect = roleEffects.FindRoleEffect(RoleType.Commissar);
            if (comissarEffect != null && guest.team.teamType != TeamType.Good) 
            {
                Logger.Log.Debug($"comissar try resist from {guest.playerRole}");

                if (guest.playerRole.IsKiller())
                {
                    var fromRoleString = ColorString.GetColoredFromRole(guest.playerRole.roleType);

                    owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Commissar,
                    NightActionId.Role,
                    () =>
                    {
                        owner.GetRoom().roomChat.PublicMessage(
                            $"{ColorString.GetColoredRole("Комиссар")} " +
                            $"предотвратил покушение {fromRoleString} " +
                            $"на {guest.targetPlayer.GetColoredName()}");
                    }, 
                    comissarEffect.owner
                    );

                    resist = true;
                }
            }

            //защита доктором
            var doctorEffect = roleEffects.FindRoleEffect(RoleType.Doctor);
            if (doctorEffect != null && guest.team.teamType != TeamType.Good)
            {
                if(guest.playerRole.IsKiller())
                {
                    var fromRoleString = ColorString.GetColoredFromRole(guest.playerRole.roleType);

                    owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                    (
                    RoleType.Doctor,
                    NightActionId.Role,
                    () =>
                    {
                    owner.GetRoom().roomChat.PublicMessage($"{ColorString.GetColoredRole("Доктор")} спас " +
                        $"{guest.targetPlayer.GetColoredName()} " +
                        $"от покушения {fromRoleString}");
                    }
                    );

                    roleEffects.RemoveRoleEffect(doctorEffect);
                    resist = true;
                }                
            }

            return resist;
        }


        public bool CheckResistSkills(BasePlayer guest)
        {
            if(roleType == RoleType.Commissar && guest.playerRole.IsKiller())
            {
                if(owner.GetRoom().roomLogic.saintVisit.CheckSaintGuard(owner))
                {
                    foreach (var g in owner.GetRoom().roomLogic.saintVisit.guardList)
                    {
                        //персональное сообщение каждому святому, у которого сработал "оберег"
                        owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Saint,
                        NightActionId.Skill,
                        () =>
                        {
                            owner.GetRoom().roomChat.Skill_PersonalMessage(g, owner.GetRoom().roomLogic.saintVisit.skillSaintGuard,
                                    $"{ColorString.GetColoredRole("Комиссар")} был спасен " +
                                    $"{ColorString.GetColoredRole("Святым")}, благодаря навыку " +
                                    $"{ColorString.GetColoredSkill("Оберег")}. Ваш навык сработал");
                        }
                        );
                    }

                    owner.GetRoom().roomLogic.nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Saint,
                        NightActionId.Skill,
                        () =>
                        {
                            var excludedPlayers = owner.GetRoom().roomLogic.saintVisit.guardList.ToArray();

                            owner.GetRoom().roomChat.Skill_PublicMessageExcludePlayers(
                               $"{ColorString.GetColoredRole("Комиссар")} был спасен " +
                               $"{ColorString.GetColoredRole("Святым")}, благодаря навыку " +
                               $"{ColorString.GetColoredSkill("Оберег")}",
                               owner.GetRoom(), owner.GetRoom().roomLogic.saintVisit.skillSaintGuard, excludedPlayers);
                        }
                        );

                    return true;
                }
            }
            return false;
        }

        public virtual void SendVisitMessage(BasePlayer targetPlayer)
        {
           
        }

        public void UpdateRoleStatus()
        {
            if (owner.playerType != PlayerType.Player) return;

            if (!CanVote())
            {
                DisableVote();
            }

            if (!CanVisit())
            {
                DisableVisit();
            }
        }

        public void DisableVote()
        {
            OperationResponse resp = new OperationResponse((byte)Request.DisableVote);
            resp.Parameters = new Dictionary<byte, object>();
            owner.client.SendOperationResponse(resp, Options.sendParameters);
        }

        public void DisableVisit()
        {
            OperationResponse resp = new OperationResponse((byte)Request.DisableVisit);
            resp.Parameters = new Dictionary<byte, object>();
            owner.client.SendOperationResponse(resp, Options.sendParameters);
        }
    }
}
