using ExitGames.Client.Photon.LoadBalancing;
using log4net.Appender;
using Mafia_Server;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Xml.Linq;
//using WebSocketSharp;

namespace Mafia_Server
{
    public class RoomLogic
    {
        private Room room;

        public RoomNightActionMessages nightActionMessages;
        public RoomLogic(Room room)
        {
            this.room = room;

            nightActionMessages = new RoomNightActionMessages(room);

            mafiaVisit = new MafiaVisit(room);
            sinnerVisit = new SinnerVisit(room);
            saintVisit = new SaintVisit(room);

            commissarVisit = new CommissarVisit(room);
            witnessVisit = new WitnessVisit(room);

            doctorVisit = new DoctorVisit(room);
            maniacVisit = new ManiacVisit(room);
            werewolfVisit = new WerewolfVisit(room);
            guerillaVisit = new GuerillaVisit(room);
            mafiaBossVisit = new MafiaBossVisit(room);
        }

        public void FirstNightWerewolfRecruitment()
        {
            var werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);

            if (werewolf == null) return;

            var werewolfRole = (Werewolf)werewolf.playerRole;

            //кидаем кубик на умение "гром" оборотня            
            var thunderSuccess = werewolfRole.Check_WerewolfThunder();

            //Лишняя строка
            if (thunderSuccess)
            {
                room.roomChat.Skill_PersonalMessage(werewolf, werewolfRole.skill_WerewolfThunder,
                       $"{ColorString.GetColoredRole("Оборотень")} выбрал играть за себя. " +
                       $"Сработал {ColorString.GetColoredSkill("Гром")}. " +
                       $"Вы теперь убийца - одиночка!");

                var excludedPlayers = new BasePlayer[] { werewolf };
                room.roomChat.Skill_PublicMessageExcludePlayers(
                    $"{ColorString.GetColoredRole("Оборотень")} выбрал играть за себя. " +
                       $"Сработал {ColorString.GetColoredSkill("Гром")}",
                    room, werewolfRole.skill_WerewolfThunder, excludedPlayers);

                werewolf.playerRole.BlockVisit(true);
                return;
            }

            var mafiaBoss = RoomHelper.FindPlayerByRole(RoleType.MafiaBoss, room);

            bool mafiaBossSuccess = false;

            if (!thunderSuccess && mafiaBoss != null)
            {
                var mafiaBossRole = (MafiaBoss)mafiaBoss.playerRole;

                //кидаем кубик на умение "перехват" босса мафии                    
                mafiaBossSuccess = mafiaBossRole.Check_MafiaBossInterception();

                if (mafiaBossSuccess)
                {
                    room.roomChat.Skill_PersonalMessage(mafiaBoss, mafiaBossRole.skill_MafiaBossInterception,
                        $"Сработал навык {ColorString.GetColoredSkill("Перехват")}, оборотень играет за Мафию." +
                        $"Ваш навык сработал!");

                    room.roomChat.Skill_PersonalMessage(werewolf, mafiaBossRole.skill_MafiaBossInterception,
                        $"Сработал навык {ColorString.GetColoredSkill("Перехват")}, оборотень играет за Мафию." +
                        $"Вас перехватили, теперь вы играете за команду Мафии");

                    var excludedPlayers = new BasePlayer[] { mafiaBoss, werewolf };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"Сработал навык {ColorString.GetColoredSkill("Перехват")}, оборотень играет за Мафию",
                        room, mafiaBossRole.skill_MafiaBossInterception, excludedPlayers);

                    //меняем команду
                    RoomHelper.AddPlayerToTeam(room,werewolf, TeamType.Bad);

                    //отправляем ивент о смене команды
                    werewolfRole.Event_chnageTeam();

                    RoleHelper.UnlockRole_GroupToPlayer(mafiaBoss.team.GetPlayers(), werewolf);

                    RoleHelper.UnlockRole_PlayerToGroup(mafiaBoss.team.GetPlayers(), werewolf);

                    werewolf.playerRole.BlockVisit(true);
                    return;
                }
            }

