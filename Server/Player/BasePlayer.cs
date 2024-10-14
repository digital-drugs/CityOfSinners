using Mafia_Server.Extras;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public enum PlayerGameStatus
    {
        Live, 
        Dead,
    }

    public class BasePlayer
    {
        public Client client;
        public BasePlayer(Client client=null)
        {
            this.client = client;
        }

        private Lobby lobby;
        public Lobby GetLobby() { return lobby; }
        public void SetLobby(Lobby lobby) { this.lobby = lobby; }

        private Room room;
        public Room GetRoom() { return room; }
        public void SetRoom(Room room) { this.room = room; }

        public PlayerType playerType;

        public PlayerQueueStatus queueStatus { get; private set; }
        public void SetPlayerQueueStatus(PlayerQueueStatus status)
        {
            this.queueStatus = status;

            switch(status)
            {

            }

            if (playerType != PlayerType.Player) return;

            OperationResponse resp = new OperationResponse((byte)Request.PlayerStatus);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.PlayerStatus, status);
            
            client.SendOperationResponse(resp, Options.sendParameters);
        }

        public PlayerGameStatus gameStatus { get; private set; }

       

        public void SetPlayerGameStatus(PlayerGameStatus status)
        {
            gameStatus = status;
        }

        public bool isLive()
        {
            return gameStatus == PlayerGameStatus.Live;
        }

        public string playerName;
        public long playerId;

        public BasePlayer targetPlayer;
        //public long currentTarget = -1;

       

        public Team team { get; private set; }  
        public void SetTeam(Team team)
        {
            this.team = team;
        }

        public Role oldRole { get; private set; }
        public void SetOldRole(Role role)
        {
            oldRole = role;
        }

        public Role playerRole { get; private set; }
        public void SetupRole(Role role)
        {
            playerRole = role;

            if (role == null) return;

            role.SetPlayer(this);

            SetupRoleEffect();
        }

        public Role_Effect roleEffect;
        private void SetupRoleEffect()
        {
            roleEffect = Helper.GetEffectByRoleType(playerRole.roleType);
            roleEffect.effectOwner = playerId;
        }

        public Role_Effect CurrentEffect;            

        public Dictionary<int, Extra> inGameSlots { get; private set; } = new Dictionary<int, Extra>();
        public void SetupGameExtraSlots(Dictionary<int, object> extraSlotsData)
        {
            inGameSlots.Clear();

            foreach (var e in extraSlotsData)
            {
                var extraData = (Dictionary<byte, object>)e.Value;

                //создаем новыую экстру и настраиваем в ней current count, который будет передан на клиент
                var extra = new Extra(this, e.Key, extraData);

                inGameSlots.Add(e.Key, extra);              
            }

            if(playerType== PlayerType.Player)
            {
                //узнаем доступное кол-во слотов

                var userData = DBManager.Inst.LoadUserData(client);
                var slotCount = (int)userData[(byte)Params.SlotCount];

                //отправляем игроку слоты с экстрами
                OperationResponse resp = new OperationResponse((byte)Request.CreateGameSlots);
                resp.Parameters = new Dictionary<byte, object>();
                resp.Parameters.Add((byte)Params.SlotCount, slotCount );
                resp.Parameters.Add((byte)Params.ExtraSlots, extraSlotsData);
                client.SendOperationResponse(resp, Options.sendParameters);
            }        
        }

        /// <summary>
        /// находит экстру в "инвентаре" игрока, если таковая имеется и кол-во использований в текущей партии > 0
        /// </summary>
        public Extra FindExtraInSlots(ExtraEffect effect)
        {
            return FindExtraInSlots(effect.ToString());
        }

        /// <summary>
        /// находит экстру в "инвентаре" игрока, если таковая имеется и кол-во использований в текущей партии > 0
        /// </summary>
        public Extra FindExtraInSlots(string extraId)
        {
            foreach (var e in inGameSlots.Values)
            {
                //Logger.Log.Debug();

                if (extraId == e. extraId)                 
                {      
                    if (e. currentCount > 0) return e;
                }
            }

            return null;
        }

        public void UseExtra(Dictionary<byte, object> parameters)
        {
            var slotId = (int)parameters[(byte)Params.SlotId];

            UseExtra(slotId, parameters);
        }


        public void UseExtra(int slotId, Dictionary<byte, object> parameters=null)
        {
            //проверка на соответствие экстры к фазе комнаты (если есть ограничение на время активации экстры)

            //проверка на то, что игрок жив

            //проверяем может ли игрок использовать экстры
            var azotExtra =  playerRole.roleEffects. FindExtraEffect(ExtraEffect.Hidrogen);
            if(azotExtra != null)
            {
                return;
            }

            OnAction?.Invoke(this, EventArgs.Empty);

            if (!inGameSlots.ContainsKey(slotId))
            {
                Logger.Log.Debug($"{playerName} dont have slot id {slotId}");
                return;
            }

            var extra = inGameSlots[slotId];

            //проверить можно ли использовать экстру в текущую фазу игры
            if(room.roomPhases.gamePhase == GamePhase.Judging)
            {
                //Logger.Log.Debug($"cant use extras in judging");
                return;
            }

            if (room.roomPhases.gamePhase == GamePhase.Day)
            {
                if (extra.gamePhase != GamePhase.Day && extra.gamePhase != GamePhase.Any)
                {
                    //Logger.Log.Debug($"cant use extra {extra.extraId} in DAY");
                    return;
                }
            }

            if (room.roomPhases.gamePhase == GamePhase.Night)
            {
                if (extra.gamePhase != GamePhase.Night && extra.gamePhase != GamePhase.Any)
                {
                    //Logger.Log.Debug($"cant use extra {extra.extraId} in NIGHT");
                    return;
                }
            }

            if (room.roomPhases.gamePhase == GamePhase.FirstNight)
            {
                if (extra.gamePhase != GamePhase.Any)
                {
                    //Logger.Log.Debug($"cant use extra {extra.extraId} in FIRST NIGHT");
                    return;
                }
            }

            //после смерит можно использовать только гранату
            if(!isLive() && extra.effect != ExtraEffect.Grenade)
            {
                return;
            }

            //проверяем, можно ли использовать экстру
            if (extra.currentCount > 0)
            {
                var extraUsed = room.extraHelper.UseExtra(this, extra, parameters);

                if (extraUsed)
                {
                    extra.DecreaseCount();
                }
            }

            room.roomEndGame.CheckEndGame2();

        }

        public void UseExtra(Extra extra)
        {
           
        }

        public void ResetTarget()
        {
            targetPlayer = null;
        }

        public event EventHandler OnAction;
        public void SelectPlayer(Dictionary<byte, object> parameters)
        {
            OnAction?.Invoke(this, EventArgs.Empty);

            room.roomLogic.SelectPlayer(this, parameters);
        }

        public event EventHandler OnChatMessage;
        public void SendChatMessage(Dictionary<byte, object> parameters)
        {        
            var chatType = (ChatType)parameters[(byte)Params.ChatType];

            switch (chatType)
            {
                case ChatType.RoomChat: { SendRoomMeesage(parameters); } break;
                case ChatType.PrivateChat: {  } break;
                case ChatType.GlobalChat: {  } break;
            }
        }

        private void SendRoomMeesage(Dictionary<byte, object> parameters)
        {
            if (isLive() == false)
            {
                room.roomChat.PersonalMessage(this, "Вы мертвы и не можете отправлять сообщения");
                return;
            }

            OnChatMessage?.Invoke(this, EventArgs.Empty);

            playerRole.roleEffects.UpdateChatTimer();

            var message = (string)parameters[(byte)Params.ChatMessage];

            var chatId = (ChatId)parameters[(byte)Params.ChatId];

            Logger.Log.Debug($"  chat id => {chatId}");

            if(chatId == ChatId.Private)
            {
                var targetPlayerId = (long)parameters[(byte)Params.UserId];

                if (room.players.ContainsKey(targetPlayerId))
                {
                    var targetPlayer = room.players[targetPlayerId];

                    if (targetPlayer.client != null)
                    {
                        OperationResponse resp = new OperationResponse((byte)Request.PrivateChatMessage);
                        resp.Parameters = new Dictionary<byte, object> { };

                        resp.Parameters.Add((byte)Params.ChatType, ChatType.PrivateChat);

                        resp.Parameters.Add((byte)Params.OwnerId, playerId);
                        resp.Parameters.Add((byte)Params.FromId, playerId);
                        resp.Parameters.Add((byte)Params.ToId, targetPlayerId);

                        resp.Parameters.Add((byte)Params.UserName, playerName);
                        resp.Parameters.Add((byte)Params.ChatMessage, message);

                        targetPlayer.client.SendOperationResponse(resp, Options.sendParameters);

                        client.SendOperationResponse(resp, Options.sendParameters);
                    }
                }
            }
            else
            {
                var chat = room.roomChat.chats[chatId];

                EventData eventData = new EventData((byte)Events.RoomChatMessage);
                eventData.Parameters = new Dictionary<byte, object>();

                eventData.Parameters.Add((byte)Params.ChatType, ChatType.RoomChat);
                eventData.Parameters.Add((byte)Params.ChatId, chatId);

                eventData.Parameters.Add((byte)Params.OwnerId, playerId);
                eventData.Parameters.Add((byte)Params.UserName, playerName);

                eventData.Parameters.Add((byte)Params.ChatMessage, message);

                eventData.SendTo(chat.clients, Options.sendParameters);
            }
            
        }

        //public void SendChatMessage(
        //    string message, 
        //    ChatId chatId, 
        //    Dictionary<byte, object> parameters, 
        //    ChatType chatType = ChatType.GlobalChat )
        //{
        //    OnChatMessage?.Invoke(this, EventArgs.Empty);

        //    switch (chatType)
        //    {
        //        case ChatType.GlobalChat:
        //            {
        //                EventData eventData = new EventData((byte)Events.GeneralChatMessage);
        //                eventData.Parameters = new Dictionary<byte, object> { };

        //                eventData.Parameters.Add((byte)Params.UserId, playerId);
        //                eventData.Parameters.Add((byte)Params.UserName, playerName);
        //                eventData.Parameters.Add((byte)Params.ChatMessage, message);

        //                eventData.Parameters.Add((byte)Params.ChatType, ChatType.GlobalChat);

        //                SendParameters sendParameters = new SendParameters();

        //                var chatUsers = ManagerUser.instance.onlineUsers.Values;
        //                eventData.SendTo(chatUsers, sendParameters);
        //            }
        //            break;

        //        //case ChatType.PrivateChat:
        //        //    {
        //        //        var targetPlayerId = (long)parameters[(byte)Params.UserId];

        //        //        var targetPlayer = room.players[targetPlayerId];

        //        //        if (targetPlayer.playerType != PlayerType.Player) return;

        //        //        OperationResponse resp = new OperationResponse((byte)Request.PrivateChatMessage);
        //        //        resp.Parameters = new Dictionary<byte, object> { };
        //        //        resp.Parameters.Add((byte)Params.ChatType, ChatType.PrivateChat);

        //        //        resp.Parameters.Add((byte)Params.OwnerId, playerId);
        //        //        resp.Parameters.Add((byte)Params.FromId, playerId);
        //        //        resp.Parameters.Add((byte)Params.ToId, targetPlayerId);

        //        //        resp.Parameters.Add((byte)Params.UserName, playerName);
        //        //        resp.Parameters.Add((byte)Params.ChatMessage, message);
        //        //        targetPlayer.client.SendOperationResponse(resp, Options.sendParameters);
        //        //        client.SendOperationResponse(resp, Options.sendParameters);
        //        //    }
        //        //    break;
        //    }
        //}

        public void SendAddExtraEffectToExtraOwner(IExtraHandler extraHandler)
        {
            if (extraHandler.GetExtra() .extraId == ExtraEffect.SleepElexir.ToString())
            {
                return;
            }

            var extraOwner = extraHandler.GetExtra().owner;

            if (extraOwner.playerType != PlayerType.Player) return;

            OperationResponse resp = new OperationResponse((byte)Request.AddExtraEffectToPlayer);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.UserId, playerId);

            resp.Parameters.Add((byte)Params.ExtraEffectId, extraHandler.GetEffectId());
            resp.Parameters.Add((byte)Params.ExtraId, extraHandler.GetExtra().extraId);

            var extraOwnerClient = extraOwner.client;

            extraOwnerClient.SendOperationResponse(resp, Options.sendParameters);
        }

        public void SendRemoveExtraEffectToExtraOwner(IExtraHandler extraHandler)
        {
            var extraOwner = extraHandler.GetExtra().owner;

            if (extraOwner.playerType != PlayerType.Player) return;

            OperationResponse resp = new OperationResponse((byte)Request.RemoveExtraEffectFromPlayer);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.UserId, playerId);

            resp.Parameters.Add((byte)Params.ExtraEffectId, extraHandler.GetEffectId());
            resp.Parameters.Add((byte)Params.ExtraId, extraHandler.GetExtra().extraId);

            var extraOwnerClient = extraOwner.client;

            extraOwnerClient.SendOperationResponse(resp, Options.sendParameters);
        }

       

        #region Judge
        private bool canJudge = true;
        public bool CanJudje()
        {
            return canJudge;
        }

        public void EnableJudge()
        {
            canJudge = true;
        }

        public void DisableJudge()
        {
            canJudge = false;
        }

        public int voteCount { get; private set; } = 0;
        public void AddVoteCount(int value)
        {
            this.voteCount += value;
        }

        public void ResetPlayerVoteCount()
        {
            voteCount = 0;
        }

        #endregion

        #region reusable voids

        public string GetColoredName()
        {
            var result = playerName;

            result = ColorString.GetColoredString(result, ColorId.Player);

            return result;
        }

        public string GetColoredRole()
        {
            var roleRus = Helper.GetRoleNameById_Rus(playerRole.roleType);

            var result = ColorString.GetColoredString(roleRus, ColorId.Role);

            return result;
        }
        #endregion

        public void ResetPlayer_StartGame()
        {
            sharedActions = new Dictionary<int, Action>();

            SetPlayerGameStatus(PlayerGameStatus.Live);

            SetPlayerQueueStatus(PlayerQueueStatus.InGame);

            playerRole = null;
            oldRole = null;
        }

        public void ResetPlayer_EndNight()
        {
            targetPlayer = null;
            oldRole = null;
        }

        private Dictionary<int, Action> sharedActions = new Dictionary<int, Action>();
        public int AddShareAction(Action action)
        {
            var messageId = room.GetShareMessageId();
            sharedActions.Add(messageId, action);

            return messageId;
        }

        public void UseShareAction(Dictionary<byte, object> parameters)
        {
            var actionId = (int)parameters[(byte)Params.messageId];            

            if (sharedActions.ContainsKey(actionId))
            {
                Action action = sharedActions[actionId];

                //room.roomChat.PublicMessage(message);

                action();
            }           
        }

        public BasePlayer killer { get; private set; }
        public void SetKiller(BasePlayer killer)
        {
            this.killer = killer;
        }

        public bool wasActiveAtNight { get; private set; } = false;
        public void SetActiveAtNight(bool active)
        {
            wasActiveAtNight = active;
        }
    }
}
