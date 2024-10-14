using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomPhases
    {
        private Room room;

        public RoomPhases(Room room) 
        {
            this.room = room;
        }

        public int currentPhaseTime;


        public void GameTimer()
        {
            currentPhaseTime--;

            SendGameTickToPlayers();

            if (currentPhaseTime == 0)
            {
                //меняем фазу день <=> ночь
                SwitchGamePhase();
            }
        }

        private void SendGameTickToPlayers()
        {
            EventData eventData = new EventData((byte)Events.GameTimer);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.Timer, currentPhaseTime);
            eventData.SendTo(room. clients, Options.sendParameters);
        }

        public void SwitchGamePhase()
        {
            if (room.roomIsStoped) return;

            switch (gamePhase)
            {
                case GamePhase.FirstNight: {  SetGamePhase(GamePhase.EndFirstNight); } break;
                case GamePhase.EndFirstNight: { SetGamePhase(GamePhase.Day); } break;

                case GamePhase.Day: { SetGamePhase(GamePhase.EndDay); } break;
                case GamePhase.EndDay: { SetGamePhase(GamePhase.Judging); } break;

                case GamePhase.Judging: { SetGamePhase(GamePhase.EndJudging); } break;
                case GamePhase.EndJudging: { SetGamePhase(GamePhase.Night); } break;

                case GamePhase.Night: { SetGamePhase(GamePhase.EndNight); } break;
                case GamePhase.EndNight: { SetGamePhase(GamePhase.Day); } break;
            }

            //Logger.Log.Debug($"start {gamePhase}");
        }

        public GamePhase gamePhase { get; private set; } = GamePhase.Any;
        public void SetGamePhase(GamePhase phase)
        {
            if (room.roomIsStoped) return;

            gamePhase = phase;

            //Logger.Log.Debug($"SetGamePhase {gamePhase}");

            switch (gamePhase)
            {
                case GamePhase.FirstNight:
                    {
                        currentPhaseTime = room.testFNightDuration;
                        SendGamePhaseToPlayers();                       
                        StartFirstNight();                        
                    }
                    break;

                case GamePhase.EndFirstNight:
                    {                        
                        EndFirstNight();
                    }
                    break;

                case GamePhase.Day:
                    {
                        currentPhaseTime = room.testDayDuration;
                        SendGamePhaseToPlayers();                       
                        StartDay();                        
                    }
                    break;

                case GamePhase.EndDay:
                    {
                        EndDay();
                    }
                    break;

                case GamePhase.Night:
                    {
                        currentPhaseTime = room.testNightDuration;
                        SendGamePhaseToPlayers();                       
                        StartNight();                       
                    }
                    break;
                case GamePhase.EndNight:
                    {                      
                        EndNight();
                    }
                    break;

                case GamePhase.Judging:
                    {
                        currentPhaseTime = room.testJudgeDuration;
                        SendGamePhaseToPlayers();                       
                        room.roomJudging.StartJudging();                       
                    }
                    break;

                case GamePhase.EndJudging:
                    {
                        room.roomJudging.EndJudging();
                    }
                    break;

            }           
        }

        private void SendGamePhaseToPlayers()
        {
            EventData eventData = new EventData((byte)Events.ChangeGamePhase);
            eventData.Parameters = new Dictionary<byte, object> { };
            
            eventData.Parameters.Add((byte)Params.Timer, currentPhaseTime);
            eventData.Parameters.Add((byte)Params.GamePhase, gamePhase);
            eventData.Parameters.Add((byte)Params.DayCount, dayCount);

            eventData.SendTo(room.clients, Options.sendParameters);

            //Logger.Log.Debug($"room.roomClients { room.roomClients.Count}");
        }

        private void StartFirstNight()
        {
            //в этой фазе игры происходит знакомство игроков и выбор роли оборотня
            room.roomChat.PublicMessage("Первая ночь. Все знакомятся");

            //вербовка оборотня вначале игры с помощью талантов
            room.roomLogic.FirstNightWerewolfRecruitment();

            //активация автоматических экстр вначале игры
            room.roomLogic.CheckStartGameExtras();

            //проверка безлимита для маньяка и оборотня
            room.roomLogic.CheckManiacUnlimit();
            room.roomLogic.CheckWerewolfUnlimit();
        }      

        private void EndFirstNight()
        {
             room.roomLogic.CheckWerewolfFirstNightAction();            

            //SetGamePhase(GamePhase.Day);
            SwitchGamePhase();
        }

        public int dayCount { get; private set; } = 1;
        public event EventHandler OnDayStart;
        private void StartDay()
        {
            OnDayStart?.Invoke(this, EventArgs.Empty);

            room.roomLogic.ResetVotes();  

            //скиллы первого дня
            if (dayCount == 1)
            {
                //проверка на скилл гражданина "раскрыть тайну"
                foreach (var p in room.players.Values)
                {
                    if(p.isLive() && p.playerRole.roleType == RoleType.Citizen)
                    {
                        var citizenRole = (Citizen)p.playerRole;

                        if (citizenRole.Check_CitizenSecret())
                        {
                            //var personalMessage = $"Ваш навык {ColorString.GetColoredSkill("Раскрыть тайну")} " +
                            //  $"сработал!";
                            var personalMessage = $"{p.GetColoredName()} - {ColorString.GetColoredRole("Гражданин")}, " +
                               $"раскрыл Всем мирным свою роль! Ваш навык {ColorString.GetColoredSkill("Раскрыть тайну")} " +
                               $"сработал!";

                            room.roomChat.Skill_PersonalMessage(p, citizenRole.skill_CitizenSecret, personalMessage);
                                 
                            var publicMessage = $"{p.GetColoredName()} - {ColorString.GetColoredRole("Гражданин")}, " +
                                $"раскрыл Всем мирным свою роль!";

                            var targetPlayers = new List<BasePlayer>();

                            foreach (var tp in room.players.Values)
                            {
                                if (tp.team.teamType != TeamType.Good || tp == p) continue;
                                {
                                    targetPlayers.Add(p);
                                    //room.roomChat.Skill_PersonalMessage(gp, citizenRole.skill_CitizenSecret, publicMessage);
                                }
                            }

                            room.roomChat.Skill_GroupMessage(targetPlayers, citizenRole.skill_CitizenSecret, publicMessage);

                            RoleHelper.UnlockRole_PlayerToGroup(targetPlayers, p);
                        }
                    }
                }

                //где ты? скилл доктора
                foreach (var p in room.players.Values)
                {
                    if (p.isLive() && p.playerRole.roleType == RoleType.Doctor)
                    {
                        var doctorRole = (Doctor)p.playerRole;

                        if (doctorRole.Check_DoctorWhereAreYou())
                        {
                            var comissar = RoomHelper.FindPlayerByRole(RoleType.Commissar, room);

                            room.roomChat.Skill_PersonalMessage(p, doctorRole.skill_DoctorWhereAreYou, 
                                $"{comissar.GetColoredName()} играет за " +
                                $"{ColorString.GetColoredRole("Комиссара")}. " +
                                $"Сработал навык {ColorString.GetColoredSkill("Где ты?")}");

                            RoleHelper.UnlockRole_PlayerToPlayer(comissar, p, comissar.playerRole.roleType);
                            //room.roomLogic.UnlockPlayerRoleToOtherPlayer(comissar, p, comissar.playerRole.roleType);
                        }
                    }
                }
            }

            //сброс целей игроков
            foreach (var p in room.players.Values)
            {
                p.ResetTarget();
                p.EnableJudge();
            }

           

            room.roomBots.BotVotesEmulation();

            room.roomBots.BotChatEmulation();

            //room.roomBots.BotExtraEmulation(ExtraEffect.Bit);           

            //проверяем, кто из игроков может голосовать и отправляем на клиенты соотв пакеты
            SendVotesToPlayers();
        }

        private List<BasePlayer> GetLivePlayers()
        {
            var result = new List<BasePlayer>();

            foreach (var p in room.players.Values)
            {
                if (p.isLive()) result.Add(p);
            }

            return result;
        }

        private void SendVotesToPlayers()
        {
            var livePlayers = GetLivePlayers();

            foreach(var p in livePlayers)
            {
                if (p.playerType == PlayerType.Player && p.playerRole.CanVote()) 
                {
                    SendVotesToPlayer(p, livePlayers);
                }
            }
        }

        private void SendVotesToPlayer(BasePlayer player, List<BasePlayer> votes=null)
        {
            if(!player.isLive()) return;

            if (votes == null)
            {
                votes = GetLivePlayers();
            }

            var voteIds = new Dictionary<byte, long>();
            byte idCounter=0;

            foreach (var p in votes)
            {
                if(p.playerId != player.playerId)
                {
                    voteIds.Add(idCounter++, p.playerId);
                }
            }

            OperationResponse resp = new OperationResponse((byte)Request.EnableVote);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.Votes, voteIds);
            player.client.SendOperationResponse(resp, Options.sendParameters);
        }

        public event EventHandler OnDayEnd;
        private void EndDay()
        {
            //проверка "меняюсь" у оборотня
            if (dayCount == 1)
            {
                var werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);
                if (werewolf != null)
                {
                    var werewolfRole = (Werewolf)werewolf.playerRole;

                    if (werewolfRole.Check_WerewolfChange())
                    {
                        var newTeam = werewolfRole.ChangeTeamToRandom();

                        RoomHelper.AddPlayerToTeam(room, werewolf, newTeam.teamType);

                        //отправить на клиент ивент о смене команды
                        werewolfRole.Event_chnageTeam();

                        switch (newTeam.teamType)
                        {
                            case TeamType.Good :
                                {
                                    room.roomChat.Skill_PersonalMessage(werewolf, werewolfRole.skill_WerewolfChange,
                                        $"{ColorString.GetColoredRole("Оборотень")} сменил команду. " +
                                        $"Навык {ColorString.GetColoredSkill("Меняюсь")} в действии." +
                                        $"Теперь Вы играете на строне команды Мирных");
                                } break;
                            case TeamType.Bad:
                                {
                                    room.roomChat.Skill_PersonalMessage(werewolf, werewolfRole.skill_WerewolfChange,
                                        $"{ColorString.GetColoredRole("Оборотень")} сменил команду. " +
                                        $"Навык {ColorString.GetColoredSkill("Меняюсь")} в действии." +
                                        $"Теперь Вы играете на строне команды Мафии");
                                }
                                break;
                            case TeamType.Neutral:
                                {
                                    room.roomChat.Skill_PersonalMessage(werewolf, werewolfRole.skill_WerewolfChange,
                                        $"{ColorString.GetColoredRole("Оборотень")} сменил команду. " +
                                        $"Навык {ColorString.GetColoredSkill("Меняюсь")} в действии." +
                                        $"Теперь Вы за себя");
                                }
                                break;
                        }

                        var excludedPlayers = new BasePlayer[] { werewolf };
                        room.roomChat.Skill_PublicMessageExcludePlayers(
                            $"{ColorString.GetColoredRole("Оборотень")} сменил команду. " +
                            $"Навык {ColorString.GetColoredSkill("Меняюсь")} в действии",
                            room, werewolfRole.skill_WerewolfChange, excludedPlayers);
                    }
                }
            }

            if (room.roomEndGame.CheckEndGame2()) return;

            OnDayEnd?.Invoke(this, EventArgs.Empty);

            room.roomLogic.CheckVotes();

            room.roomPhases.SetGamePhase(GamePhase.Judging);
        }

        public event EventHandler OnNightStart;
        private void StartNight()
        {
            //room.roomChat.PublicMessage($"Началась ночь #{dayCount}");

            OnNightStart?.Invoke(this, EventArgs.Empty);

           

            //проверка умений ролей, срабатывающих вначале ночи
            CheckStartNightSkills();

            room.roomBots.BotVisitsEmulation();

            //room.roomBots.BotExtraEmulation(ExtraEffect.Bit); 

            SendVisitsToPlayers();
        }


        private void SendVisitsToPlayers()
        {
            var livePlayers = GetLivePlayers();

            foreach (var p in livePlayers)
            {
                if (p.playerType == PlayerType.Player && p.playerRole.CanVisit())
                {
                    SendVisitsToPlayer(p, livePlayers);
                }
            }
        }

        public void SendVisitsToPlayer(BasePlayer player, List<BasePlayer> votes = null)
        {
            if (!player.isLive()) return;

            if(votes == null )
            {
                votes = GetLivePlayers();
            }

            var visitIds = new Dictionary<byte, long>();
            byte idCounter = 0;

            if(player.playerRole.roleType != RoleType.Citizen)
            {
                foreach (var p in votes)
                {
                    if (p.playerId != player.playerId)
                    {
                        if (player.team.teamType == TeamType.Bad && p.team.teamType == TeamType.Bad) { continue; }

                        if (player.team.teamType == TeamType.Pirate && p.team.teamType == TeamType.Pirate) { continue; }

                        visitIds.Add(idCounter++, p.playerId);
                    }
                }
            }            

            if(player.playerType == PlayerType.Player)
            {
                OperationResponse resp = new OperationResponse((byte)Request.EnableVisit);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.Visits, visitIds);
                player.client.SendOperationResponse(resp, Options.sendParameters);
            }          
        }

        private void CheckStartNightSkills()
        {
            //проверка на "песнь" маньяка
            var maniac = RoomHelper.FindPlayerByRole(RoleType.Maniac, room);

            if(maniac != null )
            {

                ((Maniac)maniac.playerRole).Check_ManiacSong();
                ((Maniac)maniac.playerRole).Check_ManiacIdeal();
            }

            //проверка на "туман" грешника
            var sinners = RoomHelper.FindPlayersByRole(RoleType.Sinner, room);

            foreach(var s in sinners)
            {
                var sinnerRole = (Sinner)s.playerRole;

                if (sinnerRole.Check_SinnerMist())
                {
                    room.roomChat.Skill_PersonalMessage(s, sinnerRole.skill_SinnerMist,
                        $"В городе {ColorString.GetColoredSkill("Туман")}, действия этой ночи будут скрыты. " +
                        $"Ваш навык сработал!");

                    var excludedPlayers = new BasePlayer[] { s };
                    room.roomChat.Skill_PublicMessageExcludePlayers(
                        $"В городе {ColorString.GetColoredSkill("Туман")}, действия этой ночи будут скрыты. ",
                        room, sinnerRole.skill_SinnerMist, excludedPlayers);

                    //do some action            
                    foreach (var p in room.players.Values)
                    {
                        if (p.team.teamType != TeamType.Bad)
                        {
                            SkillHelper.AddSkillEffectToPlayer(p, sinnerRole.skill_SinnerMist, DurationType.DayStart);
                        }
                    }                  

                    break;
                }               
            }           
        }

        public event EventHandler OnNightEnd;
        private void EndNight()
        {
            //room.roomChat.PublicMessage("ночь закончилась");

            room.roomLogic.CheckVisits();

            OnNightEnd?.Invoke(this, EventArgs.Empty);         

            dayCount++;

            SetGamePhase(GamePhase.Day);
        }
    }
}
