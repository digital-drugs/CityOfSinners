using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class ExtrasManager
    {
        public ExtrasManager()
        {
            LoadExtras();
        }

        public static Dictionary<string, object> rawExtrasData;

        private void LoadExtras()
        {     
            rawExtrasData = DBManager.Inst.LoadExtras();
        }

        public static Dictionary<string, object> LoadExtrasData()
        {
            return DBManager.Inst.LoadExtras();
        }
    }
}
