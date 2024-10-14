using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;
using Share;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Mafia_Server
{
    public class Server : ApplicationBase
    {
        public static Server inst;

        UMoney uMoney;
        GifConverter gifConverter;

        DBManager dbManagerInst;
        ManagerRooms managerRooms;

        SkillManager skillManager;
        ExtrasManager extrasManager;
        
        ManagerEvent managerEvent;

        ManagerUser managerUser;

        WebServer webServer;

        CensureFilter censureFilter;

        ManagerDuels managerDuels;

        ManagerReitings managerReitings;

        //список пользователей онлайн
        public List<Client> LIST_Online_Users = new List<Client>();

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            //var TokenData = (Dictionary<byte, object>)initRequest.ResponseObject;
            //var CustomInitData = (Dictionary<byte,object>)initRequest.InitObject;

            if (initRequest.ResponseObject != null)
            {
                Logger.Log.Debug($"ResponseObject {initRequest.ResponseObject}");
            }

            if (initRequest.UserData != null)
            {
                Logger.Log.Debug($"UserData {initRequest.UserData}");
            }

            if (initRequest.InitObject != null)
            {
                Logger.Log.Debug($"InitObject {initRequest.InitObject}");
            }

            return new Client(initRequest);
        }

        protected override async void Setup()
        {
            inst = this;

            var file = new FileInfo(Path.Combine(BinaryPath, "Log4net.config"));
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }

            //Logger.Log.Debug($"Проверка кириллицы");

            uMoney = new UMoney();

            //gifConverter = new GifConverter();

            //добавляем менеджер быз данных
            dbManagerInst = new DBManager();

			 RoleHelper.LoadRoles();					   
            skillManager = new SkillManager();
            extrasManager = new ExtrasManager();

            managerRooms = new ManagerRooms();

            managerUser = new ManagerUser();

            censureFilter = new CensureFilter();

            managerDuels = new ManagerDuels();

            managerReitings = new ManagerReitings();

            Logger.Log.Debug($"server ready");


            //ManagerReitings.inst.CreateCompetition();
            //ManagerReitings.inst.CreateTestReitingRow(1000);
            //ManagerReitings.inst.GetReiting(ManagerReitings.inst.daylyStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.WinGame.ToString(), RaitingEventWinGameSubType.Mafia.ToString());
            //ManagerReitings.inst.GetReiting(ReitingInterval.all, ReitingType.Duels);

            //var reitings = DBManager.Inst.GetReitings();
            //Logger.Log.Debug("reitings count " + reitings.Count);

            //dbManagerInst.GetClansDuelTop();
            //managerDuels.CreateClansPairs();

            //managerDuels.CreateMissionsForClanPair(7, 1, 6);

            //managerDuels.EndDuelDay(28);
            //Logger.Log.Debug("Pay for upgrade " + DBManager.Inst.TryPayForUpgrade(6, 100));
            //Logger.Log.Debug("price " + DBManager.Inst.GetPriceForUpgade(ClansUpgardesId.PlusExp));
            //Logger.Log.Debug("update " + DBManager.Inst.TryPayForUpgrade(6, 100));

            //Logger.Log.Debug("Check balance and get price " + DBManager.Inst.CheckBalanceForUpgradeAndGetPrice(6, ClansUpgardesId.RoomDisount));
            //DBManager.Inst.BuyClanUpgrade(6, 100, ClansUpgardesId.PlusExp);
            //DBManager.Inst.TryBuyClanUpgrade(6, ClansUpgardesId.PlusExp);

            //ManagerDuels.inst.RotateWheel(5);

            //var count = DBManager.Inst.GetClanWinsInDuel(28, 6);
            //Logger.Log.Debug("COUNT " + count);

            //DBManager.Inst.AddWinGameInDuelTime(28, 6, 28);
            //ManagerDuels.inst.EndDuelDay(28);

            //ManagerDuels.inst.AwardClanUsers(24, 6);
            //ManagerDuels.inst.AwardUser(28, ResourseType.coins, 1000);
            //ManagerDuels.inst.AddRandomExtraToUser(28, 20, ExtraType.Clan);
            //DBManager.Inst.AddRespectsToUser(28, 5);

            //dbManagerInst.IncrementUserDuelMission(22,25,DuelsAchiveId.WinGameFor08,23);

            //int id = dbManagerInst.CreateDuelDay("2023-05-12 12:00:00", "2023-05-12 15:00:00");
            //Logger.Log.Debug("new duel day id " + id);
        }

        protected override void TearDown()
        {
            //if (webServer != null) webServer.Stop();
            Logger.Log.Debug($"web server STOP");

            Logger.Log.Debug($"mafia server STOP");
        }

        public void Reload()
        {
            this.Setup();
        }
        
    }
}
