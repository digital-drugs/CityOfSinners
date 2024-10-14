using ExitGames.Client.Photon.LoadBalancing;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomJudging
    {
        private Room room;
        public RoomJudging(Room room) 
        { 
            this.room = room;
        }

        public void StartJudging()
        {
            if (room.roomLogic. jailPlayer == null)
            {
                room.roomPhases.SetGamePhase(GamePhase.Night);
                return;
            }

            if (room.GetLivePlayers().Count == 2)
            {
                var endGame = room.roomEndGame.CheckEndGame2();

                if (endGame) return;
            }

            var escape = room.roomLogic.TryEscapeBeforeJail();
            //если игрок сбежал или погиб, пытаясь, то отменяем суд
            if (escape || room.roomLogic.jailPlayer.isLive() == false)
            {
                room.roomLogic.jailPlayer = null;
                room.roomPhases.SetGamePhase(GamePhase.Night);
                return;
            }

            Logger.Log.Debug($"jailPlayer => {room.roomLogic.jailPlayer.playerName}");

            Logger.Log.Debug($"start judging");          

            sentenceCount = 0;
            justifyCount = 0;

            var canSee = new List<Client>();
            var canJudge = new List<Client>();

            //исключить игроков, которые не могут голосовать 
            //var JudgingPlayers = new List<Client>();
            foreach (var p in room.players.Values)
            {
                if(p.isLive() && p.playerRole.CanVote() && p.client !=null && room.roomLogic.jailPlayer != p)
                {
                    canJudge.Add(p.client);
                }

                if ((!p.isLive() || !p.playerRole.CanVote() || p== room.roomLogic.jailPlayer ) && p.client != null) 
                {
                    canSee.Add(p.client);
                }
            }

            //создаем ивент
            //передаем на клиенты инфо о подсудимом
            EventData eventData = null;

            eventData = new EventData((byte)Events.StartJudging);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, room.roomLogic.jailPlayer.playerId);
            eventData.SendTo(canJudge, Options.sendParameters);

            eventData = new EventData((byte)Events.SeeJudging);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.Parameters.Add((byte)Params.UserId, room.roomLogic.jailPlayer.playerId);
            eventData.SendTo(canSee, Options.sendParameters);

            var jailPlayerText = ColorString.GetColoredString(room.roomLogic.jailPlayer.playerName, ColorId.Player);
            room.roomChat.PublicMessage($"{jailPlayerText} подозревается в пособничестве мафии!");
        }

        private int sentenceCount;
        private int justifyCount;
        public void Judging(BasePlayer player, Dictionary<byte, object> parameters)
        {
            if(!player.isLive() || !player.CanJudje()) { return; }

            player.DisableJudge();

            var sentence = (bool)parameters[(byte)Params.Sentence];

            //учитываем голоса приговорить/оправдать
            var voiceCount = 1;
            if (player.playerRole.Check_SkillVoteX2())
            {
                room.roomChat.Skill_PublicMessage(player.playerRole.skill_VoteX2, 
                    $"{player.GetColoredName()} удвоил голос при помощи навыка {ColorString.GetColoredSkill("x2")}");
                voiceCount = 2;

                Logger.Log.Debug($"have x2");
            }
            else
            {
                Logger.Log.Debug($"NOT have x2");
            }

            var judgeText = "";
            if (sentence)
            {
                sentenceCount += voiceCount;
                judgeText = "приговаривает";
            }
            else
            {
                justifyCount += voiceCount;
                judgeText = "оправдывает";
            }

            //пишем в чат результаты голосов
            var judgingPlayerText = ColorString.GetColoredString(player.playerName, ColorId.Player);
            var jailPlayerText = ColorString.GetColoredString(room.roomLogic.jailPlayer.playerName, ColorId.Player);
            judgeText = ColorString.GetColoredString(judgeText, ColorId.Judging);

            room.roomChat.PublicMessage($"{judgingPlayerText} {judgeText} {jailPlayerText} " +
                $"/ приговорить:{sentenceCount} оправдать:{justifyCount}");

            CheckAllPlayersJudge();
        }

        public void CheckAllPlayersJudge()
        {
            var allPlayersJudge = true;

            foreach (var p in room.GetLivePlayers().Values)
            {
                if (p.playerType == PlayerType.Player && p.CanJudje()) 
                { 
                    allPlayersJudge = false;
                    break; 
                }
            }

            if (allPlayersJudge)
            {
                room.roomPhases.SwitchGamePhase();
            }
        }

        public void EndJudging()
        {
            //суд завршается по таймеру или если все проголосовали

            //ивент окончания суда
            EventData eventData = new EventData((byte)Events.EndJudging);
            eventData.Parameters = new Dictionary<byte, object> { };
            eventData.SendTo(room.clients, Options.sendParameters);

            var sentenced = sentenceCount > justifyCount;

            Logger.Log.Debug($"sentenced => {sentenced} / s {sentenceCount} / j {justifyCount}");

            var judgeText = "";
            if (sentenced) { judgeText = "приговорен"; } else { judgeText = "оправдан"; }

            var judgeResultText = "";
            //var jailPlayerRole = Helper.GetRoleNameById_Rus(room.roomLogic.jailPlayer.playerRole.roleType);
            if(room.roomLogic.jailPlayer.team.teamType != TeamType.Good)
            {
                judgeResultText = $"правосудие торжествует! " +
                    $"{room.roomLogic.jailPlayer.playerName} - " +
                    $"{room.roomLogic.jailPlayer.GetColoredRole()}";
            }
            else
            {
                judgeResultText = $"мафия торжествует! " +
                  $"{room.roomLogic.jailPlayer.playerName} - " +
                  $"{room.roomLogic.jailPlayer.GetColoredRole()}";
            }

            var jailPlayerText = ColorString.GetColoredString(room.roomLogic.jailPlayer.playerName, ColorId.Player);
            judgeText = ColorString.GetColoredString(judgeText, ColorId.Judging);

            //пишем на клиенты результат судейства
            if(sentenced)
            {
                room.roomChat.PublicMessage($"{jailPlayerText} {judgeText}" +
                                            $"\n{judgeResultText}");
            }
            else
            {
                room.roomChat.PublicMessage($"{jailPlayerText} {judgeText}");
            }        

            //проверяем самолет и взрывчатку
            if (sentenced)
            {
                var escape = room.roomLogic.TryEscapeAfterJail();                
                if (!escape)
                {
                    room.roomLogic.SendPlayerToJail();
                }
            }

            room.roomLogic.jailPlayer = null;

            room.roomEndGame.CheckEndGame2();           

            //запускаем ночь
            room.roomPhases.SetGamePhase(GamePhase.Night);
        }
    }
}
