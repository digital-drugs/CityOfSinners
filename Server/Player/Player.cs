using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using EventData = Photon.SocketServer.EventData;

namespace Mafia_Server
{
    public class Player:BasePlayer
    {
        //Права
        public Dictionary<int, object> clanRights;

        public Dictionary<byte, object> playerClan;
        //задания

        //навыки

        //клан

        //итемы

        public Player(Client client):base (client)
        {         
            playerType = PlayerType.Player;

            playerId = client.userDbId;

            playerName = client.userName;

            SetPlayerQueueStatus(PlayerQueueStatus.Idle);

            clanRights = DBManager.Inst.GetUserClanRights(playerId);

            playerClan = DBManager.Inst.GetUserClan(playerId);

            //грузим из БД прогресс игрока

            //скиллы игрока из БД
        }

        public void LevelUpskill(Dictionary<byte, object> parameters)
        {
            var skillId = (string)parameters[(byte)Params.SkillId];

            //проверяем текущий уровень скилла игрока
            var currentSkillLevel = DBManager.Inst.LoadUserSkillLevel(skillId, client);

            var skillData = (Dictionary<byte, object>)SkillManager.rawSkillData[skillId];
            var skillLevels = (Dictionary<byte, object>)skillData[(byte)Params.Levels];

            int skillCost = 0;
            foreach(var level in skillLevels )
            {
                if( level.Key == currentSkillLevel + 1)
                {
                    var levelData = (Dictionary<byte, object>)level.Value;
                    skillCost = (int)levelData[(byte)Params.SkillCost];

                    Logger.Log.Debug($"user try lelel up skill {skillId}");
                    Logger.Log.Debug($"current user level {currentSkillLevel}");
                    Logger.Log.Debug($"skill level up cost {skillCost}");
                }
            }

            if (skillCost == 0)
            {
                Logger.Log.Debug($"cant level up skill with cost {skillCost}");
                return;
            }

            //проверить наличие средств у игрока для покупки скилла
            DBManager.Inst.SaveUserSkillLevel(skillId, currentSkillLevel+1, client);

            OperationResponse resp = new OperationResponse((byte)Request.LevelUpSkill);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.SkillId, skillId);
            resp.Parameters.Add((byte)Params.SkillLevel, currentSkillLevel+1);
            client.SendOperationResponse(resp, Options.sendParameters);
        }

        public void BuyExtra(Dictionary<byte, object> parameters)
        {
            var extraId = (string)parameters[(byte)Params.ExtraId];

            Logger.Log.Debug($"user start buy extra {extraId}");

            int extraCost = 0;
            int extraBuyCount = 0;
            DBManager.Inst.LoadExtraByExtraId(extraId,out extraCost, out extraBuyCount);

            //проверяем наличие средств у юзера

            //покупакем и сохраняем в бдщ новое кол-во экстр
            var userExtraCount = DBManager.Inst.LoadUserExtraCount(client, extraId);

            Logger.Log.Debug($"{extraId} user count {userExtraCount}");

            //увеличиваем счетчик экстр  
            Logger.Log.Debug($"{extraId} to buy count  {extraBuyCount}");
            userExtraCount += extraBuyCount;

            Logger.Log.Debug($"{extraId}user count {userExtraCount}");

            //обновляем в БД кол-во экстр юзера
            DBManager.Inst.SaveUserExtraCount(this, extraId, userExtraCount);

            //отправляем юзеру обновленное кол-во экстр
            OperationResponse resp = new OperationResponse((byte)Request.BuyExtra);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.ExtraId, extraId);
            resp.Parameters.Add((byte)Params.ExtraCount, userExtraCount);
            client.SendOperationResponse(resp, Options.sendParameters);
        }

        public void SaveUserExtraSlot(Dictionary<byte, object> parameters)
        {
            DBManager.Inst.SaveUserExtraSlot(client, parameters);
        }

        public void ClearUserExtraSlot(Dictionary<byte, object> parameters)
        {
            DBManager.Inst.ClearUserExtraSlot(client, parameters);
        }
    }
}
