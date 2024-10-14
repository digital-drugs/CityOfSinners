using ExitGames.Concurrency.Fibers;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Mafia_Server
{
    public class ManagerReitings
    {
        public static ManagerReitings inst;

        /// <summary>
        /// Недельный пулфайбер
        /// </summary>
        public PoolFiber weeeklyPoolFiber = new PoolFiber();

        /// <summary>
        /// Дневной пулфайбер пулфайбер
        /// </summary>
        public PoolFiber daylyPoolFiber = new PoolFiber();

        /// <summary>
        /// Сдвиг времени для недельного рейтинга кланов
        /// </summary>
        TimeSpan weeklySpan;

        public DateTime weeklyStart, weeklyEnd;

        public DateTime seasonStart, seasonEnd;


        /// <summary>
        /// Сдвиг времени для дневных рейтингов
        /// </summary>
        TimeSpan daylySpan;
        /// <summary>
        /// Временные рамки для дневного рейтинга
        /// </summary>
        public DateTime daylyStart, daylyEnd;

        /// <summary>
        /// Текущее соревнование кланов по опыту
        /// </summary>
        int currentWeeklyComp = 0;

        public Dictionary<byte,object> periods = new Dictionary<byte, object>();

        /// <summary>
        /// Констурктор
        /// </summary>
        public ManagerReitings()
        {
            inst = this;
            Logger.Log.Debug("ManagerReitings created");

            //Недельный сдвиг: пятница(5) 22 часа
            weeklySpan = TimeSpan.FromDays(5 - 1) + TimeSpan.FromHours(22);
            //Дневной сдвиг: 22 часа
            daylySpan = TimeSpan.FromHours(22);

            SetDaylyBorders();
            SetWeeeklyBorders();
            SetSeasonBorders();

            FillPeriods();
        }

        void FillPeriods()
        {
            periods.Add((byte)ReitingInterval.day, daylyEnd.ToString("yyyy-MM-dd H:mm:ss"));
            periods.Add((byte)ReitingInterval.week, weeklyEnd.ToString("yyyy-MM-dd H:mm:ss"));
            periods.Add((byte)ReitingInterval.season, seasonEnd.ToString("yyyy-MM-dd H:mm:ss"));
        }

        void SetSeasonBorders()
        {
            //Начала и окончания сезонов
            List<DateTime> seasonDates = new List<DateTime>();

            var now = DateTime.Now;
            var year = now.Year;
            //01.01
            seasonDates.Add(new DateTime(year,1,1));
            //01.04
            seasonDates.Add(new DateTime(year, 4, 1));
            //01.07
            seasonDates.Add(new DateTime(year, 7, 1));
            //01.10
            seasonDates.Add(new DateTime(year, 10, 1));

            seasonDates.Add(new DateTime(year + 1, 1, 1));

            for(int i = 0; i < seasonDates.Count; i++)
            {
                if (seasonDates[i] > now)
                {
                    seasonEnd = seasonDates[i];
                    seasonStart = seasonDates[i - 1];
                    break;
                }
            }

            Logger.Log.Debug("season start " + seasonStart);
            Logger.Log.Debug("season end " + seasonEnd);


        }

        void SetWeeeklyBorders()
        {
            //Получить текущее время
            var now = DateTime.Now;
            //Получить текущее смещение времени от начала недели
            var currentSpan = ManagerDuels.inst.GetWeeklyTimeSpan(now);
            //Наййти дату понедельника полночь
            var mon = now.Date - TimeSpan.FromDays(((int)now.DayOfWeek-1));
            //Logger.Log.Debug("Monday " + mon);

            //Logger.Log.Debug("CURRENT WEEK SPAN " + currentSpan);

            if (currentSpan < weeklySpan)
            {
                weeklyEnd = mon + weeklySpan;
                weeklyStart = weeklyEnd - TimeSpan.FromDays(7);
            }
            else
            {
                weeklyStart = mon + weeklySpan;
                weeklyEnd = weeklyStart + TimeSpan.FromDays(7);
            }

            //Logger.Log.Debug("WEEK START " + weeklyStart);
            //Logger.Log.Debug("WEEK END " + weeklyEnd);
        }

        void SetDaylyBorders()
        {
            var now = DateTime.Now;
            var date = DateTime.Today;
            var nowTime = now.TimeOfDay;

            Logger.Log.Debug("NOW " + now);
            Logger.Log.Debug("DATE " + date);
            Logger.Log.Debug("TIME " + nowTime);

            if (nowTime <= daylySpan)
            {
                daylyStart = date.AddDays(-1) + daylySpan;
                daylyEnd = date + daylySpan;
            }
            else
            {
                daylyStart = date + daylySpan;
                daylyEnd = date.AddDays(1) + daylySpan;
            }

            Logger.Log.Debug("START " + daylyStart);
            Logger.Log.Debug("END " + daylyEnd);
        }

        /// <summary>
        /// Создание недельного соревнования между кланами на опыт
        /// </summary>
        public void CreateCompetition()
        {
            var now = DateTime.Now;
            //Logger.Log.Debug("NOW " + now);
            //Добавить запись о недельном соревновании и получить его идентификатор
            //Logger.Log.Debug("CREATE WEEKLY COMPETITION");
            int newCompId = DBManager.Inst.CreateWeeklyExpCompetition(now.ToString("yyyy-MM-dd H:mm:ss"));
            //Logger.Log.Debug("newcompId " + newCompId);
            this.currentWeeklyComp = newCompId;

            var clans = DBManager.Inst.GetRandomClanList();

            int group_count = 0;
            int last_group_id = 0;
            for(int i = 0; i < clans.Count; i++)
            {
                if(group_count == 0)
                {
                    last_group_id = DBManager.Inst.CreateWeeklyExpCompetitionGroup(newCompId);
                }

                //Logger.Log.Debug(last_group_id + " " + clans[i]);
                group_count++;
                //Добавить запись клана с группой
                DBManager.Inst.CreateCompGroupClanRow(last_group_id, clans[i]);

                if(group_count == Options.weeklyExpClanCompGroupCount)
                {
                    group_count = 0;
                }
            }
        }

        public void EndWeeklyCompetition(int compId)
        {
            if(compId == 0)
            {
                return;
            }

            //Получить группы недельного соревнования
            //var groups = 
        }

        public void CreateReitingEventRow(string datetime, long userId, int clanId, string eventType, string eventSubType, int amount)
        {
            DBManager.Inst.CreateRaitingEventRow(datetime, userId, clanId, eventType, eventSubType, amount);
        }

        public Dictionary<int, object> GetReiting(string dateTime, string eventType, string eventSubType)
        {
            var result = DBManager.Inst.GetReiting(dateTime, eventType, eventSubType);

            Logger.Log.Debug("result count " + result.Count);
            foreach (var e in result)
            {
                var ed = (Dictionary<byte, object>)e.Value;
                Logger.Log.Debug(ed[(byte)Params.Amount]);
            }

            return result;
        }

        public Dictionary<int, object> GetReitings()
        {
            var reitings = DBManager.Inst.GetReitings();

            foreach(var r in reitings)
            {
                var data = (Dictionary<byte, object>)r.Value;
                Logger.Log.Debug("reiting id " + data[(byte)Params.Id]);

                var periods = DBManager.Inst.GetReitingPeriods((ReitingType)data[(byte)Params.Id]);

                data.Add((byte)Params.periods, periods);

                var sets = DBManager.Inst.GetReitingPlacesSets((ReitingType)data[(byte)Params.Id]);
                if (sets.Count > 0)
                {
                    foreach (var set in sets)
                    {
                        var setData = (Dictionary<byte, object>)set.Value;

                        var items = DBManager.Inst.GetReitingPlaceSetItems((int)setData[(byte)Params.SetId]);
                        setData.Add((byte)Params.items, items);
                    }

                    data.Add((byte)Params.sets, sets);
                }
            }

            return reitings;
        }

        public Dictionary<byte, object> GetReiting(ReitingInterval reitingInterval, ReitingType reitingType)
        {
            var result = new Dictionary<byte, object>();
            var reiting = new Dictionary<int, object>();

            //Пока заглушка по доступности рейтингов
            switch (reitingType)
            {
                case ReitingType.Respcts:
                    result.Add((byte)Params.message, "Рейтинг будет доступен во время ивента.");
                    return result;
                    break;
            }

            DateTime reitingStart = DateTime.MinValue;
            //Logger.Log.Debug(reitingStart);

            switch (reitingInterval)
            {
                case ReitingInterval.day:
                    reitingStart = this.daylyStart;
                    break;

                case ReitingInterval.week:
                    reitingStart = this.weeklyStart;
                    break;

                case ReitingInterval.season:
                    reitingStart = this.seasonStart;
                    break;

                case ReitingInterval.all:
                    reitingStart = DateTime.MinValue;
                    break;
            }

            switch (reitingType)
            {
                //01
                case ReitingType.Respcts:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.RespectsUp.ToString(), RaitingEventRespectsUpSubType.DiamondsDown.ToString());
                    break;
                //02
                case ReitingType.Lottery:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.LotteryGame.ToString(), "");
                    break;
                //03
                case ReitingType.DaylyRaiting:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.RaitingUp.ToString(), "");
                    break;
                //04
                case ReitingType.ExpFighter:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.ExpUp.ToString(), "");
                    break;
                //05
                case ReitingType.Auction:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.AuctionWin.ToString(), "");
                    break;
                //06
                case ReitingType.Peaceful:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.WinGame.ToString(), RaitingEventWinGameSubType.Peaceful.ToString());
                    break;
                //07
                case ReitingType.Mafia:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.WinGame.ToString(), RaitingEventWinGameSubType.Mafia.ToString());
                    break;
                //08
                case ReitingType.Maniac:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.WinGame.ToString(), RaitingEventWinGameSubType.Maniac.ToString());
                    break;
                //09
                case ReitingType.Duels:
                    //Неправильный рейтинг
                    //reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.DuelPoint.ToString(), "");
                    break;
                //10
                case ReitingType.DonsTasks:
                    reiting = GetReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.RespectsUp.ToString(), RaitingEventRespectsUpSubType.DonsTask.ToString());
                    break;
                //11
                case ReitingType.Clans:
                    //Неправильный рейтинг
                    //reiting = DBManager.Inst.GetClanReiting(reitingStart.ToString("yyyy-MM-dd H:mm:ss"), ReitingEventType.ExpUp.ToString(), "");
                    break;
            }

            result.Add((byte)Params.Rating, reiting);

            return result;
        }

        /// <summary>
        /// Ntcnjdst bltynbabrfnjhs gjkmpjdfntktq
        /// </summary>
        List<long> TestReitingUsers = new List<long> {3, 7, 16, 24, 25, 26, 28, 35, 37};
        /// <summary>
        /// Максимальное колличетво минут для разброса
        /// </summary>
        int maxDelaySeconds = 60 * 60 * 24 * 7*31;

        public void CreateTestReitingRow(int count = 1)
        {
            var now = DateTime.Now;

            Random r = new Random();
            var delay = r.Next(maxDelaySeconds);
            var timeDelay = TimeSpan.FromSeconds(delay);
            long userId = 1;
            int amount = 1;

            var newDate = now + timeDelay;

            Logger.Log.Debug("NOW " + now);
            Logger.Log.Debug("NEW DATE " + newDate);

            for(int i=0; i<count; i++)
            {
                delay = r.Next(maxDelaySeconds);
                timeDelay = TimeSpan.FromSeconds(delay);
                newDate = now - timeDelay;

                userId = TestReitingUsers[r.Next(TestReitingUsers.Count - 1)];
                amount = r.Next(1, 3);

                CreateReitingEventRow(newDate.ToString("yyyy-MM-dd H:mm:ss"), userId, amount, ReitingEventType.ExpUp.ToString(), "", amount);

            }
        }
    }
}
