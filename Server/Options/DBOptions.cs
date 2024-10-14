using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class DBOptions
    {
        /// <summary>
        /// Хост базы данных
        /// </summary>
        public static string host = "127.0.0.1";
        /// <summary>
        /// Порт базы данных
        /// </summary>
        public static int port = 3306;
        /// <summary>
        /// Имя базы данных
        /// </summary>
        public static string database = "mafia";
        /// <summary>
        /// Имя пользователя базы данных
        /// </summary>
        public static string username = "root";
        /// <summary>
        /// Пароль пользователя базы данных
        /// </summary>
        public static string password = "xPQm0EkR4tmQ";

        public static string workDomen = "https://cityofsinners.ru";

        public static string avatarsUrl = "/avatars/";
        public static string extrasUrl = "/extras/";
        public static string skillsUrl = "/skills/";
    }
}
