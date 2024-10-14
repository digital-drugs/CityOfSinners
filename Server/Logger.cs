using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public static class Logger
    {
        public static readonly ILogger Log = ExitGames.Logging.LogManager.GetCurrentClassLogger();
    }
}
