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
    public class ManagerUser
    {
        public static ManagerUser instance;

        //список онлайн клиентов // ключ = id клиента из бд              


        public ManagerUser()
        {
            instance = this;

            onlineUsers = new Dictionary<long, Client>();

            Load_DbRoles();
        }

        Dictionary<string, object> dbRoles;
        private void Load_DbRoles()
        {
            dbRoles = DBManager.Inst.LoadRoles();
        }

        public void UserAuthorization(Client client, OperationRequest operationRequest, SendParameters sendParameters)
        {
            //получаем данные от юзера для авторизации
            var userLogin = (string)operationRequest.Parameters[(byte)Params.UserLogin];

            Logger.Log.Debug($"start login with login => {userLogin}");

            //получаем айди юзера и его ник из бд, если в ней есть такая запись
            DBManager.Inst.CheckUser(userLogin, client);

            //ответ юзеру
            OperationResponse resp = new OperationResponse((byte)Request.Login);

            resp.Parameters = new Dictionary<byte, object>();

            ///если dbId > 0 , тоюзер успешно авторизовался
            ///если <0 то такого юзера в дб нет, регистрируем нового
            if (client.userDbId < 0)
            {
                //прописать авторизацию 

                resp.ReturnCode = (short)ReturnCode.Fail;

                Logger.Log.Debug($"login FAIL => user  {userLogin} not found");

                client.userDbId = DBManager.Inst.RegisterUser(userLogin);

                //create default User Data
                DBManager.Inst.CreateUserData(client);

                Logger.Log.Debug($"Register new user => {client.userDbId}");

                //дефолтные настройки для нового юзера
            }
            else
            {
                resp.ReturnCode = (short)ReturnCode.Succes;
            }

            resp.Parameters.Add((byte)Params.UserId, client.userDbId);
            resp.Parameters.Add((byte)Params.UserName, client.userName);
            resp.Parameters.Add((byte)Params.UserSystemRole, client.systemRole);

            client.player = new Player(client);

            //данные игрока // коины // даймонды // слоты для экстр и пр.
            SetupUserData(resp, client);

            //все скиллы в игре и скиллы игрока
            SetupUserSkills(resp, client);

            //все экстры в игре и экстры игрока //и слоты игрока для экстр в игровой комнате
            SetupUserExtras(resp, client);         
            
            //все достижения в игре // и достижения игрока
            SetupUserAchieves(resp, client);

            SetupUserRoles(resp, client);

            Logger.Log.Debug($"login SUCCES => user {userLogin} id {client.userDbId}  ");

            client.SendOperationResponse(resp, sendParameters);

            AddOnlineUser(client.userDbId, client);
        }

        private void SetupUserRoles(OperationResponse resp, Client client)
        {
            resp.Parameters.Add((byte)Params.Roles, dbRoles);           
        }

        private void SetupUserSkills(OperationResponse resp, Client client)
        {
            var DbSkills = DBManager.Inst.LoadSkills();
            resp.Parameters.Add((byte)Params.Skills, DbSkills);

            var userSkills = DBManager.Inst.LoadUserSkills(client);
            foreach (var skill in userSkills)
            {
                var skillData = (Dictionary<byte, object>)DbSkills[skill.Key];
                skillData.Add((byte)Params.UserSkillLevel, skill.Value);
            }
        }

        private void SetupUserExtras(OperationResponse resp, Client client)
        {
            var DbExtras = DBManager.Inst.LoadExtras();
            resp.Parameters.Add((byte)Params.Extras, DbExtras);

            var userExtras = DBManager.Inst.LoadUserExtras(client);
            foreach (var extra in userExtras)
            {
                var extraData = (Dictionary<byte, object>)DbExtras[extra.Key];
                extraData.Add((byte)Params.ExtraCount, extra.Value);
            }

            //слоты юзера для экстр в игре
            SetupUserExtraSlots(resp, client);
        }

        private void SetupUserData(OperationResponse resp, Client client)
        {
            var userData = DBManager.Inst.LoadUserData(client);

            //client.player
            //result.Add((byte)Params.SlotCount, (int)Reader["slotCount"]);

            resp.Parameters.Add((byte)Params.UserData, userData);            
        }

        private void SetupUserAchieves(OperationResponse resp, Client client)
        {
            var dbAchieves = DBManager.Inst.LoadAchieves();
            resp.Parameters.Add((byte)Params.Achieves, dbAchieves);

            var userAchieves = DBManager.Inst.LoadUserAchieves(client);
            foreach (var a in userAchieves)
            {
                var achieveData = (Dictionary<byte, object>)dbAchieves[a.Key];

                var userAchiveData = (Dictionary<byte, object>)a.Value;

                achieveData.Add((byte)Params.AchieveCurrentLevel, (int)userAchiveData[(byte)Params.AchieveCurrentLevel]);
                achieveData.Add((byte)Params.AchieveCurrentExp, (int)userAchiveData[(byte)Params.AchieveCurrentExp]);
            }
        }

        private void SetupUserExtraSlots(OperationResponse resp, Client client)
        {
            var extraSlots = DBManager.Inst.LoadUserExtraSlots(client);
            resp.Parameters.Add((byte)Params.ExtraSlots, extraSlots);
        }

        public Dictionary<long, Client> onlineUsers { get; private set; }
        public void AddOnlineUser(long UserDbId, Client User)
        {
            if (onlineUsers.ContainsKey(UserDbId))
            {
                Logger.Log.Debug($"cant add user id {UserDbId} to online list, user already online");
                return;
            }

            onlineUsers.Add(UserDbId, User);

            Logger.Log.Debug($"user id {UserDbId} now ONline. count {onlineUsers.Count}");
        }

        public void RemoveOnlineUser(long UserDbId)
        {
            if (!onlineUsers.ContainsKey(UserDbId))
            {
                Logger.Log.Debug($"cant remove user id {UserDbId} from online list, user already offline");
                return;
            }

            onlineUsers.Remove(UserDbId);

            Logger.Log.Debug($"user id {UserDbId} now OFFline. count {onlineUsers.Count}");
        }

        public Client FindUser(int TargetId)
        {
            Client Result = null;

            if (onlineUsers.ContainsKey(TargetId))
            {
                Result = onlineUsers[TargetId];
            }

            return Result;
        }
    }
}
