using MySqlX.XDevAPI.Common;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class RoomEndGame
    {
        private Room room;
        public RoomEndGame(Room room)
        { 
            this.room = room;
        }

        public bool CheckEndGame2()
        {
            var goodTeam = room.roomRoles.goodTeam;
            var badTeam = room.roomRoles.badTeam;
            var neutralTeam = room.roomRoles.neutralTeam;
            var pirateTeam = room.roomRoles.pirateTeam;

            var goodPlayers = goodTeam.GetLivePlayers();
            var badPlayers = badTeam.GetLivePlayers();
            var neutralPlayers = neutralTeam.GetLivePlayers();
            var piratePlayers = pirateTeam.GetLivePlayers();

            var allLivePlayers = new List<BasePlayer>();
            foreach(var p in room.players.Values)
            {
                if (p.isLive())
                {
                    allLivePlayers.Add(p);
                }
            }

            //проверяем жив ли оборотень
            var werewolf = RoomHelper.FindPlayerByRole(RoleType.Werewolf, room);
            var werewolfIsGood = false;

            if (werewolf != null && werewolf.team == goodTeam)
            {
                werewolfIsGood = true;
            }

            //проверяем жив ли маньяк
            //var maniac = RoomHelper.FindPlayerByRole(RoleType.Maniac, room);  

            //ничья
            if (badPlayers.Count == 0 && goodPlayers.Count == 0 && piratePlayers.Count == 0 && neutralPlayers.Count == 0)
            {
                EndGameTie();
            }

            //автоматическая победа мафиози
            if (badPlayers.Count > goodPlayers.Count 
                && (piratePlayers.Count == 0 && neutralPlayers.Count == 0)
                && !werewolfIsGood) 
            {
                EndGame(badTeam);
                return true;
            }

            //автоматическая победа пиратов
            if (piratePlayers.Count > goodPlayers.Count 
                && (badPlayers.Count == 0 && neutralPlayers.Count == 0)
                && !werewolfIsGood)
            {
                EndGame(pirateTeam);
                return true;
            }

            //победа мафиози
            if (badPlayers.Count > 0 && goodPlayers.Count == 0 && piratePlayers.Count == 0 && neutralPlayers.Count == 0) 
            {
                EndGame(badTeam);
                return true;
            }

            //победа пиратов
            if (piratePlayers.Count > 0 && goodPlayers.Count == 0 && badPlayers.Count == 0 && neutralPlayers.Count == 0) 
            {
                EndGame(pirateTeam);
                return true;
            }

            //победа мирных
            if (goodPlayers.Count > 0 && piratePlayers.Count == 0 && badPlayers.Count == 0 && neutralPlayers.Count == 0) 
            {
                EndGame(goodTeam);
                return true;
            }

            //победа нейтралов
            if (neutralPlayers.Count ==1 && goodPlayers.Count == 0 && badPlayers.Count == 0 && piratePlayers.Count == 0) 
            {
                EndGame(neutralTeam);
                return true;
            }

         

            //драка
            if(allLivePlayers.Count == 2 )
            {
                Logger.Log.Debug("2 players fight");

                if (allLivePlayers[0].targetPlayer != null)
                {
                    Logger.Log.Debug($"{allLivePlayers[0].targetPlayer.playerName}");
                }

                if (allLivePlayers[1].targetPlayer != null)
                {
                    Logger.Log.Debug($"{allLivePlayers[1].targetPlayer.playerName}");
                }

                if (allLivePlayers[0].targetPlayer == allLivePlayers[1] &&
                    allLivePlayers[1].targetPlayer == allLivePlayers[0])
                {
                    Logger.Log.Debug("2 players one to one targets");

                    BasePlayer winner = null;
                    BasePlayer looser = null;

                    for (int i = 0; i < allLivePlayers.Count; i++)
                    {
                        var player = allLivePlayers[i];

                        if (allLivePlayers[i].playerRole.roleType == RoleType.MafiaBoss)
                        {
                            var mafiaBossRole = (MafiaBoss)player.playerRole;

                            if (mafiaBossRole.Check_MafiaBossBodyBuilder())
                            {
                                if (i == 0)
                                {
                                    winner = allLivePlayers[0];
                                    looser = allLivePlayers[1];
                                }
                                else
                                {
                                    winner = allLivePlayers[1];
                                    looser = allLivePlayers[0];
                                }

                                room.roomChat.PublicMessage(
                                     $"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                                     $"победил в драке " +
                                     $"{looser.GetColoredName()} - {looser.GetColoredRole()}");

                                //room.roomChat.PersonalMessage(
                                //    winner,
                                //    //$"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                                //    //$"победил в драке " +
                                //    //$"{looser.GetColoredName()} - {looser.GetColoredRole()}. ");
                                //    $"Вы победили!");

                                //room.roomChat.PersonalMessage(
                                //    looser,
                                //    //$"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                                //    //$"победил в драке " +
                                //    //$"{looser.GetColoredName()} - {looser.GetColoredRole()}. " +
                                //    $"Вы проиграли, увы!");

                                var excludedPlayers = new BasePlayer[] { winner,looser };

                                                             

                                room.roomChat.Skill_PersonalMessage(
                                    winner, mafiaBossRole.skill_MafiaBossBodyBuilder,
                                   $"Сработал навык {ColorString.GetColoredSkill("Качок")} " +
                                   $"Вы победили!");

                                room.roomChat.Skill_PersonalMessage(
                                    looser, mafiaBossRole.skill_MafiaBossBodyBuilder,
                                    $"Сработал навык {ColorString.GetColoredSkill("Качок")} " +
                                    $"Вы проиграли, увы!");

                                room.roomChat.Skill_PublicMessageExcludePlayers(
                                     $"Сработал навык {ColorString.GetColoredSkill("Качок")} ",
                                     room, mafiaBossRole.skill_MafiaBossBodyBuilder, excludedPlayers);

                                EndGame(badTeam);
                                return true;
                            }
                        }
                    }

                    var braceKnuklesA = allLivePlayers[0].FindExtraInSlots(ExtraEffect.BraceKnukles);
                    var braceKnuklesB = allLivePlayers[1].FindExtraInSlots(ExtraEffect.BraceKnukles);

                    winner = null;
                    looser = null;

                    bool extraUsed = false;
                    Extra winnerExtra = null;

                    //у обоих есть кастеты победитель 50 на 50
                    if (braceKnuklesA != null && braceKnuklesB != null)
                    {
                        var dice = room.dice.Next(0, 2);

                        if (dice == 0)
                        {
                            winner = allLivePlayers[0];
                            looser = allLivePlayers[1];

                            winnerExtra = braceKnuklesA;
                        }
                        else
                        {
                            looser = allLivePlayers[0];
                            winner = allLivePlayers[1];

                            winnerExtra = braceKnuklesB;
                        }

                        extraUsed = true;

                        braceKnuklesA.DecreaseCount();
                        braceKnuklesB.DecreaseCount();
                    }

                    //у обоих нет кастетов победитель 50 на 50
                    if (braceKnuklesA == null && braceKnuklesB == null)
                    {
                        var dice = room.dice.Next(0, 2);

                        if (dice == 0)
                        {
                            winner = allLivePlayers[0];
                            looser = allLivePlayers[1];
                        }
                        else
                        {
                            looser = allLivePlayers[0];
                            winner = allLivePlayers[1];
                        }
                    }

                    //кастет у игрока А
                    if (braceKnuklesA != null && braceKnuklesB == null)
                    {
                        winner = allLivePlayers[0];
                        looser = allLivePlayers[1];

                        extraUsed = true;
                        winnerExtra = braceKnuklesA;
                        braceKnuklesA.DecreaseCount();
                    }

                    //кастет у игрока Б
                    if (braceKnuklesA == null && braceKnuklesB != null)
                    {
                        looser = allLivePlayers[0];
                        winner = allLivePlayers[1];

                        extraUsed = true;
                        winnerExtra = braceKnuklesB;
                        braceKnuklesB.DecreaseCount();
                    }

                    //если использован кастет
                    if (extraUsed)
                    {
                        room.roomChat.Extra_PersonalMessage(winner, winnerExtra,
                             $"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                             $"победил в драке " +
                             $"{looser.GetColoredName()} - {looser.GetColoredRole()} " +
                             $"благодаря экстре {ColorString.GetColoredExtra("Кастет")}! " +
                             $"Вы победили, ура!");

                        room.roomChat.Extra_PersonalMessage(looser, winnerExtra,
                             $"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                             $"победил в драке " +
                             $"{looser.GetColoredName()} - {looser.GetColoredRole()} " +
                             $"благодаря экстре {ColorString.GetColoredExtra("Кастет")}! " +
                             $"Вы проиграли, увы!");

                        var excludedPlayers = new BasePlayer[] { winner, looser };
                        room.roomChat.Extra_PublicMessageExcludePlayers(
                              $"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                             $"победил в драке " +
                             $"{looser.GetColoredName()} - {looser.GetColoredRole()} " +
                             $"благодаря экстре {ColorString.GetColoredExtra("Кастет")}!",
                            room, winnerExtra, excludedPlayers);
                    }
                    //если кастетов не было
                    else
                    {
                        room.roomChat.PublicMessage(
                            $"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                            $"победил в драке " +
                            $"{looser.GetColoredName()} - {looser.GetColoredRole()}");

                        room.roomChat.PersonalMessage(winner,
                            $"Вы победили!");

                        room.roomChat.PersonalMessage(looser,
                            //$"{winner.GetColoredName()} - {winner.GetColoredRole()} " +
                            //$"победил в драке " +
                            //$"{looser.GetColoredName()} - {looser.GetColoredRole()}. " +
                            $"Вы проиграли, увы!");
                    }

                    EndGame(winner.team);

                    return true;
                }
            }

            return false;
        }

        private void EndGameTie()
        {
            var message = $"Ничья!" +
                $"\nРейтинг: 0" +
                $"\nОпыт: 0" +
                $"\nМонеты: 0";

            foreach(var p in room.players.Values)
            {
                SendToPlayerEndGameResult(p,message);
            }

            ManagerRooms.instance.DisposeRoom(room);
        }

        private bool useDetailedResult = false;

        private void EndGame(Team winnerTeam)
        {
            CheckRoleAchievements();

            var goodTeam = room.roomRoles.goodTeam;
            var badTeam = room.roomRoles.badTeam;
            var neutralTeam = room.roomRoles.neutralTeam;
            var pirateTeam = room.roomRoles.pirateTeam;

            var goodPlayers = goodTeam.GetLivePlayers();
            var badPlayers = badTeam.GetLivePlayers();
            var neutralPlayers = neutralTeam.GetLivePlayers();
            var piratePlayers = pirateTeam.GetLivePlayers();

            var winnerLive = new List<BasePlayer>();
            var winnerDead = new List<BasePlayer>();
            var loosers = new List<BasePlayer>();

            if(goodTeam != winnerTeam)
            {
                loosers.AddRange(goodTeam.GetPlayers());
            }
            else
            {
                foreach(var p in goodTeam.GetPlayers())
                {
                    if (p.isLive())
                    {
                        winnerLive.Add(p);
                    }
                    else
                    {
                        winnerDead.Add(p);
                    }
                }
            }

            if(badTeam != winnerTeam)
            {
                loosers.AddRange(badTeam.GetPlayers());
            }
            else
            {
                foreach (var p in badTeam.GetPlayers())
                {
                    if (p.isLive())
                    {
                        winnerLive.Add(p);
                    }
                    else
                    {
                        winnerDead.Add(p);
                    }
                }
            }

            if (pirateTeam != winnerTeam)
            {
                loosers.AddRange(pirateTeam.GetPlayers());
            }
            else
            {
                foreach (var p in pirateTeam.GetPlayers())
                {
                    if (p.isLive())
                    {
                        winnerLive.Add(p);
                    }
                    else
                    {
                        winnerDead.Add(p);
                    }
                }
            }

            if(neutralTeam != winnerTeam)
            {
                loosers.AddRange(neutralTeam.GetPlayers());
            }
            else
            {
                foreach (var p in neutralTeam.GetPlayers())
                {
                    if (p.isLive())
                    {
                        winnerLive.Add(p);
                    }
                    else
                    {
                        loosers.Add(p);
                    }
                }
            }

            int roomCoin = room.players.Count * room.bet;

            int coinPrize_1 = 0;
            if (winnerLive.Count > 0) coinPrize_1 = (int)(roomCoin * 0.8f / winnerLive.Count);

            int coinPrize_2 = 0;
            if (winnerDead.Count > 0) coinPrize_2 = (int)(roomCoin * 0.15f / winnerDead.Count);

            int coinPrize_3 = 0;
            if (loosers.Count > 0) coinPrize_3 = (int)(roomCoin * 0.05f / loosers.Count);

            Logger.Log.Debug($"room Coin {roomCoin}");
            Logger.Log.Debug($"live win {winnerLive.Count} dead win {winnerDead.Count} loosers {loosers.Count}");
            Logger.Log.Debug($"prize 1 {coinPrize_1} 2 {coinPrize_2} 3 {coinPrize_3}");

           

            //награждаем живых победителей
            Logger.Log.Debug($" live winners {winnerLive.Count}");
            foreach (var p in winnerLive)
            {
                int rating = 0;
                int exp = 0;

                var status = EndGameResult.LiveWinner;

                var message = GetWinMessageByTeamAndStatus(p, status);

                //coinPrize_1              
                if (useDetailedResult) message += "\nБонусы:";

                AplyRoleToRatingAndExp(p ,ref rating,ref exp, status);

                AplyExtraToRatingAndExp(p, ref rating, ref exp, ref message, status);
                
                AplySkillToRating(p, ref rating, ref message, status);

                AplySkillToExp(p, ref exp, ref message, status);

                message += $"\nИтог:" +
                    $"\nРейтинг: {rating}" +
                    $"\nОпыт: {exp}" +
                    $"\nМонеты: {coinPrize_1}";

                SendToPlayerEndGameResult(p, message);
            }

            Logger.Log.Debug($" dead winners {winnerDead.Count}");
            foreach (var p in winnerDead)
            {
                int rating = 0;
                int exp = 0;

                var status = EndGameResult.DeadWinner;

                var message = GetWinMessageByTeamAndStatus(p, status);

                //coinPrize_1              
                if (useDetailedResult) message += "\nБонусы:";

                AplyRoleToRatingAndExp(p, ref rating, ref exp, status);

                AplyExtraToRatingAndExp(p, ref rating, ref exp, ref message, status);

                AplySkillToRating(p, ref rating, ref message, status);

                AplySkillToExp(p, ref exp, ref message, status);

                message += $"\nИтог:" +
                    $"\nРейтинг: {rating}" +
                    $"\nОпыт: {exp}" +
                    $"\nМонеты: {coinPrize_2}";

                SendToPlayerEndGameResult(p, message);
            }

            Logger.Log.Debug($" loosers {loosers.Count}");
            foreach (var p in loosers)
            {
                int rating = 0;
                int exp = 0;

                var status = EndGameResult.Looser;

                var message = GetWinMessageByTeamAndStatus(p, status);

                //coinPrize_1              
                if (useDetailedResult) message += "\nБонусы:";

                AplyRoleToRatingAndExp(p, ref rating, ref exp, status);

                AplyExtraToRatingAndExp(p, ref rating, ref exp, ref message, status);

                AplySkillToRating(p, ref rating, ref message, status);

                AplySkillToExp(p, ref exp, ref message, status);

                message += $"\nИтог:" +
                    $"\nРейтинг: {rating}" +
                    $"\nОпыт: {exp}" +
                    $"\nМонеты: {coinPrize_3}";

                SendToPlayerEndGameResult(p, message);
            }

            ManagerRooms.instance.DisposeRoom(room);
        }

        //private void StopRoom()
        //{
        //    //room.StopRoom();



        //    //reset players
        //    foreach (var p in room.players.Values)
        //    {
        //        p.room = null;
        //        p.targetPlayer = null;
        //        p.playerRole.roleEffects.StopChatTimer();
        //        //p.SetOldRole(null);
        //        //p.SetupRole(null);
        //        p.SetPlayerQueueStatus(PlayerQueueStatus.Idle);               
        //    }

        //    room.Dispose();
        //}

        private string GetWinMessageByTeamAndStatus(BasePlayer player, EndGameResult status)
        {
            var result = "";

            var playerInTeam = player.team != room.roomRoles.neutralTeam;          

            switch (status)
            {
                case EndGameResult.LiveWinner:
                    {
                        if (playerInTeam)
                        {
                            result = "Ваша команда победила и Вы выжили!";
                        }
                        else
                        {
                            result = "Вы победили!";
                        }
                    } break;
                case EndGameResult.DeadWinner:
                    {
                        result = "Ваша команда победила, но Вы погибли!";
                    } break;
                case EndGameResult.Looser: 
                    {
                        if (playerInTeam)
                        {
                            result = "Ваша команда проиграла.";
                        }
                        else
                        {
                            result = "Вы проиграли.";
                        }
                    } break;
            }

            return result;
        }

       

        private void CheckRoleAchievements()
        {
            foreach(var p in room.players.Values)
            {
                if(p.playerType != PlayerType.Player) continue;

                var roleType = p.playerRole.roleType;

                switch (roleType)
                {
                    case RoleType.Maniac: { IncreaseAchieveProgress(p, AchieveId.Role_ExpManiac);  } break;

                        default: { } break;
                }
            }
        }

        public void IncreaseAchieveProgress(BasePlayer player, AchieveId achieveId)
        {
           

            var userAchiveData = DBManager.Inst.LoadUserAchieve(player. client, achieveId);

            var currentLevel = 0;
            var currentExp = 0;

            if (userAchiveData.Count > 0)
            {                
                currentLevel = (int)userAchiveData[(byte)Params.AchieveCurrentLevel];
                currentExp = (int)userAchiveData[(byte)Params.AchieveCurrentExp];

                Logger.Log.Debug($"have user achieve {achieveId} data in db");
            }

            Logger.Log.Debug($"progress currentLevel {currentLevel} / currentExp {currentExp} ");

            var achiveData = DBManager.Inst.GetAchieveLevelData(achieveId, currentLevel + 1);

            Logger.Log.Debug($"achiveData {achieveId}  {achiveData.Count} / level {currentLevel + 1} ");

            var levelExp = 0;
            var levelReward = 0;

            //если у юзера еще нет такой ачивки
            if (achiveData.Count>0)
            {               
                levelExp = (int)achiveData[(byte)Params.AchieveLevelExp];
                levelReward=(int)achiveData[(byte)Params.AchieveLevelReward];

                Logger.Log.Debug($"have data for achieve {achieveId} in db");
            }
            else
            {
                Logger.Log.Debug($"user get max level in achieve {achieveId}");
                return;
            }

            Logger.Log.Debug($"achieve data  {currentLevel + 1} exp {levelExp} / rew {levelReward} ");

            currentExp += 1;

            var achieveMaxLevel = DBManager.Inst.GetAchieveMaxLevel(achieveId);
            Logger.Log.Debug($"achive {achieveId} max level => {achieveMaxLevel}");

            if (currentExp == levelExp)
            {
                currentLevel += 1;

                //if(currentLevel > achieveMaxLevel + 1) 
                //{
                //    Logger.Log.Debug($"user achive {achieveId} get max level");
                //    currentLevel = achieveMaxLevel; 
                //}

                currentExp = 0;

                Logger.Log.Debug($"user get {levelReward} rating for achieve lvlup");
            }

            Logger.Log.Debug($"increase progress currentLevel {currentLevel} / currentExp {currentExp} / levelExp {levelExp} / levelReward {levelReward}");

            DBManager.Inst.SaveUserAchieve(player. client, achieveId, currentLevel, currentExp);
        }

        private void SendToPlayerEndGameResult(BasePlayer player, string message)
        {
            if (player.playerType != PlayerType.Player) return;

            OperationResponse resp = new OperationResponse((byte)Request.EndGameResult);
            resp.Parameters = new Dictionary<byte, object>();

            resp.Parameters.Add((byte)Params.EndGameResult, message);

            player.client.SendOperationResponse(resp, Options.sendParameters);
        }

        public void AplyRoleToRatingAndExp(BasePlayer player,
            ref int rating, ref int exp,
            EndGameResult status)
        {
            //если игрок ранее покинул комнату - суицид
            if (player == null || player.playerRole == null) return;

            switch (player.playerRole.roleType)
            {
                case RoleType.Citizen: GetCitizenRaiting(ref rating, ref exp, status); break;

                case RoleType.Doctor:
                case RoleType.Guerilla:
                case RoleType.Saint:
                case RoleType.Witness:
                case RoleType.Commissar: GetActiveGoodRoleRaiting(ref rating, ref exp, status); break;

                case RoleType.Mafia:
                case RoleType.MafiaBoss:
                case RoleType.Sinner: GetActiveBadRoleRaiting(ref rating, ref exp, status); break;

                case RoleType.Maniac:
                case RoleType.Werewolf: GetActiveSoloRaiting(ref rating, ref exp, status); break;
            }
        }

        private void GetCitizenRaiting(ref int rating, ref int exp, EndGameResult status)
        {
            switch (status)
            {
                case EndGameResult.LiveWinner: rating += 10; exp += 20; break;
                case EndGameResult.DeadWinner: rating += 5; exp += 10; break;
                case EndGameResult.Looser: rating += -3; exp += 5; break;
            }
        }

        private void GetActiveGoodRoleRaiting(ref int rating, ref int exp, EndGameResult status)
        {           
            switch (status)
            {
                case EndGameResult.LiveWinner: rating += 20; exp += 30; break;
                case EndGameResult.DeadWinner: rating += 10; exp += 15; break;
                case EndGameResult.Looser: rating += -5; exp += 5; break;
            }
        }

        private void GetActiveBadRoleRaiting(ref int rating, ref int exp, EndGameResult status)
        {
            switch (status)
            {
                case EndGameResult.LiveWinner: rating += 30; exp += 40; break;
                case EndGameResult.DeadWinner: rating += 15; exp += 20; break;
                case EndGameResult.Looser: rating += -10; exp += 5; break;
            }
        }

        private void GetActiveSoloRaiting(ref int rating, ref int exp, EndGameResult status)
        {
            switch (status)
            {
                case EndGameResult.LiveWinner: rating += 50; exp += 50; break;
                case EndGameResult.Looser: rating += -10; exp += 5; break;
            }
        }

        public void AplyExtraToRatingAndExp(BasePlayer player, 
            ref int rating, ref int exp, ref string message,
             EndGameResult status)
        {
            int ratingBonus = 0;
            int expBonus = 0;
            var extraBonus = "";

            //экстра молния
            var lightningExtra = player.FindExtraInSlots(ExtraEffect.Lightning);
            if (lightningExtra != null)
            {
                ratingBonus += 10;
                if(useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Молния")} - рейтинг +{10}";
                lightningExtra.DecreaseCount();
            }

            //экстра колпак шута
            var foolsCapExtra = player.FindExtraInSlots(ExtraEffect.FoolsCap);
            if (foolsCapExtra != null)
            {
                ratingBonus += 15;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Колпак шута")} - рейтинг +{15}";
                foolsCapExtra.DecreaseCount();
            }

            //экстра попкорн
            var popCornExtra = player.FindExtraInSlots(ExtraEffect.PopCorn);
            if (popCornExtra != null)
            {
                expBonus += 15;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Попкорн")} - опыт +{15}";
                popCornExtra.DecreaseCount();
            }

            //экстра косморейт
            var cosmoRaitExtra = player.FindExtraInSlots(ExtraEffect.CosmoRait);
            if (cosmoRaitExtra != null)
            {
                ratingBonus += 30;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Косморейт")} - рейтинг +{30}";
                cosmoRaitExtra.DecreaseCount();
            }

            //лапка паука рейт и опыт
            var spiderPawExtra = player.FindExtraInSlots(ExtraEffect.SpiderPaw);
            if (spiderPawExtra != null)
            {
                ratingBonus += 15;
                expBonus += 15;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Лапка паука")} - рейтинг +{15} опыт +{15}";
                spiderPawExtra.DecreaseCount();
            }

            //крюк
            var hookExtra = player.FindExtraInSlots(ExtraEffect.Hook);
            if (hookExtra != null)
            {
                ratingBonus += 15;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Крюк")} - рейтинг +{15}";
                hookExtra.DecreaseCount();
            }

            //череп
            var skullExtra = player.FindExtraInSlots(ExtraEffect.Skull);
            if (skullExtra != null)
            {
                expBonus += 15;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Череп")} - опыт +{15}";
                skullExtra.DecreaseCount();
            }

            //игрушка
            var newYearToyExtra = player.FindExtraInSlots(ExtraEffect.NewYearToy);
            if (newYearToyExtra != null)
            {
                ratingBonus += 30;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Игрушка")} - рейтинг +{30}";
                newYearToyExtra.DecreaseCount();
            }

            //ландыш
            var liliExtra = player.FindExtraInSlots(ExtraEffect.Lili);
            if (liliExtra != null)
            {
                ratingBonus += 30;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Ландыш")} - рейтинг +{30}";
                liliExtra.DecreaseCount();
            }

            //тыква опыт
            var pumpkinExtra = player.FindExtraInSlots(ExtraEffect.Pumpkin);
            if (pumpkinExtra != null)
            {
                expBonus += 30;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Тыква")} - опыт +{30}";
                pumpkinExtra.DecreaseCount();
            }

            //золотое руно опыт и рейт
            var goldenFleeceExtra = player.FindExtraInSlots(ExtraEffect.GoldenFleece);
            if (goldenFleeceExtra != null)
            {
                ratingBonus += 10;
                if (useDetailedResult) extraBonus += $"\nЭкстра {ColorString.GetColoredExtra("Золотое руно")} - рейтинг +{10}";
            }

            if (useDetailedResult) message += extraBonus;
            rating += ratingBonus;
            exp += expBonus;
        }
   
        public void AplySkillToExp(BasePlayer player, ref int exp, ref string message, EndGameResult status)
        {
            if (status == EndGameResult.Looser) return;
            
            //опытный босс %
            if(player.playerRole.roleType == RoleType.MafiaBoss)
            {
                var mafiaBossRole = (MafiaBoss)player.playerRole;

                if (mafiaBossRole.Check_MafiaBossExperienced())
                {
                    var expBonusFactor = mafiaBossRole.skill_MafiaBossExperienced.skillValue;

                    var expBonus = exp / 100f * expBonusFactor;

                    exp += (int)expBonus;

                    if (useDetailedResult) message += $"\nCC {ColorString.GetColoredSkill("Опытный Босс")} - опыт +{expBonusFactor}% = {(int)expBonus}";
                }
            }
        }

        public void AplySkillToRating(BasePlayer player, ref int rating, ref string message, EndGameResult status)
        {
            if (status == EndGameResult.Looser) return;

            if(player == null)
            {
                Logger.Log.Debug($"{status} player is null");
                return;
            }

            if (player.playerRole == null)
            {
                Logger.Log.Debug($"{status} player role is null");
                return;
            }

            //повезло
            if (player.playerRole.roleType == RoleType.Saint)
            {
                var role = (Saint)player.playerRole;

                if (role.Check_SaintLucky())
                {
                    var ratingBonus = role.skill_SaintLucky.skillValue;

                    rating += ratingBonus;

                    if (useDetailedResult) message += $"\nCC {ColorString.GetColoredSkill("Повезло")} - рейтинг +{ratingBonus}";
                }
            }

            //золотой скальпель
            if (player.playerRole.roleType == RoleType.Doctor)
            {
                var role = (Doctor)player.playerRole;

                if (role.Check_DoctorEvilScalpel())
                {
                    var ratingBonus = role.skill_DoctorGoldScalpel.skillValue;

                    rating += ratingBonus;

                    if (useDetailedResult) message += $"\nCC {ColorString.GetColoredSkill("Золотой скальпель")} - рейтинг +{ratingBonus}";
                }
            }

            //награда за разговоры
            if (player.playerRole.roleType == RoleType.Guerilla)
            {
                var role = (Guerilla)player.playerRole;

                if (role.Check_GuerillaReward())
                {
                    var ratingBonus = role.skill_GuerillaReward.skillValue;

                    rating += ratingBonus;

                    if (useDetailedResult) message += $"\nCC {ColorString.GetColoredSkill("Награда за разговоры")} - рейтинг +{ratingBonus}";
                }
            }
        }
    }
}
