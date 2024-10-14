using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    /// <summary>
    /// Настройки для приложения
    /// </summary>
    public class Options
    {
        #region Database connection parameters

        //public static string password = "mysql";
        #endregion

        /// <summary>
        /// Проверять ли колличество енергии при атаке.
        /// </summary>
        //public static bool CheckEnergy = false;

        public static int ChatSilenceKillLimit = 90;


        public static int clanCost = 50000;

        public static int maxClanUsers = 40;
        //public static int maxMissionsInDuel = 40;
        public static int maxMissionsInDuel = 5;

        public static int maxAlienceCount = 3;

        /// <summary>
        /// Максимальная разница позиций в топе дуэлей для составления пар кланов для дуэлей
        /// </summary>
        public static int maxDuelDif = 10;
        /// <summary>
        /// Стоимость вращения дуэльного колеса
        /// </summary>
        public static int wheelCost = 3;

        /// <summary>
        /// Колличество кланов в группе в недельном соревновании кланов нв опыт
        /// </summary>
        public static int weeklyExpClanCompGroupCount = 4;

        public static SendParameters sendParameters;

        public static RoomSettings roomSettings = new RoomSettings();
    }

    public class RoomSettings
    {
        public int FirstNightDuration = 15;
        public int NightDuration = 35;
        public int DayDuration = 35;
        public int JudgeDuration = 15;
    }
}


