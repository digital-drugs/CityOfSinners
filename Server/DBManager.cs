using System;
using System.Collections.Generic;
using Share;
using MySql.Data.MySqlClient;
using System.Web.UI.WebControls;
using System.Linq;
using MySqlX.XDevAPI.Common;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.Web.Security;
using BEPUutilities;
using log4net.Core;
using System.Text.RegularExpressions;
using System.Globalization;
using BEPUphysics.Constraints.TwoEntity.Joints;
using System.Security.Claims;
using Photon.SocketServer.Diagnostics;
using System.Data;
using System.Xml.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Mafia_Server
{
    public class DBManager
    {
        public static DBManager Inst;

        public DBManager()
        {
            Inst = this;
        }

        public MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            String connString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;
            Console.WriteLine(database);
            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }

        public MySqlConnection GetDBConnection()
        {
            string host = DBOptions.host;
            int port = DBOptions.port;
            string database = DBOptions.database;
            string username = DBOptions.username;
            string password = DBOptions.password;

            // Connection String.
            String connString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;
            //////logger.log.debug("connect to " + database);
            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }

        #region Autorization / registration

        /// <summary>
        /// авторизация юзера
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="userPassword"></param>
        /// <returns></returns>
        public void CheckUser(string userLogin, Client client)
        {
            //id = -1;
            //name = null;
            //permission = "user";
            // out long id, out string name, out string permission

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `Users` WHERE login = '{userLogin}'";

            try
            {
                Conn.Open();
                ////logger.log.debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            if (Reader.HasRows)
            {
                ////logger.log.debug("Login " + Login + " found =>" + Password);

                while (Reader.Read())
                {
                    client.userDbId = (long)Reader["id"];
                    client.userName = (string)Reader["name"];
                    var systemRole = (string)Reader["systemRole"];
                    client.SetSystemRole(systemRole);
                    ////logger.log.debug("Login " + Login + " NOT found =>" + Password + " ID " + result);
                }
            }
            else
            {
                ////logger.log.debug("Login " + Login + " NOT found =>" + Password);
            }

            Conn.Close();
        }

        public long RegisterUser(string login)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"INSERT INTO `Users` " +
                $" (`login`) " +
                $" VALUES('{login}')";

            //Logger.Log.Debug($"user registration data {Comm.CommandText}");

            //Logger.Log.Debug($"Comm.CommandText {Comm.CommandText}");

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            Comm.ExecuteNonQuery();

            var Result = Comm.LastInsertedId;

            Conn.Close();

            return Result;
        }

        public long SetUserName(long id, string name)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"UPDATE `Users` SET `name`= '{name}' WHERE `id`= {id}";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
            }

            Comm.ExecuteNonQuery();

            var Result = Comm.LastInsertedId;

            Conn.Close();

            return Result;
        }

        public void CreateUserData(Client client)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
               $"INSERT INTO `userdata`" +
               $" (userId , coins, diamonds, slotCount)" +
               $" VALUES" +
               $" ('{client.userDbId}', '0', '0', '10')";
            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            Comm.ExecuteNonQuery();

            Conn.Close();

            Logger.Log.Debug($"create start Data for user => {client.userDbId}");
        }

        public void SaveUserCoin()
        {

        }

        public void SaveUserExtraSlot(Client client, Dictionary<byte, object> parameters)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var slotId = (int)parameters[(byte)Params.SlotId];
            var extraId = (string)parameters[(byte)Params.ExtraId];

            var userExtraCount = LoadUserExtraCount(client, extraId);

            if (userExtraCount == 0)
            {
                SaveUserExtraCount(client.player, extraId, 0);
            }

            var clearString = $"DELETE FROM `userslots`" +
                $" WHERE `slotId` = '{slotId}' AND `ownerId` = '{client.userDbId}'";

            var saveString = $"INSERT INTO `userslots`" +
                $" (slotId, ownerId, extraId)" +
                $" VALUES" +
                $" ('{slotId}','{client.userDbId}','{extraId}')";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }
            Comm.CommandText = clearString;
            Comm.ExecuteNonQuery();

            Comm.CommandText = saveString;
            Comm.ExecuteNonQuery();

            Conn.Close();
        }

        public void ClearUserExtraSlot(Client client, Dictionary<byte, object> parameters)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var slotId = (int)parameters[(byte)Params.SlotId];

            var clearString = $"DELETE FROM `userslots`" +
                $" WHERE `slotId` = '{slotId}' AND `ownerId` = '{client.userDbId}'";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }
            Comm.CommandText = clearString;
            Comm.ExecuteNonQuery();

            Conn.Close();
        }

        public Dictionary<byte, object> LoadUserData(Client client)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `userdata` " +
                $" WHERE `userId` = '{client.userDbId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<byte, object>();

            while (Reader.Read())
            {
                result.Add((byte)Params.Coins, (int)Reader["coins"]);
                result.Add((byte)Params.Diamonds, (int)Reader["diamonds"]);
                result.Add((byte)Params.SlotCount, (int)Reader["slotCount"]);
            }

            Conn.Close();

            Logger.Log.Debug($"load user data => {result.Count}");

            return result;
        }

        public Dictionary<int, string> LoadUserExtraSlots(Client client)
        {
            var result = new Dictionary<int, string>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `userslots` " +
                $" WHERE `ownerId` = '{client.userDbId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var slotId = (int)Reader["slotId"];
                var extraId = (string)Reader["extraId"];

                result.Add(slotId, extraId);
            }

            Conn.Close();

            Logger.Log.Debug($"load user data => {result.Count}");

            return result;
        }

        public Dictionary<int, object> LoadUserExtraSlotsForGame(Client client)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `userslots`  " +
                $" LEFT JOIN `userextras` ON `userextras`.`extraId` = `userslots`.`extraId` AND `userextras`.`ownerId` = `userslots`.`ownerId`" +
                $" LEFT JOIN `extras` ON `extras`.`extraId` = `userslots`.`extraId`" +
                $" WHERE `userslots`.`ownerId` = '{client.userDbId}'" +
                $" ORDER BY `userslots`.`slotId`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var extraData = new Dictionary<byte, object>();

                var slotId = (int)Reader["slotId"];

                extraData.Add((byte)Params.ExtraId, (string)Reader["extraId"]);
                extraData.Add((byte)Params.ExtraName, (string)Reader["extraName"]);
                extraData.Add((byte)Params.ExtraCount, (int)Reader["extraCount"]);
                extraData.Add((byte)Params.ExtraGameCount, (int)Reader["extraGameCount"]);
                extraData.Add((byte)Params.ExtraUseType, (string)Reader["extraUseType"]);
                extraData.Add((byte)Params.ExtraType, (string)Reader["extraType"]);
                extraData.Add((byte)Params.GamePhase, (string)Reader["extraPhase"]);

                result.Add(slotId, extraData);
            }

            Conn.Close();

            //Logger.Log.Debug($"load user data => {result.Count}");

            return result;
        }

        #endregion

        #region Skills

        public event EventHandler OnSkillsChange;
        public void SaveSkill(Dictionary<byte, object> skillData)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var roleId = (string)skillData[(byte)Params.RoleId];
            var skillId = (string)skillData[(byte)Params.SkillId];
            var skillName = (string)skillData[(byte)Params.SkillName];
            var skillDescription = (string)skillData[(byte)Params.SkillDescription];
            var imageUrl = (string)skillData[(byte)Params.SkillUrl];

            var skillString = $"INSERT INTO `skills` (skillId, skillName, skillDescription, roleId, imageUrl) " +
               $" VALUES('{skillId}', '{skillName}', '{skillDescription}', '{roleId}', '{imageUrl}') " +
               $" ON DUPLICATE KEY UPDATE " +
               $" skillName = '{skillName}'," +
               $" skillDescription = '{skillDescription}'," +
               $" roleId = '{roleId}'," +
               $" imageUrl = '{imageUrl}'";



            var clearSkillLevel = $"DELETE FROM `skillslevels` WHERE `skillId` = '{skillId}'";

            var saveSkillLevel = $"INSERT INTO `skillslevels` (skillId, skillLevel, skillValue, skillCost)" +
                $" VALUES";
            var skillLevelsData = (Dictionary<byte, object>)skillData[(byte)Params.Level];
            for (int i = 0; i < skillLevelsData.Count; i++)
            {
                var skillLevelData = (Dictionary<byte, object>)skillLevelsData.ElementAt(i).Value;

                var skillLevel = (byte)skillLevelData[(byte)Params.SkillLevel];
                var skillCost = (int)skillLevelData[(byte)Params.SkillCost];
                var skillChance = (byte)skillLevelData[(byte)Params.SkillValue];

                if (i > 0) saveSkillLevel += ",";

                saveSkillLevel += $" ('{skillId}',{skillLevel},{skillChance},{skillCost})";
            }

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            Comm.CommandText = skillString;
            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save skill => {Result}");

            Comm.CommandText = clearSkillLevel;
            Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"clear skill level => {Result}");

            if (skillLevelsData.Count > 0)
            {
                Comm.CommandText = saveSkillLevel;
                Result = Comm.ExecuteNonQuery();
            }

            Logger.Log.Debug($"save skill level => {Result}");

            Conn.Close();

            OnSkillsChange?.Invoke(this, EventArgs.Empty);
        }

        public void SaveUserSkillLevel(string skillId, int skillLevel, Client client)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var clearString =
                $"DELETE FROM userskills " +
                $"WHERE skillId = '{skillId}' AND skillOwner = {client.userDbId}";

            var saveString = $"INSERT INTO `userskills` " +
                $"(`skillId`, `skillLevel`, `skillOwner`) " +
                $"VALUES ('{skillId}', '{skillLevel}', '{client.userDbId}')";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            Comm.CommandText = clearString;
            Comm.ExecuteNonQuery();

            Comm.CommandText = saveString;
            Comm.ExecuteNonQuery();

            Conn.Close();
        }

        public Dictionary<string, object> LoadSkills()
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `skills` " +
                "LEFT JOIN `skillslevels` ON `skillslevels`.`skillId` = `skills`.`skillId`" +
                " ORDER BY `skills`.`skillId`, `skillslevels`.`skillLevel`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var imageUrl = DBOptions.workDomen + DBOptions.skillsUrl;

            var result = new Dictionary<string, object>();

            while (Reader.Read())
            {
                var skillId = (string)Reader["skillId"];

                Dictionary<byte, object> skillData = null;

                if (result.ContainsKey(skillId))
                {
                    skillData = (Dictionary<byte, object>)result[skillId];
                }
                else
                {
                    skillData = new Dictionary<byte, object>();
                    result.Add(skillId, skillData);

                    skillData.Add((byte)Params.RoleId, (string)Reader["roleId"]);
                    skillData.Add((byte)Params.SkillName, (string)Reader["skillName"]);
                    skillData.Add((byte)Params.SkillDescription, (string)Reader["skillDescription"]);
                    skillData.Add((byte)Params.SkillUrl, imageUrl + (string)Reader["imageUrl"]);

                    skillData.Add((byte)Params.Levels, new Dictionary<byte, object>());
                }

                try
                {
                    var skillLevelsData = (Dictionary<byte, object>)skillData[(byte)Params.Levels];
                    var skillLevelData = new Dictionary<byte, object>();

                    var skillLevel = (int)Reader["skillLevel"];
                    skillLevelsData.Add((byte)skillLevel, skillLevelData);

                    skillLevelData.Add((byte)Params.SkillCost, (int)Reader["skillCost"]);
                    skillLevelData.Add((byte)Params.SkillValue, (int)Reader["skillValue"]);
                }
                catch (Exception e)
                {

                }
            }

            Conn.Close();

            Logger.Log.Debug($"db skills => {result.Count}");

            return result;
        }

        public int LoadUserSkillLevel(string skillId, Client client)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `userskills` WHERE `skillId` = '{skillId}' AND `skillOwner` = {client.userDbId} ";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int skillLevel = 0;

            while (Reader.Read())
            {
                skillLevel = (int)Reader["skillLevel"];
            }

            Conn.Close();

            return skillLevel;
        }

        public Dictionary<string, int> LoadUserSkills(Client client)
        {
            var result = new Dictionary<string, int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `userskills` " +
                $" LEFT JOIN `skills` ON `skills`.`skillId` = `userskills`.`skillId` " +
                $" WHERE `skillOwner` = {client.userDbId}";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var skillId = (string)Reader["skillId"];
                var skillLevel = (int)Reader["skillLevel"];

                result.Add(skillId, skillLevel);
            }

            Conn.Close();

            return result;
        }

        public Dictionary<string, int> LoadUserSkillsByRole(Client client, RoleType roleType)
        {
            var result = new Dictionary<string, int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT `userskills`.`skillId`, `skillValue` FROM `userskills`" +
                $" LEFT JOIN `skillslevels` ON `skillslevels`.`skillId` = `userskills`.`skillId` " +
                $" LEFT JOIN `skills` ON `skills`.`skillId` = `userskills`.`skillId`" +
                $" WHERE `skillOwner` = {client.userDbId} " +
                $" AND `skillslevels`.`skillLevel` = `userskills`.`skillLevel` " +
                $" AND (`skills`.`roleId` = '{roleType.ToString()}'" +
                $" OR `skills`.`roleId` = 'NULL')";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var skillId = (string)Reader["skillId"];
                var skillValue = (int)Reader["skillValue"];

                result.Add(skillId, skillValue);
            }

            Conn.Close();

            return result;
        }
        public Dictionary<string, int> LoadSkillsByRole(RoleType roleType)
        {
            var result = new Dictionary<string, int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT `skills`.`skillId`, `skillValue` FROM `skills`" +
                $" LEFT JOIN `skillslevels` ON `skillslevels`.`skillId` = `skills`.`skillId` " +
                $" WHERE `skillslevels`.`skillLevel` = 3 " +
                $" AND `skills`.`roleId` = '{roleType.ToString()}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var skillId = (string)Reader["skillId"];
                var skillValue = (int)Reader["skillValue"];

                result.Add(skillId, skillValue);
            }

            Conn.Close();

            return result;
        }

        public void RemovePlayerSkillsByRole(long userId, RoleType roleId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"DELETE FROM `userskills` WHERE `skillOwner` = '{userId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            Comm.ExecuteNonQuery();

            Conn.Close();
        }

        #endregion

        #region Extras

        public void SaveExtra(Dictionary<byte, object> extraData)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var extraId = (string)extraData[(byte)Params.ExtraId];
            var extraName = (string)extraData[(byte)Params.ExtraName];
            var extraDescription = (string)extraData[(byte)Params.ExtraDescription];
            var extraType = (string)extraData[(byte)Params.ExtraType];
            var currencyType = (string)extraData[(byte)Params.CurrencyType];
            var useType = (string)extraData[(byte)Params.ExtraUseType];
            var extraPhase = (string)extraData[(byte)Params.ExtraPhase];

            var extraCost = (int)extraData[(byte)Params.ExtraCost];
            var extraBuyCount = (int)extraData[(byte)Params.ExtraBuyCount];
            var extraDuration = (int)extraData[(byte)Params.ExtraDuration];
            var extraGameCount = (int)extraData[(byte)Params.ExtraGameCount];

            var imageUrl = (string)extraData[(byte)Params.ExtraImageUrl];

            Comm.CommandText =
                $"INSERT INTO `extras`" +
                $" (extraId, extraName, extraDescription, extraType, extraPhase, extraUseType," +
                $" currencyType, extraCost, extraBuyCount, extraDuration, extraGameCount, imageUrl)" +
                $" VALUES " +
                $" ('{extraId}', '{extraName}', '{extraDescription}', '{extraType}', '{extraPhase}', '{useType}', " +
                $" '{currencyType}', '{extraCost}', '{extraBuyCount}', '{extraDuration}', '{extraGameCount}', '{imageUrl}')" +
                $" ON DUPLICATE KEY UPDATE" +
                $" extraName = '{extraName}'," +
                $" extraDescription = '{extraDescription}'," +
                $" extraType = '{extraType}'," +
                $" extraPhase = '{extraPhase}'," +
                $" currencyType = '{currencyType}'," +
                $" extraUseType = '{useType}'," +
                $" extraCost = '{extraCost}'," +
                $" extraBuyCount = '{extraBuyCount}'," +
                $" extraDuration = '{extraDuration}'," +
                $" extraGameCount = '{extraGameCount}'," +
                $" imageUrl = '{imageUrl}'";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save extra => {Result}");

            Conn.Close();
        }

        public Dictionary<string, object> LoadExtras()
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `extras`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<string, object>();

            while (Reader.Read())
            {
                Dictionary<byte, object> extraData = new Dictionary<byte, object>();

                var extraId = (string)Reader["extraId"];
                extraData.Add((byte)Params.ExtraId, extraId);

                extraData.Add((byte)Params.ExtraName, (string)Reader["extraName"]);
                extraData.Add((byte)Params.ExtraDescription, (string)Reader["extraDescription"]);
                extraData.Add((byte)Params.ExtraType, (string)Reader["extraType"]);
                extraData.Add((byte)Params.CurrencyType, (string)Reader["currencyType"]);
                extraData.Add((byte)Params.ExtraUseType, (string)Reader["extraUseType"]);

                extraData.Add((byte)Params.ExtraPhase, (string)Reader["extraPhase"]);

                extraData.Add((byte)Params.ExtraCost, (int)Reader["extraCost"]);
                extraData.Add((byte)Params.ExtraBuyCount, (int)Reader["extraBuyCount"]);
                extraData.Add((byte)Params.ExtraDuration, (int)Reader["extraDuration"]);
                extraData.Add((byte)Params.ExtraGameCount, (int)Reader["extraGameCount"]);

                extraData.Add((byte)Params.ExtraImageUrl, (string)Reader["imageUrl"]);

                result.Add(extraId, extraData);
            }

            Conn.Close();

            return result;
        }



        public Dictionary<int, object> GetExtrasByType(ExtraType type)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $" SELECT * FROM `extras`  WHERE extraType = '{type.ToString()}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<int, object>();

            int c = 0;
            while (Reader.Read())
            {
                Dictionary<byte, object> extraData = new Dictionary<byte, object>();

                var Id = (string)Reader["extraId"];
                var extraId = (ExtraEffect)Helper.GetEnumElement<ExtraEffect>(Id);

                extraData.Add((byte)Params.ExtraId, extraId);

                extraData.Add((byte)Params.ExtraName, (string)Reader["extraName"]);
                extraData.Add((byte)Params.ExtraDescription, (string)Reader["extraDescription"]);
                extraData.Add((byte)Params.ExtraType, (string)Reader["extraType"]);
                extraData.Add((byte)Params.CurrencyType, (string)Reader["currencyType"]);
                extraData.Add((byte)Params.ExtraUseType, (string)Reader["extraUseType"]);

                extraData.Add((byte)Params.ExtraPhase, (string)Reader["extraPhase"]);

                extraData.Add((byte)Params.ExtraCost, (int)Reader["extraCost"]);
                extraData.Add((byte)Params.ExtraBuyCount, (int)Reader["extraBuyCount"]);
                extraData.Add((byte)Params.ExtraDuration, (int)Reader["extraDuration"]);
                extraData.Add((byte)Params.ExtraGameCount, (int)Reader["extraGameCount"]);

                extraData.Add((byte)Params.ExtraImageUrl, (string)Reader["imageUrl"]);

                result.Add(c++, extraData);
            }

            Conn.Close();

            return result;
        }


        public void LoadExtraByExtraId(string extraId, out int cost, out int buyCount)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT `extras`.`extraCost`, `extras`.`extraBuyCount` FROM `extras`" +
                $" WHERE `extraId` = '{extraId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            cost = 0;
            buyCount = 0;

            while (Reader.Read())
            {
                cost = (int)Reader["extraCost"];
                buyCount = (int)Reader["extraBuyCount"];
            }

            Conn.Close();
        }




        public Dictionary<string, int> LoadUserExtras(Client client)
        {
            var result = new Dictionary<string, int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `userextras` " +
                $" WHERE `ownerId` = {client.userDbId}";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var extraId = (string)Reader["extraId"];
                var extraCount = (int)Reader["extraCount"];

                result.Add(extraId, extraCount);
            }

            Conn.Close();

            return result;
        }

        public int LoadUserExtraCount(Client client, string extraId)
        {
            int result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `userextras`" +
                $" WHERE `extraId` = '{extraId}' AND `ownerId` = '{client.userDbId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result = (int)Reader["extraCount"];
            }

            Conn.Close();

            return result;
        }

        public int SaveUserExtraCount(BasePlayer player, string extraId, int count)
        {
            int result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var clearString = $"DELETE FROM `userextras`" +
                $" WHERE `extraId` = '{extraId}' AND `ownerId` = '{player.playerId}'";

            var saveString = $"INSERT INTO `userextras` " +
                $"(`extraId`, `extraCount`, `ownerId`) " +
                $"VALUES ('{extraId}', '{count}', '{player.playerId}')";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            Comm.CommandText = clearString;
            Comm.ExecuteNonQuery();

            Comm.CommandText = saveString;
            Comm.ExecuteNonQuery();

            Conn.Close();

            return result;
        }



        public void AddExtraToUser(long userId, string extraId, int count)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `userextras`(`extraId`, `extraCount`, `ownerId`) VALUES ('{extraId}','{count}','{userId}')" +
                $" ON DUPLICATE KEY UPDATE" +
                $" extraCount = extraCount + {count}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            //Logger.Log.Debug($"save extra => {Result}");

            Conn.Close();
        }

        #endregion

        #region Achieves

        public void SaveAchieve(Dictionary<byte, object> parameters)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var achiveId = (string)parameters[(byte)Params.AchieveId];
            var achiveType = (string)parameters[(byte)Params.AchieveType];
            var achiveName = (string)parameters[(byte)Params.AchieveName];
            var achiveDescription = (string)parameters[(byte)Params.AchieveDescription];

            var achiveString = $" INSERT INTO `achieves` (achieveId, achieveName, achieveDescription, achieveType)" +
               $" VALUES('{achiveId}', '{achiveName}', '{achiveDescription}', '{achiveType}') " +
               $" ON DUPLICATE KEY UPDATE " +
               $" achieveName = '{achiveName}'," +
               $" achieveDescription = '{achiveDescription}'," +
               $" achieveType = '{achiveType}'";


            var clearAchieveLevels = $"DELETE FROM `achievelevels` WHERE `achieveId` = '{achiveId}'";

            var saveAchieveLevels = $"INSERT INTO `achievelevels` (achieveId, achieveLevel, levelExp, levelReward)" +
                $" VALUES";

            var achieveLevelsData = (Dictionary<byte, object>)parameters[(byte)Params.AchieveLevels];

            for (int i = 0; i < achieveLevelsData.Count; i++)
            {
                var achiveLevelData = (Dictionary<byte, object>)achieveLevelsData.ElementAt(i).Value;

                var achieveLevel = (int)achiveLevelData[(byte)Params.AchieveLevel];
                var levelExp = (int)achiveLevelData[(byte)Params.AchieveLevelExp];
                var levelReward = (int)achiveLevelData[(byte)Params.AchieveLevelReward];

                if (i > 0) saveAchieveLevels += ",";

                saveAchieveLevels += $" ('{achiveId}','{achieveLevel}','{levelExp}','{levelReward}')";
            }

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            Comm.CommandText = achiveString;
            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save achive => {Result}");

            Comm.CommandText = clearAchieveLevels;
            Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"clear achive levels => {Result}");

            if (achieveLevelsData.Count > 0)
            {
                Comm.CommandText = saveAchieveLevels;
                Result = Comm.ExecuteNonQuery();

                Logger.Log.Debug($"save achive levels => {Result}");
            }

            Conn.Close();
        }

        public Dictionary<string, object> LoadAchieves()
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT* FROM `achieves` " +
                " LEFT JOIN `achievelevels` ON `achievelevels`.`achieveId` = `achieves`.`achieveId`" +
                " ORDER BY `achieves`.`achieveId`, `achievelevels`.`achieveLevel`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<string, object>();

            while (Reader.Read())
            {
                var achieveId = (string)Reader["achieveId"];

                Dictionary<byte, object> achieveData = null;

                if (result.ContainsKey(achieveId))
                {
                    achieveData = (Dictionary<byte, object>)result[achieveId];
                }
                else
                {
                    achieveData = new Dictionary<byte, object>();
                    result.Add(achieveId, achieveData);

                    achieveData.Add((byte)Params.AchieveName, (string)Reader["achieveName"]);
                    achieveData.Add((byte)Params.AchieveDescription, (string)Reader["achieveDescription"]);
                    achieveData.Add((byte)Params.AchieveType, (string)Reader["achieveType"]);

                    achieveData.Add((byte)Params.AchieveLevels, new Dictionary<byte, object>());
                }

                try
                {
                    var achieveLevelsData = (Dictionary<byte, object>)achieveData[(byte)Params.AchieveLevels];

                    var achieveLevelData = new Dictionary<byte, object>();

                    var achieveLevel = (int)Reader["achieveLevel"];
                    achieveLevelsData.Add((byte)achieveLevel, achieveLevelData);

                    achieveLevelData.Add((byte)Params.AchieveLevelExp, (int)Reader["levelExp"]);
                    achieveLevelData.Add((byte)Params.AchieveLevelReward, (int)Reader["levelReward"]);
                }
                catch (Exception e)
                {

                }
            }

            Conn.Close();

            Logger.Log.Debug($"db achieves => {result.Count}");

            return result;
        }

        public Dictionary<string, object> LoadUserAchieves(Client client)
        {
            var result = new Dictionary<string, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT* FROM `userachieves`" +
                $" WHERE `ownerId` = '{client.userDbId}' ";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var achieveId = (string)Reader["achieveId"];

                var achieveData = new Dictionary<byte, object>();

                achieveData.Add((byte)Params.AchieveCurrentLevel, (int)Reader["currentLevel"]);
                achieveData.Add((byte)Params.AchieveCurrentExp, (int)Reader["currentExp"]);

                result.Add(achieveId, achieveData);
            }

            Conn.Close();

            return result;
        }

        public int GetAchieveMaxLevel(AchieveId achieveId)
        {
            int result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"  SELECT MAX(achieveLevel), achieveId FROM `achievelevels` WHERE achieveId = '{achieveId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result = (int)Reader["MAX(achieveLevel)"];
            }

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetAchieveLevelData(AchieveId achieveId, int level)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `achievelevels` " +
                               $"WHERE `achieveId` = '{achieveId}' AND `achieveLevel` = '{level}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.AchieveLevelExp, (int)Reader["levelExp"]);
                result.Add((byte)Params.AchieveLevelReward, (int)Reader["levelReward"]);
            }

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> LoadUserAchieve(Client client, AchieveId achieveId)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $" SELECT* FROM `userachieves` " +
            $"  WHERE `ownerId` = '{client.userDbId}'  " +
            $"  AND `userachieves`.`achieveId` = '{achieveId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.AchieveCurrentLevel, (int)Reader["currentLevel"]);
                result.Add((byte)Params.AchieveCurrentExp, (int)Reader["currentExp"]);
            }

            Conn.Close();

            return result;
        }

        public void SaveUserAchieve(Client client, AchieveId achieveId, int currentLevel, int currentExp)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"DELETE FROM `userachieves` " +
                $"WHERE `achieveId`= '{achieveId}' " +
                $"AND `ownerId`= '{client.userDbId}' ;" +
                $" INSERT INTO `userachieves` (achieveId, currentLevel, currentExp, ownerId)" +
                $" VALUES('{achieveId}','{currentLevel}','{currentExp}','{client.userDbId}')";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save user achieve => {Result}");

            Conn.Close();
        }

        #endregion

        #region Gifts

        public Dictionary<int, object> GetGifts()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `gifts`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 1;
            while (Reader.Read())
            {
                //result.Add((int)Reader["userId"]);
                var gift = new Dictionary<byte, object>();

                gift.Add((byte)Params.giftId, (int)Reader["id"]);
                gift.Add((byte)Params.giftName, (string)Reader["name"]);
                gift.Add((byte)Params.giftDescription, (string)Reader["description"]);
                gift.Add((byte)Params.giftCost, (int)Reader["cost"]);
                gift.Add((byte)Params.giftCostType, (CostType)Helper.GetEnumElement<CostType>((string)Reader["costType"]));
                gift.Add((byte)Params.giftImage, (string)Reader["image"]);
                gift.Add((byte)Params.giftEventType, (GiftEventType)Helper.GetEnumElement<GiftEventType>((string)Reader["giftEventType"]));
                gift.Add((byte)Params.giftType, (GiftType)Helper.GetEnumElement<GiftType>((string)Reader["giftType"]));
                gift.Add((byte)Params.giftExtraEffect, (ExtraEffect)Helper.GetEnumElement<ExtraEffect>((string)Reader["giftEffect"]));

                //gift.id = (int)Reader["id"];
                //gift.name = (string)Reader["name"];
                //gift.description = (string)Reader["description"];
                //gift.cost = (int)Reader["cost"];
                //gift.costType = (CostType)Helper.GetEnumElement<CostType>((string)Reader["costType"]);
                //gift.image = (string)Reader["image"];
                //gift.giftEventType = (GiftEventType)Helper.GetEnumElement<GiftEventType>((string)Reader["giftEventType"]);
                //gift.giftType = (GiftType)Helper.GetEnumElement<GiftType>((string)Reader["giftType"]);
                //gift.giftEffect = (ExtraEffect)Helper.GetEnumElement<ExtraEffect>((string)Reader["giftEffect"]);

                result.Add(count, gift);
                count++;
            }
            Conn.Close();

            return result;
        }

        public void CreateGift
            (
                int id,
                string name,
                string description,
                string cost,
                string costType,
                string image,
                string giftEventType,
                string giftType,
                string giftEffect
            )
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `gifts` (`id`, `name`, `description`, `cost`, `costType`, `image`, `giftEventType`, `giftType`, `giftEffect`)" +
                $" VALUES('{id.ToString()}', '{name}', '{description}', '{cost}', '{costType}', '{image}', '{giftEventType}', '{giftType}', '{giftEffect}') " +
                $" ON DUPLICATE KEY UPDATE name='{name}'";


            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"add user to clan => {Result}");

            Conn.Close();
        }

        #endregion

        #region Duels

        public void AddRespectsToUser(long userId, int respects)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `userdata` SET `respects`=(`respects` + {respects}) WHERE userId = {userId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            //Logger.Log.Debug($"save extra => {Result}");

            Conn.Close();
        }

        public Dictionary<byte, object> TryGetCurrentDuelDay(string now)
        {
            var result = new Dictionary<byte, object>();

            var newNow =  DateTime.ParseExact(now, "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd H:mm:ss");

            //Logger.Log.Debug("NEW NOW: " + newNow);
            

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `duels_days`" +
                $" WHERE start < '{newNow}' AND end > '{newNow}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.Id,     (int)Reader["id"]);
                result.Add((byte)Params.Start,  Reader["start"].ToString());
                result.Add((byte)Params.End,    Reader["end"].ToString());
            }

            Conn.Close();

            return result;

        }

        /// <summary>
        /// Создание дуэльного дня
        /// </summary>
        /// <returns></returns>
        public int CreateDuelDay(string start, string end)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `duels_days` (`id`, `start`, `end`) VALUES (NULL, '{start}', '{end}');" +
                $" SELECT MAX(id) FROM `duels_days`; ";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int id = 0;
            while (Reader.Read())
            {
                id = (int)Reader["Max(id)"];
            }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return id;
        }

        
        public void SaveClanPairMission(int dueldayId, int clan1Id, int clan2Id, int missionNumber, string missionId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `duels_missions` (`id`, `clanpairId`, `missionNumber`, `missionId`) VALUES(NULL, (SELECT id FROM `duels_clanpairs` WHERE dueldayId = '{dueldayId}' and clan1Id = '{clan1Id}' and clan2Id = '{clan2Id}'), '{missionNumber}', '{missionId}');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }

        /// <summary>
        /// Добавить очки колеса для пользователя
        /// </summary>
        public void AddWheelPointsToUser(long userId, int wheelPoints)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `userdata` SET `wheelPoints`=(`wheelPoints` + {wheelPoints}) WHERE userId={userId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }

        /// <summary>
        /// Добавить очки улучшения
        /// </summary>
        public void AddUpPointsToClan(int clanId, int points)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `clans` SET `upPoints`=(`upPoints` + {points}) WHERE id = {clanId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }



        /// <summary>
        /// Получить пары кланов для дуэльного дня
        /// </summary>
        public Dictionary<int, object> GetClanPairsForDuelday(int duelDayId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `duels_clanpairs`" +
                $" WHERE dueldayId = {duelDayId}";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var clanPair = new Dictionary<byte, object>();

                clanPair.Add((byte)Params.Id, Reader["id"]);

                clanPair.Add((byte)Params.clan1Id, Reader["clan1Id"]);
                clanPair.Add((byte)Params.clan2Id, Reader["clan2Id"]);

                result.Add(count, clanPair);

                count++;
            }

            Logger.Log.Debug("result count " + result.Count);

            Conn.Close();

            return result;
        }


        /// <summary>
        /// Получить уровни вознаграждения
        /// </summary>
        public Dictionary<int, object> GetAwardsLevels()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `duels_awards_scale`" +
                $" ORDER BY number";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.Number, (int)Reader["number"]);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);

                result.Add(count, row);

                count++;
            }

            Logger.Log.Debug("result count " + result.Count);

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить очки пользователя для вращения дуэльного колеса
        /// </summary>
        public int GetUserWheelPoinst(long userId)
        {
            int result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `userdata` WHERE userId = {userId};";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                result = (int)(Reader["WheelPoints"]);
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить колличество выигранных игр членами клана во время дуэли
        /// </summary>
        public int GetClanWinsInDuel(int duelDayId, int cId)
        {
            int result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT count(userId) as count FROM `duels_wins_in_games`" +
                $" WHERE clanPairId = (SELECT id FROM `duels_clanpairs` WHERE dueldayId = {duelDayId} and(clan1Id = {cId} or clan2Id = {cId})) AND clanId = {cId};";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                result = (int)((Int64)Reader["count"]);
            }

            Conn.Close();

            return result;
        }


        public Dictionary<int, object> GetDuelMissions(int duelPair)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT  missionNumber, missionId FROM `duels_missions`" +
                $" WHERE clanpairId = {duelPair}" +
                $" ORDER BY missionNumber";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.missionNumber, (int)Reader["missionNumber"]);

                var id = Helper.GetEnumElement<DuelsAchiveId>((string)Reader["missionId"]);
                Logger.Log.Debug((DuelsAchiveId)id);

                row.Add((byte)Params.Id, id);

                result.Add(count, row);
                count++;
            }

            Conn.Close();

            return result;
        }


        /// <summary>
        /// Увеличить колличество дуэльных очков
        /// </summary>
        public void AddDuelPointsToClan(int clanId, int duelPoints)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `clans` SET `duelPoints` = (`duelPoints` + {duelPoints})" +
                $" WHERE id = {clanId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }


        /// <summary>
        /// SetWinnerInDuel
        /// </summary>
        public void SetDuelWinner(int clanPairId, int clanId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `duels_clanpairs` SET `winnerId`={clanId}" +
                $" WHERE id = {clanPairId};";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }


        /// <summary>
        /// Увеличить очки пользователя в его дуэли
        /// </summary>
        public void IncrementUserDuelMission(int duelDayId, long userId, DuelsAchiveId duelId, int incScore)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `duels_user_mission`" +
                $" JOIN duels_clanpairs ON duels_user_mission.clanpairId = duels_clanpairs.id" +
                $" JOIN duels_missions ON duels_user_mission.missionNumber = duels_missions.missionNumber" +
                $" SET `amount` = `amount` + {incScore}" +
                $" WHERE userId = {userId} AND duels_clanpairs.dueldayId = {duelDayId} and duels_missions.missionId = '{duelId.ToString()}';";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }


        /// <summary>
        /// Получить личный топ в дуэли по идентификатору дуэльной пары
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, object> GetDuelPersonalTop(int clanPairId)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT DENSE_RANK() over(ORDER BY amount DESC) as `number` , id, clanpairId, userId, clanId, amount FROM `duels_user_mission`" +
                $" WHERE clanpairId = {clanPairId}" +
                $" ORDER BY amount DESC;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.Id, (long)Reader["id"]);
                row.Add((byte)Params.UserId, (long)Reader["userId"]);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);
                row.Add((byte)Params.clanId, (int)Reader["clanId"]);

                //Logger.Log.Debug("NUMBER " + (int)(UInt64)Reader["number"]);
                row.Add((byte)Params.Number, (int)(UInt64)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }



        /// <summary>
        /// Получить личный топ в дуэли с именами игроков
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, object> GetDuelPersonalTopWithNames(int clanPairId)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT DENSE_RANK() over(ORDER BY amount DESC) as `number` , duels_user_mission.clanpairId, userId, `users`.`name`, `clanId`, duels_user_mission.missionNumber, amount FROM `duels_user_mission`" +
                $" JOIN users ON users.id = `duels_user_mission`.`userId`" +
                $" WHERE duels_user_mission.clanpairId = {clanPairId}" +
                $" ORDER BY amount DESC;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                //row.Add((byte)Params.Id, (long)Reader["id"]);
                row.Add((byte)Params.UserId, (long)Reader["userId"]);
                row.Add((byte)Params.Name, (string)Reader["name"]);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);
                row.Add((byte)Params.clanId, (int)Reader["clanId"]);
                row.Add((byte)Params.missionNumber, (int)Reader["missionNumber"]);

                //Logger.Log.Debug("NUMBER " + (int)(UInt64)Reader["number"]);
                row.Add((byte)Params.Number, (int)(UInt64)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить вознаграждения для уровня
        /// </summary>
        public Dictionary<int, object> GetAwardsForLevel(int levelNumber)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `duels_awardsitems`" +
                $" WHERE levelNumber = {levelNumber}" +
                $" ORDER BY number;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                ResourseType resourse = (ResourseType)Helper.GetEnumElement<ResourseType>((string)Reader["resourse"]);

                row.Add((byte)Params.Resourse, resourse);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);
                row.Add((byte)Params.Number, (int)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }



        /// <summary>
        /// Получить идентификатор дуэльной пары и кланы для пользователя
        /// </summary>
        public Dictionary<int, object> GetDuelClansForUser(int duelDayId, int clanId, ref int clanPairId)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT `duels_clanpairs`.`id`, clan1Id, clan2Id, clans.id AS `clanId`, name, clans.abbr FROM `duels_clanpairs`" +
                $" JOIN clans ON `duels_clanpairs`.`clan1Id` = clans.id OR clan2Id = clans.id" +
                $" WHERE dueldayId = {duelDayId} AND(clan1Id = {clanId} or clan2Id = {clanId});";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                clanPairId = (int)Reader["id"];
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.Id, (int)Reader["clanId"]);
                row.Add((byte)Params.Name, (string)Reader["name"]);
                //row.Add((byte)Params.Amount, (int)Reader["amount"]);
                //row.Add((byte)Params.clanId, (int)Reader["clanId"]);

                //Logger.Log.Debug("NUMBER " + (int)(UInt64)Reader["number"]);
                //row.Add((byte)Params.Number, (int)(UInt64)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить пользователей и их победы в ииграх
        /// </summary>
        public Dictionary<int, object> GetClanDuelUsers(int clanPairId, int clanId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT clanPairId, clanId, userId, COUNT(userId) AS amount FROM `duels_wins_in_games`" +
                $" WHERE clanPairId = {clanPairId} AND clanId = {clanId}" +
                $" GROUP BY userId;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.UserId, (long)Reader["userId"]);
                row.Add((byte)Params.Amount, (int)(Int64)Reader["amount"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }


        /// <summary>
        /// Поставить выигрыш в строчке задания
        /// </summary>
        public void SetWinUserDuelMission(long id, int win)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `duels_user_mission` SET `win`='{win}' WHERE id = {id}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }

        /// <summary>
        /// Получить игроков клана дуэли с очками
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, object> GetDuelClanUsers(int clanPairId, int clanId){

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `duels_user_mission`" +
                $" WHERE clanpairId = {clanPairId} AND clanId = {clanId}" +
                $" ORDER BY missionNumber";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.Id, (long)Reader["id"]);
                row.Add((byte)Params.UserId, (long)Reader["userId"]);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        public void SaveDuelUserMission(int dueldayId, int clanId, int clan1Id, int clan2Id, int userId, int missionNumber, int amount)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `duels_user_mission`(`clanpairId`, `clanId`, `userId`, `missionNumber`, `amount`) VALUES ((SELECT id FROM `duels_clanpairs` WHERE dueldayId = '{dueldayId}' and clan1Id = '{clan1Id}' and clan2Id = '{clan2Id}'),'{clanId}','{userId}','{missionNumber}','{amount}');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }


        public void SaveClansPair(int dueldayId, int clan1Id, int clan2Id)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `duels_clanpairs` (`id`, `dueldayId`, `clan1Id`, `clan2Id`, `winnerId`) VALUES (NULL, '{dueldayId}', '{clan1Id}', '{clan2Id}', '0');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            
            //while (Reader.Read())
            //{
                
            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

        }

        #endregion

        #region Clans

        /// <summary>
        /// Получение дуэльного топа кланов
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, object> GetClansDuelTop()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT `t1`.`id`, `t1`.`name`, `duelPoints`, `count`, alliances_clans.allianceId, alliances.name as `allianceName` FROM" +
                $" (SELECT clans.id, clans.name, clans.duelPoints, COUNT(clans.id) as `count` from `clans`" +
                $" join clansusers on clans.id = clansusers.clanId" +
                $" GROUP BY clans.id" +
                $" ORDER BY clans.duelPoints DESC, `count` DESC) as t1" +
                $" LEFT JOIN alliances_clans ON `t1`.`id` = alliances_clans.clanId" +
                $" LEFT JOIN alliances ON alliances_clans.allianceId = alliances.id;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var clanString = new Dictionary<byte, object>();

                clanString.Add((byte)Params.Id, (int)Reader["id"]);
                clanString.Add((byte)Params.Name, (string)Reader["name"]);
                clanString.Add((byte)Params.duelPoints, (int)Reader["duelPoints"]);

                if (Reader["allianceId"] is DBNull)
                {
                    clanString.Add((byte)Params.allianceId, 0);
                }
                else
                {
                    clanString.Add((byte)Params.allianceId, (int)Reader["allianceId"]);
                    clanString.Add((byte)Params.allianceName, (string)Reader["allianceName"]);
                }
                result.Add(count, clanString);
                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetClans()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT* FROM `clans`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var clanId = (int)Reader["id"];

                var clanDict = new Dictionary<byte, object>();

                clanDict.Add((byte)Params.Name, (string)Reader["name"]);
				//clanDict.Add((byte)Params.OwnerId, (int)Reader["ownerId"]);														   
                clanDict.Add((byte)Params.Image, (string)Reader["image"]);

                var id = (int)Reader["id"];

                result.Add(clanId, clanDict);
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetClansForProposal(long userId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT id, name, image,  `clansusers`.`clanId`, count(`clansusers`.`clanId`) as `count` FROM `clans`" +
                $" left join `clansusers` on `clans`.`id` = `clansusers`.`clanId`" +
                $" WHERE id NOT IN" +
                $" (SELECT clanId FROM `clansinvitations` WHERE userId = '{userId}')" +
                $" GROUP BY id" +
                $" HAVING `count` < '{Options.maxClanUsers}'" +
                $" ORDER by `count` DESC;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int number = 0;
            while (Reader.Read())
            {
                var clan = new Dictionary<byte, object>();

                clan.Add((byte)Params.Id, (int)Reader["id"]);
                clan.Add((byte)Params.Name, (string)Reader["name"]);
                clan.Add((byte)Params.Image, (string)Reader["image"]);
                clan.Add((byte)Params.Count, (int)(Int64)Reader["count"]);

                //Logger.Log.Debug((string)Reader["name"]);
                //Logger.Log.Debug((int)(Int64)Reader["count"]);

                //if(Reader["invite"] is DBNull)
                //{
                //    clan.Add((byte)Params.Invite, false);
                //}
                //else
                //{
                //    clan.Add((byte)Params.Invite, true);
                //}

                result.Add(number, clan);

                number++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetUserClan(long userId)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `clansusers`" +
                $" JOIN `clans` on `clansusers`.`clanId` = `clans`.`id`" +
                $" WHERE `userId` = '{userId}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.RoleId, (string)Reader["role"]);


                result.Add((byte)Params.Name, (string)Reader["name"]);
                result.Add((byte)Params.Image, (string)Reader["image"]);
                result.Add((byte)Params.Id, (int)Reader["clanId"]);
                result.Add((byte)Params.upPoints, (int)Reader["upPoints"]);
            }

            Conn.Close();

            return result;
        }


        public Dictionary<int, object> GetUserClanRights(long userId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT `rightId` FROM `users`" +
                $" JOIN `clansusers` ON `users`.`id` = `clansusers`.`userId`" +
                $" JOIN `clanroles_clanrights` ON `clansusers`.`role` = `clanroles_clanrights`.`roleId`" +
                $" WHERE `users`.`id` = '{userId}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 1;
            while (Reader.Read())
            {
                ClanRights right = (ClanRights)Helper.GetEnumElement<ClanRights>((string)Reader["rightId"]);

                //Logger.Log.Debug((int)right);

                result.Add(count, (int)right);

                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetFreePlayers()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `users`" +
                $" WHERE id NOT IN" +
                $" (SELECT userId FROM clansusers)" +
                $" AND id NOT IN" +
                $" (SELECT userId from clansinvitations);";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 1;
            while (Reader.Read())
            {
                var user = new Dictionary<byte, object>();

                user.Add((byte)Params.Id, (long)Reader["id"]);
                user.Add((byte)Params.Name, (string)Reader["name"]);

                result.Add(count, user);
                count++;
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получение игроков клана
        /// </summary>
        /// <param name="clanId"></param>
        /// <returns></returns>
        public Dictionary<int, object> GetClanUsers(int clanId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `clansusers`" +
                $" JOIN `users` ON `clansusers`.`userId` = `users`.`id`" +
                $" LEFT JOIN `clanroles` ON `clansusers`.`role` = `clanroles`.`id`" +
                $" WHERE clanId = '{clanId}'" +
                $" ORDER BY `clanroles`.`priority` DESC;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 1;
            while (Reader.Read())
            {
                var userDict = new Dictionary<byte, object>();

                //clanDict.Add((byte)Params.Name, (string)Reader["name"]);

                userDict.Add((byte)Params.Id, (int)Reader["userId"]);
                userDict.Add((byte)Params.Name, (string)Reader["name"]);
                userDict.Add((byte)Params.RoleId, (string)Reader["role"]);

                result.Add(count, userDict);

                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetCoinsForClan(long userId)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `userdata` WHERE `userId` = '{userId}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            result.Add((byte)Params.Cost, Options.clanCost);

            while (Reader.Read())
            {
                result.Add((byte)Params.Coins, (int)Reader["coins"]);
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить улучшения для кланов и те улучшения, которые есть у данного клана
        /// </summary>
        public Dictionary<int, object> GetClanUpgarades(int clanId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" SELECT * FROM `clansupgrades`" +
                $" LEFT JOIN clans_clansupgades on clansupgrades.id = clans_clansupgades.clansupgradeId and clans_clansupgades.clanId = {clanId}" +
                $" ORDER BY number";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                var id = Helper.GetEnumElement<ClansUpgardesId>((string)Reader["id"]);

                //Logger.Log.Debug(id.ToString());
                //Logger.Log.Debug(Reader["description"]);
                //Logger.Log.Debug(Reader["price"]);

                row.Add((byte)Params.Id, id);
                row.Add((byte)Params.Description, Reader["description"]);
                row.Add((byte)Params.Cost, Reader["price"]);

                if(!(Reader["clanId"] is DBNull))
                {
                    Logger.Log.Debug("CLAN HAS UPGRADE " + Reader["id"]);
                    row.Add((byte)Params.Level, (byte)Params.Level);
                }

                result.Add(count, row);
                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetUserInvites(long userId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" SELECT * FROM `clansinvitations`" +
                $" JOIN clans ON `clansinvitations`.`clanId` = `clans`.`id`" +
                $" WHERE userId = '{userId}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var inv = new Dictionary<byte, object>();

                //clanId
                inv.Add((byte)Params.Id, (int)Reader["id"]);
                inv.Add((byte)Params.Name, (string)Reader["name"]);
                inv.Add((byte)Params.Image, (string)Reader["image"]);

                //Type of invite
                inv.Add((byte)Params.Invite, (string)Reader["type"]);

                //Logger.Log.Debug((string)Reader["type"]);

                result.Add(count, inv);
                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetClanInvites(int clanId)
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" SELECT * FROM `clansinvitations`" +
                $" JOIN `users` ON `clansinvitations`.`userId` = `users`.`id`" +
                $" WHERE `clansinvitations`.`clanId` = '{clanId}';";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var inv = new Dictionary<byte, object>();

                //clanId
                inv.Add((byte)Params.Id, (long)Reader["id"]);
                inv.Add((byte)Params.Name, (string)Reader["name"]);
                //inv.Add((byte)Params.Image, (string)Reader["image"]);

                //Type of invite
                inv.Add((byte)Params.Invite, (string)Reader["type"]);

                //Logger.Log.Debug((string)Reader["type"]);

                result.Add(count, inv);
                count++;
            }

            Conn.Close();

            return result;
        }

        public void TryCreateClan(Dictionary<byte, object> data, long userId)
        {
            if (TryPay(userId))
            {
                CreateClan(data, userId);
            }
        }

        public bool TryPay(long userId)
        {
            int cost = Options.clanCost;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" UPDATE `userdata` SET `coins`=(`coins` - '{cost}')" +
                $" WHERE userId = '{userId}' AND coins >= '{cost}';";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug("Result " + Result);
            Conn.Close();

            if (Result != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Попытка заплатить очки колеса удачи
        /// </summary>
        public bool TryPayWheelPoints(long userId, int points)
        {
            int cost = Options.clanCost;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" UPDATE `userdata` SET `wheelPoints` = (`wheelPoints` - '{points}')" +
                $" WHERE userId = '{userId}' AND `wheelPoints` >= '{points}'";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug("Result " + Result);
            Conn.Close();

            if (Result != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Добавить пользоватлю монеты
        /// </summary>
        public void AddCoinsToUser(long userId, int coins)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `userdata` SET `coins`=(`coins` + {coins}) WHERE `userId` = {userId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }


        public void AddEnergyToUser(long userId, int points)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `userdata` SET `energy`=(`energy` + {points}) WHERE userId = {userId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }

        public void AddDiamondsToUser(long userId, int diamonds)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `userdata` SET `diamonds`=(`diamonds` + {diamonds}) WHERE `userId` = {userId}";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }

        /// <summary>
        /// Добавить очко выйгранной игры во время дуэли
        /// </summary>
        public void AddWinGameInDuelTime(int duelDayId, int clanId, long userId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `duels_wins_in_games`(`clanPairId`, `clanId`, `userId`)" +
                $" VALUES((SELECT id FROM `duels_clanpairs` WHERE dueldayId = {duelDayId} and (clan1Id = {clanId} or clan2Id = {clanId})), {clanId}, {userId})";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            //while (Reader.Read())
            //{

            //}

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

        }


        /// <summary>
        /// Проверить условия и купить апгрейд
        /// </summary>
        public void TryBuyClanUpgrade(int clanId, ClansUpgardesId upgradeId)
        {
            int price = CheckBalanceForUpgradeAndGetPrice(clanId, upgradeId);

            if(price > 0)
            {
                BuyClanUpgrade(clanId, price, upgradeId);
            }
        }

        /// <summary>
        /// Проверить баланс очков улучшения у клана для нужного апгрейда
        /// </summary>
        private int CheckBalanceForUpgradeAndGetPrice(int clanId, ClansUpgardesId upgaradeId)
        {
            //марккер, что проверка не прошла
            int price = -100;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `clans`" +
                $" JOIN clansupgrades" +
                $" WHERE clansupgrades.id = '{upgaradeId.ToString()}' and clans.id = {clanId} and upPoints >= clansupgrades.price;";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();
            //Logger.Log.Debug($"save stock => {Result}");


            while (Reader.Read())
            {
                price = (int)Reader["price"];
            }

            Conn.Close();

            return price;
        }


        private void BuyClanUpgrade(int clanId, int price, ClansUpgardesId upgaradeId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" UPDATE `clans` SET `upPoints`=(`upPoints` - {price}) WHERE clans.id = {clanId};" +
                $" INSERT INTO `clans_clansupgades`(`clanId`, `clansupgradeId`) VALUES({clanId}, '{upgaradeId.ToString()}');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            while (Reader.Read())
            {

            }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();
        }



        public void CreateClan(Dictionary<byte, object> data, long userId)
        {
            var name = (string)data[(byte)Params.Name];
            var abbr = (string)data[(byte)Params.Abbr];
            var image = (string)data[(byte)Params.Image];

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `clans`(`name`, `abbr`, `image`) VALUES ('{name}','{abbr}','{image}');" +
                $" INSERT INTO `clansusers`(`clanId`, `userId`, `role`) VALUES((SELECT MAX(`id`) FROM clans), '{userId}', 'Leader');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            
            while (Reader.Read())
            {
                    
            }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

        }

        public void AddUserToClan(int clanId, long userId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" INSERT INTO `clansusers`(`clanId`, `userId`, `role`) VALUES('{clanId}', '{userId}', '');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var result = Comm.ExecuteNonQuery();

            Conn.Close();
        }

        public int CreateInviteProposal(int clanId, long userId, InviteType invType)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"INSERT INTO `clansinvitations` (`clanId`, `userId`, `type`) VALUES ('{clanId}', '{userId}', '{invType.ToString()}');";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return Result;

        }

        //Удалить заявку или запрос, если она существует, так же если игрок вне клана
        public bool DeleteInvPropClan(int clanId, long userId, InviteType invType)
        {
            var result = false;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" DELETE FROM `clansinvitations`" +
                $" WHERE userId = '{userId}' AND clanId = '{clanId}' AND type = '{invType.ToString()}'" +
                $" AND userId not IN(SELECT userId from clansusers WHERE userId = '{userId}');";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            var intRes = Comm.ExecuteNonQuery();

            if (intRes > 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }

            Conn.Close();

            return result;

        }
        //Проверить существование инвайта или запроса и условия, что пользователь не состоит в клане
        public bool CheckInvPropClan(int clanId, long userId, InviteType invType)
        {
            var result = false;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" SELECT * FROM `clansinvitations`" +
                $" WHERE userId = '{userId}' AND clanId = '{clanId}' AND type = '{invType.ToString()}'" +
                $" AND userId not IN(SELECT userId from clansusers WHERE userId = '{userId}');";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            if (Reader.HasRows)
            {
                result = true;
            }
            else
            {
                result = false;
            }
                
            Conn.Close();

            return result;
            
        }
        public int DeleteInviteProposal(int clanId, long userId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            //var clanId = (int)data[(byte)Params.Id];

            Comm.CommandText =
                $"DELETE FROM `clansinvitations` WHERE `clanId` = '{clanId}' AND `userId` = '{userId}'";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return Result;

        }

        #endregion

        #region Raiting

        public int CreateWeeklyExpCompetition(string start)
        {
            var result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `raiting_weeklyclancompetition`(`start`) VALUES ('{start}');" +
                $" SELECT max(id) as id FROM `raiting_weeklyclancompetition`;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            while (Reader.Read())
            {
                Logger.Log.Debug(Reader["id"]);
                result = (int)Reader["id"];
            }

            Conn.Close();

            return result;
        }


        /// <summary>
        /// Создать запись о группе кланов в недельном соревновании
        /// </summary>
        public int CreateWeeklyExpCompetitionGroup(int compId)
        {
            var result = 0;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `raiting_weeklycomp_group`(`compId`) VALUES ('{compId}');" +
                $" SELECT MAX(id) as id FROM `raiting_weeklycomp_group`;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            while (Reader.Read())
            {
                //Logger.Log.Debug(Reader["id"]);
                result = (int)Reader["id"];
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Добавить связь между группой в недельном соревновании и кланом
        /// </summary>
        public void CreateCompGroupClanRow(int groupId, int clanId)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText =
                $" INSERT INTO `raiting_weeklycomp_group_clan`(`groupId`, `clanId`) VALUES ('{groupId}','{clanId}')";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var result = Comm.ExecuteNonQuery();

            Conn.Close();
        }

        /// <summary>
        /// Плучить идентификаторы кланов в случайном порядке
        /// </summary>
        public List<int> GetRandomClanList()
        {

            var result = new List<int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT id FROM `clans` ORDER by RAND();"; 

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            
            while (Reader.Read())
            {
                //Logger.Log.Debug("CLAN ID " + (int)Reader["id"]);
                result.Add((int)Reader["id"]);
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить группы соревнования
        /// </summary>
        public List<int> GetCompGroups(int compId)
        {

            var result = new List<int>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT id FROM `raiting_weeklycomp_group` WHERE compId = {compId}";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();


            while (Reader.Read())
            {
                result.Add((int)Reader["id"]);
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Создать строчку события влияющего на рейтинг
        /// </summary>
        public void CreateRaitingEventRow(string datetime, long userId, int clanId, string eventType, string eventSubtype, int amount)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" INSERT INTO `raiting_events`(`datetime`, `userId`, `clanId`, `eventType`, `eventSubType`, `amount`) " +
                $" VALUES ('{datetime}','{userId}','{clanId}','{eventType}','{eventSubtype}','{amount}')";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int id = 0;
            while (Reader.Read())
            {
                //id = (int)Reader["Max(id)"];
            }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            //return id;
        }

        public Dictionary<int, object> GetReiting(string datetime, string eventType, string eventSubType)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT DENSE_RANK() over(ORDER BY amount DESC) as number, userId, name, amount FROM" +
                $" (SELECT userId, SUM(amount) AS amount FROM `raiting_events`" +
                $" WHERE eventType = '{eventType}' AND eventSubType = '{eventSubType}'" +
                $" AND datetime > '{datetime}'" +
                $" GROUP BY userId" +
                $" LIMIT 20) as T1" +
                $" join users on T1.userId = users.id";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                //row.Add((byte)Params.Id, (long)Reader["id"]);
                row.Add((byte)Params.UserId, (long)Reader["userId"]);
                row.Add((byte)Params.Amount, (int)(decimal)Reader["amount"]);
                row.Add((byte)Params.Name, (string)Reader["name"]);

                row.Add((byte)Params.Number, (int)(UInt64)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }


        public Dictionary<int, object> GetClanReiting(string datetime, string eventType, string eventSubType)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT DENSE_RANK() over(ORDER BY amount DESC) as number, clanId, name, amount FROM" +
                $" (SELECT clanId, SUM(amount) AS amount FROM `raiting_events`" +
                $" WHERE eventType = '{eventType}' AND eventSubType = '{eventSubType}'" +
                $" AND datetime > '{datetime}'" +
                $" GROUP BY clanId" +
                $" LIMIT 20) as T1" +
                $" join clans on T1.clanId = clans.id;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                //row.Add((byte)Params.Id, (long)Reader["id"]);
                row.Add((byte)Params.UserId, (long)(int)Reader["clanId"]);
                row.Add((byte)Params.Amount, (int)(decimal)Reader["amount"]);
                row.Add((byte)Params.Name, (string)Reader["name"]);

                row.Add((byte)Params.Number, (int)(UInt64)Reader["number"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetReitingPeriods(ReitingType reitingType)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `raiting_raitings_periods`" +
                $" WHERE raitingId = '{reitingType.ToString()}'" +
                $" ORDER BY number";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                string sId = (string)Reader["periodId"];
                Logger.Log.Debug("str id " + sId);
                int id = (int)Helper.GetEnumElement<ReitingInterval>(sId);

                row.Add((byte)Params.Id, id);
                
                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetReitingPlacesSets(ReitingType reitingType)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `raiting_raitings_sets`" +
                $" WHERE reitingId = '{reitingType.ToString()}' ORDER BY number";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                row.Add((byte)Params.SetId, (int)Reader["setId"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        public Dictionary<int, object> GetReitingPlaceSetItems(int setId)
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `award_items`" +
                $" WHERE setId = '{setId}' ORDER BY number;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                var sRes = (string)Reader["resourse"];
                int iRes = (int)Helper.GetEnumElement<ResourseType>(sRes);

                row.Add((byte)Params.Resourse, iRes);
                row.Add((byte)Params.Amount, (int)Reader["amount"]);

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        /// <summary>
        /// Получить рейтинги
        /// </summary>
        public Dictionary<int, object> GetReitings()
        {

            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $" SELECT * FROM `reiting` ORDER BY `reiting`.`number` ASC";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var row = new Dictionary<byte, object>();

                string sId = (string)Reader["id"];
                int id = (int)Helper.GetEnumElement<ReitingType>(sId);
                //Logger.Log.Debug("ID " + sId + " " + id);

                row.Add((byte)Params.Id, id);
                row.Add((byte)Params.Name, (string)Reader["name"]);
                row.Add((byte)Params.Description, (string)Reader["description"]);

                var sPeriod = (string)Reader["awardPeriod"];
                Logger.Log.Debug("sPeriod " + sPeriod);
                if(sPeriod != "")
                {
                    //Logger.Log.Debug("not empty");
                    int iPeriod = (int)Helper.GetEnumElement<ReitingInterval>(sPeriod);
                    row.Add((byte)Params.awardPeriod, iPeriod);
                }

                result.Add(count, row);

                count++;
            }

            Conn.Close();

            return result;
        }

        #endregion

        #region Stocks

        public Dictionary<int, object> GetStocks()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT `stocks`.`id` as `stockId`, `stocks`.`name`, `stocks`.`start`, `stocks`.`end`, `sets`.`id` as `setId`, `sets`.`number` as `setNumber`, `sets`.`cost`, `items`.`id` as `itemId`, `items`.`number` as `itemNumber`, `items`.`resourse`, `items`.`resourseId`, `items`.`amount` FROM `stocks` " +
                $"LEFT JOIN `sets` ON `stocks`.`id` = `sets`.`stockId` " +
                $"LEFT JOIN `items` ON `sets`.`id` = `items`.`setId` " +
                $"ORDER BY `stocks`.`start`, `stocks`.`id`, `sets`.`number`, `items`.`number`;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int stockId = 0;
            int setId = 0;

            var stock = new Dictionary<byte, object>();
            var set = new Dictionary<byte, object>();

            var sets = new Dictionary<int, object>();
            var items = new Dictionary  <int, object>();        

            while (Reader.Read())
            {
                Logger.Log.Debug((int)Reader["stockId"]);
                if(stockId != (int)Reader["stockId"])
                {
                    stockId = (int)Reader["stockId"];
                    stock = new Dictionary<byte, object>();
                    result.Add(stockId, stock);

                    sets = new Dictionary<int, object>();

                    stock.Add((byte)Params.sets, sets);
                    stock.Add((byte)Params.Name, (string)Reader["name"]);

                    //stock.Add((byte)Params.Start, );
                    //stock.Add((byte)Params.End, );
                }

                if (setId != (int)Reader["setId"])
                {
                    setId = (int)Reader["setId"];
                    set = new Dictionary<byte, object>();
                    sets.Add(setId, set);

                    items = new Dictionary<int, object>();

                    set.Add((byte)Params.items, items);
                    set.Add((byte)Params.Cost, (int)Reader["cost"]);
                }

                var item = new Dictionary<byte, object>();

                item.Add((byte)Params.Amount, (int)Reader["amount"]);
                item.Add((byte)Params.Resourse, (string)Reader["resourse"]);
                item.Add((byte)Params.ResourseId, (string)Reader["resourseId"]);

                //Logger.Log.Debug(Reader["resourseId"]);

                //if(Reader["resourseId"] != null)
                //{
                //    item.Add((byte)Params.Resourse, (string)Reader["resourseId"]);
                //}
                
                items.Add((int)Reader["itemId"], item);
                //var clanId = (int)Reader["id"];

                //var clanDict = new Dictionary<byte, object>();

                //clanDict.Add((byte)Params.Name, (string)Reader["name"]);
                //clanDict.Add((byte)Params.OwnerId, (int)Reader["ownerId"]);
                //clanDict.Add((byte)Params.Image, (string)Reader["image"]);

                //var id = (int)Reader["id"];

                //result.Add(clanId, clanDict);
            }

            Logger.Log.Debug("result count " + result.Count);

            Conn.Close();

            return result;
        }

        public int SaveStock(Dictionary<byte, object> data)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            var id = (int)data[(byte)Params.Id];
            var name = (string)data[(byte)Params.Name];
            
            var start = DateTime.ParseExact((string)data[(byte)Params.Start], "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd H:mm:ss");
            var end = DateTime.ParseExact((string)data[(byte)Params.End], "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd H:mm:ss");

            Comm.CommandText =
                $"INSERT INTO `stocks`" +
                $" (id, name, start, end)" +
                $" VALUES " +
                $" ('{id}', '{name}', '{start}', '{end}')" +
                $" ON DUPLICATE KEY UPDATE" +
                $" name = '{name}'," +
                $" start = '{start}'," +
                $" end = '{end}'; " +
                $" SELECT MAX(id) FROM stocks; ";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            if(id == 0)
            while (Reader.Read())
            {
                id = (int)Reader["Max(id)"];
            }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return id;
                
        }

        public int SaveSet(Dictionary<byte, object> data)
        {
            var id = (int)data[(byte)Params.Id];
            var stockId = (int)data[(byte)Params.StockId];
            var number = (int)data[(byte)Params.Number];
            var cost = (int)data[(byte)Params.Cost];

            //Logger.Log.Debug("id " + id);
            //Logger.Log.Debug("setId " + stockId);
            //Logger.Log.Debug("number " + number);
            //Logger.Log.Debug("cost " + cost);

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"INSERT INTO `sets` " +
                $" (id, stockId, number, cost) " +
                $" VALUES " +
                $" ('{id}', '{stockId}', {number}, {cost}) " +
                $" ON DUPLICATE KEY UPDATE " +
                $" stockId = '{stockId}' ," +
                $" number = '{number}', " +
                $" cost = '{cost}'; " +
                $" SELECT MAX(id) FROM sets; ";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            if (id == 0)
                while (Reader.Read())
                {
                    id = (int)Reader["Max(id)"];
                }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return id;

        }

        public int SaveItem(Dictionary<byte, object> data)
        {
            var id = (int)data[(byte)Params.Id];
            var setId = (int)data[(byte)Params.SetId];
            var number = (int)data[(byte)Params.Number];
            var amount = (int)data[(byte)Params.Amount];
            var resourse = (string)data[(byte)Params.Resourse];
            var resourseId = (string)data[(byte)Params.ResourseId];


            //Logger.Log.Debug("id " + id);
            //Logger.Log.Debug("setId " + stockId);
            //Logger.Log.Debug("number " + number);
            //Logger.Log.Debug("cost " + cost);
            //return;

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"INSERT INTO `items` " +
                $" (id, setId, number, amount, resourse, resourseId) " +
                $" VALUES " +
                $" ('{id}', '{setId}', '{number}', '{amount}', '{resourse}', '{resourseId}') " +
                $" ON DUPLICATE KEY UPDATE " +
                $" setId = '{setId}' ," +
                $" number = '{number}', " +
                $" amount = '{amount}', " +
                $" resourse = '{resourse}', " +
                $" resourseId = '{resourseId}'; " +
                $" SELECT MAX(id) FROM items; ";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            if (id == 0)
                while (Reader.Read())
                {
                    id = (int)Reader["Max(id)"];
                }

            //Logger.Log.Debug($"save stock => {Result}");

            Conn.Close();

            return id;

        }

        public Dictionary<int, object> GetStocksList()
        {
            var result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `stocks` " + 
                $"order by start;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            int count = 0;
            while (Reader.Read())
            {
                var stock = new Dictionary<byte, object>();

                stock.Add((byte)Params.Id,      (int)Reader["id"]);
                stock.Add((byte)Params.Name,    (string)Reader["name"]);
                //stock.Add((byte)Params.Start,   (string)Reader["start"]);
                //stock.Add((byte)Params.End,     (string)Reader["end"]);

                result.Add(count, stock);

                count++;
            }

            Logger.Log.Debug("result count " + result.Count);

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetStock(int id)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `stocks` " +
                $"WHERE id=\"{id}\";";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.Id, (int)Reader["id"]);
                result.Add((byte)Params.Name, (string)Reader["name"]);
                result.Add((byte)Params.Start, Reader["start"].ToString());
                result.Add((byte)Params.End, Reader["end"].ToString());
            }
            Conn.Close();


            Comm.CommandText =
                $"SELECT * FROM `sets` " +
                $"WHERE stockId = \"{id}\" " +
                $"ORDER BY number;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            Reader = Comm.ExecuteReader();
            
            int count = 0;
            var sets = new Dictionary<int, object>();

            while (Reader.Read())
            {
                var set = new Dictionary<byte, object>();

                set.Add((byte)Params.Id, (int)Reader["id"]);
                set.Add((byte)Params.Number, (int)Reader["number"]);
                set.Add((byte)Params.Cost, (int)Reader["cost"]);

                sets.Add(count, set);

                count++;
            }

            result.Add((byte)Params.sets, sets);

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetSet(int id)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `sets` " +
                $"WHERE id=\"{id}\";";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.Id, (int)Reader["id"]);
                result.Add((byte)Params.StockId, (int)Reader["stockId"]);
                result.Add((byte)Params.Number, (int)Reader["number"]);
                result.Add((byte)Params.Cost, (int)Reader["cost"]);
            }
            Conn.Close();


            Comm.CommandText =
                $"SELECT * FROM `items` " +
                $"WHERE setId = \"{id}\" " +
                $"ORDER BY number;";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            Reader = Comm.ExecuteReader();

            int count = 0;
            var items = new Dictionary<int, object>();

            while (Reader.Read())
            {
                var item = new Dictionary<byte, object>();

                item.Add((byte)Params.Id, (int)Reader["id"]);
                item.Add((byte)Params.Number, (int)Reader["number"]);
                item.Add((byte)Params.Resourse, (string)Reader["resourse"]);
                item.Add((byte)Params.ResourseId, (string)Reader["resourseId"]);
                item.Add((byte)Params.Amount, (int)Reader["amount"]);

                items.Add(count, item);

                count++;
            }

            result.Add((byte)Params.items, items);

            Conn.Close();

            return result;
        }

        public Dictionary<byte, object> GetItem(int id)
        {
            var result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText =
                $"SELECT * FROM `Items` " +
                $"WHERE id=\"{id}\";";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                result.Add((byte)Params.Id, (int)Reader["id"]);
                result.Add((byte)Params.SetId, (int)Reader["setId"]);
                result.Add((byte)Params.Number, (int)Reader["number"]);
                result.Add((byte)Params.Resourse, (string)Reader["resourse"]);
                result.Add((byte)Params.ResourseId, (string)Reader["resourseId"]);
                result.Add((byte)Params.Amount, (int)Reader["amount"]);
            }
            Conn.Close();
            
            return result;
        }

        #endregion

        #region Dragon Save Data
        public Dictionary<byte, object> GetUserSaveData(string login)
        {
            var Result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText = $"SELECT * FROM `DragonData` WHERE login='{login}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {   
                var Progress = (string)Reader["progress"];
                //Result.Add((byte)Params.SaveData, Progress);

                Logger.Log.Debug($"loaded for user {login} Progress {Progress} ");
            }

            Conn.Close();

            return Result;
        }

        public void SaveProgress(string login, string Progress)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"INSERT INTO `DragonData` (login, progress) " +
                $" VALUES('{login}', '{Progress}') " +
                $" ON DUPLICATE KEY UPDATE progress = '{Progress}'";

            Logger.Log.Debug($" save data => [{login}] => [{Progress}]");

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

           var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save result => {Result}");

            Conn.Close();
        }

        #region Shop

        public ShopItem GetShopItem(string ItemName)
        {
            //var Result = new Dictionary<byte, object>();

            var Result = new ShopItem();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText = $"SELECT * FROM `DragonShop` WHERE name='{ItemName}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                Result.item_id = (int)Reader["id"];

                Result.title = (string)Reader["title"];

                Result.photo_url= (string)Reader["photo_url"];

                Result.price = (int)Reader["price"];

                Logger.Log.Debug($"load info for item {ItemName} ");
            }

            Conn.Close();

            return Result;
        }

        public void AddShopOrder(string OrderId, string ItemId, string Price, string UserId, string Date)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"INSERT INTO `DragonOrder` (order_id, item_id, item_price, user_id, date, status)" +
                $" VALUES('{OrderId}', '{ItemId}', '{Price}', '{UserId}', '{Date}', 'new')";
                //$" ON DUPLICATE KEY UPDATE progress = '{Progress}'";

            Logger.Log.Debug($"add new order {OrderId}");

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var Result = Comm.ExecuteNonQuery();

            Logger.Log.Debug($"save order {OrderId} result => {Result}");

            Conn.Close();
        }

        public ShopOrder GetShopOrder(string OrderId)
        {
            //var Result = new Dictionary<byte, object>();

            var Result = new ShopOrder();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText = $"SELECT * FROM `DragonOrder` WHERE order_id='{OrderId}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                Result.order_id = (string)Reader["order_id"];

                Result.item_id = (string)Reader["item_id"];

                Result.item_price = (string)Reader["item_price"];

                Result.user_id = (string)Reader["user_id"];

                Logger.Log.Debug($"load info for order {OrderId} ");
            }

            Conn.Close();

            return Result;
        }

        public Dictionary<byte,object> GetShopItems()
        {
            var Result = new Dictionary<byte, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            Comm.CommandText = $"SELECT * FROM `DragonShop`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            var ItemCounter = 0; 

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var _ShopItem = new Dictionary<byte, object>();

               var item_id = (int)Reader["id"];
                _ShopItem.Add((byte)Params.ShopItemId, item_id);

                var item_name = (string)Reader["name"];
                _ShopItem.Add((byte)Params.ShopItemName, item_name);

                var title = (string)Reader["title"];
                _ShopItem.Add((byte)Params.ShopItemTitle, title);

                var photo_url = (string)Reader["photo_url"];
                _ShopItem.Add((byte)Params.ShopItemPhoto, photo_url);

                var price = (int)Reader["price"];
                _ShopItem.Add((byte)Params.ShopItemPrice, price);

                Result.Add((byte)ItemCounter++, _ShopItem);
            }

            Conn.Close();

            return Result;
        }

        #endregion



        //public void SaveCoin(string login, int Coins)
        //{
        //    MySqlConnection Conn = GetDBConnection();
        //    MySqlCommand Comm = Conn.CreateCommand();

        //    Comm.CommandText = $"INSERT INTO DragonData (login, coins) " +
        //        $" VALUES('{login}', '{Coins}') " +
        //        $" ON DUPLICATE KEY UPDATE coins = {Coins}";

        //    try
        //    {
        //        Conn.Open();
        //        //Log.Debug("connection succes =>" + Conn.Database);
        //    }
        //    catch (Exception Exc)
        //    {
        //        ////logger.log.debug("Error: " + Exc.Message);
        //    }

        //    Comm.ExecuteNonQuery();

        //    Conn.Close();
        //}

        //public void SaveLevel(string login, int Level)
        //{
        //    MySqlConnection Conn = GetDBConnection();
        //    MySqlCommand Comm = Conn.CreateCommand();

        //    Comm.CommandText = $"INSERT INTO DragonData (login, openLevel) " +
        //        $" VALUES('{login}', '{Level}') " +
        //        $" ON DUPLICATE KEY UPDATE openLevel = {Level}";

        //    try
        //    {
        //        Conn.Open();
        //        //Log.Debug("connection succes =>" + Conn.Database);
        //    }
        //    catch (Exception Exc)
        //    {
        //        ////logger.log.debug("Error: " + Exc.Message);
        //    }

        //    Comm.ExecuteNonQuery();

        //    Conn.Close();
        //}

        #endregion

        #region Roles

        public Dictionary<byte,object> LoadRoleData(RoleType roleType)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `roles` WHERE roleId='{roleType}'";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                throw Exc;
            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<byte, object>();

            while (Reader.Read())
            {
                result.Add((byte)Params.RoleId, (string)Reader["roleId"]);

                result.Add((byte)Params.RoleName, (string)Reader["name"]);
                result.Add((byte)Params.RoleDescription, (string)Reader["description"]);
                result.Add((byte)Params.RoleMaleUrl, (string)Reader["maleAvatarUrl"]);
                result.Add((byte)Params.RoleFemaleUrl, (string)Reader["femaleAvatarUrl"]);
                result.Add((byte)Params.RoleUnicUrl, (string)Reader["unicAvatarUrl"]);             
            }

            Conn.Close();

            return result;
        }

        public Dictionary<string, object> LoadRoles()
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = $"SELECT * FROM `roles`";

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {

            }

            MySqlDataReader Reader = Comm.ExecuteReader();

            var result = new Dictionary<string, object>();
            //byte roleCounter = 0;

            while (Reader.Read())
            {
                var role = new Dictionary<byte, object>();

                var roleId = (string)Reader["roleId"];
                role.Add((byte)Params.RoleId, roleId);

                role.Add((byte)Params.RoleName, (string)Reader["name"]);
                role.Add((byte)Params.RoleDescription, (string)Reader["description"]);
                role.Add((byte)Params.RoleMaleUrl, (string)Reader["maleAvatarUrl"]);
                role.Add((byte)Params.RoleFemaleUrl, (string)Reader["femaleAvatarUrl"]);
                role.Add((byte)Params.RoleUnicUrl, (string)Reader["unicAvatarUrl"]);

                result.Add(roleId, role);
            }

            Conn.Close();

            return result;
        }

        #endregion

        #region User Items


        /// <summary>
        /// получить словарь айдишников итемов, принадлжещих юзеру
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public Dictionary<int, object> GetUserItems(Client User)
        {
            var Result = new Dictionary<int, object>();

            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();
            //Comm.CommandText = $"SELECT * FROM `UserItems` WHERE userId='{User.UserDbId}'";

            Comm.CommandText =
$" SELECT" +
$" Items.Type, Items.Name, Items.Description, Items.Stack, Items.ImageId,Items.ModelId," +
$" UserItems.Id, UserItems.Quality, UserItems.Count, UserItems.Level," +
$" ItemParameter.ParameterType, ItemParameter.ParameterValue" +
$" FROM UserItems" +
$" LEFT JOIN Items ON Items.Id = UserItems.ItemId" +
$" LEFT JOIN ItemParameter ON ItemParameter.ItemId = UserItems.ItemId" +
$" WHERE UserItems.UserId = '{User.userDbId}'";
            //$" SELECT * " +
            //$" FROM UserItems " +
            //$" INNER JOIN  Items ON Items.id = UserItems.itemId" +
            //$" INNER JOIN  ItemParameter ON ItemParameter.id = UserItems.itemId" +
            //$" WHERE UserItems.userId = '{User.UserDbId}'";

            Logger.Log.Debug($"Comm.CommandText {Comm.CommandText}");

            try
            {
                Conn.Open();
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }

            //выгружаем итемы

            int ParameterCount = 0;

            MySqlDataReader Reader = Comm.ExecuteReader();

            while (Reader.Read())
            {
                var UserItem = new Dictionary<byte, object>();

                //общие праметры
                var ItemId = (int)Reader["Id"];

                //если итем дублируется, то это значит, что мы извлекаем его параметр
                if (Result.ContainsKey((byte)ItemId))
                {
                    UserItem = (Dictionary<byte, object>)Result[ItemId];

                    try
                    {
                        //var Parameters = (Dictionary<byte, object>)UserItem[(byte)Params.Parameter];
                        //var Parameter = new Dictionary<byte, object>();

                        //var ParameterType = (int)Reader["ParameterType"];
                        //Parameter.Add((byte)Params.Type, ParameterType);

                        //var ParameterValue = (int)Reader["ParameterValue"];
                        //Parameter.Add((byte)Params.Value, ParameterValue);

                        //Parameters.Add((byte)ParameterCount++, Parameter);
                    }
                    catch { }

                    continue;
                }

                //UserItem.Add((byte)Params.Id, ItemId);

                try
                {
                    //var Parameters = new Dictionary<byte, object>();

                    //var Parameter = new Dictionary<byte, object>();

                    //var ParameterType = (int)Reader["ParameterType"];
                    //Parameter.Add((byte)Params.Type, ParameterType);

                    //var ParameterValue = (int)Reader["ParameterValue"];
                    //Parameter.Add((byte)Params.Value, ParameterValue);

                    //Parameters.Add((byte)ParameterCount++, Parameter);

                    //UserItem.Add((byte)Params.Parameter, Parameters);
                }
                catch { }

                //var ItemDBId = (int)Reader["ItemId"];
                //UserItem.Add((byte)Params.DBId, ItemDBId);

                //var ItemType = (int)Reader["Type"];
                //UserItem.Add((byte)Params.Type, ItemType);

                //var ItemName = (string)Reader["Name"];
                //UserItem.Add((byte)Params.Name, ItemName);

                //var ItemDescription = (string)Reader["Description"];
                //UserItem.Add((byte)Params.Description, ItemDescription);

                //var ItemQuality = (int)Reader["Quality"];
                //UserItem.Add((byte)Params.Quality, ItemQuality);

                //var ItemCount = (int)Reader["Count"];
                //UserItem.Add((byte)Params.Count, ItemCount);

                //var ItemLevel = (int)Reader["Level"];
                //UserItem.Add((byte)Params.Level, ItemLevel);

                //Result.Add(ItemId, UserItem);
            }


            Conn.Close();

            return Result;
        }

       

        /// <summary>
        /// получить все итемы юзера
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        /// Старый метод, возможно из другого проекта
        //public Dictionary<byte, object> GetItem(int ItemId)
        //{
        //    var Result = new Dictionary<byte, object>();

        //    MySqlConnection Conn = GetDBConnection();
        //    MySqlCommand Comm = Conn.CreateCommand();
        //    Comm.CommandText = $"SELECT * FROM `Items` WHERE id='{ItemId}'";

        //    try
        //    {
        //        Conn.Open();
        //    }
        //    catch (Exception Exc)
        //    {
        //        ////logger.log.debug("Error: " + Exc.Message);
        //    }

        //    MySqlDataReader Reader = Comm.ExecuteReader();


        //    while (Reader.Read())
        //    {
        //        //запоминаем айди итема
        //        //Result.Add((byte)Params.Id, ItemId);

        //        //var ItemType = (int)Reader["type"];
        //        //Result.Add((byte)Params.Type, ItemType);

        //        //var ItemQuality = (int)Reader["quality"];
        //        //Result.Add((byte)Params.Quality, ItemQuality);

        //        //var ItemName = (string)Reader["name"];
        //        //Result.Add((byte)Params.Name, ItemName);

        //        //var ItemDescription = (string)Reader["description"];
        //        //Result.Add((byte)Params.Description, ItemDescription);

        //        //var ItemLevel = (int)Reader["level"];
        //        //Result.Add((byte)Params.Level, ItemLevel);

        //        //var ItemCount = (int)Reader["count"];
        //        //Result.Add((byte)Params.Count, ItemCount);

        //        //var ItemStack = (int)Reader["stackLimit"];
        //        //Result.Add((byte)Params.Stack, ItemStack);

        //        //var ItemLive = (bool)Reader["Live"];
        //        //Result.Add((byte)Params.ItemLive, ItemLive);

        //        //var ItemDefence = (int)Reader["defence"];
        //        //Result.Add((byte)Params.ItemDefence, ItemDefence);

        //        //var ItemBlock = (int)Reader["block"];
        //        //Result.Add((byte)Params.ItemBlock, ItemBlock);

        //        //var ItemEvade = (int)Reader["evade"];
        //        //Result.Add((byte)Params.ItemEvade, ItemEvade);

        //        //var ItemDamage = (int)Reader["damage"];
        //        //Result.Add((byte)Params.ItemDamage, ItemDamage);

        //        //var ItemCritRate = (int)Reader["critRate"];
        //        //Result.Add((byte)Params.ItemCritRate, ItemCritRate);

        //        //var ItemCritPower = (int)Reader["critPower"];
        //        //Result.Add((byte)Params.ItemCritPower, ItemCritPower);

        //        //var ItemAtackSpeed = (float)Reader["atackSpeed"];
        //        //Result.Add((byte)Params.ItemAtackSpeed, ItemAtackSpeed);

        //        //var ItemPower = (int)Reader["Power"];
        //        //Result.Add((byte)Params.ItemPower, ItemPower);
        //    }

        //    Conn.Close();

        //    return Result;
        //}

        #endregion    
      

        /// <summary>
        /// Сохранение рейтинга пользователя в таблице
        /// </summary>
        public void Save_User_Rating(int user_id, int points, int hits, int crystals, int distance)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                ////logger.log.debug("Error: " + Exc.Message);
            }


            Comm.CommandText = "INSERT INTO Rating (user_id, points, hits, crystals, distance) VALUES(" + user_id + ", " + points + ", " + hits + " , " + crystals + " , " + distance + ") ON DUPLICATE KEY UPDATE points = " + points + ", hits = " + hits + ", crystals = " + crystals + ", distance = " + distance;

            Comm.ExecuteNonQuery();

            Conn.Close();
        }

        public void Reset_User_Rating(Client User)
        {
            MySqlConnection Conn = GetDBConnection();
            MySqlCommand Comm = Conn.CreateCommand();

            Comm.CommandText = "INSERT INTO Rating (user_id, points, hits, crystals, distance, startCheck, midleCheck, userHp, userCry, userX) " +
                "VALUES(" + User.userDbId + ", 0, 0, 0, 0, 0, 0, 0, 0, 0 )" +
                "ON DUPLICATE KEY UPDATE points=0, hits=0, crystals=0, distance=0, startCheck = 0, midleCheck = 0," +
                "userHp=0, userCry=0, userX=0, lastX=0, lastZ=0";

            try
            {
                Conn.Open();
                //Log.Debug("connection succes =>" + Conn.Database);
            }
            catch (Exception Exc)
            {
                //////logger.log.debug("Error: " + Exc.Message);
            }

            Comm.ExecuteNonQuery();

            Conn.Close();
        }

      
    }
}