            if (!thunderSuccess && !mafiaBossSuccess)
            {
                room.roomPhases.SendVisitsToPlayer(werewolf);
            }
        }

        public void CheckManiacUnlimit()
        {
            var maniac = RoomHelper.FindPlayerByRole(RoleType.Maniac, room);

            if (maniac == null) return;

            var maniacRole = (Maniac)maniac.playerRole;

            if (maniacRole.Check_ManiacWorse())
            {
                //do some action
                foreach (var extra in maniac.inGameSlots.Values)
                {
                    if (extra.extraType == ExtraType.Clan)
                    {
                        extra.SetUnlimitCurrentCount();
                    }
                }

                room.roomChat.Skill_PersonalMessage(maniac, maniacRole.skill_ManiacWorse,
                    $"{ColorString.GetColoredRole("Маньяк")} " +
                    $"на безлимите, {ColorString.GetColoredSkill("Чем я хуже?")} " +
                    $"Вы можете использовать клановые экстры без ограничений!");

                var excludedPlayers = new BasePlayer[] { maniac };

                room.roomChat.Skill_PublicMessageExcludePlayers(
                    $"{ColorString.GetColoredRole("Маньяк")} " +
                    $"на безлимите, {ColorString.GetColoredSkill("Чем я хуже?")}",
                    room, maniacRole.skill_ManiacWorse, excludedPlayers);
            }
        }

        public void CheckWerewolfUnlimit()
        {
            var werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);

            if (werewolf == null) return;

            var werewolfRole = (Werewolf)werewolf.playerRole;

            if (werewolfRole.Check_WerewolfUnlimit())
            {
                //do some action
                foreach (var extra in werewolf.inGameSlots.Values)
                {
                    if (extra.extraType == ExtraType.Clan)
                    {
                        extra.SetUnlimitCurrentCount();
                    }
                }

                room.roomChat.Skill_PersonalMessage(werewolf, werewolfRole.skill_WerewolfUnlimit,
                    $"{ColorString.GetColoredRole("Оборотень")} " +
                    $"на {ColorString.GetColoredSkill("Безлимите")}. " +
                    $"Вы можете использовать клановые экстры без ограничений!");

                var excludedPlayers = new BasePlayer[] { werewolf };

                room.roomChat.Skill_PublicMessageExcludePlayers(
                    $"{ColorString.GetColoredRole("Оборотень")} " +
                    $"на {ColorString.GetColoredSkill("Безлимите")}",
                    room, werewolfRole.skill_WerewolfUnlimit, excludedPlayers);
            }
        }

        internal void ResetVotes()
        {
            room.roomLogic.jailPlayer = null;

            dayVotes = new Dictionary<BasePlayer, int>();

            foreach(var p in room.players.Values)
            {
                p.ResetPlayerVoteCount();
            }
        }

        public void CheckStartGameExtras()
        {
            //активация рации
            foreach (var p in room.players.Values)
            {
                var radioSetExtra = p.FindExtraInSlots(ExtraEffect.RadioSet);

                if (radioSetExtra != null)
                {
                    room.extraHelper.AddExtraEffectToPlayer(p, radioSetExtra);
                    radioSetExtra.DecreaseCount();

                    room.roomChat.Extra_PersonalMessage (
                        p, 
                        radioSetExtra, 
                        $"Ваша {ColorString.GetColoredExtra("Рация")} активна");
                }
            }
        }

        public bool CheckWerewolfFirstNightAction()
        {
            var werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);

            if (werewolf == null) { return false; }

            if(werewolf.playerRole.visitBlocked) 
            {
                werewolf.playerRole.BlockVisit(false);
                return false; 
            }

            if(werewolf.targetPlayer == null)
            {
                room.roomChat.PersonalMessage(werewolf, "Ночью Вы никого не посетили, поэтому играете за себя");
                room.roomChat.PublicMessage("Оборотень выходит на охоту в одиночку");
                return false;
            }

            if (werewolf.targetPlayer.playerRole.CheckResistExtras(werewolf)) return false;

            //зеркало

            var werewolfRole = (Werewolf)werewolf.playerRole;

            switch (werewolf.targetPlayer.team.teamType)
            {
                case TeamType.Good:
                    {
                        //Заполнение ачивки оборотня
                        room.roomEndGame.IncreaseAchieveProgress(werewolf, AchieveId.Werewolf_GoodSide);
                        //сообщение
                        room.roomChat.PersonalMessage(werewolf, "Этой ночью Вы выбрали мирного игрока и теперь играете за команду Мирных");
                        room.roomChat.PublicMessage("Оборотень сделал свой выбор в пользу команды Мирных");

                        RoomHelper.AddPlayerToTeam(room, werewolf, TeamType.Good);

                        //отправляем ивент о смене команды
                        werewolfRole.Event_chnageTeam();
                    }
                    break;
                case TeamType.Bad:
                    {
                        //Заполнение ачивки оборотня
                        room.roomEndGame.IncreaseAchieveProgress(werewolf, AchieveId.Werewolf_EvilSide);
                        //сообщение
                        room.roomChat.PersonalMessage(werewolf, "Этой ночью Вы выбрали Мафиози и теперь играете за команду Мафиози");
                        room.roomChat.PublicMessage("Оборотень сделал свой выбор в пользу команды Мафиози");

                        RoomHelper.AddPlayerToTeam(room,werewolf, TeamType.Bad);

                        //отправляем ивент о смене команды
                        werewolfRole.Event_chnageTeam();
                    }
                    break;
                case TeamType.Neutral:
                case TeamType.Pirate:
                    {
                        room.roomChat.PersonalMessage(werewolf, "Ночью Вы не встретили ни мирных, ни мафиози, поэтому играете за себя");
                        room.roomChat.PublicMessage("Оборотень выходит на охоту в одиночку");
                    }
                    break;
            }

            return room.roomEndGame.CheckEndGame2();
        }

        public IDisposable gameTimer;

        //public Dictionary<long, BasePlayer> activePlayers { get; private set; } = new Dictionary<long, BasePlayer>();

        public void SelectPlayer(BasePlayer player, Dictionary<byte, object> parameters)
        {
            var playerId = (long)parameters[(byte)Params.UserId];

            SelectPlayer(player, playerId);
        }

        public void SelectPlayer(BasePlayer player, long playerId)
        {
            //мертвые и заключенные игроки не могу голосовать
            if (!player.isLive())
            {
                Logger.Log.Debug($"{player.playerName} cant select. is dead.");
                return;
            }

            //если игрок уже голосовал/выбирал
            if (player.targetPlayer != null)
            {
                Logger.Log.Debug($"{player.playerName} cant select. already select.");
                return;
            }

            //проверяем, что игрок выбирает/голосует против живого игрока
            if (!room.GetLivePlayers().ContainsKey(playerId))
            {
                Logger.Log.Debug($"player try kill/vote not active player");
                return;
            }

            switch (room.roomPhases.gamePhase)
            {
                case GamePhase.FirstNight: { FirstNightSelect(player, playerId); } break;

                case GamePhase.Day: { DaySelect(player, playerId); } break;

                case GamePhase.Night: { NightSelect(player, playerId); } break;
            }

            CheckAllPlayersAction();
        }

        /// <summary>
        /// проверка, что все игроки сделали выбор
        /// </summary>
        public void CheckAllPlayersAction()
        {
            var allPlayersAction = true;

            foreach (var p in room.GetLivePlayers().Values)
            {
                if (p.playerType == PlayerType.Player && p.targetPlayer == null )
                {
                    if (p.playerRole.roleType == RoleType.Citizen && room.roomPhases.gamePhase == GamePhase.Night) 
                    {
                        continue;
                    }

                    allPlayersAction = false;
                    break;
                }
            }

            if (allPlayersAction)
            {
                room.roomPhases.SwitchGamePhase();
            }
        }

        private void FirstNightSelect(BasePlayer player, long playerId)
        {
            //в первую ночь может "ходить" только оборотень
            if (player.playerRole.roleType != RoleType.Werewolf) return;

            player.targetPlayer = GetTargetPlayerById(playerId);
        }

        public Dictionary<BasePlayer, int> dayVotes { get; private set; } = new Dictionary<BasePlayer, int>();
        private void DaySelect(BasePlayer player, long playerId)
        {
            if (!player.playerRole.CanVote())
            {
                Logger.Log.Debug($"{player.playerName} cant vote.  ");
                return;
            }

            player.targetPlayer = GetTargetPlayerById(playerId);
            player.targetPlayer = CheckTargetPlayerMirror(player);

            //проверяем мину 
            var mineExploded = CheckMineExtra(player, playerId);

            if (mineExploded) return;

            player.AddVoteCount(1);
            AddDayVoteForPlayer(player.targetPlayer);           

             var playerAction = $"{player.GetColoredName()} проголосовал против {player.targetPlayer.GetColoredName()}";
            room.roomChat.PublicMessage($"{playerAction}");
        }

        public void AddDayVoteForPlayer(BasePlayer player)
        {
            if (dayVotes.ContainsKey(player))
            {
                dayVotes[player]++;
            }
            else
            {
                dayVotes.Add(player, 1);
            }

            UpdatePlayerVoteCount(player);
        }

        /// <summary>
        /// удалить дневное голосование игрока
        /// </summary>
        /// <param name="player">игрок, чей голосо нужно удалить</param>
        public void RemoveDayVoteByPlayer(BasePlayer player)
        {            
            if (dayVotes.ContainsKey(player.targetPlayer))
            {
                dayVotes[player.targetPlayer] -= player.voteCount;               

                UpdatePlayerVoteCount(player.targetPlayer);

                if (dayVotes[player.targetPlayer] <= 0) dayVotes.Remove(player.targetPlayer);
            }
        }

        public void UpdatePlayerVoteCount(BasePlayer player)
        {
            EventData eventData = new EventData((byte)Events.SetPlayerVoteCount);
            eventData.Parameters = new Dictionary<byte, object>();
            eventData.Parameters.Add((byte)Params.UserId, player.playerId);
            eventData.Parameters.Add((byte)Params.Votes, dayVotes[player]);

            eventData.SendTo(room.clients, Options.sendParameters);
        }

        private void NightSelect(BasePlayer player, long playerId)
        {
            //граждане не могут ходить ночью
            if (player.playerRole.roleType == RoleType.Citizen) return;

            //оглушенные не могут ходить ночью
            if (!player.playerRole.CanVisit()) return;

            //ищем цель
            var targetPlayer = GetTargetPlayerById(playerId);

            //если цель найти не удалось, ничего не делаем
            if (targetPlayer == null)
            {
                Logger.Log.Debug($"failed to select player {playerId}");
                return;
            }           

            //проверяем цель на защиту дымом
            var smokeResist = CheckSmokeResist(player, targetPlayer);         

            //если у цели есть зеркало, подменяем цель собой
            if (targetPlayer.playerRole.GetMirror() != null)
            {
                targetPlayer = player;
            }

            //проверяем рацию
            CheckRadioSetExtra(player, targetPlayer);

            //проверяем маячок
            CheckTrackerExtra(player, targetPlayer);

            if (smokeResist) 
            {
                player.targetPlayer = null;
                return; 
            }

            //запоминаем цель
            player.targetPlayer = targetPlayer;

            //отправляем сообщение о ходе
            player.playerRole.SendVisitMessage(player.targetPlayer);

            //проверяем не угодил ли оборотень к гражданину
            if (player.playerRole.roleType == RoleType.Werewolf 
                && player. targetPlayer.playerRole.roleType == RoleType.Citizen)
            {
                var citizenRole = (Citizen)player.targetPlayer.playerRole;

                if (citizenRole.Check_CitizenWerewolf())
                {
                    //do some action 
                    SendPlayerToMorgue(player);

                  
            room.roomChat.Skill_PersonalMessage(player.targetPlayer, citizenRole.skill_CitizenWerewolf,
                      $"{player.GetColoredName()} - {player.GetColoredRole()} " +
                      $"убит навыком {ColorString.GetColoredSkill("Оборотень уходи")}. " +
                      $"Ваш навык спас Вам жизнь!");

            room.roomChat.Skill_PersonalMessage(player, citizenRole.skill_CitizenWerewolf,
               $"{player.GetColoredName()} - {player.GetColoredRole()} " +
                $"убит навыком {ColorString.GetColoredSkill("Оборотень уходи")}. Вы убиты, увы!");

            var excludedPlayers = new BasePlayer[] { player.targetPlayer, player };
            var message = $"{player.GetColoredName()} - {player.GetColoredRole()} " +
                $"убит навыком {ColorString.GetColoredSkill("Оборотень уходи")}";
            room.roomChat.Skill_PublicMessageExcludePlayers(message, room,
                citizenRole.skill_CitizenWerewolf, excludedPlayers);

                    player.targetPlayer = null;
                }
            }

            //проверяем не сработала ли у доктора клизма
            if (player.playerRole.roleType == RoleType.Doctor)
            {
                var doctorRole = (Doctor)player.playerRole;

                if (doctorRole.Check_DoctorClizma())
                {
                    var pacient = player.targetPlayer;

                    //do some action 
                    SendPlayerToMorgue(pacient);

                    doctorRole.isClizma = true;

                    room.roomChat.Skill_PersonalMessage(player, doctorRole.skill_DoctorClizma,
                        $"{pacient.GetColoredName()} - {pacient.GetColoredRole()} " +
                        $"убит навыком {ColorString.GetColoredSkill("Клизма на убой")}. " +
                        $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(pacient, doctorRole.skill_DoctorClizma,
                     $"{pacient.GetColoredName()} - {pacient.GetColoredRole()} " +
                        $"убит навыком {ColorString.GetColoredSkill("Клизма на убой")}. " +
                        $"Вы убиты, увы!");

                    var excludedPlayers = new BasePlayer[] { player, pacient };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{pacient.GetColoredName()} - {pacient.GetColoredRole()} " +
                        $"убит навыком {ColorString.GetColoredSkill("Клизма на убой")}",
                        room, doctorRole.skill_DoctorClizma, excludedPlayers);

                    player.targetPlayer = null;
                }
            }

            //проверяем не завербовал ли святой маньяка
            if (player.playerRole.roleType == RoleType.Saint
                && player.targetPlayer.playerRole.roleType == RoleType.Maniac)
            {
                var saintRole = (Saint)player.playerRole;

                var maniac = player.targetPlayer;

                if (saintRole.Check_SaintPower())
                {
                    RoomHelper.AddPlayerToTeam(room, maniac, TeamType.Good);

                    //меняем роль маньяка на святого
                    RoleHelper.ChangePlayerRole(maniac, RoleType.Saint);

                    //открываем роль нового святого комнате
                    RoleHelper.UnlockRole_PlayerToRoom(maniac);

                    //открываем новомусвятому остальных святых
                    var saints = RoomHelper.FindPlayersByRole(RoleType.Saint, room);
                    RoleHelper.UnlockRole_GroupToPlayer(saints, maniac);

                    room.roomChat.Skill_PersonalMessage
                        (player, saintRole.skill_SaintPower,
                        $"{maniac.GetColoredName()} - {ColorString.GetColoredRole("Маньяк")} " +
                        $"теперь играет за {ColorString.GetColoredRole("Святого")}, " +
                        $"благодаря навыку {ColorString.GetColoredSkill("Власть")}. " +
                        $"Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage
                        (maniac, saintRole.skill_SaintPower,
                        $"{maniac.GetColoredName()} - {ColorString.GetColoredRole("Маньяк")} " +
                        $"теперь играет за {ColorString.GetColoredRole("Святого")}, " +
                        $"благодаря навыку {ColorString.GetColoredSkill("Власть")}. " +
                        $"Вы теперь играете за {ColorString.GetColoredRole("Святого")} " +
                        $"на стороне Мирных");

                    var excludedPlayers = new BasePlayer[] { player, maniac };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{maniac.GetColoredName()} - {ColorString.GetColoredRole("Маньяк")} " +
                        $"теперь играет за {ColorString.GetColoredRole("Святого")}, " +
                        $"благодаря навыку {ColorString.GetColoredSkill("Власть")}",
                        room, saintRole.skill_SaintPower, excludedPlayers);

                    player.targetPlayer = null;
                }
            }

            //проверяем не пошел ли маньяк к гражданину
            if (player.playerRole.roleType == RoleType.Maniac &&
                player.targetPlayer.playerRole.roleType == RoleType.Citizen)
            {
                var citizenRole = (Citizen)player.targetPlayer.playerRole;

                if (citizenRole.Check_CitizenManiac())
                {
                    SkillHelper.AddSkillEffectToPlayer(player, citizenRole.skill_CitizenManiac, DurationType.NightStart, 1);

                    //гражданин
                    room.roomChat.Skill_PersonalMessage(player.targetPlayer, citizenRole.skill_CitizenManiac,
                        $"{player.GetColoredRole()} лишился хода и дневного голоса, " +
                        $"сработал навык {ColorString.GetColoredSkill("Маньяк стоять")}. " +
                        $"Ваш навык сработал!");

                    //маньяк
                    room.roomChat.Skill_PersonalMessage(player, citizenRole.skill_CitizenManiac,
                       $"{player.GetColoredRole()} лишился хода и дневного голоса, " +
                        $"сработал навык {ColorString.GetColoredSkill("Маньяк стоять")}. " +
                        $"Отдыхайте до следующей ночи");

                    //общее
                    var excludedPlayers = new BasePlayer[] { player.targetPlayer, player };
                    var message = $"{player.GetColoredRole()} лишился хода и дневного голоса, " +
                        $"сработал навык {ColorString.GetColoredSkill("Маньяк стоять")}";
                    room.roomChat.Skill_PublicMessageExcludePlayers(message, room,
                        citizenRole.skill_CitizenManiac, excludedPlayers);

                    player.targetPlayer = null;
                }
            }

            //ход мафиози - Резкий/дерзкий
            if (player.playerRole.roleType == RoleType.Mafia)
            {
                var mafiaRole = (Mafia)player.playerRole;

                if (mafiaRole.Check_MafiaBold())
                {
                    CheckTargetPlayerMirror(player);                  

                    room.roomChat.Skill_PersonalMessage(player, mafiaRole.skill_MafiaBold,
                        $"{ColorString.GetColoredRole("Мафиози")} убил {player.targetPlayer.GetColoredName()} - " +
                        $"{player.targetPlayer.GetColoredRole()} c помощью навыка " +
                        $"{ColorString.GetColoredSkill("Резкий/Дерзкий")}. Ваш навык сработал");

                    room.roomChat.Skill_PersonalMessage(player.targetPlayer, mafiaRole.skill_MafiaBold,
                        $"{ColorString.GetColoredRole("Мафиози")} убил {player.targetPlayer.GetColoredName()} - " +
                        $"{player.targetPlayer.GetColoredRole()} c помощью навыка " +
                        $"{ColorString.GetColoredSkill("Резкий/Дерзкий")}. Вы убиты, увы!");

                    var exludedPlayers = new BasePlayer[] { player, player.targetPlayer };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"{ColorString.GetColoredRole("Мафиози")} убил {player.targetPlayer.GetColoredName()} - " +
                        $"{player.targetPlayer.GetColoredRole()} c помощью навыка " +
                        $"{ColorString.GetColoredSkill("Резкий/Дерзкий")}",
                        room, mafiaRole.skill_MafiaBold, exludedPlayers);

                    room.roomLogic.SendPlayerToMorgue(player.targetPlayer);

                    if (mafiaRole.Check_MafiaKillX2())
                    {
                        var randomTarget = RoomHelper.FindNearPlayers(
                            room, player, player.targetPlayer, 1, true, true);

                        if (randomTarget.Count > 0)
                        {
                           

                           
         room.roomChat.Skill_PersonalMessage(player, mafiaRole.skill_MafiaKillX2,
                               $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
                               $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}. Ваш навык сработал");

         room.roomChat.Skill_PersonalMessage(randomTarget[0], mafiaRole.skill_MafiaKillX2,
             $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
             $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}. Вы убиты, увы!");

         exludedPlayers = new BasePlayer[] { player, randomTarget[0] };
         room.roomChat.Skill_PublicMessageExcludePlayers(
             $"{randomTarget[0].GetColoredName()} - {randomTarget[0].GetColoredRole()} " +
             $"убит навыком {ColorString.GetColoredSkill("X2 убийство")}",
             room, mafiaRole.skill_MafiaKillX2, exludedPlayers);

                            room.roomLogic.SendPlayerToMorgue(randomTarget[0]);
                        }
                    }

                    player.targetPlayer = null;
                }
            }
        }

        private bool CheckSmokeResist(BasePlayer guest, BasePlayer target)
        {
            //дым
            var smokeEffect = target.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Smoke);
            var smokeExtra = target.FindExtraInSlots(ExtraEffect.Smoke);

            if (smokeEffect == null)
            {
                if (smokeExtra != null)
                {
                    room.extraHelper.AddExtraEffectToPlayer(target, smokeExtra);
                    smokeExtra.DecreaseCount();

                    smokeEffect = smokeExtra;
                }
            }

            if (smokeEffect != null)
            {
                var maniacIsIdeal = false;

                Maniac maniacRole = null;

                if (guest.playerRole.roleType == RoleType.Maniac)
                {
                    maniacRole = (Maniac)guest.playerRole;

                    maniacIsIdeal = maniacRole.maniacIsIdeal;
                }

                if (maniacIsIdeal)
                {
                    maniacRole.idealUsed = true;

                    return false;
                }
                else
                {
                    //сообщение владельцу дыма
                    room.roomChat.Extra_PersonalMessage(target, smokeExtra, 
                        $"{ColorString.GetColoredExtra("Дым")} " +
                        $"помог Вам избежать визита " +
                        $"{ColorString.GetColoredFromRole(guest.playerRole.roleType)}",
                        ExtraEffect.Smoke);

                    //сообщение гостю
                    room.roomChat.Extra_PersonalMessage(guest, smokeExtra, 
                        $"У {target.GetColoredName()} " +
                        $"{ColorString.GetColoredExtra("Дым")}, бегите домой", 
                        ExtraEffect.Smoke);

                    var excludePlayers = new BasePlayer[] { guest, target };

                    var extraAction = $"{target.GetColoredName()} применил " +
                        $"{ColorString.GetColoredExtra("Дым")} и " +
                        $"{guest.GetColoredRole()} вернулся домой";

                    room.roomChat.Extra_PublicMessageExcludePlayers($"{extraAction}", room, smokeExtra, excludePlayers,
                        ExtraEffect.Smoke);

                    guest.ResetTarget();

                    return true;
                }
            }

            return false;
        }

        private void CheckRadioSetExtra(BasePlayer player, BasePlayer targetPlayer)
        {
            if (player.team.teamType == TeamType.Good) return;

            //если у игрока тоже есть рация, то ничего не делаем
            var badRadioSetExtra = player.playerRole.roleEffects.FindExtraEffect(ExtraEffect.RadioSet);

            foreach (var p in room.GetLivePlayers().Values)
            {
                if (p.team.teamType == TeamType.Good)
                {
                    var goodRadioSetExtra = p.playerRole.roleEffects.FindExtraEffect(ExtraEffect.RadioSet);
                    if (goodRadioSetExtra != null)
                    {
                        bool badRadioSetResist = false;

                        if (badRadioSetExtra != null)
                        {
                            if (goodRadioSetExtra.owner.playerRole.roleType == RoleType.Witness)
                            {
                                var witnessRole = (Witness)goodRadioSetExtra.owner.playerRole;

                                if (!witnessRole.Check_WitnessNoInterference())
                                {
                                    badRadioSetResist = true;
                                }
                            }
                            else
                            {
                                badRadioSetResist = true;
                            }
                        }

                        if (badRadioSetResist)
                        {
                            room.roomChat.Extra_PersonalMessage(p, goodRadioSetExtra,
                              $"{ColorString.GetColoredExtra("Рация")} не работает, проверьте сигнал");
                        }
                        else
                        {
                            room.roomChat.Extra_PersonalMessage(p, goodRadioSetExtra,
                                $"{ColorString.GetColoredExtra("Рация")} сообщает: {player.GetColoredRole()} " +
                                $"направляется к {targetPlayer.GetColoredName()}");
                        }
                    }
                }
            }
        }

        private void CheckTrackerExtra(BasePlayer player, BasePlayer targetPlayer)
        {          
            var trackerExtra = player.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Tracker);
            if (trackerExtra != null)
            {
                Action action = () =>
                {
                    player.GetRoom().roomChat.PublicMessage($"{trackerExtra.owner.GetColoredName()} подслушал," +
                        $" что {player.GetColoredName()}" +
                        $" ночью ходил к {targetPlayer.GetColoredName()}");

                    RoleHelper.UnlockRole_PlayerToRoom(player);
                    //player.room.roomLogic.UnlockPlayerRoleToOtherPlayer(player);
                };

                RoleHelper.UnlockRole_PlayerToPlayer(player, trackerExtra.owner, player.playerRole.roleType);

                var actionId = trackerExtra.owner.AddShareAction(action);

                room.roomChat.Extra_SharePersonalMessage(trackerExtra.owner, trackerExtra, actionId,
                    $"{player.GetColoredName()} ночью ходил к {targetPlayer.GetColoredName()}");
            }
        }

        private bool CheckMineExtra(BasePlayer player, long playerId)
        {
            var targetPlayer = room.GetLivePlayers()[playerId];

            var targetMineExtra = targetPlayer.FindExtraInSlots(ExtraEffect.Mine);

            if (targetMineExtra != null)
            {
                var selfMineDetector = player.FindExtraInSlots(ExtraEffect.MineDetector);

                if (selfMineDetector != null)
                {
                    room.roomChat.Extra_PersonalMessage(player, selfMineDetector, $"Вы не взорвались на " +
                        $"{ColorString.GetColoredExtra("Мине")} {targetPlayer.GetColoredName()}, благодаря " +
                        $"{ColorString.GetColoredExtra("Миноискателю")}");

                    selfMineDetector.DecreaseCount();

                    return false;
                }
                else
                {
                    room.roomChat.Extra_PersonalMessage(targetMineExtra.owner, targetMineExtra, $"{player.GetColoredName()} " +
                        $"- {player.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Мине")} " +
                        $"{targetMineExtra.owner.GetColoredName()}");
                    //. Ваша {ColorString.GetColoredExtra("Мина")} сработала!

                    room.roomChat.Extra_PersonalMessage(player, targetMineExtra, $"{player.GetColoredName()} " +
                         $"- {player.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Мине")} " +
                         $"{targetMineExtra.owner.GetColoredName()}. Вы убиты, увы!");

                    var excludedPlayers = new BasePlayer[] { player, targetMineExtra.owner };

                    room.roomChat.Extra_PublicMessageExcludePlayers($"{player.GetColoredName()} " +
                        $"- {player.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Мине")} " +
                        $"{targetMineExtra.owner.GetColoredName()}", room, targetMineExtra, excludedPlayers);

                    SendPlayerToMorgue(player);

                    targetMineExtra.DecreaseCount();

                    return true;
                }
            }

            return false;
        }

        public BasePlayer jailPlayer;
        public BasePlayer CheckVotes()
        {
            jailPlayer = null;

            var maxVote = int.MinValue;

            foreach(var p in dayVotes)
            {
                if (p.Key.isLive() && p.Value > maxVote) maxVote = p.Value;
            }

            var sameVote = 0;
            foreach(var p in dayVotes)
            {
                if (p.Key.isLive() && p.Value == maxVote) 
                {
                    jailPlayer = p.Key;

                    sameVote++; 
                }
            }

            var livePlayersCount = 0;
            foreach(var p in room.players.Values)
            {
                if (p.isLive()) livePlayersCount++;
            }

            //ничья
            if (sameVote > 1 && livePlayersCount > 2) 
            {
                room.roomChat.PublicMessage($"Несколько игроков получили одинаковое количество голосов, ничья!");

                jailPlayer = null;
            }

            if (sameVote == 0)
            {
                room.roomChat.PublicMessage($"Никто не голосовал. Все остаются на свободе");
            }           

            room.roomEndGame.CheckEndGame2();

            foreach(var p in room.players.Values)
            {
                p.ResetTarget();
            }

            return jailPlayer;
        }

        public bool TryEscapeBeforeJail()
        {
            var escape = false;

            var airPlanExtra = jailPlayer.FindExtraInSlots(ExtraEffect.AirPlane);
            if (airPlanExtra != null)
            {
                var explosion = jailPlayer.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Explosion);
                if (explosion != null)
                {
                    escape = false;

                    //владелец взрывчатки
                    room.roomChat.Extra_PersonalMessage(explosion.owner, explosion, $"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Самолёте")} " +
                        $"Ваша {ColorString.GetColoredExtra("Взрывчатка")} сработала");

                    //владелец самолета
                    room.roomChat.Extra_PersonalMessage(jailPlayer, explosion, $"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Самолёте")}, " +
                        $"кто-то подложил {ColorString.GetColoredExtra("Взрычатку")}. Вас взорвали, увы!");

                    var excludedPlayers = new BasePlayer[] { explosion.owner, jailPlayer };

                    //остальные
                    room.roomChat.Extra_PublicMessageExcludePlayers($"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} взорвался на {ColorString.GetColoredExtra("Самолёте")}, " +
                        $"кто-то подложил {ColorString.GetColoredExtra("Взрычатку")}", room, explosion, excludedPlayers);

                    SendPlayerToMorgue(jailPlayer);
                }
                else
                {
                    escape = true;

                    room.roomChat.Extra_PersonalMessage(jailPlayer, airPlanExtra, $"Вы улетаете с голосования, помашите всем ручкой");

                    room.roomChat.Extra_PublicMessage(airPlanExtra, $"{jailPlayer.GetColoredName()} улетел " +
                        $"на {ColorString.GetColoredExtra("Самолёте")} и на суд не явится");
                }

                airPlanExtra.DecreaseCount();
            }

            var spaceSuitExtra = jailPlayer.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Spacesuit);
            if (spaceSuitExtra != null)
            {
                escape = true;
                room.roomChat.PublicMessage("игрок избежал голосования в скафандре");
            }

            return escape;
        }

        public bool TryEscapeAfterJail()
        {
            var escape = false;

            if (jailPlayer.playerRole.roleType == RoleType.Mafia)
            {
                var mafiaRole = (Mafia)jailPlayer.playerRole;

                if (mafiaRole.Check_MafiaToFreedom())
                {
                    room.roomChat.Skill_PersonalMessage(jailPlayer, mafiaRole.skill_MafiaToFreedom, $"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} " +
                        $"сбежал при помощи навыка {ColorString.GetColoredSkill("Бегу на волю")}. " +
                        $"Ваш навык сработал");

                    var excludedPlayers = new BasePlayer[] { jailPlayer };
                    room.roomChat.Skill_PublicMessageExcludePlayers($"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} " +
                        $"сбежал при помощи навыка {ColorString.GetColoredSkill("Бегу на волю")}",
                        room, mafiaRole.skill_MafiaToFreedom, excludedPlayers);

                    escape = true;
                }
            }

            if (jailPlayer.playerRole.roleType == RoleType.Maniac)
            {
                var maniacRole = (Maniac)jailPlayer.playerRole;

                if (maniacRole.Check_ManiacToFreedom())
                {
                    room.roomChat.Skill_PersonalMessage(jailPlayer, maniacRole.skill_ManiacToFreedom,
                        $"{jailPlayer.GetColoredName()} - {jailPlayer.GetColoredRole()} " +
                        $"сбежал при помощи навыка {ColorString.GetColoredSkill("Бегу на волю")}. " +
                        $"Ваш навык сработал");

                    var excludedPlayers = new BasePlayer[] { jailPlayer };
                    room.roomChat.Skill_PublicMessageExcludePlayers($"{jailPlayer.GetColoredName()} - " +
                        $"{jailPlayer.GetColoredRole()} " +
                        $"сбежал при помощи навыка {ColorString.GetColoredSkill("Бегу на волю")}",
                        room, maniacRole.skill_ManiacToFreedom, excludedPlayers);

                    escape = true;

                    RoleHelper.UnlockRole_PlayerToRoom(jailPlayer);
                }
            }

            return escape;
        }

        public void SendPlayerToJail()
        {
            if (jailPlayer == null)
            {
                Logger.Log.Debug($"cant send player to jail. player is NULL");
                return;
            }

            //проверка на замену босса мафии грешником
            if (jailPlayer.playerRole.roleType == RoleType.MafiaBoss)
            {
                CheckSinnerSwitch();
            }

            //activePlayers.Remove(jailPlayer.playerId);

            jailPlayer.SetPlayerGameStatus(PlayerGameStatus.Dead);

            jailPlayer.playerRole.roleEffects.StopChatTimer();
            //Logger.Log.Debug($"Player {jailPlayer.playerName} go to jail");

            EventData eventData = new EventData((byte)Events.PlayerToJail);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, jailPlayer.playerId);
            eventData.Parameters.Add((byte)Params.RoleId, jailPlayer.playerRole.roleType);
            eventData.SendTo(room.clients, Options.sendParameters);

            RoleHelper.UnlockRole_PlayerToRoom(jailPlayer);
            //UnlockPlayerRoleToOtherPlayer(jailPlayer);

            //room.roomChat.PublicMessage($"{jailPlayer.GetColoredName()} - {jailPlayer.GetColoredRole()}");
        }

        public MafiaVisit mafiaVisit { get; private set; }
        public SinnerVisit sinnerVisit { get; private set; }
        public SaintVisit saintVisit { get; private set; }
        public CommissarVisit commissarVisit { get; private set; }
        public WitnessVisit witnessVisit { get; private set; }
        DoctorVisit doctorVisit;
        ManiacVisit maniacVisit;
        WerewolfVisit werewolfVisit;
        GuerillaVisit guerillaVisit;
        MafiaBossVisit mafiaBossVisit;

        public bool noMoreKillers { get; private set; } = false;
        public void CheckVisits()
        {
            noMoreKillers = false;

            //создаем стеки оповещений
            nightActionMessages.Clear();

            //чистим старые роли, если они по какой-то причине сохранились
            foreach (var p in room.players.Values)
            {
                p.SetOldRole(null);
            }

            //проверяем игроков, которые не делали ночные ходы
            foreach (var p in room.players.Values)
            {
                if (p.isLive())
                {
                    if (p.targetPlayer == null && p.playerRole.roleType != RoleType.Citizen)
                    {
                        room.roomChat.PublicMessage($"{p.GetColoredRole()} этой ночью остался дома");
                    }
                }
            }

            //ищем всех предствителей всех ролей среди живых игроков
            mafiaBossVisit.Setup();

            var jane = RoomHelper.FindPlayerByRole(RoleType.Jane, room);
            Jane janeRole = null;
            if (jane != null) { janeRole = (Jane)jane.playerRole; }

            guerillaVisit.Setup();

            //одиночные добрые роли
            commissarVisit.Setup();

            doctorVisit.Setup();

            witnessVisit.Setup();
            var astronaut = RoomHelper.FindPlayerByRole(RoleType.Astronaut, room);
            var goodClown = RoomHelper.FindPlayerByRole(RoleType.GoodClown, room);

            //одиночные злые роли
            maniacVisit.Setup();

            werewolfVisit.Setup();

            var garry = RoomHelper.FindPlayerByRole(RoleType.Garry, room);
            var loki = RoomHelper.FindPlayerByRole(RoleType.Loki, room);

            var alien = RoomHelper.FindPlayerByRole(RoleType.Alien, room);
            var badClown = RoomHelper.FindPlayerByRole(RoleType.BadClown, room);
            var scientist = RoomHelper.FindPlayerByRole(RoleType.Scientist, room);

            //подготовка ролей
            mafiaVisit.Setup();
           
            sinnerVisit.Setup();
            
            saintVisit.Setup();       

            //убивает или оглушает джейн
            if (jane != null)
            {
                janeRole.CheckKill();
            }

            //ходы ролей

            //оглушающие роли

            //оглушающий ход партизана
            guerillaVisit.Visit();

            //если у босса мафии не сработал "упс" и в команде злодеев больше 1 игрока, то он делает оглушающий ход
            mafiaBossVisit.Visit_Freeze();

            //если джейн не одна в команде пиратов, то она делает оглушающий ход
            if (jane != null && !janeRole.IsKiller())
            {
                var targetPlayer = DoNightAction(jane, DurationType.NightEnd);

                if (targetPlayer != null)
                {
                       nightActionMessages. AddNightActionMessage
                       (
                       RoleType.Jane,
                       NightActionId.Role,
                       () =>
                       {
                           room.roomChat.PublicMessage
                       ($"{jane.GetColoredRole()} охмурила {targetPlayer.GetColoredName()}");
                       }
                       );


                }
            }

            //ходы одиночных добрых ролей
            //комиссар

            commissarVisit.Visit();

            //доктор
            doctorVisit.Visit_Heal();        

            //космонавт
            if (astronaut != null)
            {
                var targetPlayer = DoNightActionNoRoleEffect(astronaut);

                if (targetPlayer != null)
                {
                    var astronautRole = (Astronaut)astronaut.playerRole;

                    astronautRole.PresentTickets(targetPlayer);
                }
            }

            //добрый клоун
            if (goodClown != null)
            {
                var targetPlayer = DoNightActionNoRoleEffect(goodClown);

                if (targetPlayer != null)
                {
                    var goodClownRole = (GoodClown)goodClown.playerRole;

                    goodClownRole.PresentKeys(targetPlayer);
                }
            }

            //ходы одиночных злых ролей
            maniacVisit.Visit();            

            werewolfVisit.Visit();            

            mafiaBossVisit.Visit_Kill();

           

            #region Event Evil solo visit

            if (jane != null && janeRole.IsKiller())
            {
                var targetPlayer = DoNightAction(jane, DurationType.NightEnd);

                if (targetPlayer != null)
                {
                    nightActionMessages.AddNightActionMessage
               (
               RoleType.Jane,
               NightActionId.Role,
               () =>
               {
                   room.roomChat.PublicMessage
                        ($"{jane.GetColoredRole()} убила {targetPlayer.GetColoredName()} " +
                        $"- {targetPlayer.GetColoredRole()}");
               }
               );



                    //отправляем цель босса мафии в морг
                    SendPlayerToMorgue(targetPlayer);

                    targetPlayer.SetKiller(jane);
                }
            }

           

            if (garry != null)
            {
                var targetPlayer = DoNightAction(garry, DurationType.NightEnd);

                if (targetPlayer != null)
                {
                    nightActionMessages.AddNightActionMessage
               (
               RoleType.Garry,
               NightActionId.Role,
               () =>
               {
                   room.roomChat.PublicMessage
                       ($"{garry.GetColoredRole()} убил {targetPlayer.GetColoredName()} " +
                       $"- {targetPlayer.GetColoredRole()}");
               }
               );



                    //отправляем цель босса мафии в морг
                    SendPlayerToMorgue(targetPlayer);

                    targetPlayer.SetKiller(garry);
                }
            }

           

            if (loki != null)
            {
                var targetPlayer = DoNightAction(loki, DurationType.NightEnd);

                if (targetPlayer != null)
                {
                    var lokiRole = (Loki)loki.playerRole;

                    lokiRole.CheckKill();

                    if (lokiRole.IsKiller())
                    {
                        nightActionMessages.AddNightActionMessage
              (
              RoleType.Loki,
              NightActionId.Role,
              () =>
              {
                  room.roomChat.PublicMessage
                          ($"{loki.GetColoredRole()} убил {targetPlayer.GetColoredName()} " +
                          $"- {targetPlayer.GetColoredRole()}");
              }
              );



                        //отправляем цель босса мафии в морг
                        SendPlayerToMorgue(targetPlayer);

                        targetPlayer.SetKiller(loki);
                    }
                    else
                    {
                        var coins = lokiRole.Rob(targetPlayer);

                        nightActionMessages.AddNightActionMessage
              (
              RoleType.Loki,
              NightActionId.Role,
              () =>
              {
                  room.roomChat.PublicMessage
                           ($"{loki.GetColoredRole()} ограбил {targetPlayer.GetColoredName()} на {coins} коинов");
              }
              );


                    }
                }
            }

           

            if (alien != null)
            {
                var targetPlayer = DoNightActionNoRoleEffect(alien);

                if (targetPlayer != null)
                {
                    var alienRole = (Alien)alien.playerRole;

                    alienRole.Rob(targetPlayer);
                }
            }

          

            if (badClown != null)
            {
                var targetPlayer = DoNightActionNoRoleEffect(badClown);

                if (targetPlayer != null)
                {
                    SendPlayerToMorgue(targetPlayer);

                    var badClownRole = (BadClown)badClown.playerRole;

                    badClownRole.GetKey(targetPlayer);
                }
            }

          

            if (scientist != null)
            {
                var targetPlayer = DoNightActionNoRoleEffect(scientist);

                if (targetPlayer != null)
                {
                    var scientistRole = (Scientist)scientist.playerRole;

                    scientistRole.Visit(targetPlayer);
                }
            }          

            #endregion

            //ход командных злых ролей
            mafiaVisit.Visit();

            noMoreKillers = true;

            commissarVisit.Visit(false);

            sinnerVisit.Visit();

            //ходы командных добрых ролей
            saintVisit.Visit();         

            //результат хода свидетеля
            witnessVisit.Visit();

            //воскрешение комиссара свидетелем
            witnessVisit.CheckWitnessFriends(commissarVisit.comissar);

            //проверяем была ли атакована цель доктора
            doctorVisit.Visit_Report();

            //проверяем действие прослушки
            foreach (var p in room.players.Values)
            {
                var wireTapeEffect = p.playerRole.roleEffects.FindExtraEffect(ExtraEffect.WireTape);
                if (wireTapeEffect != null)
                {
                    var wireTapeExtra = p.FindExtraInSlots(ExtraEffect.WireTape);

                    if (wireTapeExtra != null)
                    {
                        room.roomChat.Extra_PersonalMessage(wireTapeExtra.owner, wireTapeExtra,
                            $"Ваша {ColorString.GetColoredExtra("Прослушка")} обезвредила шпиона");

                        room.roomChat.Extra_PersonalMessage(wireTapeEffect.owner, wireTapeEffect,
                           $"Ваша {ColorString.GetColoredExtra("Прослушка")} была обезврежена " +
                           $"{ColorString.GetColoredExtra("Прослушкой")} {wireTapeExtra.owner.GetColoredName()}");

                        wireTapeExtra.DecreaseCount();
                        continue;
                    }                   

                    var spyResult = "";
                    if (p.wasActiveAtNight) spyResult = "активен";
                    else spyResult = "не активен";

                    Action action = () =>
                    {
                        room.roomChat.Extra_PublicMessage(wireTapeEffect, $"{wireTapeEffect.owner.GetColoredName()} " +
                            $"с помощью {ColorString.GetColoredExtra("Прослушки")} узнал," +
                            $" что {p.GetColoredName()} был {spyResult} этой ночью");
                    };

                    var actionId = wireTapeEffect.owner.AddShareAction(action);

                    if (p.wasActiveAtNight)
                    {
                        room.roomChat.Extra_SharePersonalMessage(wireTapeEffect.owner, wireTapeEffect, actionId,
                            $"Прослушанный Вами игрок {p.GetColoredName()} был активен этой ночью");
                    }
                    else
                    {
                        room.roomChat.Extra_SharePersonalMessage(wireTapeEffect.owner, wireTapeEffect, actionId,
                            $"Прослушанный Вами игрок {p.GetColoredName()} был неактивен этой ночью");
                    }
                }
            }

            //выводим последовательно ночные сообщения
            //Logger.Log.Debug($"comissar {commissarVisit.comissar.isLive()}");
            nightActionMessages.CheckNightActionMessages();


            //очищаем ночные ходы у всех игроков
            foreach (var p in room.players.Values)
            {
                p.ResetTarget();
                p.SetActiveAtNight(false);
            }

            //удаляем флаг, если игрок был воскрешен этой ночью
            foreach(var p in room.players.Values)
            {
                //p.playerRole.SetResurection(false);
            }

            room.roomEndGame.CheckEndGame2();
        }  

        private BasePlayer DoNightAction(BasePlayer player, DurationType durationType, BasePlayer targetPlayer = null)
        {
            if (!player.playerRole.CanVisit()) return null;

            if (targetPlayer == null)
            {
                if (player.playerRole.roleType == RoleType.Maniac)
                {
                    var maniacRole = (Maniac)player.playerRole;

                    if (!maniacRole.maniacIsIdeal)
                    {
                        targetPlayer = CheckTargetPlayerMirror(player);
                    }
                }
                else
                {
                    targetPlayer = CheckTargetPlayerMirror(player);
                }
            }

            if (targetPlayer == null) return null;

            //сделать проверку на защиту от гостя
            var targetResist = targetPlayer.playerRole.CheckResist(player);

            if (targetResist) return null;

            targetPlayer.playerRole.roleEffects.AddRoleEffect(player.playerRole);

            //Logger.Log.Debug($"do night action {player.playerRole.GetType()}");

            var newRoleHandler = new RoleHandler(targetPlayer, player.playerRole, room);

            targetPlayer.playerRole.roleEffects.AddRoleHandler(newRoleHandler);

            newRoleHandler.Subscribe(durationType);

            return targetPlayer;
        }

        public BasePlayer DoNightActionNoRoleEffect(BasePlayer player, BasePlayer targetPlayer = null)
        {
            if (!player.playerRole.CanVisit()) return null;

            if (targetPlayer == null)
            {
                targetPlayer = CheckTargetPlayerMirror(player);
            }

            if (targetPlayer == null) return null;

            //сделать проверку на защиту от гостя
            var targetResist = targetPlayer.playerRole.CheckResist(player);

            if (targetResist) return null;

            return targetPlayer;
        }

        public void ResurectPlayer(BasePlayer player)
        {
            //activePlayers.Add(player.playerId, player);

            player.SetPlayerGameStatus(PlayerGameStatus.Live);

            //Logger.Log.Debug($"Player {player.playerName} go to morgue");

            EventData eventData = new EventData((byte)Events.ResurectPlayer);
            eventData.Parameters = new Dictionary<byte, object> { };

            eventData.Parameters.Add((byte)Params.UserId, player.playerId);

            eventData.SendTo(room.clients, Options.sendParameters);
        }

        public void SendPlayerToMorgue(BasePlayer player, bool forcedDeath = false)
        {
            //проверка на воскрешение гражданина
            if (player.playerRole.roleType == RoleType.Citizen && forcedDeath==false)
            {
                var citizenRole = (Citizen)player.playerRole;

                if (citizenRole.Check_CitizenVitality())
                {
                    room.roomChat.Skill_PublicMessage(citizenRole.skill_CitizenVitality,
                        $"{player.GetColoredName()} - {ColorString.GetColoredRole("Гражданин")} " +
                        $"воскрес, благодаря навыку {ColorString.GetColoredSkill("Живучесть")}");

                    RoleHelper.UnlockRole_PlayerToRoom(player);

                    //запоминаем, что гражданин воскрешен //
                    //citizenRole.SetResurection(true);
                    return;
                }
            }

            //проверка на воскрешение грешника
            if (player.playerRole.roleType == RoleType.Sinner && forcedDeath == false)
            {
                var sinnerRole = (Sinner)player.playerRole;

                if (sinnerRole.Check_SinnerNotGiveUp())
                {
                    room.roomChat.Skill_PublicMessage(sinnerRole.skill_SinnerNotGiveUp, 
                        $"{player.GetColoredName()} - {ColorString.GetColoredRole("Грешник")} " +
                        $"воскрес, благодаря навыку {ColorString.GetColoredSkill("Я не сдаюсь")}");

                    RoleHelper.UnlockRole_PlayerToRoom(player);

                    //запоминаем, что грешник воскрешен //
                    //sinnerRole.SetResurection(true);
                    return;
                }
            }

            //если игрок умер днем, отменяем его голос
            if(room.roomPhases.gamePhase == GamePhase.Day)
            {
                if (player.targetPlayer != null && player.voteCount > 0) 
                {
                    RemoveDayVoteByPlayer(player);                    
                }
            }

            //проверка на замену босса мафии грешником
            if (player.playerRole.roleType == RoleType.MafiaBoss)
            {
                CheckSinnerSwitch();
            }           

            //activePlayers.Remove(player.playerId);

            player.SetPlayerGameStatus(PlayerGameStatus.Dead);

            player.playerRole.UpdateRoleStatus();

            player.playerRole.roleEffects.StopChatTimer();

            //отправляем ивент о смерти игрока
            EventData eventData = new EventData((byte)Events.PlayerToMorgue);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, player.playerId);
            eventData.Parameters.Add((byte)Params.RoleId, player.playerRole.roleType);
            eventData.SendTo(room.clients, Options.sendParameters);

            RoleHelper.UnlockRole_PlayerToRoom(player);
            //UnlockPlayerRoleToOtherPlayer(player);

            //room.roomChat.PublicMessage($"{player.GetColoredName()} - {player.GetColoredRole()}");
        }      

        private BasePlayer CheckTargetPlayerMirror(BasePlayer player)
        {
            if (player.targetPlayer == null) return null;

            var targetPlayerMirrorExtra = player.targetPlayer.playerRole.roleEffects.FindExtraEffect(ExtraEffect.Mirror);
            if (targetPlayerMirrorExtra != null)
            {
                player.targetPlayer = player;
            }

            return player.targetPlayer;
        }

        private void CheckSinnerSwitch()
        {
            var sinners = RoomHelper.FindPlayersByRole(RoleType.Sinner, room);

            //проверка, на "замену" грешником

            foreach (var s in sinners)
            {
                var sinnerRole = (Sinner)s.playerRole;

                if (sinnerRole.Check_SinnerChange())
                {
                    RoleHelper.ChangePlayerRole(s, RoleType.MafiaBoss);

                    if(room.roomPhases.gamePhase == GamePhase.EndNight)
                    {
                        nightActionMessages.AddNightActionMessage
                        (
                        RoleType.Sinner,
                        NightActionId.Skill,
                        () =>
                        {
                            room.roomChat.Skill_PersonalMessage(s, sinnerRole.skill_SinnerChange,
                                $"{ColorString.GetColoredRole("Грешник")} заменил {ColorString.GetColoredRole("Боса Мафии")} " +
                                $"благодаря навыку {ColorString.GetColoredSkill("Замена")}. " +
                                $"Теперь Вы играете за {ColorString.GetColoredRole("Боса Мафии")}, " +
                                $"ночью должны морозить игроков!");

                            var excludedPlayers = new BasePlayer[] { s };

                            room.roomChat.Skill_PublicMessageExcludePlayers(
                                $"{ColorString.GetColoredRole("Грешник")} заменил {ColorString.GetColoredRole("Боса Мафии")} " +
                                $"благодаря навыку {ColorString.GetColoredSkill("Замена")}",
                                room, sinnerRole.skill_SinnerChange, excludedPlayers);
                        }
                        );
                    }
                    else
                    {
                        room.roomChat.Skill_PersonalMessage(s, sinnerRole.skill_SinnerChange,
                            $"{ColorString.GetColoredRole("Грешник")} заменил {ColorString.GetColoredRole("Боса Мафии")} " +
                            $"благодаря навыку {ColorString.GetColoredSkill("Замена")}. " +
                            $"Теперь Вы играете за {ColorString.GetColoredRole("Боса Мафии")}, " +
                            $"ночью должны морозить игроков!");

                        var excludedPlayers = new BasePlayer[] { s };

                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{ColorString.GetColoredRole("Грешник")} заменил {ColorString.GetColoredRole("Боса Мафии")} " +
                            $"благодаря навыку {ColorString.GetColoredSkill("Замена")}",
                            room, sinnerRole.skill_SinnerChange, excludedPlayers);
                    }

                    RoleHelper.UnlockRole_PlayerToGroup(s.team.GetPlayers(), s);
                   
                    break;
                }
            }

        }

        private BasePlayer GetTargetPlayerById(long id)
        {
            if (room.players.ContainsKey(id))
            {
                return room.players[id];
            }

            return null;
        }
    }
}

public class NightActionMessage
{
    public NightActionId actionId { get; private set; }

    public BasePlayer owner;

    public RoleType roleId;
    public Action action { get; private set; }

    public NightActionMessage(RoleType roleId, NightActionId actionId, Action action, BasePlayer owner=null)
    {
        this.owner = owner;       
        this.actionId = actionId;
        this.action = action;

        if (roleId == RoleType.NULL)
        {
            this.roleId = owner.playerRole.roleType;
        }
        else
        {
            this.roleId = roleId;
        }
    }

    public RoleType GetRoleId()
    {
        return roleId;
    }

    public void DoAction()
    {
        if (action != null)
        {
            action();
        }
    }
}

public enum NightActionId
{
    Skill,
    Extra,
    Role,
}