using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Extra
    {
        public string extraId { get; private set; }

        public ExtraEffect effect { get; private set; }

        private string extraName;

        public DurationType durationType;

        public GamePhase gamePhase { get; private set; }

        public int count { get; private set; }
        public int currentCount { get; private set; }
        public int slotId { get; private set; }

        public ExtraType extraType { get; private set; }

        public BasePlayer owner { get; private set; }
        public Extra(BasePlayer owner, int slotId, Dictionary<byte,object> extraData)        
        {
            this.owner = owner;

            this.slotId = slotId;

            extraId = (string)extraData[(byte)Params.ExtraId];

            extraName = (string)extraData[(byte)Params.ExtraName];

            effect = (ExtraEffect)Helper.GetEnumElement<ExtraEffect>(extraId);

            durationType = Helper.GetExtraDurationById(extraId);

            count = (int)extraData[(byte)Params.ExtraCount];
            var countForGame = (int)extraData[(byte)Params.ExtraGameCount];

            var extraTypeString = (string)extraData[(byte)Params.ExtraType];
            extraType = (ExtraType)Helper.GetEnumElement<ExtraType>(extraTypeString);

            var extraPhaseString = (string)extraData[(byte)Params.GamePhase];

            if(string.IsNullOrEmpty(extraPhaseString))
            {
                gamePhase = GamePhase.Any;
            }
            else
            {
                gamePhase = (GamePhase)Helper.GetEnumElement<GamePhase>(extraPhaseString);
            }

            if(owner.playerType == PlayerType.Player)
            {
                //+1 оборотень
                if (owner.playerRole.roleType == RoleType.Werewolf && effect == ExtraEffect.AirPlane)
                {
                    var role = (Werewolf)owner.playerRole;

                    if (role.Check_WerewolfOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 гражданин
                if (owner.playerRole.roleType == RoleType.Citizen && effect == ExtraEffect.Cape)
                {
                    var role = (Citizen)owner.playerRole;

                    if (role.Check_CitizenOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 босс мафии
                if (owner.playerRole.roleType == RoleType.MafiaBoss && effect == ExtraEffect.Mine)
                {
                    var role = (MafiaBoss)owner.playerRole;

                    if (role.Check_MafiaBossOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 грешник
                if (owner.playerRole.roleType == RoleType.Sinner && effect == ExtraEffect.Eye)
                {
                    var role = (Sinner)owner.playerRole;

                    if (role.Check_SinnerOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 маньяк
                if (owner.playerRole.roleType == RoleType.Maniac && effect == ExtraEffect.Gun)
                {
                    var role = (Maniac)owner.playerRole;

                    if (role.Check_ManiacOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 свидетель
                if (owner.playerRole.roleType == RoleType.Witness && effect == ExtraEffect.Explosion)
                {
                    var role = (Witness)owner.playerRole;

                    if (role.Check_WitnessOneMore())
                    {
                        countForGame += 1;
                    }
                }

                //+1 комиссар
                if (owner.playerRole.roleType == RoleType.Commissar && effect == ExtraEffect.Cuff)
                {
                    var role = (Commissar)owner.playerRole;

                    if (role.Check_CommissarOneMore())
                    {
                        countForGame += 1;
                    }
                }
            }                       

            //запоминаем сколько раз игрок может использовать эту экстру за игру
            currentCount = 0;

            if (count < countForGame)
                currentCount = count;
            else 
                currentCount = countForGame;

            extraData.Add((byte)Params.ExtraCurrentCount, currentCount);
        }

        public void SetUnlimitCurrentCount()
        {
            currentCount = count;

            SendExtraCount();
        }

        public void DecreaseCount()
        {           
            count -= 1;

            currentCount -= 1;

            Logger.Log.Debug($"decrease extra {extraId} current count {currentCount}");

            if (owner.playerType == PlayerType.Player)
            {
                //отправляем сообщение в чат
                //var extraString = ColorString.GetColoredString(extraName, ColorId.Extra);

                //owner.room.roomChat.PublicMessage_UseExtra(this, $"Вы использовали экстру {extraString}");
                //owner.room.roomChat.PersonalMessage_UseExtra(owner,this, $"Вы использовали экстру {extraString}");

                //уменьшаем счетчик экстры 
               
                DBManager.Inst.SaveUserExtraCount(owner, extraId, count);

                //отправляем новый счетчик на клиент
                SendExtraCount();
            }
        }

        public void SendExtraCount()
        {
            if (owner.playerType != PlayerType.Player) return;           

            OperationResponse resp = new OperationResponse((byte)Request.ExtraGameCount);
            resp.Parameters = new Dictionary<byte, object>();
            resp.Parameters.Add((byte)Params.SlotId, slotId);
            resp.Parameters.Add((byte)Params.ExtraCurrentCount, currentCount);
            owner. client.SendOperationResponse(resp, Options.sendParameters);
        }
    }
}
