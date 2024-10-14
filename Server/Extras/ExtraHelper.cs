using Share;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mafia_Server.Extras
{
    public class ExtraHelper
    {
        private Room room;

        public ExtraHelper(Room room)
        {
            this.room = room;
        }

        #region General Extras
        public void AddExtraEffectToPlayer(BasePlayer player, Extra extra, DurationType duration = DurationType.Null)
        {
            player.playerRole.roleEffects.AddExtraEffect(extra);

            var extraHandler = new ExtraHandler(player, extra, room, duration);

            player.playerRole.roleEffects.AddExtraHandler(extraHandler);
        }
        public bool UseExtra(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraId = extra.extraId;

            var result = false;

            var extraEffect = Helper.GetEnumElement<ExtraEffect>(extraId);

            Logger.Log.Debug($"player {player.playerName} use extra {extraEffect}");

            switch (extraEffect)
            {
                //smoke
                case ExtraEffect.Kalash: { result = UseExtra_Kalash(player, extra); } break;
                //case ExtraEffect.Armor: { result = UseExtra_Armor(player); } break;
                case ExtraEffect.SleepElexir: { result = UseExtra_SleepElexir(player, extra); } break;
                case ExtraEffect.Bit: { result = UseExtra_Bit(player, extra, extraData); } break;
                case ExtraEffect.Сonfession: { result = UseExtra_Сonfession(player,extra, extraData); } break;
                case ExtraEffect.Eye: { result = UseExtra_Eye(player,extra, extraData); } break;
                //case ExtraEffect.HotWater: { result = UseExtra_HotWater(player); } break;
                //case ExtraEffect.GoldenFleece: { result = UseExtra_GoldenFleece(player); } break;
                case ExtraEffect.WireTape: { result = UseExtra_WireTape(player, extra, extraData); } break;
                //case ExtraEffect.RadioSet: { result = UseExtra_RadioSet(player); } break;
                case ExtraEffect.Iron: { result = UseExtra_Iron(player, extra, extraData); } break;
                case ExtraEffect.ActiveRole: { result = UseExtra_ActiveRole(player); } break;
                case ExtraEffect.Cuff: { result = UseExtra_Cuff(player, extra, extraData); } break;
                //case ExtraEffect.AirPlane: { result = UseExtra_AirPlane(player); } break;
                //case ExtraEffect.MineDetector: { result = UseExtra_MineDetector(player); } break;
                case ExtraEffect.Grenade: { result = UseExtra_Grenade(player, extra); } break;
                //case ExtraEffect.Lightning: { result = UseExtra_Lightning(player); } break;
                case ExtraEffect.Explosion: { result = UseExtra_Explosion(player, extra, extraData); } break;
                //case ExtraEffect.Mine: { result = UseExtra_Mine(player); } break;
                //case ExtraEffect.Cape: { result = UseExtra_Cape(player); } break;
                //case ExtraEffect.Dublicat: { result = UseExtra_Dublicat(player); } break;
                case ExtraEffect.Gun: { result = UseExtra_Gun(player, extra, extraData); } break;
                case ExtraEffect.BraceKnukles: { result = UseExtra_BraceKnukles(player); } break;
                case ExtraEffect.MagicCube: { result = UseExtra_MagicCube(player, extra, extraData); } break;
                //case ExtraEffect.FoolsCap: { result = UseExtra_FoolsCap(player); } break;
                //case ExtraEffect.PopCorn: { result = UseExtra_PopCorn(player); } break;
                //case ExtraEffect.Keys: { result = UseExtra_Keys(player); } break;
                case ExtraEffect.Cola: { result = UseExtra_Cola(player, extra); } break;
                case ExtraEffect.Mirror: { result = UseExtra_Mirror(player, extra); } break;
                case ExtraEffect.Dart: { result = UseExtra_Dart(player, extra); } break;
                case ExtraEffect.Kinder: { result = UseExtra_Kinder(player); } break;
                case ExtraEffect.Fizz: { result = UseExtra_Fizz(player, extra, extraData); } break;
                //case ExtraEffect.CosmoRait: { result = UseExtra_CosmoRait(player); } break;
                case ExtraEffect.Hidrogen: { result = UseExtra_Hidrogen(player, extra); } break;
                //case ExtraEffect.Star: { result = UseExtra_Star(player); } break;
                case ExtraEffect.Asteroid: { result = UseExtra_Asteroid(player); } break;
                case ExtraEffect.Comet: { result = UseExtra_Comet(player,extra,extraData); } break;

                //case ExtraEffect.Orbit: { result = UseExtra_Orbit(player); } break;
                case ExtraEffect.Spacesuit: { result = UseExtra_Spacesuit(player, extra); } break;
                //case ExtraEffect.SpiderPaw: { result = UseExtra_SpiderPaw(player); } break;
                case ExtraEffect.Trap: { result = UseExtra_Trap(player); } break;
                case ExtraEffect.Web: { result = UseExtra_Web(player, extra, extraData); } break;
                case ExtraEffect.Cocoon: { result = UseExtra_Cocoon(player, extra); } break;
                case ExtraEffect.Scarab: { result = UseExtra_Scarab(player, extra); } break;
                case ExtraEffect.Toxin: { result = UseExtra_Toxin(player, extra); } break;
                //case ExtraEffect.Hook: { result = UseExtra_Hook(player); } break;
                //case ExtraEffect.Skull: { result = UseExtra_Skull(player); } break;
                case ExtraEffect.PirateBomb: { result = UseExtra_PirateBomb(player); } break;
                case ExtraEffect.Saber: { result = UseExtra_Saber(player); } break;
                case ExtraEffect.Tracker: { result = UseExtra_Tracker(player, extra, extraData); } break;
                case ExtraEffect.Botle: { result = UseExtra_Botle(player); } break;
                //case ExtraEffect.PirateBox: { result = UseExtra_PirateBox(player); } break;
                //case ExtraEffect.NewYearToy: { result = UseExtra_NewYearToy(player); } break;
                case ExtraEffect.SnowBall: { result = UseExtra_SnowBall(player); } break;
                //case ExtraEffect.Pumpkin: { result = UseExtra_Pumpkin(player); } break;
                //case ExtraEffect.Mask: { result = UseExtra_Mask(player); } break;
                //case ExtraEffect.Lili: { result = UseExtra_Lili(player); } break;
                case ExtraEffect.Shovel: { result = UseExtra_Shovel(player); } break;
                default:
                    {
                        Logger.Log.Debug($"player {player.playerName} use BAD extra {extraEffect}");
                    }
                    break;
            }

            return result;
        }


        public bool UseExtra_Kalash(BasePlayer player, Extra extra)
        {
            var tempActivePlayer = room.GetLivePlayers();

            if (player.playerRole.roleType == RoleType.Saint)
            {
                var resistantSkill = ((Saint)player.playerRole).Check_SaintResistant();

                if (resistantSkill)
                {
                    tempActivePlayer.Remove(player.playerId);
                }
            }

            if (tempActivePlayer.Count == 0)
            {
                Logger.Log.Debug($"no target for extra kalash. active players => {tempActivePlayer.Count}");
                return false;
            }

            var randomPlayerIndex = room.dice.Next(tempActivePlayer.Count);

            var randomPlayerId = tempActivePlayer.ElementAt(randomPlayerIndex).Key;

            var targetPlayer = GetExtraTarget(player, randomPlayerId);

            room.roomLogic.SendPlayerToMorgue(targetPlayer);


            var kalash = ColorString.GetColoredExtra("Калаша");

            //сообщение владельцу 

            //сообщение жертве
            targetPlayer.GetRoom().roomChat.Extra_PersonalMessage(targetPlayer, extra, $"{player.GetColoredName()} убил из {kalash} " +
                $"{targetPlayer.GetColoredName()} - {targetPlayer.GetColoredRole()}. Вас убили, увы!");

            var excludePlayers = new BasePlayer[] { targetPlayer };
            var extraAction = $"{player.GetColoredName()} убил из {kalash} " +
                $"{targetPlayer.GetColoredName()} - {targetPlayer.GetColoredRole()}";
            room.roomChat.Extra_PublicMessageExcludePlayers($"{extraAction}", room, extra, excludePlayers);

            return true;
        }       

        public bool UseExtra_SleepElexir(BasePlayer player, Extra extra)
        {
            var sleepGoodTeam = true;
            var sleepBadTeam = true;
            //var sleepNeutralTeam = true;
            var sleepPirateTeam = true;

            switch (player.team.teamType)
            {
                case TeamType.Good: sleepGoodTeam = false; break;
                case TeamType.Bad: sleepBadTeam = false; break;
                //case TeamType.Neutral: sleepNeutralTeam = false; break;
                case TeamType.Pirate: sleepPirateTeam = false; break;
            }

            var sleepElexir = ColorString.GetColoredExtra("Эликсир сна");

            foreach (var p in room.GetLivePlayers().Values)
            {
                var mustSleep = false;

                switch (p.team.teamType)
                {
                    case TeamType.Good: mustSleep= sleepGoodTeam; break;
                    case TeamType.Bad: mustSleep = sleepBadTeam ; break;
                    case TeamType.Neutral: if (p != player) mustSleep = true; break;
                    case TeamType.Pirate: mustSleep = sleepPirateTeam ; break;
                }    

                if (mustSleep)
                {
                    var extraTargetPlayer = GetExtraTarget(player, p.playerId);
                    AddExtraEffectToPlayer(extraTargetPlayer, extra);

                    room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extra, $"{sleepElexir} лишил Вас ночного хода");
                }
            }            

            room.roomChat.Extra_PersonalMessage(player, extra, $"Вы использовали {sleepElexir}");

            var excludePlayers = new BasePlayer[] { player };

            switch (player.team.teamType)
            {
                case TeamType.Good: 
                    {
                        var message = $"Команда Мирных использовала {sleepElexir}";
                        room.roomChat.Extra_TeamMessage(player.team, extra, message);
                    }break;
                case TeamType.Bad:
                    {
                        var message = $"Команда Мафии использовала {sleepElexir}";
                        room.roomChat.Extra_TeamMessage(player.team, extra, message);
                    }
                    break;
                case TeamType.Pirate:
                    {
                        var message = $"Команда Пиратов использовала {sleepElexir}";
                        room.roomChat.Extra_TeamMessage(player.team, extra, message);
                    }
                    break;
                case TeamType.Neutral:
                    {
                        var message = $"{player.GetColoredRole()} использовал {sleepElexir}";
                        room.roomChat.Extra_PublicMessageExcludePlayers(message, room, extra, excludePlayers);
                    }
                    break;
            }

            return true;
        }
        public bool UseExtra_Bit(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            //проверка на доктора
            if (player.playerRole.roleType == RoleType.Doctor)
            {
                var doctorRole = (Doctor)player.playerRole;

                if(doctorRole.Check_DoctorCrazyDoc())
                {
                    room.roomLogic.SendPlayerToMorgue(extraTargetPlayer);

                    room.roomChat.Skill_PersonalMessage(player, doctorRole.skill_DoctorCrazyDoc,
                       $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} убит " +
                       $"{ColorString.GetColoredExtra("Битой")} {ColorString.GetColoredSkill("Бешенного дока")}. " +
                       $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(extraTargetPlayer, doctorRole.skill_DoctorCrazyDoc,
                        $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} убит " +
                       $"{ColorString.GetColoredExtra("Битой")} {ColorString.GetColoredSkill("Бешенного дока")}. " +
                       $"Вы убиты, увы!");

                    var excludedPlayers = new BasePlayer[] { player, extraTargetPlayer };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} убит " +
                       $"{ColorString.GetColoredExtra("Битой")} {ColorString.GetColoredSkill("Бешенного дока")}",
                        room, doctorRole.skill_DoctorCrazyDoc, excludedPlayers);

                    return true;
                }
            }           

          

            var bit = ColorString.GetColoredExtra("Битой");
            var excludePlayers = new BasePlayer[] { player, extraTargetPlayer };

            var dice = room.dice.Next(0, 100);
            if(dice < 20)
            {
                //владелец биты
                room.roomChat.Extra_PersonalMessage(player, extra, $"Вы ударили {extraTargetPlayer.GetColoredName()} " +
               $"{bit} и лишили его права голоса и ночного хода");

                //цель биты
                room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extra, $"Вас ударили {bit}, отдохните");

                //остальные
                room.roomChat.Extra_PublicMessageExcludePlayers($"{extraTargetPlayer.GetColoredName()} ударили {bit}, " +
                    $"он лишен права голоса и ночного хода"
                    , room, extra,excludePlayers);

                AddExtraEffectToPlayer(extraTargetPlayer, extra, DurationType.NightEnd);
            }
            else
            {
                //владелец биты
                room.roomChat.Extra_PersonalMessage(player, extra, $"Вы ударили {extraTargetPlayer.GetColoredName()} " +
                    $"{bit} и лишили его права голоса");

                //цель биты
                room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extra, $"Вас ударили {bit}, отдохните");

                //остальные
                room.roomChat.Extra_PublicMessageExcludePlayers($"{extraTargetPlayer.GetColoredName()} ударили {bit}, " +
                    $"он лишен права голоса"
                    , room, extra, excludePlayers);

                AddExtraEffectToPlayer(extraTargetPlayer, extra, DurationType.DayEnd);
            }


            return true;
        }
        public bool UseExtra_Сonfession(BasePlayer player,Extra extra, Dictionary<byte, object> extraData)
        {
            //var randomPlayer = GetRandomActivePlayerInRoom(player);
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            var dice = room.dice.Next(100);

            var confessionSucces = false;

            if (dice < 50) confessionSucces = true;

            if (confessionSucces)
            {
                room.roomChat.Extra_PersonalMessage(player,extra, $"Вы исповедались {extraTargetPlayer.GetColoredName()}, " +
                    $"раскрыв свою роль");

                Action action = () => 
                {
                    player.GetRoom().roomChat.Extra_PublicMessage(extra, $"{extraTargetPlayer.GetColoredName()} рассказал, что " +
                        $"{player.GetColoredName()} раскрыл свою роль - {player.GetColoredRole()}");

                    RoleHelper.UnlockRole_PlayerToRoom(player);
                };

                RoleHelper.UnlockRole_PlayerToPlayer(player, extraTargetPlayer, player.playerRole.roleType);

                var actionId = extraTargetPlayer.AddShareAction(action);

                room.roomChat.Extra_SharePersonalMessage(extraTargetPlayer, extra, actionId,
                    $"{player.GetColoredName()} раскрыл Вам свою роль - {player.GetColoredRole()}");
            }
            else
            {
                room.roomChat.Extra_PersonalMessage(player, extra,
                    $"Не удалось использовать {ColorString.GetColoredExtra("Исповедь")} попробуйте еще раз");
            }

            return true;
        }
        public bool UseExtra_Eye(BasePlayer player,Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            var dice = room.dice.Next(100);

            var eye = ColorString.GetColoredExtra("Око");

            if (dice < 50)
            {
                var targetRole = ColorString.GetColoredFromRole(extraTargetPlayer.playerRole.roleType);

                Action action = () =>
                {
                    player.GetRoom().roomChat.Extra_PublicMessage(extra, $"{player.GetColoredName()} рассказал," +
                    $" что {extraTargetPlayer.GetColoredName()}" +
                    $" играет за {targetRole}");

                    RoleHelper.UnlockRole_PlayerToRoom(extraTargetPlayer);
                };

                RoleHelper.UnlockRole_PlayerToPlayer(extraTargetPlayer, player, extraTargetPlayer.playerRole.roleType);

                var actionId = player.AddShareAction(action);

                room.roomChat.Extra_SharePersonalMessage(player, extra, actionId, 
                    $"{eye} сообщает, что {extraTargetPlayer.GetColoredName()} играет за {targetRole}");
            }
            else
            {
                room.roomChat.Extra_PersonalMessage(player, extra,
                    $"Не удалось использовать {ColorString.GetColoredExtra("Око")} попробуйте еще раз");
            }

            return true;
        }
      
        public bool UseExtra_GoldenFleece(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_WireTape(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData); 

            room.roomChat.Extra_PersonalMessage(player, extra,
                $"Вы использовали {ColorString.GetColoredExtra("Прослушку")} на {extraTargetPlayer.GetColoredName()}");

            AddExtraEffectToPlayer(extraTargetPlayer, extra);

            return true;
        }
        public bool UseExtra_RadioSet(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Iron(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);          

            room.roomChat.Extra_PersonalMessage(player,extra, $"Вы применили {ColorString.GetColoredExtra("Утюг")} " +
                $"на {extraTargetPlayer.GetColoredName()}, ждите от него 3 смс");

            AddEventExtraEffectToPlayer(extraTargetPlayer, extra, PlayerEventType.ChatMessage, 3);

            return true;
        }
        public bool UseExtra_ActiveRole(BasePlayer player)
        {
            var extraString = ColorString.GetColoredString("активная роль", ColorId.Extra);
            room.roomChat.PersonalMessage(player, $"у вас включилась {extraString}");
            return true;
        }
        public bool UseExtra_Cuff(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            //AddExtraEffectToPlayer(player, extra);

            if(player.targetPlayer == null)
            {
                Logger.Log.Debug($"cant use cuff. player dont vote");
                return false;
            }

            player.AddVoteCount(1);

            player.GetRoom().roomLogic.AddDayVoteForPlayer(player.targetPlayer);

            room.roomChat.Extra_PersonalMessage(player, extra, $"Вы удвоили свой голос, " +
                $"благодаря {ColorString.GetColoredExtra("Наручникам")}");

            room.roomChat.Extra_PersonalMessage(
                player.targetPlayer, extra,
                   $"Вы в {ColorString.GetColoredExtra("Наручниках")}, " +
                   $"поэтому получаете +1 голос");

            room.roomChat.Extra_PublicMessage(extra, $"{player.GetColoredName()} удвоил голос, " +
                $"благодаря {ColorString.GetColoredExtra("Наручникам")}");

            return true;
        }
        public bool UseExtra_AirPlane(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_MineDetector(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Grenade(BasePlayer player, Extra extra)
        {
            //запрещает пользоваться гранатой живому игроку
            if (player.isLive())
            {
                Logger.Log.Debug($"player {player.playerName} cant use grenade. because player not dead");
                return false;
            }

            var randomPlayer = GetRandomActivePlayerInRoom(player);

            room.roomChat.Extra_PersonalMessage(player, extra, $"Вы убили {randomPlayer.GetColoredName()} - " +
                $"{randomPlayer.GetColoredRole()} {ColorString.GetColoredExtra("Гранатой")}");

            room.roomChat.Extra_PersonalMessage(randomPlayer, extra, $"{randomPlayer.GetColoredName()} - " +
              $"{randomPlayer.GetColoredRole()} был убит {ColorString.GetColoredExtra("Гранатой")}. Вас убила " +
              $"{ColorString.GetColoredExtra("Граната")},увы!");

            var excludedPlayers = new BasePlayer[] { player, randomPlayer };
            var message = $"{randomPlayer.GetColoredName()} - {randomPlayer.GetColoredRole()} " +
                $"был убит {ColorString.GetColoredExtra("Гранатой")}";
            room.roomChat.Extra_PublicMessageExcludePlayers(message, room, extra, excludedPlayers);

            room.roomLogic.SendPlayerToMorgue(randomPlayer);

            return true;
        }
        public bool UseExtra_Lightning(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Explosion(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraId = (string)extraData[(byte)Params.ExtraId];

            var extraTargetPlayer = GetExtraTarget(player, extraData);

            extraTargetPlayer.playerRole.roleEffects.AddExtraEffect(extra);

            room.roomChat.Extra_PersonalMessage(player, extra, $"Вы подложили " +
                       $"{ColorString.GetColoredExtra("Взрывчатку")} {extraTargetPlayer.GetColoredName()}");

            return true;
        }
        public bool UseExtra_Mine(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Cape(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Dublicat(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Gun(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);           

            var targetPlayerIsDead = true;
            var excludePlayers = new BasePlayer[] { extraTargetPlayer };

            var extraTargetArmor = extraTargetPlayer.FindExtraInSlots(ExtraEffect.Armor);
            if (extraTargetArmor != null)
            {
                extraTargetArmor.DecreaseCount();

                var surviveDice = room.dice.Next(100);

                if (surviveDice < 50) 
                { 
                    targetPlayerIsDead = false;

                    //сообщение владельцу брони
                    room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extraTargetArmor, $"Ваша " +
                        $"{ColorString.GetColoredExtra("Броня")} спасла Вам жизнь");

                    //сообщение владельцу ружья
                    room.roomChat.Extra_PersonalMessage(player, extraTargetArmor, 
                        $"{ColorString.GetColoredExtra("Броня")} " +
                        $"спасла {extraTargetPlayer.GetColoredName()} от Вашего " +
                        $"{ColorString.GetColoredExtra("Ружья")}");
                }
                else
                {
                    //сообщение владельцу брони
                    room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extraTargetArmor, 
                        $"{ColorString.GetColoredExtra("Броня")} " +
                        $"не спасла Вам жизнь, увы!");

                    //сообщение владельцу ружья
                    room.roomChat.Extra_PersonalMessage(player, extra, $"Вы убили {extraTargetPlayer.GetColoredName()} - " +
                        $"{extraTargetPlayer.GetColoredRole()} из " +
                        $"{ColorString.GetColoredExtra("Ружья")}");
                }                
            }      

            if (targetPlayerIsDead)
            {              
                room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extra, $"{extraTargetPlayer.GetColoredName()} - " +
                    $"{extraTargetPlayer.GetColoredRole()} убит из " +
                    $"{ColorString.GetColoredExtra("Ружья")}. Вы убиты, увы!");

                var publicMessage = $"{extraTargetPlayer.GetColoredName()} - " +
                        $"{extraTargetPlayer.GetColoredRole()} убит из " +
                        $"{ColorString.GetColoredExtra("Ружья")}";
                room.roomChat.Extra_PublicMessageExcludePlayers(publicMessage, room, extra, excludePlayers);

                Logger.Log.Debug($"gun excluded players => {excludePlayers.Length}");

                room.roomLogic.SendPlayerToMorgue(extraTargetPlayer);
            }

            //проверка на партизина
            if (player.playerRole.roleType == RoleType.Guerilla)
            {
                var guerillaRole = (Guerilla)player.playerRole;

                if (guerillaRole.Check_GuerillaThatsAll())
                {
                    var randomTarget = RoomHelper.FindNearPlayers(room, player, extraTargetPlayer, 1, false, false);

                    if (randomTarget.Count > 0)
                    {
                        room.roomChat.Skill_PersonalMessage(player, guerillaRole.skill_GuerillaThatsAll,
                            $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                            $"убит из {ColorString.GetColoredExtra("Ружья")} навыком " +
                            $"{ColorString.GetColoredSkill("Ну всё...")}. " +
                            $"Ваш навык сработал");

                        room.roomChat.Skill_PersonalMessage(randomTarget[0], guerillaRole.skill_GuerillaThatsAll,
                           $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                            $"убит из {ColorString.GetColoredExtra("Ружья")} навыком " +
                            $"{ColorString.GetColoredSkill("Ну всё...")}. " +
                            $"Вас убили, увы!");

                        var excludedPlayers = new BasePlayer[] { player, randomTarget[0] };
                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                            $"убит из {ColorString.GetColoredExtra("Ружья")} навыком " +
                            $"{ColorString.GetColoredSkill("Ну всё...")}",
                            room, guerillaRole.skill_GuerillaThatsAll, excludedPlayers);

                        room.roomLogic.SendPlayerToMorgue(randomTarget[0]);
                    }
                }
            }

            return true;
        }
        public bool UseExtra_BraceKnukles(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_MagicCube(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            if (player.playerRole.roleType == RoleType.Commissar)
            {
                var comissarRole = (Commissar)player.playerRole;

               if(comissarRole.Check_CommissarRage())
               {
                    room.roomLogic.SendPlayerToMorgue(extraTargetPlayer);

                    room.roomChat.Skill_PersonalMessage(player, comissarRole.skill_CommissarRage,
                        $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} " +
                        $"убит {ColorString.GetColoredExtra("Магическим кубом")} " +
                        $"навыком {ColorString.GetColoredSkill("Злость")}. " +
                        $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(extraTargetPlayer, comissarRole.skill_CommissarRage,
                        $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} " +
                        $"убит {ColorString.GetColoredExtra("Магическим кубом")} " +
                        $"навыком {ColorString.GetColoredSkill("Злость")}. " +
                        $"Вы убиты злым {ColorString.GetColoredRole("Комиссаром")}, увы!");

                    var excludedPlayers = new BasePlayer[] { player, extraTargetPlayer };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{extraTargetPlayer.GetColoredName()} - {extraTargetPlayer.GetColoredRole()} " +
                        $"убит {ColorString.GetColoredExtra("Магическим кубом")} " +
                        $"навыком {ColorString.GetColoredSkill("Злость")}",
                        room, comissarRole.skill_CommissarRage, excludedPlayers);

                    return true;
               }
            }

            //для владельца куба
            room.roomChat.Extra_PersonalMessage(player,extra,$"Вы использовали " +
                $"{ColorString.GetColoredExtra("Магический куб")} и заставили всех проголосовать против " +
                $"{extraTargetPlayer.GetColoredName()}");

            //для жертвы куба
            room.roomChat.Extra_PersonalMessage(extraTargetPlayer, extra, $"В {extraTargetPlayer.GetColoredName()} " +
                $"использовали {ColorString.GetColoredExtra("Магический куб")}, его судьба в Ваших руках! " +
                $"Вы под действием магии, удачи");

            //для остальных
            var message = $"В {extraTargetPlayer.GetColoredName()} " +
                $"использовали {ColorString.GetColoredExtra("Магический куб")}, его судьба в Ваших руках!";
            room.roomChat.Extra_PublicMessage(extra,message);

            foreach (var p in room.GetLivePlayers().Values)
            {                
                if (p.targetPlayer == null)
                {
                    room.roomLogic.SelectPlayer(p, extraData);

                    //AddDayVoteForPlayer

                    AddExtraEffectToPlayer(p, extra, DurationType.NightStart);
                }
            }

            return true;
        }
        public bool UseExtra_FoolsCap(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_PopCorn(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Keys(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Cola(BasePlayer player, Extra extra)
        {
            var colaString = ColorString.GetColoredString("колу", ColorId.Extra);

            foreach (var p in room.GetLivePlayers().Values)
            {
                if(p.team != player.team)
                {
                    var targetExtraPlayer = GetExtraTarget(player, p.playerId);

                    AddExtraEffectToPlayer(targetExtraPlayer, extra);

                    Logger.Log.Debug($"aply cola to {p.playerName}");
                    
                    room.roomChat.PersonalMessage(p, $"вам подарили {colaString}");
                }            

                if (player.team.teamType == TeamType.Neutral && p != player)
                {
                    var targetExtraPlayer = GetExtraTarget(player, p.playerId);

                    AddExtraEffectToPlayer(targetExtraPlayer, extra);

                    Logger.Log.Debug($"aply cola to {p.playerName}");

                    room.roomChat.PersonalMessage(p, $"вам подарили {colaString}");
                }
            }

            return true;
        }
        public bool UseExtra_Mirror(BasePlayer player, Extra extra)
        {
            //По идее не должно быть оповещений

            AddExtraEffectToPlayer(player, extra);

            return true;
        }
        public bool UseExtra_Dart(BasePlayer player, Extra extra)
        {
            var randomPlayer = GetRandomActivePlayerInRoom(player);

            var playerString = ColorString.GetColoredString(randomPlayer.playerName, ColorId.Player);
            var dartString = ColorString.GetColoredString("дротик", ColorId.Extra);

            AddDelayedExtraEffectToPlayer(randomPlayer, extra);

            //предыдущий вариант
            //room.roomChat.PersonalMessage(player, $"вы плпали дротиком в {randomPlayer.playerName}");

            room.roomChat.PublicMessage($"в {playerString} попал {dartString}");

            return true;
        }

        public bool UseExtra_Kinder(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Fizz(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            room.roomLogic.SelectPlayer(extraTargetPlayer, extraTargetPlayer.playerId);

            //AddExtraEffectToPlayer(extraTargetPlayer, extra);

            //добавить логику голосования игрока за себя.
            //лишить возможности голосовать

            var playerString = ColorString.GetColoredString(extraTargetPlayer.playerName, ColorId.Player);
            var extraString = ColorString.GetColoredString("шипучку", ColorId.Extra);
            
            room.roomChat.PublicMessage($"{playerString} выпил {extraString}");

            return true;
        }
        public bool UseExtra_CosmoRait(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Hidrogen(BasePlayer player, Extra extra)
        {
            var extraString = ColorString.GetColoredString("Азот", ColorId.Extra);

            foreach (var p in room.GetLivePlayers().Values)
            {
                var azotApplyed = false;

                if (p.team != player.team)
                {
                    var targetExtraPlayer = GetExtraTarget(player, p.playerId);

                    AddExtraEffectToPlayer(targetExtraPlayer, extra);

                    Logger.Log.Debug($"aply azot to {p.playerName}");

                    azotApplyed = true;
                }

                if (player.team.teamType == TeamType.Neutral && p != player)
                {
                    var targetExtraPlayer = GetExtraTarget(player, p.playerId);

                    AddExtraEffectToPlayer(targetExtraPlayer, extra);

                    Logger.Log.Debug($"aply cola to {p.playerName}");

                    azotApplyed = true;
                }

                if (azotApplyed)
                {
                    room.roomChat.PersonalMessage(p, $"против вас была использована экстра {extraString}");
                }
            }

            return true;
        }
        public bool UseExtra_Star(BasePlayer player)
        {
            return true;
        }
        public bool UseExtra_Asteroid(BasePlayer player)
        {
            var extraString = ColorString.GetColoredString("астеройд", ColorId.Extra);
            room.roomChat.PublicMessage($"был использован {extraString}");

            var activePlayers = room.GetLivePlayers();

            //удаляем из активных игроков владельца экстры
            activePlayers.Remove(player.playerId);

            var deadList = new List<BasePlayer>();

            //получаем рандомное кол-во игроков
            if (activePlayers.Count < 2)
            {
                Logger.Log.Debug($"cant use asteriod. not enought players in room");
                return false;
            }

            //кидаем кубик на случайное кол-во игроков. минимум 1
            var randomPlayerCount = room.dice.Next(1, activePlayers.Count + 1);

            Logger.Log.Debug($"active players {activePlayers.Count} rnd => {randomPlayerCount} ");

            //проверяем умер ли владелец экстры с шансом 1%
            var ownerDie = room.dice.NextDouble();
            Logger.Log.Debug($"owner dead {ownerDie < 0.01} => {ownerDie} ");
            if (ownerDie < 0.01)
            {
                deadList.Add(player);
            }

            //убиваем случайных игроков
            for (int i = 0; i < randomPlayerCount; i++)
            {
                var randomPlayerIndex = room.dice.Next(activePlayers.Count);

                var randomPlayerId = activePlayers.ElementAt(randomPlayerIndex).Key;

                var targetExtraPlayer = GetExtraTarget(player, randomPlayerId);

                activePlayers.Remove(randomPlayerId);

                room.roomLogic.SendPlayerToMorgue(targetExtraPlayer);
            }

            return true;
        }

        public bool UseExtra_Comet(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            //По идее оповещение не при использовании, а при взрыве

            var extraTargetPlayer = GetExtraTarget(player, extraData);

            Logger.Log.Debug($"{player.playerName} use {extra.extraId} to {extraTargetPlayer.playerName}");

            AddEventExtraEffectToPlayer(extraTargetPlayer, extra, PlayerEventType.Action, 1);             

            return true;
        }

        private bool UseExtra_Orbit(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Spacesuit(BasePlayer player, Extra extra)
        {
            //По идее оповещение не при использовании, а при голосовании

            AddExtraEffectToPlayer(player, extra);

            return true;
        }

        private bool UseExtra_SpiderPaw(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Trap(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Web(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            var extraTargetPlayer = GetExtraTarget(player, extraData);

            var playerString = ColorString.GetColoredString(extraTargetPlayer.playerName, ColorId.Player);
            var extraString = ColorString.GetColoredString("сеть", ColorId.Extra);
            room.roomChat.PersonalMessage(player, $"вы накинули {extraString} на {playerString}");

            //Необходимо добавить оповещение при попытке голосования

            AddExtraEffectToPlayer(extraTargetPlayer, extra);

            return true;
        }

        private bool UseExtra_Cocoon(BasePlayer player, Extra extra)
        {
            AddExtraEffectToPlayer(player, extra);

            var extraString = ColorString.GetColoredString("кокон", ColorId.Extra);
            room.roomChat.PersonalMessage(player, $"вы использовали {extraString}");

            //Необходимо добавить оповещение при проверке кокона

            return true;
        }

        private bool UseExtra_Scarab(BasePlayer player, Extra extra)
        {
            var activePlayers = room.GetLivePlayers();

            //удаляем из активных игроков владельца экстры
            activePlayers.Remove(player.playerId);

            if (activePlayers.Count == 0)
            {
                Logger.Log.Debug($"no target for scarab");
                return false;
            }

            var randomPlayerCount = room.dice.Next(1, activePlayers.Count + 1);

            //убиваем случайных игроков
            //Проверить логику экстры
            for (int i = 0; i < randomPlayerCount; i++)
            {
                var randomPlayerIndex = room.dice.Next(activePlayers.Count);

                var randomPlayerId = activePlayers.ElementAt(randomPlayerIndex).Key;

                var randomPlayer = GetExtraTarget(player, randomPlayerId);

                activePlayers.Remove(randomPlayer.playerId);

                AddDelayedExtraEffectToPlayer(randomPlayer, extra);
            }

            return true;
        }

        private bool UseExtra_Toxin(BasePlayer player, Extra extra)
        {
            //По идее оповещение во время смерти

            var activePlayers = room.GetLivePlayers();

            //удаляем из активных игроков владельца экстры
            activePlayers.Remove(player.playerId);

            var durationType = DurationType.Null;

            if (room.roomPhases.gamePhase == GamePhase.Day)
            {
                durationType = DurationType.DayStart;
            }

            if (room.roomPhases.gamePhase == GamePhase.Night || room.roomPhases.gamePhase == GamePhase.FirstNight)
            {
                durationType = DurationType.NightStart;
            }

            foreach (var p in activePlayers.Values)
            {
                var targetExtraPlayer = GetExtraTarget(player, p.playerId);

                AddDelayedExtraEffectToPlayer(targetExtraPlayer, extra, durationType);
            }

            return true;
        }

        private bool UseExtra_Hook(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Skull(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_PirateBomb(BasePlayer player)
        {
            //По идее оповещение во время смерти

            var playerCount = 5;
            if (room.GetLivePlayers().Count < 5) playerCount = room.GetLivePlayers().Count;

            var activePlayers = room.GetLivePlayers();

            //убиваем случайных игроков
            for (int i = 0; i < playerCount; i++)
            {
                var randomPlayerIndex = room.dice.Next(activePlayers.Count);

                var randomPlayerId = activePlayers.ElementAt(randomPlayerIndex).Key;

                var randomPlayer = GetExtraTarget(player, randomPlayerId);

                activePlayers.Remove(randomPlayer.playerId);

                room.roomLogic.SendPlayerToMorgue(randomPlayer);
            }

            return true;
        }

        private bool UseExtra_Saber(BasePlayer player)
        {
            //По идее оповещение во время смерти

            var activePlayers = room.GetLivePlayers();

            if (activePlayers.Count == 1)
            {
                Logger.Log.Debug("no target for saber");
                return false;
            }

            List<BasePlayer> playerlist = new List<BasePlayer>();

            foreach (var p in activePlayers.Values)
            {
                playerlist.Add(p);
            }

            var ownerIndex = playerlist.IndexOf(player);
            Logger.Log.Debug($"saber owner index => {ownerIndex}");

            var upIndex = ownerIndex;
            var downIndex = ownerIndex;

            BasePlayer upTarget = null;
            BasePlayer downTarget = null;

            for (int i = 0; i < playerlist.Count; i++)
            {
                if (i % 2 == 0)
                {
                    upIndex--;

                    if (upIndex < 0) upIndex = playerlist.Count - 1;

                    if (playerlist[upIndex] == player) continue;

                    if (upTarget == null) upTarget = GetExtraTarget(player, playerlist[upIndex].playerId);

                    //room.roomLogic.SendPlayerToMorgue(saberTarget);

                    continue;
                }
                else
                {
                    downIndex++;

                    if (downIndex == playerlist.Count) downIndex = 0;

                    if (playerlist[downIndex] == player) continue;

                    if (downTarget == null) downTarget = GetExtraTarget(player, playerlist[downIndex].playerId);

                    //room.roomLogic.SendPlayerToMorgue(saberTarget);

                    continue;
                }

                //Logger.Log.Debug($"{upIndex} / {downIndex}");
            }

            if (upTarget != null && downTarget != null)
            {
                var dice = room.dice.Next(2);

                Logger.Log.Debug($"saber dice => {dice}");

                if (dice == 0)
                {
                    room.roomLogic.SendPlayerToMorgue(upTarget);
                }
                else
                {
                    room.roomLogic.SendPlayerToMorgue(downTarget);
                }
            }
            else if (upTarget != null)
            {
                room.roomLogic.SendPlayerToMorgue(upTarget);
            }
            else
            {
                room.roomLogic.SendPlayerToMorgue(downTarget);
            }

            //if (ownerIndex == 0)
            //{
            //    var upTarget = playerlist[playerlist.Count - 1];
            //    var downTarget = playerlist[ownerIndex+1];
            //}
            //else if(ownerIndex > 0)
            //{

            //}

            return true;
        }

        private bool UseExtra_Tracker(BasePlayer player, Extra extra, Dictionary<byte, object> extraData)
        {
            //По идее оповещение во время хода

            var extraTarget = GetExtraTarget(player, extraData);

            AddExtraEffectToPlayer(extraTarget, extra);

            Logger.Log.Debug($"add ectra tracker to {extraTarget.playerName}");

            return true;
        }

        private bool UseExtra_Botle(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_PirateBox(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_NewYearToy(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_SnowBall(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Pumpkin(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Mask(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Lili(BasePlayer player)
        {
            throw new NotImplementedException();
        }

        private bool UseExtra_Shovel(BasePlayer player)
        {
            //По идее оповещение во время смерти

            var activePlayers = room.GetLivePlayers();

            if (activePlayers.Count == 1)
            {
                Logger.Log.Debug("no target for Shovel");
                return false;
            }

            List<BasePlayer> playerlist = new List<BasePlayer>();

            foreach (var p in activePlayers.Values)
            {
                playerlist.Add(p);
            }

            var ownerIndex = playerlist.IndexOf(player);
            Logger.Log.Debug($"Shovel owner index => {ownerIndex}");

            var upIndex = ownerIndex;
            var downIndex = ownerIndex;

            BasePlayer upTarget = null;
            BasePlayer downTarget = null;

            for (int i = 0; i < playerlist.Count; i++)
            {
                if (i % 2 == 0)
                {
                    upIndex--;

                    if (upIndex < 0) upIndex = playerlist.Count - 1;

                    if (playerlist[upIndex] == player) continue;

                    if (upTarget == null) upTarget = GetExtraTarget(player, playerlist[upIndex].playerId);

                    //room.roomLogic.SendPlayerToMorgue(saberTarget);

                    continue;
                }
                else
                {
                    downIndex++;

                    if (downIndex == playerlist.Count) downIndex = 0;

                    if (playerlist[downIndex] == player) continue;

                    if (downTarget == null) downTarget = GetExtraTarget(player, playerlist[downIndex].playerId);

                    //room.roomLogic.SendPlayerToMorgue(saberTarget);

                    continue;
                }

                //Logger.Log.Debug($"{upIndex} / {downIndex}");
            }

            if (upTarget != null && downTarget != null)
            {
                var dice = room.dice.Next(2);

                Logger.Log.Debug($"Shovel dice => {dice}");

                if (dice == 0)
                {
                    room.roomLogic.SendPlayerToMorgue(upTarget);
                }
                else
                {
                    room.roomLogic.SendPlayerToMorgue(downTarget);
                }
            }
            else if (upTarget != null)
            {
                room.roomLogic.SendPlayerToMorgue(upTarget);
            }
            else
            {
                room.roomLogic.SendPlayerToMorgue(downTarget);
            }

            return true;
        }

        #endregion

        #region Event Extras
        public void AddEventExtraEffectToPlayer(BasePlayer player, Extra extra, PlayerEventType playerEventType, int targetEventCount)
        {
            player.playerRole.roleEffects.AddExtraEffect(extra);

            var eventExtraHandler = new EventExtraHandler(player, extra, room, playerEventType, targetEventCount);

            player.playerRole.roleEffects.AddEventExtraHandler(eventExtraHandler);
        }
        public bool UseEventExtra(BasePlayer player, Extra extra)
        {
            var extraId = extra.extraId;

            var result = false;

            var extraEffect = Helper.GetEnumElement<ExtraEffect>(extraId);

            Logger.Log.Debug($"player {player.playerName} use event extra {extraEffect}");

            switch (extraEffect)
            {
                case ExtraEffect.Iron: { result = UseEventExtra_Iron(player,extra); } break;
                case ExtraEffect.Comet: { result = UseEventExtra_Comet(player); } break;

                default:
                    {
                        Logger.Log.Debug($"player {player.playerName} use BAD delayed extra {extraEffect}");
                    }
                    break;
            }

            return result;
        }       

        private bool UseEventExtra_Iron(BasePlayer player,Extra extra)
        {
            var targetRole = ColorString.GetColoredFromRole(player.playerRole.roleType);

            Action action = () =>
            {
                player.GetRoom().roomChat.Extra_PublicMessage(extra, $"{extra.owner.GetColoredName()} воспользовался " +
                    $"{ColorString.GetColoredExtra("Утюгом")} и узнал," +
                 $" что {player.GetColoredName()} играет за {targetRole}");

                RoleHelper.UnlockRole_PlayerToRoom(player);
            };

            RoleHelper.UnlockRole_PlayerToPlayer(player, extra.owner);

            var actionId = extra.owner.AddShareAction(action);

            room.roomChat.Extra_SharePersonalMessage(extra.owner,extra, actionId,
                $"Пытки {ColorString.GetColoredExtra("Утюгом")} дали результат, {player.GetColoredName()} " +
                $"играет за {targetRole}");

            return true;
        }
        private bool UseEventExtra_Comet(BasePlayer player)
        {
            room.roomLogic.SendPlayerToMorgue(player);

            return true;
        }

        #endregion

        #region Help Staff / reuse voids
        private BasePlayer GetRandomActivePlayerInRoom(BasePlayer player)
        {
            //список активных игроков
            var activePlayers = room.GetLivePlayers();
            //рандом из активных игроков
            var randomDice = room.dice.Next(activePlayers.Count);
            //рандомный игрок
            var randomPlayer = activePlayers.ElementAt(randomDice).Value;

            var mirrorExtra = randomPlayer.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Mirror);
            if (mirrorExtra != null)
            {
                return player;
            }

            return randomPlayer;
        }

        private BasePlayer GetExtraTarget(BasePlayer player, Dictionary<byte, object> extraData)
        {
            var extraTargetId = (long)extraData[(byte)Params.UserId];

            return GetExtraTarget(player, extraTargetId);
        }

        private BasePlayer GetExtraTarget(BasePlayer player, long id)
        {
            if (!room.GetLivePlayers().ContainsKey(id))
            {
                Logger.Log.Debug($"no target for extra {id}");

                return null;
            }

            var extraTargetPlayer = room.GetLivePlayers()[id];

            var mirrorExtra = extraTargetPlayer.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Mirror);
            if (mirrorExtra != null)
            {
                return player;
            }

            return extraTargetPlayer;
        }

        #endregion

        #region Delayed Extras

        public void AddDelayedExtraEffectToPlayer(BasePlayer player, Extra extra, DurationType durationType = DurationType.Null)
        {
            player.playerRole.roleEffects. AddExtraEffect(extra);

            var delayedExtraHandler = new DelayedExtraHandler(player, extra, room, durationType);

            player.playerRole.roleEffects.AddDelayedExtraHandler(delayedExtraHandler);
        }

        public bool UseDelayedExtra(BasePlayer player, Extra extra)
        {
            var extraId = extra.extraId;

            var result = false;

            var extraEffect = Helper.GetEnumElement<ExtraEffect>(extraId);

            Logger.Log.Debug($"player {player.playerName} use extra {extraEffect}");

            switch (extraEffect)
            {
                case ExtraEffect.Dart: { result = UseDelayedExtra_Dart(player); } break;
                case ExtraEffect.Toxin: { result = UseDelayedExtra_Toxin(player); } break;
                case ExtraEffect.Scarab: { result = UseDelayedExtra_Scarab(player, extra); } break;

                default:
                    {
                        Logger.Log.Debug($"player {player.playerName} use BAD delayed extra {extraEffect}");
                    }
                    break;
            }

            return result;
        }

        public bool UseDelayedExtra_Dart(BasePlayer player)
        {
            if (player.isLive())
            {
                room.roomLogic.SendPlayerToMorgue(player);
            }

            return true;
        }
        public bool UseDelayedExtra_Toxin(BasePlayer player)
        {
            if (player.isLive())
            {
                room.roomLogic.SendPlayerToMorgue(player);
            }

            return true;
        }
        public bool UseDelayedExtra_Scarab(BasePlayer player,Extra extra)
        {
            room.roomChat.PersonalMessage(extra.owner, $"скарабей узнал роль игрока {player.playerName} => {player.playerRole.roleType}");              

            return true;
        }

        #endregion

    }

}
