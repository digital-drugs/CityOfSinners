using BEPUphysics.Constraints.TwoEntity.Joints;
using ExitGames.Concurrency.Fibers;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Mafia_Server
{
    public class ManagerDuels
    {
        public static ManagerDuels inst;

        public int currentDuelDayId = 0;
        public DateTime start;
        public DateTime end;

        public PoolFiber poolFiber = new PoolFiber();

        //длительность дуэли в секундах
        int duelTime = 3 * 60 * 60;

        //Тестовая длительность дуэли в секундах
        //int duelTime = 5;
        
        
        TimeSpan duelInterval;

        //Список начал дуэлей с начала недели
        List<TimeSpan> duelsSpans = new List<TimeSpan>();

        /// <summary>
        /// Дуэльное колесо (словарь сеторов)
        /// </summary>
        public Dictionary<int, Dictionary<byte, object>> DuelWheel = new Dictionary<int, Dictionary<byte, object>>();
        
        /// <summary>
        /// Уровни вознаграждений после дуэлей
        /// </summary>
        public Dictionary<int, object> AwardsLevels = new Dictionary<int, object>();

        /// <summary>
        /// Конструктор
        /// </summary>
        public ManagerDuels() 
        {
            inst = this;

            Logger.Log.Debug("ManagerDuels created");

            duelInterval = TimeSpan.FromSeconds(duelTime);
            poolFiber.Start();

            //Заполнить список начал дуэлей с начала недели - Пятница 12 и Суббота 12
            FillDuelsSpans();

            //Тестовое заполнение смещений начал дуэлей
            //TestFillDuelsSpans(1, 300);
            
            
            //Дополнить список смещений первым элементом + 7 дней
            duelsSpans.Add(duelsSpans.First() + TimeSpan.FromDays(7));

            //Показать список смещений дуэлей
            foreach(var ts in duelsSpans)
            {
                //Logger.Log.Debug("DUEL TIME SPAN " + ts);
            }



            //Получить текущее время
            var now = DateTime.Now;
            //Определить, включился ли сервак во время дуэли или вне дуэли - Запросить из базы данных дуэльный день, где старт меньше сейчас и конец больше сейчас
            var currentDuel = DBManager.Inst.TryGetCurrentDuelDay(now.ToString());
            
            //Если сервер стартовал во время дуэльного времени
            if (currentDuel.ContainsKey((byte)Params.Id))
            {
                Logger.Log.Debug("SERVER STARTED IN DUEL TIME");
                //заполнить данные текущего дуэльного дня
                UpdateStateOfManager((int)currentDuel[(byte)Params.Id], DateTime.Parse((string)currentDuel[(byte)Params.Start]), DateTime.Parse((string)currentDuel[(byte)Params.End]));

                //Запалнировать окончание дуэльного времени
                PlanStopDuelTime();
            }
            //Если сервер стартовал вне дуэльного времени, запланировать начало дуэльного времени
            else
            {
                Logger.Log.Debug("SERVER STARTED OUT OF DUEL TIME");

                PlanStartDuelTime();
            }

            //Заполнить дуэльное колесо
            FillDuelWheel(this.DuelWheel);
            AugmentDuelWheel();

            FillAwardsLevels();
        }


        /// <summary>
        /// Получить уровень вознаграждения по набранным очкам
        /// </summary>
        public int GetAwardLevel(int points)
        {
            //первоначально взять макс
            int result = AwardsLevels.Count - 1;

            for(int i=0; i<AwardsLevels.Count; i++)
            {
                var el = (Dictionary<byte, object>)AwardsLevels[i];
                if (points < (int)el[(byte)Params.limit])
                {
                    return i - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Наградить игроков клана
        /// </summary>
        public void AwardClanUsers(int clanPair, int clanId)
        {
            Logger.Log.Debug("clanPair " + clanPair);
            Logger.Log.Debug("clanId " + clanId);

            var users = DBManager.Inst.GetClanDuelUsers(clanPair, clanId);
            Logger.Log.Debug("users count " + users.Count);
            int index = 0;
            foreach(var e in users)
            {
                var data = (Dictionary<byte, object>)e.Value;
                index = index + (int)data[(byte)Params.Amount];
            }
            Logger.Log.Debug("index " + index);

            //TEST IINDEX
            index = 900;

            var awardIndex = this.GetAwardLevel(index);
            Logger.Log.Debug("award index " + awardIndex);

            //Если клан не достиг ни одного уровня вознагарждений
            if (awardIndex < 0) return;

            Logger.Log.Debug("NEXT");

            Logger.Log.Debug("LEVELS COUNT" + AwardsLevels.Count);

            var level = (Dictionary<byte, object>)AwardsLevels[awardIndex];
            var prizes = (Dictionary<int,object>)level[(byte)Params.prizes];

            foreach(var p in prizes)
            {
                var pd = (Dictionary<byte, object>)p.Value;

                ResourseType res = (ResourseType)pd[(byte)Params.Resourse];
                int amount = (int)pd[(byte)Params.Amount];
                //Logger.Log.Debug("Resourse " + res + " " + amount);

                foreach (var u in users)
                {
                    var pu = (Dictionary<byte, object>)u.Value;
                    long userId = (long)pu[(byte)Params.UserId];
                    //Logger.Log.Debug(userId);                    
                    AwardUser(userId, res, amount);
                }
            }

        }


        public void AwardUser(long userId, ResourseType resourse, int amount)
        {
            switch (resourse)
            {
                case ResourseType.coins:
                    DBManager.Inst.AddCoinsToUser(userId, amount);
                    break;

                case ResourseType.diamonds:
                    DBManager.Inst.AddDiamondsToUser(userId, amount);
                    break;

                case ResourseType.energy:
                    DBManager.Inst.AddEnergyToUser(userId, amount);
                    break;

                case ResourseType.random_clan_extra:
                    AddRandomExtraToUser(userId, amount, ExtraType.Clan);
                    break;

                case ResourseType.rand_usual_extra:
                    AddRandomExtraToUser(userId, amount, ExtraType.General);
                    break;

                case ResourseType.respects:
                    DBManager.Inst.AddRespectsToUser(userId, amount);
                    break;

                default:
                    Logger.Log.Debug("RESORSE TYPE NOT HANDLED");
                    break;
            }
        }

        /// <summary>
        /// Добавить пользователю случайные экстры определённого типа
        /// </summary>
        public void AddRandomExtraToUser(long userId, int amount, ExtraType extraType)
        {
            var extras = DBManager.Inst.GetExtrasByType(extraType);

            Logger.Log.Debug("EXTRAS COUNT " + extras.Count);

            var extrasToUser = new Dictionary<ExtraEffect, int>();

            foreach(var e in extras)
            {
                var ed = (Dictionary<byte, object>)e.Value;

                Logger.Log.Debug("EX ID " + ed[(byte)Params.ExtraId]);
            }

            Random r = new Random();

            for(int i=0; i<amount; i++)
            {
                int index = r.Next(extras.Count);

                var rExtra = (Dictionary<byte, object>)extras[index];
                var extraId = (ExtraEffect)rExtra[(byte)Params.ExtraId];

                if(extrasToUser.ContainsKey(extraId))
                {
                    extrasToUser[extraId] = extrasToUser[extraId] + 1;
                }
                else
                {
                    extrasToUser.Add(extraId, 1);
                }
            }

            foreach(var ex in extrasToUser)
            {
                var extraId = ex.Key;
                var count = ex.Value;

                //Logger.Log.Debug("extra to user " + extraId + " " + count);
                DBManager.Inst.AddExtraToUser(userId, extraId.ToString(), count);
            }
        }

        public void FillAwardsLevels()
        {
            AwardsLevels = DBManager.Inst.GetAwardsLevels();

            var el = (Dictionary<byte, object>)AwardsLevels[0];
            //Logger.Log.Debug("el amount " + el[(byte)Params.Amount]);
            el.Add((byte)Params.limit, (int)el[(byte)Params.Amount]);

            for(int i=1; i<AwardsLevels.Count; i++)
            {
                var curEl = (Dictionary<byte, object>)AwardsLevels[i];
                var prevEl = (Dictionary<byte, object>)AwardsLevels[i-1];

                curEl.Add((byte)Params.limit, (int)prevEl[(byte)Params.limit] + (int)curEl[(byte)Params.Amount]);
            }

            foreach(var e in AwardsLevels)
            {
                var data = (Dictionary<byte, object>)e.Value;

                var prizes = DBManager.Inst.GetAwardsForLevel((int)data[(byte)Params.Number]);
                data.Add((byte)Params.prizes, prizes);
            }

            //foreach (var e in AwardsLevels)
            //{
            //    var data = (Dictionary<byte, object>)e.Value;

            //    Logger.Log.Debug("Number " + (int)data[(byte)Params.Number]);
            //    Logger.Log.Debug("Amount " + (int)data[(byte)Params.Amount]);
            //    Logger.Log.Debug("Limit " + (int)data[(byte)Params.limit]);
            //}
        }

        /// <summary>
        /// Заполнить дуэльное колесо
        /// </summary>
        public void FillDuelWheel(Dictionary<int, Dictionary<byte, object>> wheel)
        {
            Dictionary<byte, object> sector;
            int c = 0;
            //1
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (89d / 2d));
            sector.Add((byte)Params.Amount, 1000);
            sector.Add((byte)Params.Resourse, ResourseType.coins);
            wheel.Add(c++, sector);
            //2
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (89d / 2d));
            sector.Add((byte)Params.Amount, 5000);
            sector.Add((byte)Params.Resourse, ResourseType.coins);
            wheel.Add(c++, sector);
            //3
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (10d / 4d));
            sector.Add((byte)Params.Amount, 10000);
            sector.Add((byte)Params.Resourse, ResourseType.coins);
            wheel.Add(c++, sector);
            //4
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (10d / 4d));
            sector.Add((byte)Params.Amount, 3);
            sector.Add((byte)Params.Resourse, ResourseType.diamonds);
            wheel.Add(c++, sector);
            //5
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (10d / 4d));
            sector.Add((byte)Params.Amount, 5);
            sector.Add((byte)Params.Resourse, ResourseType.diamonds);
            wheel.Add(c++, sector);
            //6
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (10d / 4d));
            sector.Add((byte)Params.Amount, 10);
            sector.Add((byte)Params.Resourse, ResourseType.diamonds);
            wheel.Add(c++, sector);
            //7
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (1d / 2d));
            sector.Add((byte)Params.Amount, 100000);
            sector.Add((byte)Params.Resourse, ResourseType.coins);
            wheel.Add(c++, sector);
            //8
            sector = new Dictionary<byte, object>();
            sector.Add((byte)Params.weight, (1d / 2d));
            sector.Add((byte)Params.Amount, 50);
            sector.Add((byte)Params.Resourse, ResourseType.diamonds);
            wheel.Add(c++, sector);
        }

        /// <summary>
        /// Дополнить элементы значениями пределов секторов для удобства
        /// </summary>
        public void AugmentDuelWheel()
        {
            //foreach(var e in this.DuelWheel)
            //{
            //    Logger.Log.Debug("SECTOR WEIGTH " + e.Value[(byte)Params.weight]);
            //}

            DuelWheel[0].Add((byte)Params.limit, DuelWheel[0][(byte)Params.weight]);

            for(int i = 1; i < DuelWheel.Count; i++)
            {
                var newLimit = (double)DuelWheel[i - 1][(byte)Params.limit] + (double)DuelWheel[i][(byte)Params.weight];
                DuelWheel[i].Add((byte)Params.limit, newLimit);
            }

            //foreach (var e in this.DuelWheel)
            //{
            //    Logger.Log.Debug("SECTOR LIMIT " + e.Value[(byte)Params.limit]);
            //}
        }


        public int RotateWheel()
        {
            int result = 0;

            Random r = new Random();

            double state = r.NextDouble() * (double)DuelWheel[DuelWheel.Count - 1][(byte)Params.limit];

            for(int i = 0; i<DuelWheel.Count; i++)
            {
                if(state < (double)DuelWheel[i][(byte)Params.limit])
                {
                    //Logger.Log.Debug("NUMBER " + i);
                    return i;
                }
            }

            return result;
        }

        public Dictionary<byte, object> PlayDuelWheel(long userId)
        {
            

            int index = RotateWheel();

            int amount = (int)DuelWheel[index][(byte)Params.Amount];
            ResourseType resourse = (ResourseType)DuelWheel[index][(byte)Params.Resourse];

            Logger.Log.Debug("amount " + amount);
            Logger.Log.Debug("resourse " + resourse);

            //Скорее всего надо вынести в отдельный метод или класс
            AwardUser(userId, resourse, amount);
            
            return DuelWheel[index];
        }
        
        /// <summary>
        /// Заполнение списка смещений начал дуэлей
        /// </summary>
        public void FillDuelsSpans()
        {
            duelsSpans.Add(TimeSpan.FromDays(6 - 1) + TimeSpan.FromHours(12));
            duelsSpans.Add(TimeSpan.FromDays(7 - 1) + TimeSpan.FromHours(12));
        }

        /// <summary>
        /// Тестовое заполнение начал дуэлей
        /// </summary>
        public void TestFillDuelsSpans(int count, int delay)
        {
            var now = DateTime.Now;

            var days = (int)now.DayOfWeek;

            for(int i=0; i < count; i++)
            {
                duelsSpans.Add(TimeSpan.FromDays(days) + TimeSpan.FromHours(now.Hour) + TimeSpan.FromMinutes(now.Minute + 1) + TimeSpan.FromSeconds(i * (duelTime + 5) + delay));
            }
        }

        /// <summary>
        /// Получение недельного интервала до начала дуэли
        /// </summary>
        /// <returns></returns>
        public int GetNextDuelSpan(TimeSpan ts)
        {
            for(int i = 0; i<duelsSpans.Count; i++)
            {
                if (duelsSpans[i] > ts)
                {
                    //Logger.Log.Debug("Get index of timespan");
                    return i;
                }
            }

            Logger.Log.Debug("MISTAKE");
            return 0;
        }

        public TimeSpan GetWeeklyTimeSpan(DateTime dt)
        {
            var day = (int)dt.DayOfWeek;
            var hour = dt.Hour;
            var minute = dt.Minute;
            var second = dt.Second;
            var millisecond = dt.Millisecond;

            TimeSpan ts =
                +TimeSpan.FromDays(day)
                +TimeSpan.FromHours(hour)
                +TimeSpan.FromMinutes(minute)
                +TimeSpan.FromSeconds(second)
                +TimeSpan.FromMilliseconds(millisecond);

            return ts;
        }

        /// <summary>
        /// Обновить состояние менеджера дуэлей
        /// </summary>
        public void UpdateStateOfManager(int id, DateTime start, DateTime end)
        {
            this.currentDuelDayId = id;
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Начало дуэльного времени
        /// </summary>
        public void StartDuelTime()
        {
            Logger.Log.Debug("START DUEL TIME " + DateTime.Now);

            //Создать дуэльное время и перечень дуэлей
            //this.currentDuelDayId = CreateDuels();

            //Запланировать окончание дуэльного времени
            PlanStopDuelTime();
        }

        public void PlanStopDuelTime()
        {
            var now = DateTime.Now;
            var timeSpan = this.end - now;

            poolFiber.Schedule(() => StopDuelTime(), (long)timeSpan.TotalMilliseconds);
        }

        /// <summary>
        /// Окончание дуэльного времени
        /// </summary>
        public void StopDuelTime()
        {
            Logger.Log.Debug("STOP DUEL TIME " + DateTime.Now);
            
            //Подвести результаты дуэлей
            EndDuelDay(this.currentDuelDayId);

            //Запланировать начало дуэльного времени
            PlanStartDuelTime();
        }

        /// <summary>
        /// Запланировать начало дуэли
        /// </summary>
        public void PlanStartDuelTime()
        {
            //Получить даты и время начала и окончания слуд дуэли
            var now = DateTime.Now;
            var currentSpan = GetWeeklyTimeSpan(now);

            var timeSpanIndex = GetNextDuelSpan(currentSpan);

            var mon = now.Date - TimeSpan.FromDays(((int)now.DayOfWeek-1));

            var start = mon + duelsSpans[timeSpanIndex];
            var end = start + duelInterval;

            Logger.Log.Debug("NEW DUEL AT " + start);

            //Обновить состояние менеджера
            UpdateStateOfManager(0, start, end);

            //Посчитать интервал до начала след дуэли от текущего момента
            var timeSpan = duelsSpans[timeSpanIndex] - currentSpan;
            //Запланировать начало дуэльного времени
            poolFiber.Schedule(() => StartDuelTime(), (long)timeSpan.TotalMilliseconds);
        }

        public int CreateDuels()
        {
            int newDuelDayId = CreateDuelDay();

            CreateClansPairs(newDuelDayId);

            return newDuelDayId;
        }

        /// <summary>
        /// Создание дуэльного дня
        /// </summary>
        public int CreateDuelDay()
        {
            Logger.Log.Debug("Create duelDay");

            return DBManager.Inst.CreateDuelDay(start.ToString("yyyy-MM-dd H:mm:ss"), end.ToString("yyyy-MM-dd H:mm:ss"));
        }

        /// <summary>
        /// Создать пары кланов для дуэлей
        /// </summary>
        public void CreateClansPairs(int newDueldayId)
        {
            var clans = DBManager.Inst.GetClansDuelTop();

            if (clans.Count <= 0) return;
            //Logger.Log.Debug("create clans pairs " + clans.Count);

            foreach (var c in clans)
            {
                var clanData = (Dictionary<byte, object>)c.Value;

                Logger.Log.Debug(clanData[(byte)Params.Id]);
            }

            for(int i = 0; i < clans.Count - 1; i++)
            {
                var clan = (Dictionary<byte, object>)clans[i];

                Logger.Log.Debug(clan[(byte)Params.Id]);

                if (!clan.ContainsKey((byte)Params.pair))
                {
                    //Logger.Log.Debug("Try create pair");
                    TryCreatePair(clans, i);
                }
            }

            //CheckPairs(clans);
            SaveClansPairs(clans, newDueldayId);

            CreateMissionsForDuels(clans, newDueldayId);
        }

        public void CreateMissionsForDuels(Dictionary <int, object> clans, int newDuelDayId)
        {
            foreach (var c in clans)
            {
                var clanData = (Dictionary<byte, object>)c.Value;

                int id = (int)clanData[(byte)Params.Id];

                if (!clanData.ContainsKey((byte)Params.pair)) continue;

                int pairId = (int)clanData[(byte)Params.pair];

                if (pairId == 0) continue;

                CreateMissionsForClanPair(dueldayId: newDuelDayId, clan1Id: id, clan2Id: pairId);
            }
        }
        
        public void CreateMissionsForClanPair(int dueldayId, int clan1Id, int clan2Id)
        {
            Random rand = new Random();

            var clan1Users = DBManager.Inst.GetClanUsers(clan1Id);
            var clan2Users = DBManager.Inst.GetClanUsers(clan2Id);

            Logger.Log.Debug("clan1Users.Count " + clan1Users.Count);
            Logger.Log.Debug("clan1Users.Count " + clan2Users.Count);

            int index = 0;
            int maxIndex = Enum.GetNames(typeof(DuelsAchiveId)).Length;

            int maxMissions = Math.Max(clan1Users.Count, clan2Users.Count);

            Logger.Log.Debug("max" + maxMissions);

            for (int i = 1; i < maxMissions + 1; i++)
            {
                index = rand.Next(maxIndex);

                Logger.Log.Debug(i + " mission is " + ((DuelsAchiveId)index).ToString());

                DBManager.Inst.SaveClanPairMission(dueldayId, clan1Id, clan2Id, i, ((DuelsAchiveId)index).ToString());

                if (clan1Users.ContainsKey(i))
                {
                    CreateDuelUserMission(clan1Users, dueldayId, clan1Id, clan1Id, clan2Id, i);
                }

                if (clan2Users.ContainsKey(i))
                {
                    CreateDuelUserMission(clan2Users, dueldayId, clan2Id, clan1Id, clan2Id, i);
                }
            }
        }

        public void CreateDuelUserMission(Dictionary<int, object> clan, int dueldayId, int clanId, int clan1Id, int clan2Id, int missionNumber)
        {
            
            var user = (Dictionary<byte, object>)clan[missionNumber];
            var userId = (int)user[(byte)Params.Id];

            Logger.Log.Debug("userId " + userId);

            DBManager.Inst.SaveDuelUserMission(dueldayId, clanId, clan1Id, clan2Id, userId, missionNumber, 0);
        }
        
        public void TryCreatePair(Dictionary<int, object> clans, int current)
        {
            int maxLimit = clans.Count - current - 1;
            int lenght = maxLimit;

            if(maxLimit > Options.maxDuelDif)
            {
                maxLimit = Options.maxDuelDif;
            }

            var r = new Random();

            //Начальная позиция для проверки
            int startPos = r.Next(maxLimit) + 1;

            Logger.Log.Debug("startPos " + startPos);
            Logger.Log.Debug("lenght " + lenght);

            TryFindPair(clans,current,startPos,lenght);
        }

        public void TryFindPair(Dictionary<int, object> clans, int current, int startPos, int lenght)
        {
            int newP;

            for(int c = 0; c < lenght; c++)
            {
                newP = c + startPos;
                if(newP > lenght)
                {
                    newP = newP - lenght;
                }

                //Logger.Log.Debug("newP " + newP);

                var clan1 = (Dictionary<byte, object>)clans[current];
                var clan2 = (Dictionary<byte, object>)clans[newP];

                //Если клан не состоит в альянсе
                if ((int)clan1[(byte)Params.allianceId] == 0)
                {
                    MakePair(clan1, clan2);
                    return;
                } 
                else
                {
                    //Если у кланов разные альянсы
                    if((int)clan1[(byte)Params.allianceId] != (int)clan2[(byte)Params.allianceId])
                    {
                        MakePair(clan1, clan2);
                        return;
                    }
                }

            }
        }

        public void MakePair(Dictionary<byte, object> clan1, Dictionary<byte, object> clan2)
        {
            clan1.Add((byte)Params.pair, (int)clan2[(byte)Params.Id]);
            //0 как метка, что является парой
            clan2.Add((byte)Params.pair, 0);
        }

        public void SaveClansPairs(Dictionary<int, object> clans, int dueldayId)
        {
            foreach(var c in clans)
            {
                var clanData = (Dictionary<byte, object>)c.Value;

                int id = (int)clanData[(byte)Params.Id];

                if (!clanData.ContainsKey((byte)Params.pair)) continue;

                int pairId = (int)clanData[(byte)Params.pair];

                if (pairId == 0) continue;

                DBManager.Inst.SaveClansPair(dueldayId: dueldayId, clan1Id: id, clan2Id: pairId);
            }
        }

        public void CheckPairs(Dictionary<int, object> clans)
        {
            Logger.Log.Debug("Show Pairs");

            foreach(var c in clans)
            {
                var clan = c.Value as Dictionary<byte, object>;

                string s;

                s = "clan " + clan[(byte)Params.Id].ToString();

                if (clan.ContainsKey((byte)Params.pair)){
                    if ((int)clan[(byte)Params.pair] != 0)
                    {
                        s = s + " has pair " + (int)clan[(byte)Params.pair];
                    }
                    else
                    {
                        s = s + " is pair";
                    }
                    
                }

                Logger.Log.Debug(s);
            }
        }

        /// <summary>
        /// Подвести итоги дуэлбного дня
        /// </summary>
        public void EndDuelDay(int id)
        {
            if(id == 0) return;

            //Получить идентификаторы дуэльных пар для дуэльного периода
            var clanPairs = DBManager.Inst.GetClanPairsForDuelday(id);

            Logger.Log.Debug("clanPairs count " + clanPairs.Count);

            foreach(var cp in clanPairs)
            {
                EndDuel((Dictionary<byte, object>)cp.Value);
            }

        }

        /// <summary>
        /// Подвести итог конкретной дуэли
        /// </summary>
        public void EndDuel(Dictionary<byte, object> clanPair)
        {
            int bonusPoints = 10;
            int upPoints = 10;

            Logger.Log.Debug("CP id " + clanPair[(byte)Params.Id]);
            Logger.Log.Debug("Clan1 id " + clanPair[(byte)Params.clan1Id]);
            Logger.Log.Debug("Clan2 id " + clanPair[(byte)Params.clan2Id]);

            int clanPairId = (int)clanPair[(byte)Params.Id];
            int clan1Id = (int)clanPair[(byte)Params.clan1Id];
            int clan2Id = (int)clanPair[(byte)Params.clan2Id];

            int clan1Points = 0;
            int clan2Points = 0;

            var clan1usermissions = DBManager.Inst.GetDuelClanUsers((int)clanPair[(byte)Params.Id], clan1Id);
            var clan2usermissions = DBManager.Inst.GetDuelClanUsers((int)clanPair[(byte)Params.Id], clan2Id);

            if(clan1usermissions.Count >= clan2usermissions.Count)
            {
                CompareClansUsersMissions(clan1Id, clan2Id, clan1usermissions, clan2usermissions, ref clan1Points, ref clan2Points);
            }
            else
            {
                CompareClansUsersMissions(clan2Id, clan1Id, clan2usermissions, clan1usermissions, ref clan2Points, ref clan1Points);
            }

            //Logger.Log.Debug("clan1Points " + clan1Points);
            //Logger.Log.Debug("clan2Points " + clan2Points);

            //Получить личный топ для начисления бонусных очков
            var duelTop = DBManager.Inst.GetDuelPersonalTop(clanPairId);

            for(int i = 0; i < duelTop.Count; i++)
            {
                var rowData = (Dictionary<byte, object>)duelTop[i];

                int number = (int)rowData[(byte)Params.Number];
                int clanId = (int)rowData[(byte)Params.clanId];

                //Logger.Log.Debug("NUMBER " + number);
                //Logger.Log.Debug("CLAN ID " + clanId);

                if(number < 6)
                {
                    if(clanId == clan1Id)
                    {
                        clan1Points = clan1Points + 6 - number;
                    }
                    if(clanId == clan2Id)
                    {
                        clan2Points = clan2Points + 6 - number;
                    }
                }

                if (number == 6)
                {
                    break;
                }
            }

            //Logger.Log.Debug("clan1Points " + clan1Points);
            //Logger.Log.Debug("clan2Points " + clan2Points);

            //Сравнить очки, задать выигравший клан, добавить бонусные, добавить дуэльные очки клану
            if(clan1Points == clan2Points)
            {
                clan1Points = clan1Points + bonusPoints / 2;
                clan2Points = clan2Points + bonusPoints / 2;

            } 
            else if(clan1Points > clan2Points)
            {
                clan1Points = clan1Points + bonusPoints;
                DBManager.Inst.SetDuelWinner(clanPairId: clanPairId, clanId: clan1Id);
                DBManager.Inst.AddUpPointsToClan(clanId: clan1Id, points: upPoints);
                
            } 
            else if(clan2Points > clan1Points)
            {
                clan2Points = clan2Points + bonusPoints;
                DBManager.Inst.SetDuelWinner(clanPairId: clanPairId, clanId: clan2Id);
                DBManager.Inst.AddUpPointsToClan(clanId: clan2Id, points: upPoints);
            }

            DBManager.Inst.AddDuelPointsToClan(clanId: clan1Id, duelPoints: clan1Points);
            DBManager.Inst.AddDuelPointsToClan(clanId: clan2Id, duelPoints: clan2Points);

            //Award clans
            AwardClanUsers(clanPairId, clan1Id);
            AwardClanUsers(clanPairId, clan2Id);
        }

        /// <summary>
        /// Попарное сравнение строчек миссий
        /// </summary>
        public void CompareClansUsersMissions(int clan1Id, int clan2Id, Dictionary<int, object> clan1usermis, Dictionary<int, object> clan2usermis, ref int clan1Points, ref int clan2Points)
        {
            for(int i = 0; i < clan1usermis.Count; i++)
            {
                var row1Data = (Dictionary<byte, object>)clan1usermis[i];
                var row1Id = (long)row1Data[(byte)Params.Id];
                int amount1 = (int)row1Data[(byte)Params.Amount];

                if (!clan2usermis.ContainsKey(i))
                {
                    if (amount1 == 0)
                        continue;

                    //Logger.Log.Debug("ADD point to clan1");
                    clan1Points++;
                    DBManager.Inst.SetWinUserDuelMission(row1Id, 1);
                    continue;
                } 
                
                
                var row2Data = (Dictionary<byte, object>)clan2usermis[i];
                var row2Id = (long)row2Data[(byte)Params.Id];
                int amount2 = (int)row2Data[(byte)Params.Amount];

                if (amount1 > amount2)
                {
                    DBManager.Inst.SetWinUserDuelMission(row1Id, 1);
                    DBManager.Inst.SetWinUserDuelMission(row2Id, -1);
                    clan1Points++;
                }

                if(amount2 > amount1)
                {
                    DBManager.Inst.SetWinUserDuelMission(row2Id, 1);
                    DBManager.Inst.SetWinUserDuelMission(row1Id, -1);
                    clan2Points++;
                }
            }

            //Logger.Log.Debug("clan1Points " + clan1Points);
            //Logger.Log.Debug("clan2Points " + clan2Points);
        }



        /// <summary>
        /// Добавить очки в задании игрока
        /// </summary>
        public void IncrementUserMisionScore(long userId, DuelsAchiveId duelType, int score)
        {
            if (this.currentDuelDayId == 0)
            {
                Logger.Log.Debug("NOT IN DUEL TIME");
                return;
            }

            //Возможно здесь будет учёт старнных заданий для попарного сравнения
            //Для них будут создаваться строчки в дополнительной таблице БД.(Может быть)

            DBManager.Inst.IncrementUserDuelMission(duelDayId:this.currentDuelDayId, userId,duelType, score);
        }


        /// <summary>
        /// Проверка идёт ли сейчас дуэльное время для запрета соответствующих днйствий.
        /// </summary>
        public bool CheckDuelTime()
        {
            return (this.currentDuelDayId != 0);
        }
    }
}